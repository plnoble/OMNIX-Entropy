using System.Diagnostics;
using Css.Core.Software;
using Css.Rules.Winapp2;

namespace Css.Scanner.Winapp2;

public sealed class Winapp2EvidenceResolver
{
    private readonly IWinapp2ReadOnlyFileSystem _fileSystem;
    private readonly TimeProvider _timeProvider;

    public Winapp2EvidenceResolver(
        IWinapp2ReadOnlyFileSystem? fileSystem = null,
        TimeProvider? timeProvider = null)
    {
        _fileSystem = fileSystem ?? new WindowsWinapp2ReadOnlyFileSystem();
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Winapp2ResolvedEvidence Resolve(
        Winapp2SoftwareEvidence evidence,
        Winapp2EvidenceResolverOptions? options = null,
        Func<string, string>? expandVariables = null,
        IProgress<Winapp2EvidenceProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        cancellationToken.ThrowIfCancellationRequested();
        expandVariables ??= Environment.ExpandEnvironmentVariables;
        options = Normalize(options ?? new Winapp2EvidenceResolverOptions());
        var includesRemoveSelf = evidence.CandidateFileTargets.Any(expression =>
            Winapp2FileTargetPattern.TryParse(expression, expandVariables, out var target)
            && target.RemoveSelf);

        var state = new ResolutionState(
            evidence,
            options,
            progress,
            _timeProvider.GetUtcNow(),
            includesRemoveSelf);
        var roots = CanonicalRoots(evidence.MatchedProfilePaths, options, state);
        var assignments = BuildAssignments(evidence, roots, options, expandVariables, state);
        var exclusions = ParseExclusions(evidence.ExclusionTargets, options, expandVariables);
        var stopwatch = Stopwatch.StartNew();

        foreach (var assignment in assignments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stopwatch.Elapsed >= options.MaxDuration)
            {
                state.AddLimit(Winapp2EvidenceLimitReason.TimeLimit);
                break;
            }

            ProbeRoot(
                assignment.Key,
                assignment.Value,
                exclusions,
                state,
                stopwatch,
                cancellationToken);
            if (state.StopRequested)
                break;
        }

        state.ReportProgress();
        return state.Build();
    }

