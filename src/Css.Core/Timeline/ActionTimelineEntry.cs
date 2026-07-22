using System;
using System.Collections.Generic;
using Css.Core.Operations;

namespace Css.Core.Timeline;

public enum RestoreState
{
    Unknown,
    Restorable,
    PartiallyRestorable,
    NotRestorable,
    Restored
}

public sealed class ActionTimelineEntry
{
    public long Id { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.Now;
    public OperationSource Source { get; init; } = OperationSource.Manual;
    public required string Title { get; init; }
    public required string EvidenceSummary { get; init; }
    public IReadOnlyList<string> AffectedPaths { get; init; } = [];
    public IReadOnlyList<string> AffectedRegistryKeys { get; init; } = [];
    public RestoreState RestoreState { get; init; } = RestoreState.Unknown;
    public string? RestoreOperationKind { get; init; }
    public IReadOnlyList<string> RestoreManifestPaths { get; init; } = [];
}
