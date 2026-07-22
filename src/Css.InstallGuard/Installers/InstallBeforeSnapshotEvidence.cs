using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Css.Core.Operations;
using Css.Core.Software;

namespace Css.InstallGuard.Installers;

public sealed record InstallBeforeSnapshotEvidence
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public required string SnapshotId { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required DateTimeOffset InventoryCapturedAtUtc { get; init; }
    public required string PackagePath { get; init; }
    public required string PackageSha256 { get; init; }
    public int SoftwareCount { get; init; }
    public required string InventoryFingerprintSha256 { get; init; }
    public InstallFootprintCaptureStatus FootprintStatus { get; init; }
    public int FootprintPathCount { get; init; }
    public required string FootprintFingerprintSha256 { get; init; }
}

public sealed record InstallBeforeSnapshotEvidenceCreationResult
{
    public required InstallBeforeSnapshotEvidence Evidence { get; init; }
    public required string EvidencePath { get; init; }
    public required string Sha256 { get; init; }
}

public sealed record InstallBeforeSnapshotEvidenceVerificationResult
{
    public required bool IsValid { get; init; }
    public InstallBeforeSnapshotEvidence? Evidence { get; init; }
    public string? ActualSha256 { get; init; }
    public string? Error { get; init; }
}

public interface IInstallBeforeSnapshotEvidenceReader
{
    Task<InstallBeforeSnapshotEvidenceVerificationResult> ReadVerifiedAsync(
        string evidencePath,
        string expectedSha256,
        CancellationToken cancellationToken = default);
}

public sealed class InstallBeforeSnapshotEvidenceReader
    : IInstallBeforeSnapshotEvidenceReader
{
    public Task<InstallBeforeSnapshotEvidenceVerificationResult> ReadVerifiedAsync(
        string evidencePath,
        string expectedSha256,
        CancellationToken cancellationToken = default) =>
        InstallBeforeSnapshotEvidenceStore.ReadVerifiedAsync(
            evidencePath,
            expectedSha256,
            cancellationToken);
}

public static class InstallBeforeSnapshotEvidenceService
{
    private const int MaximumSoftwareCount = 4096;

    public static async Task<InstallBeforeSnapshotEvidenceCreationResult> CreateAsync(
        InstallerPackageEvidence package,
        InstallSystemSnapshot inventory,
        string evidencePath,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(inventory);
        if (!package.HasStableIdentity || !IsSha256(package.Sha256))
            throw new InvalidOperationException("Stable installer package evidence is required.");
        if (inventory.SoftwareProfiles.Count > MaximumSoftwareCount)
            throw new InvalidOperationException("Install inventory is too large.");
        var footprint = inventory.CDriveFootprint ?? InstallFootprintCapture.EmptyComplete;
        if (footprint.Paths.Count > WindowsInstallFootprintProbe.MaximumEntries)
            throw new InvalidOperationException("Install footprint inventory is too large.");

        var createdAt = now.ToUniversalTime();
        var capturedAt = inventory.CapturedAt.ToUniversalTime();
        var age = createdAt - capturedAt;
        if (age > TimeSpan.FromMinutes(5) || age < -TimeSpan.FromMinutes(1))
            throw new InvalidOperationException("Install inventory is stale or future-dated.");

        var evidence = new InstallBeforeSnapshotEvidence
        {
            SnapshotId = "install-before-" + Guid.NewGuid().ToString("N"),
            CreatedAtUtc = createdAt,
            InventoryCapturedAtUtc = capturedAt,
            PackagePath = Path.GetFullPath(package.PackagePath),
            PackageSha256 = package.Sha256!.ToUpperInvariant(),
            SoftwareCount = inventory.SoftwareProfiles.Count,
            InventoryFingerprintSha256 = ComputeInventoryFingerprint(
                inventory.SoftwareProfiles),
            FootprintStatus = footprint.Status,
            FootprintPathCount = footprint.Paths.Count,
            FootprintFingerprintSha256 = ComputeFootprintFingerprint(footprint)
        };
        await InstallBeforeSnapshotEvidenceStore.WriteAsync(
            evidence,
            evidencePath,
            cancellationToken);
        var sha256 = await InstallBeforeSnapshotEvidenceStore.ComputeSha256Async(
            evidencePath,
            cancellationToken);
        return new InstallBeforeSnapshotEvidenceCreationResult
        {
            Evidence = evidence,
            EvidencePath = Path.GetFullPath(evidencePath),
            Sha256 = sha256
        };
    }

    public static string? ValidateForOperation(
        InstallBeforeSnapshotEvidence evidence,
        OperationDescriptor descriptor,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(descriptor);
        if (evidence.SchemaVersion != InstallBeforeSnapshotEvidence.CurrentSchemaVersion
            || evidence.SoftwareCount is < 0 or > MaximumSoftwareCount
            || evidence.FootprintPathCount is < 0 or > WindowsInstallFootprintProbe.MaximumEntries
            || !Enum.IsDefined(evidence.FootprintStatus)
            || !IsSha256(evidence.PackageSha256)
            || !IsSha256(evidence.InventoryFingerprintSha256)
            || !IsSha256(evidence.FootprintFingerprintSha256)
            || !string.Equals(evidence.SnapshotId, descriptor.SnapshotId, StringComparison.Ordinal))
        {
            return "Install snapshot identity is invalid.";
        }

        var age = now.ToUniversalTime() - evidence.CreatedAtUtc.ToUniversalTime();
        var inventoryAge = now.ToUniversalTime() - evidence.InventoryCapturedAtUtc.ToUniversalTime();
        if (age > TimeSpan.FromMinutes(30) || age < -TimeSpan.FromMinutes(1)
            || inventoryAge > TimeSpan.FromMinutes(30) || inventoryAge < -TimeSpan.FromMinutes(1))
        {
            return "Install snapshot evidence is stale or future-dated.";
        }

        if (!PathsEqual(evidence.PackagePath, StringArgument(descriptor, "packagePath"))
            || !HashesEqual(evidence.PackageSha256, StringArgument(descriptor, "packageSha256")))
        {
            return "Install snapshot is not bound to the selected package.";
        }
        return null;
    }

