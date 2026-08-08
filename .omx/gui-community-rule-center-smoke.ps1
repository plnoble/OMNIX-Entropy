$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "wpf-smoke-helpers.ps1")
Initialize-WpfSmokeAutomation

function Get-UnicodeText {
    param([int[]]$CodePoints)

    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Find-NamedControl {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [System.Windows.Automation.ControlType]$ControlType,
        [string]$Name
    )

    $condition = [System.Windows.Automation.AndCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            $ControlType),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            $Name))
    return $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Require-TextParts {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [string]$AutomationId,
        [string[]]$Parts
    )

    $element = Require-FullyVisibleElement $Root $AutomationId 1500
    foreach ($part in $Parts) {
        if (-not $element.Current.Name.Contains($part)) {
            throw "$AutomationId did not contain '$part': $($element.Current.Name)"
        }
    }
    return $element.Current.Name
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
$dataRoot = Join-Path $repoRoot ".omx\qa-rule-center-data"
$fixturePath = Join-Path $repoRoot ".omx\qa-rule-center-fixture.json"
$statusScreenshot = Join-Path $repoRoot ".omx\qa-rule-center-status.png"
$previewScreenshot = Join-Path $repoRoot ".omx\qa-rule-center-preview.png"
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$process = $null
$utf8 = [System.Text.UTF8Encoding]::new($false)

