using System.Collections.Generic;
using Css.Core.Operations;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class AppCacheCleanupPlan
{
    public required string Summary { get; init; }
    public required string NextStepText { get; init; }
    public required string SafetyText { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public bool CanContinue => Operation is not null;
}

public static class AppCacheCleanupPlanBuilder
{
    public const string OperationKind = "app.cache.quarantine";
    public const int MaxCachePathCount = 32;

    private static readonly HashSet<string> AllowedCacheFolderNames = new(
        ["Cache", "Caches", "Code Cache", "GPUCache", "ShaderCache", "DawnCache"],
        StringComparer.OrdinalIgnoreCase);

    public static AppCacheCleanupPlan Create(
        SoftwareProfile profile,
        IReadOnlyList<string> approvedUserDataRoots,
        Func<string, bool> directoryExists,
        Func<string, bool> isReparsePoint,
        Func<string, long>? sizeResolver = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(approvedUserDataRoots);
        ArgumentNullException.ThrowIfNull(directoryExists);
        ArgumentNullException.ThrowIfNull(isReparsePoint);

        if (profile.Category == SoftwareCategory.SystemTool)
            return Refused("这是系统相关应用，缓存只解释，不提供一键处理。", "系统相关数据默认保留，避免影响 Windows 或硬件功能。");

        if (profile.RunningProcesses.Count > 0)
            return Refused("这个应用还在运行，暂不处理缓存。", "请先正常关闭应用，再重新扫描并生成方案。");

        var distinctCandidates = profile.CachePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (distinctCandidates.Length == 0)
            return Refused("暂未找到可以证明为缓存的目录。", "先重新扫描；只有明确的缓存目录才会进入隔离方案。");

        if (distinctCandidates.Length > MaxCachePathCount)
            return Refused("缓存候选过多，已停止生成一键方案。", "请重新扫描或逐项复核，OMNIX-Entropy 不会截断后只处理一部分。");

        var approvedRoots = CanonicalRoots(approvedUserDataRoots);
        var safePaths = new List<string>();
        foreach (var candidate in distinctCandidates)
        {
            if (TryValidatePath(candidate, approvedRoots, directoryExists, isReparsePoint, out var canonical))
                safePaths.Add(canonical);
        }

        if (safePaths.Count == 0)
            return Refused("当前缓存候选没有通过本地安全校验。", "它们可能越出当前用户数据区、已经消失，或经过了链接目录；因此不会处理。");

        if (HasOverlappingPaths(safePaths))
            return Refused("缓存候选互相包含，已停止生成方案。", "为避免同一批文件被重复处理，需要重新扫描后再判断。");

        var estimatedBytes = 0L;
        foreach (var path in safePaths)
            estimatedBytes = SaturatingAdd(estimatedBytes, SafeSize(sizeResolver, path));

        var skipped = distinctCandidates.Length - safePaths.Count;
        var evidence = $"{profile.Name} 有 {safePaths.Count} 个缓存目录通过当前用户数据区校验";
        var operation = new OperationDescriptor
        {
            Kind = OperationKind,
            Title = $"清理 {profile.Name} 缓存",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RequiresSnapshot = false,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = evidence,
            EstimatedImpactBytes = estimatedBytes,
            ConfirmationText = $"确认将 {profile.Name} 的低风险缓存移动到隔离区？",
            AffectedPaths = safePaths
        };

        return new AppCacheCleanupPlan
        {
            Summary = skipped == 0
                ? $"已确认 {safePaths.Count} 个低风险缓存位置，可以进入隔离方案。"
                : $"{safePaths.Count} 个缓存位置通过校验，另有 {skipped} 个暂不处理。",
            NextStepText = "下一步：再次确认后移动到隔离区；完成后可在后悔药中心还原。",
            SafetyText = "不会永久删除，也不会处理用户数据、安装目录、注册表、服务或自启动。",
            Lines =
            [
                "已确认候选位于当前用户数据区域。",
                "已排除系统应用、运行中应用和链接目录。",
                "执行前会重新扫描应用并再次校验同一批位置。"
            ],
            Operation = operation
        };
    }

    public static OperationResult ValidateForExecution(
        OperationDescriptor operation,
        IReadOnlyList<string> approvedUserDataRoots,
        Func<string, bool> directoryExists,
        Func<string, bool> isReparsePoint)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(approvedUserDataRoots);
        ArgumentNullException.ThrowIfNull(directoryExists);
        ArgumentNullException.ThrowIfNull(isReparsePoint);
        if (!operation.Kind.Equals(OperationKind, StringComparison.OrdinalIgnoreCase))
            return OperationResult.Fail("缓存操作类型不匹配。");

        var candidates = operation.AffectedPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (candidates.Length == 0
            || candidates.Length > MaxCachePathCount
            || candidates.Length != operation.AffectedPaths.Count)
            return OperationResult.Fail("缓存位置数量没有通过安全校验。");

        var approvedRoots = CanonicalRoots(approvedUserDataRoots);
        var canonicalPaths = new List<string>();
        foreach (var candidate in candidates)
        {
            if (!TryValidatePath(candidate, approvedRoots, directoryExists, isReparsePoint, out var canonical))
                return OperationResult.Fail("至少一个缓存位置已变化或不再安全。");
            canonicalPaths.Add(canonical);
        }

        if (HasOverlappingPaths(canonicalPaths))
            return OperationResult.Fail("缓存位置互相包含，已拒绝处理。");

        return OperationResult.Ok("缓存位置通过执行前复核。");
    }

    public static bool MatchesCurrentProfile(
        SoftwareProfile profile,
        OperationDescriptor operation)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(operation);
        if (profile.Category == SoftwareCategory.SystemTool
            || profile.RunningProcesses.Count > 0
            || !operation.Kind.Equals(OperationKind, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var currentCachePaths = profile.CachePaths
            .Select(TryCanonicalPath)
            .Where(path => path is not null)
            .Cast<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return operation.AffectedPaths.Count > 0
            && operation.AffectedPaths.All(path =>
            {
                var canonical = TryCanonicalPath(path);
                return canonical is not null && currentCachePaths.Contains(canonical);
            });
    }

    private static AppCacheCleanupPlan Refused(string summary, string nextStep) =>
        new()
        {
            Summary = summary,
            NextStepText = nextStep,
            SafetyText = "没有生成可执行操作，也不会移动或删除任何文件。",
            Lines = ["Agent 选择了停止，而不是根据名称或路径猜测。"]
        };

    private static IReadOnlyList<string> CanonicalRoots(IEnumerable<string> roots) =>
        roots
            .Select(TryCanonicalPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static bool TryValidatePath(
        string candidate,
        IReadOnlyList<string> approvedRoots,
        Func<string, bool> directoryExists,
        Func<string, bool> isReparsePoint,
        out string canonical)
    {
        canonical = TryCanonicalPath(candidate) ?? string.Empty;
        if (canonical.Length == 0
            || !Path.IsPathRooted(canonical)
            || !AllowedCacheFolderNames.Contains(Path.GetFileName(canonical)))
            return false;

        var validatedPath = canonical;
        var approvedRoot = approvedRoots.FirstOrDefault(root => IsInside(root, validatedPath));
        if (approvedRoot is null || !SafeCall(directoryExists, validatedPath))
            return false;

        var current = validatedPath;
        while (true)
        {
            if (SafeIsReparsePoint(isReparsePoint, current))
                return false;
            if (current.Equals(approvedRoot, StringComparison.OrdinalIgnoreCase))
                return true;

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrWhiteSpace(parent) || !IsInsideOrEqual(approvedRoot, parent))
                return false;
            current = parent;
        }
    }

    private static bool HasOverlappingPaths(IReadOnlyList<string> paths) =>
        paths.Any(path => paths.Any(other =>
            !path.Equals(other, StringComparison.OrdinalIgnoreCase)
            && IsInside(path, other)));

    private static string? TryCanonicalPath(string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsInside(string root, string path)
    {
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInsideOrEqual(string root, string path) =>
        path.Equals(root, StringComparison.OrdinalIgnoreCase) || IsInside(root, path);

    private static bool SafeCall(Func<string, bool> probe, string path)
    {
        try
        {
            return probe(path);
        }
        catch
        {
            return false;
        }
    }

    private static bool SafeIsReparsePoint(Func<string, bool> probe, string path)
    {
        try
        {
            return probe(path);
        }
        catch
        {
            return true;
        }
    }

    private static long SafeSize(Func<string, long>? resolver, string path)
    {
        try
        {
            return Math.Max(0, resolver?.Invoke(path) ?? 0);
        }
        catch
        {
            return 0;
        }
    }

    private static long SaturatingAdd(long left, long right) =>
        right >= long.MaxValue - left ? long.MaxValue : left + right;
}
