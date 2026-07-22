using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Core.Startup;

namespace Css.Core.Agent;

public enum AgentQuestionIntent
{
    Empty,
    CDrive,
    SystemDiagnosis,
    MachineHealth,
    HardwareInfo,
    WindowsSettings,
    Troubleshooting,
    SystemTool,
    Applications,
    StartupAndBackground,
    InstallRouting,
    Migration,
    Uninstall,
    Restore,
    ApplicationSpecific,
    SkillOverview,
    General
}

public enum AgentShortcutKind
{
    WindowsSettings,
    SystemTool
}

public enum AgentApplicationHandoff
{
    Details,
    UninstallReview,
    MigrationReview,
    CacheCleanupReview,
    StartupControlReview
}

public sealed class AgentConversationReply
{
    public required AgentQuestionIntent Intent { get; init; }
    public required string Headline { get; init; }
    public required string Answer { get; init; }
    public required IReadOnlyList<string> EvidenceLines { get; init; }
    public required IReadOnlyList<string> NextSteps { get; init; }
    public required string SafetyBoundary { get; init; }
    public required string PrivacyLine { get; init; }
    public string? NavigationTargetPage { get; init; }
    public string? TargetAppName { get; init; }
    public AppCatalogFilter? TargetAppFilter { get; init; }
    public AgentApplicationHandoff TargetAppHandoff { get; init; } = AgentApplicationHandoff.Details;
    public AgentShortcutKind? ShortcutKind { get; init; }
    public string? ShortcutId { get; init; }
    public string? NavigationLabel { get; init; }
    public bool CanNavigate =>
        !string.IsNullOrWhiteSpace(NavigationTargetPage) ||
        (ShortcutKind is not null && !string.IsNullOrWhiteSpace(ShortcutId));
    public bool CanExecuteDirectly => false;
    public bool UsedCloudAi => false;
}

public static class AgentConversationPresenter
{
    private static readonly string[] CDriveWords = ["c盘", "系统盘", "磁盘", "空间", "垃圾", "满了", "爆满"];
    private static readonly string[] SystemDiagnosisWords =
    [
        "体检电脑",
        "电脑体检",
        "全面体检",
        "检查一下电脑",
        "检查电脑",
        "电脑整体状态",
        "整机状态",
        "电脑健康状态",
        "电脑健不健康",
        "电脑需要怎么优化",
        "电脑该怎么优化",
        "优化一下电脑"
    ];
    private static readonly string[] MachineHealthWords = ["d盘", "内存", "电池", "进程", "卡顿", "电脑卡", "为什么卡", "有点卡", "性能"];
    private static readonly string[] HardwareWords = ["电脑配置", "硬件配置", "cpu", "处理器", "显卡", "gpu", "windows版本", "系统版本", "多少核"];
    private static readonly string[] StartupWords = ["自启动", "启动项", "开机", "后台", "常驻", "服务"];
    private static readonly string[] InstallWords = ["安装", "安装包", "装到", "setup", "installer"];
    private static readonly string[] MigrationWords = ["迁移", "搬到", "移到d", "挪到d", "换地方"];
    private static readonly string[] UninstallWords = ["卸载", "删除软件", "不想要", "不用了"];
    private static readonly string[] RestoreWords = ["后悔", "还原", "恢复", "隔离区", "撤销"];
    private static readonly string[] ApplicationWords = ["软件", "应用", "程序", "装在哪", "占用最多"];
    private static readonly string[] CacheWords = ["缓存", "清理"];
    private static readonly string[] AppGrowthWords =
    [
        "越来越大", "变大", "占用变多", "越占越多", "一直增长", "不断增长", "还在增长", "继续增长",
        "还在写c盘", "还在写 c 盘", "继续写c盘", "继续写 c 盘", "往c盘写", "往 c 盘写",
        "c盘还在增加", "c 盘还在增加", "c盘一直增加", "c 盘一直增加"
    ];
    private static readonly string[] AppCrashWords = ["闪退", "崩溃", "启动失败", "打不开", "报错"];
    private static readonly string[] AppFreezeWords = ["卡死", "无响应", "假死", "卡住", "一直转圈"];
    private static readonly string[] AppResourceWords =
    [
        "占内存", "内存占用", "内存高", "内存很高", "内存太高", "内存过高",
        "cpu占用", "cpu 占用", "cpu高", "cpu 高", "cpu很高", "cpu 很高",
        "cpu太高", "cpu 太高", "cpu过高", "cpu 过高",
        "处理器占用", "处理器很高", "处理器过高", "占资源", "资源占用", "占用多少资源"
    ];
    private static readonly string[] AppVagueProblemWords = ["有点奇怪", "异常", "不正常", "出问题", "有问题"];
    private static readonly HashSet<string> NonDiagnosticConversationQuestions =
        new(StringComparer.CurrentCultureIgnoreCase)
        {
            "你好",
            "您好",
            "嗨",
            "hello",
            "hi",
            "谢谢",
            "感谢",
            "你能做什么",
            "你会什么",
            "有什么功能",
            "怎么使用",
            "怎么用",
            "帮助",
            "你是谁"
        };

    public static bool QuestionNeedsSoftwareInventory(
        string? question,
        HealthCheckSummary? health)
    {
        if (IsNonDiagnosticConversation(question))
            return false;
        if (LooksLikeNamedApplicationTroubleshooting(question))
            return true;

        var intent = Answer(question, health, []).Intent;
        return intent is AgentQuestionIntent.CDrive
            or AgentQuestionIntent.Applications
            or AgentQuestionIntent.StartupAndBackground
            or AgentQuestionIntent.Migration
            or AgentQuestionIntent.Uninstall
            or AgentQuestionIntent.ApplicationSpecific
            or AgentQuestionIntent.General;
    }

    public static bool SkillNeedsSoftwareInventory(AgentSkillCategory category) =>
        category == AgentSkillCategory.ProcessAndServiceManagement;

    public static bool SkillNeedsHealthScan(
        AgentSkillCategory category,
        HealthCheckSummary? health) =>
        category == AgentSkillCategory.SystemDiagnosis
        && health is null;

    public static bool QuestionNeedsFullHealthScan(
        string? question,
        HealthCheckSummary? health) =>
        health is null
        && Answer(question, null, []).Intent is
            AgentQuestionIntent.CDrive or AgentQuestionIntent.SystemDiagnosis;

    public static bool QuestionNeedsCDriveScan(
        string? question,
        HealthCheckSummary? health) =>
        health is null
        && Answer(question, null, []).Intent == AgentQuestionIntent.CDrive;

    public static bool QuestionNeedsMachineObservation(
        string? question,
        HealthCheckSummary? health,
        MachineHealthObservation? machineHealth)
    {
        if (machineHealth is not null)
            return false;

        var intent = Answer(question, health, [], machineHealth).Intent;
        return intent switch
        {
            AgentQuestionIntent.HardwareInfo =>
                health?.Hardware?.Availability != MachineMetricAvailability.Available,
            AgentQuestionIntent.MachineHealth => !HasMachineDimensions(health),
            _ => false
        };
    }

    public static bool SkillNeedsMachineObservation(
        AgentSkillCategory category,
        HealthCheckSummary? health,
        MachineHealthObservation? machineHealth) =>
        category == AgentSkillCategory.HardwareInfo
        && machineHealth is null
        && health?.Hardware?.Availability != MachineMetricAvailability.Available;

    public static SoftwareProfile? ApplicationCrashObservationTarget(
        string? question,
        IEnumerable<SoftwareProfile>? softwareProfiles)
    {
        var normalized = Normalize(question);
        if (!LooksLikeNamedApplicationTroubleshooting(normalized)
            || !ContainsAny(normalized, AppCrashWords.Concat(AppFreezeWords))
            || ContainsAny(
                normalized,
                UninstallWords
                    .Concat(MigrationWords)
                    .Concat(StartupWords)
                    .Concat(CacheWords)
                    .Concat(InstallWords)))
        {
            return null;
        }

        var mention = ResolveProfileMention(
            normalized,
            (softwareProfiles ?? []).ToList());
        return mention.IsAmbiguous ? null : mention.Profile;
    }

