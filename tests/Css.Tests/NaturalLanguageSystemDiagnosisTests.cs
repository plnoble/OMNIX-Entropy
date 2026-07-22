using Css.Core.Agent;
using Css.Core.Apps;
using FluentAssertions;

namespace Css.Tests;

public sealed class NaturalLanguageSystemDiagnosisTests
{
    [Theory]
    [InlineData("帮我体检电脑")]
    [InlineData("电脑整体状态怎么样")]
    [InlineData("电脑需要怎么优化")]
    [InlineData("看看电脑健不健康")]
    public void Whole_computer_wording_is_a_distinct_full_diagnosis_intent(string question)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.SystemDiagnosis);
        AgentConversationPresenter.QuestionNeedsFullHealthScan(question, null)
            .Should().BeTrue();
        AgentConversationPresenter.QuestionNeedsSoftwareInventory(question, null)
            .Should().BeFalse();
        AgentConversationPresenter.QuestionNeedsMachineObservation(question, null, null)
            .Should().BeFalse();
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData("电脑为什么卡", AgentQuestionIntent.MachineHealth, false)]
    [InlineData("CPU 和显卡是什么", AgentQuestionIntent.HardwareInfo, false)]
    [InlineData("C盘怎么优化", AgentQuestionIntent.CDrive, true)]
    [InlineData("微信需要怎么优化", AgentQuestionIntent.General, false)]
    public void Nearby_questions_keep_their_narrower_evidence_scope(
        string question,
        AgentQuestionIntent expectedIntent,
        bool needsFullHealth)
    {
        AgentConversationPresenter.Answer(question, null, []).Intent
            .Should().Be(expectedIntent);
        AgentConversationPresenter.QuestionNeedsFullHealthScan(question, null)
            .Should().Be(needsFullHealth);
    }

    [Fact]
    public void Completed_full_diagnosis_answers_from_real_summary_without_another_scan()
    {
        var summary = new HealthCheckSummary
        {
            OverallScore = 77,
            Dimensions =
            [
                new HealthDimensionResult
                {
                    Name = "综合评分",
                    Result = "77 分（当前按磁盘空间）",
                    Rating = "良好，有优化空间"
                },
                new HealthDimensionResult
                {
                    Name = "磁盘健康",
                    Result = "C 盘已使用 73%",
                    Rating = "有优化空间"
                }
            ],
            KeyFindings = []
        };

        var reply = AgentConversationPresenter.Answer("帮我体检电脑", summary, []);

        AgentConversationPresenter.QuestionNeedsFullHealthScan("帮我体检电脑", summary)
            .Should().BeFalse();
        reply.Intent.Should().Be(AgentQuestionIntent.SystemDiagnosis);
        reply.Answer.Should().Contain("77 分");
        reply.EvidenceLines.Should().Contain(line => line.Contains("磁盘健康"));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Main_agent_awaits_the_shared_full_health_gate_before_answering()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var ask = Method(
            code,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");

        ask.Should().Contain("QuestionNeedsFullHealthScan");
        ask.Should().Contain("await EnsureHealthScanLoadedAsync()");
        AssertBefore(ask, "await EnsureHealthScanLoadedAsync()", "AgentConversationPresenter.Answer");
        ask.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("ExecuteSelectedRecommendationAsync")
            .And.NotContain("Process.Start")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete");
    }

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
