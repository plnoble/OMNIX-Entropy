using Css.Core.Agent;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public static class AgentDecisionContextBuilder
{
    public static AgentDecisionContext Build(
        CDriveRootCauseSummary? rootCauseSummary,
        DriveHealthPlanViewModel? drivePlan,
        IReadOnlyList<GrowthFinding> growthFindings,
        int observedSnapshotCount)
    {
        ArgumentNullException.ThrowIfNull(growthFindings);

        var storageSources = (rootCauseSummary?.Cards ?? [])
            .Where(card => card.EvidenceBytes > 0)
            .OrderByDescending(card => card.EvidenceBytes)
            .Take(6)
            .Select(card => new AgentStorageSourceEvidence
            {
                PrimaryText = SafeText(card.PrimaryText, "占用来源已隐藏"),
                Explanation = SafeText(card.Explanation, "这是只读占用线索。"),
                Suggestion = SafeText(card.AgentSuggestion, "先确认来源，不直接处理。"),
                EvidenceBytes = card.EvidenceBytes,
                IsLowerBound = card.IsSizeLowerBound
            })
            .ToArray();

        var growthSources = GrowthFindingPresenter.CreateList(growthFindings)
            .Select(item => item.Finding)
            .Where(finding => finding is not null)
            .Cast<GrowthFinding>()
            .Take(8)
            .Select(CreateGrowthEvidence)
            .ToArray();

        return new AgentDecisionContext
        {
            DrivePlan = drivePlan is null
                ? null
                : new AgentDrivePlanEvidence
                {
                    Headline = SafeText(drivePlan.Headline, "磁盘改善目标已生成"),
                    Progress = SafeText(drivePlan.Progress, "请打开磁盘页面查看当前计划。"),
                    Steps = drivePlan.Steps
                        .Select(step => SafeText(step, "先确认来源，不直接处理。"))
                        .ToArray(),
                    TargetReleaseBytes = drivePlan.TargetReleaseBytes,
                    SafeCleanupBytes = drivePlan.SafeCleanupBytes,
                    RemainingGapBytes = drivePlan.RemainingGapBytes
                },
            StorageSources = storageSources,
            GrowthSources = growthSources,
            ObservedSnapshotCount = Math.Max(0, observedSnapshotCount)
        };
    }

    private static AgentGrowthSourceEvidence CreateGrowthEvidence(GrowthFinding finding)
    {
        var decision = GrowthDecisionPresenter.Create(finding);
        var owner = SafeText(
            GrowthFindingPresenter.OwnerLabel(finding),
            "未知来源");
        return new AgentGrowthSourceEvidence
        {
            OwnerLabel = owner,
            LatestGrowthBytes = Math.Max(0, finding.GrowthBytes),
            TrendGrowthBytes = Math.Max(0, finding.TrendGrowthBytes),
            ObservationInterval = finding.ObservationInterval > TimeSpan.Zero
                ? finding.ObservationInterval
                : TimeSpan.Zero,
            TrendWindow = finding.TrendWindow > TimeSpan.Zero
                ? finding.TrendWindow
                : TimeSpan.Zero,
            ObservedSnapshots = Math.Max(0, finding.ObservedSnapshots),
            IsFirstObservation = finding.IsNewObservation,
            IsSustainedGrowth = finding.IsSustainedGrowth,
            OneTimeAction = SafeText(
                decision.OneTimeAction,
                "现在：先确认内容类型，不直接处理。"),
            PreventionAction = SafeText(
                decision.PreventionAction,
                "以后：再次体检并观察是否继续增长。"),
            TargetAppName = IsSafeText(decision.TargetAppName)
                ? decision.TargetAppName
                : null
        };
    }

    private static string SafeText(string? value, string fallback) =>
        IsSafeText(value) ? value!.Trim() : fallback;

    private static bool IsSafeText(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Contains(":\\", StringComparison.Ordinal)
        && !value.Contains(":/", StringComparison.Ordinal)
        && !value.StartsWith("\\\\", StringComparison.Ordinal);
}
