$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Join-Chars {
    param([int[]]$Codes)

    $chars = New-Object System.Collections.Generic.List[char]
    foreach ($code in $Codes) {
        $chars.Add([char]$code)
    }

    return -join $chars
}

function Find-ControlByNameParts {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [System.Windows.Automation.ControlType]$ControlType,
        [string[]]$NameParts
    )

    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        $ControlType)
    $items = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)

    foreach ($item in $items) {
        $name = $item.Current.Name
        if ([string]::IsNullOrWhiteSpace($name)) {
            continue
        }

        $matches = $true
        foreach ($part in $NameParts) {
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

function Select-ListItem {
    param([System.Windows.Automation.AutomationElement]$Element)

    $pattern = $Element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
    $pattern.Select()
}

function Close-ProcessDialogWindows {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [int]$ProcessId,
        [int]$MainWindowHandle
    )

    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $ProcessId)
    $windowCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Window)
    $condition = [System.Windows.Automation.AndCondition]::new($processCondition, $windowCondition)
    try {
        $windows = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    }
    catch [System.Runtime.InteropServices.COMException] {
        return 0
    }

    $closed = 0
    foreach ($candidate in $windows) {
        try {
            $handle = $candidate.Current.NativeWindowHandle
            if ($handle -eq 0 -or $handle -eq $MainWindowHandle) {
                continue
            }

            $pattern = $candidate.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
            $pattern.Close()
            $closed++
            Start-Sleep -Milliseconds 300
        }
        catch {
            continue
        }
    }

    return $closed
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

$uiText = @{
    Apps = Join-Chars @(0x5E94, 0x7528)
    Scan = Join-Chars @(0x626B, 0x63CF)
    Software = Join-Chars @(0x8F6F, 0x4EF6)
    Clean = Join-Chars @(0x6E05, 0x7406)
    Cache = Join-Chars @(0x7F13, 0x5B58)
    Close = Join-Chars @(0x5173, 0x95ED)
    Startup = Join-Chars @(0x81EA, 0x542F, 0x52A8)
    Preview = Join-Chars @(0x9884, 0x89C8)
    UninstallPreview = Join-Chars @(0x5378, 0x8F7D, 0x65B9, 0x6848, 0x9884, 0x89C8)
    MigrationPreview = Join-Chars @(0x8FC1, 0x79FB, 0x65B9, 0x6848, 0x9884, 0x89C8)
    CachePreview = Join-Chars @(0x7F13, 0x5B58, 0x6E05, 0x7406, 0x65B9, 0x6848)
    StartupPreview = Join-Chars @(0x81EA, 0x542F, 0x52A8, 0x68C0, 0x67E5)
}

$screenshotPath = Join-Path $repoRoot ".omx\qa-app-drawer-action-previews.png"
$isolatedDataRoot = Join-Path $repoRoot ".omx\qa-app-drawer-data"
$softwareFixturePath = Join-Path $repoRoot ".omx\qa-app-drawer-software-fixture.json"
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousSoftwareFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$process = $null

