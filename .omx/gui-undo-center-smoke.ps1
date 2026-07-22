$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$seedTool = Join-Path $repo 'src\Css.SmokeTools\bin\Debug\net8.0-windows\Css.SmokeTools.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-undo-center.png'
$isolatedDataRoot = Join-Path $PSScriptRoot 'qa-undo-center-data'
$isolatedQuarantineRoot = Join-Path $PSScriptRoot 'qa-undo-center-quarantine'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousQuarantineRoot = $env:OMNIX_ENTROPY_QUARANTINE_ROOT

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

if (-not (Test-Path -LiteralPath $seedTool)) {
    throw "Css.SmokeTools.exe not found. Build the solution first: $seedTool"
}

Initialize-WpfSmokeAutomation

function Seed-RestorableUndoRecord([string]$toolPath) {
    $output = & $toolPath seed-undo-center 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Seed-RestorableUndoRecord failed: $($output -join [Environment]::NewLine)"
    }

    return $output -join [Environment]::NewLine
}

$process = $null
try {
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $isolatedDataRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $isolatedQuarantineRoot -Force | Out-Null
    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $isolatedQuarantineRoot
    $seedOutput = Seed-RestorableUndoRecord $seedTool

    $process = Start-Process -FilePath $exe -PassThru
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ProcessIdProperty), $process.Id

    $window = Wait-Until -TimeoutSeconds 12 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }

    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    $window.SetFocus()

    $timelineNav = Find-ByAutomationId $window 'TimelineNavButton'
    if ($null -eq $timelineNav) {
        throw 'TimelineNavButton was not found.'
    }

    Invoke-Element $timelineNav

    $title = Find-ByAutomationId $window 'TimelineTitleTextBlock'
    $loadButton = Find-ByAutomationId $window 'LoadTimelineButton'
    $policy = Find-ByAutomationId $window 'TimelineQuarantinePolicyTextBlock'
    $timelineList = Find-ByAutomationId $window 'TimelineListBox'
    $restoreLine = Find-ByAutomationId $window 'TimelineRestoreLineTextBlock'
    $technicalDetailsExpander = Find-ByAutomationId $window 'TimelineTechnicalDetailsExpander'
    $restoreButton = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'TimelineRestoreButton' 500
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) {
            return $candidate
        }

        return $null
    }

    if ($null -eq $title) { throw 'TimelineTitleTextBlock was not found.' }
    if ($null -eq $loadButton) { throw 'LoadTimelineButton was not found.' }
    if ($null -eq $policy) { throw 'TimelineQuarantinePolicyTextBlock was not found.' }
    if ($null -eq $timelineList) { throw 'TimelineListBox was not found.' }
    if ($null -eq $restoreButton) { throw 'TimelineRestoreButton was not found.' }
    if ($null -eq $restoreLine) { throw 'TimelineRestoreLineTextBlock was not found.' }
    if ($null -eq $technicalDetailsExpander) { throw 'TimelineTechnicalDetailsExpander was not found.' }

    if (-not $restoreButton.Current.IsEnabled) {
        throw 'Timeline restore button should be enabled for seeded restorable data.'
    }

    if ([string]::IsNullOrWhiteSpace($policy.Current.Name)) {
        throw 'Timeline quarantine policy text is empty.'
    }

    Save-WindowScreenshot $window $screenshotPath

    [PSCustomObject]@{
        timelineTitleFound = $true
        quarantinePolicyFound = $true
        timelineListFound = $true
        restoreButtonFound = $true
        restoreButtonEnabled = $true
        technicalDetailsExpanderFound = $true
        seedOutput = $seedOutput
        isolatedDataRoot = $isolatedDataRoot
        isolatedQuarantineRoot = $isolatedQuarantineRoot
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $previousQuarantineRoot
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
}
