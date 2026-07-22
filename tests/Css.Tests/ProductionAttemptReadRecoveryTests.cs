using FluentAssertions;

namespace Css.Tests;

public sealed class ProductionAttemptReadRecoveryTests
{
    [Fact]
    public void Production_attempt_rescans_use_one_read_only_failure_boundary()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var helper = SourceMethodExtractor.Extract(
            code,
            "private async Task<IReadOnlyList<SoftwareProfile>?> TryScanSoftwareProfilesAfterProductionAttemptAsync()");
        var uninstall = SourceMethodExtractor.Extract(
            code,
            "private async Task ShowUninstallPlanAsync(SoftwareProfile profile)");
        var migration = SourceMethodExtractor.Extract(
            code,
            "private async Task ShowMigrationPlanAsync(SoftwareProfile profile)");

        helper.Should().Contain("try")
            .And.Contain("return await ScanSoftwareProfilesAsync();")
            .And.Contain("catch")
            .And.Contain("return null;")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("ExecuteAsync(")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete");

        AssertAttemptRefresh(uninstall);
        uninstall.Should().Contain("refreshedProfiles is null")
            .And.Contain("return;")
            .And.Contain("ReviewUninstallResidueAsync(profile, refreshedProfiles)");
        uninstall.IndexOf("refreshedProfiles is null", StringComparison.Ordinal)
            .Should().BeLessThan(uninstall.IndexOf(
                "ReviewUninstallResidueAsync(profile, refreshedProfiles)",
                StringComparison.Ordinal));

        AssertAttemptRefresh(migration);
        migration.Should().Contain("refreshedProfiles is null")
            .And.Contain("return;")
            .And.Contain("RefreshMigrationClosureAsync(refreshUi: true)");
        migration.IndexOf("refreshedProfiles is null", StringComparison.Ordinal)
            .Should().BeLessThan(migration.IndexOf(
                "RefreshMigrationClosureAsync(refreshUi: true)",
                StringComparison.Ordinal));
    }

    private static void AssertAttemptRefresh(string method)
    {
        method.Should().Contain("if (window.ProductionExecutionAttempted)")
            .And.Contain("await TryScanSoftwareProfilesAfterProductionAttemptAsync()");
        method.IndexOf("if (window.ProductionExecutionAttempted)", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(
                "await TryScanSoftwareProfilesAfterProductionAttemptAsync()",
                StringComparison.Ordinal));
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
