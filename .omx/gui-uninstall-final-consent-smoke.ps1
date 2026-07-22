$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Toggle-Element($element) {
    $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
    $pattern.Toggle()
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$consentScreenshotPath = Join-Path $repoRoot '.omx\qa-uninstall-final-consent.png'
$resultScreenshotPath = Join-Path $repoRoot '.omx\qa-uninstall-final-consent-result.png'

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

$process = Start-Process -FilePath $exe -ArgumentList '--smoke-uninstall-final-consent' -PassThru
$confirmInitiallyDisabled = $false
$confirmEnabledAfterAllChecks = $false
$fakeResultVisible = $false
$pipeResultFactCount = 0
$runtimeVisualReceiptAccepted = $false
$noRealExecutionControl = $false
$closedResultWindow = $false

try {
    $null = $process.WaitForInputIdle(15000)
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $consentWindow = Wait-Until -TimeoutSeconds 15 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $processCondition)
    }
    if ($null -eq $consentWindow) {
        throw 'The final-consent window was not found.'
    }

    Show-WpfWindowForSmoke $consentWindow
    $consentHandle = $consentWindow.Current.NativeWindowHandle

    $confirmButton = Find-ByAutomationId $consentWindow 'OfficialUninstallFinalConsentConfirmButton' 1500
    $commandCheck = Find-ByAutomationId $consentWindow 'OfficialUninstallFinalConsentCommandCheckBox' 1500
    $undoCheck = Find-ByAutomationId $consentWindow 'OfficialUninstallFinalConsentUndoCheckBox' 1500
    $postScanCheck = Find-ByAutomationId $consentWindow 'OfficialUninstallFinalConsentPostScanCheckBox' 1500
    $readiness = Find-ByAutomationId $consentWindow 'OfficialUninstallFinalConsentReadinessTextBlock' 1500
    $requiredConsentControls = @(
        @{ Id = 'confirm'; Element = $confirmButton },
        @{ Id = 'command'; Element = $commandCheck },
        @{ Id = 'undo'; Element = $undoCheck },
        @{ Id = 'postScan'; Element = $postScanCheck },
        @{ Id = 'readiness'; Element = $readiness }
    )
    foreach ($required in $requiredConsentControls) {
        if ($null -eq $required.Element) {
            throw "Final-consent control '$($required.Id)' was missing."
        }
        if ($required.Element.Current.IsOffscreen) {
            throw "Final-consent control '$($required.Id)' was offscreen."
        }
    }

    if ($confirmButton.Current.IsEnabled) {
        throw 'The final confirmation was enabled before all acknowledgements.'
    }
    $confirmInitiallyDisabled = $true

    Toggle-Element $commandCheck
    Toggle-Element $undoCheck
    Toggle-Element $postScanCheck
    $enabledConfirm = Wait-Until -TimeoutSeconds 5 -Probe {
        $candidate = Find-ByAutomationId $consentWindow 'OfficialUninstallFinalConsentConfirmButton' 250
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $enabledConfirm) {
        throw 'The final confirmation did not enable after all three acknowledgements.'
    }
    $confirmEnabledAfterAllChecks = $true

    $unexpectedRun = Find-ByAutomationId $consentWindow 'OfficialUninstallRunButton' 250
    if ($null -ne $unexpectedRun) {
        throw 'A real uninstaller execution control was exposed in the fake flow.'
    }
    $noRealExecutionControl = $true

    Save-WindowScreenshot $consentWindow $consentScreenshotPath
    Invoke-Element $enabledConfirm

    $resultWindow = Wait-Until -TimeoutSeconds 10 -Probe {
        Find-SecondaryWindowWithChild `
            -ProcessId $process.Id `
            -MainWindowHandle $consentHandle `
            -ChildAutomationId 'UninstallPostScanStatusTextBlock'
    }
    if ($null -eq $resultWindow) {
        throw 'The fake post-scan result did not appear after final consent.'
    }

    Show-WpfWindowForSmoke $resultWindow
    $resultStatus = Find-ByAutomationId $resultWindow 'UninstallPostScanStatusTextBlock' 1500
    $resultFacts = Find-ByAutomationId $resultWindow 'UninstallPostScanFactsListBox' 1500
    $resultAdvice = Find-ByAutomationId $resultWindow 'UninstallPostScanAgentAdviceTextBlock' 1500
    $resultSafety = Find-ByAutomationId $resultWindow 'UninstallPostScanSafetyTextBlock' 1500
    $resultClose = Find-ByAutomationId $resultWindow 'UninstallPostScanCloseButton' 1500
    foreach ($required in @($resultStatus, $resultFacts, $resultAdvice, $resultSafety, $resultClose)) {
        if ($null -eq $required -or $required.Current.IsOffscreen) {
            throw 'A required fake result field was missing or offscreen.'
        }
    }
    $pipeResultFactCount = $resultFacts.FindAll(
        [System.Windows.Automation.TreeScope]::Children,
        [System.Windows.Automation.Condition]::TrueCondition).Count
    if ($pipeResultFactCount -ne 2) {
        throw "The fake pipe result had $pipeResultFactCount facts; expected the two typed IPC facts."
    }
    # This result opens only after OfficialUninstallElevatedRequestSession consumes the runtime PNG ticket.
    $runtimeVisualReceiptAccepted = $true
    $fakeResultVisible = $true

    Save-WindowScreenshot $resultWindow $resultScreenshotPath
    Invoke-Element $resultClose
    $closedResultWindow = $process.WaitForExit(5000)
    if (-not $closedResultWindow) {
        throw 'The fake final-consent flow did not exit after closing the result.'
    }

    [pscustomobject]@{
        confirmInitiallyDisabled = $confirmInitiallyDisabled
        confirmEnabledAfterAllChecks = $confirmEnabledAfterAllChecks
        fakeResultVisible = $fakeResultVisible
        pipeResultFactCount = $pipeResultFactCount
        runtimeVisualReceiptAccepted = $runtimeVisualReceiptAccepted
        noRealExecutionControl = $noRealExecutionControl
        closedResultWindow = $closedResultWindow
        consentScreenshot = $consentScreenshotPath
        resultScreenshot = $resultScreenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}
