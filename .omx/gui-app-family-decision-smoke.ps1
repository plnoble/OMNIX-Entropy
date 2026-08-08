$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "wpf-smoke-helpers.ps1")
Initialize-WpfSmokeAutomation

function Select-ListItem {
    param([System.Windows.Automation.AutomationElement]$Element)

    $pattern = $Element.GetCurrentPattern(
        [System.Windows.Automation.SelectionItemPattern]::Pattern)
    $pattern.Select()
}

function Get-UnicodeText {
    param([int[]]$CodePoints)

    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Find-AppListItem {
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [string]$NamePart
    )

    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $items = $Window.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $condition)
    foreach ($item in $items) {
        if ($item.Current.Name.Contains($NamePart)) {
            return $item
        }
    }

    return $null
}

function Require-Text {
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [string]$AutomationId,
        [string[]]$Parts
    )

    $element = Find-ByAutomationId $Window $AutomationId 1000
    if ($null -eq $element) {
        throw "Missing UI element: $AutomationId"
    }

    foreach ($part in $Parts) {
        if (-not $element.Current.Name.Contains($part)) {
            throw "$AutomationId did not contain '$part': $($element.Current.Name)"
        }
    }

    if ($element.Current.IsOffscreen) {
        throw "$AutomationId was outside the first visible drawer area."
    }

    return $element.Current.Name
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
$dataRoot = Join-Path $repoRoot ".omx\qa-app-family-data"
$quarantineRoot = Join-Path $repoRoot ".omx\qa-app-family-quarantine"
$uninstallEvidenceRoot = Join-Path $repoRoot ".omx\qa-app-family-uninstall-evidence"
$fixturePath = Join-Path $repoRoot ".omx\qa-app-family-fixture.json"
$screenshotPath = Join-Path $repoRoot ".omx\qa-app-family-decision.png"
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$previousQuarantineRoot = $env:OMNIX_ENTROPY_QUARANTINE_ROOT
$previousUninstallEvidenceRoot = $env:OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT
$process = $null
$ui = [ordered]@{
    Family3 = (Get-UnicodeText @(0x540C, 0x7C7B)) + " 3 " + (Get-UnicodeText @(0x6761))
    NeverMergeUninstall = Get-UnicodeText @(0x4E0D, 0x4F1A, 0x5408, 0x5E76, 0x5378, 0x8F7D)
    OfficialUninstaller = Get-UnicodeText @(0x5B98, 0x65B9, 0x5378, 0x8F7D, 0x5165, 0x53E3)
    ExactEntryOnly = Get-UnicodeText @(0x53EA, 0x9488, 0x5BF9, 0x8FD9, 0x6761, 0x8BB0, 0x5F55)
    CDrive = "C " + (Get-UnicodeText @(0x76D8))
    DDrive = "D " + (Get-UnicodeText @(0x76D8))
    MayGrowAgain = Get-UnicodeText @(0x4ECD, 0x53EF, 0x80FD, 0x589E, 0x957F)
    UninstallThisVersion = Get-UnicodeText @(0x5378, 0x8F7D, 0x8FD9, 0x4E2A, 0x7248, 0x672C)
    NoOfficialUninstaller = Get-UnicodeText @(0x6CA1, 0x6709, 0x5B98, 0x65B9, 0x5378, 0x8F7D, 0x5165, 0x53E3)
    ProgramOrCopyAlreadyOnD = (Get-UnicodeText @(0x7A0B, 0x5E8F, 0x6216, 0x526F, 0x672C, 0x5DF2, 0x7ECF, 0x5728)) + " D " + (Get-UnicodeText @(0x76D8))
    EntryNotUninstallable = Get-UnicodeText @(0x6B64, 0x6761, 0x4E0D, 0x53EF, 0x5378, 0x8F7D)
}

