using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public class UninstallResidueScanTests
{
    [Fact]
    public void Residue_scan_groups_leftovers_after_official_uninstall_by_risk()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            InstallPath = @"D:\Software\Example\Install",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Example\Logs"],
            DataPaths = [@"C:\Users\Me\AppData\Roaming\Example"],
            StartupEntries = ["Example Tray"],
            Services = ["ExampleService"],
            ScheduledTasks = [@"\Example Update"]
        };

        var report = UninstallResidueScanBuilder.Build(
            before,
            afterProfiles: [],
            pathExists: path => path.Contains("Example", StringComparison.OrdinalIgnoreCase),
            sizeResolver: path => path.Contains("Cache", StringComparison.OrdinalIgnoreCase) ? 128 * 1024 * 1024 : 0);

        report.OfficialUninstallAppearsComplete.Should().BeTrue();
        report.WouldDeleteAutomatically.Should().BeFalse();
        report.Groups.Should().Contain(group =>
            group.Risk == RiskLevel.Low
            && group.CanMoveToQuarantine
            && group.Candidates.Any(candidate => candidate.Path == before.CachePaths[0])
            && group.Candidates.Any(candidate => candidate.Path == before.LogPaths[0]));
        report.Groups.Should().Contain(group =>
            group.Risk == RiskLevel.Medium
            && !group.CanMoveToQuarantine
            && group.Candidates.Any(candidate => candidate.Path == before.DataPaths[0])
            && group.Candidates.Any(candidate => candidate.Path == before.InstallPath));
        report.Groups.Should().Contain(group =>
            group.Risk == RiskLevel.High
            && !group.CanMoveToQuarantine
            && group.Candidates.Any(candidate => candidate.Identifier == "ExampleService")
            && group.Candidates.Any(candidate => candidate.Identifier == @"\Example Update"));
        report.Groups.SelectMany(group => group.Candidates)
            .Should()
            .OnlyContain(candidate => candidate.RequiresConfirmation);
        report.Summary.Should().Contain("只生成残留清单");
    }

    [Fact]
    public void Residue_scan_does_not_offer_cleanup_when_software_still_installed()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"]
        };
        var after = new[]
        {
            new SoftwareProfile
            {
                Name = "Example App",
                InstallPath = @"D:\Software\Example\Install"
            }
        };

        var report = UninstallResidueScanBuilder.Build(
            before,
            after,
            pathExists: _ => true);

        report.OfficialUninstallAppearsComplete.Should().BeFalse();
        report.Groups.Should().BeEmpty();
        report.Summary.Should().Contain("仍然检测到这个软件");
    }

    [Fact]
    public void Residue_quarantine_operation_contains_only_low_risk_path_candidates()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Example\Logs"],
            DataPaths = [@"C:\Users\Me\AppData\Roaming\Example"],
            Services = ["ExampleService"]
        };
        var report = UninstallResidueScanBuilder.Build(
            before,
            afterProfiles: [],
            pathExists: _ => true,
            sizeResolver: path => path.Contains("Cache", StringComparison.OrdinalIgnoreCase)
                ? 64 * 1024 * 1024
                : 16 * 1024 * 1024);

        var operation = UninstallResidueOperationPlanner.CreateLowRiskQuarantineOperation(report);

        operation.Should().NotBeNull();
        operation!.Kind.Should().Be("uninstall.residue.quarantine");
        operation.Title.Should().Contain("Example App");
        operation.Risk.Should().Be(RiskLevel.Low);
        operation.IsDestructive.Should().BeTrue();
        operation.RollbackRequired.Should().BeTrue();
        operation.ConfirmationAccepted.Should().BeFalse();
        operation.AffectedPaths.Should().Equal(before.CachePaths[0], before.LogPaths[0]);
        operation.AffectedPaths.Should().NotContain(before.DataPaths[0]);
        operation.AffectedServices.Should().BeEmpty();
        operation.EstimatedImpactBytes.Should().Be(80 * 1024 * 1024);
        operation.ConfirmationText.Should().Contain("隔离区");
    }

    [Fact]
    public void Quarantine_policy_accepts_low_risk_uninstall_residue_plan_only_after_confirmation()
    {
        var descriptor = new OperationDescriptor
        {
            Kind = "uninstall.residue.quarantine",
            Title = "清理低风险卸载残留",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EvidenceSummary = "卸载后残留扫描发现低风险缓存",
            AffectedPaths = [@"C:\Users\Me\AppData\Local\Example\Cache"]
        };

        var gate = QuarantineOperationPolicy.ValidateCandidate(descriptor);
        var confirm = () => QuarantineOperationPolicy.ConfirmForExecution(descriptor);

        gate.Success.Should().BeTrue();
        confirm.Should().Throw<InvalidOperationException>()
            .WithMessage("*候选身份*");
    }

    [Fact]
    public void Residue_review_presentation_exposes_low_risk_quarantine_action_after_uninstall()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Example\Logs"],
            Services = ["ExampleService"]
        };
        var report = UninstallResidueScanBuilder.Build(
            before,
            afterProfiles: [],
            pathExists: _ => true,
            sizeResolver: _ => 1024);

        var review = UninstallResidueReviewPresentationBuilder.Create(report);

        review.Title.Should().Be("Example App 卸载后残留复查");
        review.Summary.Should().Contain("只处理低风险");
        review.CanMoveLowRiskToQuarantine.Should().BeTrue();
        review.PrimaryButtonText.Should().Be("移动低风险残留到隔离区");
        review.LowRiskOperation.Should().NotBeNull();
        review.Groups.Should().Contain(group => group.RiskLabel == "低" && group.CanMoveToQuarantine);
        review.Groups.Should().Contain(group => group.RiskLabel == "高" && !group.CanMoveToQuarantine);
    }

    [Fact]
    public void Residue_review_presentation_blocks_action_when_software_still_exists()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"]
        };
        var after = new[]
        {
            new SoftwareProfile
            {
                Name = "Example App",
                InstallPath = @"D:\Software\Example"
            }
        };
        var report = UninstallResidueScanBuilder.Build(before, after, pathExists: _ => true);

        var review = UninstallResidueReviewPresentationBuilder.Create(report);

        review.CanMoveLowRiskToQuarantine.Should().BeFalse();
        review.PrimaryButtonText.Should().Be("暂不能处理");
        review.LowRiskOperation.Should().BeNull();
        review.Summary.Should().Contain("仍然检测到这个软件");
    }

    [Fact]
    public void Residue_review_presentation_is_non_executable_until_user_confirms_a_safe_operation()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"]
        };
        var after = new[]
        {
            new SoftwareProfile
            {
                Name = "Example App",
                InstallPath = @"D:\Software\Example"
            }
        };
        var report = UninstallResidueScanBuilder.Build(before, after, pathExists: _ => true);

        var review = UninstallResidueReviewPresentationBuilder.Create(report);

        review.CanExecuteDirectly.Should().BeFalse();
        review.SafetyText.Should().Contain("不会自动删除或移动");
        review.SafetyText.Should().Contain("二次确认");
    }

    [Fact]
    public void Residue_review_planner_uses_cached_inventory_to_block_when_app_is_still_visible()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example"
        };
        var current = new[]
        {
            new SoftwareProfile
            {
                Name = "Example App",
                InstallPath = @"D:\Software\Example"
            }
        };

        var immediate = UninstallResidueReviewPlanner.TryBuildStillInstalledReport(before, current);
        var afterUninstall = UninstallResidueReviewPlanner.TryBuildStillInstalledReport(before, []);

        immediate.Should().NotBeNull();
        immediate!.OfficialUninstallAppearsComplete.Should().BeFalse();
        immediate.Summary.Should().Contain("仍然检测到这个软件");
        afterUninstall.Should().BeNull();
    }

    [Fact]
    public void Residue_drawer_inline_status_blocks_cleanup_when_app_still_installed_and_hides_paths()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"]
        };
        var current = new[]
        {
            new SoftwareProfile
            {
                Name = "Example App",
                InstallPath = @"D:\Software\Example"
            }
        };
        var report = UninstallResidueScanBuilder.Build(before, current, pathExists: _ => true);
        var review = UninstallResidueReviewPresentationBuilder.Create(report);

        var drawer = UninstallResidueDrawerReviewPresenter.Create(review);

        drawer.SectionTitle.Should().Be("\u6b8b\u7559\u68c0\u67e5\u7ed3\u679c");
        drawer.Lines.Should().Contain(line => line.Contains("\u8fd8\u6ca1\u6709\u786e\u8ba4\u5378\u8f7d\u5b8c\u6210"));
        drawer.Lines.Should().Contain(line => line.Contains("\u5148\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d"));
        drawer.Lines.Should().Contain(line => line.Contains("\u4e0d\u4f1a\u79fb\u52a8\u4efb\u4f55\u6587\u4ef6"));
        drawer.PrimaryButtonText.Should().Be("\u6682\u4e0d\u80fd\u5904\u7406");
        drawer.CanMoveLowRiskToQuarantine.Should().BeFalse();
        drawer.LowRiskOperation.Should().BeNull();
        drawer.VisibleText.Should().NotContain(@"C:\");
        drawer.VisibleText.Should().NotContain(@"D:\");
    }

    [Fact]
    public void Residue_drawer_inline_status_explains_cancel_and_quarantine_outcomes_without_paths()
    {
        var before = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example",
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"]
        };
        var report = UninstallResidueScanBuilder.Build(before, [], pathExists: _ => true);
        var review = UninstallResidueReviewPresentationBuilder.Create(report);

        var canceled = UninstallResidueDrawerReviewPresenter.CreateCanceled(review);
        var quarantined = UninstallResidueDrawerReviewPresenter.CreateQuarantined(
            review,
            "\u4f4e\u98ce\u9669\u6b8b\u7559\u5df2\u79fb\u52a8\u5230\u9694\u79bb\u533a\u3002");

        canceled.SectionTitle.Should().Be("\u6b8b\u7559\u5904\u7406\u7ed3\u679c");
        canceled.Lines.Should().Contain(line => line.Contains("\u5df2\u53d6\u6d88"));
        canceled.Lines.Should().Contain(line => line.Contains("\u6ca1\u6709\u79fb\u52a8\u4efb\u4f55\u6587\u4ef6"));
        canceled.Lines.Should().Contain(line => line.Contains("\u540e\u6094\u836f\u4e2d\u5fc3\u6ca1\u6709\u65b0\u589e\u8bb0\u5f55"));
        canceled.CanMoveLowRiskToQuarantine.Should().BeFalse();
        canceled.LowRiskOperation.Should().BeNull();
        canceled.PrimaryActionText.Should().BeEmpty();
        canceled.PrimaryActionKey.Should().BeEmpty();
        canceled.VisibleText.Should().NotContain(@"C:\");
        canceled.VisibleText.Should().NotContain(@"D:\");

        quarantined.SectionTitle.Should().Be("\u6b8b\u7559\u5904\u7406\u7ed3\u679c");
        quarantined.Lines.Should().Contain(line => line.Contains("\u5df2\u79fb\u52a8\u5230\u9694\u79bb\u533a"));
        quarantined.Lines.Should().Contain(line => line.Contains("\u540e\u6094\u836f\u4e2d\u5fc3"));
        quarantined.Lines.Should().Contain(line => line.Contains("\u53ef\u4ee5\u8fd8\u539f"));
        quarantined.CanMoveLowRiskToQuarantine.Should().BeFalse();
        quarantined.LowRiskOperation.Should().BeNull();
        quarantined.PrimaryActionText.Should().Be("\u67e5\u770b\u540e\u6094\u836f\u4e2d\u5fc3");
        quarantined.PrimaryActionKey.Should().Be("Timeline");
        quarantined.VisibleText.Should().NotContain(@"C:\");
        quarantined.VisibleText.Should().NotContain(@"D:\");
    }

    [Fact]
    public void Residue_quarantine_refreshes_timeline_and_apps_after_every_pipeline_attempt()
    {
        var main = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "MainWindow.xaml.cs"));
        var helper = SourceMethodExtractor.Extract(
            main,
            "private async Task RefreshUninstallResidueStateAfterAttemptAsync()");
        var execute = SourceMethodExtractor.Extract(
            main,
            "private async Task ReviewUninstallResidueAsync(");

        const string attempted = "pipelineAttempted = true;";
        const string executePipeline = "await pipeline.ExecuteAsync(descriptor)";
        const string synchronize = "await RefreshUninstallResidueStateAfterAttemptAsync();";
        const string successGate = "if (!result.Success)";
        execute.Should().Contain("var pipelineAttempted = false;");
        execute.Should().Contain("var stateSynchronized = false;");
        execute.IndexOf(attempted, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(executePipeline, StringComparison.Ordinal));
        execute.IndexOf(executePipeline, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(synchronize, StringComparison.Ordinal));
        execute.IndexOf(synchronize, StringComparison.Ordinal)
            .Should().BeLessThan(execute.IndexOf(successGate, StringComparison.Ordinal));
        execute.Should().Contain("if (pipelineAttempted && !stateSynchronized)");
        execute.Split(synchronize, StringSplitOptions.None).Length.Should().Be(3);

        helper.Should().Contain("await LoadTimelineAsync();");
        helper.Should().Contain("SetSoftwareProfiles(await ScanSoftwareProfilesAsync());");
        helper.Should().NotContain("SafetyOperationPipeline");
        helper.Should().NotContain("QuarantineAsync");
        helper.Should().NotContain("RestoreAsync");
        helper.Should().NotContain("PurgeAsync");
        helper.Should().NotContain("File.Delete");
        helper.Should().NotContain("Directory.Delete");
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException(
            "Repository file was not found.",
            Path.Combine(segments));
    }
}
