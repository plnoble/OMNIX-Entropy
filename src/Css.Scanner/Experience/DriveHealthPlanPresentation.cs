using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public enum DriveHealthPlanAction
{
    None,
    ReviewSafeCleanup,
    ReviewPersonalStorage,
    ReviewSpaceSources
}

public sealed class DriveHealthPlanViewModel
{
    public required string ScopeLabel { get; init; }
    public required string Headline { get; init; }
    public required string Progress { get; init; }
    public required IReadOnlyList<string> Steps { get; init; }
    public required string PrimaryActionLabel { get; init; }
    public required string SecondaryActionLabel { get; init; }
    public required string SafetyBoundary { get; init; }
    public required DriveHealthPlanAction PrimaryAction { get; init; }
    public required DriveHealthPlanAction SecondaryAction { get; init; }
    public required long TargetReleaseBytes { get; init; }
    public required long SafeCleanupBytes { get; init; }
    public required long RemainingGapBytes { get; init; }
    public required double SafeContributionPercent { get; init; }
    public required bool IsSafeCleanupMeaningful { get; init; }
    public bool HasPrimaryAction => PrimaryAction != DriveHealthPlanAction.None;
    public bool HasSecondaryAction =>
        SecondaryAction != DriveHealthPlanAction.None
        && SecondaryAction != PrimaryAction;
    public bool CanExecuteDirectly => false;
}

public static class DriveHealthPlanPresenter
{
    private const double ComfortUsedRatio = 0.80;
    private const double MeaningfulContributionRatio = 0.10;

    public static DriveHealthPlanViewModel Create(
        DriveScanResult result,
        IReadOnlyList<Recommendation> recommendations,
        int personalStorageCandidateCount,
        int rootCauseCount)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(recommendations);
        if (personalStorageCandidateCount < 0)
            throw new ArgumentOutOfRangeException(nameof(personalStorageCandidateCount));
        if (rootCauseCount < 0)
            throw new ArgumentOutOfRangeException(nameof(rootCauseCount));