    public static SoftwareProfile? ApplicationRuntimeObservationTarget(
        string? question,
        IEnumerable<SoftwareProfile>? softwareProfiles)
    {
        var normalized = Normalize(question);
        if (!LooksLikeNamedApplicationTroubleshooting(normalized)
            || !ContainsAny(normalized, AppFreezeWords.Concat(AppResourceWords))
            || ContainsAny(
                normalized,
                UninstallWords
                    .Concat(MigrationWords)
                    .Concat(StartupWords)
                    .Concat(CacheWords)
                    .Concat(InstallWords)))
        {
            return null;
        }

        var mention = ResolveProfileMention(
            normalized,
            (softwareProfiles ?? []).ToList());
        return mention.IsAmbiguous ? null : mention.Profile;
    }

    public static SoftwareProfile? ApplicationGrowthObservationTarget(
        string? question,
        IEnumerable<SoftwareProfile>? softwareProfiles)
    {
        var normalized = Normalize(question);
        if (!ContainsAny(normalized, AppGrowthWords)
            || HasExplicitApplicationOperation(normalized))
        {
            return null;
        }

        var mention = ResolveProfileMention(
            normalized,
            (softwareProfiles ?? []).ToList());
        return mention.IsAmbiguous ? null : mention.Profile;
    }

    public static AgentConversationReply Answer(
        string? question,
        HealthCheckSummary? health,
        IEnumerable<SoftwareProfile>? softwareProfiles,
        MachineHealthObservation? machineHealth = null,
        ApplicationCrashObservation? applicationCrashObservation = null,
        ApplicationRuntimeObservation? applicationRuntimeObservation = null,
        ApplicationGrowthObservation? applicationGrowthObservation = null)
    {
        var profiles = (softwareProfiles ?? []).ToList();
        var normalized = Normalize(question);
        if (string.IsNullOrWhiteSpace(normalized))
            return EmptyReply(health, profiles);
        if (IsNonDiagnosticConversation(normalized))
            return CapabilityReply();

        var mention = ResolveProfileMention(normalized, profiles);
        if (mention.IsAmbiguous)
            return AmbiguousApplicationReply();
        if (mention.Profile is not null)
            return ApplicationReply(
                normalized,
                mention.Profile,
                applicationCrashObservation,
                applicationRuntimeObservation,
                applicationGrowthObservation);

        var shortcutReply = ShortcutReply(normalized);
        if (shortcutReply is not null)
            return shortcutReply;

        if (ContainsAny(normalized, RestoreWords))
            return RestoreReply();
        if (ContainsAny(normalized, MigrationWords))
            return MigrationReply(profiles);
        if (ContainsAny(normalized, UninstallWords))
            return UninstallReply(profiles);
        if (ContainsAny(normalized, InstallWords))
            return InstallReply();
        if (ContainsAny(normalized, StartupWords))
            return StartupReply(profiles);
        if (ContainsAny(normalized, SystemDiagnosisWords))
            return DiagnosisSkillReply(health, AgentQuestionIntent.SystemDiagnosis);
        if (ContainsAny(normalized, HardwareWords))
            return HardwareReply(health, machineHealth);
        if (ContainsAny(normalized, MachineHealthWords))
            return MachineHealthReply(health, machineHealth);
        if (ContainsAny(normalized, CDriveWords))
            return CDriveReply(health, profiles);
        if (ContainsAny(normalized, ApplicationWords))
            return ApplicationsReply(profiles);

        return GeneralReply(health, profiles);
    }

    public static AgentConversationReply ExplainSkill(
        AgentSkillCategory category,
        HealthCheckSummary? health,
        IEnumerable<SoftwareProfile>? softwareProfiles,
        MachineHealthObservation? machineHealth = null)
    {
        var profiles = (softwareProfiles ?? []).ToList();
        return category switch
        {
            AgentSkillCategory.SystemDiagnosis => DiagnosisSkillReply(health),
            AgentSkillCategory.SystemSettings => Reply(
                AgentQuestionIntent.SkillOverview,
                "先选择你要查看的设置",
                "可以选择网络、蓝牙、声音、显示、电源、存储、已安装应用或启动应用。Agent 只会解释并打开对应的 Windows 设置页，不会替你切换开关。",
                ["右侧的 Windows 设置直达只包含固定白名单入口。"],
                ["在右侧列表选择一个设置入口，或在问题框描述具体设置问题。"]),
            AgentSkillCategory.Troubleshooting => Reply(
                AgentQuestionIntent.Troubleshooting,
                "先描述具体故障表现",
                "请描述什么时候发生、看到了什么提示、是否每次都会发生。只有“电脑有问题”还不能判断根因，Agent 不会先批量修复。",
                ["可先分类网络、驱动、闪退、蓝屏、声音或显示问题。"],
                ["在问题框输入具体表现；需要时再打开设备管理器或事件查看器查看证据。"]),
            AgentSkillCategory.WindowAndDesktop => Reply(
                AgentQuestionIntent.SkillOverview,
                "桌面和窗口整理目前还没有开放",
                "当前版本还没有读取窗口标题、桌面图标或多显示器布局，因此不会假装已经生成整理方案。",
                ["没有窗口或桌面状态证据。"],
                ["这项能力先保留为规划项；不要根据空白建议批量移动图标或关闭窗口。"]),
            AgentSkillCategory.ProcessAndServiceManagement => StartupReply(profiles),
            AgentSkillCategory.HardwareInfo => HardwareReply(health, machineHealth),
            AgentSkillCategory.SystemTools => Reply(
                AgentQuestionIntent.SystemTool,
                "先选择要查看的 Windows 工具",
                "请在右侧系统工具列表选择任务管理器、回收站、设备管理器、磁盘管理、事件查看器、安全中心或注册表编辑器。Agent 只负责打开入口。",
                ["高风险工具会先显示单独确认。"],
                ["先选择与你的问题对应的工具；看不懂时不要点击修改、删除或结束按钮。"]),
            AgentSkillCategory.InputAndSession => Reply(
                AgentQuestionIntent.SkillOverview,
                "会话控制目前不提供执行",
                "当前版本不提供锁屏、休眠、关机或重启的执行入口，避免一句话或误点中断你的工作。",
                ["没有创建会话控制操作。"],
                ["需要这些动作时请使用 Windows 开始菜单，并先保存正在编辑的内容。"]),
            _ => Reply(
                AgentQuestionIntent.SkillOverview,
                "这个能力还没有可用入口",
                "当前版本没有足够证据和安全流程，所以不会假装可以执行。",
                ["没有可引用的本地能力证据。"],
                ["先选择其他已经可用的体检、应用或系统工具能力。"])
        };
    }

    private static AgentConversationReply DiagnosisSkillReply(
        HealthCheckSummary? health,
        AgentQuestionIntent intent = AgentQuestionIntent.SkillOverview)
    {
        if (health is null)
        {
            return Reply(
                intent,
                "还没有完成整机体检",
                "当前没有本机体检结果，Agent 不会猜 C 盘、内存、电池或后台状态。",
                ["没有可引用的本次体检证据。"],
                ["Agent 可以先完成一次只读体检；也可以打开首页查看扫描过程。"],
                navigationTargetPage: "Home",
                navigationLabel: "去首页体检");
        }

        return Reply(
            intent,
            "本次电脑体检已有结果",
            $"当前综合评分 {health.OverallScore} 分。评分仍以磁盘空间为主，其他机器状态作为单独观察，不会悄悄改变分数。",
            health.Dimensions
                .Take(4)
                .Select(dimension =>
                    $"{dimension.Name}：{BeginnerSafeEvidence(dimension.Result, "本次结果已隐藏")}；{dimension.Rating}")
                .ToArray(),
            ["回到首页查看完整体检表和关键发现。"],
            navigationTargetPage: "Home",
            navigationLabel: "查看体检摘要");
    }

