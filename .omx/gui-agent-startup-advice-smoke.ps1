$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

function Get-Sha256Text([string]$Text) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return ([System.BitConverter]::ToString($hash)).Replace('-', '')
}

function Get-Sha256Bytes([byte[]]$Bytes) {
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($Bytes)
    return ([System.BitConverter]::ToString($hash)).Replace('-', '')
}

function Get-UnicodeText([int[]]$CodePoints) {
    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Get-DescendantText($Element) {
    $textCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    $texts = $Element.FindAll([System.Windows.Automation.TreeScope]::Descendants, $textCondition)
    return (($texts | ForEach-Object { $_.Current.Name }) -join "`n")
}

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$dataRoot = Join-Path $PSScriptRoot 'qa-agent-startup-advice-data'
$softwareFixturePath = Join-Path $PSScriptRoot 'qa-agent-startup-advice-software.json'
$screenshotPath = Join-Path $PSScriptRoot 'qa-agent-startup-advice.png'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousSoftwareFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE
$process = $null

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

try {
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null

    $name = 'Fixture Startup'
    $profileName = 'Startup Safety Fixture'
    $command = 'D:\Software\Fixture\fixture.exe --background'
    $sourceLocator = 'HKCU64\Software\Microsoft\Windows\CurrentVersion\Run'
    $approvalLocator = 'HKCU64\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run'
    $approvalBytes = [byte[]](2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    $approvalHash = Get-Sha256Bytes $approvalBytes
    $approvalMaterial = $approvalLocator.ToUpperInvariant() + "`n" +
        $name.ToUpperInvariant() + "`nPresentBinary`n12`n" + $approvalHash
    $identityMaterial = "StartupEntry`n" + $sourceLocator.ToUpperInvariant() + "`n" + $name.ToUpperInvariant()
    $observationMaterial = $identityMaterial + "`n" + $command + "`n" + $approvalMaterial + "`nUnknown`nNotApplicable"
    $observedAt = [DateTimeOffset]::UtcNow

    $profile = [ordered]@{
        name = $profileName
        publisher = 'OMNIX Smoke'
        category = 1
        installPath = 'D:\Software\Fixture\Install'
        startupEntries = @($name)
        backgroundComponents = @(
            [ordered]@{
                identity = [ordered]@{
                    kind = 0
                    stableId = Get-Sha256Text $identityMaterial
                    displayName = $name
                    sourceLocator = $sourceLocator
                }
                observedAtUtc = $observedAt.ToString('o')
                observationFingerprint = Get-Sha256Text $observationMaterial
                activationState = 0
                runtimeState = 0
                startupApproval = [ordered]@{
                    approvalKeyLocator = $approvalLocator
                    valueName = $name
                    status = 2
                    payloadFingerprint = $approvalHash
                    payloadLength = 12
                }
                requiredRollbackEvidence = @(0, 1, 5)
            }
        )
    }
    [ordered]@{ scans = @(,@($profile)) } |
        ConvertTo-Json -Depth 12 |
        Set-Content -LiteralPath $softwareFixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $dataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $softwareFixturePath
    $process = Start-Process -FilePath $exe -PassThru

    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 20 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }
    if ($null -eq $window) { throw 'Main window was not found.' }

    Invoke-Element (Find-ByAutomationId $window 'AppsNavButton' 5000)
    Invoke-Element (Find-ByAutomationId $window 'ScanSoftwareButton' 5000)
    $tileList = Wait-Until -TimeoutSeconds 20 -Probe {
        $list = Find-ByAutomationId $window 'AppTilesListBox' 500
        if ($null -eq $list) { return $null }
        $text = Get-DescendantText $list
        if ($text.Contains($profileName)) { return $list }
        return $null
    }
    if ($null -eq $tileList) { throw 'Structured startup fixture app was not loaded.' }

    Invoke-Element (Find-ByAutomationId $window 'AgentNavButton' 5000)
    $question = Find-ByAutomationId $window 'AgentQuestionTextBox' 5000
    $ask = Find-ByAutomationId $window 'AskComputerAgentButton' 5000
    if ($null -eq $question -or $null -eq $ask) { throw 'Agent question controls were not found.' }
    $valuePattern = $question.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    $questionPrefix = Get-UnicodeText @(0x5E2E, 0x6211, 0x5173, 0x95ED)
    $questionSuffix = Get-UnicodeText @(0x7684, 0x5F00, 0x673A, 0x81EA, 0x542F, 0x52A8)
    $valuePattern.SetValue("$questionPrefix $profileName $questionSuffix")
    Invoke-Element $ask

    $answer = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'AgentConversationAnswerTextBlock' 500
        if ($null -ne $candidate -and $candidate.Current.Name.Contains('OMNIX')) { return $candidate }
        return $null
    }
    $headline = Find-ByAutomationId $window 'AgentConversationHeadlineTextBlock' 2000
    $nextSteps = Find-ByAutomationId $window 'AgentConversationNextStepsListBox' 2000
    $navigate = Find-ByAutomationId $window 'AgentConversationNavigateButton' 2000
    if ($null -eq $answer -or $null -eq $headline -or $null -eq $nextSteps -or $null -eq $navigate) {
        throw 'Agent startup answer was incomplete.'
    }
    if ($answer.Current.IsOffscreen -or $headline.Current.IsOffscreen -or $navigate.Current.IsOffscreen) {
        throw 'Agent startup conclusion was not in the visible working area.'
    }

    $nextStepText = Get-DescendantText $nextSteps
    $visibleText = $headline.Current.Name + "`n" + $answer.Current.Name + "`n" + $nextStepText
    $reversibleText = Get-UnicodeText @(0x53EF, 0x8FD8, 0x539F)
    $freshReadText = Get-UnicodeText @(0x91CD, 0x65B0, 0x8BFB, 0x53D6)
    $reviewPlanText = Get-UnicodeText @(0x5BA1, 0x6838, 0x5173, 0x95ED, 0x65B9, 0x6848)
    foreach ($required in @('OMNIX', $reversibleText, $freshReadText, $reviewPlanText)) {
        if (-not $visibleText.Contains($required)) {
            throw "Agent startup answer missed required beginner text: $required"
        }
    }
    foreach ($private in @('HKCU64', 'fixture.exe', 'D:\Software')) {
        if ($visibleText.Contains($private)) {
            throw "Agent startup answer exposed a technical identifier: $private"
        }
    }
    if (-not $navigate.Current.IsEnabled) { throw 'Exact-app Agent navigation was not enabled.' }

    Start-Sleep -Milliseconds 750
    Save-WindowScreenshot $window $screenshotPath
    Invoke-Element $navigate
    $drawerTitle = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'DrawerTitleTextBlock' 500
        if ($null -ne $candidate -and $candidate.Current.Name.Contains($profileName)) { return $candidate }
        return $null
    }
    $startupButton = Find-ByAutomationId $window 'DrawerDisableStartupButton' 2000
    if ($null -eq $drawerTitle -or $null -eq $startupButton -or -not $startupButton.Current.IsEnabled) {
        throw 'Agent did not navigate to the exact app startup review entry.'
    }

    [PSCustomObject]@{
        agentAnswerVisible = $true
        localReviewExplained = $true
        exactAppNavigationReached = $true
        startupReviewButtonEnabled = $true
        noOperationExecuted = $true
        screenshot = $screenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousSoftwareFixture
    Remove-Item -LiteralPath $dataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
}
