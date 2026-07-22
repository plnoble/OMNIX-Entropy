using System.Collections.ObjectModel;
using Css.Core.Operations;
using Css.Core.Timeline;

namespace Css.Core.Startup;

public sealed record StartupRestoreEvidence
{
    public required string ManifestPath { get; init; }
    public required string ManifestSha256 { get; init; }
    public required string SnapshotId { get; init; }
    public required string StateFingerprint { get; init; }
    public required string SourceLocator { get; init; }
}

public sealed class StartupRestorePreparationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public ActionTimelineEntry? CurrentEntry { get; init; }

    public static StartupRestorePreparationResult Accepted(
        OperationDescriptor operation,
        ActionTimelineEntry currentEntry) =>
        new()
        {
            Success = true,
            Operation = operation,
            CurrentEntry = currentEntry
        };

    public static StartupRestorePreparationResult Refused(string error) =>
        new() { Success = false, Error = error };
}

public sealed class StartupRestoreEvidenceVerification
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public VerifiedStartupRollbackManifest? VerifiedManifest { get; init; }

    public static StartupRestoreEvidenceVerification Accepted(
        VerifiedStartupRollbackManifest verifiedManifest) =>
        new() { Success = true, VerifiedManifest = verifiedManifest };

    public static StartupRestoreEvidenceVerification Refused(string error) =>
        new() { Success = false, Error = error };
}

public sealed class StartupRestoreOperationOutcome
{
    public required RestoreState RestoreState { get; init; }
    public required bool TimelineUpdated { get; init; }
    public required bool MutationAttempted { get; init; }
    public required bool MutationSucceeded { get; init; }
}

public static class StartupRestoreOperationPolicy
{
    public const string TimelineEntryIdArgument = "startup.restore.timeline-entry-id";
    public const string RestoreEvidenceArgument = "startup.restore.evidence";

    public static async Task<StartupRestorePreparationResult> PrepareForConfirmationAsync(
        long timelineEntryId,
        ActionTimelineStore timeline,
        StartupRollbackManifestStore manifests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(manifests);
        if (timelineEntryId <= 0)
            return StartupRestorePreparationResult.Refused("The startup timeline entry id is invalid.");

        var entry = await timeline.LoadByIdAsync(timelineEntryId, cancellationToken);
        if (entry is null)
            return StartupRestorePreparationResult.Refused("The startup timeline entry no longer exists.");
        if (entry.RestoreState != RestoreState.Restorable
            || entry.RestoreOperationKind?.Equals(
                StartupEntryControlOperationPolicy.RestoreKind,
                StringComparison.OrdinalIgnoreCase) != true)
        {
            return StartupRestorePreparationResult.Refused("The startup timeline entry is not currently restorable.");
        }
        if (entry.RestoreManifestPaths.Count != 1
            || entry.AffectedPaths.Count != 0
            || entry.AffectedRegistryKeys.Count != 1
            || !StartupEntryControlPolicy.IsSupportedLocator(entry.AffectedRegistryKeys[0]))
        {
            return StartupRestorePreparationResult.Refused("The startup timeline evidence is incomplete or outside the supported scope.");
        }

        VerifiedStartupRollbackManifest verified;
        try
        {
            verified = await manifests.LoadVerifiedAsync(
                entry.RestoreManifestPaths[0],
                cancellationToken);
        }
        catch
        {
            return StartupRestorePreparationResult.Refused("The startup rollback manifest could not be verified.");
        }

        if (!string.Equals(
                verified.Manifest.State.SourceLocator,
                entry.AffectedRegistryKeys[0],
                StringComparison.OrdinalIgnoreCase))
        {
            return StartupRestorePreparationResult.Refused("The startup rollback manifest does not match the timeline scope.");
        }

        var evidence = new StartupRestoreEvidence
        {
            ManifestPath = verified.ManifestPath,
            ManifestSha256 = verified.Sha256,
            SnapshotId = verified.Manifest.ManifestId,
            StateFingerprint = verified.Manifest.State.StateFingerprint,
            SourceLocator = verified.Manifest.State.SourceLocator
        };
        var arguments = new ReadOnlyDictionary<string, object?>(
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [TimelineEntryIdArgument] = entry.Id,
                [RestoreEvidenceArgument] = evidence
            });
        var operation = new OperationDescriptor
        {
            Kind = StartupEntryControlOperationPolicy.RestoreKind,
            Title = "Restore one startup entry",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Medium,
            IsDestructive = true,
            RequiresElevation = false,
            RequiresSnapshot = true,
            SnapshotId = verified.Manifest.ManifestId,
            RollbackRequired = false,
            ConfirmationAccepted = false,
            EvidenceSummary = "One current-user startup entry has verified rollback evidence.",
            EstimatedImpactBytes = 0,
            ConfirmationText = "Restore this ordinary startup entry? The application will not be started now.",
            AffectedRegistryKeys = [verified.Manifest.State.SourceLocator],
            Arguments = arguments
        };

