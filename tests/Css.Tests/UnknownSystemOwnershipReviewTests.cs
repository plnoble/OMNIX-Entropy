using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public class UnknownSystemOwnershipReviewTests
{
    [Fact]
    public void Unknown_profile_under_windows_root_is_read_only_without_reclassification()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        windowsRoot.Should().NotBeNullOrWhiteSpace();
        var profile = Profile(Path.Combine(windowsRoot, "System32", "UnknownComponent"));

        var tile = AppPresentationBuilder.CreateTile(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        tile.Category.Should().Be(SoftwareCategory.Unknown);
        tile.Status.Should().Be(AppTileStatus.Attention);
        tile.ShortTag.Should().Be("系统归属待确认");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Observe);
        drawer.AgentAdvice.Text.Should().Contain("Windows 管理位置")
            .And.Contain("归属未确认")
            .And.Contain("不会提供普通应用操作");
        drawer.AvailableActions.Where(action => action.Kind != AppActionKind.TechnicalDetails)
            .Should().OnlyContain(action => !action.IsEnabled && action.Reason.Contains("归属"));
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.TechnicalDetails)
            .IsEnabled.Should().BeTrue();
        drawer.MigrationSummary.Should().Contain("系统归属待确认");
        drawer.CacheCleanupSummary.Should().Contain("系统归属待确认");
        drawer.StartupControlSummary.Should().Contain("系统归属待确认");

        Visible(tile, drawer).Should().NotContain(windowsRoot)
            .And.NotContain("UnknownService")
            .And.NotContain("unknown-uninstall.exe");
    }

    [Fact]
    public void Unknown_profile_under_windows_apps_is_also_read_only()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        programFiles.Should().NotBeNullOrWhiteSpace();
        var profile = Profile(Path.Combine(programFiles, "WindowsApps", "Unknown.Package"));

        var tile = AppPresentationBuilder.CreateTile(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        tile.ShortTag.Should().Be("系统归属待确认");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Observe);
        drawer.AvailableActions.Where(action => action.Kind != AppActionKind.TechnicalDetails)
            .Should().OnlyContain(action => !action.IsEnabled);
    }

    [Fact]
    public void Microsoft_publisher_alone_does_not_block_an_unknown_ordinary_install()
    {
        var profile = Profile(@"D:\Software\Microsoft Utility", "Microsoft Corporation");

        var tile = AppPresentationBuilder.CreateTile(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        tile.ShortTag.Should().NotBe("系统归属待确认");
        drawer.AgentAdvice.Text.Should().NotContain("系统归属待确认");
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.Uninstall)
            .IsEnabled.Should().BeTrue();
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.CacheCleanup)
            .IsEnabled.Should().BeTrue();
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.StartupControl)
            .IsEnabled.Should().BeTrue();
    }

    private static SoftwareProfile Profile(string installPath, string? publisher = null) =>
        new()
        {
            Name = "Unknown Utility",
            Publisher = publisher,
            Category = SoftwareCategory.Unknown,
            InstallPath = installPath,
            UninstallCommand = "unknown-uninstall.exe",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Unknown\Cache"],
            CacheSizeBytes = 64L * 1024 * 1024,
            StartupEntries = ["Unknown Startup"],
            Services = ["UnknownService"]
        };

    private static string Visible(AppTileViewModel tile, AppDrawerViewModel drawer) =>
        string.Join("\n",
            tile.VisibleText,
            tile.AccessibilityName,
            drawer.InstallLocationSummary,
            drawer.SizeSummary,
            drawer.ResidencySummary,
            drawer.AgentAdvice.Text,
            drawer.AgentAdvice.Reason,
            drawer.MigrationSummary,
            drawer.CacheCleanupSummary,
            drawer.StartupControlSummary,
            string.Join("\n", drawer.AvailableActions.Select(action => action.Reason)));
}