    private static AgentConversationReply? ShortcutReply(string question)
    {
        var systemToolId = MatchSystemTool(question);
        if (systemToolId is not null)
        {
            var shortcut = SystemToolShortcutCatalog.FindById(systemToolId);
            if (shortcut is null)
                return null;

            var recycleBin = systemToolId == SystemToolShortcutCatalog.RecycleBinId;
            var troubleshooting = systemToolId is "device-manager" or "event-viewer";
            return Reply(
                troubleshooting ? AgentQuestionIntent.Troubleshooting : AgentQuestionIntent.SystemTool,
                recycleBin ? "先打开回收站查看" : "先打开" + shortcut.Name + "查看证据",
                recycleBin
                    ? "我只会打开 Windows 回收站让你查看，不会清空、删除或还原任何文件。清空后通常不能还原，所以请先确认没有要恢复的内容。"
                    : troubleshooting
                    ? "我现在不能只凭一句话确定故障根因，但可以带你到 Windows 的只读查看入口，先确认驱动、设备或错误记录。"
                    : "这是 Windows 自带工具入口。OMNIX 只负责帮你找到它，不会替你点击其中的修改、删除或结束按钮。",
                [shortcut.Description, shortcut.SafetyHint],
                recycleBin
                    ? ["先看文件名和删除时间。", "确认没有要恢复的内容后，再由你自己决定是否在 Windows 中清空。"]
                    : ["先查看当前状态和时间点。", "看不懂时回到 OMNIX 描述你看到的错误，不要先批量修改。"],
                navigationLabel: recycleBin ? "打开回收站查看" : "打开" + shortcut.Name,
                shortcutKind: AgentShortcutKind.SystemTool,
                shortcutId: shortcut.Id);
        }

        var settingsId = MatchWindowsSettings(question);
        if (settingsId is null)
            return null;

        var settings = WindowsSettingsShortcutCatalog.FindById(settingsId);
        if (settings is null)
            return null;

        return Reply(
            AgentQuestionIntent.WindowsSettings,
            "可以先查看" + settings.Name + "设置",
            "我可以把你带到准确的 Windows 设置页面，并解释应该看什么；不会替你切换开关、删除设备、卸载软件或修改配置。",
            [settings.Description, settings.SafetyHint],
            ["打开页面后先看当前状态，不急着修改。", "如果仍不确定，把页面上的状态告诉我，再继续判断。"],
            navigationLabel: "打开" + settings.Name,
            shortcutKind: AgentShortcutKind.WindowsSettings,
            shortcutId: settings.Id);
    }

    private static string? MatchSystemTool(string question)
    {
        if (ContainsAny(question, ["回收站"]))
            return SystemToolShortcutCatalog.RecycleBinId;
        if (ContainsAny(question, ["注册表编辑器", "regedit"]))
            return "registry-editor";
        if (ContainsAny(question, ["设备管理器", "驱动问题", "驱动异常", "设备异常", "设备驱动"]))
            return "device-manager";
        if (ContainsAny(question, ["事件查看器", "蓝屏", "闪退", "崩溃日志", "错误日志"]))
            return "event-viewer";
        if (ContainsAny(question, ["磁盘管理", "分区管理", "改盘符"]))
            return "disk-management";
        if (ContainsAny(question, ["任务管理器", "task manager"]))
            return "task-manager";
        if (ContainsAny(question, ["Windows 安全中心", "windows安全中心", "病毒防护", "防火墙状态"]))
            return "windows-security";
        return null;
    }

    private static string? MatchWindowsSettings(string question)
    {
        if (ContainsAny(question,
            ["新应用保存位置", "新应用默认保存", "新应用保存到哪里", "新应用装到哪里", "默认安装到哪个盘"]))
            return "default-save-locations";
        if (ContainsAny(question, ["已安装应用设置", "windows应用列表", "系统应用列表"]))
            return "installed-apps";
        if (ContainsAny(question, ["启动应用设置", "windows启动设置"]))
            return "startup-apps";
        if (ContainsAny(question, ["存储设置", "存储感知"]))
            return "storage";
        if (ContainsAny(question, ["wi-fi", "wifi", "网络设置", "网络连不上", "网络断开"]))
            return "network";
        if (ContainsAny(question, ["蓝牙", "配对设备"]))
            return "bluetooth";
        if (ContainsAny(question, ["声音设置", "没有声音", "麦克风", "扬声器", "音量设置"]))
            return "sound";
        if (ContainsAny(question, ["显示设置", "分辨率", "缩放设置", "多屏设置", "显示器设置"]))
            return "display";
        if (ContainsAny(question, ["电源设置", "睡眠设置", "休眠设置", "不睡眠", "电源计划"]))
            return "power";
        return null;
    }

    public static AgentConversationReply TargetUnavailable() =>
        Reply(
            AgentQuestionIntent.ApplicationSpecific,
            "暂时不能确定对应应用",
            "应用画像已经变化、缺失或存在同名记录。为了避免打开错误的卸载、迁移或缓存方案，我停止了这次定位。",
            ["旧回答不会继续作为应用身份依据。"],
            ["打开应用管理重新扫描并确认具体应用。"],
            navigationTargetPage: "Apps",
            navigationLabel: "打开应用管理");

    private static AgentConversationReply EmptyReply(
        HealthCheckSummary? health,
        IReadOnlyList<SoftwareProfile> profiles) =>
        Reply(
            AgentQuestionIntent.Empty,
            "还没有收到问题",
            health is null && profiles.Count == 0
                ? "我还没有本机扫描证据。可以先做体检或扫描应用，再根据结果回答。"
                : "可以询问 C 盘、软件、自启动、安装位置、迁移、卸载或还原；我只使用当前本地扫描摘要。",
            health is null && profiles.Count == 0
                ? ["没有 C 盘体检结果。", "没有应用扫描结果。"]
                : EvidenceAvailability(health, profiles),
            ["先说清楚你最担心的问题；没有证据时我会明确告诉你。"],
            navigationTargetPage: health is null ? "Home" : null,
            navigationLabel: health is null ? "去首页体检" : null);

    private static AgentConversationReply CDriveReply(
        HealthCheckSummary? health,
        IReadOnlyList<SoftwareProfile> profiles)
    {
        if (health is null)
        {
            return Reply(
                AgentQuestionIntent.CDrive,
                "需要先体检 C 盘",
                "目前没有本次 C 盘扫描结果，所以不能判断是谁占满了空间。",
                ["应用扫描不能替代磁盘体检。", $"当前已有 {profiles.Count} 个应用画像，但没有磁盘占用结论。"],
                ["打开首页完成一次只读体检。", "体检后再看增长来源和低风险清理建议。"],
                navigationTargetPage: "Home",
                navigationLabel: "去首页体检");
        }

        var disk = health.Dimensions.FirstOrDefault(item =>
            item.Name.Contains("磁盘", StringComparison.OrdinalIgnoreCase));
        var cleanCount = health.KeyFindings.Count(item =>
            HealthFindingRiskPolicy.IsLowRiskClean(item.Action, item.Risk));
        var higherRiskCleanCount = health.KeyFindings.Count(item =>
            HealthFindingRiskPolicy.IsHigherRiskClean(item.Action, item.Risk));
        var growthCount = health.KeyFindings.Count(item =>
            item.Kind == HealthFindingKind.SustainedGrowth);
        var candidates = AgentActionCandidateCatalog.Create(profiles);
        var cleanupGuidance = cleanCount > 0
            ? "先处理低风险候选，再观察哪些内容会继续增长。"
            : higherRiskCleanCount > 0
                ? "当前没有低风险清理候选；风险偏高项只观察，先补齐快照和回滚。"
                : "当前未发现低风险清理候选，先观察持续增长来源。";
        var cleanupNextSteps = cleanCount > 0
            ? new[] { "打开 C 盘清理页查看根因。", "只选择低风险项目，确认后先进入隔离区。" }
            : higherRiskCleanCount > 0
                ? new[] { "打开 C 盘清理页查看根因。", "当前只有风险偏高项，不进入隔离处理；先保持观察。" }
                : new[] { "打开 C 盘清理页查看根因。", "暂时没有低风险处理项，先保持观察。" };

        return Reply(
            AgentQuestionIntent.CDrive,
            "C 盘先看可清理项和持续增长",
            disk is null
                ? "体检已经完成，但磁盘摘要不完整；建议打开 C 盘页查看本次证据。"
                : $"本次磁盘结果：{BeginnerSafeEvidence(disk.Result, "占用摘要已生成，详细路径已隐藏")}。{cleanupGuidance}",
            [
                $"综合评分 {health.OverallScore} 分。",
                $"低风险清理发现 {cleanCount} 项；风险偏高清理提醒 {higherRiskCleanCount} 项；持续增长提醒 {growthCount} 项。",
                $"有 {candidates.OrdinaryCDriveProfiles.Count} 个普通应用被观察到安装或写入 C 盘。",
                $"另有 {candidates.ReadOnlyCDriveProfiles.Count} 个系统相关或归属待确认项也有 C 盘线索，仅供查看。"
            ],
            cleanupNextSteps,
            navigationTargetPage: "CDrive",
            navigationLabel: "打开 C 盘清理");
    }

