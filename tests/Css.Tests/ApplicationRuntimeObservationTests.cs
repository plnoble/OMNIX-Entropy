using System.Diagnostics;
using Css.Core.Apps;
using Css.Core.Software;
using Css.Win32.SystemHealth;
using FluentAssertions;

namespace Css.Tests;

public sealed class ApplicationRuntimeObservationTests
{
    [Fact]
    public void Probe_reduces_exact_profile_processes_to_bounded_aggregate_facts()
    {
        var now = new DateTimeOffset(2026, 7, 15, 14, 0, 0, TimeSpan.Zero);
        var reader = new FakeReader(new WindowsApplicationRuntimeAggregate
        {
            MatchedProcessCount = 2,
            TotalWorkingSetBytes = 1_610_612_736,
            CpuPercent = 52
        });
        var probe = new WindowsApplicationRuntimeProbe(reader, () => now);

        var observation = probe.Observe(Profile());

        observation.Availability.Should().Be(ApplicationRuntimeObservationAvailability.Available);
        observation.SoftwareName.Should().Be("微信");
        observation.ObservedAtUtc.Should().Be(now);
        observation.MatchedProcessCount.Should().Be(2);
        observation.TotalWorkingSetBytes.Should().Be(1_610_612_736);
        observation.CpuActivity.Should().Be(ApplicationCpuActivity.High);
        observation.SampleDurationMilliseconds.Should().Be(350);
        observation.CanExecuteDirectly.Should().BeFalse();
        reader.LastNames.Should().BeEquivalentTo("wechat", "wechatagent");
        reader.LastDuration.Should().Be(TimeSpan.FromMilliseconds(350));
        reader.LastMaximumProcesses.Should().Be(WindowsApplicationRuntimeProbe.MaximumMatchingProcesses);
    }

    [Fact]
    public void Probe_preserves_not_running_unavailable_and_untrusted_identity_states()
    {
        var now = new DateTimeOffset(2026, 7, 15, 14, 0, 0, TimeSpan.Zero);
        var notRunning = new WindowsApplicationRuntimeProbe(
            new FakeReader(new WindowsApplicationRuntimeAggregate()),
            () => now)
            .Observe(Profile());
        var failed = new WindowsApplicationRuntimeProbe(
            new ThrowingReader(),
            () => now)
            .Observe(Profile());
        var neverCalled = new FakeReader(new WindowsApplicationRuntimeAggregate
        {
            MatchedProcessCount = 1
        });
        var untrustedIdentity = new WindowsApplicationRuntimeProbe(neverCalled, () => now)
            .Observe(new SoftwareProfile
            {
                Name = "微信",
                RunningProcesses = ["app", "x.exe"]
            });

        notRunning.Availability.Should().Be(ApplicationRuntimeObservationAvailability.NotRunning);
        notRunning.MatchedProcessCount.Should().Be(0);
        failed.Availability.Should().Be(ApplicationRuntimeObservationAvailability.Unavailable);
        untrustedIdentity.Availability.Should().Be(ApplicationRuntimeObservationAvailability.Unavailable);
        neverCalled.CallCount.Should().Be(0);
    }

    [Fact]
    public void Reader_and_core_model_keep_process_identity_and_authority_outside_the_result()
    {
        var reader = Read("src", "Css.Win32", "SystemHealth", "WindowsApplicationRuntimeProbe.cs");
        var model = Read("src", "Css.Core", "Apps", "ApplicationRuntimeObservation.cs");

        reader.Should().Contain("Process.GetProcesses()")
            .And.Contain("TotalProcessorTime")
            .And.Contain("WorkingSet64")
            .And.Contain("Thread.Sleep")
            .And.Contain("MaximumMatchingProcesses")
            .And.NotContain("MainModule")
            .And.NotContain("CommandLine")
            .And.NotContain(".Kill(")
            .And.NotContain("CloseMainWindow")
            .And.NotContain("PriorityClass")
            .And.NotContain("Process.Start")
            .And.NotContain("Suspend")
            .And.NotContain("Terminate")
            .And.NotContain("File.Delete")
            .And.NotContain("Registry.SetValue");
        model.Should().Contain("public bool CanExecuteDirectly => false;")
            .And.NotContain("ProcessName")
            .And.NotContain("ProcessId")
            .And.NotContain("CommandLine")
            .And.NotContain("Executable")
            .And.NotContain("Path")
            .And.NotContain("CpuPercent");
    }

    [Fact]
    public void Real_reader_observes_the_current_test_host_as_aggregate_only()
    {
        using var current = Process.GetCurrentProcess();
        var observation = new WindowsApplicationRuntimeProbe().Observe(new SoftwareProfile
        {
            Name = "测试宿主",
            RunningProcesses = [current.ProcessName]
        });

        observation.Availability.Should().Be(ApplicationRuntimeObservationAvailability.Available);
        observation.MatchedProcessCount.Should().BeGreaterThan(0);
        observation.TotalWorkingSetBytes.Should().BeGreaterThan(0);
        observation.SampleDurationMilliseconds.Should().Be(350);
        observation.CanExecuteDirectly.Should().BeFalse();
    }

    private static SoftwareProfile Profile() => new()
    {
        Name = "微信",
        DisplayIconPath = @"C:\Software\WeChat\WeChat.exe",
        RunningProcesses = ["WeChat.exe", "WeChatAgent", "software", "x.exe"]
    };

    private sealed class FakeReader(WindowsApplicationRuntimeAggregate result)
        : IWindowsApplicationRuntimeReader
    {
        public int CallCount { get; private set; }
        public IReadOnlySet<string> LastNames { get; private set; } = new HashSet<string>();
        public TimeSpan LastDuration { get; private set; }
        public int LastMaximumProcesses { get; private set; }

        public WindowsApplicationRuntimeAggregate Read(
            IReadOnlySet<string> exactProcessNames,
            TimeSpan sampleDuration,
            int maximumProcesses)
        {
            CallCount++;
            LastNames = exactProcessNames;
            LastDuration = sampleDuration;
            LastMaximumProcesses = maximumProcesses;
            return result;
        }
    }

    private sealed class ThrowingReader : IWindowsApplicationRuntimeReader
    {
        public WindowsApplicationRuntimeAggregate Read(
            IReadOnlySet<string> exactProcessNames,
            TimeSpan sampleDuration,
            int maximumProcesses) =>
            throw new InvalidOperationException("private fixture failure");
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
