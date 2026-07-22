$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Get-UnicodeText([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Assert-ConfinedPath([string]$Path, [string]$Root) {
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    $fullRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd('\')
    if ($fullPath.Equals($fullRoot, [StringComparison]::OrdinalIgnoreCase) -or
        -not $fullPath.StartsWith($fullRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing fixture cleanup outside the expected root: $fullPath"
    }
}

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$dataRoot = Join-Path $PSScriptRoot 'qa-agent-troubleshooting-data'
$answerScreenshot = Join-Path $PSScriptRoot 'qa-agent-troubleshooting-routing.png'
$confirmationScreenshot = Join-Path $PSScriptRoot 'qa-agent-tool-open-confirmation.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$process = $null
$baselineMmcIds = @(Get-Process mmc -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })

Assert-ConfinedPath $dataRoot $PSScriptRoot
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Css.App.exe was not found: $exe"
}

try {
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
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
    $mainWindowHandle = $window.Current.NativeWindowHandle

    Invoke-Element (Find-ByAutomationId $window 'AgentNavButton' 5000)
    $questionBox = Find-ByAutomationId $window 'AgentQuestionTextBox' 3000
    $askButton = Find-ByAutomationId $window 'AskComputerAgentButton' 3000
    if ($null -eq $questionBox -or $null -eq $askButton) {
        throw 'Agent question controls were not found.'
    }
    $question = Get-UnicodeText @(0x9A71, 0x52A8, 0x5F02, 0x5E38, 0x600E, 0x4E48, 0x529E)
    $valuePattern = $questionBox.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    $valuePattern.SetValue($question)
    Invoke-Element $askButton

    $headline = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationHeadlineTextBlock' 500
        if ($null -ne $candidate -and -not $candidate.Current.IsOffscreen -and
            $candidate.Current.Name.Contains((Get-UnicodeText @(0x8BBE, 0x5907, 0x7BA1, 0x7406, 0x5668)))) {
            return $candidate
        }
        return $null
    }
    $answer = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 2000
    $safety = Find-ByAutomationId $window 'AgentConversationSafetyTextBlock' 2000
    $navigate = Find-ByAutomationId $window 'AgentConversationNavigateButton' 2000
    if ($null -eq $headline -or $null -eq $answer -or $null -eq $safety -or $null -eq $navigate) {
        throw 'The troubleshooting answer was incomplete.'
    }
    $rootCause = Get-UnicodeText @(0x6839, 0x56E0)
    $cannot = Get-UnicodeText @(0x4E0D, 0x80FD)
    $openDeviceManager = Get-UnicodeText @(0x6253, 0x5F00, 0x8BBE, 0x5907, 0x7BA1, 0x7406, 0x5668)
    if (-not $answer.Current.Name.Contains($rootCause) -or
        -not $answer.Current.Name.Contains($cannot) -or
        $navigate.Current.Name -ne $openDeviceManager -or
        -not $navigate.Current.IsEnabled -or $navigate.Current.IsOffscreen) {
        throw 'Agent did not show the uncertain diagnosis and exact safe next step together.'
    }
    if ($answer.Current.Name.Contains('devmgmt') -or $safety.Current.Name.Contains('devmgmt')) {
        throw 'The beginner answer exposed the system-tool command.'
    }
    Show-WpfWindowForSmoke $window
    Start-Sleep -Milliseconds 800
    Save-WindowScreenshot $window $answerScreenshot

    Invoke-Element $navigate
    $confirmationTitle = Get-UnicodeText @(0x786E, 0x8BA4, 0x6253, 0x5F00, 0x7CFB, 0x7EDF, 0x5DE5, 0x5177)
    $confirmation = Wait-Until -TimeoutSeconds 10 -Probe {
        foreach ($handle in (Get-WpfTopLevelWindowHandlesForProcess $process.Id)) {
            if ($handle.ToInt64() -eq [int64]$mainWindowHandle) { continue }
            try {
                $candidate = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
                if ($null -ne $candidate -and
                    $candidate.Current.ControlType -eq [System.Windows.Automation.ControlType]::Window -and
                    $candidate.Current.Name -eq $confirmationTitle) {
                    return $candidate
                }
            }
            catch {
                continue
            }
        }
        return $null
    }
    if ($null -eq $confirmation) { throw 'The protected system-tool confirmation did not open.' }
    Show-WpfWindowForSmoke $confirmation
    Start-Sleep -Milliseconds 500
    Save-WindowScreenshot $confirmation $confirmationScreenshot
    $windowPattern = $confirmation.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
    $windowPattern.Close()
    Start-Sleep -Milliseconds 800

    $newMmc = @(Get-Process mmc -ErrorAction SilentlyContinue | Where-Object { $baselineMmcIds -notcontains $_.Id })
    if ($newMmc.Count -ne 0) {
        throw 'Device Manager started even though the user cancelled the confirmation.'
    }

    [PSCustomObject]@{
        troubleshootingAnswerVisible = $true
        uncertaintyExplained = $true
        exactToolNextStepVisible = $true
        protectedToolConfirmationVisible = $true
        confirmationCancelled = $true
        externalToolStarted = $false
        noOperationExecuted = $true
        answerScreenshot = $answerScreenshot
        confirmationScreenshot = $confirmationScreenshot
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    Assert-ConfinedPath $dataRoot $PSScriptRoot
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
}