try {
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Css.App.exe was not found: $exe"
    }

    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $quarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $uninstallEvidenceRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null

    $registered = [ordered]@{
        name = "OpenCode 1.14.41"
        publisher = "OpenCode"
        category = 4
        categoryAssessment = [ordered]@{
            category = 4
            confidence = 3
            isFallback = $false
            evidence = @([ordered]@{ source = 0; matchedRule = "opencode" })
        }
        displayVersion = "1.14.41"
        inventorySource = "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenCode"
        installPath = "C:\Program Files\OpenCode"
        uninstallCommand = '"C:\Program Files\OpenCode\Uninstall OpenCode.exe" /allusers'
        installedSizeBytes = 468713472
    }
    $portable = [ordered]@{
        name = "OpenCode"
        publisher = "OpenCode"
        category = 4
        categoryAssessment = [ordered]@{
            category = 4
            confidence = 3
            isFallback = $false
            evidence = @([ordered]@{ source = 0; matchedRule = "opencode" })
        }
        displayVersion = "1.4.3"
        installPath = "D:\Development\opencode"
        installedSizeBytes = 231735296
    }
    $dataClue = [ordered]@{
        name = "OpenCode 1.18.4"
        publisher = "OpenCode"
        category = 4
        categoryAssessment = [ordered]@{
            category = 4
            confidence = 3
            isFallback = $false
            evidence = @([ordered]@{ source = 0; matchedRule = "opencode" })
        }
        displayVersion = "1.18.4"
        installPath = "C:\Users\Fixture\AppData\Local\opencode-updater"
        installedSizeBytes = 248512512
        dataSizeBytes = 92589261
        cDriveDataSizeBytes = 92589261
        dataPaths = @("C:\Users\Fixture\AppData\Roaming\OpenCode")
        cDriveWritePaths = @(
            "C:\Users\Fixture\AppData\Local\opencode-updater",
            "C:\Users\Fixture\AppData\Roaming\OpenCode")
    }
    [ordered]@{ scans = @(,@($registered, $portable, $dataClue)) } |
        ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath $fixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $fixturePath
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $quarantineRoot
    $env:OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT = $uninstallEvidenceRoot
    $process = Start-Process -FilePath $exe -PassThru

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 20 -Probe {
        $root.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $processCondition)
    }
    if ($null -eq $window) {
        throw "Main window was not found."
    }

    $appsButton = Find-ByAutomationId $window "AppsNavButton" 1000
    if ($null -eq $appsButton) {
        throw "Apps navigation button was not found."
    }
    Invoke-Element $appsButton

    $scanButton = Find-ByAutomationId $window "ScanSoftwareButton" 1000
    if ($null -eq $scanButton) {
        throw "Software scan button was not found."
    }
    Invoke-Element $scanButton

    $registeredItem = Wait-Until -TimeoutSeconds 25 -Probe {
        Find-AppListItem $window "OpenCode 1.14.41"
    }
    if ($null -eq $registeredItem) {
        throw "Registered OpenCode fixture was not found."
    }
    if (-not $registeredItem.Current.Name.Contains($ui.Family3)) {
        throw "Registered OpenCode tile did not explain the three related records."
    }
    Select-ListItem $registeredItem
    Start-Sleep -Milliseconds 400

    $familyText = Require-Text $window "DrawerFamilySummaryTextBlock" @(
        ("3 " + (Get-UnicodeText @(0x6761))),
        $ui.NeverMergeUninstall)
    $entryText = Require-Text $window "DrawerCurrentEntryTextBlock" @(
        "1.14.41",
        $ui.OfficialUninstaller,
        $ui.ExactEntryOnly)
    $storageText = Require-Text $window "DrawerStorageOutcomeTextBlock" @(
        $ui.CDrive,
        "447.0 MB",
        "237.0 MB",
        "88.3 MB",
        $ui.MayGrowAgain)
    $uninstallButton = Find-ByAutomationId $window "DrawerUninstallButton" 1000
    if ($null -eq $uninstallButton -or -not $uninstallButton.Current.IsEnabled) {
        throw "The exact registered OpenCode version did not expose its official uninstall review."
    }
    if ($uninstallButton.Current.Name -ne $ui.UninstallThisVersion) {
        throw "Unexpected registered uninstall label: $($uninstallButton.Current.Name)"
    }

    Save-WindowScreenshot $window $screenshotPath

    $portableItem = Find-AppListItem $window ("OpenCode, " + $ui.Family3)
    if ($null -eq $portableItem) {
        throw "Portable OpenCode fixture was not found."
    }
    Select-ListItem $portableItem
    Start-Sleep -Milliseconds 400
    $portableEntryText = Require-Text $window "DrawerCurrentEntryTextBlock" @(
        $ui.DDrive,
        $ui.NoOfficialUninstaller)
    $portableStorageText = Require-Text $window "DrawerStorageOutcomeTextBlock" @(
        $ui.ProgramOrCopyAlreadyOnD,
        "684.0 MB",
        "88.3 MB",
        $ui.MayGrowAgain)
    $portableUninstallButton = Find-ByAutomationId $window "DrawerUninstallButton" 1000
    if ($null -eq $portableUninstallButton -or $portableUninstallButton.Current.IsEnabled) {
        throw "The D-drive copy incorrectly exposed uninstall execution."
    }
    if ($portableUninstallButton.Current.Name -ne $ui.EntryNotUninstallable) {
        throw "Unexpected portable uninstall label: $($portableUninstallButton.Current.Name)"
    }

    $quarantineManifestCount = @(
        Get-ChildItem -LiteralPath $quarantineRoot -Recurse -File -Filter 'manifest.json' -ErrorAction SilentlyContinue
    ).Count
    $uninstallEvidenceCreated = Test-Path -LiteralPath $uninstallEvidenceRoot
    if ($quarantineManifestCount -ne 0 -or $uninstallEvidenceCreated) {
        throw 'Application-family review created operation evidence unexpectedly.'
    }

    [PSCustomObject]@{
        Family = $familyText
        RegisteredEntry = $entryText
        RegisteredStorageOutcome = $storageText
        PortableEntry = $portableEntryText
        PortableStorageOutcome = $portableStorageText
        RegisteredUninstallEnabled = $true
        PortableUninstallEnabled = $false
        NoOperationExecuted = ($quarantineManifestCount -eq 0 -and -not $uninstallEvidenceCreated)
        QuarantineManifestCount = $quarantineManifestCount
        UninstallEvidenceCreated = $uninstallEvidenceCreated
        Screenshot = $screenshotPath
    } | ConvertTo-Json -Depth 4
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousFixture
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $previousQuarantineRoot
    $env:OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT = $previousUninstallEvidenceRoot
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $quarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $uninstallEvidenceRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
}
