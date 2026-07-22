using Css.Core.Software;
using Css.InstallGuard.Installers;
using FluentAssertions;

namespace Css.Tests;

public class InstallSnapshotDiffTests
{
    [Fact]
    public void Diff_reports_added_software_startup_services_tasks_and_c_drive_paths()
    {
        var before = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero),
            [Profile("Existing Tool", @"D:\Software\Existing Tool\Install")]);
        var after = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 6, 30, 10, 15, 0, TimeSpan.Zero),
            [
                Profile("Existing Tool", @"D:\Software\Existing Tool\Install"),
                Profile(
                    "New Tool",
                    @"C:\Program Files\New Tool",
                    cDriveWritePaths: [@"C:\Program Files\New Tool", @"C:\Users\Me\AppData\Local\New Tool"],
                    startupEntries: ["New Tool"],
                    services: ["NewToolSvc"],
                    scheduledTasks: ["New Tool Updater"])
            ]);

        var report = InstallSnapshotDiffBuilder.Build(before, after);

        report.AddedSoftware.Should().ContainSingle(p => p.Name == "New Tool");
        report.NewStartupEntries.Should().Contain("New Tool");
        report.NewServices.Should().Contain("NewToolSvc");
        report.NewScheduledTasks.Should().Contain("New Tool Updater");
        report.NewCDrivePaths.Should().Contain(@"C:\Program Files\New Tool");
        report.NewCDrivePaths.Should().Contain(@"C:\Users\Me\AppData\Local\New Tool");
        report.HasCDriveWrites.Should().BeTrue();
        report.Summary.Should().Contain("New Tool");
    }

    [Fact]
    public void Diff_reports_changes_added_to_existing_profiles_without_duplicate_software()
    {
        var before = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero),
            [Profile("Existing Tool", @"D:\Software\Existing Tool\Install")]);
        var after = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 6, 30, 10, 15, 0, TimeSpan.Zero),
            [
                Profile(
                    "Existing Tool",
                    @"D:\Software\Existing Tool\Install",
                    cDriveWritePaths: [@"C:\Users\Me\AppData\Local\Existing Tool\Cache"],
                    startupEntries: ["Existing Tool Agent"])
            ]);

        var report = InstallSnapshotDiffBuilder.Build(before, after);

        report.AddedSoftware.Should().BeEmpty();
        report.NewStartupEntries.Should().Contain("Existing Tool Agent");
        report.NewCDrivePaths.Should().Contain(@"C:\Users\Me\AppData\Local\Existing Tool\Cache");
        report.HasCDriveWrites.Should().BeTrue();
    }

    [Fact]
    public void Diff_presenter_creates_beginner_cards_before_technical_details()
    {
        var before = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 6, 30, 10, 0, 0, TimeSpan.Zero),
            [Profile("Existing Tool", @"D:\Software\Existing Tool\Install")]);
        var after = new InstallSystemSnapshot(
            new DateTimeOffset(2026, 6, 30, 10, 15, 0, TimeSpan.Zero),
            [
                Profile("Existing Tool", @"D:\Software\Existing Tool\Install"),
                Profile(
                    "New Tool",
                    @"C:\Program Files\New Tool",
                    cDriveWritePaths: [@"C:\Program Files\New Tool", @"C:\Users\Me\AppData\Local\New Tool"],
                    startupEntries: ["New Tool"],
                    services: ["NewToolSvc"],
                    scheduledTasks: ["New Tool Updater"])
            ]);
        var report = InstallSnapshotDiffBuilder.Build(before, after);

        var view = InstallSnapshotDiffPresenter.Create(report);

        view.Title.Should().Contain("\u5b89\u88c5\u53d8\u5316");
        view.Summary.Should().Contain("\u65b0\u589e\u8f6f\u4ef6 1 \u4e2a");
        view.SafetyText.Should().Contain("\u4e0d\u4f1a\u81ea\u52a8\u5904\u7406");
        view.CanExecuteDirectly.Should().BeFalse();
        view.Cards.Should().HaveCount(4);
        view.Cards.Should().Contain(card => card.Title.Contains("\u88c5\u4e86\u4ec0\u4e48") && card.Body.Contains("New Tool"));
        view.Cards.Should().Contain(card =>
            card.Title.Contains("C \u76d8")
            && card.Body.Contains("主程序在 C 盘")
            && card.Body.Contains("安装目录之外 1 个"));
        view.Cards.Should().Contain(card => card.Title.Contains("\u540e\u53f0") && card.Body.Contains("3 \u9879"));
        view.Cards.Should().Contain(card => card.Title.Contains("Agent") && card.Body.Contains("\u5148\u67e5\u770b"));

        var visibleText = string.Join("\n", view.Cards.Select(card => card.Title + card.Body + card.Detail));
        visibleText.Should().NotContain(@"C:\Users\Me");
        visibleText.Should().NotContain("NewToolSvc");
        view.TechnicalDetails.Should().Contain(line => line.Contains(@"C:\Users\Me\AppData\Local\New Tool"));
        view.TechnicalDetails.Should().Contain(line => line.Contains("NewToolSvc"));
    }

    [Fact]
    public void Diff_agent_explanation_prioritizes_c_drive_and_background_review_without_exposing_details()
    {
        var report = InstallSnapshotDiffBuilder.Build(
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
                [Profile("Existing Tool", @"D:\Software\Existing Tool\Install")]),
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
                [
                    Profile("Existing Tool", @"D:\Software\Existing Tool\Install"),
                    Profile(
                        "New Tool",
                        @"D:\Software\New Tool\Install",
                        cDriveWritePaths: [@"C:\Users\Me\AppData\Local\New Tool\Cache"],
                        startupEntries: ["New Tool"],
                        services: ["NewToolSvc"])
                ]));

        var advice = InstallSnapshotDiffAgentPresenter.Create(report);

        advice.Title.Should().Be("Computer Agent \u89e3\u91ca");
        advice.Headline.Should().Contain("C \u76d8").And.Contain("\u540e\u53f0");
        advice.WhatThisMeans.Should().Contain("1 \u4e2a\u65b0\u4f4d\u7f6e").And.Contain("2 \u9879\u540e\u53f0\u53d8\u5316");
        advice.NextSteps.Should().Contain(step => step.Contains("\u7f13\u5b58") && step.Contains("\u914d\u7f6e"));
        advice.NextSteps.Should().Contain(step => step.Contains("\u751f\u6210\u5904\u7406\u65b9\u6848"));
        advice.SafetyBoundary.Should().Contain("\u4e0d\u4f1a\u76f4\u63a5").And.Contain("\u672c\u5730\u5b89\u5168\u7ba1\u7ebf");
        advice.CanExecuteDirectly.Should().BeFalse();

        var visibleText = string.Join("\n", [advice.Title, advice.Headline, advice.WhatThisMeans, advice.SafetyBoundary, .. advice.NextSteps]);
        visibleText.Should().NotContain(@"C:\Users\Me");
        visibleText.Should().NotContain("NewToolSvc");
    }

    [Fact]
    public void Diff_agent_explanation_recommends_observation_when_install_has_no_new_pressure()
    {
        var profile = Profile("Existing Tool", @"D:\Software\Existing Tool\Install");
        var report = InstallSnapshotDiffBuilder.Build(
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
                [profile]),
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
                [profile]));

        var advice = InstallSnapshotDiffAgentPresenter.Create(report);

        advice.Headline.Should().Contain("\u4e0d\u7528\u6025\u7740\u5904\u7406");
        advice.WhatThisMeans.Should().Contain("\u6ca1\u6709\u53d1\u73b0\u65b0\u7684 C \u76d8\u538b\u529b");
        advice.NextSteps.Should().Contain(step => step.Contains("\u7ee7\u7eed\u89c2\u5bdf"));
        advice.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Diff_action_plan_orders_c_drive_and_background_review_without_executing()
    {
        var report = InstallSnapshotDiffBuilder.Build(
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
                [Profile("Existing Tool", @"D:\Software\Existing Tool\Install")]),
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
                [
                    Profile("Existing Tool", @"D:\Software\Existing Tool\Install"),
                    Profile(
                        "New Tool",
                        @"D:\Software\New Tool\Install",
                        cDriveWritePaths: [@"C:\Users\Me\AppData\Local\New Tool\Cache"],
                        startupEntries: ["New Tool"],
                        services: ["NewToolSvc"])
                ]));

        var plan = InstallSnapshotDiffActionPlanPresenter.Create(report);

        plan.Title.Should().Be("Agent \u5904\u7406\u65b9\u6848");
        plan.Summary.Should().Contain("3 \u4ef6\u4e8b").And.Contain("\u987a\u5e8f");
        plan.Items.Select(item => item.Order).Should().Equal(1, 2, 3);
        plan.Items.Should().Contain(item =>
            item.Title.Contains("C \u76d8") &&
            item.Decision.Contains("\u5148\u4fdd\u7559") &&
            item.EvidenceSummary.Contains("1 \u4e2a\u65b0\u4f4d\u7f6e"));
        plan.Items.Should().Contain(item =>
            item.Title.Contains("\u540e\u53f0") &&
            item.Decision.Contains("\u5148\u4fdd\u6301") &&
            item.EvidenceSummary.Contains("2 \u9879"));
        plan.Items.Should().Contain(item => item.Title.Contains("\u7ee7\u7eed\u89c2\u5bdf"));
        plan.Items.Should().OnlyContain(item => !item.CanExecuteDirectly);
        plan.SafetyBoundary.Should().Contain("\u5c1a\u672a\u6267\u884c").And.Contain("\u672c\u5730\u5b89\u5168\u7ba1\u7ebf");
        plan.RequiresUserConfirmation.Should().BeTrue();
        plan.CanExecuteDirectly.Should().BeFalse();

        var visibleText = string.Join("\n", plan.Items.SelectMany(item =>
            new[] { item.Title, item.Decision, item.Reason, item.EvidenceSummary, item.RiskLabel }));
        visibleText.Should().NotContain(@"C:\Users\Me");
        visibleText.Should().NotContain("NewToolSvc");
    }

    [Fact]
    public void Diff_action_plan_recommends_no_action_when_install_has_no_new_pressure()
    {
        var profile = Profile("Existing Tool", @"D:\Software\Existing Tool\Install");
        var report = InstallSnapshotDiffBuilder.Build(
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
                [profile]),
            new InstallSystemSnapshot(
                new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
                [profile]));

        var plan = InstallSnapshotDiffActionPlanPresenter.Create(report);

        plan.Items.Should().ContainSingle();
        plan.Items[0].Title.Should().Contain("\u7ee7\u7eed\u89c2\u5bdf");
        plan.Items[0].Decision.Should().Contain("\u73b0\u5728\u4e0d\u7528\u5904\u7406");
        plan.Items[0].CanExecuteDirectly.Should().BeFalse();
        plan.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Diff_evidence_review_classifies_each_c_drive_location_without_exposing_paths()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewCDrivePaths =
            [
                @"C:\Program Files\New Tool",
                @"C:\Users\Me\AppData\Local\New Tool\Cache",
                @"C:\Users\Me\AppData\Roaming\New Tool\Config",
                @"C:\Users\Me\AppData\Local\New Tool\Logs",
                @"C:\Users\Me\.newtool\Models",
                @"C:\OddArea\New Tool"
            ],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);

        review.CDriveItems.Should().HaveCount(6);
        review.CDriveItems.Select(item => item.Kind).Should().Equal(
            InstallSnapshotCDriveContentKind.InstallFiles,
            InstallSnapshotCDriveContentKind.Cache,
            InstallSnapshotCDriveContentKind.Configuration,
            InstallSnapshotCDriveContentKind.Logs,
            InstallSnapshotCDriveContentKind.ModelOrData,
            InstallSnapshotCDriveContentKind.Unknown);
        review.CDriveItems.Select(item => item.Index).Should().Equal(1, 2, 3, 4, 5, 6);
        review.CDriveItems.Should().OnlyContain(item =>
            item.DisplayName.StartsWith("C \u76d8\u65b0\u589e\u4f4d\u7f6e ") &&
            item.ConfidenceLabel.Contains("\u521d\u6b65\u5224\u65ad") &&
            !item.CanExecuteDirectly);
        review.Summary.Should().Contain("\u5b89\u88c5\u6587\u4ef6 1 \u4e2a")
            .And.Contain("\u7f13\u5b58 1 \u4e2a")
            .And.Contain("\u914d\u7f6e 1 \u4e2a")
            .And.Contain("\u65e5\u5fd7 1 \u4e2a")
            .And.Contain("\u6a21\u578b/\u6570\u636e 1 \u4e2a")
            .And.Contain("\u5f85\u786e\u8ba4 1 \u4e2a");
        review.CanExecuteDirectly.Should().BeFalse();

        var visibleText = string.Join("\n", review.CDriveItems.SelectMany(item =>
            new[] { item.DisplayName, item.KindLabel, item.Purpose, item.Advice, item.ConfidenceLabel }));
        visibleText.Should().NotContain(@"C:\");
        visibleText.Should().NotContain("New Tool");
        visibleText.Should().NotContain("OddArea");
    }

    [Fact]
    public void Diff_evidence_review_explains_background_mechanisms_without_exposing_names()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewStartupEntries = ["New Tool Updater"],
            NewServices = ["NewToolSvc"],
            NewScheduledTasks = ["New Tool Sync"],
            Summary = "fixture"
        };

        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);

        review.BackgroundItems.Should().HaveCount(3);
        review.BackgroundItems.Select(item => item.Kind).Should().Equal(
            InstallSnapshotBackgroundKind.Startup,
            InstallSnapshotBackgroundKind.Service,
            InstallSnapshotBackgroundKind.ScheduledTask);
        review.BackgroundItems.Should().Contain(item =>
            item.Kind == InstallSnapshotBackgroundKind.Startup &&
            item.LikelyPurpose.Contains("\u81ea\u52a8\u66f4\u65b0"));
        review.BackgroundItems.Should().Contain(item =>
            item.Kind == InstallSnapshotBackgroundKind.Service &&
            item.LikelyPurpose.Contains("\u540e\u53f0\u6838\u5fc3\u529f\u80fd") &&
            item.RiskLabel.Contains("\u8f83\u9ad8"));
        review.BackgroundItems.Should().Contain(item =>
            item.Kind == InstallSnapshotBackgroundKind.ScheduledTask &&
            item.LikelyPurpose.Contains("\u540c\u6b65"));
        review.BackgroundItems.Should().OnlyContain(item =>
            item.ConfidenceLabel.Contains("\u521d\u6b65\u5224\u65ad") &&
            !item.CanExecuteDirectly);

        var visibleText = string.Join("\n", review.BackgroundItems.SelectMany(item =>
            new[] { item.DisplayName, item.KindLabel, item.LikelyPurpose, item.Advice, item.RiskLabel }));
        visibleText.Should().NotContain("New Tool");
        visibleText.Should().NotContain("NewToolSvc");
    }

    [Fact]
    public void Diff_action_plan_includes_compact_evidence_classification_summary()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewCDrivePaths =
            [
                @"C:\Users\Me\AppData\Local\New Tool\Cache",
                @"C:\Users\Me\.newtool\Models",
                @"C:\OddArea\New Tool"
            ],
            NewStartupEntries = ["New Tool"],
            NewServices = ["NewToolSvc"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var plan = InstallSnapshotDiffActionPlanPresenter.Create(report);

        plan.ReviewSummary.Should().Contain("\u7f13\u5b58 1 \u4e2a")
            .And.Contain("\u6a21\u578b/\u6570\u636e 1 \u4e2a")
            .And.Contain("\u5f85\u786e\u8ba4 1 \u4e2a")
            .And.Contain("\u5f00\u673a\u542f\u52a8 1 \u9879")
            .And.Contain("\u540e\u53f0\u670d\u52a1 1 \u9879");
        plan.ReviewSummary.Should().NotContain(@"C:\");
        plan.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Diff_action_plan_carries_a_read_only_evidence_review_without_raw_identifiers()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewCDrivePaths = [@"C:\Users\Me\AppData\Local\New Tool\Cache"],
            NewStartupEntries = ["New Tool Updater"],
            NewServices = ["NewToolSvc"],
            NewScheduledTasks = [@"\New Tool Sync"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var plan = InstallSnapshotDiffActionPlanPresenter.Create(report);

        plan.EvidenceReview.Summary.Should().Be(plan.ReviewSummary);
        plan.EvidenceReview.CDriveItems.Should().ContainSingle();
        plan.EvidenceReview.BackgroundItems.Should().HaveCount(3);
        plan.EvidenceReview.CDriveItems.Should().OnlyContain(item => !item.CanExecuteDirectly);
        plan.EvidenceReview.BackgroundItems.Should().OnlyContain(item => !item.CanExecuteDirectly);
        plan.EvidenceReview.SafetyBoundary.Should().Contain("\u521d\u6b65\u5224\u65ad")
            .And.Contain("\u4e0d\u4f1a\u76f4\u63a5");
        plan.EvidenceReview.CanExecuteDirectly.Should().BeFalse();

        var visibleText = string.Join("\n", plan.EvidenceReview.CDriveItems.SelectMany(item =>
                new[] { item.DisplayName, item.KindLabel, item.Purpose, item.Advice, item.ConfidenceLabel }))
            + "\n"
            + string.Join("\n", plan.EvidenceReview.BackgroundItems.SelectMany(item =>
                new[] { item.DisplayName, item.KindLabel, item.LikelyPurpose, item.Advice, item.RiskLabel, item.ConfidenceLabel }));
        visibleText.Should().NotContain(@"C:\Users\Me");
        visibleText.Should().NotContain("New Tool");
        visibleText.Should().NotContain("NewToolSvc");
    }

    [Fact]
    public void Diff_evidence_review_derives_deduplicated_plan_only_candidates_from_classifications()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewCDrivePaths =
            [
                @"C:\Users\Me\AppData\Local\New Tool\Cache",
                @"C:\Users\Me\AppData\Local\New Tool\Logs",
                @"C:\Users\Me\AppData\Roaming\New Tool\Config",
                @"C:\Users\Me\.newtool\Models",
                @"C:\Program Files\New Tool",
                @"C:\OddArea\New Tool"
            ],
            NewStartupEntries = ["New Tool", "New Tool Updater"],
            NewServices = ["NewToolSvc"],
            NewScheduledTasks = [@"\New Tool Update"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);

        review.EligibleActions.Select(action => action.Kind).Should().Equal(
            InstallSnapshotEligibleActionKind.CacheCleanupPlan,
            InstallSnapshotEligibleActionKind.StorageSettingGuidance,
            InstallSnapshotEligibleActionKind.ReinstallOrMigrationPlan,
            InstallSnapshotEligibleActionKind.StartupDisablePlan,
            InstallSnapshotEligibleActionKind.ObserveOnly);
        review.EligibleActions.Should().OnlyContain(action =>
            !string.IsNullOrWhiteSpace(action.Title) &&
            !string.IsNullOrWhiteSpace(action.Reason) &&
            !string.IsNullOrWhiteSpace(action.EvidenceSummary) &&
            !string.IsNullOrWhiteSpace(action.NextEvidenceNeeded) &&
            !string.IsNullOrWhiteSpace(action.SafetyLabel) &&
            !action.CanExecuteDirectly);
        review.EligibleActions.Single(action =>
            action.Kind == InstallSnapshotEligibleActionKind.CacheCleanupPlan).RequiresRollback.Should().BeTrue();
        review.EligibleActions.Single(action =>
            action.Kind == InstallSnapshotEligibleActionKind.StorageSettingGuidance).RequiresRollback.Should().BeFalse();
        review.EligibleActions.Single(action =>
            action.Kind == InstallSnapshotEligibleActionKind.ReinstallOrMigrationPlan).RequiresRollback.Should().BeTrue();
        review.EligibleActions.Single(action =>
            action.Kind == InstallSnapshotEligibleActionKind.StartupDisablePlan).RequiresRollback.Should().BeTrue();
        review.EligibleActions.Single(action =>
            action.Kind == InstallSnapshotEligibleActionKind.ObserveOnly).RequiresRollback.Should().BeFalse();

        var visibleText = string.Join("\n", review.EligibleActions.SelectMany(action =>
            new[] { action.Title, action.Reason, action.EvidenceSummary, action.NextEvidenceNeeded, action.SafetyLabel }));
        visibleText.Should().NotContain(@"C:\");
        visibleText.Should().NotContain("New Tool");
        visibleText.Should().NotContain("NewToolSvc");
    }

    [Fact]
    public void Diff_evidence_review_recommends_observation_only_when_there_is_no_new_pressure()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            Summary = "fixture"
        };

        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);

        review.EligibleActions.Should().ContainSingle();
        review.EligibleActions[0].Kind.Should().Be(InstallSnapshotEligibleActionKind.ObserveOnly);
        review.EligibleActions[0].Reason.Should().Contain("\u6ca1\u6709\u53d1\u73b0");
        review.EligibleActions[0].RequiresRollback.Should().BeFalse();
        review.EligibleActions[0].CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Diff_candidate_preview_reuses_cache_and_startup_safety_planners_for_owned_evidence()
    {
        var profile = Profile(
            "New Tool",
            @"D:\Software\New Tool\Install",
            cDriveWritePaths: [@"C:\Users\Me\AppData\Local\New Tool\Cache"],
            startupEntries: ["New Tool Updater"]);
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            AddedSoftware = [profile],
            NewCDrivePaths = [@"C:\Users\Me\AppData\Local\New Tool\Cache"],
            NewStartupEntries = ["New Tool Updater"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var cache = InstallSnapshotCandidatePreviewPresenter.Create(
            report,
            InstallSnapshotEligibleActionKind.CacheCleanupPlan);
        var startup = InstallSnapshotCandidatePreviewPresenter.Create(
            report,
            InstallSnapshotEligibleActionKind.StartupDisablePlan);

        cache.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.Ready);
        cache.Lines.Should().Contain(line => line.Contains("\u9694\u79bb\u533a"));
        cache.RequiresRollback.Should().BeTrue();
        cache.CanExecuteDirectly.Should().BeFalse();
        cache.CanNavigateToApp.Should().BeTrue();
        cache.TargetAppName.Should().Be("New Tool");
        cache.NavigationLabel.Should().Contain("应用").And.Contain("缓存");
        startup.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.Ready);
        startup.Lines.Should().Contain(line => line.Contains("\u5feb\u7167"));
        startup.RequiresRollback.Should().BeTrue();
        startup.CanExecuteDirectly.Should().BeFalse();
        startup.CanNavigateToApp.Should().BeTrue();
        startup.TargetAppName.Should().Be("New Tool");
        startup.NavigationLabel.Should().Contain("应用").And.Contain("自启动");

        var visibleText = string.Join("\n", cache.Lines.Concat(startup.Lines)
            .Concat(cache.MissingEvidence)
            .Concat(startup.MissingEvidence)
            .Append(cache.Summary)
            .Append(startup.Summary));
        visibleText.Should().NotContain(@"C:\");
        visibleText.Should().NotContain("New Tool Updater");
    }

    [Fact]
    public void Diff_candidate_preview_reuses_migration_planner_without_exposing_paths()
    {
        var profile = Profile("New Tool", @"C:\Program Files\New Tool");
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            AddedSoftware = [profile],
            NewCDrivePaths = [@"C:\Program Files\New Tool"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var preview = InstallSnapshotCandidatePreviewPresenter.Create(
            report,
            InstallSnapshotEligibleActionKind.ReinstallOrMigrationPlan);

        preview.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.Ready);
        preview.Lines.Should().Contain(line => line.Contains("\u8fc1\u79fb\u8bc4\u5206"));
        preview.MissingEvidence.Should().Contain(line => line.Contains("\u5feb\u7167"));
        preview.RequiresSnapshot.Should().BeTrue();
        preview.RequiresRollback.Should().BeTrue();
        preview.CanExecuteDirectly.Should().BeFalse();
        preview.CanNavigateToApp.Should().BeTrue();
        preview.TargetAppName.Should().Be("New Tool");
        preview.NavigationLabel.Should().Contain("应用").And.Contain("迁移");
        string.Join("\n", preview.Lines.Concat(preview.MissingEvidence))
            .Should().NotContain(@"C:\").And.NotContain(@"D:\");
    }

    [Fact]
    public void Diff_candidate_preview_refuses_app_specific_preview_without_unique_owned_software()
    {
        var noOwnerReport = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewCDrivePaths = [@"C:\Users\Me\AppData\Local\Mystery\Cache"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };
        var ambiguousReport = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = noOwnerReport.BeforeCapturedAt,
            AfterCapturedAt = noOwnerReport.AfterCapturedAt,
            AddedSoftware =
            [
                Profile("Tool A", @"D:\Software\ToolA"),
                Profile("Tool B", @"D:\Software\ToolB")
            ],
            NewStartupEntries = ["Mystery Startup"],
            Summary = "fixture"
        };

        var noOwner = InstallSnapshotCandidatePreviewPresenter.Create(
            noOwnerReport,
            InstallSnapshotEligibleActionKind.CacheCleanupPlan);
        var ambiguous = InstallSnapshotCandidatePreviewPresenter.Create(
            ambiguousReport,
            InstallSnapshotEligibleActionKind.StartupDisablePlan);

        noOwner.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.Refused);
        ambiguous.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.Refused);
        noOwner.MissingEvidence.Should().Contain(line => line.Contains("\u552f\u4e00") && line.Contains("\u8f6f\u4ef6"));
        ambiguous.MissingEvidence.Should().Contain(line => line.Contains("\u552f\u4e00") && line.Contains("\u8f6f\u4ef6"));
        noOwner.CanExecuteDirectly.Should().BeFalse();
        ambiguous.CanExecuteDirectly.Should().BeFalse();
        noOwner.CanNavigateToApp.Should().BeFalse();
        ambiguous.CanNavigateToApp.Should().BeFalse();
        noOwner.TargetAppName.Should().BeNull();
        ambiguous.TargetAppName.Should().BeNull();
    }

    [Fact]
    public void Diff_candidate_preview_keeps_storage_guidance_and_observation_generic()
    {
        var report = new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 0, 0, TimeSpan.Zero),
            AfterCapturedAt = new DateTimeOffset(2026, 7, 10, 10, 10, 0, TimeSpan.Zero),
            NewCDrivePaths = [@"C:\Users\Me\.tool\Models", @"C:\OddArea\Tool"],
            NewServices = ["MysteryService"],
            HasCDriveWrites = true,
            Summary = "fixture"
        };

        var storage = InstallSnapshotCandidatePreviewPresenter.Create(
            report,
            InstallSnapshotEligibleActionKind.StorageSettingGuidance);
        var observe = InstallSnapshotCandidatePreviewPresenter.Create(
            report,
            InstallSnapshotEligibleActionKind.ObserveOnly);

        storage.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.GuidanceOnly);
        storage.Lines.Should().Contain(line => line.Contains("\u8f6f\u4ef6") && line.Contains("\u8bbe\u7f6e"));
        observe.Status.Should().Be(InstallSnapshotCandidatePreviewStatus.GuidanceOnly);
        observe.Lines.Should().Contain(line => line.Contains("\u4e0b\u6b21") || line.Contains("\u89c2\u5bdf"));
        storage.CanExecuteDirectly.Should().BeFalse();
        observe.CanExecuteDirectly.Should().BeFalse();
        storage.CanNavigateToApp.Should().BeFalse();
        observe.CanNavigateToApp.Should().BeFalse();
        string.Join("\n", storage.Lines.Concat(observe.Lines))
            .Should().NotContain(@"C:\").And.NotContain("MysteryService");
    }

    private static SoftwareProfile Profile(
        string name,
        string installPath,
        IReadOnlyList<string>? cDriveWritePaths = null,
        IReadOnlyList<string>? startupEntries = null,
        IReadOnlyList<string>? services = null,
        IReadOnlyList<string>? scheduledTasks = null) =>
        new()
        {
            Name = name,
            InstallPath = installPath,
            CDriveWritePaths = cDriveWritePaths ?? [],
            StartupEntries = startupEntries ?? [],
            Services = services ?? [],
            ScheduledTasks = scheduledTasks ?? []
        };
}
