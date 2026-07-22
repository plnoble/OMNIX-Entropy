$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $repo 'src\Css.App\bin\Debug\net8.0-windows\Css.App.exe'
$reportScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-cards.png'
$agentScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-agent.png'
$actionPlanScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-action-plan.png'
$evidenceReviewScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-evidence-review.png'
$eligibleActionsScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-eligible-actions.png'
$candidatePreviewScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-candidate-preview.png'
$appHandoffScreenshotPath = Join-Path $PSScriptRoot 'qa-install-diff-app-handoff.png'
$isolatedDataRoot = Join-Path $PSScriptRoot 'qa-install-diff-data'
$softwareFixturePath = Join-Path $PSScriptRoot 'qa-install-diff-software-fixture.json'
$previousDataRoot = $env:OMNIX_ENTROPY_DATA_ROOT
$previousSoftwareFixture = $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE

. (Join-Path $PSScriptRoot 'wpf-smoke-helpers.ps1')
Initialize-WpfSmokeAutomation

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Css.App.exe not found. Build the solution first: $exe"
}

function Wait-ForEnabledControl {
    param(
        [System.Windows.Automation.AutomationElement]$Window,
        [string]$AutomationId,
        [int]$TimeoutSeconds = 10
    )

    return Wait-Until -TimeoutSeconds $TimeoutSeconds -Probe {
        $control = Find-ByAutomationId $Window $AutomationId 250
        if ($null -ne $control -and $control.Current.IsEnabled) {
            return $control
        }

        return $null
    }
}

function Get-ListItemCount {
    param([System.Windows.Automation.AutomationElement]$List)

    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)
    return $List.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition).Count
}

function Test-ElementIntersectsViewport {
    param(
        [System.Windows.Automation.AutomationElement]$ScrollViewer,
        [System.Windows.Automation.AutomationElement]$Element
    )

    $viewport = $ScrollViewer.Current.BoundingRectangle
    $elementBounds = $Element.Current.BoundingRectangle
    if ($viewport.Width -le 0 -or $viewport.Height -le 0 -or
        $elementBounds.Width -le 0 -or $elementBounds.Height -le 0) {
        return $false
    }

    $visibleWidth = [Math]::Min($viewport.Right, $elementBounds.Right) -
        [Math]::Max($viewport.Left, $elementBounds.Left)
    $visibleHeight = [Math]::Min($viewport.Bottom, $elementBounds.Bottom) -
        [Math]::Max($viewport.Top, $elementBounds.Top)
    $requiredWidth = [Math]::Min($elementBounds.Width, [Math]::Max(40, $elementBounds.Width * 0.25))
    $requiredHeight = [Math]::Min($elementBounds.Height, [Math]::Max(24, $elementBounds.Height * 0.40))
    return $visibleWidth -ge $requiredWidth -and $visibleHeight -ge $requiredHeight
}

function Scroll-ElementIntoView {
    param(
        [System.Windows.Automation.AutomationElement]$ScrollViewer,
        [System.Windows.Automation.AutomationElement]$Element
    )

    if (Test-ElementIntersectsViewport $ScrollViewer $Element) {
        return
    }

    $pattern = $ScrollViewer.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    if (-not $pattern.Current.VerticallyScrollable) {
        throw "Element is offscreen but the install page is not vertically scrollable: $($Element.Current.AutomationId)"
    }

    foreach ($percent in @(0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)) {
        $pattern.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, [double]$percent)
        Start-Sleep -Milliseconds 180
        if (Test-ElementIntersectsViewport $ScrollViewer $Element) {
            return
        }
    }

    throw "Could not scroll the element into view: $($Element.Current.AutomationId)"
}

function Scroll-InstallPageToBottom {
    param([System.Windows.Automation.AutomationElement]$ScrollViewer)

    $pattern = $ScrollViewer.GetCurrentPattern([System.Windows.Automation.ScrollPattern]::Pattern)
    if ($pattern.Current.VerticallyScrollable) {
        $pattern.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, 100)
        Start-Sleep -Milliseconds 700
    }
}

