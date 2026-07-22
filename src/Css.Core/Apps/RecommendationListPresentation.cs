using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Recommendations;

namespace Css.Core.Apps;

public sealed class RecommendationListViewModel
{
    public required IReadOnlyList<RecommendationCardViewModel> Cards { get; init; }
    public required string ActionExplanationText { get; init; }
}

public static class RecommendationListPresenter
{
    private const string UnexpectedRootPrefix = "\u975e\u9884\u671f\u6839\u76ee\u5f55: ";

    public static RecommendationListViewModel Create(IEnumerable<Recommendation> recommendations)
    {
        var source = recommendations.ToList();
        var unexpectedRoots = source
            .Where(IsUnexpectedRootObservation)
            .OrderByDescending(recommendation => recommendation.EstimatedImpactBytes)
            .ToList();

        var cards = new List<RecommendationCardViewModel>();
        if (unexpectedRoots.Count > 1)
        {
            cards.Add(CreateUnexpectedRootGroup(unexpectedRoots));
        }

        foreach (var recommendation in source)
        {
            if (unexpectedRoots.Count > 1 && IsUnexpectedRootObservation(recommendation))
                continue;

            cards.Add(RecommendationCardPresenter.Create(recommendation));
        }

        return new RecommendationListViewModel
        {
            Cards = cards,
            ActionExplanationText = "\u9694\u79bb\u533a\u4e0d\u662f\u6c38\u4e45\u5220\u9664\uff1a\u5b83\u4f1a\u5148\u628a\u4f4e\u98ce\u9669\u6e05\u7406\u9879\u653e\u5230 OMNIX-Entropy \u7684\u540e\u6094\u836f\u6682\u5b58\u533a\uff0c\u786e\u8ba4\u7535\u8111\u6b63\u5e38\u540e\u518d\u7531\u4f60\u51b3\u5b9a\u662f\u5426\u6e05\u6389\u3002"
        };
    }

    private static bool IsUnexpectedRootObservation(Recommendation recommendation) =>
        recommendation.Action == RecommendationAction.Observe &&
        recommendation.Title.StartsWith(UnexpectedRootPrefix, StringComparison.OrdinalIgnoreCase);

    private static RecommendationCardViewModel CreateUnexpectedRootGroup(IReadOnlyList<Recommendation> recommendations)
    {
        var totalBytes = recommendations.Sum(recommendation => recommendation.EstimatedImpactBytes);
        var examples = string.Join("\u3001", recommendations
            .Select(recommendation => recommendation.Title[UnexpectedRootPrefix.Length..])
            .Select(name => BeginnerTextSanitizer.HideLocalPaths(name))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Take(3));

        return new RecommendationCardViewModel
        {
            Title = "\u9700\u8981\u786e\u8ba4\u6765\u6e90\uff1a" + recommendations.Count + " \u4e2a C \u76d8\u6839\u76ee\u5f55",
            WhatHappened = "\u53d1\u751f\u4e86\u4ec0\u4e48\uff1a\u53d1\u73b0 " + recommendations.Count + " \u4e2a\u4e0d\u5e38\u89c1\u7684 C \u76d8\u6839\u76ee\u5f55\uff0c\u5360\u7528\u7ea6 " + FormatBytes(totalBytes) + "\u3002\u793a\u4f8b\uff1a" + examples + "\u3002",
            AgentSuggestion = "Agent \u5efa\u8bae\uff1a\u4e0d\u7528\u9010\u4e2a\u70b9\u5361\u7247\uff1b\u5148\u786e\u8ba4\u5b83\u4eec\u5c5e\u4e8e\u9a71\u52a8\u3001\u7f13\u5b58\u3001\u65e7\u5b89\u88c5\u6b8b\u7559\u8fd8\u662f\u67d0\u4e2a\u8f6f\u4ef6\u7684\u5de5\u4f5c\u76ee\u5f55\uff0c\u518d\u51b3\u5b9a\u4fdd\u7559\u3001\u8fc1\u79fb\u6216\u6e05\u7406\u3002",
            UndoStatus = "\u80fd\u4e0d\u80fd\u540e\u6094\uff1a\u73b0\u5728\u53ea\u662f\u89c2\u5bdf\u5efa\u8bae\uff0c\u4e0d\u4f1a\u52a8\u4efb\u4f55\u6587\u4ef6\u3002",
            ImpactText = "\u9884\u8ba1\u5f71\u54cd\uff1a" + FormatBytes(totalBytes),
            SafetyLine = "\u5b89\u5168\u8fb9\u754c\uff1a\u8fd9\u7ec4\u53ea\u80fd\u67e5\u770b\u548c\u751f\u6210\u65b9\u6848\uff0c\u4e0d\u80fd\u76f4\u63a5\u6267\u884c\uff1b\u98ce\u9669\uff1a\u4e2d\u3002",
            CanExecute = false,
            Operation = null
        };
    }

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
