using System.Collections.Generic;
using Css.Core.Startup;

namespace Css.Core.Apps;

public sealed class AppDrawerActionHostViewModel
{
    public required bool IsVisible { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string AgentTakeaway { get; init; }
    public required string NextStepText { get; init; }
    public required string SafetyText { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public required bool CanExecuteDirectly { get; init; }
    public required string StatusText { get; init; }
    public string PrimaryActionText { get; init; } = "";
    public string PrimaryActionKey { get; init; } = "";
}

public static class AppDrawerActionHostPresenter
{
    public static AppDrawerActionHostViewModel Collapsed() =>
        new()
        {
            IsVisible = false,
            Title = "",
            Summary = "",
            AgentTakeaway = "",
            NextStepText = "",
            SafetyText = "",
            Lines = [],
            CanExecuteDirectly = false,
            StatusText = ""
        };

    public static AppDrawerActionHostViewModel NoSelectionForUninstall() =>
        CollapsedWithStatus("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528\uff0c\u518d\u67e5\u770b\u5378\u8f7d\u65b9\u6848\u3002");

    public static AppDrawerActionHostViewModel NoSelectionForMigration() =>
        CollapsedWithStatus("\u8bf7\u5148\u9009\u62e9\u4e00\u4e2a\u5e94\u7528\uff0c\u518d\u751f\u6210\u8fc1\u79fb\u65b9\u6848\u3002");

    public static AppDrawerActionHostViewModel UninstallRefused(string reason) =>
        new()
        {
            IsVisible = true,
            Title = "卸载方案未打开",
            Summary = reason,
            AgentTakeaway = "Agent 判断：当前应用不适合进入普通卸载流程。",
            NextStepText = "下一步：保留当前应用，或查看技术详情确认它的归属。",
            SafetyText = "没有读取卸载恢复准备，没有运行卸载器，也没有处理残留。",
            Lines = ["当前入口已停止，不会继续使用上一次方案。"],
            CanExecuteDirectly = false,
            StatusText = "卸载方案已安全停止。"
        };

    public static AppDrawerActionHostViewModel ShowUninstall(AppDrawerViewModel drawer) =>
        new()
        {
            IsVisible = true,
            Title = "\u5378\u8f7d\u65b9\u6848\u9884\u89c8",
            Summary = "\u5148\u770b\u5b98\u65b9\u5378\u8f7d\u6d41\u7a0b\uff0c\u518d\u5904\u7406\u4f4e\u98ce\u9669\u6b8b\u7559\u3002",
            AgentTakeaway = "Agent \u5224\u65ad\uff1a\u5148\u7528\u5b98\u65b9\u5378\u8f7d\uff0c\u6b8b\u7559\u8981\u5206\u98ce\u9669\u770b\u3002",
            NextStepText = "\u4e0b\u4e00\u6b65\uff1a\u5148\u770b\u5b98\u65b9\u5378\u8f7d\u65b9\u6848\uff0c\u771f\u8981\u6267\u884c\u65f6\u8fd8\u9700\u4f60\u518d\u786e\u8ba4\u3002",
            SafetyText = "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u8fd0\u884c\u5378\u8f7d\u5668\uff0c\u4e5f\u4e0d\u4f1a\u76f4\u63a5\u5220\u9664\u6b8b\u7559\u3002",
            Lines = drawer.UninstallPreviewLines,
            CanExecuteDirectly = false,
            StatusText = "\u5df2\u663e\u793a\u5378\u8f7d\u65b9\u6848\uff1b\u6ca1\u6709\u8fd0\u884c\u5378\u8f7d\u5668\uff0c\u4e5f\u6ca1\u6709\u5220\u9664\u6b8b\u7559\u3002"
        };

    public static AppDrawerActionHostViewModel ShowMigration(AppDrawerViewModel drawer) =>
        new()
        {
            IsVisible = true,
            Title = "\u8fc1\u79fb\u65b9\u6848\u9884\u89c8",
            Summary = drawer.MigrationSummary,
            AgentTakeaway = "Agent \u5224\u65ad\uff1a\u5148\u751f\u6210\u8fc1\u79fb\u65b9\u6848\uff0c\u4e0d\u8981\u76f4\u63a5\u642c\u6587\u4ef6\u3002",
            NextStepText = "\u4e0b\u4e00\u6b65\uff1a\u68c0\u67e5\u76ee\u6807 D \u76d8\u4f4d\u7f6e\u3001\u56de\u6eda\u6e05\u5355\u548c\u8fc1\u79fb\u540e\u76d1\u63a7\u3002",
            SafetyText = "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u79fb\u52a8\u6587\u4ef6\uff0c\u4e5f\u4e0d\u4f1a\u4fee\u6539\u7cfb\u7edf\u8bbe\u7f6e\u3002",
            Lines = drawer.MigrationPreviewLines,
            CanExecuteDirectly = false,
            StatusText = "\u5df2\u663e\u793a\u8fc1\u79fb\u65b9\u6848\uff1b\u6ca1\u6709\u79fb\u52a8\u6587\u4ef6\uff0c\u4e5f\u6ca1\u6709\u4fee\u6539\u7cfb\u7edf\u8bbe\u7f6e\u3002"
        };

    public static AppDrawerActionHostViewModel ShowMigration(
        AppDrawerViewModel drawer,
        MigrationClosureSummaryViewModel? closure)
    {
        if (closure?.NeedsAttention != true)
            return ShowMigration(drawer);

        return new AppDrawerActionHostViewModel
        {
            IsVisible = true,
            Title = "迁移闭环复查",
            Summary = closure.Headline + "。" + closure.Detail,
            AgentTakeaway = "Agent 判断：旧迁移状态已经变化，需要重新扫描和规划，不能直接继续搬文件。",
            NextStepText = "下一步：打开安全方案，重新生成快照、回滚清单和迁移后验证步骤。",
            SafetyText = "安全边界：旧监控记录不会直接执行，也不会自动修复链接或移动目录。",
            Lines =
            [
                "先确认应用和相关后台组件已经关闭。",
                "重新扫描当前 C 盘写入和 D 盘目标状态。",
                "新证据齐全后仍需你在最终确认页逐项同意。",
                .. drawer.MigrationPreviewLines
            ],
            CanExecuteDirectly = false,
            StatusText = "已打开迁移闭环复查；当前没有移动文件或修改系统。"
        };
    }

    public static AppDrawerActionHostViewModel ShowCacheCleanup(AppDrawerViewModel drawer)
    {
        var state = AppDrawerActionPreviewPresenter.ShowCacheCleanup(drawer);
        return FromPreviewState(
            "\u7f13\u5b58\u6e05\u7406\u9884\u89c8",
            state,
            "Agent \u5224\u65ad\uff1a\u53d1\u73b0\u7f13\u5b58\u5019\u9009\uff0c\u5148\u786e\u8ba4\u54ea\u4e9b\u771f\u7684\u53ef\u6e05\u7406\u3002",
            "\u4e0b\u4e00\u6b65\uff1a\u53ea\u6709\u4f4e\u98ce\u9669\u7f13\u5b58\u624d\u80fd\u8fdb\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002",
            "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u5220\u9664\u6587\u4ef6\uff0c\u771f\u6e05\u7406\u4e5f\u9ed8\u8ba4\u5148\u8fdb\u9694\u79bb\u533a\u3002");
    }

    public static AppDrawerActionHostViewModel ShowCacheCleanup(
        AppDrawerViewModel drawer,
        AppCacheCleanupPlan plan)
    {
        var state = AppDrawerActionPreviewPresenter.ShowCacheCleanup(drawer);
        return new AppDrawerActionHostViewModel
        {
            IsVisible = true,
            Title = "缓存清理方案",
            Summary = plan.Summary,
            AgentTakeaway = plan.CanContinue
                ? "Agent 判断：这些位置符合低风险缓存规则，可以在你确认后进入隔离区。"
                : "Agent 判断：当前证据不足或状态不安全，所以先不处理。",
            NextStepText = plan.NextStepText,
            SafetyText = plan.SafetyText,
            Lines = state.Lines.Concat(plan.Lines).ToArray(),
            CanExecuteDirectly = false,
            StatusText = plan.CanContinue
                ? "缓存方案已经准备好；仍需你再次确认，当前没有移动文件。"
                : "缓存方案已停止；没有移动或删除文件。",
            PrimaryActionText = plan.CanContinue ? "确认后移到隔离区" : "",
            PrimaryActionKey = plan.CanContinue ? "CacheCleanup" : ""
        };
    }

    public static AppDrawerActionHostViewModel CacheCleanupRefused(string reason) =>
        new()
        {
            IsVisible = true,
            Title = "缓存处理已停止",
            Summary = reason,
            AgentTakeaway = "Agent 判断：当前证据已经变化，继续处理不够安全。",
            NextStepText = "下一步：关闭应用并重新扫描，再生成新的缓存方案。",
            SafetyText = "没有移动或删除文件，也没有修改注册表、服务或自启动。",
            Lines = ["旧方案不会被继续使用。"],
            CanExecuteDirectly = false,
            StatusText = "缓存处理已安全停止。"
        };

    public static AppDrawerActionHostViewModel CacheCleanupCompleted(string summary) =>
        new()
        {
            IsVisible = true,
            Title = "缓存已移到隔离区",
            Summary = summary,
            AgentTakeaway = "Agent 判断：处理已完成，但这不是永久删除。",
            NextStepText = "下一步：正常使用应用；需要反悔时到后悔药中心还原。",
            SafetyText = "本次只处理确认过的缓存位置，没有修改注册表、服务或自启动。",
            Lines = ["后悔药中心已记录本次操作和还原入口。"],
            CanExecuteDirectly = false,
            StatusText = "缓存已进入隔离区，可在后悔药中心还原。",
            PrimaryActionText = "打开后悔药中心",
            PrimaryActionKey = "Timeline"
        };

    public static AppDrawerActionHostViewModel ShowStartupControl(AppDrawerViewModel drawer)
    {
        var state = AppDrawerActionPreviewPresenter.ShowStartupControl(drawer);
        return FromPreviewState(
            "\u81ea\u542f\u52a8\u9884\u89c8",
            state,
            StartupAgentTakeaway(drawer),
            StartupNextStep(drawer),
            StartupSafetyText(drawer));
    }

    public static AppDrawerActionHostViewModel ShowStartupControl(
        AppDrawerViewModel drawer,
        AppStartupSettingsHandoff handoff)
    {
        var state = AppDrawerActionPreviewPresenter.ShowStartupControl(drawer);
        return new AppDrawerActionHostViewModel
        {
            IsVisible = true,
            Title = "自启动检查",
            Summary = handoff.Summary,
            AgentTakeaway = handoff.AgentTakeaway,
            NextStepText = handoff.NextStepText,
            SafetyText = handoff.SafetyText,
            Lines = state.Lines.Concat(handoff.Lines).ToArray(),
            CanExecuteDirectly = false,
            StatusText = handoff.CanOpenStartupSettings
                ? "已准备 Windows 启动应用入口；当前没有修改设置。"
                : "自启动方案已停止；当前没有修改设置。",
            PrimaryActionText = handoff.CanOpenStartupSettings ? "在 Windows 中查看" : "",
            PrimaryActionKey = handoff.CanOpenStartupSettings ? "StartupSettings" : ""
        };
    }

    public static AppDrawerActionHostViewModel ShowStartupControl(
        AppDrawerViewModel drawer,
        StartupControlPreparation preparation,
        AppStartupSettingsHandoff handoff)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        if (!preparation.CanContinue)
        {
            var fallback = ShowStartupControl(drawer, handoff);
            return new AppDrawerActionHostViewModel
            {
                IsVisible = fallback.IsVisible,
                Title = fallback.Title,
                Summary = preparation.Summary,
                AgentTakeaway = "Agent 判断：当前证据不足，OMNIX 不会猜测或批量关闭后台组件。",
                NextStepText = fallback.NextStepText,
                SafetyText = fallback.SafetyText,
                Lines = preparation.Lines.Concat(handoff.Lines).ToArray(),
                CanExecuteDirectly = false,
                StatusText = fallback.StatusText,
                PrimaryActionText = fallback.PrimaryActionText,
                PrimaryActionKey = fallback.PrimaryActionKey
            };
        }

        var state = AppDrawerActionPreviewPresenter.ShowStartupControl(drawer);
        return new AppDrawerActionHostViewModel
        {
            IsVisible = true,
            Title = "自启动安全方案",
            Summary = preparation.Summary,
            AgentTakeaway = "Agent 判断：已唯一确认 1 个普通自启动入口，可以准备可恢复的关闭方案。",
            NextStepText = "下一步：查看影响范围并逐项确认；确认前不会修改设置。",
            SafetyText = "只处理当前用户的 1 个普通自启动入口；不停止软件，不改服务或计划任务。",
            Lines = state.Lines.Concat(preparation.Lines).ToArray(),
            CanExecuteDirectly = false,
            StatusText = "本地自启动方案已准备好；当前没有修改设置。",
            PrimaryActionText = "审核关闭方案",
            PrimaryActionKey = "StartupDisableReview"
        };
    }

    public static AppDrawerActionHostViewModel StartupControlRefused(
        string reason,
        AppStartupSettingsHandoff? handoff = null) =>
        new()
        {
            IsVisible = true,
            Title = "自启动处理已停止",
            Summary = reason,
            AgentTakeaway = "Agent 判断：启动项证据已经变化或不够唯一，继续处理不安全。",
            NextStepText = handoff?.NextStepText ?? "下一步：重新扫描应用，再生成新的自启动方案。",
            SafetyText = "没有修改注册表，也没有停止软件、服务或计划任务。",
            Lines = handoff?.Lines ?? ["旧方案不会被继续使用。"],
            CanExecuteDirectly = false,
            StatusText = "自启动处理已安全停止。",
            PrimaryActionText = handoff?.CanOpenStartupSettings == true ? "在 Windows 中查看" : "",
            PrimaryActionKey = handoff?.CanOpenStartupSettings == true ? "StartupSettings" : ""
        };

    public static AppDrawerActionHostViewModel StartupControlCompleted(string softwareName) =>
        new()
        {
            IsVisible = true,
            Title = "已关闭自启动",
            Summary = $"{softwareName} 下次登录时不会通过这个普通入口自动启动。",
            AgentTakeaway = "Agent 判断：操作已完成，并且保留了原始设置。",
            NextStepText = "需要反悔时，可到后悔药中心恢复这个自启动入口。",
            SafetyText = "没有关闭当前软件，也没有修改服务、计划任务或其他应用。",
            Lines = ["后悔药中心已经记录本次操作和还原入口。"],
            CanExecuteDirectly = false,
            StatusText = "已关闭 1 个普通自启动入口，可在后悔药中心还原。",
            PrimaryActionText = "打开后悔药中心",
            PrimaryActionKey = "Timeline"
        };

    public static AppDrawerActionHostViewModel NoSelectionForCacheCleanup() =>
        FromPreviewState(
            "\u7f13\u5b58\u6e05\u7406\u9884\u89c8",
            AppDrawerActionPreviewPresenter.NoSelectionForCacheCleanup(),
            "",
            "",
            "");

    public static AppDrawerActionHostViewModel NoSelectionForStartupControl() =>
        FromPreviewState(
            "\u81ea\u542f\u52a8\u9884\u89c8",
            AppDrawerActionPreviewPresenter.NoSelectionForStartupControl(),
            "",
            "",
            "");

    private static AppDrawerActionHostViewModel FromPreviewState(
        string title,
        AppDrawerActionPreviewState state,
        string agentTakeaway,
        string nextStepText,
        string safetyText) =>
        new()
        {
            IsVisible = state.CachePreviewVisible || state.StartupPreviewVisible,
            Title = title,
            Summary = state.Summary,
            AgentTakeaway = agentTakeaway,
            NextStepText = nextStepText,
            SafetyText = safetyText,
            Lines = state.Lines,
            CanExecuteDirectly = state.CanExecuteDirectly,
            StatusText = state.StatusText
        };

    private static AppDrawerActionHostViewModel CollapsedWithStatus(string statusText) =>
        new()
        {
            IsVisible = false,
            Title = "",
            Summary = "",
            AgentTakeaway = "",
            NextStepText = "",
            SafetyText = "",
            Lines = [],
            CanExecuteDirectly = false,
            StatusText = statusText
        };

    private static string StartupAgentTakeaway(AppDrawerViewModel drawer)
    {
        if (drawer.StartupControlSummary.Contains("\u5efa\u8bae\u4fdd\u7559"))
            return "Agent \u5224\u65ad\uff1a\u5efa\u8bae\u4fdd\u7559\uff0c\u8fd9\u7c7b\u540e\u53f0\u53ef\u80fd\u8fde\u7740\u7cfb\u7edf\u3001\u9a71\u52a8\u6216\u5b89\u5168\u529f\u80fd\u3002";

        if (drawer.StartupControlSummary.Contains("\u5148\u89c2\u5bdf"))
            return "Agent \u5224\u65ad\uff1a\u5148\u89c2\u5bdf\uff0c\u6b63\u5728\u8fd0\u884c\u4e0d\u7b49\u4e8e\u53ef\u4ee5\u7981\u7528\u3002";

        if (drawer.StartupControlSummary.Contains("\u672a\u6765\u53ef\u7981\u7528"))
            return "Agent \u5224\u65ad\uff1a\u81ea\u542f\u52a8\u6216\u540e\u53f0\u5e38\u9a7b\u662f\u672a\u6765\u53ef\u7981\u7528\u5019\u9009\uff0c\u4f46\u73b0\u5728\u53ea\u751f\u6210\u65b9\u6848\u3002";

        return "Agent \u5224\u65ad\uff1a\u53d1\u73b0\u81ea\u542f\u52a8\u6216\u540e\u53f0\u5e38\u9a7b\u7ebf\u7d22\uff0c\u5148\u5224\u65ad\u662f\u5426\u5fc5\u8981\u3002";
    }

    private static string StartupNextStep(AppDrawerViewModel drawer)
    {
        if (drawer.StartupControlSummary.Contains("\u5efa\u8bae\u4fdd\u7559"))
            return "\u4e0b\u4e00\u6b65\uff1a\u5148\u4fdd\u7559\uff0c\u53ea\u5728\u660e\u663e\u5f02\u5e38\u6216\u4f60\u786e\u8ba4\u4e0d\u9700\u8981\u65f6\u518d\u770b\u6280\u672f\u8be6\u60c5\u3002";

        if (drawer.StartupControlSummary.Contains("\u5148\u89c2\u5bdf"))
            return "\u4e0b\u4e00\u6b65\uff1a\u7ee7\u7eed\u89c2\u5bdf\u6216\u91cd\u65b0\u626b\u63cf\uff0c\u7b49\u627e\u5230\u81ea\u542f\u52a8\u3001\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u8bc1\u636e\u518d\u51b3\u5b9a\u3002";

        if (drawer.StartupControlSummary.Contains("\u672a\u6765\u53ef\u7981\u7528"))
            return "\u4e0b\u4e00\u6b65\uff1a\u5148\u786e\u8ba4\u7528\u9014\uff0c\u518d\u51c6\u5907\u5feb\u7167\u3001\u56de\u6eda\u548c\u672c\u5730\u64cd\u4f5c\u8ba1\u5212\u3002";

        return "\u4e0b\u4e00\u6b65\uff1a\u5148\u770b\u54ea\u4e9b\u5f71\u54cd\u5f00\u673a\uff0c\u54ea\u4e9b\u53ef\u80fd\u662f\u5fc5\u8981\u540e\u53f0\u80fd\u529b\u3002";
    }

    private static string StartupSafetyText(AppDrawerViewModel drawer)
    {
        if (drawer.StartupControlSummary.Contains("\u5efa\u8bae\u4fdd\u7559"))
            return "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\uff1b\u6ca1\u6709\u5feb\u7167\u3001\u56de\u6eda\u548c\u4f60\u786e\u8ba4\u65f6\uff0c\u7cfb\u7edf\u76f8\u5173\u540e\u53f0\u4e0d\u8fdb\u5165\u6267\u884c\u3002";

        if (drawer.StartupControlSummary.Contains("\u5148\u89c2\u5bdf"))
            return "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\uff0c\u8fd8\u6ca1\u6709\u8db3\u591f\u8bc1\u636e\u65f6\u53ea\u80fd\u7ed9\u4f60\u89c2\u5bdf\u5efa\u8bae\u3002";

        if (drawer.StartupControlSummary.Contains("\u672a\u6765\u53ef\u7981\u7528"))
            return "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\uff1b\u771f\u5904\u7406\u524d\u5fc5\u987b\u6709\u5feb\u7167\u3001\u56de\u6eda\u548c\u7528\u6237\u786e\u8ba4\u3002";

        return "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\u542f\u52a8\u9879\u3001\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u3002";
    }
}
