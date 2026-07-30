$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-multidisk-selector.png'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

$process = $null
try {
    $process = Start-Process -FilePath $exe -PassThru
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)

    $window = Wait-Until -TimeoutSeconds 12 -Probe {
        $root.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $pidCondition)
    }
    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    Show-WpfWindowForSmoke $window
    $windowHandle = [IntPtr]$window.Current.NativeWindowHandle
    $topmostHandle = [IntPtr]::new(-1)
    $showWindowFlag = [uint32]0x0040
    if (-not [OmnixWpfWindowVisibility]::SetWindowPos(
        $windowHandle,
        $topmostHandle,
        40,
        40,
        1500,
        1000,
        $showWindowFlag)) {
        throw 'Could not place the WPF smoke window inside the primary screen.'
    }
    $window.SetFocus()

    $selector = Find-ByAutomationId $window 'DriveRootComboBox'
    if ($null -eq $selector) {
        throw 'DriveRootComboBox was not found.'
    }

    $expandPattern = $selector.GetCurrentPattern(
        [System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $expandPattern.Expand()

    $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $items = Wait-Until -TimeoutSeconds 5 -Probe {
        $found = @()
        $processWindows = $root.FindAll(
            [System.Windows.Automation.TreeScope]::Children,
            $pidCondition)
        foreach ($processWindow in $processWindows) {
            try {
                $found += @($processWindow.FindAll(
                    [System.Windows.Automation.TreeScope]::Descendants,
                    $listItemCondition))
            }
            catch [System.Runtime.InteropServices.COMException] {
                continue
            }
        }
        $driveItems = @($found | Where-Object {
            $supportsSelection = $false
            try {
                [void]$_.GetCurrentPattern(
                    [System.Windows.Automation.SelectionItemPattern]::Pattern)
                $supportsSelection = $true
            }
            catch {
                $supportsSelection = $false
            }

            $_.Current.ProcessId -eq $process.Id -and
            $_.Current.Name -match '\b[A-Z]\b.*\b(GB|MB|KB)\b' -and
            $supportsSelection
        })
        if ($driveItems.Count -ge 2) {
            return $driveItems
        }

        return $null
    }
    if ($null -eq $items) {
        throw 'Fewer than two fixed local drive items were exposed.'
    }

    $itemNames = @($items | ForEach-Object { $_.Current.Name })
    $systemItem = @($items | Where-Object {
        $_.Current.Name -match '\bC\b'
    }) | Select-Object -First 1
    $dataItem = @($items | Where-Object {
        $_.Current.Name -match '\bD\b'
    }) | Select-Object -First 1
    if ($null -eq $systemItem -or $null -eq $dataItem) {
        throw "Expected C and D drive choices were not found: $($itemNames -join ' | ')"
    }

    $selectionPattern = $dataItem.GetCurrentPattern(
        [System.Windows.Automation.SelectionItemPattern]::Pattern)
    $selectionPattern.Select()
    $expandPattern.Collapse()
    Start-Sleep -Milliseconds 600

    $emptyState = Find-ByAutomationId $window 'KeyFindingsEmptyStateTextBlock'
    if ($null -eq $emptyState -or $emptyState.Current.Name -notmatch '\bD\b') {
        throw 'Selecting D did not invalidate the page to a D-drive pending state.'
    }

    Show-WpfWindowForSmoke $window
    Save-WindowScreenshot $window $screenshotPath

    [PSCustomObject]@{
        listedDrives = $itemNames
        selectedDrive = $selector.Current.Name
        pendingConclusion = $emptyState.Current.Name
        selectorEnabled = $selector.Current.IsEnabled
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
