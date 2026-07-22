using System.Text.Json;
using Css.Core.Operations;

namespace Css.Core.Migration;

public enum MigrationExecutionStatus
{
    Completed,
    Refused,
    FailedRolledBack,
    FailedRollbackIncomplete
}

public sealed class MigrationExecutionResult
{
    public required MigrationExecutionStatus Status { get; init; }
    public required string Summary { get; init; }
    public int MovedPathCount { get; init; }
    public bool RollbackAttempted { get; init; }
    public bool RollbackSucceeded { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public MigrationMonitoringRecord? MonitoringRecord { get; init; }
}

public sealed record MigrationActivityRequest
{
    public required IReadOnlyList<string> ProcessNames { get; init; }
    public required IReadOnlyList<string> ServiceNames { get; init; }
    public required IReadOnlyList<string> ScheduledTasks { get; init; }
}

public interface IMigrationActivityProbe
{
    Task<IReadOnlyList<string>> FindActiveAsync(
        MigrationActivityRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record MigrationPathObservation
{
    public required string Path { get; init; }
    public bool Exists { get; init; }
    public bool IsDirectory { get; init; }
    public bool IsRedirect { get; init; }
    public string? RedirectTarget { get; init; }
    public long ObservedBytes { get; init; }
    public DateTimeOffset? LastWriteUtc { get; init; }
}

public sealed record MigrationMoveResult
{
    public required string OriginalPath { get; init; }
    public required string DestinationPath { get; init; }
    public required bool RedirectCreated { get; init; }
}

public interface IMigrationPathObserver
{
    Task<MigrationPathObservation> ObserveAsync(
        string path,
        CancellationToken cancellationToken = default);
}

public interface IMigrationPathAdapter : IMigrationPathObserver
{
    Task<MigrationMoveResult> MoveAndRedirectAsync(
        MigrationRollbackManifestEntry entry,
        CancellationToken cancellationToken = default);

    Task RollbackAsync(
        MigrationRollbackManifestEntry entry,
        CancellationToken cancellationToken = default);
}

public interface IMigrationPathPolicy
{
    string? Validate(
        MigrationRollbackManifest manifest,
        MigrationRollbackManifestEntry entry);
}

public sealed record MigrationMonitoringPath
{
    public required string OriginalPath { get; init; }
    public required string ExpectedRedirectTarget { get; init; }
}

public sealed class MigrationMonitoringRecord
{
    public required string Id { get; init; }
    public required string SoftwareName { get; init; }
    public required string SnapshotId { get; init; }
    public required string RollbackManifestPath { get; init; }
    public required string RollbackManifestSha256 { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public required IReadOnlyList<MigrationMonitoringPath> Paths { get; init; }
}

public interface IMigrationMonitoringStore
{
    Task SaveAsync(
        MigrationMonitoringRecord record,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MigrationMonitoringRecord>> LoadAsync(
        CancellationToken cancellationToken = default);
}

public sealed class MigrationOperationHandler
{
    private static readonly TimeSpan MaximumManifestAge = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MaximumFutureSkew = TimeSpan.FromMinutes(1);

    private readonly IMigrationActivityProbe _activityProbe;
    private readonly IMigrationPathAdapter _paths;
    private readonly IMigrationPathPolicy _pathPolicy;
    private readonly IMigrationSnapshotSourceReader _snapshotSourceReader;
    private readonly IMigrationMonitoringStore _monitoringStore;
    private readonly Func<DateTimeOffset> _clock;

    public MigrationOperationHandler(
        IMigrationActivityProbe activityProbe,
        IMigrationPathAdapter paths,
        IMigrationPathPolicy pathPolicy,
        IMigrationSnapshotSourceReader snapshotSourceReader,
        IMigrationMonitoringStore monitoringStore,
        Func<DateTimeOffset>? clock = null)
    {
        _activityProbe = activityProbe ?? throw new ArgumentNullException(nameof(activityProbe));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _pathPolicy = pathPolicy ?? throw new ArgumentNullException(nameof(pathPolicy));
        _snapshotSourceReader = snapshotSourceReader
            ?? throw new ArgumentNullException(nameof(snapshotSourceReader));
        _monitoringStore = monitoringStore ?? throw new ArgumentNullException(nameof(monitoringStore));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        var basicError = ValidateDescriptor(descriptor);
        if (basicError is not null)
            return Refused(basicError);

        var manifestPath = StringArgument(descriptor, "rollbackManifestPath")!;
        var expectedSha256 = StringArgument(descriptor, "rollbackManifestSha256")!;
        var verification = await MigrationRollbackManifestStore.ReadVerifiedAsync(
            manifestPath,
            expectedSha256,
            cancellationToken);
        if (!verification.IsValid || verification.Manifest is null)
            return Refused(verification.Error ?? "Rollback manifest verification failed.");

        var manifest = verification.Manifest;
        var manifestError = ValidateManifest(descriptor, manifest, _clock());
        if (manifestError is not null)
            return Refused(manifestError);

        var snapshotVerification = await MigrationSnapshotEvidenceStore.ReadVerifiedAsync(
            StringArgument(descriptor, "snapshotEvidencePath")!,
            StringArgument(descriptor, "snapshotEvidenceSha256")!,
            cancellationToken);
        if (!snapshotVerification.IsValid || snapshotVerification.Evidence is null)
            return Refused(snapshotVerification.Error ?? "Migration snapshot verification failed.");
        var snapshotError = MigrationSnapshotEvidenceService.ValidateForOperation(
            snapshotVerification.Evidence,
            manifest,
            descriptor,
            _clock());
        if (snapshotError is not null)
            return Refused(snapshotError);

        var active = await _activityProbe.FindActiveAsync(
            new MigrationActivityRequest
            {
                ProcessNames = StringListArgument(descriptor, "affectedProcesses"),
                ServiceNames = descriptor.AffectedServices,
                ScheduledTasks = StringListArgument(descriptor, "scheduledTasks")
            },
            cancellationToken);
        if (active.Count > 0)
            return Refused("Related app components are still active: " + string.Join(", ", active.Take(8)));

        var currentSourceError = await ValidateCurrentSourcesAsync(
            snapshotVerification.Evidence,
            manifest,
            cancellationToken);
        if (currentSourceError is not null)
            return Refused(currentSourceError);

        foreach (var entry in manifest.Entries)
        {
            var policyError = _pathPolicy.Validate(manifest, entry);
            if (policyError is not null)
                return Refused(policyError);

            var source = await _paths.ObserveAsync(entry.OriginalPath, cancellationToken);
            if (!source.Exists || !source.IsDirectory || source.IsRedirect)
                return Refused("A migration source is missing, is not a directory, or is already redirected.");
            var destination = await _paths.ObserveAsync(entry.PlannedDestinationPath, cancellationToken);
            if (destination.Exists)
                return Refused("A migration destination already exists.");
        }

        var attempted = new List<MigrationRollbackManifestEntry>();
        var moved = new List<MigrationMoveResult>();
        try
        {
            foreach (var entry in manifest.Entries)
            {
                attempted.Add(entry);
                var result = await _paths.MoveAndRedirectAsync(entry, cancellationToken);
                if (!result.RedirectCreated
                    || !PathsEqual(result.OriginalPath, entry.OriginalPath)
                    || !PathsEqual(result.DestinationPath, entry.PlannedDestinationPath))
                {
                    throw new InvalidOperationException("Migration adapter did not create the expected redirect.");
                }
                moved.Add(result);
            }

            var record = new MigrationMonitoringRecord
            {
                Id = "migration-monitor-" + Guid.NewGuid().ToString("N"),
                SoftwareName = manifest.SoftwareName,
                SnapshotId = manifest.SnapshotId,
                RollbackManifestPath = manifestPath,
                RollbackManifestSha256 = expectedSha256.ToUpperInvariant(),
                CreatedAtUtc = _clock().ToUniversalTime(),
                Paths = moved.Select(result => new MigrationMonitoringPath
                {
                    OriginalPath = result.OriginalPath,
                    ExpectedRedirectTarget = result.DestinationPath
                }).ToArray()
            };
            await _monitoringStore.SaveAsync(record, cancellationToken);

            return OperationResult.Ok(
                $"Migration completed for {manifest.SoftwareName}; {moved.Count} paths are redirected and monitored.",
                new MigrationExecutionResult
                {
                    Status = MigrationExecutionStatus.Completed,
                    Summary = "Migration completed and original paths are being monitored.",
                    MovedPathCount = moved.Count,
                    RollbackSucceeded = true,
                    MonitoringRecord = record
                });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var rollback = await RollbackAsync(attempted, CancellationToken.None);
            if (!rollback.Success)
            {
                var result = new MigrationExecutionResult
                {
                    Status = MigrationExecutionStatus.FailedRollbackIncomplete,
                    Summary = "Migration was canceled and rollback is incomplete.",
                    MovedPathCount = moved.Count,
                    RollbackAttempted = attempted.Count > 0,
                    RollbackSucceeded = false,
                    Errors = rollback.Errors
                };
                return new OperationResult
                {
                    Success = false,
                    Error = result.Summary,
                    Payload = result
                };
            }
            throw;
        }
        catch (Exception exception)
        {
            var rollback = await RollbackAsync(attempted, CancellationToken.None);
            var status = rollback.Success
                ? MigrationExecutionStatus.FailedRolledBack
                : MigrationExecutionStatus.FailedRollbackIncomplete;
            var result = new MigrationExecutionResult
            {
                Status = status,
                Summary = rollback.Success
                    ? "Migration failed; moved paths were restored."
                    : "Migration failed and rollback is incomplete.",
                MovedPathCount = moved.Count,
                RollbackAttempted = attempted.Count > 0,
                RollbackSucceeded = rollback.Success,
                Errors = [exception.Message, .. rollback.Errors]
            };
            return new OperationResult
            {
                Success = false,
                Error = result.Summary,
                Payload = result
            };
        }
    }

    private async Task<(bool Success, IReadOnlyList<string> Errors)> RollbackAsync(
        IReadOnlyList<MigrationRollbackManifestEntry> attempted,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        foreach (var entry in attempted.Reverse())
        {
            try
            {
                await _paths.RollbackAsync(entry, cancellationToken);
            }
            catch
            {
                errors.Add("A migrated path could not be restored.");
            }
        }
        return (errors.Count == 0, errors);
    }

    private string? ValidateManifest(
        OperationDescriptor descriptor,
        MigrationRollbackManifest manifest,
        DateTimeOffset now)
    {
        if (!manifest.IsPlanOnly
            || string.IsNullOrWhiteSpace(manifest.Id)
            || string.IsNullOrWhiteSpace(manifest.SoftwareName)
            || manifest.Entries.Count is < 1 or > 32)
            return "Rollback manifest content is invalid.";
        if (manifest.CreatedAt.ToUniversalTime() < now.ToUniversalTime() - MaximumManifestAge
            || manifest.CreatedAt.ToUniversalTime() > now.ToUniversalTime() + MaximumFutureSkew)
            return "Rollback manifest is stale or future-dated.";
        if (!string.Equals(manifest.SnapshotId, descriptor.SnapshotId, StringComparison.Ordinal))
            return "Rollback manifest snapshot does not match the operation.";
        if (!PathsEqual(manifest.DestinationRoot, StringArgument(descriptor, "destinationRoot")!))
            return "Rollback manifest destination does not match the operation.";

        var originals = NormalizeDistinct(manifest.Entries.Select(entry => entry.OriginalPath));
        var destinations = NormalizeDistinct(manifest.Entries.Select(entry => entry.PlannedDestinationPath));
        var affected = NormalizeDistinct(descriptor.AffectedPaths);
        if (originals is null || destinations is null || affected is null
            || originals.Count != manifest.Entries.Count
            || destinations.Count != manifest.Entries.Count
            || !originals.SetEquals(affected))
            return "Rollback manifest paths do not exactly match the operation.";
        foreach (var entry in manifest.Entries)
        {
            if (PathsEqual(entry.OriginalPath, entry.PlannedDestinationPath)
                || IsRoot(entry.OriginalPath)
                || IsRoot(entry.PlannedDestinationPath))
                return "Rollback manifest contains an unsafe root or overlapping path.";
        }
        return null;
    }

    private async Task<string?> ValidateCurrentSourcesAsync(
        MigrationSnapshotEvidence evidence,
        MigrationRollbackManifest manifest,
        CancellationToken cancellationToken)
    {
        var expected = evidence.Sources.ToDictionary(
            source => Path.GetFullPath(source.Path),
            StringComparer.OrdinalIgnoreCase);
        foreach (var entry in manifest.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!expected.TryGetValue(Path.GetFullPath(entry.OriginalPath), out var snapshot))
                return "Migration source is not represented in the verified snapshot.";

            MigrationSnapshotSourceEvidence current;
            try
            {
                current = await _snapshotSourceReader.ObserveAsync(
                    entry.OriginalPath,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return "Migration source could not be rechecked after confirmation.";
            }

            if (!current.Exists
                || !current.IsDirectory
                || current.IsRedirect
                || !PathsEqual(current.Path, snapshot.Path)
                || current.ObservedBytes != snapshot.ObservedBytes
                || !SameTimestamp(current.LastWriteUtc, snapshot.LastWriteUtc))
            {
                return "Migration source changed after the snapshot; create a new plan before moving it.";
            }
        }

        return null;
    }

    private static string? ValidateDescriptor(OperationDescriptor descriptor)
    {
        if (!string.Equals(descriptor.Kind, "migration.execute", StringComparison.Ordinal)
            || descriptor.Source != OperationSource.Manual
            || descriptor.Risk != RiskLevel.High
            || !descriptor.IsDestructive
            || !descriptor.RequiresElevation
            || !descriptor.RequiresSnapshot
            || !descriptor.RollbackRequired
            || !descriptor.ConfirmationAccepted
            || string.IsNullOrWhiteSpace(descriptor.SnapshotId)
            || descriptor.AffectedPaths.Count == 0)
            return "Migration operation did not pass the execution contract.";
        if (StringArgument(descriptor, "destinationRoot") is null
            || StringArgument(descriptor, "rollbackManifestPath") is null
            || !IsSha256(StringArgument(descriptor, "rollbackManifestSha256"))
            || StringArgument(descriptor, "snapshotEvidencePath") is null
            || !IsSha256(StringArgument(descriptor, "snapshotEvidenceSha256")))
            return "Migration operation evidence is incomplete.";
        return null;
    }

    private static bool SameTimestamp(DateTimeOffset? left, DateTimeOffset? right) =>
        left?.ToUniversalTime().Ticks == right?.ToUniversalTime().Ticks;

    private static OperationResult Refused(string error) =>
        new()
        {
            Success = false,
            Error = error,
            Payload = new MigrationExecutionResult
            {
                Status = MigrationExecutionStatus.Refused,
                Summary = error,
                RollbackSucceeded = true
            }
        };

    private static string? StringArgument(OperationDescriptor descriptor, string key) =>
        descriptor.Arguments.TryGetValue(key, out var value)
            && value is string text
            && !string.IsNullOrWhiteSpace(text)
                ? text
                : null;

    private static IReadOnlyList<string> StringListArgument(
        OperationDescriptor descriptor,
        string key)
    {
        if (!descriptor.Arguments.TryGetValue(key, out var value) || value is null)
            return [];
        return value switch
        {
            string[] array => array.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray(),
            IReadOnlyList<string> list => list.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray(),
            _ => []
        };
    }

    private static HashSet<string>? NormalizeDistinct(IEnumerable<string> paths)
    {
        try
        {
            return paths.Select(Path.GetFullPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return null;
        }
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsRoot(string path)
    {
        try
        {
            var full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            var root = Path.GetPathRoot(full)?.TrimEnd(Path.DirectorySeparatorChar);
            return string.Equals(full, root, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true;
        }
    }

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed class JsonMigrationMonitoringStore(string root) : IMigrationMonitoringStore
{
    private const int MaximumRecordBytes = 1024 * 1024;
    private const int MaximumRecords = 256;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _root = Path.GetFullPath(
        string.IsNullOrWhiteSpace(root)
            ? throw new ArgumentException("Monitoring root is required.", nameof(root))
            : root);

    public async Task SaveAsync(
        MigrationMonitoringRecord record,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        Directory.CreateDirectory(_root);
        var target = Path.Combine(_root, record.Id + ".json");
        var temporary = target + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var stream = new FileStream(
                             temporary,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             4096,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, record, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            File.Move(temporary, target, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    public async Task<IReadOnlyList<MigrationMonitoringRecord>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_root))
            return [];

        var files = Directory.EnumerateFiles(
                _root,
                "migration-monitor-*.json",
                SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Take(MaximumRecords + 1)
            .ToArray();
        if (files.Length > MaximumRecords)
            throw new InvalidOperationException("Migration monitoring record capacity was exceeded.");

        var records = new List<MigrationMonitoringRecord>();
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var info = new FileInfo(file);
            if (info.Length <= 0 || info.Length > MaximumRecordBytes
                || info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidOperationException("A migration monitoring record is unsafe or invalid.");
            await using var stream = new FileStream(
                file,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var record = await JsonSerializer.DeserializeAsync<MigrationMonitoringRecord>(
                stream,
                cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("A migration monitoring record is empty.");
            if (string.IsNullOrWhiteSpace(record.Id)
                || string.IsNullOrWhiteSpace(record.SoftwareName)
                || record.Paths.Count is < 1 or > 32)
                throw new InvalidOperationException("A migration monitoring record is incomplete.");
            records.Add(record);
        }
        return records;
    }
}

public sealed class WindowsMigrationPathPolicy : IMigrationPathPolicy
{
    private static readonly string[] AllowedDestinationRoots =
    [
        @"D:\Software",
        @"D:\Game",
        @"D:\Agent",
        @"D:\Development"
    ];

    private static readonly string[] ProtectedSourceRoots =
    [
        @"C:\Windows",
        @"C:\Program Files",
        @"C:\Program Files (x86)",
        @"C:\ProgramData",
        @"C:\Recovery",
        @"C:\$Recycle.Bin",
        @"C:\System Volume Information"
    ];

    public string? Validate(
        MigrationRollbackManifest manifest,
        MigrationRollbackManifestEntry entry)
    {
        try
        {
            var original = Normalize(entry.OriginalPath);
            var destination = Normalize(entry.PlannedDestinationPath);
            var destinationRoot = Normalize(manifest.DestinationRoot);
            var restore = Normalize(entry.RestorePath);

            if (!IsUnder(original, @"C:\") || !IsUnder(destination, @"D:\"))
                return "Migration currently permits only C drive sources and D drive destinations.";
            if (ProtectedSourceRoots.Any(root => IsUnder(original, root)))
                return "Protected Windows or machine-wide paths cannot be migrated by file movement.";
            if (!AllowedDestinationRoots.Any(root => IsUnder(destinationRoot, root)))
                return "Migration destination is outside the OMNIX D drive allowlist.";
            if (!IsUnder(destination, destinationRoot))
                return "Migration entry destination is outside the verified destination root.";
            if (!string.Equals(original, restore, StringComparison.OrdinalIgnoreCase))
                return "Migration restore path does not match the original path.";
            if (IsUnder(original, destination) || IsUnder(destination, original))
                return "Migration source and destination overlap.";
            return null;
        }
        catch
        {
            return "Migration path policy could not validate the requested paths.";
        }
    }

    private static string Normalize(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool IsUnder(string path, string root)
    {
        var normalizedRoot = Normalize(root);
        return string.Equals(path, normalizedRoot, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}

public enum MigrationClosureFindingKind
{
    RedirectHealthy,
    OriginalPathMissing,
    RedirectTargetChanged,
    OriginalWriteReturned
}

public sealed class MigrationClosureFinding
{
    public required MigrationClosureFindingKind Kind { get; init; }
    public required string OriginalPath { get; init; }
    public required string Summary { get; init; }
    public string SoftwareName { get; init; } = string.Empty;
    public string MonitoringRecordId { get; init; } = string.Empty;
    public DateTimeOffset MonitoringStartedAtUtc { get; init; }
    public bool NeedsAttention => Kind != MigrationClosureFindingKind.RedirectHealthy;
}

public static class MigrationClosureMonitor
{
    public static async Task<IReadOnlyList<MigrationClosureFinding>> ScanAllAsync(
        IMigrationMonitoringStore store,
        IMigrationPathObserver paths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        var records = await store.LoadAsync(cancellationToken);
        var findings = new List<MigrationClosureFinding>();
        foreach (var record in records)
        {
            ValidateForObservation(record);
            findings.AddRange(await ScanAsync(record, paths, cancellationToken));
        }
        return findings;
    }

    public static async Task<IReadOnlyList<MigrationClosureFinding>> ScanLatestAsync(
        IMigrationMonitoringStore store,
        IMigrationPathObserver paths,
        int maximumSoftwareCount = 64,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(paths);
        if (maximumSoftwareCount is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(maximumSoftwareCount));

        var records = await store.LoadAsync(cancellationToken);
        var latestRecords = records
            .Where(record => !string.IsNullOrWhiteSpace(record.SoftwareName))
            .GroupBy(record => record.SoftwareName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(record => record.CreatedAtUtc)
                .ThenByDescending(record => record.Id, StringComparer.Ordinal)
                .First())
            .OrderByDescending(record => record.CreatedAtUtc)
            .ThenBy(record => record.SoftwareName, StringComparer.OrdinalIgnoreCase)
            .Take(maximumSoftwareCount)
            .ToArray();

        var findings = new List<MigrationClosureFinding>();
        foreach (var record in latestRecords)
        {
            ValidateForObservation(record);
            findings.AddRange(await ScanAsync(record, paths, cancellationToken));
        }
        return findings;
    }

    public static async Task<IReadOnlyList<MigrationClosureFinding>> ScanAsync(
        MigrationMonitoringRecord record,
        IMigrationPathObserver paths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(paths);
        var findings = new List<MigrationClosureFinding>();
        foreach (var expected in record.Paths)
        {
            var actual = await paths.ObserveAsync(expected.OriginalPath, cancellationToken);
            if (!actual.Exists)
            {
                findings.Add(Finding(
                    record,
                    MigrationClosureFindingKind.OriginalPathMissing,
                    expected.OriginalPath,
                    "Original path redirect is missing; verify the app before using it."));
            }
            else if (!actual.IsRedirect)
            {
                findings.Add(Finding(
                    record,
                    MigrationClosureFindingKind.OriginalWriteReturned,
                    expected.OriginalPath,
                    "The app created a real folder at the original location; migration is not closed."));
            }
            else if (!PathsEqual(actual.RedirectTarget, expected.ExpectedRedirectTarget))
            {
                findings.Add(Finding(
                    record,
                    MigrationClosureFindingKind.RedirectTargetChanged,
                    expected.OriginalPath,
                    "Original path points somewhere unexpected; stop and review the migration."));
            }
            else
            {
                findings.Add(Finding(
                    record,
                    MigrationClosureFindingKind.RedirectHealthy,
                    expected.OriginalPath,
                    "Original path still redirects to the verified destination."));
            }
        }
        return findings;
    }

    private static void ValidateForObservation(MigrationMonitoringRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Id)
            || string.IsNullOrWhiteSpace(record.SoftwareName)
            || record.Paths is null
            || record.Paths.Count is < 1 or > 32)
            throw new InvalidOperationException("Migration monitoring record is incomplete.");

        var originals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var expected in record.Paths)
        {
            if (!TryNormalizeLocalDrivePath(expected.OriginalPath, "C", out var original)
                || !TryNormalizeLocalDrivePath(expected.ExpectedRedirectTarget, "D", out _)
                || !originals.Add(original))
                throw new InvalidOperationException("Migration monitoring record contains an unsafe path.");
        }
    }

    private static bool TryNormalizeLocalDrivePath(
        string? path,
        string driveLetter,
        out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(path)
            || path.Length > 1024
            || path.StartsWith(@"\\", StringComparison.Ordinal))
            return false;

        try
        {
            var full = Path.GetFullPath(path);
            var expectedRoot = driveLetter + @":\";
            var actualRoot = Path.GetPathRoot(full);
            if (!string.Equals(actualRoot, expectedRoot, StringComparison.OrdinalIgnoreCase))
                return false;
            normalized = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return !string.Equals(
                normalized,
                expectedRoot.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static MigrationClosureFinding Finding(
        MigrationMonitoringRecord record,
        MigrationClosureFindingKind kind,
        string path,
        string summary) =>
        new()
        {
            Kind = kind,
            OriginalPath = path,
            Summary = summary,
            SoftwareName = record.SoftwareName,
            MonitoringRecordId = record.Id,
            MonitoringStartedAtUtc = record.CreatedAtUtc
        };

    private static bool PathsEqual(string? left, string right)
    {
        if (string.IsNullOrWhiteSpace(left))
            return false;
        try
        {
            return string.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
