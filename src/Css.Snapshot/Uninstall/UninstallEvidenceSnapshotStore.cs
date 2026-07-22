using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Css.Core.Apps;
using Css.Core.Software;

namespace Css.Snapshot.Uninstall;

public sealed class UninstallEvidenceSnapshotManifest
{
    public int SchemaVersion { get; init; } = 1;
    public required string SnapshotId { get; init; }
    public string Purpose { get; init; } = "pre-uninstall-evidence";
    public bool CanRestoreApplication { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required string SoftwareName { get; init; }
    public string? Publisher { get; init; }
    public string? InstallPath { get; init; }
    public string? UninstallCommand { get; init; }
    public IReadOnlyList<string> DataPaths { get; init; } = [];
    public IReadOnlyList<string> CachePaths { get; init; } = [];
    public IReadOnlyList<string> LogPaths { get; init; } = [];
    public IReadOnlyList<string> StartupEntries { get; init; } = [];
    public IReadOnlyList<string> Services { get; init; } = [];
    public IReadOnlyList<string> ScheduledTasks { get; init; } = [];
    public required string RecoveryMethod { get; init; }
    public required string RecoveryReference { get; init; }
    public required bool UserDataBackupConfirmed { get; init; }
}

public sealed class UninstallEvidenceSnapshotStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _root;
    private readonly Func<DateTimeOffset> _clock;

    public UninstallEvidenceSnapshotStore(string root, Func<DateTimeOffset>? clock = null)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException("Snapshot root is required.", nameof(root));

        _root = Path.GetFullPath(root);
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<OfficialUninstallSnapshotEvidence> CreateAsync(
        SoftwareProfile profile,
        OfficialUninstallRecoveryEvidence recoveryEvidence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(recoveryEvidence);
        if (!OfficialUninstallRecoveryEvidenceValidator.IsUsable(recoveryEvidence))
            throw new ArgumentException("Usable recovery evidence is required.", nameof(recoveryEvidence));

        var createdAt = _clock().ToUniversalTime();
        var snapshotId = $"uninstall-{createdAt:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        var manifest = new UninstallEvidenceSnapshotManifest
        {
            SnapshotId = snapshotId,
            CanRestoreApplication = false,
            CreatedAtUtc = createdAt,
            SoftwareName = profile.Name,
            Publisher = profile.Publisher,
            InstallPath = profile.InstallPath,
            UninstallCommand = profile.UninstallCommand,
            DataPaths = profile.DataPaths,
            CachePaths = profile.CachePaths,
            LogPaths = profile.LogPaths,
            StartupEntries = profile.StartupEntries,
            Services = profile.Services,
            ScheduledTasks = profile.ScheduledTasks,
            RecoveryMethod = recoveryEvidence.Method.ToString(),
            RecoveryReference = recoveryEvidence.Reference!,
            UserDataBackupConfirmed = recoveryEvidence.UserDataBackupConfirmed
        };

        Directory.CreateDirectory(_root);
        var manifestPath = Path.Combine(_root, snapshotId + ".json");
        var temporaryPath = manifestPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            var json = JsonSerializer.Serialize(manifest, JsonOptions);
            await File.WriteAllTextAsync(
                temporaryPath,
                json,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);
            File.Move(temporaryPath, manifestPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return new OfficialUninstallSnapshotEvidence
        {
            SnapshotId = snapshotId,
            ManifestPath = manifestPath,
            SoftwareName = profile.Name,
            CreatedAtUtc = createdAt,
            Sha256 = ComputeSha256(manifestPath),
            CanRestoreApplication = false
        };
    }

    public async Task<UninstallEvidenceSnapshotManifest?> LoadAsync(
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(manifestPath);
        return await JsonSerializer.DeserializeAsync<UninstallEvidenceSnapshotManifest>(
            stream,
            JsonOptions,
            cancellationToken);
    }

    public async Task<OfficialUninstallSnapshotValidationResult> VerifyAsync(
        OfficialUninstallSnapshotEvidence evidence,
        SoftwareProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(profile);

        var validation = OfficialUninstallSnapshotEvidenceValidator.Validate(
            profile,
            evidence,
            ComputeSha256Safely,
            _clock());
        if (!validation.IsValid)
            return validation;

        UninstallEvidenceSnapshotManifest? manifest;
        try
        {
            manifest = await LoadAsync(evidence.ManifestPath, cancellationToken);
        }
        catch
        {
            return Invalid("\u5feb\u7167\u8bc1\u636e\u6e05\u5355\u65e0\u6cd5\u89e3\u6790\u3002");
        }

        if (manifest is null
            || manifest.SchemaVersion != 1
            || manifest.Purpose != "pre-uninstall-evidence"
            || manifest.CanRestoreApplication
            || !string.Equals(manifest.SnapshotId, evidence.SnapshotId, StringComparison.Ordinal)
            || !string.Equals(manifest.SoftwareName, profile.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Invalid("\u5feb\u7167\u8bc1\u636e\u6e05\u5355\u5185\u5bb9\u4e0e\u5f53\u524d\u8f6f\u4ef6\u4e0d\u5339\u914d\u3002");
        }

        return validation;
    }

    public static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string? ComputeSha256Safely(string path)
    {
        try
        {
            return File.Exists(path) ? ComputeSha256(path) : null;
        }
        catch
        {
            return null;
        }
    }

    private static OfficialUninstallSnapshotValidationResult Invalid(string reason) =>
        new() { IsValid = false, Reasons = [reason] };
}
