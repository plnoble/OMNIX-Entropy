using System;
using System.Collections.Generic;

namespace Css.Core.Timeline;

public sealed class ActionTimelineItemViewModel
{
    public long Id { get; init; }
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public required string TechnicalDetailsButtonText { get; init; }
    public IReadOnlyList<string> TechnicalDetails { get; init; } = [];
    public required string RestoreLine { get; init; }
    public required string RestoreButtonText { get; init; }
    public required string RestoreHint { get; init; }
    public bool CanRestore { get; init; }
    public string? RestoreOperationKind { get; init; }
    public IReadOnlyList<string> RestoreManifestPaths { get; init; } = [];
}

public static class ActionTimelinePresenter
{
    public static ActionTimelineItemViewModel CreateItem(ActionTimelineEntry entry)
    {
        var supportedRestore = entry.RestoreOperationKind?.Equals("quarantine.restore", StringComparison.OrdinalIgnoreCase) == true
            || entry.RestoreOperationKind?.Equals("startup.restore.hkcu.run", StringComparison.OrdinalIgnoreCase) == true;
        var canRestore = entry.RestoreState == RestoreState.Restorable
            && supportedRestore
            && entry.RestoreManifestPaths.Count > 0;

        var affectedScope = BuildAffectedScope(entry);

        return new ActionTimelineItemViewModel
        {
            Id = entry.Id,
            Title = $"{entry.OccurredAt:yyyy-MM-dd HH:mm}  {entry.Title}",
            Detail = $"{SourceLabel(entry.Source)} / {entry.EvidenceSummary} / {affectedScope}",
            TechnicalDetailsButtonText = "\u67E5\u770B\u6280\u672F\u8BE6\u60C5",
            TechnicalDetails = BuildTechnicalDetails(entry),
            RestoreLine = RestoreLine(entry),
            RestoreButtonText = canRestore ? "还原" : "不可还原",
            RestoreHint = canRestore
                ? entry.RestoreOperationKind?.Equals("startup.restore.hkcu.run", StringComparison.OrdinalIgnoreCase) == true
                    ? "会按启动项 manifest 恢复原始值；如果当前位置已有值或权限已变化，不会覆盖。"
                    : "会按隔离区 manifest 还原到原路径；如果原路径已有内容，不会覆盖。"
                : "当前记录没有可用的还原入口。",
            CanRestore = canRestore,
            RestoreOperationKind = entry.RestoreOperationKind,
            RestoreManifestPaths = entry.RestoreManifestPaths
        };
    }

    public static ActionTimelineItemViewModel Message(string message) =>
        new()
        {
            Title = message,
            TechnicalDetailsButtonText = "\u67E5\u770B\u6280\u672F\u8BE6\u60C5",
            TechnicalDetails = ["\u8FD9\u662F\u72B6\u6001\u63D0\u793A\uFF0C\u6682\u65E0\u53EF\u5C55\u5F00\u7684\u8DEF\u5F84\u6216 manifest \u8BE6\u60C5\u3002"],
            Detail = "当前不会执行任何清理、迁移、卸载或系统修改。",
            RestoreLine = "只读状态",
            RestoreButtonText = "不可还原",
            RestoreHint = "没有可还原动作。",
            CanRestore = false
        };

    private static IReadOnlyList<string> BuildTechnicalDetails(ActionTimelineEntry entry)
    {
        var lines = new List<string>
        {
            $"\u8BB0\u5F55 ID\uFF1A{entry.Id}",
            $"\u6765\u6E90\uFF1A{SourceLabel(entry.Source)}",
            $"\u8FD8\u539F\u72B6\u6001\uFF1A{entry.RestoreState}",
            $"\u8FD8\u539F\u64CD\u4F5C\uFF1A{(string.IsNullOrWhiteSpace(entry.RestoreOperationKind) ? "\u65E0" : entry.RestoreOperationKind)}"
        };

        if (entry.AffectedPaths.Count == 0)
        {
            lines.Add("\u5F71\u54CD\u8DEF\u5F84\uFF1A\u672A\u8BB0\u5F55");
        }
        else
        {
            lines.Add("\u5F71\u54CD\u8DEF\u5F84\uFF1A");
            foreach (var path in entry.AffectedPaths)
                lines.Add("  - " + path);
        }

        if (entry.AffectedRegistryKeys.Count == 0)
        {
            lines.Add("影响注册表：未记录");
        }
        else
        {
            lines.Add("影响注册表：");
            foreach (var key in entry.AffectedRegistryKeys)
                lines.Add("  - " + key);
        }

        if (entry.RestoreManifestPaths.Count == 0)
        {
            lines.Add("manifest\uFF1A\u672A\u8BB0\u5F55");
        }
        else
        {
            lines.Add("manifest\uFF1A");
            foreach (var path in entry.RestoreManifestPaths)
                lines.Add("  - " + path);
        }

        return lines;
    }

    private static string BuildAffectedScope(ActionTimelineEntry entry)
    {
        if (entry.AffectedPaths.Count == 0 && entry.AffectedRegistryKeys.Count == 0)
            return "\u5F71\u54CD\u8303\u56F4\uFF1A\u6682\u672A\u8BB0\u5F55\u5177\u4F53\u4F4D\u7F6E";
        var parts = new List<string>();
        if (entry.AffectedPaths.Count > 0)
            parts.Add($"{entry.AffectedPaths.Count} 个位置");
        if (entry.AffectedRegistryKeys.Count > 0)
            parts.Add($"{entry.AffectedRegistryKeys.Count} 个注册表项");
        return "影响范围：" + string.Join("、", parts);
    }

    private static string RestoreLine(ActionTimelineEntry entry) =>
        entry.RestoreState switch
        {
            RestoreState.Restorable => "状态：可以还原，点击前会再次确认。",
            RestoreState.Restored => "状态：已还原。",
            RestoreState.PartiallyRestorable => "状态：暂不能自动还原，通常是原路径已有内容或部分文件缺失。",
            RestoreState.NotRestorable => "状态：不可还原。",
            _ => "状态：未知。"
        };

    private static string SourceLabel(Operations.OperationSource source) =>
        source switch
        {
            Operations.OperationSource.Agent => "Agent 建议",
            Operations.OperationSource.System => "系统记录",
            _ => "手动确认"
        };
}
