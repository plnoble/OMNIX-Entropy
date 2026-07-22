using System.Text.Json;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Timeline;

namespace Css.Snapshot.Uninstall;

public sealed class UninstallEvidenceArchiveOperationHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _snapshotRoot;
    private readonly string _snapshotRootPrefix;
    private readonly Func<string, string, CancellationToken, Task<QuarantineRecord>> _archive;
    private readonly Func<string, CancellationToken, Task<QuarantineRestoreResult>> _restore;
    private readonly ActionTimelineStore _timeline;

    public UninstallEvidenceArchiveOperationHandler(
        string snapshotRoot,
        FileQuarantineService quarantine,
        ActionTimelineStore timeline)
    {
        if (string.IsNullOrWhiteSpace(snapshotRoot))
            throw new ArgumentException("Snapshot root is required.", nameof(snapshotRoot));
        ArgumentNullException.ThrowIfNull(quarantine);
        ArgumentNullException.ThrowIfNull(timeline);

        _snapshotRoot = Path.GetFullPath(snapshotRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _snapshotRootPrefix = _snapshotRoot + Path.DirectorySeparatorChar;
        _archive = quarantine.QuarantineAsync;
        _restore = quarantine.RestoreAsync;
        _timeline = timeline;
    }

    public UninstallEvidenceArchiveOperationHandler(
        string snapshotRoot,
        Func<string, string, CancellationToken, Task<QuarantineRecord>> archive,
        Func<string, CancellationToken, Task<QuarantineRestoreResult>> restore,
        ActionTimelineStore timeline)
    {
        if (string.IsNullOrWhiteSpace(snapshotRoot))
            throw new ArgumentException("Snapshot root is required.", nameof(snapshotRoot));
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(restore);
        ArgumentNullException.ThrowIfNull(timeline);

        _snapshotRoot = Path.GetFullPath(snapshotRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _snapshotRootPrefix = _snapshotRoot + Path.DirectorySeparatorChar;
        _archive = archive;
        _restore = restore;
        _timeline = timeline;
    }

    public async Task<OperationResult> ExecuteAsync(
        OperationDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(
                descriptor.Kind,
                UninstallEvidenceArchiveOperationPolicy.OperationKind,
                StringComparison.Ordinal))
        {
            return OperationResult.Fail("Unsupported uninstall-evidence archive operation.");
        }

        if (!TryGetExpectedHashes(descriptor, out var expectedHashes))
            return OperationResult.Fail("Archive operation is missing planned hash evidence.");

        var validationError = await ValidateBatchAsync(
            descriptor.AffectedPaths,
            expectedHashes,
            cancellationToken);
        if (validationError is not null)
            return OperationResult.Fail(validationError);

        var moved = new List<QuarantineRecord>();
        try
        {
            foreach (var path in descriptor.AffectedPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var currentHash = UninstallEvidenceSnapshotStore.ComputeSha256(path);
                if (!string.Equals(currentHash, expectedHashes[path], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("\u5f52\u6863\u524d\u5feb\u7167\u8bc1\u636e\u54c8\u5e0c\u5df2\u53d8\u5316\uff0c\u5df2\u505c\u6b62\u3002");

                moved.Add(await _archive(
                    path,
                    descriptor.EvidenceSummary ?? descriptor.Title,
                    cancellationToken));
            }

            await _timeline.AddAsync(new ActionTimelineEntry
            {
                OccurredAt = DateTimeOffset.Now,
                Source = descriptor.Source,
                Title = descriptor.Title,
                EvidenceSummary = descriptor.EvidenceSummary
                    ?? "\u5df2\u79fb\u5165\u53ef\u8fd8\u539f\u8bc1\u636e\u5f52\u6863\u533a\u3002",
                AffectedPaths = moved.Select(record => record.OriginalPath).ToList(),
                RestoreState = RestoreState.Restorable,
                RestoreOperationKind = "quarantine.restore",
                RestoreManifestPaths = moved.Select(record => record.ManifestPath).ToList()
            }, cancellationToken);

            return OperationResult.Ok(
                $"\u5df2\u5f52\u6863 {moved.Count} \u4efd\u5378\u8f7d\u524d\u8bc1\u636e\uff0c\u53ef\u4ece\u540e\u6094\u836f\u4e2d\u5fc3\u8fd8\u539f\u3002",
                moved);
        }
        catch (Exception exception)
        {
            var rollbackErrors = await RestoreMovedAsync(moved);
            if (exception is OperationCanceledException)
                throw;

            var suffix = rollbackErrors.Count == 0
                ? string.Empty
                : " \u56de\u6eda\u5f02\u5e38: " + string.Join("; ", rollbackErrors);
            return OperationResult.Fail(exception.Message + suffix);
        }
    }

    private async Task<string?> ValidateBatchAsync(
        IReadOnlyList<string> paths,
        IReadOnlyDictionary<string, string> expectedHashes,
        CancellationToken cancellationToken)
    {
        if (paths.Count == 0 || expectedHashes.Count != paths.Count)
            return "Archive operation paths do not match planned hash evidence.";

        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = Path.GetFullPath(path);
            if (!IsDirectChild(fullPath)
                || !File.Exists(fullPath)
                || IsReparsePoint(fullPath))
            {
                return "Archive source is missing, outside the snapshot root, or is a reparse point: " + path;
            }

            if (!expectedHashes.TryGetValue(path, out var expectedHash)
                || string.IsNullOrWhiteSpace(expectedHash))
            {
                return "Archive source lacks expected hash evidence: " + path;
            }

            var actualHash = UninstallEvidenceSnapshotStore.ComputeSha256(fullPath);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                return "\u5feb\u7167\u8bc1\u636e\u54c8\u5e0c\u4e0d\u5339\u914d\uff0c\u6587\u4ef6\u5728\u89c4\u5212\u540e\u53ef\u80fd\u5df2\u53d8\u5316\u3002";

            var manifest = await TryLoadManifestAsync(fullPath, cancellationToken);
            if (!IsRecognizedManifest(manifest, fullPath))
                return "Archive source is no longer a recognized OMNIX uninstall evidence manifest: " + path;
        }

        return null;
    }

    private static bool TryGetExpectedHashes(
        OperationDescriptor descriptor,
        out IReadOnlyDictionary<string, string> expectedHashes)
    {
        if (descriptor.Arguments.TryGetValue("expectedSha256ByPath", out var value)
            && value is IReadOnlyDictionary<string, string> typed)
        {
            expectedHashes = typed;
            return true;
        }

        expectedHashes = new Dictionary<string, string>();
        return false;
    }

    private bool IsDirectChild(string path) =>
        path.StartsWith(_snapshotRootPrefix, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Path.GetDirectoryName(path), _snapshotRoot, StringComparison.OrdinalIgnoreCase);

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
                cancellationToken: cancellationToken);
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
        && string.Equals(
            Path.GetFileName(path),
            manifest.SnapshotId + ".json",
            StringComparison.Ordinal);

    private async Task<IReadOnlyList<string>> RestoreMovedAsync(
        IReadOnlyList<QuarantineRecord> moved)
    {
        var errors = new List<string>();
        foreach (var record in moved.Reverse())
        {
            try
            {
                var restore = await _restore(
                    record.ManifestPath,
                    CancellationToken.None);
                if (!restore.Success)
                    errors.Add(restore.Summary);
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
            }
        }

        return errors;
    }
}