    private static AgentConversationReply MachineHealthReply(
        HealthCheckSummary? health,
        MachineHealthObservation? machineHealth)
    {
        if (health is null && machineHealth is null)
        {
            return Reply(
                AgentQuestionIntent.MachineHealth,
                "还没有读到电脑状态",
                "目前没有 D 盘、内存、进程或电池的本次只读摘要，所以我不会猜电脑为什么卡。",
                ["没有可引用的本机健康观察。"],
                ["打开首页完成一次手动体检，或重新提问让 Agent 再做一次只读读取。"],
                navigationTargetPage: "Home",
                navigationLabel: "去首页体检");
        }

        var names = new HashSet<string>(StringComparer.Ordinal)
        {
            "D 盘空间",
            "内存占用",
            "电池状态",
            "使用趋势"
        };
        var dimensions = health is null
            ? MachineHealthPresentationBuilder.CreateDimensions(machineHealth).ToArray()
            : health.Dimensions
                .Where(dimension => names.Contains(dimension.Name))
                .Take(4)
                .ToArray();
        if (dimensions.Length == 0)
        {
            return Reply(
                AgentQuestionIntent.MachineHealth,
                "本次体检没有机器状态摘要",
                "C 盘结果可能已经存在，但 D 盘、内存和电池数据没有成功读取；我不会用估计值补上。",
                ["缺少可验证的机器状态维度。"],
                ["回到首页重新做一次手动体检。"],
                navigationTargetPage: "Home",
                navigationLabel: "重新体检");
        }

        var hasObservedValue = health is not null
            || machineHealth is not null
                && MachineHealthPresentationBuilder.HasAnyMachineStateValue(machineHealth);
        if (!hasObservedValue)
        {
            return Reply(
                AgentQuestionIntent.MachineHealth,
                "这次没有读到机器状态",
                "Agent 已尝试只读读取，但 D 盘、内存、电池和进程数量都没有返回可验证结果；我不会把未知说成正常。",
                dimensions.Select(dimension =>
                    $"{dimension.Name}：{dimension.Result}；{dimension.Rating}")
                    .ToArray(),
                ["可以稍后重试；如果电脑仍然卡，请描述发生时间和正在使用的软件。"]);
        }

        var attention = dimensions.Count(dimension =>
            dimension.Rating is "需要关注" or "有优化空间" or "建议观察"
                or "本次偏高" or "电量较低" or "线索较多");
        return Reply(
            AgentQuestionIntent.MachineHealth,
            attention == 0 ? "本次机器状态没有明显警报" : $"本次有 {attention} 项值得先观察",
            "这些结论来自刚才的只读采样。内存和进程数量会随使用变化，一次偏高不等于电脑故障。",
            dimensions.Select(dimension =>
                $"{dimension.Name}：{BeginnerSafeEvidence(dimension.Result, "本次没有可显示的数据")}；{dimension.Rating}")
                .ToArray(),
            health is null
                ? ["先关闭自己确认不用的普通应用；不要批量结束未知进程或禁用系统服务。"]
                : ["回到首页查看完整体检表。", "先关闭自己确认不用的普通应用；不要批量结束未知进程或禁用系统服务。"],
            navigationTargetPage: health is null ? null : "Home",
            navigationLabel: health is null ? null : "查看体检摘要");
    }

    private static AgentConversationReply HardwareReply(
        HealthCheckSummary? health,
        MachineHealthObservation? machineHealth)
    {
        var healthHardware = health?.Hardware;
        var hardware = healthHardware?.Availability == MachineMetricAvailability.Available
            ? healthHardware
            : machineHealth?.Hardware ?? healthHardware;
        if (hardware is null || hardware.Availability != MachineMetricAvailability.Available)
        {
            return Reply(
                AgentQuestionIntent.HardwareInfo,
                machineHealth is null ? "需要先读取一次电脑配置" : "这次没有读到电脑配置",
                machineHealth is null
                    ? "当前没有本次只读硬件摘要，所以我不会猜 CPU、显卡或 Windows 版本。"
                    : "Agent 已尝试只读读取，但没有返回可验证的 CPU、显卡或 Windows 版本；我不会用猜测补上。",
                ["没有可引用的本机硬件配置证据。"],
                machineHealth is null
                    ? ["打开首页完成一次手动只读体检。"]
                    : ["完整体检时会再次尝试；不需要为读取配置授予系统修改权限。"],
                navigationTargetPage: machineHealth is null ? "Home" : null,
                navigationLabel: machineHealth is null ? "去首页体检" : null);
        }

        var evidence = new List<string>();
        var cpu = BeginnerSafeEvidence(hardware.CpuName, "处理器名称未读取到");
        var logical = hardware.LogicalProcessorCount is > 0
            ? $"，{hardware.LogicalProcessorCount.Value} 个逻辑处理器"
            : string.Empty;
        evidence.Add("处理器：" + cpu + logical);
        evidence.Add("显卡：" + BeginnerSafeEvidence(hardware.GpuName, "显卡名称未读取到"));
        evidence.Add("系统：" + BeginnerSafeEvidence(hardware.OperatingSystem, "Windows 版本未读取到"));
        evidence.Add("架构：" + BeginnerSafeEvidence(hardware.Architecture, "系统架构未读取到"));

        return Reply(
            AgentQuestionIntent.HardwareInfo,
            "这是本次读取到的电脑配置",
            "这是本次只读读取到的本机摘要。它能说明硬件名称和系统架构，但不能只凭名称保证某个软件或游戏一定能流畅运行。",
            evidence,
            health is null
                ? ["要判断具体软件或游戏，请提供名称和官方最低配置、推荐配置。"]
                : ["要判断具体软件或游戏，请提供名称和官方最低配置、推荐配置。", "回到首页可以重新读取当前配置。"],
            navigationTargetPage: health is null ? null : "Home",
            navigationLabel: health is null ? null : "查看体检摘要");
    }

    private static bool HasMachineDimensions(HealthCheckSummary? health)
    {
        if (health is null)
            return false;

        return health.Dimensions.Any(dimension =>
            dimension.Name is "D 盘空间" or "内存占用" or "电池状态");
    }

    private static AgentConversationReply ApplicationsReply(IReadOnlyList<SoftwareProfile> profiles)
    {
        if (profiles.Count == 0)
        {
            return Reply(
                AgentQuestionIntent.Applications,
                "需要先扫描应用",
                "当前没有应用画像，不能判断哪些软件占 C 盘、常驻后台或适合卸载。",
                ["应用列表尚未扫描。"],
                ["打开应用管理并执行只读扫描。"],
                navigationTargetPage: "Apps",
                navigationLabel: "去应用管理");
        }

        var candidates = AgentActionCandidateCatalog.Create(profiles);
        return Reply(
            AgentQuestionIntent.Applications,
            "应用画像已经可以帮你筛选",
            $"扫描到 {profiles.Count} 个应用；{candidates.OrdinaryCDriveProfiles.Count} 个普通应用有 C 盘线索，{candidates.ReadOnlyCDriveProfiles.Count} 个系统相关项只读；{candidates.OrdinaryResidentProfiles.Count} 个普通应用有后台常驻线索。",
            [
                "普通应用的 C 盘线索：" + JoinSafeNames(candidates.OrdinaryCDriveProfiles, "暂未发现"),
                "系统相关 C 盘线索（仅供查看）：" + JoinSafeNames(candidates.ReadOnlyCDriveProfiles, "暂未发现"),
                "普通后台常驻应用：" + JoinSafeNames(candidates.OrdinaryResidentProfiles, "暂未发现"),
                "系统相关后台线索（仅供查看）：" + JoinSafeNames(candidates.ReadOnlyResidentProfiles, "暂未发现")
            ],
            ["打开应用管理后用“占 C 盘”或“后台常驻”筛选。", "点开普通应用再看可用动作；系统相关和归属待确认项只查看。"],
            navigationTargetPage: "Apps",
            navigationLabel: "打开应用管理");
    }

