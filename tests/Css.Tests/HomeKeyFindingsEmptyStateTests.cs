using FluentAssertions;

namespace Css.Tests;

public sealed class HomeKeyFindingsEmptyStateTests
{
    [Fact]
    public void Home_places_a_stable_compact_empty_state_before_the_collapsed_findings_list()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var emptyState = xaml.IndexOf(
            "x:Name=\"KeyFindingsEmptyStateTextBlock\"",
            StringComparison.Ordinal);
        var findingsList = xaml.IndexOf(
            "x:Name=\"KeyFindingsListBox\"",
            StringComparison.Ordinal);
        var listDeclaration = xaml.Substring(
            findingsList,
            Math.Min(500, xaml.Length - findingsList));

        emptyState.Should().BeGreaterThanOrEqualTo(0);
        findingsList.Should().BeGreaterThan(emptyState);
        xaml.Should().Contain(
            "AutomationProperties.AutomationId=\"KeyFindingsEmptyStateTextBlock\"");
        xaml.Should().Contain("完成体检后，这里会显示最值得处理的项目。");
        listDeclaration.Should().Contain("Visibility=\"Collapsed\"");
    }

    [Fact]
    public void Health_summary_switches_between_valid_empty_copy_and_real_findings()
    {
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var start = main.IndexOf(
            "private HealthCheckSummary? RefreshHealthSummaryFromBase()",
            StringComparison.Ordinal);
        var end = main.IndexOf(
            "private async Task<bool> TrySaveHealthDigestAsync",
            start,
            StringComparison.Ordinal);
        var method = main.Substring(start, end - start);

        method.Should().Contain("var hasKeyFindings = summary.KeyFindings.Count > 0;");
        method.Should().Contain("KeyFindingsEmptyStateTextBlock.Visibility = hasKeyFindings");
        method.Should().Contain("KeyFindingsListBox.Visibility = hasKeyFindings");
        method.Should().Contain("本次体检没有发现需要优先处理的项目。");
        method.Should().Contain("KeyFindingsListBox.ItemsSource = summary.KeyFindings;");
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
