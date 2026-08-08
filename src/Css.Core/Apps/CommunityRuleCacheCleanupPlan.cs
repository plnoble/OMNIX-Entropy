using System.Collections.ObjectModel;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class CommunityRuleCacheCleanupPlan
{
    public required string Summary { get; init; }
    public required string NextStepText { get; init; }
    public required string SafetyText { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public int EligibleFileCount { get; init; }
    public long EligibleBytes { get; init; }
    public int SkippedRecentFileCount { get; init; }
    public int SkippedUnsupportedFileCount { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public bool CanContinue => Operation is not null;
}

public sealed record CommunityRuleProfileBinding
{
    public required string Name { get; init; }
    public required string Publisher { get; init; }
    public required string DisplayVersion { get; init; }
    public required string InventorySource { get; init; }
    public required string InstallPath { get; init; }
}

public sealed record CommunityRulePlannedFileBinding
{
    public required string Path { get; init; }
    public long SizeBytes { get; init; }
    public long LastWriteTimeUtcTicks { get; init; }
    public FileAttributes Attributes { get; init; }
    public required IReadOnlyList<string> RuleNames { get; init; }
}

public sealed record CommunityRuleCacheCleanupBinding
{
    public required string ActiveRulePackSha256 { get; init; }
    public required CommunityRuleProfileBinding Profile { get; init; }
    public required IReadOnlyList<CommunityRulePlannedFileBinding> Files { get; init; }
}

public static class CommunityRuleCacheCleanupPlanBuilder
{
    public const string OperationKind = "app.community-cache.quarantine";
    public const string BindingArgument = "community-cache.binding";

    public static CommunityRuleCacheCleanupPlan Create(
        SoftwareProfile profile,
        string activeRulePackSha256)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!IsSha256(activeRulePackSha256))
            return Refused("扩展规则版本无法确认，已停止生成方案。", "重新打开扩展规则中心并确认当前版本后再扫描。");
        if (profile.Category == SoftwareCategory.SystemTool)
            return Refused("这是系统相关应用，只保留查看。", "系统组件不会由社区规则生成缓存处理方案。");
        if (profile.RunningProcesses.Count > 0)
            return Refused("应用仍在运行，暂不处理缓存。", "请正常关闭应用，再重新扫描并生成方案。");

        var eligibleEvidence = profile.CommunityCacheEvidence
            .Where(item => item.CandidateAssessment?.Disposition
                == CommunityRuleCandidateDisposition.EligibleForSafePreview)
            .ToArray();
        if (eligibleEvidence.Length == 0)
            return Refused("没有通过安全筛选的精确旧缓存文件。", "保留当前发现，稍后重新扫描；不会根据规则名称猜测处理。");
        if (eligibleEvidence.Any(item =>
                !item.RulePackSha256.Equals(activeRulePackSha256, StringComparison.OrdinalIgnoreCase)))
        {
            return Refused("扩展规则版本已经变化，旧结果已停止。", "请重新扫描应用，让新规则重新生成证据。");
        }

        var files = new Dictionary<string, MutableFileBinding>(StringComparer.OrdinalIgnoreCase);
        foreach (var evidence in eligibleEvidence)
        {
            var assessment = evidence.CandidateAssessment!;
            if (!evidence.CandidateFilesComplete
                || assessment.EligibleFiles.Count == 0
                || assessment.IsExecutionAuthorized)
            {
                return Refused("精确文件证据不完整，已停止生成方案。", "请重新扫描；OMNIX 不会只处理截断后的部分文件。");
            }

            foreach (var candidate in assessment.EligibleFiles)
            {
                var canonical = TryCanonical(candidate.Path);
                if (canonical is null)
                    return Refused("发现无法确认的文件路径，已停止生成方案。", "请重新扫描，不要手动拼接或修改候选路径。");

                var current = new MutableFileBinding(
                    canonical,
                    Math.Max(0, candidate.SizeBytes),
                    candidate.LastWriteTimeUtc.UtcDateTime.Ticks,
                    candidate.Attributes,
                    [evidence.RuleName]);
                if (!files.TryGetValue(canonical, out var existing))
                {
                    files.Add(canonical, current);
                    continue;
                }

                if (existing.SizeBytes != current.SizeBytes
                    || existing.LastWriteTimeUtcTicks != current.LastWriteTimeUtcTicks
                    || existing.Attributes != current.Attributes)
                {
                    return Refused("不同规则对同一文件的证据不一致，已停止生成方案。", "请更新规则或重新扫描，OMNIX 不会猜测哪一条记录正确。");
                }

                existing.RuleNames.Add(evidence.RuleName);
            }
        }

        if (files.Count == 0)
            return Refused("没有完整的精确文件可以进入方案。", "保留当前统计，不会执行任何处理。");
        if (files.Count > QuarantineCandidatePathPolicy.MaximumCandidateCount)
        {
            return Refused(
                $"本次有 {files.Count} 个文件，超过单次安全处理上限。",
                "请等待更窄的规则或重新扫描；OMNIX 不会截断后只处理前一部分。");
        }

        var plannedFiles = files.Values
            .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .Select(item => new CommunityRulePlannedFileBinding
            {
                Path = item.Path,
                SizeBytes = item.SizeBytes,
                LastWriteTimeUtcTicks = item.LastWriteTimeUtcTicks,
                Attributes = item.Attributes,
                RuleNames = item.RuleNames
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            })
            .ToArray();
        var eligibleBytes = plannedFiles.Aggregate(0L, (total, item) => SaturatingAdd(total, item.SizeBytes));
        var skippedRecent = SaturatingSum(eligibleEvidence.Select(item =>
            item.CandidateAssessment!.SkippedRecentFileCount));
        var skippedUnsupported = SaturatingSum(eligibleEvidence.Select(item =>
            item.CandidateAssessment!.SkippedUnsupportedFileCount));
        var binding = new CommunityRuleCacheCleanupBinding
        {
            ActiveRulePackSha256 = activeRulePackSha256.ToUpperInvariant(),
            Profile = BindProfile(profile),
            Files = plannedFiles
        };
        var arguments = new ReadOnlyDictionary<string, object?>(
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [BindingArgument] = binding
            });
        var operation = new OperationDescriptor
        {
            Kind = OperationKind,
            Title = $"处理 {profile.Name} 的旧缓存文件",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RequiresSnapshot = false,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = $"{profile.Name} 有 {plannedFiles.Length} 个精确旧缓存文件通过当前扩展规则安全筛选",
            EstimatedImpactBytes = eligibleBytes,
            ConfirmationText = $"确认将 {profile.Name} 的 {plannedFiles.Length} 个旧缓存文件移动到隔离区？",
            AffectedPaths = plannedFiles.Select(item => item.Path).ToArray(),
            Arguments = arguments
        };

        var lines = new List<string>
        {
            $"将复核 {plannedFiles.Length} 个精确旧缓存文件，预计释放 {FormatBytes(eligibleBytes)}。",
            $"证据来自当前规则包中的 {eligibleEvidence.Length} 条已归属规则；重叠文件只计算一次。"
        };
        if (skippedRecent > 0)
            lines.Add($"另有 {skippedRecent} 个近期文件继续保留。规则之间可能重叠，不把它当成释放量。");
        if (skippedUnsupported > 0)
            lines.Add($"另有 {skippedUnsupported} 个用途或类型不够明确的文件继续保留。");
        lines.Add("点击下一步后还会重新扫描应用、核对规则版本并绑定每个文件的当前身份。");

        return new CommunityRuleCacheCleanupPlan
        {
            Summary = $"{plannedFiles.Length} 个旧缓存文件可以进入安全预演，预计释放 {FormatBytes(eligibleBytes)}。",
            NextStepText = "下一步：重新核验后查看最终清单；确认后只移动到隔离区。",
            SafetyText = "不会处理近期文件、未知类型、安装目录、注册表、服务或自启动；可以在后悔药中心还原。",
            Lines = lines,
            EligibleFileCount = plannedFiles.Length,
            EligibleBytes = eligibleBytes,
            SkippedRecentFileCount = skippedRecent,
            SkippedUnsupportedFileCount = skippedUnsupported,
            Operation = operation
        };
    }

    public static OperationResult ValidateForExecution(
        OperationDescriptor operation,
        SoftwareProfile currentProfile,
        string currentActiveRulePackSha256)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(currentProfile);
        if (!operation.Kind.Equals(OperationKind, StringComparison.OrdinalIgnoreCase))
            return OperationResult.Fail("扩展缓存操作类型不匹配。");
        if (!TryGetBinding(operation, out var binding))
            return OperationResult.Fail("扩展缓存方案缺少规则和应用身份绑定。");
        if (!IsSha256(currentActiveRulePackSha256)
            || !binding.ActiveRulePackSha256.Equals(currentActiveRulePackSha256, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult.Fail("扩展规则版本已经变化，请重新扫描。");
        }
        if (!MatchesProfile(binding.Profile, currentProfile))
            return OperationResult.Fail("应用登记身份已经变化，请重新选择并扫描。");
        if (currentProfile.RunningProcesses.Count > 0)
            return OperationResult.Fail("应用仍在运行，请关闭后重新扫描。");

        var fresh = Create(currentProfile, currentActiveRulePackSha256);
        if (!fresh.CanContinue || fresh.Operation is null || !TryGetBinding(fresh.Operation, out var freshBinding))
            return OperationResult.Fail("当前证据不再符合扩展缓存安全预演条件。");
        if (!SameFiles(binding.Files, freshBinding.Files)
            || !operation.AffectedPaths.SequenceEqual(
                binding.Files.Select(item => item.Path),
                StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult.Fail("精确旧缓存文件集合已经变化，请重新扫描。");
        }
        return OperationResult.Ok("扩展规则、应用身份和精确文件集合通过复核。");
    }

    public static bool TryGetBinding(
        OperationDescriptor operation,
        out CommunityRuleCacheCleanupBinding binding)
    {
        binding = null!;
        if (!operation.Arguments.TryGetValue(BindingArgument, out var value)
            || value is not CommunityRuleCacheCleanupBinding typed)
        {
            return false;
        }
        binding = typed;
        return true;
    }

    public static bool TryResolveBoundProfile(
        OperationDescriptor operation,
        IReadOnlyList<SoftwareProfile> profiles,
        out SoftwareProfile? profile)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(profiles);
        profile = null;
        if (!TryGetBinding(operation, out var binding)) return false;
        var matches = profiles.Where(candidate => MatchesProfile(binding.Profile, candidate)).Take(2).ToArray();
        if (matches.Length != 1) return false;
        profile = matches[0];
        return true;
    }

    private static CommunityRuleProfileBinding BindProfile(SoftwareProfile profile) =>
        new()
        {
            Name = NormalizeText(profile.Name),
            Publisher = NormalizeText(profile.Publisher),
            DisplayVersion = NormalizeText(profile.DisplayVersion),
            InventorySource = NormalizeText(profile.InventorySource),
            InstallPath = TryCanonical(profile.InstallPath) ?? NormalizeText(profile.InstallPath)
        };

    private static bool MatchesProfile(CommunityRuleProfileBinding expected, SoftwareProfile current)
    {
        var actual = BindProfile(current);
        return expected.Name.Equals(actual.Name, StringComparison.OrdinalIgnoreCase)
            && expected.Publisher.Equals(actual.Publisher, StringComparison.OrdinalIgnoreCase)
            && expected.DisplayVersion.Equals(actual.DisplayVersion, StringComparison.OrdinalIgnoreCase)
            && expected.InventorySource.Equals(actual.InventorySource, StringComparison.OrdinalIgnoreCase)
            && expected.InstallPath.Equals(actual.InstallPath, StringComparison.OrdinalIgnoreCase);
    }

    private static bool SameFiles(
        IReadOnlyList<CommunityRulePlannedFileBinding> expected,
        IReadOnlyList<CommunityRulePlannedFileBinding> current)
    {
        if (expected.Count != current.Count) return false;
        for (var index = 0; index < expected.Count; index++)
        {
            var left = expected[index];
            var right = current[index];
            if (!left.Path.Equals(right.Path, StringComparison.OrdinalIgnoreCase)
                || left.SizeBytes != right.SizeBytes
                || left.LastWriteTimeUtcTicks != right.LastWriteTimeUtcTicks
                || left.Attributes != right.Attributes
                || !left.RuleNames.SequenceEqual(right.RuleNames, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    private static CommunityRuleCacheCleanupPlan Refused(string summary, string nextStep) =>
        new()
        {
            Summary = summary,
            NextStepText = nextStep,
            SafetyText = "没有生成操作，也不会移动或删除任何文件。",
            Lines = ["Agent 选择了停止；旧规则结果不会被继续使用。"]
        };

    private static string NormalizeText(string? value) => value?.Trim() ?? string.Empty;

    private static string? TryCanonical(string? path)
    {
        try
        {
            return string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)
                ? null
                : Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);

    private static long SaturatingAdd(long left, long right) =>
        right >= long.MaxValue - left ? long.MaxValue : left + Math.Max(0, right);

    private static int SaturatingSum(IEnumerable<int> values)
    {
        var total = 0L;
        foreach (var value in values) total = Math.Min(int.MaxValue, total + Math.Max(0, value));
        return (int)total;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = Math.Max(0, bytes);
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return unit == 0 ? $"{bytes} B" : $"{value:0.0} {units[unit]}";
    }

    private sealed class MutableFileBinding(
        string path,
        long sizeBytes,
        long lastWriteTimeUtcTicks,
        FileAttributes attributes,
        IEnumerable<string> ruleNames)
    {
        public string Path { get; } = path;
        public long SizeBytes { get; } = sizeBytes;
        public long LastWriteTimeUtcTicks { get; } = lastWriteTimeUtcTicks;
        public FileAttributes Attributes { get; } = attributes;
        public HashSet<string> RuleNames { get; } = new(ruleNames, StringComparer.OrdinalIgnoreCase);
    }
}
