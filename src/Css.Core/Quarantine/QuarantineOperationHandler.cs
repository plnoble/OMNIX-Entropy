using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Operations;
using Css.Core.Timeline;

namespace Css.Core.Quarantine;

public sealed class QuarantineOperationHandler
{
    private readonly FileQuarantineService _quarantine;
    private readonly ActionTimelineStore _timeline;
    private readonly IQuarantineCandidateIdentityReader _identityReader;

    public QuarantineOperationHandler(
        FileQuarantineService quarantine,
        ActionTimelineStore timeline,
        IQuarantineCandidateIdentityReader identityReader)
    {
        _quarantine = quarantine;
        _timeline = timeline;
        _identityReader = identityReader;
    }

    public async Task<OperationResult> ExecuteAsync(OperationDescriptor descriptor, CancellationToken ct = default)
    {
        var policy = QuarantineOperationPolicy.ValidatePreparedCandidate(descriptor);
        if (!policy.Success)
            return policy;
        if (!descriptor.ConfirmationAccepted)
            return OperationResult.Fail("Quarantine operations require explicit user confirmation.");

        if (!QuarantineOperationPolicy.TryGetCandidateEvidence(descriptor, out var evidence))
            return OperationResult.Fail("隔离方案缺少确认前候选身份。");

        var preflight = QuarantineCandidateEvidencePolicy.Revalidate(
            descriptor.AffectedPaths,
            evidence,
            _quarantine.QuarantineRoot,
            _identityReader);
        if (!preflight.Success)
            return preflight;

        var records = new List<QuarantineRecord>();
        try
        {
            for (var index = 0; index < descriptor.AffectedPaths.Count; index++)
            {
                ct.ThrowIfCancellationRequested();
                records.Add(await _quarantine.QuarantineAsync(
                    descriptor.AffectedPaths[index],
                    descriptor.EvidenceSummary ?? descriptor.Title,
                    evidence[index],
                    _identityReader,
                    ct));
            }

            await _timeline.AddAsync(new ActionTimelineEntry
            {
                OccurredAt = DateTimeOffset.Now,
                Source = descriptor.Source,
                Title = descriptor.Title,
                EvidenceSummary = descriptor.EvidenceSummary ?? "已移动到隔离区。",
                AffectedPaths = records.Select(record => record.OriginalPath).ToList(),
                RestoreState = RestoreState.Restorable,
                RestoreOperationKind = "quarantine.restore",
                RestoreManifestPaths = records.Select(record => record.ManifestPath).ToList()
            }, ct);
        }
        catch
        {
            var incomplete = new List<QuarantineRecord>();
            foreach (var record in records.AsEnumerable().Reverse())
            {
                try
                {
                    var restore = await _quarantine.RestoreAsync(record.ManifestPath, CancellationToken.None);
                    if (!restore.Success)
                        incomplete.Add(record);
                }
                catch
                {
                    incomplete.Add(record);
                }
            }

            if (incomplete.Count > 0)
            {
                try
                {
                    await _timeline.AddAsync(new ActionTimelineEntry
                    {
                        OccurredAt = DateTimeOffset.Now,
                        Source = descriptor.Source,
                        Title = descriptor.Title + "（未完整完成）",
                        EvidenceSummary = "隔离过程未完整完成，部分项目需要在后悔药中心复查。",
                        AffectedPaths = incomplete.Select(record => record.OriginalPath).ToList(),
                        RestoreState = RestoreState.PartiallyRestorable,
                        RestoreOperationKind = "quarantine.restore",
                        RestoreManifestPaths = incomplete.Select(record => record.ManifestPath).ToList()
                    }, CancellationToken.None);
                }
                catch
                {
                    // Each moved item still has a manifest written before its move.
                }

                return OperationResult.Fail("隔离过程未完整完成，部分项目需要在后悔药中心复查。");
            }

            return OperationResult.Fail("隔离过程没有完成，已移动项目已自动还原。");
        }

        var movedBytes = records.Sum(record => record.SizeBytes);
        return OperationResult.Ok($"已移动 {records.Count} 项到隔离区，约 {movedBytes} bytes。", records);
    }
}
