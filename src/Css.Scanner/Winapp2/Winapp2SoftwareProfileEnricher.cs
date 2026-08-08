using System.Diagnostics;
using Css.Core.Software;
using Css.Rules.Winapp2;

namespace Css.Scanner.Winapp2;

public enum Winapp2SoftwareProfileEnrichmentLimit
{
    ProfileLimit,
    RulesPerProfile,
    TotalRuleLimit,
    TotalTimeLimit,
    RuleResolutionLimit
}

public sealed class Winapp2SoftwareProfileEnrichmentOptions
{
    public int MaxProfiles { get; init; } = 240;
    public int MaxRulesPerProfile { get; init; } = 16;
    public int MaxResolvedRules { get; init; } = 512;
    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromSeconds(30);
    public IReadOnlySet<string> IgnoredRuleKeys { get; init; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<string> ApprovedUserDataRoots { get; init; } =
        new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        }
        .Where(path => !string.IsNullOrWhiteSpace(path))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    public Func<string, bool> CandidateFileExists { get; init; } = File.Exists;
    public Func<string, bool> CandidatePathIsReparsePoint { get; init; } = path =>
        (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;
    public Winapp2EvidenceResolverOptions ResolverOptions { get; init; } = new()
    {
        MaxDuration = TimeSpan.FromSeconds(2),
        MaxSamplePaths = 3
    };
}

public sealed class Winapp2SoftwareProfileEnrichmentProgress
{
    public int ProcessedProfiles { get; init; }
    public int TotalProfiles { get; init; }
    public int ResolvedRules { get; init; }
    public int EnrichedProfiles { get; init; }
}

public sealed class Winapp2SoftwareProfileEnrichmentResult
{
    public required IReadOnlyList<SoftwareProfile> Profiles { get; init; }
    public int EnrichedProfileCount { get; init; }
    public int RuleEvidenceCount { get; init; }
    public int IgnoredRuleCount { get; init; }
    public required IReadOnlyList<Winapp2SoftwareProfileEnrichmentLimit> LimitReasons { get; init; }
    public required string BeginnerSummary { get; init; }
    public bool IsComplete => LimitReasons.Count == 0;
    public bool IsExecutionAuthorized => false;
}

public sealed class Winapp2SoftwareProfileEnricher
{
    private readonly Winapp2EvidenceResolver _resolver;

    public Winapp2SoftwareProfileEnricher(Winapp2EvidenceResolver? resolver = null)
    {
        _resolver = resolver ?? new Winapp2EvidenceResolver();
    }

    public Winapp2SoftwareProfileEnrichmentResult Enrich(
        IReadOnlyList<SoftwareProfile> profiles,
        Winapp2RuleCatalog catalog,
        Winapp2SoftwareProfileEnrichmentOptions? options = null,
        Func<string, string>? expandVariables = null,
        IProgress<Winapp2SoftwareProfileEnrichmentProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(catalog);
        cancellationToken.ThrowIfCancellationRequested();
        options = Normalize(options ?? new Winapp2SoftwareProfileEnrichmentOptions());
        expandVariables ??= Environment.ExpandEnvironmentVariables;

        var output = profiles.ToArray();
        var limits = new HashSet<Winapp2SoftwareProfileEnrichmentLimit>();
        var stopwatch = Stopwatch.StartNew();
        var profileCount = Math.Min(profiles.Count, options.MaxProfiles);
        if (profiles.Count > options.MaxProfiles)
            limits.Add(Winapp2SoftwareProfileEnrichmentLimit.ProfileLimit);
        var resolvedRules = 0;
        var enrichedProfiles = 0;
        var evidenceCount = 0;
        var ignoredRuleCount = 0;

        for (var profileIndex = 0; profileIndex < profileCount; profileIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stopwatch.Elapsed >= options.MaxDuration)
            {
                limits.Add(Winapp2SoftwareProfileEnrichmentLimit.TotalTimeLimit);
                break;
            }

            var profile = profiles[profileIndex];
            var matches = Winapp2SoftwareEvidenceMatcher.Match(catalog, profile, expandVariables)
                .Where(match => match.CandidateFileTargets.Count > 0)
                .Where(match =>
                {
                    var ignored = options.IgnoredRuleKeys.Contains(
                        Winapp2RulePreferenceKey.Create(match.RulePackSha256, match.RuleName));
                    if (ignored) ignoredRuleCount++;
                    return !ignored;
                })
                .ToArray();
            if (matches.Length > options.MaxRulesPerProfile)
                limits.Add(Winapp2SoftwareProfileEnrichmentLimit.RulesPerProfile);
            var profileEvidence = new List<CommunityRuleCacheEvidence>();
            foreach (var match in matches.Take(options.MaxRulesPerProfile))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (resolvedRules >= options.MaxResolvedRules)
                {
                    limits.Add(Winapp2SoftwareProfileEnrichmentLimit.TotalRuleLimit);
                    break;
                }
                var remaining = options.MaxDuration - stopwatch.Elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    limits.Add(Winapp2SoftwareProfileEnrichmentLimit.TotalTimeLimit);
                    break;
                }

                var resolverOptions = ResolverOptions(options.ResolverOptions, remaining);
                var resolved = _resolver.Resolve(
                    match,
                    resolverOptions,
                    expandVariables,
                    cancellationToken: cancellationToken);
                resolvedRules++;
                if (resolved.LimitReasons.Count > 0)
                    limits.Add(Winapp2SoftwareProfileEnrichmentLimit.RuleResolutionLimit);
                if (resolved.FileCount <= 0)
                    continue;

                profileEvidence.Add(ToCoreEvidence(profile, resolved, resolverOptions.StaleAge, options));
            }

