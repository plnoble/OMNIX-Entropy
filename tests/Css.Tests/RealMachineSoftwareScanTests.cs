using Css.Core.Software;
using Css.Scanner.Software;
using FluentAssertions;

namespace Css.Tests;

public class RealMachineSoftwareScanTests
{
    [Fact]
    public async Task Real_machine_scan_identifies_marvis_when_enabled()
    {
        if (Environment.GetEnvironmentVariable("OMNIX_REAL_MACHINE_TESTS") != "1")
            return;

        var scanner = new SoftwareInventoryScanner();
        var profiles = await scanner.ScanAsync();
        var marvis = profiles.FirstOrDefault(p => p.Name.Equals("Marvis", StringComparison.OrdinalIgnoreCase));

        marvis.Should().NotBeNull("Marvis is installed on this machine for read-only validation");
        marvis!.Category.Should().Be(SoftwareCategory.Ai);
        marvis.InstallPath.Should().NotBeNull();
        marvis.InstallPath!.StartsWith(@"D:\Software\Marvis", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        marvis.Services.Should().Contain("MarvisSvr");
        marvis.RunningProcesses.Should().Contain(p => p.Contains("Marvis", StringComparison.OrdinalIgnoreCase));
        marvis.InstalledSizeBytes.Should().BeGreaterThan(500L * 1024 * 1024);
    }
}
