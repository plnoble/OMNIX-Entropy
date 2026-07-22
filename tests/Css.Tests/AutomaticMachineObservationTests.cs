using Css.Core.Agent;
using Css.Core.Apps;
using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticMachineObservationTests
{
    private const long GiB = 1024L * 1024 * 1024;

    [Theory]
    [InlineData("我的电脑配置怎么样", true)]
    [InlineData("CPU 和显卡是什么", true)]
    [InlineData("内存和电池怎么样", true)]
    [InlineData("电脑为什么卡", true)]
    [InlineData("C盘为什么满", false)]
    [InlineData("Wi-Fi 在哪里设置", false)]
    [InlineData("哪些软件会开机启动", false)]
    public void Only_hardware_and_machine_health_questions_need_lightweight_observation(
        string question,
        bool expected)
    {
        AgentConversationPresenter.QuestionNeedsMachineObservation(question, null, null)
            .Should().Be(expected);
        AgentConversationPresenter.QuestionNeedsMachineObservation(
                question,
                null,
                Observation())
            .Should().BeFalse();
    }

    [Fact]
    public void Existing_full_health_evidence_does_not_trigger_another_observation()
    {
        var health = new HealthCheckSummary
        {
            OverallScore = 80,
            Hardware = Observation().Hardware,
            Dimensions =
            [
                new HealthDimensionResult
                {
                    Name = "内存占用",
                    Result = "本次结果",
                    Rating = "正常"
                }
            ],
            KeyFindings = []
        };

        AgentConversationPresenter.QuestionNeedsMachineObservation(
                "电脑为什么卡",
                health,
                null)
            .Should().BeFalse();
        AgentConversationPresenter.QuestionNeedsMachineObservation(
                "电脑配置怎么样",
                health,
                null)
            .Should().BeFalse();
    }

    [Fact]
    public void Lightweight_observation_answers_without_inventing_a_disk_score()
    {
        var observation = Observation();

        var machine = AgentConversationPresenter.Answer(
            "我的内存和电池怎么样",
            null,
            [],
            observation);
        var hardware = AgentConversationPresenter.Answer(
            "我的电脑配置怎么样",
            null,
            [],
            observation);

        machine.Intent.Should().Be(AgentQuestionIntent.MachineHealth);
        machine.Headline.Should().NotContain("需要先");
        machine.EvidenceLines.Should().Contain(line => line.Contains("内存占用") && line.Contains("50%"));
        machine.EvidenceLines.Should().Contain(line => line.Contains("电池状态") && line.Contains("80%"));
        hardware.Intent.Should().Be(AgentQuestionIntent.HardwareInfo);
        hardware.EvidenceLines.Should().Contain(line => line.Contains("Example CPU"));
        hardware.EvidenceLines.Should().Contain(line => line.Contains("Example GPU"));
        string.Join("\n", machine.EvidenceLines.Concat(hardware.EvidenceLines))
            .Should().NotContain("综合评分")
            .And.NotContain(@"C:\")
            .And.NotContain("序列号")
            .And.NotContain("设备 ID");
        machine.NavigationTargetPage.Should().BeNull();
        hardware.NavigationTargetPage.Should().BeNull();
        machine.CanExecuteDirectly.Should().BeFalse();
        hardware.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Hardware_skill_can_use_the_same_lightweight_observation()
    {
        AgentConversationPresenter.SkillNeedsMachineObservation(
                AgentSkillCategory.HardwareInfo,
                null,
                null)
            .Should().BeTrue();

        var reply = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.HardwareInfo,
            null,
            [],
            Observation());

        reply.Headline.Should().Contain("电脑配置");
        reply.EvidenceLines.Should().Contain(line => line.Contains("Example CPU"));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Unavailable_machine_observation_is_not_presented_as_normal()
    {
        var unavailable = new MachineHealthObservation
        {
            ObservedAtUtc = DateTimeOffset.UtcNow,
            SecondaryDrive = new LocalDriveHealthObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            },
            Memory = new MemoryHealthObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            },
            Battery = new BatteryHealthObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            },
            Hardware = new HardwareSummaryObservation
            {
                Availability = MachineMetricAvailability.Available,
                CpuName = "Example CPU"
            }
        };

        var reply = AgentConversationPresenter.Answer(
            "电脑为什么卡",
            null,
            [],
            unavailable);

        reply.Headline.Should().Contain("没有读到")
            .And.NotContain("没有明显警报");
        reply.EvidenceLines.Should().OnlyContain(line => line.Contains("未读取到"));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Agent_machine_observation_is_shared_read_only_and_precedes_the_answer()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var ask = Method(
            code,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");
        var skill = Method(
            code,
            "private async void AgentSkillAction_Click",
            "private async void AgentConversationNavigate_Click");
        var core = Method(
            code,
            "private async Task<bool> RunMachineObservationCoreAsync",
            "private Task EnsureSoftwareInventoryLoadedAsync");
        var fullScan = Method(
            code,
            "private async Task<bool> RunHealthScanCoreAsync",
            "private void SetSoftwareProfiles");

        ask.Should().Contain("QuestionNeedsMachineObservation");
        ask.Should().Contain("await EnsureMachineObservationLoadedAsync()");
        AssertBefore(ask, "await EnsureMachineObservationLoadedAsync()", "AgentConversationPresenter.Answer");
        skill.Should().Contain("SkillNeedsMachineObservation");
        skill.Should().Contain("await EnsureMachineObservationLoadedAsync()");
        core.Should().Contain("new WindowsMachineHealthProbe().Observe()");
        core.Should().NotContain("_scanner.ScanAsync")
            .And.NotContain("ScanSoftwareProfilesAsync")
            .And.NotContain("ex.Message")
            .And.NotContain("Process.Start")
            .And.NotContain(".Kill(")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete");
        fullScan.Should().Contain("await RefreshMachineObservationAsync()")
            .And.NotContain("new WindowsMachineHealthProbe().Observe()");
    }

    private static MachineHealthObservation Observation() => new()
    {
        ObservedAtUtc = DateTimeOffset.UtcNow,
        SecondaryDrive = new LocalDriveHealthObservation
        {
            Availability = MachineMetricAvailability.Available,
            TotalBytes = 500 * GiB,
            FreeBytes = 250 * GiB
        },
        Memory = new MemoryHealthObservation
        {
            Availability = MachineMetricAvailability.Available,
            TotalBytes = 32 * GiB,
            AvailableBytes = 16 * GiB,
            LoadPercent = 50
        },
        Battery = new BatteryHealthObservation
        {
            Availability = MachineMetricAvailability.Available,
            ChargePercent = 80,
            IsOnAcPower = false,
            IsCharging = false
        },
        Hardware = new HardwareSummaryObservation
        {
            Availability = MachineMetricAvailability.Available,
            CpuName = "Example CPU",
            LogicalProcessorCount = 16,
            GpuName = "Example GPU",
            OperatingSystem = "Example Windows",
            Architecture = "X64"
        },
        ProcessCount = 120
    };

    private static string Method(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static void AssertBefore(string source, string first, string second)
    {
        var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
        var secondIndex = source.IndexOf(second, StringComparison.Ordinal);
        firstIndex.Should().BeGreaterThanOrEqualTo(0);
        secondIndex.Should().BeGreaterThan(firstIndex);
    }

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