function Ensure-MaximizedWindow {
    param([System.Windows.Automation.WindowPattern]$WindowPattern)

    $WindowPattern.SetWindowVisualState([System.Windows.Automation.WindowVisualState]::Maximized)
    Start-Sleep -Milliseconds 350
}

$process = $null
try {
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $isolatedDataRoot -Force | Out-Null

    $existingProfile = [ordered]@{
        name = 'Existing Fixture Tool'
        publisher = 'OMNIX Smoke'
        installPath = 'D:\Software\ExistingFixture\Install'
    }
    $newProfile = [ordered]@{
        name = 'New Fixture Tool'
        publisher = 'OMNIX Smoke'
        installPath = 'D:\Software\NewFixture\Install'
        cDriveWritePaths = @('C:\Users\Fixture\AppData\Local\NewFixture\Cache')
        cachePaths = @('C:\Users\Fixture\AppData\Local\NewFixture\Cache')
        cacheSizeBytes = 1024
        startupEntries = @('New Fixture Tool')
        services = @('NewFixtureToolSvc')
        scheduledTasks = @('New Fixture Tool Updater')
    }
    $fixture = [ordered]@{
        scans = @(
            @($existingProfile),
            @($existingProfile, $newProfile)
        )
    }
    $fixture | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $softwareFixturePath -Encoding UTF8

    $env:OMNIX_ENTROPY_DATA_ROOT = $isolatedDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $softwareFixturePath

    $process = Start-Process -FilePath $exe -PassThru
    if (-not $process.WaitForInputIdle(10000)) {
        throw 'The WPF app did not become input-ready before GUI automation started.'
    }
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $pidCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
        $process.Id)
    $window = Wait-Until -TimeoutSeconds 12 -Probe {
        $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $pidCondition)
    }
    if ($null -eq $window) {
        throw 'Main window was not found.'
    }

    Show-WpfWindowForSmoke $window
    $windowPattern = $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
    Ensure-MaximizedWindow $windowPattern

    $installNav = Find-ByAutomationId $window 'InstallNavButton'
    if ($null -eq $installNav) { throw 'InstallNavButton was not found.' }
    Invoke-Element $installNav
    Start-Sleep -Milliseconds 300

    $scrollViewer = Find-ByAutomationId $window 'InstallPageScrollViewer'
    if ($null -eq $scrollViewer) { throw 'InstallPageScrollViewer was not found.' }

    $manualComparison = Find-ByAutomationId $window 'InstallManualComparisonExpander'
    if ($null -eq $manualComparison) { throw 'InstallManualComparisonExpander was not found.' }
    $manualComparisonPattern = $manualComparison.GetCurrentPattern(
        [System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $manualComparisonCollapsedByDefault =
        $manualComparisonPattern.Current.ExpandCollapseState -eq
        [System.Windows.Automation.ExpandCollapseState]::Collapsed
    if (-not $manualComparisonCollapsedByDefault) {
        throw 'Manual install comparison must be collapsed for the beginner workflow.'
    }
    $manualComparisonPattern.Expand()
    Start-Sleep -Milliseconds 200

    $captureBefore = Wait-ForEnabledControl $window 'CaptureBeforeInstallButton'
    if ($null -eq $captureBefore) { throw 'CaptureBeforeInstallButton was not enabled.' }
    Invoke-Element $captureBefore
    Start-Sleep -Milliseconds 500
    if ($null -eq (Wait-ForEnabledControl $window 'CaptureBeforeInstallButton')) {
        throw 'The before-install fixture snapshot did not finish.'
    }

    $captureAfter = Wait-ForEnabledControl $window 'CaptureAfterInstallButton'
    if ($null -eq $captureAfter) { throw 'CaptureAfterInstallButton was not enabled.' }
    Invoke-Element $captureAfter
    Start-Sleep -Milliseconds 500
    if ($null -eq (Wait-ForEnabledControl $window 'CaptureAfterInstallButton')) {
        throw 'The after-install fixture snapshot did not finish.'
    }

    $buildReport = Wait-ForEnabledControl $window 'BuildInstallDiffButton'
    if ($null -eq $buildReport) { throw 'BuildInstallDiffButton was not enabled.' }
    Invoke-Element $buildReport

    $cards = Wait-Until -TimeoutSeconds 10 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffCardsListBox' 250
        if ($null -ne $candidate -and (Get-ListItemCount $candidate) -ge 4) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $cards) { throw 'Install change cards were not created from the fixture scans.' }
    $reportCardCount = Get-ListItemCount $cards
    Scroll-ElementIntoView $scrollViewer $cards

    $explainButton = Wait-ForEnabledControl $window 'InstallDiffAgentExplainButton'
    if ($null -eq $explainButton) { throw 'InstallDiffAgentExplainButton was not enabled.' }
    Scroll-ElementIntoView $scrollViewer $explainButton
    Ensure-MaximizedWindow $windowPattern
    Save-DesktopScreenshot $reportScreenshotPath
    Invoke-Element $explainButton
    Scroll-InstallPageToBottom $scrollViewer

    $headline = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffAgentHeadlineTextBlock' 250
        if ($null -ne $candidate -and -not [string]::IsNullOrWhiteSpace($candidate.Current.Name)) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $headline) { throw 'The Agent explanation headline was not created.' }

    $steps = Find-ByAutomationId $window 'InstallDiffAgentStepsListBox'
    if ($null -eq $steps) { throw 'InstallDiffAgentStepsListBox was not found.' }
    Scroll-ElementIntoView $scrollViewer $steps
    $agentStepCount = Get-ListItemCount $steps
    if ($agentStepCount -lt 3) { throw "Expected at least 3 Agent steps, found $agentStepCount." }

    $technicalDetails = Find-ByAutomationId $window 'InstallDiffTechnicalDetailsExpander'
    if ($null -eq $technicalDetails) { throw 'InstallDiffTechnicalDetailsExpander was not found.' }
    $expandPattern = $technicalDetails.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $technicalDetailsCollapsed =
        $expandPattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed
    if (-not $technicalDetailsCollapsed) { throw 'Technical details should stay collapsed during the beginner flow.' }

    Ensure-MaximizedWindow $windowPattern
    Save-DesktopScreenshot $agentScreenshotPath

    $generatePlanButton = Wait-ForEnabledControl $window 'InstallDiffGeneratePlanButton'
    if ($null -eq $generatePlanButton) { throw 'InstallDiffGeneratePlanButton was not enabled.' }
    Scroll-ElementIntoView $scrollViewer $generatePlanButton
    Invoke-Element $generatePlanButton

    $actionPlanSummary = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffActionPlanSummaryTextBlock' 250
        if ($null -ne $candidate -and -not [string]::IsNullOrWhiteSpace($candidate.Current.Name)) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $actionPlanSummary) { throw 'The install action plan summary was not created.' }

    $classificationSummary = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffActionPlanReviewSummaryTextBlock' 250
        if ($null -ne $candidate -and $candidate.Current.Name.Contains('Agent')) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $classificationSummary) { throw 'The evidence classification summary was not created.' }
    $classificationSummaryVisible = -not $classificationSummary.Current.IsOffscreen

    $actionPlanList = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffActionPlanListBox' 250
        if ($null -ne $candidate -and (Get-ListItemCount $candidate) -ge 3) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $actionPlanList) { throw 'The install action plan items were not created.' }
    $actionPlanItemCount = Get-ListItemCount $actionPlanList

    $actionPlanSafety = Find-ByAutomationId $window 'InstallDiffActionPlanSafetyTextBlock'
    if ($null -eq $actionPlanSafety) { throw 'InstallDiffActionPlanSafetyTextBlock was not found.' }
    $nothingExecutedText = -join @(
        [char]0x5C1A,
        [char]0x672A,
        [char]0x6267,
        [char]0x884C)
    $nothingExecutedVisible = $actionPlanSafety.Current.Name.Contains($nothingExecutedText)
    if (-not $nothingExecutedVisible) {
        throw "The action plan did not clearly say that nothing was executed. UIA name: '$($actionPlanSafety.Current.Name)'"
    }

    Scroll-InstallPageToBottom $scrollViewer
    $technicalDetailsCollapsed =
        $expandPattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed
    if (-not $technicalDetailsCollapsed) { throw 'Technical details expanded while generating the action plan.' }

    Ensure-MaximizedWindow $windowPattern
    Save-DesktopScreenshot $actionPlanScreenshotPath

    $evidenceReview = Find-ByAutomationId $window 'InstallDiffEvidenceReviewExpander'
    if ($null -eq $evidenceReview) { throw 'InstallDiffEvidenceReviewExpander was not found.' }
    $evidenceReviewPattern = $evidenceReview.GetCurrentPattern(
        [System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $evidenceReviewCollapsedByDefault =
        $evidenceReviewPattern.Current.ExpandCollapseState -eq
        [System.Windows.Automation.ExpandCollapseState]::Collapsed
    if (-not $evidenceReviewCollapsedByDefault) {
        throw 'The beginner evidence review should stay collapsed until the user opens it.'
    }

    $evidenceReviewPattern.Expand()
    Start-Sleep -Milliseconds 500

    $cDriveEvidenceList = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffCDriveEvidenceReviewListBox' 250
        if ($null -ne $candidate -and (Get-ListItemCount $candidate) -ge 1) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $cDriveEvidenceList) { throw 'The C-drive evidence review was not created.' }
    $evidenceReviewCDriveItemCount = Get-ListItemCount $cDriveEvidenceList

    $backgroundEvidenceList = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffBackgroundEvidenceReviewListBox' 250
        if ($null -ne $candidate -and (Get-ListItemCount $candidate) -ge 3) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $backgroundEvidenceList) { throw 'The background evidence review was not created.' }
    $evidenceReviewBackgroundItemCount = Get-ListItemCount $backgroundEvidenceList

    $eligibleActionsList = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffEligibleActionsListBox' 250
        if ($null -ne $candidate -and (Get-ListItemCount $candidate) -ge 3) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $eligibleActionsList) { throw 'The evidence-driven eligible actions were not created.' }
    $eligibleActionItemCount = Get-ListItemCount $eligibleActionsList
    $buttonCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $candidatePreviewButtons = $eligibleActionsList.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        $buttonCondition)
    $candidatePreviewButtonCount = $candidatePreviewButtons.Count
    $unexpectedCandidateButtonCount = @($candidatePreviewButtons | Where-Object {
        -not $_.Current.AutomationId.StartsWith(
            'InstallDiffCandidatePreviewButton_',
            [System.StringComparison]::Ordinal)
    }).Count
    $eligibleActionsPlanOnly =
        $candidatePreviewButtonCount -ge 1 -and $unexpectedCandidateButtonCount -eq 0
    if (-not $eligibleActionsPlanOnly) {
        throw 'Eligible actions may expose preview buttons only, not direct action buttons.'
    }

    $evidenceReviewSafety = Find-ByAutomationId $window 'InstallDiffEvidenceReviewSafetyTextBlock'
    if ($null -eq $evidenceReviewSafety) { throw 'InstallDiffEvidenceReviewSafetyTextBlock was not found.' }

    $allEvidenceNames = @()
    $allEvidenceNames += $cDriveEvidenceList.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
    $allEvidenceNames += $backgroundEvidenceList.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
    $allEvidenceNames += $eligibleActionsList.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
    $rawIdentifiers = @(
        'C:\Users\Fixture\AppData\Local\NewFixture\Cache',
        'New Fixture Tool',
        'NewFixtureToolSvc',
        'New Fixture Tool Updater')
    $evidenceReviewHidesRawIdentifiers = $true
    foreach ($identifier in $rawIdentifiers) {
        foreach ($name in $allEvidenceNames) {
            if (-not [string]::IsNullOrWhiteSpace($name) -and
                $name.IndexOf($identifier, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $evidenceReviewHidesRawIdentifiers = $false
                break
            }
        }

        if (-not $evidenceReviewHidesRawIdentifiers) {
            break
        }
    }
    if (-not $evidenceReviewHidesRawIdentifiers) {
        throw 'The beginner evidence review exposed a raw path or background identifier.'
    }

    $technicalDetailsCollapsed =
        $expandPattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed
    if (-not $technicalDetailsCollapsed) {
        throw 'Technical details expanded while opening the beginner evidence review.'
    }

    Scroll-ElementIntoView $scrollViewer $cDriveEvidenceList
    Ensure-MaximizedWindow $windowPattern
    Save-DesktopScreenshot $evidenceReviewScreenshotPath

    Scroll-ElementIntoView $scrollViewer $eligibleActionsList
    Ensure-MaximizedWindow $windowPattern
    Save-DesktopScreenshot $eligibleActionsScreenshotPath

    $candidatePreviewButton = Find-ByAutomationId $window 'InstallDiffCandidatePreviewButton_CacheCleanupPlan'
    if ($null -eq $candidatePreviewButton) {
        throw 'The cache candidate preview button was not found.'
    }
    Scroll-ElementIntoView $scrollViewer $candidatePreviewButton
    Invoke-Element $candidatePreviewButton

    $candidatePreviewTitle = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffCandidatePreviewTitleTextBlock' 250
        if ($null -ne $candidate -and -not [string]::IsNullOrWhiteSpace($candidate.Current.Name)) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $candidatePreviewTitle) { throw 'The candidate preview title was not created.' }

    $candidatePreviewStatus = Find-ByAutomationId $window 'InstallDiffCandidatePreviewStatusTextBlock'
    if ($null -eq $candidatePreviewStatus) { throw 'The candidate preview status was not found.' }
    $candidatePreviewLines = Find-ByAutomationId $window 'InstallDiffCandidatePreviewLinesListBox'
    if ($null -eq $candidatePreviewLines) { throw 'The candidate preview lines were not found.' }
    $candidatePreviewLineCount = Get-ListItemCount $candidatePreviewLines
    $candidatePreviewMissingEvidence = Find-ByAutomationId $window 'InstallDiffCandidatePreviewMissingEvidenceListBox'
    if ($null -eq $candidatePreviewMissingEvidence) { throw 'The missing-evidence list was not found.' }
    $candidatePreviewMissingEvidenceCount = Get-ListItemCount $candidatePreviewMissingEvidence
    $candidatePreviewSafety = Find-ByAutomationId $window 'InstallDiffCandidatePreviewSafetyTextBlock'
    if ($null -eq $candidatePreviewSafety) { throw 'The candidate preview safety text was not found.' }

    $candidatePreviewReady =
        -not [string]::IsNullOrWhiteSpace($candidatePreviewStatus.Current.Name) -and
        $candidatePreviewLineCount -ge 3 -and
        $candidatePreviewMissingEvidenceCount -ge 1
    if (-not $candidatePreviewReady) {
        throw 'The cache candidate did not render a complete read-only preview.'
    }
    $candidatePreviewNoExecution = $candidatePreviewSafety.Current.Name.Contains($nothingExecutedText)
    if (-not $candidatePreviewNoExecution) {
        throw 'The candidate preview did not clearly say that nothing was executed.'
    }

    $allEvidenceNames += $candidatePreviewLines.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
    $allEvidenceNames += $candidatePreviewMissingEvidence.FindAll(
        [System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }
    $allEvidenceNames += @(
        $candidatePreviewTitle.Current.Name,
        $candidatePreviewStatus.Current.Name,
        $candidatePreviewSafety.Current.Name)
    foreach ($identifier in $rawIdentifiers) {
        foreach ($name in $allEvidenceNames) {
            if (-not [string]::IsNullOrWhiteSpace($name) -and
                $name.IndexOf($identifier, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                throw 'The candidate preview exposed a raw path or background identifier.'
            }
        }
    }

    $technicalDetailsCollapsed =
        $expandPattern.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed
    if (-not $technicalDetailsCollapsed) {
        throw 'Technical details expanded while opening a candidate preview.'
    }

    $openCandidateApp = Wait-Until -TimeoutSeconds 8 -Probe {
        $candidate = Find-ByAutomationId $window 'InstallDiffCandidateOpenAppButton' 250
        if ($null -ne $candidate -and $candidate.Current.IsEnabled) {
            return $candidate
        }

        return $null
    }
    if ($null -eq $openCandidateApp) {
        throw 'The ready candidate did not expose its exact-application next step.'
    }
    Scroll-ElementIntoView $scrollViewer $candidatePreviewTitle
    if (-not (Test-ElementIntersectsViewport $scrollViewer $candidatePreviewTitle) -or
        -not (Test-ElementIntersectsViewport $scrollViewer $openCandidateApp)) {
        throw 'The candidate conclusion and exact-application next step were not visible together.'
    }
    Ensure-MaximizedWindow $windowPattern
    Save-WindowScreenshot $window $candidatePreviewScreenshotPath

    Invoke-Element $openCandidateApp

    $drawerTitle = Wait-Until -TimeoutSeconds 12 -Probe {
        $candidate = Find-ByAutomationId $window 'DrawerTitleTextBlock' 250
        if ($null -ne $candidate -and
            -not $candidate.Current.IsOffscreen -and
            $candidate.Current.Name -eq 'New Fixture Tool') {
            return $candidate
        }

        return $null
    }
    if ($null -eq $drawerTitle) {
        throw 'The install-report handoff did not open the exact new application drawer.'
    }
    $drawerCacheButton = Find-ByAutomationId $window 'DrawerCleanCacheButton'
    if ($null -eq $drawerCacheButton -or -not $drawerCacheButton.Current.IsEnabled) {
        throw 'The exact application drawer did not expose its normal cache review entry.'
    }
    Save-WindowScreenshot $window $appHandoffScreenshotPath

    [PSCustomObject]@{
        fixtureOnly = $true
        manualComparisonCollapsedByDefault = $manualComparisonCollapsedByDefault
        reportCardCount = $reportCardCount
        agentHeadlineVisible = (-not $headline.Current.IsOffscreen)
        agentStepCount = $agentStepCount
        actionPlanItemCount = $actionPlanItemCount
        nothingExecutedVisible = $nothingExecutedVisible
        classificationSummaryVisible = $classificationSummaryVisible
        evidenceReviewCollapsedByDefault = $evidenceReviewCollapsedByDefault
        evidenceReviewCDriveItemCount = $evidenceReviewCDriveItemCount
        evidenceReviewBackgroundItemCount = $evidenceReviewBackgroundItemCount
        eligibleActionItemCount = $eligibleActionItemCount
        eligibleActionsPlanOnly = $eligibleActionsPlanOnly
        candidatePreviewButtonCount = $candidatePreviewButtonCount
        candidatePreviewLineCount = $candidatePreviewLineCount
        candidatePreviewReady = $candidatePreviewReady
        candidatePreviewNoExecution = $candidatePreviewNoExecution
        exactAppHandoffReached = $true
        drawerCacheReviewAvailable = $true
        noOperationExecuted = $true
        evidenceReviewHidesRawIdentifiers = $evidenceReviewHidesRawIdentifiers
        technicalDetailsCollapsed = $true
        reportScreenshot = $reportScreenshotPath
        agentScreenshot = $agentScreenshotPath
        actionPlanScreenshot = $actionPlanScreenshotPath
        evidenceReviewScreenshot = $evidenceReviewScreenshotPath
        eligibleActionsScreenshot = $eligibleActionsScreenshotPath
        candidatePreviewScreenshot = $candidatePreviewScreenshotPath
        appHandoffScreenshot = $appHandoffScreenshotPath
    } | ConvertTo-Json -Compress
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }

    $env:OMNIX_ENTROPY_DATA_ROOT = $previousDataRoot
    $env:OMNIX_ENTROPY_SOFTWARE_FIXTURE = $previousSoftwareFixture
    Remove-Item -LiteralPath $isolatedDataRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $softwareFixturePath -Force -ErrorAction SilentlyContinue
}
