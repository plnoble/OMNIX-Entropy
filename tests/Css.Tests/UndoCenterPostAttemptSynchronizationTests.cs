using FluentAssertions;

namespace Css.Tests;

public sealed class UndoCenterPostAttemptSynchronizationTests
{
    [Fact]
    public void Quarantine_purge_refreshes_timeline_after_returned_and_thrown_attempts()
    {
        var main = ReadMainWindow();
        var method = SourceMethodExtractor.Extract(
            main,
            "private async void ReviewQuarantineCleanup_Click(object sender, RoutedEventArgs e)");

        AssertTimelineSynchronization(method, "await pipeline.ExecuteAsync(confirmed)");
    }

    [Fact]
    public void Quarantine_restore_refreshes_timeline_after_returned_and_thrown_attempts()
    {
        var main = ReadMainWindow();
        var method = SourceMethodExtractor.Extract(
            main,
            "private async Task RestoreQuarantineTimelineItemAsync(ActionTimelineItemViewModel item)");

        AssertTimelineSynchronization(method, "await pipeline.ExecuteAsync(descriptor)");
    }

    [Fact]
    public void Startup_restore_refreshes_apps_and_timeline_after_returned_and_thrown_attempts()
    {
        var main = ReadMainWindow();
        var helper = SourceMethodExtractor.Extract(
            main,
            "private async Task RefreshStartupStateAfterAttemptAsync()");
        var method = SourceMethodExtractor.Extract(
            main,
            "private async Task RestoreStartupTimelineItemAsync(ActionTimelineItemViewModel item)");
        const string executePipeline = "await pipeline.ExecuteAsync(descriptor)";
        const string synchronize = "await RefreshStartupStateAfterAttemptAsync();";

        method.Should().Contain("var pipelineAttempted = false;");
        method.Should().Contain("var stateSynchronized = false;");
        method.IndexOf("pipelineAttempted = true;", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(executePipeline, StringComparison.Ordinal));
        method.IndexOf(executePipeline, StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(synchronize, StringComparison.Ordinal));
        method.Should().Contain("if (pipelineAttempted && !stateSynchronized)");
        method.Split(synchronize, StringSplitOptions.None).Length.Should().Be(3);
        method.Should().NotContain("if (result.Success)");

        helper.Should().Contain("SetSoftwareProfiles(await ScanSoftwareProfilesAsync());");
        helper.Should().Contain("await LoadTimelineAsync();");
        helper.Should().NotContain("SafetyOperationPipeline");
        helper.Should().NotContain("DisableAsync");
        helper.Should().NotContain("RestoreAsync");
        helper.Should().NotContain("Registry");
    }

    private static void AssertTimelineSynchronization(string method, string executePipeline)
    {
        const string synchronize = "await LoadTimelineAsync();";
        method.Should().Contain("var pipelineAttempted = false;");
        method.Should().Contain("var stateSynchronized = false;");
        method.IndexOf("pipelineAttempted = true;", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(executePipeline, StringComparison.Ordinal));
        method.IndexOf(executePipeline, StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf(synchronize, StringComparison.Ordinal));
        method.Should().Contain("if (pipelineAttempted && !stateSynchronized)");
        method.Split(synchronize, StringSplitOptions.None).Length.Should().Be(3);
        method.Split("pipeline.ExecuteAsync", StringSplitOptions.None).Length.Should().Be(2);
    }

    private static string ReadMainWindow() => File.ReadAllText(FindRepositoryFile(
        "src", "Css.App", "MainWindow.xaml.cs"));

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
