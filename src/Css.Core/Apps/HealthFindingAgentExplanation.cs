using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Css.Core.Operations;
using Css.Core.Recommendations;

namespace Css.Core.Apps;

public sealed class HealthFindingAgentExplanation
{
    public required string Title { get; init; }
    public required string WhatThisMeans { get; init; }
    public required string WhyItMatters { get; init; }
    public required string RecommendedNextStep { get; init; }
    public required string SafetyBoundary { get; init; }
    public required IReadOnlyList<string> NextSteps { get; init; }
    public RecommendationAction Action { get; init; }
    public RiskLevel Risk { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class HealthFindingAgentExplanationBuilder
{
    private static readonly Regex LocalPathPattern = new(
        @"[A-Za-z]:\\[^\s\uff0c\u3002\uff1b,;]+",
        RegexOptions.Compiled);

    public static HealthFindingAgentExplanation Create(HealthFinding finding)
    {
        var findingText = HideLocalPaths(finding.Text);

        return new HealthFindingAgentExplanation
        {
            Title = "Computer Agent 解释",
            WhatThisMeans = "我发现：" + findingText,
            WhyItMatters = WhyItMatters(finding.Kind, finding.Action, finding.Risk),
            RecommendedNextStep = RecommendedNextStep(finding.Kind, finding.Action, finding.Risk),
            SafetyBoundary = "安全边界：我不会直接执行删除、迁移、禁用服务或修改注册表；真正处理必须经过你确认和本地安全管线。",
            NextSteps = NextSteps(finding.Kind, finding.Action, finding.Risk),
            Action = finding.Action,
            Risk = finding.Risk,
            CanExecuteDirectly = false
        };
    }

    private static string HideLocalPaths(string text) =>
        LocalPathPattern.Replace(text, "某个本机路径");

    private static string WhyItMatters(
        HealthFindingKind kind,
        RecommendationAction action,
        RiskLevel risk)
    {
        if (kind == HealthFindingKind.SustainedGrowth)
            return "它已经在多次体检中反复变大；如果只清理一次却不调整软件自己的存储设置，C 盘还可能继续被占用。";
        if (kind == HealthFindingKind.PersonalStorage)
            return "这是你个人文件夹中的只读候选。大文件可能只是很久没用；同名同大小也不能证明内容完全重复。";
        if (kind == HealthFindingKind.MigrationClosure)
            return action == RecommendationAction.Migrate
                ? "迁移后的原位置不再满足已记录的闭环状态，软件可能重新往 C 盘写入；继续手动移动会让回滚和定位更困难。"
                : "这是一条只读的旧迁移记录；当前应用归属或对应关系不足以支持普通迁移复查，不能把历史记录当作新的迁移授权。";

        return action switch
        {
            RecommendationAction.Clean when risk is RiskLevel.None or RiskLevel.Low =>
                "这类问题通常可以释放空间，但仍要先确认不是你还需要的内容。",
            RecommendationAction.Clean =>
                "它可能和系统或软件状态有关，风险不低，不适合直接清理。",
            RecommendationAction.Migrate =>
                "它可能持续占用 C 盘，但迁移需要确认回滚和软件是否会继续写回原位置。",
            RecommendationAction.DisableStartup =>
                "它可能影响开机或后台常驻，但关闭前要确认是否会影响你常用功能。",
            RecommendationAction.Uninstall =>
                "卸载能释放空间，但必须先走官方卸载器，再分级处理残留。",
            RecommendationAction.Keep =>
                "目前看起来不值得动，保留比盲目优化更稳。",
            _ => "它需要更多证据才能判断，先观察比直接处理更稳。"
        };
    }

    private static string RecommendedNextStep(
        HealthFindingKind kind,
        RecommendationAction action,
        RiskLevel risk)
    {
        if (kind == HealthFindingKind.SustainedGrowth)
            return "现在先打开对应应用详情，分清安装文件、缓存、日志、下载或模型；以后再决定清理一次、修改软件设置或继续观察。";
        if (kind == HealthFindingKind.PersonalStorage)
            return "到 C 盘页面查看文件名、大小和日期；先打开核对，需要保留的内容可归档到非系统盘。";
        if (kind == HealthFindingKind.MigrationClosure)
            return action == RecommendationAction.Migrate
                ? "先打开对应应用详情并重新扫描，核对迁移状态；在新快照和回滚方案生成前不要再次手动移动。"
                : "这条旧迁移记录仅供查看；请到应用管理确认系统归属或对应关系，当前不生成迁移动作。";

        return action switch
        {
            RecommendationAction.Clean when risk is RiskLevel.None or RiskLevel.Low =>
                "建议生成处理方案，执行时先移动到隔离区，不直接永久删除。",
            RecommendationAction.Clean =>
                "建议先查看详情和快照状态，暂不执行清理。",
            RecommendationAction.Migrate =>
                "建议先生成迁移方案，确认快照、回滚清单和迁移后监控。",
            RecommendationAction.DisableStartup =>
                "建议先看它的用途和影响，再生成可回滚的关闭方案。",
            RecommendationAction.Uninstall =>
                "建议先打开卸载安全方案，不直接运行卸载器。",
            _ => "建议先观察，等下次扫描或更多证据后再决定。"
        };
    }

    private static IReadOnlyList<string> NextSteps(
        HealthFindingKind kind,
        RecommendationAction action,
        RiskLevel risk)
    {
        if (kind == HealthFindingKind.SustainedGrowth)
        {
            return
            [
                "打开对应应用详情",
                "分清缓存、日志、下载或模型",
                "分别生成一次处理和防止继续增长的预案",
                "确认软件自身支持后再考虑改到 D 盘"
            ];
        }

        if (kind == HealthFindingKind.PersonalStorage)
        {
            return
            [
                "到 C 盘页面查看个人文件候选",
                "打开文件确认用途和内容",
                "需要保留时归档到非系统盘",
                "不要根据名称和大小直接删除"
            ];
        }

        if (kind == HealthFindingKind.MigrationClosure)
        {
            if (action != RecommendationAction.Migrate)
            {
                return
                [
                    "打开应用管理查看旧迁移记录",
                    "确认系统归属或应用对应关系",
                    "只查看技术详情，不运行普通迁移流程",
                    "等待可靠对应关系后再重新评估"
                ];
            }

            return
            [
                "打开对应应用详情",
                "重新扫描迁移闭环状态",
                "确认软件是否又在 C 盘生成内容",
                "生成新的快照和迁移方案后再决定"
            ];
        }

        if (action == RecommendationAction.Clean && risk is RiskLevel.None or RiskLevel.Low)
        {
            return
            [
                "生成处理方案",
                "确认会动哪些内容",
                "先移动到隔离区",
                "需要时从后悔药中心还原"
            ];
        }

        return
        [
            "查看详情",
            "保留快照和证据",
            "生成处理方案",
            "由你确认后再进入安全管线"
        ];
    }
}

public sealed class HealthFindingDetailPresentation
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Detail { get; init; }
    public required string SafetyBoundary { get; init; }
    public required string VisibleText { get; init; }
}

public static class HealthFindingDetailPresentationBuilder
{
    private static readonly Regex LocalPathPattern = new(
        @"[A-Za-z]:\\[^\s\uff0c\u3002\uff1b,;]+",
        RegexOptions.Compiled);