        var totalBytes = Math.Max(0, result.TotalBytes);
        var usedBytes = Math.Clamp(result.UsedBytes, 0, totalBytes);
        var freeBytes = Math.Max(0, result.FreeBytes);
        var targetUsedBytes = (long)Math.Floor(totalBytes * ComfortUsedRatio);
        var targetReleaseBytes = Math.Max(0, usedBytes - targetUsedBytes);
        var safeRecommendations = recommendations
            .Where(item =>
                HealthFindingRiskPolicy.IsLowRiskClean(item.Action, item.Risk)
                && item.Reversibility == ReversibilityLevel.Reversible
                && item.Operation is not null)
            .ToArray();
        var investigationCount = recommendations.Count(item =>
            item.Action == RecommendationAction.Observe);
        var safeCleanupBytes = SaturatingSum(safeRecommendations
            .Select(item => Math.Max(0, item.EstimatedImpactBytes)));
        var usefulSafeCleanupBytes = Math.Min(targetReleaseBytes, safeCleanupBytes);
        var remainingGapBytes = Math.Max(0, targetReleaseBytes - usefulSafeCleanupBytes);
        var safeContributionRatio = targetReleaseBytes <= 0
            ? 0
            : (double)usefulSafeCleanupBytes / targetReleaseBytes;
        var safeContributionPercent = safeContributionRatio * 100;
        var safeCleanupAvailable = safeRecommendations.Length > 0
            && safeCleanupBytes > 0;
        var isSafeCleanupMeaningful = safeCleanupAvailable
            && safeContributionRatio >= MeaningfulContributionRatio;
        var usedPercent = totalBytes <= 0
            ? 0
            : usedBytes * 100d / totalBytes;
        var scope = DriveScanTargetPresenter.DriveLabel(result.Drive);
        var isSystemDrive = DiskScanScopePolicy.IsSystemDrive(
            result.Drive,
            Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        var storageDestination = isSystemDrive
            && scope.StartsWith("C ", StringComparison.OrdinalIgnoreCase)
                ? "D 盘"
                : isSystemDrive
                    ? "空间更充足的非系统盘"
                    : "空间更充足的其他磁盘";

        if (targetReleaseBytes == 0)
        {
            return new DriveHealthPlanViewModel
            {
                ScopeLabel = scope + "改善目标",
                Headline = scope + "目前在 OMNIX 舒适目标内",
                Progress = $"当前已使用 {usedPercent:0.0}%，剩余 {FormatBytes(freeBytes)}；不需要为了分数额外清理。",
                Steps =
                [
                    "保持：尽量保留至少 20% 空闲空间。",
                    "处理：只处理来源明确、低风险并且可回滚的内容。",
                    $"预防：定期体检增长来源，应用缓存和下载位置优先放到{storageDestination}。"
                ],
                PrimaryActionLabel = "暂时不用处理",
                SecondaryActionLabel = string.Empty,
                SafetyBoundary = "Agent 只给建议；没有必要时不会为了提高分数制造清理任务。",
                PrimaryAction = DriveHealthPlanAction.None,
                SecondaryAction = DriveHealthPlanAction.None,
                TargetReleaseBytes = 0,
                SafeCleanupBytes = safeCleanupBytes,
                RemainingGapBytes = 0,
                SafeContributionPercent = 0,
                IsSafeCleanupMeaningful = false
            };
        }

        var largerEvidenceAction = personalStorageCandidateCount > 0
            ? DriveHealthPlanAction.ReviewPersonalStorage
            : rootCauseCount > 0 || investigationCount > 0
                ? DriveHealthPlanAction.ReviewSpaceSources
                : DriveHealthPlanAction.None;
        var hasLargerEvidence = largerEvidenceAction != DriveHealthPlanAction.None;
        var minorSafeCleanupWithLargerEvidence = safeCleanupAvailable
            && !isSafeCleanupMeaningful
            && hasLargerEvidence;
        var primaryAction = safeCleanupAvailable
            && (isSafeCleanupMeaningful || !hasLargerEvidence)
                ? DriveHealthPlanAction.ReviewSafeCleanup
                : largerEvidenceAction;
        var secondaryAction = primaryAction switch
        {
            DriveHealthPlanAction.ReviewSafeCleanup when hasLargerEvidence =>
                largerEvidenceAction,
            DriveHealthPlanAction.ReviewPersonalStorage when safeCleanupAvailable =>
                DriveHealthPlanAction.ReviewSafeCleanup,
            DriveHealthPlanAction.ReviewPersonalStorage when rootCauseCount > 0 =>
                DriveHealthPlanAction.ReviewSpaceSources,
            DriveHealthPlanAction.ReviewSpaceSources when safeCleanupAvailable =>
                DriveHealthPlanAction.ReviewSafeCleanup,
            _ => DriveHealthPlanAction.None
        };

        var progress = minorSafeCleanupWithLargerEvidence
            ? $"不是没有可调整项，但 {FormatBytes(safeCleanupBytes)} 只占改善目标的 {safeContributionPercent:0.0}%，单独处理意义不大；即使处理完仍差 {FormatBytes(remainingGapBytes)}。先找能贡献更多空间的应用、个人文件或数据来源；这 {safeRecommendations.Length} 项低风险清理可作为顺手处理。"
            : safeCleanupAvailable
                ? $"不是没有可调整项：已找到 {safeRecommendations.Length} 项现在可以安全处理，约 {FormatBytes(safeCleanupBytes)}，可完成改善目标的 {safeContributionPercent:0.0}%；另有 {investigationCount} 项需要 Agent 先确认来源。处理后距离目标仍差 {FormatBytes(remainingGapBytes)}。"
            : $"现在没有已确认可以直接处理的项目，但不是没有改善方向：有 {Math.Max(investigationCount, rootCauseCount)} 条只读线索需要继续确认，不同线索可能指向同一批文件。先不要删除；距离目标还差 {FormatBytes(remainingGapBytes)}。";

        var firstStep = minorSafeCleanupWithLargerEvidence
            ? $"先找主要差额：距离目标仍差 {FormatBytes(remainingGapBytes)}，优先确认应用、个人文件和持续增长来源；这里先只读查看。"
            : safeCleanupAvailable
                ? $"先处理：{safeRecommendations.Length} 项低风险、可回滚内容，预计最多释放 {FormatBytes(safeCleanupBytes)}。"
            : "先别删除：目前没有低风险清理证据，先只读查看空间来源。";
        var secondStep = minorSafeCleanupWithLargerEvidence
            ? $"可选顺手：{safeRecommendations.Length} 项低风险内容约 {FormatBytes(safeCleanupBytes)}，确认后仍走隔离区和后悔药，不会直接删除。"
            : personalStorageCandidateCount > 0
                ? $"继续确认：仍差 {FormatBytes(remainingGapBytes)}，先只读查看 {personalStorageCandidateCount} 组大文件或个人文件候选，再看占用来源；不碰系统文件。"
            : investigationCount > 0
                ? $"继续确认：仍差 {FormatBytes(remainingGapBytes)}，让 Agent 逐步判断 {investigationCount} 个来源属于软件、缓存、驱动还是安装残留；不直接删除。"
                : $"继续确认：仍差 {FormatBytes(remainingGapBytes)}，优先查看应用和主要占用来源；不碰系统文件。";

        return new DriveHealthPlanViewModel
        {
            ScopeLabel = scope + "改善目标",
            Headline = $"再释放 {FormatBytes(targetReleaseBytes)}，可让 {scope} 回到 80% 以内",
            Progress = progress,
            Steps =
            [
                firstStep,
                secondStep,
                $"防止再长：下次体检比较增长；能在软件设置里改的缓存、下载或模型位置，优先改到{storageDestination}。"
            ],
            PrimaryActionLabel = primaryAction switch
            {
                DriveHealthPlanAction.ReviewSafeCleanup => "先看能明显改善的安全项",
                DriveHealthPlanAction.ReviewPersonalStorage => "先找能释放更多空间的大文件",
                DriveHealthPlanAction.ReviewSpaceSources => "先找能释放更多空间的来源",
                _ => "等待更多安全证据"
            },
            SecondaryActionLabel = secondaryAction switch
            {
                DriveHealthPlanAction.ReviewSafeCleanup =>
                    $"可选：顺手处理 {FormatBytes(safeCleanupBytes)}",
                DriveHealthPlanAction.ReviewPersonalStorage => "继续找更大的文件",
                DriveHealthPlanAction.ReviewSpaceSources => "继续找更大的占用",
                _ => string.Empty
            },
            SafetyBoundary = "这只是只读计划和页面引导，不会自动删除、移动或修改系统；真实处理仍要经过本地确认和后悔药管线。",
            PrimaryAction = primaryAction,
            SecondaryAction = secondaryAction,
            TargetReleaseBytes = targetReleaseBytes,
            SafeCleanupBytes = safeCleanupBytes,
            RemainingGapBytes = remainingGapBytes,
            SafeContributionPercent = safeContributionPercent,
            IsSafeCleanupMeaningful = isSafeCleanupMeaningful
        };
    }

    private static long SaturatingSum(IEnumerable<long> values)
    {
        long total = 0;
        foreach (var value in values)
        {
            if (value > long.MaxValue - total)
                return long.MaxValue;
            total += value;
        }

        return total;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? value.ToString("0") + " " + units[unit]
            : value.ToString("0.0") + " " + units[unit];
    }
}
