using Css.Core.Agent;
using Css.Core.Apps;
using Css.Win32.SystemHealth;
using FluentAssertions;

namespace Css.Tests;

public sealed class HardwareSummaryExperienceTests
{
    [Fact]
    public void Current_hardware_evidence_becomes_a_plain_non_executable_agent_answer()
    {
        var summary = HealthSummary(new HardwareSummaryObservation
        {
            Availability = MachineMetricAvailability.Available,
            CpuName = "Example 8-Core Processor",
            LogicalProcessorCount = 16,
            GpuName = "Example Graphics 7800",
            OperatingSystem = "Microsoft Windows 11 Pro 10.0.26100",
            Architecture = "X64（64 位操作系统）"
        });

        var reply = AgentConversationPresenter.Answer("我的电脑配置怎么样", summary, []);

        reply.Intent.Should().Be(AgentQuestionIntent.HardwareInfo);
        reply.Headline.Should().Contain("电脑配置");
        reply.EvidenceLines.Should().Contain(line => line.Contains("Example 8-Core Processor") && line.Contains("16"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("Example Graphics 7800"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("Windows 11 Pro"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("X64"));
        reply.Answer.Should().Contain("只读");
        string.Join("\n", reply.Answer, string.Join("\n", reply.EvidenceLines), string.Join("\n", reply.NextSteps))
            .Should().Contain("最低配置")
            .And.NotContain(@"C:\")
            .And.NotContain("序列号")
            .And.NotContain("设备 ID");
        reply.NavigationTargetPage.Should().Be("Home");
        reply.CanExecuteDirectly.Should().BeFalse();
        summary.Hardware!.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Missing_hardware_evidence_requests_a_manual_scan_instead_of_guessing()
    {
        var reply = AgentConversationPresenter.Answer("CPU 和显卡是什么", null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.HardwareInfo);
        reply.Answer.Should().Contain("不会猜");
        reply.NavigationTargetPage.Should().Be("Home");
        reply.NavigationLabel.Should().Contain("体检");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Hardware_probe_is_bounded_read_only_and_does_not_query_identifiers()
    {
        var probe = ReadSource("src", "Css.Win32", "SystemHealth", "WindowsHardwareSummaryProbe.cs");
        var model = ReadSource("src", "Css.Core", "Apps", "HardwareSummaryObservation.cs");
        var machine = ReadSource("src", "Css.Win32", "SystemHealth", "WindowsMachineHealthProbe.cs");
        var main = ReadSource("src", "Css.App", "MainWindow.xaml.cs");

        probe.Should().Contain("SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor")
            .And.Contain("SELECT Name FROM Win32_VideoController")
            .And.Contain("SELECT Caption, Version FROM Win32_OperatingSystem")
            .And.Contain("EnumerationOptions")
            .And.Contain("Timeout = TimeSpan.FromSeconds(2)")
            .And.Contain("Take(4)")
            .And.Contain("Registry.GetValue")
            .And.Contain("EnumDisplayDevices")
            .And.NotContain("SerialNumber")
            .And.NotContain("UserName")
            .And.NotContain("Domain")
            .And.NotContain("DeviceID")
            .And.NotContain("PNPDeviceID")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("CreateSubKey")
            .And.NotContain("DeleteSubKey")
            .And.NotContain("File.Write")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete");
        model.Should().Contain("public bool CanExecuteDirectly => false;")
            .And.NotContain("Serial")
            .And.NotContain("User")
            .And.NotContain("DeviceId");
        machine.Should().Contain("Hardware = new WindowsHardwareSummaryProbe().Observe()");
        main.Should().Contain("硬件配置");
    }

    [Fact]
    public void Hardware_gui_smoke_is_read_only_isolated_and_checks_the_first_view()
    {
        var smoke = ReadSource(".omx", "gui-agent-hardware-summary-smoke.ps1");

        smoke.Should().Contain("OMNIX_ENTROPY_CDRIVE_SCAN_ROOT")
            .And.Contain("OMNIX_ENTROPY_DATA_ROOT")
            .And.Contain("AgentConversationHeadlineTextBlock")
            .And.Contain("AgentConversationEvidenceListBox")
            .And.Contain("AgentConversationNavigateButton")
            .And.Contain("pathOrIdentifierExposed = $false")
            .And.Contain("noOperationExecuted = $true")
            .And.Contain("Save-WindowScreenshot $window $screenshot");
        smoke.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("Process.Start(")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Invoke-Element $navigate");
    }

    [Fact]
    public void Real_windows_probe_returns_bounded_path_free_hardware_evidence()
    {
        if (!OperatingSystem.IsWindows())
            return;

        var observation = new WindowsHardwareSummaryProbe().Observe();

        observation.Availability.Should().Be(MachineMetricAvailability.Available);
        observation.CpuName.Should().NotBeNullOrWhiteSpace();
        observation.LogicalProcessorCount.Should().BeInRange(1, 4096);
        observation.GpuName.Should().NotBeNullOrWhiteSpace();
        observation.OperatingSystem.Should().NotBeNullOrWhiteSpace();
        observation.Architecture.Should().NotBeNullOrWhiteSpace();
        foreach (var value in new[]
                 {
                     observation.CpuName,
                     observation.GpuName,
                     observation.OperatingSystem,
                     observation.Architecture
                 })
        {
            value.Should().NotContain(":\\").And.NotStartWith("\\\\");
            value!.Any(char.IsControl).Should().BeFalse();
            value.Length.Should().BeLessThanOrEqualTo(240);
        }

        observation.CanExecuteDirectly.Should().BeFalse();
    }

    private static HealthCheckSummary HealthSummary(HardwareSummaryObservation hardware) =>
        new()
        {
            OverallScore = 80,
            Dimensions = [],
            KeyFindings = [],
            Hardware = hardware
        };

    private static string ReadSource(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "ComputerSecuritySoftware.slnx")))
            current = current.Parent;

        current.Should().NotBeNull();
        return File.ReadAllText(Path.Combine([current!.FullName, .. parts]));
    }
}
