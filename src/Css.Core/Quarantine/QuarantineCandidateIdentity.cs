using Css.Core.Operations;

namespace Css.Core.Quarantine;

public enum QuarantineCandidateKind
{
    File,
    Directory
}

public sealed record QuarantineCandidateEvidence
{
    public required string CanonicalPath { get; init; }
    public required QuarantineCandidateKind Kind { get; init; }
    public required ulong VolumeSerialNumber { get; init; }
    public required ulong FileId { get; init; }
    public required long CreationTimeUtcTicks { get; init; }
    public required long LastWriteTimeUtcTicks { get; init; }
    public required long LengthBytes { get; init; }
}

public sealed class QuarantineCandidateInspection
{
    public bool Success { get; init; }
    public required string Summary { get; init; }
    public QuarantineCandidateEvidence? Evidence { get; init; }

    public static QuarantineCandidateInspection Accepted(QuarantineCandidateEvidence evidence) =>
        new()
        {
            Success = true,
            Summary = "隔离候选身份已读取。",
            Evidence = evidence
        };

    public static QuarantineCandidateInspection Refused(string summary) =>
        new()
        {
            Success = false,
            Summary = summary
        };
}

public interface IQuarantineCandidateIdentityReader
{
    QuarantineCandidateInspection Inspect(string path);
}

public sealed class QuarantineOperationPreparationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public OperationDescriptor? Operation { get; init; }

    public static QuarantineOperationPreparationResult Accepted(OperationDescriptor operation) =>
        new() { Success = true, Operation = operation };

    public static QuarantineOperationPreparationResult Refused(string error) =>
        new() { Success = false, Error = error };
}

public static class QuarantineCandidatePathPolicy
{
    public const int MaximumCandidateCount = 64;

