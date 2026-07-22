using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public sealed class CDriveRootCauseSummary
{
    public required string Headline { get; init; }
    public required string Subheadline { get; init; }
    public required bool TechnicalReportAvailable { get; init; }
    public required IReadOnlyList<CDriveRootCauseCard> Cards { get; init; }
}

public enum CDriveRootCauseAction
{
    None,
    OpenRecycleBin,
    OpenCDriveApps,
    ReviewPersonalStorage,
    ReviewCleanupRecommendations
}

public sealed class CDriveRootCauseCard
{
    public required string Title { get; init; }
    public required string PrimaryText { get; init; }
    public required string Explanation { get; init; }
    public required string AgentSuggestion { get; init; }
    public required string SizeText { get; init; }
    public required int Severity { get; init; }
    public CDriveRootCauseAction Action { get; init; }
    public string? ActionLabel { get; init; }
    public bool HasAction => Action != CDriveRootCauseAction.None;
    public string? ActionAutomationId { get; init; }
}

public static class CDriveRootCauseSummaryBuilder
{
    public static CDriveRootCauseSummary Build(DriveScanResult result)
    {
        var cards = new List<CDriveRootCauseCard>();

        cards.AddRange(result.TopLevel
            .OrderByDescending(item => item.SizeBytes)
            .Take(6)
            .Select(CreateTopLevelCard));

        cards.AddRange(result.BigRocks
            .Where(item => item.SizeBytes > 0)
            .OrderByDescending(item => item.SizeBytes)
            .Take(3)
            .Select(CreateBigRockCard));

        return new CDriveRootCauseSummary
        {
            Headline = $"C \u76d8\u5df2\u7528 {RootCauseReportBuilder.Fmt(result.UsedBytes)}\uff0c\u5269\u4f59 {RootCauseReportBuilder.Fmt(result.FreeBytes)}",
            Subheadline = "\u5148\u770b\u54ea\u4e9b\u7c7b\u578b\u5728\u5360\u7a7a\u95f4\uff0c\u518d\u8ba9 Agent \u751f\u6210\u53ef\u56de\u6eda\u7684\u5904\u7406\u65b9\u6848\u3002",
            TechnicalReportAvailable = true,
            Cards = cards
        };
    }

    private static CDriveRootCauseCard CreateTopLevelCard(CategoryNode node)
    {
        var title = node.IsUnexpectedRoot
            ? "\u9700\u8981\u786e\u8ba4\u6765\u6e90"
            : CategoryTitle(node.Category);
        var action = TopLevelAction(node);

        return new CDriveRootCauseCard
        {
            Title = title,
            PrimaryText = $"{node.Name} \u5360\u7528 {RootCauseReportBuilder.Fmt(node.SizeBytes)}",
            Explanation = CategoryExplanation(node),
            AgentSuggestion = CategorySuggestion(node),
            SizeText = RootCauseReportBuilder.Fmt(node.SizeBytes),
            Severity = Severity(node),
            Action = action,
            ActionLabel = ActionLabel(action),
            ActionAutomationId = BuildActionAutomationId(action, node.Name)
        };
    }

    private static CDriveRootCauseCard CreateBigRockCard(BigRock rock)
    {
        if (IsRecycleBin(rock))
        {
            return new CDriveRootCauseCard
            {
                Title = "回收站",
                PrimaryText = $"回收站占用 {RootCauseReportBuilder.Fmt(rock.SizeBytes)}",
                Explanation = "这里是你以前删除过、仍保留在回收站里的文件。清空后通常不能还原，所以先别急着清空。",
                AgentSuggestion = "Agent 建议：先打开查看，确认没有要恢复的文件；OMNIX-Entropy 不会替你清空。",
                SizeText = RootCauseReportBuilder.Fmt(rock.SizeBytes),
                Severity = 1,
                Action = CDriveRootCauseAction.OpenRecycleBin,
                ActionLabel = "打开回收站查看",
                ActionAutomationId = "CDriveRootCauseAction_OpenRecycleBin"
            };
        }

        return new CDriveRootCauseCard
        {
            Title = "\u7cfb\u7edf\u4fdd\u7559\u7a7a\u95f4",
            PrimaryText = $"{FriendlyBigRockName(rock.Name)} \u5360\u7528 {RootCauseReportBuilder.Fmt(rock.SizeBytes)}",
            Explanation = "\u8fd9\u7c7b\u7a7a\u95f4\u901a\u5e38\u7531 Windows \u7ba1\u7406\uff0c\u4e0d\u9002\u5408\u76f4\u63a5\u5220\u6587\u4ef6\u3002",
            AgentSuggestion = "\u5982\u679c\u5b83\u589e\u957f\u5f88\u5feb\uff0c\u5148\u8ba9 Agent \u89e3\u91ca\u539f\u56e0\uff0c\u518d\u8003\u8651\u7cfb\u7edf\u8bbe\u7f6e\u7ea7\u65b9\u6848\u3002",
            SizeText = RootCauseReportBuilder.Fmt(rock.SizeBytes),
            Severity = 2
        };
    }

