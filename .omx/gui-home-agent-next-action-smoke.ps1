$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repoRoot 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$screenshotPath = Join-Path $PSScriptRoot 'qa-home-agent-next-action.png'
$drivePlanScreenshotPath = Join-Path $PSScriptRoot 'qa-drive-health-plan.png'
$isolatedDataRoot = Join-Path $PSScriptRoot 'qa-home-agent-data'
$scanRoot = Join-Path 'C:\tmp' ('OMNIX-HomeHealth-Smoke-' + [Guid]::NewGuid().ToString('N'))
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousScanRoot = $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Css.App.exe was not found. Build the solution first: $exe"
}

function Find-ButtonByName {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [string]$Name
    )

    $typeCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $nameCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $Name)
    $condition = [System.Windows.Automation.AndCondition]::new(
        $typeCondition,
        $nameCondition)
    return $Root.FindFirst(
        [System.Windows.Automation.TreeScope]::Descendants,
        $condition)
}

function Get-ListItemCount {
    param([System.Windows.Automation.AutomationElement]$List)

    $condition = [System.Windows.Automation.OrCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::DataItem))
    return $List.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $condition).Count
}

function Get-UnicodeText {
    param([int[]]$CodePoints)

    return -join ($CodePoints | ForEach-Object { [char]$_ })
}

function Get-DescendantText {
    param([System.Windows.Automation.AutomationElement]$Element)

    $textCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Text)
    $texts = $Element.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $textCondition)
    return (($texts | ForEach-Object { $_.Current.Name }) -join ' ')
}

