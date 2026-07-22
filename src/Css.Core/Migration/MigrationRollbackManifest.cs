using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Software;

namespace Css.Core.Migration;

public sealed class MigrationRollbackManifestEntry
{
    public required string OriginalPath { get; init; }
    public required string PlannedDestinationPath { get; init; }
    public required string RestorePath { get; init; }
    public required string Reason { get; init; }
}

public sealed class MigrationRollbackManifest
{
    public required string Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public required string SoftwareName { get; init; }
    public required string SnapshotId { get; init; }
    public required string DestinationRoot { get; init; }
    public bool IsPlanOnly { get; init; } = true;
    public required IReadOnlyList<MigrationRollbackManifestEntry> Entries { get; init; }
    public required IReadOnlyList<string> ServicesToRestore { get; init; }
    public required IReadOnlyList<string> StartupEntriesToRestore { get; init; }
    public required IReadOnlyList<string> ScheduledTasksToRestore { get; init; }
    public required IReadOnlyList<string> MonitorPaths { get; init; }
    public required IReadOnlyList<string> VerificationSteps { get; init; }
    public required IReadOnlyList<string> RollbackSteps { get; init; }
}

public sealed class MigrationRollbackManifestCreationResult
{
    public required string ManifestPath { get; init; }
    public required string Sha256 { get; init; }
    public required MigrationRollbackManifest Manifest { get; init; }
}

public sealed class MigrationRollbackManifestVerificationResult
{
    public required bool IsValid { get; init; }
    public string? Error { get; init; }
    public string? ActualSha256 { get; init; }
    public MigrationRollbackManifest? Manifest { get; init; }
}

public static class MigrationRollbackManifestBuilder
{
    public static MigrationRollbackManifest Build(
        SoftwareProfile profile,
        MigrationPlan plan,
        string snapshotId,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(plan);

        if (string.IsNullOrWhiteSpace(snapshotId))
            throw new ArgumentException("Snapshot id is required.", nameof(snapshotId));

        var entries = BuildEntries(profile, plan.DestinationRoot);
        return new MigrationRollbackManifest
        {
            Id = now.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N"),
            CreatedAt = now,
            SoftwareName = profile.Name,
            SnapshotId = snapshotId,
            DestinationRoot = plan.DestinationRoot,
            IsPlanOnly = true,
            Entries = entries,
            ServicesToRestore = profile.Services.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            StartupEntriesToRestore = profile.StartupEntries.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ScheduledTasksToRestore = profile.ScheduledTasks.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            MonitorPaths = profile.CDriveWritePaths.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            VerificationSteps = plan.VerificationSteps,
            RollbackSteps = plan.Rollback.Steps
        };
    }

    private static IReadOnlyList<MigrationRollbackManifestEntry> BuildEntries(
        SoftwareProfile profile,
        string destinationRoot)
    {
        var paths = new List<(string Path, string Reason)>();
        Add(profile.InstallPath, "Main install path");
        foreach (var path in profile.DataPaths) Add(path, "Data path");
        foreach (var path in profile.CachePaths) Add(path, "Cache path");
        foreach (var path in profile.LogPaths) Add(path, "Log path");
        foreach (var path in profile.CDriveWritePaths) Add(path, "C drive write path");

        var entries = new List<MigrationRollbackManifestEntry>();
        foreach (var group in paths.GroupBy(item => item.Path, StringComparer.OrdinalIgnoreCase))
        {
            var original = Path.GetFullPath(group.Key);
            entries.Add(new MigrationRollbackManifestEntry
            {
                OriginalPath = original,
                PlannedDestinationPath = PlannedDestinationFor(profile, original, destinationRoot),
                RestorePath = original,
                Reason = string.Join("; ", group.Select(item => item.Reason).Distinct(StringComparer.OrdinalIgnoreCase))
            });
        }

        return entries;

        void Add(string? path, string reason)
        {
            if (!string.IsNullOrWhiteSpace(path))
                paths.Add((path, reason));
        }
    }

