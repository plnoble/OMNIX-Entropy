using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentConversationTests
{
    [Fact]
    public void Empty_question_without_evidence_is_honest_local_and_non_executable()
    {
        var reply = AgentConversationPresenter.Answer("  ", null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.Empty);
        reply.Answer.Should().Contain("没有本机扫描证据");
        reply.NavigationTargetPage.Should().Be("Home");
        reply.CanNavigate.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
        reply.UsedCloudAi.Should().BeFalse();
        reply.PrivacyLine.Should().Contain("本地规则").And.Contain("没有调用云端 AI");
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("你好", false)]
    [InlineData("您好！", false)]
    [InlineData("谢谢", false)]
    [InlineData("你能做什么？", false)]
    [InlineData("怎么使用", false)]
    [InlineData("Wi-Fi 在哪里设置", false)]
    [InlineData("CPU 和显卡是什么", false)]
    [InlineData("新软件应该安装到哪里", false)]
    [InlineData("我想撤销刚才的清理", false)]
    [InlineData("C盘为什么总是满", true)]
    [InlineData("哪些软件占用最多", true)]
    [InlineData("哪些软件会开机启动", true)]
    [InlineData("把软件迁移到D盘", true)]
    [InlineData("帮我卸载微信", true)]
    [InlineData("微信最近有点奇怪", true)]
    [InlineData("你好，微信最近有点奇怪", true)]
    public void Agent_only_hydrates_software_inventory_for_evidence_dependent_questions(
        string question,
        bool expected)
    {
        AgentConversationPresenter.QuestionNeedsSoftwareInventory(question, null)
            .Should().Be(expected);
    }

    [Fact]
    public void Capability_question_answers_directly_without_claiming_a_scan_or_execution()
    {
        var reply = AgentConversationPresenter.Answer("你能做什么？", null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.General);
        reply.Headline.Should().Contain("可以帮你");
        reply.Answer.Should().Contain("体检")
            .And.Contain("应用")
            .And.NotContain("已扫描");
        reply.NavigationTargetPage.Should().BeNull();
        reply.CanExecuteDirectly.Should().BeFalse();
        reply.UsedCloudAi.Should().BeFalse();
    }

    [Fact]
    public void Only_process_and_service_skill_requires_software_inventory()
    {
        AgentConversationPresenter.SkillNeedsSoftwareInventory(
                AgentSkillCategory.ProcessAndServiceManagement)
            .Should().BeTrue();
        AgentConversationPresenter.SkillNeedsSoftwareInventory(
                AgentSkillCategory.SystemSettings)
            .Should().BeFalse();
        AgentConversationPresenter.SkillNeedsSoftwareInventory(
                AgentSkillCategory.HardwareInfo)
            .Should().BeFalse();
    }

    [Fact]
    public void C_drive_answer_uses_summary_but_hides_question_and_evidence_paths()
    {
        const string privatePath = @"C:\Users\10001\AppData\Local\PrivateCache";
        var summary = CreateHealthSummary("发现 " + privatePath + " 占用较大");

        var reply = AgentConversationPresenter.Answer(
            "C盘为什么总是满？我看到 " + privatePath,
            summary,
            [new SoftwareProfile { Name = "Example", InstallPath = @"C:\Program Files\Example" }]);

        reply.Intent.Should().Be(AgentQuestionIntent.CDrive);
        reply.NavigationTargetPage.Should().Be("CDrive");
        reply.Answer.Should().Contain("详细路径已隐藏");
        VisibleText(reply).Should().NotContain(privatePath);
        reply.EvidenceLines.Should().Contain(line => line.Contains("综合评分 72 分"));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void C_drive_question_without_health_requires_read_only_check_first()
    {
        var reply = AgentConversationPresenter.Answer(
            "C盘空间怎么不够了",
            null,
            [new SoftwareProfile { Name = "Example" }]);

        reply.Intent.Should().Be(AgentQuestionIntent.CDrive);
        reply.Answer.Should().Contain("没有本次 C 盘扫描结果");
        reply.NavigationTargetPage.Should().Be("Home");
        reply.NextSteps.Should().Contain(line => line.Contains("只读体检"));
    }

    [Fact]
    public void Startup_answer_counts_signals_and_refuses_to_guess_windows_switch_state()
    {
        var profiles = new[]
        {
            new SoftwareProfile { Name = "OneDrive", StartupEntries = ["OneDrive"] },
            new SoftwareProfile { Name = "Marvis", Services = ["MarvisSvr"], ScheduledTasks = ["MarvisTask"] }
        };

        var reply = AgentConversationPresenter.Answer("哪些软件在后台和开机启动", null, profiles);

        reply.Intent.Should().Be(AgentQuestionIntent.StartupAndBackground);
        reply.Answer.Should().Contain("1 个应用有普通自启动线索");
        reply.Answer.Should().Contain("1 个应用带服务或计划任务");
        reply.Answer.Should().Contain("0 个普通应用具备本地审核线索");
        reply.Answer.Should().Contain("名称级").And.Contain("不受支持");
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Install_and_restore_questions_route_to_internal_safe_pages()
    {
        var install = AgentConversationPresenter.Answer("新软件应该安装到哪里", null, []);
        var restore = AgentConversationPresenter.Answer("我想撤销刚才的清理", null, []);

        install.Intent.Should().Be(AgentQuestionIntent.InstallRouting);
        install.Answer.Should().Contain(@"D:\Software");
        install.Answer.Should().Contain(@"D:\Game");
        install.Answer.Should().Contain(@"D:\Agent");
        install.Answer.Should().Contain(@"D:\Development");
        install.NavigationTargetPage.Should().Be("Install");
        restore.Intent.Should().Be(AgentQuestionIntent.Restore);
        restore.NavigationTargetPage.Should().Be("Timeline");
        VisibleText(restore).Should().Contain("不会自动还原");
        install.CanExecuteDirectly.Should().BeFalse();
        restore.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData("Wi-Fi 在哪里设置", "network")]
    [InlineData("蓝牙耳机怎么配对", "bluetooth")]
    [InlineData("电脑没有声音", "sound")]
    [InlineData("怎么改显示器分辨率", "display")]
    [InlineData("怎么设置电脑不睡眠", "power")]
    [InlineData("新应用默认保存到哪里", "default-save-locations")]
    public void Common_settings_questions_route_to_fixed_open_only_catalog_entries(
        string question,
        string expectedId)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.WindowsSettings);
        reply.ShortcutKind.Should().Be(AgentShortcutKind.WindowsSettings);
        reply.ShortcutId.Should().Be(expectedId);
        reply.NavigationTargetPage.Should().BeNull();
        reply.NavigationLabel.Should().StartWith("打开");
        reply.CanNavigate.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
        VisibleText(reply).Should().Contain("不会").And.NotContain("ms-settings:");
    }

    [Theory]
    [InlineData("电脑蓝屏了怎么看原因", "event-viewer")]
    [InlineData("软件闪退了怎么看", "event-viewer")]
    [InlineData("驱动异常怎么办", "device-manager")]
    public void Troubleshooting_questions_admit_uncertainty_and_route_to_fixed_tools(
        string question,
        string expectedId)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.Troubleshooting);
        reply.ShortcutKind.Should().Be(AgentShortcutKind.SystemTool);
        reply.ShortcutId.Should().Be(expectedId);
        reply.Answer.Should().Contain("不能").And.Contain("根因");
        reply.CanNavigate.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
        VisibleText(reply).Should().NotContain("eventvwr").And.NotContain("devmgmt");
    }

    [Fact]
    public void Named_high_risk_system_tool_uses_catalog_identity_not_question_command()
    {
        var reply = AgentConversationPresenter.Answer("帮我打开注册表编辑器 regedit /s bad.reg", null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.SystemTool);
        reply.ShortcutKind.Should().Be(AgentShortcutKind.SystemTool);
        reply.ShortcutId.Should().Be("registry-editor");
        reply.NavigationLabel.Should().Be("打开注册表编辑器");
        VisibleText(reply).Should().NotContain("bad.reg").And.NotContain("regedit.exe");
        SystemToolShortcutCatalog.FindById(reply.ShortcutId!)!.RequiresConfirmation.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData("打开回收站看看")]
    [InlineData("帮我清空回收站")]
    public void Recycle_bin_wording_only_offers_fixed_review_entry(string question)
    {
        var reply = AgentConversationPresenter.Answer(question, null, []);

        reply.Intent.Should().Be(AgentQuestionIntent.SystemTool);
        reply.ShortcutKind.Should().Be(AgentShortcutKind.SystemTool);
        reply.ShortcutId.Should().Be(SystemToolShortcutCatalog.RecycleBinId);
        reply.NavigationLabel.Should().Be("打开回收站查看");
        reply.Answer.Should().Contain("只会打开").And.Contain("不会清空");
        VisibleText(reply).Should().Contain("清空后通常不能还原")
            .And.NotContain("explorer.exe")
            .And.NotContain("shell:RecycleBinFolder");
        reply.CanNavigate.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Skill_catalog_actions_use_current_evidence_and_admit_unavailable_capabilities()
    {
        var health = CreateHealthSummary("C 盘占用正常");
        health = new HealthCheckSummary
        {
            OverallScore = health.OverallScore,
            Dimensions = health.Dimensions,
            KeyFindings = health.KeyFindings,
            Hardware = new HardwareSummaryObservation
            {
                Availability = MachineMetricAvailability.Available,
                CpuName = "Example Processor",
                GpuName = "Example Graphics",
                OperatingSystem = "Windows 11",
                Architecture = "X64"
            }
        };
        var profiles = new[]
        {
            new SoftwareProfile { Name = "Chat", StartupEntries = ["Chat Startup"] }
        };

        var diagnosis = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.SystemDiagnosis, health, profiles);
        var settings = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.SystemSettings, health, profiles);
        var troubleshooting = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.Troubleshooting, health, profiles);
        var desktop = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.WindowAndDesktop, health, profiles);
        var background = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.ProcessAndServiceManagement, health, profiles);
        var hardware = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.HardwareInfo, health, profiles);
        var tools = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.SystemTools, health, profiles);
        var session = AgentConversationPresenter.ExplainSkill(
            AgentSkillCategory.InputAndSession, health, profiles);

        diagnosis.NavigationTargetPage.Should().Be("Home");
        diagnosis.Answer.Should().Contain("72");
        settings.Answer.Should().Contain("选择").And.Contain("设置");
        settings.CanNavigate.Should().BeFalse();
        troubleshooting.Answer.Should().Contain("描述").And.Contain("不能判断");
        desktop.Answer.Should().Contain("还没有读取").And.Contain("窗口").And.Contain("桌面");
        desktop.CanNavigate.Should().BeFalse();
        background.NavigationTargetPage.Should().Be("Apps");
        background.Answer.Should().Contain("自启动线索");
        hardware.EvidenceLines.Should().Contain(line => line.Contains("Example Processor"));
        hardware.NavigationTargetPage.Should().Be("Home");
        tools.Answer.Should().Contain("系统工具列表").And.Contain("选择");
        tools.CanNavigate.Should().BeFalse();
        session.Answer.Should().Contain("不提供").And.Contain("锁屏").And.Contain("重启");
        session.CanNavigate.Should().BeFalse();

        new[] { diagnosis, settings, troubleshooting, desktop, background, hardware, tools, session }
            .Should().OnlyContain(reply => !reply.CanExecuteDirectly && !reply.UsedCloudAi);
    }

    [Fact]
    public void Skill_card_buttons_only_render_local_agent_replies()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = Extract(
            main,
            "private async void AgentSkillAction_Click",
            "private async void AgentConversationNavigate_Click");

        xaml.Should().Contain("Click=\"AgentSkillAction_Click\"")
            .And.Contain("AgentSkillActionButton_{0}")
            .And.Contain("Content=\"问 Agent\"")
            .And.Contain("Tag=\"{Binding Category}\"");
        handler.Should().Contain("AgentConversationPresenter.ExplainSkill")
            .And.Contain("ApplyAgentConversationReply")
            .And.NotContain("Process.Start")
            .And.NotContain("OpenAllowlistedSystemTool")
            .And.NotContain("OpenAllowlistedWindowsSettings")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("ShowPage(")
            .And.NotContain("Registry")
            .And.NotContain("File.")
            .And.NotContain("Directory.");
    }

    [Fact]
    public void Skill_card_gui_smoke_is_isolated_and_non_executable()
    {
        var smoke = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-agent-skill-cards-smoke.ps1"));

        smoke.Should().Contain("AgentSkillActionButton_WindowAndDesktop")
            .And.Contain("AgentConversationHeadlineTextBlock")
            .And.Contain("AgentConversationNavigateButton")
            .And.Contain("truthfulUnavailableConclusionVisible = $true")
            .And.Contain("unsafeNextActionVisible = $false")
            .And.Contain("noOperationExecuted = $true")
            .And.Contain("OMNIX_ENTROPY_DATA_ROOT")
            .And.Contain("Save-WindowScreenshot $window $screenshot");
        smoke.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Invoke-Element $navigate");
    }

    [Fact]
    public void Exact_unique_application_answer_targets_drawer_without_exposing_profile_paths()
    {
        const string installPath = @"D:\Software\Marvis\Install";
        var profile = new SoftwareProfile
        {
            Name = "Marvis",
            InstallPath = installPath,
            DataPaths = [@"C:\Users\10001\AppData\Local\Marvis"],
            RunningProcesses = ["Marvis", "MarvisAgent"],
            Services = ["MarvisSvr"]
        };

        var reply = AgentConversationPresenter.Answer("Marvis 装在哪里", null, [profile]);

        reply.Intent.Should().Be(AgentQuestionIntent.ApplicationSpecific);
        reply.TargetAppName.Should().Be("Marvis");
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.Answer.Should().Contain("D 盘");
        reply.EvidenceLines.Should().Contain(line => line.Contains("正在运行的进程"));
        VisibleText(reply).Should().NotContain(installPath);
        VisibleText(reply).Should().NotContain(@"C:\Users\10001");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Duplicate_application_names_refuse_automatic_selection()
    {
        var profiles = new[]
        {
            new SoftwareProfile { Name = "Marvis", InstallPath = @"D:\Software\Marvis" },
            new SoftwareProfile { Name = "marvis", InstallPath = @"C:\Program Files\Marvis" }
        };

        var reply = AgentConversationPresenter.Answer("帮我卸载 Marvis", null, profiles);

        reply.Intent.Should().Be(AgentQuestionIntent.ApplicationSpecific);
        reply.Answer.Should().Contain("同名记录");
        reply.TargetAppName.Should().BeNull();
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Exact_application_migration_answer_uses_plain_chinese_and_remains_navigation_only()
    {
        var profile = new SoftwareProfile
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = @"C:\Users\10001\AppData\Local\Programs\Ollama",
            Services = ["OllamaService"]
        };

        var reply = AgentConversationPresenter.Answer("Ollama 能迁移到 D 盘吗", null, [profile]);

        reply.Intent.Should().Be(AgentQuestionIntent.ApplicationSpecific);
        reply.Answer.Should().Contain("先关闭软件和相关后台组件");
        reply.Answer.Contains("Migration", StringComparison.OrdinalIgnoreCase).Should().BeFalse();
        reply.TargetAppName.Should().Be("Ollama");
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Stale_application_target_returns_path_free_recovery_guidance()
    {
        var reply = AgentConversationPresenter.TargetUnavailable();

        reply.TargetAppName.Should().BeNull();
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.Answer.Should().Contain("停止了这次定位");
        reply.CanExecuteDirectly.Should().BeFalse();
        reply.UsedCloudAi.Should().BeFalse();
    }

    [Fact]
    public void Agent_conversation_ui_is_first_visible_testable_and_navigation_only()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var presenter = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Agent", "AgentConversationPresentation.cs"));

        var automationIds = new[]
        {
            "AgentConversationScrollViewer",
            "AgentQuestionTextBox",
            "AskComputerAgentButton",
            "AgentConversationResponsePanel",
            "AgentConversationHeadlineTextBlock",
            "AgentConversationAnswerTextBlock",
            "AgentConversationEvidenceListBox",
            "AgentConversationNextStepsListBox",
            "AgentConversationSafetyTextBlock",
            "AgentConversationPrivacyTextBlock",
            "AgentConversationNavigateButton"
        };

        foreach (var automationId in automationIds)
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{automationId}\"");

        xaml.Should().Contain("Click=\"AskComputerAgent_Click\"");
        xaml.Should().Contain("Click=\"AgentConversationNavigate_Click\"");
        xaml.IndexOf("AgentConversationScrollViewer", StringComparison.Ordinal)
            .Should().BeGreaterThan(xaml.IndexOf("x:Name=\"AgentPage\"", StringComparison.Ordinal));
        xaml.IndexOf("AgentConversationResponsePanel", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("AgentNextStepTitleTextBlock", StringComparison.Ordinal));
        xaml.Should().Contain("AutomationProperties.AutomationId=\"CDrivePageScrollViewer\"");

        var handlers = string.Join(
            Environment.NewLine,
            SourceMethodExtractor.Extract(
                main,
                "private async void AskComputerAgent_Click(object sender, RoutedEventArgs e)"),
            SourceMethodExtractor.Extract(
                main,
                "private void ApplyAgentConversationReply(AgentConversationReply reply)"),
            SourceMethodExtractor.Extract(
                main,
                "private async void AgentConversationNavigate_Click(object sender, RoutedEventArgs e)"));
        handlers.Should().Contain("AgentConversationPresenter.Answer");
        handlers.Should().Contain("AgentConversationScrollViewer.ScrollToTop()");
        handlers.Should().NotContain("AgentConversationResponsePanel.BringIntoView()");
        handlers.Should().Contain("ResolveAndOpenAppTargetAsync");
        handlers.Should().Contain("OpenAllowlistedWindowsSettings(reply.ShortcutId)");
        handlers.Should().Contain("OpenAllowlistedSystemTool(reply.ShortcutId)");
        handlers.Should().Contain("IsAgentNavigationTarget");
        handlers.Should().NotContain("Process.Start");
        handlers.Should().NotContain("SafetyOperationPipeline");
        handlers.Should().NotContain("OperationDescriptor");
        handlers.Should().NotContain("Registry");
        handlers.Should().NotContain("File.Move");
        handlers.Should().NotContain("File.Delete");
        handlers.Should().NotContain("Directory.Move");
        handlers.Should().NotContain("Directory.Delete");

        presenter.Should().Contain("public bool CanExecuteDirectly => false;");
        presenter.Should().Contain("public bool UsedCloudAi => false;");
        presenter.Should().NotContain("OperationDescriptor");
        presenter.Should().NotContain("Process.Start");
        presenter.Should().NotContain("Registry.SetValue");
    }

    [Fact]
    public void Agent_troubleshooting_gui_smoke_is_cancel_only_and_does_not_launch_the_tool()
    {
        var smoke = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-agent-troubleshooting-routing-smoke.ps1"));

        smoke.Should().Contain("AgentQuestionTextBox")
            .And.Contain("AgentConversationHeadlineTextBlock")
            .And.Contain("AgentConversationNavigateButton")
            .And.Contain("Get-Process mmc")
            .And.Contain("Get-WpfTopLevelWindowHandlesForProcess")
            .And.Contain("$candidate.Current.Name -eq $confirmationTitle")
            .And.Contain("$windowPattern.Close()")
            .And.Contain("externalToolStarted = $false")
            .And.Contain("noOperationExecuted = $true")
            .And.Contain("Save-WindowScreenshot $window $answerScreenshot")
            .And.Contain("Save-WindowScreenshot $confirmation $confirmationScreenshot");
        smoke.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Find-ButtonByName $confirmation")
            .And.NotContain("Start-Process devmgmt");
    }

    private static HealthCheckSummary CreateHealthSummary(string diskResult) =>
        new()
        {
            OverallScore = 72,
            Dimensions =
            [
                new HealthDimensionResult
                {
                    Name = "磁盘健康",
                    Result = diskResult,
                    Rating = "有优化空间"
                }
            ],
            KeyFindings =
            [
                new HealthFinding
                {
                    Text = "发现可复查的低风险缓存",
                    Action = RecommendationAction.Clean,
                    Risk = Css.Core.Operations.RiskLevel.Low
                }
            ]
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

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Repository file was not found.", Path.Combine(segments));
    }

    private static string Extract(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }
}
