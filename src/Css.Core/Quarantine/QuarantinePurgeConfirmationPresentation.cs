using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Quarantine;

public sealed class QuarantinePurgeConfirmationViewModel
{
    public required string Title { get; init; }
    public required string Warning { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> OutcomeLines { get; init; }
    public required IReadOnlyList<string> TechnicalDetails { get; init; }
    public required string AcknowledgementText { get; init; }
    public required string ConfirmButtonText { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class QuarantinePurgeConfirmationPresenter
{
    public static QuarantinePurgeConfirmationViewModel Create(
        OperationDescriptor descriptor,
        QuarantineRetentionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(plan);
        var validation = QuarantinePurgeOperationPolicy.ValidateCandidate(descriptor);
        if (!validation.Success)
            throw new InvalidOperationException(validation.Error);

        return new QuarantinePurgeConfirmationViewModel
        {
            Title = "确认永久整理隔离区",
            Warning = "这不是普通清理。确认后会永久删除隔离副本，后悔药中心不能再还原这些项目。",
            Summary = descriptor.EvidenceSummary ?? "隔离区永久整理方案。",
            OutcomeLines =
            [
                $"本次最多整理 {descriptor.AffectedPaths.Count} 项。",
                $"预计最多释放 {FormatBytes(plan.ReclaimableBytes)}。",
                $"整理后可还原内容预计剩余 {FormatBytes(plan.ProjectedActiveBytes)}。",
                "只处理受管隔离目录中的副本和 manifest，不会删除原路径中的文件。",
                "任一记录路径不可信时，会在删除前拒绝整批方案。"
            ],
            TechnicalDetails = descriptor.AffectedPaths
                .Select(path => "manifest: " + path)
                .ToArray(),
            AcknowledgementText = "我知道永久整理后，这些项目不能还原。",
            ConfirmButtonText = "确认永久整理",
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
