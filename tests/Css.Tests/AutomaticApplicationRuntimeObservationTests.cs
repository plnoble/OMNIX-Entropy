using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticApplicationRuntimeObservationTests
{
    [Theory]
    [InlineData("微信卡死", true)]
    [InlineData("微信无响应", true)]
    [InlineData("微信占内存很高", true)]
    [InlineData("微信内存高", true)]
    [InlineData("微信 CPU 占用高", true)]
    [InlineData("微信 CPU 过高", true)]
    [InlineData("微信资源占用怎么样", true)]
    [InlineData("微信闪退", false)]
    [InlineData("微信最近有点奇怪", false)]
    [InlineData("软件卡死了怎么看", false)]
    [InlineData("帮我卸载微信，它总是卡死", false)]
    public void Only_unique_exact_app_freeze_or_resource_question_has_a_runtime_target(
        string question,
        bool expected)
    {
        var profile = Profile("微信");

        var target = AgentConversationPresenter.ApplicationRuntimeObservationTarget(
            question,
            [profile]);

        if (expected)
            target.Should().BeSameAs(profile);
        else
            target.Should().BeNull();
    }

    [Fact]
    public void Generic_subject_and_ambiguous_profile_never_start_runtime_observation()
    {
        AgentConversationPresenter.ApplicationRuntimeObservationTarget(
                "软件卡死了怎么看",
                [Profile("软件")])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationRuntimeObservationTarget(
                "应用的内存占用很高",
                [Profile("应用")])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationRuntimeObservationTarget(
                "微信卡死",
                [Profile("微信"), Profile("微信")])
            .Should().BeNull();
    }

    [Theory]
    [InlineData("微信占内存很高")]
    [InlineData("微信内存高")]
    [InlineData("微信 CPU 占用高")]
    public void Named_app_resource_question_hydrates_inventory(string question)
    {
        AgentConversationPresenter.QuestionNeedsSoftwareInventory(question, null)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData("软件的内存占用很高")]
    [InlineData("应用的 CPU 过高")]
    [InlineData("电脑内存高")]
    public void Generic_resource_question_does_not_hydrate_application_inventory(string question)
    {
        AgentConversationPresenter.QuestionNeedsSoftwareInventory(question, null)
            .Should().BeFalse();
    }

    [Fact]
    public void Available_runtime_sample_is_plain_aggregate_evidence_not_a_root_cause()
    {
        var observation = Observation(
            ApplicationRuntimeObservationAvailability.Available,
            count: 2,
            bytes: 1_610_612_736,
            cpu: ApplicationCpuActivity.High);

        var reply = AgentConversationPresenter.Answer(
            "微信卡死无响应",
            null,
            [Profile("微信")],
            null,
            null,
            observation);
        var visible = VisibleText(reply);

        visible.Should().Contain("2 个")
            .And.Contain("1.5 GB")
            .And.Contain("较高")
            .And.Contain("短时")
            .And.Contain("不能单独证明")
            .And.NotContain("WeChat.exe")
            .And.NotContain("1234")
            .And.NotContain(@"C:\Private")
            .And.NotContain("直接结束")
            .And.NotContain("强制结束");
        reply.TargetAppName.Should().Be("微信");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData(ApplicationRuntimeObservationAvailability.NotRunning, "重新观察时没有看到它正在运行", "不代表应用一直没有运行")]
    [InlineData(ApplicationRuntimeObservationAvailability.Unavailable, "没有成功读取运行状态", "不会把未知说成正常")]
    public void Runtime_unavailable_states_remain_honest(
        ApplicationRuntimeObservationAvailability availability,
        string expected,
        string limit)
    {
        var reply = AgentConversationPresenter.Answer(
            "微信卡死",
            null,
            [Profile("微信")],
            null,
            null,
            Observation(availability));

        VisibleText(reply).Should().Contain(expected).And.Contain(limit);
    }

    [Fact]
    public void Resource_question_uses_runtime_result_without_irrelevant_crash_log_wording()
    {
        var reply = AgentConversationPresenter.Answer(
            "微信占内存很高",
            null,
            [Profile("微信")],
            null,
            null,
            Observation(
                ApplicationRuntimeObservationAvailability.Available,
                count: 1,
                bytes: 805_306_368,
                cpu: ApplicationCpuActivity.Low));
        var visible = VisibleText(reply);

        reply.Headline.Should().Contain("资源占用");
        visible.Should().Contain("768.0 MB").And.Contain("较低");
        visible.Should().NotContain("崩溃日志").And.NotContain("错误记录");
    }

    [Fact]
    public void Main_observes_runtime_after_inventory_and_before_the_final_answer()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var ask = Method(
            code,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");
        var observe = Method(
            code,
            "private static Task<ApplicationRuntimeObservation> ObserveApplicationRuntimeAsync",
            "private static Task<ApplicationCrashObservation> ObserveApplicationCrashAsync");

        ask.Should().Contain("ApplicationRuntimeObservationTarget")
            .And.Contain("await ObserveApplicationRuntimeAsync")
            .And.Contain("applicationRuntimeObservation");
        AssertBefore(ask, "await EnsureSoftwareInventoryLoadedAsync()", "ApplicationRuntimeObservationTarget");
        AssertBefore(ask, "await ObserveApplicationRuntimeAsync", "AgentConversationPresenter.Answer");
        observe.Should().Contain("new WindowsApplicationRuntimeProbe().Observe(profile)")
            .And.NotContain("Process.Start")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain(".Kill(")
            .And.NotContain("File.Delete")
            .And.NotContain("Registry.SetValue");
    }

    private static ApplicationRuntimeObservation Observation(
        ApplicationRuntimeObservationAvailability availability,
        int count = 0,
        long bytes = 0,
        ApplicationCpuActivity cpu = ApplicationCpuActivity.Unknown) =>
        new()
        {
            Availability = availability,
            SoftwareName = "微信",
            ObservedAtUtc = new DateTimeOffset(2026, 7, 15, 14, 0, 0, TimeSpan.Zero),
            MatchedProcessCount = count,
            TotalWorkingSetBytes = bytes,
            CpuActivity = cpu,
            SampleDurationMilliseconds = 350
        };

    private static SoftwareProfile Profile(string name) => new()
    {
        Name = name,
        InstallPath = @"C:\Private\WeChat",
        DisplayIconPath = @"C:\Private\WeChat\WeChat.exe",
        RunningProcesses = ["WeChat.exe"]
    };

    private static string VisibleText(AgentConversationReply reply) =>
        string.Join(
            "\n",
            new[]
            {
                reply.Headline,
                reply.Answer,
                reply.SafetyBoundary,
                reply.PrivacyLine,
                reply.NavigationLabel ?? string.Empty
            }
            .Concat(reply.EvidenceLines)
            .Concat(reply.NextSteps));

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
