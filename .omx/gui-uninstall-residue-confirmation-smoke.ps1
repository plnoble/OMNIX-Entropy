$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-uninstall-residue-confirmation.png'
$cancelOutcomeScreenshotPath = Join-Path $PSScriptRoot 'qa-uninstall-residue-cancel-outcome.png'
$isolatedDataRoot = Join-Path $PSScriptRoot 'qa-uninstall-residue-data'
$isolatedQuarantineRoot = Join-Path $PSScriptRoot 'qa-uninstall-residue-quarantine'
$residueRoot = Join-Path $PSScriptRoot 'qa-uninstall-residue-leftovers'
$softwareFixturePath = Join-Path $PSScriptRoot 'qa-uninstall-residue-software-fixture.json'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousQuarantineRoot = $env:OMNIX_ENTROPY_QUARANTINE_ROOT
$previousSoftwareFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE

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
    Remove-Item -LiteralPath $residueRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $isolatedDataRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $isolatedQuarantineRoot -Force | Out-Null

    $cacheRoot = Join-Path $residueRoot 'Cache'
    $logRoot = Join-Path $residueRoot 'Logs'
    New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
    [System.IO.File]::WriteAllBytes((Join-Path $cacheRoot 'cache.bin'), (New-Object byte[] 4096))
    [System.IO.File]::WriteAllBytes((Join-Path $logRoot 'app.log'), (New-Object byte[] 2048))

    $profile = [ordered]@{
        name = 'Fixture Residue App'
        publisher = 'OMNIX Smoke'
        installPath = 'D:\Software\FixtureResidue\Install'
        uninstallCommand = '"D:\Software\FixtureResidue\Install\uninstall.exe"'
        cachePaths = @($cacheRoot)
        logPaths = @($logRoot)
    }
    $fixture = [ordered]@{
        scans = @(
            @($profile),
            @()
        )
    }
    $fixture | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $softwareFixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $isolatedQuarantineRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $softwareFixturePath

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

    $appsNav = Find-ByAutomationId $window 'AppsNavButton'
    if ($null -eq $appsNav) { throw 'AppsNavButton was not found.' }
    Invoke-Element $appsNav

    $scanSoftware = Find-ByAutomationId $window 'ScanSoftwareButton'
    if ($null -eq $scanSoftware) { throw 'ScanSoftwareButton was not found.' }
    Invoke-Element $scanSoftware

    $appList = Find-ByAutomationId $window 'AppTilesListBox' 1000
    if ($null -eq $appList) {
        throw 'AppTilesListBox was not found.'
    }

    $fixtureItem = Wait-Until -TimeoutSeconds 20 -Probe {
        $itemCondition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem)
        $items = $appList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $itemCondition)
        foreach ($item in $items) {
            if ($item.Current.Name -like '*Fixture Residue App*') {
                return $item
            }
        }

        return $null
    }

    if ($null -eq $fixtureItem) {
        throw 'Fixture Residue App tile was not found after software fixture scan.'
    }

    Select-ListItem $fixtureItem
    Start-Sleep -Milliseconds 500

    $residueButton = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'DrawerResidueReviewButton' 250
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $residueButton) {
        throw 'DrawerResidueReviewButton was not found or enabled.'
    }

    $mainWindowHandle = $window.Current.NativeWindowHandle
    Invoke-Element $residueButton

    $confirmation = Wait-Until -TimeoutSeconds 10 -Probe {
        Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'CleanupConfirmationSummaryTextBlock'
    }

    if ($null -eq $confirmation) {
        throw 'Residue cleanup confirmation window was not found.'
    }

    foreach ($fieldId in @(
        'CleanupConfirmationSummaryTextBlock',
        'CleanupConfirmationOutcomeListBox',
        'CleanupConfirmationTechnicalDetailsExpander',
        'CleanupConfirmationCancelButton')) {
        $field = Find-ByAutomationId $confirmation $fieldId 1000
        if ($null -eq $field) {
            throw "Residue confirmation field was not found: $fieldId"
        }

        if ($field.Current.IsOffscreen) {
            throw "Residue confirmation field was offscreen: $fieldId"
        }
    }

    Save-DesktopScreenshot $screenshotPath

    $cancelButton = Find-ByAutomationId $confirmation 'CleanupConfirmationCancelButton' 1000
    Invoke-Element $cancelButton
    Start-Sleep -Milliseconds 700

    $cancelOutcome = Wait-Until -TimeoutSeconds 8 -Probe {
        $title = Find-ByAutomationId $window 'DrawerActionPreviewTitleTextBlock' 250
        if ($null -ne $title -and -not $title.Current.IsOffscreen) {
            return $title
        }

        return $null
    }
    if ($null -eq $cancelOutcome) {
        throw 'Residue cancel outcome panel was not visible.'
    }

    $primaryButton = Find-ByAutomationId $window 'DrawerActionPreviewPrimaryButton' 500
    $primaryButtonHiddenAfterCancel = $null -eq $primaryButton -or $primaryButton.Current.IsOffscreen
    if (-not $primaryButtonHiddenAfterCancel) {
        throw 'DrawerActionPreviewPrimaryButton should stay hidden after cancel.'
    }
    Save-DesktopScreenshot $cancelOutcomeScreenshotPath

    [PSCustomObject]@{
        residueConfirmationFound = $true
        cancelClicked = $true
        cancelOutcomeVisible = $true
        primaryButtonHiddenAfterCancel = $true
        residueStillExists = ((Test-Path -LiteralPath $cacheRoot) -and (Test-Path -LiteralPath $logRoot))
        quarantineItemCount = @(
            Get-ChildItem -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
        ).Count
        screenshot = $screenshotPath
        cancelOutcomeScreenshot = $cancelOutcomeScreenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $previousQuarantineRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousSoftwareFixture
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $isolatedQuarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $residueRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
}