    public static bool TryNormalizeBatch(
        IReadOnlyList<string> paths,
        string quarantineRoot,
        out IReadOnlyList<string> normalizedPaths,
        out string error)
    {
        normalizedPaths = [];
        error = string.Empty;
        try
        {
            if (paths.Count is 0 or > MaximumCandidateCount)
            {
                error = "隔离候选数量超出安全范围。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(quarantineRoot)
                || !Path.IsPathFullyQualified(quarantineRoot))
            {
                error = "隔离区路径无效。";
                return false;
            }

            var normalizedRoot = Normalize(quarantineRoot);
            if (normalizedRoot.StartsWith("\\\\", StringComparison.Ordinal)
                || HasAlternateDataStream(normalizedRoot)
                || IsVolumeRoot(normalizedRoot)
                || (File.Exists(normalizedRoot) && !Directory.Exists(normalizedRoot))
                || HasReparsePointInExistingChain(normalizedRoot))
            {
                error = "隔离区本身不是安全的本地目录。";
                return false;
            }
            var normalized = new List<string>(paths.Count);
            var distinct = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in paths)
            {
                if (!TryNormalizeLocalPath(path, out var candidate, out error))
                    return false;
                if (!distinct.Add(candidate))
                {
                    error = "隔离候选包含重复路径。";
                    return false;
                }
                if (IsVolumeRoot(candidate))
                {
                    error = "不能把整个磁盘根目录移入隔离区。";
                    return false;
                }
                if (IsProtectedRoot(candidate))
                {
                    error = "不能把 Windows、程序目录或用户数据根目录整体移入隔离区。";
                    return false;
                }
                if (IsInsideOrEqual(normalizedRoot, candidate)
                    || IsInsideOrEqual(candidate, normalizedRoot))
                {
                    error = "隔离候选与隔离区互相包含。";
                    return false;
                }
                normalized.Add(candidate);
            }

            for (var left = 0; left < normalized.Count; left++)
            {
                for (var right = left + 1; right < normalized.Count; right++)
                {
                    if (IsInsideOrEqual(normalized[left], normalized[right])
                        || IsInsideOrEqual(normalized[right], normalized[left]))
                    {
                        error = "隔离候选路径互相包含。";
                        return false;
                    }
                }
            }

            normalizedPaths = normalized;
            return true;
        }
        catch
        {
            error = "隔离候选路径无法完成安全规范化。";
            return false;
        }
    }

    public static bool TryInspectCurrentPath(
        string path,
        out string canonicalPath,
        out QuarantineCandidateKind kind,
        out string error)
    {
        canonicalPath = string.Empty;
        kind = default;
        if (!TryNormalizeLocalPath(path, out canonicalPath, out error))
            return false;

        try
        {
            var isFile = File.Exists(canonicalPath);
            var isDirectory = Directory.Exists(canonicalPath);
            if (isFile == isDirectory)
            {
                error = "隔离候选已经不存在或类型不明确。";
                return false;
            }
            if (HasReparsePointInExistingChain(canonicalPath))
            {
                error = "隔离候选路径经过重解析点 (reparse path)，已拒绝。";
                return false;
            }

            kind = isDirectory ? QuarantineCandidateKind.Directory : QuarantineCandidateKind.File;
            return true;
        }
        catch
        {
            error = "隔离候选当前无法完成安全检查。";
            return false;
        }
    }

    public static bool HasReparsePointInExistingChain(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrWhiteSpace(root))
            return true;

        var current = root;
        foreach (var segment in fullPath[root.Length..].Split(
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

    public static bool HasAlternateDataStream(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath) ?? string.Empty;
        return fullPath[root.Length..].Contains(':');
    }

    private static bool TryNormalizeLocalPath(string? path, out string normalized, out string error)
    {
        normalized = string.Empty;
        error = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            {
                error = "隔离候选必须是完整的本地路径。";
                return false;
            }
            normalized = Normalize(path);
            if (normalized.StartsWith("\\\\", StringComparison.Ordinal)
                || HasAlternateDataStream(normalized))
            {
                error = "隔离候选不能是网络路径或备用数据流。";
                return false;
            }
            return true;
        }
        catch
        {
            error = "隔离候选路径无效。";
            return false;
        }
    }

    private static bool IsVolumeRoot(string path)
    {
        var root = Path.GetPathRoot(path);
        return !string.IsNullOrWhiteSpace(root)
            && Normalize(root).Equals(Normalize(path), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProtectedRoot(string path)
    {
        var protectedRoots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        };
        return protectedRoots
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(Normalize)
            .Any(root => root.Equals(path, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool IsInsideOrEqual(string root, string path)
    {
        if (root.Equals(path, StringComparison.OrdinalIgnoreCase))
            return true;
        return path.StartsWith(
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }
}

public static class QuarantineCandidateEvidencePolicy
{
    public static OperationResult Revalidate(
        IReadOnlyList<string> paths,
        IReadOnlyList<QuarantineCandidateEvidence> expected,
        string quarantineRoot,
        IQuarantineCandidateIdentityReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (!QuarantineCandidatePathPolicy.TryNormalizeBatch(
                paths,
                quarantineRoot,
                out var normalized,
                out var error))
        {
            return OperationResult.Fail(error);
        }
        if (expected.Count != normalized.Count)
            return OperationResult.Fail("隔离候选身份数量与当前方案不一致。");

        for (var index = 0; index < normalized.Count; index++)
        {
            var prior = expected[index];
            if (string.IsNullOrWhiteSpace(prior.CanonicalPath)
                || !prior.CanonicalPath.Equals(normalized[index], StringComparison.OrdinalIgnoreCase))
                return Changed();

            var current = reader.Inspect(normalized[index]);
            if (!current.Success || current.Evidence is null)
                return OperationResult.Fail(current.Summary);
            if (!SameIdentity(prior, current.Evidence))
                return Changed();
        }
        return OperationResult.Ok("隔离候选身份仍与确认时一致。");
    }

    public static bool SameIdentity(
        QuarantineCandidateEvidence expected,
        QuarantineCandidateEvidence current) =>
        !string.IsNullOrWhiteSpace(expected.CanonicalPath)
        && !string.IsNullOrWhiteSpace(current.CanonicalPath)
        && expected.CanonicalPath.Equals(current.CanonicalPath, StringComparison.OrdinalIgnoreCase)
        && expected.Kind == current.Kind
        && expected.VolumeSerialNumber == current.VolumeSerialNumber
        && expected.FileId == current.FileId
        && expected.CreationTimeUtcTicks == current.CreationTimeUtcTicks
        && expected.LastWriteTimeUtcTicks == current.LastWriteTimeUtcTicks
        && expected.LengthBytes == current.LengthBytes;

    private static OperationResult Changed() =>
        OperationResult.Fail("至少一个隔离候选在确认后发生变化，旧方案已停止。");
}