try {
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Css.App.exe was not found: $exe"
    }
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
    $profileRoot = Join-Path $dataRoot "FixtureProfile"
    $cacheRoot = Join-Path $profileRoot "Cache"
    New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
    $oldFile = Join-Path $cacheRoot "old.tmp"
    $newFile = Join-Path $cacheRoot "new.tmp"
    $stream = [System.IO.File]::Create($oldFile)
    $stream.SetLength(1572864)
    $stream.Dispose()
    [System.IO.File]::SetLastWriteTimeUtc($oldFile, [DateTime]::UtcNow.AddDays(-45))
    $stream = [System.IO.File]::Create($newFile)
    $stream.SetLength(262144)
    $stream.Dispose()

    $profile = [ordered]@{
        name = "Fixture App"
        publisher = "OMNIX Fixture"
        category = 1
        installPath = (Join-Path $profileRoot "Install")
        dataPaths = @($profileRoot)
    }
    $fixtureJson = [ordered]@{ scans = @(,@($profile)) } | ConvertTo-Json -Depth 6
    [System.IO.File]::WriteAllText($fixturePath, $fixtureJson, $utf8)

    $packText = "[Fixture App Cache]`nFileKey1=$cacheRoot|*.tmp|RECURSE`n"
    $packBytes = $utf8.GetBytes($packText)
    $hasher = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $hasher.ComputeHash($packBytes)
    }
    finally {
        $hasher.Dispose()
    }
    $sha256 = -join ($hashBytes | ForEach-Object { $_.ToString("X2") })
    $packRoot = Join-Path $dataRoot "RulePacks\Winapp2"
    $packsRoot = Join-Path $packRoot "packs"
    New-Item -ItemType Directory -Path $packsRoot -Force | Out-Null
    [System.IO.File]::WriteAllBytes((Join-Path $packsRoot ($sha256 + ".ini")), $packBytes)
    $descriptor = [ordered]@{
        sourceName = "OMNIX GUI fixture"
        sourceUri = "https://example.invalid/winapp2.ini"
        version = "fixture-v1"
        licenseName = "Fixture-only"
        licenseUri = "https://example.invalid/license"
        expectedSha256 = $sha256
    }
    $state = [ordered]@{
        schemaVersion = 1
        activeDescriptor = $descriptor
        previousDescriptor = $null
        activatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    }
    [System.IO.File]::WriteAllText(
        (Join-Path $packRoot "active-state.json"),
        ($state | ConvertTo-Json -Depth 6),
        $utf8)

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $fixturePath
    $process = Start-Process -FilePath $exe -PassThru
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $mainWindow = Wait-Until -TimeoutSeconds 20 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $processCondition)
    }
    if ($null -eq $mainWindow) {
        throw "Main window was not found."
    }
    Show-WpfWindowForSmoke $mainWindow
    Invoke-Element (Find-ByAutomationId $mainWindow "AppsNavButton" 1000)
    Invoke-Element (Find-ByAutomationId $mainWindow "ScanSoftwareButton" 1000)
    $ruleButton = Wait-Until -TimeoutSeconds 30 -Probe {
        $candidate = Find-ByAutomationId $mainWindow "CommunityRulesButton" 500
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) { $candidate } else { $null }
    }
    if ($null -eq $ruleButton) {
        throw "Rule-center button was not ready."
    }
    Invoke-Element $ruleButton

    $ruleWindow = Wait-Until -TimeoutSeconds 20 -Probe {
        Find-SecondaryWindowWithChild `
            -ProcessId $process.Id `
            -MainWindowHandle $mainWindow.Current.NativeWindowHandle `
            -ChildAutomationId "RuleCenterStatusHeadlineTextBlock"
    }
    if ($null -eq $ruleWindow) {
        throw "Rule-center window was not found."
    }
    Show-WpfWindowForSmoke $ruleWindow

    $enabledText = Get-UnicodeText @(0x6269, 0x5C55, 0x89C4, 0x5219, 0x5DF2, 0x542F, 0x7528)
    $statusText = Require-TextParts $ruleWindow "RuleCenterStatusHeadlineTextBlock" @($enabledText)
    $sourceText = Require-TextParts $ruleWindow "RuleCenterSourceTextBlock" @("OMNIX GUI fixture", "example.invalid")
    $licenseText = Require-TextParts $ruleWindow "RuleCenterLicenseTextBlock" @("Fixture-only", "example.invalid")
    $versionText = Require-TextParts $ruleWindow "RuleCenterVersionTextBlock" @("fixture-v1")
    $noCleanup = Get-UnicodeText @(0x4E0D, 0x4F1A, 0x521B, 0x5EFA, 0x6E05, 0x7406, 0x64CD, 0x4F5C)
    $noDelete = Get-UnicodeText @(0x4E0D, 0x4F1A, 0x5220, 0x9664, 0x6587, 0x4EF6)
    $safetyText = Require-TextParts $ruleWindow "RuleCenterSafetyTextBlock" @($noCleanup, $noDelete)
    Save-WindowScreenshot $ruleWindow $statusScreenshot

    $previewTabName = Get-UnicodeText @(0x53EA, 0x8BFB, 0x9884, 0x89C8)
    $previewTab = Find-NamedControl $ruleWindow ([System.Windows.Automation.ControlType]::TabItem) $previewTabName
    if ($null -eq $previewTab) {
        throw "Read-only preview tab was not found."
    }
    $selection = $previewTab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $selection.Select()
    Start-Sleep -Milliseconds 300

    $oneFinding = (Get-UnicodeText @(0x53D1, 0x73B0)) + " 1 " + (Get-UnicodeText @(0x6761, 0x53EA, 0x8BFB, 0x53D1, 0x73B0))
    $largest = Get-UnicodeText @(0x6700, 0x5927, 0x4E00, 0x6761, 0x81F3, 0x5C11)
    $noDirect = Get-UnicodeText @(0x4E0D, 0x4F1A, 0x76F4, 0x63A5, 0x6E05, 0x7406)
    $noSafeCandidate = Get-UnicodeText @(0x6CA1, 0x6709, 0x53D1, 0x73B0, 0x901A, 0x8FC7, 0x7B2C, 0x4E00, 0x8F6E, 0x5B89, 0x5168, 0x7B5B, 0x9009)
    $previewSummary = Require-TextParts $ruleWindow "RuleCenterPreviewSummaryTextBlock" @(
        $oneFinding,
        $largest,
        "1.8 MB",
        $noSafeCandidate,
        $noDirect)
    $previewList = Require-FullyVisibleElement $ruleWindow "RuleCenterPreviewListBox" 1000
    Save-WindowScreenshot $ruleWindow $previewScreenshot
    $listItem = Find-NamedControl $previewList ([System.Windows.Automation.ControlType]::ListItem) "Fixture App: at least 1.8 MB; at least 1.5 MB older than 30 days, read-only preview"
    if ($null -eq $listItem) {
        $items = $previewList.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::ListItem))
        $itemNames = @($items | ForEach-Object { $_.Current.Name })
        if ($items.Count -ne 1 -or -not $items[0].Current.Name.Contains("Fixture App")) {
            throw "Expected one Fixture App preview row. Count=$($items.Count); Names=$($itemNames -join ' | ')"
        }
        $listItem = $items[0]
    }
    $promotionRefused = Get-UnicodeText @(0x5DF2, 0x62D2, 0x7EDD)
    if (-not $listItem.Current.Name.Contains($promotionRefused)) {
        throw "Fixture rule was not refused by the approved-user-data boundary: $($listItem.Current.Name)"
    }

    $itemSelection = $listItem.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $itemSelection.Select()
    $ignoreButton = Wait-Until -TimeoutSeconds 5 -Probe {
        $candidate = Find-ByAutomationId $ruleWindow "RuleCenterIgnoreRuleButton" 500
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) { $candidate } else { $null }
    }
    if ($null -eq $ignoreButton) {
        throw "Ignore-rule button did not become enabled."
    }
    Invoke-Element $ignoreButton

    $ignoredList = Require-FullyVisibleElement $ruleWindow "RuleCenterIgnoredRulesListBox" 1000
    $ignoredItem = Wait-Until -TimeoutSeconds 5 -Probe {
        $items = $ignoredList.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::ListItem))
        if ($items.Count -eq 1 -and $items[0].Current.Name.Contains("Fixture App")) {
            $items[0]
        }
        else {
            $null
        }
    }
    if ($null -eq $ignoredItem) {
        throw "Ignored rule did not move to the managed preference list."
    }
    $ignoredSelection = $ignoredItem.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $ignoredSelection.Select()
    $restoreButton = Wait-Until -TimeoutSeconds 5 -Probe {
        $candidate = Find-ByAutomationId $ruleWindow "RuleCenterRestoreRuleButton" 500
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) { $candidate } else { $null }
    }
    if ($null -eq $restoreButton) {
        throw "Restore-rule button did not become enabled."
    }
    Invoke-Element $restoreButton

    $restoredItem = Wait-Until -TimeoutSeconds 5 -Probe {
        $items = $previewList.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::ListItem))
        if ($items.Count -eq 1 -and $items[0].Current.Name.Contains("Fixture App")) {
            $items[0]
        }
        else {
            $null
        }
    }
    if ($null -eq $restoredItem) {
        throw "Restored rule did not return to the read-only preview."
    }

    [PSCustomObject]@{
        Status = $statusText
        Source = $sourceText
        License = $licenseText
        Version = $versionText
        Safety = $safetyText
        Preview = $previewSummary
        CandidateDecision = "Refused"
        PreferenceRoundTrip = $true
        OperationExecuted = $false
        StatusScreenshot = $statusScreenshot
        PreviewScreenshot = $previewScreenshot
    } | ConvertTo-Json -Depth 4
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousFixture
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
}
