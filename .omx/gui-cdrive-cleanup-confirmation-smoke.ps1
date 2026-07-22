$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-cdrive-cleanup-confirmation.png'
$isolatedDataRoot = Join-Path $PSScriptRoot 'qa-cdrive-cleanup-data'
$isolatedQuarantineRoot = Join-Path $PSScriptRoot 'qa-cdrive-cleanup-quarantine'
$scanRoot = Join-Path $PSScriptRoot 'qa-cdrive-cleanup-scan-root'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousQuarantineRoot = $env:OMNIX_ENTROPY_QUARANTINE_ROOT
$previousScanRoot = $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

function Select-ListItem {
    param([System.Windows.Automation.AutomationElement]$Element)

    $pattern = $Element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $pattern.Select()
}

$process = $null
try {
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $isolatedDataRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $isolatedQuarantineRoot -Force | Out-Null

    $fixtureTemp = Join-Path $scanRoot 'Temp'
    New-Item -ItemType Directory -Path $fixtureTemp -Force | Out-Null
    $bytes = New-Object byte[] 4096
    [System.IO.File]::WriteAllBytes((Join-Path $fixtureTemp 'agent-cache.bin'), $bytes)

    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $isolatedQuarantineRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $scanRoot

    $process = Start-Process -FilePath $exe -PassThru
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)

    $window = Wait-Until -TimeoutSeconds 12 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }

    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    $window.SetFocus()

    $cDriveNav = Find-ByAutomationId $window 'CDriveNavButton'
    if ($null -eq $cDriveNav) { throw 'CDriveNavButton was not found.' }
    Invoke-Element $cDriveNav

    $startScan = Find-ByAutomationId $window 'StartScanButton'
    if ($null -eq $startScan) { throw 'StartScanButton was not found.' }
    Invoke-Element $startScan

    $recommendations = Find-ByAutomationId $window 'RecommendationsListBox' 1000
    if ($null -eq $recommendations) {
        throw 'RecommendationsListBox was not found.'
    }

    $executeButton = Find-ByAutomationId $window 'ExecuteRecommendationButton' 1000
    if ($null -eq $executeButton) {
        throw 'ExecuteRecommendationButton was not found.'
    }

    $actionableItem = Wait-Until -TimeoutSeconds 30 -Probe {
        $itemCondition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem)
        $items = $recommendations.FindAll([System.Windows.Automation.TreeScope]::Descendants, $itemCondition)
        foreach ($item in $items) {
            try {
                Select-ListItem $item
                Start-Sleep -Milliseconds 200
                if ($executeButton.Current.IsEnabled) {
                    return $item
                }
            }
            catch {
                continue
            }
        }

        return $null
    }

    if ($null -eq $actionableItem) {
        throw 'No actionable low-risk C-drive cleanup item was found in the fixture scan.'
    }

    foreach ($fieldId in @(
        'RecommendationActionTakeawayTextBlock',
        'RecommendationActionNextStepTextBlock',
        'RecommendationActionSafetyTextBlock',
        'RecommendationActionPlanListBox')) {
        $field = Find-ByAutomationId $window $fieldId 1000
        if ($null -eq $field) {
            throw "Recommendation preview field was not found: $fieldId"
        }

        if ($field.Current.IsOffscreen) {
            throw "Recommendation preview field was offscreen: $fieldId"
        }
    }

    Invoke-Element $executeButton

    $mainWindowHandle = $window.Current.NativeWindowHandle
    $confirmation = Wait-Until -TimeoutSeconds 10 -Probe {
        Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'CleanupConfirmationSummaryTextBlock'
    }

    if ($null -eq $confirmation) {
        throw 'Cleanup confirmation window was not found.'
    }

    foreach ($fieldId in @(
        'CleanupConfirmationSummaryTextBlock',
        'CleanupConfirmationOutcomeListBox',
        'CleanupConfirmationTechnicalDetailsExpander',
        'CleanupConfirmationCancelButton')) {
        $field = Find-ByAutomationId $confirmation $fieldId 1000
        if ($null -eq $field) {
            throw "Cleanup confirmation field was not found: $fieldId"
        }

        if ($field.Current.IsOffscreen) {
            throw "Cleanup confirmation field was offscreen: $fieldId"
        }
    }

    Save-DesktopScreenshot $screenshotPath

    $cancelButton = Find-ByAutomationId $confirmation 'CleanupConfirmationCancelButton' 1000
    Invoke-Element $cancelButton
    Start-Sleep -Milliseconds 700

    [PSCustomObject]@{
        fixtureScanRoot = $scanRoot
        recommendationPreviewFound = $true
        confirmationDialogFound = $true
        cancelClicked = $true
        fixtureStillExists = (Test-Path -LiteralPath $fixtureTemp)
        quarantineItemCount = @(
            Get-ChildItem -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
        ).Count
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $previousQuarantineRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $previousScanRoot
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
}
