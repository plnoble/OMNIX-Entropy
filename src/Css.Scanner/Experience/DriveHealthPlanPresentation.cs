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
    public required string SafetyBoundary { get; init; }
    public required DriveHealthPlanAction PrimaryAction { get; init; }
    public required long TargetReleaseBytes { get; init; }
    public required long SafeCleanupBytes { get; init; }
    public required long RemainingGapBytes { get; init; }
    public bool HasPrimaryAction => PrimaryAction != DriveHealthPlanAction.None;
    public bool CanExecuteDirectly => false;
}

public static class DriveHealthPlanPresenter
{
    private const double ComfortUsedRatio = 0.80;

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
        var safeCleanupBytes = SaturatingSum(safeRecommendations
            .Select(item => Math.Max(0, item.EstimatedImpactBytes)));
        var usefulSafeCleanupBytes = Math.Min(targetReleaseBytes, safeCleanupBytes);
        var remainingGapBytes = Math.Max(0, targetReleaseBytes - usefulSafeCleanupBytes);
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
                SafetyBoundary = "Agent 只给建议；没有必要时不会为了提高分数制造清理任务。",
                PrimaryAction = DriveHealthPlanAction.None,
                TargetReleaseBytes = 0,
                SafeCleanupBytes = safeCleanupBytes,
                RemainingGapBytes = 0
            };
        }

        var primaryAction = safeRecommendations.Length > 0 && safeCleanupBytes > 0
            ? DriveHealthPlanAction.ReviewSafeCleanup
            : personalStorageCandidateCount > 0
                ? DriveHealthPlanAction.ReviewPersonalStorage
                : rootCauseCount > 0
                    ? DriveHealthPlanAction.ReviewSpaceSources
                    : DriveHealthPlanAction.None;

        var progress = safeCleanupBytes > 0
            ? $"当前已使用 {usedPercent:0.0}%。已确认低风险内容约 {FormatBytes(safeCleanupBytes)}；处理后距离目标仍差 {FormatBytes(remainingGapBytes)}。"
            : $"当前已使用 {usedPercent:0.0}%。现在没有已确认可安全清理的内容，先不要删除；距离目标还差 {FormatBytes(remainingGapBytes)}。";

        var firstStep = safeCleanupBytes > 0
            ? $"先处理：{safeRecommendations.Length} 项低风险、可回滚内容，预计最多释放 {FormatBytes(safeCleanupBytes)}。"
            : "先别删除：目前没有低风险清理证据，先只读查看空间来源。";
        var secondStep = personalStorageCandidateCount > 0
            ? $"再腾空间：仍差 {FormatBytes(remainingGapBytes)}，先只读查看 {personalStorageCandidateCount} 组大文件或个人文件候选，再看占用来源；不碰系统文件。"
            : $"再腾空间：仍差 {FormatBytes(remainingGapBytes)}，优先查看大文件和占用来源；不碰系统文件。";

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
                DriveHealthPlanAction.ReviewSafeCleanup => "先看 Agent 选好的最安全一项",
                DriveHealthPlanAction.ReviewPersonalStorage => "先看可移动的大文件",
                DriveHealthPlanAction.ReviewSpaceSources => "先看主要占用来源",
                _ => "等待更多安全证据"
            },
            SafetyBoundary = "这只是只读计划和页面引导，不会自动删除、移动或修改系统；真实处理仍要经过本地确认和后悔药管线。",
            PrimaryAction = primaryAction,
            TargetReleaseBytes = targetReleaseBytes,
            SafeCleanupBytes = safeCleanupBytes,
            RemainingGapBytes = remainingGapBytes
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
