using FluentAssertions;

namespace Css.Tests;

public sealed class HealthDigestEvidenceNavigationTests
{
    [Fact]
    public void Persisted_digest_action_hydrates_current_evidence_before_success_copy()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var handler = SourceMethodExtractor.Extract(
            code,
            "private async void OpenHealthDigestEvidence_Click(object sender, RoutedEventArgs e)");
        var apply = SourceMethodExtractor.Extract(
            code,
            "private void ApplyHealthDigestHistory(HealthDigestHistoryViewModel history)");

        xaml.Should().Contain("Content=\"重新体检并查看当前证据\"")
            .And.Contain(
                "AutomationProperties.AutomationId=\"OpenHealthDigestEvidenceButton\"");
        handler.Should().Contain("_isOpeningHealthDigestEvidence")
            .And.Contain("OpenHealthDigestEvidenceButton.IsEnabled = false")
            .And.Contain("ShowPage(\"CDrive\")")
            .And.Contain("await EnsureHealthScanLoadedAsync()")
            .And.Contain("_healthScanLoadGate.HasCompletedLoad")
            .And.Contain("_lastHealthSummary is null")
            .And.Contain("_healthDigestHistoryHasEvidence")
            .And.NotContain("RefreshHealthScanAsync")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("File.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Directory.Move");

        var loadIndex = handler.IndexOf(
            "await EnsureHealthScanLoadedAsync()",
            StringComparison.Ordinal);
        var readinessIndex = handler.IndexOf(
            "_healthScanLoadGate.HasCompletedLoad",
            StringComparison.Ordinal);
        var successIndex = handler.IndexOf(
            "\\u5f53\\u524d C \\u76d8\\u8bc1\\u636e\\u5df2\\u6253\\u5f00",
            StringComparison.Ordinal);
        loadIndex.Should().BeGreaterThanOrEqualTo(0);
        readinessIndex.Should().BeGreaterThan(loadIndex);
        successIndex.Should().BeGreaterThan(readinessIndex);

        apply.Should().Contain("_healthDigestHistoryHasEvidence = history.HasEvidence")
            .And.Contain("!_isOpeningHealthDigestEvidence")
            .And.Contain("_lastHealthSummary is null")
            .And.Contain("\\u91cd\\u65b0\\u4f53\\u68c0\\u5e76\\u67e5\\u770b\\u5f53\\u524d\\u8bc1\\u636e")
            .And.Contain("\\u67e5\\u770b\\u5f53\\u524d C \\u76d8\\u8bc1\\u636e");
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
