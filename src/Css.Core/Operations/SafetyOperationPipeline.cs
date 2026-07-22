using System;
using System.Threading;
using System.Threading.Tasks;

namespace Css.Core.Operations;

/// <summary>
/// Minimal V1 implementation of the operation chokepoint. It enforces the safety
/// contract before delegating to an executor supplied by UI, agent, or tests.
/// </summary>
public sealed class SafetyOperationPipeline : IOperationPipeline
{
    private readonly Func<OperationDescriptor, CancellationToken, Task<OperationResult>> _executor;

    public SafetyOperationPipeline(Func<OperationDescriptor, CancellationToken, Task<OperationResult>> executor)
    {
        _executor = executor;
    }

    public static SafetyOperationPipeline DryRun(Func<OperationDescriptor, OperationResult> executor) =>
        new((descriptor, _) => Task.FromResult(executor(descriptor)));

    public Task<OperationResult> ExecuteAsync(OperationDescriptor descriptor, CancellationToken ct = default)
    {
        var gate = Validate(descriptor);
        if (!gate.Success) return Task.FromResult(gate);
        return _executor(descriptor, ct);
    }

    private static OperationResult Validate(OperationDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Kind))
            return OperationResult.Fail("Operation kind is required.");

        if (descriptor.IsDestructive && !descriptor.ConfirmationAccepted)
            return OperationResult.Fail("Destructive operations require explicit user confirmation.");

        if (descriptor.RequiresSnapshot && string.IsNullOrWhiteSpace(descriptor.SnapshotId))
            return OperationResult.Fail("A snapshot is required before this operation can run.");

        if (descriptor.IsDestructive && string.IsNullOrWhiteSpace(descriptor.EvidenceSummary))
            return OperationResult.Fail("Destructive operations require evidence for the decision card.");

        if (descriptor.EstimatedImpactBytes < 0)
            return OperationResult.Fail("Estimated impact cannot be negative.");

        return OperationResult.Ok("Safety gate passed.");
    }
}
