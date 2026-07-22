using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public sealed class GrowthFindingViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Detail { get; init; }
    public required string AgentSuggestion { get; init; }
    public GrowthFinding? Finding { get; init; }
}

public sealed class GrowthDecisionViewModel
{
    public required string Headline { get; init; }
    public required string EvidenceText { get; init; }
    public required string OneTimeAction { get; init; }
    public required string PreventionAction { get; init; }
    public required string SafetyText { get; init; }
    public string? TargetAppName { get; init; }
    public bool RequiresMoreObservation { get; init; }
    public bool CanOpenApp => !string.IsNullOrWhiteSpace(TargetAppName);
    public bool CanExecuteDirectly { get; init; }
}

public static class GrowthFindingPresenter
{
    public static GrowthFindingViewModel Create(GrowthFinding finding)
    {
        var owner = OwnerLabel(finding);
        return new GrowthFindingViewModel
        {
            Title = finding.IsNewObservation
                ? "首次记录：" + owner
                : finding.IsSustainedGrowth
                    ? "持续增长：" + owner
                    : "最近增长：" + owner,
            Summary = finding.IsNewObservation
                ? "当前 " + RootCauseReportBuilder.Fmt(finding.CurrentBytes)
                : "+" + RootCauseReportBuilder.Fmt(finding.GrowthBytes),
            Detail = Detail(finding),
            AgentSuggestion = Suggestion(owner, finding),
            Finding = finding
        };
    }

    public static IReadOnlyList<GrowthFindingViewModel> CreateList(IReadOnlyList<GrowthFinding> findings) =>
        findings.Count == 0
            ? [NoData()]
            : RankForDisplay(findings).Take(12).Select(Create).ToList();

    private static IEnumerable<GrowthFinding> RankForDisplay(
        IReadOnlyList<GrowthFinding> findings)
    {
        var attributed = findings
            .Where(item => item.SourceKind is GrowthSourceKind.Software
                or GrowthSourceKind.SharedSoftware)
            .ToArray();
        return findings
            .Where(item =>
                item.SourceKind is GrowthSourceKind.Software or GrowthSourceKind.SharedSoftware
                || !AttributedChildrenExplain(item, attributed))
            .OrderByDescending(item => item.IsSustainedGrowth)
            .ThenBy(item => item.IsNewObservation)
            .ThenByDescending(item => item.SourceKind == GrowthSourceKind.Software)
            .ThenByDescending(item => item.GrowthBytes);
    }

    private static bool AttributedChildrenExplain(
        GrowthFinding parent,
        IReadOnlyList<GrowthFinding> attributed)
    {
        var parentBytes = RelevantBytes(parent);
        if (parentBytes <= 0)
            return false;
        var candidates = attributed
            .Where(child =>
                child.IsNewObservation == parent.IsNewObservation
                && (!parent.IsSustainedGrowth || child.IsSustainedGrowth)
                && IsDescendantPath(parent.Path, child.Path))
            .ToArray();
        var explainedBytes = candidates
            .Where(child => !candidates.Any(ancestor =>
                !ancestor.Path.Equals(child.Path, StringComparison.OrdinalIgnoreCase)
                && IsDescendantPath(ancestor.Path, child.Path)))
            .Sum(child => (decimal)RelevantBytes(child));
        return explainedBytes >= (decimal)parentBytes * 0.8m;
    }

    private static long RelevantBytes(GrowthFinding finding) =>
        Math.Max(0, finding.IsNewObservation ? finding.CurrentBytes : finding.GrowthBytes);

    private static GrowthFindingViewModel NoData() =>
        new()
        {
            Title = "\u6682\u65e0\u589e\u957f\u8bb0\u5f55",
            Summary = "\u7b2c\u4e8c\u6b21\u626b\u63cf\u540e\u4f1a\u663e\u793a\u53d8\u5316",
            Detail = "\u589e\u957f\u699c\u7528\u6765\u627e\u51fa\u54ea\u4e2a\u8f6f\u4ef6\u6216\u7c7b\u578b\u5728\u6301\u7eed\u5360 C \u76d8\u3002",
            AgentSuggestion = "Agent \u5efa\u8bae\uff1a\u5148\u4fdd\u7559\u57fa\u51c6\u5feb\u7167\uff0c\u4e0b\u6b21\u626b\u63cf\u518d\u5224\u65ad\u589e\u957f\u6765\u6e90\u3002",
            Finding = null
        };

