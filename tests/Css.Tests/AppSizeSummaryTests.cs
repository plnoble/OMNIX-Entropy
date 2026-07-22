using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public class AppSizeSummaryTests
{
    [Fact]
    public void Known_sizes_show_program_data_identifiable_cache_and_growth()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = @"D:\Software\Marvis",
            InstalledSizeBytes = 600L * 1024 * 1024,
            DataSizeBytes = 5L * 1024 * 1024 * 1024,
            CacheSizeBytes = 320L * 1024 * 1024,
            RecentGrowthBytes = 64L * 1024 * 1024,
            DataPaths = [@"C:\Users\Fixture\AppData\Local\Marvis"],
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Marvis\Cache"]
        });

        drawer.SizeSummary.Should().Contain("主程序安装 600.0 MB")
            .And.Contain("数据 5.0 GB")
            .And.Contain("可识别缓存 320.0 MB")
            .And.Contain("最近增长 64.0 MB")
            .And.NotContain("可释放")
            .And.NotContain(@"C:\")
            .And.NotContain(@"D:\");
    }

    [Fact]
    public void Default_zero_values_are_not_presented_as_measured_zero_bytes()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Unknown App",
            InstallPath = @"D:\Software\Unknown"
        });

        drawer.SizeSummary.Should().Contain("主程序大小未统计")
            .And.Contain("未识别单独数据位置")
            .And.Contain("未识别缓存位置")
            .And.Contain("最近增长暂无可用数值")
            .And.NotContain("0 B")
            .And.NotContain("可释放");
    }

    [Fact]
    public void Identified_locations_with_no_size_are_distinct_from_unidentified_locations()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Observed App",
            DataPaths = [@"C:\Users\Fixture\AppData\Roaming\Observed"],
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Observed\Cache"]
        });

        drawer.SizeSummary.Should().Contain("数据位置已识别，大小未统计")
            .And.Contain("缓存位置已识别，大小未统计")
            .And.NotContain("未识别单独数据位置")
            .And.NotContain("未识别缓存位置")
            .And.NotContain(@"C:\Users\Fixture");
    }
}
