using Css.Scanner.Software;
using FluentAssertions;

namespace Css.Tests;

public class SoftwareInventoryFixtureScannerTests
{
    [Fact]
    public async Task Fixture_scanner_returns_scripted_scan_sequence_and_repeats_last_scan()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-software-fixture-" + Guid.NewGuid().ToString("N"));
        var fixturePath = Path.Combine(root, "software-fixture.json");

        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(
                fixturePath,
                """
                {
                  "scans": [
                    [
                      {
                        "name": "Fixture App",
                        "publisher": "Fixture Inc.",
                        "installPath": "D:\\Software\\Fixture\\Install",
                        "uninstallCommand": "\"D:\\Software\\Fixture\\Install\\uninstall.exe\"",
                        "cachePaths": [ "C:\\Users\\Me\\AppData\\Local\\Fixture\\Cache" ],
                        "logPaths": [ "C:\\Users\\Me\\AppData\\Local\\Fixture\\Logs" ]
                      }
                    ],
                    []
                  ]
                }
                """);

            var scanner = SoftwareInventoryFixtureScanner.TryCreate(fixturePath);

            scanner.Should().NotBeNull();
            var first = await scanner!.ScanAsync();
            var second = await scanner.ScanAsync();
            var third = await scanner.ScanAsync();

            first.Should().ContainSingle(profile => profile.Name == "Fixture App");
            first[0].CachePaths.Should().Contain(@"C:\Users\Me\AppData\Local\Fixture\Cache");
            first[0].LogPaths.Should().Contain(@"C:\Users\Me\AppData\Local\Fixture\Logs");
            second.Should().BeEmpty();
            third.Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Fixture_scanner_is_disabled_when_no_fixture_path_is_set()
    {
        SoftwareInventoryFixtureScanner.TryCreate(null).Should().BeNull();
        SoftwareInventoryFixtureScanner.TryCreate("   ").Should().BeNull();
    }
}
