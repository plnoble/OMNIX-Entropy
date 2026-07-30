using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class DriveHealthPlanExperienceTests
{
    private const long GiB = 1024L * 1024 * 1024;

    [Fact]
    public void Full_drive_plan_separates_safe_cleanup_from_remaining_comfort_gap()
    {
        var result = Drive(totalGiB: 100, freeGiB: 10);
        var plan = DriveHealthPlanPresenter.Create(
            result,
            [Cleanup(2 * GiB)],
            personalStorageCandidateCount: 3,
            rootCauseCount: 4);

        plan.TargetReleaseBytes.Should().Be(10 * GiB);
        plan.SafeCleanupBytes.Should().Be(2 * GiB);
        plan.RemainingGapBytes.Should().Be(8 * GiB);
        plan.Headline.Should().Contain("10.0 GB").And.Contain("80%");
        plan.Progress.Should().Contain("2.0 GB").And.Contain("8.0 GB");
        plan.Steps.Should().HaveCount(3);
        plan.Steps[0].Should().Contain("低风险").And.Contain("2.0 GB");
        plan.Steps[1].Should().Contain("8.0 GB").And.Contain("大文件");
        plan.Steps[2].Should().Contain("D 盘");
        plan.PrimaryAction.Should().Be(DriveHealthPlanAction.ReviewSafeCleanup);
        plan.PrimaryActionLabel.Should().Contain("最安全");
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
        plan.Progress.Should().Contain("没有已确认").And.Contain("不要删除");
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
        plan.Headline.Should().Contain("舒适目标内");
        plan.PrimaryAction.Should().Be(DriveHealthPlanAction.None);
        plan.PrimaryActionLabel.Should().Contain("暂时不用处理");
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
