$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Join-Chars {
    param([int[]]$Codes)

    $chars = [System.Collections.Generic.List[char]]::new()
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

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
$screenshotPath = Join-Path $repoRoot ".omx\qa-uninstall-plan-window.png"
$evidenceRoot = Join-Path $repoRoot (".omx\qa-uninstall-evidence-" + [Guid]::NewGuid().ToString('N'))
$evidenceRootEnvironmentVariable = 'OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT'
$previousEvidenceRoot = [Environment]::GetEnvironmentVariable($evidenceRootEnvironmentVariable, 'Process')
[Environment]::SetEnvironmentVariable($evidenceRootEnvironmentVariable, $evidenceRoot, 'Process')

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

$uiText = @{
    Apps = Join-Chars @(0x5E94, 0x7528)
    Scan = Join-Chars @(0x626B, 0x63CF)
    Software = Join-Chars @(0x8F6F, 0x4EF6)
    NoUninstallerRun = Join-Chars @(0x4E0D, 0x4F1A, 0x8FD0, 0x884C, 0x5378, 0x8F7D, 0x5668)
}

$process = Start-Process -FilePath $exe -PassThru

try {
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
    $items = Wait-Until -TimeoutSeconds 45 -Probe {
        $found = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
        if ($found.Count -gt 10) {
            return $found
        }

        return $null
    }
    if ($null -eq $items) {
        throw "App list items were not found after scan."
    }

    $uninstallButton = $null
    for ($index = 0; $index -lt $items.Count -and $null -eq $uninstallButton; $index++) {
        $item = $items.Item($index)
        try {
            Select-ListItem $item
        }
        catch {
            continue
        }

        Start-Sleep -Milliseconds 250
        $candidate = Find-ByAutomationId $window 'DrawerUninstallButton' 250
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) {
            $uninstallButton = $candidate
        }
    }

    if ($null -eq $uninstallButton) {
        throw "No scanned app exposed an enabled DrawerUninstallButton."
    }

    $mainWindowHandle = $window.Current.NativeWindowHandle
    Invoke-Element $uninstallButton

    $planWindow = Wait-Until -TimeoutSeconds 10 -Probe {
        Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'UninstallPlanTitleTextBlock'
    }
    if ($null -eq $planWindow) {
        $processExited = $process.HasExited
        $statusText = '<unavailable>'
        if (-not $processExited) {
            $statusElement = Find-ByAutomationId $window 'StatusTextBlock' 250
            if ($null -ne $statusElement) {
                $statusText = $statusElement.Current.Name
            }
        }

        $visibleWindows = @()
        if (-not $processExited) {
            $windowCondition = [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Window)
            $processWindows = $root.FindAll(
                [System.Windows.Automation.TreeScope]::Children,
                [System.Windows.Automation.AndCondition]::new($processCondition, $windowCondition))
            foreach ($candidateWindow in $processWindows) {
                $visibleWindows += "name='$($candidateWindow.Current.Name)' handle=$($candidateWindow.Current.NativeWindowHandle)"
            }
        }

        $failureScreenshot = Join-Path $repoRoot '.omx\qa-uninstall-plan-window-failure.png'
        Save-DesktopScreenshot $failureScreenshot
        throw "Uninstall plan window was not found. processExited=$processExited; status='$statusText'; windows=[$($visibleWindows -join '; ')]"
    }

    Show-WpfWindowForSmoke $planWindow

    foreach ($fieldId in @(
        'UninstallPlanTitleTextBlock',
        'UninstallPlanSummaryTextBlock',
        'UninstallPlanSafetyTextBlock',
        'UninstallPlanAgentConclusionTextBlock',
        'UninstallPlanUndoHeadlineTextBlock',
        'UninstallPlanReinstallReadinessTextBlock',
        'UninstallPlanReinstallNextActionTextBlock',
        'UninstallPlanRestorePointStatusTextBlock',
        'UninstallPlanChooseInstallerButton',
        'UninstallPlanBackupCheckBox',
        'UninstallPlanPreparationSummaryTextBlock',
        'UninstallPlanBuildFinalChecklistButton',
        'UninstallPlanProtectionListBox',
        'UninstallPlanSimpleStepsListBox',
        'UninstallPlanNextActionTextBlock',
        'UninstallPlanFinalReminderTextBlock',
        'UninstallPlanCloseButton')) {
        $field = Find-ByAutomationId $planWindow $fieldId 1000
        if ($null -eq $field) {
            throw "Uninstall plan field was not found: $fieldId"
        }

        if ($field.Current.IsOffscreen) {
            throw "Uninstall plan field was offscreen: $fieldId"
        }
    }

    $reinstallReadinessVisible = $true
    $recoveryPreparationVisible = $true

    $protectionList = Find-ByAutomationId $planWindow 'UninstallPlanProtectionListBox' 1000
    $simpleStepsList = Find-ByAutomationId $planWindow 'UninstallPlanSimpleStepsListBox' 1000
    $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $protectionItems = $protectionList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
    $simpleStepItems = $simpleStepsList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
    if ($protectionItems.Count -lt 3 -or $simpleStepItems.Count -ne 3) {
        throw "The beginner recovery explanation did not expose the expected protection and three-step rows."
    }
    $recoveryTruthVisible = $true

    $buildFinalChecklistButton = Find-ByAutomationId $planWindow 'UninstallPlanBuildFinalChecklistButton' 1000
    Invoke-Element $buildFinalChecklistButton
    $finalChecklistTitle = Wait-Until -TimeoutSeconds 5 -Probe {
        $candidate = Find-ByAutomationId $planWindow 'UninstallPlanFinalChecklistTitleTextBlock' 250
        if ($null -ne $candidate -and -not $candidate.Current.IsOffscreen) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $finalChecklistTitle) {
        throw 'The final confirmation checklist did not become visible.'
    }

    $finalChecklistStatus = Find-ByAutomationId $planWindow 'UninstallPlanFinalChecklistStatusTextBlock' 1000
    $finalChecklistSummary = Find-ByAutomationId $planWindow 'UninstallPlanFinalChecklistSummaryTextBlock' 1000
    $finalChecklistMissing = Find-ByAutomationId $planWindow 'UninstallPlanFinalChecklistMissingListBox' 1000
    $finalChecklistSafety = Find-ByAutomationId $planWindow 'UninstallPlanFinalChecklistSafetyTextBlock' 1000
    foreach ($requiredField in @(
        $finalChecklistStatus,
        $finalChecklistSummary,
        $finalChecklistMissing,
        $finalChecklistSafety)) {
        if ($null -eq $requiredField -or $requiredField.Current.IsOffscreen) {
            throw 'A required final confirmation checklist field was missing or offscreen.'
        }
    }

    $missingItems = $finalChecklistMissing.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $listItemCondition)
    if ($missingItems.Count -lt 1) {
        throw 'Incomplete recovery preparation did not explain what is missing.'
    }
    if ($finalChecklistSafety.Current.Name.IndexOf(
        $uiText.NoUninstallerRun,
        [StringComparison]::Ordinal) -lt 0) {
        $failureScreenshot = Join-Path $repoRoot '.omx\qa-uninstall-final-checklist-failure.png'
        Save-WindowScreenshot $planWindow $failureScreenshot
        throw "The final confirmation checklist did not preserve the no-execution boundary. actual='$($finalChecklistSafety.Current.Name)' screenshot='$failureScreenshot'"
    }
    if (Test-Path -LiteralPath $evidenceRoot) {
        throw 'Incomplete recovery preparation created uninstall evidence unexpectedly.'
    }
    $finalChecklistVisible = $true
    $evidenceRootCreated = $false

    $technicalDetails = Find-ByAutomationId $planWindow 'UninstallPlanTechnicalDetailsExpander' 1000
    if ($null -eq $technicalDetails) {
        throw "Technical details expander was not found."
    }
    $expandPattern = $technicalDetails.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    if ($expandPattern.Current.ExpandCollapseState -ne [System.Windows.Automation.ExpandCollapseState]::Collapsed) {
        throw "Technical uninstall details were expanded by default."
    }
    $technicalDetailsCollapsed = $true

    $unexpectedRunButton = Find-ByAutomationId $planWindow 'UninstallPlanRunOfficialUninstallerButton' 250
    if ($null -ne $unexpectedRunButton) {
        throw "An official-uninstaller execution control was exposed in the preview window."
    }
    $noExecutionControl = $true

    Save-WindowScreenshot $planWindow $screenshotPath

    $closeButton = Find-ByAutomationId $planWindow 'UninstallPlanCloseButton' 1000
    Invoke-Element $closeButton
    Start-Sleep -Milliseconds 700

    [PSCustomObject]@{
        planWindowFound = $true
        recoveryTruthVisible = $true
        reinstallReadinessVisible = $true
        recoveryPreparationVisible = $true
        finalChecklistVisible = $true
        finalChecklistMissingCount = $missingItems.Count
        evidenceRootCreated = $false
        protectionLineCount = $protectionItems.Count
        simpleStepCount = $simpleStepItems.Count
        technicalDetailsCollapsed = $true
        noExecutionControl = $true
        closedPlanWindow = $true
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        $null = $process.CloseMainWindow()
        Start-Sleep -Milliseconds 700
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }

    [Environment]::SetEnvironmentVariable(
        $evidenceRootEnvironmentVariable,
        $previousEvidenceRoot,
        'Process')
    if (Test-Path -LiteralPath $evidenceRoot) {
        Remove-Item -LiteralPath $evidenceRoot -Recurse -Force
    }
}
