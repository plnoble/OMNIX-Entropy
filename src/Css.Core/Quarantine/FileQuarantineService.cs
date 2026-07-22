using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Timeline;

namespace Css.Core.Quarantine;

public sealed class QuarantineRecord
{
    public required string Id { get; init; }
    public DateTimeOffset MovedAt { get; init; } = DateTimeOffset.Now;
    public required string OriginalPath { get; init; }
    public required string QuarantinedPath { get; init; }
    public required string ManifestPath { get; init; }
    public required string Reason { get; init; }
    public long SizeBytes { get; init; }
    public RestoreState RestoreState { get; init; } = RestoreState.Restorable;
    public DateTimeOffset? RestoredAt { get; init; }
}

public sealed class QuarantineRestoreResult
{
    public bool Success { get; init; }
    public RestoreState RestoreState { get; init; }
    public required string Summary { get; init; }
    public QuarantineRecord? Record { get; init; }
}

public sealed class QuarantineManifestInspection
{
    public bool Success { get; init; }
    public required string Summary { get; init; }
    public QuarantineRecord? Record { get; init; }
}

public sealed class QuarantinePurgeResult
{
    public bool Success { get; init; }
    public bool MayHaveChanged { get; init; }
    public required string Summary { get; init; }
    public QuarantineRecord? Record { get; init; }
}

public sealed class FileQuarantineService
{
    private const long MaximumManifestBytes = 256L * 1024;
    private readonly string _quarantineRoot;

    public string QuarantineRoot => _quarantineRoot;

    public FileQuarantineService(string quarantineRoot)
    {
        if (string.IsNullOrWhiteSpace(quarantineRoot))
            throw new ArgumentException("Quarantine root is required.", nameof(quarantineRoot));

        _quarantineRoot = Path.GetFullPath(quarantineRoot);
    }

    public async Task<QuarantineRecord> QuarantineAsync(string sourcePath, string reason, CancellationToken ct = default)
    {
        return await QuarantineCoreAsync(sourcePath, reason, null, null, ct);
    }

    public async Task<QuarantineRecord> QuarantineAsync(
        string sourcePath,
        string reason,
        QuarantineCandidateEvidence expectedEvidence,
        IQuarantineCandidateIdentityReader identityReader,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(expectedEvidence);
        ArgumentNullException.ThrowIfNull(identityReader);
        return await QuarantineCoreAsync(sourcePath, reason, expectedEvidence, identityReader, ct);
    }

    private async Task<QuarantineRecord> QuarantineCoreAsync(
        string sourcePath,
        string reason,
        QuarantineCandidateEvidence? expectedEvidence,
        IQuarantineCandidateIdentityReader? identityReader,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path is required.", nameof(sourcePath));

        var originalPath = ValidateMoveCandidate(sourcePath, expectedEvidence, identityReader);

        var id = DateTimeOffset.Now.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N");
        var itemRoot = Path.Combine(_quarantineRoot, DateTimeOffset.Now.ToString("yyyyMMdd"), id);
        Directory.CreateDirectory(itemRoot);

        var name = Path.GetFileName(originalPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(name))
            name = "item";

        var quarantinedPath = Path.Combine(itemRoot, name);
        var manifestPath = Path.Combine(itemRoot, "manifest.json");
        var sizeBytes = CalculateSize(originalPath);
        var record = new QuarantineRecord
        {
            Id = id,
            MovedAt = DateTimeOffset.Now,
            OriginalPath = originalPath,
            QuarantinedPath = quarantinedPath,
            ManifestPath = manifestPath,
            Reason = string.IsNullOrWhiteSpace(reason) ? "未提供原因" : reason,
            SizeBytes = sizeBytes,
            RestoreState = RestoreState.Restorable
        };

        try
        {
            // Persist the recovery coordinates before the source leaves its original path.
            await WriteManifestAsync(record, ct);
            originalPath = ValidateMoveCandidate(originalPath, expectedEvidence, identityReader);
            if (expectedEvidence?.Kind == QuarantineCandidateKind.File
                || (expectedEvidence is null && File.Exists(originalPath)))
                File.Move(originalPath, quarantinedPath);
            else
                Directory.Move(originalPath, quarantinedPath);
            return record;
        }
        catch
        {
            if (!File.Exists(quarantinedPath) && !Directory.Exists(quarantinedPath))
                TryRemoveUnmovedPlan(itemRoot, manifestPath);
            throw;
        }
    }

