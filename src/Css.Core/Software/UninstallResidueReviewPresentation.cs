using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Software;

public sealed class UninstallResidueReviewViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string SafetyText { get; init; }
    public required string PrimaryButtonText { get; init; }
    public bool CanExecuteDirectly { get; init; }
    public bool CanMoveLowRiskToQuarantine { get; init; }
    public OperationDescriptor? LowRiskOperation { get; init; }
    public required IReadOnlyList<UninstallResidueReviewGroupViewModel> Groups { get; init; }
}

public sealed class UninstallResidueReviewGroupViewModel
{
    public required string Title { get; init; }
    public required string RiskLabel { get; init; }
    public required bool CanMoveToQuarantine { get; init; }
    public required string ActionLine { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
}

public static class UninstallResidueReviewPresentationBuilder
{
    public static UninstallResidueReviewViewModel Create(UninstallResidueScanReport report)
    {
        var operation = report.OfficialUninstallAppearsComplete
            ? UninstallResidueOperationPlanner.CreateLowRiskQuarantineOperation(report)
            : null;
        var groups = report.Groups.Select(CreateGroup).ToList();

        return new UninstallResidueReviewViewModel
        {
            Title = report.SoftwareName + " 卸载后残留复查",
            Summary = operation is null
                ? report.Summary
                : report.Summary + " 只处理低风险缓存/日志，其他项目只解释。",
            SafetyText = "当前不会自动删除或移动任何东西；只有低风险缓存/日志在二次确认后才会进入隔离区。",
            PrimaryButtonText = operation is null ? "暂不能处理" : "移动低风险残留到隔离区",
            CanExecuteDirectly = false,
            CanMoveLowRiskToQuarantine = operation is not null,
            LowRiskOperation = operation,
            Groups = groups
        };
    }

    private static UninstallResidueReviewGroupViewModel CreateGroup(UninstallResidueGroup group)
    {
        return new UninstallResidueReviewGroupViewModel
        {
            Title = group.Title,
            RiskLabel = RiskLabel(group.Risk),
            CanMoveToQuarantine = group.CanMoveToQuarantine,
            ActionLine = group.CanMoveToQuarantine
                ? "可在二次确认后移动到隔离区。"
                : "只解释，不自动处理。",
            Items = group.Candidates.Select(DescribeCandidate).ToList()
        };
    }

    private static string DescribeCandidate(UninstallResidueCandidate candidate)
    {
        var target = string.IsNullOrWhiteSpace(candidate.Path)
            ? candidate.Identifier ?? "未知项目"
            : candidate.Path;
        return candidate.EstimatedBytes > 0
            ? $"{KindLabel(candidate.Kind)}: {target} / 约 {candidate.EstimatedBytes} bytes"
            : $"{KindLabel(candidate.Kind)}: {target}";
    }

    private static string KindLabel(UninstallResidueKind kind) =>
        kind switch
        {
            UninstallResidueKind.CacheDirectory => "缓存",
            UninstallResidueKind.LogDirectory => "日志",
            UninstallResidueKind.DataDirectory => "用户数据",
            UninstallResidueKind.InstallDirectory => "安装目录",
            UninstallResidueKind.StartupEntry => "自启动",
            UninstallResidueKind.Service => "服务",
            UninstallResidueKind.ScheduledTask => "计划任务",
            _ => kind.ToString()
        };

    private static string RiskLabel(RiskLevel risk) =>
        risk switch
        {
            RiskLevel.Low => "低",
            RiskLevel.Medium => "中",
            RiskLevel.High => "高",
            RiskLevel.None => "无",
            _ => risk.ToString()
        };
}
