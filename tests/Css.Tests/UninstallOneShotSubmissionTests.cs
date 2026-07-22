using Css.App;
using FluentAssertions;

namespace Css.Tests;

public sealed class UninstallOneShotSubmissionTests
{
    [Fact]
    public void Unknown_attempt_has_a_path_free_beginner_conclusion()
    {
        var view = OfficialUninstallWorkerResultPresenter.CreateUnknownAttempt();

        view.Title.Should().Be("\u5378\u8f7d\u7ed3\u679c\u6ca1\u6709\u5b8c\u6574\u786e\u8ba4");
        view.StatusLabel.Should().Contain("\u91cd\u65b0\u626b\u63cf");
        view.Conclusion.Should().Contain("\u4e0d\u4f1a\u628a\u5b83\u8bf4\u6210\u5378\u8f7d\u6210\u529f");
        view.AgentAdvice.Should().Contain("\u91cd\u65b0\u626b\u63cf\u5e94\u7528");
        view.SafetyText.Should().Contain("\u4e0d\u4f1a\u81ea\u52a8\u91cd\u8bd5");
        view.CanExecuteDirectly.Should().BeFalse();
        view.VisibleText.Should().NotContain(@"C:\")
            .And.NotContain(@"D:\")
            .And.NotContain("registry")
            .And.NotContain("service");
    }

    [Fact]
    public void Coordinator_boundary_is_one_shot_and_unknown_result_forces_parent_rescan()
    {
        var planCode = Read("src", "Css.App", "UninstallPlanWindow.xaml.cs");
        var mainCode = Read("src", "Css.App", "MainWindow.xaml.cs");
        var request = SourceMethodExtractor.Extract(
            planCode,
            "private async void ContinueFinalConsent_Click(object sender, RoutedEventArgs e)");
        var reset = SourceMethodExtractor.Extract(
            planCode,
            "private void ResetFinalChecklist()");
        var parent = SourceMethodExtractor.Extract(
            mainCode,
            "private async Task ShowUninstallPlanAsync(SoftwareProfile profile)");

        var boundaryIndex = request.IndexOf(
            "ProductionExecutionAttempted = true;",
            StringComparison.Ordinal);
        var executionIndex = request.IndexOf(
            "await _executionCoordinator.ExecuteAsync(_preparedRequest)",
            StringComparison.Ordinal);
        boundaryIndex.Should().BeGreaterThanOrEqualTo(0);
        executionIndex.Should().BeGreaterThan(boundaryIndex);

        request.Should().Contain("if (ProductionExecutionAttempted)")
            .And.Contain("catch")
            .And.Contain("CreateUnknownAttempt()")
            .And.Contain("ProductionCompleted = false;")
            .And.Contain("ProductionResidueReviewRecommended = false;")
            .And.NotContain("ProductionExecutionAttempted = outcome.ProductionAttempted;");
        reset.Should().Contain("ProductionExecutionAttempted = false;");

        parent.Should().Contain("if (window.ProductionExecutionAttempted)")
            .And.Contain("await TryScanSoftwareProfilesAfterProductionAttemptAsync()");
    }

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

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