    public static string ComputeInventoryFingerprint(
        IReadOnlyList<SoftwareProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        if (profiles.Count > MaximumSoftwareCount)
            throw new InvalidOperationException("Install inventory is too large.");

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendInt32(hash, profiles.Count);
        foreach (var profile in profiles
                     .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.Publisher, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.InstallPath, StringComparer.OrdinalIgnoreCase))
        {
            AppendString(hash, profile.Name);
            AppendString(hash, profile.Publisher);
            AppendString(hash, profile.InstallPath);
            AppendString(hash, profile.UninstallCommand);
            AppendStrings(hash, profile.StartupEntries);
            AppendStrings(hash, profile.Services);
            AppendStrings(hash, profile.ScheduledTasks);
            AppendStrings(hash, profile.CDriveWritePaths);
        }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    public static string ComputeFootprintFingerprint(InstallFootprintCapture? capture)
    {
        capture ??= InstallFootprintCapture.EmptyComplete;
        ArgumentNullException.ThrowIfNull(capture.Paths);
        if (!Enum.IsDefined(capture.Status))
            throw new InvalidOperationException("Install footprint status is invalid.");
        if (capture.Paths.Count > WindowsInstallFootprintProbe.MaximumEntries)
            throw new InvalidOperationException("Install footprint inventory is too large.");

        var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in capture.Paths)
        {
            if (string.IsNullOrWhiteSpace(path)
                || path.Length > 1024
                || path.StartsWith(@"\\", StringComparison.Ordinal))
                throw new InvalidOperationException("Install footprint path is unsafe.");
            var full = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!string.Equals(Path.GetPathRoot(full), @"C:\", StringComparison.OrdinalIgnoreCase)
                || string.Equals(full, "C:", StringComparison.OrdinalIgnoreCase)
                || !normalized.Add(full))
                throw new InvalidOperationException("Install footprint path is unsafe or duplicated.");
        }

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendInt32(hash, (int)capture.Status);
        AppendInt32(hash, normalized.Count);
        foreach (var path in normalized.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            AppendString(hash, path);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static void AppendStrings(IncrementalHash hash, IReadOnlyList<string> values)
    {
        if (values.Count > 4096)
            throw new InvalidOperationException("Install inventory list is too large.");
        var ordered = values.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
        AppendInt32(hash, ordered.Length);
        foreach (var value in ordered)
            AppendString(hash, value);
    }

    private static void AppendString(IncrementalHash hash, string? value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        if (bytes.Length > 32 * 1024)
            throw new InvalidOperationException("Install inventory value is too large.");
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        hash.AppendData(buffer);
    }

    private static string? StringArgument(OperationDescriptor descriptor, string key) =>
        descriptor.Arguments.TryGetValue(key, out var value) ? value as string : null;

    internal static bool PathsEqual(string? left, string? right)
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

    internal static bool HashesEqual(string? left, string? right)
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

    internal static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public static class InstallBeforeSnapshotEvidenceStore
{
    private const int MaximumEvidenceBytes = 64 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 8
    };

    public static async Task WriteAsync(
        InstallBeforeSnapshotEvidence evidence,
        string evidencePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (string.IsNullOrWhiteSpace(evidencePath))
            throw new ArgumentException("Install snapshot evidence path is required.", nameof(evidencePath));
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
                throw new InvalidOperationException("Install snapshot evidence size is invalid.");
            File.Move(temporary, fullPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    public static async Task<InstallBeforeSnapshotEvidenceVerificationResult> ReadVerifiedAsync(
        string evidencePath,
        string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(evidencePath) || !File.Exists(evidencePath))
            return Invalid("Install snapshot evidence is missing.");
        if (!InstallBeforeSnapshotEvidenceService.IsSha256(expectedSha256))
            return Invalid("Install snapshot hash is invalid.");
        try
        {
            var fullPath = Path.GetFullPath(evidencePath);
            var info = new FileInfo(fullPath);
            if (info.Length is <= 0 or > MaximumEvidenceBytes)
                return Invalid("Install snapshot evidence size is invalid.");
            var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            var actualBytes = SHA256.HashData(bytes);
            var expectedBytes = Convert.FromHexString(expectedSha256);
            try
            {
                var actual = Convert.ToHexString(actualBytes);
                if (!CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes))
                    return Invalid("Install snapshot hash does not match.", actual);
                var evidence = JsonSerializer.Deserialize<InstallBeforeSnapshotEvidence>(bytes, JsonOptions);
                return evidence is null
                    ? Invalid("Install snapshot evidence is empty.", actual)
                    : new InstallBeforeSnapshotEvidenceVerificationResult
                    {
                        IsValid = true,
                        Evidence = evidence,
                        ActualSha256 = actual
                    };
            }
            finally
            {
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
            return Invalid("Install snapshot evidence could not be read.");
        }
    }

    public static async Task<string> ComputeSha256Async(
        string evidencePath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(
            Path.GetFullPath(evidencePath),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
    }

    private static InstallBeforeSnapshotEvidenceVerificationResult Invalid(
        string error,
        string? actualSha256 = null) =>
        new()
        {
            IsValid = false,
            Error = error,
            ActualSha256 = actualSha256
        };
}
