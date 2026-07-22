using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Apps;

public sealed class CleanupConfirmationViewModel
{
    public required string Title { get; init; }
    public required string BeginnerText { get; init; }
    public required IReadOnlyList<string> OutcomePreviewLines { get; init; }
    public required IReadOnlyList<string> TechnicalDetails { get; init; }
    public required string MessageText { get; init; }
}

public static class CleanupConfirmationPresenter
{
    public static CleanupConfirmationViewModel Create(OperationDescriptor operation, string quarantineRoot)
    {
        var affectedCount = operation.AffectedPaths.Count;
        var impact = FormatBytes(operation.EstimatedImpactBytes);
        var beginnerLines = new[]
        {
            "Agent \u5224\u65ad\uff1a\u8fd9\u662f\u4f4e\u98ce\u9669\u6e05\u7406\u9879\uff0c\u786e\u8ba4\u540e\u5148\u79fb\u5230\u9694\u79bb\u533a\u3002",
            "\u4f1a\u53d1\u751f\u4ec0\u4e48\uff1a\u5904\u7406 " + affectedCount + " \u4e2a\u4f4d\u7f6e\uff0c\u9884\u8ba1\u91ca\u653e " + impact + "\u3002",
            "\u80fd\u4e0d\u80fd\u540e\u6094\uff1a\u8fd9\u4e0d\u662f\u6c38\u4e45\u5220\u9664\uff1b\u540e\u6094\u836f\u4e2d\u5fc3\u4f1a\u4fdd\u7559\u8fd8\u539f\u8bb0\u5f55\u3002",
            "\u5b89\u5168\u8fb9\u754c\uff1a\u53ea\u6709\u4f60\u70b9\u51fb\u786e\u8ba4\u540e\uff0c\u624d\u4f1a\u8fdb\u5165\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002"
        };
        var outcomePreviewLines = new[]
        {
            "\u7b2c\u4e00\u6b65\uff1a\u53ea\u628a\u5019\u9009\u9879\u79fb\u5230 OMNIX-Entropy \u9694\u79bb\u533a\uff0c\u4e0d\u662f\u6c38\u4e45\u5220\u9664\u3002",
            "\u7b2c\u4e8c\u6b65\uff1a\u5199\u5165\u540e\u6094\u836f\u4e2d\u5fc3\u65f6\u95f4\u7ebf\uff0c\u540e\u7eed\u53ef\u4ee5\u770b\u5230\u5904\u7406\u8bb0\u5f55\u548c\u8fd8\u539f\u5165\u53e3\u3002",
            "\u7b2c\u4e09\u6b65\uff1a\u5982\u679c\u9694\u79bb\u6216\u8bb0\u5f55\u5931\u8d25\uff0c\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u4f1a\u62d2\u7edd\u7ee7\u7eed\u5904\u7406\u3002",
            "\u4e0d\u4f1a\u4fee\u6539\u6ce8\u518c\u8868\u3001\u670d\u52a1\u3001\u81ea\u542f\u52a8\u3001\u8ba1\u5212\u4efb\u52a1\uff0c\u4e5f\u4e0d\u4f1a\u8fd0\u884c\u5b89\u88c5\u5668\u6216\u4e91\u7aef AI\u3002"
        };

        var technicalDetails = BuildTechnicalDetails(operation, quarantineRoot);
        var message = string.Join(System.Environment.NewLine, beginnerLines)
            + System.Environment.NewLine
            + System.Environment.NewLine
            + "\u786e\u8ba4\u540e\u4f1a\u53d1\u751f\u4ec0\u4e48:"
            + System.Environment.NewLine
            + string.Join(System.Environment.NewLine, outcomePreviewLines)
            + System.Environment.NewLine
            + System.Environment.NewLine
            + "\u6280\u672f\u8be6\u60c5\uff08\u9700\u8981\u6838\u5bf9\u65f6\u770b\uff09:"
            + System.Environment.NewLine
            + string.Join(System.Environment.NewLine, technicalDetails);

        return new CleanupConfirmationViewModel
        {
            Title = "\u786e\u8ba4\u79fb\u52a8\u5230\u9694\u79bb\u533a",
            BeginnerText = string.Join(System.Environment.NewLine, beginnerLines),
            OutcomePreviewLines = outcomePreviewLines,
            TechnicalDetails = technicalDetails,
            MessageText = message
        };
    }

    private static IReadOnlyList<string> BuildTechnicalDetails(OperationDescriptor operation, string quarantineRoot)
    {
        var lines = new List<string>
        {
            "Operation: " + operation.Kind,
            "Evidence: " + (string.IsNullOrWhiteSpace(operation.EvidenceSummary) ? "\u672a\u8bb0\u5f55" : operation.EvidenceSummary),
            "Estimated impact: " + FormatBytes(operation.EstimatedImpactBytes),
            "Quarantine root: " + quarantineRoot
        };

        if (!string.IsNullOrWhiteSpace(operation.ConfirmationText))
            lines.Add("Original confirmation: " + operation.ConfirmationText);

        lines.Add("Affected paths:");
        lines.AddRange(operation.AffectedPaths.Count == 0
            ? ["  - \u672a\u8bb0\u5f55"]
            : operation.AffectedPaths.Select(path => "  - " + path));

        return lines;
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
