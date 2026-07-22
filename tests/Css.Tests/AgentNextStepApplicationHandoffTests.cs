using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentNextStepApplicationHandoffTests
{
    [Fact]
    public void Next_step_actions_preserve_resident_and_c_drive_catalog_context()
    {
        var panel = AgentNextStepPresenter.Create(null, Profiles());
        var empty = AgentNextStepPresenter.Create(null, []);
        var filterProperty = typeof(AgentNextActionViewModel).GetProperty("TargetAppFilter");
        var automationIdProperty = typeof(AgentNextActionViewModel).GetProperty("AutomationId");

        filterProperty.Should().NotBeNull();
        automationIdProperty.Should().NotBeNull();

        var resident = panel.NavigationActions.Single(action =>
            action.Label.Contains("后台常驻", StringComparison.Ordinal));
        var cDrive = panel.NavigationActions.Single(action =>
            action.TargetPage == "Apps"
            && action.Description.Contains("C 盘数据线索", StringComparison.Ordinal));

        filterProperty!.GetValue(resident).Should().Be(AppCatalogFilter.Resident);
        filterProperty.GetValue(cDrive).Should().Be(AppCatalogFilter.CDrive);
        resident.TargetPage.Should().Be("Apps");
        cDrive.TargetPage.Should().Be("Apps");
        automationIdProperty!.GetValue(resident).Should().Be("AgentNextAction_Apps_Resident");
        automationIdProperty.GetValue(cDrive).Should().Be("AgentNextAction_Apps_CDrive");
        empty.NavigationActions.Should().OnlyContain(action =>
            filterProperty.GetValue(action) == null);
    }

    [Fact]
    public void Main_window_binds_typed_next_step_actions_to_the_bounded_catalog_handoff()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var handler = SourceMethodExtractor.Extract(
            code,
            "private async void AgentNextAction_Click(object sender, RoutedEventArgs e)");

        xaml.Should().Contain("AutomationProperties.AutomationId=\"{Binding AutomationId}\"")
            .And.Contain("Tag=\"{Binding}\"")
            .And.Contain("Click=\"AgentNextAction_Click\"");
        handler.Should().Contain("AgentNextActionViewModel action")
            .And.Contain("action.IsNavigationOnly")
            .And.Contain("action.TargetAppFilter is { } appFilter")
            .And.Contain("action.TargetPage")
            .And.Contain("await OpenAgentAppCatalogFilterAsync(appFilter)")
            .And.NotContain("ShowMigrationPlanAsync")
            .And.NotContain("ShowUninstallPlanAsync")
            .And.NotContain("ShowStartupControlPreviewAsync")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry")
            .And.NotContain("ServiceController");
    }

    private static SoftwareProfile[] Profiles() =>
    [
        new()
        {
            Name = "C Drive App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Apps\CDriveApp"
        },
        Resident("Sync Tool"),
        Resident("Chat Helper"),
        Resident("Updater"),
        Resident("Agent Sidecar")
    ];

    private static SoftwareProfile Resident(string name) =>
        new()
        {
            Name = name,
            Category = SoftwareCategory.Normal,
            InstallPath = $@"D:\Software\{name}\Install",
            RunningProcesses = [name.Replace(" ", string.Empty, StringComparison.Ordinal)]
        };

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
