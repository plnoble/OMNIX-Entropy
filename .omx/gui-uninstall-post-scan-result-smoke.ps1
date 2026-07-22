$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Join-Chars {
    param([int[]]$Codes)

    $characters = [System.Collections.Generic.List[char]]::new()
    foreach ($code in $Codes) {
        $characters.Add([char]$code)
    }

    return -join $characters
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $repoRoot '.omx\qa-uninstall-post-scan-result.png'

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

$uiText = @{
    ReviewNeeded = Join-Chars @(0x9700, 0x8981, 0x68C0, 0x67E5)
    AgentAdvice = Join-Chars @(0x5EFA, 0x8BAE, 0x5148, 0x67E5, 0x770B)
    NoFurtherDelete = Join-Chars @(0x4E0D, 0x4F1A, 0x7EE7, 0x7EED, 0x5220, 0x9664)
}

$process = Start-Process -FilePath $exe -ArgumentList '--smoke-uninstall-post-scan-review' -PassThru
$closedResultWindow = $false

try {
    $null = $process.WaitForInputIdle(15000)
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 15 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $processCondition)
    }
    if ($null -eq $window) {
        throw 'The post-scan result window was not found.'
    }

    Show-WpfWindowForSmoke $window

    $requiredIds = @(
        'UninstallPostScanTitleTextBlock',
        'UninstallPostScanStatusTextBlock',
        'UninstallPostScanConclusionTextBlock',
        'UninstallPostScanFactsListBox',
        'UninstallPostScanAgentAdviceTextBlock',
        'UninstallPostScanNextActionTextBlock',
        'UninstallPostScanSafetyTextBlock',
        'UninstallPostScanCloseButton'
    )
    $controls = @{}
    foreach ($automationId in $requiredIds) {
        $control = Find-ByAutomationId $window $automationId 1500
        if ($null -eq $control -or $control.Current.IsOffscreen) {
            throw "Required post-scan field was missing or offscreen: $automationId"
        }

        $controls[$automationId] = $control
    }

    if ($controls['UninstallPostScanStatusTextBlock'].Current.Name.IndexOf(
        $uiText.ReviewNeeded,
        [StringComparison]::Ordinal) -lt 0) {
        throw 'The post-scan status did not explain that review is needed.'
    }
    if ($controls['UninstallPostScanAgentAdviceTextBlock'].Current.Name.IndexOf(
        $uiText.AgentAdvice,
        [StringComparison]::Ordinal) -lt 0) {
        throw 'The post-scan result did not show the expected Agent advice.'
    }
    if ($controls['UninstallPostScanSafetyTextBlock'].Current.Name.IndexOf(
        $uiText.NoFurtherDelete,
        [StringComparison]::Ordinal) -lt 0) {
        throw 'The result window did not preserve the no-further-deletion safety boundary.'
    }

    $listItemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    $factItems = $controls['UninstallPostScanFactsListBox'].FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $listItemCondition)
    if ($factItems.Count -ne 3) {
        throw "Expected three beginner-facing facts, found $($factItems.Count)."
    }

    $visibleText = @(
        $controls['UninstallPostScanTitleTextBlock'].Current.Name,
        $controls['UninstallPostScanConclusionTextBlock'].Current.Name,
        $controls['UninstallPostScanAgentAdviceTextBlock'].Current.Name,
        $controls['UninstallPostScanSafetyTextBlock'].Current.Name
    ) -join "`n"
    if ($visibleText.Contains('C:\') -or $visibleText.Contains('SecretExampleService')) {
        throw 'The beginner result exposed a raw path or background identifier.'
    }

    $unexpectedExecute = Find-ByAutomationId $window 'UninstallPostScanExecuteButton' 250
    if ($null -ne $unexpectedExecute) {
        throw 'An execution control was exposed in the read-only result window.'
    }
    $noExecutionControl = $true

    Save-WindowScreenshot $window $screenshotPath
    Invoke-Element $controls['UninstallPostScanCloseButton']
    $closedResultWindow = $process.WaitForExit(5000)
    if (-not $closedResultWindow) {
        throw 'The post-scan result window did not close.'
    }

    [pscustomobject]@{
        resultWindowFound = $true
        statusVisible = $true
        agentAdviceVisible = $true
        factCount = $factItems.Count
        noExecutionControl = $noExecutionControl
        closedResultWindow = $closedResultWindow
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}
