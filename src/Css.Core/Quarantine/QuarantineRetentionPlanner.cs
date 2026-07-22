using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Timeline;

namespace Css.Core.Quarantine;

public enum QuarantineCleanupReason
{
    Expired,
    OverCapacity,
    AlreadyRestored
}

public sealed class QuarantineRetentionOptions
{
    public DateTimeOffset Now { get; init; } = DateTimeOffset.Now;
    public int RetentionDays { get; init; } = 30;
    public long MaxTotalBytes { get; init; } = 20L * 1024 * 1024 * 1024;
    public int MaximumCandidates { get; init; } = 100;
}

public sealed class QuarantineCleanupCandidate
{
    public required QuarantineRecord Record { get; init; }
    public QuarantineCleanupReason Reason { get; init; }
    public required string ReasonText { get; init; }
    public long EstimatedReclaimableBytes { get; init; }
    public bool RequiresConfirmation { get; init; } = true;
}

public sealed class QuarantineRetentionPlan
{
    public required string Summary { get; init; }
    public long TotalBytes { get; init; }
    public long ActiveBytes { get; init; }
    public long ReclaimableBytes { get; init; }
    public long ProjectedActiveBytes { get; init; }
    public int RetentionDays { get; init; }
    public long MaxTotalBytes { get; init; }
    public bool IsOverCapacity { get; init; }
    public bool WasTruncated { get; init; }
    public bool WouldDeleteAutomatically { get; init; }
    public IReadOnlyList<QuarantineCleanupCandidate> Candidates { get; init; } = [];
}

public static class QuarantineRetentionPlanner
{
    public static QuarantineRetentionPlan Build(
        IReadOnlyList<QuarantineRecord> records,
        QuarantineRetentionOptions options)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        var totalBytes = SaturatingSum(records.Select(SafeBytes));
        var activeRecords = records
            .Where(record => record.RestoreState != RestoreState.Restored)
            .ToList();
        var activeBytes = SaturatingSum(activeRecords.Select(SafeBytes));
        var cutoff = SafeCutoff(options.Now, options.RetentionDays);
        var candidates = new List<QuarantineCleanupCandidate>();
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var wasTruncated = false;

        foreach (var record in activeRecords
            .Where(record => record.MovedAt < cutoff)
            .OrderBy(record => record.MovedAt))
        {
            if (candidates.Count >= options.MaximumCandidates)
            {
                wasTruncated = true;
                break;
            }
            AddCandidate(candidates, selected, record, QuarantineCleanupReason.Expired, $"已超过 {options.RetentionDays} 天保留期。");
        }

        var candidateBytes = candidates
            .Where(candidate => candidate.Record.RestoreState != RestoreState.Restored)
            .Select(candidate => candidate.EstimatedReclaimableBytes);
        var bytesAfterCandidateCleanup = SubtractFloorZero(
            activeBytes,
            SaturatingSum(candidateBytes));

        if (bytesAfterCandidateCleanup > options.MaxTotalBytes)
        {
            foreach (var record in activeRecords.OrderBy(record => record.MovedAt))
            {
                if (bytesAfterCandidateCleanup <= options.MaxTotalBytes)
                    break;
                if (candidates.Count >= options.MaximumCandidates)
                {
                    wasTruncated = true;
                    break;
                }

                if (selected.Contains(Key(record)))
                    continue;

                AddCandidate(candidates, selected, record, QuarantineCleanupReason.OverCapacity, "隔离区超过容量上限，建议确认后清理较旧副本。");
                bytesAfterCandidateCleanup = SubtractFloorZero(
                    bytesAfterCandidateCleanup,
                    SafeBytes(record));
            }
        }

        foreach (var record in records.Where(record => record.RestoreState == RestoreState.Restored))
        {
            if (candidates.Count >= options.MaximumCandidates)
            {
                wasTruncated = true;
                break;
            }
            AddCandidate(candidates, selected, record, QuarantineCleanupReason.AlreadyRestored, "已还原，只剩隔离区记录可整理。");
        }

        var reclaimableBytes = SaturatingSum(candidates
            .Select(candidate => candidate.EstimatedReclaimableBytes));
        var projectedActiveBytes = SubtractFloorZero(activeBytes, reclaimableBytes);

        return new QuarantineRetentionPlan
        {
            Summary = candidates.Count == 0
                ? "隔离区当前无需处理；这里只读盘点，不会自动删除。"
                : $"隔离区发现 {candidates.Count} 项可考虑整理，预计最多释放 {FormatBytes(reclaimableBytes)}；只生成建议，不会自动删除。",
            TotalBytes = totalBytes,
            ActiveBytes = activeBytes,
            ReclaimableBytes = reclaimableBytes,
            ProjectedActiveBytes = projectedActiveBytes,
            RetentionDays = options.RetentionDays,
            MaxTotalBytes = options.MaxTotalBytes,
            IsOverCapacity = activeBytes > options.MaxTotalBytes,
            WasTruncated = wasTruncated,
            WouldDeleteAutomatically = false,
            Candidates = candidates
        };
    }

    private static void AddCandidate(
        List<QuarantineCleanupCandidate> candidates,
        HashSet<string> selected,
        QuarantineRecord record,
        QuarantineCleanupReason reason,
        string reasonText)
    {
        if (!selected.Add(Key(record)))
            return;

        candidates.Add(new QuarantineCleanupCandidate
        {
            Record = record,
            Reason = reason,
            ReasonText = reasonText,
            EstimatedReclaimableBytes = record.RestoreState == RestoreState.Restored
                ? 0
                : SafeBytes(record),
            RequiresConfirmation = true
        });
    }

    private static void ValidateOptions(QuarantineRetentionOptions options)
    {
        if (options.RetentionDays is < 1 or > 3650)
            throw new ArgumentOutOfRangeException(nameof(options), "Retention days must be between 1 and 3650.");
        if (options.MaxTotalBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum quarantine bytes must be positive.");
        if (options.MaximumCandidates is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum candidates must be between 1 and 100.");
    }

    private static DateTimeOffset SafeCutoff(DateTimeOffset now, int retentionDays)
    {
        try
        {
            return now.AddDays(-retentionDays);
        }
        catch (ArgumentOutOfRangeException)
        {
            return DateTimeOffset.MinValue;
        }
    }

    private static long SafeBytes(QuarantineRecord record) =>
        Math.Max(0, record.SizeBytes);

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

    private static long SubtractFloorZero(long value, long subtract) =>
        subtract >= value ? 0 : value - subtract;

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

    private static string Key(QuarantineRecord record) =>
        string.IsNullOrWhiteSpace(record.ManifestPath) ? record.Id : record.ManifestPath;
}
