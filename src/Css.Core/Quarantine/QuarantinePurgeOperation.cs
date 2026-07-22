using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Operations;
using Css.Core.Timeline;

namespace Css.Core.Quarantine;

public static class QuarantinePurgeOperationPolicy
{
    public const string OperationKind = "quarantine.purge";
    public const int MaximumManifestsPerOperation = 100;

    public static OperationDescriptor CreatePlan(QuarantineRetentionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var manifests = plan.Candidates
            .Select(candidate => candidate.Record.ManifestPath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (manifests.Length == 0)
            throw new InvalidOperationException("Quarantine purge requires at least one candidate.");
        if (manifests.Length > MaximumManifestsPerOperation)
            throw new InvalidOperationException("Quarantine purge candidate count exceeds the batch limit.");

        return new OperationDescriptor
        {
            Kind = OperationKind,
            Title = "永久整理隔离区",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Medium,
            IsDestructive = true,
            RequiresSnapshot = false,
            RollbackRequired = false,
            ConfirmationAccepted = false,
            EvidenceSummary = $"隔离区有 {manifests.Length} 项到期或超出容量建议，预计最多释放 {FormatBytes(plan.ReclaimableBytes)}。",
            EstimatedImpactBytes = plan.ReclaimableBytes,
            ConfirmationText = $"确认永久整理这 {manifests.Length} 项隔离记录？整理后不能还原。",
            AffectedPaths = manifests
        };
    }

    public static OperationResult ValidateCandidate(OperationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!descriptor.Kind.Equals(OperationKind, StringComparison.OrdinalIgnoreCase))
            return OperationResult.Fail("Only quarantine purge operations are supported.");
        if (descriptor.Source != OperationSource.Manual)
            return OperationResult.Fail("Permanent quarantine cleanup must originate from a manual user action.");
        if (!descriptor.IsDestructive || descriptor.Risk != RiskLevel.Medium)
            return OperationResult.Fail("Permanent quarantine cleanup must be a medium-risk destructive operation.");
        if (descriptor.RollbackRequired || descriptor.RequiresSnapshot)
            return OperationResult.Fail("Permanent quarantine cleanup cannot claim rollback or snapshot recovery.");
        if (string.IsNullOrWhiteSpace(descriptor.EvidenceSummary)
            || string.IsNullOrWhiteSpace(descriptor.ConfirmationText)
            || !descriptor.ConfirmationText.Contains("永久", StringComparison.Ordinal)
            || !descriptor.ConfirmationText.Contains("不能还原", StringComparison.Ordinal))
        {
            return OperationResult.Fail("Permanent quarantine cleanup requires explicit irreversible confirmation text.");
        }
        if (descriptor.AffectedPaths.Count is < 1 or > MaximumManifestsPerOperation)
            return OperationResult.Fail("Permanent quarantine cleanup requires a bounded manifest list.");
        if (descriptor.AffectedRegistryKeys.Count > 0 || descriptor.AffectedServices.Count > 0)
            return OperationResult.Fail("Permanent quarantine cleanup cannot modify registry keys or services.");

        try
        {
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in descriptor.AffectedPaths)
            {
                if (string.IsNullOrWhiteSpace(path)
                    || !Path.IsPathFullyQualified(path)
                    || path.StartsWith("\\\\", StringComparison.Ordinal)
                    || !Path.GetFileName(path).Equals("manifest.json", StringComparison.OrdinalIgnoreCase)
                    || !unique.Add(Path.GetFullPath(path)))
                {
                    return OperationResult.Fail("Permanent quarantine cleanup contains an invalid or duplicate manifest path.");
                }
            }
        }
        catch
        {
            return OperationResult.Fail("Permanent quarantine cleanup contains a manifest path that cannot be normalized.");
        }

