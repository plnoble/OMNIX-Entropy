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

function Scroll-ElementIntoView {
    param([System.Windows.Automation.AutomationElement]$Element)

    try {
        $pattern = $Element.GetCurrentPattern(
            [System.Windows.Automation.ScrollItemPattern]::Pattern)
        $pattern.ScrollIntoView()
        Start-Sleep -Milliseconds 250
    }
    catch {
        # The following visibility assertion remains authoritative.
    }
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

function Assert-NonBlankScreenshot {
    param([string]$Path)

    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        $sampleCount = 0
        $visibleCount = 0
        for ($x = 0; $x -lt $bitmap.Width; $x += 32) {
            for ($y = 0; $y -lt $bitmap.Height; $y += 32) {
                $pixel = $bitmap.GetPixel($x, $y)
                $sampleCount++
                if (($pixel.R + $pixel.G + $pixel.B) -gt 60) {
                    $visibleCount++
                }
            }
        }
        if ($sampleCount -eq 0 -or ($visibleCount / $sampleCount) -lt 0.1) {
            throw "Screenshot was blank or nearly black: $Path"
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
$dataRoot = Join-Path $repoRoot ".omx\qa-community-preview-data"
$fixturePath = Join-Path $repoRoot ".omx\qa-community-preview-fixture.json"
$previewScreenshot = Join-Path $repoRoot ".omx\qa-community-cache-preview.png"
$confirmationScreenshot = Join-Path $repoRoot ".omx\qa-community-cache-confirmation.png"
$localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
$candidateRoot = Join-Path $localAppData ("OMNIX-Entropy\Qa\CommunityCachePreview-" + [Guid]::NewGuid().ToString("N"))
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$process = $null
$utf8 = [System.Text.UTF8Encoding]::new($false)

try {
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Css.App.exe was not found: $exe"
    }
    if ([string]::IsNullOrWhiteSpace($localAppData)) {
        throw "Local application data root was unavailable."
    }

    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
    $cacheRoot = Join-Path $candidateRoot "Example\Cache"
    New-Item -ItemType Directory -Path $cacheRoot -Force | Out-Null
    $oldFile = Join-Path $cacheRoot "old.tmp"
    $recentFile = Join-Path $cacheRoot "recent.tmp"
    $stream = [System.IO.File]::Create($oldFile)
    $stream.SetLength(1572864)
    $stream.Dispose()
    [System.IO.File]::SetLastWriteTimeUtc($oldFile, [DateTime]::UtcNow.AddDays(-45))
    $stream = [System.IO.File]::Create($recentFile)
    $stream.SetLength(262144)
    $stream.Dispose()

    $profile = [ordered]@{
        name = "Fixture App"
        publisher = "OMNIX Fixture"
        displayVersion = "1.0"
        inventorySource = "HKCU\Software\OMNIX-Entropy-QA-Fixture"
        category = 1
        installPath = "D:\Software\OMNIX-Entropy-QA-Fixture\Install"
        dataPaths = @($candidateRoot)
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
        $root.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $processCondition)
    }
    if ($null -eq $mainWindow) {
        throw "Main window was not found."
    }
    Show-WpfWindowForSmoke $mainWindow
    Invoke-Element (Find-ByAutomationId $mainWindow "AppsNavButton" 1000)
    Invoke-Element (Find-ByAutomationId $mainWindow "ScanSoftwareButton" 1000)

    $appItem = Wait-Until -TimeoutSeconds 30 -Probe {
        Find-AppListItem $mainWindow "Fixture App"
    }
    if ($null -eq $appItem) {
        throw "Fixture application was not found."
    }
    Select-ListItem $appItem
    Start-Sleep -Milliseconds 400

    $firstRound = Get-UnicodeText @(0x901A, 0x8FC7, 0x7B2C, 0x4E00, 0x8F6E, 0x7B5B, 0x9009)
    $safePreview = Get-UnicodeText @(0x5B89, 0x5168, 0x9884, 0x6F14)
    $communityConclusion = Require-TextParts $mainWindow "DrawerCommunityCacheSummaryTextBlock" @(
        $firstRound,
        $safePreview)
    $cleanButton = Find-ByAutomationId $mainWindow "DrawerCleanCacheButton" 1000
    if ($null -eq $cleanButton -or -not $cleanButton.Current.IsEnabled) {
        throw "Eligible exact-file evidence did not enable the preview entry."
    }
    Invoke-Element $cleanButton

    $oldFiles = Get-UnicodeText @(0x65E7, 0x7F13, 0x5B58, 0x6587, 0x4EF6)
    $canUndo = Get-UnicodeText @(0x53EF, 0x4EE5, 0x5728, 0x540E, 0x6094, 0x836F, 0x4E2D, 0x5FC3, 0x8FD8, 0x539F)
    $previewSummary = Require-TextParts $mainWindow "DrawerActionPreviewSummaryTextBlock" @(
        "1",
        $oldFiles,
        "1.5 MB")
    $previewSafety = Require-TextParts $mainWindow "DrawerActionPreviewSafetyTextBlock" @($canUndo)
    $previewButton = Find-ByAutomationId $mainWindow "DrawerActionPreviewPrimaryButton" 1000
    if ($null -eq $previewButton) {
        throw "The revalidation entry was not found."
    }
    Scroll-ElementIntoView $previewButton
    if ($previewButton.Current.IsOffscreen) {
        $drawerScroll = Find-ByAutomationId $mainWindow "AppDrawerScrollViewer" 1000
        if ($null -ne $drawerScroll) {
            try {
                $scrollPattern = $drawerScroll.GetCurrentPattern(
                    [System.Windows.Automation.ScrollPattern]::Pattern)
                $scrollPattern.SetScrollPercent(
                    [System.Windows.Automation.ScrollPattern]::NoScroll,
                    100)
                Start-Sleep -Milliseconds 250
            }
            catch {
                # The strict visibility assertion below still decides the result.
            }
        }
    }
    $previewButton = Require-FullyVisibleElement $mainWindow "DrawerActionPreviewPrimaryButton" 1000
    if (-not $previewButton.Current.IsEnabled) {
        throw "The revalidation entry was disabled."
    }
    Start-Sleep -Seconds 6
    Show-WpfWindowForSmoke $mainWindow
    $previewButton = Require-FullyVisibleElement $mainWindow "DrawerActionPreviewPrimaryButton" 1000
    Require-TextParts $mainWindow "DrawerActionPreviewSummaryTextBlock" @(
        "1",
        $oldFiles,
        "1.5 MB") | Out-Null
    Save-WindowScreenshot $mainWindow $previewScreenshot
    Assert-NonBlankScreenshot $previewScreenshot
    Invoke-Element $previewButton

    $confirmationWindow = Wait-Until -TimeoutSeconds 30 -Probe {
        Find-SecondaryWindowWithChild `
            -ProcessId $process.Id `
            -MainWindowHandle $mainWindow.Current.NativeWindowHandle `
            -ChildAutomationId "CleanupConfirmationSummaryTextBlock"
    }
    if ($null -eq $confirmationWindow) {
        throw "Cleanup confirmation window was not found."
    }
    Show-WpfWindowForSmoke $confirmationWindow
    $exactFiles = Get-UnicodeText @(0x7CBE, 0x786E, 0x65E7, 0x7F13, 0x5B58, 0x6587, 0x4EF6)
    $undoCenter = Get-UnicodeText @(0x540E, 0x6094, 0x836F, 0x4E2D, 0x5FC3)
    $confirmationText = Require-TextParts $confirmationWindow "CleanupConfirmationSummaryTextBlock" @(
        "1",
        $exactFiles,
        "1.5 MB",
        $undoCenter)
    Require-FullyVisibleElement $confirmationWindow "CleanupConfirmationOutcomeListBox" 1000 | Out-Null
    Save-WindowScreenshot $confirmationWindow $confirmationScreenshot
    Assert-NonBlankScreenshot $confirmationScreenshot
    Invoke-Element (Find-ByAutomationId $confirmationWindow "CleanupConfirmationCancelButton" 1000)
    Start-Sleep -Milliseconds 500

    if (-not (Test-Path -LiteralPath $oldFile) -or -not (Test-Path -LiteralPath $recentFile)) {
        throw "Cancel changed a fixture file."
    }
    $quarantineRoot = Join-Path $dataRoot "Quarantine"
    $manifestCount = 0
    if (Test-Path -LiteralPath $quarantineRoot) {
        $manifestCount = @(Get-ChildItem -LiteralPath $quarantineRoot -Filter "*.json" -File -Recurse).Count
    }
    if ($manifestCount -ne 0) {
        throw "Cancel created quarantine manifests."
    }

    [PSCustomObject]@{
        CandidateDecision = "EligibleForSafePreview"
        CommunityConclusion = $communityConclusion
        PreviewSummary = $previewSummary
        PreviewSafety = $previewSafety
        Confirmation = $confirmationText
        ExactFileCount = 1
        RecentFileKept = (Test-Path -LiteralPath $recentFile)
        ConfirmationCanceled = $true
        OperationExecuted = $false
        FirstViewport = $true
        PreviewScreenshot = $previewScreenshot
        ConfirmationScreenshot = $confirmationScreenshot
    } | ConvertTo-Json -Depth 4
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousFixture
    if (Test-Path -LiteralPath $candidateRoot) {
        Remove-Item -LiteralPath $candidateRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
}
