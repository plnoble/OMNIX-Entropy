using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Recommendations;
using Css.Core.Software;

namespace Css.Scanner.Disk;

public sealed class DiskScanSession
{
    public required DriveScanResult Result { get; init; }
    public required string Report { get; init; }
    public required ScanSnapshot CurrentSnapshot { get; init; }
    public IReadOnlyList<Recommendation> Recommendations { get; init; } = [];
    public IReadOnlyList<GrowthFinding> GrowthFindings { get; init; } = [];
    public PersonalStorageAnalysis PersonalStorage { get; init; } = new();
}

/// <summary>
/// Builds the UI/test-friendly output for one disk scan: report, decision cards,
/// current snapshot, and optional growth findings compared to a previous snapshot.
/// </summary>
public static class DiskScanSessionBuilder
{
    public static DiskScanSession Build(
        DriveScanResult result,
        ScanSnapshot? previousSnapshot,
        DateTimeOffset capturedAt,
        IReadOnlyList<SoftwareProfile>? softwareProfiles = null,
        IReadOnlyList<ScanSnapshot>? previousSnapshots = null,
        IReadOnlyList<string>? personalStorageRoots = null,
        PersonalStorageAnalysisOptions? personalStorageOptions = null)
    {
        var current = ScanSnapshotBuilder.Build(result, capturedAt, softwareProfiles);
        IReadOnlyList<GrowthFinding> latestGrowth = previousSnapshot is null
            ? []
            : GrowthAnalyzer.Compare(previousSnapshot, current);
        IReadOnlyList<ScanSnapshot> history = previousSnapshots
            ?? (previousSnapshot is null ? [] : [previousSnapshot]);
        var growth = latestGrowth.Count == 0
            ? latestGrowth
            : GrowthTrendAnalyzer.Enrich(latestGrowth, [.. history, current]);

        return new DiskScanSession
        {
            Result = result,
            Report = RootCauseReportBuilder.Build(result),
            CurrentSnapshot = current,
            Recommendations = DiskRecommendationBuilder.Build(result),
            GrowthFindings = growth,
            PersonalStorage = PersonalStorageAnalyzer.Analyze(
                result,
                personalStorageRoots ?? PersonalStorageAnalyzer.DefaultPersonalRoots(),
                capturedAt,
                personalStorageOptions)
        };
    }

}
