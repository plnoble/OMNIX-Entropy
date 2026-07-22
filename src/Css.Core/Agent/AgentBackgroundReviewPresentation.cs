using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Software;
using Css.Core.Startup;

namespace Css.Core.Agent;

public sealed class AgentBackgroundReviewViewModel
{
    public required bool IsVisible { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<AgentBackgroundReviewItemViewModel> Items { get; init; }
    public required string SafetyLine { get; init; }
}

public sealed class AgentBackgroundReviewItemViewModel
{
    public required string AppName { get; init; }
    public required string EvidenceSummary { get; init; }
    public required string RiskLabel { get; init; }
    public required string RecommendedNextStep { get; init; }
    public string? TargetAppName { get; init; }
    public string NavigationLabel { get; init; } = "\u67e5\u770b\u5e94\u7528";
    public bool CanOpenApp => !string.IsNullOrWhiteSpace(TargetAppName);
    public bool CanExecuteDirectly { get; init; }
}

public static class AgentBackgroundReviewPresenter
{
    public static AgentBackgroundReviewViewModel Create(IEnumerable<SoftwareProfile>? profiles)
    {
        var items = (profiles ?? [])
            .Where(IsResident)
            .OrderByDescending(RiskOrder)
            .ThenBy(profile => profile.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(6)
            .Select(CreateItem)
            .ToList();

        if (items.Count == 0)
        {
            return new AgentBackgroundReviewViewModel
            {
                IsVisible = false,
                Summary = "\u6682\u672a\u53d1\u73b0\u9700\u8981\u4f18\u5148\u68c0\u67e5\u7684\u540e\u53f0\u5e38\u9a7b\u5e94\u7528\u3002",
                Items = [],
                SafetyLine = "\u6ca1\u6709\u8db3\u591f\u8bc1\u636e\u65f6\uff0c\u4e0d\u751f\u6210\u5173\u95ed\u81ea\u542f\u52a8\u6216\u505c\u6b62\u670d\u52a1\u65b9\u6848\u3002"
            };
        }

        return new AgentBackgroundReviewViewModel
        {
            IsVisible = true,
            Summary = "\u53d1\u73b0 " + items.Count + " \u4e2a\u503c\u5f97\u770b\u770b\u7684\u540e\u53f0\u5e38\u9a7b\u5e94\u7528\uff0c\u5148\u770b\u5b83\u4eec\u662f\u5426\u5fc5\u8981\u5f00\u673a\u6216\u957f\u671f\u8fd0\u884c\u3002",
            Items = items,
            SafetyLine = "\u8fd9\u91cc\u53ea\u89e3\u91ca\u548c\u751f\u6210\u65b9\u6848\uff0c\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed\u8fdb\u7a0b\u3001\u7981\u7528\u670d\u52a1\u6216\u4fee\u6539\u81ea\u542f\u52a8\u3002"
        };
    }

    private static AgentBackgroundReviewItemViewModel CreateItem(SoftwareProfile profile)
    {
        var targetAppName = SafeTargetAppName(profile.Name);
        return new AgentBackgroundReviewItemViewModel
        {
            AppName = targetAppName ?? "\u8fd9\u4e2a\u5e94\u7528",
            EvidenceSummary = BuildEvidenceSummary(profile),
            RiskLabel = BuildRiskLabel(profile),
            RecommendedNextStep = BuildRecommendedNextStep(profile),
            TargetAppName = targetAppName,
            CanExecuteDirectly = false
        };
    }

    private static string? SafeTargetAppName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)
            || name.Contains(":\\", StringComparison.Ordinal)
            || name.Contains('/')
            || name.Contains('\\'))
        {
            return null;
        }

        return name.Trim();
    }

    private static string BuildEvidenceSummary(SoftwareProfile profile)
    {
        var parts = new List<string>();

        if (profile.RunningProcesses.Count > 0)
            parts.Add(profile.RunningProcesses.Count + " \u4e2a\u540e\u53f0\u8fdb\u7a0b");
        if (profile.StartupEntries.Count > 0)
            parts.Add(profile.StartupEntries.Count + " \u4e2a\u81ea\u542f\u52a8\u9879");
        if (profile.Services.Count > 0)
            parts.Add(profile.Services.Count + " \u4e2a\u540e\u53f0\u670d\u52a1");
        if (profile.ScheduledTasks.Count > 0)
            parts.Add(profile.ScheduledTasks.Count + " \u4e2a\u8ba1\u5212\u4efb\u52a1");

        return parts.Count == 0
            ? "\u6682\u672a\u53d1\u73b0\u660e\u786e\u5e38\u9a7b\u8bc1\u636e"
            : string.Join("\u3001", parts);
    }

    private static string BuildRiskLabel(SoftwareProfile profile)
    {
        if (!AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
        {
            return profile.Category == SoftwareCategory.SystemTool
                ? "\u7cfb\u7edf\u6216\u9a71\u52a8\u76f8\u5173\uff0c\u4e0d\u5efa\u8bae\u76f4\u63a5\u52a8"
                : "系统归属待确认，仅供查看";
        }

        if (profile.Services.Count > 0 || profile.ScheduledTasks.Count > 0)
            return "\u9700\u8981\u786e\u8ba4\uff0c\u53ef\u80fd\u5f71\u54cd\u540e\u53f0\u529f\u80fd";

        return "\u4f4e\u5230\u4e2d\u98ce\u9669\uff0c\u5148\u89c2\u5bdf";
    }

    private static string BuildRecommendedNextStep(SoftwareProfile profile)
    {
        if (!AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
            return "先保留并观察；当前只查看技术详情，不生成关闭方案。";

        if (StartupEntryControlPolicy.HasSingleSupportedObservation(profile))
            return "打开应用详情并选择“管理自启动”；重新读取证据后可进入“审核关闭方案”，服务和计划任务仍不处理。";

        if (profile.StartupEntries.Count > 0 || profile.Services.Count > 0 || profile.ScheduledTasks.Count > 0)
            return "\u53ef\u4ee5\u751f\u6210\u65b9\u6848\uff1a\u5148\u5224\u65ad\u5b83\u662f\u5426\u5fc5\u8981\u5f00\u673a\u8fd0\u884c\uff0c\u518d\u51b3\u5b9a\u8981\u4e0d\u8981\u5173\u3002";

        return "\u5148\u89c2\u5bdf\uff1a\u5b83\u6b63\u5728\u8fd0\u884c\u4e0d\u7b49\u4e8e\u53ef\u4ee5\u5173\u95ed\u6216\u7981\u7528\u3002";
    }

    private static bool IsResident(SoftwareProfile profile) =>
        profile.RunningProcesses.Count > 0
        || profile.StartupEntries.Count > 0
        || profile.Services.Count > 0
        || profile.ScheduledTasks.Count > 0;

    private static int RiskOrder(SoftwareProfile profile)
    {
        if (!AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
            return 3;

        if (profile.Services.Count > 0 || profile.ScheduledTasks.Count > 0)
            return 2;

        if (profile.StartupEntries.Count > 0)
            return 1;

        return 0;
    }
}
