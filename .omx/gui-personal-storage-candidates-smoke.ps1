$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Get-UnicodeText([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Get-DescendantText($Element) {
    $textCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    $texts = $Element.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $textCondition)
    return (($texts | ForEach-Object { $_.Current.Name }) -join ' ')
}

function Get-ListItems($Element) {
    $condition = [System.Windows.Automation.OrCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::DataItem))
    return $Element.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $condition)
}

function Find-ButtonByName($Element, [string]$Name) {
    $condition = [System.Windows.Automation.AndCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Button),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            $Name))
    return $Element.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        $condition)
}

function Assert-ConfinedPath([string]$Path, [string]$Root) {
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    $fullRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd('\')
    if ($fullPath.Equals($fullRoot, [StringComparison]::OrdinalIgnoreCase) -or
        -not $fullPath.StartsWith($fullRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing recursive fixture cleanup outside the expected root: $fullPath"
    }
}

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$dataRoot = Join-Path $PSScriptRoot 'qa-personal-storage-data'
$scanRoot = Join-Path 'C:\tmp' ('OMNIX-PersonalStorage-Smoke-' + [Guid]::NewGuid().ToString('N'))
$personalRoot = Join-Path $scanRoot 'Downloads'
$screenshotPath = Join-Path $PSScriptRoot 'qa-personal-storage-candidates.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousScanRoot = $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT
$previousPersonalRoot = $env:OMNIX_ENTROPY_PERSONAL_STORAGE_ROOT
$process = $null

Assert-ConfinedPath $dataRoot $PSScriptRoot
Assert-ConfinedPath $scanRoot 'C:\tmp'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

try {
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
    $copyA = Join-Path $personalRoot 'A'
    $copyB = Join-Path $personalRoot 'B'
    New-Item -ItemType Directory -Path $copyA -Force | Out-Null
    New-Item -ItemType Directory -Path $copyB -Force | Out-Null

    $oldVideo = Join-Path $personalRoot 'old-video.mp4'
    [System.IO.File]::WriteAllBytes($oldVideo, (New-Object byte[] (12 * 1024)))
    [System.IO.File]::SetLastWriteTimeUtc($oldVideo, [DateTime]::UtcNow.AddDays(-60))
    [System.IO.File]::WriteAllBytes(
        (Join-Path $copyA 'archive.zip'),
        (New-Object byte[] (4 * 1024)))
    [System.IO.File]::WriteAllBytes(
        (Join-Path $copyB 'archive.zip'),
        (New-Object byte[] (4 * 1024)))

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $scanRoot
    $env:OMNIX_ENTROPY_PERSONAL_STORAGE_ROOT = $personalRoot
    $process = Start-Process -FilePath $exe -PassThru

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 20 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }
    if ($null -eq $window) { throw 'Main window was not found.' }
    Show-WpfWindowForSmoke $window

    Invoke-Element (Find-ByAutomationId $window 'StartScanButton' 5000)
    $longUnused = Get-UnicodeText @(0x957F, 0x671F, 0x672A, 0x7528)
    $possibleDuplicate = Get-UnicodeText @(0x7591, 0x4F3C, 0x91CD, 0x590D)
    $findings = Wait-Until -TimeoutSeconds 60 -Probe {
        $candidate = Find-ByAutomationId $window 'KeyFindingsListBox' 500
        if ($null -eq $candidate) { return $null }
        $text = Get-DescendantText $candidate
        if ($text.Contains($longUnused) -and $text.Contains($possibleDuplicate)) { return $candidate }
        return $null
    }
    if ($null -eq $findings) { throw 'Personal-storage Home findings were not rendered.' }

    $personalFinding = $null
    foreach ($item in (Get-ListItems $findings)) {
        if ((Get-DescendantText $item).Contains($longUnused)) {
            $personalFinding = $item
            break
        }
    }
    if ($null -eq $personalFinding) { throw 'Long-unused Home finding was not found.' }
    $detailsName = Get-UnicodeText @(0x67E5, 0x770B, 0x8BE6, 0x60C5)
    $detailsButton = Find-ButtonByName $personalFinding $detailsName
    if ($null -eq $detailsButton) { throw 'Personal-storage detail button was not found.' }
    Invoke-Element $detailsButton

    $navigate = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'HomeAgentResponseNavigateButton' 500
        if ($null -ne $candidate -and $candidate.Current.IsEnabled -and -not $candidate.Current.IsOffscreen) {
            return $candidate
        }
        return $null
    }
    if ($null -eq $navigate) { throw 'Personal-storage exact navigation was not visible.' }
    $candidateLabel = Get-UnicodeText @(0x67E5, 0x770B, 0x4E2A, 0x4EBA, 0x6587, 0x4EF6, 0x5019, 0x9009)
    if (-not $navigate.Current.Name.Contains($candidateLabel)) {
        throw 'Home Agent did not offer the personal-storage destination.'
    }
    $agentBody = Find-ByAutomationId $window 'HomeAgentResponseBodyTextBlock' 1000
    $agentSafety = Find-ByAutomationId $window 'HomeAgentResponseSafetyTextBlock' 1000
    $agentVisibleText = $agentBody.Current.Name + ' ' + $agentSafety.Current.Name
    if ($agentVisibleText.Contains('C:\tmp') -or $agentVisibleText.Contains($scanRoot)) {
        throw 'Home Agent exposed the personal-storage fixture path.'
    }

    Invoke-Element $navigate
    $summary = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'PersonalStorageSummaryTextBlock' 500
        if ($null -ne $candidate -and -not $candidate.Current.IsOffscreen) { return $candidate }
        return $null
    }
    $list = Find-ByAutomationId $window 'PersonalStorageFindingsListBox' 2000
    if ($null -eq $summary -or $null -eq $list -or $list.Current.IsOffscreen) {
        throw 'Personal-storage destination was not visible after navigation.'
    }
    $candidateWords = Get-UnicodeText @(0x4E2A, 0x4EBA, 0x6587, 0x4EF6, 0x5019, 0x9009)
    $noAutoDelete = Get-UnicodeText @(0x4E0D, 0x4F1A, 0x81EA, 0x52A8, 0x5220, 0x9664)
    if (-not $summary.Current.Name.Contains($candidateWords) -or
        -not $summary.Current.Name.Contains($noAutoDelete)) {
        throw 'Personal-storage summary did not explain its read-only boundary.'
    }

    $candidateItems = Get-ListItems $list
    if ($candidateItems.Count -ne 2) {
        throw "Expected exactly two personal-storage candidates, found $($candidateItems.Count)."
    }
    $combinedText = Get-DescendantText $list
    $readOnlyCandidate = Get-UnicodeText @(0x53EA, 0x8BFB, 0x5019, 0x9009)
    $noContentComparison = Get-UnicodeText @(0x6CA1, 0x6709, 0x8BFB, 0x53D6, 0x6216, 0x6BD4, 0x5BF9, 0x6587, 0x4EF6, 0x5185, 0x5BB9)
    foreach ($required in @('old-video.mp4', 'archive.zip', $possibleDuplicate, $readOnlyCandidate, $noContentComparison)) {
        if (-not $combinedText.Contains($required)) {
            throw "Personal-storage first level missed required text: $required"
        }
    }
    foreach ($item in $candidateItems) {
        if (-not $item.Current.AutomationId.StartsWith('PersonalStorageFinding_', [StringComparison]::Ordinal)) {
            throw 'Personal-storage item did not expose its stable AutomationId.'
        }
        if ($item.Current.IsOffscreen) {
            throw "Personal-storage candidate was not actually visible after exact navigation: $($item.Current.AutomationId)"
        }
    }
    if ($combinedText.Contains('C:\tmp') -or $combinedText.Contains($scanRoot)) {
        throw 'Personal-storage first level exposed the fixture path.'
    }

    $visibleWindowText = Get-DescendantText $window
    if ($visibleWindowText.Contains('C:\tmp') -or $visibleWindowText.Contains($scanRoot)) {
        throw 'The visible beginner window exposed the personal-storage fixture path.'
    }

    Start-Sleep -Milliseconds 750
    Save-WindowScreenshot $window $screenshotPath
    [PSCustomObject]@{
        candidateCount = $candidateItems.Count
        exactSectionNavigation = $true
        stableCandidateAutomationIds = $true
        fullPathsHidden = $true
        contentComparisonClaimed = $false
        noOperationExecuted = $true
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $previousScanRoot
    $env:OMNIX_ENTROPY_PERSONAL_STORAGE_ROOT = $previousPersonalRoot
    Assert-ConfinedPath $dataRoot $PSScriptRoot
    Assert-ConfinedPath $scanRoot 'C:\tmp'
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
}
