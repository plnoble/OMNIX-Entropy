using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationOneShotSubmissionTests
{
    [Fact]
    public void Submitted_plan_is_one_shot_and_unknown_outcomes_force_parent_rescan()
    {
        var planCode = Read("src", "Css.App", "MigrationPlanWindow.xaml.cs");
        var mainCode = Read("src", "Css.App", "MainWindow.xaml.cs");
        var request = SourceMethodExtractor.Extract(
            planCode,
            "private async void RequestMigration_Click(object sender, RoutedEventArgs e)");
        var availability = SourceMethodExtractor.Extract(
            planCode,
            "private void UpdateActionAvailability()");
        var parent = SourceMethodExtractor.Extract(
            mainCode,
            "private async Task ShowMigrationPlanAsync(SoftwareProfile profile)");

        var boundaryIndex = request.IndexOf(
            "ProductionExecutionAttempted = true;",
            StringComparison.Ordinal);
        var executionIndex = request.IndexOf(
            "await _executionCoordinator.ExecuteAsync(request)",
            StringComparison.Ordinal);
        boundaryIndex.Should().BeGreaterThanOrEqualTo(0);
        executionIndex.Should().BeGreaterThan(boundaryIndex);

        request.Should().Contain("catch")
            .And.Contain("ProductionCompleted = false;")
            .And.Contain("LastExecutionConclusion =")
            .And.Contain("ShowExecutionResult(OperationResult.Fail(")
            .And.NotContain("ProductionExecutionAttempted = outcome.ProductionAttempted;");
        availability.Should().Contain("&& !ProductionExecutionAttempted")
            .And.NotContain("ProductionExecutionAttempted = false");

        parent.Should().Contain("if (window.ProductionExecutionAttempted)")
            .And.Contain("await TryScanSoftwareProfilesAfterProductionAttemptAsync()")
            .And.Contain("await RefreshMigrationClosureAsync(refreshUi: true)");
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