    private static AgentConversationReply StartupReply(IReadOnlyList<SoftwareProfile> profiles)
    {
        if (profiles.Count == 0)
        {
            return Reply(
                AgentQuestionIntent.StartupAndBackground,
                "需要先扫描应用和后台线索",
                "当前没有应用画像，不能判断哪些软件会开机启动或常驻后台。",
                ["没有自启动、服务或计划任务的归属证据。"],
                ["先到应用管理执行只读扫描。"],
                navigationTargetPage: "Apps",
                navigationLabel: "去扫描应用",
                targetAppFilter: AppCatalogFilter.Resident);
        }

        var candidates = AgentActionCandidateCatalog.Create(profiles);
        var startup = candidates.OrdinaryStartupProfiles;
        var localReview = candidates.StartupReviewProfiles;
        var nameOnlyOrUnsupportedCount = candidates.UnsupportedOrdinaryStartupProfiles.Count;
        var serviceOrTask = profiles.Where(profile =>
            profile.Services.Count > 0 || profile.ScheduledTasks.Count > 0).ToList();
        return Reply(
            AgentQuestionIntent.StartupAndBackground,
            "先区分普通自启动和系统后台能力",
            $"发现 {startup.Count} 个应用有普通自启动线索，其中 {localReview.Count} 个普通应用具备本地审核线索、{nameOnlyOrUnsupportedCount} 个仍是名称级或不受支持线索；另有 {candidates.ReadOnlyStartupProfiles.Count} 个系统相关或归属待确认项仅供查看；{serviceOrTask.Count} 个应用带服务或计划任务。",
            [
                "普通自启动应用：" + JoinSafeNames(startup, "暂未发现"),
                "可审核的普通自启动应用：" + JoinSafeNames(localReview, "暂未发现"),
                "系统相关或归属待确认自启动（仅供查看）：" + JoinSafeNames(candidates.ReadOnlyStartupProfiles, "暂未发现"),
                nameOnlyOrUnsupportedCount > 0
                    ? $"另有 {nameOnlyOrUnsupportedCount} 个只有名称级线索、多个启动项或不受支持，需要重新扫描或到 Windows 页面确认。"
                    : "具备本地审核线索的项目仍会在打开后重新读取，不能直接执行。",
                "服务或计划任务不会按普通自启动直接处理。"
            ],
            [
                "在应用管理中打开具体应用；出现“审核关闭方案”时再查看影响和回滚方式。",
                "服务、计划任务和不确定的自启动项当前只解释或交给 Windows 官方页面。"
            ],
            navigationTargetPage: "Apps",
            navigationLabel: "查看后台常驻应用",
            targetAppFilter: AppCatalogFilter.Resident);
    }

    private static AgentConversationReply InstallReply() =>
        Reply(
            AgentQuestionIntent.InstallRouting,
            "安装前先让 Agent 检查安装包",
            "普通软件优先建议到 D:\\Software，游戏到 D:\\Game，AI 工具到 D:\\Agent，开发工具到 D:\\Development。具体安装器能否自动带入位置，需要先识别和核验。",
            ["不会全局修改 Windows ProgramFilesDir。", "不能可靠传入目录的安装器只提供引导，不猜参数。"],
            ["选择官网下载的安装包。", "查看发布者、安装器类型和推荐目录后再确认。"],
            navigationTargetPage: "Install",
            navigationLabel: "打开安装管控");

    private static AgentConversationReply MigrationReply(IReadOnlyList<SoftwareProfile> profiles)
    {
        var candidates = AgentActionCandidateCatalog.Create(profiles);
        return Reply(
            AgentQuestionIntent.Migration,
            "迁移不能只搬文件夹",
            profiles.Count == 0
                ? "当前没有应用画像，不能判断迁移候选。"
                : $"有 {candidates.MigrationReviewProfiles.Count} 个普通应用可评估主程序迁移；{candidates.DataLocationReviewProfiles.Count} 个普通应用只复查 C 盘数据位置；{candidates.ReadOnlyCDriveProfiles.Count} 个系统相关或归属待确认项仅供查看。",
            [
                "可评估主程序迁移：" + JoinSafeNames(candidates.MigrationReviewProfiles, "暂未发现"),
                "只复查 C 盘数据位置：" + JoinSafeNames(candidates.DataLocationReviewProfiles, "暂未发现"),
                "系统相关或归属待确认（仅供查看）：" + JoinSafeNames(candidates.ReadOnlyCDriveProfiles, "暂未发现"),
                "没有快照、验证和回滚方案时不会执行迁移。"
            ],
            ["到应用管理选择一个应用。", "先生成迁移方案，不要手动剪切安装目录。"],
            navigationTargetPage: "Apps",
            navigationLabel: "查看占 C 盘应用",
            targetAppFilter: AppCatalogFilter.CDrive);
    }

    private static AgentConversationReply UninstallReply(IReadOnlyList<SoftwareProfile> profiles)
    {
        var candidates = AgentActionCandidateCatalog.Create(profiles);
        return Reply(
            AgentQuestionIntent.Uninstall,
            "先选具体软件，再走官方卸载和残留复查",
            profiles.Count == 0
                ? "当前没有应用画像，不能判断哪个软件能安全卸载。"
                : $"已扫描应用中有 {candidates.UninstallReviewProfiles.Count} 个普通应用带可审核的官方卸载入口；{candidates.ReadOnlyUninstallProfiles.Count} 个系统相关或归属待确认项即使有命令也仅供查看。没有指定软件时，我不会替你猜。",
            [
                "可审核官方卸载：" + JoinSafeNames(candidates.UninstallReviewProfiles, "暂未发现"),
                "系统相关或归属待确认卸载线索（仅供查看）：" + JoinSafeNames(candidates.ReadOnlyUninstallProfiles, "暂未发现"),
                "官方卸载器必须先运行。",
                "卸载后只把确认过的低风险残留移入隔离区。"
            ],
            ["到应用管理选择软件。", "查看 Agent 的保留或卸载建议，再确认官方卸载方案。"],
            navigationTargetPage: "Apps",
            navigationLabel: "查看可审核卸载应用",
            targetAppFilter: AppCatalogFilter.Uninstallable);
    }

    private static AgentConversationReply RestoreReply() =>
        Reply(
            AgentQuestionIntent.Restore,
            "到后悔药中心查看可还原记录",
            "清理缓存和低风险残留进入隔离区后，会在时间线保留还原入口。还原时如果原位置已有新内容，系统会拒绝覆盖。",
            ["打开后悔药中心不会自动还原。", "只有带有效 manifest 的记录才会显示还原按钮。"],
            ["打开后悔药中心。", "选中记录并再次确认后才会尝试还原。"],
            navigationTargetPage: "Timeline",
            navigationLabel: "打开后悔药中心");

