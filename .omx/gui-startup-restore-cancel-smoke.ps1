$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$seedTool = Join-Path $repoRoot 'src\Css.SmokeTools\bin\Debug\net8.0-windows\Css.SmokeTools.exe'
$dataRoot = Join-Path $repoRoot '.omx\qa-startup-restore-data'
$startupFixturePath = Join-Path $repoRoot '.omx\qa-startup-restore-entry.json'
$screenshotPath = Join-Path $repoRoot '.omx\qa-startup-restore-confirmation.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousStartupFixture = $env:OMNIX_ENTROPY_STARTUP_FIXTURE
$process = $null

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}
if (-not (Test-Path -LiteralPath $seedTool)) {
    throw "Css.SmokeTools.exe was not found. Build the solution first: $seedTool"
}

try {
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $startupFixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null

    [ordered]@{
        ValueName = 'OMNIX Startup Restore Fixture'
        ValueKind = 'String'
        ValueData = 'fixture.exe --background'
        KeyAclSha256 = ('A' * 64)
    } |
        ConvertTo-Json -Depth 4 |
        Set-Content -LiteralPath $startupFixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_STARTUP_FIXTURE = $startupFixturePath
    $seedOutput = & $seedTool seed-startup-undo-center 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Startup timeline seed failed: $($seedOutput -join [Environment]::NewLine)"
    }
    $seed = ($seedOutput -join [Environment]::NewLine) | ConvertFrom-Json
    $manifestPath = [string]$seed.RestoreManifestPaths[0]
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        throw 'The startup rollback manifest was not seeded.'
    }

    $process = Start-Process -FilePath $exe -PassThru
    if (-not $process.WaitForInputIdle(10000)) {
        throw 'The WPF app did not become input-ready.'
    }
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 15 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $processCondition)
    }
    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    $timelineNav = Find-ByAutomationId $window 'TimelineNavButton' 5000
    Invoke-Element $timelineNav
    $restoreButton = Wait-Until -TimeoutSeconds 15 -Probe {
        $button = Find-ByAutomationId $window 'TimelineRestoreButton' 250
        if ($null -ne $button -and $button.Current.IsEnabled) { return $button }
        return $null
    }
    if ($null -eq $restoreButton) {
        throw 'The seeded startup timeline restore button was not enabled.'
    }

    $mainWindowHandle = $window.Current.NativeWindowHandle
    Invoke-Element $restoreButton
    $dialog = Wait-Until -TimeoutSeconds 15 -Probe {
        Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'TimelineRestoreConfirmationHeadlineTextBlock'
    }
    if ($null -eq $dialog) {
        throw 'Startup restore confirmation window was not found.'
    }
    Show-WpfWindowForSmoke $dialog

    $headline = Find-ByAutomationId $dialog 'TimelineRestoreConfirmationHeadlineTextBlock' 2000
    $summary = Find-ByAutomationId $dialog 'TimelineRestoreConfirmationSummaryTextBlock' 2000
    $cancel = Find-ByAutomationId $dialog 'TimelineRestoreConfirmationCancelButton' 2000
    if ($null -eq $headline -or $null -eq $summary -or $null -eq $cancel) {
        throw 'Startup restore confirmation was incomplete.'
    }
    if ($headline.Current.IsOffscreen -or $summary.Current.IsOffscreen) {
        throw 'Startup restore conclusion was not visible in the first working area.'
    }
    $beginnerText = $headline.Current.Name + "`n" + $summary.Current.Name
    if ($beginnerText.Contains($manifestPath) -or $beginnerText.Contains('HKCU64')) {
        throw 'Startup restore confirmation exposed technical identifiers.'
    }

    Save-WindowScreenshot $dialog $screenshotPath
    Invoke-Element $cancel
    Start-Sleep -Milliseconds 500

    if (-not (Test-Path -LiteralPath $manifestPath)) {
        throw 'Cancelling startup restore removed its recovery evidence.'
    }
    if (-not $restoreButton.Current.IsEnabled) {
        throw 'Cancelling startup restore changed the timeline record.'
    }

    [PSCustomObject]@{
        startupRestoreConfirmationVisible = $true
        beginnerTextPathFree = $true
        startupManifestStillExists = $true
        restoreButtonStillEnabled = $true
        noRestoreExecuted = $true
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $null = $process.CloseMainWindow()
        Start-Sleep -Milliseconds 500
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_STARTUP_FIXTURE = $previousStartupFixture
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $startupFixturePath -Force -ErrorAction SilentlyContinue
}
