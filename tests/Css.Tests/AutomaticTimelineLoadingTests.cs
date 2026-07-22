using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticTimelineLoadingTests
{
    [Fact]
    public void Timeline_navigation_ensures_once_while_refresh_paths_force_a_reload()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var showPage = Method(code, "private void ShowPage", "private static void SetNavSelected");
        var click = Method(code, "private async void LoadTimeline_Click", "private async void RestoreTimeline_Click");
        var ensure = Method(code, "private Task EnsureTimelineLoadedAsync", "private async Task LoadTimelineAsync");
        var refresh = Method(code, "private async Task LoadTimelineAsync", "private async Task<bool> LoadTimelineCoreAsync");
        var core = Method(code, "private async Task<bool> LoadTimelineCoreAsync", "private async Task RefreshQuarantinePolicyAsync");

        xaml.Should().Contain("x:Name=\"LoadTimelineButton\"");
        xaml.Should().Contain("Content=\"重新加载\"");
        xaml.Should().Contain("进入本页会自动读取");
        xaml.Should().NotContain("Content=\"&#x52A0;&#x8F7D;&#x65F6;&#x95F4;&#x7EBF;\"");

        showPage.Should().Contain("EnsureTimelineLoadedAsync");
        showPage.Should().NotContain("_ = LoadTimelineAsync()");
        click.Should().Contain("await LoadTimelineAsync()");
        ensure.Should().Contain("_timelineLoadGate.EnsureLoadedAsync");
        refresh.Should().Contain("_timelineLoadGate.RefreshAsync");
        core.Should().Contain("_timelineStore.LoadRecentAsync");
        core.Should().NotContain("ex.Message");

        string.Join("\n", showPage, ensure, refresh, core)
            .Should().NotContain("RestoreTimelineItemAsync")
            .And.NotContain("QuarantinePurgeOperationPolicy")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete");
    }

    private static string Method(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
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