    internal static string OwnerLabel(GrowthFinding finding)
    {
        var owner = finding.OwnerSoftware;
        var path = finding.Path;
        if (finding.SourceKind == GrowthSourceKind.Software
            && !string.IsNullOrWhiteSpace(owner))
            return owner;
        if (finding.SourceKind == GrowthSourceKind.SharedSoftware)
            return "多个软件共用位置";
        var combined = (owner + " " + path).ToLower(CultureInfo.InvariantCulture);
        if (combined.Contains("recycle"))
            return "\u56de\u6536\u7ad9";
        if (combined.Contains("users"))
            return "\u7528\u6237\u6587\u4ef6";
        if (combined.Contains("program"))
            return "\u7a0b\u5e8f\u548c\u5de5\u5177";
        if (combined.Contains("windows"))
            return "\u7cfb\u7edf\u6587\u4ef6";
        if (combined.Contains("temp"))
            return "\u4e34\u65f6\u7f13\u5b58";
        if (combined.Contains("appdata"))
            return "\u8f6f\u4ef6\u6570\u636e";
        if (combined.Contains("pagefile"))
            return "\u865a\u62df\u5185\u5b58";

        return string.IsNullOrWhiteSpace(owner)
            ? "\u672a\u77e5\u6765\u6e90"
            : owner;
    }

    private static string Detail(GrowthFinding finding)
    {
        if (finding.IsNewObservation)
        {
            return $"这是第一次纳入观察，当前占用 {RootCauseReportBuilder.Fmt(finding.CurrentBytes)}；还不能判断增长速度。";
        }
        if (finding.IsSustainedGrowth)
        {
            return $"最近 {finding.ObservedSnapshots} 次观察中有 {finding.PositiveGrowthIntervals} 次变大，累计增加 {RootCauseReportBuilder.Fmt(finding.TrendGrowthBytes)}。";
        }
        var reason = finding.Reason.Contains("new", System.StringComparison.OrdinalIgnoreCase)
            ? "\u65b0\u51fa\u73b0"
            : "\u6bd4\u4e0a\u6b21\u53d8\u5927";
        return $"{reason}：{IntervalText(finding.ObservationInterval)}从 {RootCauseReportBuilder.Fmt(finding.PreviousBytes)} 到 {RootCauseReportBuilder.Fmt(finding.CurrentBytes)}。";
    }

    private static string Suggestion(string owner, GrowthFinding finding)
    {
        if (finding.IsNewObservation)
            return "Agent 建议：先把这次当作基线；下次扫描后再判断是否持续增长。";
        if (finding.IsSustainedGrowth && finding.SourceKind == GrowthSourceKind.Software)
            return $"Agent 建议：{owner} 已多次变大，先区分缓存、日志、下载或模型，再分别生成一次清理和防止继续增长的方案。";
        if (finding.SourceKind == GrowthSourceKind.Software)
            return $"Agent 建议：先查看 {owner} 的缓存、日志、下载或模型位置，再决定清理一次还是改到 D 盘。";
        if (finding.SourceKind == GrowthSourceKind.SharedSoftware)
            return "Agent 建议：这个位置被多个软件共用，先确认归属，不自动清理。";
        if (owner.Contains("\u56de\u6536\u7ad9", System.StringComparison.Ordinal))
            return "Agent \u5efa\u8bae\uff1a\u53ef\u751f\u6210\u6e05\u7a7a\u56de\u6536\u7ad9\u65b9\u6848\uff0c\u6267\u884c\u524d\u5148\u786e\u8ba4\u4e0d\u9700\u8981\u8fd8\u539f\u3002";
        if (owner.Contains("\u4e34\u65f6", System.StringComparison.Ordinal))
            return "Agent \u5efa\u8bae\uff1a\u53ef\u751f\u6210\u4f4e\u98ce\u9669\u9694\u79bb\u65b9\u6848\u3002";
        if (owner.Contains("\u7528\u6237", System.StringComparison.Ordinal))
            return "Agent \u5efa\u8bae\uff1a\u5148\u627e\u5927\u6587\u4ef6\u548c\u957f\u671f\u672a\u7528\u6587\u4ef6\uff0c\u4e0d\u81ea\u52a8\u5220\u7528\u6237\u6570\u636e\u3002";

        return "Agent \u5efa\u8bae\uff1a\u5148\u89c2\u5bdf\u589e\u957f\u8d8b\u52bf\uff0c\u518d\u751f\u6210\u5904\u7406\u65b9\u6848\u3002";
    }

    private static string IntervalText(TimeSpan interval)
    {
        if (interval >= TimeSpan.FromDays(1))
            return $"约 {Math.Max(1, (int)Math.Round(interval.TotalDays))} 天内";
        if (interval >= TimeSpan.FromHours(1))
            return $"约 {Math.Max(1, (int)Math.Round(interval.TotalHours))} 小时内";
        return string.Empty;
    }