    private static string PlannedDestinationFor(
        SoftwareProfile profile,
        string originalPath,
        string destinationRoot)
    {
        if (!string.IsNullOrWhiteSpace(profile.InstallPath)
            && Path.GetFullPath(profile.InstallPath).Equals(originalPath, StringComparison.OrdinalIgnoreCase))
            return destinationRoot;

        var leaf = Path.GetFileName(originalPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(leaf))
            leaf = "path";

        return Path.Combine(destinationRoot, "MigratedData", SafePathName(leaf));
    }

    private static string SafePathName(string value)
    {
        var invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
        var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "path" : cleaned;
    }
}

public static class MigrationRollbackManifestStore
{
    private const int MaximumManifestBytes = 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task WriteAsync(
        MigrationRollbackManifest manifest,
        string manifestPath,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("Manifest path is required.", nameof(manifestPath));

        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var temporaryPath = manifestPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, ct);
                await stream.FlushAsync(ct);
            }

            if (new FileInfo(temporaryPath).Length > MaximumManifestBytes)
                throw new InvalidOperationException("Migration rollback manifest is too large.");
            File.Move(temporaryPath, manifestPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    public static async Task<MigrationRollbackManifest?> ReadAsync(
        string manifestPath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            return null;

        await using var stream = File.OpenRead(manifestPath);
        return await JsonSerializer.DeserializeAsync<MigrationRollbackManifest>(stream, cancellationToken: ct);
    }

    public static async Task<MigrationRollbackManifestVerificationResult> ReadVerifiedAsync(
        string manifestPath,
        string expectedSha256,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            return Invalid("Rollback manifest is missing.");
        if (!IsSha256(expectedSha256))
            return Invalid("Rollback manifest hash evidence is invalid.");

        try
        {
            var info = new FileInfo(manifestPath);
            if (info.Length <= 0 || info.Length > MaximumManifestBytes)
                return Invalid("Rollback manifest size is invalid.");

            var bytes = await File.ReadAllBytesAsync(manifestPath, ct);
            var actualBytes = SHA256.HashData(bytes);
            var expectedBytes = Convert.FromHexString(expectedSha256);
            try
            {
                var actualSha256 = Convert.ToHexString(actualBytes);
                if (!CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes))
                    return Invalid("Rollback manifest hash does not match.", actualSha256);

                var manifest = JsonSerializer.Deserialize<MigrationRollbackManifest>(bytes);
                return manifest is null
                    ? Invalid("Rollback manifest content is empty.", actualSha256)
                    : new MigrationRollbackManifestVerificationResult
                    {
                        IsValid = true,
                        ActualSha256 = actualSha256,
                        Manifest = manifest
                    };
            }
            finally
            {
                CryptographicOperations.ZeroMemory(actualBytes);
                CryptographicOperations.ZeroMemory(expectedBytes);
                CryptographicOperations.ZeroMemory(bytes);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Invalid("Rollback manifest could not be verified.");
        }
    }

    public static async Task<string> ComputeSha256Async(
        string manifestPath,
        CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            manifestPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream, ct);
        try
        {
            return Convert.ToHexString(hash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(hash);
        }
    }

    private static MigrationRollbackManifestVerificationResult Invalid(
        string error,
        string? actualSha256 = null) =>
        new()
        {
            IsValid = false,
            Error = error,
            ActualSha256 = actualSha256
        };

    private static bool IsSha256(string value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public static class MigrationRollbackManifestCreationService
{
    public static async Task<MigrationRollbackManifestCreationResult> CreateAsync(
        SoftwareProfile profile,
        MigrationPlan plan,
        string manifestPath,
        string snapshotId,
        DateTimeOffset now,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(plan);
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("Manifest path is required.", nameof(manifestPath));

        var manifest = MigrationRollbackManifestBuilder.Build(profile, plan, snapshotId, now);
        await MigrationRollbackManifestStore.WriteAsync(manifest, manifestPath, ct);
        var sha256 = await MigrationRollbackManifestStore.ComputeSha256Async(manifestPath, ct);

        return new MigrationRollbackManifestCreationResult
        {
            ManifestPath = manifestPath,
            Sha256 = sha256,
            Manifest = manifest
        };
    }
}
