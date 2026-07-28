$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "wpf-smoke-helpers.ps1")
Initialize-WpfSmokeAutomation

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot "src\Css.App\bin\Debug\net8.0-windows\Css.App.exe"
$dataRoot = Join-Path $repoRoot ".omx\qa-app-system-footprint-data"
$fixturePath = Join-Path $repoRoot ".omx\qa-app-system-footprint-fixture.json"
$screenshotPath = Join-Path $repoRoot ".omx\qa-app-system-footprint.png"
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$process = $null

try {
    if (-not (Test-Path -LiteralPath $exe)) {
        throw "Css.App.exe was not found: $exe"
    }

    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null

    $profile = [ordered]@{
        name = "OMNIX Fixture"
        publisher = "OMNIX Smoke"
        category = 1
        installPath = "D:\Software\OMNIX Fixture\Install"
        systemFootprints = @(
            [ordered]@{
                kind = 0
                displayName = "Upload with OMNIX"
                sourceLocator = "HKCU64\Software\Classes\*\shell\OMNIX"
                evidence = "D:\Software\OMNIX Fixture\Install\shell.dll"
            },
            [ordered]@{
                kind = 2
                displayName = "com.omnix.fixture"
                sourceLocator = "HKCU64\Software\Google\Chrome\NativeMessagingHosts\com.omnix.fixture"
                evidence = "D:\Software\OMNIX Fixture\Install\host.json"
            }
        )
    }
    [ordered]@{ scans = @(,@($profile)) } |
        ConvertTo-Json -Depth 8 |
        Set-Content -LiteralPath $fixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $fixturePath
    $process = Start-Process -FilePath $exe -PassThru

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

    $appsButton = Find-ByAutomationId $window "AppsNavButton" 1000
    if ($null -eq $appsButton) {
        throw "Apps navigation button was not found."
    }
    Invoke-Element $appsButton

    $scanButton = Find-ByAutomationId $window "ScanSoftwareButton" 1000
    if ($null -eq $scanButton) {
        throw "Software scan button was not found."
    }
    Invoke-Element $scanButton

    $rightClickText = -join @([char]0x53F3, [char]0x952E, [char]0x83DC, [char]0x5355)
    $browserText = -join @([char]0x6D4F, [char]0x89C8, [char]0x5668, [char]0x8FDE, [char]0x63A5)
    $summary = Wait-Until -TimeoutSeconds 25 -Probe {
        $candidate = Find-ByAutomationId $window "DrawerSystemFootprintTextBlock" 250
        if (($null -ne $candidate) -and
            $candidate.Current.Name.Contains($rightClickText) -and
            $candidate.Current.Name.Contains($browserText)) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $summary) {
        throw "Beginner-facing system footprint summary was not visible."
    }

    $advice = Find-ByAutomationId $window "DrawerAdviceTextBlock" 1000
    if ($null -eq $advice -or [string]::IsNullOrWhiteSpace($advice.Current.Name)) {
        throw "Agent advice was not visible."
    }

    Save-WindowScreenshot $window $screenshotPath
    [PSCustomObject]@{
        App = "OMNIX Fixture"
        Summary = $summary.Current.Name
        AgentAdvice = $advice.Current.Name
        Screenshot = $screenshotPath
        ReadOnly = $true
    } | ConvertTo-Json -Depth 4
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousFixture
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $fixturePath -Force -ErrorAction SilentlyContinue
}