    private static bool IsRecycleBin(BigRock rock) =>
        rock.Name.Contains("recycle", StringComparison.OrdinalIgnoreCase);

    private static CDriveRootCauseAction TopLevelAction(CategoryNode node)
    {
        if (node.IsUnexpectedRoot)
            return CDriveRootCauseAction.None;

        return node.Category switch
        {
            UsageCategory.UserProfiles => CDriveRootCauseAction.ReviewPersonalStorage,
            UsageCategory.Programs or UsageCategory.AppData => CDriveRootCauseAction.OpenCDriveApps,
            UsageCategory.Temp => CDriveRootCauseAction.ReviewCleanupRecommendations,
            _ => CDriveRootCauseAction.None
        };
    }

    private static string? ActionLabel(CDriveRootCauseAction action) =>
        action switch
        {
            CDriveRootCauseAction.OpenCDriveApps => "查看占 C 盘应用",
            CDriveRootCauseAction.ReviewPersonalStorage => "查看大文件候选",
            CDriveRootCauseAction.ReviewCleanupRecommendations => "查看可安全清理项",
            CDriveRootCauseAction.OpenRecycleBin => "打开回收站查看",
            _ => null
        };

    private static string? BuildActionAutomationId(
        CDriveRootCauseAction action,
        string sourceIdentity)
    {
        if (action == CDriveRootCauseAction.None)
            return null;

        var normalized = sourceIdentity.Trim().ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return $"CDriveRootCauseAction_{action}_{Convert.ToHexString(hash.AsSpan(0, 4))}";
    }

    private static string CategoryTitle(UsageCategory category) =>
        category switch
        {
            UsageCategory.UserProfiles => "\u7528\u6237\u6587\u4ef6",
            UsageCategory.Programs => "\u7a0b\u5e8f\u548c\u5de5\u5177",
            UsageCategory.AppData => "\u8f6f\u4ef6\u6570\u636e",
            UsageCategory.Temp => "\u4e34\u65f6\u7f13\u5b58",
            UsageCategory.System => "\u7cfb\u7edf\u6587\u4ef6",
            UsageCategory.SystemFiles => "\u7cfb\u7edf\u4fdd\u7559\u7a7a\u95f4",
            UsageCategory.PackageCaches => "\u5b89\u88c5\u5305\u7f13\u5b58",
            UsageCategory.RecycleBin => "\u56de\u6536\u7ad9",
            UsageCategory.ShadowStorage => "\u8fd8\u539f\u70b9",
            _ => "\u5176\u4ed6\u7a7a\u95f4"
        };