    public static HealthFindingDetailPresentation Create(HealthFinding finding)
    {
        var text = HideLocalPaths(finding.Text);
        var summary = finding.Kind switch
        {
            HealthFindingKind.PersonalStorage => "请到 C 盘清理页查看个人文件候选的名称、大小和日期。",
            HealthFindingKind.MigrationClosure => "请到应用管理页重新查看对应应用的迁移闭环状态。",
            _ => "请到 C 盘清理页查看对应的决策卡片、增长记录和技术报告。"
        };
        var detail = finding.Kind switch
        {
            HealthFindingKind.PersonalStorage => "这里只提供只读候选，不会把个人文件加入清理计划。疑似重复项也没有读取或比对文件内容。",
            HealthFindingKind.MigrationClosure => "这里只展示迁移后的只读复查结果，不会自动修复链接、移动目录或回滚。",
            _ => "这里只解释发现，不执行清理。如果需要动手，必须从决策卡片进入确认页。"
        };
        var safety = "安全边界：查看详情不会处理、移动或删除任何内容，也不会直接执行；真正动作必须进入本地安全管线。";

        return new HealthFindingDetailPresentation
        {
            Title = "查看详情",
            Summary = summary,
            Detail = text + "\n" + detail,
            SafetyBoundary = safety,
            VisibleText = string.Join("\n", ["查看详情", summary, text, detail, safety])
        };
    }

