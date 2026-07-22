using Css.Core.Operations;
using System.Collections.ObjectModel;

namespace Css.Core.Quarantine;

public static class QuarantineOperationPolicy
{
    public const string CandidateEvidenceArgument = "quarantine.candidate-evidence";

    public static OperationResult ValidateCandidate(OperationDescriptor descriptor)
    {
        if (!IsSupportedQuarantineKind(descriptor.Kind))
            return OperationResult.Fail("Only supported low-risk temporary, application cache, or uninstall residue cleanup can be moved to quarantine in V1.");

        if (!descriptor.IsDestructive)
            return OperationResult.Fail("Quarantine execution requires a destructive operation descriptor.");

        if (descriptor.Risk != RiskLevel.Low)
            return OperationResult.Fail("Only low-risk cleanup can be executed from the V1 decision card.");

        if (!descriptor.RollbackRequired)
            return OperationResult.Fail("Quarantine execution requires rollback tracking.");

        if (string.IsNullOrWhiteSpace(descriptor.EvidenceSummary))
            return OperationResult.Fail("Quarantine execution requires evidence.");

        if (descriptor.AffectedPaths.Count == 0)
            return OperationResult.Fail("Quarantine execution requires affected paths.");

        return OperationResult.Ok("Quarantine candidate accepted.");
    }

    private static bool IsSupportedQuarantineKind(string kind) =>
        kind.Equals("clean.temp", StringComparison.OrdinalIgnoreCase)
        || kind.Equals("app.cache.quarantine", StringComparison.OrdinalIgnoreCase)
        || kind.Equals("uninstall.residue.quarantine", StringComparison.OrdinalIgnoreCase);

    public static QuarantineOperationPreparationResult PrepareForConfirmation(
        OperationDescriptor descriptor,
        string quarantineRoot,
        IQuarantineCandidateIdentityReader identityReader)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(identityReader);
        var gate = ValidateCandidate(descriptor);
        if (!gate.Success)
            return QuarantineOperationPreparationResult.Refused(gate.Error ?? "隔离方案未通过策略校验。");

        if (!QuarantineCandidatePathPolicy.TryNormalizeBatch(
                descriptor.AffectedPaths,
                quarantineRoot,
                out var normalized,
                out var error))
        {
            return QuarantineOperationPreparationResult.Refused(error);
        }

        var evidence = new List<QuarantineCandidateEvidence>(normalized.Count);
        foreach (var path in normalized)
        {
            var inspection = identityReader.Inspect(path);
            if (!inspection.Success || inspection.Evidence is null)
                return QuarantineOperationPreparationResult.Refused(inspection.Summary);
            if (!inspection.Evidence.CanonicalPath.Equals(path, StringComparison.OrdinalIgnoreCase)
                || inspection.Evidence.FileId == 0
                || inspection.Evidence.CreationTimeUtcTicks <= 0)
                return QuarantineOperationPreparationResult.Refused("隔离候选身份与方案路径不一致。");
            evidence.Add(inspection.Evidence);
        }

        var arguments = new Dictionary<string, object?>(descriptor.Arguments, StringComparer.Ordinal)
        {
            [CandidateEvidenceArgument] = Array.AsReadOnly(evidence.ToArray())
        };
        return QuarantineOperationPreparationResult.Accepted(Clone(
            descriptor,
            normalized,
            new ReadOnlyDictionary<string, object?>(arguments),
            confirmationAccepted: false));
    }

    public static OperationResult ValidatePreparedCandidate(OperationDescriptor descriptor)
    {
        var gate = ValidateCandidate(descriptor);
        if (!gate.Success)
            return gate;
        if (!TryGetCandidateEvidence(descriptor, out var evidence)
            || evidence.Count != descriptor.AffectedPaths.Count)
        {
            return OperationResult.Fail("隔离确认前必须绑定最新候选身份。");
        }
        return OperationResult.Ok("Quarantine candidate identity is bound.");
    }

    public static bool TryGetCandidateEvidence(
        OperationDescriptor descriptor,
        out IReadOnlyList<QuarantineCandidateEvidence> evidence)
    {
        evidence = [];
        if (!descriptor.Arguments.TryGetValue(CandidateEvidenceArgument, out var value))
            return false;
        if (value is IReadOnlyList<QuarantineCandidateEvidence> typed)
        {
            evidence = typed;
            return true;
        }
        return false;
    }

    public static OperationDescriptor ConfirmForExecution(OperationDescriptor descriptor)
    {
        var gate = ValidatePreparedCandidate(descriptor);
        if (!gate.Success)
            throw new InvalidOperationException(gate.Error);

        return Clone(
            descriptor,
            descriptor.AffectedPaths,
            descriptor.Arguments,
            confirmationAccepted: true);
    }

    private static OperationDescriptor Clone(
        OperationDescriptor descriptor,
        IReadOnlyList<string> affectedPaths,
        IReadOnlyDictionary<string, object?> arguments,
        bool confirmationAccepted) =>
        new()
        {
            Kind = descriptor.Kind,
            Title = descriptor.Title,
            Source = descriptor.Source,
            Risk = descriptor.Risk,
            IsDestructive = descriptor.IsDestructive,
            RequiresElevation = descriptor.RequiresElevation,
            RequiresSnapshot = descriptor.RequiresSnapshot,
            SnapshotId = descriptor.SnapshotId,
            RollbackRequired = descriptor.RollbackRequired,
            ConfirmationAccepted = confirmationAccepted,
            EvidenceSummary = descriptor.EvidenceSummary,
            EstimatedImpactBytes = descriptor.EstimatedImpactBytes,
            ConfirmationText = descriptor.ConfirmationText,
            AffectedPaths = affectedPaths,
            AffectedRegistryKeys = descriptor.AffectedRegistryKeys,
            AffectedServices = descriptor.AffectedServices,
            Arguments = arguments
        };
}