        return StartupRestorePreparationResult.Accepted(operation, entry);
    }

    public static OperationResult ValidatePreparedCandidate(OperationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!descriptor.Kind.Equals(
                StartupEntryControlOperationPolicy.RestoreKind,
                StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Fail("Only supported startup restore operations are accepted.");
        }
        if (descriptor.Source != OperationSource.Manual
            || descriptor.Risk != RiskLevel.Medium
            || !descriptor.IsDestructive
            || descriptor.RequiresElevation
            || !descriptor.RequiresSnapshot
            || descriptor.RollbackRequired
            || descriptor.EstimatedImpactBytes != 0)
        {
            return OperationResult.Fail("The startup restore safety classification is invalid.");
        }
        if (string.IsNullOrWhiteSpace(descriptor.EvidenceSummary)
            || string.IsNullOrWhiteSpace(descriptor.ConfirmationText)
            || descriptor.AffectedPaths.Count != 0
            || descriptor.AffectedServices.Count != 0
            || descriptor.AffectedRegistryKeys.Count != 1
            || !StartupEntryControlPolicy.IsSupportedLocator(descriptor.AffectedRegistryKeys[0]))
        {
            return OperationResult.Fail("The startup restore scope is invalid.");
        }
        if (!TryGetTimelineEntryId(descriptor, out _)
            || !TryGetEvidence(descriptor, out var evidence)
            || string.IsNullOrWhiteSpace(descriptor.SnapshotId)
            || !string.Equals(descriptor.SnapshotId, evidence.SnapshotId, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(evidence.ManifestPath)
            || !Path.IsPathFullyQualified(evidence.ManifestPath)
            || evidence.ManifestPath.StartsWith("\\\\", StringComparison.Ordinal)
            || !StartupEntryControlPolicy.IsSha256(evidence.ManifestSha256)
            || !StartupEntryControlPolicy.IsSha256(evidence.StateFingerprint)
            || string.IsNullOrWhiteSpace(evidence.SnapshotId)
            || !StartupEntryControlPolicy.IsSupportedLocator(evidence.SourceLocator)
            || !string.Equals(
                descriptor.AffectedRegistryKeys[0],
                evidence.SourceLocator,
                StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Fail("The startup restore evidence is invalid or inconsistent.");
        }

        return OperationResult.Ok("Startup restore evidence accepted.");
    }

    public static OperationDescriptor ConfirmForExecution(OperationDescriptor descriptor)
    {
        var validation = ValidatePreparedCandidate(descriptor);
        if (!validation.Success)
            throw new InvalidOperationException(validation.Error);
        return Clone(descriptor, confirmationAccepted: true);
    }

    public static bool TryGetTimelineEntryId(
        OperationDescriptor descriptor,
        out long timelineEntryId)
    {
        timelineEntryId = 0;
        return descriptor.Arguments.TryGetValue(TimelineEntryIdArgument, out var value)
            && value is long typed
            && (timelineEntryId = typed) > 0;
    }

    public static bool TryGetEvidence(
        OperationDescriptor descriptor,
        out StartupRestoreEvidence evidence)
    {
        evidence = null!;
        if (!descriptor.Arguments.TryGetValue(RestoreEvidenceArgument, out var value)
            || value is not StartupRestoreEvidence typed)
        {
            return false;
        }
        evidence = typed;
        return true;
    }

    public static bool MatchesCurrentEntry(
        ActionTimelineEntry entry,
        StartupRestoreEvidence evidence) =>
        entry.RestoreState == RestoreState.Restorable
        && entry.RestoreOperationKind?.Equals(
            StartupEntryControlOperationPolicy.RestoreKind,
            StringComparison.OrdinalIgnoreCase) == true
        && entry.RestoreManifestPaths.Count == 1
        && PathEquals(entry.RestoreManifestPaths[0], evidence.ManifestPath)
        && entry.AffectedPaths.Count == 0
        && entry.AffectedRegistryKeys.Count == 1
        && string.Equals(
            entry.AffectedRegistryKeys[0],
            evidence.SourceLocator,
            StringComparison.OrdinalIgnoreCase);

    public static async Task<StartupRestoreEvidenceVerification> RevalidateEvidenceAsync(
        StartupRestoreEvidence evidence,
        StartupRollbackManifestStore manifests,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verified = await manifests.LoadVerifiedAsync(
                new StartupRollbackManifestEvidence(
                    evidence.SnapshotId,
                    evidence.ManifestPath,
                    evidence.ManifestSha256),
                cancellationToken);
            if (!string.Equals(
                    verified.Manifest.State.StateFingerprint,
                    evidence.StateFingerprint,
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(
                    verified.Manifest.State.SourceLocator,
                    evidence.SourceLocator,
                    StringComparison.OrdinalIgnoreCase))
            {
                return StartupRestoreEvidenceVerification.Refused(
                    "The startup rollback state changed after confirmation.");
            }
            return StartupRestoreEvidenceVerification.Accepted(verified);
        }
        catch
        {
            return StartupRestoreEvidenceVerification.Refused(
                "The startup rollback manifest changed after confirmation.");
        }
    }

    private static OperationDescriptor Clone(
        OperationDescriptor descriptor,
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
            AffectedPaths = descriptor.AffectedPaths,
            AffectedRegistryKeys = descriptor.AffectedRegistryKeys,
            AffectedServices = descriptor.AffectedServices,
            Arguments = descriptor.Arguments
        };

    private static bool PathEquals(string left, string right)
    {
        try
        {
            return Path.GetFullPath(left).Equals(
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

public sealed class StartupRestoreOperationHandler
{
    private readonly IStartupEntryControlStore _store;
    private readonly StartupRollbackManifestStore _manifests;
    private readonly ActionTimelineStore _timeline;

    public StartupRestoreOperationHandler(
        IStartupEntryControlStore store,
        StartupRollbackManifestStore manifests,
        ActionTimelineStore timeline)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _manifests = manifests ?? throw new ArgumentNullException(nameof(manifests));
        _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        var validation = StartupRestoreOperationPolicy.ValidatePreparedCandidate(descriptor);
        if (!validation.Success)
            return validation;
        if (!descriptor.ConfirmationAccepted)
            return OperationResult.Fail("Startup restore requires explicit user confirmation.");
        if (!StartupRestoreOperationPolicy.TryGetTimelineEntryId(descriptor, out var timelineEntryId)
            || !StartupRestoreOperationPolicy.TryGetEvidence(descriptor, out var evidence))
        {
            return OperationResult.Fail("Startup restore evidence is unavailable.");
        }

        var currentEntry = await _timeline.LoadByIdAsync(timelineEntryId, cancellationToken);
        if (currentEntry is null
            || !StartupRestoreOperationPolicy.MatchesCurrentEntry(currentEntry, evidence))
        {
            return OperationResult.Fail("The startup timeline entry changed after confirmation.");
        }

        var verification = await StartupRestoreOperationPolicy.RevalidateEvidenceAsync(
            evidence,
            _manifests,
            cancellationToken);
        if (!verification.Success || verification.VerifiedManifest is null)
            return OperationResult.Fail(verification.Error ?? "Startup rollback evidence is unavailable.");

        StartupEntryMutationResult restored;
        try
        {
            restored = await _store.RestoreAsync(
                verification.VerifiedManifest.Manifest.State,
                cancellationToken);
        }
        catch
        {
            return await CompleteAttemptFailureAsync(timelineEntryId);
        }

        if (!restored.Success)
            return await CompleteAttemptFailureAsync(timelineEntryId);

        var timelineUpdated = await TryUpdateTimelineAsync(
            timelineEntryId,
            RestoreState.Restored,
            restoreOperationKind: null);
        var outcome = new StartupRestoreOperationOutcome
        {
            RestoreState = RestoreState.Restored,
            TimelineUpdated = timelineUpdated,
            MutationAttempted = true,
            MutationSucceeded = true
        };
        return timelineUpdated
            ? OperationResult.Ok("The startup entry was restored and the timeline was updated.", outcome)
            : new OperationResult
            {
                Success = false,
                Error = "The startup entry was restored, but the timeline could not be updated.",
                Payload = outcome
            };
    }

    private async Task<OperationResult> CompleteAttemptFailureAsync(long timelineEntryId)
    {
        var timelineUpdated = await TryUpdateTimelineAsync(
            timelineEntryId,
            RestoreState.PartiallyRestorable,
            StartupEntryControlOperationPolicy.RestoreKind);
        return new OperationResult
        {
            Success = false,
            Error = "Startup restore did not complete; review the current startup state before retrying.",
            Payload = new StartupRestoreOperationOutcome
            {
                RestoreState = RestoreState.PartiallyRestorable,
                TimelineUpdated = timelineUpdated,
                MutationAttempted = true,
                MutationSucceeded = false
            }
        };
    }

    private async Task<bool> TryUpdateTimelineAsync(
        long timelineEntryId,
        RestoreState restoreState,
        string? restoreOperationKind)
    {
        try
        {
            await _timeline.UpdateRestoreStateAsync(
                timelineEntryId,
                restoreState,
                restoreOperationKind,
                CancellationToken.None);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
