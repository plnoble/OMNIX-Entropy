using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppCatalogSummaryTests
{
    [Fact]
    public void Summary_and_c_drive_filter_share_one_deduplicated_footprint_policy()
    {
        var cMainOnly = Profile(
            "C Main Only",
            @"C:\Program Files\C Main Only",
            [@"C:\Program Files\C Main Only"]);
        var dMainWithCData = Profile(
            "D Main With C Data",
            @"D:\Software\D Main\Install",
            [
                @"C:\Users\Fixture\AppData\Local\D Main\Cache",
                @"C:\Users\Fixture\AppData\Local\D Main\Cache",
                @"C:\Users\Fixture\AppData\Local\D Main\Cache\Nested"
            ]);
        var cMainWithCData = Profile(
            "C Main With C Data",
            @"C:\Program Files\C Main With Data",
            [
                @"C:\Program Files\C Main With Data",
                @"C:\Users\Fixture\AppData\Local\C Main With Data"
            ]);
        var unknownMainWithCData = Profile(
            "Unknown Main With C Data",
            null,
            [@"C:\Users\Fixture\AppData\Local\Unknown\Cache"]);
        var dClean = Profile("D Clean", @"D:\Software\Clean\Install", []);
        var misleadingNonCClue = Profile(
            "D Path In C Field",
            @"D:\Software\Misleading\Install",
            [@"D:\Software\Misleading\Data"]);
        var profiles = new[]
        {
            cMainOnly,
            dMainWithCData,
            cMainWithCData,
            unknownMainWithCData,
            dClean,
            misleadingNonCClue
        };

        var summary = AppCatalogSummaryPresenter.Create(profiles, visibleCount: 2);
        var filtered = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.CDrive,
                Sort = AppCatalogSort.Name
            });

        summary.TotalCount.Should().Be(6);
        summary.VisibleCount.Should().Be(2);
        summary.MainProgramOnCCount.Should().Be(2);
        summary.MainProgramOnDCount.Should().Be(3);
        summary.CDriveDataAppCount.Should().Be(3);
        summary.CDriveFootprintCount.Should().Be(4);
        filtered.Should().HaveCount(summary.CDriveFootprintCount);
        filtered.Should().Contain([cMainOnly, dMainWithCData, cMainWithCData, unknownMainWithCData]);
        filtered.Should().NotContain([dClean, misleadingNonCClue]);
        AppPresentationBuilder.CDriveDataLocationCount(cMainOnly).Should().Be(0);
        AppPresentationBuilder.CDriveDataLocationCount(cMainWithCData).Should().Be(1);
    }

    [Fact]
    public void Beginner_summary_explains_main_program_and_data_without_paths_or_cleanup_claims()
    {
        var profiles = new[]
        {
            Profile("C Main", @"C:\Program Files\C Main", [@"C:\Program Files\C Main"]),
            Profile("D Data", @"D:\Software\D Data\Install", [@"C:\Users\Fixture\D Data\Cache"]),
            Profile("D Clean", @"D:\Software\D Clean\Install", [])
        };

        var summary = AppCatalogSummaryPresenter.Create(profiles, visibleCount: 2);

        summary.Text.Should().Contain("扫描到 3 个应用")
            .And.Contain("主程序在 C 盘 1 个")
            .And.Contain("D 盘 2 个")
            .And.Contain("C 盘数据或缓存线索 1 个")
            .And.Contain("“占 C 盘”共 2 个")
            .And.Contain("当前显示 2 个")
            .And.NotContain(@"C:\")
            .And.NotContain(@"D:\")
            .And.NotContain("可释放")
            .And.NotContain("可清理");
    }

    [Fact]
    public void Main_window_uses_the_tested_summary_instead_of_a_private_counter()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var start = code.IndexOf("private void RefreshAppCatalog", StringComparison.Ordinal);
        var end = code.IndexOf("private string BuildMigrationClosureCatalogSummary", start, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        end.Should().BeGreaterThan(start);
        var refreshMethod = code[start..end];

        refreshMethod.Should().Contain("AppCatalogSummaryPresenter.Create(_softwareProfiles, filtered.Count).Text")
            .And.NotContain("BuildSoftwareSummary")
            .And.NotContain("CDriveWritePaths.Count > 0");
        code.Should().NotContain("private static string BuildSoftwareSummary");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AppsSummaryTextBlock\"");
        xaml.IndexOf("AppsSummaryTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("AppTilesListBox", StringComparison.Ordinal));
    }

    private static SoftwareProfile Profile(
        string name,
        string? installPath,
        IReadOnlyList<string> cDriveWritePaths) =>
        new()
        {
            Name = name,
            InstallPath = installPath,
            CDriveWritePaths = cDriveWritePaths
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

        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(segments));
    }
}
