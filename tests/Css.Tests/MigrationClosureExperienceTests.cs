using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Recommendations;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationClosureExperienceTests
{
    [Fact]
    public async Task Latest_scan_observes_only_the_newest_record_for_each_software()
    {
        var now = DateTimeOffset.UtcNow;
        var store = new MemoryStore(
        [
            Record("old", "Example App", @"C:\old", now.AddDays(-1)),
            Record("new", "Example App", @"C:\new", now),
            Record("other", "Other App", @"C:\other", now.AddMinutes(-1))
        ]);
        var observer = new RecordingObserver();

        var findings = await MigrationClosureMonitor.ScanLatestAsync(store, observer);

        observer.Paths.Should().BeEquivalentTo(@"C:\new", @"C:\other");
        findings.Should().HaveCount(2);
        findings.Should().Contain(finding =>
            finding.SoftwareName == "Example App"
            && finding.MonitoringRecordId == "new"
            && finding.MonitoringStartedAtUtc == now);
        findings.Should().NotContain(finding => finding.MonitoringRecordId == "old");
    }

    [Fact]
    public void Presenter_prioritizes_write_return_and_never_exposes_paths()
    {
        var now = DateTimeOffset.UtcNow;
        var summaries = MigrationClosurePresenter.CreateLatest(
        [
            Finding("Example App", "record", now, MigrationClosureFindingKind.RedirectHealthy, @"C:\private\cache"),
            Finding("Example App", "record", now, MigrationClosureFindingKind.OriginalWriteReturned, @"C:\private\data")
        ]);

        var summary = summaries.Should().ContainSingle().Subject;
        summary.State.Should().Be(MigrationClosureFindingKind.OriginalWriteReturned);
        summary.NeedsAttention.Should().BeTrue();
        summary.CanExecuteDirectly.Should().BeFalse();
        string.Join("\n", summary.Headline, summary.Detail)
            .Should().NotContain(@"C:\")
            .And.NotContain("private");

        var profile = new Css.Core.Software.SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install"
        };
        var reviewedPlan = MigrationPlanPresentationBuilder.AddClosureReview(
            MigrationPlanPresentationBuilder.Create(profile),
            summary);
        reviewedPlan.Sections[0].Title.Should().Be("先复查迁移闭环");
        reviewedPlan.CanRunMigration.Should().BeFalse();
        reviewedPlan.RequiresSnapshot.Should().BeTrue();
        reviewedPlan.IsAlreadyReasonable.Should().BeFalse();
        reviewedPlan.Summary.Should().NotContain(@"C:\private");
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var drawerReview = AppDrawerActionHostPresenter.ShowMigration(drawer, summary);
        drawerReview.Title.Should().Be("迁移闭环复查");
        drawerReview.CanExecuteDirectly.Should().BeFalse();
        drawerReview.Summary.Should().NotContain(@"C:\private");
    }

    [Fact]
    public async Task Unsafe_monitoring_path_is_rejected_before_observation()
    {
        var record = Record(
            "unsafe",
            "Example App",
            @"\\server\private\cache",
            DateTimeOffset.UtcNow);
        var observer = new RecordingObserver();

        var action = () => MigrationClosureMonitor.ScanLatestAsync(
            new MemoryStore([record]),
            observer);

        await action.Should().ThrowAsync<InvalidOperationException>();
        observer.Paths.Should().BeEmpty();
    }

    [Fact]
    public void Unsafe_software_name_is_not_promoted_to_an_app_target()
    {
        var summary = MigrationClosurePresenter.CreateLatest(
        [
            Finding(@"C:\private\App", "record", DateTimeOffset.UtcNow,
                MigrationClosureFindingKind.OriginalPathMissing, @"C:\private\App")
        ]).Should().ContainSingle().Subject;

        summary.DisplayName.Should().Be("某个已迁移应用");
        summary.TargetAppNameCandidate.Should().BeNull();
        summary.Headline.Should().NotContain(@"C:\");
    }

    [Fact]
    public void Health_enrichment_is_idempotent_and_ambiguous_names_are_not_targets()
    {
        var health = new HealthCheckSummary
        {
            OverallScore = 80,
            Dimensions =
            [
                new HealthDimensionResult { Name = "磁盘健康", Result = "正常", Rating = "正常" }
            ],
            KeyFindings = []
        };
        var closure = MigrationClosurePresenter.CreateLatest(
        [
            Finding("Duplicate App", "record", DateTimeOffset.UtcNow,
                MigrationClosureFindingKind.OriginalWriteReturned, @"C:\duplicate")
        ]);

        var once = MigrationClosureHealthEnricher.Apply(
            health,
            closure,
            _ => MigrationClosureTargetDisposition.Unavailable);
        var twice = MigrationClosureHealthEnricher.Apply(
            once,
            closure,
            _ => MigrationClosureTargetDisposition.Unavailable);

        twice.Dimensions.Count(dimension => dimension.Name == "迁移闭环").Should().Be(1);
        twice.Dimensions[1].Name.Should().Be("迁移闭环");
        var finding = twice.KeyFindings
            .Should().ContainSingle(item => item.Kind == HealthFindingKind.MigrationClosure)
            .Subject;
        twice.KeyFindings[0].Should().BeSameAs(finding);
        finding.TargetAppName.Should().BeNull();
        finding.Action.Should().Be(RecommendationAction.Observe);
        finding.Text.Should().Contain("仅供查看");
        finding.Risk.Should().Be(RiskLevel.Medium);
    }

    [Fact]
    public void Main_window_uses_read_only_observer_and_surfaces_closure_in_home_and_apps()
    {
        var main = ReadSource("src", "Css.App", "MainWindow.xaml.cs");
        var win32 = ReadSource("src", "Css.Win32", "Migration", "WindowsDirectoryMigrationPathAdapter.cs");

        main.Should().Contain("new WindowsMigrationPathObserver()")
            .And.Contain("MigrationClosureMonitor.ScanLatestAsync")
            .And.Contain("MigrationClosureHealthEnricher.Apply")
            .And.Contain("ResolveMigrationClosureTargetDisposition")
            .And.Contain("AppTileUi.From(profile, FindMigrationClosure(profile))")
            .And.Contain("MigrationClosureTileStatePresenter.ShouldPrioritize")
            .And.Contain("BuildMigrationClosureCatalogSummary()")
            .And.Contain("MigrationClosureDrawerStatePresenter.Create")
            .And.Contain("migrationState.AdviceText")
            .And.Contain("MigrationPlanPresentationBuilder.AddClosureReview")
            .And.Contain("ShowMigration(drawer, migrationClosure)")
            .And.Contain("migrationState.ButtonText")
            .And.NotContain("new WindowsDirectoryMigrationPathAdapter(");
        win32.Should().Contain("class WindowsMigrationPathObserver : IMigrationPathObserver");
        ExtractType(win32, "public sealed class WindowsMigrationPathObserver", "public sealed class WindowsDirectoryMigrationPathAdapter")
            .Should().NotContain("MoveAndRedirectAsync")
            .And.NotContain("RollbackAsync")
            .And.NotContain("Directory.Move")
            .And.NotContain("Directory.Delete")
            .And.NotContain("CreateSymbolicLink");
    }

    [Fact]
    public void Gui_smoke_proves_beginner_closure_warning_without_invoking_migration()
    {
        var script = ReadSource(".omx", "gui-migration-closure-smoke.ps1");

        script.Should().Contain("OMNIX_ENTROPY_DATA_ROOT")
            .And.Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE")
            .And.Contain("migration-monitor-")
            .And.Contain("ClosureDimensionAutomationId")
            .And.Contain("KeyFindingsListBox")
            .And.Contain("AppTilesListBox")
            .And.Contain("DrawerAdviceTextBlock")
            .And.Contain("DrawerMigrateButton")
            .And.Contain("ClosureTag = Join-Chars")
            .And.Contain("ReviewMigration = Join-Chars")
            .And.Contain("rawFixturePathHidden = $true")
            .And.Contain("noOperationExecuted = -not $migrationActionInvoked")
            .And.Contain("Assert-ChildPath")
            .And.Contain("Save-WindowScreenshot");
        script.Should().NotContain("Invoke-Element $migrationButton")
            .And.NotContain("CreateSymbolicLink")
            .And.NotContain("New-Item -ItemType Junction")
            .And.NotContain("Directory.Move")
            .And.NotContain("Directory.Delete");
    }

    private static MigrationMonitoringRecord Record(
        string id,
        string softwareName,
        string path,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            SoftwareName = softwareName,
            SnapshotId = "snapshot",
            RollbackManifestPath = "manifest",
            RollbackManifestSha256 = new string('A', 64),
            CreatedAtUtc = createdAt,
            Paths =
            [
                new MigrationMonitoringPath
                {
                    OriginalPath = path,
                    ExpectedRedirectTarget = @"D:\target"
                }
            ]
        };

    private static MigrationClosureFinding Finding(
        string softwareName,
        string recordId,
        DateTimeOffset startedAt,
        MigrationClosureFindingKind kind,
        string path) =>
        new()
        {
            SoftwareName = softwareName,
            MonitoringRecordId = recordId,
            MonitoringStartedAtUtc = startedAt,
            Kind = kind,
            OriginalPath = path,
            Summary = "low-level summary " + path
        };

    private static string ReadSource(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "ComputerSecuritySoftware.slnx")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string ExtractType(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private sealed class MemoryStore(IReadOnlyList<MigrationMonitoringRecord> records)
        : IMigrationMonitoringStore
    {
        public Task SaveAsync(MigrationMonitoringRecord record, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<MigrationMonitoringRecord>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(records);
    }

    private sealed class RecordingObserver : IMigrationPathObserver
    {
        public List<string> Paths { get; } = [];

        public Task<MigrationPathObservation> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            Paths.Add(path);
            return Task.FromResult(new MigrationPathObservation
            {
                Path = path,
                Exists = true,
                IsDirectory = true,
                IsRedirect = true,
                RedirectTarget = @"D:\target"
            });
        }
    }
}
