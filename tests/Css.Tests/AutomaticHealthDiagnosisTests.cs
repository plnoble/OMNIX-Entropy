using Css.App;
using Css.Core.Agent;
using Css.Core.Apps;
using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticHealthDiagnosisTests
{
    [Fact]
    public async Task Shared_evidence_gate_deduplicates_inflight_work_and_retries_failures()
    {
        var gate = new ReadOnlyEvidenceLoadGate();
        var release = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;

        Task<bool> Load()
        {
            calls++;
            return release.Task;
        }

        var first = gate.EnsureLoadedAsync(Load);
        var second = gate.EnsureLoadedAsync(Load);
        first.Should().BeSameAs(second);
        calls.Should().Be(1);
        release.SetResult(true);
        await first;

        await gate.EnsureLoadedAsync(Load);
        calls.Should().Be(1);

        var retryGate = new ReadOnlyEvidenceLoadGate();
        var retryCalls = 0;
        await retryGate.EnsureLoadedAsync(() => Task.FromResult(++retryCalls > 1));
        await retryGate.EnsureLoadedAsync(() => Task.FromResult(++retryCalls > 1));
        retryCalls.Should().Be(2);
    }

    [Fact]
    public async Task Manual_refresh_forces_new_work_but_joins_an_existing_scan()
    {
        var gate = new ReadOnlyEvidenceLoadGate();
        var calls = 0;
        await gate.EnsureLoadedAsync(() => Task.FromResult(++calls > 0));
        await gate.RefreshAsync(() => Task.FromResult(++calls > 0));
        calls.Should().Be(2);

        var release = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var inFlightCalls = 0;
        var inFlight = new ReadOnlyEvidenceLoadGate();
        var first = inFlight.RefreshAsync(() =>
        {
            inFlightCalls++;
            return release.Task;
        });
        var second = inFlight.RefreshAsync(() =>
        {
            inFlightCalls++;
            return release.Task;
        });
        first.Should().BeSameAs(second);
        inFlightCalls.Should().Be(1);
        release.SetResult(true);
        await first;
    }

    [Theory]
    [InlineData("C盘为什么总是满", true)]
    [InlineData("哪些垃圾占了系统盘", true)]
    [InlineData("帮我体检电脑", true)]
    [InlineData("电脑整体状态怎么样", true)]
    [InlineData("电脑为什么卡", false)]
    [InlineData("CPU 和显卡是什么", false)]
    [InlineData("Wi-Fi 在哪里设置", false)]
    [InlineData("哪些软件会开机启动", false)]
    public void Only_missing_C_drive_or_whole_computer_evidence_requests_automatic_full_diagnosis(
        string question,
        bool expected)
    {
        AgentConversationPresenter.QuestionNeedsFullHealthScan(question, null)
            .Should().Be(expected);
        AgentConversationPresenter.QuestionNeedsFullHealthScan(question, ExistingSummary())
            .Should().BeFalse();
    }

    [Fact]
    public void Home_and_agent_share_the_read_only_scan_before_answering()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var start = Method(
            code,
            "private async void StartScan_Click",
            "private void InstallRoutingMemoryListBox_SelectionChanged");
        var ask = Method(
            code,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");
        var core = Method(
            code,
            "private async Task<bool> RunHealthScanCoreAsync",
            "private void SetSoftwareProfiles");

        start.Should().Contain("await RefreshHealthScanAsync()");
        ask.Should().Contain("QuestionNeedsFullHealthScan");
        ask.Should().Contain("await EnsureHealthScanLoadedAsync()");
        AssertBefore(ask, "await EnsureHealthScanLoadedAsync()", "AgentConversationPresenter.Answer");
        core.Should().Contain("_scanner.ScanAsync");
        core.Should().NotContain("ex.Message");
        string.Join("\n", start, ask)
            .Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("ExecuteSelectedRecommendationAsync")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Move")
            .And.NotContain("Directory.Delete");
    }

    private static HealthCheckSummary ExistingSummary() => new()
    {
        OverallScore = 80,
        Dimensions = [],
        KeyFindings = []
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
