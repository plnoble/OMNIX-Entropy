using System.Collections.Generic;
using Css.Core.Software;

namespace Css.Core.Migration;

public enum MigrationRiskBand
{
    Safe,
    NeedsStopAndVerify,
    NotRecommended,
    CacheOnly
}

public sealed class MigrationScore
{
    public MigrationRiskBand Band { get; init; }
    public required string Reason { get; init; }
}

public sealed class RollbackPlan
{
    public IReadOnlyList<string> Steps { get; init; } = [];
}

public sealed class MigrationPlan
{
    public required SoftwareProfile Software { get; init; }
    public required string DestinationRoot { get; init; }
    public required MigrationScore Score { get; init; }
    public bool RequiresSnapshot { get; init; }
    public IReadOnlyList<string> Steps { get; init; } = [];
    public IReadOnlyList<string> VerificationSteps { get; init; } = [];
    public required RollbackPlan Rollback { get; init; }
}

public static class MigrationPlanner
{
    public static MigrationPlan CreatePlan(SoftwareProfile software, string destinationRoot, bool snapshotAvailable)
    {
        var score = Score(software);
        var sourcePaths = new List<string>();
        if (!string.IsNullOrWhiteSpace(software.InstallPath)) sourcePaths.Add(software.InstallPath);
        sourcePaths.AddRange(software.DataPaths);
        sourcePaths.AddRange(software.CachePaths);
        sourcePaths.AddRange(software.LogPaths);

        var steps = new List<string>();
        if (software.Services.Count > 0) steps.Add("Stop related services before moving files.");
        if (software.StartupEntries.Count > 0) steps.Add("Disable related startup entries during migration.");
        foreach (var path in sourcePaths) steps.Add("Move " + path + " to " + destinationRoot + ".");
        steps.Add("Create redirect or update configuration for moved paths.");

        var verification = new List<string>
        {
            "Launch the software and confirm it starts from the new location.",
            "Scan original C: paths to confirm no new writes happened after migration."
        };

        var rollbackSteps = new List<string>
        {
            "Stop the software and related background tasks.",
            "Remove redirects created during migration.",
            "Move files back to their original paths from the migration manifest."
        };

        return new MigrationPlan
        {
            Software = software,
            DestinationRoot = destinationRoot,
            Score = score,
            RequiresSnapshot = true,
            Steps = steps,
            VerificationSteps = verification,
            Rollback = new RollbackPlan { Steps = rollbackSteps }
        };
    }

    private static MigrationScore Score(SoftwareProfile software)
    {
        if (software.Category is SoftwareCategory.SystemTool)
            return new MigrationScore { Band = MigrationRiskBand.NotRecommended, Reason = "System tools often bind services, drivers, or machine-wide paths." };

        if (software.Services.Count > 0 || software.ScheduledTasks.Count > 0 || software.StartupEntries.Count > 0)
            return new MigrationScore { Band = MigrationRiskBand.NeedsStopAndVerify, Reason = "Background components must be stopped and verified after migration." };

        if (string.IsNullOrWhiteSpace(software.InstallPath) && software.CachePaths.Count > 0)
            return new MigrationScore { Band = MigrationRiskBand.CacheOnly, Reason = "Only cache/model/download paths are known, so avoid moving the main app." };

        return new MigrationScore { Band = MigrationRiskBand.Safe, Reason = "No background components were found in the software profile." };
    }
}
