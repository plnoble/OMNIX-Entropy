$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-agent-system-and-settings.png'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

Initialize-WpfSmokeAutomation

$process = Start-Process -FilePath $exe -PassThru
try {
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ProcessIdProperty), $process.Id

    $window = Wait-Until -TimeoutSeconds 12 -IntervalMilliseconds 250 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }
    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    $window.SetFocus()
    $agentNav = Find-ByAutomationId $window 'AgentNavButton'
    if ($null -eq $agentNav) {
        throw 'AgentNavButton was not found.'
    }

    Invoke-Element $agentNav
    Start-Sleep -Milliseconds 500

    $toolList = Find-ByAutomationId $window 'AgentSystemToolListBox'
    if ($null -eq $toolList) {
        throw 'AgentSystemToolListBox was not found.'
    }

    $settingsList = Find-ByAutomationId $window 'AgentWindowsSettingsListBox'
    if ($null -eq $settingsList) {
        throw 'AgentWindowsSettingsListBox was not found.'
    }

    $buttonCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ControlTypeProperty), ([System.Windows.Automation.ControlType]::Button)
    $buttons = $toolList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    if ($buttons.Count -lt 1) {
        throw 'No open buttons were found inside AgentSystemToolListBox.'
    }

    $settingsButtons = $settingsList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    if ($settingsButtons.Count -lt 1) {
        throw 'No open buttons were found inside AgentWindowsSettingsListBox.'
    }

    Save-WindowScreenshot $window $screenshotPath

    [PSCustomObject]@{
        agentSystemToolListFound = $true
        visibleOpenButtonCount = $buttons.Count
        agentWindowsSettingsListFound = $true
        visibleSettingsOpenButtonCount = $settingsButtons.Count
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
