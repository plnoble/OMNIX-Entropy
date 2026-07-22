using Css.Core.Agent;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class ApplicationTroubleshootingAgentTests
{
    [Theory]
    [InlineData("微信闪退", true)]
    [InlineData("最近微信总是闪退", true)]
    [InlineData("Chrome 闪退", true)]
    [InlineData("软件闪退了怎么看", false)]
    [InlineData("应用闪退怎么办", false)]
    [InlineData("电脑蓝屏了怎么看", false)]
    public void Only_likely_named_app_crashes_hydrate_inventory(
        string question,
        bool expected)
    {
        AgentConversationPresenter.QuestionNeedsSoftwareInventory(question, null)
            .Should().Be(expected);
    }

    [Fact]
    public void Exact_app_crash_answer_names_missing_evidence_and_keeps_app_handoff()
    {
        var profile = Profile(
            runningProcesses: ["WeChat.exe"],
            startupEntries: ["WeChat Startup"],
            services: ["WeChatService"],
            scheduledTasks: ["WeChatUpdate"]);

        var reply = AgentConversationPresenter.Answer("微信总是闪退", null, [profile]);
        var visible = VisibleText(reply);

        reply.Intent.Should().Be(AgentQuestionIntent.ApplicationSpecific);
        reply.Headline.Should().Contain("微信").And.Contain("闪退");
        reply.Answer.Should().Contain("没有崩溃日志").And.Contain("不能判断根因");
        reply.EvidenceLines.Should().Contain(line => line.Contains("1 个正在运行"));
        reply.EvidenceLines.Should().Contain(line =>
            line.Contains("1 项自启动")
            && line.Contains("1 项服务")
            && line.Contains("1 项计划任务"));
        reply.NextSteps.Should().Contain(line => line.Contains("事件查看器") && line.Contains("只查看"));
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.TargetAppName.Should().Be("微信");
        reply.ShortcutKind.Should().BeNull();
        reply.CanExecuteDirectly.Should().BeFalse();
        visible.Should().NotContain("WeChat.exe")
            .And.NotContain("WeChatService")
            .And.NotContain("WeChatUpdate")
            .And.NotContain(@"C:\Private");
    }

    [Fact]
    public void Exact_app_freeze_answer_does_not_invent_cpu_or_force_end_a_process()
    {
        var reply = AgentConversationPresenter.Answer(
            "微信卡死无响应",
            null,
            [Profile(runningProcesses: ["WeChat.exe"])]);
        var visible = VisibleText(reply);

        reply.Headline.Should().Contain("无响应");
        reply.Answer.Should().Contain("不能判断")
            .And.NotContain("CPU 过高")
            .And.NotContain("内存不足");
        reply.NextSteps.Should().Contain(line =>
            line.Contains("任务管理器")
            && line.Contains("不会结束进程"));
        visible.Should().NotContain("WeChat.exe")
            .And.NotContain("强制结束")
            .And.NotContain("直接结束");
        reply.TargetAppName.Should().Be("微信");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Vague_app_problem_asks_for_a_symptom_instead_of_guessing()
    {
        var reply = AgentConversationPresenter.Answer(
            "微信最近有点奇怪",
            null,
            [Profile()]);

        reply.Headline.Should().Contain("还需要").And.Contain("微信");
        reply.Answer.Should().Contain("闪退").And.Contain("卡死").And.Contain("报错");
        reply.Answer.Should().Contain("不能判断");
        reply.TargetAppName.Should().Be("微信");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Explicit_uninstall_request_still_wins_over_crash_explanation()
    {
        var reply = AgentConversationPresenter.Answer(
            "帮我卸载微信，它总闪退",
            null,
            [Profile(uninstallCommand: "uninstall.exe")]);

        reply.Answer.Should().Contain("官方卸载入口")
            .And.NotContain("没有崩溃日志");
        reply.TargetAppName.Should().Be("微信");
    }

    private static SoftwareProfile Profile(
        IReadOnlyList<string>? runningProcesses = null,
        IReadOnlyList<string>? startupEntries = null,
        IReadOnlyList<string>? services = null,
        IReadOnlyList<string>? scheduledTasks = null,
        string? uninstallCommand = null) =>
        new()
        {
            Name = "微信",
            InstallPath = @"C:\Private\WeChat",
            RunningProcesses = runningProcesses ?? [],
            StartupEntries = startupEntries ?? [],
            Services = services ?? [],
            ScheduledTasks = scheduledTasks ?? [],
            UninstallCommand = uninstallCommand
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
}
