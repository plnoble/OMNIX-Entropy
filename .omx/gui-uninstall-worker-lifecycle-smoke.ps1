param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Accept', 'Cancel')]
    [string]$ExpectedOutcome
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshot = Join-Path $repoRoot ".omx\qa-uninstall-worker-$($ExpectedOutcome.ToLowerInvariant()).png"

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

Write-Host 'Windows will show an administrator confirmation for the fake-only OMNIX worker.'
Write-Host "For this smoke, choose: $ExpectedOutcome"
Write-Host 'No uninstaller, cleanup handler, registry edit, service change, or file mutation is registered.'

$process = Start-Process `
    -FilePath $exe `
    -ArgumentList '--smoke-uninstall-worker-lifecycle' `
    -PassThru
$windowClosed = $false

try {
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $window = Wait-Until -TimeoutSeconds 90 -IntervalMilliseconds 300 -Probe {
        $processCondition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
            $process.Id)
        $candidate = $root.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $processCondition)
        if ($null -eq $candidate) {
            return $null
        }
        $title = Find-ByAutomationId `
            $candidate `
            'OfficialUninstallWorkerResultTitleTextBlock' `
            250
        if ($null -eq $title) {
            return $null
        }
        return $candidate
    }
    if ($null -eq $window) {
        throw 'The worker lifecycle result window was not found after the UAC choice.'
    }

    Show-WpfWindowForSmoke $window
    $title = Find-ByAutomationId $window 'OfficialUninstallWorkerResultTitleTextBlock' 1500
    $status = Find-ByAutomationId $window 'OfficialUninstallWorkerResultStatusTextBlock' 1500
    $conclusion = Find-ByAutomationId $window 'OfficialUninstallWorkerResultConclusionTextBlock' 1500
    $advice = Find-ByAutomationId $window 'OfficialUninstallWorkerResultAgentAdviceTextBlock' 1500
    $safety = Find-ByAutomationId $window 'OfficialUninstallWorkerResultSafetyTextBlock' 1500
    $close = Find-ByAutomationId $window 'OfficialUninstallWorkerResultCloseButton' 1500
    foreach ($required in @($title, $status, $conclusion, $advice, $safety, $close)) {
        if ($null -eq $required -or $required.Current.IsOffscreen) {
            throw 'A required beginner worker result was missing or offscreen.'
        }
    }

    $acceptTitle = -join (0x5B89, 0x5168, 0x8FDE, 0x63A5, 0x6D4B, 0x8BD5, 0x5DF2, 0x5B8C, 0x6210 | ForEach-Object { [char]$_ })
    $cancelTitle = -join (0x4F60, 0x53D6, 0x6D88, 0x4E86, 0x20, 0x57, 0x69, 0x6E, 0x64, 0x6F, 0x77, 0x73, 0x20, 0x786E, 0x8BA4 | ForEach-Object { [char]$_ })
    $expectedTitle = if ($ExpectedOutcome -eq 'Accept') { $acceptTitle } else { $cancelTitle }
    if (-not $title.Current.Name.Contains($expectedTitle)) {
        throw "Unexpected worker result '$($title.Current.Name)'; expected '$expectedTitle'."
    }
    if ($conclusion.Current.Name -match '[A-Z]:\\|PID|ECDH|Protocol') {
        throw 'The beginner conclusion exposed a path or protocol term.'
    }
    if ([string]::IsNullOrWhiteSpace($safety.Current.Name) -or
        $safety.Current.Name -match '[A-Z]:\\|PID|ECDH|Protocol') {
        throw 'The worker result did not provide a path-free safety outcome.'
    }

    $titleText = $title.Current.Name
    $statusText = $status.Current.Name

    Save-WindowScreenshot $window $screenshot
    Invoke-Element $close
    $windowClosed = $process.WaitForExit(5000)
    if (-not $windowClosed) {
        throw 'The App did not exit after closing the worker result.'
    }

    [pscustomobject]@{
        expectedOutcome = $ExpectedOutcome
        title = $titleText
        status = $statusText
        beginnerConclusionVisible = $true
        agentAdviceVisible = $true
        safetyTextVisible = $true
        appExited = $windowClosed
        screenshot = $screenshot
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}
