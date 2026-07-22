using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Timeline;

namespace Css.Core.Quarantine;

public sealed class QuarantineRetentionCandidateViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string SafetyText { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public sealed class QuarantineRetentionViewModel
{
    public required string Headline { get; init; }
    public required string UsageText { get; init; }
    public required string ImpactText { get; init; }
    public required string SafetyText { get; init; }
    public IReadOnlyList<QuarantineRetentionCandidateViewModel> Candidates { get; init; } = [];
    public bool CanGeneratePlan { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class QuarantineRetentionPresenter
{
    public static QuarantineRetentionViewModel Create(QuarantineRetentionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var candidates = plan.Candidates.Select(CreateCandidate).ToArray();
        return new QuarantineRetentionViewModel
        {
            Headline = candidates.Length == 0
                ? "隔离区状态正常"
                : $"隔离区有 {candidates.Length} 项建议整理",
            UsageText = $"当前可还原内容约 {FormatBytes(plan.ActiveBytes)}，上限 {FormatBytes(plan.MaxTotalBytes)}，保留期 {plan.RetentionDays} 天。",
            ImpactText = candidates.Length == 0
                ? "当前不需要生成永久整理方案。"
                : $"全部确认后预计最多释放 {FormatBytes(plan.ReclaimableBytes)}，可还原内容预计剩余 {FormatBytes(plan.ProjectedActiveBytes)}。",
            SafetyText = "这里只读盘点，不会自动删除。永久整理会失去还原能力，必须再次查看清单并明确确认。" +
                (plan.WasTruncated ? " 候选较多，本次只显示并处理第一批。" : string.Empty),
            Candidates = candidates,
            CanGeneratePlan = candidates.Length > 0,
            CanExecuteDirectly = false
        };
    }

    private static QuarantineRetentionCandidateViewModel CreateCandidate(
        QuarantineCleanupCandidate candidate)
    {
        var title = candidate.Reason switch
        {
            QuarantineCleanupReason.Expired => "已超过保留期",
            QuarantineCleanupReason.OverCapacity => "容量空间紧张",
            _ => "已还原的记录"
        };
        var size = candidate.EstimatedReclaimableBytes > 0
            ? $"预计可释放 {FormatBytes(candidate.EstimatedReclaimableBytes)}。"
            : "隔离副本已还原，只剩记录可整理。";
        return new QuarantineRetentionCandidateViewModel
        {
            Title = title,
            Summary = candidate.ReasonText + " " + size,
            SafetyText = candidate.Record.RestoreState == RestoreState.Restored
                ? "整理记录不会再次移动原文件。"
                : "永久整理后，这一项不能再从后悔药中心还原。",
            CanExecuteDirectly = false
        };
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
        return unit == 0 ? $"{bytes} B" : $"{value:0.0} {units[unit]}";
    }
}
