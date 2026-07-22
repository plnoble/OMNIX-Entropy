using Css.Core.Apps;
using Css.Core.Agent;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class MachineHealthExperienceTests
{
    private const long GiB = 1024L * 1024 * 1024;

    [Fact]
    public void Real_observations_become_plain_bounded_health_dimensions()
    {
        var machine = new MachineHealthObservation
        {
            ObservedAtUtc = DateTimeOffset.UtcNow,
            SecondaryDrive = new LocalDriveHealthObservation
            {
                Availability = MachineMetricAvailability.Available,
                TotalBytes = 500 * GiB,
                FreeBytes = 100 * GiB
            },
            Memory = new MemoryHealthObservation
            {
                Availability = MachineMetricAvailability.Available,
                TotalBytes = 32 * GiB,
                AvailableBytes = 8 * GiB,
                LoadPercent = 75
            },
            Battery = new BatteryHealthObservation
            {
                Availability = MachineMetricAvailability.Available,
                ChargePercent = 12,
                IsOnAcPower = false,
                IsCharging = false
            },
            ProcessCount = 383
        };
        var profiles = new List<SoftwareProfile>
        {
            Profile("One", SoftwareCategory.Normal, true),
            Profile("Two", SoftwareCategory.Normal, true),
            Profile("Three", SoftwareCategory.Normal, true),
            Profile("Four", SoftwareCategory.Normal, true),
            Profile("Windows Component", SoftwareCategory.SystemTool, true)
        };
        var growth = new GrowthFinding
        {
            Path = @"C:\private\cache",
            OwnerSoftware = "Example App",
            PreviousBytes = GiB,
            CurrentBytes = 3 * GiB,
            ObservedSnapshots = 4,
            PositiveGrowthIntervals = 3,
            IsSustainedGrowth = true,
            TrendGrowthBytes = 2 * GiB,
            SourceKind = GrowthSourceKind.Software,
            Reason = "fixture"
        };

        var summary = HealthCheckSummaryBuilder.Build(
            DriveResult(),
            [],
            [growth],
            machineHealth: machine,
            softwareProfiles: profiles,
            observedSnapshotCount: 4);

        summary.Dimensions.Select(dimension => dimension.Name).Should().ContainInOrder(
            "综合评分",
            "磁盘健康",
            "D 盘空间",
            "内存占用",
            "电池状态",
            "自启动线索",
            "使用趋势");
        Dimension(summary, "综合评分").Result.Should().Contain("当前按磁盘空间");
        Dimension(summary, "D 盘空间").Result.Should().Contain("80.0%").And.Contain("100.0 GB");
        Dimension(summary, "内存占用").Result.Should().Contain("24.0 GB/32.0 GB").And.Contain("383 个进程");
        Dimension(summary, "内存占用").Rating.Should().Be("建议观察");
        Dimension(summary, "电池状态").Rating.Should().Be("电量较低");
        Dimension(summary, "自启动线索").Result.Should().Contain("4 个普通应用").And.Contain("1 个系统组件线索");
        Dimension(summary, "使用趋势").Result.Should().Contain("4 次手动体检").And.Contain("1 个持续增长来源");
        string.Join("\n", summary.Dimensions.Select(dimension => dimension.Result))
            .Should().NotContain(@"C:\private")
            .And.NotContain("Example App");
        machine.CanExecuteDirectly.Should().BeFalse();

        var reply = AgentConversationPresenter.Answer("我的内存和电池怎么样", summary, profiles);
        reply.Intent.Should().Be(AgentQuestionIntent.MachineHealth);
        reply.EvidenceLines.Should().Contain(line => line.Contains("内存占用"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("电池状态"));
        reply.Answer.Should().Contain("一次偏高不等于电脑故障");
        reply.CanExecuteDirectly.Should().BeFalse();
        reply.EvidenceLines.Should().NotContain(line => line.Contains(@"C:\"));
    }

    [Fact]
    public void Missing_data_and_short_history_are_never_invented()
    {
        var summary = HealthCheckSummaryBuilder.Build(
            DriveResult(),
            [],
            growthFindings:
            [
                new GrowthFinding
                {
                    Path = @"C:\private",
                    OwnerSoftware = "Unknown",
                    PreviousBytes = 1,
                    CurrentBytes = 2,
                    ObservedSnapshots = 2,
                    PositiveGrowthIntervals = 1,
                    IsSustainedGrowth = true,
                    TrendGrowthBytes = 1,
                    Reason = "fixture"
                }
            ],
            machineHealth: null,
            softwareProfiles: null,
            observedSnapshotCount: 2);

        Dimension(summary, "D 盘空间").Rating.Should().Be("未检测");
        Dimension(summary, "内存占用").Result.Should().Contain("未读取到");
        Dimension(summary, "电池状态").Rating.Should().Be("未检测");
        Dimension(summary, "自启动线索").Result.Should().Contain("尚未扫描");
        Dimension(summary, "使用趋势").Rating.Should().Be("历史不足");
        Dimension(summary, "使用趋势").Result.Should().Contain("至少 3 次");

        var noEvidenceReply = AgentConversationPresenter.Answer("电脑为什么卡", null, []);
        noEvidenceReply.Intent.Should().Be(AgentQuestionIntent.MachineHealth);
        noEvidenceReply.Answer.Should().Contain("不会猜");
        noEvidenceReply.NavigationTargetPage.Should().Be("Home");
        noEvidenceReply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Machine_observations_do_not_silently_change_cleanup_score_or_authority()
    {
        var withoutMachine = HealthCheckSummaryBuilder.Build(DriveResult(), []);
        var withMachine = HealthCheckSummaryBuilder.Build(
            DriveResult(),
            [],
            machineHealth: new MachineHealthObservation
            {
                ObservedAtUtc = DateTimeOffset.UtcNow,
                SecondaryDrive = new LocalDriveHealthObservation { Availability = MachineMetricAvailability.NotPresent },
                Memory = new MemoryHealthObservation
                {
                    Availability = MachineMetricAvailability.Available,
                    TotalBytes = 16 * GiB,
                    AvailableBytes = GiB,
                    LoadPercent = 94
                },
                Battery = new BatteryHealthObservation { Availability = MachineMetricAvailability.NotPresent },
                ProcessCount = 999
            },
            softwareProfiles:
            [
                Profile("Startup", SoftwareCategory.Normal, true)
            ],
            observedSnapshotCount: 3);

        withMachine.OverallScore.Should().Be(withoutMachine.OverallScore);
        withMachine.KeyFindings.Should().BeEquivalentTo(withoutMachine.KeyFindings);
        Dimension(withMachine, "D 盘空间").Rating.Should().Be("未配置");
        Dimension(withMachine, "电池状态").Rating.Should().Be("不适用");
        Dimension(withMachine, "内存占用").Rating.Should().Be("本次偏高");
    }

    [Fact]
    public void Windows_probe_is_read_only_private_name_free_and_manual_scan_only()
    {
        var probe = ReadSource("src", "Css.Win32", "SystemHealth", "WindowsMachineHealthProbe.cs");
        var model = ReadSource("src", "Css.Core", "Apps", "MachineHealthObservation.cs");
        var main = ReadSource("src", "Css.App", "MainWindow.xaml.cs");
        var xaml = ReadSource("src", "Css.App", "MainWindow.xaml");

        probe.Should().Contain("DriveType.Fixed")
            .And.Contain("candidate.Name.Equals")
            .And.Contain("@\"D:\\\"")
            .And.Contain("GlobalMemoryStatusEx")
            .And.Contain("GetSystemPowerStatus")
            .And.NotContain("ProcessName")
            .And.NotContain("MainModule")
            .And.NotContain(".Kill(")
            .And.NotContain("CloseMainWindow")
            .And.NotContain("Microsoft.Win32")
            .And.NotContain("System.Management");
        model.Should().NotContain("string Process")
            .And.NotContain("IReadOnlyList<string>")
            .And.Contain("public bool CanExecuteDirectly => false;");
        main.Should().Contain("new WindowsMachineHealthProbe().Observe()")
            .And.Contain("正在只读读取 D 盘、内存、进程、电池和硬件配置")
            .And.NotContain("new WindowsMachineHealthProbe().Observe();");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"HealthDimensionListView\"")
            .And.Contain("StringFormat=HealthDimension_{0}")
            .And.Contain("Property=\"AutomationProperties.Name\"")
            .And.Contain("Text=\"{Binding Result}\" TextWrapping=\"Wrap\"")
            .And.Contain("Height=\"260\"")
            .And.Contain("AutomationProperties.AutomationId=\"HomeNavButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"TimelineNavButton\"")
            .And.Contain("AutomationProperties.AutomationId=\"AgentNavButton\"");
    }

    private static HealthDimensionResult Dimension(HealthCheckSummary summary, string name) =>
        summary.Dimensions.Single(dimension => dimension.Name == name);

    private static SoftwareProfile Profile(
        string name,
        SoftwareCategory category,
        bool startup) =>
        new()
        {
            Name = name,
            Category = category,
            StartupEntries = startup ? [name + " Startup"] : []
        };

    private static DriveScanResult DriveResult() =>
        new()
        {
            Drive = @"C:\",
            TotalBytes = 100 * GiB,
            FreeBytes = 30 * GiB,
            TopLevel = []
        };

    private static string ReadSource(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "ComputerSecuritySoftware.slnx")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
