using System.Collections.Generic;
using System.Linq;
using Css.Core.Software;
using Css.Core.Startup;

namespace Css.Core.Agent;

public sealed class AgentStartupServicePlanViewModel
{
    public required bool IsVisible { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> EvidenceLines { get; init; }
    public required IReadOnlyList<string> PlanSteps { get; init; }
    public required IReadOnlyList<string> RequiredBeforeExecution { get; init; }
    public required IReadOnlyList<string> BlockedActions { get; init; }
    public required string SafetyLine { get; init; }
    public required bool RequiresSnapshot { get; init; }
    public required bool CanExecuteDirectly { get; init; }
}

public static class AgentStartupServicePlanPresenter
{
    public static AgentStartupServicePlanViewModel Create(IEnumerable<SoftwareProfile>? profiles)
    {
        var residentProfiles = (profiles ?? [])
            .Where(IsResident)
            .ToList();

        if (residentProfiles.Count == 0)
        {
            return new AgentStartupServicePlanViewModel
            {
                IsVisible = false,
                Title = "\u540e\u53f0\u5904\u7406\u65b9\u6848\u9884\u89c8",
                Summary = "\u8fd8\u6ca1\u6709\u8db3\u591f\u8bc1\u636e\u751f\u6210\u540e\u53f0\u6216\u81ea\u542f\u52a8\u5904\u7406\u65b9\u6848\u3002",
                EvidenceLines = [],
                PlanSteps = [],
                RequiredBeforeExecution = [],
                BlockedActions =
                [
                    "\u6ca1\u6709\u8bc1\u636e\u65f6\uff0c\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\u670d\u52a1\u6216\u6539\u81ea\u542f\u52a8\u3002"
                ],
                SafetyLine = "\u5148\u626b\u63cf\u5e94\u7528\uff0c\u518d\u751f\u6210\u53ea\u8bfb\u65b9\u6848\u3002",
                RequiresSnapshot = true,
                CanExecuteDirectly = false
            };
        }

        var candidates = AgentActionCandidateCatalog.Create(residentProfiles);
        var startupCount = candidates.OrdinaryStartupProfiles.Sum(profile => profile.StartupEntries.Count);
        var localReviewCount = candidates.StartupReviewProfiles.Count;
        var serviceCount = residentProfiles.Sum(profile => profile.Services.Count);
        var taskCount = residentProfiles.Sum(profile => profile.ScheduledTasks.Count);
        var processCount = residentProfiles.Sum(profile => profile.RunningProcesses.Count);
        var readOnlyCount = candidates.ReadOnlyResidentProfiles.Count;
        var ordinaryCount = candidates.OrdinaryResidentProfiles.Count;

        return new AgentStartupServicePlanViewModel
        {
            IsVisible = true,
            Title = "\u540e\u53f0 / \u81ea\u542f\u52a8\u5904\u7406\u65b9\u6848\u9884\u89c8",
            Summary = "基于 " + residentProfiles.Count + " 个后台常驻应用，这里仍只生成方案；" + localReviewCount + " 个普通自启动项可尝试本地审核，服务和计划任务不执行。",
            EvidenceLines = BuildEvidenceLines(startupCount, localReviewCount, serviceCount, taskCount, processCount, readOnlyCount),
            PlanSteps = BuildPlanSteps(ordinaryCount, readOnlyCount, localReviewCount),
            RequiredBeforeExecution =
            [
                "普通自启动本地审核前会重新读取精确项目，并先写入可验证的回滚恢复证据。",
                "需要你确认：应用可以关闭开机自启动，且不会影响你需要的同步或登录功能。",
                "服务和计划任务需要各自的快照与回滚能力；当前版本不会借普通自启动方案处理它们。"
            ],
            BlockedActions =
            [
                "不会直接禁用服务或修改计划任务。",
                "不会批量修改多个自启动项。",
                "\u4e0d\u4f1a\u76f4\u63a5\u7ed3\u675f\u8fdb\u7a0b\u3002"
            ],
            SafetyLine = "\u771f\u6b63\u5904\u7406\u5fc5\u987b\u8f6c\u6210\u672c\u5730\u64cd\u4f5c\u8ba1\u5212\uff0c\u518d\u8fdb\u5165\u786e\u8ba4\u3001\u5feb\u7167\u548c\u56de\u6eda\u6d41\u7a0b\u3002",
            RequiresSnapshot = true,
            CanExecuteDirectly = false
        };
    }

