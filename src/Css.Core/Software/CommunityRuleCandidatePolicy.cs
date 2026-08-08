namespace Css.Core.Software;

public sealed class CommunityRuleFileEvidence
{
    public required string Path { get; init; }
    public long SizeBytes { get; init; }
    public DateTimeOffset LastWriteTimeUtc { get; init; }
    public FileAttributes Attributes { get; init; }
}

public enum CommunityRuleCandidateDisposition
{
    PreviewOnly,
    EligibleForSafePreview,
    Refused
}

public enum CommunityRuleCandidateReason
{
    EligibleStaleCacheFiles,
    SystemApplication,
    ApplicationRunning,
    RuleWarning,
    IncompleteScan,
    IncompleteCandidateSet,
    RegistryIntentPresent,
    DirectoryRemovalIntentPresent,
    NoExactFiles,
    ApprovedUserDataUnavailable,
    OutsideApprovedUserData,
    InsideProtectedSystemLocation,
    InsideInstallLocation,
    MissingFile,
    ReparsePath,
    DuplicatePath,
    UnsafeFileAttributes,
    UnrecognizedCacheLocation,
    UnsupportedFileType,
    RecentFilesKept,
    NoEligibleStaleFiles
}

public sealed class CommunityRuleCandidateAssessment
{
    public CommunityRuleCandidateDisposition Disposition { get; init; }
    public required string Summary { get; init; }
    public required string Explanation { get; init; }
    public required IReadOnlyList<CommunityRuleCandidateReason> Reasons { get; init; }
    public required IReadOnlyList<CommunityRuleFileEvidence> EligibleFiles { get; init; }
    public long EligibleBytes { get; init; }
    public int SkippedRecentFileCount { get; init; }
    public int SkippedUnsupportedFileCount { get; init; }
    public bool IsExecutionAuthorized => false;
}

