using Css.Core.Operations;
using Css.Core.Recommendations;

namespace Css.Core.Apps;

public static class HealthFindingRiskPolicy
{
    public static bool IsLowRiskClean(RecommendationAction action, RiskLevel risk) =>
        action == RecommendationAction.Clean
        && risk is RiskLevel.None or RiskLevel.Low;

    public static bool IsHigherRiskClean(RecommendationAction action, RiskLevel risk) =>
        action == RecommendationAction.Clean
        && risk is RiskLevel.Medium or RiskLevel.High;
}
