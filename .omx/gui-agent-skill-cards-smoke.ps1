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
$dataRoot = Join-Path $PSScriptRoot 'qa-agent-skill-card-data'
$screenshot = Join-Path $PSScriptRoot 'qa-agent-skill-card-response.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$process = $null

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

    Invoke-Element (Find-ByAutomationId $window 'AgentNavButton' 3000)
    $skillButton = Find-ByAutomationId $window 'AgentSkillActionButton_WindowAndDesktop' 5000
    if ($null -eq $skillButton -or -not $skillButton.Current.IsEnabled) {
        throw 'The window-and-desktop skill action was not available.'
    }
    Invoke-Element $skillButton

    $headline = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationHeadlineTextBlock' 300
        $notOpen = Get-UnicodeText @(0x8FD8, 0x6CA1, 0x6709, 0x5F00, 0x653E)
        if ($null -ne $candidate -and -not $candidate.Current.IsOffscreen -and
            $candidate.Current.Name.Contains($notOpen)) {
            return $candidate
        }
        return $null
    }
    $answer = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 2000
    $safety = Find-ByAutomationId $window 'AgentConversationSafetyTextBlock' 2000
    if ($null -eq $headline -or $null -eq $answer -or $null -eq $safety) {
        throw 'The skill-card Agent response was incomplete.'
    }
    if ($answer.Current.IsOffscreen -or $safety.Current.IsOffscreen) {
        throw 'The skill-card conclusion was outside the first visible Agent area.'
    }

    $notRead = Get-UnicodeText @(0x8FD8, 0x6CA1, 0x6709, 0x8BFB, 0x53D6)
    $windowLabel = Get-UnicodeText @(0x7A97, 0x53E3)
    $desktopLabel = Get-UnicodeText @(0x684C, 0x9762)
    if (-not $answer.Current.Name.Contains($notRead) -or
        -not $answer.Current.Name.Contains($windowLabel) -or
        -not $answer.Current.Name.Contains($desktopLabel)) {
        throw 'The unavailable skill did not explain its missing evidence plainly.'
    }
    $navigate = Find-ByAutomationId $window 'AgentConversationNavigateButton' 500
    if ($null -ne $navigate -and -not $navigate.Current.IsOffscreen) {
        throw 'The unavailable desktop skill exposed a visible navigation action.'
    }

    Show-WpfWindowForSmoke $window
    Start-Sleep -Milliseconds 800
    Save-WindowScreenshot $window $screenshot

    [PSCustomObject]@{
        skillButtonInvoked = $true
        truthfulUnavailableConclusionVisible = $true
        unsafeNextActionVisible = $false
        noOperationExecuted = $true
        screenshot = $screenshot
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
}
