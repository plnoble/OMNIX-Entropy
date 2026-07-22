using FluentAssertions;

namespace Css.Tests;

public sealed class RecommendationCleanupPostAttemptTests
{
    [Fact]
    public void Direct_cleanup_refreshes_timeline_and_health_after_every_pipeline_attempt()
    {
        var main = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "MainWindow.xaml.cs"));
        var helper = SourceMethodExtractor.Extract(
            main,
            "private async Task RefreshRecommendationCleanupStateAfterAttemptAsync()");
        var execute = SourceMethodExtractor.Extract(
            main,
            "private async Task ExecuteSelectedRecommendationAsync()");

        const string attempted = "pipelineAttempted = true;";
        const string executePipeline = "await pipeline.ExecuteAsync(descriptor)";
        const string synchronize = "await RefreshRecommendationCleanupStateAfterAttemptAsync();";
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
        execute.Should().Contain("RecommendationSelectionPresenter.Create(");
        execute.Should().NotContain("ExecuteRecommendationButton.IsEnabled = true;");
        execute.Should().NotContain("await LoadTimelineAsync();");
        execute.Should().NotContain("await RefreshHealthScanAsync();");

        helper.Should().Contain("await LoadTimelineAsync();");
        helper.Should().Contain("await RefreshHealthScanAsync();");
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