    private string ValidateMoveCandidate(
        string sourcePath,
        QuarantineCandidateEvidence? expectedEvidence,
        IQuarantineCandidateIdentityReader? identityReader)
    {
        if (!QuarantineCandidatePathPolicy.TryNormalizeBatch(
                [sourcePath],
                _quarantineRoot,
                out var normalized,
                out var error))
        {
            throw new InvalidOperationException(error);
        }

        var originalPath = normalized[0];
        if (!QuarantineCandidatePathPolicy.TryInspectCurrentPath(
                originalPath,
                out var currentPath,
                out var currentKind,
                out error))
        {
            throw new InvalidOperationException(error);
        }

        if (expectedEvidence is null)
            return currentPath;
        if (identityReader is null)
            throw new InvalidOperationException("隔离候选身份读取器缺失。");

        var inspection = identityReader.Inspect(currentPath);
        if (!inspection.Success || inspection.Evidence is null)
            throw new InvalidOperationException(inspection.Summary);
        if (currentKind != expectedEvidence.Kind
            || !QuarantineCandidateEvidencePolicy.SameIdentity(expectedEvidence, inspection.Evidence))
        {
            throw new InvalidOperationException("隔离候选在确认后发生变化，旧方案已停止。");
        }
        return currentPath;
    }

    public async Task<IReadOnlyList<QuarantineRecord>> LoadRecordsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (!Directory.Exists(_quarantineRoot))
            return [];

        var records = new List<QuarantineRecord>();
        foreach (var manifestPath in EnumerateManifestsSafe(_quarantineRoot))
        {
            ct.ThrowIfCancellationRequested();
            var inspection = await InspectManifestAsync(manifestPath, ct);
            if (inspection.Success && inspection.Record is not null)
                records.Add(inspection.Record);
        }

