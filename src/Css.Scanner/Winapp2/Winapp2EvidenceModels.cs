using Css.Core.Software;

namespace Css.Scanner.Winapp2;

public enum Winapp2EvidenceLimitReason
{
    ProfileRootLimit,
    TargetExpressionLimit,
    DirectoryVisitLimit,
    FileVisitLimit,
    MatchLimit,
    TimeLimit
}

public sealed class Winapp2EvidenceResolverOptions
{
    public int MaxProfileRoots { get; init; } = 32;
    public int MaxTargetExpressions { get; init; } = 256;
    public int MaxDirectoriesVisited { get; init; } = 20_000;
    public int MaxFilesVisited { get; init; } = 50_000;
    public int MaxMatchedFiles { get; init; } = 20_000;
    public int MaxSamplePaths { get; init; } = 25;
    public int MaxCandidateFiles { get; init; } = 128;
    public int ProgressIntervalFiles { get; init; } = 250;
    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromSeconds(15);
    public TimeSpan StaleAge { get; init; } = TimeSpan.FromDays(30);
}

public sealed class Winapp2EvidenceProgress
{
    public required string SoftwareName { get; init; }
    public required string RuleName { get; init; }
    public int DirectoriesVisited { get; init; }
    public int FilesVisited { get; init; }
    public int MatchedFiles { get; init; }
    public long SizeBytes { get; init; }
}

public sealed class Winapp2ResolvedEvidence
{
    public required string SoftwareName { get; init; }
    public required string RuleName { get; init; }
    public required string RulePackSource { get; init; }
    public required string RulePackVersion { get; init; }
    public required string RulePackSha256 { get; init; }
    public string? Warning { get; init; }
    public int ProfileRootsScanned { get; init; }
    public int TargetExpressionsResolved { get; init; }
    public int UnresolvedTargetCount { get; init; }
    public int DirectoriesVisited { get; init; }
    public int FilesVisited { get; init; }
    public int FileCount { get; init; }
    public long SizeBytes { get; init; }
    public int StaleFileCount { get; init; }
    public long StaleSizeBytes { get; init; }
    public int ExcludedFileCount { get; init; }
    public int SkippedReparsePointCount { get; init; }
    public int RejectedPathCount { get; init; }
    public int AccessErrorCount { get; init; }
    public DateTimeOffset? OldestWriteTimeUtc { get; init; }
    public DateTimeOffset? NewestWriteTimeUtc { get; init; }
    public required IReadOnlyList<string> SamplePaths { get; init; }
    public required IReadOnlyList<CommunityRuleFileEvidence> CandidateFiles { get; init; }
    public bool CandidateFilesComplete { get; init; }
    public int RegistryTargetCount { get; init; }
    public bool IncludesRemoveSelf { get; init; }
    public required IReadOnlyList<Winapp2EvidenceLimitReason> LimitReasons { get; init; }
    public bool IsAccessible { get; init; }
    public bool IsSizeLowerBound { get; init; }
    public bool IsExecutionAuthorized => false;
}

public sealed record Winapp2ReadOnlyFileEntry(
    string Path,
    string Name,
    long Length,
    DateTimeOffset LastWriteTimeUtc,
    FileAttributes Attributes);

public sealed record Winapp2ReadOnlyDirectoryEntry(
    string Path,
    FileAttributes Attributes);

public interface IWinapp2ReadOnlyFileSystem
{
    bool DirectoryExists(string path);
    FileAttributes GetAttributes(string path);
    IEnumerable<Winapp2ReadOnlyFileEntry> EnumerateFiles(string directory);
    IEnumerable<Winapp2ReadOnlyDirectoryEntry> EnumerateDirectories(string directory);
}

public sealed class WindowsWinapp2ReadOnlyFileSystem : IWinapp2ReadOnlyFileSystem
{
    private readonly EnumerationOptions _options = new()
    {
        AttributesToSkip = FileAttributes.Device,
        IgnoreInaccessible = false,
        RecurseSubdirectories = false,
        ReturnSpecialDirectories = false,
        BufferSize = 8192
    };

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);

    public IEnumerable<Winapp2ReadOnlyFileEntry> EnumerateFiles(string directory) =>
        new DirectoryInfo(directory)
            .EnumerateFiles("*", _options)
            .Select(file => new Winapp2ReadOnlyFileEntry(
                file.FullName,
                file.Name,
                Math.Max(0, file.Length),
                new DateTimeOffset(file.LastWriteTimeUtc),
                file.Attributes));

    public IEnumerable<Winapp2ReadOnlyDirectoryEntry> EnumerateDirectories(string directory) =>
        new DirectoryInfo(directory)
            .EnumerateDirectories("*", _options)
            .Select(item => new Winapp2ReadOnlyDirectoryEntry(item.FullName, item.Attributes));
}
