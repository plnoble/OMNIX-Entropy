using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class DriveHealthPlanExperienceTests
{
    private const long MiB = 1024L * 1024;
    private const long GiB = 1024L * 1024 * 1024;

    [Fact]
    public void Full_drive_plan_separates_safe_cleanup_from_remaining_comfort_gap()
    {
        var result = Drive(totalGiB: 100, freeGiB: 10);
        var plan = DriveHealthPlanPresenter.Create(
            result,
            [
                Cleanup(2 * GiB),
                Observation("Unknown cache", 3 * GiB),
                Observation("Old installer", 1 * GiB)
            ],
            personalStorageCandidateCount: 3,
            rootCauseCount: 4);

        plan.TargetReleaseBytes.Should().Be(10 * GiB);
        plan.SafeCleanupBytes.Should().Be(2 * GiB);
        plan.RemainingGapBytes.Should().Be(8 * GiB);
        plan.SafeContributionPercent.Should().BeApproximately(20, 0.01);
        plan.IsSafeCleanupMeaningful.Should().BeTrue();
        plan.Headline.Should().Contain("10.0 GB").And.Contain("80%");
        plan.Progress.Should().Contain("不是没有可调整项")
            .And.Contain("1 项")
            .And.Contain("2 项需要")
            .And.Contain("2.0 GB")
            .And.Contain("8.0 GB");
        plan.Steps.Should().HaveCount(3);
        plan.Steps[0].Should().Contain("低风险").And.Contain("2.0 GB");
        plan.Steps[1].Should().Contain("8.0 GB").And.Contain("继续确认");
        plan.Steps[2].Should().Contain("D 盘");
        plan.PrimaryAction.Should().Be(DriveHealthPlanAction.ReviewSafeCleanup);
        plan.PrimaryActionLabel.Should().Contain("明显改善").And.Contain("安全");
        plan.SecondaryAction.Should().Be(DriveHealthPlanAction.ReviewPersonalStorage);
        plan.SecondaryActionLabel.Should().Contain("继续找");
        plan.HasSecondaryAction.Should().BeTrue();
        plan.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Minor_safe_cleanup_is_optional_when_it_barely_changes_the_comfort_gap()
    {
        var safeBytes = (long)(337.6 * MiB);
        var result = new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 300 * GiB,
            FreeBytes = (long)(40.5 * GiB)
        };

        var plan = DriveHealthPlanPresenter.Create(
            result,
            [
                Cleanup(safeBytes),
                Observation("Large application data", 8 * GiB),
                Observation("Unexpected root", 2 * GiB)
            ],
            personalStorageCandidateCount: 0,
            rootCauseCount: 2);

        plan.TargetReleaseBytes.Should().Be((long)(19.5 * GiB));
        plan.SafeCleanupBytes.Should().Be(safeBytes);
        plan.SafeContributionPercent.Should().BeApproximately(1.7, 0.1);
        plan.IsSafeCleanupMeaningful.Should().BeFalse();
        plan.Progress.Should().Contain("337.6 MB")
            .And.Contain("1.7%")
            .And.Contain("单独处理意义不大")
            .And.Contain("19.2 GB");
        plan.Steps[0].Should().Contain("主要差额");
        plan.Steps[1].Should().Contain("可选").And.Contain("337.6 MB");
        plan.PrimaryAction.Should().Be(DriveHealthPlanAction.ReviewSpaceSources);
        plan.PrimaryActionLabel.Should().Contain("释放更多空间");
        plan.SecondaryAction.Should().Be(DriveHealthPlanAction.ReviewSafeCleanup);
        plan.SecondaryActionLabel.Should().Contain("可选").And.Contain("337.6 MB");
        plan.HasSecondaryAction.Should().BeTrue();
        plan.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Full_drive_without_safe_cleanup_says_do_not_delete_and_routes_to_read_only_candidates()
    {
        var plan = DriveHealthPlanPresenter.Create(
            Drive(totalGiB: 100, freeGiB: 12),
            [Cleanup(2 * GiB, executable: false)],
            personalStorageCandidateCount: 2,
            rootCauseCount: 1);

        plan.SafeCleanupBytes.Should().Be(0);
        plan.Progress.Should().Contain("没有已确认")
            .And.Contain("不要删除")
            .And.Contain("只读线索")
            .And.Contain("可能指向同一批文件");
        plan.PrimaryAction.Should().Be(DriveHealthPlanAction.ReviewPersonalStorage);
        plan.PrimaryActionLabel.Should().Contain("大文件");
        plan.Steps.Should().Contain(line => line.Contains("只读"));
    }

    [Fact]
    public void Comfortable_drive_does_not_invent_cleanup_work()
    {
        var plan = DriveHealthPlanPresenter.Create(
            Drive(totalGiB: 100, freeGiB: 25),
            [Cleanup(2 * GiB)],
            personalStorageCandidateCount: 2,
            rootCauseCount: 2);

        plan.TargetReleaseBytes.Should().Be(0);
        plan.RemainingGapBytes.Should().Be(0);
        plan.SafeContributionPercent.Should().Be(0);
        plan.IsSafeCleanupMeaningful.Should().BeFalse();
        plan.Headline.Should().Contain("舒适目标内");
        plan.PrimaryAction.Should().Be(DriveHealthPlanAction.None);
        plan.PrimaryActionLabel.Should().Contain("暂时不用处理");
        plan.SecondaryAction.Should().Be(DriveHealthPlanAction.None);
        plan.HasSecondaryAction.Should().BeFalse();
    }

    [Fact]
    public void Data_drive_plan_does_not_tell_user_to_move_data_to_same_drive()
    {
        var plan = DriveHealthPlanPresenter.Create(
            Drive(totalGiB: 100, freeGiB: 10, driveRoot: @"D:\"),
            [],
            personalStorageCandidateCount: 2,
            rootCauseCount: 2);

        plan.ScopeLabel.Should().Contain("D 盘");
        plan.Steps[2].Should().Contain("其他磁盘");
        plan.Steps[2].Should().NotContain("优先改到D 盘");
    }

    [Fact]
    public void Home_first_view_exposes_agent_led_health_target_before_result_details()
    {
        var root = FindRepositoryRoot();
        var xaml = File.ReadAllText(Path.Combine(root, "src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "Css.App", "MainWindow.xaml.cs"));

        foreach (var id in new[]
        {
            "HomeDriveHealthPlanPanel",
            "HomeDriveHealthPlanHeadlineTextBlock",
            "HomeDriveHealthPlanProgressTextBlock",
            "HomeDriveHealthPlanButton",
            "CDriveHealthPlanHeadlineTextBlock",
            "CDriveHealthPlanProgressTextBlock",
            "CDriveHealthPlanStepsItemsControl",
            "CDriveHealthPlanSafetyTextBlock",
            "CDriveHealthPlanSecondaryButton",
            "CDriveRecommendationsScrollViewer"
        })
        {
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
        }

        xaml.IndexOf("HomeDriveHealthPlanPanel", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("HealthSummaryLayoutGrid", StringComparison.Ordinal));
        code.Should().Contain("DriveHealthPlanPresenter.Create")
            .And.Contain("ApplyDriveHealthPlan")
            .And.Contain("RecommendationsListBox.SelectedItem = preferredCard")
            .And.Contain("OpenDriveHealthPlan_Click")
            .And.Contain("OpenDriveHealthPlanSecondary_Click")
            .And.Contain("ResetDriveHealthPlanPresentation(")
            .And.Contain("CDriveHealthPlanStepsItemsControl.ItemsSource = null;")
            .And.Contain("CDriveHealthPlanSafetyTextBlock.Text = safetyBoundary;");
        var genericExplanation = code.IndexOf(
            "RecommendationActionTextBlock.Text = recommendationList.ActionExplanationText;",
            StringComparison.Ordinal);
        var selectedExplanation = code.IndexOf(
            "ApplyRecommendationSelection(RecommendationSelectionPresenter.Create(preferredCard));",
            StringComparison.Ordinal);
        genericExplanation.Should().BeGreaterThanOrEqualTo(0);
        selectedExplanation.Should().BeGreaterThan(genericExplanation);
        code.Should().Contain("var isSystemDriveScan = DiskScanScopePolicy.IsSystemDrive(")
            .And.Contain("其他磁盘结果只保留在当前页面，不计入系统盘体检历史。")
            .And.Contain("SelectSystemDriveTarget();")
            .And.NotContain("查看当前 C 盘证据");
        xaml.Should().Contain("x:Name=\"CDriveRecommendationsScrollViewer\"")
            .And.Contain("ScrollViewer.VerticalScrollBarVisibility=\"Disabled\"")
            .And.Contain("Text=\"系统盘体检历史\"");
    }

    private static DriveScanResult Drive(
        int totalGiB,
        int freeGiB,
        string driveRoot = @"C:\") =>
        new()
        {
            Drive = driveRoot,
            TotalBytes = totalGiB * GiB,
            FreeBytes = freeGiB * GiB
        };

    private static Recommendation Cleanup(long bytes, bool executable = true) =>
        new()
        {
            Title = "可清理缓存",
            Finding = "发现低风险缓存",
            Reason = "可回滚",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Low,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = bytes,
            Evidence = ["fixture"],
            Operation = executable
                ? new OperationDescriptor
                {
                    Kind = "clean.fixture",
                    Title = "清理 fixture",
                    Risk = RiskLevel.Low,
                    IsDestructive = true,
                    EstimatedImpactBytes = bytes
                }
                : null
        };

    private static Recommendation Observation(string title, long bytes) =>
        new()
        {
            Title = title,
            Finding = title + " finding",
            Reason = "Needs ownership evidence",
            Action = RecommendationAction.Observe,
            Risk = RiskLevel.Medium,
            Reversibility = ReversibilityLevel.PartiallyReversible,
            EstimatedImpactBytes = bytes,
            Evidence = [title + " evidence"]
        };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
