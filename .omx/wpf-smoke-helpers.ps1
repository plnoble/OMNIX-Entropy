$script:WpfSmokeAutomationInitialized = $false

function Initialize-WpfSmokeAutomation {
    if ($script:WpfSmokeAutomationInitialized) {
        return
    }

    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing

    if ($null -eq ('OmnixWpfWindowVisibility' -as [type])) {
        Add-Type @"
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class OmnixWpfWindowVisibility {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);

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
    }

    $script:WpfSmokeAutomationInitialized = $true
}

function Get-WpfTopLevelWindowHandlesForProcess([int]$ProcessId) {
    Initialize-WpfSmokeAutomation
    [OmnixWpfWindowVisibility]::GetTopLevelWindowHandlesForProcess($ProcessId)
}

function Find-ByAutomationId($root, [string]$automationId, [int]$timeoutMs = 10000) {
    Initialize-WpfSmokeAutomation

    $deadline = [DateTime]::UtcNow.AddMilliseconds($timeoutMs)
    $condition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::AutomationIdProperty), $automationId

    do {
        $element = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
        if ($null -ne $element) {
            return $element
        }

        Start-Sleep -Milliseconds 200
    } while ([DateTime]::UtcNow -lt $deadline)

    return $null
}

function Find-WindowByDescendantAutomationId {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [int]$ProcessId,
        [int]$MainWindowHandle,
        [string]$ChildAutomationId
    )

    Initialize-WpfSmokeAutomation

    $childCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty,
        $ChildAutomationId)
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker

    try {
        $children = $Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, $childCondition)
        foreach ($child in $children) {
            $candidate = $child
            while ($null -ne $candidate) {
                try {
                    if ($candidate.Current.ControlType -eq [System.Windows.Automation.ControlType]::Window) {
                        if ($candidate.Current.ProcessId -eq $ProcessId -and
                            $candidate.Current.NativeWindowHandle -ne $MainWindowHandle) {
                            return $candidate
                        }

                        break
                    }

                    $candidate = $walker.GetParent($candidate)
                }
                catch {
                    break
                }
            }
        }
    }
    catch {
        return $null
    }

    return $null
}

function Find-SecondaryWindowWithChild {
    param(
        [int]$ProcessId,
        [int]$MainWindowHandle,
        [string]$ChildAutomationId
    )

    Initialize-WpfSmokeAutomation

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $ProcessId)
    $windowCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Window)
    $condition = [System.Windows.Automation.AndCondition]::new($processCondition, $windowCondition)
    $windows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $condition)

    foreach ($candidate in $windows) {
        try {
            $handle = $candidate.Current.NativeWindowHandle
            if ($handle -eq 0 -or $handle -eq $MainWindowHandle) {
                continue
            }

            $child = Find-ByAutomationId $candidate $ChildAutomationId 250
            if ($null -ne $child) {
                return $candidate
            }
        }
        catch {
            continue
        }
    }

    $nativeHandles = Get-WpfTopLevelWindowHandlesForProcess $ProcessId
    foreach ($nativeHandle in $nativeHandles) {
        if ($nativeHandle.ToInt64() -eq [int64]$MainWindowHandle) {
            continue
        }

        try {
            $candidate = [System.Windows.Automation.AutomationElement]::FromHandle($nativeHandle)
            if ($null -eq $candidate) {
                continue
            }

            $child = Find-ByAutomationId $candidate $ChildAutomationId 250
            if ($null -ne $child) {
                return $candidate
            }
        }
        catch {
            continue
        }
    }

    $descendantWindow = Find-WindowByDescendantAutomationId $root $ProcessId $MainWindowHandle $ChildAutomationId
    if ($null -ne $descendantWindow) {
        return $descendantWindow
    }

    return $null
}

function Wait-Until {
    param(
        [scriptblock]$Probe,
        [int]$TimeoutSeconds = 10,
        [int]$IntervalMilliseconds = 200
    )

    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        $result = & $Probe
        if ($null -ne $result -and $result -ne $false) {
            return $result
        }

        Start-Sleep -Milliseconds $IntervalMilliseconds
    }

    return $null
}

function Show-WpfWindowForSmoke {
    param([System.Windows.Automation.AutomationElement]$Window)

    Initialize-WpfSmokeAutomation
    $handle = [IntPtr]$Window.Current.NativeWindowHandle
    if ($handle -eq [IntPtr]::Zero) {
        throw 'Cannot show a WPF window without a native window handle.'
    }

    [OmnixWpfWindowVisibility]::ShowWindowAsync($handle, 9) | Out-Null
    $topmostHandle = [IntPtr]::new(-1)
    $flags = [uint32](0x0001 -bor 0x0002 -bor 0x0040)
    if (-not [OmnixWpfWindowVisibility]::SetWindowPos($handle, $topmostHandle, 0, 0, 0, 0, $flags)) {
        $errorCode = [Runtime.InteropServices.Marshal]::GetLastWin32Error()
        throw "Could not place WPF smoke window above other windows. Win32 error: $errorCode."
    }

    Start-Sleep -Milliseconds 200
}

function Invoke-Element($element) {
    Initialize-WpfSmokeAutomation

    $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $pattern.Invoke()
}

function Save-WindowScreenshot($window, [string]$path) {
    Initialize-WpfSmokeAutomation

    $bounds = $window.Current.BoundingRectangle
    if ($bounds.Width -le 0 -or $bounds.Height -le 0) {
        throw 'Window bounds are empty.'
    }

    $bitmap = New-Object System.Drawing.Bitmap ([int]$bounds.Width), ([int]$bounds.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen([int]$bounds.X, [int]$bounds.Y, 0, 0, $bitmap.Size)
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}

function Save-DesktopScreenshot([string]$path) {
    Initialize-WpfSmokeAutomation

    $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bitmap = [System.Drawing.Bitmap]::new($bounds.Width, $bounds.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}
