using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticApplicationCrashObservationTests
{
    [Fact]
    public void Only_unique_exact_app_failure_question_has_an_observation_target()
    {
        var profile = Profile("微信");

        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "微信闪退",
                [profile])
            .Should().BeSameAs(profile);
        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "微信卡死",
                [profile])
            .Should().BeSameAs(profile);
        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "帮我卸载微信，它总闪退",
                [profile])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "软件闪退了怎么看",
                [profile])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "软件闪退了怎么看",
                [Profile("软件")])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "微信最近有点奇怪",
                [profile])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationCrashObservationTarget(
                "微信闪退",
                [profile, Profile("微信")])
            .Should().BeNull();
    }

    [Fact]
    public void Freeze_evidence_uses_the_freeze_symptom_instead_of_crash_wording()
    {
        var observation = new ApplicationCrashObservation
        {
            Availability = ApplicationCrashObservationAvailability.NotFound,
            SoftwareName = "微信",
            ObservedAtUtc = DateTimeOffset.UtcNow,
            WindowStartUtc = DateTimeOffset.UtcNow.AddHours(-24)
        };

        var reply = AgentConversationPresenter.Answer(
            "微信卡死",
            null,
            [Profile("微信")],
            null,
            observation);

        string.Join("\n", reply.EvidenceLines)
            .Should().Contain("不代表没有发生卡死或无响应")
            .And.NotContain("不代表没有发生闪退");
    }

    [Theory]
    [InlineData(ApplicationCrashObservationAvailability.Available, "找到 2 条", "不能单独证明根因")]
    [InlineData(ApplicationCrashObservationAvailability.NotFound, "没有找到匹配记录", "不代表没有发生")]
    [InlineData(ApplicationCrashObservationAvailability.Unavailable, "没有成功读取", "不会把未知说成正常")]
    public void Agent_presents_crash_observation_without_private_event_content(
        ApplicationCrashObservationAvailability availability,
        string expected,
        string evidenceLimit)
    {
        var observation = new ApplicationCrashObservation
        {
            Availability = availability,
            SoftwareName = "微信",
            ObservedAtUtc = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero),
            WindowStartUtc = new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero),
            MatchCount = availability == ApplicationCrashObservationAvailability.Available ? 2 : 0,
            LatestOccurrenceUtc = availability == ApplicationCrashObservationAvailability.Available
                ? new DateTimeOffset(2026, 7, 15, 10, 30, 0, TimeSpan.Zero)
                : null
        };

        var reply = AgentConversationPresenter.Answer(
            "微信闪退",
            null,
            [Profile("微信")],
            null,
            observation);
        var visible = string.Join("\n", reply.EvidenceLines.Prepend(reply.Answer));

        visible.Should().Contain(expected).And.Contain(evidenceLimit);
        visible.Should().NotContain(@"C:\")
            .And.NotContain("WeChat.exe")
            .And.NotContain("Application Error")
            .And.NotContain("EventID");
        reply.TargetAppName.Should().Be("微信");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Main_observes_after_inventory_and_before_the_final_answer()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var ask = Method(
            code,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");
        var observe = Method(
            code,
            "private static Task<ApplicationCrashObservation> ObserveApplicationCrashAsync",
            "private Task EnsureMachineObservationLoadedAsync");

        ask.Should().Contain("ApplicationCrashObservationTarget");
        ask.Should().Contain("await ObserveApplicationCrashAsync");
        ask.Should().Contain("applicationCrashObservation");
        AssertBefore(ask, "await EnsureSoftwareInventoryLoadedAsync()", "ApplicationCrashObservationTarget");
        AssertBefore(ask, "await ObserveApplicationCrashAsync", "AgentConversationPresenter.Answer");
        observe.Should().Contain("new WindowsApplicationCrashProbe().Observe(profile)")
            .And.NotContain("Process.Start")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("EventLog.Clear")
            .And.NotContain("File.Delete")
            .And.NotContain("Registry.SetValue");
    }

    private static SoftwareProfile Profile(string name) => new()
    {
        Name = name,
        DisplayIconPath = @"C:\Software\WeChat\WeChat.exe"
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
