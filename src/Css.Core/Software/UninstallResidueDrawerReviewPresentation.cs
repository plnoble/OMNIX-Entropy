using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Software;

public sealed class UninstallResidueDrawerReviewViewModel
{
    public required string SectionTitle { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public required string PrimaryButtonText { get; init; }
    public string PrimaryActionText { get; init; } = "";
    public string PrimaryActionKey { get; init; } = "";
    public bool CanMoveLowRiskToQuarantine { get; init; }
    public OperationDescriptor? LowRiskOperation { get; init; }
    public string VisibleText => string.Join("\n", Lines);
}

public static class UninstallResidueDrawerReviewPresenter
{
    public static UninstallResidueDrawerReviewViewModel Create(UninstallResidueReviewViewModel review)
    {
        var lines = review.CanMoveLowRiskToQuarantine && review.LowRiskOperation is not null
            ? BuildActionableLines(review)
            : BuildBlockedLines(review);

        return new UninstallResidueDrawerReviewViewModel
        {
            SectionTitle = "\u6b8b\u7559\u68c0\u67e5\u7ed3\u679c",
            Lines = lines,
            PrimaryButtonText = review.PrimaryButtonText,
            CanMoveLowRiskToQuarantine = review.CanMoveLowRiskToQuarantine,
            LowRiskOperation = review.LowRiskOperation
        };
    }

    public static UninstallResidueDrawerReviewViewModel CreateCanceled(UninstallResidueReviewViewModel review) =>
        new()
        {
            SectionTitle = "\u6b8b\u7559\u5904\u7406\u7ed3\u679c",
            Lines =
            [
                "\u7ed3\u679c\uff1a\u5df2\u53d6\u6d88\uff0c\u672c\u6b21\u6ca1\u6709\u79fb\u52a8\u4efb\u4f55\u6587\u4ef6\u3002",
                "\u540e\u6094\u836f\u4e2d\u5fc3\u6ca1\u6709\u65b0\u589e\u8bb0\u5f55\uff0c\u56e0\u4e3a\u6ca1\u6709\u6267\u884c\u6b8b\u7559\u5904\u7406\u3002",
                "\u4e0b\u4e00\u6b65\uff1a\u4f60\u53ef\u4ee5\u7ee7\u7eed\u89c2\u5bdf\uff0c\u6216\u8005\u518d\u6b21\u590d\u67e5\u6b8b\u7559\u3002",
                HideLocalPaths(review.SafetyText)
            ],
            PrimaryButtonText = "\u518d\u6b21\u590d\u67e5\u6b8b\u7559",
            PrimaryActionText = "",
            PrimaryActionKey = "",
            CanMoveLowRiskToQuarantine = false,
            LowRiskOperation = null
        };

    public static UninstallResidueDrawerReviewViewModel CreateQuarantined(
        UninstallResidueReviewViewModel review,
        string? operationSummary) =>
        new()
        {
            SectionTitle = "\u6b8b\u7559\u5904\u7406\u7ed3\u679c",
            Lines =
            [
                "\u7ed3\u679c\uff1a" + HideLocalPaths(string.IsNullOrWhiteSpace(operationSummary)
                    ? "\u4f4e\u98ce\u9669\u6b8b\u7559\u5df2\u79fb\u52a8\u5230\u9694\u79bb\u533a\u3002"
                    : operationSummary),
                "\u540e\u6094\u836f\u4e2d\u5fc3\u5df2\u8bb0\u5f55\u8fd9\u6b21\u5904\u7406\uff0c\u540e\u7eed\u53ef\u4ee5\u8fd8\u539f\u3002",
                "\u6280\u672f\u8def\u5f84\u4fdd\u7559\u5728\u64cd\u4f5c\u8bb0\u5f55\u548c\u786e\u8ba4\u660e\u7ec6\u91cc\uff0c\u9996\u5c4f\u4e0d\u76f4\u63a5\u5806\u8def\u5f84\u3002",
                HideLocalPaths(review.SafetyText)
            ],
            PrimaryButtonText = "\u67e5\u770b\u540e\u6094\u836f\u4e2d\u5fc3",
            PrimaryActionText = "\u67e5\u770b\u540e\u6094\u836f\u4e2d\u5fc3",
            PrimaryActionKey = "Timeline",
            CanMoveLowRiskToQuarantine = false,
            LowRiskOperation = null
        };

    private static IReadOnlyList<string> BuildBlockedLines(UninstallResidueReviewViewModel review)
    {
        var lines = new List<string>
        {
            "\u7ed3\u8bba\uff1a\u8fd8\u6ca1\u6709\u786e\u8ba4\u5378\u8f7d\u5b8c\u6210\uff0c\u73b0\u5728\u4e0d\u628a\u5b83\u5f53\u4f5c\u6b8b\u7559\u6e05\u7406\u3002",
            "\u4e0b\u4e00\u6b65\uff1a\u5148\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\uff0c\u5378\u8f7d\u540e\u518d\u70b9\u8fd9\u91cc\u590d\u67e5\u3002",
            "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u79fb\u52a8\u4efb\u4f55\u6587\u4ef6\uff0c\u4e0d\u4f1a\u5220\u670d\u52a1\u3001\u81ea\u542f\u52a8\u6216\u6ce8\u518c\u8868\u3002"
        };

        if (!string.IsNullOrWhiteSpace(review.Summary))
            lines.Add("\u8bc1\u636e\uff1a" + HideLocalPaths(review.Summary));

        return lines;
    }

    private static IReadOnlyList<string> BuildActionableLines(UninstallResidueReviewViewModel review)
    {
        var lines = new List<string>
        {
            HideLocalPaths(review.Summary),
            HideLocalPaths(review.SafetyText)
        };

        foreach (var group in review.Groups)
        {
            lines.Add(group.Title + " / " + group.RiskLabel + "\u98ce\u9669 / " + group.ActionLine);
            lines.AddRange(group.Items.Take(3).Select(item => "  - " + HideLocalPaths(item)));
            if (group.Items.Count > 3)
                lines.Add("  - \u8fd8\u6709 " + (group.Items.Count - 3) + " \u9879\uff0c\u53ef\u5728\u786e\u8ba4\u9875\u67e5\u770b\u3002");
        }

        return lines;
    }

    private static string HideLocalPaths(string text)
    {
        var sanitized = text;
        var driveIndex = sanitized.IndexOf(@":\", System.StringComparison.Ordinal);
        while (driveIndex > 0 && driveIndex - 1 < sanitized.Length && char.IsLetter(sanitized[driveIndex - 1]))
        {
            var start = driveIndex - 1;
            var end = sanitized.IndexOf(' ', driveIndex + 2);
            if (end < 0)
                end = sanitized.Length;

            sanitized = sanitized[..start] + "\u672c\u5730\u8def\u5f84" + sanitized[end..];
            driveIndex = sanitized.IndexOf(@":\", System.StringComparison.Ordinal);
        }

        return sanitized;
    }
}
