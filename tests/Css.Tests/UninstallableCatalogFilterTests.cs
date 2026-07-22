using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class UninstallableCatalogFilterTests
{
    [Fact]
    public void Filter_membership_matches_the_drawer_uninstall_review_availability()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        windowsRoot.Should().NotBeNullOrWhiteSpace();
        var profiles = new[]
        {
            Profile("Ordinary With Command", SoftwareCategory.Normal, @"D:\Software\Ordinary", "ordinary-uninstall.exe"),
            Profile("Ordinary Without Command", SoftwareCategory.Normal, @"D:\Software\No Command", null),
            Profile("System With Command", SoftwareCategory.SystemTool, Path.Combine(windowsRoot, "System32", "System App"), "system-uninstall.exe"),
            Profile("Unknown Managed Component", SoftwareCategory.Unknown, Path.Combine(windowsRoot, "System32", "Unknown App"), "unknown-uninstall.exe"),
            Profile("Unknown Ordinary Utility", SoftwareCategory.Unknown, @"D:\Software\Microsoft Utility", "utility-uninstall.exe", "Microsoft Corporation")
        };

        var filtered = AppCatalogPresenter.Apply(
            profiles,
            new AppCatalogQuery
            {
                Filter = AppCatalogFilter.Uninstallable,
                Sort = AppCatalogSort.Name
            });

        filtered.Select(profile => profile.Name).Should().Equal(
            "Ordinary With Command",
            "Unknown Ordinary Utility");
        foreach (var profile in profiles)
        {
            var drawerAllowsReview = AppPresentationBuilder.CreateDrawer(profile)
                .AvailableActions
                .Single(action => action.Kind == AppActionKind.Uninstall)
                .IsEnabled;
            filtered.Contains(profile).Should().Be(
                drawerAllowsReview,
                $"catalog and drawer must agree for {profile.Name}");
        }
    }

    [Fact]
    public void Filter_does_not_treat_a_registered_command_as_execution_readiness()
    {
        var system = Profile(
            "System With Command",
            SoftwareCategory.SystemTool,
            @"C:\Windows\System32\System App",
            "system-uninstall.exe");

        var filtered = AppCatalogPresenter.Apply(
            [system],
            new AppCatalogQuery { Filter = AppCatalogFilter.Uninstallable });
        var drawer = AppPresentationBuilder.CreateDrawer(system);

        filtered.Should().BeEmpty();
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.Uninstall)
            .Reason.Should().Contain("系统组件");
        drawer.UninstallPreviewLines.Should().Contain(line => line.Contains("不会生成普通应用卸载计划"));
    }

    private static SoftwareProfile Profile(
        string name,
        SoftwareCategory category,
        string installPath,
        string? uninstallCommand,
        string? publisher = null) =>
        new()
        {
            Name = name,
            Publisher = publisher,
            Category = category,
            InstallPath = installPath,
            UninstallCommand = uninstallCommand
        };
}
