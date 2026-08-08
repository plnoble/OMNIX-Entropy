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

function Get-VisibleDescendantText($Element) {
    $names = New-Object System.Collections.Generic.List[string]
    $descendants = $Element.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($descendant in $descendants) {
        try {
            if (-not $descendant.Current.IsOffscreen -and
                -not [string]::IsNullOrWhiteSpace($descendant.Current.Name)) {
                $names.Add($descendant.Current.Name)
            }
        }
        catch {
            continue
        }
    }
    return [string]::Join(' ', $names)
}

function Assert-NonBlankScreenshot([string]$Path) {
    $bitmap = [System.Drawing.Bitmap]::new($Path)
    try {
        $sampleCount = 0
        $visibleCount = 0
        for ($x = 0; $x -lt $bitmap.Width; $x += 32) {
            for ($y = 0; $y -lt $bitmap.Height; $y += 32) {
                $pixel = $bitmap.GetPixel($x, $y)
                $sampleCount++
                if (($pixel.R + $pixel.G + $pixel.B) -gt 60) {
                    $visibleCount++
                }
            }
        }
        if ($sampleCount -eq 0 -or ($visibleCount / $sampleCount) -lt 0.1) {
            throw "Screenshot was blank or nearly black: $Path"
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$dataRoot = Join-Path $PSScriptRoot 'qa-agent-symptom-data'
$quarantineRoot = Join-Path $dataRoot 'Quarantine'
$screenshot = Join-Path $PSScriptRoot 'qa-agent-symptom-triage.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousQuarantineRoot = $env:OMNIX_ENTROPY_QUARANTINE_ROOT
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
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $quarantineRoot
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

    Invoke-Element (Find-ByAutomationId $window 'AgentNavButton' 5000)
    $choiceIds = @(
        'AgentSymptomQuickChoice_network',
        'AgentSymptomQuickChoice_sound',
        'AgentSymptomQuickChoice_bluetooth',
        'AgentSymptomQuickChoice_display',
        'AgentSymptomQuickChoice_driver',
        'AgentSymptomQuickChoice_blue-screen')
    foreach ($choiceId in $choiceIds) {
        $null = Require-FullyVisibleElement $window $choiceId 3000
    }

    $blueScreenChoice = Find-ByAutomationId $window 'AgentSymptomQuickChoice_blue-screen' 2000
    Invoke-Element $blueScreenChoice

    $blueScreen = Get-UnicodeText @(0x84DD, 0x5C4F)
    $headline = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationHeadlineTextBlock' 500
        if ($null -ne $candidate -and
            -not $candidate.Current.IsOffscreen -and
            $candidate.Current.Name.Contains($blueScreen)) {
            return $candidate
        }
        return $null
    }
    $answer = Require-FullyVisibleElement $window 'AgentConversationAnswerTextBlock' 2000
    $evidence = Require-FullyVisibleElement $window 'AgentConversationEvidenceListBox' 2000
    $nextSteps = Require-FullyVisibleElement $window 'AgentConversationNextStepsListBox' 2000
    $safety = Require-FullyVisibleElement $window 'AgentConversationSafetyTextBlock' 2000
    $navigate = Require-FullyVisibleElement $window 'AgentConversationNavigateButton' 2000
    if ($null -eq $headline) { throw 'The symptom triage headline was not visible.' }

    $evidenceText = Get-VisibleDescendantText $evidence
    $nextStepText = Get-VisibleDescendantText $nextSteps
    $checked = Get-UnicodeText @(0x5DF2, 0x68C0, 0x67E5)
    $unknown = Get-UnicodeText @(0x4ECD, 0x672A, 0x77E5)
    $urgency = Get-UnicodeText @(0x7D27, 0x6025, 0x7A0B, 0x5EA6)
    $moreUrgent = Get-UnicodeText @(0x8F83, 0x7D27, 0x6025)
    $eventViewer = Get-UnicodeText @(0x4E8B, 0x4EF6, 0x67E5, 0x770B, 0x5668)
    $openEventViewer = Get-UnicodeText @(0x6253, 0x5F00, 0x4E8B, 0x4EF6, 0x67E5, 0x770B, 0x5668)
    if (-not $evidenceText.Contains($checked) -or
        -not $evidenceText.Contains($unknown) -or
        -not $evidenceText.Contains($urgency) -or
        -not $evidenceText.Contains($moreUrgent)) {
        throw 'The symptom answer did not expose checked, unknown, and urgency conclusions.'
    }
    if (-not $nextStepText.Contains($eventViewer) -or
        $navigate.Current.Name -ne $openEventViewer -or
        -not $navigate.Current.IsEnabled) {
        throw 'The symptom answer did not expose exactly the expected allowlisted next step.'
    }
    if ($answer.Current.Name.Contains('eventvwr') -or
        $safety.Current.Name.Contains('eventvwr')) {
        throw 'The beginner answer exposed the system-tool command.'
    }

    Show-WpfWindowForSmoke $window
    Start-Sleep -Milliseconds 800
    Save-WindowScreenshot $window $screenshot
    Assert-NonBlankScreenshot $screenshot

    $newMmc = @(Get-Process mmc -ErrorAction SilentlyContinue | Where-Object { $baselineMmcIds -notcontains $_.Id })
    if ($newMmc.Count -ne 0) {
        throw 'Event Viewer started without the user choosing the allowlisted next step.'
    }
    $quarantineManifestCount = @(
        Get-ChildItem -LiteralPath $quarantineRoot -Recurse -File -Filter 'manifest.json' -ErrorAction SilentlyContinue
    ).Count
    if ($quarantineManifestCount -ne 0) {
        throw 'A quarantine manifest was created by a read-only symptom answer.'
    }

    [PSCustomObject]@{
        allQuickChoicesFirstView = $true
        symptomAnswerVisible = $true
        checkedUnknownUrgencyVisible = $true
        exactlyOneNextStepVisible = $true
        externalToolStarted = $false
        noOperationExecuted = ($quarantineManifestCount -eq 0)
        quarantineManifestCount = $quarantineManifestCount
        screenshotNonBlank = $true
        screenshot = $screenshot
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_QUARANTINE_ROOT = $previousQuarantineRoot
    Assert-ConfinedPath $dataRoot $PSScriptRoot
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
}
