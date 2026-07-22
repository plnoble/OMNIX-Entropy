using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentBackgroundApplicationHandoffTests
{
    [Fact]
    public void Background_items_and_aggregate_reply_preserve_safe_application_context()
    {
        var ordinary = new SoftwareProfile
        {
            Name = "Chat App",
            Category = SoftwareCategory.Normal,
            RunningProcesses = ["chat"]
        };
        var invalidName = new SoftwareProfile
        {
            Name = @"C:\private\service.exe",
            Category = SoftwareCategory.Unknown,
            Services = ["private-service"]
        };

        var review = AgentBackgroundReviewPresenter.Create([ordinary, invalidName]);
        var ordinaryItem = review.Items.Single(item => item.AppName == "Chat App");
        var protectedItem = review.Items.Single(item => item.AppName == "这个应用");
        var reply = AgentConversationPresenter.Answer(
            "哪些应用在后台常驻",
            null,
            [ordinary, invalidName]);

        ordinaryItem.TargetAppName.Should().Be("Chat App");
        ordinaryItem.CanOpenApp.Should().BeTrue();
        ordinaryItem.NavigationLabel.Should().Be("查看应用");
        protectedItem.TargetAppName.Should().BeNull();
        protectedItem.CanOpenApp.Should().BeFalse();
        string.Join("\n", review.Items.SelectMany(Visible))
            .Should().NotContain(@"C:\")
            .And.NotContain("private-service");

        reply.Intent.Should().Be(AgentQuestionIntent.StartupAndBackground);
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.TargetAppFilter.Should().Be(AppCatalogFilter.Resident);
        reply.TargetAppName.Should().BeNull();
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Main_window_exposes_details_only_item_and_whitelisted_resident_catalog_handoffs()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var itemHandler = SourceMethodExtractor.Extract(
            code,
            "private async void OpenAgentBackgroundApp_Click(object sender, RoutedEventArgs e)");
        var aggregateHandler = SourceMethodExtractor.Extract(
            code,
            "private async Task OpenAgentAppCatalogFilterAsync(AppCatalogFilter filter)");
        var conversationHandler = SourceMethodExtractor.Extract(
            code,
            "private async void AgentConversationNavigate_Click(object sender, RoutedEventArgs e)");

        xaml.Should().Contain("Content=\"查看应用\"")
            .And.Contain("Click=\"OpenAgentBackgroundApp_Click\"")
            .And.Contain("StringFormat=AgentBackgroundOpen_{0}");
        itemHandler.Should().Contain("ResolveAndOpenAppTargetAsync")
            .And.NotContain("ShowStartupControlPreviewAsync")
            .And.NotContain("StartupEntryControl")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry")
            .And.NotContain("ServiceController");
        aggregateHandler.Should().Contain("AppCatalogFilter.Resident")
            .And.Contain("AppCatalogFilter.CDrive")
            .And.Contain("AppCatalogFilter.Uninstallable")
            .And.Contain("_appCatalogFilter = filter")
            .And.Contain("AppSearchTextBox.Text = string.Empty")
            .And.Contain("await EnsureSoftwareInventoryLoadedAsync()")
            .And.Contain("RefreshAppCatalog()")
            .And.Contain("AppTilesListBox.BringIntoView()")
            .And.NotContain("ShowStartupControlPreviewAsync")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry")
            .And.NotContain("ServiceController");
        conversationHandler.Should().Contain("reply.TargetAppFilter is { } appFilter")
            .And.Contain("await OpenAgentAppCatalogFilterAsync(appFilter)");
    }

    private static IEnumerable<string> Visible(AgentBackgroundReviewItemViewModel item) =>
        [
            item.AppName,
            item.EvidenceSummary,
            item.RiskLabel,
            item.RecommendedNextStep,
            item.NavigationLabel
        ];

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