    private static AgentConversationReply ApplicationReply(
        string normalizedQuestion,
        SoftwareProfile profile,
        ApplicationCrashObservation? applicationCrashObservation,
        ApplicationRuntimeObservation? applicationRuntimeObservation,
        ApplicationGrowthObservation? applicationGrowthObservation)
    {
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var safeName = SafeAppName(profile.Name);
        var intent = AgentQuestionIntent.ApplicationSpecific;
        var headline = "关于 " + safeName;
        var targetAppHandoff = AgentApplicationHandoff.Details;
        var navigationLabel = "打开这个应用";
        string answer;
        IReadOnlyList<string> steps;
        IReadOnlyList<string> evidence =
            [drawer.InstallLocationSummary, drawer.SizeSummary, drawer.ResidencySummary];

        if (ContainsAny(normalizedQuestion, UninstallWords))
        {
            if (CanPrepareApplicationReview(profile, drawer, AppActionKind.Uninstall))
            {
                targetAppHandoff = AgentApplicationHandoff.UninstallReview;
                navigationLabel = "查看卸载安全方案";
            }
            answer = profile.Category == SoftwareCategory.SystemTool
                ? $"{safeName} 被识别为系统相关应用，建议保留，不进入普通卸载快捷流程。"
                : string.IsNullOrWhiteSpace(profile.UninstallCommand)
                    ? $"暂未找到 {safeName} 的可靠官方卸载入口，所以现在不建议强制删除。"
                    : $"{safeName} 有官方卸载入口；建议先看恢复准备，再运行官方卸载并复查低风险残留。";
            steps = ["打开这个应用的详情。", "选择“卸载干净点”查看方案，执行前仍需确认。"];
        }
        else if (ContainsAny(normalizedQuestion, MigrationWords))
        {
            if (CanPrepareApplicationReview(profile, drawer, AppActionKind.Migration))
            {
                targetAppHandoff = AgentApplicationHandoff.MigrationReview;
                navigationLabel = "查看迁移方案";
            }
            answer = drawer.MigrationSummary;
            steps = ["打开应用详情并生成迁移方案。", "没有快照和回滚时不要手动搬目录。"];
        }
        else if (ContainsAny(normalizedQuestion, StartupWords))
        {
            var canReviewStartup = CanPrepareApplicationReview(
                profile,
                drawer,
                AppActionKind.StartupControl);
            if (canReviewStartup)
            {
                targetAppHandoff = AgentApplicationHandoff.StartupControlReview;
                navigationLabel = "查看自启动方案";
            }
            if (canReviewStartup && StartupEntryControlPolicy.HasSingleSupportedObservation(profile))
            {
                answer = $"{safeName} 有一个受支持的普通自启动线索，可以在 OMNIX 中审核可还原的关闭方案。打开后会重新读取当前状态；证据变化就停止。";
                steps =
                [
                    "打开应用详情，选择“管理自启动”。",
                    "看到“审核关闭方案”后再查看影响；真正关闭仍需恢复证据和两次明确确认。"
                ];
            }
            else
            {
                var handoff = AppStartupSettingsHandoffPresenter.Create(profile);
                answer = handoff.Summary + " " + handoff.AgentTakeaway;
                steps = handoff.CanOpenStartupSettings
                    ? ["打开应用详情，选择“管理自启动”。", "本地条件不满足时再到 Windows 页面确认当前开关。"]
                    : ["打开应用详情查看原因。", "当前只观察，不直接修改服务或计划任务。"];
            }
        }
        else if (ContainsAny(normalizedQuestion, AppGrowthWords)
                 && !HasExplicitApplicationOperation(normalizedQuestion))
        {
            var growth = CreateApplicationGrowthAdvice(
                profile,
                safeName,
                applicationGrowthObservation);
            headline = growth.Headline;
            answer = growth.Answer;
            evidence = growth.Evidence;
            steps = growth.NextSteps;
            navigationLabel = "查看增长详情";
        }
        else if (ContainsAny(normalizedQuestion, CacheWords))
        {
            if (CanPrepareApplicationReview(profile, drawer, AppActionKind.CacheCleanup))
            {
                targetAppHandoff = AgentApplicationHandoff.CacheCleanupReview;
                navigationLabel = "查看缓存方案";
            }
            answer = drawer.CacheCleanupSummary;
            steps = ["打开应用详情查看缓存方案。", "只有低风险缓存在再次确认后才能进入隔离区。"];
        }
        else if (ContainsAny(normalizedQuestion, InstallWords)
                 || normalizedQuestion.Contains("位置", StringComparison.OrdinalIgnoreCase)
                 || normalizedQuestion.Contains("哪里", StringComparison.OrdinalIgnoreCase))
        {
            answer = drawer.InstallLocationSummary;
            steps = ["打开应用详情查看占用和最近增长。", "需要换位置时先判断应迁移、只迁缓存还是重新安装。"];
        }
        else if (CreateApplicationTroubleshootingAdvice(
                     normalizedQuestion,
                     profile,
                     safeName,
                     applicationCrashObservation,
                     applicationRuntimeObservation) is { } troubleshooting)
        {
            headline = troubleshooting.Headline;
            answer = troubleshooting.Answer;
            evidence = troubleshooting.Evidence;
            steps = troubleshooting.NextSteps;
        }
        else
        {
            answer = drawer.AgentAdvice.Text;
            steps = ["打开应用详情查看 Agent 建议。", "所有真实操作仍需生成方案并再次确认。"];
        }

        return Reply(
            intent,
            headline,
            answer,
            evidence,
            steps,
            navigationTargetPage: "Apps",
            navigationLabel: navigationLabel,
            targetAppName: profile.Name,
            targetAppHandoff: targetAppHandoff);
    }

    private static bool CanPrepareApplicationReview(
        SoftwareProfile profile,
        AppDrawerViewModel drawer,
        AppActionKind kind) =>
        profile.Category != SoftwareCategory.SystemTool
        && drawer.AvailableActions.Any(action => action.Kind == kind && action.IsEnabled);

