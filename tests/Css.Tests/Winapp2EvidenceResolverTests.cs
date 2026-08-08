using Css.Rules.Winapp2;
using Css.Scanner.Winapp2;
using FluentAssertions;

namespace Css.Tests;

public sealed class Winapp2EvidenceResolverTests
{
    [Fact]
    public void Resolver_counts_recursive_matches_exclusions_age_deduplication_and_progress()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-rule-resolver-" + Guid.NewGuid().ToString("N"));
        var cache = Path.Combine(root, "Cache");
        var nested = Path.Combine(cache, "Nested");
        var unrelated = Path.Combine(root, "Unrelated");
        Directory.CreateDirectory(nested);
        Directory.CreateDirectory(unrelated);
        var old = DateTimeOffset.UtcNow.AddDays(-45);
        Write(Path.Combine(cache, "a.tmp"), 10, old);
        Write(Path.Combine(cache, "keep.db"), 20, DateTimeOffset.UtcNow);
        Write(Path.Combine(cache, "other.log"), 30, old);
        Write(Path.Combine(nested, "b.tmp"), 40, old);
        Write(Path.Combine(unrelated, "ignored.tmp"), 100, old);
        var progress = new List<Winapp2EvidenceProgress>();

        try
        {
            var result = new Winapp2EvidenceResolver().Resolve(
                Evidence(
                    root,
                    [
                        "%PROFILE%\\Cache|*.tmp;keep.db|RECURSE",
                        "%PROFILE%\\Cache|a.tmp|RECURSE"
                    ],
                    ["FILE|%PROFILE%\\Cache|keep.db"]),
                new Winapp2EvidenceResolverOptions
                {
                    MaxSamplePaths = 1,
                    ProgressIntervalFiles = 1,
                    StaleAge = TimeSpan.FromDays(30)
                },
                value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase),
                new InlineProgress<Winapp2EvidenceProgress>(progress.Add));

            result.IsAccessible.Should().BeTrue();
            result.FileCount.Should().Be(2);
            result.SizeBytes.Should().Be(50);
            result.StaleFileCount.Should().Be(2);
            result.StaleSizeBytes.Should().Be(50);
            result.ExcludedFileCount.Should().Be(1);
            result.DirectoriesVisited.Should().Be(3);
            result.FilesVisited.Should().Be(4);
            result.SamplePaths.Should().ContainSingle();
            result.CandidateFiles.Should().HaveCount(2);
            result.CandidateFilesComplete.Should().BeTrue();
            result.IsSizeLowerBound.Should().BeFalse();
            result.LimitReasons.Should().BeEmpty();
            result.IsExecutionAuthorized.Should().BeFalse();
            progress.Should().NotBeEmpty();
            progress[^1].MatchedFiles.Should().Be(2);
            progress[^1].SizeBytes.Should().Be(50);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Resolver_discards_partial_candidate_identity_sets_without_lying_about_size()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-rule-candidates-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Write(Path.Combine(root, "one.tmp"), 10, DateTimeOffset.UtcNow.AddDays(-40));
        Write(Path.Combine(root, "two.tmp"), 20, DateTimeOffset.UtcNow.AddDays(-40));

        try
        {
            var result = new Winapp2EvidenceResolver().Resolve(
                Evidence(root, ["%PROFILE%|*.tmp"], []),
                new Winapp2EvidenceResolverOptions { MaxCandidateFiles = 1 },
                value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase));

            result.FileCount.Should().Be(2);
            result.SizeBytes.Should().Be(30);
            result.IsSizeLowerBound.Should().BeFalse();
            result.CandidateFilesComplete.Should().BeFalse();
            result.CandidateFiles.Should().BeEmpty("a partial identity set must never look actionable");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Resolver_preserves_registry_and_directory_removal_intent_as_refusal_evidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-rule-intent-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Write(Path.Combine(root, "one.tmp"), 10, DateTimeOffset.UtcNow.AddDays(-40));
        var evidence = Evidence(
            root,
            ["%PROFILE%|*.tmp|REMOVESELF"],
            [],
            registryTargetCount: 1);

        try
        {
            var result = new Winapp2EvidenceResolver().Resolve(
                evidence,
                expandVariables: value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase));

