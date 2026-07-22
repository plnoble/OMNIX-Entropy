using Css.Core.Operations;
using Css.Core.Recommendations;

namespace Css.Core.Agent;

public sealed class AgentRecommendation
{
    public required string Text { get; init; }
    public required string Reason { get; init; }
    public RecommendationAction Action { get; init; } = RecommendationAction.Observe;
    public RiskLevel Risk { get; init; } = RiskLevel.None;
    public bool RequiresUserConfirmation { get; init; }
}
