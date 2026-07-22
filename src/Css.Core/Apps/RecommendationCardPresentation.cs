using System;
using System.Collections.Generic;
using Css.Core.Operations;
using Css.Core.Recommendations;

namespace Css.Core.Apps;

public sealed class RecommendationCardViewModel
{
    public required string Title { get; init; }
    public required string WhatHappened { get; init; }
    public required string AgentSuggestion { get; init; }
    public required string UndoStatus { get; init; }
    public required string ImpactText { get; init; }
    public required string SafetyLine { get; init; }
    public required bool CanExecute { get; init; }
    public OperationDescriptor? Operation { get; init; }
}

public static class RecommendationCardPresenter
{
    public static RecommendationCardViewModel Create(Recommendation recommendation)
    {
        var canExecute = recommendation.Operation is not null &&
            recommendation.Risk is RiskLevel.None or RiskLevel.Low;
        IReadOnlyList<string> affectedPaths = recommendation.Operation?.AffectedPaths ?? [];

        return new RecommendationCardViewModel
        {
            Title = BeginnerTextSanitizer.HideLocalPaths(HumanTitle(recommendation), affectedPaths),
            WhatHappened = "\u53d1\u751f\u4e86\u4ec0\u4e48\uff1a" +
                BeginnerTextSanitizer.HideLocalPaths(recommendation.Finding, affectedPaths),
            AgentSuggestion = "Agent \u5efa\u8bae\uff1a" +
                BeginnerTextSanitizer.HideLocalPaths(AgentSuggestion(recommendation), affectedPaths),
            UndoStatus = "\u80fd\u4e0d\u80fd\u540e\u6094\uff1a" + UndoStatus(recommendation.Reversibility),
            ImpactText = ImpactText(recommendation),
            SafetyLine = SafetyLine(recommendation, canExecute),
            CanExecute = canExecute,
            Operation = recommendation.Operation
        };
    }

    private static string HumanTitle(Recommendation recommendation)
    {
        var title = recommendation.Title;
        const string unexpectedPrefix = "\u975e\u9884\u671f\u6839\u76ee\u5f55: ";
        const string tempPrefix = "\u53ef\u6e05\u7406\u4e34\u65f6\u76ee\u5f55: ";

        if (title.StartsWith(unexpectedPrefix, StringComparison.OrdinalIgnoreCase))
            return "\u5148\u522b\u5220\uff0c\u5148\u786e\u8ba4\u6765\u6e90\uff1a" + title[unexpectedPrefix.Length..];

        if (title.StartsWith(tempPrefix, StringComparison.OrdinalIgnoreCase))
            return "\u53ef\u4ee5\u6e05\u7406\uff1a" + title[tempPrefix.Length..];

        return title;
    }