try {
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $isolatedDataRoot -Force | Out-Null

    $profile = [ordered]@{
        name = 'OMNIX Preview Fixture'
        publisher = 'OMNIX Smoke'
        installPath = 'C:\OMNIX-Fixture\Install'
        uninstallCommand = '"C:\OMNIX-Fixture\Install\uninstall.exe"'
        cachePaths = @('C:\Users\Fixture\AppData\Local\OMNIX Preview Fixture\Cache')
        cDriveWritePaths = @('C:\Users\Fixture\AppData\Local\OMNIX Preview Fixture\Cache')
        startupEntries = @('OMNIX Preview Fixture')
    }
    $fixture = [ordered]@{ scans = @(,@($profile)) }
    $fixture | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $softwareFixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
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
        throw "Main window was not found."
    }

    $appsButton = Find-ControlByNameParts $window ([System.Windows.Automation.ControlType]::Button) @($uiText.Apps)
    if ($null -eq $appsButton) {
        throw "Apps navigation button was not found."
    }

    Invoke-Element $appsButton
    Start-Sleep -Milliseconds 500

    $scanButton = Find-ControlByNameParts $window ([System.Windows.Automation.ControlType]::Button) @($uiText.Scan, $uiText.Software)
    if ($null -eq $scanButton) {
        $scanButton = Find-ControlByNameParts $window ([System.Windows.Automation.ControlType]::Button) @($uiText.Scan, $uiText.Apps)
    }
    if ($null -eq $scanButton) {
        throw "Scan software button was not found."
    }

    Invoke-Element $scanButton

    $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $itemResult = Wait-Until -TimeoutSeconds 45 -Probe {
        $found = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
        if ($found.Count -gt 0) {
            return [PSCustomObject]@{ Items = @($found) }
        }

        return $null
    }
    if ($null -eq $itemResult) {
        throw "App list items were not found after scan."
    }
    $items = @($itemResult.Items)

    $actions = @(
        @{ Id = "DrawerUninstallButton"; Title = $uiText.UninstallPreview },
        @{ Id = "DrawerMigrateButton"; Title = $uiText.MigrationPreview },
        @{ Id = "DrawerCleanCacheButton"; Title = $uiText.CachePreview },
        @{ Id = "DrawerDisableStartupButton"; Title = $uiText.StartupPreview }
    )
    $verified = New-Object System.Collections.Generic.List[string]
    $closedDialogCount = 0
    $mainWindowHandle = $window.Current.NativeWindowHandle

    foreach ($action in $actions) {
        $button = $null
        for ($index = 0; $index -lt $items.Count -and $null -eq $button; $index++) {
            $item = $items[$index]
            try {
                Select-ListItem $item
            }
            catch {
                continue
            }

            Start-Sleep -Milliseconds 250
            $candidate = Find-ByAutomationId $window $action.Id 250
            if ($null -ne $candidate -and $candidate.Current.IsEnabled) {
                $button = $candidate
            }
        }

        if ($null -eq $button) {
            throw "No scanned app exposed an enabled action button: $($action.Id)"
        }

        Invoke-Element $button
        Start-Sleep -Milliseconds 700
        $closedDialogCount += Close-ProcessDialogWindows $root $process.Id $mainWindowHandle

        $titleElement = Wait-Until -TimeoutSeconds 8 -Probe {
            $title = Find-ByAutomationId $window "DrawerActionPreviewTitleTextBlock" 250
            if ($null -ne $title -and $title.Current.Name.Contains($action.Title)) {
                return $title
            }

            return $null
        }
        if ($null -eq $titleElement) {
            throw "Shared action preview did not show expected title '$($action.Title)' after $($action.Id)."
        }

        if ($titleElement.Current.IsOffscreen) {
            throw "Shared action preview title was offscreen after $($action.Id)."
        }

        foreach ($fieldId in @("DrawerActionPreviewAgentTextBlock", "DrawerActionPreviewNextStepTextBlock", "DrawerActionPreviewSafetyTextBlock")) {
            $field = Find-ByAutomationId $window $fieldId 500
            if ($null -eq $field) {
                throw "Shared action card field was not found after $($action.Id): $fieldId"
            }

            if ($field.Current.IsOffscreen) {
                throw "Shared action card field was offscreen after $($action.Id): $fieldId"
            }

            if ([string]::IsNullOrWhiteSpace($field.Current.Name) -or $field.Current.Name -eq "-") {
                throw "Shared action card field was empty after $($action.Id): $fieldId"
            }
        }

        $list = Find-ByAutomationId $window "DrawerActionPreviewListBox" 500
        if ($null -eq $list) {
            throw "Shared action preview list was not found after $($action.Id)."
        }

        if ($list.Current.IsOffscreen) {
            throw "Shared action preview list was offscreen after $($action.Id)."
        }

        $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem)
        $previewItems = $list.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
        if ($previewItems.Count -lt 1) {
            throw "Shared action preview list had no lines after $($action.Id)."
        }

        $verified.Add($action.Id)
    }

    Save-DesktopScreenshot $screenshotPath

    Write-Output "verifiedActionButtons=$($verified.Count)"
    Write-Output "verifiedActionButtonIds=$($verified -join ',')"
    Write-Output "closedDialogCount=$closedDialogCount"
    Write-Output "screenshot=$screenshotPath"
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $null = $process.CloseMainWindow()
        Start-Sleep -Milliseconds 700
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousSoftwareFixture
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
}
