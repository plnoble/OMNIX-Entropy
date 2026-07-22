using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Css.Core.Operations;
using Css.Core.Recommendations;

namespace Css.Scanner.Disk;

/// <summary>
/// Converts scan findings into decision cards. This layer never executes cleanup;
/// it only prepares auditable recommendations for UI/AI to show the user.
/// </summary>
public static class DiskRecommendationBuilder
{
    public static IReadOnlyList<Recommendation> Build(DriveScanResult result)
    {
        var cards = new List<Recommendation>();

        foreach (var root in result.UnexpectedRoots.OrderByDescending(n => n.SizeBytes))
        {
            cards.Add(new Recommendation
            {
                Title = "非预期根目录: " + root.Name,
                Finding = $"{root.Path ?? root.Name} 占用 {RootCauseReportBuilder.Fmt(root.SizeBytes)}",
                Reason = "它不在 C: 根目录白名单内，应该先确认来源再决定清理或迁移。",
                Action = RecommendationAction.Observe,
                Risk = RiskLevel.Medium,
                Reversibility = ReversibilityLevel.PartiallyReversible,
                EstimatedImpactBytes = root.SizeBytes,
                Evidence =
                [
                    "根目录白名单未包含: " + root.Name,
                    "分类结果: " + root.Category.ToString()
                ]
            });
        }

        foreach (var temp in result.TopLevel.Where(n => n.Category == UsageCategory.Temp && n.SizeBytes > 0))
        {
            var path = temp.Path ?? temp.Name;
            var evidence = $"临时目录 {path} 占用 {RootCauseReportBuilder.Fmt(temp.SizeBytes)}";
            cards.Add(new Recommendation
            {
                Title = "可清理临时目录: " + temp.Name,
                Finding = evidence,
                Reason = "临时目录通常可重建；V1 只建议移动到隔离区，确认可用后再过期删除。",
                Action = RecommendationAction.Clean,
                Risk = RiskLevel.Low,
                Reversibility = ReversibilityLevel.Reversible,
                EstimatedImpactBytes = temp.SizeBytes,
                Evidence =
                [
                    "分类结果: Temp",
                    "路径: " + path
                ],
                Operation = new OperationDescriptor
                {
                    Kind = "clean.temp",
                    Title = "清理临时目录: " + temp.Name,
                    Risk = RiskLevel.Low,
                    IsDestructive = true,
                    RollbackRequired = true,
                    EvidenceSummary = evidence,
                    EstimatedImpactBytes = temp.SizeBytes,
                    ConfirmationText = string.Format(CultureInfo.InvariantCulture, "确认将 {0} 移动到隔离区", path),
                    AffectedPaths = [path]
                }
            });
        }

        return cards;
    }
}
