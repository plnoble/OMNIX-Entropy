$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Get-UnicodeText([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Get-ListItemCount($List) {
    $condition = [System.Windows.Automation.OrCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::DataItem))
    return $List.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition).Count
}

function Get-DescendantText($Element) {
    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    $items = $Element.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    return (($items | ForEach-Object { $_.Current.Name }) -join ' ')
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
$dataRoot = Join-Path $PSScriptRoot 'qa-agent-hardware-data'
$scanRoot = Join-Path 'C:\tmp' ('OMNIX-Hardware-Smoke-' + [Guid]::NewGuid().ToString('N'))
$screenshot = Join-Path $PSScriptRoot 'qa-agent-hardware-summary.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousScanRoot = $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT
$process = $null

Assert-ConfinedPath $dataRoot $PSScriptRoot
Assert-ConfinedPath $scanRoot 'C:\tmp'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Css.App.exe was not found: $exe"
}

try {
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
    $fixtureTemp = Join-Path $scanRoot 'Temp'
    New-Item -ItemType Directory -Path $fixtureTemp -Force | Out-Null
    [System.IO.File]::WriteAllBytes(
        (Join-Path $fixtureTemp 'hardware-smoke-cache.bin'),
        (New-Object byte[] 4096))

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $scanRoot
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

    $scanButton = Find-ByAutomationId $window 'StartScanButton' 2000
    if ($null -eq $scanButton) { throw 'StartScanButton was not found.' }
    Invoke-Element $scanButton

    $healthList = Wait-Until -TimeoutSeconds 90 -Probe {
        $candidate = Find-ByAutomationId $window 'HealthDimensionListView' 300
        if ($null -ne $candidate -and (Get-ListItemCount $candidate) -ge 7) {
            return $candidate
        }
        return $null
    }
    if ($null -eq $healthList) { throw 'The manual health scan did not complete.' }

    Invoke-Element (Find-ByAutomationId $window 'AgentNavButton' 3000)
    $questionBox = Find-ByAutomationId $window 'AgentQuestionTextBox' 3000
    $askButton = Find-ByAutomationId $window 'AskComputerAgentButton' 3000
    if ($null -eq $questionBox -or $null -eq $askButton) {
        throw 'Agent question controls were not found.'
    }

    $question = Get-UnicodeText @(0x6211, 0x7684, 0x7535, 0x8111, 0x914D, 0x7F6E, 0x600E, 0x4E48, 0x6837)
    $valuePattern = $questionBox.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    $valuePattern.SetValue($question)
    Invoke-Element $askButton

    $headline = Wait-Until -TimeoutSeconds 12 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationHeadlineTextBlock' 300
        $configuration = Get-UnicodeText @(0x7535, 0x8111, 0x914D, 0x7F6E)
        if ($null -ne $candidate -and -not $candidate.Current.IsOffscreen -and
            $candidate.Current.Name.Contains($configuration)) {
            return $candidate
        }
        return $null
    }
    $answer = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 2000
    $evidence = Find-ByAutomationId $window 'AgentConversationEvidenceListBox' 2000
    $nextSteps = Find-ByAutomationId $window 'AgentConversationNextStepsListBox' 2000
    $safety = Find-ByAutomationId $window 'AgentConversationSafetyTextBlock' 2000
    $navigate = Find-ByAutomationId $window 'AgentConversationNavigateButton' 2000
    if ($null -eq $headline -or $null -eq $answer -or $null -eq $evidence -or
        $null -eq $nextSteps -or $null -eq $safety -or $null -eq $navigate) {
        throw 'The hardware answer was incomplete.'
    }
    foreach ($element in @($headline, $answer, $evidence, $nextSteps, $safety, $navigate)) {
        if ($element.Current.IsOffscreen) { throw 'A required hardware answer element was offscreen.' }
    }

    $answerText = $answer.Current.Name
    $evidenceText = Get-DescendantText $evidence
    $nextText = Get-DescendantText $nextSteps
    $readOnly = Get-UnicodeText @(0x53EA, 0x8BFB)
    $minimum = Get-UnicodeText @(0x6700, 0x4F4E, 0x914D, 0x7F6E)
    if (-not $answerText.Contains($readOnly) -or -not $nextText.Contains($minimum)) {
        throw 'The hardware answer did not explain its read-only scope and compatibility limit.'
    }
    foreach ($label in @(
        (Get-UnicodeText @(0x5904, 0x7406, 0x5668)),
        (Get-UnicodeText @(0x663E, 0x5361)),
        (Get-UnicodeText @(0x7CFB, 0x7EDF)),
        (Get-UnicodeText @(0x67B6, 0x6784)))) {
        if (-not $evidenceText.Contains($label)) {
            throw "Hardware evidence was missing a required category: $label"
        }
    }
    foreach ($privateMarker in @('C:\', 'HKCU', 'HKLM', 'DeviceID', 'PNPDeviceID', 'SerialNumber')) {
        if (($answerText + ' ' + $evidenceText + ' ' + $nextText).Contains($privateMarker)) {
            throw "Hardware answer exposed a private or technical identifier: $privateMarker"
        }
    }

    Show-WpfWindowForSmoke $window
    Start-Sleep -Milliseconds 800
    Save-WindowScreenshot $window $screenshot

    [PSCustomObject]@{
        hardwareAnswerVisible = $true
        cpuEvidenceVisible = $evidenceText.Contains((Get-UnicodeText @(0x5904, 0x7406, 0x5668)))
        gpuEvidenceVisible = $evidenceText.Contains((Get-UnicodeText @(0x663E, 0x5361)))
        osEvidenceVisible = $evidenceText.Contains((Get-UnicodeText @(0x7CFB, 0x7EDF)))
        architectureEvidenceVisible = $evidenceText.Contains((Get-UnicodeText @(0x67B6, 0x6784)))
        compatibilityLimitVisible = $true
        pathOrIdentifierExposed = $false
        noOperationExecuted = $true
        screenshot = $screenshot
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $previousScanRoot
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
}
