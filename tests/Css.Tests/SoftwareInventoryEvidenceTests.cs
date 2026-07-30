using Css.Scanner.Software;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class SoftwareInventoryEvidenceTests
{
    [Fact]
    public void Growth_enrichment_preserves_exact_entry_and_C_drive_size_evidence()
    {
        var source = new Css.Core.Software.SoftwareProfile
        {
            Name = "OpenCode 1.14.41",
            DisplayVersion = "1.14.41",
            InventorySource = @"HKLM\Software\Example",
            CDriveDataSizeBytes = 248512512
        };

        var enriched = SoftwareGrowthProfileEnricher.Apply([source], []).Single();

        enriched.DisplayVersion.Should().Be(source.DisplayVersion);
        enriched.InventorySource.Should().Be(source.InventorySource);
        enriched.CDriveDataSizeBytes.Should().Be(source.CDriveDataSizeBytes);
    }

    [Fact]
    public async Task Fixture_scanner_preserves_new_exact_entry_evidence()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "omnix-software-fixture-" + Guid.NewGuid().ToString("N") + ".json");
        await File.WriteAllTextAsync(
            path,
            """
            {
              "scans": [[{
                "name": "OpenCode 1.14.41",
                "displayVersion": "1.14.41",
                "inventorySource": "HKLM\\Software\\Example",
                "cDriveDataSizeBytes": 248512512
              }]]
            }
            """);

        try
        {
            var scanner = SoftwareInventoryFixtureScanner.TryCreate(path);
            scanner.Should().NotBeNull();

            var profile = (await scanner!.ScanAsync()).Single();

            profile.DisplayVersion.Should().Be("1.14.41");
            profile.InventorySource.Should().Be(@"HKLM\Software\Example");
            profile.CDriveDataSizeBytes.Should().Be(248512512);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Registry_version_source_and_bounded_data_size_reach_the_profile()
    {
        const string dataRoot = @"C:\Users\Me\AppData\Local";
        var appRoot = Path.Combine(dataRoot, "Example App");
        var cacheRoot = Path.Combine(appRoot, "Cache");
        var existingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            appRoot,
            cacheRoot
        };
        var record = new InstalledSoftwareRecord(
            DisplayName: "Example App",
            Publisher: "Example Inc.",
            InstallLocation: @"D:\Software\Example App\Install",
            UninstallCommand: @"D:\Software\Example App\Install\uninstall.exe",
            DisplayIcon: null,
            RegistryKeyPath: @"HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\Example",
            DisplayVersion: "2.7.1");

        var profile = SoftwareInventoryBuilder.Build(
            [record],
            [],
            [],
            [],
            userDataRoots: [dataRoot],
            pathExists: existingPaths.Contains,
            cacheSizeResolver: path =>
                path.Equals(appRoot, StringComparison.OrdinalIgnoreCase)
                    ? 400L * 1024 * 1024
                    : path.Equals(cacheRoot, StringComparison.OrdinalIgnoreCase)
                        ? 100L * 1024 * 1024
                        : 0)
            .Single();

        profile.DisplayVersion.Should().Be("2.7.1");
        profile.InventorySource.Should().Be(record.RegistryKeyPath);
        profile.DataSizeBytes.Should().Be(400L * 1024 * 1024);
        profile.CDriveDataSizeBytes.Should().Be(400L * 1024 * 1024);
        profile.CacheSizeBytes.Should().Be(100L * 1024 * 1024);
    }
}