    private static ApplicationGrowthAdvice CreateApplicationGrowthAdvice(
        SoftwareProfile profile,
        string safeName,
        ApplicationGrowthObservation? observation)
    {
        var matched = observation is not null
            && string.Equals(
                observation.SoftwareName?.Trim(),
                profile.Name?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        var effective = matched ? observation : null;
        var headline = safeName + " 的增长结论";
        string answer;
        if (effective is null
            || effective.Availability == ApplicationGrowthObservationAvailability.Unavailable)
        {
            answer = "本次没有形成可用的增长对比，所以不能判断它最近是否变大；我不会把未知说成正常。";
        }
        else if (effective.Availability == ApplicationGrowthObservationAvailability.InsufficientBaseline)
        {
            answer = $"目前只有 {Math.Max(1, effective.ObservedSnapshotCount)} 次体检记录，这只是基线，不能判断它是否持续增长。";
        }
        else if (effective.RecentGrowthBytes > 0)
        {
            answer = $"最近 {Math.Max(2, effective.ObservedSnapshotCount)} 次体检的最新对比确认增加了 {FormatBytes(effective.RecentGrowthBytes)}；这只能说明占用变大，不能单独证明是缓存、日志还是必要数据。";
        }
        else
        {
            answer = "最近一次对比没有确认到增长；这只代表当前比较窗口，不代表以后不会增长。";
        }

        var snapshots = Math.Max(0, effective?.ObservedSnapshotCount ?? 0);
        var growth = Math.Max(0, effective?.RecentGrowthBytes ?? 0);
        var cDriveLocations = Math.Max(0, effective?.CDriveWriteLocationCount ?? 0);
        var cacheLocations = Math.Max(0, effective?.CacheLocationCount ?? 0);
        var evidence = effective?.Availability switch
        {
            ApplicationGrowthObservationAvailability.Available =>
                new[]
                {
                    $"趋势证据：{snapshots} 次体检，最新对比增长 {FormatBytes(growth)}。",
                    $"C 盘写入线索：{cDriveLocations} 个位置；缓存线索：{cacheLocations} 个位置。"
                },
            ApplicationGrowthObservationAvailability.InsufficientBaseline =>
                new[]
                {
                    $"趋势证据：{Math.Max(1, snapshots)} 次体检，目前只有基线。",
                    $"C 盘写入线索：{cDriveLocations} 个位置；缓存线索：{cacheLocations} 个位置。"
                },
            _ =>
                new[]
                {
                    "趋势证据：本次不可用，不能判断增长量。",
                    $"C 盘写入线索：{cDriveLocations} 个位置；缓存线索：{cacheLocations} 个位置。"
                }
        };

        var immediate = cacheLocations > 0
            ? "现在腾空间：打开应用详情查看缓存方案；只有明确的低风险缓存才能在确认后进入隔离区。"
            : "现在腾空间：先在应用详情确认安装、数据和缓存占用；没有明确缓存证据时不生成清理。";
        var prevention = PreventionStep(profile, cDriveLocations, snapshots);
        return new ApplicationGrowthAdvice(
            headline,
            answer,
            evidence,
            [immediate, prevention]);
    }

    private static string PreventionStep(
        SoftwareProfile profile,
        int cDriveLocationCount,
        int snapshotCount)
    {
        if (cDriveLocationCount > 0 && IsInstalledOnDrive(profile, 'D'))
        {
            return $"以后防止继续增长：主程序虽在 D 盘，但仍有 {cDriveLocationCount} 个 C 盘写入线索；先在应用设置中确认缓存、日志或数据位置，不要只搬目录。";
        }

        if (cDriveLocationCount > 0 && IsInstalledOnDrive(profile, 'C'))
        {
            return "以后防止继续增长：先评估迁移、只迁缓存还是重新安装；没有快照、验证和回滚时不要手动搬目录。";
        }

        return snapshotCount < 2
            ? "以后防止继续增长：保留这次基线，隔一段时间再次体检后再判断趋势。"
            : "以后防止继续增长：继续观察后续体检；没有明确 C 盘写入线索时不猜测配置位置。";
    }

    private static bool IsInstalledOnDrive(SoftwareProfile profile, char driveLetter)
    {
        try
        {
            var root = Path.GetPathRoot(profile.InstallPath);
            return root is not null
                && root.Equals($"{char.ToUpperInvariant(driveLetter)}:\\", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool HasExplicitApplicationOperation(string question) =>
        ContainsAny(
            question,
            UninstallWords
                .Concat(MigrationWords)
                .Concat(StartupWords)
                .Concat(InstallWords))
        || ContainsAny(question, ["清理", "清缓存", "删除缓存"]);

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1024L * 1024 * 1024)
            return $"{bytes / (1024d * 1024 * 1024):0.0} GB";
        if (bytes >= 1024L * 1024)
            return $"{bytes / (1024d * 1024):0.0} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024d:0.0} KB";
        return bytes + " B";
    }

    private static ApplicationTroubleshootingAdvice? CreateApplicationTroubleshootingAdvice(
        string question,
        SoftwareProfile profile,
        string safeName,
        ApplicationCrashObservation? applicationCrashObservation,
        ApplicationRuntimeObservation? applicationRuntimeObservation)
    {
        if (ContainsAny(question, AppCrashWords))
        {
            return new ApplicationTroubleshootingAdvice(
                $"{safeName} 发生闪退，先确认时间和错误记录",
                "当前应用画像没有崩溃日志，因此不能判断根因。现有安装和后台状态也不能证明是软件自身、配置还是 Windows 组件导致。",
                ApplicationTroubleshootingEvidence(
                    profile,
                    applicationCrashObservation,
                    null,
                    "闪退"),
                [
                    "先正常关闭并重新打开一次，记下发生时间和屏幕上的提示。",
                    "如果再次发生，可以再问“打开事件查看器”；它只查看 Windows 错误记录，不会删除日志。"
                ]);
        }

        if (ContainsAny(question, AppFreezeWords))
        {
            return new ApplicationTroubleshootingAdvice(
                $"{safeName} 卡死或无响应，先确认它现在是否仍在运行",
                ApplicationFreezeAnswer(profile, applicationRuntimeObservation),
                ApplicationTroubleshootingEvidence(
                    profile,
                    applicationCrashObservation,
                    applicationRuntimeObservation,
                    "卡死或无响应"),
                [
                    "先等待片刻并尝试在应用内正常关闭，不要批量结束未知进程。",
                    "如果仍无响应，可以再问“打开任务管理器”查看状态；OMNIX 只打开查看，不会结束进程。"
                ]);
        }

        if (ContainsAny(question, AppResourceWords))
        {
            return new ApplicationTroubleshootingAdvice(
                $"{safeName} 当前资源占用",
                ApplicationResourceAnswer(profile, applicationRuntimeObservation),
                [ApplicationRuntimeEvidence(profile, applicationRuntimeObservation)],
                [
                    "先观察它是否正在执行你需要的任务，不要只凭一次采样结束进程。",
                    "需要比较时可以稍后再问一次，或打开应用详情查看后台和占用结论。"
                ]);
        }

        if (ContainsAny(question, AppVagueProblemWords))
        {
            return new ApplicationTroubleshootingAdvice(
                $"还需要你描述 {safeName} 哪里不正常",
                "目前只知道你认为它有异常，不能判断是闪退、卡死、报错、启动失败、网络问题还是占用变大。现有应用画像不能替代故障现象和发生时间。",
                ApplicationTroubleshootingEvidence(
                    profile,
                    applicationCrashObservation,
                    null,
                    "异常"),
                [
                    "说明具体表现、出现时间、是否每次发生，以及屏幕上有没有提示。",
                    "先打开应用详情查看当前安装、后台和占用结论；不要先卸载、删目录或结束未知进程。"
                ]);
        }

        return null;
    }

    private static IReadOnlyList<string> ApplicationTroubleshootingEvidence(
        SoftwareProfile profile,
        ApplicationCrashObservation? applicationCrashObservation,
        ApplicationRuntimeObservation? applicationRuntimeObservation,
        string symptom)
    {
        var running = profile.RunningProcesses.Count == 0
            ? "当前状态：本次扫描没有看到它正在运行；这不代表它从未启动。"
            : $"当前状态：扫描时发现 {profile.RunningProcesses.Count} 个正在运行的进程；这里只显示数量。";
        var background = profile.StartupEntries.Count == 0
            && profile.Services.Count == 0
            && profile.ScheduledTasks.Count == 0
                ? "后台线索：未发现自启动、服务或计划任务。"
                : $"后台线索：{profile.StartupEntries.Count} 项自启动、{profile.Services.Count} 项服务、{profile.ScheduledTasks.Count} 项计划任务。";
        var evidence = new List<string> { running, background };
        if (applicationRuntimeObservation is not null)
            evidence.Add(ApplicationRuntimeEvidence(profile, applicationRuntimeObservation));
        evidence.Add(ApplicationCrashEvidence(
            profile,
            applicationCrashObservation,
            symptom));
        return evidence;
    }

    private static string ApplicationFreezeAnswer(
        SoftwareProfile profile,
        ApplicationRuntimeObservation? observation)
    {
        if (!RuntimeObservationMatches(profile, observation))
        {
            return "当前应用画像只记录运行进程的数量，不包含这个应用的 CPU、内存或窗口响应采样，因此不能判断卡死原因。";
        }

        return observation!.Availability switch
        {
            ApplicationRuntimeObservationAvailability.Available =>
                "这次只读短时采样提供了当前运行和资源线索，但仍不能判断卡死根因；一次采样也不能证明应用必须被结束。",
            ApplicationRuntimeObservationAvailability.NotRunning =>
                "重新观察时没有看到它正在运行，可能已经退出或完成关闭；这不代表应用之前没有卡死。",
            _ => "这次没有成功读取运行状态，因此仍不能判断卡死原因；我不会把未知说成正常。"
        };
    }

    private static string ApplicationResourceAnswer(
        SoftwareProfile profile,
        ApplicationRuntimeObservation? observation)
    {
        if (!RuntimeObservationMatches(profile, observation))
            return "本次还没有取得这个应用的短时 CPU 和内存采样，不能判断它当前是否占用过高。";

        return observation!.Availability switch
        {
            ApplicationRuntimeObservationAvailability.Available =>
                "已完成一次只读短时采样。它只能说明当前片刻的聚合占用，不能单独证明应用异常，也不会据此结束进程。",
            ApplicationRuntimeObservationAvailability.NotRunning =>
                "重新观察时没有看到它正在运行，因此没有当前资源占用可比较；这不代表应用一直没有运行。",
            _ => "这次没有成功读取运行状态，因此不能判断当前占用；我不会把未知说成正常。"
        };
    }

    private static string ApplicationRuntimeEvidence(
        SoftwareProfile profile,
        ApplicationRuntimeObservation? observation)
    {
        if (!RuntimeObservationMatches(profile, observation))
            return "短时资源：本次尚未重新读取 CPU 和内存状态。";

        return observation!.Availability switch
        {
            ApplicationRuntimeObservationAvailability.Available =>
                $"短时资源：约 {observation.SampleDurationMilliseconds} 毫秒内匹配到 {observation.MatchedProcessCount} 个运行进程，合计内存约 {FormatRuntimeBytes(observation.TotalWorkingSetBytes)}，CPU 活动{CpuActivityText(observation.CpuActivity)}；这是瞬时聚合，不能单独证明故障原因。",
            ApplicationRuntimeObservationAvailability.NotRunning =>
                "短时资源：重新观察时没有看到它正在运行；这不代表应用一直没有运行。",
            _ => "短时资源：没有成功读取运行状态；我不会把未知说成正常。"
        };
    }

    private static bool RuntimeObservationMatches(
        SoftwareProfile profile,
        ApplicationRuntimeObservation? observation) =>
        observation is not null
        && observation.SoftwareName.Equals(
            profile.Name,
            StringComparison.CurrentCultureIgnoreCase);

    private static string CpuActivityText(ApplicationCpuActivity activity) =>
        activity switch
        {
            ApplicationCpuActivity.Idle => "几乎没有",
            ApplicationCpuActivity.Low => "较低",
            ApplicationCpuActivity.Moderate => "中等",
            ApplicationCpuActivity.High => "较高",
            _ => "无法判断"
        };

    private static string FormatRuntimeBytes(long bytes)
    {
        var safe = Math.Max(0, bytes);
        if (safe >= 1024L * 1024 * 1024)
            return $"{safe / (1024d * 1024 * 1024):0.0} GB";
        if (safe >= 1024L * 1024)
            return $"{safe / (1024d * 1024):0.0} MB";
        return $"{safe / 1024d:0.0} KB";
    }

    private static string ApplicationCrashEvidence(
        SoftwareProfile profile,
        ApplicationCrashObservation? observation,
        string symptom)
    {
        if (observation is null
            || !observation.SoftwareName.Equals(
                profile.Name,
                StringComparison.CurrentCultureIgnoreCase))
        {
            return "错误记录：本次尚未读取 Windows 崩溃日志。";
        }

        return observation.Availability switch
        {
            ApplicationCrashObservationAvailability.Available when observation.MatchCount > 0 =>
                $"错误记录：最近 24 小时找到 {observation.MatchCount} 条匹配记录，最近一次是 {observation.LatestOccurrenceUtc?.ToLocalTime():yyyy-MM-dd HH:mm}；这些记录不能单独证明根因。",
            ApplicationCrashObservationAvailability.NotFound =>
                $"错误记录：最近 24 小时没有找到匹配记录；这不代表没有发生{symptom}。",
            ApplicationCrashObservationAvailability.Unavailable =>
                "错误记录：没有成功读取 Windows 错误记录；我不会把未知说成正常。",
            _ => "错误记录：本次读取没有形成可用结论。"
        };
    }

    private static AgentConversationReply AmbiguousApplicationReply() =>
        Reply(
            AgentQuestionIntent.ApplicationSpecific,
            "需要先确认具体是哪一个应用",
            "问题里对应到多个应用或同名记录。为了避免打开错误的卸载、迁移或缓存方案，我不会自动选择。",
            ["应用名称不是唯一身份。"],
            ["打开应用管理，使用搜索和安装位置确认具体项目。"],
            navigationTargetPage: "Apps",
            navigationLabel: "打开应用管理");

    private static AgentConversationReply GeneralReply(
        HealthCheckSummary? health,
        IReadOnlyList<SoftwareProfile> profiles)
    {
        var next = AgentNextStepPresenter.Create(health, profiles);
        return Reply(
            AgentQuestionIntent.General,
            BeginnerSafeEvidence(next.Title, "先查看本机体检摘要"),
            BeginnerSafeEvidence(next.Summary, "本地摘要已生成，详细路径已隐藏。"),
            EvidenceAvailability(health, profiles),
            next.SafeNextActions
                .Take(3)
                .Select(action => BeginnerSafeEvidence(action, "打开对应页面查看安全建议。"))
                .ToArray(),
            navigationTargetPage: health is null ? "Home" : null,
            navigationLabel: health is null ? "去首页体检" : null);
    }

    private static AgentConversationReply CapabilityReply() =>
        Reply(
            AgentQuestionIntent.General,
            "我可以帮你看懂和管理这台电脑",
            "我可以自动准备只读证据，帮你做电脑体检、分析 C 盘、管理应用和自启动，并为清理、迁移、卸载或安装位置给出安全方案。",
            [
                "体检和分析只读取当前状态，不会因为一句话修改电脑。",
                "能安全处理的动作也会先展示影响、风险和能否还原。"
            ],
            ["直接告诉我你担心什么，例如“C 盘为什么满了”或“微信能不能迁到 D 盘”。"]);

    private static AgentConversationReply Reply(
        AgentQuestionIntent intent,
        string headline,
        string answer,
        IReadOnlyList<string> evidence,
        IReadOnlyList<string> nextSteps,
        string? navigationTargetPage = null,
        string? navigationLabel = null,
        string? targetAppName = null,
        AgentApplicationHandoff targetAppHandoff = AgentApplicationHandoff.Details,
        AgentShortcutKind? shortcutKind = null,
        string? shortcutId = null,
        AppCatalogFilter? targetAppFilter = null) =>
        new()
        {
            Intent = intent,
            Headline = headline,
            Answer = answer,
            EvidenceLines = evidence,
            NextSteps = nextSteps,
            SafetyBoundary = "这次回答只解释本地摘要和打开安全入口，不会删除、迁移、卸载、禁用服务、改注册表或运行安装器。",
            PrivacyLine = "当前使用本地规则回答，没有调用云端 AI，也不会发送完整路径或文件内容。",
            NavigationTargetPage = navigationTargetPage,
            NavigationLabel = navigationLabel,
            TargetAppName = targetAppName,
            TargetAppFilter = targetAppFilter,
            TargetAppHandoff = targetAppHandoff,
            ShortcutKind = shortcutKind,
            ShortcutId = shortcutId
        };

    private static IReadOnlyList<string> EvidenceAvailability(
        HealthCheckSummary? health,
        IReadOnlyList<SoftwareProfile> profiles) =>
        [
            health is null ? "C 盘体检：暂无结果。" : $"C 盘体检：综合评分 {health.OverallScore} 分。",
            profiles.Count == 0 ? "应用画像：暂无结果。" : $"应用画像：已扫描 {profiles.Count} 个应用。"
        ];

    private static ProfileMention ResolveProfileMention(
        string normalizedQuestion,
        IReadOnlyList<SoftwareProfile> profiles)
    {
        var mentioned = profiles
            .Where(profile => !string.IsNullOrWhiteSpace(profile.Name) && profile.Name.Trim().Length >= 2)
            .GroupBy(profile => profile.Name.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Where(group => normalizedQuestion.Contains(group.Key, StringComparison.CurrentCultureIgnoreCase))
            .ToArray();

        if (mentioned.Length == 0)
            return new ProfileMention(null, false);
        if (mentioned.Length != 1 || mentioned[0].Count() != 1)
            return new ProfileMention(null, true);
        return new ProfileMention(mentioned[0].Single(), false);
    }

    private static string JoinSafeNames(
        IEnumerable<SoftwareProfile> profiles,
        string emptyText)
    {
        var names = profiles
            .Select(profile => SafeAppName(profile.Name))
            .Where(name => !name.Equals("这个应用", StringComparison.Ordinal))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Take(3)
            .ToArray();
        return names.Length == 0 ? emptyText : string.Join("、", names);
    }

    private static string SafeAppName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)
            || name.Contains(":\\", StringComparison.Ordinal)
            || name.Contains('/')
            || name.Contains('\\'))
        {
            return "这个应用";
        }

        return name.Trim();
    }

