using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public class AppTileStorageReasonTests
{
    [Fact]
    public void C_drive_main_program_uses_an_explicit_grid_tag()
    {
        var tile = AppPresentationBuilder.CreateTile(new SoftwareProfile
        {
            Name = "C App",
            InstallPath = @"C:\Program Files\C App",
            CDriveWritePaths = [@"C:\Users\Fixture\AppData\Local\C App"]
        });

        tile.Status.Should().Be(AppTileStatus.Attention);
        tile.ShortTag.Should().Be("主程序在 C 盘");
        tile.AccessibilityName.Should().Be("C App, 主程序在 C 盘");
        tile.VisibleText.Should().NotContain(@"C:\");
    }

    [Fact]
    public void D_drive_main_program_with_c_drive_writes_uses_a_data_tag()
    {
        var tile = AppPresentationBuilder.CreateTile(new SoftwareProfile
        {
            Name = "D App",
            InstallPath = @"D:\Software\D App",
            CDriveWritePaths = [@"C:\Users\Fixture\AppData\Local\D App"]
        });

        tile.Status.Should().Be(AppTileStatus.Attention);
        tile.ShortTag.Should().Be("数据写入 C 盘");
        tile.AccessibilityName.Should().Be("D App, 数据写入 C 盘");
        tile.VisibleText.Should().NotContain(@"C:\").And.NotContain(@"D:\");
    }

    [Fact]
    public void Unknown_main_program_with_c_drive_writes_keeps_ownership_uncertain()
    {
        var tile = AppPresentationBuilder.CreateTile(new SoftwareProfile
        {
            Name = "Mystery App",
            CDriveWritePaths = [@"C:\Users\Fixture\AppData\Local\Mystery"]
        });

        tile.Status.Should().Be(AppTileStatus.Attention);
        tile.ShortTag.Should().Be("C 盘线索待确认");
        tile.AccessibilityName.Should().Be("Mystery App, C 盘线索待确认");
    }

    [Fact]
    public void Existing_non_storage_priority_tags_remain_unchanged()
    {
        var growth = AppPresentationBuilder.CreateTile(new SoftwareProfile
        {
            Name = "Growing App",
            InstallPath = @"D:\Software\Growing",
            RecentGrowthBytes = 128
        });
        var resident = AppPresentationBuilder.CreateTile(new SoftwareProfile
        {
            Name = "Resident App",
            InstallPath = @"D:\Software\Resident",
            RunningProcesses = ["Resident"]
        });
        var system = AppPresentationBuilder.CreateTile(new SoftwareProfile
        {
            Name = "System App",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"C:\Windows\System32"
        });

        growth.ShortTag.Should().Be("最近变大");
        resident.ShortTag.Should().Be("后台常驻");
        system.ShortTag.Should().Be("系统组件");
    }
}