    private static string AgentSuggestion(Recommendation recommendation) =>
        recommendation.Action switch
        {
            RecommendationAction.Clean when recommendation.Risk is RiskLevel.None or RiskLevel.Low =>
                "\u5148\u79fb\u5230\u9694\u79bb\u533a\uff0c\u4e0d\u4f1a\u76f4\u63a5\u6c38\u4e45\u5220\u9664\uff1b\u7528\u4e00\u6bb5\u65f6\u95f4\u6ca1\u95ee\u9898\u540e\u518d\u6e05\u6389\u9694\u79bb\u533a\u526f\u672c\u3002",
            RecommendationAction.Clean =>
                "\u53ef\u80fd\u80fd\u6e05\u7406\uff0c\u4f46\u98ce\u9669\u504f\u9ad8\uff0c\u9700\u8981\u5148\u6709\u5feb\u7167\u548c\u56de\u6eda\u65b9\u6848\u3002",
            RecommendationAction.Observe =>
                "\u5148\u522b\u5220\uff0c\u5148\u786e\u8ba4\u5b83\u662f\u54ea\u4e2a\u8f6f\u4ef6\u6216\u7cfb\u7edf\u884c\u4e3a\u4ea7\u751f\u7684\u3002",
            RecommendationAction.Migrate =>
                "\u53ef\u4ee5\u5148\u751f\u6210\u8fc1\u79fb\u65b9\u6848\uff0c\u68c0\u67e5\u5feb\u7167\u3001\u56de\u6eda\u548c\u539f\u4f4d\u7f6e\u662f\u5426\u7ee7\u7eed\u5199\u5165\u3002",
            RecommendationAction.DisableStartup =>
                "\u53ef\u4ee5\u5148\u770b\u5b83\u662f\u5426\u5f71\u54cd\u5f00\u673a\uff0c\u771f\u8981\u5173\u95ed\u4e5f\u5fc5\u987b\u5148\u8bb0\u5f55\u56de\u6eda\u65b9\u5f0f\u3002",
            RecommendationAction.Uninstall =>
                "\u5148\u7528\u5b98\u65b9\u5378\u8f7d\u5668\uff0c\u5378\u8f7d\u540e\u518d\u626b\u63cf\u6b8b\u7559\uff1b\u4e0d\u76f4\u63a5\u5220\u6ce8\u518c\u8868\u6216\u670d\u52a1\u3002",
            RecommendationAction.RepairInstallLocation =>
                "\u5148\u751f\u6210\u5b89\u88c5\u4f4d\u7f6e\u4fee\u590d\u65b9\u6848\uff0c\u4e0d\u5168\u5c40\u6539 Windows \u5b89\u88c5\u76ee\u5f55\u3002",
            RecommendationAction.Keep =>
                "\u5efa\u8bae\u4fdd\u7559\uff0c\u76ee\u524d\u770b\u4e0d\u503c\u5f97\u52a8\u3002",
            _ => recommendation.Reason
        };

    private static string UndoStatus(ReversibilityLevel reversibility) =>
        reversibility switch
        {
            ReversibilityLevel.Reversible => "\u53ef\u4ee5\u4ece\u540e\u6094\u836f\u4e2d\u5fc3\u8fd8\u539f\u3002",
            ReversibilityLevel.PartiallyReversible => "\u53ea\u6709\u4e00\u90e8\u5206\u80fd\u8fd8\u539f\uff0c\u5148\u89c2\u5bdf\u66f4\u7a33\u3002",
            ReversibilityLevel.NotReversible => "\u4e0d\u80fd\u5b89\u5168\u8fd8\u539f\uff0cV1 \u4e0d\u5efa\u8bae\u76f4\u63a5\u6267\u884c\u3002",
            _ => "\u9700\u8981\u66f4\u591a\u8bc1\u636e\u624d\u80fd\u5224\u65ad\u3002"
        };

    private static string ImpactText(Recommendation recommendation)
    {
        var label = recommendation.Action == RecommendationAction.Clean
            ? "\u9884\u8ba1\u91ca\u653e\uff1a"
            : "\u9884\u8ba1\u5f71\u54cd\uff1a";
        return label + FormatBytes(recommendation.EstimatedImpactBytes);
    }

    private static string SafetyLine(Recommendation recommendation, bool canExecute)
    {
        var executeText = canExecute
            ? "\u9700\u8981\u4f60\u786e\u8ba4\u540e\u624d\u4f1a\u8fdb\u5165\u5b89\u5168\u7ba1\u7ebf"
            : "\u53ea\u751f\u6210\u5efa\u8bae\uff0c\u5f53\u524d\u4e0d\u5f00\u653e\u6267\u884c";

        var noPermanentDelete = recommendation.Action == RecommendationAction.Clean
            ? "\uff1b\u4e0d\u4f1a\u76f4\u63a5\u6c38\u4e45\u5220\u9664"
            : string.Empty;

        return "\u5b89\u5168\u8fb9\u754c\uff1a" + executeText + noPermanentDelete + "\uff1b\u98ce\u9669\uff1a" + RiskLabel(recommendation.Risk) + "\u3002";
    }

    private static string RiskLabel(RiskLevel risk) =>
        risk switch
        {
            RiskLevel.None => "\u65e0",
            RiskLevel.Low => "\u4f4e",
            RiskLevel.Medium => "\u4e2d",
            RiskLevel.High => "\u9ad8",
            _ => risk.ToString()
        };

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
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
