using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Css.Scanner.Disk;

public enum PersonalStorageFindingKind
{
    LongUnusedLargeFile,
    PossibleDuplicateGroup
}

public sealed class PersonalStorageFinding
{
    public required PersonalStorageFindingKind Kind { get; init; }
    public required string DisplayName { get; init; }
    public int ItemCount { get; init; }
    public long ItemSizeBytes { get; init; }
    public long CandidateBytes { get; init; }
    public DateTime? LastWriteUtc { get; init; }
    public IReadOnlyList<string> EvidencePaths { get; init; } = [];
    public bool CanExecuteDirectly => false;
}

public sealed class PersonalStorageAnalysis
{
    public int VisitedNodeCount { get; init; }
    public int EligibleFileCount { get; init; }
    public bool WasTruncated { get; init; }
    public IReadOnlyList<PersonalStorageFinding> Findings { get; init; } = [];
}

public sealed class PersonalStorageAnalysisOptions
{
    public long MinimumLargeFileBytes { get; init; } = 512L * 1024 * 1024;
    public long MinimumDuplicateFileBytes { get; init; } = 64L * 1024 * 1024;
    public int UnusedDays { get; init; } = 180;
    public int MaximumVisitedNodes { get; init; } = 200_000;
    public int MaximumLargeFiles { get; init; } = 20;
    public int MaximumDuplicateGroups { get; init; } = 10;
    public int MaximumPathsPerDuplicateGroup { get; init; } = 8;
}

public static class PersonalStorageAnalyzer
{
    public static PersonalStorageAnalysis Analyze(
        DriveScanResult result,
        IReadOnlyList<string> personalRoots,
        DateTimeOffset now,
        PersonalStorageAnalysisOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(personalRoots);
        options ??= new PersonalStorageAnalysisOptions();
        ValidateOptions(options);

        var roots = personalRoots
            .Select(TryNormalizeDirectory)
            .Where(path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (roots.Length == 0)
            return new PersonalStorageAnalysis();

        var files = new List<CategoryNode>();
        var stack = new Stack<CategoryNode>(result.TopLevel.AsEnumerable().Reverse());
        var visited = 0;
        var truncated = false;
        while (stack.Count > 0)
        {
            if (visited >= options.MaximumVisitedNodes)
            {
                truncated = true;
                break;
            }

            var node = stack.Pop();
            visited++;
            if (node.IsFile
                && node.SizeBytes > 0
                && IsInsideAnyRoot(node.Path, roots))
            {
                files.Add(node);
            }

            for (var index = node.Children.Count - 1; index >= 0; index--)
                stack.Push(node.Children[index]);
        }

        var findings = new List<PersonalStorageFinding>();
        var staleBefore = now.UtcDateTime.AddDays(-options.UnusedDays);
        findings.AddRange(files
            .Where(file => file.SizeBytes >= options.MinimumLargeFileBytes)
            .Where(file => file.LastWriteUtc is { } written && written.ToUniversalTime() <= staleBefore)
            .OrderByDescending(file => file.SizeBytes)
            .ThenBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
            .Take(options.MaximumLargeFiles)
            .Select(file => new PersonalStorageFinding
            {
                Kind = PersonalStorageFindingKind.LongUnusedLargeFile,
                DisplayName = SafeDisplayName(file.Name),
                ItemCount = 1,
                ItemSizeBytes = file.SizeBytes,
                CandidateBytes = file.SizeBytes,
                LastWriteUtc = file.LastWriteUtc?.ToUniversalTime(),
                EvidencePaths = [Path.GetFullPath(file.Path!)]
            }));

        var duplicateGroups = files
            .Where(file => file.SizeBytes >= options.MinimumDuplicateFileBytes)
            .Where(file => !string.IsNullOrWhiteSpace(file.Name))
            .GroupBy(
                file => file.SizeBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "|" + file.Name.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .GroupBy(file => Path.GetFullPath(file.Path!), StringComparer.OrdinalIgnoreCase)
                .Select(unique => unique.First())
                .ToArray())
            .Where(group => group.Length >= 2)
            .OrderByDescending(group => CandidateBytes(group.Length, group[0].SizeBytes))
            .ThenBy(group => group[0].Name, StringComparer.OrdinalIgnoreCase)
            .Take(options.MaximumDuplicateGroups);
        foreach (var group in duplicateGroups)
        {
            var itemSize = group[0].SizeBytes;
            findings.Add(new PersonalStorageFinding
            {
                Kind = PersonalStorageFindingKind.PossibleDuplicateGroup,
                DisplayName = SafeDisplayName(group[0].Name),
                ItemCount = group.Length,
                ItemSizeBytes = itemSize,
                CandidateBytes = CandidateBytes(group.Length, itemSize),
                LastWriteUtc = group
                    .Select(file => file.LastWriteUtc?.ToUniversalTime())
                    .Max(),
                EvidencePaths = group
                    .Take(options.MaximumPathsPerDuplicateGroup)
                    .Select(file => Path.GetFullPath(file.Path!))
                    .ToArray()
            });
        }

        return new PersonalStorageAnalysis
        {
            VisitedNodeCount = visited,
            EligibleFileCount = files.Count,
            WasTruncated = truncated,
            Findings = findings
        };
    }

    public static IReadOnlyList<string> DefaultPersonalRoots()
    {
        var userRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                string.IsNullOrWhiteSpace(userRoot) ? null : Path.Combine(userRoot, "Downloads")
            }
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void ValidateOptions(PersonalStorageAnalysisOptions options)
    {
        if (options.MinimumLargeFileBytes <= 0
            || options.MinimumDuplicateFileBytes <= 0
            || options.UnusedDays <= 0
            || options.MaximumVisitedNodes is <= 0 or > 1_000_000
            || options.MaximumLargeFiles is <= 0 or > 100
            || options.MaximumDuplicateGroups is <= 0 or > 100
            || options.MaximumPathsPerDuplicateGroup is < 2 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(options));
        }
    }

    private static string? TryNormalizeDirectory(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)
                || path.StartsWith("\\\\", StringComparison.Ordinal)
                || !Path.IsPathFullyQualified(path))
            {
                return null;
            }
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsInsideAnyRoot(string? path, IReadOnlyList<string> roots)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)
                || path.StartsWith("\\\\", StringComparison.Ordinal)
                || !Path.IsPathFullyQualified(path))
            {
                return false;
            }
            var fullPath = Path.GetFullPath(path);
            return roots.Any(root => fullPath.StartsWith(
                root + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static string SafeDisplayName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? "未命名文件" : name.Trim();

    private static long CandidateBytes(int itemCount, long itemSize)
    {
        if (itemCount <= 1 || itemSize <= 0)
            return 0;
        var duplicateCount = itemCount - 1L;
        return itemSize > long.MaxValue / duplicateCount
            ? long.MaxValue
            : itemSize * duplicateCount;
    }
}