        return OperationResult.Ok("Quarantine purge candidate accepted.");
    }

    public static OperationDescriptor ConfirmForExecution(OperationDescriptor descriptor)
    {
        var validation = ValidateCandidate(descriptor);
        if (!validation.Success)
            throw new InvalidOperationException(validation.Error);
        return new OperationDescriptor
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
            ConfirmationAccepted = true,
            EvidenceSummary = descriptor.EvidenceSummary,
            EstimatedImpactBytes = descriptor.EstimatedImpactBytes,
            ConfirmationText = descriptor.ConfirmationText,
            AffectedPaths = descriptor.AffectedPaths,
            AffectedRegistryKeys = descriptor.AffectedRegistryKeys,
            AffectedServices = descriptor.AffectedServices,
            Arguments = descriptor.Arguments
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return unit == 0 ? $"{bytes} B" : $"{value:0.0} {units[unit]}";
    }
}

public sealed class QuarantinePurgeOperationHandler
{
    private readonly FileQuarantineService _quarantine;
    private readonly ActionTimelineStore _timeline;

    public QuarantinePurgeOperationHandler(
        FileQuarantineService quarantine,
        ActionTimelineStore timeline)
    {
        _quarantine = quarantine;
        _timeline = timeline;
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken ct = default)
    {
        var validation = QuarantinePurgeOperationPolicy.ValidateCandidate(descriptor);
        if (!validation.Success)
            return validation;
        if (!descriptor.ConfirmationAccepted)
            return OperationResult.Fail("Permanent quarantine cleanup requires explicit final confirmation.");

        foreach (var manifest in descriptor.AffectedPaths)
        {
            var inspection = await _quarantine.InspectPurgeCandidateAsync(manifest, ct);
            if (!inspection.Success || inspection.Record is null)
                return OperationResult.Fail(inspection.Summary);
        }

        var completed = new List<QuarantinePurgeResult>();
        foreach (var manifest in descriptor.AffectedPaths)
        {
            QuarantinePurgeResult result;
            try
            {
                result = await _quarantine.PurgeAsync(manifest, ct);
            }
            catch (OperationCanceledException)
            {
                await TryRecordTimelineAsync(descriptor, completed, incomplete: true);
                return OperationResult.Fail("永久整理已停止；已完成项目不能还原，请重新加载后悔药中心复查。");
            }
            if (!result.Success)
            {
                if (result.MayHaveChanged)
                    completed.Add(result);
                await TryRecordTimelineAsync(descriptor, completed, incomplete: true);
                return OperationResult.Fail(result.Summary);
            }
            completed.Add(result);
        }

        if (!await TryRecordTimelineAsync(descriptor, completed, incomplete: false))
            return OperationResult.Fail("隔离副本已永久整理，但时间线记录失败；请重新加载后悔药中心复查。");
        return OperationResult.Ok(
            $"已永久整理 {completed.Count} 项隔离记录；这些项目不能再还原。",
            completed);
    }

    private async Task<bool> TryRecordTimelineAsync(
        OperationDescriptor descriptor,
        IReadOnlyList<QuarantinePurgeResult> completed,
        bool incomplete)
    {
        try
        {
            await RecordTimelineAsync(
                descriptor,
                completed,
                incomplete,
                CancellationToken.None);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task RecordTimelineAsync(
        OperationDescriptor descriptor,
        IReadOnlyList<QuarantinePurgeResult> completed,
        bool incomplete,
        CancellationToken ct)
    {
        if (completed.Count == 0)
            return;
        var records = completed
            .Select(result => result.Record)
            .Where(record => record is not null)
            .Cast<QuarantineRecord>()
            .ToArray();
        await _timeline.AddAsync(new ActionTimelineEntry
        {
            OccurredAt = DateTimeOffset.Now,
            Source = descriptor.Source,
            Title = incomplete ? descriptor.Title + "（未完整完成）" : descriptor.Title,
            EvidenceSummary = incomplete
                ? $"已永久整理 {records.Length} 项，后续项目失败；已整理项目不能还原。"
                : $"已永久整理 {records.Length} 项隔离记录；这些项目不能还原。",
            AffectedPaths = records.Select(record => record.OriginalPath).ToArray(),
            RestoreState = RestoreState.NotRestorable
        }, ct);
    }
}