        return records
            .OrderByDescending(record => record.MovedAt)
            .ToList();
    }

    public Task<QuarantineRestoreResult> RestoreAsync(
        string manifestPath,
        CancellationToken ct = default) =>
        RestoreCoreAsync(manifestPath, null, null, ct);

    public Task<QuarantineRestoreResult> RestoreAsync(
        string manifestPath,
        QuarantineCandidateEvidence expectedPayload,
        IQuarantineCandidateIdentityReader identityReader,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(expectedPayload);
        ArgumentNullException.ThrowIfNull(identityReader);
        return RestoreCoreAsync(manifestPath, expectedPayload, identityReader, ct);
    }

    private async Task<QuarantineRestoreResult> RestoreCoreAsync(
        string manifestPath,
        QuarantineCandidateEvidence? expectedPayload,
        IQuarantineCandidateIdentityReader? identityReader,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var inspection = await InspectManifestAsync(manifestPath, ct);
        var record = inspection.Record;
        if (!inspection.Success || record is null)
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.NotRestorable,
                Summary = inspection.Summary
            };
        }

        if (!File.Exists(record.QuarantinedPath) && !Directory.Exists(record.QuarantinedPath))
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.NotRestorable,
                Summary = "隔离文件已不存在，无法还原。",
                Record = record
            };
        }

        var payloadGate = ValidateExpectedRestorePayload(record, expectedPayload, identityReader);
        if (payloadGate is not null)
            return payloadGate;

        if (File.Exists(record.OriginalPath) || Directory.Exists(record.OriginalPath))
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.PartiallyRestorable,
                Summary = "原路径已有内容，为避免覆盖，已拒绝还原。",
                Record = record
            };
        }

        var parent = Path.GetDirectoryName(record.OriginalPath);
        if (!string.IsNullOrWhiteSpace(parent))
            Directory.CreateDirectory(parent);

        if (File.Exists(record.OriginalPath)
            || Directory.Exists(record.OriginalPath)
            || HasReparsePointInExistingChain(record.OriginalPath))
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.PartiallyRestorable,
                Summary = "原路径在还原前发生变化或经过重解析点，为避免覆盖，已拒绝还原。",
                Record = record
            };
        }

        payloadGate = ValidateExpectedRestorePayload(record, expectedPayload, identityReader);
        if (payloadGate is not null)
            return payloadGate;

        if (File.Exists(record.QuarantinedPath))
            File.Move(record.QuarantinedPath, record.OriginalPath);
        else
            Directory.Move(record.QuarantinedPath, record.OriginalPath);

        var restored = new QuarantineRecord
        {
            Id = record.Id,
            MovedAt = record.MovedAt,
            OriginalPath = record.OriginalPath,
            QuarantinedPath = record.QuarantinedPath,
            ManifestPath = record.ManifestPath,
            Reason = record.Reason,
            SizeBytes = record.SizeBytes,
            RestoreState = RestoreState.Restored,
            RestoredAt = DateTimeOffset.Now
        };

        await WriteManifestAsync(restored, ct);
        return new QuarantineRestoreResult
        {
            Success = true,
            RestoreState = RestoreState.Restored,
            Summary = "已还原到原路径。",
            Record = restored
        };
    }

    private static QuarantineRestoreResult? ValidateExpectedRestorePayload(
        QuarantineRecord record,
        QuarantineCandidateEvidence? expectedPayload,
        IQuarantineCandidateIdentityReader? identityReader)
    {
        if (expectedPayload is null)
            return null;
        if (identityReader is null)
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.Restorable,
                Summary = "隔离副本身份读取器缺失，已拒绝还原。",
                Record = record
            };
        }

        var current = identityReader.Inspect(record.QuarantinedPath);
        if (!current.Success || current.Evidence is null)
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.Restorable,
                Summary = current.Summary,
                Record = record
            };
        }
        if (!QuarantineCandidateEvidencePolicy.SameIdentity(expectedPayload, current.Evidence))
        {
            return new QuarantineRestoreResult
            {
                Success = false,
                RestoreState = RestoreState.Restorable,
                Summary = "隔离副本在确认后发生变化，已拒绝还原。",
                Record = record
            };
        }
        return null;
    }

    public async Task<QuarantineManifestInspection> InspectManifestAsync(
        string manifestPath,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!TryNormalizeManifestPath(manifestPath, out var normalizedManifest, out var error))
            return RefusedInspection(error);

        try
        {
            var info = new FileInfo(normalizedManifest);
            if (info.Length is <= 0 or > MaximumManifestBytes)
                return RefusedInspection("隔离区 manifest 大小异常，已拒绝读取。");

            await using var stream = File.OpenRead(normalizedManifest);
            var record = await JsonSerializer.DeserializeAsync<QuarantineRecord>(
                stream,
                cancellationToken: ct);
            if (!TryValidateRecord(normalizedManifest, record, out error))
                return RefusedInspection(error);

            return new QuarantineManifestInspection
            {
                Success = true,
                Summary = "隔离区 manifest 已通过路径约束检查。",
                Record = record
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return RefusedInspection("隔离区 manifest 不存在、无法读取或内容无效。");
        }
    }

    public async Task<QuarantineManifestInspection> InspectPurgeCandidateAsync(
        string manifestPath,
        CancellationToken ct = default)
    {
        var inspection = await InspectManifestAsync(manifestPath, ct);
        var record = inspection.Record;
        if (!inspection.Success || record is null)
            return inspection;

        try
        {
            var itemRoot = Path.GetDirectoryName(record.ManifestPath);
            if (string.IsNullOrWhiteSpace(itemRoot)
                || !Directory.Exists(itemRoot)
                || HasUnexpectedTopLevelEntries(itemRoot, record))
            {
                return RefusedInspection("隔离记录目录包含未知内容，已拒绝永久整理。");
            }

            var payloadExists = File.Exists(record.QuarantinedPath)
                || Directory.Exists(record.QuarantinedPath);
            if (record.RestoreState == RestoreState.Restored && payloadExists)
                return RefusedInspection("已还原记录仍存在隔离副本，状态不一致，已拒绝永久整理。");
            if (record.RestoreState != RestoreState.Restored && !payloadExists)
                return RefusedInspection("可还原记录缺少隔离副本，已拒绝永久整理。");

            if (payloadExists && !IsDeletionTreeSafe(record.QuarantinedPath, itemRoot))
                return RefusedInspection("隔离副本包含重解析点、越界位置或过多项目，已拒绝永久整理。");

            return new QuarantineManifestInspection
            {
                Success = true,
                Summary = record.RestoreState == RestoreState.Restored
                    ? "已还原记录可以在确认后整理。"
                    : "隔离副本可以在确认后永久整理；整理后不能还原。",
                Record = record
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return RefusedInspection("隔离记录目录无法完成安全检查。");
        }
    }

    public async Task<QuarantinePurgeResult> PurgeAsync(
        string manifestPath,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var inspection = await InspectPurgeCandidateAsync(manifestPath, ct);
        var record = inspection.Record;
        if (!inspection.Success || record is null)
        {
            return new QuarantinePurgeResult
            {
                Success = false,
                MayHaveChanged = false,
                Summary = inspection.Summary,
                Record = record
            };
        }

        try
        {
            // Once permanent deletion begins, finish this one record instead of
            // honoring cancellation halfway through an irreversible operation.
            await Task.Run(() => PurgeValidatedRecord(record), CancellationToken.None);
            return new QuarantinePurgeResult
            {
                Success = true,
                MayHaveChanged = true,
                Summary = record.RestoreState == RestoreState.Restored
                    ? "已整理还原后的隔离记录。"
                    : "已永久整理隔离副本；这一项不能再还原。",
                Record = record
            };
        }
        catch
        {
            return new QuarantinePurgeResult
            {
                Success = false,
                MayHaveChanged = true,
                Summary = "永久整理未完整完成，请重新加载后悔药中心复查。",
                Record = record
            };
        }
    }

    private static async Task WriteManifestAsync(QuarantineRecord record, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(record.ManifestPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(record.ManifestPath);
        await JsonSerializer.SerializeAsync(stream, record, new JsonSerializerOptions { WriteIndented = true }, ct);
    }

    private static long CalculateSize(string path)
    {
        if (File.Exists(path))
            return new FileInfo(path).Length;

        if (!Directory.Exists(path))
            return 0;

        long total = 0;
        foreach (var file in EnumerateFilesSafe(path))
        {
            try
            {
                var length = new FileInfo(file).Length;
                if (length > long.MaxValue - total)
                    return long.MaxValue;
                total += length;
            }
            catch
            {
                // Ignore files that disappear or become unreadable while sizing.
            }
        }
        return total;
    }

    private static IEnumerable<string> EnumerateFilesSafe(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            string[] files;
            try
            {
                files = Directory.GetFiles(current);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
                yield return file;

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var directory in directories)
            {
                try
                {
                    if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(directory);
                }
                catch
                {
                    // Ignore unreadable directories while estimating size.
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateManifestsSafe(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            string[] manifests;
            try
            {
                manifests = Directory.GetFiles(current, "manifest.json", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                manifests = [];
            }

            foreach (var manifest in manifests)
                yield return manifest;

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var directory in directories)
            {
                try
                {
                    if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(directory);
                }
                catch
                {
                    // Skip directories that disappear or cannot be read.
                }
            }
        }
    }

    private static bool IsInside(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private bool TryNormalizeManifestPath(
        string? manifestPath,
        out string normalizedManifest,
        out string error)
    {
        normalizedManifest = string.Empty;
        error = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(manifestPath)
                || !Path.IsPathFullyQualified(manifestPath))
            {
                error = "隔离区 manifest 路径无效。";
                return false;
            }

            normalizedManifest = Path.GetFullPath(manifestPath);
            if (!File.Exists(normalizedManifest)
                || !IsInside(_quarantineRoot, normalizedManifest)
                || !Path.GetFileName(normalizedManifest).Equals(
                    "manifest.json",
                    StringComparison.OrdinalIgnoreCase))
            {
                error = "隔离区 manifest 不在受管隔离目录中。";
                return false;
            }

            if (HasReparsePointInExistingChain(normalizedManifest))
            {
                error = "隔离区 manifest 路径经过重解析点，已拒绝。";
                return false;
            }

            return true;
        }
        catch
        {
            error = "隔离区 manifest 路径无法规范化。";
            return false;
        }
    }

    private bool TryValidateRecord(
        string normalizedManifest,
        QuarantineRecord? record,
        out string error)
    {
        error = string.Empty;
        try
        {
            if (record is null
                || string.IsNullOrWhiteSpace(record.Id)
                || record.Id.Length > 128
                || string.IsNullOrWhiteSpace(record.OriginalPath)
                || string.IsNullOrWhiteSpace(record.QuarantinedPath)
                || string.IsNullOrWhiteSpace(record.ManifestPath)
                || record.SizeBytes < 0)
            {
                error = "隔离区 manifest 缺少可信的还原信息。";
                return false;
            }

            var recordedManifest = Path.GetFullPath(record.ManifestPath);
            var quarantinedPath = Path.GetFullPath(record.QuarantinedPath);
            var originalPath = Path.GetFullPath(record.OriginalPath);
            var itemRoot = Path.GetDirectoryName(normalizedManifest);
            if (string.IsNullOrWhiteSpace(itemRoot)
                || !recordedManifest.Equals(normalizedManifest, StringComparison.OrdinalIgnoreCase)
                || !Path.GetFileName(itemRoot).Equals(record.Id, StringComparison.Ordinal)
                || !IsImmediateChild(itemRoot, quarantinedPath)
                || !IsInside(_quarantineRoot, quarantinedPath)
                || IsInside(_quarantineRoot, originalPath)
                || originalPath.StartsWith("\\\\", StringComparison.Ordinal)
                || HasAlternateDataStream(originalPath)
                || HasReparsePointInExistingChain(originalPath))
            {
                error = "隔离区 manifest 中的路径关系不可信，已拒绝。";
                return false;
            }

            if ((File.Exists(quarantinedPath) || Directory.Exists(quarantinedPath))
                && HasReparsePointInExistingChain(quarantinedPath))
            {
                error = "隔离副本路径经过重解析点，已拒绝。";
                return false;
            }

            return true;
        }
        catch
        {
            error = "隔离区 manifest 中的路径无法规范化。";
            return false;
        }
    }

    private static bool IsImmediateChild(string parent, string child)
    {
        var childParent = Path.GetDirectoryName(child);
        return !string.IsNullOrWhiteSpace(childParent)
            && Path.GetFullPath(childParent)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Equals(
                    Path.GetFullPath(parent)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasReparsePointInExistingChain(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrWhiteSpace(root))
            return true;

        var relative = fullPath[root.Length..];
        var current = root;
        foreach (var segment in relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (!File.Exists(current) && !Directory.Exists(current))
                continue;
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                return true;
        }
        return false;
    }

    private static bool HasAlternateDataStream(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath) ?? string.Empty;
        return fullPath[root.Length..].Contains(':');
    }

    private static QuarantineManifestInspection RefusedInspection(string summary) =>
        new()
        {
            Success = false,
            Summary = summary
        };

    private static bool HasUnexpectedTopLevelEntries(
        string itemRoot,
        QuarantineRecord record)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.GetFullPath(record.ManifestPath)
        };
        if (File.Exists(record.QuarantinedPath) || Directory.Exists(record.QuarantinedPath))
            allowed.Add(Path.GetFullPath(record.QuarantinedPath));

        return Directory.EnumerateFileSystemEntries(itemRoot)
            .Select(Path.GetFullPath)
            .Any(path => !allowed.Contains(path));
    }

    private static bool IsDeletionTreeSafe(string payloadPath, string itemRoot)
    {
        const int maximumEntries = 500_000;
        var pending = new Stack<string>();
        pending.Push(Path.GetFullPath(payloadPath));
        var visited = 0;
        while (pending.Count > 0)
        {
            if (++visited > maximumEntries)
                return false;

            var current = pending.Pop();
            if (!IsInside(itemRoot, current)
                || (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                return false;
            }

            if (!Directory.Exists(current))
                continue;
            foreach (var child in Directory.EnumerateFileSystemEntries(current))
                pending.Push(child);
        }
        return true;
    }

    private static void PurgeValidatedRecord(QuarantineRecord record)
    {
        var itemRoot = Path.GetDirectoryName(record.ManifestPath)
            ?? throw new InvalidOperationException("Quarantine item root is missing.");
        if (File.Exists(record.QuarantinedPath))
        {
            EnsureDeletePathSafe(record.QuarantinedPath, itemRoot);
            File.Delete(record.QuarantinedPath);
        }
        else if (Directory.Exists(record.QuarantinedPath))
        {
            DeleteDirectoryTree(record.QuarantinedPath, itemRoot);
        }

        EnsureDeletePathSafe(record.ManifestPath, itemRoot);
        File.Delete(record.ManifestPath);
        if (Directory.Exists(itemRoot)
            && !Directory.EnumerateFileSystemEntries(itemRoot).Any())
        {
            Directory.Delete(itemRoot);
        }
    }

    private static void DeleteDirectoryTree(string directory, string itemRoot)
    {
        var pending = new Stack<string>();
        var directories = new List<string>();
        pending.Push(directory);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            EnsureDeletePathSafe(current, itemRoot);
            directories.Add(current);
            foreach (var file in Directory.EnumerateFiles(current))
            {
                EnsureDeletePathSafe(file, itemRoot);
                File.Delete(file);
            }
            foreach (var child in Directory.EnumerateDirectories(current))
                pending.Push(child);
        }

        for (var index = directories.Count - 1; index >= 0; index--)
        {
            EnsureDeletePathSafe(directories[index], itemRoot);
            Directory.Delete(directories[index]);
        }
    }

    private static void EnsureDeletePathSafe(string path, string itemRoot)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
            throw new FileNotFoundException("Quarantine deletion target no longer exists.", path);
        if (!IsInside(itemRoot, path)
            || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidOperationException("Quarantine deletion target escaped its item root.");
        }
    }

    private static void TryRemoveUnmovedPlan(string itemRoot, string manifestPath)
    {
        try
        {
            if (File.Exists(manifestPath))
                File.Delete(manifestPath);
            if (Directory.Exists(itemRoot)
                && Directory.GetFileSystemEntries(itemRoot).Length == 0)
            {
                Directory.Delete(itemRoot);
            }
        }
        catch
        {
            // A stale plan is safer than deleting an item whose state is uncertain.
        }
    }
}
