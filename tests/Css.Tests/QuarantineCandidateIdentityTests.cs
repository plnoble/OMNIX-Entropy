using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Timeline;
using Css.Win32.Quarantine;
using FluentAssertions;

namespace Css.Tests;

public class QuarantineCandidateIdentityTests
{
    [Fact]
    public void Confirmation_refuses_a_quarantine_plan_without_bound_file_identity()
    {
        var operation = CandidateOperation([@"C:\tmp\cache.tmp"]);

        var action = () => QuarantineOperationPolicy.ConfirmForExecution(operation);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*候选身份*");
    }

    [Fact]
    public async Task Handler_refuses_the_whole_batch_when_one_candidate_was_recreated_after_confirmation()
    {
        var root = CreateTempRoot();
        try
        {
            var sourceRoot = Path.Combine(root, "source");
            var first = Path.Combine(sourceRoot, "first.tmp");
            var second = Path.Combine(sourceRoot, "second.tmp");
            var quarantineRoot = Path.Combine(root, "quarantine");
            var timelinePath = Path.Combine(root, "timeline.db");
            Directory.CreateDirectory(sourceRoot);
            await File.WriteAllTextAsync(first, "first-before");
            await File.WriteAllTextAsync(second, "second-before");

            var reader = new WindowsQuarantineCandidateIdentityReader();
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                CandidateOperation([first, second]),
                quarantineRoot,
                reader);
            preparation.Success.Should().BeTrue(preparation.Error);
            preparation.Operation.Should().NotBeNull();

            File.Delete(second);
            await File.WriteAllTextAsync(second, "second-after");

            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);
            var handler = new QuarantineOperationHandler(
                new FileQuarantineService(quarantineRoot),
                new ActionTimelineStore(timelinePath),
                reader);
            var result = await new SafetyOperationPipeline(handler.ExecuteAsync).ExecuteAsync(descriptor);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("确认后发生变化");
            File.Exists(first).Should().BeTrue("batch preflight must happen before the first move");
            File.Exists(second).Should().BeTrue();
            Directory.Exists(quarantineRoot).Should().BeFalse();
            (await new ActionTimelineStore(timelinePath).LoadRecentAsync(5)).Should().BeEmpty();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Service_refuses_a_candidate_when_its_parent_chain_is_reported_as_a_reparse_path()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var reader = new ScriptedIdentityReader(path =>
                QuarantineCandidateInspection.Refused("candidate parent is a reparse path"));
            var service = new FileQuarantineService(Path.Combine(root, "quarantine"));

