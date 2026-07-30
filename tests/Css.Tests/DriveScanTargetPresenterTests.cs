using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class DriveScanTargetPresenterTests
{
    private const long GiB = 1024L * 1024 * 1024;

    [Fact]
    public void Ready_fixed_drives_are_presented_with_system_drive_first()
    {
        LocalDriveScanObservation[] observations =
        [
            new(@"D:\", true, DriveType.Fixed, 500 * GiB, 120 * GiB),
            new(@"C:\", true, DriveType.Fixed, 250 * GiB, 80 * GiB),
            new(@"E:\", true, DriveType.Removable, 64 * GiB, 40 * GiB),
            new(@"F:\", false, DriveType.Fixed, 0, 0),
            new(@"Z:\", true, DriveType.Network, 1000 * GiB, 900 * GiB)
        ];

        var targets = DriveScanTargetPresenter.Create(observations, @"C:\");

        targets.Select(target => target.Root).Should().Equal(@"C:\", @"D:\");
        targets[0].IsSystemDrive.Should().BeTrue();
        targets[0].DisplayName.Should().Be("系统盘 C 盘");
        targets[1].DisplayName.Should().Be("D 盘");
        targets[1].SpaceSummary.Should().Contain("剩余 120 GB");
        targets.Should().OnlyContain(target =>
            !string.IsNullOrWhiteSpace(target.AccessibilityName));
    }

    [Fact]
    public void Missing_drive_inventory_falls_back_to_the_system_drive_without_path_entry()
    {
        var targets = DriveScanTargetPresenter.Create([], @"C:\");

        targets.Should().ContainSingle();
        targets[0].Root.Should().Be(@"C:\");
        targets[0].DisplayName.Should().Be("系统盘 C 盘");
        targets[0].SpaceSummary.Should().Be("容量暂时不可用");
    }
}