public static class CommunityRuleCandidatePolicy
{
    private static readonly HashSet<string> AllowedCacheDirectoryNames = new(
        ["Cache", "Caches", "Code Cache", "GPUCache", "ShaderCache", "DawnCache"],
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AllowedLowRiskExtensions = new(
        [".tmp", ".temp", ".cache", ".log", ".dmp", ".old"],
        StringComparer.OrdinalIgnoreCase);

    public static CommunityRuleCandidateAssessment Evaluate(
        SoftwareProfile profile,
        CommunityRuleCacheEvidence evidence,
        IReadOnlyList<string> approvedUserDataRoots,
        Func<string, bool> fileExists,
        Func<string, bool> isReparsePoint,
        DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(approvedUserDataRoots);
        ArgumentNullException.ThrowIfNull(fileExists);
        ArgumentNullException.ThrowIfNull(isReparsePoint);

        if (profile.Category == SoftwareCategory.SystemTool)
            return Refused(CommunityRuleCandidateReason.SystemApplication,
                "这是系统相关应用，只保留观察。",
                "Agent 不会把系统工具的扩展规则发现晋级为清理候选。");
        if (profile.RunningProcesses.Count > 0)
            return PreviewOnly(CommunityRuleCandidateReason.ApplicationRunning,
                "应用仍在运行，暂不进入安全预演。",
                "请正常关闭应用后重新扫描，避免把正在使用的缓存当成旧文件。");
        if (!string.IsNullOrWhiteSpace(evidence.Warning))
            return PreviewOnly(CommunityRuleCandidateReason.RuleWarning,
                "规则带有特别警告，只提供查看。",
                "这条社区规则要求额外人工判断，OMNIX 不会自动提高处理权限。");
        if (evidence.IsSizeLowerBound
            || evidence.UnresolvedTargetCount > 0
            || evidence.AccessErrorCount > 0
            || evidence.SkippedReparsePointCount > 0
            || evidence.RejectedPathCount > 0)
        {
            return PreviewOnly(CommunityRuleCandidateReason.IncompleteScan,
                "扫描证据不完整，只提供查看。",
                "有路径未解析、无法访问、被安全跳过或达到扫描上限，不能据此生成完整候选。");
        }
        if (!evidence.CandidateFilesComplete)
            return PreviewOnly(CommunityRuleCandidateReason.IncompleteCandidateSet,
                "精确文件较多，只保留统计和查看。",
                "OMNIX 没有保留一份完整且受限的精确文件清单，因此不会生成部分处理方案。");
        if (evidence.RegistryTargetCount > 0)
            return PreviewOnly(CommunityRuleCandidateReason.RegistryIntentPresent,
                "规则同时涉及注册表，只提供查看。",
                "社区规则中的注册表意图不会被导入 OMNIX 的清理权限。");
        if (evidence.IncludesRemoveSelf)
            return PreviewOnly(CommunityRuleCandidateReason.DirectoryRemovalIntentPresent,
                "规则包含整目录处理意图，只提供查看。",
                "OMNIX 不会沿用社区规则的整目录删除语义。");
        if (evidence.CandidateFiles.Count == 0)
            return PreviewOnly(CommunityRuleCandidateReason.NoExactFiles,
                "没有完整的精确文件清单。",
                "当前发现仍可用于解释占用，但不能进入安全预演。");

        var approvedRoots = CanonicalRoots(approvedUserDataRoots);
        if (approvedRoots.Count == 0)
            return PreviewOnly(CommunityRuleCandidateReason.ApprovedUserDataUnavailable,
                "无法确认常规的个人应用缓存位置。",
                "Agent 无法证明这些路径属于当前用户数据区，因此停止晋级。");

        var installPath = TryCanonical(profile.InstallPath);
        var protectedRoots = ProtectedRoots();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var eligible = new List<CommunityRuleFileEvidence>();
        var reasons = new HashSet<CommunityRuleCandidateReason>();
        var skippedRecent = 0;
        var skippedUnsupported = 0;
        var cutoff = utcNow - TimeSpan.FromDays(Math.Clamp(evidence.StaleThresholdDays, 1, 3650));

        foreach (var file in evidence.CandidateFiles)
        {
            var canonical = TryCanonical(file.Path);
            if (canonical is null)
                return Refused(CommunityRuleCandidateReason.OutsideApprovedUserData,
                    "发现了无法验证的文件路径，已拒绝。",
                    "路径不能被规范化，Agent 不会猜测它实际指向哪里。");
            if (!seen.Add(canonical))
                return Refused(CommunityRuleCandidateReason.DuplicatePath,
                    "精确文件清单包含重复项，已拒绝。",
                    "重复路径可能造成重复处理，必须重新扫描。");

            var approvedRoot = approvedRoots.FirstOrDefault(root => IsInside(root, canonical));
            if (approvedRoot is null)
                return Refused(CommunityRuleCandidateReason.OutsideApprovedUserData,
                    "文件不在常规的个人应用缓存位置，已拒绝。",
                    "社区规则不能把系统区、共享区或未知位置变成清理候选。");
            if (protectedRoots.Any(root => IsInsideOrEqual(root, canonical)))
                return Refused(CommunityRuleCandidateReason.InsideProtectedSystemLocation,
                    "文件位于受保护的系统或程序区域，已拒绝。",
                    "Windows、Program Files 和共享系统数据不会由社区规则晋级。");
            if (installPath is not null && IsInsideOrEqual(installPath, canonical))
                return Refused(CommunityRuleCandidateReason.InsideInstallLocation,
                    "文件位于应用安装目录，已拒绝。",
                    "主程序文件不能因为目录名像缓存就进入清理候选。");
            if (!SafeFileExists(fileExists, canonical))
                return PreviewOnly(CommunityRuleCandidateReason.MissingFile,
                    "文件状态已经变化，请重新扫描。",
                    "至少一个文件在复核时已不存在或无法确认。");
            if (HasReparsePoint(approvedRoot, canonical, isReparsePoint))
                return Refused(CommunityRuleCandidateReason.ReparsePath,
                    "路径经过链接或重定向，已拒绝。",
                    "Agent 无法证明链接后的真实位置仍在批准区域内。");
            if ((file.Attributes & (FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Device)) != 0)
                return Refused(CommunityRuleCandidateReason.UnsafeFileAttributes,
                    "文件带有不适合自动处理的系统属性，已拒绝。",
                    "系统、设备或链接文件不会进入安全预演。");
            if (!HasRecognizedCacheDirectory(approvedRoot, canonical))
            {
                skippedUnsupported++;
                reasons.Add(CommunityRuleCandidateReason.UnrecognizedCacheLocation);
                continue;
            }
            if (!AllowedLowRiskExtensions.Contains(Path.GetExtension(canonical)))
            {
                skippedUnsupported++;
                reasons.Add(CommunityRuleCandidateReason.UnsupportedFileType);
                continue;
            }
            if (file.LastWriteTimeUtc > cutoff)
            {
                skippedRecent++;
                reasons.Add(CommunityRuleCandidateReason.RecentFilesKept);
                continue;
            }

            eligible.Add(new CommunityRuleFileEvidence
            {
                Path = canonical,
                SizeBytes = Math.Max(0, file.SizeBytes),
                LastWriteTimeUtc = file.LastWriteTimeUtc,
                Attributes = file.Attributes
            });
        }

        if (eligible.Count == 0)
        {
            reasons.Add(CommunityRuleCandidateReason.NoEligibleStaleFiles);
            return new CommunityRuleCandidateAssessment
            {
                Disposition = CommunityRuleCandidateDisposition.PreviewOnly,
                Summary = "没有文件通过第一轮安全筛选。",
                Explanation = "近期文件和无法确认用途的文件都会保留；当前不会生成处理方案。",
                Reasons = reasons.Order().ToArray(),
                EligibleFiles = [],
                SkippedRecentFileCount = skippedRecent,
                SkippedUnsupportedFileCount = skippedUnsupported
            };
        }

        var bytes = eligible.Aggregate(0L, (total, file) => SaturatingAdd(total, file.SizeBytes));
        return new CommunityRuleCandidateAssessment
        {
            Disposition = CommunityRuleCandidateDisposition.EligibleForSafePreview,
            Summary = $"{eligible.Count} 个旧缓存文件通过第一轮筛选，可进入安全预演。",
            Explanation = "这仍不是清理命令；下一步必须重新核验文件身份、显示影响并准备隔离回滚。",
            Reasons = [CommunityRuleCandidateReason.EligibleStaleCacheFiles, .. reasons.Order()],
            EligibleFiles = eligible,
            EligibleBytes = bytes,
            SkippedRecentFileCount = skippedRecent,
            SkippedUnsupportedFileCount = skippedUnsupported
        };
    }

    private static CommunityRuleCandidateAssessment PreviewOnly(
        CommunityRuleCandidateReason reason,
        string summary,
        string explanation) =>
        Result(CommunityRuleCandidateDisposition.PreviewOnly, reason, summary, explanation);

    private static CommunityRuleCandidateAssessment Refused(
        CommunityRuleCandidateReason reason,
        string summary,
        string explanation) =>
        Result(CommunityRuleCandidateDisposition.Refused, reason, summary, explanation);

    private static CommunityRuleCandidateAssessment Result(
        CommunityRuleCandidateDisposition disposition,
        CommunityRuleCandidateReason reason,
        string summary,
        string explanation) =>
        new()
        {
            Disposition = disposition,
            Summary = summary,
            Explanation = explanation,
            Reasons = [reason],
            EligibleFiles = []
        };

    private static IReadOnlyList<string> CanonicalRoots(IEnumerable<string> paths) =>
        paths.Select(TryCanonical)
            .Where(path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static IReadOnlyList<string> ProtectedRoots() =>
        new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
        }
        .Select(TryCanonical)
        .Where(path => path is not null)
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private static bool HasRecognizedCacheDirectory(string root, string file)
    {
        var current = Path.GetDirectoryName(file);
        while (!string.IsNullOrWhiteSpace(current) && IsInsideOrEqual(root, current))
        {
            if (AllowedCacheDirectoryNames.Contains(Path.GetFileName(current))) return true;
            if (current.Equals(root, StringComparison.OrdinalIgnoreCase)) break;
            current = Path.GetDirectoryName(current);
        }
        return false;
    }

    private static bool HasReparsePoint(string root, string file, Func<string, bool> probe)
    {
        var current = file;
        while (IsInsideOrEqual(root, current))
        {
            try
            {
                if (probe(current)) return true;
            }
            catch
            {
                return true;
            }
            if (current.Equals(root, StringComparison.OrdinalIgnoreCase)) return false;
            current = Path.GetDirectoryName(current) ?? string.Empty;
        }
        return true;
    }

    private static bool SafeFileExists(Func<string, bool> probe, string path)
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

    private static string? TryCanonical(string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)) return null;
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private static bool IsInside(string root, string path) =>
        path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static bool IsInsideOrEqual(string root, string path) =>
        root.Equals(path, StringComparison.OrdinalIgnoreCase) || IsInside(root, path);

    private static long SaturatingAdd(long left, long right) =>
        right >= long.MaxValue - left ? long.MaxValue : left + right;
}
