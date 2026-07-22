$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-agent-settings-confirm-cancel.png'
$beforeClickScreenshotPath = Join-Path $PSScriptRoot 'qa-agent-settings-before-click.png'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

Initialize-WpfSmokeAutomation

Add-Type @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public static class OmnixMouseClicker {
    [DllImport("user32.dll")]
    public static extern void mouse_event(int flags, int dx, int dy, int data, int extraInfo);
}

public static class OmnixNativeWindows {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    public static IntPtr[] GetTopLevelWindowHandlesForProcess(int targetProcessId) {
        var handles = new List<IntPtr>();
        EnumWindows((hWnd, lParam) => {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);
            if (processId == targetProcessId) {
                handles.Add(hWnd);
            }

            return true;
        }, IntPtr.Zero);

        return handles.ToArray();
    }
}
"@

function Try-GetClickablePoint($element) {
    try {
        return $element.GetClickablePoint()
    }
    catch {
        return $null
    }
}

function Click-Point($point) {
    [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point ([int]$point.X), ([int]$point.Y)
    Start-Sleep -Milliseconds 150
    [OmnixMouseClicker]::mouse_event(0x0002, 0, 0, 0, 0)
    Start-Sleep -Milliseconds 80
    [OmnixMouseClicker]::mouse_event(0x0004, 0, 0, 0, 0)
}

function Find-FirstClickableButton($root) {
    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Button)
    $buttons = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)

    for ($i = 0; $i -lt $buttons.Count; $i++) {
        $button = $buttons.Item($i)
        if (-not $button.Current.IsOffscreen -and $button.Current.IsEnabled) {
            $point = Try-GetClickablePoint $button
            if ($null -ne $point) {
                return [PSCustomObject]@{
                    Element = $button
                    Point = $point
                }
            }
        }
    }

    return $null
}

function Get-TopLevelWindowHandlesForProcess([int]$processId) {
    [OmnixNativeWindows]::GetTopLevelWindowHandlesForProcess($processId)
}

function Find-WindowForProcess($root, [int]$processId, [int]$exceptNativeWindowHandle) {
    $pidCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ProcessIdProperty), $processId
    $windowCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Window)
    $condition = New-Object System.Windows.Automation.AndCondition -ArgumentList $pidCondition, $windowCondition

    try {
        $windows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $condition)
        for ($i = 0; $i -lt $windows.Count; $i++) {
            $candidate = $windows.Item($i)
            if ($candidate.Current.NativeWindowHandle -ne $exceptNativeWindowHandle) {
                return $candidate
            }
        }
    }
    catch {
    }

    $handles = Get-TopLevelWindowHandlesForProcess $processId
    foreach ($handle in $handles) {
        if ($handle.ToInt64() -eq [int64]$exceptNativeWindowHandle) {
            continue
        }

        try {
            $candidate = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
            if ($null -ne $candidate) {
                return $candidate
            }
        }
        catch {
            continue
        }
    }

    try {
        $windows = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
        for ($i = 0; $i -lt $windows.Count; $i++) {
            $candidate = $windows.Item($i)
            if ($candidate.Current.NativeWindowHandle -ne $exceptNativeWindowHandle) {
                return $candidate
            }
        }
    }
    catch {
    }

    return $null
}

function Get-SettingsProcessIds {
    @(Get-Process -Name SystemSettings -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })
}

