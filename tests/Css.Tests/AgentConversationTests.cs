using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
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
    public void Decision_questions_request_only_the_read_only_evidence_they_need()
    {
        AgentConversationPresenter.QuestionNeedsFullHealthScan(
                "怎样最安全地释放 10GB？",
                null)
            .Should().BeTrue();
        AgentConversationPresenter.QuestionNeedsFullHealthScan(
                "最近一周谁增长最快？",
                null)
            .Should().BeTrue();
        AgentConversationPresenter.QuestionNeedsGrowthEvidence(
                "最近一周谁增长最快？",
                0)
            .Should().BeTrue();
        AgentConversationPresenter.QuestionNeedsGrowthEvidence(
                "最近一周谁增长最快？",
                1)
            .Should().BeFalse();
        AgentConversationPresenter.QuestionNeedsGrowthEvidence(
                "怎样最安全地释放 10GB？",
                0)
            .Should().BeFalse();
    }

    [Fact]
    public void Beginner_decision_prompts_are_compact_unique_and_navigation_only()
    {
        var prompts = AgentDecisionPromptCatalog.CreateDefault();

        prompts.Should().HaveCount(3);
        prompts.Select(prompt => prompt.Id).Should().OnlyHaveUniqueItems();
        prompts.Select(prompt => prompt.Label).Should().OnlyHaveUniqueItems();
        prompts.Select(prompt => AgentConversationPresenter.Answer(prompt.Question, null, []))
            .Should().OnlyContain(reply => !reply.CanExecuteDirectly && !reply.UsedCloudAi);
        prompts.Select(prompt => AgentConversationPresenter.Answer(prompt.Question, null, []).Intent)
            .Should().Equal(
                AgentQuestionIntent.CDrive,
                AgentQuestionIntent.Growth,
                AgentQuestionIntent.StoragePlan);
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
    [InlineData("打开蓝牙设置", "bluetooth")]
    [InlineData("打开声音设置", "sound")]
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
    public void C_drive_question_names_largest_non_additive_sources_from_current_evidence()
    {
        var context = new AgentDecisionContext
        {
            DrivePlan = DrivePlan(
                targetBytes: 20L * 1024 * 1024 * 1024,
                safeBytes: 512L * 1024 * 1024),
            StorageSources =
            [
                StorageSource("用户文件占用 82.0 GB", 82L * 1024 * 1024 * 1024),
                StorageSource("程序和工具占用 38.0 GB", 38L * 1024 * 1024 * 1024),
                StorageSource("旧缓存文件 512.0 MB", 512L * 1024 * 1024)
            ],
            ObservedSnapshotCount = 2
        };

        var reply = AgentConversationPresenter.Answer(
            "C盘为什么还是这么满？",
            CreateHealthSummary("C 盘已使用 92.0%"),
            [],
            decisionContext: context);

        reply.Intent.Should().Be(AgentQuestionIntent.CDrive);
        reply.Answer.Should().Contain("主要线索").And.Contain("不能直接相加");
        reply.EvidenceLines.Should().Contain(line => line.Contains("用户文件占用 82.0 GB"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("程序和工具占用 38.0 GB"));
        reply.NavigationTargetPage.Should().Be("CDrive");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Global_growth_question_ranks_current_evidence_and_admits_short_window()
    {
        var context = new AgentDecisionContext
        {
            ObservedSnapshotCount = 3,
            GrowthSources =
            [
                GrowthSource("Antigravity", 2L * 1024 * 1024 * 1024, TimeSpan.FromDays(2), true),
                GrowthSource("用户文件", 700L * 1024 * 1024, TimeSpan.FromDays(2), false)
            ]
        };

        var reply = AgentConversationPresenter.Answer(
            "最近一周谁增长最快？",
            CreateHealthSummary("C 盘已使用 92.0%"),
            [],
            decisionContext: context);

        reply.Intent.Should().Be(AgentQuestionIntent.Growth);
        reply.Answer.Should().Contain("Antigravity").And.Contain("增长最快");
        VisibleText(reply).Should().Contain("约 2 天").And.Contain("不是完整一周");
        reply.NavigationTargetPage.Should().Be("CDrive");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void D_drive_program_with_C_drive_family_data_explains_the_storage_split()
    {
        var mainProgram = new SoftwareProfile
        {
            Name = "Antigravity 2.4.3",
            DisplayVersion = "2.4.3",
            InstallPath = @"D:\Agent\Antigravity",
            UninstallCommand = @"D:\Agent\Antigravity\uninstall.exe",
            InstalledSizeBytes = 1200L * 1024 * 1024
        };
        var userData = new SoftwareProfile
        {
            Name = "Antigravity (User)",
            CDriveDataSizeBytes = 352L * 1024 * 1024,
            DataSizeBytes = 352L * 1024 * 1024,
            CacheSizeBytes = 300L * 1024 * 1024,
            CDriveWritePaths =
            [
                @"C:\Users\Me\AppData\Local\antigravity-updater",
                @"C:\Users\Me\AppData\Roaming\Antigravity"
            ]
        };

        var reply = AgentConversationPresenter.Answer(
            "Antigravity 迁移到 D 盘后为什么缓存还在 C 盘？",
            null,
            [mainProgram, userData]);

        reply.Intent.Should().Be(AgentQuestionIntent.ApplicationSpecific);
        reply.Answer.Should().Contain("主程序在 D 盘")
            .And.Contain("缓存和数据是另一回事")
            .And.Contain("352.0 MB")
            .And.Contain("不能保证");
        reply.NextSteps.Should().Contain(line => line.Contains("软件自己的设置"));
        reply.TargetAppName.Should().Be("Antigravity 2.4.3");
        VisibleText(reply).Should().NotContain(@"C:\Users\Me");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Duplicate_family_uninstall_question_targets_only_the_exact_registered_entry()
    {
        var registered = new SoftwareProfile
        {
            Name = "OpenCode 1.14.41",
            DisplayVersion = "1.14.41",
            InstallPath = @"C:\Program Files\OpenCode",
            UninstallCommand = @"""C:\Program Files\OpenCode\uninstall.exe"""
        };
        var portable = new SoftwareProfile
        {
            Name = "OpenCode",
            DisplayVersion = "1.4.3",
            InstallPath = @"D:\Development\OpenCode"
        };
        var dataOnly = new SoftwareProfile
        {
            Name = "OpenCode 1.18.4",
            DisplayVersion = "1.18.4",
            CDriveDataSizeBytes = 237L * 1024 * 1024,
            CDriveWritePaths = [@"C:\Users\Me\AppData\Local\opencode-updater"]
        };

        var reply = AgentConversationPresenter.Answer(
            "三个 OpenCode 中哪个可以卸载？",
            null,
            [registered, portable, dataOnly]);

        reply.Intent.Should().Be(AgentQuestionIntent.Uninstall);
        reply.Answer.Should().Contain("3 条")
            .And.Contain("只有 OpenCode 1.14.41")
            .And.Contain("官方卸载审核");
        reply.EvidenceLines.Should().Contain(line => line.Contains("D 盘") && line.Contains("不可直接卸载"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("数据") && line.Contains("不是可卸载主程序"));
        reply.TargetAppName.Should().Be("OpenCode 1.14.41");
        reply.TargetAppHandoff.Should().Be(AgentApplicationHandoff.UninstallReview);
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Requested_safe_release_plan_shows_confirmed_amount_and_remaining_gap()
    {
        var context = new AgentDecisionContext
        {
            DrivePlan = DrivePlan(
                targetBytes: 18L * 1024 * 1024 * 1024,
                safeBytes: 2L * 1024 * 1024 * 1024),
            StorageSources =
            [
                StorageSource("用户文件占用 42.0 GB", 42L * 1024 * 1024 * 1024),
                StorageSource("应用数据占用 16.0 GB", 16L * 1024 * 1024 * 1024)
            ],
            ObservedSnapshotCount = 2
        };

        var reply = AgentConversationPresenter.Answer(
            "怎样最安全地释放 10GB？",
            CreateHealthSummary("C 盘已使用 92.0%"),
            [],
            decisionContext: context);

        reply.Intent.Should().Be(AgentQuestionIntent.StoragePlan);
        reply.Answer.Should().Contain("10.0 GB")
            .And.Contain("2.0 GB")
            .And.Contain("还差 8.0 GB");
        reply.EvidenceLines.Should().Contain(line => line.Contains("不能相加"));
        reply.NextSteps.First().Should().Contain("低风险").And.Contain("隔离区");
        reply.NavigationTargetPage.Should().Be("CDrive");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Scanner_decision_context_keeps_numeric_evidence_but_removes_raw_paths()
    {
        const string privatePath = @"C:\Users\Me\AppData\Local\PrivateCache";
        var rootCause = new CDriveRootCauseSummary
        {
            Headline = "C 盘占用摘要",
            Subheadline = "只读结果",
            TechnicalReportAvailable = true,
            Cards =
            [
                new CDriveRootCauseCard
                {
                    Title = "应用数据",
                    PrimaryText = "发现 " + privatePath + " 占用 4.0 GB",
                    Explanation = "这是应用数据线索。",
                    AgentSuggestion = "先确认归属。",
                    SizeText = "4.0 GB",
                    Severity = 1,
                    EvidenceBytes = 4L * 1024 * 1024 * 1024
                }
            ]
        };
        var growth = new GrowthFinding
        {
            Path = privatePath,
            OwnerSoftware = privatePath,
            PreviousBytes = 1024,
            CurrentBytes = 4096,
            SourceKind = GrowthSourceKind.Software,
            ObservationInterval = TimeSpan.FromDays(1),
            ObservedSnapshots = 2,
            TrendGrowthBytes = 3072,
            TrendWindow = TimeSpan.FromDays(1),
            Reason = "Grew since previous scan."
        };

        var context = AgentDecisionContextBuilder.Build(rootCause, null, [growth], 2);
        var visible = string.Join(
            "\n",
            context.StorageSources.SelectMany(source => new[]
            {
                source.PrimaryText,
                source.Explanation,
                source.Suggestion
            }).Concat(context.GrowthSources.SelectMany(source => new[]
            {
                source.OwnerLabel,
                source.OneTimeAction,
                source.PreventionAction
            })));

        context.StorageSources.Single().EvidenceBytes.Should().Be(4L * 1024 * 1024 * 1024);
        context.StorageSources.Single().PrimaryText.Should().Be("占用来源已隐藏");
        context.GrowthSources.Single().LatestGrowthBytes.Should().Be(3072);
        context.GrowthSources.Single().OwnerLabel.Should().Be("未知来源");
        context.GrowthSources.Single().TargetAppName.Should().BeNull();
        visible.Should().NotContain(privatePath).And.NotContain(@"C:\Users\Me");
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
            "AgentDecisionPromptTitleTextBlock",
            "AgentDecisionQuickChoicesItemsControl",
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
        xaml.IndexOf("AgentDecisionQuickChoicesItemsControl", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("AgentSymptomQuickChoicesItemsControl", StringComparison.Ordinal));
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
                "private async Task RunComputerAgentQuestionAsync(string question)"),
            SourceMethodExtractor.Extract(
                main,
                "private void ApplyAgentConversationReply(AgentConversationReply reply)"),
            SourceMethodExtractor.Extract(
                main,
                "private async void AgentConversationNavigate_Click(object sender, RoutedEventArgs e)"));
        handlers.Should().Contain("AgentConversationPresenter.Answer");
        handlers.Should().Contain("AgentDecisionContextBuilder.Build")
            .And.Contain("decisionContext: decisionContext");
        handlers.Should().Contain("AgentConversationResponsePanel.UpdateLayout()")
            .And.Contain("AgentConversationResponsePanel.BringIntoView()");
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
            .And.Contain("OMNIX_ENTROPY_QUARANTINE_ROOT")
            .And.Contain("quarantineManifestCount")
            .And.Contain("noOperationExecuted = ($quarantineManifestCount -eq 0)")
            .And.Contain("Save-WindowScreenshot $window $answerScreenshot")
            .And.Contain("Save-WindowScreenshot $confirmation $confirmationScreenshot");
        smoke.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Find-ButtonByName $confirmation")
            .And.NotContain("Start-Process devmgmt");
    }

    [Fact]
    public void Agent_decision_gui_smoke_is_isolated_path_free_and_non_executable()
    {
        var smoke = File.ReadAllText(FindRepositoryFile(
            ".omx", "gui-agent-decision-workflows-smoke.ps1"));

        smoke.Should().Contain("AgentDecisionQuickChoice_c-drive-full")
            .And.Contain("AgentDecisionQuickChoice_fastest-growth")
            .And.Contain("AgentDecisionQuickChoice_safe-release")
            .And.Contain("AgentConversationAnswerTextBlock")
            .And.Contain("OpenCode 1.14.41")
            .And.Contain("352.0 MB")
            .And.Contain("OMNIX_ENTROPY_CDRIVE_SCAN_ROOT")
            .And.Contain("OMNIX_ENTROPY_SOFTWARE_FIXTURE")
            .And.Contain("OMNIX_ENTROPY_QUARANTINE_ROOT")
            .And.Contain("OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT")
            .And.Contain("Get-DescendantText $window")
            .And.Contain("noOperationExecuted = ($quarantineManifestCount -eq 0 -and -not $uninstallEvidenceCreated)")
            .And.Contain("Save-WindowScreenshot $window $cDriveScreenshot")
            .And.Contain("Save-WindowScreenshot $window $cacheScreenshot");
        smoke.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Get-DescendantText $responsePanel")
            .And.NotContain("Invoke-Element (Find-ByAutomationId $window 'AgentConversationNavigateButton'");
        smoke.All(character => character <= 0x7F).Should().BeTrue();
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

    private static AgentDrivePlanEvidence DrivePlan(long targetBytes, long safeBytes) =>
        new()
        {
            Headline = "磁盘改善目标",
            Progress = "先处理可回滚内容，再确认大项。",
            Steps = ["先处理低风险项。", "再查看主要来源。", "最后防止继续增长。"],
            TargetReleaseBytes = targetBytes,
            SafeCleanupBytes = safeBytes,
            RemainingGapBytes = Math.Max(0, targetBytes - safeBytes)
        };

    private static AgentStorageSourceEvidence StorageSource(string text, long bytes) =>
        new()
        {
            PrimaryText = text,
            Explanation = "这是只读占用线索。",
            Suggestion = "先确认来源，不直接删除。",
            EvidenceBytes = bytes
        };

    private static AgentGrowthSourceEvidence GrowthSource(
        string owner,
        long bytes,
        TimeSpan interval,
        bool sustained) =>
        new()
        {
            OwnerLabel = owner,
            LatestGrowthBytes = bytes,
            TrendGrowthBytes = bytes,
            ObservationInterval = interval,
            TrendWindow = interval,
            ObservedSnapshots = 3,
            IsFirstObservation = false,
            IsSustainedGrowth = sustained,
            OneTimeAction = "现在：先确认内容类型。",
            PreventionAction = "以后：支持时再把缓存位置改到 D 盘。",
            TargetAppName = owner == "Antigravity" ? owner : null
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
