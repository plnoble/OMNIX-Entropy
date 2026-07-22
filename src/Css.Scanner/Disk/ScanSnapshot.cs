using System;
using System.Collections.Generic;
using System.Linq;

namespace Css.Scanner.Disk;

public sealed record ScanSnapshot(DateTimeOffset CapturedAt, IReadOnlyList<ScanSnapshotItem> Items);

public sealed record ScanSnapshotItem(string Path, string OwnerSoftware, long SizeBytes);

public enum GrowthSourceKind
{
    Unknown,
    Software,
    SharedSoftware,
    UserArea,
    SystemArea
}

public sealed class GrowthFinding
{
    public required string Path { get; init; }
    public required string OwnerSoftware { get; init; }
    public long PreviousBytes { get; init; }
    public long CurrentBytes { get; init; }
    public long GrowthBytes => CurrentBytes - PreviousBytes;
    public bool IsNewObservation { get; init; }
    public GrowthSourceKind SourceKind { get; init; }
    public TimeSpan ObservationInterval { get; init; }
    public int ObservedSnapshots { get; init; } = 2;
    public int PositiveGrowthIntervals { get; init; } = 1;
    public bool IsSustainedGrowth { get; init; }
    public long TrendGrowthBytes { get; init; }
    public TimeSpan TrendWindow { get; init; }
    public required string Reason { get; init; }
}

public static class GrowthAnalyzer
{
    public static IReadOnlyList<GrowthFinding> Compare(ScanSnapshot previous, ScanSnapshot current)
    {
        var previousByPath = previous.Items
            .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(item => item.SizeBytes).First(),
                StringComparer.OrdinalIgnoreCase);
        var interval = current.CapturedAt - previous.CapturedAt;

        return current.Items
            .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(item => item.SizeBytes).First())
            .Select(item =>
            {
                previousByPath.TryGetValue(item.Path, out var old);
                var oldBytes = old?.SizeBytes ?? 0;
                return new GrowthFinding
                {
                    Path = item.Path,
                    OwnerSoftware = item.OwnerSoftware,
                    PreviousBytes = oldBytes,
                    CurrentBytes = item.SizeBytes,
                    IsNewObservation = old is null,
                    SourceKind = ClassifySource(item.OwnerSoftware),
                    ObservationInterval = interval > TimeSpan.Zero ? interval : TimeSpan.Zero,
                    ObservedSnapshots = old is null ? 1 : 2,
                    PositiveGrowthIntervals = old is null ? 0 : 1,
                    TrendGrowthBytes = item.SizeBytes - oldBytes,
                    TrendWindow = interval > TimeSpan.Zero ? interval : TimeSpan.Zero,
                    Reason = old is null
                        ? "First observation in the current snapshot model."
                        : "Grew since previous scan."
                };
            })
            .Where(f => f.GrowthBytes > 0)
            .OrderByDescending(f => f.GrowthBytes)
            .ToList();
    }

    internal static GrowthSourceKind ClassifySource(string owner)
    {
        if (string.Equals(
                owner,
                ScanSnapshotBuilder.SharedSoftwareOwner,
                StringComparison.OrdinalIgnoreCase))
            return GrowthSourceKind.SharedSoftware;
        if (!Enum.TryParse<UsageCategory>(owner, ignoreCase: true, out var category))
            return string.IsNullOrWhiteSpace(owner)
                ? GrowthSourceKind.Unknown
                : GrowthSourceKind.Software;
        return category switch
        {
            UsageCategory.UserProfiles => GrowthSourceKind.UserArea,
            UsageCategory.Mystery or UsageCategory.Other => GrowthSourceKind.Unknown,
            _ => GrowthSourceKind.SystemArea
        };
    }
}

public static class GrowthTrendAnalyzer
{
    public static IReadOnlyList<GrowthFinding> Enrich(
        IReadOnlyList<GrowthFinding> latestFindings,
        IReadOnlyList<ScanSnapshot> snapshots)
    {
        if (latestFindings.Count == 0 || snapshots.Count < 2)
            return latestFindings;
        var ordered = snapshots
            .OrderBy(snapshot => snapshot.CapturedAt)
            .GroupBy(snapshot => snapshot.CapturedAt)
            .Select(group => group.Last())
            .ToArray();
        var indexes = ordered
            .Select(snapshot => snapshot.Items
                .GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(item => item.SizeBytes).First(),
                    StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return latestFindings.Select(finding =>
        {
            var observations = new List<(DateTimeOffset CapturedAt, long Bytes)>();
            for (var index = ordered.Length - 1; index >= 0; index--)
            {
                if (indexes[index].TryGetValue(finding.Path, out var item))
                    observations.Add((ordered[index].CapturedAt, item.SizeBytes));
                else if (observations.Count > 0)
                    break;
            }
            observations.Reverse();
            if (observations.Count < 2)
                return Copy(finding, observations.Count, 0, false, 0, TimeSpan.Zero);

            var positiveIntervals = 0;
            for (var index = 1; index < observations.Count; index++)
            {
                if (observations[index].Bytes > observations[index - 1].Bytes)
                    positiveIntervals++;
            }
            var comparedIntervals = observations.Count - 1;
            var totalGrowth = observations[^1].Bytes - observations[0].Bytes;
            var sustained = observations.Count >= 3
                && positiveIntervals >= 2
                && positiveIntervals * 3 >= comparedIntervals * 2
                && totalGrowth > 0;
            var window = observations[^1].CapturedAt - observations[0].CapturedAt;
            return Copy(
                finding,
                observations.Count,
                positiveIntervals,
                sustained,
                totalGrowth,
                window > TimeSpan.Zero ? window : TimeSpan.Zero);
        }).ToArray();
    }

    private static GrowthFinding Copy(
        GrowthFinding source,
        int observedSnapshots,
        int positiveIntervals,
        bool sustained,
        long trendGrowthBytes,
        TimeSpan trendWindow) =>
        new()
        {
            Path = source.Path,
            OwnerSoftware = source.OwnerSoftware,
            PreviousBytes = source.PreviousBytes,
            CurrentBytes = source.CurrentBytes,
            IsNewObservation = source.IsNewObservation,
            SourceKind = source.SourceKind,
            ObservationInterval = source.ObservationInterval,
            ObservedSnapshots = observedSnapshots,
            PositiveGrowthIntervals = positiveIntervals,
            IsSustainedGrowth = sustained,
            TrendGrowthBytes = trendGrowthBytes,
            TrendWindow = trendWindow,
            Reason = source.Reason
        };
}
