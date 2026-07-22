$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$homeScreenshotPath = Join-Path $PSScriptRoot 'qa-migration-closure-home.png'
$appScreenshotPath = Join-Path $PSScriptRoot 'qa-migration-closure-app.png'
$runId = [Guid]::NewGuid().ToString('N')
$cTempRoot = [IO.Path]::GetFullPath('C:\tmp').TrimEnd('\')
$omxRoot = [IO.Path]::GetFullPath($PSScriptRoot).TrimEnd('\')
$sourceRoot = Join-Path $cTempRoot "omnix-migration-closure-$runId"
$sourcePath = Join-Path $sourceRoot 'cache'
$targetRoot = Join-Path $PSScriptRoot "qa-migration-closure-target-$runId"
$targetPath = Join-Path $targetRoot 'cache'
$isolatedDataRoot = Join-Path $PSScriptRoot "qa-migration-closure-data-$runId"
$scanRoot = Join-Path $PSScriptRoot "qa-migration-closure-scan-$runId"
$softwareFixturePath = Join-Path $PSScriptRoot "qa-migration-closure-software-$runId.json"
$monitoringRoot = Join-Path $isolatedDataRoot 'MigrationRollback\Monitoring'
$monitoringPath = Join-Path $monitoringRoot "migration-monitor-$runId.json"
$appName = 'Closure Fixture App'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousScanRoot = $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT
$previousSoftwareFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$process = $null
$migrationActionInvoked = $false

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Join-Chars {
    param([int[]]$Codes)

    $characters = New-Object System.Collections.Generic.List[char]
    foreach ($code in $Codes) {
        $characters.Add([char]$code)
    }
    return -join $characters
}

$uiText = @{
    ClosureDimension = Join-Chars @(0x8FC1, 0x79FB, 0x95ED, 0x73AF)
    ClosureDimensionAutomationId = 'HealthDimension_' + (Join-Chars @(0x8FC1, 0x79FB, 0x95ED, 0x73AF))
    ClosureWarning = Join-Chars @(0x8FC1, 0x79FB, 0x6CA1, 0x6709, 0x95ED, 0x73AF)
    ClosureTag = Join-Chars @(0x8FC1, 0x79FB, 0x672A, 0x95ED, 0x73AF)
    ReviewMigration = Join-Chars @(0x590D, 0x67E5, 0x8FC1, 0x79FB)
}

if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}
if (-not (Test-Path -LiteralPath $cTempRoot -PathType Container)) {
    throw "The isolated C-drive smoke root does not exist: $cTempRoot"
}

function Assert-ChildPath {
    param(
        [string]$Path,
        [string]$AllowedRoot
    )

    $fullPath = [IO.Path]::GetFullPath($Path).TrimEnd('\')
    $fullRoot = [IO.Path]::GetFullPath($AllowedRoot).TrimEnd('\')
    if (-not $fullPath.StartsWith($fullRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Fixture path escaped its allowed root: $fullPath"
    }
    return $fullPath
}

function Remove-IsolatedFixturePath {
    param(
        [string]$Path,
        [string]$AllowedRoot
    )

    $fullPath = Assert-ChildPath $Path $AllowedRoot
    if (Test-Path -LiteralPath $fullPath) {
        Remove-Item -LiteralPath $fullPath -Recurse -Force
    }
}

function Find-TextContaining {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [string[]]$Parts
    )

    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    $items = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    foreach ($item in $items) {
        $name = $item.Current.Name
        if ([string]::IsNullOrWhiteSpace($name)) {
            continue
        }
        $matches = $true
        foreach ($part in $Parts) {
            if ($name.IndexOf($part, [StringComparison]::CurrentCultureIgnoreCase) -lt 0) {
                $matches = $false
                break
            }
        }
        if ($matches) {
            return $item
        }
    }
    return $null
}

function Find-AppTile {
    param(
        [System.Windows.Automation.AutomationElement]$List,
        [string]$Name,
        [string]$Tag
    )

    $condition = [System.Windows.Automation.OrCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::DataItem))
    $items = $List.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    foreach ($item in $items) {
        $accessibleName = $item.Current.Name
        if ($accessibleName.Contains($Name) -and $accessibleName.Contains($Tag)) {
            return $item
        }
    }
    return $null
}

function Select-UiItem {
    param([System.Windows.Automation.AutomationElement]$Element)

    $pattern = $Element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $pattern.Select()
}

function Get-VisibleText {
    param([System.Windows.Automation.AutomationElement]$Root)

    $names = New-Object System.Collections.Generic.List[string]
    $items = $Root.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($item in $items) {
        try {
            if (-not $item.Current.IsOffscreen -and -not [string]::IsNullOrWhiteSpace($item.Current.Name)) {
                $names.Add($item.Current.Name)
            }
        }
        catch {
            continue
        }
    }
    return $names -join "`n"
}

function Get-HealthRowDiagnostics {
    param([System.Windows.Automation.AutomationElement]$Window)

    $list = Find-ByAutomationId $Window 'HealthDimensionListView' 250
    if ($null -eq $list) {
        return 'health-list-missing'
    }
    $condition = [System.Windows.Automation.OrCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::DataItem))
    $rows = $list.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    $values = New-Object System.Collections.Generic.List[string]
    foreach ($row in $rows) {
        $values.Add("$($row.Current.AutomationId)|$($row.Current.Name)")
    }
    return $values -join ';'
}

try {
    $sourceRoot = Assert-ChildPath $sourceRoot $cTempRoot
    $sourcePath = Assert-ChildPath $sourcePath $sourceRoot
    $targetRoot = Assert-ChildPath $targetRoot $omxRoot
    $targetPath = Assert-ChildPath $targetPath $targetRoot
    $isolatedDataRoot = Assert-ChildPath $isolatedDataRoot $omxRoot
    $scanRoot = Assert-ChildPath $scanRoot $omxRoot
    $softwareFixturePath = Assert-ChildPath $softwareFixturePath $omxRoot

    New-Item -ItemType Directory -Path $sourcePath -Force | Out-Null
    New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
    New-Item -ItemType Directory -Path $monitoringRoot -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $scanRoot 'Temp') -Force | Out-Null
    [IO.File]::WriteAllBytes(
        (Join-Path $scanRoot 'Temp\fixture-cache.bin'),
        (New-Object byte[] 4096))

    $profile = [ordered]@{
        name = $appName
        publisher = 'OMNIX Smoke'
        installPath = 'D:\Software\ClosureFixture\Install'
        installedSizeBytes = 1048576
        dataPaths = @($sourcePath)
        cDriveWritePaths = @($sourcePath)
    }
    $softwareFixture = [ordered]@{
        scans = @((,$profile), (,$profile))
    }
    $softwareFixture | ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath $softwareFixturePath -Encoding UTF8

    $monitoringRecord = [ordered]@{
        Id = "migration-monitor-$runId"
        SoftwareName = $appName
        SnapshotId = "fixture-snapshot-$runId"
        RollbackManifestPath = (Join-Path $targetRoot 'fixture-rollback.json')
        RollbackManifestSha256 = ('A' * 64)
        CreatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
        Paths = @(
            [ordered]@{
                OriginalPath = $sourcePath
                ExpectedRedirectTarget = $targetPath
            }
        )
    }
    $monitoringRecord | ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath $monitoringPath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $scanRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $softwareFixturePath
    $process = Start-Process -FilePath $exe -PassThru

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 20 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $processCondition)
    }
    if ($null -eq $window) {
        throw 'Main window was not found.'
    }
    Show-WpfWindowForSmoke $window

    $startScan = Find-ByAutomationId $window 'StartScanButton' 1000
    if ($null -eq $startScan) {
        throw 'StartScanButton was not found.'
    }
    Invoke-Element $startScan

    $closureDimension = Wait-Until -TimeoutSeconds 60 -Probe {
        Find-ByAutomationId $window $uiText.ClosureDimensionAutomationId 250
    }
    if ($null -eq $closureDimension) {
        Save-WindowScreenshot $window $homeScreenshotPath
        $status = Find-ByAutomationId $window 'StatusTextBlock' 250
        $statusText = if ($null -eq $status) { 'status-missing' } else { $status.Current.Name }
        $rows = Get-HealthRowDiagnostics $window
        $closureText = Find-TextContaining $window @($uiText.ClosureDimension)
        $closureTextFound = $null -ne $closureText
        throw "The migration-closure health dimension was not rendered. Status='$statusText'; rows='$rows'; closureTextFound=$closureTextFound"
    }
    $closureDimensionName = $closureDimension.Current.Name
    $closureDimensionAutomationId = $closureDimension.Current.AutomationId

    $keyFindings = Find-ByAutomationId $window 'KeyFindingsListBox' 1000
    if ($null -eq $keyFindings) {
        throw 'KeyFindingsListBox was not found.'
    }
    $homeWarning = Wait-Until -TimeoutSeconds 10 -Probe {
        Find-TextContaining $keyFindings @($appName, $uiText.ClosureWarning)
    }
    if ($null -eq $homeWarning -or $homeWarning.Current.IsOffscreen) {
        throw 'The path-free migration warning was not visible on Home.'
    }

    $homeVisibleText = (Get-VisibleText $closureDimension) + "`n" + (Get-VisibleText $keyFindings)
    if ($homeVisibleText.Contains($sourcePath) -or $homeVisibleText.Contains($targetPath)) {
        throw 'A raw fixture path leaked into the beginner Home warning.'
    }
    Save-WindowScreenshot $window $homeScreenshotPath

    $appsButton = Find-ByAutomationId $window 'AppsNavButton' 1000
    if ($null -eq $appsButton) {
        throw 'AppsNavButton was not found.'
    }
    Invoke-Element $appsButton

    $scanSoftware = Find-ByAutomationId $window 'ScanSoftwareButton' 1000
    if ($null -eq $scanSoftware) {
        throw 'ScanSoftwareButton was not found.'
    }
    Invoke-Element $scanSoftware

    $appList = Find-ByAutomationId $window 'AppTilesListBox' 1000
    if ($null -eq $appList) {
        throw 'AppTilesListBox was not found.'
    }
    $appTile = Wait-Until -TimeoutSeconds 45 -Probe {
        Find-AppTile $appList $appName $uiText.ClosureTag
    }
    if ($null -eq $appTile) {
        throw 'The matching application did not show the migration-closure warning tag.'
    }
    Select-UiItem $appTile

    $drawerTitle = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'DrawerTitleTextBlock' 250
        if ($null -ne $candidate -and $candidate.Current.Name -eq $appName) {
            return $candidate
        }
        return $null
    }
    $drawerAdvice = Find-ByAutomationId $window 'DrawerAdviceTextBlock' 1000
    $migrationButton = Find-ByAutomationId $window 'DrawerMigrateButton' 1000
    if ($null -eq $drawerTitle -or $null -eq $drawerAdvice -or $null -eq $migrationButton) {
        throw 'The application drawer closure controls were not found.'
    }
    if (-not $drawerAdvice.Current.Name.Contains($uiText.ClosureWarning)) {
        throw 'The application drawer did not repeat the migration-closure warning.'
    }
    if ($migrationButton.Current.Name -ne $uiText.ReviewMigration -or -not $migrationButton.Current.IsEnabled) {
        throw 'The safe migration-review button was not visible and enabled.'
    }

    $appVisibleText = Get-VisibleText $window
    if ($appVisibleText.Contains($sourcePath) -or $appVisibleText.Contains($targetPath)) {
        throw 'A raw fixture path leaked into the beginner application view.'
    }
    Save-WindowScreenshot $window $appScreenshotPath

    [PSCustomObject]@{
        closureDimension = $closureDimensionName
        closureDimensionAutomationId = $closureDimensionAutomationId
        homeWarningVisible = $true
        appTileName = $appTile.Current.Name
        drawerAdviceVisible = -not $drawerAdvice.Current.IsOffscreen
        migrationReviewLabel = $migrationButton.Current.Name
        migrationReviewEnabled = $migrationButton.Current.IsEnabled
        rawFixturePathHidden = $true
        noOperationExecuted = -not $migrationActionInvoked
        sourceWasOrdinaryDirectory = (Test-Path -LiteralPath $sourcePath -PathType Container)
        homeScreenshot = $homeScreenshotPath
        appScreenshot = $appScreenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $previousScanRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousSoftwareFixture

    Remove-IsolatedFixturePath $sourceRoot $cTempRoot
    Remove-IsolatedFixturePath $targetRoot $omxRoot
    Remove-IsolatedFixturePath $isolatedDataRoot $omxRoot
    Remove-IsolatedFixturePath $scanRoot $omxRoot
    Remove-IsolatedFixturePath $softwareFixturePath $omxRoot
}
