using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppResidueReviewAvailabilityTests
{
    [Fact]
    public void Ordinary_app_keeps_external_uninstall_residue_review_without_an_uninstall_command()
    {
        var profile = new SoftwareProfile
        {
            Name = "Externally Removed App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Externally Removed App"
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var uninstall = drawer.AvailableActions.Single(action => action.Kind == AppActionKind.Uninstall);

        uninstall.IsEnabled.Should().BeFalse("there is no official uninstaller to launch");
        drawer.UninstallResidueReview.IsEnabled.Should().BeTrue();
        drawer.UninstallResidueReview.Reason.Should().Contain("外部卸载")
            .And.Contain("仍在")
            .And.Contain("不会处理");
    }

    [Fact]
    public void System_and_managed_root_ownership_pending_apps_cannot_enter_ordinary_residue_review()
    {
        var windowsRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        windowsRoot.Should().NotBeNullOrWhiteSpace();
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Windows Component",
                Category = SoftwareCategory.SystemTool,
                InstallPath = Path.Combine(windowsRoot, "System32", "Component")
            },
            new SoftwareProfile
            {
                Name = "Unknown Managed Component",
                Category = SoftwareCategory.Unknown,
                InstallPath = Path.Combine(windowsRoot, "System32", "UnknownComponent")
            }
        };

        var drawers = profiles.Select(AppPresentationBuilder.CreateDrawer).ToList();

        drawers.Should().OnlyContain(drawer => !drawer.UninstallResidueReview.IsEnabled);
        drawers[0].UninstallResidueReview.Reason.Should().Contain("系统组件")
            .And.Contain("不会");
        drawers[1].UninstallResidueReview.Reason.Should().Contain("系统归属未确认")
            .And.Contain("暂不");
        string.Join("\n", drawers.Select(drawer => drawer.UninstallResidueReview.Reason))
            .Should().NotContain(windowsRoot);
    }

    [Fact]
    public void Main_window_binds_and_rechecks_the_shared_residue_review_policy()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var show = ExtractMethod(
            code,
            "private void ShowAppDrawer",
            "private static void ApplyActionState");
        var review = ExtractMethod(
            code,
            "private async Task ReviewUninstallResidueAsync",
            "private void ShowResidueReviewInline");

        show.Should().Contain("ApplyResidueReviewState(DrawerResidueReviewButton, drawer.UninstallResidueReview);")
            .And.NotContain("DrawerResidueReviewButton.IsEnabled = true;");
        review.Should().Contain("AppPresentationBuilder.CreateUninstallResidueReviewAvailability(before)")
            .And.Contain("if (!availability.IsEnabled)")
            .And.Contain("RefreshResidueReviewButtonForCurrentSelection();")
            .And.NotContain("DrawerResidueReviewButton.IsEnabled = AppTilesListBox.SelectedItem is AppTileUi;");
        code.Should().Contain("private void RefreshResidueReviewButtonForCurrentSelection()")
            .And.Contain("ApplyResidueReviewState(DrawerResidueReviewButton, availability);");
    }

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

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }
}
