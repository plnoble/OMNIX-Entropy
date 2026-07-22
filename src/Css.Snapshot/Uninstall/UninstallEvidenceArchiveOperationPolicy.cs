using Css.Core.Operations;

namespace Css.Snapshot.Uninstall;

public sealed class UninstallEvidenceArchiveOperationPolicy
{
    public const string OperationKind = "snapshot.uninstall-evidence.archive";

    private readonly string _snapshotRoot;
    private readonly string _snapshotRootPrefix;

    public UninstallEvidenceArchiveOperationPolicy(string snapshotRoot)
    {
        if (string.IsNullOrWhiteSpace(snapshotRoot))
            throw new ArgumentException("Snapshot root is required.", nameof(snapshotRoot));

        _snapshotRoot = Path.GetFullPath(snapshotRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _snapshotRootPrefix = _snapshotRoot + Path.DirectorySeparatorChar;
    }

    public OperationDescriptor CreatePreview(
        UninstallEvidenceRetentionPlan plan,
        IReadOnlyCollection<string> selectedSnapshotIds)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(selectedSnapshotIds);
        if (selectedSnapshotIds.Count == 0)
            throw new InvalidOperationException("At least one snapshot must be selected.");

        var selectedIds = selectedSnapshotIds.ToHashSet(StringComparer.Ordinal);
        var selected = plan.Candidates
            .Where(item => selectedIds.Contains(item.SnapshotId))
            .ToList();
        if (selected.Count != selectedIds.Count)
            throw new InvalidOperationException("Every selected snapshot must be a retention candidate.");

        foreach (var item in selected)
        {
            if (!IsDirectChild(item.ManifestPath)
                || string.IsNullOrWhiteSpace(item.Sha256))
            {
                throw new InvalidOperationException("A selected snapshot is outside the configured root or lacks hash evidence.");
            }
        }

        return new OperationDescriptor
        {
            Kind = OperationKind,
            Title = "\u5f52\u6863\u65e7\u7684\u5378\u8f7d\u524d\u8bc1\u636e",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RequiresElevation = false,
            RequiresSnapshot = false,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = $"\u5df2\u9009\u62e9 {selected.Count} \u4efd\u8fc7\u671f\u6216\u8d85\u91cf\u7684 OMNIX \u8bc1\u636e\u6e05\u5355\uff1b\u53ea\u4f1a\u79fb\u5165\u53ef\u8fd8\u539f\u5f52\u6863\u533a\u3002",
            ConfirmationText = $"\u5c06 {selected.Count} \u4efd\u65e7\u8bc1\u636e\u6e05\u5355\u79fb\u5165\u53ef\u8fd8\u539f\u5f52\u6863\u533a\uff1f",
            AffectedPaths = selected.Select(item => item.ManifestPath).ToList(),
            Arguments = new Dictionary<string, object?>
            {
                ["snapshotRoot"] = _snapshotRoot,
                ["expectedSha256ByPath"] = selected.ToDictionary(
                    item => item.ManifestPath,
                    item => item.Sha256,
                    StringComparer.OrdinalIgnoreCase)
            }
        };
    }

    public OperationDescriptor ConfirmForExecution(OperationDescriptor preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (!string.Equals(preview.Kind, OperationKind, StringComparison.Ordinal)
            || preview.AffectedPaths.Count == 0
            || preview.RollbackRequired != true)
        {
            throw new InvalidOperationException("The descriptor is not a valid uninstall-evidence archive preview.");
        }

        return new OperationDescriptor
        {
            Kind = preview.Kind,
            Title = preview.Title,
            Source = preview.Source,
            Risk = preview.Risk,
            IsDestructive = preview.IsDestructive,
            RequiresElevation = preview.RequiresElevation,
            RequiresSnapshot = preview.RequiresSnapshot,
            SnapshotId = preview.SnapshotId,
            RollbackRequired = preview.RollbackRequired,
            ConfirmationAccepted = true,
            EvidenceSummary = preview.EvidenceSummary,
            EstimatedImpactBytes = preview.EstimatedImpactBytes,
            ConfirmationText = preview.ConfirmationText,
            AffectedPaths = preview.AffectedPaths,
            AffectedRegistryKeys = preview.AffectedRegistryKeys,
            AffectedServices = preview.AffectedServices,
            Arguments = preview.Arguments
        };
    }

    private bool IsDirectChild(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(_snapshotRootPrefix, StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                Path.GetDirectoryName(fullPath),
                _snapshotRoot,
                StringComparison.OrdinalIgnoreCase);
    }
}
