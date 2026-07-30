using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppFamilyDecisionTests
{
    [Fact]
    public void Same_family_entries_are_explained_but_keep_exact_uninstall_identity()
    {
        var registered = Profile(
            "OpenCode 1.14.41",
            @"C:\Program Files\OpenCode",
            uninstallCommand: @"""C:\Program Files\OpenCode\Uninstall OpenCode.exe"" /allusers",
            version: "1.14.41",
            installedBytes: 447L * 1024 * 1024);
        var portable = Profile(
            "OpenCode",
            @"D:\Development\opencode",
            uninstallCommand: null,
            version: "1.4.3",
            installedBytes: 221L * 1024 * 1024);
        var dataClue = Profile(
            "OpenCode 1.18.4",
            null,
            uninstallCommand: null,
            version: "1.18.4",
            cDriveDataBytes: 237L * 1024 * 1024,
            cDrivePaths: [@"C:\Users\Me\AppData\Local\opencode-updater"]);
        var family = new[] { registered, portable, dataClue };

        var registeredDrawer = AppPresentationBuilder.CreateDrawer(registered, family);
        var portableDrawer = AppPresentationBuilder.CreateDrawer(portable, family);
        var tile = AppPresentationBuilder.CreateTile(registered, family);

        registeredDrawer.FamilySummary.Should().Contain("3 条")
            .And.Contain("不会合并卸载");
        registeredDrawer.CurrentEntrySummary.Should().Contain("1.14.41")
            .And.Contain("官方卸载入口");
        registeredDrawer.UninstallActionLabel.Should().Be("卸载这个版本");
        registeredDrawer.AvailableActions.Single(action => action.Kind == AppActionKind.Uninstall)
            .IsEnabled.Should().BeTrue();

        portableDrawer.CurrentEntrySummary.Should().Contain("D 盘")
            .And.Contain("没有官方卸载入口");
        portableDrawer.StorageOutcomeSummary.Should()
            .Contain("程序或副本已经在 D 盘")
            .And.Contain("C 盘程序或更新载荷")
            .And.NotContain("主程序已经在 D 盘");
        portableDrawer.UninstallActionLabel.Should().Be("此条不可卸载");
        portableDrawer.AvailableActions.Single(action => action.Kind == AppActionKind.Uninstall)
            .IsEnabled.Should().BeFalse();
        tile.ShortTag.Should().Contain("同类 3 条");
    }

    [Fact]
    public void Moving_a_D_drive_main_program_does_not_claim_C_data_will_follow()
    {
        var mainProgram = Profile(
            "Antigravity 2.4.3",
            @"D:\Agent\Google AntiGravity",
            uninstallCommand: @"D:\Agent\Google AntiGravity\uninstall.exe",
            version: "2.4.3",
            installedBytes: 1214L * 1024 * 1024);
        var userData = Profile(
            "Antigravity (User)",
            null,
            uninstallCommand: null,
            cDriveDataBytes: 352L * 1024 * 1024,
            cDrivePaths:
            [
                @"C:\Users\Me\AppData\Local\antigravity-updater",
                @"C:\Users\Me\AppData\Roaming\Antigravity"
            ]);

        var drawer = AppPresentationBuilder.CreateDrawer(
            mainProgram,
            [mainProgram, userData]);

        drawer.StorageOutcomeSummary.Should().Contain("主程序已经在 D 盘")
            .And.Contain("至少")
            .And.Contain("352.0 MB")
            .And.Contain("仍可能增长");
        drawer.MigrationSummary.Should().Contain("不需要迁移");
    }

    [Fact]
    public void Moving_one_C_drive_version_does_not_hide_another_C_drive_family_program()
    {
        var registered = Profile(
            "OpenCode 1.14.41",
            @"C:\Program Files\OpenCode",
            uninstallCommand: @"""C:\Program Files\OpenCode\Uninstall OpenCode.exe"" /allusers",
            version: "1.14.41",
            installedBytes: 447L * 1024 * 1024);
        var updaterPayload = Profile(
            "OpenCode 1.18.4",
            @"C:\Users\Me\AppData\Local\opencode-updater",
            uninstallCommand: null,
            version: "1.18.4",
            installedBytes: 237L * 1024 * 1024);

        var drawer = AppPresentationBuilder.CreateDrawer(
            registered,
            [registered, updaterPayload]);

        drawer.StorageOutcomeSummary.Should().Contain("当前主程序约 447.0 MB")
            .And.Contain("同类记录另有约 237.0 MB")
            .And.Contain("当前迁移不会改变它");
    }

    [Fact]
    public void Unregistered_C_drive_entry_is_not_called_the_main_program()
    {
        var registered = Profile(
            "OpenCode 1.14.41",
            @"C:\Program Files\OpenCode",
            uninstallCommand: @"""C:\Program Files\OpenCode\Uninstall OpenCode.exe"" /allusers",
            version: "1.14.41",
            installedBytes: 447L * 1024 * 1024);
        var updaterPayload = Profile(
            "OpenCode 1.18.4",
            @"C:\Users\Me\AppData\Local\opencode-updater",
            uninstallCommand: null,
            version: "1.18.4",
            installedBytes: 237L * 1024 * 1024);

        var drawer = AppPresentationBuilder.CreateDrawer(
            updaterPayload,
            [registered, updaterPayload]);

        drawer.StorageOutcomeSummary.Should()
            .Contain("当前这条程序、副本或更新载荷约 237.0 MB")
            .And.NotContain("当前主程序约 237.0 MB");
    }

    [Fact]
    public void Beginner_family_and_storage_conclusions_have_stable_ids_before_actions()
    {
        var xaml = File.ReadAllText(
            FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(
            FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain(
                "AutomationProperties.AutomationId=\"DrawerFamilySummaryTextBlock\"")
            .And.Contain(
                "AutomationProperties.AutomationId=\"DrawerCurrentEntryTextBlock\"")
            .And.Contain(
                "AutomationProperties.AutomationId=\"DrawerStorageOutcomeTextBlock\"");
        xaml.IndexOf("DrawerFamilySummaryTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(
                xaml.IndexOf("DrawerLocationTextBlock", StringComparison.Ordinal));
        xaml.IndexOf("DrawerStorageOutcomeTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(
                xaml.IndexOf("DrawerUninstallButton", StringComparison.Ordinal));

        code.Should().Contain(
                "AppPresentationBuilder.CreateDrawer(profile, _softwareProfiles)")
            .And.Contain("DrawerFamilySummaryTextBlock.Text = drawer.FamilySummary;")
            .And.Contain("DrawerCurrentEntryTextBlock.Text = drawer.CurrentEntrySummary;")
            .And.Contain("DrawerStorageOutcomeTextBlock.Text = drawer.StorageOutcomeSummary;");
    }

    private static SoftwareProfile Profile(
        string name,
        string? installPath,
        string? uninstallCommand,
        string? version = null,
        long installedBytes = 0,
        long cDriveDataBytes = 0,
        IReadOnlyList<string>? cDrivePaths = null) =>
        new()
        {
            Name = name,
            InstallPath = installPath,
            UninstallCommand = uninstallCommand,
            DisplayVersion = version,
            InstalledSizeBytes = installedBytes,
            DataSizeBytes = cDriveDataBytes,
            CDriveDataSizeBytes = cDriveDataBytes,
            DataPaths = cDrivePaths ?? [],
            CDriveWritePaths = cDrivePaths ?? []
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
