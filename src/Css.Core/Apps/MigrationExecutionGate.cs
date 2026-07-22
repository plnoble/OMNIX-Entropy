using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class MigrationExecutionReadiness
{
    public bool FeatureEnabled { get; init; }
    public string? SnapshotId { get; init; }
    public bool UserConfirmedPlan { get; init; }
    public bool UserConfirmedAppsClosed { get; init; }
    public string? RollbackManifestPath { get; init; }
    public string? RollbackManifestSha256 { get; init; }
    public string? SnapshotEvidencePath { get; init; }
    public string? SnapshotEvidenceSha256 { get; init; }
    public long? DestinationAvailableBytes { get; init; }
    public bool UserConfirmedPostMigrationMonitoring { get; init; }
}

public sealed class MigrationExecutionGateResult
{
    public required bool CanRequestExecution { get; init; }
    public required string PrimaryButtonText { get; init; }
    public required IReadOnlyList<string> BlockingReasons { get; init; }
    public required long RequiredBytes { get; init; }
    public OperationDescriptor? Operation { get; init; }
}

public static class MigrationExecutionGate
{
    public static MigrationExecutionGateResult Evaluate(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationExecutionReadiness readiness,
        Func<string, bool> rollbackManifestExists)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(readiness);
        ArgumentNullException.ThrowIfNull(rollbackManifestExists);

        var requiredBytes = EstimateRequiredBytes(profile);
        if (!readiness.FeatureEnabled)
        {
            return Blocked(
                "Prepare migration evidence",
                ["Migration request is not enabled for this plan."],
                requiredBytes);
        }

        var blockers = new List<string>();

        if (profile.Category == SoftwareCategory.SystemTool || plan.Score.Band == MigrationRiskBand.NotRecommended)
            blockers.Add("System tools or not-recommended apps cannot be migrated by OMNIX-Entropy.");

        if (IsOnDrive(profile.InstallPath, "D"))
            blockers.Add("This app is already installed on D drive; migration is not needed.");

        if (string.IsNullOrWhiteSpace(readiness.SnapshotId))
            blockers.Add("A snapshot is required before migration.");

        if (string.IsNullOrWhiteSpace(readiness.RollbackManifestPath))
        {
            blockers.Add("A rollback manifest path is required before migration.");
        }
        else if (!ExistsSafely(readiness.RollbackManifestPath, rollbackManifestExists))
        {
            blockers.Add("Rollback manifest file is missing or unreadable.");
        }

        if (!IsSha256(readiness.RollbackManifestSha256))
            blockers.Add("Rollback manifest SHA-256 evidence is required before migration.");

        if (string.IsNullOrWhiteSpace(readiness.SnapshotEvidencePath))
        {
            blockers.Add("A migration snapshot evidence path is required before migration.");
        }
        else if (!ExistsSafely(readiness.SnapshotEvidencePath, rollbackManifestExists))
        {
            blockers.Add("Migration snapshot evidence is missing or unreadable.");
        }

        if (!IsSha256(readiness.SnapshotEvidenceSha256))
            blockers.Add("Migration snapshot SHA-256 evidence is required before migration.");

        if (readiness.DestinationAvailableBytes is null)
        {
            blockers.Add("Destination free space has not been checked.");
        }
        else if (readiness.DestinationAvailableBytes.Value < requiredBytes)
        {
            blockers.Add("Destination free space is too low for this migration plan.");
        }

        if (blockers.Count > 0)
            return Blocked("Finish migration preflight", blockers, requiredBytes);

        return new MigrationExecutionGateResult
        {
            CanRequestExecution = true,
            PrimaryButtonText = "Request migration",
            BlockingReasons = [],
            RequiredBytes = requiredBytes,
            Operation = CreateOperation(profile, plan, readiness, requiredBytes)
        };
    }

    private static MigrationExecutionGateResult Blocked(
        string primaryButtonText,
        IReadOnlyList<string> blockers,
        long requiredBytes) =>
        new()
        {
            CanRequestExecution = false,
            PrimaryButtonText = primaryButtonText,
            BlockingReasons = blockers,
            RequiredBytes = requiredBytes,
            Operation = null
        };

    private static OperationDescriptor CreateOperation(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationExecutionReadiness readiness,
        long requiredBytes)
    {
        var affectedPaths = CollectAffectedPaths(profile);
        return new OperationDescriptor
        {
            Kind = "migration.execute",
            Title = profile.Name + " migration",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = readiness.SnapshotId,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "Migration plan reviewed; snapshot, rollback manifest, free-space check, app-close confirmation, and monitoring are required.",
            EstimatedImpactBytes = requiredBytes,
            ConfirmationText = "Request migration for " + profile.Name + "?",
            AffectedPaths = affectedPaths,
            AffectedServices = profile.Services,
            Arguments = new Dictionary<string, object?>
            {
                ["destinationRoot"] = plan.DestinationRoot,
                ["rollbackManifestPath"] = readiness.RollbackManifestPath,
                ["rollbackManifestSha256"] = readiness.RollbackManifestSha256,
                ["snapshotEvidencePath"] = readiness.SnapshotEvidencePath,
                ["snapshotEvidenceSha256"] = readiness.SnapshotEvidenceSha256,
                ["affectedProcesses"] = profile.RunningProcesses.ToArray(),
                ["scheduledTasks"] = profile.ScheduledTasks.ToArray(),
                ["startupEntries"] = profile.StartupEntries.ToArray(),
                ["monitorPaths"] = profile.CDriveWritePaths
            }
        };
    }

    internal static long EstimateRequiredBytes(SoftwareProfile profile)
    {
        var total = profile.InstalledSizeBytes + profile.DataSizeBytes + profile.CacheSizeBytes;
        return total > 0 ? total : 1;
    }

    internal static IReadOnlyList<string> CollectAffectedPaths(SoftwareProfile profile)
    {
        var paths = new List<string>();
        Add(profile.InstallPath);
        foreach (var path in profile.DataPaths) Add(path);
        foreach (var path in profile.CachePaths) Add(path);
        foreach (var path in profile.LogPaths) Add(path);
        foreach (var path in profile.CDriveWritePaths) Add(path);

        return paths
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        void Add(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                paths.Add(path);
        }
    }

    private static bool ExistsSafely(string path, Func<string, bool> exists)
    {
        try
        {
            return exists(path);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsOnDrive(string? path, string driveLetter) =>
        !string.IsNullOrWhiteSpace(path) &&
        path.StartsWith(driveLetter + ":\\", StringComparison.OrdinalIgnoreCase);

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}
