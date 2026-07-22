using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;

namespace Css.Core.Agent;

public sealed class AgentNextStepViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }
    public required IReadOnlyList<string> SafeNextActions { get; init; }
    public required IReadOnlyList<AgentNextActionViewModel> NavigationActions { get; init; }
    public required IReadOnlyList<string> BlockedActions { get; init; }
    public required string SafetyBoundary { get; init; }
    public required string PrivacyLine { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public sealed class AgentNextActionViewModel
{
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required string TargetPage { get; init; }
    public AppCatalogFilter? TargetAppFilter { get; init; }
    public bool IsNavigationOnly { get; init; } = true;
    public string AutomationId => TargetAppFilter is { } filter
        ? $"AgentNextAction_Apps_{filter}"
        : $"AgentNextAction_{TargetPage}";
}

public static class AgentNextStepPresenter
{
    private const int ResidentPriorityThreshold = 3;

    public static AgentNextStepViewModel Create(
        HealthCheckSummary? summary,
        IEnumerable<SoftwareProfile>? softwareProfiles)
    {
        var profiles = softwareProfiles?.ToList() ?? [];
        if (summary is null && profiles.Count == 0)
            return CreateEmpty();

        var candidates = AgentActionCandidateCatalog.Create(profiles);
        var ordinaryCDriveApps = candidates.OrdinaryCDriveProfiles.Count;
        var readOnlyCDriveApps = candidates.ReadOnlyCDriveProfiles.Count;
        var ordinaryResidentApps = candidates.OrdinaryResidentProfiles.Count;
        var readOnlyResidentApps = candidates.ReadOnlyResidentProfiles.Count;
        var firstCleanFinding = summary?.KeyFindings.FirstOrDefault(IsLowRiskCleanFinding);
        var prioritizeResidentApps = ShouldPrioritizeResidentApps(firstCleanFinding, ordinaryResidentApps);

        var title = firstCleanFinding is not null
            ? "\u5efa\u8bae\u5148\u5904\u7406 C \u76d8\u53ef\u56de\u6eda\u6e05\u7406"
            : prioritizeResidentApps
                ? "\u5efa\u8bae\u5148\u770b\u540e\u53f0\u5e38\u9a7b\u5e94\u7528"
                : ordinaryCDriveApps > 0
                ? "\u5efa\u8bae\u5148\u770b\u5360\u7528 C \u76d8\u7684\u5e94\u7528"
                : readOnlyCDriveApps > 0
                    ? "C \u76d8\u6709\u7cfb\u7edf\u76f8\u5173\u7ebf\u7d22\uff0c\u5148\u522b\u5904\u7406"
                : ordinaryResidentApps > 0
                    ? "\u5efa\u8bae\u5148\u770b\u540e\u53f0\u5e38\u9a7b\u5e94\u7528"
                    : readOnlyResidentApps > 0
                        ? "\u540e\u53f0\u6709\u7cfb\u7edf\u76f8\u5173\u7ebf\u7d22\uff0c\u5148\u522b\u5173"
                    : "\u76ee\u524d\u5148\u89c2\u5bdf\uff0c\u4e0d\u6025\u7740\u52a8\u7535\u8111";

        var summaryText = firstCleanFinding is not null
            ? "\u5148\u4ece\u4f4e\u98ce\u9669\u3001\u53ef\u56de\u6eda\u7684 C \u76d8\u95ee\u9898\u5165\u624b\uff0c\u7136\u540e\u518d\u770b\u5e94\u7528\u548c\u540e\u53f0\u5e38\u9a7b\u3002"
            : prioritizeResidentApps
                ? "\u626b\u63cf\u5230\u591a\u4e2a\u53ef\u80fd\u5f71\u54cd\u5f00\u673a\u548c\u540e\u53f0\u8d44\u6e90\u7684\u5e94\u7528\uff0c\u5148\u770b\u54ea\u4e9b\u662f\u5fc5\u8981\u5e38\u9a7b\uff0c\u518d\u5904\u7406 C \u76d8\u6216\u8fc1\u79fb\u95ee\u9898\u3002"
            : ordinaryCDriveApps == 0 && readOnlyCDriveApps > 0
                ? "C \u76d8\u7ebf\u7d22\u6765\u81ea\u7cfb\u7edf\u76f8\u5173\u6216\u5f52\u5c5e\u5f85\u786e\u8ba4\u9879\uff1b\u53ea\u5c55\u793a\u8bc1\u636e\uff0c\u4e0d\u8fdb\u5165\u666e\u901a\u5e94\u7528\u64cd\u4f5c\u3002"
            : "\u5148\u770b\u672c\u5730\u626b\u63cf\u7ed3\u8bba\uff0c\u6211\u4f1a\u628a\u80fd\u505a\u3001\u8be5\u5148\u505a\u548c\u6682\u65f6\u4e0d\u80fd\u505a\u7684\u4e8b\u5206\u5f00\u8bf4\u3002";

        return new AgentNextStepViewModel
        {
            Title = title,
            Summary = summaryText,
            Reasons = BuildReasons(summary, candidates),
            SafeNextActions = BuildSafeNextActions(firstCleanFinding, candidates, prioritizeResidentApps),
            NavigationActions = BuildNavigationActions(firstCleanFinding, candidates, prioritizeResidentApps),
            BlockedActions =
            [
                "\u6211\u4e0d\u4f1a\u76f4\u63a5\u5220\u9664\u6587\u4ef6\u3001\u8fc1\u79fb\u8f6f\u4ef6\u3001\u7981\u7528\u670d\u52a1\u6216\u6539\u6ce8\u518c\u8868\u3002",
                "\u9ad8\u98ce\u9669\u52a8\u4f5c\u5fc5\u987b\u5148\u6709\u5feb\u7167\u3001\u56de\u6eda\u65b9\u6848\u548c\u6700\u540e\u786e\u8ba4\u3002"
            ],
            SafetyBoundary = "\u8fd9\u4e9b\u662f\u672c\u5730\u5efa\u8bae\uff1b\u771f\u6b63\u6267\u884c\u5fc5\u987b\u8f6c\u6210\u672c\u5730\u64cd\u4f5c\u8ba1\u5212\u5e76\u518d\u6b21\u786e\u8ba4\u3002",
            PrivacyLine = "\u5f53\u524d\u53ea\u4f7f\u7528\u672c\u5730\u6458\u8981\u751f\u6210\u5efa\u8bae\uff1b\u4e0d\u4f1a\u4e0a\u4f20\u5b8c\u6574\u8def\u5f84\u6216\u9690\u79c1\u6587\u4ef6\u3002",
            CanExecuteDirectly = false
        };
    }

    private static AgentNextStepViewModel CreateEmpty() =>
        new()
        {
            Title = "\u5148\u505a\u4e00\u6b21\u4f53\u68c0",
            Summary = "\u6211\u8fd8\u6ca1\u6709\u672c\u673a\u626b\u63cf\u7ed3\u679c\uff0c\u5148\u505a\u53ea\u8bfb\u4f53\u68c0\uff0c\u518d\u7ed9\u4f60\u6392\u51fa\u4f18\u5148\u7ea7\u3002",
            Reasons =
            [
                "\u8fd8\u6ca1\u6709 C \u76d8\u4f53\u68c0\u7ed3\u679c\u3002",
                "\u8fd8\u6ca1\u6709\u5e94\u7528\u626b\u63cf\u7ed3\u679c\u3002"
            ],
            SafeNextActions =
            [
                "\u5f00\u59cb\u4f53\u68c0\uff1a\u5148\u626b\u63cf C \u76d8\u548c\u53ef\u5b89\u5168\u89c2\u5bdf\u7684\u9879\u76ee\u3002",
                "\u626b\u63cf\u5e94\u7528\uff1a\u8bc6\u522b\u8f6f\u4ef6\u88c5\u5728\u54ea\u91cc\u3001\u662f\u5426\u5e38\u9a7b\u540e\u53f0\u3002"
            ],
            NavigationActions =
            [
                new()
                {
                    Label = "\u53bb\u9996\u9875\u4f53\u68c0",
                    Description = "\u53ea\u6253\u5f00\u4f53\u68c0\u9875\uff0c\u9700\u8981\u4f60\u518d\u70b9\u51fb\u5f00\u59cb\u4f53\u68c0\u3002",
                    TargetPage = "Home"
                },
                new()
                {
                    Label = "\u53bb\u5e94\u7528\u7ba1\u7406",
                    Description = "\u53ea\u6253\u5f00\u5e94\u7528\u9875\uff0c\u626b\u63cf\u540e\u518d\u770b\u88c5\u5728\u54ea\u91cc\u548c\u662f\u5426\u5e38\u9a7b\u3002",
                    TargetPage = "Apps"
                }
            ],
            BlockedActions =
            [
                "\u6211\u4e0d\u4f1a\u5728\u6ca1\u6709\u626b\u63cf\u8bc1\u636e\u65f6\u5220\u9664\u3001\u8fc1\u79fb\u3001\u7981\u7528\u670d\u52a1\u6216\u6539\u6ce8\u518c\u8868\u3002"
            ],
            SafetyBoundary = "\u6ca1\u6709\u8bc1\u636e\u65f6\u53ea\u80fd\u505a\u4f53\u68c0\u548c\u89e3\u91ca\u3002",
            PrivacyLine = "\u5f53\u524d\u53ea\u4f7f\u7528\u672c\u5730\u6458\u8981\u751f\u6210\u5efa\u8bae\u3002",
            CanExecuteDirectly = false
        };

    private static IReadOnlyList<string> BuildReasons(
        HealthCheckSummary? summary,
        AgentActionCandidateCatalog candidates)
    {
        var reasons = new List<string>();

        if (summary is not null)
        {
            reasons.Add("\u7efc\u5408\u8bc4\u5206 " + summary.OverallScore + " \u5206\u3002");

            foreach (var finding in summary.KeyFindings.Take(2))
                reasons.Add(finding.Text);
        }

        if (candidates.OrdinaryCDriveProfiles.Count > 0)
            reasons.Add($"{candidates.OrdinaryCDriveProfiles.Count} \u4e2a\u5e94\u7528\u5360\u7528\u6216\u5199\u5165 C \u76d8\uff1b\u5176\u4e2d {candidates.OrdinaryCDriveProfiles.Count} \u4e2a\u666e\u901a\u5e94\u7528\u53ef\u5728\u5e94\u7528\u7ba1\u7406\u4e2d\u590d\u67e5\u3002");

        if (candidates.ReadOnlyCDriveProfiles.Count > 0)
            reasons.Add(candidates.ReadOnlyCDriveProfiles.Count + " \u4e2a\u7cfb\u7edf\u76f8\u5173\u6216\u5f52\u5c5e\u5f85\u786e\u8ba4\u9879\u4e5f\u6709 C \u76d8\u7ebf\u7d22\uff0c\u4ec5\u4f9b\u67e5\u770b\u3002");

        if (candidates.OrdinaryResidentProfiles.Count > 0)
            reasons.Add($"{candidates.OrdinaryResidentProfiles.Count} \u4e2a\u5e94\u7528\u6b63\u5728\u540e\u53f0\u5e38\u9a7b\uff1b\u5176\u4e2d {candidates.OrdinaryResidentProfiles.Count} \u4e2a普通应用可先查看用途。");

        if (candidates.ReadOnlyResidentProfiles.Count > 0)
            reasons.Add(candidates.ReadOnlyResidentProfiles.Count + " \u4e2a\u7cfb\u7edf\u76f8\u5173\u6216\u5f52\u5c5e\u5f85\u786e\u8ba4\u9879\u6709\u540e\u53f0\u7ebf\u7d22\uff0c\u4ec5\u4f9b\u67e5\u770b\u3002");

        if (reasons.Count == 0)
            reasons.Add("\u6682\u65f6\u6ca1\u6709\u53d1\u73b0\u9700\u8981\u7acb\u523b\u5904\u7406\u7684\u9879\u76ee\u3002");

        return reasons;
    }

    private static IReadOnlyList<string> BuildSafeNextActions(
        HealthFinding? firstCleanFinding,
        AgentActionCandidateCatalog candidates,
        bool prioritizeResidentApps)
    {
        var actions = new List<string>();

        if (prioritizeResidentApps)
            actions.Add("\u67e5\u770b\u540e\u53f0\u5e38\u9a7b\uff1a\u5148\u8ba9 Agent \u5224\u65ad\u54ea\u4e9b\u53ef\u80fd\u5f71\u54cd\u5f00\u673a\uff0c\u54ea\u4e9b\u662f\u540c\u6b65\u3001\u9a71\u52a8\u6216\u5b89\u5168\u7ec4\u4ef6\u3002");

        if (firstCleanFinding is not null)
            actions.Add("\u6253\u5f00 C \u76d8\u6e05\u7406\uff1a\u5148\u9009\u62e9\u4f4e\u98ce\u9669\u6e05\u7406\u5361\uff0c\u786e\u8ba4\u540e\u53ea\u79fb\u5230\u9694\u79bb\u533a\u3002");

        if (candidates.MigrationReviewProfiles.Count > 0)
            actions.Add($"\u6253\u5f00\u5e94\u7528\u7ba1\u7406\uff1a\u6709 {candidates.MigrationReviewProfiles.Count} \u4e2a\u666e\u901a\u5e94\u7528\u7684 C \u76d8\u4e3b\u7a0b\u5e8f\u53ef\u8bc4\u4f30\u4e3b\u7a0b\u5e8f\u8fc1\u79fb\uff1b\u5148\u770b\u65b9\u6848\uff0c\u4e0d\u4f1a\u76f4\u63a5\u79fb\u52a8\u3002");

        if (candidates.DataLocationReviewProfiles.Count > 0)
            actions.Add($"\u53e6\u6709 {candidates.DataLocationReviewProfiles.Count} \u4e2a\u666e\u901a\u5e94\u7528\u4e3b\u7a0b\u5e8f\u5df2\u5728 D \u76d8\u6216\u4f4d\u7f6e\u8fd8\u4e0d\u660e\u786e\uff0c\u53ea\u590d\u67e5 C \u76d8\u6570\u636e\u6765\u6e90\uff0c\u4e0d\u91cd\u590d\u8fc1\u79fb\u4e3b\u7a0b\u5e8f\u3002");

        if (candidates.ReadOnlyCDriveProfiles.Count > 0)
            actions.Add("\u7cfb\u7edf\u76f8\u5173\u6216\u5f52\u5c5e\u5f85\u786e\u8ba4\u7684 C \u76d8\u7ebf\u7d22\u4ec5\u4f9b\u67e5\u770b\uff0c\u4e0d\u8fdb\u5165\u666e\u901a\u64cd\u4f5c\u3002");

        if (candidates.OrdinaryResidentProfiles.Count > 0 && !prioritizeResidentApps)
            actions.Add("\u67e5\u770b\u540e\u53f0\u5e38\u9a7b\uff1a\u5148\u8ba9 Agent \u89e3\u91ca\u5b83\u662f\u5426\u5fc5\u8981\uff0c\u4e0d\u76f4\u63a5\u5173\u3002");

        if (candidates.ReadOnlyResidentProfiles.Count > 0)
            actions.Add("\u7cfb\u7edf\u76f8\u5173\u6216\u5f52\u5c5e\u5f85\u786e\u8ba4\u7684\u540e\u53f0\u7ebf\u7d22\u53ea\u89e3\u91ca\uff0c\u4e0d\u4ece\u666e\u901a\u5e94\u7528\u5165\u53e3\u5173\u95ed\u3002");

        if (actions.Count == 0)
            actions.Add("\u5148\u4fdd\u6301\u89c2\u5bdf\uff1a\u7b49\u4e0b\u6b21\u4f53\u68c0\u6216\u5e94\u7528\u626b\u63cf\u518d\u51b3\u5b9a\u3002");

        return actions;
    }

    private static IReadOnlyList<AgentNextActionViewModel> BuildNavigationActions(
        HealthFinding? firstCleanFinding,
        AgentActionCandidateCatalog candidates,
        bool prioritizeResidentApps)
    {
        var actions = new List<AgentNextActionViewModel>();

        if (prioritizeResidentApps)
        {
            actions.Add(new AgentNextActionViewModel
            {
                Label = "\u67e5\u770b\u540e\u53f0\u5e38\u9a7b",
                Description = "\u5148\u67e5\u770b\u54ea\u4e9b\u5e94\u7528\u5728\u540e\u53f0\u6216\u5f00\u673a\u8fd0\u884c\uff0c\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed\u3002",
                TargetPage = "Apps",
                TargetAppFilter = AppCatalogFilter.Resident
            });
        }

        if (firstCleanFinding is not null)
        {
            actions.Add(new AgentNextActionViewModel
            {
                Label = "\u6253\u5f00 C \u76d8\u6e05\u7406",
                Description = "\u53ea\u5e26\u4f60\u53bb\u770b C \u76d8\u5efa\u8bae\uff0c\u4e0d\u4f1a\u76f4\u63a5\u6e05\u7406\u6216\u5220\u9664\u3002",
                TargetPage = "CDrive"
            });
        }

        if (candidates.CDriveProfiles.Count > 0)
        {
            actions.Add(new AgentNextActionViewModel
            {
                Label = "\u6253\u5f00\u5e94\u7528\u7ba1\u7406",
                Description = "\u5148\u67e5\u770b\u5e76\u5206\u6e05\u666e\u901a\u5e94\u7528\u3001C \u76d8\u6570\u636e\u7ebf\u7d22\u548c\u7cfb\u7edf\u53ea\u8bfb\u9879\uff0c\u4e0d\u4f1a\u76f4\u63a5\u8fc1\u79fb\u3002",
                TargetPage = "Apps",
                TargetAppFilter = AppCatalogFilter.CDrive
            });
        }
        else if ((candidates.OrdinaryResidentProfiles.Count > 0
                  || candidates.ReadOnlyResidentProfiles.Count > 0)
                 && !prioritizeResidentApps)
        {
            actions.Add(new AgentNextActionViewModel
            {
                Label = "\u67e5\u770b\u540e\u53f0\u5e38\u9a7b",
                Description = "\u5148\u67e5\u770b\u54ea\u4e9b\u5e94\u7528\u5728\u540e\u53f0\u8fd0\u884c\uff0c\u4e0d\u4f1a\u76f4\u63a5\u5173\u95ed\u3002",
                TargetPage = "Apps",
                TargetAppFilter = AppCatalogFilter.Resident
            });
        }

        if (firstCleanFinding is not null)
        {
            actions.Add(new AgentNextActionViewModel
            {
                Label = "\u67e5\u770b\u540e\u6094\u836f",
                Description = "\u5148\u7406\u89e3\u9694\u79bb\u533a\u548c\u8fd8\u539f\u5165\u53e3\uff0c\u4e0d\u4f1a\u81ea\u52a8\u5220\u9664\u9694\u79bb\u5185\u5bb9\u3002",
                TargetPage = "Timeline"
            });
        }

        if (actions.Count == 0)
        {
            actions.Add(new AgentNextActionViewModel
            {
                Label = "\u56de\u5230\u9996\u9875\u4f53\u68c0",
                Description = "\u53ea\u6253\u5f00\u4f53\u68c0\u9875\uff0c\u7b49\u4e0b\u6b21\u626b\u63cf\u518d\u51b3\u5b9a\u3002",
                TargetPage = "Home"
            });
        }

        return actions;
    }

    private static bool IsLowRiskCleanFinding(HealthFinding finding) =>
        HealthFindingRiskPolicy.IsLowRiskClean(finding.Action, finding.Risk);

    private static bool ShouldPrioritizeResidentApps(HealthFinding? firstCleanFinding, int residentApps) =>
        firstCleanFinding is null && residentApps >= ResidentPriorityThreshold;
}