            if (profileEvidence.Count > 0 || profile.CommunityCacheEvidence.Count > 0)
            {
                output[profileIndex] = new SoftwareProfile(profile)
                {
                    CommunityCacheEvidence = profileEvidence.ToArray()
                };
                if (profileEvidence.Count > 0)
                    enrichedProfiles++;
            }
            evidenceCount += profileEvidence.Count;
            progress?.Report(new Winapp2SoftwareProfileEnrichmentProgress
            {
                ProcessedProfiles = profileIndex + 1,
                TotalProfiles = profileCount,
                ResolvedRules = resolvedRules,
                EnrichedProfiles = enrichedProfiles
            });

            if (limits.Contains(Winapp2SoftwareProfileEnrichmentLimit.TotalRuleLimit)
                || limits.Contains(Winapp2SoftwareProfileEnrichmentLimit.TotalTimeLimit))
            {
                break;
            }
        }

        var completion = limits.Count == 0 ? "完整" : "受安全上限限制";
        return new Winapp2SoftwareProfileEnrichmentResult
        {
            Profiles = output,
            EnrichedProfileCount = enrichedProfiles,
            RuleEvidenceCount = evidenceCount,
            IgnoredRuleCount = ignoredRuleCount,
            LimitReasons = limits.Order().ToArray(),
            BeginnerSummary = enrichedProfiles == 0
                ? $"扩展缓存规则已检查，暂未发现额外缓存（{completion}）。"
                : $"扩展缓存规则为 {enrichedProfiles} 个应用补充了 {evidenceCount} 条只读发现（{completion}）。"
        };
    }

    private static CommunityRuleCacheEvidence ToCoreEvidence(
        SoftwareProfile profile,
        Winapp2ResolvedEvidence resolved,
        TimeSpan staleAge,
        Winapp2SoftwareProfileEnrichmentOptions options)
    {
        var evidence = new CommunityRuleCacheEvidence
        {
            RuleName = resolved.RuleName,
            RulePackSource = resolved.RulePackSource,
            RulePackVersion = resolved.RulePackVersion,
            RulePackSha256 = resolved.RulePackSha256,
            Warning = resolved.Warning,
            FileCount = resolved.FileCount,
            SizeBytes = resolved.SizeBytes,
            StaleFileCount = resolved.StaleFileCount,
            StaleSizeBytes = resolved.StaleSizeBytes,
            StaleThresholdDays = Math.Max(0, (int)Math.Round(staleAge.TotalDays)),
            IsSizeLowerBound = resolved.IsSizeLowerBound,
            ExcludedFileCount = resolved.ExcludedFileCount,
            UnresolvedTargetCount = resolved.UnresolvedTargetCount,
            SkippedReparsePointCount = resolved.SkippedReparsePointCount,
            RejectedPathCount = resolved.RejectedPathCount,
            AccessErrorCount = resolved.AccessErrorCount,
            RegistryTargetCount = resolved.RegistryTargetCount,
            IncludesRemoveSelf = resolved.IncludesRemoveSelf,
            CandidateFiles = resolved.CandidateFiles,
            CandidateFilesComplete = resolved.CandidateFilesComplete,
            SamplePaths = resolved.SamplePaths
        };
        return new CommunityRuleCacheEvidence(evidence)
        {
            CandidateAssessment = CommunityRuleCandidatePolicy.Evaluate(
                profile,
                evidence,
                options.ApprovedUserDataRoots,
                options.CandidateFileExists,
                options.CandidatePathIsReparsePoint,
                options.TimeProvider.GetUtcNow())
        };
    }

    private static Winapp2SoftwareProfileEnrichmentOptions Normalize(
        Winapp2SoftwareProfileEnrichmentOptions options) =>
        new()
        {
            MaxProfiles = Math.Clamp(options.MaxProfiles, 1, 500),
            MaxRulesPerProfile = Math.Clamp(options.MaxRulesPerProfile, 1, 64),
            MaxResolvedRules = Math.Clamp(options.MaxResolvedRules, 1, 2_000),
            MaxDuration = options.MaxDuration <= TimeSpan.Zero
                ? TimeSpan.FromMilliseconds(100)
                : TimeSpan.FromMinutes(Math.Min(options.MaxDuration.TotalMinutes, 2)),
            ResolverOptions = options.ResolverOptions ?? new Winapp2EvidenceResolverOptions(),
            ApprovedUserDataRoots = options.ApprovedUserDataRoots ?? [],
            CandidateFileExists = options.CandidateFileExists ?? File.Exists,
            CandidatePathIsReparsePoint = options.CandidatePathIsReparsePoint ?? (_ => true),
            TimeProvider = options.TimeProvider ?? TimeProvider.System,
            IgnoredRuleKeys = (options.IgnoredRuleKeys ?? new HashSet<string>())
                .Where(Winapp2RulePreferenceKey.IsValid)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
        };

    private static Winapp2EvidenceResolverOptions ResolverOptions(
        Winapp2EvidenceResolverOptions source,
        TimeSpan remaining) =>
        new()
        {
            MaxProfileRoots = source.MaxProfileRoots,
            MaxTargetExpressions = source.MaxTargetExpressions,
            MaxDirectoriesVisited = source.MaxDirectoriesVisited,
            MaxFilesVisited = source.MaxFilesVisited,
            MaxMatchedFiles = source.MaxMatchedFiles,
            MaxSamplePaths = source.MaxSamplePaths,
            MaxCandidateFiles = source.MaxCandidateFiles,
            ProgressIntervalFiles = source.ProgressIntervalFiles,
            MaxDuration = source.MaxDuration <= remaining ? source.MaxDuration : remaining,
            StaleAge = source.StaleAge
        };

}
