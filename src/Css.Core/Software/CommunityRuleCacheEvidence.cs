using System.Diagnostics.CodeAnalysis;

namespace Css.Core.Software;

public sealed class CommunityRuleCacheEvidence
{
    public CommunityRuleCacheEvidence()
    {
    }

    [SetsRequiredMembers]
    public CommunityRuleCacheEvidence(CommunityRuleCacheEvidence source)
    {
        ArgumentNullException.ThrowIfNull(source);
        RuleName = source.RuleName;
        RulePackSource = source.RulePackSource;
        RulePackVersion = source.RulePackVersion;
        RulePackSha256 = source.RulePackSha256;
        Warning = source.Warning;
        FileCount = source.FileCount;
        SizeBytes = source.SizeBytes;
        StaleFileCount = source.StaleFileCount;
        StaleSizeBytes = source.StaleSizeBytes;
        StaleThresholdDays = source.StaleThresholdDays;
        IsSizeLowerBound = source.IsSizeLowerBound;
        ExcludedFileCount = source.ExcludedFileCount;
        UnresolvedTargetCount = source.UnresolvedTargetCount;
        SkippedReparsePointCount = source.SkippedReparsePointCount;
        RejectedPathCount = source.RejectedPathCount;
        AccessErrorCount = source.AccessErrorCount;
        RegistryTargetCount = source.RegistryTargetCount;
        IncludesRemoveSelf = source.IncludesRemoveSelf;
        CandidateFiles = source.CandidateFiles;
        CandidateFilesComplete = source.CandidateFilesComplete;
        CandidateAssessment = source.CandidateAssessment;
        SamplePaths = source.SamplePaths;
    }

    public required string RuleName { get; init; }
    public required string RulePackSource { get; init; }
    public required string RulePackVersion { get; init; }
    public required string RulePackSha256 { get; init; }
    public string? Warning { get; init; }
    public int FileCount { get; init; }
    public long SizeBytes { get; init; }
    public int StaleFileCount { get; init; }
    public long StaleSizeBytes { get; init; }
    public int StaleThresholdDays { get; init; } = 30;
    public bool IsSizeLowerBound { get; init; }
    public int ExcludedFileCount { get; init; }
    public int UnresolvedTargetCount { get; init; }
    public int SkippedReparsePointCount { get; init; }
    public int RejectedPathCount { get; init; }
    public int AccessErrorCount { get; init; }
    public int RegistryTargetCount { get; init; }
    public bool IncludesRemoveSelf { get; init; }
    public IReadOnlyList<CommunityRuleFileEvidence> CandidateFiles { get; init; } = [];
    public bool CandidateFilesComplete { get; init; }
    public CommunityRuleCandidateAssessment? CandidateAssessment { get; init; }
    public IReadOnlyList<string> SamplePaths { get; init; } = [];
    public bool IsExecutionAuthorized => false;
}