    private static string HideLocalPaths(string text) =>
        LocalPathPattern.Replace(text, "某个本机路径");
}

public sealed class HealthFindingActionPlan
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Steps { get; init; }
    public required string SafetyBoundary { get; init; }
    public required string VisibleText { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class HealthFindingActionPlanBuilder
{
    private static readonly Regex LocalPathPattern = new(
        @"[A-Za-z]:\\[^\s\uff0c\u3002\uff1b,;]+",
        RegexOptions.Compiled);

    public static HealthFindingActionPlan Create(HealthFinding finding)
    {
        var text = HideLocalPaths(finding.Text);
        var steps = Steps(finding);
        var summary = Summary(finding);
        var safety = "安全边界：这只是生成处理方案，不会直接执行；真正动作必须由你确认并进入本地安全管线。";

        return new HealthFindingActionPlan
        {
            Title = "生成处理方案",
            Summary = text + "\n" + summary,
            Steps = steps,
            SafetyBoundary = safety,
            CanExecuteDirectly = false,
            VisibleText = string.Join("\n", ["生成处理方案", text, summary, safety, .. steps])
        };
    }

    private static IReadOnlyList<string> Steps(HealthFinding finding)
    {
        if (finding.Kind == HealthFindingKind.SustainedGrowth)
        {
            return
            [
                "先进入应用详情确认归属，不根据目录名称猜测",
                "区分可以重建的缓存和必须保留的个人数据",
                "生成一次处理预案，并单独生成防止继续增长的设置预案",
                "任何清理或迁移仍由你确认后进入本地安全管线"
            ];
        }

        if (finding.Kind == HealthFindingKind.PersonalStorage)
        {
            return
            [
                "查看候选文件名、大小和最后修改日期",
                "打开文件确认是否仍需保留",
                "疑似重复项需要逐份核对内容",
                "本方案不生成删除、移动或隔离区操作"
            ];
        }

        if (finding.Kind == HealthFindingKind.MigrationClosure)
        {
            return finding.Action == RecommendationAction.Migrate
                ?
                [
                    "打开对应应用详情并重新扫描",
                    "核对旧迁移位置和当前写入状态",
                    "重新生成快照、回滚清单和验证步骤",
                    "由你确认后才能进入新的迁移流程"
                ]
                :
                [
                    "打开应用管理查看旧迁移记录",
                    "确认系统归属或应用对应关系",
                    "保留只读证据，不生成迁移动作",
                    "有可靠对应关系后再重新评估"
                ];
        }

        if (finding.Action == RecommendationAction.Clean
            && finding.Risk is RiskLevel.None or RiskLevel.Low)
        {
            return
            [
                "确认它是可以清理的内容",
                "检查决策卡片的证据和影响范围",
                "由你确认后先移动到隔离区",
                "记录到后悔药中心，需要时可还原"
            ];
        }

        return
        [
            "先查看详情和证据",
            "确认快照或回滚方案是否可用",
            "生成只读处理方案",
            "由你确认后才能继续"
        ];
    }

    private static string Summary(HealthFinding finding)
    {
        if (finding.Kind == HealthFindingKind.SustainedGrowth)
            return "可以准备两个互不混淆的方案：一个处理当前占用，一个防止软件继续写回 C 盘；目前都不会直接执行。";
        if (finding.Kind == HealthFindingKind.PersonalStorage)
            return "可以准备人工核对和归档方案；当前证据不能授权自动删除任何个人文件。";
        if (finding.Kind == HealthFindingKind.MigrationClosure)
            return finding.Action == RecommendationAction.Migrate
                ? "可以准备迁移方案，但不会直接移动文件。"
                : "这条旧迁移记录仅供查看；当前不生成迁移动作，也不会移动文件或修改链接。";

        return finding.Action switch
        {
            RecommendationAction.Clean when finding.Risk is RiskLevel.None or RiskLevel.Low =>
                "可以准备低风险清理方案，但默认进隔离区，不永久删除。",
            RecommendationAction.Clean =>
                "可能可以清理，但必须先有快照和回滚证据。",
            RecommendationAction.Migrate =>
                "可以准备迁移方案，但不会直接移动文件。",
            RecommendationAction.Uninstall =>
                "可以准备卸载安全方案，但不会直接运行卸载器。",
            RecommendationAction.DisableStartup =>
                "可以准备关闭自启方案，但需要明确回滚步骤。",
            _ => "建议先观察，下次扫描有更多证据后再处理。"
        };
    }

    private static string HideLocalPaths(string text) =>
        LocalPathPattern.Replace(text, "某个本机路径");
}

public sealed class HomeAgentResponseViewModel
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string SafetyBoundary { get; init; }
    public required HomeAgentNavigationDestination NavigationDestination { get; init; }
    public required string NavigationLabel { get; init; }
    public string? TargetAppName { get; init; }
    public AppCatalogFilter? TargetAppFilter { get; init; }
    public bool CanNavigate =>
        NavigationDestination != HomeAgentNavigationDestination.None
        && !string.IsNullOrWhiteSpace(NavigationLabel);
    public bool CanExecuteDirectly { get; init; }
}

public enum HomeAgentNavigationDestination
{
    None,
    CDrive,
    CDrivePersonalStorage,
    Applications
}

public static class HomeAgentResponsePresenter
{
    public static HomeAgentResponseViewModel AppTargetUnavailable(
        AppDrawerTargetResolution resolution) =>
        new()
        {
            Title = resolution.Headline,
            Body = resolution.Explanation,
            SafetyBoundary = resolution.SafetyBoundary,
            NavigationDestination = HomeAgentNavigationDestination.Applications,
            NavigationLabel = "打开应用管理",
            CanExecuteDirectly = false
        };