            var action = () => service.QuarantineAsync(
                source,
                "test",
                Expected(source),
                reader);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*reparse path*");
            File.Exists(source).Should().BeTrue();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Service_moves_an_unchanged_directory_with_bound_identity()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "Cache");
            var quarantineRoot = Path.Combine(root, "quarantine");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "entry.bin"), "fixture");
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var inspection = reader.Inspect(source);
            inspection.Success.Should().BeTrue(inspection.Summary);
            reader.Inspect(source).Evidence.Should().Be(inspection.Evidence);
            _ = Directory.GetFiles(source).Sum(file => new FileInfo(file).Length);
            reader.Inspect(source).Evidence.Should().Be(inspection.Evidence);
            var unrelated = Path.Combine(quarantineRoot, "plan");
            Directory.CreateDirectory(unrelated);
            await File.WriteAllTextAsync(Path.Combine(unrelated, "manifest.json"), "fixture");
            reader.Inspect(source).Evidence.Should().Be(inspection.Evidence);
            Directory.Delete(unrelated, recursive: true);
            var service = new FileQuarantineService(quarantineRoot);

            var action = () => service.QuarantineAsync(
                source,
                "test",
                inspection.Evidence!,
                reader);

            await action.Should().NotThrowAsync();
            Directory.Exists(source).Should().BeFalse();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Preparation_refuses_duplicate_overlapping_and_quarantine_overlap_paths()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source");
            var child = Path.Combine(source, "child");
            var quarantineRoot = Path.Combine(root, "quarantine");
            Directory.CreateDirectory(child);
            var reader = new ScriptedIdentityReader(path =>
                QuarantineCandidateInspection.Accepted(Expected(path)));

            var duplicate = QuarantineOperationPolicy.PrepareForConfirmation(
                CandidateOperation([source, source]), quarantineRoot, reader);
            var overlap = QuarantineOperationPolicy.PrepareForConfirmation(
                CandidateOperation([source, child]), quarantineRoot, reader);
            var quarantineOverlap = QuarantineOperationPolicy.PrepareForConfirmation(
                CandidateOperation([root]), quarantineRoot, reader);
            var protectedRoot = QuarantineOperationPolicy.PrepareForConfirmation(
                CandidateOperation([Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)]),
                quarantineRoot,
                reader);

            duplicate.Success.Should().BeFalse();
            overlap.Success.Should().BeFalse();
            quarantineOverlap.Success.Should().BeFalse();
            protectedRoot.Success.Should().BeFalse();
            duplicate.Error.Should().Contain("重复");
            overlap.Error.Should().Contain("互相包含");
            quarantineOverlap.Error.Should().Contain("隔离区");
            protectedRoot.Error.Should().Contain("用户数据根目录");
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Windows_identity_reader_contains_reparse_ads_and_double_chain_checks()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Css.Win32",
            "Quarantine",
            "WindowsQuarantineCandidateIdentityReader.cs"));

        source.Should().Contain("FileAttributes.ReparsePoint");
        source.Should().Contain("HasAlternateDataStream");
        source.Should().Contain("TryInspectCurrentPath");
        source.Should().Contain("HasReparsePointInExistingChain");
        source.IndexOf("TryInspectCurrentPath", StringComparison.Ordinal)
            .Should().BeLessThan(source.IndexOf("CreateFileW(", StringComparison.Ordinal));
        source.LastIndexOf("HasReparsePointInExistingChain", StringComparison.Ordinal)
            .Should().BeGreaterThan(source.IndexOf("GetFileInformationByHandle(handle", StringComparison.Ordinal));
        source.Should().Contain("GetFileInformationByHandle");
    }

    [Fact]
    public void Production_entry_points_bind_identity_and_service_revalidates_after_manifest_before_move()
    {
        var root = FindRepositoryRoot();
        var main = File.ReadAllText(Path.Combine(root, "src", "Css.App", "MainWindow.xaml.cs"));
        var handler = File.ReadAllText(Path.Combine(
            root, "src", "Css.Core", "Quarantine", "QuarantineOperationHandler.cs"));
        var service = File.ReadAllText(Path.Combine(
            root, "src", "Css.Core", "Quarantine", "FileQuarantineService.cs"));

        Occurrences(main, "QuarantineOperationPolicy.PrepareForConfirmation(")
            .Should().Be(3, "C-drive cleanup, application cache, and uninstall residue all prepare identity");
        Occurrences(main, "CleanupConfirmationPresenter.Create(preparedOperation")
            .Should().Be(3, "all three dialogs must show only an identity-bound operation");
        handler.IndexOf("var preflight = QuarantineCandidateEvidencePolicy.Revalidate", StringComparison.Ordinal)
            .Should().BeLessThan(handler.IndexOf("var records = new List<QuarantineRecord>()", StringComparison.Ordinal));

        var manifest = service.IndexOf("await WriteManifestAsync(record, ct);", StringComparison.Ordinal);
        var finalValidation = service.IndexOf(
            "originalPath = ValidateMoveCandidate(originalPath, expectedEvidence, identityReader);",
            manifest,
            StringComparison.Ordinal);
        manifest.Should().BeGreaterThanOrEqualTo(0);
        finalValidation.Should().BeGreaterThan(manifest);
        finalValidation.Should().BeLessThan(service.IndexOf("File.Move(originalPath, quarantinedPath);", StringComparison.Ordinal));
        finalValidation.Should().BeLessThan(service.IndexOf("Directory.Move(originalPath, quarantinedPath);", StringComparison.Ordinal));
    }

    private static OperationDescriptor CandidateOperation(IReadOnlyList<string> paths) =>
        new()
        {
            Kind = "clean.temp",
            Title = "清理临时文件",
            Source = OperationSource.Manual,
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EvidenceSummary = "测试临时文件",
            AffectedPaths = paths
        };

    private static QuarantineCandidateEvidence Expected(string path) =>
        new()
        {
            CanonicalPath = Path.GetFullPath(path),
            Kind = Directory.Exists(path)
                ? QuarantineCandidateKind.Directory
                : QuarantineCandidateKind.File,
            VolumeSerialNumber = 7,
            FileId = 11,
            CreationTimeUtcTicks = 13,
            LastWriteTimeUtcTicks = 17,
            LengthBytes = 19
        };

    private sealed class ScriptedIdentityReader(
        Func<string, QuarantineCandidateInspection> inspect) : IQuarantineCandidateIdentityReader
    {
        public QuarantineCandidateInspection Inspect(string path) => inspect(path);
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-quarantine-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (!Directory.Exists(root))
            return;

        foreach (var entry in Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories)
                     .OrderByDescending(path => path.Length))
        {
            try
            {
                File.SetAttributes(entry, FileAttributes.Normal);
            }
            catch
            {
                // Best-effort cleanup for isolated test data.
            }
        }
        Directory.Delete(root, recursive: true);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "ComputerSecuritySoftware.slnx")))
                return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static int Occurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
