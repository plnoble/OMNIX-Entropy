using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Css.Core.Operations;
using Css.Core.Timeline;

namespace Css.Core.Quarantine;

public sealed record QuarantineRestoreEvidence
{
    public required string ManifestPath { get; init; }
    public required string ManifestSha256 { get; init; }
    public required string RecordId { get; init; }
    public required string OriginalPath { get; init; }
    public required string QuarantinedPath { get; init; }
    public long SizeBytes { get; init; }
    public required QuarantineCandidateEvidence PayloadEvidence { get; init; }
}

public sealed class QuarantineRestorePreparationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public ActionTimelineEntry? CurrentEntry { get; init; }

    public static QuarantineRestorePreparationResult Accepted(
        OperationDescriptor operation,
        ActionTimelineEntry currentEntry) =>
        new()
        {
            Success = true,
            Operation = operation,
            CurrentEntry = currentEntry
        };

    public static QuarantineRestorePreparationResult Refused(string error) =>
        new() { Success = false, Error = error };
}

public sealed class QuarantineRestoreOperationOutcome
{
    public required RestoreState RestoreState { get; init; }
    public required bool TimelineUpdated { get; init; }
    public required IReadOnlyList<QuarantineRestoreResult> Results { get; init; }
}

public static class QuarantineRestoreOperationPolicy
{
    public const string OperationKind = "quarantine.restore";
    public const string TimelineEntryIdArgument = "quarantine.restore.timeline-entry-id";
    public const string RestoreEvidenceArgument = "quarantine.restore.evidence";
    public const int MaximumManifestsPerOperation = 64;

    public static async Task<QuarantineRestorePreparationResult> PrepareForConfirmationAsync(
        long timelineEntryId,
        ActionTimelineStore timeline,
        FileQuarantineService quarantine,
        IQuarantineCandidateIdentityReader identityReader,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(quarantine);
        ArgumentNullException.ThrowIfNull(identityReader);
        if (timelineEntryId <= 0)
            return QuarantineRestorePreparationResult.Refused("后悔药记录编号无效。");

        var entry = await timeline.LoadByIdAsync(timelineEntryId, ct);
        if (entry is null)
            return QuarantineRestorePreparationResult.Refused("后悔药记录已经不存在，请重新加载。");
        if (entry.RestoreState != RestoreState.Restorable
            || entry.RestoreOperationKind?.Equals(OperationKind, StringComparison.OrdinalIgnoreCase) != true)
        {
            return QuarantineRestorePreparationResult.Refused("这条后悔药记录当前不能执行隔离区还原。");
        }
        if (entry.RestoreManifestPaths.Count is < 1 or > MaximumManifestsPerOperation)
            return QuarantineRestorePreparationResult.Refused("还原记录数量超出安全范围。");

        var evidence = new List<QuarantineRestoreEvidence>(entry.RestoreManifestPaths.Count);
        var distinctManifests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var manifestPath in entry.RestoreManifestPaths)
        {
            ct.ThrowIfCancellationRequested();
            var prepared = await PrepareEvidenceAsync(
                manifestPath,
                quarantine,
                identityReader,
                ct);
            if (!prepared.Success || prepared.Evidence is null)
                return QuarantineRestorePreparationResult.Refused(prepared.Error);
            if (!distinctManifests.Add(prepared.Evidence.ManifestPath))
                return QuarantineRestorePreparationResult.Refused("还原记录包含重复 manifest。");
            evidence.Add(prepared.Evidence);
        }

        if (!SequenceMatches(entry.AffectedPaths, evidence.Select(item => item.OriginalPath).ToArray()))
            return QuarantineRestorePreparationResult.Refused("后悔药记录的影响范围与还原 manifest 不一致。");