    private static string CategoryExplanation(CategoryNode node)
    {
        if (node.IsUnexpectedRoot)
            return "\u5b83\u4e0d\u5728 C \u76d8\u5e38\u89c1\u7cfb\u7edf\u76ee\u5f55\u91cc\uff0c\u53ef\u80fd\u662f\u9a71\u52a8\u3001\u5b89\u88c5\u6b8b\u7559\u6216\u67d0\u4e2a\u8f6f\u4ef6\u653e\u7684\u6570\u636e\u3002";

        return node.Category switch
        {
            UsageCategory.UserProfiles => "\u901a\u5e38\u5305\u542b\u684c\u9762\u3001\u4e0b\u8f7d\u3001\u6587\u6863\u3001\u804a\u5929\u8bb0\u5f55\u548c\u8f6f\u4ef6\u6570\u636e\u3002",
            UsageCategory.Programs => "\u8fd9\u91cc\u901a\u5e38\u662f\u5df2\u5b89\u88c5\u7684\u8f6f\u4ef6\u548c\u5de5\u5177\u3002",
            UsageCategory.AppData => "\u8fd9\u91cc\u591a\u662f\u8f6f\u4ef6\u7f13\u5b58\u3001\u914d\u7f6e\u548c\u672c\u5730\u6570\u636e\u3002",
            UsageCategory.Temp => "\u8fd9\u91cc\u591a\u662f\u4e34\u65f6\u6587\u4ef6\uff0c\u4f46\u4ecd\u9700\u8981\u5148\u8fdb\u9694\u79bb\u533a\u800c\u4e0d\u662f\u76f4\u63a5\u5220\u3002",
            UsageCategory.System => "\u8fd9\u662f Windows \u6838\u5fc3\u7cfb\u7edf\u533a\u57df\uff0c\u4e0d\u5efa\u8bae\u76f4\u63a5\u6e05\u7406\u3002",
            _ => "\u6682\u65f6\u53ea\u80fd\u5224\u65ad\u7c7b\u578b\uff0c\u9700\u8981\u66f4\u591a\u8bc1\u636e\u518d\u51b3\u5b9a\u3002"
        };
    }

    private static string CategorySuggestion(CategoryNode node)
    {
        if (node.IsUnexpectedRoot)
            return "Agent \u5efa\u8bae\uff1a\u5148\u786e\u8ba4\u6765\u6e90\uff0c\u4e0d\u8981\u76f4\u63a5\u5220\u3002";

        return node.Category switch
        {
            UsageCategory.Temp => "Agent \u5efa\u8bae\uff1a\u53ef\u751f\u6210\u4f4e\u98ce\u9669\u9694\u79bb\u65b9\u6848\u3002",
            UsageCategory.UserProfiles => "Agent \u5efa\u8bae\uff1a\u5148\u627e\u5927\u6587\u4ef6\u548c\u957f\u671f\u672a\u7528\u6587\u4ef6\uff0c\u4e0d\u8981\u81ea\u52a8\u5220\u7528\u6237\u6570\u636e\u3002",
            UsageCategory.Programs => "Agent \u5efa\u8bae\uff1a\u53bb\u5e94\u7528\u7ba1\u7406\u770b\u54ea\u4e9b\u7a0b\u5e8f\u53ef\u5378\u8f7d\u6216\u8fc1\u79fb\u3002",
            UsageCategory.AppData => "Agent \u5efa\u8bae\uff1a\u5148\u5f52\u5c5e\u5230\u8f6f\u4ef6\uff0c\u518d\u51b3\u5b9a\u6e05\u7f13\u5b58\u6216\u8fc1\u79fb\u3002",
            _ => "Agent \u5efa\u8bae\uff1a\u5148\u89c2\u5bdf\uff0c\u9700\u8981\u66f4\u591a\u8bc1\u636e\u3002"
        };
    }

    private static int Severity(CategoryNode node)
    {
        if (node.IsUnexpectedRoot)
            return 3;

        return node.Category switch
        {
            UsageCategory.Temp or UsageCategory.RecycleBin => 1,
            UsageCategory.UserProfiles or UsageCategory.Programs or UsageCategory.AppData => 2,
            _ => 2
        };
    }

    private static string FriendlyBigRockName(string name)
    {
        var lower = name.ToLower(CultureInfo.InvariantCulture);
        if (lower.Contains("page"))
            return "\u865a\u62df\u5185\u5b58\u6587\u4ef6";
        if (lower.Contains("hiber"))
            return "\u4f11\u7720\u6587\u4ef6";
        if (lower.Contains("shadow") || lower.Contains("restore"))
            return "\u7cfb\u7edf\u8fd8\u539f\u70b9";
        if (lower.Contains("recycle"))
            return "\u56de\u6536\u7ad9";
        return name;
    }
}
