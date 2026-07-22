using Css.Core.Software;
using Css.Elevated.Uninstall;
using Css.Snapshot.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class InventoryOfficialUninstallPostScannerTests
{
    [Fact]
    public async Task Post_scanner_reports_only_reverified_paths_and_separates_background_hints()
    {
        var manifest = Manifest();
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            manifest.CachePaths[0],
            manifest.LogPaths[0]
        };
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ => Task.FromResult<IReadOnlyList<SoftwareProfile>>([]),
            existing.Contains,
            path => path == manifest.CachePaths[0] ? 512 : 128);

        var result = await scanner.ScanAsync(manifest.SoftwareName, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SoftwareStillPresent.Should().BeFalse();
        result.ResidueCandidateCount.Should().Be(2);
        result.UnverifiedBackgroundHintCount.Should().Be(3);
        result.RequiresBackgroundRescan.Should().BeTrue();
        result.ResidueReport.Should().NotBeNull();
        result.ResidueReport!.Groups.SelectMany(group => group.Candidates)
            .Should().OnlyContain(candidate => candidate.Path != null);
        result.ResidueReport.Groups.Should().NotContain(group => group.Risk == Css.Core.Operations.RiskLevel.High);
        result.Summary.Should().NotContain(@"C:\");
        existing.Should().BeEquivalentTo(manifest.CachePaths[0], manifest.LogPaths[0]);
    }

    [Fact]
    public async Task Post_scanner_reports_software_still_present_from_fresh_inventory()
    {
        var manifest = Manifest();
        var after = new SoftwareProfile
        {
            Name = manifest.SoftwareName,
            InstallPath = manifest.InstallPath
        };
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ => Task.FromResult<IReadOnlyList<SoftwareProfile>>([after]),
            _ => true);

        var result = await scanner.ScanAsync(manifest.SoftwareName, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.SoftwareStillPresent.Should().BeTrue();
        result.ResidueCandidateCount.Should().Be(0);
        result.ResidueReport!.OfficialUninstallAppearsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task Post_scanner_reports_inventory_failure_instead_of_claiming_clean_uninstall()
    {
        var manifest = Manifest();
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ => throw new InvalidOperationException("inventory failed"),
            _ => false);

        var result = await scanner.ScanAsync(manifest.SoftwareName, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Summary.Should().Contain("inventory failed");
        result.ResidueReport.Should().BeNull();
    }

    [Fact]
    public async Task Post_scanner_propagates_cancellation()
    {
        var manifest = Manifest();
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ => throw new OperationCanceledException(),
            _ => false);

        var action = () => scanner.ScanAsync(manifest.SoftwareName, CancellationToken.None);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Post_scanner_refuses_a_software_name_that_does_not_match_the_manifest()
    {
        var manifest = Manifest();
        var inventoryCalled = false;
        var scanner = new InventoryOfficialUninstallPostScanner(
            manifest,
            _ =>
            {
                inventoryCalled = true;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>([]);
            },
            _ => false);

        var result = await scanner.ScanAsync("Another App", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ResidueReport.Should().BeNull();
        inventoryCalled.Should().BeFalse();
    }

    [Fact]
    public void Post_scanner_source_is_read_only_and_registered_only_in_elevated_production_mode()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall", "InventoryOfficialUninstallPostScanner.cs"));
        var worker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "OfficialUninstallProductionWorker.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        source.Should().Contain("UninstallResidueScanBuilder.Build");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        source.Should().NotContain("Quarantine");
        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("Process.Start");
        worker.Should().Contain("InventoryOfficialUninstallPostScanner");
        worker.Should().Contain("WindowsOfficialUninstallProductionPackageAuthorizer");
        app.Should().NotContain("InventoryOfficialUninstallPostScanner");
    }

    private static UninstallEvidenceSnapshotManifest Manifest() =>
        new()
        {
            SnapshotId = "snapshot-post-scan",
            CreatedAtUtc = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero),
            SoftwareName = "Example App",
            Publisher = "Example Inc.",
            InstallPath = @"D:\Software\Example\Install",
            UninstallCommand = @"""D:\Software\Example\Install\Uninstall.exe"" /remove",
            DataPaths = [@"C:\Users\Me\AppData\Local\Example\Data"],
            CachePaths = [@"C:\Users\Me\AppData\Local\Example\Cache"],
            LogPaths = [@"C:\Users\Me\AppData\Local\Example\Logs"],
            StartupEntries = ["Example Startup"],
            Services = ["ExampleService"],
            ScheduledTasks = ["Example Task"],
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
}