$process = $null
try {
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $isolatedDataRoot -Force | Out-Null
    $fixtureTemp = Join-Path $scanRoot 'Temp'
    New-Item -ItemType Directory -Path $fixtureTemp -Force | Out-Null
    [System.IO.File]::WriteAllBytes(
        (Join-Path $fixtureTemp 'home-agent-cache.bin'),
        (New-Object byte[] 4096))
    foreach ($index in 1..6) {
        $unexpectedRoot = Join-Path $scanRoot ('Unexpected-' + $index)
        New-Item -ItemType Directory -Path $unexpectedRoot -Force | Out-Null
        [System.IO.File]::WriteAllBytes(
            (Join-Path $unexpectedRoot ('evidence-' + $index + '.bin')),
            (New-Object byte[] 4096))
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $scanRoot

    $process = Start-Process -FilePath $exe -PassThru
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $processCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 15 -Probe {
        $root.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $processCondition)
    }
    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    Show-WpfWindowForSmoke $window

    $startScan = Find-ByAutomationId $window 'StartScanButton' 1000
    if ($null -eq $startScan) {
        throw 'StartScanButton was not found.'
    }
    Invoke-Element $startScan

    $lastHealthDimensionCount = 0
    $healthDimensions = Wait-Until -TimeoutSeconds 90 -Probe {
        if ($process.HasExited) {
            throw "The app exited before rendering health dimensions. Exit code: $($process.ExitCode)"
        }
        $candidate = Find-ByAutomationId $window 'HealthDimensionListView' 250
        if ($null -ne $candidate) {
            $lastHealthDimensionCount = Get-ListItemCount $candidate
            if ($lastHealthDimensionCount -ge 7) {
                return $candidate
            }
        }
        return $null
    }
    if ($null -eq $healthDimensions) {
        $status = Find-ByAutomationId $window 'StatusTextBlock' 500
        $statusText = if ($null -eq $status) { '<missing>' } else { $status.Current.Name }
        throw "The whole-PC health dimensions were not rendered after the fixture scan. UIAutomation list items: $lastHealthDimensionCount; status: $statusText"
    }
    $healthDimensionCount = Get-ListItemCount $healthDimensions

    $dimensionNames = @(
        [PSCustomObject]@{ Key = 'secondaryDrive'; Name = 'D ' + (Get-UnicodeText @(0x76D8, 0x7A7A, 0x95F4)) },
        [PSCustomObject]@{ Key = 'memory'; Name = Get-UnicodeText @(0x5185, 0x5B58, 0x5360, 0x7528) },
        [PSCustomObject]@{ Key = 'battery'; Name = Get-UnicodeText @(0x7535, 0x6C60, 0x72B6, 0x6001) },
        [PSCustomObject]@{ Key = 'startup'; Name = Get-UnicodeText @(0x81EA, 0x542F, 0x52A8, 0x7EBF, 0x7D22) },
        [PSCustomObject]@{ Key = 'usage'; Name = Get-UnicodeText @(0x4F7F, 0x7528, 0x8D8B, 0x52BF) }
    )
    $machineHealthRows = [ordered]@{}
    foreach ($dimension in $dimensionNames) {
        $automationId = 'HealthDimension_' + $dimension.Name
        $row = Find-ByAutomationId $window $automationId 1000
        if ($null -eq $row) {
            throw "Machine-health row was not found: $automationId"
        }
        if ($row.Current.IsOffscreen) {
            try {
                $scrollItem = $row.GetCurrentPattern(
                    [System.Windows.Automation.ScrollItemPattern]::Pattern)
                $scrollItem.ScrollIntoView()
            }
            catch {
                throw "Machine-health row could not scroll into view: $automationId"
            }

            $visibleRow = Wait-Until -TimeoutSeconds 3 -Probe {
                if (-not $row.Current.IsOffscreen) {
                    return $row
                }

                return $null
            }
            if ($null -eq $visibleRow) {
                throw "Machine-health row stayed offscreen after scrolling: $automationId"
            }
        }

        $rowText = Get-DescendantText $row
        if ([string]::IsNullOrWhiteSpace($rowText) -or $rowText.Length -le $dimension.Name.Length) {
            throw "Machine-health row had no beginner result: $automationId"
        }
        foreach ($privateMarker in @(':\', 'HKCU', 'HKLM', '.exe')) {
            if ($rowText.Contains($privateMarker)) {
                throw "Machine-health row exposed a private or technical identifier: $automationId"
            }
        }
        $machineHealthRows[$dimension.Key] = $rowText
    }

    $notRead = Get-UnicodeText @(0x672A, 0x8BFB, 0x53D6)
    $notFound = Get-UnicodeText @(0x672A, 0x53D1, 0x73B0)
    $used = Get-UnicodeText @(0x5DF2, 0x4F7F, 0x7528)
    if (-not ($machineHealthRows.secondaryDrive.Contains($used) -or
        $machineHealthRows.secondaryDrive.Contains($notFound) -or
        $machineHealthRows.secondaryDrive.Contains($notRead))) {
        throw 'D-drive row did not explain available, absent, or unavailable state.'
    }
    if (-not ($machineHealthRows.memory.Contains('GB') -or $machineHealthRows.memory.Contains($notRead))) {
        throw 'Memory row did not contain a plausible value or unavailable state.'
    }
    $processLabel = Get-UnicodeText @(0x4E2A, 0x8FDB, 0x7A0B)
    if ($machineHealthRows.memory.Contains('GB') -and
        $machineHealthRows.memory -notmatch ('\d+\s*' + [regex]::Escape($processLabel))) {
        throw 'Available memory row did not include a count-only process result.'
    }
    $charge = Get-UnicodeText @(0x7535, 0x91CF)
    $notDetected = Get-UnicodeText @(0x672A, 0x68C0, 0x6D4B)
    if (-not ($machineHealthRows.battery.Contains($charge) -or
        $machineHealthRows.battery.Contains($notDetected) -or
        $machineHealthRows.battery.Contains($notRead))) {
        throw 'Battery row did not explain available, absent, or unavailable state.'
    }
    $clue = Get-UnicodeText @(0x7EBF, 0x7D22)
    if (-not $machineHealthRows.startup.Contains($clue)) {
        throw 'Startup row was not explicitly labeled as a clue.'
    }
    $manualCheck = Get-UnicodeText @(0x624B, 0x52A8, 0x4F53, 0x68C0)
    if (-not $machineHealthRows.usage.Contains($manualCheck)) {
        throw 'Usage row did not explain that its history comes from manual checks.'
    }

    $healthGoalHeadline = Find-ByAutomationId $window 'HomeDriveHealthPlanHeadlineTextBlock' 1000
    $healthGoalProgress = Find-ByAutomationId $window 'HomeDriveHealthPlanProgressTextBlock' 1000
    $healthGoalButton = Find-ByAutomationId $window 'HomeDriveHealthPlanButton' 1000
    foreach ($goalControl in @($healthGoalHeadline, $healthGoalProgress, $healthGoalButton)) {
        if ($null -eq $goalControl -or $goalControl.Current.IsOffscreen) {
            throw 'The beginner drive-health goal was not visible in the first working area.'
        }
        if ([string]::IsNullOrWhiteSpace($goalControl.Current.Name)) {
            throw 'The beginner drive-health goal contained an empty field.'
        }
    }
    if (-not $healthGoalHeadline.Current.Name.Contains('80%')) {
        throw 'The drive-health goal did not state the transparent 80% comfort target.'
    }
    if ($healthGoalProgress.Current.Name -notmatch '\d+(\.\d+)?\s+(KB|MB|GB|TB)') {
        throw 'The drive-health progress did not quantify safe cleanup or the remaining gap.'
    }
    if (-not $healthGoalButton.Current.IsEnabled) {
        throw 'The Agent-led drive-health next step was not enabled for the cleanup fixture.'
    }

    Invoke-Element $healthGoalButton
    $selectedPlanTakeaway = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'RecommendationActionTakeawayTextBlock' 250
        if ($null -ne $candidate -and
            -not $candidate.Current.IsOffscreen -and
            -not [string]::IsNullOrWhiteSpace($candidate.Current.Name)) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $selectedPlanTakeaway) {
        throw 'The Agent-led next step did not open the selected low-risk preview.'
    }
    $selectedPlanButton = Find-ByAutomationId $window 'ExecuteRecommendationButton' 1000
    if ($null -eq $selectedPlanButton -or
        $selectedPlanButton.Current.IsOffscreen -or
        -not $selectedPlanButton.Current.IsEnabled) {
        throw 'The selected low-risk preview did not bring its non-executing confirmation entry into view.'
    }
    Save-WindowScreenshot $window $drivePlanScreenshotPath

    $homeButton = Find-ByAutomationId $window 'HomeNavButton' 1000
    if ($null -eq $homeButton) {
        throw 'HomeNavButton was not found after the drive-plan preview.'
    }
    Invoke-Element $homeButton

    $keyFindings = Find-ByAutomationId $window 'KeyFindingsListBox' 1000
    if ($null -eq $keyFindings) {
        throw 'KeyFindingsListBox was not found.'
    }

    $planButtonName = -join @(
        [char]0x751F, [char]0x6210, [char]0x5904, [char]0x7406,
        [char]0x65B9, [char]0x6848)
    $planButton = Wait-Until -TimeoutSeconds 30 -Probe {
        Find-ButtonByName $keyFindings $planButtonName
    }
    if ($null -eq $planButton) {
        throw 'The health-finding plan button was not found after the fixture scan.'
    }
    Invoke-Element $planButton

    $navigateButton = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'HomeAgentResponseNavigateButton' 250
        if ($null -ne $candidate -and
            -not $candidate.Current.IsOffscreen -and
            $candidate.Current.IsEnabled -and
            -not [string]::IsNullOrWhiteSpace($candidate.Current.Name)) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $navigateButton) {
        throw 'The home Agent next-action button was not visible and enabled.'
    }

    $visibleFields = New-Object System.Collections.Generic.List[string]
    foreach ($fieldId in @(
        'HomeAgentResponseTitleTextBlock',
        'HomeAgentResponseBodyTextBlock',
        'HomeAgentResponseSafetyTextBlock',
        'HomeAgentResponseNavigateButton')) {
        $field = Find-ByAutomationId $window $fieldId 1000
        if ($null -eq $field) {
            throw "Home Agent response field was not found: $fieldId"
        }
        if ($field.Current.IsOffscreen) {
            throw "Home Agent response field was offscreen: $fieldId"
        }
        if ([string]::IsNullOrWhiteSpace($field.Current.Name)) {
            throw "Home Agent response field was empty: $fieldId"
        }
        $visibleFields.Add($fieldId)
    }

    $keyFindingScroll = $keyFindings.GetCurrentPattern(
        [System.Windows.Automation.ScrollPattern]::Pattern)
    if (-not $keyFindingScroll.Current.VerticallyScrollable) {
        throw 'KeyFindingsListBox did not expose a vertically scrollable viewport.'
    }
    $scrollBefore = $keyFindingScroll.Current.VerticalScrollPercent
    $keyFindingScroll.Scroll(
        [System.Windows.Automation.ScrollAmount]::NoAmount,
        [System.Windows.Automation.ScrollAmount]::LargeIncrement)
    $scrollAfter = Wait-Until -TimeoutSeconds 5 -Probe {
        $percent = $keyFindingScroll.Current.VerticalScrollPercent
        if ($percent -gt $scrollBefore) {
            return $percent
        }

        return $null
    }
    if ($null -eq $scrollAfter) {
        throw 'KeyFindingsListBox accepted a scroll command but did not change position.'
    }

    $historyScrollElement = Find-ByAutomationId $window 'HealthDigestHistoryScrollViewer' 1000
    if ($null -eq $historyScrollElement) {
        throw 'HealthDigestHistoryScrollViewer was not found.'
    }
    $historyScroll = $historyScrollElement.GetCurrentPattern(
        [System.Windows.Automation.ScrollPattern]::Pattern)
    if (-not $historyScroll.Current.VerticallyScrollable) {
        throw 'The health-history summary did not expose a vertically scrollable viewport.'
    }
    $historyScrollBefore = $historyScroll.Current.VerticalScrollPercent
    $historyScroll.Scroll(
        [System.Windows.Automation.ScrollAmount]::NoAmount,
        [System.Windows.Automation.ScrollAmount]::LargeIncrement)
    $historyScrollAfter = Wait-Until -TimeoutSeconds 5 -Probe {
        $percent = $historyScroll.Current.VerticalScrollPercent
        if ($percent -gt $historyScrollBefore) {
            return $percent
        }

        return $null
    }
    if ($null -eq $historyScrollAfter) {
        throw 'The health-history summary did not change position after scrolling.'
    }

    Start-Sleep -Milliseconds 500
    Save-WindowScreenshot $window $screenshotPath

    $nextActionLabel = $navigateButton.Current.Name
    Invoke-Element $navigateButton
    $recommendations = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'RecommendationsListBox' 250
        if ($null -ne $candidate -and -not $candidate.Current.IsOffscreen) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $recommendations) {
        throw 'The Agent next action did not open the C-drive evidence page.'
    }

    [PSCustomObject]@{
        fixtureScanRoot = $scanRoot
        healthDimensionCount = $healthDimensionCount
        machineHealthRows = $machineHealthRows
        visibleResponseFields = $visibleFields.Count
        keyFindingScrollBefore = $scrollBefore
        keyFindingScrollAfter = $scrollAfter
        historyScrollBefore = $historyScrollBefore
        historyScrollAfter = $historyScrollAfter
        healthGoalHeadline = $healthGoalHeadline.Current.Name
        healthGoalProgress = $healthGoalProgress.Current.Name
        selectedPlanTakeaway = $selectedPlanTakeaway.Current.Name
        selectedPlanButton = $selectedPlanButton.Current.Name
        nextActionLabel = $nextActionLabel
        cDrivePageOpened = $true
        fixtureStillExists = (Test-Path -LiteralPath $fixtureTemp -PathType Container)
        noOperationExecuted = $true
        screenshot = $screenshotPath
        drivePlanScreenshot = $drivePlanScreenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_CDRIVE_SCAN_ROOT = $previousScanRoot
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $scanRoot -Recurse -Force -ErrorAction SilentlyContinue
}
