$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "wpf-smoke-helpers.ps1")
Initialize-WpfSmokeAutomation

function Get-UnicodeText {
    param([int[]]$CodePoints)

    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Select-ListItem {
    param([System.Windows.Automation.AutomationElement]$Element)

    $pattern = $Element.GetCurrentPattern(
        [System.Windows.Automation.SelectionItemPattern]::Pattern)
    $pattern.Select()
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

function Require-VisibleText {
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [string]$AutomationId,
        [string[]]$Parts
    )

    $element = Require-FullyVisibleElement $Window $AutomationId 1000
    foreach ($part in $Parts) {
        if (-not $element.Current.Name.Contains($part)) {
            throw "$AutomationId did not contain '$part': $($element.Current.Name)"
        }
    }
    return $element.Current.Name
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
$dataRoot = Join-Path $repoRoot ".omx\qa-community-cache-data"
$fixturePath = Join-Path $repoRoot ".omx\qa-community-cache-fixture.json"
$screenshotPath = Join-Path $repoRoot ".omx\qa-community-cache-conclusion.png"
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
    $oldStream = [System.IO.File]::Create($oldFile)
    $oldStream.SetLength(1572864)
    $oldStream.Dispose()
    [System.IO.File]::SetLastWriteTimeUtc($oldFile, [DateTime]::UtcNow.AddDays(-45))
    $newStream = [System.IO.File]::Create($newFile)
    $newStream.SetLength(262144)
    $newStream.Dispose()

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
    $stateJson = $state | ConvertTo-Json -Depth 6
    [System.IO.File]::WriteAllText((Join-Path $packRoot "active-state.json"), $stateJson, $utf8)

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $fixturePath
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
    Show-WpfWindowForSmoke $window

    $appsButton = Find-ByAutomationId $window "AppsNavButton" 1000
    Invoke-Element $appsButton
    $scanButton = Find-ByAutomationId $window "ScanSoftwareButton" 1000
    Invoke-Element $scanButton

    $appItem = Wait-Until -TimeoutSeconds 30 -Probe {
        Find-AppListItem $window "Fixture App"
    }
    if ($null -eq $appItem) {
        throw "Fixture application was not found."
    }
    $foundCache = Get-UnicodeText @(0x53D1, 0x73B0, 0x7F13, 0x5B58)
    if (-not $appItem.Current.Name.Contains($foundCache)) {
        $summaryElement = Find-ByAutomationId $window "AppsSummaryTextBlock" 1000
        $summaryText = if ($null -eq $summaryElement) { "missing" } else { $summaryElement.Current.Name }
        throw "The application tile did not show the cache conclusion. Tile='$($appItem.Current.Name)'; Summary='$summaryText'"
    }
    Select-ListItem $appItem
    Start-Sleep -Milliseconds 500

    $readOnlyFinding = Get-UnicodeText @(0x6269, 0x5C55, 0x89C4, 0x5219, 0x53EA, 0x8BFB, 0x53D1, 0x73B0)
    $olderThan = "30 " + (Get-UnicodeText @(0x5929, 0x4EE5, 0x4E0A))
    $promotionRefused = Get-UnicodeText @(0x5DF2, 0x62D2, 0x7EDD, 0x664B, 0x7EA7)
    $safetyFailed = Get-UnicodeText @(0x6CA1, 0x6709, 0x901A, 0x8FC7, 0x5B89, 0x5168, 0x68C0, 0x67E5)
    $promotionStopped = Get-UnicodeText @(0x5DF2, 0x505C, 0x6B62, 0x664B, 0x7EA7)
    $viewOnly = Get-UnicodeText @(0x53EA, 0x4FDD, 0x7559, 0x67E5, 0x770B)
    $cacheText = Require-VisibleText $window "DrawerCommunityCacheSummaryTextBlock" @(
        $readOnlyFinding,
        "1.8 MB",
        $olderThan,
        "1.5 MB",
        $promotionRefused)
    $agentText = Require-VisibleText $window "DrawerAdviceTextBlock" @(
        $safetyFailed,
        $promotionStopped,
        $viewOnly)
    $cleanButton = Find-ByAutomationId $window "DrawerCleanCacheButton" 1000
    if ($null -eq $cleanButton -or $cleanButton.Current.IsEnabled) {
        throw "Rule-only evidence incorrectly enabled cache cleanup."
    }

    Save-WindowScreenshot $window $screenshotPath

    [PSCustomObject]@{
        Tile = $appItem.Current.Name
        CacheConclusion = $cacheText
        AgentAdvice = $agentText
        CandidateDecision = "Refused"
        CacheCleanupEnabled = $false
        FirstViewport = $true
        OperationExecuted = $false
        Screenshot = $screenshotPath
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