    public static HomeAgentResponseViewModel Explain(HealthFinding finding)
    {
        var explanation = HealthFindingAgentExplanationBuilder.Create(finding);
        var navigation = CreateNavigation(finding, "查看 C 盘证据");
        return new HomeAgentResponseViewModel
        {
            Title = explanation.Title,
            Body = explanation.WhatThisMeans + "\n\n" +
                explanation.WhyItMatters + "\n\n" +
                explanation.RecommendedNextStep + "\n\n" +
                "下一步：\n" + BulletList(explanation.NextSteps),
            SafetyBoundary = explanation.SafetyBoundary,
            NavigationDestination = navigation.Destination,
            NavigationLabel = navigation.Label,
            TargetAppName = navigation.TargetAppName,
            TargetAppFilter = navigation.TargetAppFilter,
            CanExecuteDirectly = false
        };
    }

    public static HomeAgentResponseViewModel ShowDetails(HealthFinding finding)
    {
        var detail = HealthFindingDetailPresentationBuilder.Create(finding);
        var navigation = CreateNavigation(finding, "打开 C 盘详情");
        return new HomeAgentResponseViewModel
        {
            Title = detail.Title,
            Body = detail.Summary + "\n\n" + detail.Detail,
            SafetyBoundary = detail.SafetyBoundary,
            NavigationDestination = navigation.Destination,
            NavigationLabel = navigation.Label,
            TargetAppName = navigation.TargetAppName,
            TargetAppFilter = navigation.TargetAppFilter,
            CanExecuteDirectly = false
        };
    }

    public static HomeAgentResponseViewModel CreatePlan(HealthFinding finding)
    {
        var plan = HealthFindingActionPlanBuilder.Create(finding);
        var navigation = CreateNavigation(finding, "去 C 盘选择方案");
        return new HomeAgentResponseViewModel
        {
            Title = plan.Title,
            Body = plan.Summary + "\n\n" +
                "方案步骤：\n" + BulletList(plan.Steps),
            SafetyBoundary = plan.SafetyBoundary,
            NavigationDestination = navigation.Destination,
            NavigationLabel = navigation.Label,
            TargetAppName = navigation.TargetAppName,
            TargetAppFilter = navigation.TargetAppFilter,
            CanExecuteDirectly = false
        };
    }

    private static HomeAgentNavigation CreateNavigation(
        HealthFinding finding,
        string cDriveLabel) =>
        finding.Kind == HealthFindingKind.PersonalStorage
            ? new HomeAgentNavigation(
                HomeAgentNavigationDestination.CDrivePersonalStorage,
                "查看个人文件候选",
                null,
                null)
            : finding.Kind == HealthFindingKind.MigrationClosure
                && string.IsNullOrWhiteSpace(finding.TargetAppName)
            ? new HomeAgentNavigation(
                HomeAgentNavigationDestination.Applications,
                "打开应用管理",
                null,
                AppCatalogFilter.CDrive)
            : string.IsNullOrWhiteSpace(finding.TargetAppName)
            ? new HomeAgentNavigation(
                HomeAgentNavigationDestination.CDrive,
                cDriveLabel,
                null,
                null)
            : new HomeAgentNavigation(
                HomeAgentNavigationDestination.Applications,
                "打开对应应用",
                finding.TargetAppName.Trim(),
                null);

    private static string BulletList(IReadOnlyList<string> steps) =>
        string.Join("\n", steps.Select(step => "  - " + step));

    private sealed record HomeAgentNavigation(
        HomeAgentNavigationDestination Destination,
        string Label,
        string? TargetAppName,
        AppCatalogFilter? TargetAppFilter);
}