$settingsBefore = Get-SettingsProcessIds
$process = Start-Process -FilePath $exe -PassThru
try {
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ProcessIdProperty), $process.Id

    $window = Wait-Until -TimeoutSeconds 12 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }

    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    $windowHandle = $window.Current.NativeWindowHandle
    $window.SetFocus()

    $agentNav = Find-ByAutomationId $window 'AgentNavButton'
    if ($null -eq $agentNav) {
        throw 'AgentNavButton was not found.'
    }

    Invoke-Element $agentNav
    Start-Sleep -Milliseconds 500

    $settingsList = Find-ByAutomationId $window 'AgentWindowsSettingsListBox'
    if ($null -eq $settingsList) {
        throw 'AgentWindowsSettingsListBox was not found.'
    }

    $capabilityScrollViewer = Find-ByAutomationId $window 'AgentCapabilityScrollViewer'
    if ($null -eq $capabilityScrollViewer) {
        throw 'AgentCapabilityScrollViewer was not found.'
    }

    $firstClickableSettingsButton = Find-FirstClickableButton $settingsList
    $storageButton = $null
    $storageClickPoint = $null
    if ($null -ne $firstClickableSettingsButton) {
        $storageButton = $firstClickableSettingsButton.Element
        $storageClickPoint = $firstClickableSettingsButton.Point
    }

    if ($null -eq $storageButton) {
        $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 1000
    }
    if ($null -eq $storageButton) {
        try {
            $scroll = $settingsList.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            for ($i = 0; $i -lt 6 -and $null -eq $storageButton; $i++) {
                $scroll.Scroll(
                    [System.Windows.Automation.ScrollAmount]::NoAmount,
                    [System.Windows.Automation.ScrollAmount]::LargeIncrement)
                Start-Sleep -Milliseconds 250
                $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 500
            }
        }
        catch {
            throw "Could not find or scroll to the Storage settings button: $($_.Exception.Message)"
        }
    }

    if ($null -eq $storageButton) {
        throw 'Storage settings button was not found.'
    }

    if ($storageButton.Current.IsOffscreen) {
        try {
            $scrollItem = $storageButton.GetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern)
            $scrollItem.ScrollIntoView()
            Start-Sleep -Milliseconds 500
            $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 1000
        }
        catch {
            # Fall through to the explicit visibility check below.
        }
    }

    if ($null -eq $storageButton -or $storageButton.Current.IsOffscreen) {
        throw 'Storage settings button was found but could not be scrolled into view.'
    }

    if ($null -eq $storageClickPoint) {
        $storageClickPoint = Try-GetClickablePoint $storageButton
    }
    if ($null -eq $storageClickPoint) {
        try {
            $outerScroll = $capabilityScrollViewer.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            foreach ($percent in @(100, 85, 70, 55, 40, 25, 0)) {
                if ($null -ne $storageClickPoint) {
                    break
                }

                $outerScroll.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, [double]$percent)
                Start-Sleep -Milliseconds 250
                $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 500
                if ($null -ne $storageButton) {
                    $storageClickPoint = Try-GetClickablePoint $storageButton
                }
            }
        }
        catch {
            throw "Could not scroll Agent capability panel to the Storage settings button: $($_.Exception.Message)"
        }
    }

    if ($null -eq $storageClickPoint) {
        try {
            $scroll = $settingsList.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            for ($i = 0; $i -lt 12 -and $null -eq $storageClickPoint; $i++) {
                $scroll.Scroll(
                    [System.Windows.Automation.ScrollAmount]::NoAmount,
                    [System.Windows.Automation.ScrollAmount]::SmallIncrement)
                Start-Sleep -Milliseconds 200
                $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 500
                if ($null -ne $storageButton) {
                    $storageClickPoint = Try-GetClickablePoint $storageButton
                }
            }

            foreach ($percent in @(100, 80, 60, 40, 20, 0)) {
                if ($null -ne $storageClickPoint) {
                    break
                }

                $scroll.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, [double]$percent)
                Start-Sleep -Milliseconds 250
                $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 500
                if ($null -ne $storageButton) {
                    $storageClickPoint = Try-GetClickablePoint $storageButton
                }
            }
        }
        catch {
            throw "Could not scroll Storage settings button to a clickable point: $($_.Exception.Message)"
        }
    }

    if ($null -eq $storageClickPoint) {
        $rect = $storageButton.Current.BoundingRectangle
        throw "Storage settings button was found but never exposed a clickable point. type=$($storageButton.Current.ControlType.ProgrammaticName); name=$($storageButton.Current.Name); offscreen=$($storageButton.Current.IsOffscreen); enabled=$($storageButton.Current.IsEnabled); rect=$($rect.X),$($rect.Y),$($rect.Width),$($rect.Height)"
    }

    $windowRect = $window.Current.BoundingRectangle
    if ($storageClickPoint.Y -gt ($windowRect.Y + $windowRect.Height - 140)) {
        try {
            $outerScroll = $capabilityScrollViewer.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
            for ($i = 0; $i -lt 8 -and $storageClickPoint.Y -gt ($windowRect.Y + $windowRect.Height - 140); $i++) {
                $outerScroll.Scroll(
                    [System.Windows.Automation.ScrollAmount]::NoAmount,
                    [System.Windows.Automation.ScrollAmount]::SmallIncrement)
                Start-Sleep -Milliseconds 200
                $storageButton = Find-ByAutomationId $window 'AgentWindowsSettingsOpenButton_storage' 500
                if ($null -ne $storageButton) {
                    $point = Try-GetClickablePoint $storageButton
                    if ($null -ne $point) {
                        $storageClickPoint = $point
                    }
                }
            }
        }
        catch {
            throw "Could not move Storage settings button away from the lower window edge: $($_.Exception.Message)"
        }
    }

    Save-WindowScreenshot $window $beforeClickScreenshotPath
    Click-Point $storageClickPoint

    $dialog = Wait-Until -TimeoutSeconds 8 -Probe {
        Find-WindowForProcess $root $process.Id $windowHandle
    }

    if ($null -eq $dialog) {
        Invoke-Element $storageButton
        $dialog = Wait-Until -TimeoutSeconds 4 -Probe {
            Find-WindowForProcess $root $process.Id $windowHandle
        }
    }

    if ($null -eq $dialog) {
        try {
            $storageButton.SetFocus()
            Start-Sleep -Milliseconds 150
            [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
        }
        catch {
            # Fall through to the final diagnostic error.
        }

        $dialog = Wait-Until -TimeoutSeconds 4 -Probe {
            Find-WindowForProcess $root $process.Id $windowHandle
        }
    }

    if ($null -eq $dialog) {
        $rect = $storageButton.Current.BoundingRectangle
        throw "Confirmation dialog was not shown for the medium-risk Storage settings shortcut. buttonName=$($storageButton.Current.Name); offscreen=$($storageButton.Current.IsOffscreen); enabled=$($storageButton.Current.IsEnabled); rect=$($rect.X),$($rect.Y),$($rect.Width),$($rect.Height); beforeClickScreenshot=$beforeClickScreenshotPath"
    }

    Save-WindowScreenshot $dialog $screenshotPath

    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Button)
    $dialogButtons = $dialog.FindAll([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    $cancelName = -join ([char[]](0x53D6, 0x6D88))
    $cancelButton = $null
    for ($i = 0; $i -lt $dialogButtons.Count; $i++) {
        $button = $dialogButtons.Item($i)
        if ($button.Current.AutomationId -eq '2' -or $button.Current.Name -eq 'Cancel' -or $button.Current.Name -eq $cancelName) {
            $cancelButton = $button
            break
        }
    }

    if ($null -eq $cancelButton) {
        for ($i = 0; $i -lt $dialogButtons.Count; $i++) {
            $button = $dialogButtons.Item($i)
            if ($null -eq $cancelButton -or $button.Current.BoundingRectangle.X -gt $cancelButton.Current.BoundingRectangle.X) {
                $cancelButton = $button
            }
        }
    }

    if ($null -ne $cancelButton) {
        Invoke-Element $cancelButton
    }
    else {
        $dialogRect = $dialog.Current.BoundingRectangle
        $cancelPoint = [PSCustomObject]@{
            X = $dialogRect.X + ($dialogRect.Width * 0.85)
            Y = $dialogRect.Y + $dialogRect.Height - 50
        }
        Click-Point $cancelPoint
    }
    Start-Sleep -Seconds 2

    $settingsAfter = Get-SettingsProcessIds
    $newSettingsProcesses = @($settingsAfter | Where-Object { $settingsBefore -notcontains $_ })
    if ($newSettingsProcesses.Count -gt 0) {
        throw "Windows Settings process was launched after cancel: $($newSettingsProcesses -join ',')"
    }

    [PSCustomObject]@{
        confirmationDialogFound = $true
        cancelClicked = $true
        newSettingsProcessCount = $newSettingsProcesses.Count
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