    private void ProbeRoot(
        string root,
        IReadOnlyList<Winapp2FileTargetPattern> targets,
        IReadOnlyList<Winapp2FileTargetPattern> exclusions,
        ResolutionState state,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!_fileSystem.DirectoryExists(root))
                return;
            if ((_fileSystem.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
            {
                state.SkippedReparsePointCount++;
                return;
            }
        }
        catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
        {
            state.AccessErrorCount++;
            return;
        }

        state.IsAccessible = true;
        state.ProfileRootsScanned++;
        var pending = new Stack<string>();
        var seenDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        pending.Push(root);
        seenDirectories.Add(root);

        while (pending.Count > 0 && !state.StopRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stopwatch.Elapsed >= state.Options.MaxDuration)
            {
                state.AddLimit(Winapp2EvidenceLimitReason.TimeLimit);
                break;
            }
            if (state.DirectoriesVisited >= state.Options.MaxDirectoriesVisited)
            {
                state.AddLimit(Winapp2EvidenceLimitReason.DirectoryVisitLimit);
                break;
            }

            var current = pending.Pop();
            state.DirectoriesVisited++;
            ProbeFiles(current, root, targets, exclusions, state, stopwatch, cancellationToken);
            if (state.StopRequested)
                break;
            ProbeDirectories(current, root, targets, pending, seenDirectories, state, stopwatch, cancellationToken);
        }
    }

    private void ProbeFiles(
        string directory,
        string root,
        IReadOnlyList<Winapp2FileTargetPattern> targets,
        IReadOnlyList<Winapp2FileTargetPattern> exclusions,
        ResolutionState state,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var file in _fileSystem.EnumerateFiles(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (stopwatch.Elapsed >= state.Options.MaxDuration)
                {
                    state.AddLimit(Winapp2EvidenceLimitReason.TimeLimit);
                    return;
                }
                if (state.FilesVisited >= state.Options.MaxFilesVisited)
                {
                    state.AddLimit(Winapp2EvidenceLimitReason.FileVisitLimit);
                    return;
                }

                state.FilesVisited++;
                if ((file.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    state.SkippedReparsePointCount++;
                    continue;
                }
                if (!TryCanonicalContainedFile(file.Path, directory, root, out var canonical))
                {
                    state.RejectedPathCount++;
                    continue;
                }
                if (!targets.Any(target => target.MatchesDirectory(directory) && target.MatchesFileName(file.Name)))
                {
                    state.ReportPeriodicProgress();
                    continue;
                }
                if (exclusions.Any(exclusion => exclusion.MatchesDirectory(directory) && exclusion.MatchesFileName(file.Name)))
                {
                    state.ExcludedFileCount++;
                    state.ReportPeriodicProgress();
                    continue;
                }
                if (state.MatchedPaths.Contains(canonical))
                {
                    state.ReportPeriodicProgress();
                    continue;
                }
                if (state.FileCount >= state.Options.MaxMatchedFiles)
                {
                    state.AddLimit(Winapp2EvidenceLimitReason.MatchLimit);
                    return;
                }

                state.MatchedPaths.Add(canonical);
                state.AddMatch(canonical, file);
                state.ReportPeriodicProgress();
            }
        }
        catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
        {
            state.AccessErrorCount++;
        }
    }

    private void ProbeDirectories(
        string directory,
        string root,
        IReadOnlyList<Winapp2FileTargetPattern> targets,
        Stack<string> pending,
        HashSet<string> seenDirectories,
        ResolutionState state,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var child in _fileSystem.EnumerateDirectories(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (stopwatch.Elapsed >= state.Options.MaxDuration)
                {
                    state.AddLimit(Winapp2EvidenceLimitReason.TimeLimit);
                    return;
                }
                if ((child.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    state.SkippedReparsePointCount++;
                    continue;
                }
                if (!TryCanonicalDirectChild(child.Path, directory, root, out var canonical))
                {
                    state.RejectedPathCount++;
                    continue;
                }
                if (!targets.Any(target => target.CanMatchWithin(canonical))
                    || !seenDirectories.Add(canonical))
                {
                    continue;
                }
                if (state.DirectoriesVisited + pending.Count >= state.Options.MaxDirectoriesVisited)
                {
                    state.AddLimit(Winapp2EvidenceLimitReason.DirectoryVisitLimit);
                    return;
                }

                pending.Push(canonical);
            }
        }
        catch (Exception exception) when (IsExpectedFileSystemFailure(exception))
        {
            state.AccessErrorCount++;
        }
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<Winapp2FileTargetPattern>> BuildAssignments(
        Winapp2SoftwareEvidence evidence,
        IReadOnlyList<string> roots,
        Winapp2EvidenceResolverOptions options,
        Func<string, string> expandVariables,
        ResolutionState state)
    {
        var assignments = new Dictionary<string, List<Winapp2FileTargetPattern>>(StringComparer.OrdinalIgnoreCase);
        var targetCount = Math.Min(evidence.CandidateFileTargets.Count, options.MaxTargetExpressions);
        if (evidence.CandidateFileTargets.Count > options.MaxTargetExpressions)
            state.AddLimit(Winapp2EvidenceLimitReason.TargetExpressionLimit);

        for (var index = 0; index < targetCount; index++)
        {
            var expression = evidence.CandidateFileTargets[index];
            if (!Winapp2FileTargetPattern.TryParse(expression, expandVariables, out var target))
            {
                state.UnresolvedTargetCount++;
                continue;
            }

            var owner = roots
                .Where(target.IsOwnedBy)
                .OrderByDescending(path => path.Length)
                .FirstOrDefault();
            if (owner is null)
            {
                state.UnresolvedTargetCount++;
                continue;
            }

            if (!assignments.TryGetValue(owner, out var ownerTargets))
            {
                ownerTargets = [];
                assignments[owner] = ownerTargets;
            }
            ownerTargets.Add(target);
            state.TargetExpressionsResolved++;
        }

        return assignments.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<Winapp2FileTargetPattern>)pair.Value,
            StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<Winapp2FileTargetPattern> ParseExclusions(
        IReadOnlyList<string> expressions,
        Winapp2EvidenceResolverOptions options,
        Func<string, string> expandVariables)
    {
        var exclusions = new List<Winapp2FileTargetPattern>();
        foreach (var expression in expressions.Take(options.MaxTargetExpressions))
        {
            var parts = expression.Split('|');
            if (parts.Length != 3
                || (!parts[0].Equals("FILE", StringComparison.OrdinalIgnoreCase)
                    && !parts[0].Equals("PATH", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (Winapp2FileTargetPattern.TryParse(
                    $"{parts[1]}|{parts[2]}",
                    expandVariables,
                    out var exclusion))
            {
                exclusions.Add(exclusion);
            }
        }

        return exclusions;
    }

    private static IReadOnlyList<string> CanonicalRoots(
        IReadOnlyList<string> paths,
        Winapp2EvidenceResolverOptions options,
        ResolutionState state)
    {
        var roots = new List<string>();
        foreach (var path in paths)
        {
            if (!TryCanonicalDirectory(path, out var canonical) || IsVolumeRoot(canonical))
            {
                state.RejectedPathCount++;
                continue;
            }
            if (!roots.Contains(canonical, StringComparer.OrdinalIgnoreCase))
                roots.Add(canonical);
        }

        if (roots.Count > options.MaxProfileRoots)
        {
            state.AddLimit(Winapp2EvidenceLimitReason.ProfileRootLimit);
            roots = roots.Take(options.MaxProfileRoots).ToList();
        }

        return roots;
    }

    private static bool TryCanonicalContainedFile(
        string path,
        string directory,
        string root,
        out string canonical)
    {
        if (!TryCanonicalFile(path, out canonical))
            return false;
        var parent = Path.GetDirectoryName(canonical);
        return parent is not null
            && parent.Equals(directory, StringComparison.OrdinalIgnoreCase)
            && IsSameOrDescendant(canonical, root);
    }

    private static bool TryCanonicalDirectChild(
        string path,
        string directory,
        string root,
        out string canonical)
    {
        if (!TryCanonicalDirectory(path, out canonical))
            return false;
        var parent = Directory.GetParent(canonical)?.FullName;
        return parent is not null
            && parent.Equals(directory, StringComparison.OrdinalIgnoreCase)
            && IsSameOrDescendant(canonical, root);
    }

    private static bool TryCanonicalDirectory(string path, out string canonical) =>
        TryCanonical(path, out canonical);

    private static bool TryCanonicalFile(string path, out string canonical) =>
        TryCanonical(path, out canonical);

    private static bool TryCanonical(string path, out string canonical)
    {
        canonical = string.Empty;
        try
        {
            if (!Path.IsPathFullyQualified(path) || path.IndexOfAny(['*', '?']) >= 0)
                return false;
            canonical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            return canonical.Length > 0;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static bool IsVolumeRoot(string path)
    {
        var root = Path.GetPathRoot(path);
        return !string.IsNullOrWhiteSpace(root)
            && Path.TrimEndingDirectorySeparator(root).Equals(path, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSameOrDescendant(string candidate, string owner)
    {
        if (candidate.Equals(owner, StringComparison.OrdinalIgnoreCase))
            return true;
        return candidate.Length > owner.Length
            && candidate.StartsWith(owner, StringComparison.OrdinalIgnoreCase)
            && (candidate[owner.Length] == Path.DirectorySeparatorChar
                || candidate[owner.Length] == Path.AltDirectorySeparatorChar);
    }

    private static bool IsExpectedFileSystemFailure(Exception exception) =>
        exception is UnauthorizedAccessException
            or DirectoryNotFoundException
            or FileNotFoundException
            or IOException
            or System.Security.SecurityException;

    private static Winapp2EvidenceResolverOptions Normalize(Winapp2EvidenceResolverOptions options) =>
        new()
        {
            MaxProfileRoots = Math.Clamp(options.MaxProfileRoots, 1, 128),
            MaxTargetExpressions = Math.Clamp(options.MaxTargetExpressions, 1, 1024),
            MaxDirectoriesVisited = Math.Clamp(options.MaxDirectoriesVisited, 1, 100_000),
            MaxFilesVisited = Math.Clamp(options.MaxFilesVisited, 1, 500_000),
            MaxMatchedFiles = Math.Clamp(options.MaxMatchedFiles, 1, 100_000),
            MaxSamplePaths = Math.Clamp(options.MaxSamplePaths, 0, 100),
            MaxCandidateFiles = Math.Clamp(options.MaxCandidateFiles, 0, 512),
            ProgressIntervalFiles = Math.Clamp(options.ProgressIntervalFiles, 1, 10_000),
            MaxDuration = options.MaxDuration <= TimeSpan.Zero
                ? TimeSpan.FromMilliseconds(100)
                : TimeSpan.FromMinutes(Math.Min(options.MaxDuration.TotalMinutes, 5)),
            StaleAge = options.StaleAge < TimeSpan.Zero ? TimeSpan.Zero : options.StaleAge
        };

    private sealed class ResolutionState
    {
        private readonly Winapp2SoftwareEvidence _evidence;
        private readonly IProgress<Winapp2EvidenceProgress>? _progress;
        private readonly DateTimeOffset _staleCutoffUtc;
        private readonly HashSet<Winapp2EvidenceLimitReason> _limitReasons = [];
        private readonly List<string> _samplePaths = [];
        private readonly List<CommunityRuleFileEvidence> _candidateFiles = [];
        private bool _candidateFilesComplete = true;

        public ResolutionState(
            Winapp2SoftwareEvidence evidence,
            Winapp2EvidenceResolverOptions options,
            IProgress<Winapp2EvidenceProgress>? progress,
            DateTimeOffset utcNow,
            bool includesRemoveSelf)
        {
            _evidence = evidence;
            Options = options;
            _progress = progress;
            _staleCutoffUtc = utcNow - options.StaleAge;
            IncludesRemoveSelf = includesRemoveSelf;
        }

        public Winapp2EvidenceResolverOptions Options { get; }
        public HashSet<string> MatchedPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
        public bool StopRequested => _limitReasons.Any(reason => reason is
            Winapp2EvidenceLimitReason.DirectoryVisitLimit
            or Winapp2EvidenceLimitReason.FileVisitLimit
            or Winapp2EvidenceLimitReason.MatchLimit
            or Winapp2EvidenceLimitReason.TimeLimit);
        public bool IsAccessible { get; set; }
        public int ProfileRootsScanned { get; set; }
        public int TargetExpressionsResolved { get; set; }
        public int UnresolvedTargetCount { get; set; }
        public int DirectoriesVisited { get; set; }
        public int FilesVisited { get; set; }
        public int FileCount { get; set; }
        public long SizeBytes { get; set; }
        public int StaleFileCount { get; set; }
        public long StaleSizeBytes { get; set; }
        public int ExcludedFileCount { get; set; }
        public int SkippedReparsePointCount { get; set; }
        public int RejectedPathCount { get; set; }
        public int AccessErrorCount { get; set; }
        public DateTimeOffset? OldestWriteTimeUtc { get; set; }
        public DateTimeOffset? NewestWriteTimeUtc { get; set; }
        public bool IncludesRemoveSelf { get; }

        public void AddLimit(Winapp2EvidenceLimitReason reason) => _limitReasons.Add(reason);

        public void AddMatch(string canonicalPath, Winapp2ReadOnlyFileEntry file)
        {
            FileCount++;
            SizeBytes = SaturatingAdd(SizeBytes, file.Length);
            OldestWriteTimeUtc = OldestWriteTimeUtc is null || file.LastWriteTimeUtc < OldestWriteTimeUtc
                ? file.LastWriteTimeUtc
                : OldestWriteTimeUtc;
            NewestWriteTimeUtc = NewestWriteTimeUtc is null || file.LastWriteTimeUtc > NewestWriteTimeUtc
                ? file.LastWriteTimeUtc
                : NewestWriteTimeUtc;
            if (file.LastWriteTimeUtc <= _staleCutoffUtc)
            {
                StaleFileCount++;
                StaleSizeBytes = SaturatingAdd(StaleSizeBytes, file.Length);
            }
            if (_samplePaths.Count < Options.MaxSamplePaths)
                _samplePaths.Add(canonicalPath);
            if (_candidateFilesComplete)
            {
                if (_candidateFiles.Count >= Options.MaxCandidateFiles)
                {
                    _candidateFiles.Clear();
                    _candidateFilesComplete = false;
                }
                else
                {
                    _candidateFiles.Add(new CommunityRuleFileEvidence
                    {
                        Path = canonicalPath,
                        SizeBytes = Math.Max(0, file.Length),
                        LastWriteTimeUtc = file.LastWriteTimeUtc,
                        Attributes = file.Attributes
                    });
                }
            }
        }

        public void ReportPeriodicProgress()
        {
            if (FilesVisited % Options.ProgressIntervalFiles == 0)
                ReportProgress();
        }

        public void ReportProgress() => _progress?.Report(new Winapp2EvidenceProgress
        {
            SoftwareName = _evidence.SoftwareName,
            RuleName = _evidence.RuleName,
            DirectoriesVisited = DirectoriesVisited,
            FilesVisited = FilesVisited,
            MatchedFiles = FileCount,
            SizeBytes = SizeBytes
        });

        public Winapp2ResolvedEvidence Build() => new()
        {
            SoftwareName = _evidence.SoftwareName,
            RuleName = _evidence.RuleName,
            RulePackSource = _evidence.RulePackSource,
            RulePackVersion = _evidence.RulePackVersion,
            RulePackSha256 = _evidence.RulePackSha256,
            Warning = _evidence.Warning,
            ProfileRootsScanned = ProfileRootsScanned,
            TargetExpressionsResolved = TargetExpressionsResolved,
            UnresolvedTargetCount = UnresolvedTargetCount,
            DirectoriesVisited = DirectoriesVisited,
            FilesVisited = FilesVisited,
            FileCount = FileCount,
            SizeBytes = SizeBytes,
            StaleFileCount = StaleFileCount,
            StaleSizeBytes = StaleSizeBytes,
            ExcludedFileCount = ExcludedFileCount,
            SkippedReparsePointCount = SkippedReparsePointCount,
            RejectedPathCount = RejectedPathCount,
            AccessErrorCount = AccessErrorCount,
            OldestWriteTimeUtc = OldestWriteTimeUtc,
            NewestWriteTimeUtc = NewestWriteTimeUtc,
            SamplePaths = _samplePaths.ToArray(),
            CandidateFiles = _candidateFilesComplete ? _candidateFiles.ToArray() : [],
            CandidateFilesComplete = _candidateFilesComplete,
            RegistryTargetCount = _evidence.RegistryTargetCount,
            IncludesRemoveSelf = IncludesRemoveSelf,
            LimitReasons = _limitReasons.Order().ToArray(),
            IsAccessible = IsAccessible,
            IsSizeLowerBound = _limitReasons.Count > 0
                || UnresolvedTargetCount > 0
                || SkippedReparsePointCount > 0
                || RejectedPathCount > 0
                || AccessErrorCount > 0
        };

        private static long SaturatingAdd(long left, long right)
        {
            right = Math.Max(0, right);
            return right > long.MaxValue - left ? long.MaxValue : left + right;
        }
    }
}