    private static string BeginnerSafeEvidence(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains(":\\", StringComparison.Ordinal)
            || value.Contains(":/", StringComparison.Ordinal)
            || value.StartsWith("\\\\", StringComparison.Ordinal))
        {
            return fallback;
        }

        return value.Trim();
    }

    private static bool ContainsAny(string value, IEnumerable<string> words) =>
        words.Any(word => value.Contains(word, StringComparison.CurrentCultureIgnoreCase));

    private static bool IsNonDiagnosticConversation(string? question)
    {
        var normalized = Normalize(question).TrimEnd(
            '。',
            '.',
            '？',
            '?',
            '！',
            '!',
            '，',
            ',',
            ' ');
        return NonDiagnosticConversationQuestions.Contains(normalized);
    }

    private static bool LooksLikeNamedApplicationTroubleshooting(string? question)
    {
        var normalized = Normalize(question);
        var firstSymptomIndex = AppCrashWords
            .Concat(AppFreezeWords)
            .Concat(AppVagueProblemWords)
            .Concat(AppResourceWords)
            .Select(word => normalized.IndexOf(word, StringComparison.CurrentCultureIgnoreCase))
            .Where(index => index >= 0)
            .DefaultIfEmpty(-1)
            .Min();
        if (firstSymptomIndex <= 0)
            return false;

        var subject = normalized[..firstSymptomIndex];
        foreach (var filler in new[]
                 {
                     "最近", "这几天", "总是", "经常", "有时", "偶尔", "我的", "这个", "那个",
                     "为什么", "请问", "帮我看看", "帮我", "看看"
                 })
        {
            subject = subject.Replace(filler, string.Empty, StringComparison.CurrentCultureIgnoreCase);
        }

        subject = subject.Trim(' ', '，', ',', '。', '.', '？', '?', '！', '!');
        subject = subject.TrimEnd('的');
        if (subject.Length < 2)
            return false;

        return subject is not ("软件" or "应用" or "程序" or "游戏" or "电脑" or "系统" or "机器" or "Windows");
    }

    private static string Normalize(string? question) => question?.Trim() ?? string.Empty;

    private sealed record ApplicationTroubleshootingAdvice(
        string Headline,
        string Answer,
        IReadOnlyList<string> Evidence,
        IReadOnlyList<string> NextSteps);

    private sealed record ApplicationGrowthAdvice(
        string Headline,
        string Answer,
        IReadOnlyList<string> Evidence,
        IReadOnlyList<string> NextSteps);

    private sealed record ProfileMention(SoftwareProfile? Profile, bool IsAmbiguous);
}
