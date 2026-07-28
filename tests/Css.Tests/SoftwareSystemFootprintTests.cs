using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Scanner.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class SoftwareSystemFootprintTests
{
    [Fact]
    public void Inventory_correlates_only_footprints_with_application_evidence()
    {
        var profiles = SoftwareInventoryBuilder.Build(
            [
                new InstalledSoftwareRecord(
                    "Example App",
                    "Example Publisher",
                    @"D:\Software\Example App\Install",
                    null,
                    null,
                    @"HKCU\Software\Uninstall\Example")
            ],
            [],
            [],
            [],
            systemFootprints:
            [
                new SoftwareSystemFootprintEntry(
                    SoftwareSystemFootprintKind.ContextMenu,
                    "Upload with Example",
                    @"HKCU64\Software\Classes\*\shell\Example",
                    @"Example D:\Software\Example App\Install\shell.dll"),
                new SoftwareSystemFootprintEntry(
                    SoftwareSystemFootprintKind.BrowserIntegration,
                    "com.other.host",
                    @"HKCU64\Software\Google\Chrome\NativeMessagingHosts\com.other.host",
                    @"C:\Other\host.json")
            ]);

        var profile = profiles.Should().ContainSingle().Subject;
        profile.SystemFootprints.Should().ContainSingle();
        profile.SystemFootprints[0].Kind.Should().Be(SoftwareSystemFootprintKind.ContextMenu);
        profile.SystemFootprints[0].DisplayName.Should().Be("Upload with Example");
    }

    [Fact]
    public void Drawer_explains_system_footprints_without_exposing_technical_locations()
    {
        var profile = ProfileWithFootprints();

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.SystemFootprintSummary.Should()
            .Contain("右键菜单 1 处")
            .And.Contain("浏览器连接 1 处")
            .And.Contain("不等于病毒")
            .And.NotContain("HKCU")
            .And.NotContain("NativeMessagingHosts");
        drawer.AgentAdvice.Text.Should().Contain("先确认这些入口是否有用");
        drawer.AgentAdvice.Reason.Should().Contain("入口存在不等于恶意");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Observe);
        drawer.AgentAdvice.RequiresUserConfirmation.Should().BeFalse();
        drawer.TechnicalDetailsHiddenByDefault.Should().BeTrue();
        drawer.TechnicalDetails.Should().Contain(line =>
            line.Contains("NativeMessagingHosts", StringComparison.Ordinal));
    }

    [Fact]
    public void No_match_is_described_as_this_scan_result_instead_of_a_global_clean_claim()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Plain App",
            Category = SoftwareCategory.Normal
        });

        drawer.SystemFootprintSummary.Should()
            .StartWith("本次扫描")
            .And.NotContain("绝对")
            .And.NotContain("完全没有");
    }

    [Fact]
    public void Application_drawer_places_beginner_summary_before_agent_advice()
    {
        var xaml = File.ReadAllText(
            FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var main = File.ReadAllText(
            FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerSystemFootprintTextBlock\"");
        xaml.IndexOf("DrawerResidencyTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("DrawerSystemFootprintTextBlock", StringComparison.Ordinal));
        xaml.IndexOf("DrawerSystemFootprintTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("DrawerAdviceTextBlock", StringComparison.Ordinal));
        main.Should().Contain("DrawerSystemFootprintTextBlock.Text = drawer.SystemFootprintSummary;");
        main.Should().Contain("DrawerSystemFootprintTextBlock.Text = empty.SystemFootprintSummary;");
    }

    [Fact]
    public void Windows_footprint_scanner_is_bounded_and_read_only()
    {
        var scanner = File.ReadAllText(
            FindRepositoryFile(
                "src",
                "Css.Scanner",
                "Software",
                "WindowsSoftwareSystemFootprintScanner.cs"));
        var inventory = File.ReadAllText(
            FindRepositoryFile(
                "src",
                "Css.Scanner",
                "Software",
                "SoftwareInventoryScanner.cs"));

        scanner.Should()
            .Contain(@"Software\Classes\Directory\Background\shell")
            .And.Contain(@"Software\Google\Chrome\NativeMessagingHosts")
            .And.Contain(@"Software\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace")
            .And.Contain("CommonFileExtensions")
            .And.NotContain("DeleteSubKey")
            .And.NotContain(".SetValue(")
            .And.NotContain("Process.Start")
            .And.NotContain("\"sc.exe\"")
            .And.NotContain("\"schtasks.exe\"");
        inventory.Should().Contain("new WindowsSoftwareSystemFootprintScanner().Scan()");
    }

    [Fact]
    public void Gui_smoke_uses_an_isolated_fixture_and_proves_the_visible_conclusion()
    {
        var smoke = File.ReadAllText(
            FindRepositoryFile(".omx", "gui-app-system-footprint-smoke.ps1"));

        smoke.Should()
            .Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE")
            .And.Contain("DrawerSystemFootprintTextBlock")
            .And.Contain("DrawerAdviceTextBlock")
            .And.Contain("Save-WindowScreenshot")
            .And.Contain("ReadOnly = $true")
            .And.NotContain("Set-ItemProperty")
            .And.NotContain("New-Service")
            .And.NotContain("schtasks.exe");
    }

    private static SoftwareProfile ProfileWithFootprints() =>
        new()
        {
            Name = "Example App",
            Category = SoftwareCategory.Normal,
            SystemFootprints =
            [
                new SoftwareSystemFootprintObservation
                {
                    Kind = SoftwareSystemFootprintKind.ContextMenu,
                    DisplayName = "Upload with Example",
                    SourceLocator = @"HKCU64\Software\Classes\*\shell\Example",
                    Evidence = @"D:\Software\Example\shell.dll"
                },
                new SoftwareSystemFootprintObservation
                {
                    Kind = SoftwareSystemFootprintKind.BrowserIntegration,
                    DisplayName = "com.example.host",
                    SourceLocator = @"HKCU64\Software\Google\Chrome\NativeMessagingHosts\com.example.host",
                    Evidence = @"D:\Software\Example\host.json"
                }
            ]
        };

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException(
            "Unable to locate repository file.",
            Path.Combine(segments));
    }
}
