using System.Collections.Generic;

namespace Css.Core.Operations;

/// <summary>
/// Where an operation originated. Manual = UI click; Agent = AI agent tool call.
/// Both flow through the same <see cref="IOperationPipeline"/>.
/// </summary>
public enum OperationSource
{
    Manual,
    Agent,
    System
}

/// <summary>
/// Risk classification drives confirmation gating and blast-radius limits.
/// Read-only scans are <see cref="None"/>; deletions are at least <see cref="Medium"/>.
/// </summary>
public enum RiskLevel
{
    None,
    Low,
    Medium,
    High
}

/// <summary>
/// Declarative description of a single operation. The pipeline reads this to decide
/// snapshot / confirm / elevate / quarantine / journal behavior. It carries no execution
/// logic — the <see cref="IOperationPipeline"/> resolves an <see cref="IOperationHandler"/>.
/// </summary>
public sealed class OperationDescriptor
{
    /// <summary>Stable id, e.g. "scan.disk", "clean.category", "uninstall.app".</summary>
    public required string Kind { get; init; }

    /// <summary>Human-readable title for UI / journal.</summary>
    public required string Title { get; init; }

    public OperationSource Source { get; init; } = OperationSource.Manual;

    public RiskLevel Risk { get; init; } = RiskLevel.None;

    /// <summary>True for any mutation (delete/move/uninstall/disable). Read-only ops skip snapshot + confirm.</summary>
    public bool IsDestructive { get; init; }

    /// <summary>True when the op needs admin privileges; routed to Css.Elevated worker.</summary>
    public bool RequiresElevation { get; init; }

    /// <summary>True when the op must have a system/app snapshot before execution.</summary>
    public bool RequiresSnapshot { get; init; }

    /// <summary>Snapshot id proving a required snapshot was created before execution.</summary>
    public string? SnapshotId { get; init; }

    /// <summary>True when this operation must be represented in a rollback plan before execution.</summary>
    public bool RollbackRequired { get; init; }

    /// <summary>Set only after the user has reviewed the decision card and confirmed execution.</summary>
    public bool ConfirmationAccepted { get; init; }

    /// <summary>Evidence shown to the user before execution.</summary>
    public string? EvidenceSummary { get; init; }

    /// <summary>Estimated bytes changed, freed, moved, or otherwise affected.</summary>
    public long EstimatedImpactBytes { get; init; }

    /// <summary>Exact confirmation text shown before running a risky operation.</summary>
    public string? ConfirmationText { get; init; }

    /// <summary>Paths the op will touch, for snapshot/quarantine manifests.</summary>
    public IReadOnlyList<string> AffectedPaths { get; init; } = [];

    /// <summary>Registry keys the op will touch, for audit and rollback manifests.</summary>
    public IReadOnlyList<string> AffectedRegistryKeys { get; init; } = [];

    /// <summary>Windows services the op will touch, for audit and elevation decisions.</summary>
    public IReadOnlyList<string> AffectedServices { get; init; } = [];

    /// <summary>Free-form arguments consumed by the matching handler.</summary>
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
}

/// <summary>Outcome of running an operation through the pipeline.</summary>
public sealed class OperationResult
{
    public bool Success { get; init; }
    public string? Summary { get; init; }
    public object? Payload { get; init; }
    public string? Error { get; init; }

    public static OperationResult Ok(string? summary = null, object? payload = null) =>
        new() { Success = true, Summary = summary, Payload = payload };

    public static OperationResult Fail(string error) =>
        new() { Success = false, Error = error };
}
