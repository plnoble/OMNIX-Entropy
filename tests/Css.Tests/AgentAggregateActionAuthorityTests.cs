using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using Css.Core.Startup;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentAggregateActionAuthorityTests
{
    [Fact]
    public void Aggregate_catalog_separates_ordinary_data_review_and_protected_profiles()
    {
        var profiles = Profiles();

        var catalog = AgentActionCandidateCatalog.Create(profiles);

        catalog.CDriveProfiles.Select(item => item.Name).Should().BeEquivalentTo(
            "Ordinary C App",
            "D Data App",
            "Windows Component",
            "Ownership Pending");
        catalog.MigrationReviewProfiles.Select(item => item.Name)
            .Should().Equal("Ordinary C App");
        catalog.DataLocationReviewProfiles.Select(item => item.Name)
            .Should().Equal("D Data App");
        catalog.ReadOnlyCDriveProfiles.Select(item => item.Name)
            .Should().BeEquivalentTo("Windows Component", "Ownership Pending");
        catalog.UninstallReviewProfiles.Select(item => item.Name)
            .Should().Equal("Ordinary C App");
        catalog.ReadOnlyUninstallProfiles.Select(item => item.Name)
            .Should().BeEquivalentTo("Windows Component", "Ownership Pending");
        catalog.StartupReviewProfiles.Select(item => item.Name)
            .Should().Equal("Ordinary C App");
        catalog.ReadOnlyStartupProfiles.Select(item => item.Name)
            .Should().BeEquivalentTo("Windows Component", "Ownership Pending");
    }

    [Fact]
    public void Homepage_next_step_distinguishes_actionable_data_only_and_read_only_c_drive_clues()
    {
        var panel = AgentNextStepPresenter.Create(null, Profiles());
        var visible = string.Join(
            "\n",
            new[] { panel.Title, panel.Summary, panel.SafetyBoundary }
                .Concat(panel.Reasons)
                .Concat(panel.SafeNextActions));

        panel.Reasons.Should().Contain(line =>
            line.Contains("2 个普通应用") && line.Contains("C 盘"));
        panel.Reasons.Should().Contain(line =>
            line.Contains("2 个系统相关或归属待确认项") && line.Contains("仅供查看"));
        panel.SafeNextActions.Should().Contain(line =>
            line.Contains("1 个普通应用") && line.Contains("主程序迁移"));
        panel.SafeNextActions.Should().Contain(line =>
            line.Contains("1 个") && line.Contains("只复查 C 盘数据来源"));
        panel.SafeNextActions.Should().Contain(line =>
            line.Contains("系统相关或归属待确认") && line.Contains("不进入普通操作"));
        panel.NavigationActions.Should().Contain(action => action.TargetPage == "Apps");
        panel.CanExecuteDirectly.Should().BeFalse();
        visible.Should().NotContain(@"C:\Windows")
            .And.NotContain(@"D:\Software");
    }

    [Fact]
    public void General_agent_replies_never_mix_protected_or_d_data_profiles_into_actionable_lists()
    {
        var profiles = Profiles();

        var migration = AgentConversationPresenter.Answer("把软件迁移到D盘", null, profiles);
        var uninstall = AgentConversationPresenter.Answer("哪些软件可以卸载", null, profiles);
        var startup = AgentConversationPresenter.Answer("哪些软件会开机启动", null, profiles);

        var migrationCandidates = migration.EvidenceLines.Single(line => line.StartsWith("可评估主程序迁移："));
        migrationCandidates.Should().Contain("Ordinary C App")
            .And.NotContain("D Data App")
            .And.NotContain("Windows Component")
            .And.NotContain("Ownership Pending");
        migration.EvidenceLines.Should().Contain(line =>
            line.StartsWith("只复查 C 盘数据位置：") && line.Contains("D Data App"));
        migration.EvidenceLines.Should().Contain(line =>
            line.Contains("仅供查看")
            && line.Contains("Windows Component")
            && line.Contains("Ownership Pending"));

        uninstall.Answer.Should().Contain("1 个普通应用带可审核的官方卸载入口");
        uninstall.EvidenceLines.Single(line => line.StartsWith("可审核官方卸载："))
            .Should().Contain("Ordinary C App")
            .And.NotContain("Windows Component")
            .And.NotContain("Ownership Pending");
        uninstall.EvidenceLines.Should().Contain(line =>
            line.Contains("仅供查看")
            && line.Contains("Windows Component")
            && line.Contains("Ownership Pending"));

        startup.Answer.Should().Contain("1 个普通应用具备本地审核线索");
        startup.EvidenceLines.Single(line => line.StartsWith("可审核的普通自启动应用："))
            .Should().Contain("Ordinary C App")
            .And.NotContain("Windows Component")
            .And.NotContain("Ownership Pending");
        startup.EvidenceLines.Should().Contain(line =>
            line.Contains("仅供查看")
            && line.Contains("Windows Component")
            && line.Contains("Ownership Pending"));

        migration.TargetAppFilter.Should().Be(AppCatalogFilter.CDrive);
        uninstall.TargetAppFilter.Should().Be(AppCatalogFilter.Uninstallable);
        startup.TargetAppFilter.Should().Be(AppCatalogFilter.Resident);
        new[] { migration, uninstall, startup }.Should().OnlyContain(reply =>
            reply.TargetAppName == null);

        new[] { migration, uninstall, startup }.Should().OnlyContain(reply =>
            reply.NavigationTargetPage == "Apps"
            && reply.CanNavigate
            && !reply.CanExecuteDirectly);
    }

    [Fact]
    public void Aggregate_application_handoff_is_a_bounded_read_only_catalog_filter()
    {
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "MainWindow.xaml.cs"));
        var method = SourceMethodExtractor.Extract(
            code,
            "private async Task OpenAgentAppCatalogFilterAsync(AppCatalogFilter filter)");

        method.Should().Contain("AppCatalogFilter.Resident")
            .And.Contain("AppCatalogFilter.CDrive")
            .And.Contain("AppCatalogFilter.Uninstallable")
            .And.Contain("_appCatalogFilter = filter")
            .And.Contain("AppSearchTextBox.Text = string.Empty")
            .And.Contain("await EnsureSoftwareInventoryLoadedAsync()")
            .And.Contain("RefreshAppCatalog()")
            .And.Contain("AppTilesListBox.BringIntoView()")
            .And.Contain("只迁缓存")
            .And.Contain("逐个查看")
            .And.NotContain("ShowMigrationPlanAsync")
            .And.NotContain("ShowUninstallPlanAsync")
            .And.NotContain("ShowStartupControlPreviewAsync")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationPipeline")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry")
            .And.NotContain("ServiceController");
    }

    [Fact]
    public void Aggregate_authority_reuses_current_drawer_policies()
    {
        var presentation = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Agent", "AgentActionCandidateCatalog.cs"));
        var appPresentation = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Apps", "AppPresentation.cs"));

        presentation.Should().Contain("AppPresentationBuilder.CanUseOrdinaryApplicationActions")
            .And.Contain("AppPresentationBuilder.CanReviewMigration")
            .And.Contain("AppPresentationBuilder.CanReviewUninstall")
            .And.Contain("StartupEntryControlPolicy.HasSingleSupportedObservation");
        appPresentation.Should().Contain("public static bool CanUseOrdinaryApplicationActions")
            .And.Contain("public static bool CanReviewMigration");
    }

    [Fact]
    public void Ownership_pending_startup_stays_read_only_in_exact_and_skill_presentations()
    {
        var profiles = Profiles();
        var ordinary = profiles.Single(profile => profile.Name == "Ordinary C App");
        var ownershipPending = profiles.Single(profile => profile.Name == "Ownership Pending");

        var exact = AgentConversationPresenter.Answer(
            "关闭 Ownership Pending 自启动",
            null,
            profiles);
        var review = AgentBackgroundReviewPresenter.Create([ownershipPending]);
        var plan = AgentStartupServicePlanPresenter.Create([ordinary, ownershipPending]);

        exact.Answer.Should().Contain("系统归属待确认")
            .And.NotContain("可以在 OMNIX 中审核");
        exact.TargetAppHandoff.Should().Be(AgentApplicationHandoff.Details);
        review.Items.Single().RiskLabel.Should().Contain("归属待确认");
        review.Items.Single().RecommendedNextStep.Should().Contain("只查看");
        plan.Summary.Should().Contain("1 个普通自启动项可尝试本地审核");
        plan.EvidenceLines.Should().Contain(line =>
            line.Contains("1 个系统相关或归属待确认应用")
            && line.Contains("仅供查看"));
        plan.CanExecuteDirectly.Should().BeFalse();
    }

    private static SoftwareProfile[] Profiles() =>
    [
        new()
        {
            Name = "Ordinary C App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Apps\Ordinary",
            UninstallCommand = @"C:\Apps\Ordinary\uninstall.exe",
            StartupEntries = ["Ordinary C App Startup"],
            BackgroundComponents = [StartupObservation("Ordinary C App Startup")]
        },
        new()
        {
            Name = "D Data App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\DData\Install",
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\DData"]
        },
        new()
        {
            Name = "Windows Component",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"C:\Windows\System32\WindowsComponent",
            UninstallCommand = @"C:\Windows\System32\remove-component.exe",
            StartupEntries = ["Windows Component Startup"],
            BackgroundComponents = [StartupObservation("Windows Component Startup")]
        },
        new()
        {
            Name = "Ownership Pending",
            Category = SoftwareCategory.Unknown,
            InstallPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "SystemApps",
                "OwnershipPending"),
            UninstallCommand = "ownership-pending-uninstall.exe",
            StartupEntries = ["Ownership Pending Startup"],
            BackgroundComponents = [StartupObservation("Ownership Pending Startup")]
        }
    ];

    private static BackgroundComponentObservation StartupObservation(string valueName)
    {
        var now = DateTimeOffset.UtcNow;
        return BackgroundComponentObservationFactory.Startup(
            valueName,
            StartupEntryControlPolicy.SupportedSourceLocator,
            "fixture.exe --background",
            now,
            StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                valueName,
                new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
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

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }
}
