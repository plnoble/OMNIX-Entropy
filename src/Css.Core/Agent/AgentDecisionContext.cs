namespace Css.Core.Agent;

public sealed class AgentDecisionPrompt
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Question { get; init; }
}

public static class AgentDecisionPromptCatalog
{
    private static readonly IReadOnlyList<AgentDecisionPrompt> DefaultPrompts =
    [
        new()
        {
            Id = "c-drive-full",
            Label = "C 盘为什么还满",
            Question = "C盘为什么还是这么满？"
        },
        new()
        {
            Id = "fastest-growth",
            Label = "最近谁长得最快",
            Question = "最近一周谁增长最快？"
        },
        new()
        {
            Id = "safe-release",
            Label = "安全释放 10GB",
            Question = "怎样最安全地释放 10GB？"
        }
    ];

    public static IReadOnlyList<AgentDecisionPrompt> CreateDefault() => DefaultPrompts;
}

public sealed class AgentDecisionContext
{
    public AgentDrivePlanEvidence? DrivePlan { get; init; }
    public IReadOnlyList<AgentStorageSourceEvidence> StorageSources { get; init; } = [];
    public IReadOnlyList<AgentGrowthSourceEvidence> GrowthSources { get; init; } = [];
    public int ObservedSnapshotCount { get; init; }
}

public sealed class AgentDrivePlanEvidence
{
    public required string Headline { get; init; }
    public required string Progress { get; init; }
    public required IReadOnlyList<string> Steps { get; init; }
    public required long TargetReleaseBytes { get; init; }
    public required long SafeCleanupBytes { get; init; }
    public required long RemainingGapBytes { get; init; }
}

public sealed class AgentStorageSourceEvidence
{
    public required string PrimaryText { get; init; }
    public required string Explanation { get; init; }
    public required string Suggestion { get; init; }
    public required long EvidenceBytes { get; init; }
    public bool IsLowerBound { get; init; }
}

public sealed class AgentGrowthSourceEvidence
{
    public required string OwnerLabel { get; init; }
    public required long LatestGrowthBytes { get; init; }
    public required long TrendGrowthBytes { get; init; }
    public required TimeSpan ObservationInterval { get; init; }
    public required TimeSpan TrendWindow { get; init; }
    public required int ObservedSnapshots { get; init; }
    public required bool IsFirstObservation { get; init; }
    public required bool IsSustainedGrowth { get; init; }
    public required string OneTimeAction { get; init; }
    public required string PreventionAction { get; init; }
    public string? TargetAppName { get; init; }
}
