using System;
using System.Collections.Generic;
using System.Linq;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public sealed class PersonalStorageFindingViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string AgentSuggestion { get; init; }
    public required string SafetyText { get; init; }
    public IReadOnlyList<string> EvidencePaths { get; init; } = [];
    public bool CanInspectLocations => EvidencePaths.Count > 0;
    public bool CanExecuteDirectly { get; init; }
}

public sealed class PersonalStorageFindingListViewModel
{
    public required string Summary { get; init; }
    public IReadOnlyList<PersonalStorageFindingViewModel> Items { get; init; } = [];
}

public static class PersonalStorageFindingPresenter
{
    public static PersonalStorageFindingListViewModel Create(
        PersonalStorageAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        var rows = analysis.Findings
            .Select(CreateItem)
            .ToArray();
        var summary = rows.Length == 0
            ? "个人文件夹里暂未发现长期未用的大文件或明显的同名同大小候选。"
            : $"发现 {rows.Length} 条个人文件候选；先确认用途，不会自动删除。";
        if (analysis.WasTruncated)
            summary += " 文件数量较多，本次只分析了有界范围。";

        return new PersonalStorageFindingListViewModel
        {
            Summary = summary,
            Items = rows
        };
    }

    private static PersonalStorageFindingViewModel CreateItem(
        PersonalStorageFinding finding) =>
        finding.Kind switch
        {
            PersonalStorageFindingKind.LongUnusedLargeFile => new PersonalStorageFindingViewModel
            {
                Title = "长期未用的大文件：" + finding.DisplayName,
                Summary = $"约 {RootCauseReportBuilder.Fmt(finding.ItemSizeBytes)}，最后修改于 {FormatDate(finding.LastWriteUtc)}。",
                AgentSuggestion = "建议先确认是否还需要；需要保留时可考虑归档到非系统盘。",
                SafetyText = "只读候选，没有生成删除或移动操作。",
                EvidencePaths = InspectablePaths(finding),
                CanExecuteDirectly = false
            },
            _ => new PersonalStorageFindingViewModel
            {
                Title = $"疑似重复：{finding.DisplayName}（{finding.ItemCount} 份）",
                Summary = $"名称和大小相同，每份约 {RootCauseReportBuilder.Fmt(finding.ItemSizeBytes)}；最多可能重复占用 {RootCauseReportBuilder.Fmt(finding.CandidateBytes)}。",
                AgentSuggestion = "这还不是重复内容证明；请打开文件核对后再决定保留哪一份。",
                SafetyText = "没有读取或比对文件内容，不能据此自动删除。",
                EvidencePaths = InspectablePaths(finding),
                CanExecuteDirectly = false
            }
        };

    private static IReadOnlyList<string> InspectablePaths(PersonalStorageFinding finding) =>
        finding.EvidencePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToLocalTime().ToString("yyyy-MM-dd") : "未知日期";
}