            result.RegistryTargetCount.Should().Be(1);
            result.IncludesRemoveSelf.Should().BeTrue();
            result.IsExecutionAuthorized.Should().BeFalse();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Resolver_reports_visit_limits_as_lower_bounds_and_propagates_cancellation()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-rule-limit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, "one.tmp"), new byte[10]);
        File.WriteAllBytes(Path.Combine(root, "two.tmp"), new byte[20]);
        File.WriteAllBytes(Path.Combine(root, "three.tmp"), new byte[30]);

        try
        {
            var resolver = new Winapp2EvidenceResolver();
            var result = resolver.Resolve(
                Evidence(root, ["%PROFILE%|*"], []),
                new Winapp2EvidenceResolverOptions { MaxFilesVisited = 2 },
                value => value.Replace("%PROFILE%", root, StringComparison.OrdinalIgnoreCase));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var cancelled = () => resolver.Resolve(
                Evidence(root, ["%PROFILE%|*"], []),
                cancellationToken: cancellation.Token);

            result.FilesVisited.Should().Be(2);
            result.FileCount.Should().Be(2);
            result.IsSizeLowerBound.Should().BeTrue();
            result.LimitReasons.Should().Contain(Winapp2EvidenceLimitReason.FileVisitLimit);
            cancelled.Should().Throw<OperationCanceledException>();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Resolver_rejects_broad_targets_reparse_points_and_directory_escape()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "omnix-rule-boundary"));
        var parent = Directory.GetParent(root)!.FullName;
        var safe = Path.Combine(root, "Safe");
        var linked = Path.Combine(root, "Linked");
        var escaped = Path.Combine(parent, "Outside");
        var fileSystem = new FakeFileSystem(root)
            .WithDirectories(
                root,
                new Winapp2ReadOnlyDirectoryEntry(safe, FileAttributes.Directory),
                new Winapp2ReadOnlyDirectoryEntry(linked, FileAttributes.Directory | FileAttributes.ReparsePoint),
                new Winapp2ReadOnlyDirectoryEntry(escaped, FileAttributes.Directory))
            .WithFile(safe, new Winapp2ReadOnlyFileEntry(
                Path.Combine(safe, "cache.bin"),
                "cache.bin",
                9,
                DateTimeOffset.UtcNow.AddDays(-40),
                FileAttributes.Normal));
        var resolver = new Winapp2EvidenceResolver(fileSystem);

        var result = resolver.Resolve(Evidence(root, [$"{root}|*|RECURSE"], []));
        var broad = resolver.Resolve(Evidence(root, [parent + "|*|RECURSE"], []));

        result.FileCount.Should().Be(1);
        result.SizeBytes.Should().Be(9);
        result.SkippedReparsePointCount.Should().Be(1);
        result.RejectedPathCount.Should().Be(1);
        result.IsSizeLowerBound.Should().BeTrue();
        fileSystem.EnumeratedRoots.Should().NotContain(linked).And.NotContain(escaped);
        broad.FileCount.Should().Be(0);
        broad.UnresolvedTargetCount.Should().Be(1);
        broad.IsExecutionAuthorized.Should().BeFalse();
    }

    [Fact]
    public void File_target_pattern_prunes_directories_without_losing_exact_wildcard_or_recursive_matches()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "omnix-rule-pattern"));
        var app = Path.Combine(root, "AppOne");
        var cache = Path.Combine(app, "Cache");
        var nested = Path.Combine(cache, "Nested");
        var unrelated = Path.Combine(root, "Unrelated");
        Winapp2FileTargetPattern.TryParse(
            $"{root}\\App*\\Cache|*",
            value => value,
            out var exact).Should().BeTrue();
        Winapp2FileTargetPattern.TryParse(
            $"{root}\\App*\\Cache|*|RECURSE",
            value => value,
            out var recursive).Should().BeTrue();

        exact.CanMatchWithin(root).Should().BeTrue();
        exact.CanMatchWithin(app).Should().BeTrue();
        exact.CanMatchWithin(cache).Should().BeTrue();
        exact.CanMatchWithin(nested).Should().BeFalse();
        exact.CanMatchWithin(unrelated).Should().BeFalse();
        recursive.CanMatchWithin(nested).Should().BeTrue();
    }

    [Fact]
    public void Resolver_source_has_no_mutation_network_process_registry_or_operation_authority()
    {
        var sourceRoot = FindRepositoryDirectory("src", "Css.Scanner", "Winapp2");
        var source = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        source.Should().NotContain("OperationDescriptor");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("Directory.Delete");
        source.Should().NotContain("RegistryKey");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("HttpClient");
    }

    private static Winapp2SoftwareEvidence Evidence(
        string root,
        IReadOnlyList<string> targets,
        IReadOnlyList<string> exclusions,
        int registryTargetCount = 0) =>
        new()
        {
            SoftwareName = "Example",
            RuleName = "Example Cache",
            RulePackSource = "Fixture",
            RulePackVersion = "1",
            RulePackSha256 = new string('A', 64),
            MatchedProfilePaths = [root],
            CandidateFileTargets = targets,
            ExclusionTargets = exclusions,
            RegistryTargetCount = registryTargetCount
        };

    private static void Write(string path, int length, DateTimeOffset lastWrite)
    {
        File.WriteAllBytes(path, new byte[length]);
        File.SetLastWriteTimeUtc(path, lastWrite.UtcDateTime);
    }

    private static string FindRepositoryDirectory(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (Directory.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException(Path.Combine(segments));
    }

    private sealed class InlineProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }

    private sealed class FakeFileSystem(string root) : IWinapp2ReadOnlyFileSystem
    {
        private readonly Dictionary<string, IReadOnlyList<Winapp2ReadOnlyDirectoryEntry>> _directories =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IReadOnlyList<Winapp2ReadOnlyFileEntry>> _files =
            new(StringComparer.OrdinalIgnoreCase);

        public List<string> EnumeratedRoots { get; } = [];

        public FakeFileSystem WithDirectories(
            string path,
            params Winapp2ReadOnlyDirectoryEntry[] entries)
        {
            _directories[path] = entries;
            return this;
        }

        public FakeFileSystem WithFile(string path, Winapp2ReadOnlyFileEntry entry)
        {
            _files[path] = [entry];
            return this;
        }

        public bool DirectoryExists(string path) =>
            path.Equals(root, StringComparison.OrdinalIgnoreCase)
            || _directories.ContainsKey(path)
            || _files.ContainsKey(path);

        public FileAttributes GetAttributes(string path) => FileAttributes.Directory;

        public IEnumerable<Winapp2ReadOnlyFileEntry> EnumerateFiles(string directory)
        {
            EnumeratedRoots.Add(directory);
            return _files.GetValueOrDefault(directory) ?? [];
        }

        public IEnumerable<Winapp2ReadOnlyDirectoryEntry> EnumerateDirectories(string directory)
        {
            EnumeratedRoots.Add(directory);
            return _directories.GetValueOrDefault(directory) ?? [];
        }
    }
}
