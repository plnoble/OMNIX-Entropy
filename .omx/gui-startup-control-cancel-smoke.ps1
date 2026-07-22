$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Get-Sha256Text([string]$Text) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return ([System.BitConverter]::ToString($hash)).Replace('-', '')
}

function Get-Sha256Bytes([byte[]]$Bytes) {
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($Bytes)
    return ([System.BitConverter]::ToString($hash)).Replace('-', '')
}

function Select-FirstAppTile($Window) {
    $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $item = Wait-Until -TimeoutSeconds 20 -Probe {
        $items = $Window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
        if ($items.Count -gt 0) { return $items[0] }
        return $null
    }
    if ($null -eq $item) {
        throw "Fixture app tile was not found."
    }

    $selection = $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $selection.Select()
    Start-Sleep -Milliseconds 250
}

function Open-StartupConfirmation($Window, [int]$ProcessId, [int]$MainWindowHandle) {
    Select-FirstAppTile $Window
    $startupButton = Find-ByAutomationId $Window 'DrawerDisableStartupButton' 5000
    if ($null -eq $startupButton -or -not $startupButton.Current.IsEnabled) {
        throw "Startup management button was not enabled."
    }

    Invoke-Element $startupButton
    $reviewButton = Wait-Until -TimeoutSeconds 15 -Probe {
        $button = Find-ByAutomationId $Window 'DrawerActionPreviewPrimaryButton' 250
        if ($null -ne $button -and $button.Current.IsEnabled -and -not $button.Current.IsOffscreen) {
            return $button
        }
        return $null
    }
    if ($null -eq $reviewButton) {
        throw "Local startup review action was not available."
    }

    $previewTitle = Find-ByAutomationId $Window 'DrawerActionPreviewTitleTextBlock' 2000
    if ($null -eq $previewTitle -or $previewTitle.Current.IsOffscreen) {
        throw "Local startup conclusion was not visible."
    }

    Invoke-Element $reviewButton
    $dialog = Wait-Until -TimeoutSeconds 20 -Probe {
        Find-SecondaryWindowWithChild $ProcessId $MainWindowHandle 'StartupConfirmationHeadlineTextBlock'
    }
    if ($null -eq $dialog) {
        throw "Startup confirmation window was not found."
    }

    Show-WpfWindowForSmoke $dialog
    return $dialog
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

$dataRoot = Join-Path $repoRoot '.omx\qa-startup-control-data'
$softwareFixturePath = Join-Path $repoRoot '.omx\qa-startup-control-software.json'
$startupFixturePath = Join-Path $repoRoot '.omx\qa-startup-control-entry.json'
$screenshotPath = Join-Path $repoRoot '.omx\qa-startup-control-confirmation.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousSoftwareFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$previousStartupFixture = $env:OMNIX_ENTROPY_STARTUP_FIXTURE
$process = $null

try {
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $startupFixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null

    $name = 'Fixture Startup'
    $command = 'D:\Software\Fixture\fixture.exe --background'
    $sourceLocator = 'HKCU64\Software\Microsoft\Windows\CurrentVersion\Run'
    $approvalLocator = 'HKCU64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run'
    $approvalBytes = [byte[]](2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    $approvalHash = Get-Sha256Bytes $approvalBytes
    $approvalMaterial = $approvalLocator.ToUpperInvariant() + "`n" +
        $name.ToUpperInvariant() + "`nPresentBinary`n12`n" + $approvalHash
    $identityMaterial = "StartupEntry`n" + $sourceLocator.ToUpperInvariant() + "`n" + $name.ToUpperInvariant()
    $observedConfiguration = $command + "`n" + $approvalMaterial
    $observationMaterial = $identityMaterial + "`n" + $observedConfiguration + "`nUnknown`nNotApplicable"
    $observedAt = [DateTimeOffset]::UtcNow

    $profile = [ordered]@{
        name = 'Startup Safety Fixture'
        publisher = 'OMNIX Smoke'
        category = 1
        installPath = 'D:\Software\Fixture\Install'
        startupEntries = @($name)
        backgroundComponents = @(
            [ordered]@{
                identity = [ordered]@{
                    kind = 0
                    stableId = Get-Sha256Text $identityMaterial
                    displayName = $name
                    sourceLocator = $sourceLocator
                }
                observedAtUtc = $observedAt.ToString('o')
                observationFingerprint = Get-Sha256Text $observationMaterial
                activationState = 0
                runtimeState = 0
                startupApproval = [ordered]@{
                    approvalKeyLocator = $approvalLocator
                    valueName = $name
                    status = 2
                    payloadFingerprint = $approvalHash
                    payloadLength = 12
                }
                requiredRollbackEvidence = @(0, 1, 5)
            }
        )
    }
    [ordered]@{ scans = @(,@($profile)) } |
        ConvertTo-Json -Depth 12 |
        Set-Content -LiteralPath $softwareFixturePath -Encoding UTF8
    [ordered]@{
        ValueName = $name
        ValueKind = 'String'
        ValueData = $command
        KeyAclSha256 = ('A' * 64)
    } |
        ConvertTo-Json -Depth 4 |
        Set-Content -LiteralPath $startupFixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $softwareFixturePath
    $env:OMNIX_ENTROPY_STARTUP_FIXTURE = $startupFixturePath
    $process = Start-Process -FilePath $exe -PassThru

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 20 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $processCondition)
    }
    if ($null -eq $window) {
        throw "Main window was not found."
    }

    $appsButton = Find-ByAutomationId $window 'AppsNavButton' 5000
    Invoke-Element $appsButton
    $scanButton = Find-ByAutomationId $window 'ScanSoftwareButton' 5000
    Invoke-Element $scanButton
    Start-Sleep -Milliseconds 500

    $mainWindowHandle = $window.Current.NativeWindowHandle
    $dialog = Open-StartupConfirmation $window $process.Id $mainWindowHandle
    $headline = Find-ByAutomationId $dialog 'StartupConfirmationHeadlineTextBlock' 2000
    $outcome = Find-ByAutomationId $dialog 'StartupConfirmationOutcomeListBox' 2000
    $firstCheck = Find-ByAutomationId $dialog 'StartupConfirmationFirstCheckBox' 2000
    $secondCheck = Find-ByAutomationId $dialog 'StartupConfirmationSecondCheckBox' 2000
    $confirmButton = Find-ByAutomationId $dialog 'StartupConfirmationConfirmButton' 2000
    $cancelButton = Find-ByAutomationId $dialog 'StartupConfirmationCancelButton' 2000
    $technical = Find-ByAutomationId $dialog 'StartupConfirmationTechnicalDetailsExpander' 2000
    foreach ($required in @($headline, $outcome, $firstCheck, $secondCheck, $confirmButton, $cancelButton, $technical)) {
        if ($null -eq $required) {
            throw "A required startup confirmation control was missing."
        }
    }
    if ($headline.Current.IsOffscreen -or $outcome.Current.IsOffscreen) {
        throw "The beginner startup conclusion was not in the visible working area."
    }
    if ($confirmButton.Current.IsEnabled) {
        throw "Startup confirm button should start disabled."
    }
    $expandState = $technical.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Current.ExpandCollapseState
    if ($expandState -ne [System.Windows.Automation.ExpandCollapseState]::Collapsed) {
        throw "Technical startup details should start collapsed."
    }
    $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $outcomeCount = $outcome.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition).Count
    if ($outcomeCount -lt 3) {
        throw "Startup confirmation should show at least three plain-language outcomes."
    }
    $visibleText = $headline.Current.Name + "`n" + $outcome.Current.Name
    if ($visibleText.Contains('fixture.exe') -or $visibleText.Contains('HKCU64') -or $visibleText.Contains('D:\Software')) {
        throw "Beginner startup confirmation exposed technical identifiers."
    }

    $manifestRoot = Join-Path $dataRoot 'StartupRollback\Manifests'
    $manifestCountBeforeCancel = @(Get-ChildItem -LiteralPath $manifestRoot -File -ErrorAction SilentlyContinue).Count
    if ($manifestCountBeforeCancel -ne 1) {
        throw "Exactly one rollback manifest should exist while confirmation is open."
    }
    Save-WindowScreenshot $dialog $screenshotPath
    Invoke-Element $cancelButton
    Start-Sleep -Milliseconds 700

    $manifestCountAfterCancel = @(Get-ChildItem -LiteralPath $manifestRoot -File -ErrorAction SilentlyContinue).Count
    if ($manifestCountAfterCancel -ne 0) {
        throw "Cancelled startup review left an uncommitted manifest behind."
    }

    $secondDialog = Open-StartupConfirmation $window $process.Id $mainWindowHandle
    $secondCancel = Find-ByAutomationId $secondDialog 'StartupConfirmationCancelButton' 2000
    if ($null -eq $secondCancel) {
        throw "The startup fixture was changed even though the first review was cancelled."
    }
    Invoke-Element $secondCancel

    [PSCustomObject]@{
        localReviewVisible = $true
        confirmationVisible = $true
        confirmInitiallyEnabled = $false
        outcomeCount = $outcomeCount
        technicalDetailsCollapsed = $true
        manifestCountBeforeCancel = $manifestCountBeforeCancel
        manifestCountAfterCancel = $manifestCountAfterCancel
        secondReviewReached = $true
        noOperationExecuted = $true
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
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousSoftwareFixture
    $env:OMNIX_ENTROPY_STARTUP_FIXTURE = $previousStartupFixture
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $startupFixturePath -Force -ErrorAction SilentlyContinue
}
