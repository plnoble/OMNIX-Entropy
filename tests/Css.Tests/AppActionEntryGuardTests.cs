using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppActionEntryGuardTests
{
    [Fact]
    public void Entry_policy_matches_drawer_actions_for_protected_and_ordinary_profiles()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        windowsRoot.Should().NotBeNullOrWhiteSpace();
        var protectedProfiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Windows Component",
                Category = SoftwareCategory.SystemTool,
                InstallPath = Path.Combine(windowsRoot, "System32", "Component"),
                UninstallCommand = "remove.exe",
                CachePaths = [Path.Combine(windowsRoot, "Temp", "Component")],
                StartupEntries = ["Component Startup"]
            },
            new SoftwareProfile
            {
                Name = "Unknown Managed Component",
                Category = SoftwareCategory.Unknown,
                InstallPath = Path.Combine(windowsRoot, "System32", "UnknownComponent"),
                UninstallCommand = "unknown-remove.exe",
                CachePaths = [Path.Combine(windowsRoot, "Temp", "Unknown")],
                StartupEntries = ["Unknown Startup"]
            }
        };
        var ordinary = new SoftwareProfile
        {
            Name = "Ordinary App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Ordinary App",
            UninstallCommand = "ordinary-remove.exe",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Ordinary\Cache"],
            StartupEntries = ["Ordinary Startup"]
        };
        var guardedKinds = new[]
        {
            AppActionKind.Uninstall,
            AppActionKind.CacheCleanup,
            AppActionKind.StartupControl
        };

        foreach (var profile in protectedProfiles)
        {
            var drawer = AppPresentationBuilder.CreateDrawer(profile);
            foreach (var kind in guardedKinds)
            {
                var action = drawer.AvailableActions.Single(item => item.Kind == kind);
                var entry = AppActionEntryPolicy.Evaluate(drawer, kind);

                entry.IsAllowed.Should().BeFalse();
                entry.Reason.Should().Be(action.Reason);
            }
        }

        var ordinaryDrawer = AppPresentationBuilder.CreateDrawer(ordinary);
        guardedKinds.Should().OnlyContain(kind =>
            AppActionEntryPolicy.Evaluate(ordinaryDrawer, kind).IsAllowed);
    }

    [Fact]
    public void Main_window_guards_each_central_action_before_specific_evidence_or_plan_work()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var uninstall = ExtractMethod(
            code,
            "private async Task ShowUninstallPlanAsync",
            "private async void PreviewMigration_Click");
        var cache = ExtractMethod(
            code,
            "private void ShowCacheCleanupPreview",
            "private async void PreviewStartupControl_Click");
        var startup = ExtractMethod(
            code,
            "private async Task ShowStartupControlPreviewAsync",
            "private void ApplyDrawerActionHost");

        AssertGuardBefore(
            uninstall,
            "AppActionEntryPolicy.Evaluate(drawer, AppActionKind.Uninstall)",
            "new WindowsRestorePointScanner()",
            "UninstallPlanPresentationBuilder.Create",
            "new UninstallPlanWindow");
        uninstall.Should().Contain("AppDrawerActionHostPresenter.UninstallRefused(entry.Reason)");

        AssertGuardBefore(
            cache,
            "AppActionEntryPolicy.Evaluate(drawer, AppActionKind.CacheCleanup)",
            "AppCacheCleanupPlanBuilder.Create",
            "_pendingDrawerOperation = plan.Operation");
        cache.Should().Contain("AppDrawerActionHostPresenter.CacheCleanupRefused(entry.Reason)");

        AssertGuardBefore(
            startup,
            "AppActionEntryPolicy.Evaluate(drawer, AppActionKind.StartupControl)",
            "AppStartupSettingsHandoffPresenter.Create",
            "StartupControlPreparationService.PrepareAsync",
            "_pendingStartupTargetAppName =");
        startup.Should().Contain("AppDrawerActionHostPresenter.StartupControlRefused(entry.Reason)");
    }

    private static void AssertGuardBefore(string method, string decision, params string[] laterMarkers)
    {
        var decisionIndex = method.IndexOf(decision, StringComparison.Ordinal);
        var guardIndex = method.IndexOf("if (!entry.IsAllowed)", decisionIndex, StringComparison.Ordinal);
        decisionIndex.Should().BeGreaterThanOrEqualTo(0);
        guardIndex.Should().BeGreaterThan(decisionIndex);
        foreach (var marker in laterMarkers)
        {
            method.IndexOf(marker, StringComparison.Ordinal)
                .Should().BeGreaterThan(guardIndex, marker + " must run only after the entry guard");
        }
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(segments));
    }

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }
}
