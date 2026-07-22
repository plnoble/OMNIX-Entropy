using System;
using System.IO;
using System.Linq;
using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Recovery;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public class ProductExperienceTests
{
    private const string VerifiedSnapshotHash = "A1B2C3D4E5F60718293A4B5C6D7E8F90";
    private static readonly DateTimeOffset VerifiedSnapshotNow =
        new(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Health_summary_turns_scan_results_into_plain_language_findings()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 31L * 1024 * 1024 * 1024,
            TopLevel =
            [
                new() { Name = "$Recycle.Bin", Path = @"C:\$Recycle.Bin", SizeBytes = 7L * 1024 * 1024 * 1024, Category = UsageCategory.Temp },
                new() { Name = "Users", Path = @"C:\Users", SizeBytes = 42L * 1024 * 1024 * 1024, Category = UsageCategory.UserProfiles }
            ]
        };
        var recommendations = new[]
        {
            new Recommendation
            {
                Title = "Recycle bin backlog",
                Finding = "Recycle bin uses 7.0 GB",
                Reason = "Can be reviewed before cleanup",
                Action = RecommendationAction.Clean,
                Risk = RiskLevel.Low,
                Reversibility = ReversibilityLevel.Reversible,
                EstimatedImpactBytes = 7L * 1024 * 1024 * 1024,
                Evidence = ["Recycle bin is a low-risk cleanup candidate"]
            }
        };

        var summary = HealthCheckSummaryBuilder.Build(result, recommendations);

        summary.OverallScore.Should().BeInRange(0, 100);
        summary.Dimensions.Should().NotBeEmpty();
        summary.KeyFindings.Should().NotBeEmpty();
        summary.KeyFindings.Select(f => f.Text).Should().NotContain(text => text.Contains(@"C:\"));
    }

    [Fact]
    public void Sustained_software_growth_becomes_a_path_free_home_agent_finding()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 40L * 1024 * 1024 * 1024,
            TopLevel = []
        };
        var growth = new GrowthFinding
        {
            Path = @"C:\Users\Fixture\AppData\Local\Docker",
            OwnerSoftware = "Docker Desktop",
            PreviousBytes = 2L * 1024 * 1024 * 1024,
            CurrentBytes = 5L * 1024 * 1024 * 1024,
            SourceKind = GrowthSourceKind.Software,
            ObservedSnapshots = 4,
            PositiveGrowthIntervals = 3,
            IsSustainedGrowth = true,
            TrendGrowthBytes = 4L * 1024 * 1024 * 1024,
            Reason = "Grew repeatedly."
        };

        var summary = HealthCheckSummaryBuilder.Build(result, [], [growth]);
        var finding = summary.KeyFindings.Single();
        var explanation = HealthFindingAgentExplanationBuilder.Create(finding);
        var plan = HealthFindingActionPlanBuilder.Create(finding);

        finding.Kind.Should().Be(HealthFindingKind.SustainedGrowth);
        finding.TargetAppName.Should().Be("Docker Desktop");
        finding.Text.Should().Contain("Docker Desktop").And.Contain("累计增加 4.0 GB");
        finding.Text.Should().NotContain(@"C:\");
        explanation.WhyItMatters.Should().Contain("多次体检");
        explanation.RecommendedNextStep.Should().Contain("缓存").And.Contain("以后");
        plan.Steps.Should().Contain(step => step.Contains("防止继续增长"));
        plan.CanExecuteDirectly.Should().BeFalse();
        plan.VisibleText.Should().NotContain(@"C:\");
    }

    [Fact]
    public void App_drawer_target_resolution_requires_one_exact_current_profile()
    {
        var docker = new SoftwareProfile { Name = "Docker Desktop" };
        var unique = AppDrawerTargetResolver.Resolve(
            "  docker desktop  ",
            [docker, new SoftwareProfile { Name = "Other App" }]);
        var missing = AppDrawerTargetResolver.Resolve("Missing App", [docker]);
        var ambiguous = AppDrawerTargetResolver.Resolve(
            "Docker Desktop",
            [docker, new SoftwareProfile { Name = "docker desktop" }]);
        var noTarget = AppDrawerTargetResolver.Resolve(null, [docker]);
        var unavailable = AppDrawerTargetResolver.InventoryUnavailable();

        unique.Status.Should().Be(AppDrawerTargetStatus.Found);
        unique.Profile.Should().BeSameAs(docker);
        unique.CanOpen.Should().BeTrue();
        new[] { missing, ambiguous, noTarget, unavailable }
            .Should().OnlyContain(result => !result.CanOpen && result.Profile == null);
        ambiguous.Status.Should().Be(AppDrawerTargetStatus.Ambiguous);
        ambiguous.Explanation.Should().Contain("不会替你猜");
        string.Join("\n", missing.Explanation, ambiguous.Explanation, unavailable.Explanation)
            .Should().NotContain(@"C:\");
        new[] { unique, missing, ambiguous, noTarget, unavailable }
            .Should().OnlyContain(result => result.SafetyBoundary.Contains("不会卸载"));
    }

    [Fact]
    public void Shared_growth_never_gets_a_direct_app_target()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 100,
            FreeBytes = 50,
            TopLevel = []
        };
        var shared = new GrowthFinding
        {
            Path = @"C:\Fixture\Shared",
            OwnerSoftware = ScanSnapshotBuilder.SharedSoftwareOwner,
            PreviousBytes = 10,
            CurrentBytes = 40,
            SourceKind = GrowthSourceKind.SharedSoftware,
            ObservedSnapshots = 3,
            PositiveGrowthIntervals = 2,
            IsSustainedGrowth = true,
            TrendGrowthBytes = 30,
            Reason = "Grew repeatedly."
        };

        var finding = HealthCheckSummaryBuilder.Build(result, [], [shared])
            .KeyFindings.Single();

        finding.Kind.Should().Be(HealthFindingKind.SustainedGrowth);
        finding.TargetAppName.Should().BeNull();
    }

    [Fact]
    public void Home_agent_conclusion_has_stable_automation_ids_before_findings()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var ids = new[]
        {
            "HealthDimensionListView",
            "HomeAgentResponseTitleTextBlock",
            "HomeAgentResponseBodyTextBlock",
            "HomeAgentResponseSafetyTextBlock",
            "HomeAgentResponseNavigateButton",
            "KeyFindingsListBox"
        };

        foreach (var id in ids)
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
        xaml.IndexOf("HomeAgentResponseTitleTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("KeyFindingsListBox", StringComparison.Ordinal));
        xaml.IndexOf("HomeAgentResponseNavigateButton", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("KeyFindingsListBox", StringComparison.Ordinal));
        xaml.Should().Contain("Click=\"HomeAgentResponseNavigate_Click\"");
        xaml.Should().Contain("StringFormat=HealthDimension_{0}");
        var findingsStart = xaml.IndexOf("<ListBox x:Name=\"KeyFindingsListBox\"", StringComparison.Ordinal);
        findingsStart.Should().BeGreaterThanOrEqualTo(0);
        var findingsOpeningTagEnd = xaml.IndexOf('>', findingsStart);
        findingsOpeningTagEnd.Should().BeGreaterThan(findingsStart);
        xaml[findingsStart..(findingsOpeningTagEnd + 1)]
            .Should().Contain("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"")
            .And.Contain("HorizontalContentAlignment=\"Stretch\"");
        code.Should().Contain("HomeAgentResponseNavigateButton.Tag = response;");
        code.Should().Contain("HomeAgentResponseNavigateButton.Visibility = response.CanNavigate");
        code.Should().Contain("session.GrowthFindings");
        code.Should().Contain("KeyFindingsListBox.ItemsSource = summary.KeyFindings");
    }

    [Fact]
    public void Home_agent_next_action_uses_only_internal_pages_and_re_resolves_app_targets()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(
            code,
            "private async void HomeAgentResponseNavigate_Click",
            "private void CreateHealthFindingPlan_Click");

        handler.Should().Contain("response.NavigationDestination");
        handler.Should().Contain("ResolveAndOpenAppTargetAsync(response.TargetAppName)");
        handler.Should().Contain("HomeAgentResponsePresenter.AppTargetUnavailable(resolution)");
        handler.Should().Contain("IsAgentNavigationTarget(targetPage)");
        handler.Should().Contain("ShowPage(targetPage)");
        handler.Should().NotContain("Process.Start")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor");
    }

    [Fact]
    public void Home_agent_next_action_has_an_isolated_repeatable_gui_smoke()
    {
        var script = File.ReadAllText(
            FindRepositoryFile(".omx", "gui-home-agent-next-action-smoke.ps1"));
        var doc = File.ReadAllText(
            FindRepositoryFile("docs", "development", "gui-smokes.md"));

        script.Should().Contain("$ErrorActionPreference = 'Stop'");
        script.Should().Contain("OMNIX_ENTROPY_DATA_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_CDRIVE_SCAN_ROOT");
        script.Should().Contain("Join-Path 'C:\\tmp'");
        script.Should().Contain("HealthDimensionListView");
        script.Should().Contain("'HealthDimension_' + $dimension.Name");
        script.Should().Contain("healthDimensionCount");
        script.Should().Contain("machineHealthRows");
        script.Should().Contain("lastHealthDimensionCount -ge 7");
        script.Should().Contain("HomeAgentResponseTitleTextBlock");
        script.Should().Contain("HomeAgentResponseBodyTextBlock");
        script.Should().Contain("HomeAgentResponseSafetyTextBlock");
        script.Should().Contain("HomeAgentResponseNavigateButton");
        script.Should().Contain("RecommendationsListBox");
        script.Should().Contain("Save-WindowScreenshot");
        script.Should().NotContain("Save-DesktopScreenshot")
            .And.NotContain("CleanupConfirmationConfirmButton")
            .And.NotContain("SafetyOperationPipeline");
        script.Should().Contain("noOperationExecuted = $true");
        script.Should().Contain("qa-home-agent-next-action.png");
        script.Should().Contain("finally");
        doc.Should().Contain("gui-home-agent-next-action-smoke.ps1");
        doc.Should().Contain("4 KB");
        doc.Should().Contain("不执行");
    }

    [Fact]
    public void Growth_and_home_findings_navigate_only_through_unique_app_target_helper()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var resolverMethod = ExtractMethod(
            code,
            "private async Task<AppDrawerTargetResolution> ResolveAndOpenAppTargetAsync",
            "private void OpenAppDrawerTarget");
        var openMethod = ExtractMethod(
            code,
            "private void OpenAppDrawerTarget",
            "private Task EnsureSoftwareInventoryLoadedAsync");

        xaml.Should().Contain("AutomationProperties.AutomationId=\"OpenGrowthAppButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerAdviceTextBlock\"");
        xaml.Should().Contain("Click=\"OpenGrowthApp_Click\"");
        xaml.IndexOf("OpenGrowthAppButton", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("GrowthDecisionSafetyTextBlock", StringComparison.Ordinal));
        code.Should().Contain("finding.TargetAppName");
        code.Should().Contain("ShowGrowthAppTargetFailure(resolution)");
        code.Should().Contain("private IReadOnlyList<GrowthFinding> _latestGrowthFindings = [];");
        code.Should().Contain("_latestGrowthFindings = session.GrowthFindings;");
        code.Should().Contain("private void SetSoftwareProfiles(");
        code.Should().Contain("SoftwareGrowthProfileEnricher.Apply");
        code.IndexOf("SoftwareGrowthProfileEnricher.Apply", StringComparison.Ordinal)
            .Should().BeLessThan(code.IndexOf("HealthCheckSummaryBuilder.Build", StringComparison.Ordinal));
        code.Split("_softwareProfiles =", StringSplitOptions.None)
            .Should().HaveCount(3, "only the field initializer and the centralized setter may assign application profiles");
        resolverMethod.Should().Contain("AppDrawerTargetResolver.Resolve");
        resolverMethod.Should().Contain("resolution.Status == AppDrawerTargetStatus.NotFound");
        resolverMethod.Should().Contain("await RefreshSoftwareInventoryAsync()");
        resolverMethod.Should().Contain("if (!resolution.CanOpen || resolution.Profile is null)");
        resolverMethod.Should().NotContain("Process.Start").And.NotContain("SafetyOperationPipeline");
        openMethod.Should().Contain("ShowPage(\"Apps\")");
        openMethod.Should().Contain("_appCatalogFilter = AppCatalogFilter.All");
        openMethod.Should().Contain("RefreshAppCatalog(profile)");
        openMethod.Should().Contain("DrawerTitleTextBlock.BringIntoView()");
        openMethod.Should().NotContain("Process.Start").And.NotContain("Execute");
    }

    [Fact]
    public void Agent_explanation_for_health_finding_is_plain_and_non_executable()
    {
        var finding = new HealthFinding
        {
            Text = "\u56de\u6536\u7ad9\u79ef\u538b 7.0 GB\uff0c\u5efa\u8bae\u786e\u8ba4\u540e\u6e05\u7406",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low
        };

        var explanation = HealthFindingAgentExplanationBuilder.Create(finding);

        explanation.Title.Should().Be("Computer Agent \u89e3\u91ca");
        explanation.WhatThisMeans.Should().Contain("\u53d1\u73b0");
        explanation.RecommendedNextStep.Should().Contain("\u9694\u79bb\u533a");
        explanation.SafetyBoundary.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u6267\u884c");
        explanation.CanExecuteDirectly.Should().BeFalse();
        explanation.NextSteps.Should().Contain(step => step.Contains("\u751f\u6210\u5904\u7406\u65b9\u6848"));
        explanation.NextSteps.Should().NotContain(step => step.Contains(@"C:\"));
    }

    [Fact]
    public void Health_finding_detail_and_plan_buttons_are_read_only_and_safe()
    {
        var finding = new HealthFinding
        {
            Text = @"C:\Temp \u5360\u7528 2.0 GB\uff0c\u5efa\u8bae\u786e\u8ba4\u540e\u6e05\u7406",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low
        };

        var detail = HealthFindingDetailPresentationBuilder.Create(finding);
        var plan = HealthFindingActionPlanBuilder.Create(finding);

        detail.Title.Should().Contain("\u67e5\u770b\u8be6\u60c5");
        detail.Summary.Should().Contain("C \u76d8\u6e05\u7406");
        detail.SafetyBoundary.Should().Contain("\u4e0d\u4f1a\u5904\u7406");
        detail.VisibleText.Should().NotContain(@"C:\");
        plan.Title.Should().Contain("\u5904\u7406\u65b9\u6848");
        plan.Steps.Should().Contain(step => step.Contains("\u786e\u8ba4"));
        plan.SafetyBoundary.Should().Contain("\u672c\u5730\u5b89\u5168\u7ba1\u7ebf");
        plan.CanExecuteDirectly.Should().BeFalse();
        plan.VisibleText.Should().NotContain(@"C:\");
    }

    [Fact]
    public void Home_agent_response_presents_explain_detail_and_plan_without_modal_execution()
    {
        var finding = new HealthFinding
        {
            Text = "\u975e\u9884\u671f\u6839\u76ee\u5f55: AMD 1.9 GB\uff0c\u5efa\u8bae\u5148\u89c2\u5bdf",
            Action = RecommendationAction.Observe,
            Risk = RiskLevel.Medium
        };

        var explanation = HomeAgentResponsePresenter.Explain(finding);
        var detail = HomeAgentResponsePresenter.ShowDetails(finding);
        var plan = HomeAgentResponsePresenter.CreatePlan(finding);

        explanation.Title.Should().Contain("Agent");
        explanation.Body.Should().Contain("\u53d1\u73b0");
        detail.Title.Should().Contain("\u8be6\u60c5");
        detail.Body.Should().Contain("C \u76d8");
        plan.Title.Should().Contain("\u65b9\u6848");
        plan.Body.Should().Contain("\u786e\u8ba4");
        new[] { explanation, detail, plan }.Should().OnlyContain(response => !response.CanExecuteDirectly);
        new[] { explanation, detail, plan }.Should().OnlyContain(response => response.CanNavigate);
        new[] { explanation, detail, plan }.Select(response => response.NavigationDestination)
            .Should().OnlyContain(destination => destination == HomeAgentNavigationDestination.CDrive);
        new[] { explanation, detail, plan }.Select(response => response.NavigationLabel)
            .Should().OnlyContain(label => !string.IsNullOrWhiteSpace(label));
        new[] { explanation, detail, plan }.Select(response => response.SafetyBoundary)
            .Should().OnlyContain(text => text.Contains("\u4e0d\u4f1a\u76f4\u63a5\u6267\u884c") || text.Contains("\u672c\u5730\u5b89\u5168\u7ba1\u7ebf"));
    }

    [Fact]
    public void Home_agent_plan_targets_exact_app_but_unavailable_target_falls_back_without_guessing()
    {
        var finding = new HealthFinding
        {
            Text = "Docker Desktop 近期多次变大",
            Kind = HealthFindingKind.SustainedGrowth,
            TargetAppName = " Docker Desktop ",
            Action = RecommendationAction.Observe,
            Risk = RiskLevel.Medium
        };

        var plan = HomeAgentResponsePresenter.CreatePlan(finding);
        var unavailable = HomeAgentResponsePresenter.AppTargetUnavailable(
            AppDrawerTargetResolver.Resolve(
                "Missing App",
                [new SoftwareProfile { Name = "Docker Desktop" }]));

        plan.NavigationDestination.Should().Be(HomeAgentNavigationDestination.Applications);
        plan.NavigationLabel.Should().Be("打开对应应用");
        plan.TargetAppName.Should().Be("Docker Desktop");
        plan.CanNavigate.Should().BeTrue();
        plan.CanExecuteDirectly.Should().BeFalse();
        unavailable.NavigationDestination.Should().Be(HomeAgentNavigationDestination.Applications);
        unavailable.NavigationLabel.Should().Be("打开应用管理");
        unavailable.TargetAppName.Should().BeNull();
        unavailable.CanNavigate.Should().BeTrue();
        unavailable.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void C_drive_recommendation_card_explains_happened_agent_advice_undo_and_impact()
    {
        var recommendation = new Recommendation
        {
            Title = "\u53ef\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Finding = @"C:\temp \u5360\u7528 2.0 GB",
            Reason = "\u4e34\u65f6\u76ee\u5f55\u901a\u5e38\u53ef\u91cd\u5efa\uff1bV1 \u53ea\u5efa\u8bae\u79fb\u52a8\u5230\u9694\u79bb\u533a\u3002",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = 2L * 1024 * 1024 * 1024,
            Evidence = ["\u5206\u7c7b\u7ed3\u679c: Temp", @"\u8def\u5f84: C:\temp"],
            Operation = new OperationDescriptor
            {
                Kind = "clean.temp",
                Title = "\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
                Risk = RiskLevel.Low,
                IsDestructive = true,
                RollbackRequired = true,
                EstimatedImpactBytes = 2L * 1024 * 1024 * 1024,
                AffectedPaths = [@"C:\temp"]
            }
        };

        var card = RecommendationCardPresenter.Create(recommendation);

        card.Title.Should().Contain("\u53ef\u4ee5\u6e05\u7406");
        card.WhatHappened.Should().StartWith("\u53d1\u751f\u4e86\u4ec0\u4e48\uff1a");
        card.WhatHappened.Should().NotContain(@"C:\temp");
        card.WhatHappened.Should().Contain("\u67d0\u4e2a\u672c\u673a\u4f4d\u7f6e");
        card.AgentSuggestion.Should().StartWith("Agent \u5efa\u8bae\uff1a");
        card.AgentSuggestion.Should().Contain("\u9694\u79bb\u533a");
        card.UndoStatus.Should().StartWith("\u80fd\u4e0d\u80fd\u540e\u6094\uff1a");
        card.UndoStatus.Should().Contain("\u540e\u6094\u836f\u4e2d\u5fc3");
        card.ImpactText.Should().Be("\u9884\u8ba1\u91ca\u653e\uff1a2.0 GB");
        card.SafetyLine.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u6c38\u4e45\u5220\u9664");
        card.CanExecute.Should().BeTrue();
        card.Operation.Should().BeSameAs(recommendation.Operation);
    }

    [Fact]
    public void C_drive_recommendation_card_hides_local_paths_with_spaces_but_preserves_operation_evidence()
    {
        const string localPath = @"C:\Users\Test User\AppData\Local\Cache";
        var operation = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "Clean fixture cache",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EstimatedImpactBytes = 1024,
            AffectedPaths = [localPath]
        };
        var recommendation = new Recommendation
        {
            Title = "\u53ef\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: Cache",
            Finding = localPath + " \u5360\u7528 1 KB",
            Reason = "Fixture",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = 1024,
            Evidence = ["Fixture"],
            Operation = operation
        };

        var card = RecommendationCardPresenter.Create(recommendation);
        var visible = string.Join("\n", [
            card.Title,
            card.WhatHappened,
            card.AgentSuggestion,
            card.UndoStatus,
            card.ImpactText,
            card.SafetyLine]);

        visible.Should().Contain("\u67d0\u4e2a\u672c\u673a\u4f4d\u7f6e")
            .And.NotContain(localPath)
            .And.NotContain(@"C:\Users");
        card.Operation.Should().BeSameAs(operation);
        card.Operation!.AffectedPaths.Should().ContainSingle().Which.Should().Be(localPath);
    }

    [Fact]
    public void C_drive_recommendation_list_groups_repeated_observe_items_and_explains_quarantine()
    {
        var observeRecommendations = new[]
        {
            CreateUnexpectedRootRecommendation("AMD", 1L * 1024 * 1024 * 1024),
            CreateUnexpectedRootRecommendation("tmp", 1L * 1024 * 1024 * 1024),
            CreateUnexpectedRootRecommendation("OneDriveTemp", 1L * 1024 * 1024 * 1024),
            CreateUnexpectedRootRecommendation("db", 1L * 1024 * 1024 * 1024)
        };
        var cleanupOperation = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            AffectedPaths = [@"C:\temp"]
        };
        var cleanupRecommendation = new Recommendation
        {
            Title = "\u53ef\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Finding = @"C:\temp \u5360\u7528 512 MB",
            Reason = "\u4e34\u65f6\u76ee\u5f55\u901a\u5e38\u53ef\u91cd\u5efa\u3002",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            Evidence = ["\u5206\u7c7b\u7ed3\u679c: Temp"],
            Operation = cleanupOperation
        };

        var list = RecommendationListPresenter.Create(observeRecommendations.Append(cleanupRecommendation));

        list.Cards.Should().HaveCount(2);
        var grouped = list.Cards[0];
        grouped.Title.Should().Contain("\u9700\u8981\u786e\u8ba4\u6765\u6e90");
        grouped.Title.Should().Contain("4 \u4e2a");
        grouped.WhatHappened.Should().NotContain(@"C:\");
        grouped.AgentSuggestion.Should().Contain("\u4e0d\u7528\u9010\u4e2a\u70b9");
        grouped.ImpactText.Should().Contain("4.0 GB");
        grouped.CanExecute.Should().BeFalse();
        grouped.Operation.Should().BeNull();
        var cleanupCard = list.Cards[1];
        cleanupCard.CanExecute.Should().BeTrue();
        cleanupCard.Operation.Should().BeSameAs(cleanupOperation);
        list.ActionExplanationText.Should().Contain("\u9694\u79bb\u533a");
        list.ActionExplanationText.Should().Contain("\u4e0d\u662f\u6c38\u4e45\u5220\u9664");
        list.ActionExplanationText.Should().Contain("\u540e\u6094\u836f");
    }

    [Fact]
    public void C_drive_recommendation_list_wraps_text_without_horizontal_scroll()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var start = xaml.IndexOf("<ListBox x:Name=\"RecommendationsListBox\"", StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = xaml.IndexOf("</ListBox>", start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        var recommendationListXaml = xaml[start..end];

        recommendationListXaml.Should().Contain("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"");
        recommendationListXaml.Should().Contain("HorizontalContentAlignment=\"Stretch\"");
    }

    [Fact]
    public void C_drive_recommendation_execute_button_starts_disabled_until_actionable_card_selected()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var recommendationListStart = xaml.IndexOf("<ListBox x:Name=\"RecommendationsListBox\"", StringComparison.Ordinal);
        var recommendationListEnd = xaml.IndexOf("</ListBox>", recommendationListStart, StringComparison.Ordinal);
        var recommendationListXaml = xaml[recommendationListStart..recommendationListEnd];
        var buttonStart = xaml.IndexOf("<Button x:Name=\"ExecuteRecommendationButton\"", StringComparison.Ordinal);
        var buttonEnd = xaml.IndexOf("/>", buttonStart, StringComparison.Ordinal);
        var buttonXaml = xaml[buttonStart..buttonEnd];

        recommendationListXaml.Should().Contain("SelectionChanged=\"RecommendationsListBox_SelectionChanged\"");
        buttonXaml.Should().Contain("IsEnabled=\"False\"");
    }

    [Fact]
    public void C_drive_recommendation_selection_preview_explains_confirmation_quarantine_and_restore()
    {
        var operation = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            AffectedPaths = [@"C:\temp"]
        };
        var recommendation = new Recommendation
        {
            Title = "\u53ef\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Finding = @"C:\temp \u5360\u7528 512 MB",
            Reason = "\u4e34\u65f6\u76ee\u5f55\u901a\u5e38\u53ef\u91cd\u5efa\u3002",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            Evidence = ["\u5206\u7c7b\u7ed3\u679c: Temp"],
            Operation = operation
        };
        var actionableCard = RecommendationCardPresenter.Create(recommendation);
        var observeCard = RecommendationCardPresenter.Create(CreateUnexpectedRootRecommendation("AMD", 1024));

        var actionableSelection = RecommendationSelectionPresenter.Create(actionableCard);
        var observeSelection = RecommendationSelectionPresenter.Create(observeCard);
        var emptySelection = RecommendationSelectionPresenter.Create(null);

        actionableSelection.CanContinue.Should().BeTrue();
        actionableSelection.ButtonText.Should().Contain("\u9694\u79bb\u533a");
        actionableSelection.ExplanationText.Should().Contain("\u4e0d\u4f1a\u9a6c\u4e0a\u6e05\u7406");
        actionableSelection.ExplanationText.Should().Contain("\u4e8c\u6b21\u786e\u8ba4");
        actionableSelection.ExplanationText.Should().Contain("\u9694\u79bb\u533a");
        actionableSelection.ExplanationText.Should().Contain("\u4e0d\u662f\u6c38\u4e45\u5220\u9664");
        actionableSelection.ExplanationText.Should().Contain("\u540e\u6094\u836f\u4e2d\u5fc3");
        actionableSelection.ExplanationText.Should().Contain("\u8fd8\u539f");
        actionableSelection.ExplanationText.Should().Contain("512.0 MB");
        observeSelection.CanContinue.Should().BeFalse();
        observeSelection.ExplanationText.Should().Contain("\u4e0d\u80fd\u76f4\u63a5\u6267\u884c");
        emptySelection.CanContinue.Should().BeFalse();
        emptySelection.ExplanationText.Should().Contain("\u5148\u9009\u62e9");
    }

    [Fact]
    public void C_drive_low_risk_cleanup_selection_preview_is_structured_and_quarantine_first()
    {
        var operation = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            EvidenceSummary = "\u8fd9\u662f\u4f4e\u98ce\u9669\u4e34\u65f6\u7f13\u5b58",
            AffectedPaths = [@"C:\temp"]
        };
        var recommendation = new Recommendation
        {
            Title = "\u53ef\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Finding = @"C:\temp \u5360\u7528 512 MB",
            Reason = "\u4e34\u65f6\u76ee\u5f55\u901a\u5e38\u53ef\u91cd\u5efa\u3002",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            Evidence = ["\u5206\u7c7b\u7ed3\u679c: Temp"],
            Operation = operation
        };

        var selection = RecommendationSelectionPresenter.Create(RecommendationCardPresenter.Create(recommendation));

        selection.CanContinue.Should().BeTrue();
        selection.CanExecuteDirectly.Should().BeFalse();
        selection.AgentTakeaway.Should().Contain("\u53ef\u4ee5\u5904\u7406");
        selection.AgentTakeaway.Should().Contain("\u9694\u79bb\u533a");
        selection.NextStepText.Should().Contain("\u4e8c\u6b21\u786e\u8ba4");
        selection.NextStepText.Should().Contain("512.0 MB");
        selection.SafetyBoundary.Should().Contain("\u4e0d\u662f\u6c38\u4e45\u5220\u9664");
        selection.SafetyBoundary.Should().Contain("\u540e\u6094\u836f\u4e2d\u5fc3");
        selection.PlanLines.Should().Contain(line => line.Contains("\u67e5\u770b\u8bc1\u636e"));
        selection.PlanLines.Should().Contain(line => line.Contains("1 \u4e2a\u4f4d\u7f6e"));
        selection.PlanLines.Should().Contain(line => line.Contains("\u79fb\u5230\u9694\u79bb\u533a"));
        selection.PlanLines.Should().Contain(line => line.Contains("\u672c\u5730\u5b89\u5168\u7ba1\u7ebf"));
        selection.PlanLines.Should().Contain(line => line.Contains("\u540e\u6094\u836f\u4e2d\u5fc3"));

        var visibleText = string.Join(
            Environment.NewLine,
            [selection.ExplanationText, selection.AgentTakeaway, selection.NextStepText, selection.SafetyBoundary, .. selection.PlanLines]);
        visibleText.Should().NotContain(@"C:\temp");
    }

    [Fact]
    public void C_drive_cleanup_selection_preview_has_stable_beginner_fields()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationActionTakeawayTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationActionNextStepTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationActionSafetyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationActionPlanListBox\"");
        code.Should().Contain("RecommendationActionTakeawayTextBlock.Text = selection.AgentTakeaway;");
        code.Should().Contain("RecommendationActionNextStepTextBlock.Text = selection.NextStepText;");
        code.Should().Contain("RecommendationActionSafetyTextBlock.Text = selection.SafetyBoundary;");
        code.Should().Contain("RecommendationActionPlanListBox.ItemsSource = selection.PlanLines;");
    }

    [Fact]
    public void C_drive_cleanup_confirmation_puts_plain_summary_before_technical_paths()
    {
        var operation = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: temp",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EstimatedImpactBytes = 512L * 1024 * 1024,
            EvidenceSummary = "\u4e34\u65f6\u7f13\u5b58\u5360\u7528 512 MB",
            ConfirmationText = @"\u786e\u8ba4\u5c06 C:\temp \u79fb\u52a8\u5230\u9694\u79bb\u533a",
            AffectedPaths = [@"C:\temp"]
        };

        var confirmation = CleanupConfirmationPresenter.Create(operation, @"D:\OMNIX-Entropy\Quarantine");

        confirmation.Title.Should().Contain("\u786e\u8ba4\u79fb\u52a8\u5230\u9694\u79bb\u533a");
        confirmation.BeginnerText.Should().Contain("Agent \u5224\u65ad");
        confirmation.BeginnerText.Should().Contain("\u4e0d\u662f\u6c38\u4e45\u5220\u9664");
        confirmation.BeginnerText.Should().Contain("\u540e\u6094\u836f\u4e2d\u5fc3");
        confirmation.BeginnerText.Should().Contain("512.0 MB");
        confirmation.BeginnerText.Should().Contain("1 \u4e2a\u4f4d\u7f6e");
        confirmation.BeginnerText.Should().NotContain(@"C:\temp");
        confirmation.OutcomePreviewLines.Should().Contain(line => line.Contains("\u9694\u79bb\u533a"));
        confirmation.OutcomePreviewLines.Should().Contain(line => line.Contains("\u540e\u6094\u836f\u4e2d\u5fc3") && line.Contains("\u65f6\u95f4\u7ebf"));
        confirmation.OutcomePreviewLines.Should().Contain(line => line.Contains("\u4e0d\u662f\u6c38\u4e45\u5220\u9664"));
        confirmation.OutcomePreviewLines.Should().Contain(line => line.Contains("\u4e0d\u4f1a\u4fee\u6539\u6ce8\u518c\u8868") && line.Contains("\u670d\u52a1") && line.Contains("\u81ea\u542f\u52a8"));
        confirmation.OutcomePreviewLines.Should().AllSatisfy(line => line.Should().NotContain(@"C:\temp"));
        confirmation.TechnicalDetails.Should().Contain(line => line.Contains(@"C:\temp"));
        confirmation.TechnicalDetails.Should().Contain(line => line.Contains(@"D:\OMNIX-Entropy\Quarantine"));
        confirmation.TechnicalDetails.Should().Contain(line => line.Contains("\u4e34\u65f6\u7f13\u5b58"));
        confirmation.MessageText.IndexOf("\u6280\u672f\u8be6\u60c5", StringComparison.Ordinal)
            .Should().BeGreaterThan(confirmation.MessageText.IndexOf("\u540e\u6094\u836f\u4e2d\u5fc3", StringComparison.Ordinal));
    }

    [Fact]
    public void C_drive_cleanup_execution_confirmation_uses_confirmation_presenter()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private async Task ExecuteSelectedRecommendationAsync", "private sealed class AppTileUi");

        handler.Should().Contain("CleanupConfirmationPresenter.Create");
        handler.Should().Contain("new CleanupConfirmationWindow(confirmation)");
        handler.Should().Contain("ShowDialog() != true");
        handler.Should().NotContain("var paths = string.Join");
        handler.Should().NotContain("MessageBox.Show");
    }

    [Fact]
    public void Uninstall_residue_low_risk_confirmation_uses_custom_quarantine_confirmation_window()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private async Task ReviewSelectedUninstallResidueAsync", "private void ShowResidueReviewInline");

        handler.Should().Contain("QuarantineOperationPolicy.ValidateCandidate");
        handler.Should().Contain("CleanupConfirmationPresenter.Create");
        handler.Should().Contain("new CleanupConfirmationWindow(confirmation)");
        handler.Should().Contain("ShowDialog() != true");
        handler.Should().Contain("SafetyOperationPipeline");
        handler.Should().NotContain("BuildResidueConfirmMessage");
        code.Should().NotContain("private static string BuildResidueConfirmMessage");
    }

    [Fact]
    public void C_drive_cleanup_confirmation_window_has_collapsed_technical_details_and_stable_hooks()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "CleanupConfirmationWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "CleanupConfirmationWindow.xaml.cs"));

        xaml.Should().Contain("x:Class=\"Css.App.CleanupConfirmationWindow\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationOutcomeHeaderTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationOutcomeListBox\"");
        xaml.Should().Contain("ItemsSource=\"{Binding OutcomePreviewLines}\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationTechnicalDetailsExpander\"");
        xaml.IndexOf("CleanupConfirmationOutcomeListBox", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("CleanupConfirmationTechnicalDetailsExpander", StringComparison.Ordinal));
        xaml.Should().Contain("IsExpanded=\"False\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationTechnicalDetailsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationConfirmButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CleanupConfirmationCancelButton\"");
        code.Should().Contain("DialogResult = true;");
        code.Should().Contain("DialogResult = false;");
    }

    [Fact]
    public void C_drive_cleanup_preview_and_execute_controls_have_stable_automation_ids()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"CDriveNavButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"StartScanButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"ExecuteRecommendationButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationActionTakeawayTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"RecommendationActionPlanListBox\"");
    }

    [Fact]
    public void C_drive_cleanup_gui_smoke_uses_isolated_scan_fixture_and_cancels_confirmation()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-cdrive-cleanup-confirmation-smoke.ps1"));
        var helpers = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var doc = File.ReadAllText(FindRepositoryFile("docs", "development", "gui-smokes.md"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("OMNIX_ENTROPY_DATA_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_QUARANTINE_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_CDRIVE_SCAN_ROOT");
        script.Should().Contain("qa-cdrive-cleanup-scan-root");
        script.Should().Contain("RecommendationActionTakeawayTextBlock");
        script.Should().Contain("RecommendationActionPlanListBox");
        script.Should().Contain("CleanupConfirmationSummaryTextBlock");
        script.Should().Contain("CleanupConfirmationOutcomeListBox");
        script.Should().Contain("CleanupConfirmationTechnicalDetailsExpander");
        script.Should().Contain("CleanupConfirmationCancelButton");
        script.Should().Contain("Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'CleanupConfirmationSummaryTextBlock'");
        script.Should().NotContain("function Find-WindowByDescendantAutomationId");
        script.Should().NotContain("function Find-SecondaryWindowWithChild");
        helpers.Should().Contain("function Find-WindowByDescendantAutomationId");
        helpers.Should().Contain("function Find-SecondaryWindowWithChild");
        helpers.Should().Contain("[System.Windows.Automation.TreeScope]::Descendants");
        helpers.Should().Contain("[System.Windows.Automation.TreeWalker]::ControlViewWalker");
        script.Should().Contain("cancelClicked = $true");
        script.Should().Contain("confirmationDialogFound = $true");
        script.Should().NotContain("CleanupConfirmationConfirmButton");
        script.Should().NotContain("Invoke-Element $confirm");
        code.Should().Contain("AppDevelopmentPathResolver.ResolveCDriveScanRoot");
        doc.Should().Contain("OMNIX_ENTROPY_CDRIVE_SCAN_ROOT");
        doc.Should().Contain("development and GUI smoke tests only");
    }

    [Fact]
    public void Uninstall_residue_confirmation_gui_smoke_uses_software_fixture_and_cancels_confirmation()
    {
        var repoRoot = Path.GetDirectoryName(FindRepositoryFile("AGENTS.md"))!;
        var scriptPath = Path.Combine(repoRoot, ".omx", "gui-uninstall-residue-confirmation-smoke.ps1");
        File.Exists(scriptPath).Should().BeTrue();
        var script = File.Exists(scriptPath) ? File.ReadAllText(scriptPath) : string.Empty;
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var doc = File.ReadAllText(FindRepositoryFile("docs", "development", "gui-smokes.md"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"AppsNavButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"ScanSoftwareButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AppTilesListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerResidueReviewButton\"");
        code.Should().Contain("SoftwareInventoryFixtureScanner.TryCreate");
        code.Should().Contain("AppDevelopmentPathResolver.ResolveSoftwareInventoryFixturePath");
        code.Should().Contain("ScanSoftwareProfilesAsync");
        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("OMNIX_ENTROPY_DATA_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_QUARANTINE_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE");
        script.Should().Contain("qa-uninstall-residue-software-fixture.json");
        script.Should().Contain("qa-uninstall-residue-cancel-outcome.png");
        script.Should().Contain("DrawerResidueReviewButton");
        script.Should().Contain("CleanupConfirmationSummaryTextBlock");
        script.Should().Contain("CleanupConfirmationOutcomeListBox");
        script.Should().Contain("CleanupConfirmationCancelButton");
        script.Should().Contain("Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'CleanupConfirmationSummaryTextBlock'");
        script.Should().Contain("DrawerActionPreviewTitleTextBlock");
        script.Should().Contain("DrawerActionPreviewPrimaryButton");
        script.Should().Contain("cancelOutcomeVisible = $true");
        script.Should().Contain("primaryButtonHiddenAfterCancel = $true");
        script.Should().Contain("residueConfirmationFound = $true");
        script.Should().Contain("cancelClicked = $true");
        script.Should().Contain("cancelOutcomeScreenshot = $cancelOutcomeScreenshotPath");
        script.Should().Contain("residueStillExists =");
        script.Should().NotContain("CleanupConfirmationConfirmButton");
        script.Should().NotContain("Invoke-Element $confirm");
        doc.Should().Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE");
        doc.Should().Contain("development and GUI smoke tests only");
    }

    [Fact]
    public void C_drive_recommendation_selection_handler_uses_selection_presenter()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(
            code,
            "private void RecommendationsListBox_SelectionChanged",
            "private void ExplainHealthFinding_Click");

        handler.Should().Contain("RecommendationSelectionPresenter.Create");
        handler.Should().NotContain("RecommendationsListBox_SelectionChangedLegacy");
        handler.Should().NotContain("RecommendationActionTextBlock.Text = \"");
    }

    [Fact]
    public void Residue_review_handler_rescans_before_deciding_software_still_installed()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var selectionHandler = SourceMethodExtractor.Extract(
            code,
            "private async Task ReviewSelectedUninstallResidueAsync()");
        var handler = SourceMethodExtractor.Extract(
            code,
            "private async Task ReviewUninstallResidueAsync(");

        var rescanIndex = handler.IndexOf(
            "knownAfterProfiles ?? await ScanSoftwareProfilesAsync()",
            StringComparison.Ordinal);
        var buildIndex = handler.IndexOf("UninstallResidueScanBuilder.Build", StringComparison.Ordinal);

        rescanIndex.Should().BeGreaterThanOrEqualTo(0);
        buildIndex.Should().BeGreaterThan(rescanIndex);
        handler.Should().Contain("SetSoftwareProfiles(afterProfiles, refreshCatalog: false);");
        selectionHandler.Should().Contain("ReviewUninstallResidueAsync(selected.Profile)");
        handler.Should().Contain("ShowResidueReviewInline(review);");
        handler.Should().NotContain("cachedReport");
        handler.Should().NotContain("TryBuildStillInstalledReport(before, _softwareProfiles)");
    }

    [Fact]
    public void Residue_review_handler_shows_inline_cancel_and_quarantine_outcomes()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private async Task ReviewSelectedUninstallResidueAsync", "private void ShowResidueReviewInline");

        handler.Should().Contain("ShowResidueOutcomeInline(UninstallResidueDrawerReviewPresenter.CreateCanceled(review))");
        handler.Should().Contain("ShowResidueOutcomeInline(UninstallResidueDrawerReviewPresenter.CreateQuarantined(review, result.Summary");
        code.Should().Contain("private void ShowResidueOutcomeInline(UninstallResidueDrawerReviewViewModel outcome)");
    }

    [Fact]
    public void App_drawer_uses_only_one_shared_action_preview_host()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        var hostIndex = xaml.IndexOf("DrawerActionPreviewPanel", StringComparison.Ordinal);
        var residueIndex = xaml.IndexOf("DrawerResidueReviewButton", StringComparison.Ordinal);

        hostIndex.Should().BeGreaterThanOrEqualTo(0);
        residueIndex.Should().BeGreaterThan(hostIndex);
        xaml.Should().Contain("DrawerActionPreviewTitleTextBlock");
        xaml.Should().Contain("DrawerActionPreviewSummaryTextBlock");
        xaml.Should().Contain("DrawerActionPreviewListBox");

        var legacyControlNames = new[]
        {
            "DrawerCachePreviewPanel",
            "DrawerStartupPreviewPanel",
            "DrawerUninstallPreviewTitleTextBlock",
            "DrawerUninstallPreviewListBox",
            "DrawerMigrationSummaryTextBlock",
            "DrawerMigrationPreviewListBox"
        };

        foreach (var legacyName in legacyControlNames)
        {
            xaml.Should().NotContain(legacyName);
            code.Should().NotContain(legacyName);
        }
    }

    [Fact]
    public void App_drawer_shared_action_host_wraps_text_without_horizontal_scroll()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var start = xaml.IndexOf("<ListBox x:Name=\"DrawerActionPreviewListBox\"", StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = xaml.IndexOf("</ListBox>", start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        var listXaml = xaml[start..end];

        listXaml.Should().Contain("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"");
        listXaml.Should().Contain("HorizontalContentAlignment=\"Stretch\"");
        listXaml.Should().Contain("TextWrapping=\"Wrap\"");
    }

    [Fact]
    public void App_drawer_action_controls_have_stable_automation_ids_for_gui_smoke()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerUninstallButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerMigrateButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerCleanCacheButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerDisableStartupButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewListBox\"");
    }

    [Fact]
    public void App_drawer_action_host_binds_agent_action_card_fields()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("DrawerActionPreviewAgentTextBlock");
        xaml.Should().Contain("DrawerActionPreviewNextStepTextBlock");
        xaml.Should().Contain("DrawerActionPreviewSafetyTextBlock");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewAgentTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewNextStepTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewSafetyTextBlock\"");
        code.Should().Contain("DrawerActionPreviewAgentTextBlock.Text = state.AgentTakeaway;");
        code.Should().Contain("DrawerActionPreviewNextStepTextBlock.Text = state.NextStepText;");
        code.Should().Contain("DrawerActionPreviewSafetyTextBlock.Text = state.SafetyText;");
    }

    [Fact]
    public void App_drawer_action_host_primary_button_uses_only_safe_navigation_or_cache_coordinator()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("x:Name=\"DrawerActionPreviewPrimaryButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerActionPreviewPrimaryButton\"");
        xaml.Should().Contain("Click=\"DrawerActionPreviewPrimary_Click\"");
        code.Should().Contain("DrawerActionPreviewPrimaryButton.Content = state.PrimaryActionText;");
        code.Should().Contain("DrawerActionPreviewPrimaryButton.Tag = state.PrimaryActionKey;");
        code.Should().Contain("case \"CacheCleanup\":");
        code.Should().Contain("await ExecutePendingAppCacheCleanupAsync();");
        code.Should().Contain("case \"Timeline\":");
        code.Should().Contain("ShowPage(\"Timeline\");");

        var handler = ExtractMethod(code, "private async void DrawerActionPreviewPrimary_Click", "private async void ReviewUninstallResidue_Click");
        handler.Should().NotContain("RestoreSelectedTimelineEntryAsync");
        handler.Should().Contain("new AppCacheCleanupOperationHandler(");
        handler.Should().Contain("new SafetyOperationPipeline(handler.ExecuteAsync)");
        handler.Should().NotContain("Process.Start");
        handler.Should().NotContain("Registry.");
        handler.Should().NotContain("File.Move");
        handler.Should().NotContain("Directory.Move");
    }

    [Fact]
    public void App_drawer_action_preview_scrolls_into_view_after_action_clicks()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("x:Name=\"AppDrawerScrollViewer\"");
        xaml.Should().Contain("VerticalScrollBarVisibility=\"Auto\"");
        code.Should().Contain("DrawerActionPreviewPanel.BringIntoView();");
    }

    [Fact]
    public void Install_guard_analysis_loads_remembered_routing_rules_without_running_installer()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private async void AnalyzeInstaller_Click", "private async void CaptureBeforeInstall_Click");

        handler.Should().Contain("InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath())");
        handler.Should().Contain("new WindowsInstallerPackageInspector().Inspect(path)");
        handler.Should().Contain("InstallerAnalyzer.AnalyzePackage(package, routingMemory: routingMemory)");
        handler.Should().Contain("InstallerRoutingCapabilityPolicy.Evaluate(result, package)");
        handler.Should().Contain("analysis.RecommendedRoute.FromUserMemory");
        code.Should().Contain("private static string DefaultInstallRoutingMemoryPath()");
        handler.Should().NotContain("Start-Process");
        handler.Should().NotContain("Process.Start");
    }

    [Fact]
    public void Install_guard_remember_route_button_uses_scope_choice_window_before_writing_memory()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private void RememberInstallRoute_Click", "private async void CaptureBeforeInstall_Click");
        var repoRoot = Path.GetDirectoryName(FindRepositoryFile("AGENTS.md"))!;
        var choiceWindowXamlPath = Path.Combine(repoRoot, "src", "Css.App", "InstallRouteMemoryChoiceWindow.xaml");
        var choiceWindowCodePath = Path.Combine(repoRoot, "src", "Css.App", "InstallRouteMemoryChoiceWindow.xaml.cs");

        File.Exists(choiceWindowXamlPath).Should().BeTrue();
        File.Exists(choiceWindowCodePath).Should().BeTrue();
        var choiceWindowXaml = File.ReadAllText(choiceWindowXamlPath);
        var choiceWindowCode = File.ReadAllText(choiceWindowCodePath);

        xaml.Should().Contain("x:Name=\"InstallRememberRouteButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallRememberRouteButton\"");
        xaml.Should().Contain("Click=\"RememberInstallRoute_Click\"");
        choiceWindowXaml.Should().Contain("AutomationProperties.AutomationId=\"InstallRouteMemoryChoiceWindow\"");
        choiceWindowXaml.Should().Contain("AutomationProperties.AutomationId=\"RememberSoftwareRouteButton\"");
        choiceWindowXaml.Should().Contain("AutomationProperties.AutomationId=\"RememberCategoryRouteButton\"");
        choiceWindowXaml.Should().Contain("AutomationProperties.AutomationId=\"CancelInstallRouteMemoryButton\"");
        choiceWindowCode.Should().Contain("InstallRoutingMemoryScope? SelectedScope");
        choiceWindowCode.Should().Contain("InstallRoutingMemoryScope.Software");
        choiceWindowCode.Should().Contain("InstallRoutingMemoryScope.Category");
        code.Should().Contain("private InstallerDetectionResult? _lastInstallerAnalysis;");
        code.Should().Contain("_lastInstallerAnalysis = result;");
        code.Should().Contain("InstallRememberRouteButton.IsEnabled = package.HasStableIdentity")
            .And.Contain("InstallerRoutingCapabilityMode.AutomaticInteractiveRoute")
            .And.Contain("InstallerRoutingCapabilityMode.GuidedInteractiveRoute");
        handler.Should().Contain("new InstallRouteMemoryChoiceWindow(InstallRouteMemoryChoicePresenter.Create(_lastInstallerAnalysis))");
        handler.Should().Contain("choiceWindow.ShowDialog() != true");
        handler.Should().Contain("choiceWindow.SelectedScope");
        handler.Should().Contain("InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath())");
        handler.Should().Contain("InstallRoutingMemoryScope.Category");
        handler.Should().Contain("memory.RememberRouteForCategory(_lastInstallerAnalysis.RecommendedRoute)");
        handler.Should().Contain("memory.RememberRoute(_lastInstallerAnalysis.RecommendedRoute)");
        handler.Should().Contain("InstallRoutingMemoryStore.Save(DefaultInstallRoutingMemoryPath(), updated)");
        handler.Should().NotContain("MessageBox.Show");
        handler.Should().NotContain("Start-Process");
        handler.Should().NotContain("Process.Start");
        handler.Should().NotContain("InstallerAnalyzer.AnalyzePath");
    }

    [Fact]
    public void Install_guard_page_shows_learned_rules_read_only()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var loader = ExtractMethod(code, "private void LoadInstallRoutingMemoryRules", "private async void StartScan_Click");
        var rememberHandler = ExtractMethod(code, "private void RememberInstallRoute_Click", "private async void CaptureBeforeInstall_Click");

        xaml.Should().Contain("x:Name=\"InstallRoutingMemoryListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallRoutingMemoryListBox\"");
        xaml.Should().Contain("x:Name=\"InstallRoutingMemorySummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallRoutingMemorySummaryTextBlock\"");
        code.Should().Contain("LoadInstallRoutingMemoryRules();");
        loader.Should().Contain("InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath())");
        loader.Should().Contain("InstallRoutingMemoryPresenter.Create");
        loader.Should().Contain("InstallRoutingMemoryListBox.ItemsSource = view.Rows;");
        loader.Should().NotContain("InstallRoutingMemoryStore.Save");
        rememberHandler.Should().Contain("LoadInstallRoutingMemoryRules();");
    }

    [Fact]
    public void Install_guard_forget_learned_rule_only_edits_memory_after_confirmation()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private void ForgetInstallRoutingRule_Click", "private void CancelScan_Click");
        var selectionHandler = ExtractMethod(code, "private void InstallRoutingMemoryListBox_SelectionChanged", "private void ForgetInstallRoutingRule_Click");

        xaml.Should().Contain("SelectionChanged=\"InstallRoutingMemoryListBox_SelectionChanged\"");
        xaml.Should().Contain("x:Name=\"ForgetInstallRoutingRuleButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"ForgetInstallRoutingRuleButton\"");
        xaml.Should().Contain("Click=\"ForgetInstallRoutingRule_Click\"");
        xaml.Should().Contain("IsEnabled=\"False\"");
        selectionHandler.Should().Contain("row.CanForget");
        selectionHandler.Should().Contain("ForgetInstallRoutingRuleButton.IsEnabled");
        handler.Should().Contain("MessageBox.Show");
        handler.Should().Contain("\u53ea\u4f1a\u5f71\u54cd\u4ee5\u540e\u7684\u5b89\u88c5\u5efa\u8bae");
        handler.Should().Contain("InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath())");
        handler.Should().Contain("memory.ForgetRule(row.RuleKey)");
        handler.Should().Contain("InstallRoutingMemoryStore.Save(DefaultInstallRoutingMemoryPath(), updated)");
        handler.Should().Contain("LoadInstallRoutingMemoryRules();");
        handler.Should().NotContain("Start-Process");
        handler.Should().NotContain("Process.Start");
        handler.Should().NotContain("InstallerAnalyzer.AnalyzePath");
    }

    [Fact]
    public void Install_page_keeps_manual_snapshot_comparison_behind_collapsed_advanced_diagnostics()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var prepareHandler = ExtractMethod(code, "private async void PrepareInstaller_Click", "private async void CaptureBeforeInstall_Click");

        xaml.Should().Contain("x:Name=\"InstallAutomaticMonitoringTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallAutomaticMonitoringTextBlock\"");
        xaml.Should().Contain("正常安装会自动记录安装前后的变化，不需要手动操作。");
        xaml.Should().Contain("x:Name=\"InstallManualComparisonExpander\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallManualComparisonExpander\"");
        xaml.Should().Contain("Header=\"高级诊断：手动变化对比\"");
        xaml.Should().Contain("IsExpanded=\"False\"");

        var prepareIndex = xaml.IndexOf("x:Name=\"PrepareInstallerButton\"", StringComparison.Ordinal);
        var automaticMonitoringIndex = xaml.IndexOf("x:Name=\"InstallAutomaticMonitoringTextBlock\"", StringComparison.Ordinal);
        var advancedExpanderIndex = xaml.IndexOf("x:Name=\"InstallManualComparisonExpander\"", StringComparison.Ordinal);
        var captureBeforeIndex = xaml.IndexOf("x:Name=\"CaptureBeforeInstallButton\"", StringComparison.Ordinal);
        var captureAfterIndex = xaml.IndexOf("x:Name=\"CaptureAfterInstallButton\"", StringComparison.Ordinal);
        var buildReportIndex = xaml.IndexOf("x:Name=\"BuildInstallDiffButton\"", StringComparison.Ordinal);

        prepareIndex.Should().BeGreaterThanOrEqualTo(0);
        prepareIndex.Should().BeLessThan(automaticMonitoringIndex);
        automaticMonitoringIndex.Should().BeLessThan(advancedExpanderIndex);
        advancedExpanderIndex.Should().BeLessThan(captureBeforeIndex);
        captureBeforeIndex.Should().BeLessThan(captureAfterIndex);
        captureAfterIndex.Should().BeLessThan(buildReportIndex);

        prepareHandler.Should().Contain("正在自动捕获安装前只读快照");
        prepareHandler.Should().Contain("InstallBeforeSnapshotEvidenceService.CreateAsync");
        prepareHandler.Should().Contain("InstallerExecutionCoordinator.CreateProduction");
        prepareHandler.Should().Contain("_afterInstallSnapshot = execution.AfterSnapshot");
        prepareHandler.Should().Contain("_lastInstallDiffReport = execution.Report");
    }

    [Fact]
    public void Install_diff_page_shows_beginner_cards_before_raw_technical_report()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(code, "private void BuildInstallDiff_Click", "private async void LoadTimeline_Click");

        xaml.Should().Contain("x:Name=\"InstallDiffSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffSummaryTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCardsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCardsListBox\"");
        xaml.Should().Contain("x:Name=\"InstallDiffTechnicalDetailsExpander\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffTechnicalDetailsExpander\"");
        xaml.IndexOf("InstallDiffCardsListBox", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffTextBox", StringComparison.Ordinal));
        xaml.IndexOf("InstallDiffTechnicalDetailsExpander", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffTextBox", StringComparison.Ordinal));
        code.Should().Contain("private void ApplyInstallDiffPresentation");
        handler.Should().Contain("InstallSnapshotDiffPresenter.Create(report)");
        handler.Should().Contain("ApplyInstallDiffPresentation(view)");
        code.Should().Contain("InstallDiffCardsListBox.ItemsSource = view.Cards;");
        code.Should().Contain("InstallDiffSummaryTextBlock.Text = view.Summary;");
        code.Should().Contain("InstallDiffTextBox.Text = string.Join(Environment.NewLine, view.TechnicalDetails);");
    }

    [Fact]
    public void Install_diff_agent_explanation_is_on_demand_and_plan_only()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var buildHandler = ExtractMethod(code, "private void BuildInstallDiff_Click", "private void ExplainInstallDiff_Click");
        var explainHandler = ExtractMethod(code, "private void ExplainInstallDiff_Click", "private void ApplyInstallDiffAgentAdvice");

        xaml.Should().Contain("x:Name=\"InstallDiffAgentExplainButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffAgentExplainButton\"");
        xaml.Should().Contain("x:Name=\"InstallDiffAgentPanel\"");
        xaml.Should().Contain("Visibility=\"Collapsed\"");
        xaml.Should().Contain("x:Name=\"InstallDiffAgentHeadlineTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffAgentHeadlineTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffAgentStepsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffAgentStepsListBox\"");
        xaml.IndexOf("InstallDiffAgentPanel", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffTechnicalDetailsExpander", StringComparison.Ordinal));

        buildHandler.Should().Contain("_lastInstallDiffReport = report;");
        buildHandler.Should().Contain("InstallDiffAgentExplainButton.IsEnabled = true;");
        explainHandler.Should().Contain("InstallSnapshotDiffAgentPresenter.Create(_lastInstallDiffReport)");
        explainHandler.Should().Contain("ApplyInstallDiffAgentAdvice(advice)");
        explainHandler.Should().NotContain("SafetyOperationPipeline");
        explainHandler.Should().NotContain("Process.Start");
        explainHandler.Should().NotContain("Start-Process");
        code.Should().Contain("InstallDiffAgentPanel.Visibility = Visibility.Visible;");
        code.Should().Contain("InstallDiffAgentStepsListBox.ItemsSource = advice.NextSteps;");
    }

    [Fact]
    public void Install_diff_action_plan_is_generated_on_demand_before_technical_details()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(
            code,
            "private void GenerateInstallDiffActionPlan_Click",
            "private void ApplyInstallDiffActionPlan");

        xaml.Should().Contain("x:Name=\"InstallDiffGeneratePlanButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffGeneratePlanButton\"");
        xaml.Should().Contain("x:Name=\"InstallDiffActionPlanPanel\"");
        xaml.Should().Contain("x:Name=\"InstallDiffActionPlanSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffActionPlanSummaryTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffActionPlanReviewSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffActionPlanReviewSummaryTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffActionPlanListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffActionPlanListBox\"");
        xaml.Should().Contain("x:Name=\"InstallDiffActionPlanSafetyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffActionPlanSafetyTextBlock\"");
        var actionPlanListStart = xaml.IndexOf("<ListBox x:Name=\"InstallDiffActionPlanListBox\"", StringComparison.Ordinal);
        var actionPlanTemplateStart = xaml.IndexOf("<ListBox.ItemTemplate>", actionPlanListStart, StringComparison.Ordinal);
        xaml[actionPlanListStart..actionPlanTemplateStart].Should().Contain("IsHitTestVisible=\"False\"")
            .And.Contain("Focusable=\"False\"");
        xaml.IndexOf("InstallDiffActionPlanPanel", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffTechnicalDetailsExpander", StringComparison.Ordinal));

        handler.Should().Contain("InstallSnapshotDiffActionPlanPresenter.Create(_lastInstallDiffReport)");
        handler.Should().Contain("ApplyInstallDiffActionPlan(plan)");
        handler.Should().NotContain("SafetyOperationPipeline");
        handler.Should().NotContain("Process.Start");
        handler.Should().NotContain("Start-Process");
        code.Should().Contain("InstallDiffActionPlanReviewSummaryTextBlock.Text = plan.ReviewSummary;");
        code.Should().Contain("InstallDiffActionPlanListBox.ItemsSource = plan.Items;");
        code.Should().Contain("InstallDiffActionPlanPanel.Visibility = Visibility.Visible;");
        code.Should().Contain("InstallDiffActionPlanPanel.Visibility = Visibility.Collapsed;");
    }

    [Fact]
    public void Install_diff_evidence_review_is_collapsed_on_demand_and_hides_technical_identifiers()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("x:Name=\"InstallDiffEvidenceReviewExpander\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffEvidenceReviewExpander\"");
        xaml.Should().Contain("IsExpanded=\"False\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCDriveEvidenceReviewListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCDriveEvidenceReviewListBox\"");
        xaml.Should().Contain("x:Name=\"InstallDiffBackgroundEvidenceReviewListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffBackgroundEvidenceReviewListBox\"");
        xaml.Should().Contain("x:Name=\"InstallDiffEvidenceReviewSafetyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffEvidenceReviewSafetyTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffEligibleActionsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffEligibleActionsListBox\"");
        foreach (var listName in new[]
                 {
                     "InstallDiffCDriveEvidenceReviewListBox",
                     "InstallDiffBackgroundEvidenceReviewListBox"
                 })
        {
            var listStart = xaml.IndexOf($"<ListBox x:Name=\"{listName}\"", StringComparison.Ordinal);
            var templateStart = xaml.IndexOf("<ListBox.ItemTemplate>", listStart, StringComparison.Ordinal);
            xaml[listStart..templateStart].Should().Contain("IsHitTestVisible=\"False\"")
                .And.Contain("Focusable=\"False\"");
        }
        xaml.IndexOf("InstallDiffActionPlanReviewSummaryTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffEvidenceReviewExpander", StringComparison.Ordinal));
        xaml.IndexOf("InstallDiffEvidenceReviewExpander", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffActionPlanListBox", StringComparison.Ordinal));
        xaml.IndexOf("InstallDiffEvidenceReviewExpander", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffTechnicalDetailsExpander", StringComparison.Ordinal));
        var evidenceExpanderStart = xaml.IndexOf("<Expander x:Name=\"InstallDiffEvidenceReviewExpander\"", StringComparison.Ordinal);
        var evidenceExpanderEnd = xaml.IndexOf("</Expander>", evidenceExpanderStart, StringComparison.Ordinal);
        xaml[evidenceExpanderStart..evidenceExpanderEnd].Should()
            .Contain("AutomationProperties.AutomationId=\"{Binding Kind, StringFormat=InstallDiffCandidatePreviewButton_{0}}\"")
            .And.Contain("Click=\"PreviewInstallDiffCandidate_Click\"")
            .And.NotContain("SafetyOperationPipeline");

        code.Should().Contain("InstallDiffEvidenceReviewExpander.IsExpanded = false;");
        code.Should().Contain("InstallDiffCDriveEvidenceReviewListBox.ItemsSource = plan.EvidenceReview.CDriveItems;");
        code.Should().Contain("InstallDiffBackgroundEvidenceReviewListBox.ItemsSource = plan.EvidenceReview.BackgroundItems;");
        code.Should().Contain("InstallDiffEligibleActionsListBox.ItemsSource = plan.EvidenceReview.EligibleActions;");
        code.Should().Contain("InstallDiffEvidenceReviewSafetyTextBlock.Text = plan.EvidenceReview.SafetyBoundary;");
        code.Should().NotContain("InstallDiffEvidenceReviewExpander.IsExpanded = true;");
    }

    [Fact]
    public void Install_diff_candidate_preview_is_on_demand_and_has_no_execution_path()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(
            code,
            "private void PreviewInstallDiffCandidate_Click",
            "private void ApplyInstallDiffCandidatePreview");

        xaml.Should().Contain("x:Name=\"InstallDiffCandidatePreviewPanel\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidatePreviewPanel\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCandidatePreviewTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidatePreviewTitleTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCandidatePreviewStatusTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidatePreviewStatusTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCandidatePreviewLinesListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidatePreviewLinesListBox\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCandidatePreviewMissingEvidenceListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidatePreviewMissingEvidenceListBox\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCandidatePreviewSafetyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidatePreviewSafetyTextBlock\"");
        xaml.Should().Contain("x:Name=\"InstallDiffCandidateOpenAppButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallDiffCandidateOpenAppButton\"");
        xaml.Should().Contain("Click=\"OpenInstallDiffCandidateApp_Click\"");
        xaml.IndexOf("InstallDiffEligibleActionsListBox", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffCandidatePreviewPanel", StringComparison.Ordinal));
        xaml.IndexOf("InstallDiffCandidatePreviewPanel", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallDiffActionPlanListBox", StringComparison.Ordinal));

        handler.Should().Contain("InstallSnapshotCandidatePreviewPresenter.Create(_lastInstallDiffReport, kind)");
        handler.Should().Contain("ApplyInstallDiffCandidatePreview(preview)");
        handler.Should().NotContain("SafetyOperationPipeline");
        handler.Should().NotContain("OperationDescriptor");
        handler.Should().NotContain("Process.Start");
        handler.Should().NotContain("Start-Process");
        code.Should().Contain("InstallDiffCandidatePreviewPanel.Visibility = Visibility.Collapsed;");
        code.Should().Contain("InstallDiffCandidatePreviewLinesListBox.ItemsSource = preview.Lines;");
        code.Should().Contain("InstallDiffCandidatePreviewMissingEvidenceListBox.ItemsSource = preview.MissingEvidence;");
        code.Should().Contain("InstallDiffCandidateOpenAppButton.Tag = preview.TargetAppName;");
        code.Should().Contain("InstallDiffCandidateOpenAppButton.Visibility = preview.CanNavigateToApp");
        code.Should().Contain("InstallDiffCandidatePreviewPanel.Visibility = Visibility.Visible;");
        var navigationHandler = ExtractMethod(
            code,
            "private async void OpenInstallDiffCandidateApp_Click",
            "private void ApplyInstallDiffPresentation");
        navigationHandler.Should().Contain("ResolveAndOpenAppTargetAsync(targetAppName)");
        navigationHandler.Should().NotContain("SafetyOperationPipeline");
        navigationHandler.Should().NotContain("OperationDescriptor");
        navigationHandler.Should().NotContain("Process.Start");
    }

    [Fact]
    public void Install_diff_gui_smoke_uses_an_isolated_fixture_and_exposes_the_full_report_flow()
    {
        var repoRoot = Path.GetDirectoryName(FindRepositoryFile("AGENTS.md"))!;
        var scriptPath = Path.Combine(repoRoot, ".omx", "gui-install-diff-agent-smoke.ps1");
        File.Exists(scriptPath).Should().BeTrue();
        var script = File.Exists(scriptPath) ? File.ReadAllText(scriptPath) : string.Empty;
        var helper = File.ReadAllText(Path.Combine(repoRoot, ".omx", "wpf-smoke-helpers.ps1"));
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var doc = File.ReadAllText(FindRepositoryFile("docs", "development", "gui-smokes.md"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallNavButton\"");
        xaml.Should().Contain("x:Name=\"InstallPageScrollViewer\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallPageScrollViewer\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CaptureBeforeInstallButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CaptureAfterInstallButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"BuildInstallDiffButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallManualComparisonExpander\"");

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("OMNIX_ENTROPY_DATA_ROOT");
        script.Should().Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE");
        script.Should().Contain("qa-install-diff-software-fixture.json");
        script.Should().Contain("CaptureBeforeInstallButton");
        script.Should().Contain("CaptureAfterInstallButton");
        script.Should().Contain("BuildInstallDiffButton");
        script.Should().Contain("InstallManualComparisonExpander");
        script.Should().Contain("manualComparisonCollapsedByDefault = $manualComparisonCollapsedByDefault");
        script.Should().Contain("InstallDiffCardsListBox");
        script.Should().Contain("InstallDiffAgentExplainButton");
        script.Should().Contain("InstallDiffAgentHeadlineTextBlock");
        script.Should().Contain("InstallDiffAgentStepsListBox");
        script.Should().Contain("InstallDiffTechnicalDetailsExpander");
        script.Should().Contain("function Scroll-InstallPageToBottom");
        script.Should().Contain("function Test-ElementIntersectsViewport");
        script.Should().NotContain("if (-not $Element.Current.IsOffscreen)");
        script.Should().Contain("Scroll-InstallPageToBottom $scrollViewer");
        script.Should().Contain("function Ensure-MaximizedWindow");
        script.Should().Contain("$process.WaitForInputIdle(10000)");
        script.IndexOf("$process.WaitForInputIdle(10000)", StringComparison.Ordinal)
            .Should().BeLessThan(script.IndexOf("Show-WpfWindowForSmoke $window", StringComparison.Ordinal));
        script.Should().Contain("Show-WpfWindowForSmoke $window");
        script.Should().NotContain("Activate-WpfWindow");
        script.Should().NotContain("Focus-WindowWhenReady");
        helper.Should().Contain("function Show-WpfWindowForSmoke");
        helper.Should().Contain("ShowWindowAsync");
        helper.Should().Contain("SetWindowPos");
        helper.Should().NotContain("SetForegroundWindow");
        script.Split("Ensure-MaximizedWindow $windowPattern").Length.Should().BeGreaterThanOrEqualTo(4);
        script.Should().Contain("Save-DesktopScreenshot $reportScreenshotPath");
        script.Should().Contain("Save-DesktopScreenshot $agentScreenshotPath");
        script.Should().Contain("technicalDetailsCollapsed = $true");
        script.Should().Contain("fixtureOnly = $true");
        script.Should().NotContain("InstallRememberRouteButton");
        script.Should().NotContain("CleanupConfirmationConfirmButton");
        doc.Should().Contain("gui-install-diff-agent-smoke.ps1");
        doc.Should().Contain("does not run an installer");
    }

    [Fact]
    public void Install_diff_gui_smoke_proves_the_action_plan_is_visible_and_nothing_executed()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-install-diff-agent-smoke.ps1"));

        script.Should().Contain("$actionPlanScreenshotPath");
        script.Should().Contain("$evidenceReviewScreenshotPath");
        script.Should().Contain("$eligibleActionsScreenshotPath");
        script.Should().Contain("$candidatePreviewScreenshotPath");
        script.Should().Contain("InstallDiffGeneratePlanButton");
        script.Should().Contain("InstallDiffActionPlanSummaryTextBlock");
        script.Should().Contain("InstallDiffActionPlanReviewSummaryTextBlock");
        script.Should().Contain("InstallDiffActionPlanListBox");
        script.Should().Contain("InstallDiffActionPlanSafetyTextBlock");
        script.Should().Contain("InstallDiffEvidenceReviewExpander");
        script.Should().Contain("InstallDiffCDriveEvidenceReviewListBox");
        script.Should().Contain("InstallDiffBackgroundEvidenceReviewListBox");
        script.Should().Contain("InstallDiffEligibleActionsListBox");
        script.Should().Contain("InstallDiffEvidenceReviewSafetyTextBlock");
        script.Should().Contain("actionPlanItemCount");
        script.Should().Contain("nothingExecutedVisible");
        script.Should().Contain("classificationSummaryVisible");
        script.Should().Contain("evidenceReviewCollapsedByDefault");
        script.Should().Contain("evidenceReviewCDriveItemCount");
        script.Should().Contain("evidenceReviewBackgroundItemCount");
        script.Should().Contain("eligibleActionItemCount");
        script.Should().Contain("eligibleActionsPlanOnly");
        script.Should().Contain("candidatePreviewButtonCount");
        script.Should().Contain("candidatePreviewLineCount");
        script.Should().Contain("candidatePreviewReady");
        script.Should().Contain("candidatePreviewNoExecution");
        script.Should().Contain("InstallDiffCandidatePreviewButton_CacheCleanupPlan");
        script.Should().Contain("InstallDiffCandidatePreviewTitleTextBlock");
        script.Should().Contain("InstallDiffCandidatePreviewStatusTextBlock");
        script.Should().Contain("InstallDiffCandidatePreviewLinesListBox");
        script.Should().Contain("InstallDiffCandidatePreviewMissingEvidenceListBox");
        script.Should().Contain("InstallDiffCandidatePreviewSafetyTextBlock");
        script.Should().Contain("InstallDiffCandidateOpenAppButton");
        script.Should().Contain("DrawerTitleTextBlock");
        script.Should().Contain("DrawerCleanCacheButton");
        script.Should().Contain("Invoke-Element $candidatePreviewButton");
        script.Should().Contain("Invoke-Element $openCandidateApp");
        script.Should().Contain("evidenceReviewHidesRawIdentifiers");
        script.Should().Contain("$nothingExecutedText = -join @(");
        script.Should().Contain("[char]0x5C1A");
        script.Should().Contain("Current.Name.Contains($nothingExecutedText)");
        script.Should().NotContain("[string]::Concat(");
        script.Should().Contain("Save-DesktopScreenshot $actionPlanScreenshotPath");
        script.Should().Contain("Save-DesktopScreenshot $evidenceReviewScreenshotPath");
        script.Should().Contain("Save-DesktopScreenshot $eligibleActionsScreenshotPath");
        script.Should().Contain("Save-WindowScreenshot $window $candidatePreviewScreenshotPath");
        script.Should().Contain("Test-ElementIntersectsViewport $scrollViewer $openCandidateApp");
        script.Should().Contain("Save-WindowScreenshot $window $appHandoffScreenshotPath");
        script.Should().Contain("exactAppHandoffReached = $true");
        script.Should().Contain("noOperationExecuted = $true");
        script.Should().Contain("technicalDetailsCollapsed = $true");
        script.Should().NotContain("Invoke-Element $technicalDetails");
    }

    [Fact]
    public void C_drive_root_cause_summary_turns_path_report_into_beginner_cards()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 300L * 1024 * 1024 * 1024,
            FreeBytes = 80L * 1024 * 1024 * 1024,
            TopLevel =
            [
                new() { Name = "Users", Path = @"C:\Users", SizeBytes = 102L * 1024 * 1024 * 1024, Category = UsageCategory.UserProfiles },
                new() { Name = "Program Files", Path = @"C:\Program Files", SizeBytes = 27L * 1024 * 1024 * 1024, Category = UsageCategory.Programs },
                new() { Name = "AMD", Path = @"C:\AMD", SizeBytes = 2L * 1024 * 1024 * 1024, Category = UsageCategory.Other, IsUnexpectedRoot = true },
                new() { Name = "temp", Path = @"C:\temp", SizeBytes = 640L * 1024 * 1024, Category = UsageCategory.Temp, IsUnexpectedRoot = true }
            ],
            BigRocks =
            [
                new() { Name = "Page file", SizeBytes = 8L * 1024 * 1024 * 1024 }
            ]
        };

        var summary = CDriveRootCauseSummaryBuilder.Build(result);

        summary.Headline.Should().Contain("C \u76d8");
        summary.Headline.Should().Contain("\u5df2\u7528 220.0 GB");
        summary.Cards.Should().NotBeEmpty();
        summary.Cards.Should().Contain(card =>
            card.Title.Contains("\u7528\u6237\u6587\u4ef6") &&
            card.Explanation.Contains("\u6587\u6863"));
        summary.Cards.Should().Contain(card =>
            card.Title.Contains("\u9700\u8981\u786e\u8ba4\u6765\u6e90") &&
            card.PrimaryText.Contains("AMD"));
        summary.Cards.SelectMany(card => new[] { card.PrimaryText, card.Explanation, card.AgentSuggestion })
            .Should().NotContain(text => text.Contains(@"C:\"));
        summary.TechnicalReportAvailable.Should().BeTrue();
    }

    [Fact]
    public void C_drive_root_cause_cards_offer_only_existing_safe_internal_destinations()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 300L * 1024 * 1024 * 1024,
            FreeBytes = 80L * 1024 * 1024 * 1024,
            TopLevel =
            [
                new() { Name = "Users", Path = @"C:\Users", SizeBytes = 90, Category = UsageCategory.UserProfiles },
                new() { Name = "Program Files", Path = @"C:\Program Files", SizeBytes = 80, Category = UsageCategory.Programs },
                new() { Name = "App data", Path = @"C:\ObservedAppData", SizeBytes = 70, Category = UsageCategory.AppData },
                new() { Name = "Windows Temp", Path = @"C:\Windows\Temp", SizeBytes = 60, Category = UsageCategory.Temp },
                new() { Name = "Windows", Path = @"C:\Windows", SizeBytes = 50, Category = UsageCategory.System },
                new() { Name = "temp", Path = @"C:\temp", SizeBytes = 40, Category = UsageCategory.Temp, IsUnexpectedRoot = true }
            ]
        };

        var cards = CDriveRootCauseSummaryBuilder.Build(result).Cards;

        cards.Single(card => card.PrimaryText.StartsWith("Users", StringComparison.Ordinal)).Should().Match<CDriveRootCauseCard>(card =>
            card.Action == CDriveRootCauseAction.ReviewPersonalStorage
            && card.ActionLabel == "查看大文件候选"
            && card.ActionAutomationId!.StartsWith("CDriveRootCauseAction_ReviewPersonalStorage_", StringComparison.Ordinal));
        cards.Single(card => card.PrimaryText.StartsWith("Program Files", StringComparison.Ordinal)).Should().Match<CDriveRootCauseCard>(card =>
            card.Action == CDriveRootCauseAction.OpenCDriveApps
            && card.ActionLabel == "查看占 C 盘应用"
            && card.ActionAutomationId!.StartsWith("CDriveRootCauseAction_OpenCDriveApps_", StringComparison.Ordinal));
        cards.Single(card => card.PrimaryText.StartsWith("App data", StringComparison.Ordinal)).Action
            .Should().Be(CDriveRootCauseAction.OpenCDriveApps);
        cards.Single(card => card.PrimaryText.StartsWith("Windows Temp", StringComparison.Ordinal)).Should().Match<CDriveRootCauseCard>(card =>
            card.Action == CDriveRootCauseAction.ReviewCleanupRecommendations
            && card.ActionLabel == "查看可安全清理项"
            && card.ActionAutomationId!.StartsWith("CDriveRootCauseAction_ReviewCleanupRecommendations_", StringComparison.Ordinal));
        cards.Single(card => card.PrimaryText.StartsWith("Windows 占用", StringComparison.Ordinal)).HasAction.Should().BeFalse();
        cards.Single(card => card.PrimaryText.StartsWith("temp ", StringComparison.Ordinal)).HasAction.Should().BeFalse();

        var actionIds = cards.Where(card => card.HasAction).Select(card => card.ActionAutomationId).ToArray();
        actionIds.Should().OnlyContain(id => !string.IsNullOrWhiteSpace(id));
        actionIds.Should().OnlyHaveUniqueItems();
        CDriveRootCauseSummaryBuilder.Build(result).Cards
            .Where(card => card.HasAction)
            .Select(card => card.ActionAutomationId)
            .Should().Equal(actionIds);
    }

    [Fact]
    public void C_drive_recycle_bin_card_is_plain_language_and_only_offers_review()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 300L * 1024 * 1024 * 1024,
            FreeBytes = 80L * 1024 * 1024 * 1024,
            BigRocks =
            [
                new() { Name = "Page file", SizeBytes = 8L * 1024 * 1024 * 1024 },
                new() { Name = "Recycle Bin (C:)", SizeBytes = 5L * 1024 * 1024 * 1024 },
                new() { Name = "Shadow storage", SizeBytes = 3L * 1024 * 1024 * 1024 }
            ]
        };

        var cards = CDriveRootCauseSummaryBuilder.Build(result).Cards;
        var recycleBin = cards.Single(card => card.Title == "回收站");

        recycleBin.PrimaryText.Should().Contain("5.0 GB");
        recycleBin.Explanation.Should().Contain("以前删除").And.Contain("清空").And.Contain("不能还原");
        recycleBin.AgentSuggestion.Should().Contain("先打开查看").And.Contain("不会替你清空");
        recycleBin.Action.Should().Be(CDriveRootCauseAction.OpenRecycleBin);
        recycleBin.HasAction.Should().BeTrue();
        recycleBin.ActionLabel.Should().Be("打开回收站查看");
        recycleBin.ActionAutomationId.Should().Be("CDriveRootCauseAction_OpenRecycleBin");

        cards.Where(card => card.Title != "回收站")
            .Should().OnlyContain(card =>
                card.Action == CDriveRootCauseAction.None &&
                !card.HasAction &&
                string.IsNullOrWhiteSpace(card.ActionLabel));
    }

    [Fact]
    public void C_drive_growth_presenter_hides_paths_and_explains_change()
    {
        var growth = new GrowthFinding
        {
            Path = @"C:\$Recycle.Bin",
            OwnerSoftware = "RecycleBin",
            PreviousBytes = 1L * 1024 * 1024 * 1024,
            CurrentBytes = 4L * 1024 * 1024 * 1024,
            Reason = "Grew since previous scan."
        };

        var item = GrowthFindingPresenter.Create(growth);

        item.Title.Should().Contain("\u56de\u6536\u7ad9");
        item.Summary.Should().Contain("+3.0 GB");
        item.AgentSuggestion.Should().Contain("Agent \u5efa\u8bae");
        new[] { item.Title, item.Summary, item.AgentSuggestion, item.Detail }
            .Should().NotContain(text => text.Contains(@"C:\"));
    }

    [Fact]
    public void C_drive_page_chrome_marks_system_drive_as_automatic_and_hides_technical_report_by_default()
    {
        var chrome = CDrivePageChromePresenter.Create(@"C:\");

        chrome.ScanTargetLabel.Should().Be("\u7cfb\u7edf\u76d8 C \u76d8");
        chrome.ScanTargetHint.Should().Contain("\u81ea\u52a8\u8bc6\u522b");
        chrome.IsDrivePathEditable.Should().BeFalse();
        chrome.TechnicalReportToggleText.Should().Be("\u663e\u793a\u6280\u672f\u62a5\u544a");
        chrome.TechnicalReportHint.Should().Contain("\u8fdb\u9636\u68c0\u67e5");
        chrome.IsTechnicalReportVisibleByDefault.Should().BeFalse();
    }

    [Fact]
    public void App_presentation_maps_software_profile_to_icon_tile_and_beginner_drawer()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Publisher = "Tencent",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install\Marvis\Application",
            DisplayIconPath = @"D:\Software\Marvis\Install\Marvis\Application\apk.ico",
            DisplayIconIndex = 3,
            InstalledSizeBytes = 624L * 1024 * 1024,
            DataSizeBytes = 5L * 1024 * 1024 * 1024,
            RunningProcesses = ["Marvis", "MarvisAgent", "MarvisHost", "MarvisMCP"],
            Services = ["MarvisSvr"],
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"""
        };

        var tile = AppPresentationBuilder.CreateTile(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        tile.Name.Should().Be("Marvis");
        tile.IconPath.Should().Be(profile.DisplayIconPath);
        tile.IconIndex.Should().Be(3);
        tile.Status.Should().Be(AppTileStatus.Warning);
        tile.VisibleText.Should().NotContain(@"D:\Software");
        tile.AccessibilityName.Should().Contain("Marvis");
        tile.AccessibilityName.Should().Contain("\u540e\u53f0\u5e38\u9a7b");
        tile.ShortTag.Should().Be("\u540e\u53f0\u5e38\u9a7b");
        tile.AccessibilityName.Should().NotContain(@"D:\Software");

        drawer.InstallLocationSummary.Should().NotContain(@"D:\Software\Marvis\Install\Marvis\Application");
        drawer.ResidencySummary.Should().NotBeNullOrWhiteSpace();
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Observe);
        drawer.AvailableActions.Should().HaveCount(5);
        drawer.TechnicalDetailsHiddenByDefault.Should().BeTrue();
    }

    [Fact]
    public void App_drawer_places_the_category_explanation_before_storage_details()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerCategorySummaryTextBlock\"");
        xaml.IndexOf("DrawerCategorySummaryTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("DrawerLocationTextBlock", StringComparison.Ordinal));
        code.Should().Contain("drawer.CategorySummary");
        code.Should().Contain("DrawerCategorySummaryTextBlock.Text");
    }

    [Fact]
    public void Application_grid_uses_local_icons_with_a_visible_letter_fallback()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var loader = File.ReadAllText(FindRepositoryFile("src", "Css.App", "ApplicationIconLoader.cs"));

        xaml.Should().Contain("<Image Source=\"{Binding IconSource}\"");
        xaml.Should().Contain("Visibility=\"{Binding IconVisibility}\"");
        xaml.Should().Contain("Visibility=\"{Binding IconFallbackVisibility}\"");
        xaml.Should().Contain("Text=\"{Binding IconText}\"");
        xaml.Should().Contain("<Border Width=\"118\" Height=\"152\"");
        xaml.Should().Contain("Text=\"{Binding ShortTag}\" Foreground=\"#6B7280\" FontSize=\"11\" TextAlignment=\"Center\" TextWrapping=\"Wrap\" MaxHeight=\"28\"");
        main.Should().Contain("ApplicationIconLoader.TryLoad(tile.IconPath, tile.IconIndex)");
        main.Should().Contain("iconSource is null ? Visibility.Visible : Visibility.Collapsed");

        loader.Should().Contain("DriveType.Fixed");
        loader.Should().Contain("HasReparsePoint");
        loader.Should().Contain("MaximumRasterBytes");
        loader.Should().Contain("MaximumCacheEntries = 256");
        loader.Should().Contain("file.LastWriteTimeUtc.Ticks");
        loader.Should().Contain("Cache.Clear()");
        loader.Should().Contain("ExtractIconEx");
        loader.Should().Contain("finally");
        loader.Should().Contain("DestroyIcon(largeIcons[0])");
        loader.Should().Contain("source.Freeze()");
        loader.Should().NotContain("Process.Start");
        loader.Should().NotContain("UseShellExecute");
        loader.Should().NotContain("WebClient");
        loader.Should().NotContain("HttpClient");
    }

    [Fact]
    public void App_drawer_actions_use_beginner_friendly_chinese_labels_and_reasons()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Example",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            StartupEntries = ["ExampleStartup"],
            UninstallCommand = @"""C:\Program Files\Example\Uninstall.exe"""
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.AvailableActions.Select(a => a.Label).Should().Equal(
        [
            "\u5378\u8f7d\u5e72\u51c0\u70b9",
            "\u8fc1\u79fb\u5230 D \u76d8",
            "\u6e05\u7406\u7f13\u5b58",
            "\u7ba1\u7406\u81ea\u542f\u52a8",
            "\u6280\u672f\u8be6\u60c5"
        ]);
        drawer.AvailableActions.Select(a => a.Reason).Should().NotContain(reason =>
            reason.Contains("uninstaller", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("Generate", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("startup", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("Technical", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void App_drawer_top_summary_uses_plain_chinese_before_technical_details()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install",
            InstalledSizeBytes = 624L * 1024 * 1024,
            DataSizeBytes = 5L * 1024 * 1024 * 1024,
            RecentGrowthBytes = 512L * 1024 * 1024,
            RunningProcesses = ["Marvis", "MarvisAgent"],
            Services = ["MarvisSvr"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.InstallLocationSummary.Should().Contain("D \u76d8");
        drawer.InstallLocationSummary.Should().NotContain("Installed");
        drawer.SizeSummary.Should().Contain("\u5b89\u88c5");
        drawer.SizeSummary.Should().Contain("\u6700\u8fd1\u589e\u957f");
        drawer.SizeSummary.Should().NotContain("Install size");
        drawer.ResidencySummary.Should().Contain("\u6b63\u5728\u8fd0\u884c");
        drawer.ResidencySummary.Should().Contain("\u540e\u53f0\u670d\u52a1");
        drawer.AgentAdvice.Text.Should().Contain("\u5148\u89c2\u5bdf");
        drawer.AgentAdvice.Text.Should().NotContain("Observe");
    }

    [Fact]
    public void App_drawer_cache_cleanup_preview_is_plain_and_non_executable()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Example\Install",
            CacheSizeBytes = 768L * 1024 * 1024,
            CachePaths =
            [
                @"C:\Users\Me\AppData\Local\Example\Cache",
                @"C:\Users\Me\AppData\Local\Example\Logs"
            ]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.CacheCleanupSummary.Should().Contain("\u53d1\u73b0\u53ef\u68c0\u67e5\u7f13\u5b58");
        drawer.CacheCleanupSummary.Should().Contain("768.0 MB");
        drawer.CacheCleanupCanExecuteDirectly.Should().BeFalse();
        drawer.CacheCleanupPreviewLines.Should().Contain(line => line.Contains("\u53ea\u751f\u6210\u65b9\u6848"));
        drawer.CacheCleanupPreviewLines.Should().Contain(line => line.Contains("\u9694\u79bb\u533a"));
        drawer.CacheCleanupPreviewLines.Should().Contain(line => line.Contains("\u540e\u6094\u836f\u4e2d\u5fc3"));
        drawer.CacheCleanupPreviewLines.Should().NotContain(line =>
            line.Contains(@"C:\Users\Me", StringComparison.OrdinalIgnoreCase)
            || line.Contains("delete", StringComparison.OrdinalIgnoreCase)
            || line.Contains("quarantine", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void App_drawer_cache_cleanup_button_shows_preview_without_execution()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("DrawerActionPreviewTitleTextBlock");
        xaml.Should().Contain("DrawerActionPreviewListBox");
        xaml.Should().Contain("Click=\"PreviewCacheCleanup_Click\"");
        code.Should().Contain("PreviewCacheCleanup_Click");
        code.Should().Contain("AppDrawerActionHostPresenter.ShowCacheCleanup");
        code.Should().Contain("ApplyDrawerActionHost");
        code.Should().NotContain("File.Delete(");
        code.Should().NotContain("Directory.Delete(");
    }

    [Fact]
    public void App_drawer_startup_control_preview_is_plain_and_non_executable()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install",
            Services = ["MarvisSvr"],
            ScheduledTasks = [@"\Marvis Update"],
            RunningProcesses = ["Marvis", "MarvisAgent"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.StartupControlSummary.Should().Contain("\u540e\u53f0\u670d\u52a1");
        drawer.StartupControlSummary.Should().Contain("\u8ba1\u5212\u4efb\u52a1");
        drawer.StartupControlCanExecuteDirectly.Should().BeFalse();
        drawer.StartupControlPreviewLines.Should().Contain(line => line.Contains("\u53ea\u751f\u6210\u65b9\u6848"));
        drawer.StartupControlPreviewLines.Should().Contain(line => line.Contains("\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528"));
        drawer.StartupControlPreviewLines.Should().Contain(line => line.Contains("\u5feb\u7167"));
        drawer.StartupControlPreviewLines.Should().Contain(line => line.Contains("\u540e\u6094\u836f\u4e2d\u5fc3"));
        drawer.StartupControlPreviewLines.Should().NotContain(line =>
            line.Contains("MarvisSvr", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Marvis Update", StringComparison.OrdinalIgnoreCase));
        drawer.AvailableActions.Should().Contain(action =>
            action.Label == "管理自启动" && action.IsEnabled);
    }

    [Fact]
    public void App_drawer_wpf_uses_the_same_manage_startup_action_key_as_the_presenter()
    {
        var code = File.ReadAllText(
            FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var showDrawer = ExtractMethod(
            code,
            "private void ShowAppDrawer",
            "private static void ApplyActionState");
        var applyAction = ExtractMethod(
            code,
            "private static void ApplyActionState",
            "private void RefreshAppCatalog");

        showDrawer.Should().Contain(
            "ApplyActionState(DrawerDisableStartupButton, drawer, AppActionKind.StartupControl)");
        showDrawer.Should().Contain("AppActionKind.Uninstall");
        showDrawer.Should().Contain("AppActionKind.Migration");
        showDrawer.Should().Contain("AppActionKind.CacheCleanup");
        showDrawer.Should().NotContain("管理自启动")
            .And.NotContain("关闭自启动");
        applyAction.Should().Contain("a.Kind == kind");
        applyAction.Should().NotContain("a.Label ==");
    }

    [Fact]
    public void App_startup_plan_classifies_system_tool_as_keep_without_raw_identifiers()
    {
        var profile = new SoftwareProfile
        {
            Name = "Driver Center",
            Category = SoftwareCategory.SystemTool,
            Services = ["DriverCenterService"],
            ScheduledTasks = [@"\Driver Center Update"],
            RunningProcesses = ["DriverAgent"]
        };

        var preview = AppStartupControlPreviewPresenter.Create(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var host = AppDrawerActionHostPresenter.ShowStartupControl(drawer);

        preview.CanExecuteDirectly.Should().BeFalse();
        preview.Summary.Should().Contain("\u5efa\u8bae\u4fdd\u7559");
        preview.Lines.Should().Contain(line => line.Contains("\u5efa\u8bae\u4fdd\u7559"));
        preview.Lines.Should().Contain(line => line.Contains("\u7cfb\u7edf") || line.Contains("\u9a71\u52a8"));
        preview.Lines.Should().Contain(line => line.Contains("\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528"));
        preview.Lines.Should().NotContain(line =>
            line.Contains("DriverCenterService", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Driver Center Update", StringComparison.OrdinalIgnoreCase)
            || line.Contains("DriverAgent", StringComparison.OrdinalIgnoreCase));
        host.AgentTakeaway.Should().Contain("\u5efa\u8bae\u4fdd\u7559");
        host.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void App_startup_plan_classifies_running_only_app_as_observe()
    {
        var profile = new SoftwareProfile
        {
            Name = "Chat",
            Category = SoftwareCategory.Normal,
            RunningProcesses = ["ChatHelper"]
        };

        var preview = AppStartupControlPreviewPresenter.Create(profile);

        preview.CanExecuteDirectly.Should().BeFalse();
        preview.Summary.Should().Contain("\u5148\u89c2\u5bdf");
        preview.Lines.Should().Contain(line => line.Contains("\u6b63\u5728\u8fd0\u884c"));
        preview.Lines.Should().Contain(line => line.Contains("\u4e0d\u4ee3\u8868\u53ef\u4ee5\u7981\u7528"));
        preview.Lines.Should().Contain(line => line.Contains("\u91cd\u65b0\u626b\u63cf") || line.Contains("\u7ee7\u7eed\u89c2\u5bdf"));
        preview.Lines.Should().NotContain(line => line.Contains("ChatHelper", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void App_startup_plan_classifies_normal_startup_as_future_disable_plan()
    {
        var profile = new SoftwareProfile
        {
            Name = "Cloud Sync",
            Category = SoftwareCategory.Normal,
            StartupEntries = ["Cloud Sync Startup"],
            Services = ["CloudSyncService"],
            ScheduledTasks = [@"\Cloud Sync Update"]
        };

        var preview = AppStartupControlPreviewPresenter.Create(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var host = AppDrawerActionHostPresenter.ShowStartupControl(drawer);

        preview.CanExecuteDirectly.Should().BeFalse();
        preview.Summary.Should().Contain("\u672a\u6765\u53ef\u7981\u7528");
        preview.Lines.Should().Contain(line => line.Contains("\u672a\u6765\u53ef\u7981\u7528"));
        preview.Lines.Should().Contain(line => line.Contains("\u5feb\u7167"));
        preview.Lines.Should().Contain(line => line.Contains("\u56de\u6eda"));
        preview.Lines.Should().NotContain(line =>
            line.Contains("Cloud Sync Startup", StringComparison.OrdinalIgnoreCase)
            || line.Contains("CloudSyncService", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Cloud Sync Update", StringComparison.OrdinalIgnoreCase));
        host.AgentTakeaway.Should().Contain("\u672a\u6765\u53ef\u7981\u7528");
        host.NextStepText.Should().Contain("\u5feb\u7167");
        host.SafetyText.Should().Contain("\u56de\u6eda");
        host.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void App_drawer_disable_startup_button_shows_preview_without_execution()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("DrawerActionPreviewTitleTextBlock");
        xaml.Should().Contain("DrawerActionPreviewListBox");
        xaml.Should().Contain("Click=\"PreviewStartupControl_Click\"");
        code.Should().Contain("PreviewStartupControl_Click");
        code.Should().Contain("AppDrawerActionHostPresenter.ShowStartupControl");
        code.Should().Contain("ApplyDrawerActionHost");
        code.Should().NotContain("Registry.SetValue");
        code.Should().NotContain("Set-Service");
    }

    [Fact]
    public void App_drawer_action_preview_presenter_switches_panels_without_execution()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Example\Install",
            CacheSizeBytes = 256L * 1024 * 1024,
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            Services = ["ExampleBackground"]
        };
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        var cacheState = AppDrawerActionPreviewPresenter.ShowCacheCleanup(drawer);

        cacheState.CachePreviewVisible.Should().BeTrue();
        cacheState.StartupPreviewVisible.Should().BeFalse();
        cacheState.Summary.Should().Be(drawer.CacheCleanupSummary);
        cacheState.Lines.Should().Equal(drawer.CacheCleanupPreviewLines);
        cacheState.CanExecuteDirectly.Should().BeFalse();
        cacheState.StatusText.Should().Contain("\u6ca1\u6709\u6267\u884c\u6e05\u7406");

        var startupState = AppDrawerActionPreviewPresenter.ShowStartupControl(drawer);

        startupState.CachePreviewVisible.Should().BeFalse();
        startupState.StartupPreviewVisible.Should().BeTrue();
        startupState.Summary.Should().Be(drawer.StartupControlSummary);
        startupState.Lines.Should().Equal(drawer.StartupControlPreviewLines);
        startupState.CanExecuteDirectly.Should().BeFalse();
        startupState.StatusText.Should().Contain("\u6ca1\u6709\u7981\u7528");
    }

    [Fact]
    public void App_drawer_action_preview_presenter_handles_no_selection()
    {
        var cacheState = AppDrawerActionPreviewPresenter.NoSelectionForCacheCleanup();

        cacheState.CachePreviewVisible.Should().BeFalse();
        cacheState.StartupPreviewVisible.Should().BeFalse();
        cacheState.Lines.Should().BeEmpty();
        cacheState.CanExecuteDirectly.Should().BeFalse();
        cacheState.StatusText.Should().Contain("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528");
        cacheState.StatusText.Should().Contain("\u7f13\u5b58\u6e05\u7406\u65b9\u6848");

        var startupState = AppDrawerActionPreviewPresenter.NoSelectionForStartupControl();

        startupState.CachePreviewVisible.Should().BeFalse();
        startupState.StartupPreviewVisible.Should().BeFalse();
        startupState.Lines.Should().BeEmpty();
        startupState.CanExecuteDirectly.Should().BeFalse();
        startupState.StatusText.Should().Contain("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528");
        startupState.StatusText.Should().Contain("\u81ea\u542f\u52a8\u7ba1\u63a7\u65b9\u6848");
    }

    [Fact]
    public void App_drawer_shared_action_preview_host_replaces_stacked_action_sections()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Example",
            UninstallCommand = @"""C:\Program Files\Example\uninstall.exe""",
            CacheSizeBytes = 256L * 1024 * 1024,
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            Services = ["ExampleBackground"]
        };
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        var uninstall = AppDrawerActionHostPresenter.ShowUninstall(drawer);
        uninstall.IsVisible.Should().BeTrue();
        uninstall.Title.Should().Be("\u5378\u8f7d\u65b9\u6848\u9884\u89c8");
        uninstall.Lines.Should().Equal(drawer.UninstallPreviewLines);
        uninstall.CanExecuteDirectly.Should().BeFalse();
        uninstall.StatusText.Should().Contain("\u6ca1\u6709\u8fd0\u884c\u5378\u8f7d\u5668");

        var migration = AppDrawerActionHostPresenter.ShowMigration(drawer);
        migration.IsVisible.Should().BeTrue();
        migration.Title.Should().Be("\u8fc1\u79fb\u65b9\u6848\u9884\u89c8");
        migration.Summary.Should().Be(drawer.MigrationSummary);
        migration.Lines.Should().Equal(drawer.MigrationPreviewLines);
        migration.CanExecuteDirectly.Should().BeFalse();
        migration.StatusText.Should().Contain("\u6ca1\u6709\u79fb\u52a8\u6587\u4ef6");

        var collapsed = AppDrawerActionHostPresenter.Collapsed();
        collapsed.IsVisible.Should().BeFalse();
        collapsed.Lines.Should().BeEmpty();

        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("DrawerActionPreviewPanel");
        xaml.Should().Contain("DrawerActionPreviewTitleTextBlock");
        xaml.Should().Contain("DrawerActionPreviewListBox");
        code.Should().Contain("ApplyDrawerActionHost");
        code.Should().Contain("AppDrawerActionHostPresenter.ShowUninstall");
        code.Should().Contain("AppDrawerActionHostPresenter.ShowMigration");
        code.Should().Contain("AppDrawerActionHostPresenter.ShowCacheCleanup");
        code.Should().Contain("AppDrawerActionHostPresenter.ShowStartupControl");
        code.Should().Contain("AppDrawerActionHostPresenter.Collapsed");
        code.Should().NotContain("DrawerUninstallPreviewListBox.ItemsSource = drawer.UninstallPreviewLines;");
        code.Should().NotContain("DrawerMigrationPreviewListBox.ItemsSource = drawer.MigrationPreviewLines;");
    }

    [Fact]
    public void App_drawer_action_host_presents_agent_takeaway_next_step_and_safety_text()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Example",
            UninstallCommand = @"""C:\Program Files\Example\uninstall.exe""",
            CacheSizeBytes = 512L * 1024 * 1024,
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            StartupEntries = ["ExampleStartup"],
            Services = ["ExampleBackground"]
        };
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        var states = new[]
        {
            AppDrawerActionHostPresenter.ShowUninstall(drawer),
            AppDrawerActionHostPresenter.ShowMigration(drawer),
            AppDrawerActionHostPresenter.ShowCacheCleanup(drawer),
            AppDrawerActionHostPresenter.ShowStartupControl(drawer)
        };

        states.Should().OnlyContain(state => state.IsVisible);
        states.Should().OnlyContain(state => !state.CanExecuteDirectly);
        states.Should().OnlyContain(state => state.AgentTakeaway.StartsWith("Agent \u5224\u65ad\uff1a"));
        states.Should().OnlyContain(state => state.NextStepText.StartsWith("\u4e0b\u4e00\u6b65\uff1a"));
        states.Should().OnlyContain(state => state.SafetyText.Contains("\u4e0d\u4f1a\u76f4\u63a5"));
        states.Should().Contain(state => state.AgentTakeaway.Contains("\u7f13\u5b58"));
        states.Should().Contain(state => state.AgentTakeaway.Contains("\u81ea\u542f\u52a8"));
        states.Should().Contain(state => state.NextStepText.Contains("\u5b98\u65b9\u5378\u8f7d"));
        states.Should().Contain(state => state.NextStepText.Contains("D \u76d8"));
    }

    [Fact]
    public void App_drawer_action_host_handles_uninstall_and_migration_no_selection()
    {
        var uninstall = AppDrawerActionHostPresenter.NoSelectionForUninstall();

        uninstall.IsVisible.Should().BeFalse();
        uninstall.Lines.Should().BeEmpty();
        uninstall.CanExecuteDirectly.Should().BeFalse();
        uninstall.StatusText.Should().Contain("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528");
        uninstall.StatusText.Should().Contain("\u5378\u8f7d\u65b9\u6848");

        var migration = AppDrawerActionHostPresenter.NoSelectionForMigration();

        migration.IsVisible.Should().BeFalse();
        migration.Lines.Should().BeEmpty();
        migration.CanExecuteDirectly.Should().BeFalse();
        migration.StatusText.Should().Contain("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528");
        migration.StatusText.Should().Contain("\u8fc1\u79fb\u65b9\u6848");

        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        code.Should().Contain("AppDrawerActionHostPresenter.NoSelectionForUninstall");
        code.Should().Contain("AppDrawerActionHostPresenter.NoSelectionForMigration");
    }

    [Fact]
    public void App_drawer_action_host_no_selection_wiring_matches_each_button()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        var uninstallHandler = ExtractMethod(code, "PreviewUninstall_Click", "PreviewMigration_Click");
        uninstallHandler.Should().Contain("AppDrawerActionHostPresenter.NoSelectionForUninstall");
        uninstallHandler.Should().NotContain("NoSelectionForCacheCleanup");

        var migrationHandler = ExtractMethod(code, "PreviewMigration_Click", "PreviewCacheCleanup_Click");
        migrationHandler.Should().Contain("AppDrawerActionHostPresenter.NoSelectionForMigration");
        migrationHandler.Should().NotContain("NoSelectionForUninstall");

        var cacheHandler = ExtractMethod(code, "PreviewCacheCleanup_Click", "PreviewStartupControl_Click");
        cacheHandler.Should().Contain("AppDrawerActionHostPresenter.NoSelectionForCacheCleanup");
        cacheHandler.Should().NotContain("NoSelectionForUninstall");

        var startupHandler = ExtractMethod(code, "PreviewStartupControl_Click", "private void ApplyDrawerActionHost");
        startupHandler.Should().Contain("AppDrawerActionHostPresenter.NoSelectionForStartupControl");
        startupHandler.Should().NotContain("NoSelectionForUninstall");
    }

    [Fact]
    public void App_drawer_no_selection_status_comes_from_action_host_presenter()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var uninstallHandler = ExtractMethod(code, "PreviewUninstall_Click", "PreviewMigration_Click");
        var noSelectionBranch = uninstallHandler[..uninstallHandler.IndexOf("var drawer", StringComparison.Ordinal)];

        noSelectionBranch.Should().Contain("ApplyDrawerActionHost(AppDrawerActionHostPresenter.NoSelectionForUninstall())");
        noSelectionBranch.Should().NotContain("StatusTextBlock.Text =");
    }

    [Fact]
    public void App_drawer_technical_details_toggle_is_tested_and_changes_button_text()
    {
        var showState = AppDrawerTechnicalDetailsPresenter.Toggle(isCurrentlyVisible: false);

        showState.IsVisible.Should().BeTrue();
        showState.ButtonText.Should().Be("\u9690\u85cf\u6280\u672f\u8be6\u60c5");
        showState.StatusText.Should().Contain("\u6280\u672f\u8be6\u60c5");

        var hideState = AppDrawerTechnicalDetailsPresenter.Toggle(isCurrentlyVisible: true);

        hideState.IsVisible.Should().BeFalse();
        hideState.ButtonText.Should().Be("\u67e5\u770b\u6280\u672f\u8be6\u60c5");

        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("Click=\"ToggleTechnicalDetails_Click\"");
        code.Should().Contain("AppDrawerTechnicalDetailsPresenter.Toggle");
        code.Should().Contain("ApplyDrawerTechnicalDetailsState");
    }

    [Fact]
    public void Software_scanner_feeds_appdata_roots_to_profile_builder_for_cache_previews()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.Scanner", "Software", "SoftwareInventoryScanner.cs"));

        code.Should().Contain("userDataRoots: GetUserDataRoots()");
        code.Should().Contain("pathExists: Directory.Exists");
        code.Should().Contain("cacheSizeResolver: EstimateDirectorySize");
    }

    [Fact]
    public void App_tile_status_labels_are_localized_for_beginner_grid()
    {
        var cDriveApp = new SoftwareProfile
        {
            Name = "WeChat",
            InstallPath = @"C:\Program Files\Tencent\WeChat",
            CDriveWritePaths = [@"C:\Users\Me\Documents\WeChat Files"]
        };
        var normalApp = new SoftwareProfile
        {
            Name = "D App",
            InstallPath = @"D:\Software\DApp\Install"
        };

        var cDriveTile = AppPresentationBuilder.CreateTile(cDriveApp);
        var normalTile = AppPresentationBuilder.CreateTile(normalApp);

        cDriveTile.ShortTag.Should().Be("主程序在 C 盘");
        cDriveTile.AccessibilityName.Should().Be("WeChat, 主程序在 C 盘");
        normalTile.ShortTag.Should().Be("\u6b63\u5e38");
        normalTile.AccessibilityName.Should().Be("D App, \u6b63\u5e38");
    }

    [Fact]
    public void App_catalog_filters_searches_and_sorts_beginner_tiles()
    {
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "WeChat",
                Publisher = "Tencent",
                Category = SoftwareCategory.Normal,
                InstallPath = @"C:\Program Files\Tencent\WeChat",
                CDriveWritePaths = [@"C:\Users\Me\Documents\WeChat Files"],
                InstalledSizeBytes = 800L * 1024 * 1024,
                InstallDate = new DateOnly(2026, 6, 1),
                UninstallCommand = "wechat-uninstall.exe"
            },
            new SoftwareProfile
            {
                Name = "Marvis",
                Publisher = "Tencent",
                Category = SoftwareCategory.Ai,
                InstallPath = @"D:\Software\Marvis",
                RunningProcesses = ["Marvis", "MarvisAgent"],
                Services = ["MarvisSvr"],
                InstallDate = new DateOnly(2026, 7, 20),
                DataSizeBytes = 5L * 1024 * 1024 * 1024
            },
            new SoftwareProfile
            {
                Name = "Visual Studio",
                Category = SoftwareCategory.DevelopmentTool,
                InstallPath = @"D:\Development\VisualStudio",
                InstalledSizeBytes = 14L * 1024 * 1024 * 1024
            }
        };

        var cDrive = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.CDrive,
                SearchText = "wechat",
                Sort = AppCatalogSort.Risk
            });
        var resident = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.Resident,
                Sort = AppCatalogSort.Name
            });
        var bySize = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.All,
                Sort = AppCatalogSort.Size
            });
        var byRecentInstall = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.All,
                Sort = AppCatalogSort.RecentInstall
            });
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));

        cDrive.Select(p => p.Name).Should().Equal("WeChat");
        resident.Select(p => p.Name).Should().Equal("Marvis");
        bySize.Select(p => p.Name).Should().StartWith("Visual Studio", "Marvis");
        byRecentInstall.Select(p => p.Name).Should().Equal("Marvis", "WeChat", "Visual Studio");
        AppPresentationBuilder.CreateDrawer(profiles[1]).TechnicalDetails
            .Should().Contain("Install date: 2026-07-20");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AppSortComboBox\"")
            .And.Contain("Content=\"按最近安装\" Tag=\"RecentInstall\"");
    }

    [Fact]
    public void App_catalog_calls_the_normal_fallback_group_ordinary_apps_not_office_study()
    {
        var normal = new SoftwareProfile
        {
            Name = "Download Utility",
            Category = SoftwareCategory.Normal
        };
        var ai = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai
        };

        var result = AppCatalogPresenter.Apply(
            [normal, ai],
            new AppCatalogQuery { Filter = AppCatalogFilter.NormalApplications });
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        result.Should().ContainSingle().Which.Should().BeSameAs(normal);
        xaml.Should().Contain("x:Name=\"NormalAppsFilterButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"NormalAppsFilterButton\"")
            .And.Contain("Content=\"普通应用\"")
            .And.Contain("Tag=\"NormalApplications\"")
            .And.NotContain("办公学习")
            .And.NotContain("OfficeStudy");
        code.Should().Contain("NormalAppsFilterButton")
            .And.NotContain("OfficeAppsFilterButton");
    }

    [Fact]
    public void App_catalog_ignores_localized_search_placeholder()
    {
        var profiles = new[]
        {
            new SoftwareProfile { Name = "WeChat", Publisher = "Tencent" },
            new SoftwareProfile { Name = "Marvis", Publisher = "Tencent" }
        };

        var result = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.All,
                SearchText = "搜索应用",
                Sort = AppCatalogSort.Name
            });

        result.Select(p => p.Name).Should().BeEquivalentTo(["WeChat", "Marvis"]);
    }

    [Fact]
    public void App_drawer_contains_uninstall_preview_without_executing_uninstall()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example",
            UninstallCommand = @"""D:\Software\Example\uninstall.exe""",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            Services = ["ExampleService"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.UninstallPreviewLines.Should().NotBeEmpty();
        drawer.UninstallPreviewLines.Should().Contain(line => line.Contains("\u53ea\u9884\u89c8", StringComparison.OrdinalIgnoreCase));
        drawer.UninstallPreviewLines.Should().Contain(line => line.Contains("\u5b98\u65b9\u5378\u8f7d\u5668", StringComparison.OrdinalIgnoreCase));
        drawer.UninstallPreviewLines.Should().NotContain(line =>
            line.Contains("Uninstall preview", StringComparison.OrdinalIgnoreCase)
            || line.Contains("First step", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Official uninstaller", StringComparison.OrdinalIgnoreCase)
            || line.Contains("quarantine", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Uninstall_workflow_guide_is_shared_by_drawer_and_safety_window()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Publisher = "Tencent",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0",
            CachePaths = [@"C:\Users\Me\AppData\Local\Marvis\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Marvis\Logs"],
            RunningProcesses = ["Marvis", "MarvisAgent"],
            Services = ["MarvisSvr"],
            ScheduledTasks = [@"\Marvis Update"]
        };

        var guide = UninstallWorkflowGuidePresenter.Create(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var window = UninstallPlanPresentationBuilder.Create(profile);

        guide.Steps.Should().ContainInOrder(
        [
            "\u5148\u770b\u6e05\u5b98\u65b9\u5378\u8f7d\u5668\uff1a\u786e\u8ba4\u5b83\u6765\u81ea\u8fd9\u4e2a\u8f6f\u4ef6\uff0c\u4e0d\u4ece\u62bd\u5c49\u76f4\u63a5\u8fd0\u884c\u3002",
            "\u5148\u5173\u95ed\u8f6f\u4ef6\uff1a\u5173\u95ed\u7a97\u53e3\u3001\u6258\u76d8\u548c\u76f8\u5173\u540e\u53f0\u8fdb\u7a0b\u3002",
            "\u4ee5\u540e\u53ea\u6709\u5728\u6700\u7ec8\u786e\u8ba4\u540e\uff0c\u624d\u5141\u8bb8\u8bf7\u6c42\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\u5668\u3002",
            "\u5378\u8f7d\u5b8c\u6210\u540e\u56de\u5230\u201c\u5378\u8f7d\u540e\u68c0\u67e5\u6b8b\u7559\u201d\uff0c\u91cd\u65b0\u626b\u63cf\u662f\u5426\u8fd8\u5728\u3002",
            "\u53ea\u628a\u4f4e\u98ce\u9669\u7f13\u5b58/\u65e5\u5fd7\u6b8b\u7559\u79fb\u52a8\u5230\u9694\u79bb\u533a\uff0c\u65b9\u4fbf\u540e\u6094\u836f\u4e2d\u5fc3\u8fd8\u539f\u3002",
            "\u4e2d/\u9ad8\u98ce\u9669\u6b8b\u7559\u53ea\u89e3\u91ca\u548c\u6807\u8bb0\uff0c\u4e0d\u4f1a\u81ea\u52a8\u5904\u7406\u3002"
        ]);
        drawer.UninstallPreviewLines.Should().Equal(guide.DrawerLines);
        window.WorkflowGuide.Steps.Should().Equal(guide.Steps);
        window.WorkflowGuide.SafetyLine.Should().Be(guide.SafetyLine);
        window.CanRunOfficialUninstaller.Should().BeFalse();
        window.OfficialConfirmation.CanRunOfficialUninstaller.Should().BeFalse();
        window.OfficialConfirmation.PreflightChecklist.CanRequestExecution.Should().BeFalse();
    }

    [Fact]
    public void App_drawer_shows_migration_preview_for_c_drive_app_without_executing_migration()
    {
        var profile = new SoftwareProfile
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
            CachePaths = [@"C:\Users\Me\.ollama\models"],
            Services = ["OllamaService"],
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\Programs\Ollama", @"C:\Users\Me\.ollama\models"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.MigrationSummary.Should().Contain("先关闭软件和相关后台组件");
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains(@"D:\Agent\Ollama\Install"));
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains("不会从应用抽屉移动任何文件"));
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains("快照和回滚清单"));
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains("原 C 盘位置不再继续产生新内容"));
        drawer.AgentAdvice.Text.Should().Contain("\u8fc1\u79fb\u65b9\u6848");
    }

    [Fact]
    public void App_drawer_marks_d_drive_app_as_already_reasonable_for_migration()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install",
            Services = ["MarvisSvr"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.MigrationSummary.Should().Contain("已经在 D 盘");
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains("不需要迁移"));
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Observe);
    }

    [Fact]
    public void App_drawer_limits_migration_to_cache_when_main_install_path_is_unknown()
    {
        var profile = new SoftwareProfile
        {
            Name = "Model Cache Tool",
            Category = SoftwareCategory.Ai,
            CachePaths = [@"C:\Users\Me\.models"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.MigrationSummary.Should().Contain("只建议迁移缓存");
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains("只迁移已识别的缓存、模型或下载目录"));
    }

    [Fact]
    public void App_drawer_blocks_migration_for_system_tools()
    {
        var profile = new SoftwareProfile
        {
            Name = "Driver Runtime",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"C:\Program Files\Driver Runtime",
            Services = ["DriverRuntimeService"]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.MigrationSummary.Should().Contain("不建议迁移");
        drawer.MigrationPreviewLines.Should().Contain(line => line.Contains("不要迁移这个应用"));
        drawer.AvailableActions.Should().Contain(a =>
            !a.IsEnabled && a.Reason.Contains("\u7cfb\u7edf\u7ec4\u4ef6\u4e0d\u5efa\u8bae\u8fc1\u79fb"));
    }

    [Fact]
    public void Migration_plan_presentation_is_preview_only_and_requires_snapshot_rollback_and_monitoring()
    {
        var profile = new SoftwareProfile
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
            CachePaths = [@"C:\Users\Me\.ollama\models"],
            Services = ["OllamaService"],
            RunningProcesses = ["ollama"],
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\Programs\Ollama", @"C:\Users\Me\.ollama\models"]
        };

        var view = MigrationPlanPresentationBuilder.Create(profile);

        view.Title.Should().Contain("Ollama");
        view.Summary.Should().Contain("\u9884\u89c8");
        view.CanRunMigration.Should().BeFalse();
        view.RequiresSnapshot.Should().BeTrue();
        view.DestinationLine.Should().Contain(@"D:\Agent\Ollama\Install");
        view.BlockingReasons.Should().Contain(reason => reason.Contains("\u5feb\u7167"));
        view.BlockingReasons.Should().Contain(reason => reason.Contains("\u4e0d\u6267\u884c\u8fc1\u79fb"));
        view.Sections.Should().Contain(section => section.Title.Contains("\u8fc1\u79fb\u524d\u68c0\u67e5"));
        view.Sections.Should().Contain(section => section.Title.Contains("\u56de\u6eda\u65b9\u6848"));
        view.Sections.Should().Contain(section => section.Title.Contains("\u8fc1\u79fb\u540e\u89c2\u5bdf"));
        view.FinalReminder.Should().Contain("\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6");
    }

    [Fact]
    public void Migration_plan_presentation_body_uses_plain_chinese_while_staying_preview_only()
    {
        var profile = new SoftwareProfile
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
            CachePaths = [@"C:\Users\Me\.ollama\models"],
            Services = ["OllamaService"],
            RunningProcesses = ["ollama"],
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\Programs\Ollama", @"C:\Users\Me\.ollama\models"]
        };

        var view = MigrationPlanPresentationBuilder.Create(profile);
        var visibleCopy = new[]
            {
                view.Title,
                view.Summary,
                view.SafetyBanner,
                view.DestinationLine,
                view.ScoreLine,
                view.RollbackManifestLine,
                view.DestinationSpaceLine,
                view.PrimaryActionText,
                view.FinalReminder,
                view.ReadinessChecklist.PrimaryActionText,
                view.ReadinessChecklist.NextActionText
            }
            .Concat(view.BlockingReasons)
            .Concat(view.Sections.Select(section => section.Title))
            .Concat(view.Sections.Select(section => section.StatusLabel))
            .Concat(view.Sections.Select(section => section.Detail))
            .Concat(view.Sections.SelectMany(section => section.Items))
            .Concat(view.ReadinessChecklist.Steps.Select(step => step.Title))
            .Concat(view.ReadinessChecklist.Steps.Select(step => step.StatusLabel))
            .Concat(view.ReadinessChecklist.Steps.Select(step => step.Detail))
            .ToList();

        view.CanRunMigration.Should().BeFalse();
        view.Title.Should().Contain("\u8fc1\u79fb\u65b9\u6848");
        view.Summary.Should().Contain("\u9884\u89c8");
        view.SafetyBanner.Should().Contain("\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6");
        view.DestinationLine.Should().Contain("\u5efa\u8bae\u76ee\u6807\u4f4d\u7f6e");
        view.RollbackManifestLine.Should().Contain("\u56de\u6eda\u6e05\u5355");
        view.DestinationSpaceLine.Should().Contain("\u76ee\u6807\u76d8\u7a7a\u95f4");
        view.PrimaryActionText.Should().Be("\u5148\u751f\u6210\u56de\u6eda\u8bc1\u636e");
        view.FinalReminder.Should().Contain("\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6");
        view.Sections.Select(section => section.Title).Should().Contain(
        [
            "\u8fc1\u79fb\u524d\u68c0\u67e5",
            "\u8fc1\u79fb\u6b65\u9aa4\u9884\u89c8",
            "\u56de\u6eda\u65b9\u6848",
            "\u8fc1\u79fb\u540e\u89c2\u5bdf"
        ]);
        view.ReadinessChecklist.PrimaryActionText.Should().Be("\u5148\u751f\u6210\u56de\u6eda\u8bc1\u636e");
        view.ReadinessChecklist.Steps.Select(step => step.Title).Should().Contain(
        [
            "\u5b89\u5168\u6267\u884c\u5165\u53e3",
            "\u8fc1\u79fb\u8bc4\u5206",
            "\u8fc1\u79fb\u524d\u5feb\u7167",
            "\u786e\u8ba4\u65b9\u6848",
            "\u5173\u95ed\u5e94\u7528",
            "\u56de\u6eda\u6e05\u5355",
            "\u76ee\u6807\u76d8\u7a7a\u95f4",
            "\u8fc1\u79fb\u540e\u89c2\u5bdf"
        ]);
        visibleCopy.Should().NotContain(text =>
            text.Contains("Preview only", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Suggested destination", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Rollback plan", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Monitoring after migration", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Execution feature", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Migration_plan_presentation_treats_d_drive_app_as_monitor_only()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Category = SoftwareCategory.Ai,
            InstallPath = @"D:\Software\Marvis\Install",
            Services = ["MarvisSvr"],
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\Marvis"]
        };

        var view = MigrationPlanPresentationBuilder.Create(profile);

        view.IsAlreadyReasonable.Should().BeTrue();
        view.CanRunMigration.Should().BeFalse();
        view.Summary.Should().Contain("\u5df2\u7ecf\u5728 D \u76d8");
        view.Sections.Should().Contain(section =>
            section.Title.Contains("\u89c2\u5bdf") &&
            section.Items.Any(item => item.Contains("C \u76d8")));
    }

    [Fact]
    public void Migration_plan_presentation_blocks_system_tools()
    {
        var profile = new SoftwareProfile
        {
            Name = "Driver Runtime",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"C:\Program Files\Driver Runtime",
            Services = ["DriverRuntimeService"]
        };

        var view = MigrationPlanPresentationBuilder.Create(profile);

        view.IsRecommended.Should().BeFalse();
        view.CanRunMigration.Should().BeFalse();
        view.ScoreLine.Should().Contain("\u4e0d\u5efa\u8bae\u8fc1\u79fb");
        view.BlockingReasons.Should().Contain(reason => reason.Contains("\u7cfb\u7edf\u5de5\u5177"));
        view.Sections.Should().Contain(section => section.Title.Contains("\u4e0d\u8981\u8fc1\u79fb"));
    }

    [Fact]
    public void Migration_execution_gate_stays_disabled_by_default()
    {
        var profile = CreateCDriveMigrationProfile();
        var plan = MigrationPlanner.CreatePlan(profile, @"D:\Agent\Ollama\Install", snapshotAvailable: false);
        var readiness = new MigrationExecutionReadiness
        {
            FeatureEnabled = false,
            SnapshotId = "snapshot-1",
            UserConfirmedPlan = true,
            UserConfirmedAppsClosed = true,
            RollbackManifestPath = @"D:\OMNIX\Rollback\ollama.json",
            RollbackManifestSha256 = new string('A', 64),
            DestinationAvailableBytes = 200L * 1024 * 1024 * 1024,
            UserConfirmedPostMigrationMonitoring = true
        };

        var gate = MigrationExecutionGate.Evaluate(profile, plan, readiness, _ => true);

        gate.CanRequestExecution.Should().BeFalse();
        gate.Operation.Should().BeNull();
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("not enabled", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Migration_execution_gate_requires_machine_verified_evidence_before_final_consent()
    {
        var profile = CreateCDriveMigrationProfile();
        var plan = MigrationPlanner.CreatePlan(profile, @"D:\Agent\Ollama\Install", snapshotAvailable: false);
        var readiness = new MigrationExecutionReadiness
        {
            FeatureEnabled = true,
            UserConfirmedPlan = false,
            UserConfirmedAppsClosed = false,
            DestinationAvailableBytes = 1024,
            UserConfirmedPostMigrationMonitoring = false
        };

        var gate = MigrationExecutionGate.Evaluate(profile, plan, readiness, _ => false);

        gate.CanRequestExecution.Should().BeFalse();
        gate.Operation.Should().BeNull();
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("snapshot", StringComparison.OrdinalIgnoreCase));
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("rollback", StringComparison.OrdinalIgnoreCase));
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("space", StringComparison.OrdinalIgnoreCase));
        gate.BlockingReasons.Should().NotContain(reason => reason.Contains("confirm", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Migration_execution_gate_creates_unconfirmed_operation_after_verified_evidence()
    {
        var profile = CreateCDriveMigrationProfile();
        var plan = MigrationPlanner.CreatePlan(profile, @"D:\Agent\Ollama\Install", snapshotAvailable: false);
        var readiness = new MigrationExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-migrate-1",
            RollbackManifestPath = @"D:\OMNIX\Rollback\ollama.json",
            RollbackManifestSha256 = new string('B', 64),
            SnapshotEvidencePath = @"D:\OMNIX\Rollback\ollama.snapshot.json",
            SnapshotEvidenceSha256 = new string('C', 64),
            DestinationAvailableBytes = 200L * 1024 * 1024 * 1024
        };

        var gate = MigrationExecutionGate.Evaluate(profile, plan, readiness, _ => true);

        gate.CanRequestExecution.Should().BeTrue();
        gate.BlockingReasons.Should().BeEmpty();
        gate.Operation.Should().NotBeNull();
        gate.Operation!.Kind.Should().Be("migration.execute");
        gate.Operation.Risk.Should().Be(RiskLevel.High);
        gate.Operation.IsDestructive.Should().BeTrue();
        gate.Operation.RequiresSnapshot.Should().BeTrue();
        gate.Operation.SnapshotId.Should().Be("snapshot-migrate-1");
        gate.Operation.ConfirmationAccepted.Should().BeFalse();
        gate.Operation.AffectedPaths.Should().Contain(@"C:\Users\Me\AppData\Local\Programs\Ollama");
        gate.Operation.AffectedPaths.Should().Contain(@"C:\Users\Me\.ollama\models");
        gate.Operation.AffectedServices.Should().Contain("OllamaService");
        gate.Operation.Arguments["destinationRoot"].Should().Be(@"D:\Agent\Ollama\Install");
        gate.Operation.Arguments["rollbackManifestPath"].Should().Be(@"D:\OMNIX\Rollback\ollama.json");
        gate.Operation.Arguments["rollbackManifestSha256"].Should().Be(new string('B', 64));
        gate.Operation.Arguments["snapshotEvidencePath"].Should()
            .Be(@"D:\OMNIX\Rollback\ollama.snapshot.json");
        gate.Operation.Arguments["snapshotEvidenceSha256"].Should().Be(new string('C', 64));
    }

    [Fact]
    public void Migration_plan_presentation_exposes_readiness_checklist()
    {
        var profile = CreateCDriveMigrationProfile();

        var view = MigrationPlanPresentationBuilder.Create(profile);

        view.ReadinessChecklist.CanRequestExecution.Should().BeFalse();
        view.ReadinessChecklist.PrimaryActionText.Should().Be("\u5148\u751f\u6210\u56de\u6eda\u8bc1\u636e");
        view.ReadinessChecklist.Steps.Should().Contain(step => step.Key == "feature-enabled" && step.State == MigrationPreflightStepState.Blocked);
        view.ReadinessChecklist.Steps.Should().Contain(step => step.Key == "snapshot" && step.State == MigrationPreflightStepState.Waiting);
        view.ReadinessChecklist.Steps.Should().Contain(step => step.Key == "destination-space" && step.State == MigrationPreflightStepState.Waiting);
        view.ReadinessChecklist.Operation.Should().BeNull();
    }

    [Fact]
    public void Migration_readiness_checklist_shows_snapshot_evidence_and_plan_confirmation_scope()
    {
        var profile = CreateCDriveMigrationProfile();

        var view = MigrationPlanPresentationBuilder.Create(
            profile,
            new MigrationPlanPresentationOptions
            {
                Readiness = new MigrationExecutionReadiness
                {
                    SnapshotId = "snapshot-20260707-001",
                    SnapshotEvidencePath = @"D:\OMNIX\Rollback\snapshot.json",
                    SnapshotEvidenceSha256 = new string('D', 64),
                    UserConfirmedPlan = true
                },
                RollbackManifestExists = _ => true
            });

        var snapshot = view.ReadinessChecklist.Steps.Single(step => step.Key == "snapshot");
        var plan = view.ReadinessChecklist.Steps.Single(step => step.Key == "plan-confirmation");

        snapshot.State.Should().Be(MigrationPreflightStepState.Complete);
        snapshot.Detail.Should().Contain("\u5feb\u7167\u8bc1\u636e");
        snapshot.Detail.Should().Contain("snapshot-20260707-001");
        plan.State.Should().Be(MigrationPreflightStepState.Complete);
        plan.Detail.Should().Contain("\u76ee\u6807\u4f4d\u7f6e");
        plan.Detail.Should().Contain("\u53d7\u5f71\u54cd\u8def\u5f84");
        plan.Detail.Should().Contain("\u56de\u6eda");
        plan.Detail.Should().Contain("\u89c2\u5bdf");
        view.CanRunMigration.Should().BeFalse();
    }

    [Fact]
    public void Migration_plan_presentation_shows_manifest_draft_and_destination_space_probe()
    {
        var profile = CreateCDriveMigrationProfile();

        var view = MigrationPlanPresentationBuilder.Create(
            profile,
            new MigrationPlanPresentationOptions
            {
                Now = new DateTimeOffset(2026, 7, 1, 11, 0, 0, TimeSpan.Zero),
                SnapshotId = "snapshot-ui",
                RollbackRoot = @"D:\OMNIX\Rollback",
                AvailableBytesProvider = _ => 200L * 1024 * 1024 * 1024
            });

        view.RollbackManifestLine.Should().Contain(@"D:\OMNIX\Rollback");
        view.RollbackManifestLine.Should().Contain("\u8349\u7a3f");
        view.RollbackManifestLine.Should().Contain("\u8def\u5f84");
        view.DestinationSpaceLine.Should().Contain("\u7a7a\u95f4\u8db3\u591f");
        view.SafetyBanner.Should().Contain("\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6");
    }

    [Fact]
    public void Migration_plan_presentation_marks_rollback_manifest_ready_after_user_confirmed_creation()
    {
        var profile = CreateCDriveMigrationProfile();
        var manifestPath = @"D:\OMNIX\Rollback\Ollama\migration.rollback.json";
        var snapshotPath = @"D:\OMNIX\Rollback\Ollama\migration.snapshot.json";

        var view = MigrationPlanPresentationBuilder.Create(
            profile,
            new MigrationPlanPresentationOptions
            {
                Now = new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.Zero),
                SnapshotId = "snapshot-after-user-confirmation",
                RollbackRoot = @"D:\OMNIX\Rollback",
                AvailableBytesProvider = _ => 200L * 1024 * 1024 * 1024,
                Readiness = new MigrationExecutionReadiness
                {
                    FeatureEnabled = true,
                    SnapshotId = "snapshot-after-user-confirmation",
                    RollbackManifestPath = manifestPath,
                    RollbackManifestSha256 = new string('A', 64),
                    SnapshotEvidencePath = snapshotPath,
                    SnapshotEvidenceSha256 = new string('B', 64),
                    DestinationAvailableBytes = 200L * 1024 * 1024 * 1024
                },
                RollbackManifestExists = path => path == manifestPath || path == snapshotPath
            });

        view.RollbackManifestLine.Should().Contain("\u5df2\u5c31\u7eea");
        view.RollbackManifestLine.Should().Contain(manifestPath);
        view.ReadinessChecklist.Steps.Should().Contain(step =>
            step.Key == "rollback-manifest" &&
            step.State == MigrationPreflightStepState.Complete);
        view.ReadinessChecklist.Steps.Should().Contain(step =>
            step.Key == "destination-space" &&
            step.State == MigrationPreflightStepState.Complete);
        view.CanRunMigration.Should().BeTrue(
            "verified evidence should enable only the separate final-consent step");
        view.ReadinessChecklist.Steps.Should().Contain(step =>
            step.Key == "plan-confirmation" &&
            step.State == MigrationPreflightStepState.Waiting);
        view.ReadinessChecklist.ExecutionGate.Operation!.ConfirmationAccepted.Should().BeFalse();

        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var planWindow = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MigrationPlanWindow.xaml.cs"));
        main.Should().Contain("FeatureEnabled = true");
        planWindow.Should().Contain("MigrationFinalConsentWindow");
        planWindow.Should().Contain("ProductionCompleted = outcome.CompletedProduction");
        main.Should().Contain("if (window.ProductionCompleted)");
        main.Should().Contain("SetSoftwareProfiles(await ScanSoftwareProfilesAsync())");
    }

    [Fact]
    public void Uninstall_plan_preview_is_safe_and_explains_the_separate_final_confirmation()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Publisher = "Tencent",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0",
            CachePaths = [@"C:\Users\Me\AppData\Local\Marvis\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Marvis\Logs"],
            Services = ["MarvisSvr"],
            ScheduledTasks = [@"\Marvis Update"]
        };

        var view = UninstallPlanPresentationBuilder.Create(profile);

        view.Title.Should().Contain("Marvis");
        view.Summary.Should().NotBeNullOrWhiteSpace();
        view.Summary.Should().Contain("\u6700\u7ec8\u786e\u8ba4");
        view.Summary.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u8fd0\u884c");
        view.PostUninstallScanLine.Should().NotBeNullOrWhiteSpace();
        view.OfficialConfirmation.ExecutablePath.Should().Contain("Uninstall.exe");
        view.OfficialConfirmation.ReadinessWarnings.Should().HaveCountGreaterThanOrEqualTo(2);
        view.OfficialConfirmation.Checklist.Should().NotBeEmpty();
        view.CanRunOfficialUninstaller.Should().BeFalse();
        view.OfficialUninstallerLine.Should().Contain("Uninstall.exe");
        view.Sections.Should().NotBeEmpty();
        view.FinalReminder.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Uninstall_recovery_assessment_distinguishes_app_reinstall_from_restorable_residue()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Uninstall.exe"" /uninstall",
            DataPaths = [@"C:\Users\Me\AppData\Local\Marvis\Data"],
            CachePaths = [@"C:\Users\Me\AppData\Local\Marvis\Cache"],
            Services = ["MarvisSvr"]
        };

        var assessment = UninstallRecoveryAssessmentPresenter.Create(profile);

        assessment.AgentConclusion.Should().Contain("可以准备");
        assessment.CanUndoOfficialUninstall.Should().BeFalse();
        assessment.CanRestoreQuarantinedResidue.Should().BeTrue();
        assessment.CanExecuteDirectly.Should().BeFalse();
        assessment.UndoHeadline.Should().Contain("不能");
        assessment.UndoHeadline.Should().Contain("重新安装");
        assessment.ProtectionLines.Should().Contain(line => line.Contains("个人数据"));
        assessment.ProtectionLines.Should().Contain(line => line.Contains("中/高风险"));
        assessment.ProtectionLines.Should().Contain(line => line.Contains("隔离区") && line.Contains("还原"));
        assessment.SimpleSteps.Should().HaveCount(3);
        assessment.NextAction.Should().Contain("备份");

        var visibleText = string.Join(
            Environment.NewLine,
            [assessment.AgentConclusion, assessment.UndoHeadline, assessment.NextAction, assessment.SafetyBoundary,
                .. assessment.ProtectionLines, .. assessment.SimpleSteps]);
        visibleText.Should().NotContain(@"C:\");
        visibleText.Should().NotContain(@"D:\");
        visibleText.Should().NotContain("MarvisSvr");
    }

    [Fact]
    public void Reinstall_source_readiness_accepts_an_existing_publisher_signed_installer_file()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            ReinstallSource = @"D:\Installers\ExampleSetup.exe"
        };

        var readiness = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => true,
            directoryExists: _ => false,
            signatureResolver: _ => "CN=Example Inc., O=Example Inc.");

        readiness.Status.Should().Be(ReinstallSourceReadinessStatus.VerifiedPublisherSignedInstaller);
        readiness.StatusLabel.Should().Contain("\u5df2\u627e\u5230");
        readiness.CanUseAsRecoveryEvidence.Should().BeTrue();
        readiness.CanExecuteDirectly.Should().BeFalse();
        readiness.RecoveryEvidence.Should().NotBeNull();
        readiness.RecoveryEvidence!.Method.Should().Be(OfficialUninstallRecoveryMethod.ReinstallSource);
        readiness.RecoveryEvidence.CanRecoverApplication.Should().BeTrue();
        readiness.RecoveryEvidence.UserDataBackupConfirmed.Should().BeFalse();

        var beginnerText = string.Join(
            Environment.NewLine,
            readiness.StatusLabel,
            readiness.AgentConclusion,
            readiness.NextAction);
        beginnerText.Should().NotContain(@"D:\");
    }

    [Theory]
    [InlineData(@"D:\Installers", true, "{0A4E2C19-CC57-4E98-A560-0A9C9A12D135}", ReinstallSourceReadinessStatus.DirectoryHint)]
    [InlineData(null, false, "{0A4E2C19-CC57-4E98-A560-0A9C9A12D135}", ReinstallSourceReadinessStatus.ProductCodeHint)]
    public void Reinstall_source_hints_cannot_become_recovery_evidence(
        string? reinstallSource,
        bool directoryExists,
        string productCode,
        ReinstallSourceReadinessStatus expectedStatus)
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            ReinstallSource = reinstallSource,
            IsWindowsInstaller = true,
            WindowsInstallerProductCode = productCode
        };

        var readiness = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => false,
            directoryExists: _ => directoryExists,
            signatureResolver: _ => null);

        readiness.Status.Should().Be(expectedStatus);
        readiness.CanUseAsRecoveryEvidence.Should().BeFalse();
        readiness.CanExecuteDirectly.Should().BeFalse();
        readiness.RecoveryEvidence.Should().BeNull();
        readiness.AgentConclusion.Should().Contain("\u7ebf\u7d22");
        readiness.AgentConclusion.Should().NotContain(productCode);
        readiness.TechnicalDetails.Should().Contain(detail => detail.Contains(productCode));
    }

    [Fact]
    public void Reinstall_source_signature_mismatch_stays_blocked()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            ReinstallSource = @"D:\Installers\ExampleSetup.exe"
        };

        var readiness = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => true,
            directoryExists: _ => false,
            signatureResolver: _ => "CN=Different Vendor");

        readiness.Status.Should().Be(ReinstallSourceReadinessStatus.SignatureMismatch);
        readiness.CanUseAsRecoveryEvidence.Should().BeFalse();
        readiness.RecoveryEvidence.Should().BeNull();
        readiness.NextAction.Should().Contain("\u4e0d\u8981\u4f7f\u7528");
    }

    [Fact]
    public void User_selected_installer_is_verified_without_being_executed()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc."
        };

        var readiness = ReinstallSourceReadinessPresenter.CreateForSelectedInstaller(
            profile,
            @"D:\Downloads\ExampleSetup.exe",
            fileExists: _ => true,
            signatureResolver: _ => "CN=Example Inc., O=Example Inc.");

        readiness.SourceOrigin.Should().Be(ReinstallSourceOrigin.UserSelected);
        readiness.Status.Should().Be(ReinstallSourceReadinessStatus.VerifiedPublisherSignedInstaller);
        readiness.AgentConclusion.Should().Contain("\u4f60\u9009\u62e9");
        readiness.CanUseAsRecoveryEvidence.Should().BeTrue();
        readiness.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Existing_restore_point_is_a_fallback_hint_not_verified_app_recovery()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var reinstall = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => false,
            directoryExists: _ => false,
            signatureResolver: _ => null);
        var restorePoint = new WindowsRestorePointInfo
        {
            SequenceNumber = 25,
            Description = "Before update",
            CreatedAt = new DateTimeOffset(2026, 7, 9, 10, 0, 0, TimeSpan.FromHours(8)),
            RestorePointType = 0,
            EventType = 100
        };

        var preparation = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstall,
            [restorePoint],
            personalDataBackupAcknowledged: false);

        preparation.HasRestorePointHint.Should().BeTrue();
        preparation.RestorePointStatus.Should().Contain("1");
        preparation.RestorePointStatus.Should().Contain("\u540e\u5907\u7ebf\u7d22");
        preparation.RequiresPersonalDataBackup.Should().BeTrue();
        preparation.IsPreparationComplete.Should().BeFalse();
        preparation.CanRequestExecution.Should().BeFalse();
    }

    [Fact]
    public void Restore_point_timeout_is_explained_instead_of_claiming_none_exist()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc."
        };
        var reinstall = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => false,
            directoryExists: _ => false,
            signatureResolver: _ => null);

        var preparation = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstall,
            [],
            personalDataBackupAcknowledged: false,
            restorePointScanState: WindowsRestorePointScanState.TimedOut);

        preparation.HasRestorePointHint.Should().BeFalse();
        preparation.RestorePointStatus.Should().Contain("读取超时");
        preparation.RestorePointStatus.Should().Contain("暂时不能确认");
        preparation.RestorePointStatus.Should().NotContain("未发现");
        preparation.CanRequestExecution.Should().BeFalse();
    }

    [Fact]
    public void Uninstall_preview_uses_bounded_restore_point_result_before_opening_window()
    {
        var mainWindowCode = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "MainWindow.xaml.cs"));
        var scannerSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Scanner", "Recovery", "WindowsRestorePointScanner.cs"));

        mainWindowCode.Should().Contain("ScanWithStatusAsync");
        mainWindowCode.Should().Contain("restorePointScan.Points");
        mainWindowCode.Should().Contain("restorePointScan.State");
        scannerSource.Should().Contain("WaitAsync");
        scannerSource.Should().Contain("TimeSpan.FromSeconds");
    }

    [Fact]
    public void Recovery_preparation_requires_verified_installer_and_separate_backup_acknowledgement()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var reinstall = ReinstallSourceReadinessPresenter.CreateForSelectedInstaller(
            profile,
            @"D:\Downloads\ExampleSetup.exe",
            fileExists: _ => true,
            signatureResolver: _ => "CN=Example Inc.");

        var beforeBackup = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstall,
            [],
            personalDataBackupAcknowledged: false);
        var afterBackup = UninstallRecoveryPreparationPresenter.Create(
            profile,
            reinstall,
            [],
            personalDataBackupAcknowledged: true);

        beforeBackup.IsPreparationComplete.Should().BeFalse();
        beforeBackup.BackupStatus.Should().Contain("\u672a\u786e\u8ba4");
        afterBackup.IsPreparationComplete.Should().BeTrue();
        afterBackup.BackupStatus.Should().Contain("\u5df2\u786e\u8ba4");
        afterBackup.CanRequestExecution.Should().BeFalse();
        afterBackup.SafetyBoundary.Should().Contain("\u4e0d\u4f1a\u8fd0\u884c");
    }

    [Fact]
    public void Recovery_preparation_session_keeps_installer_selection_and_backup_confirmation_separate()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var initial = ReinstallSourceReadinessPresenter.Create(
            profile,
            fileExists: _ => false,
            directoryExists: _ => false,
            signatureResolver: _ => null);
        var session = new UninstallRecoveryPreparationSession(profile, initial, []);

        session.SelectOfficialInstaller(
            @"D:\Downloads\ExampleSetup.exe",
            fileExists: _ => true,
            signatureResolver: _ => "CN=Example Inc.");

        session.Current.ReinstallReadiness.CanUseAsRecoveryEvidence.Should().BeTrue();
        session.Current.IsPreparationComplete.Should().BeFalse();

        session.SetPersonalDataBackupAcknowledged(true);

        session.Current.IsPreparationComplete.Should().BeTrue();
        session.Current.CanRequestExecution.Should().BeFalse();
    }

    [Fact]
    public void Uninstall_safety_window_body_uses_plain_chinese_while_official_uninstaller_stays_disabled()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Publisher = "Tencent",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0 --uninstall-entry=2",
            CachePaths = [@"C:\Users\Me\AppData\Local\Marvis\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Marvis\Logs"],
            RunningProcesses = ["Marvis", "MarvisAgent"],
            Services = ["MarvisSvr"],
            ScheduledTasks = [@"\Marvis Update"]
        };

        var view = UninstallPlanPresentationBuilder.Create(profile);
        var visibleCopy = new[]
            {
                view.Title,
                view.Summary,
                view.OfficialUninstallerLine,
                view.PostUninstallScanLine,
                view.OfficialConfirmation.SafetySummary,
                view.OfficialConfirmation.PrimaryButtonText,
                view.OfficialConfirmation.PreflightChecklist.PrimaryActionText,
                view.OfficialConfirmation.PreflightChecklist.NextActionText,
                view.OfficialConfirmation.ExecutionGate.PrimaryButtonText,
                view.OfficialConfirmation.ExecutionGate.CommandTrust.Summary,
                view.FinalReminder
            }
            .Concat(view.OfficialConfirmation.ReadinessWarnings)
            .Concat(view.OfficialConfirmation.Checklist)
            .Concat(view.OfficialConfirmation.PreflightChecklist.Steps.SelectMany(step => new[] { step.Title, step.StatusLabel, step.Detail }))
            .Concat(view.OfficialConfirmation.ExecutionGate.BlockingReasons)
            .Concat(view.Sections.SelectMany(section => new[] { section.Title, section.RiskLabel, section.ActionLine }.Concat(section.Items)))
            .ToList();

        view.CanRunOfficialUninstaller.Should().BeFalse();
        view.OfficialConfirmation.CanRunOfficialUninstaller.Should().BeFalse();
        view.OfficialConfirmation.PreflightChecklist.CanRequestExecution.Should().BeFalse();
        view.Title.Should().Contain("\u5378\u8f7d\u5b89\u5168\u65b9\u6848");
        view.Summary.Should().Contain("\u53ea\u9884\u89c8");
        view.Summary.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u8fd0\u884c\u5378\u8f7d\u5668");
        view.Summary.Should().Contain("\u4e0d\u4f1a\u5220\u9664");
        view.PostUninstallScanLine.Should().Contain("\u5378\u8f7d\u540e\u91cd\u65b0\u626b\u63cf");
        view.PostUninstallScanLine.Should().Contain("\u6b8b\u7559");
        view.OfficialConfirmation.PreflightChecklist.PrimaryActionText.Should().Be("\u5148\u5b8c\u6210\u6062\u590d\u51c6\u5907");
        view.OfficialConfirmation.PreflightChecklist.Steps.Select(step => step.Title).Should().Contain(
        [
            "\u5b89\u5168\u6267\u884c\u5165\u53e3",
            "\u547d\u4ee4\u53ef\u4fe1\u5ea6",
            "\u5378\u8f7d\u5668\u6587\u4ef6",
            "\u5378\u8f7d\u524d\u5feb\u7167",
            "\u786e\u8ba4\u5378\u8f7d\u547d\u4ee4",
            "\u5173\u95ed\u5e94\u7528",
            "\u5378\u8f7d\u540e\u91cd\u626b"
        ]);
        visibleCopy.Should().NotContain(text =>
            text.Contains("Preview only", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Official uninstall preflight", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Execution feature", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Command trust", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Official uninstaller file", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Pre-uninstall snapshot", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Official command confirmation", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Post-uninstall rescan", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Request official uninstaller", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Finish preflight checklist", StringComparison.OrdinalIgnoreCase)
            || text.Contains("This build only previews", StringComparison.OrdinalIgnoreCase)
            || text.Contains("The uninstaller", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Uninstall_plan_window_has_readable_text_and_stable_hooks()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "UninstallPlanWindow.xaml"));
        var mainWindowCode = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var planWindowCode = File.ReadAllText(FindRepositoryFile("src", "Css.App", "UninstallPlanWindow.xaml.cs"));

        var requiredAutomationIds = new[]
        {
            "UninstallPlanTitleTextBlock",
            "UninstallPlanSummaryTextBlock",
            "UninstallPlanSafetyTextBlock",
            "UninstallPlanAgentConclusionTextBlock",
            "UninstallPlanUndoHeadlineTextBlock",
            "UninstallPlanReinstallReadinessTextBlock",
            "UninstallPlanReinstallNextActionTextBlock",
            "UninstallPlanRestorePointStatusTextBlock",
            "UninstallPlanChooseInstallerButton",
            "UninstallPlanBackupCheckBox",
            "UninstallPlanPreparationSummaryTextBlock",
            "UninstallPlanProtectionListBox",
            "UninstallPlanSimpleStepsListBox",
            "UninstallPlanNextActionTextBlock",
            "UninstallPlanTechnicalDetailsExpander",
            "UninstallPlanReinstallTechnicalListBox",
            "UninstallPlanOfficialUninstallerTextBlock",
            "UninstallPlanPostScanTextBlock",
            "UninstallPlanWorkflowListBox",
            "UninstallPlanOfficialConfirmationTextBlock",
            "UninstallPlanOfficialWarningsListBox",
            "UninstallPlanOfficialChecklistListBox",
            "UninstallPlanPreflightListBox",
            "UninstallPlanExecutionGateTextBlock",
            "UninstallPlanSectionsListBox",
            "UninstallPlanFinalReminderTextBlock",
            "UninstallPlanContinueFinalConsentButton",
            "UninstallPlanRequestStatusTextBlock",
            "UninstallPlanCloseButton"
        };

        foreach (var automationId in requiredAutomationIds)
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{automationId}\"");

        xaml.Should().Contain("ItemsSource=\"{Binding WorkflowGuide.Steps}\"");
        xaml.Should().Contain("ItemsSource=\"{Binding Sections}\"");
        xaml.Should().Contain("ItemsSource=\"{Binding RecoveryAssessment.ProtectionLines}\"");
        xaml.Should().Contain("ItemsSource=\"{Binding RecoveryAssessment.SimpleSteps}\"");
        xaml.Should().NotContain("StringFormat=&#x2022; {0}");
        xaml.Should().Contain("IsExpanded=\"False\"");
        xaml.IndexOf("UninstallPlanAgentConclusionTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("UninstallPlanTechnicalDetailsExpander", StringComparison.Ordinal));
        xaml.IndexOf("UninstallPlanReinstallReadinessTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("UninstallPlanTechnicalDetailsExpander", StringComparison.Ordinal));
        xaml.IndexOf("UninstallPlanTechnicalDetailsExpander", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("UninstallPlanOfficialUninstallerTextBlock", StringComparison.Ordinal));
        xaml.Should().Contain("Click=\"Close_Click\"");
        xaml.Should().NotContain("\u9357\u6b4c\u6d47");
        xaml.Should().NotContain("\u93b5\u8f70");
        xaml.Should().NotContain("\u9438\u30e9\u4ebe");
        xaml.Should().NotContain("\u59ab");
        xaml.Should().NotContain("\u5f53\u524d\u7248\u672c\u4f1a\u8fd0\u884c\u5378\u8f7d\u5668");
        mainWindowCode.Should().Contain("reinstallSourceFileExists: File.Exists");
        mainWindowCode.Should().Contain("reinstallSourceDirectoryExists: Directory.Exists");
        mainWindowCode.Should().Contain("reinstallSourceSignatureResolver: SignatureInspector.GetSignatureSubject");
        mainWindowCode.Should().Contain("WindowsRestorePointScanner");
        mainWindowCode.Should().Contain("await restorePointScanner.ScanWithStatusAsync()");
        planWindowCode.Should().Contain("OpenFileDialog");
        planWindowCode.Should().Contain("SelectOfficialInstaller");
        planWindowCode.Should().Contain("SetPersonalDataBackupAcknowledged");
        planWindowCode.Should().Contain("ContinueFinalConsent_Click");
        planWindowCode.Should().Contain("OfficialUninstallFinalConsentWindow");
        planWindowCode.Should().Contain("OfficialUninstallRequestPreparationService.Create");
        planWindowCode.Should().Contain("ComputeFileSha256");
        planWindowCode.Should().Contain("consentWindow.Consent");
        planWindowCode.Should().NotContain("Process.Start");
        planWindowCode.Should().NotContain("OfficialUninstallOperationHandler");
        planWindowCode.Should().NotContain("SafetyOperationPipeline");
    }

    [Fact]
    public void Windows_inventory_scanner_reads_reinstall_source_and_installer_metadata()
    {
        var scannerSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Scanner", "Software", "SoftwareInventoryScanner.cs"));

        scannerSource.Should().Contain("InstalledSoftwareRegistryRecordFactory.Create");
        scannerSource.Should().Contain("sub.GetValue(\"InstallSource\")");
        scannerSource.Should().Contain("sub.GetValue(\"WindowsInstaller\")");
        scannerSource.Should().Contain("subKeyName");
    }

    [Fact]
    public void Windows_restore_point_scanner_uses_only_the_read_only_system_restore_query()
    {
        var scannerSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Scanner", "Recovery", "WindowsRestorePointScanner.cs"));

        scannerSource.Should().Contain(@"\\.\root\default");
        scannerSource.Should().Contain("EventType FROM SystemRestore");
        scannerSource.Should().Contain("ManagementDateTimeConverter.ToDateTime");
        scannerSource.Should().NotContain("CreateRestorePoint");
        scannerSource.Should().NotContain("SystemRestore.Restore");
    }

    [Fact]
    public void Uninstall_plan_window_gui_smoke_uses_shared_helpers_and_closes_without_execution()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-uninstall-plan-window-smoke.ps1"));
        var helpers = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("DrawerUninstallButton");
        script.Should().Contain("UninstallPlanTitleTextBlock");
        script.Should().Contain("UninstallPlanAgentConclusionTextBlock");
        script.Should().Contain("UninstallPlanUndoHeadlineTextBlock");
        script.Should().Contain("UninstallPlanReinstallReadinessTextBlock");
        script.Should().Contain("UninstallPlanReinstallNextActionTextBlock");
        script.Should().Contain("UninstallPlanRestorePointStatusTextBlock");
        script.Should().Contain("UninstallPlanChooseInstallerButton");
        script.Should().Contain("UninstallPlanBackupCheckBox");
        script.Should().Contain("UninstallPlanPreparationSummaryTextBlock");
        script.Should().Contain("UninstallPlanBuildFinalChecklistButton");
        script.Should().Contain("UninstallPlanFinalChecklistTitleTextBlock");
        script.Should().Contain("UninstallPlanFinalChecklistStatusTextBlock");
        script.Should().Contain("UninstallPlanFinalChecklistMissingListBox");
        script.Should().Contain("UninstallPlanProtectionListBox");
        script.Should().Contain("UninstallPlanSimpleStepsListBox");
        script.Should().Contain("UninstallPlanNextActionTextBlock");
        script.Should().Contain("UninstallPlanTechnicalDetailsExpander");
        script.Should().Contain("technicalDetailsCollapsed = $true");
        script.Should().Contain("recoveryTruthVisible = $true");
        script.Should().Contain("UninstallPlanCloseButton");
        script.Should().Contain("Find-SecondaryWindowWithChild $process.Id $mainWindowHandle 'UninstallPlanTitleTextBlock'");
        script.Should().NotContain("function Find-WindowByDescendantAutomationId");
        script.Should().NotContain("function Find-SecondaryWindowWithChild");
        helpers.Should().Contain("function Find-WindowByDescendantAutomationId");
        helpers.Should().Contain("function Find-SecondaryWindowWithChild");
        helpers.Should().Contain("[System.Windows.Automation.TreeScope]::Descendants");
        helpers.Should().Contain("[System.Windows.Automation.TreeWalker]::ControlViewWalker");
        helpers.Should().Contain("EnumWindows");
        helpers.Should().Contain("GetWindowThreadProcessId");
        helpers.Should().Contain("AutomationElement]::FromHandle");
        script.Should().Contain("qa-uninstall-plan-window.png");
        script.Should().Contain("planWindowFound = $true");
        script.Should().Contain("closedPlanWindow = $true");
        script.Should().Contain("noExecutionControl = $true");
        script.Should().Contain("reinstallReadinessVisible = $true");
        script.Should().Contain("recoveryPreparationVisible = $true");
        script.Should().Contain("finalChecklistVisible = $true");
        script.Should().Contain("evidenceRootCreated = $false");
        script.Should().Contain("OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT");
        script.Should().NotContain("UninstallPlanConfirmButton");
        script.Should().NotContain("Start-Process -FilePath $uninstaller");
        script.Should().NotContain("Invoke-Element $run");
    }

    [Fact]
    public void Uninstall_plan_keeps_execution_authority_behind_the_injected_coordinator()
    {
        var xaml = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "UninstallPlanWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "UninstallPlanWindow.xaml.cs"));

        xaml.Should().Contain("UninstallPlanBuildFinalChecklistButton");
        xaml.Should().Contain("UninstallPlanFinalChecklistTitleTextBlock");
        xaml.Should().Contain("UninstallPlanFinalChecklistStatusTextBlock");
        xaml.Should().Contain("UninstallPlanFinalChecklistSummaryTextBlock");
        xaml.Should().Contain("UninstallPlanFinalChecklistReadyListBox");
        xaml.Should().Contain("UninstallPlanFinalChecklistPendingListBox");
        xaml.Should().Contain("UninstallPlanFinalChecklistMissingListBox");
        xaml.Should().Contain("UninstallPlanFinalChecklistSafetyTextBlock");
        xaml.Should().Contain("UninstallPlanContinueFinalConsentButton");
        xaml.Should().Contain("UninstallPlanRequestStatusTextBlock");
        xaml.Should().Contain("Content=\"&#x7EE7;&#x7EED;&#x6700;&#x7EC8;&#x786E;&#x8BA4;\"");
        xaml.IndexOf("UninstallPlanFinalChecklistTitleTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("UninstallPlanTechnicalDetailsExpander", StringComparison.Ordinal));
        xaml.Should().NotContain("UninstallPlanRunOfficialUninstallerButton");

        code.Should().Contain("UninstallFinalConfirmationDraftService");
        code.Should().Contain("ResolveUninstallEvidenceRoot");
        code.Should().Contain("CreateAsync(_profile, _preparationSession.Current)");
        code.Should().Contain("CanExecuteDirectly");
        code.Should().Contain("UninstallPlanFinalChecklistStatusTextBlock.BringIntoView();");
        code.Should().Contain("draft.Status == UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation");
        code.Should().Contain(@"\u5f53\u524d\u5c1a\u672a\u542f\u52a8\u5378\u8f7d\u5668");
        code.Should().Contain("IOfficialUninstallProductionExecutionCoordinator");
        code.Should().Contain("_executionCoordinator.ExecuteAsync(_preparedRequest)");
        code.Should().NotContain("SafetyOperationPipeline");
        code.Should().NotContain("OfficialUninstallOperationHandler");
        code.Should().NotContain("WindowsOfficialUninstallProductionWorkerLauncher");
        code.Should().NotContain("OfficialUninstallWorkerLifecycleClient");
        code.Should().NotContain("official-uninstall-production-worker");
        code.Should().NotContain("Process.Start");
    }

    [Fact]
    public void Official_uninstall_post_scan_result_has_a_beginner_first_non_executable_window()
    {
        var xaml = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "UninstallPostScanResultWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "UninstallPostScanResultWindow.xaml.cs"));
        var appCode = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "App.xaml.cs"));

        xaml.Should().Contain("UninstallPostScanTitleTextBlock");
        xaml.Should().Contain("UninstallPostScanStatusTextBlock");
        xaml.Should().Contain("UninstallPostScanConclusionTextBlock");
        xaml.Should().Contain("UninstallPostScanFactsListBox");
        xaml.Should().Contain("UninstallPostScanAgentAdviceTextBlock");
        xaml.Should().Contain("UninstallPostScanNextActionTextBlock");
        xaml.Should().Contain("UninstallPostScanSafetyTextBlock");
        xaml.Should().Contain("UninstallPostScanCloseButton");
        xaml.IndexOf("UninstallPostScanStatusTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("UninstallPostScanFactsListBox", StringComparison.Ordinal));
        xaml.Should().NotContain("UninstallPostScanExecuteButton");

        code.Should().Contain("OfficialUninstallPostScanViewModel");
        code.Should().Contain("CanExecuteDirectly");
        code.Should().NotContain("SafetyOperationPipeline");
        code.Should().NotContain("OfficialUninstallOperationHandler");
        code.Should().NotContain("ExecuteAsync");
        code.Should().NotContain("Process.Start");
        code.Should().NotContain("File.Delete");
        code.Should().NotContain("File.Move");

        appCode.Should().Contain("#if DEBUG");
        appCode.Should().Contain("--smoke-uninstall-post-scan-review");
        appCode.Should().Contain("UninstallPostScanResultWindow");
    }

    [Fact]
    public void Official_uninstall_post_scan_gui_smoke_proves_plain_results_without_execution()
    {
        var script = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-uninstall-post-scan-result-smoke.ps1"));
        var doc = File.ReadAllText(FindRepositoryFile(
            "docs", "development", "gui-smokes.md"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("--smoke-uninstall-post-scan-review");
        script.Should().Contain("UninstallPostScanStatusTextBlock");
        script.Should().Contain("UninstallPostScanAgentAdviceTextBlock");
        script.Should().Contain("UninstallPostScanSafetyTextBlock");
        script.Should().Contain("UninstallPostScanCloseButton");
        script.Should().Contain("noExecutionControl = $true");
        script.Should().Contain("qa-uninstall-post-scan-result.png");
        script.Should().NotContain("OfficialUninstallOperationHandler");
        script.Should().NotContain("SafetyOperationPipeline");
        script.Should().NotContain("Invoke-Element $execute");
        doc.Should().Contain("gui-uninstall-post-scan-result-smoke.ps1");
        doc.Should().Contain("--smoke-uninstall-post-scan-review");
        doc.Should().Contain("DEBUG");
    }

    [Fact]
    public void Official_uninstall_final_consent_window_requires_three_plain_acknowledgements()
    {
        var xaml = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallFinalConsentWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallFinalConsentWindow.xaml.cs"));
        var capture = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallFinalConsentVisualCapture.cs"));
        var appCode = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "App.xaml.cs"));
        var appProject = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "Css.App.csproj"));
        var ipcProject = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Css.Ipc.csproj"));

        xaml.Should().Contain("OfficialUninstallFinalConsentTitleTextBlock");
        xaml.Should().Contain("OfficialUninstallFinalConsentSoftwareTextBlock");
        xaml.Should().Contain("OfficialUninstallFinalConsentImpactListBox");
        xaml.Should().Contain("OfficialUninstallFinalConsentCommandCheckBox");
        xaml.Should().Contain("OfficialUninstallFinalConsentUndoCheckBox");
        xaml.Should().Contain("OfficialUninstallFinalConsentPostScanCheckBox");
        xaml.Should().Contain("OfficialUninstallFinalConsentReadinessTextBlock");
        xaml.Should().Contain("OfficialUninstallFinalConsentConfirmButton");
        xaml.Should().Contain("OfficialUninstallFinalConsentCancelButton");
        xaml.Should().Contain("IsEnabled=\"False\"");
        xaml.Should().Contain("<Grid Background=\"#F6F7F9\">");
        xaml.IndexOf("<Grid Background=\"#F6F7F9\">", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("<Grid Margin=\"24\">", StringComparison.Ordinal));
        xaml.IndexOf("OfficialUninstallFinalConsentSoftwareTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("OfficialUninstallFinalConsentCommandCheckBox", StringComparison.Ordinal));

        code.Should().Contain("OfficialUninstallFinalConsentBuilder.Create");
        code.Should().Contain("OfficialUninstallFinalConsentSelection");
        code.Should().Contain("Consent");
        code.Should().Contain("OfficialUninstallVisualGateReceiptIssuer");
        code.Should().Contain("_visualCapture.Capture(this, now)");
        code.Should().Contain("VisualTicketId");
        code.Should().Contain("CryptographicOperations.ZeroMemory(capture.ScreenshotPng)");
        code.Should().NotContain("SafetyOperationPipeline");
        code.Should().NotContain("OfficialUninstallOperationHandler");
        code.Should().NotContain("Process.Start");
        code.Should().NotContain("ExecuteAsync");

        capture.Should().Contain("RenderTargetBitmap");
        capture.Should().Contain("DispatcherPriority.Render");
        capture.Should().Contain("VisualBrush(content)");
        capture.Should().Contain("PngBitmapEncoder");
        capture.Should().Contain("EnsureNonBlank");
        capture.Should().Contain("IsFullyVisible");
        capture.Should().Contain("OfficialUninstallFinalConsentImpactListBox");
        capture.Should().Contain("OfficialUninstallFinalConsentSafetyTextBlock");
        capture.Should().Contain("OfficialUninstallRunButton");
        capture.Should().NotContain("CopyFromScreen");
        capture.Should().NotContain("File.Write");
        capture.Should().NotContain("Process.Start");
        capture.Should().NotContain("SafetyOperationPipeline");
        var captureTest = File.ReadAllText(FindRepositoryFile(
            "tests", "Css.Tests", "OfficialUninstallFinalConsentVisualCaptureTests.cs"));
        captureTest.Should().Contain("OMNIX_FINAL_CONSENT_RENDER_OUTPUT");
        captureTest.Should().Contain("repository .omx directory");

        appCode.Should().Contain("#if DEBUG");
        appCode.Should().Contain("--smoke-uninstall-final-consent");
        appCode.Should().Contain("OfficialUninstallFinalConsentWindow");
        appCode.Should().Contain("UninstallPostScanResultWindow");
        appCode.Should().Contain("RunFinalConsentFakePipeAsync");
        appCode.Should().Contain("OfficialUninstallFakeNamedPipeServer");
        appCode.Should().Contain("OfficialUninstallFakeNamedPipeClient");
        appCode.Should().Contain("OfficialUninstallAuthenticatedInMemoryEndpoint");
        appCode.Should().Contain("OfficialUninstallElevatedResponsePresenter.Create");
        appCode.Should().Contain("OfficialUninstallVisualGateReceiptIssuer");
        appCode.Should().Contain("OfficialUninstallFinalConsentVisualCapture");
        appCode.Should().Contain("OfficialUninstallElevatedRequestSession");
        appCode.Should().Contain("consentWindow.VisualTicketId");
        appCode.Should().NotContain("ScreenshotSha256 = new string('B'");
        appCode.Should().NotContain("OfficialUninstallOperationHandler");
        appCode.Should().NotContain("WindowsOfficialUninstallerLauncher");
        appCode.Should().NotContain("SafetyOperationPipeline");
        appCode.Should().NotContain("Process.Start");
        appProject.Should().Contain("Css.Ipc");
        appProject.Should().NotContain("<ProjectReference Include=\"..\\Css.Elevated");
        ipcProject.Should().Contain("Css.Core");
        ipcProject.Should().NotContain("Css.Elevated");
    }

    [Fact]
    public void Official_uninstall_final_consent_gui_smoke_confirms_then_shows_fake_result()
    {
        var script = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-uninstall-final-consent-smoke.ps1"));
        var doc = File.ReadAllText(FindRepositoryFile(
            "docs", "development", "gui-smokes.md"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("--smoke-uninstall-final-consent");
        script.Should().Contain("OfficialUninstallFinalConsentConfirmButton");
        script.Should().Contain("OfficialUninstallFinalConsentCommandCheckBox");
        script.Should().Contain("OfficialUninstallFinalConsentUndoCheckBox");
        script.Should().Contain("OfficialUninstallFinalConsentPostScanCheckBox");
        script.Should().Contain("confirmInitiallyDisabled = $true");
        script.Should().Contain("confirmEnabledAfterAllChecks = $true");
        script.Should().Contain("UninstallPostScanStatusTextBlock");
        script.Should().Contain("UninstallPostScanFactsListBox");
        script.Should().Contain("pipeResultFactCount -ne 2");
        script.Should().Contain("runtimeVisualReceiptAccepted = $true");
        script.Should().Contain("OfficialUninstallElevatedRequestSession");
        script.Should().Contain("fakeResultVisible = $true");
        script.Should().Contain("noRealExecutionControl = $true");
        script.Should().Contain("qa-uninstall-final-consent.png");
        script.Should().Contain("qa-uninstall-final-consent-result.png");
        script.Should().NotContain("OfficialUninstallOperationHandler");
        script.Should().NotContain("WindowsOfficialUninstallerLauncher");
        doc.Should().Contain("gui-uninstall-final-consent-smoke.ps1");
        doc.Should().Contain("--smoke-uninstall-final-consent");
        doc.Should().Contain("current-user named pipe");
    }

    [Fact]
    public void Official_uninstall_confirmation_parses_command_and_requires_safe_preflight()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            Publisher = "Tencent",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0 --uninstall-entry=2",
            RunningProcesses = ["Marvis", "MarvisAgent", "MarvisSvr"],
            Services = ["MarvisSvr"],
            ScheduledTasks = [@"\Marvis Update"]
        };

        var confirmation = OfficialUninstallConfirmationBuilder.Create(profile);

        confirmation.SoftwareName.Should().Be("Marvis");
        confirmation.ExecutablePath.Should().Be(@"D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe");
        confirmation.ArgumentsLine.Should().Contain("--oem-uninstall=0");
        confirmation.CanRunOfficialUninstaller.Should().BeFalse();
        confirmation.RequiresSnapshot.Should().BeTrue();
        confirmation.RequiresPostUninstallScan.Should().BeTrue();
        confirmation.ExecutionGate.CanRequestExecution.Should().BeFalse();
        confirmation.SafetySummary.Should().NotBeNullOrWhiteSpace();
        confirmation.ReadinessWarnings.Should().HaveCount(3);
        confirmation.Checklist.Should().NotBeEmpty();
        confirmation.PreflightChecklist.CanRequestExecution.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_confirmation_blocks_when_uninstaller_is_missing()
    {
        var profile = new SoftwareProfile
        {
            Name = "Unknown Tool",
            InstallPath = @"D:\Software\Unknown"
        };

        var confirmation = OfficialUninstallConfirmationBuilder.Create(profile);

        confirmation.CanRunOfficialUninstaller.Should().BeFalse();
        confirmation.ReadinessWarnings.Should().NotBeEmpty();
        confirmation.PreflightChecklist.CanRequestExecution.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_execution_gate_stays_disabled_by_default_even_with_preflight()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0"
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = false,
            SnapshotId = "snapshot-1",
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(profile, readiness, _ => true);

        gate.CanRequestExecution.Should().BeFalse();
        gate.Operation.Should().BeNull();
    }

    [Fact]
    public void Official_uninstall_execution_gate_requires_snapshot_close_confirmation_and_rescan_confirmation()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0",
            RunningProcesses = ["Marvis", "MarvisSvr"],
            DataPaths = [@"C:\Users\Me\AppData\Local\Marvis\Data"]
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = false,
            UserConfirmedPostUninstallRescan = false
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(profile, readiness, _ => true);

        gate.CanRequestExecution.Should().BeFalse();
        gate.BlockingReasons.Should().NotBeEmpty();
        gate.Operation.Should().BeNull();
    }

    [Fact]
    public void Official_uninstall_execution_gate_refuses_a_snapshot_id_without_real_recovery_evidence()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-is-not-enough",
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(profile, readiness, _ => true);

        gate.CanRequestExecution.Should().BeFalse();
        gate.Operation.Should().BeNull();
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("不能一键恢复"));
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("恢复方式"));
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("个人数据"));
    }

    [Fact]
    public void Official_uninstall_execution_gate_refuses_an_unbacked_snapshot_id()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove"
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "made-up-snapshot-id",
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = CreateVerifiedReinstallRecoveryEvidence("verified-installer")
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            _ => true,
            snapshotHashResolver: _ => null,
            now: new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero));

        gate.CanRequestExecution.Should().BeFalse();
        gate.Operation.Should().BeNull();
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("\u5feb\u7167\u8bc1\u636e"));
    }

    [Fact]
    public void Official_uninstall_execution_gate_rejects_snapshot_id_mismatch()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove"
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "displayed-snapshot",
            SnapshotEvidence = CreateVerifiedSnapshotEvidence("different-snapshot", profile.Name),
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = CreateVerifiedReinstallRecoveryEvidence("verified-installer")
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            _ => true,
            snapshotHashResolver: _ => VerifiedSnapshotHash,
            now: VerifiedSnapshotNow);

        gate.CanRequestExecution.Should().BeFalse();
        gate.Operation.Should().BeNull();
        gate.BlockingReasons.Should().Contain(reason => reason.Contains("\u7f16\u53f7\u4e0d\u4e00\u81f4"));
    }

    [Fact]
    public void Official_uninstall_execution_gate_accepts_verified_reinstall_recovery_evidence()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"]
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-with-recovery",
            SnapshotEvidence = CreateVerifiedSnapshotEvidence("snapshot-with-recovery", profile.Name),
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = new OfficialUninstallRecoveryEvidence
            {
                Method = OfficialUninstallRecoveryMethod.ReinstallSource,
                Reference = "verified-offline-installer",
                CanRecoverApplication = true,
                UserDataBackupConfirmed = true
            }
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            _ => true,
            snapshotHashResolver: _ => VerifiedSnapshotHash,
            now: VerifiedSnapshotNow);

        gate.CanRequestExecution.Should().BeTrue();
        gate.BlockingReasons.Should().BeEmpty();
        gate.Operation.Should().NotBeNull();
        gate.Operation!.Arguments["recoveryMethod"].Should().Be("ReinstallSource");
        gate.Operation.Arguments["recoveryReference"].Should().Be("verified-offline-installer");
    }

    [Fact]
    public void Official_uninstall_preflight_checklist_explains_missing_user_safe_steps()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0",
            RunningProcesses = ["Marvis", "MarvisSvr"],
            DataPaths = [@"C:\Users\Me\AppData\Local\Marvis\Data"]
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true
        };

        var checklist = OfficialUninstallPreflightChecklistBuilder.Create(profile, readiness, _ => true);

        checklist.CanRequestExecution.Should().BeFalse();
        checklist.Operation.Should().BeNull();
        checklist.Steps.Should().Contain(s => s.Key == "command-trust" && s.State == OfficialUninstallPreflightStepState.Complete);
        checklist.Steps.Should().Contain(s => s.Key == "snapshot" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.Steps.Should().Contain(s => s.Key == "official-command-confirmation" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.Steps.Should().Contain(s => s.Key == "close-apps" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.Steps.Should().Contain(s => s.Key == "post-uninstall-rescan" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.Steps.Should().Contain(s => s.Key == "no-automatic-undo" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.Steps.Should().Contain(s => s.Key == "recovery-evidence" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.Steps.Should().Contain(s => s.Key == "user-data-backup" && s.State == OfficialUninstallPreflightStepState.Waiting);
        checklist.NextActionText.Should().Contain("\u5feb\u7167");
    }

    [Fact]
    public void Official_uninstall_preflight_checklist_allows_request_only_after_all_gate_steps_pass()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0"
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-ready",
            SnapshotEvidence = CreateVerifiedSnapshotEvidence("snapshot-ready", profile.Name),
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = CreateVerifiedReinstallRecoveryEvidence("ready-installer")
        };

        var checklist = OfficialUninstallPreflightChecklistBuilder.Create(
            profile,
            readiness,
            path => path.EndsWith("Uninstall.exe", StringComparison.OrdinalIgnoreCase),
            snapshotHashResolver: _ => VerifiedSnapshotHash,
            now: VerifiedSnapshotNow);

        checklist.CanRequestExecution.Should().BeTrue();
        checklist.Operation.Should().NotBeNull();
        checklist.PrimaryActionText.Should().Be("\u8bf7\u6c42\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\u5668");
        checklist.Steps.Should().OnlyContain(step => step.State == OfficialUninstallPreflightStepState.Complete);
        checklist.Steps.Should().Contain(step =>
            step.Key == "snapshot" && step.Detail.Contains("\u4e0d\u80fd\u6062\u590d\u8f6f\u4ef6"));
    }

    [Fact]
    public void Official_uninstall_confirmation_exposes_preflight_checklist()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0"
        };

        var confirmation = OfficialUninstallConfirmationBuilder.Create(profile);

        confirmation.PreflightChecklist.CanRequestExecution.Should().BeFalse();
        confirmation.PreflightChecklist.Steps.Should().Contain(s => s.Key == "feature-enabled" && s.State == OfficialUninstallPreflightStepState.Blocked);
        confirmation.PreflightChecklist.PrimaryActionText.Should().Be("\u5148\u5b8c\u6210\u6062\u590d\u51c6\u5907");
    }

    [Fact]
    public void Official_uninstall_execution_gate_creates_high_risk_operation_only_after_all_preconditions()
    {
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis\Install",
            UninstallCommand = @"""D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe"" --oem-uninstall=0 --uninstall-entry=2"
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-42",
            SnapshotEvidence = CreateVerifiedSnapshotEvidence("snapshot-42", profile.Name),
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = CreateVerifiedReinstallRecoveryEvidence("marvis-installer")
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            path => path.EndsWith("Uninstall.exe", StringComparison.OrdinalIgnoreCase),
            snapshotHashResolver: _ => VerifiedSnapshotHash,
            now: VerifiedSnapshotNow);

        gate.CanRequestExecution.Should().BeTrue();
        gate.BlockingReasons.Should().BeEmpty();
        gate.Operation.Should().NotBeNull();
        gate.Operation!.Kind.Should().Be("uninstall.official.run");
        gate.Operation.Risk.Should().Be(RiskLevel.High);
        gate.Operation.IsDestructive.Should().BeTrue();
        gate.Operation.RequiresSnapshot.Should().BeTrue();
        gate.Operation.SnapshotId.Should().Be("snapshot-42");
        gate.Operation.ConfirmationAccepted.Should().BeFalse();
        gate.Operation.AffectedPaths.Should().Contain(@"D:\Software\Marvis\Install");
        gate.Operation.Arguments["executablePath"].Should().Be(@"D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe");
        gate.Operation.Arguments["arguments"].Should().Be("--oem-uninstall=0 --uninstall-entry=2");
        gate.Operation.Arguments["snapshotManifestPath"].Should().Be(@"D:\OMNIX\Snapshots\snapshot-42.json");
        gate.Operation.Arguments["snapshotSha256"].Should().Be(VerifiedSnapshotHash);
        gate.Operation.Arguments["snapshotCanRestoreApplication"].Should().Be(false);
    }

    [Fact]
    public void Official_uninstall_command_trust_allows_uninstaller_inside_install_directory()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"D:\Software\Marvis\Install\Marvis\Application\Uninstall.exe",
            installPath: @"D:\Software\Marvis\Install");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.Trusted);
        trust.IsTrusted.Should().BeTrue();
    }

    [Fact]
    public void Official_uninstall_command_trust_blocks_shell_wrappers()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
            installPath: @"D:\Software\Marvis\Install");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.BlockedShellWrapper);
        trust.IsTrusted.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_command_trust_blocks_uninstaller_outside_install_directory()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"D:\Downloads\Uninstall.exe",
            installPath: @"D:\Software\Marvis\Install");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.BlockedOutsideInstallDirectory);
        trust.IsTrusted.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_command_trust_allows_publisher_signed_external_uninstaller()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"C:\Program Files\Common Files\Vendor\Uninstall.exe",
            installPath: @"D:\Software\Vendor App\Install",
            expectedPublisher: "Vendor Inc.",
            executableSignatureSubject: "CN=Vendor Inc., O=Vendor Inc., C=US");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.TrustedPublisherSignature);
        trust.IsTrusted.Should().BeTrue();
    }

    [Fact]
    public void Official_uninstall_command_trust_blocks_external_uninstaller_with_signature_mismatch()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"C:\Program Files\Common Files\Vendor\Uninstall.exe",
            installPath: @"D:\Software\Vendor App\Install",
            expectedPublisher: "Vendor Inc.",
            executableSignatureSubject: "CN=Different Publisher, O=Different Publisher, C=US");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.BlockedPublisherSignatureMismatch);
        trust.IsTrusted.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_execution_gate_blocks_untrusted_shell_command()
    {
        var profile = new SoftwareProfile
        {
            Name = "Suspicious App",
            InstallPath = @"D:\Software\Suspicious\Install",
            UninstallCommand = @"""C:\Windows\System32\cmd.exe"" /c del /s /q C:\Users\Me\AppData\Local\Suspicious"
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-99",
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(profile, readiness, _ => true);

        gate.CanRequestExecution.Should().BeFalse();
        gate.CommandTrust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.BlockedShellWrapper);
        gate.Operation.Should().BeNull();
    }

    [Fact]
    public void Official_uninstall_command_trust_allows_interactive_windows_installer_uninstall()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"C:\Windows\System32\msiexec.exe",
            installPath: @"D:\Software\Example\Install",
            arguments: @"/x {12345678-1234-1234-1234-1234567890AB}");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.TrustedWindowsInstaller);
        trust.IsTrusted.Should().BeTrue();
    }

    [Fact]
    public void Official_uninstall_command_trust_blocks_silent_windows_installer_uninstall()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"C:\Windows\System32\msiexec.exe",
            installPath: @"D:\Software\Example\Install",
            arguments: @"/x {12345678-1234-1234-1234-1234567890AB} /quiet");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.BlockedSilentWindowsInstaller);
        trust.IsTrusted.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_command_trust_blocks_windows_installer_repair_or_install()
    {
        var trust = OfficialUninstallCommandTrustEvaluator.Evaluate(
            executablePath: @"C:\Windows\System32\msiexec.exe",
            installPath: @"D:\Software\Example\Install",
            arguments: @"/i {12345678-1234-1234-1234-1234567890AB}");

        trust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.BlockedUnsafeWindowsInstallerCommand);
        trust.IsTrusted.Should().BeFalse();
    }

    [Fact]
    public void Official_uninstall_execution_gate_accepts_interactive_windows_installer_uninstall_when_ready()
    {
        var profile = new SoftwareProfile
        {
            Name = "MSI Example",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""C:\Windows\System32\msiexec.exe"" /x {12345678-1234-1234-1234-1234567890AB}",
            InstalledSizeBytes = 1024
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-msi",
            SnapshotEvidence = CreateVerifiedSnapshotEvidence("snapshot-msi", profile.Name),
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = CreateVerifiedReinstallRecoveryEvidence("msi-source")
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            path => path.EndsWith("msiexec.exe", StringComparison.OrdinalIgnoreCase),
            snapshotHashResolver: _ => VerifiedSnapshotHash,
            now: VerifiedSnapshotNow);

        gate.CanRequestExecution.Should().BeTrue();
        gate.CommandTrust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.TrustedWindowsInstaller);
        gate.Operation.Should().NotBeNull();
        gate.Operation!.Arguments["executablePath"].Should().Be(@"C:\Windows\System32\msiexec.exe");
        gate.Operation.Arguments["arguments"].Should().Be(@"/x {12345678-1234-1234-1234-1234567890AB}");
    }

    [Fact]
    public void Official_uninstall_execution_gate_accepts_publisher_signed_external_uninstaller_when_ready()
    {
        var profile = new SoftwareProfile
        {
            Name = "Vendor App",
            Publisher = "Vendor Inc.",
            SignatureSubject = "CN=Vendor Inc., O=Vendor Inc., C=US",
            InstallPath = @"D:\Software\Vendor App\Install",
            UninstallCommand = @"""C:\Program Files\Common Files\Vendor\Uninstall.exe"" /remove",
            InstalledSizeBytes = 1024
        };
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = "snapshot-signed",
            SnapshotEvidence = CreateVerifiedSnapshotEvidence("snapshot-signed", profile.Name),
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = CreateVerifiedReinstallRecoveryEvidence("vendor-installer")
        };

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            path => path.EndsWith("Uninstall.exe", StringComparison.OrdinalIgnoreCase),
            snapshotHashResolver: _ => VerifiedSnapshotHash,
            now: VerifiedSnapshotNow);

        gate.CanRequestExecution.Should().BeTrue();
        gate.CommandTrust.Decision.Should().Be(OfficialUninstallCommandTrustDecision.TrustedPublisherSignature);
        gate.Operation.Should().NotBeNull();
        gate.Operation!.Arguments["executablePath"].Should().Be(@"C:\Program Files\Common Files\Vendor\Uninstall.exe");
    }

    [Fact]
    public void Agent_skill_catalog_marks_marvis_like_capabilities_by_safety_level()
    {
        var catalog = AgentSkillCatalog.CreateDefault();

        catalog.Skills.Should().Contain(s =>
            s.Category == AgentSkillCategory.SystemDiagnosis
            && s.ExecutionMode == AgentExecutionMode.ReadOnly);

        catalog.Skills.Should().Contain(s =>
            s.Category == AgentSkillCategory.ProcessAndServiceManagement
            && s.ExecutionMode == AgentExecutionMode.PlanOnly
            && s.Risk == RiskLevel.High);

        catalog.Skills.Should().Contain(s =>
            s.Category == AgentSkillCategory.SystemTools
            && s.ExecutionMode == AgentExecutionMode.OpenSystemTool);
    }

    [Fact]
    public void Agent_skill_cards_show_next_step_and_safety_mode_for_beginner_users()
    {
        var cards = AgentSkillCardPresenter.CreateDefault();

        cards.Should().HaveCount(8);

        var service = cards.Single(card => card.Category == AgentSkillCategory.ProcessAndServiceManagement);
        service.ModeLabel.Should().Contain("\u53ea\u751f\u6210\u65b9\u6848");
        service.RiskLabel.Should().Contain("\u9ad8");
        service.NextStepLabel.Should().Contain("\u5148\u67e5\u770b");
        service.SafetyHint.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u7ed3\u675f\u8fdb\u7a0b");
        service.SafetyHint.Should().Contain("\u7981\u7528\u670d\u52a1");

        var tools = cards.Single(card => card.Category == AgentSkillCategory.SystemTools);
        tools.ModeLabel.Should().Contain("\u53ea\u6253\u5f00");
        tools.NextStepLabel.Should().Contain("\u6253\u5f00\u7cfb\u7edf\u5de5\u5177");
        tools.SafetyHint.Should().Contain("\u4e0d\u4f1a\u66ff\u4f60\u70b9\u51fb");

        var session = cards.Single(card => card.Category == AgentSkillCategory.InputAndSession);
        session.RiskLabel.Should().Contain("\u9ad8");
        session.NextStepLabel.Should().Contain("\u51c6\u5907\u786e\u8ba4");
        session.SafetyHint.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u9501\u5c4f");
        session.SafetyHint.Should().Contain("\u5173\u673a");

        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("{Binding NextStepLabel}");
        xaml.Should().Contain("{Binding SafetyHint}");
        code.Should().Contain("AgentSkillCardPresenter.CreateDefault()");
    }

    [Fact]
    public void Agent_system_tool_shortcuts_are_allowlisted_open_only_and_confirm_risky_tools()
    {
        var shortcuts = SystemToolShortcutCatalog.CreateDefault();

        shortcuts.Select(shortcut => shortcut.Id).Should().Equal(
        [
            "task-manager",
            "recycle-bin",
            "device-manager",
            "disk-management",
            "event-viewer",
            "windows-security",
            "registry-editor"
        ]);

        shortcuts.Should().OnlyContain(shortcut => shortcut.IsOpenOnly);
        shortcuts.Should().OnlyContain(shortcut => !string.IsNullOrWhiteSpace(shortcut.Command));
        shortcuts.Should().OnlyContain(shortcut =>
            !shortcut.Command.Contains("cmd", StringComparison.OrdinalIgnoreCase)
            && !shortcut.Command.Contains("powershell", StringComparison.OrdinalIgnoreCase));

        var taskManager = shortcuts.Single(shortcut => shortcut.Id == "task-manager");
        taskManager.Command.Should().Be("taskmgr.exe");
        taskManager.RequiresConfirmation.Should().BeFalse();
        taskManager.SafetyHint.Should().Contain("\u53ea\u6253\u5f00");

        var recycleBin = shortcuts.Single(shortcut => shortcut.Id == "recycle-bin");
        recycleBin.Command.Should().Be("explorer.exe");
        recycleBin.Arguments.Should().Be("shell:RecycleBinFolder");
        recycleBin.Risk.Should().Be(RiskLevel.Low);
        recycleBin.RequiresConfirmation.Should().BeFalse();
        recycleBin.IsOpenOnly.Should().BeTrue();
        recycleBin.SafetyHint.Should().Contain("只打开").And.Contain("不会清空");

        var registry = shortcuts.Single(shortcut => shortcut.Id == "registry-editor");
        registry.Risk.Should().Be(RiskLevel.High);
        registry.RequiresConfirmation.Should().BeTrue();
        registry.SafetyHint.Should().Contain("\u9700\u8981\u4f60\u786e\u8ba4");

        var unknown = SystemToolShortcutCatalog.FindById("not-a-tool");
        unknown.Should().BeNull();
    }

    [Fact]
    public void C_drive_recycle_bin_action_reuses_exact_allowlisted_open_only_handler()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(
            code,
            "private async void OpenCDriveRootCauseAction_Click",
            "private void OpenSystemTool_Click");

        xaml.Should().Contain("AutomationProperties.AutomationId=\"{Binding ActionAutomationId}\"")
            .And.Contain("Content=\"{Binding ActionLabel}\"")
            .And.Contain("Click=\"OpenCDriveRootCauseAction_Click\"")
            .And.Contain("<DataTrigger Binding=\"{Binding HasAction}\" Value=\"True\">");
        handler.Should().Contain("CDriveRootCauseAction.OpenRecycleBin")
            .And.Contain("CDriveRootCauseAction.OpenCDriveApps")
            .And.Contain("CDriveRootCauseAction.ReviewPersonalStorage")
            .And.Contain("CDriveRootCauseAction.ReviewCleanupRecommendations")
            .And.Contain("SystemToolShortcutCatalog.RecycleBinId")
            .And.Contain("OpenAllowlistedSystemTool")
            .And.Contain("AppCatalogFilter.CDrive")
            .And.Contain("PersonalStorageFindingsListBox.BringIntoView")
            .And.Contain("RecommendationsListBox.SelectedItem")
            .And.NotContain("Process.Start")
            .And.NotContain("SHEmptyRecycleBin")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("ExecuteRecommendation")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete");
    }

    [Fact]
    public void Agent_page_binds_system_tool_shortcuts_to_explicit_open_buttons()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AgentSystemToolListBox");
        xaml.Should().Contain("Click=\"OpenSystemTool_Click\"");
        xaml.Should().Contain("{Binding SafetyHint}");
        code.Should().Contain("SystemToolShortcutCatalog.CreateDefault()");
        code.Should().Contain("OpenSystemTool_Click");
        code.Should().Contain("SystemToolShortcutCatalog.FindById");
        code.Should().Contain("ProcessStartInfo");
        code.Should().Contain("UseShellExecute = true");
    }

    [Fact]
    public void Agent_windows_settings_shortcuts_are_ms_settings_allowlisted_and_open_only()
    {
        var shortcuts = WindowsSettingsShortcutCatalog.CreateDefault();

        shortcuts.Select(shortcut => shortcut.Id).Should().Equal(
        [
            "storage",
            "installed-apps",
            "default-save-locations",
            "startup-apps",
            "power",
            "network",
            "bluetooth",
            "sound",
            "display"
        ]);

        shortcuts.Should().OnlyContain(shortcut => shortcut.IsOpenOnly);
        shortcuts.Should().OnlyContain(shortcut => shortcut.Uri.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase));
        shortcuts.Should().OnlyContain(shortcut =>
            !shortcut.Uri.Contains("cmd", StringComparison.OrdinalIgnoreCase)
            && !shortcut.Uri.Contains("powershell", StringComparison.OrdinalIgnoreCase)
            && !shortcut.Uri.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

        var network = shortcuts.Single(shortcut => shortcut.Id == "network");
        network.Uri.Should().Be("ms-settings:network");
        network.RequiresConfirmation.Should().BeFalse();
        network.SafetyHint.Should().Contain("\u53ea\u6253\u5f00");
        network.SafetyHint.Should().Contain("\u4e0d\u4f1a\u66ff\u4f60\u5207\u6362");

        var apps = shortcuts.Single(shortcut => shortcut.Id == "installed-apps");
        apps.Risk.Should().Be(RiskLevel.Medium);
        apps.RequiresConfirmation.Should().BeTrue();
        apps.SafetyHint.Should().Contain("\u4e0d\u4f1a\u66ff\u4f60\u5378\u8f7d");

        var defaultSaveLocations = shortcuts.Single(shortcut =>
            shortcut.Id == "default-save-locations");
        defaultSaveLocations.Uri.Should().Be("ms-settings:savelocations");
        defaultSaveLocations.Risk.Should().Be(RiskLevel.Medium);
        defaultSaveLocations.RequiresConfirmation.Should().BeTrue();
        defaultSaveLocations.IsOpenOnly.Should().BeTrue();
        defaultSaveLocations.SafetyHint.Should().Contain("不会替你安装");

        var startup = shortcuts.Single(shortcut => shortcut.Id == "startup-apps");
        startup.Uri.Should().Be("ms-settings:startupapps");
        startup.Risk.Should().Be(RiskLevel.Medium);
        startup.RequiresConfirmation.Should().BeTrue();
        startup.IsOpenOnly.Should().BeTrue();
        startup.SafetyHint.Should().Contain("不会替你切换开关");

        shortcuts.Where(shortcut => shortcut.Risk >= RiskLevel.Medium)
            .Should()
            .OnlyContain(shortcut => shortcut.RequiresConfirmation);
        shortcuts.Where(shortcut => shortcut.Risk == RiskLevel.Low)
            .Should()
            .OnlyContain(shortcut => !shortcut.RequiresConfirmation);

        WindowsSettingsShortcutCatalog.FindById("not-a-setting").Should().BeNull();
    }

    [Fact]
    public void Msix_capability_panel_exposes_only_the_allowlisted_storage_settings_handoff()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = ExtractMethod(
            code,
            "private void OpenInstallerStorageSettings_Click",
            "private void OpenWindowsSettings_Click");

        xaml.Should().Contain("x:Name=\"OpenInstallerStorageSettingsButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"OpenInstallerStorageSettingsButton\"")
            .And.Contain("Content=\"打开新应用保存位置\"")
            .And.Contain("Click=\"OpenInstallerStorageSettings_Click\"");
        code.Should().Contain("OpenInstallerStorageSettingsButton.Visibility")
            .And.Contain("InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId");
        handler.Should().Contain("_lastInstallerCapability")
            .And.Contain("InstallerRoutingCapabilityMode.WindowsManagedStorage")
            .And.Contain("OpenAllowlistedWindowsSettings")
            .And.NotContain("Process.Start")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor");
    }

    [Fact]
    public void Agent_page_binds_windows_settings_shortcuts_to_explicit_open_buttons()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AgentCapabilityScrollViewer");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentCapabilityScrollViewer\"");
        xaml.Should().Contain("AgentWindowsSettingsListBox");
        xaml.IndexOf("AgentWindowsSettingsListBox", StringComparison.Ordinal)
            .Should()
            .BeLessThan(xaml.IndexOf("AgentSystemToolListBox", StringComparison.Ordinal));
        xaml.Should().Contain("Click=\"OpenWindowsSettings_Click\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"{Binding Id, StringFormat=AgentWindowsSettingsOpenButton_{0}}\"");
        xaml.Should().Contain("{Binding SafetyHint}");
        code.Should().Contain("WindowsSettingsShortcutCatalog.CreateDefault()");
        code.Should().Contain("OpenWindowsSettings_Click");
        code.Should().Contain("WindowsSettingsShortcutCatalog.FindById");
        code.Should().Contain("shortcut.RequiresConfirmation");
        code.Should().Contain("UseShellExecute = true");
    }

    [Fact]
    public void Agent_next_step_panel_turns_local_signals_into_safe_guidance()
    {
        var summary = new HealthCheckSummary
        {
            OverallScore = 72,
            Dimensions =
            [
                new() { Name = "\u78c1\u76d8\u5065\u5eb7", Result = "C \u76d8\u53ef\u91ca\u653e 7.3 GB", Rating = "\u6709\u4f18\u5316\u7a7a\u95f4" },
                new() { Name = "\u81ea\u542f\u52a8\u9879", Result = "23 \u9879", Rating = "\u5efa\u8bae\u68c0\u67e5" }
            ],
            KeyFindings =
            [
                new() { Text = "\u56de\u6536\u7ad9\u79ef\u538b 7.3 GB\uff0c\u5efa\u8bae\u6e05\u7a7a", Action = RecommendationAction.Clean, Risk = RiskLevel.Low },
                new() { Text = "OneDrive \u76f8\u5173\u81ea\u542f\u52a8\u504f\u591a", Action = RecommendationAction.DisableStartup, Risk = RiskLevel.Medium }
            ]
        };
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Ollama",
                InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
                CDriveWritePaths = [@"C:\Users\Me\.ollama\models"]
            },
            new SoftwareProfile
            {
                Name = "Marvis",
                InstallPath = @"D:\Software\Marvis",
                RunningProcesses = ["Marvis", "MarvisAgent"],
                Services = ["MarvisSvr"]
            }
        };

        var panel = AgentNextStepPresenter.Create(summary, profiles);
        var empty = AgentNextStepPresenter.Create(null, []);

        panel.Title.Should().Contain("C \u76d8");
        panel.Summary.Should().Contain("\u5148");
        panel.Reasons.Should().Contain(reason => reason.Contains("\u56de\u6536\u7ad9"));
        panel.Reasons.Should().Contain(reason => reason.Contains("1 \u4e2a\u5e94\u7528\u5360\u7528\u6216\u5199\u5165 C \u76d8"));
        panel.Reasons.Should().Contain(reason => reason.Contains("1 \u4e2a\u5e94\u7528\u6b63\u5728\u540e\u53f0\u5e38\u9a7b"));
        panel.SafeNextActions.Should().Contain(action => action.Contains("\u6253\u5f00 C \u76d8\u6e05\u7406"));
        panel.SafeNextActions.Should().Contain(action => action.Contains("\u5e94\u7528\u7ba1\u7406"));
        panel.BlockedActions.Should().Contain(action => action.Contains("\u4e0d\u4f1a\u76f4\u63a5\u5220\u9664"));
        panel.BlockedActions.Should().Contain(action => action.Contains("\u5feb\u7167"));
        panel.CanExecuteDirectly.Should().BeFalse();
        panel.PrivacyLine.Should().Contain("\u672c\u5730\u6458\u8981");
        empty.Title.Should().Contain("\u5148\u505a\u4e00\u6b21\u4f53\u68c0");
        empty.SafeNextActions.Should().Contain(action => action.Contains("\u5f00\u59cb\u4f53\u68c0"));
    }

    [Fact]
    public void Agent_next_step_panel_exposes_navigation_only_actions()
    {
        var summary = new HealthCheckSummary
        {
            OverallScore = 72,
            Dimensions = [],
            KeyFindings =
            [
                new() { Text = "\u56de\u6536\u7ad9\u79ef\u538b 7.3 GB\uff0c\u5efa\u8bae\u6e05\u7a7a", Action = RecommendationAction.Clean, Risk = RiskLevel.Low }
            ]
        };
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Ollama",
                InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
                CDriveWritePaths = [@"C:\Users\Me\.ollama\models"]
            },
            new SoftwareProfile
            {
                Name = "Marvis",
                InstallPath = @"D:\Software\Marvis",
                RunningProcesses = ["Marvis"],
                Services = ["MarvisSvr"]
            }
        };

        var panel = AgentNextStepPresenter.Create(summary, profiles);
        var empty = AgentNextStepPresenter.Create(null, []);
        var allowedPages = new[] { "Home", "Apps", "CDrive", "Install", "Timeline", "Agent" };

        panel.NavigationActions.Should().OnlyContain(action => action.IsNavigationOnly);
        panel.NavigationActions.Should().OnlyContain(action => allowedPages.Contains(action.TargetPage));
        panel.NavigationActions.Should().Contain(action =>
            action.TargetPage == "CDrive"
            && action.Label.Contains("C \u76d8")
            && action.Description.Contains("\u4e0d\u4f1a\u76f4\u63a5\u6e05\u7406"));
        panel.NavigationActions.Should().Contain(action =>
            action.TargetPage == "Apps"
            && action.Label.Contains("\u5e94\u7528\u7ba1\u7406")
            && action.Description.Contains("\u5148\u67e5\u770b"));
        empty.NavigationActions.Should().Contain(action => action.TargetPage == "Home");
        empty.NavigationActions.Should().Contain(action => action.TargetPage == "Apps");
    }

    [Fact]
    public void Agent_next_step_prioritizes_many_resident_apps_before_c_drive_apps()
    {
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Sync Tool",
                InstallPath = @"C:\Program Files\Sync Tool",
                RunningProcesses = ["SyncTool"],
                StartupEntries = ["Sync Tool"]
            },
            new SoftwareProfile
            {
                Name = "Chat Helper",
                InstallPath = @"D:\Software\ChatHelper\Install",
                RunningProcesses = ["ChatHelper"],
                Services = ["ChatHelperService"]
            },
            new SoftwareProfile
            {
                Name = "Updater",
                InstallPath = @"D:\Software\Updater\Install",
                ScheduledTasks = [@"\Updater Daily"]
            },
            new SoftwareProfile
            {
                Name = "Agent Sidecar",
                InstallPath = @"D:\Agent\Sidecar\Install",
                RunningProcesses = ["AgentSidecar"]
            }
        };

        var panel = AgentNextStepPresenter.Create(null, profiles);

        panel.Title.Should().Contain("\u540e\u53f0\u5e38\u9a7b");
        panel.Summary.Should().Contain("\u5f00\u673a");
        panel.Reasons.Should().Contain(reason => reason.Contains("4 \u4e2a\u5e94\u7528\u6b63\u5728\u540e\u53f0\u5e38\u9a7b"));
        panel.SafeNextActions[0].Should().Contain("\u540e\u53f0\u5e38\u9a7b");
        panel.SafeNextActions.Should().Contain(action => action.Contains("C \u76d8"));
        panel.NavigationActions[0].TargetPage.Should().Be("Apps");
        panel.NavigationActions[0].Description.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed");
        panel.CanExecuteDirectly.Should().BeFalse();
        panel.NavigationActions.Should().OnlyContain(action => action.IsNavigationOnly);
        panel.BlockedActions.Should().Contain(action => action.Contains("\u7981\u7528\u670d\u52a1"));
    }

    [Fact]
    public void Agent_background_review_summarizes_resident_apps_without_technical_dump_or_execution()
    {
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Marvis",
                Category = SoftwareCategory.Ai,
                RunningProcesses = ["Marvis", "MarvisHelper"],
                Services = ["MarvisSvr"]
            },
            new SoftwareProfile
            {
                Name = "Cloud Sync",
                Category = SoftwareCategory.Normal,
                StartupEntries = ["Cloud Sync"],
                ScheduledTasks = [@"\Cloud Sync Update"]
            },
            new SoftwareProfile
            {
                Name = "Driver Center",
                Category = SoftwareCategory.SystemTool,
                Services = ["DriverCenterService"]
            },
            new SoftwareProfile
            {
                Name = "Notepad",
                Category = SoftwareCategory.Normal
            }
        };

        var review = AgentBackgroundReviewPresenter.Create(profiles);

        review.IsVisible.Should().BeTrue();
        review.Summary.Should().Contain("3 \u4e2a");
        review.Summary.Should().Contain("\u540e\u53f0\u5e38\u9a7b");
        review.Items.Should().HaveCount(3);
        review.Items.Should().OnlyContain(item => !item.CanExecuteDirectly);
        review.Items.Should().OnlyContain(item => item.EvidenceSummary.Contains("\u540e\u53f0") || item.EvidenceSummary.Contains("\u81ea\u542f\u52a8") || item.EvidenceSummary.Contains("\u8ba1\u5212\u4efb\u52a1"));
        review.Items.Select(item => item.AppName).Should().Contain(["Marvis", "Cloud Sync", "Driver Center"]);
        review.Items.Should().Contain(item => item.AppName == "Driver Center" && item.RiskLabel.Contains("\u4e0d\u5efa\u8bae\u76f4\u63a5\u52a8"));
        review.Items.Should().Contain(item => item.AppName == "Cloud Sync" && item.RecommendedNextStep.Contains("\u751f\u6210\u65b9\u6848"));
        review.Items.SelectMany(item => new[] { item.EvidenceSummary, item.RecommendedNextStep })
            .Should()
            .NotContain(value => value.Contains("MarvisSvr") || value.Contains("DriverCenterService") || value.Contains(@"\Cloud Sync Update"));
        review.SafetyLine.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed");
    }

    [Fact]
    public void Agent_startup_service_plan_preview_is_auditable_and_non_executable()
    {
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Cloud Sync",
                Category = SoftwareCategory.Normal,
                StartupEntries = ["Cloud Sync"],
                Services = ["CloudSyncService"]
            },
            new SoftwareProfile
            {
                Name = "Driver Center",
                Category = SoftwareCategory.SystemTool,
                Services = ["DriverCenterService"],
                ScheduledTasks = [@"\Driver Center Update"]
            },
            new SoftwareProfile
            {
                Name = "Chat",
                Category = SoftwareCategory.Normal,
                RunningProcesses = ["Chat"]
            }
        };

        var plan = AgentStartupServicePlanPresenter.Create(profiles);

        plan.IsVisible.Should().BeTrue();
        plan.CanExecuteDirectly.Should().BeFalse();
        plan.RequiresSnapshot.Should().BeTrue();
        plan.Title.Should().Contain("\u65b9\u6848\u9884\u89c8");
        plan.Summary.Should().Contain("3 \u4e2a");
        plan.Summary.Should().Contain("\u53ea\u751f\u6210\u65b9\u6848");
        plan.EvidenceLines.Should().Contain(line => line.Contains("1 \u4e2a\u81ea\u542f\u52a8"));
        plan.EvidenceLines.Should().Contain(line => line.Contains("2 \u4e2a\u540e\u53f0\u670d\u52a1"));
        plan.PlanSteps.Should().Contain(step => step.Contains("\u5148\u5224\u65ad\u662f\u5426\u5fc5\u8981"));
        plan.PlanSteps.Should().Contain(step => step.Contains("\u666e\u901a\u5e94\u7528"));
        plan.RequiredBeforeExecution.Should().Contain(item => item.Contains("\u5feb\u7167"));
        plan.RequiredBeforeExecution.Should().Contain(item => item.Contains("\u56de\u6eda"));
        plan.BlockedActions.Should().Contain(item => item.Contains("\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\u670d\u52a1"));
        plan.BlockedActions.Should().Contain(item => item.Contains("\u4e0d\u4f1a\u76f4\u63a5\u7ed3\u675f\u8fdb\u7a0b"));
        plan.SafetyLine.Should().Contain("\u672c\u5730\u64cd\u4f5c\u8ba1\u5212");
        plan.EvidenceLines.SelectMany(line => new[] { line })
            .Should()
            .NotContain(line => line.Contains("CloudSyncService") || line.Contains("DriverCenterService") || line.Contains(@"\Driver Center Update"));
    }

    [Fact]
    public void Agent_page_contains_background_review_panel_and_refresh_binding()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AgentBackgroundReviewPanel");
        xaml.Should().Contain("AgentBackgroundReviewSummaryTextBlock");
        xaml.Should().Contain("AgentBackgroundReviewItemsListBox");
        xaml.Should().Contain("AgentBackgroundReviewSafetyTextBlock");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentBackgroundReviewPanel\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentBackgroundReviewItemsListBox\"");
        xaml.IndexOf("AgentBackgroundReviewPanel", StringComparison.Ordinal)
            .Should()
            .BeLessThan(xaml.IndexOf("AgentNextStepReasonsListBox", StringComparison.Ordinal));
        xaml.IndexOf("AgentStartupServicePlanPanel", StringComparison.Ordinal)
            .Should()
            .BeLessThan(xaml.IndexOf("AgentBackgroundReviewItemsListBox", StringComparison.Ordinal));
        xaml.Should().Contain("AgentStartupServicePlanPanel");
        xaml.Should().Contain("AgentStartupServicePlanTitleTextBlock");
        xaml.Should().Contain("AgentStartupServicePlanSummaryTextBlock");
        xaml.Should().Contain("AgentStartupServicePlanStepsListBox");
        xaml.Should().Contain("AgentStartupServicePlanSafetyTextBlock");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentStartupServicePlanPanel\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentStartupServicePlanTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentStartupServicePlanSummaryTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentStartupServicePlanStepsListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentStartupServicePlanSafetyTextBlock\"");
        code.Should().Contain("AgentBackgroundReviewPresenter.Create");
        code.Should().Contain("AgentStartupServicePlanPresenter.Create");
        code.Should().Contain("AgentBackgroundReviewPanel.Visibility");
        code.Should().Contain("AgentStartupServicePlanPanel.Visibility");
        code.Should().Contain("AgentBackgroundReviewItemsListBox.ItemsSource = backgroundReview.Items;");
        code.Should().Contain("AgentStartupServicePlanStepsListBox.ItemsSource = startupServicePlan.PlanSteps;");
    }

    [Fact]
    public void Agent_page_contains_next_step_panel_and_refresh_hooks()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AgentNextStepTitleTextBlock");
        xaml.Should().Contain("AgentNextStepReasonsListBox");
        xaml.Should().Contain("AgentNextStepActionsListBox");
        xaml.Should().Contain("AgentBlockedActionsListBox");
        xaml.Should().Contain("AgentNextStepActionButtonsItemsControl");
        xaml.Should().Contain("Click=\"AgentNextAction_Click\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"{Binding AutomationId}\"");
        xaml.Should().Contain("Tag=\"{Binding}\"");
        code.Should().Contain("LoadAgentNextSteps();");
        code.Should().Contain("AgentNextStepPresenter.Create");
        code.Should().Contain("AgentNextStepActionButtonsItemsControl.ItemsSource = panel.NavigationActions;");
        code.Should().Contain("AgentNextAction_Click");
        code.Should().Contain("ShowPage(action.TargetPage);");
        code.Should().Contain("await OpenAgentAppCatalogFilterAsync(appFilter);");
        code.Should().Contain("_lastHealthSummary = summary;");
    }

    [Fact]
    public void Undo_center_has_stable_visual_proof_hooks_for_timeline_quarantine_and_restore()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("x:Name=\"TimelinePage\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"LoadTimelineButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineDescriptionTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineQuarantinePolicyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineListBox\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineRestoreLineTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineRestoreButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineTechnicalDetailsExpander\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"TimelineTechnicalDetailsListBox\"");
        xaml.Should().Contain("ItemsSource=\"{Binding TechnicalDetails}\"");
        xaml.Should().Contain("Click=\"RestoreTimeline_Click\"");
        xaml.IndexOf("TimelineTechnicalDetailsExpander", StringComparison.Ordinal)
            .Should().BeGreaterThan(xaml.IndexOf("Text=\"{Binding Detail}\"", StringComparison.Ordinal));

        code.Should().Contain("if (page == \"Timeline\")");
        code.Should().Contain("LoadTimelineAsync()");
        code.Should().Contain("ActionTimelinePresenter.CreateItem");
        code.Should().Contain("FileQuarantineService");
        code.Should().NotContain("File.Delete(");
        code.Should().NotContain("Directory.Delete(");
    }

    [Fact]
    public void Undo_center_gui_smoke_uses_isolated_storage_overrides()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-undo-center-smoke.ps1"));

        script.Should().Contain("$env:OMNIX_ENTROPY_DATA_ROOT");
        script.Should().Contain("$env:OMNIX_ENTROPY_QUARANTINE_ROOT");
        script.Should().Contain("qa-undo-center-data");
        script.Should().Contain("qa-undo-center-quarantine");
        script.Should().Contain("Remove-Item");
        script.Should().Contain("finally");
    }

    [Fact]
    public void Undo_center_gui_smoke_seeds_restorable_data_without_invoking_restore()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-undo-center-smoke.ps1"));

        script.Should().Contain("Seed-RestorableUndoRecord");
        script.Should().Contain("seed-undo-center");
        script.Should().Contain("Timeline restore button should be enabled for seeded restorable data.");
        script.Should().Contain("restoreButtonEnabled = $true");
        script.Should().Contain("TimelineTechnicalDetailsExpander");
        script.Should().Contain("technicalDetailsExpanderFound = $true");
        script.Should().NotContain("Invoke-Element $restoreButton");
        script.Should().NotContain("RestoreTimeline_Click");
    }

    [Fact]
    public void Undo_center_gui_smoke_uses_shared_wpf_smoke_helpers()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-undo-center-smoke.ps1"));
        var helper = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("Initialize-WpfSmokeAutomation");
        helper.Should().Contain("function Initialize-WpfSmokeAutomation");
        helper.Should().Contain("function Find-ByAutomationId");
        helper.Should().Contain("function Wait-Until");
        helper.Should().Contain("function Invoke-Element");
        helper.Should().Contain("function Save-WindowScreenshot");
    }

    [Fact]
    public void App_drawer_gui_smoke_uses_shared_wpf_smoke_helpers()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-app-drawer-preview-smoke.ps1"));
        var helper = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("Initialize-WpfSmokeAutomation");
        script.Should().Contain("Find-ByAutomationId");
        script.Should().Contain("Invoke-Element");
        script.Should().Contain("Save-DesktopScreenshot");
        script.Should().Contain("catch [System.Runtime.InteropServices.COMException]");
        script.Should().Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE");
        script.Should().Contain("OMNIX Preview Fixture");
        script.Should().Contain("if ($found.Count -gt 0)");
        script.Should().Contain("[PSCustomObject]@{ Items = @($found) }");
        script.Should().Contain("$items = @($itemResult.Items)");
        script.Should().Contain("finally");
        script.Should().NotContain("Add-Type -AssemblyName UIAutomationClient");
        helper.Should().Contain("function Save-DesktopScreenshot");
    }

    [Fact]
    public void Agent_system_tools_gui_smoke_uses_shared_wpf_smoke_helpers()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-agent-system-tools-smoke.ps1"));
        var helper = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("Initialize-WpfSmokeAutomation");
        script.Should().Contain("Find-ByAutomationId");
        script.Should().Contain("Invoke-Element");
        script.Should().Contain("Save-WindowScreenshot");
        script.Should().NotContain("Add-Type -AssemblyName UIAutomationClient");
        script.Should().NotContain("function Find-ByAutomationId");
        script.Should().NotContain("function Invoke-Element");
        helper.Should().Contain("function Save-WindowScreenshot");
    }

    [Fact]
    public void Agent_settings_confirm_cancel_gui_smoke_uses_shared_wpf_smoke_helpers()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-agent-settings-confirm-cancel-smoke.ps1"));
        var helper = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("Initialize-WpfSmokeAutomation");
        script.Should().Contain("Find-ByAutomationId");
        script.Should().Contain("Wait-Until");
        script.Should().Contain("Invoke-Element");
        script.Should().Contain("Save-WindowScreenshot");
        script.Should().NotContain("Add-Type -AssemblyName UIAutomationClient");
        script.Should().NotContain("function Find-ByAutomationId");
        script.Should().NotContain("function Wait-Until");
        script.Should().NotContain("function Invoke-Element");
        script.Should().NotContain("function Save-WindowScreenshot");
        helper.Should().Contain("function Save-WindowScreenshot");
    }

    [Fact]
    public void Agent_settings_confirm_cancel_gui_smoke_defends_root_window_search()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-agent-settings-confirm-cancel-smoke.ps1"));
        var finderStart = script.IndexOf("function Find-WindowForProcess", StringComparison.Ordinal);
        var finderEnd = script.IndexOf("function Get-SettingsProcessIds", finderStart, StringComparison.Ordinal);
        var finder = script[finderStart..finderEnd];

        finder.Should().Contain("[System.Windows.Automation.TreeScope]::Children");
        finder.Should().Contain("[System.Windows.Automation.TreeScope]::Descendants");
        finder.Should().Contain("try");
        finder.Should().Contain("catch");
        finder.IndexOf("[System.Windows.Automation.TreeScope]::Children", StringComparison.Ordinal)
            .Should().BeLessThan(finder.IndexOf("[System.Windows.Automation.TreeScope]::Descendants", StringComparison.Ordinal));
    }

    [Fact]
    public void Agent_settings_confirm_cancel_gui_smoke_uses_native_window_fallback()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-agent-settings-confirm-cancel-smoke.ps1"));

        script.Should().Contain("EnumWindows");
        script.Should().Contain("GetWindowThreadProcessId");
        script.Should().Contain("Get-TopLevelWindowHandlesForProcess");
        script.Should().Contain("[System.Windows.Automation.AutomationElement]::FromHandle");
    }

    [Fact]
    public void Agent_background_review_gui_smoke_uses_shared_wpf_smoke_helpers()
    {
        var script = File.ReadAllText(FindRepositoryFile(".omx", "gui-agent-background-review-smoke.ps1"));
        var helper = File.ReadAllText(FindRepositoryFile(".omx", "wpf-smoke-helpers.ps1"));

        script.Should().Contain("wpf-smoke-helpers.ps1");
        script.Should().Contain("Initialize-WpfSmokeAutomation");
        script.Should().Contain("Find-ByAutomationId");
        script.Should().Contain("Wait-Until");
        script.Should().Contain("Invoke-Element");
        script.Should().Contain("Save-WindowScreenshot");
        script.Should().NotContain("Add-Type -AssemblyName UIAutomationClient");
        script.Should().NotContain("function Find-ByAutomationId");
        script.Should().NotContain("function Wait-Until");
        script.Should().NotContain("function Invoke-Element");
        script.Should().NotContain("function Save-WindowScreenshot");
        helper.Should().Contain("function Save-WindowScreenshot");
    }

    [Fact]
    public void Development_docs_describe_storage_overrides_as_test_only()
    {
        var doc = File.ReadAllText(FindRepositoryFile("docs", "development", "gui-smokes.md"));

        doc.Should().Contain("OMNIX_ENTROPY_DATA_ROOT");
        doc.Should().Contain("OMNIX_ENTROPY_QUARANTINE_ROOT");
        doc.Should().Contain("development and GUI smoke tests only");
        doc.Should().Contain("Do not expose these as normal user settings");
        doc.Should().Contain("restore previous environment values");
        doc.Should().Contain("Css.SmokeTools seed-undo-center");
    }

    [Fact]
    public void Agent_left_card_has_single_clean_identity_copy()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var start = xaml.IndexOf("x:Name=\"AgentConsultationTab\"", StringComparison.Ordinal);
        var end = xaml.IndexOf("x:Name=\"AgentCapabilitiesTab\"", start, StringComparison.Ordinal);
        var leftCard = xaml[start..end];

        leftCard.Should().Contain("&#x7535;&#x8111;&#x7CFB;&#x7EDF;&#x8FD0;&#x7EF4;&#x7BA1;&#x5BB6;");
        leftCard.Should().Contain("&#x6211;&#x4F1A;&#x89E3;&#x91CA;&#x95EE;&#x9898;");
        leftCard.Should().Contain("AgentNextStepActionButtonsItemsControl");
        leftCard.Should().NotContain("FontSize=\"15\" Foreground=\"#4B5563\" Margin=\"0,8,0,0\"");
        leftCard.Should().NotContain("Margin=\"0,22,0,0\"");
    }

    [Fact]
    public void Uninstall_plan_starts_with_official_uninstaller_and_quarantines_only_low_risk_residue()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe""",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            Services = ["ExampleService"]
        };

        var plan = UninstallPlanBuilder.Create(profile);

        plan.RequiresUserConfirmation.Should().BeTrue();
        plan.Steps[0].Kind.Should().Be(UninstallStepKind.RunOfficialUninstaller);
        plan.Steps[0].Command.Should().Be(profile.UninstallCommand);
        plan.ResidueGroups.Should().Contain(g => g.Risk == RiskLevel.Low && g.CanMoveToQuarantine);
        plan.ResidueGroups.Should().Contain(g => g.Risk == RiskLevel.High && !g.CanMoveToQuarantine);
    }

    private static OfficialUninstallRecoveryEvidence CreateVerifiedReinstallRecoveryEvidence(string reference) =>
        new()
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = reference,
            CanRecoverApplication = true,
            UserDataBackupConfirmed = true
        };

    private static OfficialUninstallSnapshotEvidence CreateVerifiedSnapshotEvidence(
        string snapshotId,
        string softwareName) =>
        new()
        {
            SnapshotId = snapshotId,
            ManifestPath = @"D:\OMNIX\Snapshots\" + snapshotId + ".json",
            SoftwareName = softwareName,
            CreatedAtUtc = VerifiedSnapshotNow,
            Sha256 = VerifiedSnapshotHash,
            CanRestoreApplication = false
        };

    private static Recommendation CreateUnexpectedRootRecommendation(string name, long sizeBytes) =>
        new()
        {
            Title = "\u975e\u9884\u671f\u6839\u76ee\u5f55: " + name,
            Finding = @"C:\" + name + " \u5360\u7528 1.0 GB",
            Reason = "\u5b83\u4e0d\u5728 C: \u6839\u76ee\u5f55\u767d\u540d\u5355\u5185\uff0c\u5e94\u8be5\u5148\u786e\u8ba4\u6765\u6e90\u3002",
            Action = RecommendationAction.Observe,
            Risk = RiskLevel.Medium,
            Reversibility = ReversibilityLevel.PartiallyReversible,
            EstimatedImpactBytes = sizeBytes,
            Evidence = ["\u6839\u76ee\u5f55\u767d\u540d\u5355\u672a\u5305\u542b: " + name]
        };

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static SoftwareProfile CreateCDriveMigrationProfile() =>
        new()
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
            InstalledSizeBytes = 600L * 1024 * 1024,
            CacheSizeBytes = 8L * 1024 * 1024 * 1024,
            CachePaths = [@"C:\Users\Me\.ollama\models"],
            Services = ["OllamaService"],
            RunningProcesses = ["ollama"],
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\Programs\Ollama", @"C:\Users\Me\.ollama\models"]
        };
}
