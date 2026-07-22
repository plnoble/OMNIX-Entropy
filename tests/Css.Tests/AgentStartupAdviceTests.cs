using Css.Core.Agent;
using Css.Core.Software;
using Css.Core.Startup;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentStartupAdviceTests
{
    [Fact]
    public void Exact_application_question_offers_local_review_but_not_execution()
    {
        var profile = EligibleProfile("Fixture App");

        var reply = AgentConversationPresenter.Answer(
            "帮我关闭 Fixture App 的开机自启动",
            null,
            [profile]);

        reply.Intent.Should().Be(AgentQuestionIntent.ApplicationSpecific);
        reply.TargetAppName.Should().Be("Fixture App");
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.Answer.Should().Contain("OMNIX").And.Contain("可还原");
        reply.Answer.Should().Contain("重新读取");
        reply.NextSteps.Should().Contain(step => step.Contains("审核关闭方案"));
        reply.CanExecuteDirectly.Should().BeFalse();
        Visible(reply).Should().NotContain("HKCU");
        Visible(reply).Should().NotContain(profile.BackgroundComponents[0].Identity.StableId);
    }

    [Fact]
    public void Aggregate_answer_counts_local_review_separately_from_name_only_and_service_signals()
    {
        var profiles = new[]
        {
            EligibleProfile("Eligible"),
            new SoftwareProfile { Name = "Name Only", StartupEntries = ["Name Only"] },
            new SoftwareProfile { Name = "Service App", Services = ["ServiceAppSvr"] }
        };

        var reply = AgentConversationPresenter.Answer("哪些应用会开机启动", null, profiles);

        reply.Answer.Should().Contain("2 个应用有普通自启动线索");
        reply.Answer.Should().Contain("1 个普通应用具备本地审核线索");
        reply.Answer.Should().Contain("1 个应用带服务或计划任务");
        reply.EvidenceLines.Should().Contain(line => line.Contains("可审核") && line.Contains("Eligible"));
        reply.NextSteps.Should().Contain(step => step.Contains("审核关闭方案"));
        reply.NextSteps.Should().Contain(step => step.Contains("服务") && step.Contains("计划任务"));
        reply.CanExecuteDirectly.Should().BeFalse();
        Visible(reply).Should().NotContain("ServiceAppSvr");
    }

    [Fact]
    public void Background_review_and_plan_preview_explain_the_narrow_local_capability()
    {
        var profile = EligibleProfile("Eligible");
        var service = new SoftwareProfile
        {
            Name = "Service App",
            Services = ["ServiceAppSvr"],
            ScheduledTasks = ["ServiceAppTask"]
        };

        var review = AgentBackgroundReviewPresenter.Create([profile, service]);
        var plan = AgentStartupServicePlanPresenter.Create([profile, service]);

        review.Items.Should().Contain(item =>
            item.AppName == "Eligible"
            && item.RecommendedNextStep.Contains("审核关闭方案"));
        review.Items.Should().OnlyContain(item => !item.CanExecuteDirectly);
        plan.Summary.Should().Contain("1 个普通自启动项可尝试本地审核");
        plan.EvidenceLines.Should().Contain(line => line.Contains("本地审核") && line.Contains("1 个"));
        plan.PlanSteps.Should().Contain(step => step.Contains("应用详情") && step.Contains("管理自启动"));
        plan.RequiredBeforeExecution.Should().Contain(item => item.Contains("重新读取") && item.Contains("回滚"));
        plan.BlockedActions.Should().Contain(item => item.Contains("服务") && item.Contains("计划任务"));
        plan.BlockedActions.Should().Contain(item => item.Contains("批量") && item.Contains("自启动"));
        plan.CanExecuteDirectly.Should().BeFalse();
        string.Join("\n", plan.EvidenceLines.Concat(plan.PlanSteps).Concat(plan.BlockedActions))
            .Should().NotContain("ServiceAppSvr").And.NotContain("ServiceAppTask").And.NotContain("HKCU");
    }

    [Fact]
    public void Unsupported_multiple_and_system_profiles_never_gain_local_review_advice()
    {
        var unsupported = new SoftwareProfile
        {
            Name = "Unsupported",
            Category = SoftwareCategory.Normal,
            BackgroundComponents =
            [
                StartupObservation("Unsupported One"),
                StartupObservation("Unsupported Two")
            ],
            StartupEntries = ["Unsupported One", "Unsupported Two"]
        };
        var system = new SoftwareProfile
        {
            Name = "Driver Center",
            Category = SoftwareCategory.SystemTool,
            StartupEntries = ["Driver Center Startup"],
            BackgroundComponents = [StartupObservation("Driver Center Startup")]
        };

        StartupEntryControlPolicy.HasSingleSupportedObservation(unsupported).Should().BeFalse();
        StartupEntryControlPolicy.HasSingleSupportedObservation(system).Should().BeFalse();
        var unsupportedReply = AgentConversationPresenter.Answer(
            "关闭 Unsupported 自启动", null, [unsupported]);
        var systemReply = AgentConversationPresenter.Answer(
            "关闭 Driver Center 自启动", null, [system]);
        unsupportedReply.Answer.Should().NotContain("可还原的关闭方案");
        systemReply.Answer.Should().Contain("系统相关");
        unsupportedReply.CanExecuteDirectly.Should().BeFalse();
        systemReply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Agent_startup_advice_gui_smoke_is_fixture_only_and_navigation_only()
    {
        var root = FindRepositoryRoot();
        var smoke = File.ReadAllText(Path.Combine(root, ".omx", "gui-agent-startup-advice-smoke.ps1"));

        smoke.Should().Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE");
        smoke.Should().Contain("AgentQuestionTextBox");
        smoke.Should().Contain("AgentConversationAnswerTextBlock");
        smoke.Should().Contain("Get-UnicodeText");
        smoke.Should().Contain("0x5BA1, 0x6838, 0x5173, 0x95ED, 0x65B9, 0x6848");
        smoke.Should().Contain("noOperationExecuted = $true");
        smoke.Should().NotContain("OMNIX_ENTROPY_STARTUP_FIXTURE");
        smoke.Should().NotContain("StartupConfirmationConfirmButton");
        smoke.Should().NotContain("Registry.SetValue");
    }

    private static SoftwareProfile EligibleProfile(string name) =>
        new()
        {
            Name = name,
            Category = SoftwareCategory.Normal,
            StartupEntries = [name + " Startup"],
            BackgroundComponents = [StartupObservation(name + " Startup")]
        };

    private static BackgroundComponentObservation StartupObservation(string valueName)
    {
        var now = DateTimeOffset.UtcNow;
        return BackgroundComponentObservationFactory.Startup(
            valueName,
            StartupEntryControlPolicy.SupportedSourceLocator,
            "fixture.exe --background",
            now,
            StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                valueName,
                new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
    }

    private static string Visible(AgentConversationReply reply) =>
        string.Join("\n", new[] { reply.Headline, reply.Answer, reply.SafetyBoundary, reply.PrivacyLine }
            .Concat(reply.EvidenceLines)
            .Concat(reply.NextSteps));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "ComputerSecuritySoftware.slnx")))
                return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
