using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Css.Core.Operations;

namespace Css.Core.Migration;

public sealed record MigrationSnapshotSourceEvidence
{
    public required string Path { get; init; }
    public bool Exists { get; init; }
    public bool IsDirectory { get; init; }
    public bool IsRedirect { get; init; }
    public long ObservedBytes { get; init; }
    public DateTimeOffset? LastWriteUtc { get; init; }
}

public sealed record MigrationSnapshotEvidence
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public required string SnapshotId { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required string SoftwareName { get; init; }
    public required string RollbackManifestPath { get; init; }
    public required string RollbackManifestSha256 { get; init; }
    public required IReadOnlyList<MigrationSnapshotSourceEvidence> Sources { get; init; }
}

public sealed record MigrationSnapshotEvidenceCreationResult
{
    public required string EvidencePath { get; init; }
    public required string Sha256 { get; init; }
    public required MigrationSnapshotEvidence Evidence { get; init; }
}

public sealed record MigrationSnapshotEvidenceVerificationResult
{
    public required bool IsValid { get; init; }
    public string? Error { get; init; }
    public string? ActualSha256 { get; init; }
    public MigrationSnapshotEvidence? Evidence { get; init; }
}

public interface IMigrationSnapshotSourceReader
{
    Task<MigrationSnapshotSourceEvidence> ObserveAsync(
        string path,
        CancellationToken cancellationToken = default);
}

public static class MigrationSnapshotEvidenceService
{
    public static async Task<MigrationSnapshotEvidenceCreationResult> CreateAsync(
        MigrationRollbackManifest manifest,
        string rollbackManifestPath,
        string rollbackManifestSha256,
        string evidencePath,
        IMigrationSnapshotSourceReader sourceReader,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(sourceReader);
        if (manifest.Entries.Count is < 1 or > 32)
            throw new InvalidOperationException("Migration snapshot source count is invalid.");
        if (!IsSha256(rollbackManifestSha256))
            throw new ArgumentException("Rollback manifest hash is invalid.", nameof(rollbackManifestSha256));

        var sources = new List<MigrationSnapshotSourceEvidence>();
        foreach (var entry in manifest.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var observed = await sourceReader.ObserveAsync(entry.OriginalPath, cancellationToken);
            if (!observed.Exists || !observed.IsDirectory || observed.IsRedirect)
                throw new InvalidOperationException("A migration snapshot source is unavailable or redirected.");
            if (!PathsEqual(observed.Path, entry.OriginalPath) || observed.ObservedBytes < 0)
                throw new InvalidOperationException("A migration snapshot observation does not match its source.");
            sources.Add(observed with { Path = Path.GetFullPath(observed.Path) });
        }

        var evidence = new MigrationSnapshotEvidence
        {
            SnapshotId = manifest.SnapshotId,
            CreatedAtUtc = now.ToUniversalTime(),
            SoftwareName = manifest.SoftwareName,
            RollbackManifestPath = Path.GetFullPath(rollbackManifestPath),
            RollbackManifestSha256 = rollbackManifestSha256.ToUpperInvariant(),
            Sources = sources
        };
        await MigrationSnapshotEvidenceStore.WriteAsync(
            evidence,
            evidencePath,
            cancellationToken);
        var sha256 = await MigrationSnapshotEvidenceStore.ComputeSha256Async(
            evidencePath,
            cancellationToken);
        return new MigrationSnapshotEvidenceCreationResult
        {
            EvidencePath = Path.GetFullPath(evidencePath),
            Sha256 = sha256,
            Evidence = evidence
        };
    }

    public static string? ValidateForOperation(
        MigrationSnapshotEvidence evidence,
        MigrationRollbackManifest manifest,
        OperationDescriptor descriptor,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (evidence.SchemaVersion != MigrationSnapshotEvidence.CurrentSchemaVersion
            || evidence.Sources.Count is < 1 or > 32
            || !string.Equals(evidence.SnapshotId, descriptor.SnapshotId, StringComparison.Ordinal)
            || !string.Equals(evidence.SnapshotId, manifest.SnapshotId, StringComparison.Ordinal)
            || !string.Equals(evidence.SoftwareName, manifest.SoftwareName, StringComparison.Ordinal))
        {
            return "Migration snapshot identity does not match the operation.";
        }

        var age = now.ToUniversalTime() - evidence.CreatedAtUtc.ToUniversalTime();
        if (age > TimeSpan.FromMinutes(30) || age < -TimeSpan.FromMinutes(1))
            return "Migration snapshot evidence is stale or future-dated.";
        if (!PathsEqual(
                evidence.RollbackManifestPath,
                StringArgument(descriptor, "rollbackManifestPath"))
            || !HashesEqual(
                evidence.RollbackManifestSha256,
                StringArgument(descriptor, "rollbackManifestSha256")))
        {
            return "Migration snapshot is not bound to the verified rollback manifest.";
        }

        var expected = NormalizeSet(manifest.Entries.Select(entry => entry.OriginalPath));
        var observed = NormalizeSet(evidence.Sources.Select(source => source.Path));
        if (expected is null || observed is null
            || expected.Count != manifest.Entries.Count
            || observed.Count != evidence.Sources.Count
            || !expected.SetEquals(observed)
            || evidence.Sources.Any(source =>
                !source.Exists || !source.IsDirectory || source.IsRedirect || source.ObservedBytes < 0))
        {
            return "Migration snapshot source inventory is incomplete or unsafe.";
        }
        return null;
    }

