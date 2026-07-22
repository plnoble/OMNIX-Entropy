using Css.Core.Apps;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppDrawerEmptyStateTests
{
    [Fact]
    public void Empty_drawer_state_replaces_every_previous_beginner_conclusion()
    {
        var state = AppDrawerEmptyStatePresenter.Create("没有符合条件的应用");

        state.Title.Should().Be("没有符合条件的应用");
        state.SupportingText.Should().Contain("调整分类、搜索词或排序方式");
        state.CategorySummary.Should().Contain("当前没有应用")
            .And.Contain("分类依据已清空");
        state.InstallLocationSummary.Should().Be("-");
        state.SizeSummary.Should().Be("-");
        state.ResidencySummary.Should().Be("-");
        state.AgentAdviceText.Should().Contain("当前没有应用可供分析");
        state.DisabledActionReason.Should().Contain("请先选择应用");

        Visible(state).Should().NotContain("Marvis")
            .And.NotContain(@"C:\")
            .And.NotContain(@"D:\")
            .And.NotContain("卸载命令")
            .And.NotContain("服务名");
    }

    [Fact]
    public void Technical_details_have_an_explicit_no_status_collapsed_state()
    {
        var state = AppDrawerTechnicalDetailsPresenter.Collapsed();

        state.IsVisible.Should().BeFalse();
        state.ButtonText.Should().Be("查看技术详情");
        state.StatusText.Should().BeEmpty();
    }

    [Fact]
    public void Empty_inventory_and_empty_filter_share_the_complete_reset_path()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var refresh = ExtractMethod(
            code,
            "private void RefreshAppCatalog",
            "private string BuildMigrationClosureCatalogSummary");
        var clear = ExtractMethod(
            code,
            "private void ClearAppDrawer",
            "private void SetAppFilterSelected");
        var show = ExtractMethod(
            code,
            "private void ShowAppDrawer",
            "private static void ApplyActionState");
        var host = ExtractMethod(
            code,
            "private void ApplyDrawerActionHost",
            "private async void DrawerActionPreviewPrimary_Click");

        refresh.Should().Contain("ClearAppDrawer(emptyStateTitle);")
            .And.Contain("ClearAppDrawer(\"没有符合条件的应用\");");
        clear.Should().Contain("AppDrawerEmptyStatePresenter.Create(reason)")
            .And.Contain("AppTilesListBox.SelectedIndex = -1;")
            .And.Contain("DrawerCategorySummaryTextBlock.Text = empty.CategorySummary;")
            .And.Contain("ApplyDrawerActionHost(AppDrawerActionHostPresenter.Collapsed());")
            .And.Contain("ApplyDrawerTechnicalDetailsState(AppDrawerTechnicalDetailsPresenter.Collapsed(), DrawerTechnicalDetailsButton);")
            .And.Contain("DrawerMigrateButton.Content = \"迁移到 D 盘\";")
            .And.Contain("DrawerUninstallButton.IsEnabled = false;")
            .And.Contain("DrawerMigrateButton.IsEnabled = false;")
            .And.Contain("DrawerCleanCacheButton.IsEnabled = false;")
            .And.Contain("DrawerDisableStartupButton.IsEnabled = false;")
            .And.Contain("DrawerResidueReviewButton.IsEnabled = false;");
        show.Should().Contain("ApplyDrawerTechnicalDetailsState(AppDrawerTechnicalDetailsPresenter.Collapsed(), DrawerTechnicalDetailsButton);");
        host.Should().Contain("_pendingDrawerOperation = null;")
            .And.Contain("_pendingDrawerTargetAppName = null;")
            .And.Contain("_pendingStartupTargetAppName = null;");
        xaml.Should().Contain("x:Name=\"DrawerTechnicalDetailsButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"DrawerTechnicalDetailsButton\"");
    }

    private static string Visible(AppDrawerEmptyStateViewModel state) =>
        string.Join("\n",
            state.Title,
            state.SupportingText,
            state.CategorySummary,
            state.InstallLocationSummary,
            state.SizeSummary,
            state.ResidencySummary,
            state.AgentAdviceText,
            state.DisabledActionReason);

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
