using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class ApplicationBackgroundOwnershipSummaryTests
{
    [Fact]
    public void Background_catalog_is_exhaustive_and_matches_agent_resident_groups()
    {
        var profiles = MixedProfiles();

        var background = BackgroundApplicationOwnershipCatalog.Create(profiles);
        var agent = AgentActionCandidateCatalog.Create(profiles);

        background.AllProfiles.Should().HaveCount(4);
        background.OrdinaryProfiles.Should().HaveCount(2);
        background.SystemProfiles.Should().ContainSingle(profile => profile.Name == "System Service");
        background.OwnershipPendingProfiles.Should().ContainSingle(profile => profile.Name == "Ownership Task");
        (background.OrdinaryProfiles.Count
            + background.SystemProfiles.Count
            + background.OwnershipPendingProfiles.Count)
            .Should().Be(background.AllProfiles.Count);
        agent.ResidentProfiles.Should().BeEquivalentTo(background.AllProfiles);
        agent.OrdinaryResidentProfiles.Should().BeEquivalentTo(background.OrdinaryProfiles);
        agent.ReadOnlyResidentProfiles.Should().BeEquivalentTo(background.ReadOnlyProfiles);
    }

    [Fact]
    public void Existing_application_summary_keeps_signal_totals_and_explains_ownership()
    {
        var profiles = MixedProfiles();

        var summary = AppCatalogSummaryPresenter.Create(profiles, visibleCount: profiles.Count);

        summary.ResidentAppCount.Should().Be(4);
        summary.OrdinaryResidentAppCount.Should().Be(2);
        summary.SystemResidentAppCount.Should().Be(1);
        summary.OwnershipPendingResidentAppCount.Should().Be(1);
        summary.RunningAppCount.Should().Be(1);
        summary.StartupAppCount.Should().Be(1);
        summary.ServiceAppCount.Should().Be(2);
        summary.ScheduledTaskAppCount.Should().Be(1);
        summary.Text.Should().Contain("后台线索：普通应用 2 个")
            .And.Contain("系统组件 1 个")
            .And.Contain("归属待确认 1 个")
            .And.Contain("仅供查看")
            .And.Contain("正在运行 1 个")
            .And.Contain("有自启动 1 个")
            .And.Contain("有后台服务 2 个")
            .And.Contain("有计划任务 1 个")
            .And.NotContain("Ordinary Running")
            .And.NotContain("System Service")
            .And.NotContain("Ownership Task")
            .And.NotContain(@"C:\Windows")
            .And.NotContain(@"D:\Software");
    }

    [Fact]
    public void Protected_only_background_evidence_is_explicitly_read_only()
    {
        var profiles = MixedProfiles()
            .Where(profile => profile.Category == SoftwareCategory.SystemTool
                || profile.Name == "Ownership Task")
            .ToArray();

        var summary = AppCatalogSummaryPresenter.Create(profiles, visibleCount: profiles.Length);

        summary.ResidentAppCount.Should().Be(2);
        summary.OrdinaryResidentAppCount.Should().Be(0);
        summary.Text.Should().Contain("普通应用 0 个")
            .And.Contain("系统组件 1 个")
            .And.Contain("归属待确认 1 个")
            .And.Contain("仅供查看")
            .And.NotContain("建议关闭")
            .And.NotContain("可以关闭");
    }

    private static IReadOnlyList<SoftwareProfile> MixedProfiles() =>
    [
        new SoftwareProfile
        {
            Name = "Ordinary Running",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\OrdinaryRunning\Install",
            RunningProcesses = ["OrdinaryRunning"]
        },
        new SoftwareProfile
        {
            Name = "Ordinary Startup Service",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\OrdinaryStartup\Install",
            StartupEntries = ["Ordinary Startup"],
            Services = ["OrdinaryService"]
        },
        new SoftwareProfile
        {
            Name = "System Service",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"D:\SystemFixture\Install",
            Services = ["SystemService"]
        },
        new SoftwareProfile
        {
            Name = "Ownership Task",
            Category = SoftwareCategory.Unknown,
            InstallPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "SystemApps",
                "OwnershipTask"),
            ScheduledTasks = ["OwnershipTask"]
        },
        new SoftwareProfile
        {
            Name = "Inactive",
            Category = SoftwareCategory.Normal,
            InstallPath = @"D:\Software\Inactive\Install"
        }
    ];
}
