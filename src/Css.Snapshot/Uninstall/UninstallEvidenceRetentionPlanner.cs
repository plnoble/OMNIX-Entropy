using System.Text.Json;

namespace Css.Snapshot.Uninstall;

public sealed record UninstallEvidenceRetentionPolicy(
    TimeSpan MaximumAge,
    int MaximumCount);

public sealed class UninstallEvidenceRetentionItem
{
    public required string SnapshotId { get; init; }
    public required string ManifestPath { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
    public required string Reason { get; init; }
    public required string Sha256 { get; init; }
}

public sealed class UninstallEvidenceRetentionPlan
{
    public required IReadOnlyList<UninstallEvidenceRetentionItem> Keep { get; init; }
    public required IReadOnlyList<UninstallEvidenceRetentionItem> Candidates { get; init; }
    public required IReadOnlyList<UninstallEvidenceRetentionItem> PreservedUnknown { get; init; }
    public required bool CanApplyDirectly { get; init; }
}

public sealed class UninstallEvidenceRetentionPlanner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _root;
    private readonly string _rootPrefix;

    public UninstallEvidenceRetentionPlanner(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException("Snapshot root is required.", nameof(root));

        _root = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _rootPrefix = _root + Path.DirectorySeparatorChar;
    }

    public async Task<UninstallEvidenceRetentionPlan> PlanAsync(
        UninstallEvidenceRetentionPolicy policy,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (policy.MaximumAge <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(policy), "Maximum age must be positive.");
        if (policy.MaximumCount < 1)
            throw new ArgumentOutOfRangeException(nameof(policy), "Maximum count must be at least one.");

        if (!Directory.Exists(_root))
            return EmptyPlan();

        var valid = new List<UninstallEvidenceRetentionItem>();
        var preservedUnknown = new List<UninstallEvidenceRetentionItem>();
        foreach (var candidatePath in Directory.EnumerateFiles(
                     _root,
                     "uninstall-*.json",
                     SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = Path.GetFullPath(candidatePath);
            if (!IsInsideRoot(fullPath) || IsReparsePoint(fullPath))
            {
                preservedUnknown.Add(Unknown(fullPath, "\u8def\u5f84\u4e0d\u5c5e\u4e8e\u5feb\u7167\u6839\u76ee\u5f55\u6216\u662f\u91cd\u89e3\u6790\u94fe\u63a5\uff0c\u4fdd\u7559\u4e0d\u5904\u7406\u3002"));
                continue;
            }

            var manifest = await TryLoadManifestAsync(fullPath, cancellationToken);
            if (!IsRecognizedManifest(manifest, fullPath))
            {
                preservedUnknown.Add(Unknown(fullPath, "\u4e0d\u662f\u53ef\u9a8c\u8bc1\u7684 OMNIX \u5378\u8f7d\u8bc1\u636e\u6e05\u5355\uff0c\u4fdd\u7559\u4e0d\u5904\u7406\u3002"));
                continue;
            }

            valid.Add(new UninstallEvidenceRetentionItem
            {
                SnapshotId = manifest!.SnapshotId,
                ManifestPath = fullPath,
                CreatedAtUtc = manifest.CreatedAtUtc,
                Reason = "\u6709\u6548\u7684 OMNIX \u5378\u8f7d\u524d\u8bc1\u636e\u6e05\u5355\u3002",
                Sha256 = UninstallEvidenceSnapshotStore.ComputeSha256(fullPath)
            });
        }

        var keep = new List<UninstallEvidenceRetentionItem>();
        var retentionCandidates = new List<UninstallEvidenceRetentionItem>();
        var nonExpired = new List<UninstallEvidenceRetentionItem>();
        foreach (var item in valid
                     .OrderByDescending(item => item.CreatedAtUtc)
                     .ThenBy(item => item.SnapshotId, StringComparer.Ordinal))
        {
            if (now.ToUniversalTime() - item.CreatedAtUtc.ToUniversalTime() > policy.MaximumAge)
            {
                retentionCandidates.Add(WithReason(
                    item,
                    "\u5feb\u7167\u8bc1\u636e\u5df2\u8fc7\u671f\uff0c\u53ef\u8fdb\u5165\u540e\u7eed\u53ef\u56de\u6eda\u5f52\u6863\u8ba1\u5212\u3002"));
            }
            else
            {
                nonExpired.Add(item);
            }
        }

        for (var index = 0; index < nonExpired.Count; index++)
        {
            var item = nonExpired[index];
            if (index < policy.MaximumCount)
            {
                keep.Add(WithReason(item, "\u4f4d\u4e8e\u6700\u65b0\u7684\u4fdd\u7559\u6570\u91cf\u5185\u3002"));
            }
            else
            {
                retentionCandidates.Add(WithReason(
                    item,
                    "\u5feb\u7167\u8bc1\u636e\u8d85\u51fa\u4fdd\u7559\u6570\u91cf\uff0c\u53ef\u8fdb\u5165\u540e\u7eed\u53ef\u56de\u6eda\u5f52\u6863\u8ba1\u5212\u3002"));
            }
        }

        return new UninstallEvidenceRetentionPlan
        {
            Keep = keep,
            Candidates = retentionCandidates,
            PreservedUnknown = preservedUnknown,
            CanApplyDirectly = false
        };
    }

    private bool IsInsideRoot(string path) =>
        path.StartsWith(_rootPrefix, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Path.GetDirectoryName(path), _root, StringComparison.OrdinalIgnoreCase);

    private static bool IsReparsePoint(string path)
    {
        try
        {
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }
        catch
        {
            return true;
        }
    }

    private static async Task<UninstallEvidenceSnapshotManifest?> TryLoadManifestAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<UninstallEvidenceSnapshotManifest>(
                stream,
                JsonOptions,
                cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsRecognizedManifest(
        UninstallEvidenceSnapshotManifest? manifest,
        string path) =>
        manifest is
        {
            SchemaVersion: 1,
            Purpose: "pre-uninstall-evidence",
            CanRestoreApplication: false
        }
        && !string.IsNullOrWhiteSpace(manifest.SnapshotId)
        && !string.IsNullOrWhiteSpace(manifest.SoftwareName)
        && string.Equals(
            Path.GetFileName(path),
            manifest.SnapshotId + ".json",
            StringComparison.Ordinal);

    private static UninstallEvidenceRetentionItem Unknown(string path, string reason) =>
        new()
        {
            SnapshotId = string.Empty,
            ManifestPath = path,
            CreatedAtUtc = DateTimeOffset.MinValue,
            Reason = reason,
            Sha256 = string.Empty
        };

    private static UninstallEvidenceRetentionItem WithReason(
        UninstallEvidenceRetentionItem item,
        string reason) =>
        new()
        {
            SnapshotId = item.SnapshotId,
            ManifestPath = item.ManifestPath,
            CreatedAtUtc = item.CreatedAtUtc,
            Reason = reason,
            Sha256 = item.Sha256
        };

    private static UninstallEvidenceRetentionPlan EmptyPlan() =>
        new()
        {
            Keep = [],
            Candidates = [],
            PreservedUnknown = [],
            CanApplyDirectly = false
        };
}
