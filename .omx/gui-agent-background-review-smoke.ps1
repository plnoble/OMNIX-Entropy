$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-agent-startup-service-plan.png'

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

    $appsNav = Find-ByAutomationId $window 'AppsNavButton'
    if ($null -eq $appsNav) {
        throw 'AppsNavButton was not found.'
    }

    Invoke-Element $appsNav
    Start-Sleep -Milliseconds 500

    $scanButton = Find-ByAutomationId $window 'ScanSoftwareButton'
    if ($null -eq $scanButton) {
        throw 'ScanSoftwareButton was not found.'
    }

    Invoke-Element $scanButton

    $appTiles = Wait-Until -TimeoutSeconds 45 -Probe {
        $tiles = Find-ByAutomationId $window 'AppTilesListBox' 500
        if ($null -eq $tiles) {
            return $null
        }

        $itemCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
            ([System.Windows.Automation.AutomationElement]::ControlTypeProperty), ([System.Windows.Automation.ControlType]::ListItem)
        $items = $tiles.FindAll([System.Windows.Automation.TreeScope]::Descendants, $itemCondition)
        if ($items.Count -gt 5) {
            return $items
        }

        return $null
    }

    if ($null -eq $appTiles) {
        throw 'App tiles were not found after software scan.'
    }

    $agentNav = Find-ByAutomationId $window 'AgentNavButton'
    if ($null -eq $agentNav) {
        throw 'AgentNavButton was not found.'
    }

    Invoke-Element $agentNav
    Start-Sleep -Milliseconds 500

    $summary = Find-ByAutomationId $window 'AgentBackgroundReviewSummaryTextBlock' 5000
    $itemsList = Find-ByAutomationId $window 'AgentBackgroundReviewItemsListBox' 5000
    $safety = Find-ByAutomationId $window 'AgentBackgroundReviewSafetyTextBlock' 5000
    $planTitle = Find-ByAutomationId $window 'AgentStartupServicePlanTitleTextBlock' 5000
    $planSummary = Find-ByAutomationId $window 'AgentStartupServicePlanSummaryTextBlock' 5000
    $planStepsList = Find-ByAutomationId $window 'AgentStartupServicePlanStepsListBox' 5000
    $planSafety = Find-ByAutomationId $window 'AgentStartupServicePlanSafetyTextBlock' 5000

    if ($null -eq $summary -or $summary.Current.IsOffscreen) {
        throw 'Agent background review summary is not visible.'
    }
    if ($null -eq $itemsList -or $itemsList.Current.IsOffscreen) {
        throw 'Agent background review items list is not visible.'
    }
    if ($null -eq $safety) {
        throw 'Agent background review safety line was not found.'
    }
    if ($null -eq $planTitle -or $planTitle.Current.IsOffscreen) {
        throw 'Agent startup/service plan title is not visible.'
    }
    if ($null -eq $planSummary -or $planSummary.Current.IsOffscreen) {
        throw 'Agent startup/service plan summary is not visible.'
    }
    $previewOnlyPhrase = -join ([char[]](0x53EA, 0x751F, 0x6210, 0x65B9, 0x6848))
    if (-not $planSummary.Current.Name.Contains($previewOnlyPhrase)) {
        throw "Agent startup/service plan summary did not state preview-only behavior: $($planSummary.Current.Name)"
    }
    if ($null -eq $planStepsList -or $planStepsList.Current.IsOffscreen) {
        throw 'Agent startup/service plan steps list is not visible.'
    }
    if ($null -eq $planSafety) {
        throw 'Agent startup/service plan safety line was not found.'
    }

    $listItemCondition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList `
        ([System.Windows.Automation.AutomationElement]::ControlTypeProperty), ([System.Windows.Automation.ControlType]::ListItem)
    $reviewItems = $itemsList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
    if ($reviewItems.Count -lt 1) {
        throw 'No Agent background review items were visible.'
    }
    $planStepItems = $planStepsList.FindAll([System.Windows.Automation.TreeScope]::Descendants, $listItemCondition)
    if ($planStepItems.Count -lt 1) {
        throw 'No Agent startup/service plan steps were visible.'
    }

    Save-WindowScreenshot $window $screenshotPath

    [PSCustomObject]@{
        appTileCount = $appTiles.Count
        backgroundSummaryFound = $true
        backgroundReviewItemCount = $reviewItems.Count
        startupServicePlanFound = $true
        startupServicePlanStepCount = $planStepItems.Count
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
