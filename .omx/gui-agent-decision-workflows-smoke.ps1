$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Get-UnicodeText {
    param([int[]]$CodePoints)

    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Get-DescendantText {
    param([System.Windows.Automation.AutomationElement]$Element)

    if ($null -eq $Element) {
        throw 'The text-inspection root was not found.'
    }

    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    $items = $Element.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $condition)
    return (($items | ForEach-Object { $_.Current.Name }) -join ' ')
}

function Ask-AgentQuestion {
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [System.Windows.Automation.AutomationElement]$QuestionBox,
        [System.Windows.Automation.AutomationElement]$AskButton,
        [string]$Question,
        [string]$ExpectedText,
        [int]$TimeoutSeconds = 90
    )

    $value = $QuestionBox.GetCurrentPattern(
        [System.Windows.Automation.ValuePattern]::Pattern)
    $value.SetValue($Question)
    Invoke-Element $AskButton
    $answer = Wait-Until -TimeoutSeconds $TimeoutSeconds -Probe {
        if ($process.HasExited) {
            throw "The app exited while Agent was answering. Exit code: $($process.ExitCode)"
        }
        $candidate = Find-ByAutomationId $Window 'AgentConversationAnswerTextBlock' 250
        if ($null -ne $candidate -and
            $candidate.Current.Name.Contains($ExpectedText) -and
            $AskButton.Current.IsEnabled) {
            return $candidate
        }
        return $null
    }
    if ($null -eq $answer) {
        throw "Agent answer did not contain the expected text: $ExpectedText"
    }
    return $answer.Current.Name
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$dataRoot = Join-Path $PSScriptRoot 'qa-agent-decision-data'
$quarantineRoot = Join-Path $PSScriptRoot 'qa-agent-decision-quarantine'
$uninstallEvidenceRoot = Join-Path $PSScriptRoot 'qa-agent-decision-uninstall-evidence'
$fixturePath = Join-Path $PSScriptRoot 'qa-agent-decision-fixture.json'
$scanRoot = Join-Path 'C:\tmp' ('OMNIX-Agent-Decision-' + [Guid]::NewGuid().ToString('N'))
$cDriveScreenshot = Join-Path $PSScriptRoot 'qa-agent-c-drive-decision.png'
$storagePlanScreenshot = Join-Path $PSScriptRoot 'qa-agent-storage-plan.png'
$familyScreenshot = Join-Path $PSScriptRoot 'qa-agent-family-uninstall.png'
$cacheScreenshot = Join-Path $PSScriptRoot 'qa-agent-cache-location.png'
$knownTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd('\')
$knownTempFixture = Join-Path $knownTempRoot ('OMNIX-agent-old-' + [Guid]::NewGuid().ToString('N') + '.tmp')
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousQuarantineRoot = $env:OMNIX_ENTROPY_QUARANTINE_ROOT
$previousUninstallEvidenceRoot = $env:OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT
$previousFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$previousScanRoot = $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT
$process = $null

try {
    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
        throw "Css.App.exe was not found: $exe"
    }

    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $quarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $uninstallEvidenceRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
    foreach ($folder in @('Users', 'Program Files', 'Temp', 'AMD')) {
        $path = Join-Path $scanRoot $folder
        New-Item -ItemType Directory -Path $path -Force | Out-Null
        [System.IO.File]::WriteAllBytes(
            (Join-Path $path ($folder.Replace(' ', '-') + '.bin')),
            (New-Object byte[] (4096 * (5 - [Math]::Min(4, $folder.Length % 5)))))
    }
    [System.IO.File]::WriteAllBytes($knownTempFixture, (New-Object byte[] 131072))
    [System.IO.File]::SetLastWriteTimeUtc($knownTempFixture, [DateTime]::UtcNow.AddDays(-30))

    $registered = [ordered]@{
        name = 'OpenCode 1.14.41'
        publisher = 'OpenCode'
        category = 4
        displayVersion = '1.14.41'
        inventorySource = 'HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenCode'
        installPath = 'C:\Program Files\OpenCode'
        uninstallCommand = '"C:\Program Files\OpenCode\uninstall.exe"'
        installedSizeBytes = 468713472
    }
    $portable = [ordered]@{
        name = 'OpenCode'
        publisher = 'OpenCode'
        category = 4
        displayVersion = '1.4.3'
        installPath = 'D:\Development\OpenCode'
        installedSizeBytes = 231735296
    }
    $dataOnly = [ordered]@{
        name = 'OpenCode 1.18.4'
        publisher = 'OpenCode'
        category = 4
        displayVersion = '1.18.4'
        dataSizeBytes = 248512512
        cDriveDataSizeBytes = 248512512
        dataPaths = @('C:\Users\Fixture\AppData\Roaming\OpenCode')
        cDriveWritePaths = @('C:\Users\Fixture\AppData\Local\opencode-updater')
    }
    $antigravity = [ordered]@{
        name = 'Antigravity 2.4.3'
        publisher = 'Google'
        category = 2
        displayVersion = '2.4.3'
        installPath = 'D:\Agent\Antigravity'
        uninstallCommand = '"D:\Agent\Antigravity\uninstall.exe"'
        installedSizeBytes = 1258291200
    }
    $antigravityData = [ordered]@{
        name = 'Antigravity (User)'
        publisher = 'Google'
        category = 2
        dataSizeBytes = 369098752
        cDriveDataSizeBytes = 369098752
        cacheSizeBytes = 314572800
        cDriveWritePaths = @(
            'C:\Users\Fixture\AppData\Local\antigravity-updater',
            'C:\Users\Fixture\AppData\Roaming\Antigravity')
    }
    [ordered]@{ scans = @(,@($registered, $portable, $dataOnly, $antigravity, $antigravityData)) } |
        ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath $fixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $quarantineRoot
    $env:OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT = $uninstallEvidenceRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $fixturePath
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $scanRoot

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
        throw 'Main window was not found.'
    }

    Show-WpfWindowForSmoke $window
    Invoke-Element (Find-ByAutomationId $window 'AgentNavButton' 5000)
    $questionBox = Find-ByAutomationId $window 'AgentQuestionTextBox' 3000
    $askButton = Find-ByAutomationId $window 'AskComputerAgentButton' 3000
    if ($null -eq $questionBox -or $null -eq $askButton) {
        throw 'Agent question controls were not found.'
    }

    foreach ($id in @(
        'AgentDecisionQuickChoice_c-drive-full',
        'AgentDecisionQuickChoice_fastest-growth',
        'AgentDecisionQuickChoice_safe-release')) {
        $choice = Find-ByAutomationId $window $id 2000
        if ($null -eq $choice -or $choice.Current.IsOffscreen) {
            throw "Decision prompt was not visible in the first Agent area: $id"
        }
    }

    $cannotAdd = Get-UnicodeText @(0x4E0D, 0x80FD, 0x76F4, 0x63A5, 0x76F8, 0x52A0)
    Invoke-Element (Find-ByAutomationId $window 'AgentDecisionQuickChoice_c-drive-full' 2000)
    $cDriveAnswer = Wait-Until -TimeoutSeconds 90 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 250
        if ($null -ne $candidate -and
            $candidate.Current.Name.Contains($cannotAdd) -and
            $askButton.Current.IsEnabled) {
            return $candidate.Current.Name
        }
        return $null
    }
    if ([string]::IsNullOrWhiteSpace($cDriveAnswer)) {
        throw 'The C-drive Agent conclusion did not explain non-additive source evidence.'
    }
    Show-WpfWindowForSmoke $window
    Save-WindowScreenshot $window $cDriveScreenshot

    $growthFastest = Get-UnicodeText @(0x589E, 0x957F, 0x6700, 0x5FEB)
    Invoke-Element (Find-ByAutomationId $window 'AgentDecisionQuickChoice_fastest-growth' 2000)
    $growthAnswer = Wait-Until -TimeoutSeconds 20 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 250
        if ($null -ne $candidate -and
            $candidate.Current.Name.Contains($growthFastest) -and
            $askButton.Current.IsEnabled) {
            return $candidate.Current.Name
        }
        return $null
    }
    if ([string]::IsNullOrWhiteSpace($growthAnswer)) {
        throw 'The first-snapshot growth answer was not rendered.'
    }

    Invoke-Element (Find-ByAutomationId $window 'AgentDecisionQuickChoice_safe-release' 2000)
    $storagePlanAnswer = Wait-Until -TimeoutSeconds 20 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 250
        if ($null -ne $candidate -and
            $candidate.Current.Name.Contains('10.0 GB') -and
            $askButton.Current.IsEnabled) {
            return $candidate.Current.Name
        }
        return $null
    }
    if ([string]::IsNullOrWhiteSpace($storagePlanAnswer)) {
        throw 'The requested 10 GB safety plan was not rendered.'
    }
    Show-WpfWindowForSmoke $window
    Save-WindowScreenshot $window $storagePlanScreenshot

    $whichCanUninstall = Get-UnicodeText @(
        0x4E09, 0x4E2A, 0x0020, 0x004F, 0x0070, 0x0065, 0x006E, 0x0043, 0x006F, 0x0064, 0x0065,
        0x0020, 0x4E2D, 0x54EA, 0x4E2A, 0x53EF, 0x4EE5, 0x5378, 0x8F7D, 0xFF1F)
    $familyAnswer = Ask-AgentQuestion $window $questionBox $askButton $whichCanUninstall 'OpenCode 1.14.41' 30
    if ($familyAnswer.Contains('C:\Users\Fixture')) {
        throw 'The family uninstall answer exposed a fixture path.'
    }
    Show-WpfWindowForSmoke $window
    Save-WindowScreenshot $window $familyScreenshot

    $cacheQuestion = Get-UnicodeText @(
        0x0041, 0x006E, 0x0074, 0x0069, 0x0067, 0x0072, 0x0061, 0x0076, 0x0069, 0x0074, 0x0079,
        0x0020, 0x8FC1, 0x79FB, 0x5230, 0x0020, 0x0044, 0x0020, 0x76D8, 0x540E, 0x4E3A, 0x4EC0,
        0x4E48, 0x7F13, 0x5B58, 0x8FD8, 0x5728, 0x0020, 0x0043, 0x0020, 0x76D8, 0xFF1F)
    $cacheAnswer = Ask-AgentQuestion $window $questionBox $askButton $cacheQuestion '352.0 MB' 30
    if (-not $cacheAnswer.Contains('D ') -or $cacheAnswer.Contains('C:\Users\Fixture')) {
        throw 'The cache-location answer was missing the D-drive split or exposed a path.'
    }
    $windowText = Get-DescendantText $window
    if ($windowText.Contains('C:\Users\Fixture')) {
        throw 'The visible Agent window exposed a fixture path.'
    }
    Show-WpfWindowForSmoke $window
    Save-WindowScreenshot $window $cacheScreenshot

    $quarantineManifestCount = @(Get-ChildItem -LiteralPath $quarantineRoot -Filter manifest.json -File -Recurse -ErrorAction SilentlyContinue).Count
    $uninstallEvidenceCreated = Test-Path -LiteralPath $uninstallEvidenceRoot
    [PSCustomObject]@{
        decisionPromptsVisible = $true
        cDriveConclusionVisible = $true
        growthBaselineVisible = $true
        requestedStoragePlanVisible = $true
        exactFamilyUninstallVisible = $true
        cacheLocationSplitVisible = $true
        quarantineManifestCount = $quarantineManifestCount
        uninstallEvidenceCreated = $uninstallEvidenceCreated
        noOperationExecuted = ($quarantineManifestCount -eq 0 -and -not $uninstallEvidenceCreated)
        cDriveScreenshot = $cDriveScreenshot
        storagePlanScreenshot = $storagePlanScreenshot
        familyScreenshot = $familyScreenshot
        cacheScreenshot = $cacheScreenshot
    } | ConvertTo-Json -Depth 4
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $process.Kill()
        $process.WaitForExit()
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $previousQuarantineRoot
    $env:OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT = $previousUninstallEvidenceRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousFixture
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $previousScanRoot
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $quarantineRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $uninstallEvidenceRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $knownTempFixture -Force -ErrorAction SilentlyContinue
}
