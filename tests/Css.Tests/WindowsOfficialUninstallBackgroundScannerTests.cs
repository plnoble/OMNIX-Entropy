using Css.Core.Operations;
using Css.Elevated.Uninstall;
using Css.Snapshot.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class WindowsOfficialUninstallBackgroundScannerTests
{
    [Fact]
    public async Task Scanner_rechecks_exact_manifest_entries_and_keeps_unknown_separate()
    {
        var manifest = Manifest(
            startupEntries: ["Example Tray", "example tray"],
            services: ["ExampleService"],
            scheduledTasks: [@"\Example Update"]);
        var reader = new FakeBackgroundEntryReader
        {
            StartupStates = { ["Example Tray"] = WindowsBackgroundEntryState.Exists },
            ServiceStates = { ["ExampleService"] = WindowsBackgroundEntryState.Missing },
            TaskStates = { [@"\Example Update"] = WindowsBackgroundEntryState.Unknown }
        };
        var scanner = new WindowsOfficialUninstallBackgroundScanner(manifest, reader);

        var result = await scanner.ScanAsync(CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ExistingStartupEntries.Should().Equal("Example Tray");
        result.ExistingServices.Should().BeEmpty();
        result.ExistingScheduledTasks.Should().BeEmpty();
        result.VerifiedResidueCount.Should().Be(1);
        result.UnverifiedHintCount.Should().Be(1);
        reader.StartupProbeCount.Should().Be(1);
    }

    [Fact]
    public async Task Scanner_is_complete_when_every_manifest_entry_was_confirmed_missing_or_present()
    {
        var manifest = Manifest(
            startupEntries: ["Example Tray"],
            services: ["ExampleService"],
            scheduledTasks: [@"\Example Update"]);
        var reader = new FakeBackgroundEntryReader
        {
            StartupStates = { ["Example Tray"] = WindowsBackgroundEntryState.Missing },
            ServiceStates = { ["ExampleService"] = WindowsBackgroundEntryState.Exists },
            TaskStates = { [@"\Example Update"] = WindowsBackgroundEntryState.Missing }
        };
        var scanner = new WindowsOfficialUninstallBackgroundScanner(manifest, reader);

        var result = await scanner.ScanAsync(CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ExistingServices.Should().Equal("ExampleService");
        result.VerifiedResidueCount.Should().Be(1);
        result.UnverifiedHintCount.Should().Be(0);
    }

    [Fact]
    public async Task Scanner_propagates_cancellation_before_probing_windows_state()
    {
        var reader = new FakeBackgroundEntryReader();
        var scanner = new WindowsOfficialUninstallBackgroundScanner(
            Manifest(startupEntries: ["Example Tray"]),
            reader);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var action = () => scanner.ScanAsync(cancellation.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        reader.StartupProbeCount.Should().Be(0);
    }

    [Fact]
    public async Task Post_scanner_adds_only_freshly_verified_background_residue_as_high_risk()
    {
        var manifest = Manifest(
            startupEntries: ["Example Tray"],
            services: ["ExampleService"],
            scheduledTasks: [@"\Example Update"]);
        var background = new StubBackgroundScanner(new OfficialUninstallBackgroundScanResult
        {
            Success = true,
            ExistingServices = ["ExampleService"]
        });
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ => Task.FromResult<IReadOnlyList<Css.Core.Software.SoftwareProfile>>([]),
            _ => false,
            backgroundScanner: background);

        var result = await scanner.ScanAsync(manifest.SoftwareName, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ResidueCandidateCount.Should().Be(1);
        result.PathResidueCandidateCount.Should().Be(0);
        result.VerifiedBackgroundResidueCount.Should().Be(1);
        result.UnverifiedBackgroundHintCount.Should().Be(0);
        result.RequiresBackgroundRescan.Should().BeFalse();
        result.ResidueReport!.Groups.Should().ContainSingle(group =>
            group.Risk == RiskLevel.High
            && group.Candidates.Single().Identifier == "ExampleService");
    }

    [Fact]
    public async Task Post_scanner_refuses_success_when_background_recheck_is_partial()
    {
        var manifest = Manifest(services: ["ExampleService"]);
        var background = new StubBackgroundScanner(new OfficialUninstallBackgroundScanResult
        {
            Success = false,
            UnverifiedHintCount = 1
        });
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ => Task.FromResult<IReadOnlyList<Css.Core.Software.SoftwareProfile>>([]),
            _ => false,
            backgroundScanner: background);

        var result = await scanner.ScanAsync(manifest.SoftwareName, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.RequiresBackgroundRescan.Should().BeTrue();
        result.UnverifiedBackgroundHintCount.Should().Be(1);
        result.ResidueReport.Should().BeNull();
    }

    [Fact]
    public void System_reader_rejects_crafted_identifiers_and_is_elevated_production_read_only()
    {
        var reader = new SystemWindowsBackgroundEntryReader();

        reader.ProbeService(@"folder\service").Should().Be(WindowsBackgroundEntryState.Unknown);
        reader.ProbeScheduledTask(@"\..\outside").Should().Be(WindowsBackgroundEntryState.Unknown);

        var scannerSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall", "WindowsOfficialUninstallBackgroundScanner.cs"));
        var readerSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall", "SystemWindowsBackgroundEntryReader.cs"));
        var worker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "OfficialUninstallProductionWorker.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        var combined = scannerSource + readerSource;
        combined.Should().NotContain("SetValue");
        combined.Should().NotContain("DeleteSubKey");
        combined.Should().NotContain("File.Delete");
        combined.Should().NotContain("File.Move");
        combined.Should().NotContain("Process.Start");
        combined.Should().NotContain("Start-Service");
        combined.Should().NotContain("Stop-Service");
        worker.Should().Contain("WindowsOfficialUninstallBackgroundScanner");
        worker.Should().Contain("WindowsOfficialUninstallProductionPackageAuthorizer");
        app.Should().NotContain("WindowsOfficialUninstallBackgroundScanner");
    }

    private static UninstallEvidenceSnapshotManifest Manifest(
        IReadOnlyList<string>? startupEntries = null,
        IReadOnlyList<string>? services = null,
        IReadOnlyList<string>? scheduledTasks = null) =>
        new()
        {
            SnapshotId = "background-snapshot",
            CreatedAtUtc = new DateTimeOffset(2026, 7, 10, 22, 0, 0, TimeSpan.Zero),
            SoftwareName = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe""",
            StartupEntries = startupEntries ?? [],
            Services = services ?? [],
            ScheduledTasks = scheduledTasks ?? [],
            RecoveryMethod = "ReinstallSource",
            RecoveryReference = "verified-installer",
            UserDataBackupConfirmed = true
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

    private sealed class FakeBackgroundEntryReader : IWindowsBackgroundEntryReader
    {
        public Dictionary<string, WindowsBackgroundEntryState> StartupStates { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, WindowsBackgroundEntryState> ServiceStates { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, WindowsBackgroundEntryState> TaskStates { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public int StartupProbeCount { get; private set; }

        public WindowsBackgroundEntryState ProbeStartupEntry(string name)
        {
            StartupProbeCount++;
            return StartupStates.GetValueOrDefault(name, WindowsBackgroundEntryState.Unknown);
        }

        public WindowsBackgroundEntryState ProbeService(string name) =>
            ServiceStates.GetValueOrDefault(name, WindowsBackgroundEntryState.Unknown);

        public WindowsBackgroundEntryState ProbeScheduledTask(string name) =>
            TaskStates.GetValueOrDefault(name, WindowsBackgroundEntryState.Unknown);
    }

    private sealed class StubBackgroundScanner : IOfficialUninstallBackgroundScanner
    {
        private readonly OfficialUninstallBackgroundScanResult _result;

        public StubBackgroundScanner(OfficialUninstallBackgroundScanResult result) => _result = result;

        public Task<OfficialUninstallBackgroundScanResult> ScanAsync(CancellationToken cancellationToken) =>
            Task.FromResult(_result);
    }
}
