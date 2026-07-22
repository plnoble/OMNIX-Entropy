using System.Collections.Generic;
using Css.Core.Operations;
using Css.Core.Recommendations;

namespace Css.Core.Apps;

public sealed class HealthCheckSummary
{
    public required int OverallScore { get; init; }
    public required IReadOnlyList<HealthDimensionResult> Dimensions { get; init; }
    public required IReadOnlyList<HealthFinding> KeyFindings { get; init; }
    public HardwareSummaryObservation? Hardware { get; init; }
}

public sealed class HealthDimensionResult
{
    public required string Name { get; init; }
    public required string Result { get; init; }
    public required string Rating { get; init; }
}

public sealed class HealthFinding
{
    public required string Text { get; init; }
    public HealthFindingKind Kind { get; init; }
    public string? TargetAppName { get; init; }
    public RecommendationAction Action { get; init; }
    public RiskLevel Risk { get; init; }
}

public enum HealthFindingKind
{
    General,
    SustainedGrowth,
    PersonalStorage,
    MigrationClosure
}
