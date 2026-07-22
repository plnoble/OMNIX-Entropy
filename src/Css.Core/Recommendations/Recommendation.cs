using System.Collections.Generic;
using Css.Core.Operations;

namespace Css.Core.Recommendations;

public enum RecommendationAction
{
    Observe,
    Clean,
    Migrate,
    Keep,
    DisableStartup,
    Uninstall,
    RepairInstallLocation
}

public enum ReversibilityLevel
{
    Unknown,
    Reversible,
    PartiallyReversible,
    NotReversible
}

/// <summary>
/// A user-facing decision card. AI and scanners may create recommendations, but
/// only an attached operation passed through the pipeline may mutate the machine.
/// </summary>
public sealed class Recommendation
{
    public required string Title { get; init; }
    public required string Finding { get; init; }
    public required string Reason { get; init; }
    public RecommendationAction Action { get; init; } = RecommendationAction.Observe;
    public RiskLevel Risk { get; init; } = RiskLevel.None;
    public ReversibilityLevel Reversibility { get; init; } = ReversibilityLevel.Unknown;
    public long EstimatedImpactBytes { get; init; }
    public IReadOnlyList<string> Evidence { get; init; } = [];
    public OperationDescriptor? Operation { get; init; }

    public bool HasDecisionCardMinimums =>
        !string.IsNullOrWhiteSpace(Title)
        && !string.IsNullOrWhiteSpace(Finding)
        && !string.IsNullOrWhiteSpace(Reason)
        && Evidence.Count > 0
        && EstimatedImpactBytes >= 0
        && Reversibility is not ReversibilityLevel.Unknown;
}