    private static string? StringArgument(OperationDescriptor descriptor, string key) =>
        descriptor.Arguments.TryGetValue(key, out var value) ? value as string : null;

    private static HashSet<string>? NormalizeSet(IEnumerable<string> paths)
    {
        try
        {
            return paths.Select(Path.GetFullPath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return null;
        }
    }

    private static bool PathsEqual(string? left, string? right)
    {
        try
        {
            return !string.IsNullOrWhiteSpace(left)
                && !string.IsNullOrWhiteSpace(right)
                && string.Equals(
                    Path.GetFullPath(left),
                    Path.GetFullPath(right),
                    StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool HashesEqual(string? left, string? right)
    {
        if (!IsSha256(left) || !IsSha256(right))
            return false;

        var leftBytes = Convert.FromHexString(left!);
        var rightBytes = Convert.FromHexString(right!);
        try
        {
            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(leftBytes);
            CryptographicOperations.ZeroMemory(rightBytes);
        }
    }

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public static class MigrationSnapshotEvidenceStore
{
    private const int MaximumEvidenceBytes = 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 16
    };

    public static async Task WriteAsync(
        MigrationSnapshotEvidence evidence,
        string evidencePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (string.IsNullOrWhiteSpace(evidencePath))
            throw new ArgumentException("Migration snapshot evidence path is required.", nameof(evidencePath));
        var fullPath = Path.GetFullPath(evidencePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        var temporary = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");
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
                await JsonSerializer.SerializeAsync(stream, evidence, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            if (new FileInfo(temporary).Length is <= 0 or > MaximumEvidenceBytes)
                throw new InvalidOperationException("Migration snapshot evidence size is invalid.");
            File.Move(temporary, fullPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    public static async Task<MigrationSnapshotEvidenceVerificationResult> ReadVerifiedAsync(
        string evidencePath,
        string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(evidencePath) || !File.Exists(evidencePath))
            return Invalid("Migration snapshot evidence is missing.");
        if (!IsSha256(expectedSha256))
            return Invalid("Migration snapshot hash is invalid.");
        try
        {
            var fullPath = Path.GetFullPath(evidencePath);
            var info = new FileInfo(fullPath);
            if (info.Length is <= 0 or > MaximumEvidenceBytes)
                return Invalid("Migration snapshot evidence size is invalid.");
            var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            var actualBytes = SHA256.HashData(bytes);
            var expectedBytes = Convert.FromHexString(expectedSha256);
            try
            {
                var actual = Convert.ToHexString(actualBytes);
                if (!CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes))
                    return Invalid("Migration snapshot hash does not match.", actual);
                var evidence = JsonSerializer.Deserialize<MigrationSnapshotEvidence>(bytes, JsonOptions);
                return evidence is null
                    ? Invalid("Migration snapshot evidence is empty.", actual)
                    : new MigrationSnapshotEvidenceVerificationResult
                    {
                        IsValid = true,
                        ActualSha256 = actual,
                        Evidence = evidence
                    };
            }
            finally
            {
                CryptographicOperations.ZeroMemory(bytes);
                CryptographicOperations.ZeroMemory(actualBytes);
                CryptographicOperations.ZeroMemory(expectedBytes);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Invalid("Migration snapshot evidence could not be verified.");
        }
    }

    public static async Task<string> ComputeSha256Async(
        string evidencePath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(
            evidencePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        try
        {
            return Convert.ToHexString(hash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(hash);
        }
    }

    private static MigrationSnapshotEvidenceVerificationResult Invalid(
        string error,
        string? actualSha256 = null) =>
        new() { IsValid = false, Error = error, ActualSha256 = actualSha256 };

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}