    private static bool IsDescendantPath(string parent, string child)
    {
        try
        {
            var parentPath = Path.GetFullPath(parent)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            var childPath = Path.GetFullPath(child);
            return childPath.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

public static class GrowthDecisionPresenter
{
    public static GrowthDecisionViewModel Create(GrowthFinding? finding)
    {
        if (finding is null)
        {
            return new GrowthDecisionViewModel
            {
                Headline = "还没有足够的增长证据",
                EvidenceText = "第一次体检只建立基线；下一次扫描才能比较变化。",
                OneTimeAction = "现在：不急着清理，先保留这次快照。",
                PreventionAction = "以后：再次体检时，Agent 会优先解释增长最快的来源。",
                SafetyText = "这是只读判断，没有生成删除、迁移或系统修改操作。",
                RequiresMoreObservation = true,
                CanExecuteDirectly = false
            };
        }

        var owner = GrowthFindingPresenter.OwnerLabel(finding);
        var firstObservation = finding.IsNewObservation;
        var sustained = finding.IsSustainedGrowth;
        return new GrowthDecisionViewModel
        {
            Headline = firstObservation
                ? $"先记住 {owner} 当前占用"
                : sustained
                    ? $"{owner} 近期多次占用更多 C 盘"
                    : $"{owner} 这次比上次变大",
            EvidenceText = firstObservation
                ? $"这是第一次记录，当前约 {RootCauseReportBuilder.Fmt(finding.CurrentBytes)}，暂时不能证明它一直在增长。"
                : sustained
                    ? $"最近 {finding.ObservedSnapshots} 次观察中有 {finding.PositiveGrowthIntervals} 次变大，累计增加约 {RootCauseReportBuilder.Fmt(finding.TrendGrowthBytes)}；来源已定位，但具体内容仍需确认。"
                    : $"和上次相比增加约 {RootCauseReportBuilder.Fmt(finding.GrowthBytes)}；目前只有一次变化，还不能称为持续增长。",
            OneTimeAction = BuildOneTimeAction(owner, finding),
            PreventionAction = BuildPreventionAction(owner, finding),
            SafetyText = "Agent 只给判断和下一步；没有确认来源前，不会删除、迁移或修改软件设置。",
            TargetAppName = finding.SourceKind == GrowthSourceKind.Software
                ? finding.OwnerSoftware
                : null,
            RequiresMoreObservation = !sustained,
            CanExecuteDirectly = false
        };
    }

    private static string BuildOneTimeAction(string owner, GrowthFinding finding)
    {
        var text = (owner + " " + finding.Path).ToLowerInvariant();
        if (text.Contains("recycle") || owner.Contains("回收站", StringComparison.Ordinal))
            return "现在：先查看回收站内容，再生成清空方案。";
        if (text.Contains("cache") || text.Contains("temp") || text.Contains("log"))
            return "现在：先确认是可重建缓存或日志，再生成低风险隔离预案。";
        return finding.SourceKind switch
        {
            GrowthSourceKind.Software => $"现在：打开 {owner} 的应用详情，先区分安装文件、缓存、日志、下载或模型。",
            GrowthSourceKind.SharedSoftware => "现在：先找出哪些软件共用这个位置，不按单个软件直接处理。",
            GrowthSourceKind.UserArea => "现在：先查看长期未用的大文件和下载内容，不自动删除个人文件。",
            GrowthSourceKind.SystemArea => "现在：先让 Agent 解释系统用途，不直接清理系统区域。",
            _ => "现在：先确认来源；证据不足时保持观察。"
        };
    }

    private static string BuildPreventionAction(string owner, GrowthFinding finding)
    {
        var text = (owner + " " + finding.Path).ToLowerInvariant();
        if (text.Contains("recycle") || owner.Contains("回收站", StringComparison.Ordinal))
            return "以后：可以设置定期提醒，但清空前仍要由你确认。";
        if (finding.SourceKind == GrowthSourceKind.Software)
            return $"以后：优先在 {owner} 自己的设置里把缓存、下载或模型目录改到 D 盘；不能安全修改时只观察。";
        if (finding.SourceKind == GrowthSourceKind.SharedSoftware)
            return "以后：分别调整相关软件的存储设置，避免用一个重定向规则误伤多个软件。";
        if (finding.SourceKind == GrowthSourceKind.UserArea)
            return "以后：把下载、视频和归档默认位置改到 D 盘，并保留个人文件确认。";
        if (text.Contains("cache") || text.Contains("temp") || text.Contains("log"))
            return "以后：找到所属软件后优先修改它的缓存或日志位置，不全局改 Windows 目录。";
        return "以后：再观察一次趋势；系统区域或未知来源不做自动重定向。";
    }
}