    private static IReadOnlyList<string> BuildEvidenceLines(
        int startupCount,
        int localReviewCount,
        int serviceCount,
        int taskCount,
        int processCount,
        int readOnlyCount)
    {
        var lines = new List<string>();

        if (startupCount > 0)
            lines.Add(startupCount + " \u4e2a\u81ea\u542f\u52a8\u9879\u9700\u8981\u5148\u786e\u8ba4\u7528\u9014\u3002");
        if (localReviewCount > 0)
            lines.Add(localReviewCount + " 个普通自启动项有结构化证据，可以打开应用详情尝试本地审核。");
        if (serviceCount > 0)
            lines.Add(serviceCount + " \u4e2a\u540e\u53f0\u670d\u52a1\u9700\u8981\u533a\u5206\u662f\u5fc5\u8981\u529f\u80fd\u8fd8\u662f\u5e38\u9a7b\u5360\u7528\u3002");
        if (taskCount > 0)
            lines.Add(taskCount + " \u4e2a\u8ba1\u5212\u4efb\u52a1\u9700\u8981\u786e\u8ba4\u662f\u66f4\u65b0\u3001\u540c\u6b65\u8fd8\u662f\u53ef\u9009\u540e\u53f0\u4efb\u52a1\u3002");
        if (processCount > 0)
            lines.Add(processCount + " \u4e2a\u6b63\u5728\u8fd0\u884c\u7684\u540e\u53f0\u8fdb\u7a0b\u53ea\u80fd\u5148\u89c2\u5bdf\uff0c\u4e0d\u4ee3\u8868\u53ef\u4ee5\u76f4\u63a5\u7ed3\u675f\u3002");
        if (readOnlyCount > 0)
            lines.Add(readOnlyCount + " 个系统相关或归属待确认应用仅供查看，不进入普通后台操作。");

        return lines;
    }

    private static IReadOnlyList<string> BuildPlanSteps(
        int ordinaryCount,
        int readOnlyCount,
        int localReviewCount)
    {
        var steps = new List<string>
        {
            "\u5148\u5224\u65ad\u662f\u5426\u5fc5\u8981\uff1a\u540c\u6b65\u3001\u5b89\u5168\u3001\u9a71\u52a8\u548c\u8f93\u5165\u8bbe\u5907\u7c7b\u80fd\u529b\u4f18\u5148\u4fdd\u7559\u3002"
        };

        if (ordinaryCount > 0)
            steps.Add("\u4f18\u5148\u68c0\u67e5\u666e\u901a\u5e94\u7528\uff1a\u804a\u5929\u3001\u4e0b\u8f7d\u3001\u66f4\u65b0\u548c\u7f51\u76d8\u7c7b\u5e38\u9a7b\u5148\u751f\u6210\u65b9\u6848\u3002");

        if (localReviewCount > 0)
            steps.Add("打开具体应用详情并选择“管理自启动”；只有显示“审核关闭方案”时才进入本地确认。");

        if (readOnlyCount > 0)
            steps.Add("系统相关或归属待确认项只解释：不从普通后台入口生成关闭方案。");

        steps.Add("\u6700\u540e\u518d\u51b3\u5b9a\uff1a\u9700\u8981\u4f60\u786e\u8ba4\u3001\u5feb\u7167\u548c\u56de\u6eda\u8bb0\u5f55\u540e\uff0c\u624d\u80fd\u8fdb\u5165\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002");

        return steps;
    }

    private static bool IsResident(SoftwareProfile profile) =>
        profile.RunningProcesses.Count > 0
        || profile.StartupEntries.Count > 0
        || profile.Services.Count > 0
        || profile.ScheduledTasks.Count > 0;
}
