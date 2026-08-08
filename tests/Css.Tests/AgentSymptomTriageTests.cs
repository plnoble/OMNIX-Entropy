using Css.Core.Agent;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentSymptomTriageTests
{
    [Fact]
    public void Beginner_quick_choices_are_bounded_unique_and_use_allowlisted_destinations()
    {
        var prompts = AgentSymptomPromptCatalog.CreateDefault();

        prompts.Should().HaveCount(6);
        prompts.Select(item => item.Id).Should().OnlyHaveUniqueItems();
        prompts.Select(item => item.Label).Should().OnlyHaveUniqueItems();
        prompts.Select(item => item.Question).Should().OnlyHaveUniqueItems();
        prompts.Should().OnlyContain(item =>
            item.Id.Length <= 32
            && item.Label.Length > 0
            && item.Label.Length <= 12
            && item.Question.Length > 0
            && item.Question.Length <= 40);

        foreach (var prompt in prompts)
        {
            var reply = AgentConversationPresenter.Answer(prompt.Question, null, []);

            reply.Intent.Should().Be(AgentQuestionIntent.Troubleshooting);
            reply.ShortcutId.Should().NotBeNullOrWhiteSpace();
            reply.ShortcutKind.Should().NotBeNull();
            ResolveShortcut(reply).Should().BeTrue();
            reply.CanExecuteDirectly.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("电脑连不上 Wi-Fi", AgentShortcutKind.WindowsSettings, "network", "建议先检查")]
    [InlineData("电脑突然没有声音", AgentShortcutKind.WindowsSettings, "sound", "建议先检查")]
    [InlineData("蓝牙耳机一直连不上", AgentShortcutKind.WindowsSettings, "bluetooth", "建议先检查")]
    [InlineData("显示器一直闪屏", AgentShortcutKind.WindowsSettings, "display", "需要尽快查看")]
    [InlineData("显示器闪烁", AgentShortcutKind.WindowsSettings, "display", "需要尽快查看")]
    [InlineData("打开电脑后黑屏", AgentShortcutKind.WindowsSettings, "display", "需要尽快查看")]
    [InlineData("打开应用后突然没有声音", AgentShortcutKind.WindowsSettings, "sound", "建议先检查")]
    [InlineData("设备驱动显示异常", AgentShortcutKind.SystemTool, "device-manager", "需要尽快查看")]
    [InlineData("电脑刚才蓝屏并自动重启", AgentShortcutKind.SystemTool, "event-viewer", "较紧急")]
    public void Symptom_answer_explains_checked_unknown_urgency_and_one_next_step(
        string question,
        AgentShortcutKind shortcutKind,
        string shortcutId,
        string urgency)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);
        var visible = VisibleText(reply);

        reply.Intent.Should().Be(AgentQuestionIntent.Troubleshooting);
        reply.ShortcutKind.Should().Be(shortcutKind);
        reply.ShortcutId.Should().Be(shortcutId);
        reply.EvidenceLines.Should().ContainSingle(line => line.StartsWith("已检查："));
        reply.EvidenceLines.Should().ContainSingle(line => line.StartsWith("仍未知："));
        reply.EvidenceLines.Should().ContainSingle(line => line.StartsWith("紧急程度：") && line.Contains(urgency));
        reply.NextSteps.Should().ContainSingle();
        reply.NavigationLabel.Should().StartWith("打开");
        reply.CanNavigate.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
        reply.UsedCloudAi.Should().BeFalse();
        visible.Should().Contain("不会自动")
            .And.NotContain("ms-settings:")
            .And.NotContain("eventvwr.msc")
            .And.NotContain("devmgmt.msc");
    }

    [Theory]
    [InlineData("打开网络设置", AgentQuestionIntent.WindowsSettings, "network")]
    [InlineData("打开声音设置", AgentQuestionIntent.WindowsSettings, "sound")]
    [InlineData("打开设备管理器", AgentQuestionIntent.Troubleshooting, "device-manager")]
    [InlineData("打开事件查看器", AgentQuestionIntent.Troubleshooting, "event-viewer")]
    public void Explicit_open_request_keeps_the_existing_shortcut_route(
        string question,
        AgentQuestionIntent intent,
        string shortcutId)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.Intent.Should().Be(intent);
        reply.ShortcutId.Should().Be(shortcutId);
        reply.EvidenceLines.Should().NotContain(line => line.StartsWith("紧急程度："));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Negated_blue_screen_uses_the_remaining_display_symptom()
    {
        var reply = AgentConversationPresenter.Answer("不是蓝屏，只是显示器闪烁", null, []);

        reply.ShortcutKind.Should().Be(AgentShortcutKind.WindowsSettings);
        reply.ShortcutId.Should().Be("display");
        reply.Headline.Should().Contain("屏幕异常");
        reply.EvidenceLines.Should().Contain(line => line.StartsWith("紧急程度："));
    }

    [Theory]
    [InlineData("不是黑屏，是蓝牙耳机连不上", "bluetooth")]
    [InlineData("蓝屏倒是没有，电脑就是没声音", "sound")]
    [InlineData("驱动没有异常，只是网络连不上", "network")]
    [InlineData("网络没有断开，只是显示器闪烁", "display")]
    public void Negated_symptom_does_not_steal_the_affirmed_problem(
        string question,
        string shortcutId)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.ShortcutId.Should().Be(shortcutId);
        reply.EvidenceLines.Should().Contain(line => line.StartsWith("紧急程度："));
    }

    [Fact]
    public void Negated_blue_screen_does_not_hide_a_real_restart_in_another_clause()
    {
        var reply = AgentConversationPresenter.Answer("没有蓝屏，但电脑突然重启", null, []);

        reply.ShortcutId.Should().Be("event-viewer");
        reply.EvidenceLines.Should().Contain(line => line.Contains("较紧急"));
    }

    [Fact]
    public void Purely_negated_blue_screen_does_not_open_event_viewer()
    {
        var reply = AgentConversationPresenter.Answer("没有蓝屏", null, []);

        reply.ShortcutId.Should().NotBe("event-viewer");
        reply.EvidenceLines.Should().NotContain(line => line.StartsWith("紧急程度："));
    }

    [Theory]
    [InlineData("电脑支持蓝牙吗")]
    [InlineData("电脑有没有蓝牙")]
    public void Informational_bluetooth_question_does_not_invent_a_connection_failure(string question)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.EvidenceLines.Should().NotContain(line => line.StartsWith("已检查：") && line.Contains("连接问题"));
        reply.Headline.Should().NotContain("蓝牙或外设问题");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData("查看蓝牙设置", "bluetooth")]
    [InlineData("帮我打开一下网络设置", "network")]
    public void Nearby_action_and_specific_destination_are_treated_as_explicit_navigation(
        string question,
        string shortcutId)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.ShortcutId.Should().Be(shortcutId);
        reply.EvidenceLines.Should().NotContain(line => line.StartsWith("紧急程度："));
    }

    [Fact]
    public void Unrelated_open_and_setting_words_do_not_suppress_a_real_symptom()
    {
        var reply = AgentConversationPresenter.Answer("打开电脑后黑屏，设置没动过", null, []);

        reply.ShortcutId.Should().Be("display");
        reply.EvidenceLines.Should().Contain(line => line.StartsWith("紧急程度："));
    }

    [Fact]
    public void Peripheral_and_network_terms_must_share_the_same_failure_clause()
    {
        var reply = AgentConversationPresenter.Answer("蓝牙正常，但 Wi-Fi 连不上", null, []);

        reply.ShortcutId.Should().Be("network");
        reply.Headline.Should().Contain("网络问题");
    }

    [Fact]
    public void Contrast_word_without_punctuation_separates_bluetooth_state_from_network_failure()
    {
        var reply = AgentConversationPresenter.Answer("蓝牙正常但 Wi-Fi 连不上", null, []);

        reply.ShortcutId.Should().Be("network");
        reply.Headline.Should().Contain("网络问题");
    }

    [Fact]
    public void Urgent_system_symptom_wins_over_a_loaded_application_name()
    {
        var profiles = new[]
        {
            new SoftwareProfile { Name = "Steam", InstallPath = @"D:\Game\Steam" }
        };

        var reply = AgentConversationPresenter.Answer(
            "玩 Steam 时电脑蓝屏",
            null,
            profiles);

        reply.Intent.Should().Be(AgentQuestionIntent.Troubleshooting);
        reply.ShortcutId.Should().Be("event-viewer");
        reply.EvidenceLines.Should().Contain(line => line.Contains("较紧急"));
    }

    [Fact]
    public void Past_setting_statement_does_not_suppress_a_display_symptom()
    {
        var reply = AgentConversationPresenter.Answer("我没怎么设置过，显示器闪烁", null, []);

        reply.ShortcutId.Should().Be("display");
        reply.EvidenceLines.Should().Contain(line => line.StartsWith("紧急程度："));
    }

    [Fact]
    public void Agent_page_places_quick_symptoms_before_free_form_question_and_keeps_them_navigation_only()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var quickChoices = xaml.IndexOf(
            "x:Name=\"AgentSymptomQuickChoicesItemsControl\"",
            StringComparison.Ordinal);
        var question = xaml.IndexOf(
            "x:Name=\"AgentQuestionTextBox\"",
            StringComparison.Ordinal);

        quickChoices.Should().BeGreaterThanOrEqualTo(0);
        quickChoices.Should().BeLessThan(question);
        xaml.Should().Contain("AgentSymptomQuickChoice_{0}")
            .And.Contain("Click=\"AgentSymptomQuickChoice_Click\"");
        main.Should().Contain("AgentSymptomPromptCatalog.CreateDefault()")
            .And.Contain("RunComputerAgentQuestionAsync(question)");

        var handler = SourceMethodExtractor.Extract(
            main,
            "private async void AgentSymptomQuickChoice_Click(object sender, RoutedEventArgs e)");
        handler.Should().Contain("RunComputerAgentQuestionAsync")
            .And.NotContain("Process.Start")
            .And.NotContain("OpenAllowlistedSystemTool")
            .And.NotContain("OpenAllowlistedWindowsSettings")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("Registry")
            .And.NotContain("File.")
            .And.NotContain("Directory.");
    }

    [Fact]
    public void Symptom_triage_gui_smoke_proves_first_view_conclusion_without_launching_the_tool()
    {
        var smoke = Read(".omx", "gui-agent-symptom-triage-smoke.ps1");

        smoke.Should().Contain("AgentSymptomQuickChoice_blue-screen")
            .And.Contain("AgentConversationHeadlineTextBlock")
            .And.Contain("AgentConversationEvidenceListBox")
            .And.Contain("AgentConversationNextStepsListBox")
            .And.Contain("Require-FullyVisibleElement")
            .And.Contain("allQuickChoicesFirstView = $true")
            .And.Contain("checkedUnknownUrgencyVisible = $true")
            .And.Contain("exactlyOneNextStepVisible = $true")
            .And.Contain("externalToolStarted = $false")
            .And.Contain("noOperationExecuted = ($quarantineManifestCount -eq 0)")
            .And.Contain("Assert-NonBlankScreenshot $screenshot");
        smoke.Should().NotContain("Invoke-Element $navigate")
            .And.NotContain("Start-Process eventvwr")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete");
    }

    private static bool ResolveShortcut(AgentConversationReply reply) =>
        reply.ShortcutKind switch
        {
            AgentShortcutKind.WindowsSettings =>
                WindowsSettingsShortcutCatalog.FindById(reply.ShortcutId!)?.IsOpenOnly == true,
            AgentShortcutKind.SystemTool =>
                SystemToolShortcutCatalog.FindById(reply.ShortcutId!)?.IsOpenOnly == true,
            _ => false
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