        var arguments = new ReadOnlyDictionary<string, object?>(
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [TimelineEntryIdArgument] = entry.Id,
                [RestoreEvidenceArgument] = Array.AsReadOnly(evidence.ToArray())
            });
        var operation = new OperationDescriptor
        {
            Kind = OperationKind,
            Title = "还原隔离内容",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RequiresElevation = false,
            RequiresSnapshot = false,
            RollbackRequired = false,
            ConfirmationAccepted = false,
            EvidenceSummary = $"后悔药中心当前记录包含 {evidence.Count} 项可还原内容，已核对 manifest 和隔离副本身份。",
            EstimatedImpactBytes = SaturatingSum(evidence.Select(item => item.SizeBytes)),
            ConfirmationText = $"确认把这 {evidence.Count} 项隔离内容放回原位置？如果原位置已有内容将拒绝覆盖。",
            AffectedPaths = evidence.Select(item => item.OriginalPath).ToArray(),
            Arguments = arguments
        };

        return QuarantineRestorePreparationResult.Accepted(operation, entry);
    }

    public static OperationResult ValidatePreparedCandidate(OperationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!descriptor.Kind.Equals(OperationKind, StringComparison.OrdinalIgnoreCase))
            return OperationResult.Fail("Only quarantine restore operations are supported.");
        if (descriptor.Source != OperationSource.Manual)
            return OperationResult.Fail("Quarantine restore must originate from a manual user action.");
        if (!descriptor.IsDestructive || descriptor.Risk != RiskLevel.Low)
            return OperationResult.Fail("Quarantine restore must be a low-risk destructive operation.");
        if (descriptor.RequiresSnapshot || descriptor.RollbackRequired || descriptor.RequiresElevation)
            return OperationResult.Fail("Quarantine restore cannot claim another snapshot, rollback, or elevation gate.");
        if (string.IsNullOrWhiteSpace(descriptor.EvidenceSummary)
            || string.IsNullOrWhiteSpace(descriptor.ConfirmationText))
        {
            return OperationResult.Fail("Quarantine restore requires evidence and explicit confirmation text.");
        }
        if (descriptor.AffectedRegistryKeys.Count > 0 || descriptor.AffectedServices.Count > 0)
            return OperationResult.Fail("Quarantine restore cannot modify registry keys or services.");
        if (!TryGetTimelineEntryId(descriptor, out _)
            || !TryGetEvidence(descriptor, out var evidence)
            || evidence.Count is < 1 or > MaximumManifestsPerOperation
            || descriptor.AffectedPaths.Count != evidence.Count)
        {
            return OperationResult.Fail("Quarantine restore requires a bounded, timeline-bound evidence set.");
        }

        var manifests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < evidence.Count; index++)
        {
            var item = evidence[index];
            if (string.IsNullOrWhiteSpace(item.ManifestPath)
                || !Path.IsPathFullyQualified(item.ManifestPath)
                || item.ManifestPath.StartsWith("\\\\", StringComparison.Ordinal)
                || !Path.GetFileName(item.ManifestPath).Equals("manifest.json", StringComparison.OrdinalIgnoreCase)
                || !manifests.Add(Path.GetFullPath(item.ManifestPath))
                || string.IsNullOrWhiteSpace(item.ManifestSha256)
                || item.ManifestSha256.Length != 64
                || string.IsNullOrWhiteSpace(item.RecordId)
                || string.IsNullOrWhiteSpace(item.OriginalPath)
                || string.IsNullOrWhiteSpace(item.QuarantinedPath)
                || item.SizeBytes < 0
                || !PathEquals(descriptor.AffectedPaths[index], item.OriginalPath)
                || !PathEquals(item.PayloadEvidence.CanonicalPath, item.QuarantinedPath))
            {
                return OperationResult.Fail("Quarantine restore evidence is invalid or inconsistent.");
            }
        }

        return OperationResult.Ok("Quarantine restore evidence accepted.");
    }

    public static OperationDescriptor ConfirmForExecution(OperationDescriptor descriptor)
    {
        var validation = ValidatePreparedCandidate(descriptor);
        if (!validation.Success)
            throw new InvalidOperationException(validation.Error);
        return Clone(descriptor, confirmationAccepted: true);
    }

    public static bool TryGetTimelineEntryId(OperationDescriptor descriptor, out long timelineEntryId)
    {
        timelineEntryId = 0;
        return descriptor.Arguments.TryGetValue(TimelineEntryIdArgument, out var value)
            && value is long typed
            && (timelineEntryId = typed) > 0;
    }

    public static bool TryGetEvidence(
        OperationDescriptor descriptor,
        out IReadOnlyList<QuarantineRestoreEvidence> evidence)
    {
        evidence = [];
        if (!descriptor.Arguments.TryGetValue(RestoreEvidenceArgument, out var value)
            || value is not IReadOnlyList<QuarantineRestoreEvidence> typed)
        {
            return false;
        }
        evidence = typed;
        return true;
    }

    public static async Task<OperationResult> RevalidateEvidenceAsync(
        QuarantineRestoreEvidence evidence,
        FileQuarantineService quarantine,
        IQuarantineCandidateIdentityReader identityReader,
        CancellationToken ct = default)
    {
        try
        {
            var hashBefore = await ComputeSha256Async(evidence.ManifestPath, ct);
            if (!hashBefore.Equals(evidence.ManifestSha256, StringComparison.Ordinal))
                return Changed();

            var inspection = await quarantine.InspectManifestAsync(evidence.ManifestPath, ct);
            var record = inspection.Record;
            if (!inspection.Success || record is null)
                return OperationResult.Fail(inspection.Summary);
            if (record.RestoreState != RestoreState.Restorable
                || !record.Id.Equals(evidence.RecordId, StringComparison.Ordinal)
                || !PathEquals(record.ManifestPath, evidence.ManifestPath)
                || !PathEquals(record.OriginalPath, evidence.OriginalPath)
                || !PathEquals(record.QuarantinedPath, evidence.QuarantinedPath)
                || record.SizeBytes != evidence.SizeBytes)
            {
                return Changed();
            }

            if (File.Exists(record.OriginalPath)
                || Directory.Exists(record.OriginalPath)
                || QuarantineCandidatePathPolicy.HasReparsePointInExistingChain(record.OriginalPath))
            {
                return OperationResult.Fail("原位置已有内容或路径状态变化，已拒绝还原。");
            }

            var payload = identityReader.Inspect(record.QuarantinedPath);
            if (!payload.Success || payload.Evidence is null)
                return OperationResult.Fail(payload.Summary);
            if (!QuarantineCandidateEvidencePolicy.SameIdentity(
                    evidence.PayloadEvidence,
                    payload.Evidence))
            {
                return Changed();
            }

            var hashAfter = await ComputeSha256Async(evidence.ManifestPath, ct);
            return hashAfter.Equals(evidence.ManifestSha256, StringComparison.Ordinal)
                ? OperationResult.Ok("还原 manifest 和隔离副本身份仍与确认时一致。")
                : Changed();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return OperationResult.Fail("还原证据当前无法完成安全复核。");
        }
    }

    public static bool MatchesCurrentEntry(
        ActionTimelineEntry entry,
        IReadOnlyList<QuarantineRestoreEvidence> evidence) =>
        entry.RestoreState == RestoreState.Restorable
        && entry.RestoreOperationKind?.Equals(OperationKind, StringComparison.OrdinalIgnoreCase) == true
        && SequenceMatches(entry.RestoreManifestPaths, evidence.Select(item => item.ManifestPath).ToArray())
        && SequenceMatches(entry.AffectedPaths, evidence.Select(item => item.OriginalPath).ToArray());

    private static async Task<(bool Success, string Error, QuarantineRestoreEvidence? Evidence)> PrepareEvidenceAsync(
        string manifestPath,
        FileQuarantineService quarantine,
        IQuarantineCandidateIdentityReader identityReader,
        CancellationToken ct)
    {
        try
        {
            var inspection = await quarantine.InspectManifestAsync(manifestPath, ct);
            var record = inspection.Record;
            if (!inspection.Success || record is null)
                return (false, inspection.Summary, null);
            if (record.RestoreState != RestoreState.Restorable)
                return (false, "隔离记录当前不是可还原状态。", null);
            if (File.Exists(record.OriginalPath)
                || Directory.Exists(record.OriginalPath)
                || QuarantineCandidatePathPolicy.HasReparsePointInExistingChain(record.OriginalPath))
            {
                return (false, "原位置已有内容或路径状态变化，已拒绝生成还原方案。", null);
            }

            var payload = identityReader.Inspect(record.QuarantinedPath);
            if (!payload.Success || payload.Evidence is null)
                return (false, payload.Summary, null);
            if (!PathEquals(payload.Evidence.CanonicalPath, record.QuarantinedPath))
                return (false, "隔离副本身份与 manifest 不一致。", null);

            var normalizedManifest = Path.GetFullPath(manifestPath);
            return (true, string.Empty, new QuarantineRestoreEvidence
            {
                ManifestPath = normalizedManifest,
                ManifestSha256 = await ComputeSha256Async(normalizedManifest, ct),
                RecordId = record.Id,
                OriginalPath = Path.GetFullPath(record.OriginalPath),
                QuarantinedPath = Path.GetFullPath(record.QuarantinedPath),
                SizeBytes = record.SizeBytes,
                PayloadEvidence = payload.Evidence
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return (false, "还原 manifest 或隔离副本当前无法安全读取。", null);
        }
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream, ct));
    }

    private static bool SequenceMatches(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (left.Count != right.Count)
            return false;
        for (var index = 0; index < left.Count; index++)
        {
            if (!PathEquals(left[index], right[index]))
                return false;
        }
        return true;
    }

    private static bool PathEquals(string left, string right)
    {
        try
        {
            return Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Equals(
                    Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static long SaturatingSum(IEnumerable<long> values)
    {
        long total = 0;
        foreach (var value in values.Where(value => value > 0))
        {
            if (value > long.MaxValue - total)
                return long.MaxValue;
            total += value;
        }
        return total;
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

    private static OperationResult Changed() =>
        OperationResult.Fail("还原 manifest 或隔离副本在确认后发生变化，旧方案已停止。");
}

public sealed class QuarantineRestoreOperationHandler
{
    private readonly FileQuarantineService _quarantine;
    private readonly ActionTimelineStore _timeline;
    private readonly IQuarantineCandidateIdentityReader _identityReader;

    public QuarantineRestoreOperationHandler(
        FileQuarantineService quarantine,
        ActionTimelineStore timeline,
        IQuarantineCandidateIdentityReader identityReader)
    {
        _quarantine = quarantine ?? throw new ArgumentNullException(nameof(quarantine));
        _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
        _identityReader = identityReader ?? throw new ArgumentNullException(nameof(identityReader));
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken ct = default)
    {
        var validation = QuarantineRestoreOperationPolicy.ValidatePreparedCandidate(descriptor);
        if (!validation.Success)
            return validation;
        if (!descriptor.ConfirmationAccepted)
            return OperationResult.Fail("Quarantine restore requires explicit user confirmation.");
        if (!QuarantineRestoreOperationPolicy.TryGetTimelineEntryId(descriptor, out var timelineEntryId)
            || !QuarantineRestoreOperationPolicy.TryGetEvidence(descriptor, out var evidence))
        {
            return OperationResult.Fail("Quarantine restore evidence is unavailable.");
        }

        var currentEntry = await _timeline.LoadByIdAsync(timelineEntryId, ct);
        if (currentEntry is null
            || !QuarantineRestoreOperationPolicy.MatchesCurrentEntry(currentEntry, evidence))
        {
            return OperationResult.Fail("后悔药记录在确认后发生变化，旧还原方案已停止。");
        }

        foreach (var item in evidence)
        {
            var preflight = await QuarantineRestoreOperationPolicy.RevalidateEvidenceAsync(
                item,
                _quarantine,
                _identityReader,
                ct);
            if (!preflight.Success)
                return preflight;
        }

        var results = new List<QuarantineRestoreResult>(evidence.Count);
        try
        {
            foreach (var item in evidence)
            {
                ct.ThrowIfCancellationRequested();
                results.Add(await _quarantine.RestoreAsync(
                    item.ManifestPath,
                    item.PayloadEvidence,
                    _identityReader,
                    ct));
            }
        }
        catch (OperationCanceledException)
        {
            return await CompleteFailureAsync(
                timelineEntryId,
                results,
                "还原已停止；当前状态需要在后悔药中心复查。");
        }
        catch
        {
            return await CompleteFailureAsync(
                timelineEntryId,
                results,
                "还原没有完整完成；当前状态需要在后悔药中心复查。");
        }

        var state = AggregateRestoreState(results);
        var timelineUpdated = await TryUpdateTimelineAsync(timelineEntryId, state);
        var outcome = new QuarantineRestoreOperationOutcome
        {
            RestoreState = state,
            TimelineUpdated = timelineUpdated,
            Results = results
        };
        if (!timelineUpdated)
        {
            return new OperationResult
            {
                Success = false,
                Error = "还原结果已产生，但时间线更新失败；请重新加载后悔药中心复查。",
                Payload = outcome
            };
        }
        if (state != RestoreState.Restored)
        {
            return new OperationResult
            {
                Success = false,
                Error = string.Join("；", results.Select(result => result.Summary).Distinct()),
                Payload = outcome
            };
        }

        return OperationResult.Ok($"已还原 {results.Count} 项，后悔药时间线已更新。", outcome);
    }

    private async Task<OperationResult> CompleteFailureAsync(
        long timelineEntryId,
        IReadOnlyList<QuarantineRestoreResult> results,
        string error)
    {
        var state = RestoreState.PartiallyRestorable;
        var timelineUpdated = await TryUpdateTimelineAsync(timelineEntryId, state);
        return new OperationResult
        {
            Success = false,
            Error = error,
            Payload = new QuarantineRestoreOperationOutcome
            {
                RestoreState = state,
                TimelineUpdated = timelineUpdated,
                Results = results
            }
        };
    }

    private async Task<bool> TryUpdateTimelineAsync(long timelineEntryId, RestoreState state)
    {
        try
        {
            await _timeline.UpdateRestoreStateAsync(
                timelineEntryId,
                state,
                state == RestoreState.Restored ? null : QuarantineRestoreOperationPolicy.OperationKind,
                CancellationToken.None);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static RestoreState AggregateRestoreState(
        IReadOnlyList<QuarantineRestoreResult> results)
    {
        if (results.Count == 0)
            return RestoreState.PartiallyRestorable;
        if (results.All(result => result.Success && result.RestoreState == RestoreState.Restored))
            return RestoreState.Restored;
        if (results.All(result => result.RestoreState == RestoreState.NotRestorable))
            return RestoreState.NotRestorable;
        return RestoreState.PartiallyRestorable;
    }
}
