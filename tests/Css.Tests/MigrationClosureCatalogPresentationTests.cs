using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationClosureCatalogPresentationTests
{
    [Fact]
    public void Protected_profiles_keep_base_tile_authority_when_historical_closure_needs_attention()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        windowsRoot.Should().NotBeNullOrWhiteSpace();
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Windows Component",
                Category = SoftwareCategory.SystemTool,
                InstallPath = Path.Combine(windowsRoot, "System32", "Component")
            },
            new SoftwareProfile
            {
                Name = "Unknown Managed Component",
                Category = SoftwareCategory.Unknown,
                InstallPath = Path.Combine(windowsRoot, "System32", "UnknownComponent")
            }
        };

        var states = profiles.Select(profile =>
        {
            var tile = AppPresentationBuilder.CreateTile(profile);
            return (Base: tile, State: MigrationClosureTileStatePresenter.Create(
                profile,
                tile,
                Closure(profile.Name, MigrationClosureFindingKind.OriginalWriteReturned)));
        }).ToList();

        states.Should().OnlyContain(item => !item.State.ShouldPrioritize);
        states[0].State.ShortTag.Should().Be("系统组件");
        states[0].State.Status.Should().Be(AppTileStatus.System);
        states[1].State.ShortTag.Should().Be("系统归属待确认");
        states[1].State.Status.Should().Be(AppTileStatus.Attention);
    }

    [Fact]
    public void Ordinary_closure_warning_is_prioritized_and_healthy_state_only_relabels_a_normal_tile()
    {
        var warningProfile = new SoftwareProfile
        {
            Name = "Migrated App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Migrated App"
        };
        var residentProfile = new SoftwareProfile
        {
            Name = "Resident App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Resident App",
            StartupEntries = ["Resident Startup"]
        };

        var warningTile = AppPresentationBuilder.CreateTile(warningProfile);
        var warning = MigrationClosureTileStatePresenter.Create(
            warningProfile,
            warningTile,
            Closure(warningProfile.Name, MigrationClosureFindingKind.OriginalWriteReturned));
        var healthy = MigrationClosureTileStatePresenter.Create(
            warningProfile,
            warningTile,
            Closure(warningProfile.Name, MigrationClosureFindingKind.RedirectHealthy));
        var residentTile = AppPresentationBuilder.CreateTile(residentProfile);
        var residentHealthy = MigrationClosureTileStatePresenter.Create(
            residentProfile,
            residentTile,
            Closure(residentProfile.Name, MigrationClosureFindingKind.RedirectHealthy));

        warning.ShouldPrioritize.Should().BeTrue();
        warning.ShortTag.Should().Be("迁移未闭环");
        warning.Status.Should().Be(AppTileStatus.Attention);
        healthy.ShouldPrioritize.Should().BeFalse();
        healthy.ShortTag.Should().Be("迁移正常");
        healthy.Status.Should().Be(AppTileStatus.Normal);
        residentHealthy.ShortTag.Should().Be(residentTile.ShortTag);
        residentHealthy.Status.Should().Be(residentTile.Status);
    }

    [Fact]
    public void Catalog_summary_separates_reviewable_closures_from_protected_historical_records()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var profiles = new[]
        {
            Profile("Attention App", SoftwareCategory.Normal, @"D:\Software\Attention"),
            Profile("Healthy App", SoftwareCategory.Normal, @"D:\Software\Healthy"),
            Profile("Windows Component", SoftwareCategory.SystemTool, Path.Combine(windowsRoot, "System32", "Component")),
            Profile("Unknown Managed", SoftwareCategory.Unknown, Path.Combine(windowsRoot, "System32", "Unknown"))
        };
        var closures = new Dictionary<string, MigrationClosureSummaryViewModel>(StringComparer.OrdinalIgnoreCase)
        {
            [profiles[0].Name] = Closure(profiles[0].Name, MigrationClosureFindingKind.OriginalWriteReturned),
            [profiles[1].Name] = Closure(profiles[1].Name, MigrationClosureFindingKind.RedirectHealthy),
            [profiles[2].Name] = Closure(profiles[2].Name, MigrationClosureFindingKind.OriginalWriteReturned),
            [profiles[3].Name] = Closure(profiles[3].Name, MigrationClosureFindingKind.OriginalWriteReturned)
        };

        var summary = MigrationClosureCatalogSummaryPresenter.Create(
            profiles,
            profile => closures.GetValueOrDefault(profile.Name));

        summary.ReviewableRecordCount.Should().Be(2);
        summary.AttentionCount.Should().Be(1);
        summary.ProtectedHistoricalRecordCount.Should().Be(2);
        summary.Text.Should().Contain("2 个普通应用")
            .And.Contain("1 个需要检查")
            .And.Contain("2 条系统相关旧迁移记录")
            .And.Contain("仅供查看")
            .And.NotContain(windowsRoot)
            .And.NotContain(@"D:\");
    }

    [Fact]
    public void Main_window_uses_core_tile_priority_and_catalog_summary_presenters()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var refresh = ExtractMethod(
            code,
            "private void RefreshAppCatalog",
            "private string BuildMigrationClosureCatalogSummary");
        var summary = ExtractMethod(
            code,
            "private string BuildMigrationClosureCatalogSummary",
            "private void ClearAppDrawer");
        var tile = ExtractType(code, "private sealed class AppTileUi", "private sealed class SystemToolShortcutView");

        refresh.Should().Contain("MigrationClosureTileStatePresenter.ShouldPrioritize(profile, FindMigrationClosure(profile))");
        summary.Should().Contain("MigrationClosureCatalogSummaryPresenter.Create(_softwareProfiles, FindMigrationClosure).Text")
            .And.NotContain("summary.NeedsAttention");
        tile.Should().Contain("MigrationClosureTileStatePresenter.Create(profile, tile, migrationClosure)")
            .And.Contain("ShortTag = closureState.ShortTag")
            .And.Contain("StatusColor(closureState.Status)")
            .And.NotContain("var closureNeedsAttention =")
            .And.NotContain("? \"迁移未闭环\"");
    }

    private static SoftwareProfile Profile(string name, SoftwareCategory category, string installPath) =>
        new() { Name = name, Category = category, InstallPath = installPath };

    private static MigrationClosureSummaryViewModel Closure(
        string softwareName,
        MigrationClosureFindingKind state) =>
        new()
        {
            SoftwareName = softwareName,
            DisplayName = softwareName,
            TargetAppNameCandidate = softwareName,
            State = state,
            Headline = "迁移状态",
            Detail = "只读历史记录",
            ObservedPathCount = 1,
            MonitoringStartedAtUtc = DateTimeOffset.UtcNow
        };

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

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static string ExtractType(string source, string startMarker, string endMarker) =>
        ExtractMethod(source, startMarker, endMarker);
}
