using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Timeline;
using FluentAssertions;
using System.Text.Json;

namespace Css.Tests;

public sealed class QuarantineRestorePipelineTests
{
    [Fact]
    public async Task Preparation_loads_the_current_restorable_entry_and_binds_payload_identity()
    {
        var fixture = await RestoreFixture.CreateAsync();
        try
        {
            var prepared = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                fixture.Entry.Id,
                fixture.Timeline,
                fixture.Quarantine,
                fixture.IdentityReader);

            prepared.Success.Should().BeTrue();
            prepared.Operation.Should().NotBeNull();
            prepared.Operation!.Kind.Should().Be(QuarantineRestoreOperationPolicy.OperationKind);
            prepared.Operation.IsDestructive.Should().BeTrue();
            prepared.Operation.Risk.Should().Be(RiskLevel.Low);
            prepared.Operation.ConfirmationAccepted.Should().BeFalse();
            prepared.Operation.AffectedPaths.Should().Equal(fixture.Record.OriginalPath);
            prepared.Operation.EvidenceSummary.Should().NotContain(fixture.Root);

            await fixture.Timeline.UpdateRestoreStateAsync(
                fixture.Entry.Id,
                RestoreState.Restored,
                null);
            var stale = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                fixture.Entry.Id,
                fixture.Timeline,
                fixture.Quarantine,
                fixture.IdentityReader);

            stale.Success.Should().BeFalse();
            stale.Operation.Should().BeNull();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Confirmed_restore_runs_through_pipeline_and_updates_the_same_timeline_entry()
    {
        var fixture = await RestoreFixture.CreateAsync();
        try
        {
            var prepared = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                fixture.Entry.Id,
                fixture.Timeline,
                fixture.Quarantine,
                fixture.IdentityReader);
            var handler = new QuarantineRestoreOperationHandler(
                fixture.Quarantine,
                fixture.Timeline,
                fixture.IdentityReader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var refused = await pipeline.ExecuteAsync(prepared.Operation!);
            var confirmed = QuarantineRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
            var restored = await pipeline.ExecuteAsync(confirmed);
            var reloaded = await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id);

            refused.Success.Should().BeFalse();
            restored.Success.Should().BeTrue();
            File.Exists(fixture.SourcePath).Should().BeTrue();
            File.Exists(fixture.Record.QuarantinedPath).Should().BeFalse();
            reloaded.Should().NotBeNull();
            reloaded!.RestoreState.Should().Be(RestoreState.Restored);
            reloaded.RestoreOperationKind.Should().BeNull();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Payload_changed_after_confirmation_is_refused_before_restore()
    {
        var fixture = await RestoreFixture.CreateAsync();
        try
        {
            var prepared = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                fixture.Entry.Id,
                fixture.Timeline,
                fixture.Quarantine,
                fixture.IdentityReader);
            var confirmed = QuarantineRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
            await File.AppendAllTextAsync(fixture.Record.QuarantinedPath, "changed after confirmation");
            var handler = new QuarantineRestoreOperationHandler(
                fixture.Quarantine,
                fixture.Timeline,
                fixture.IdentityReader);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(confirmed);
            var reloaded = await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id);

            result.Success.Should().BeFalse();
            File.Exists(fixture.SourcePath).Should().BeFalse();
            File.Exists(fixture.Record.QuarantinedPath).Should().BeTrue();
            reloaded.Should().NotBeNull();
            reloaded!.RestoreState.Should().Be(RestoreState.Restorable);
            reloaded.RestoreOperationKind.Should().Be(QuarantineRestoreOperationPolicy.OperationKind);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Manifest_changed_after_confirmation_is_refused_before_restore()
    {
        var fixture = await RestoreFixture.CreateAsync();
        try
        {
            var prepared = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                fixture.Entry.Id,
                fixture.Timeline,
                fixture.Quarantine,
                fixture.IdentityReader);
            var confirmed = QuarantineRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
            var current = JsonSerializer.Deserialize<QuarantineRecord>(
                await File.ReadAllTextAsync(fixture.Record.ManifestPath));
            current.Should().NotBeNull();
            await File.WriteAllTextAsync(
                fixture.Record.ManifestPath,
                JsonSerializer.Serialize(new QuarantineRecord
                {
                    Id = current!.Id,
                    MovedAt = current.MovedAt,
                    OriginalPath = current.OriginalPath,
                    QuarantinedPath = current.QuarantinedPath,
                    ManifestPath = current.ManifestPath,
                    Reason = current.Reason + " changed",
                    SizeBytes = current.SizeBytes,
                    RestoreState = current.RestoreState,
                    RestoredAt = current.RestoredAt
                }));
            var handler = new QuarantineRestoreOperationHandler(
                fixture.Quarantine,
                fixture.Timeline,
                fixture.IdentityReader);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(confirmed);

            result.Success.Should().BeFalse();
            File.Exists(fixture.SourcePath).Should().BeFalse();
            File.Exists(fixture.Record.QuarantinedPath).Should().BeTrue();
            (await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id))!.RestoreState
                .Should().Be(RestoreState.Restorable);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Timeline_state_changed_after_confirmation_is_refused_before_restore()
    {
        var fixture = await RestoreFixture.CreateAsync();
        try
        {
            var prepared = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                fixture.Entry.Id,
                fixture.Timeline,
                fixture.Quarantine,
                fixture.IdentityReader);
            var confirmed = QuarantineRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
            await fixture.Timeline.UpdateRestoreStateAsync(
                fixture.Entry.Id,
                RestoreState.NotRestorable,
                null);
            var handler = new QuarantineRestoreOperationHandler(
                fixture.Quarantine,
                fixture.Timeline,
                fixture.IdentityReader);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(confirmed);

            result.Success.Should().BeFalse();
            File.Exists(fixture.SourcePath).Should().BeFalse();
            File.Exists(fixture.Record.QuarantinedPath).Should().BeTrue();
            (await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id))!.RestoreState
                .Should().Be(RestoreState.NotRestorable);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public void Main_window_routes_quarantine_restore_through_the_safety_pipeline()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var start = main.IndexOf(
            "private async Task RestoreQuarantineTimelineItemAsync",
            StringComparison.Ordinal);
        var end = main.IndexOf(
            "private async Task ReviewSelectedUninstallResidueAsync",
            start,
            StringComparison.Ordinal);

        start.Should().BeGreaterThanOrEqualTo(0);
        end.Should().BeGreaterThan(start);
        var restoreMethod = main[start..end];

        restoreMethod.Should().Contain("QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync")
            .And.Contain("QuarantineRestoreOperationPolicy.ConfirmForExecution")
            .And.Contain("new QuarantineRestoreOperationHandler(")
            .And.Contain("new SafetyOperationPipeline(handler.ExecuteAsync)")
            .And.NotContain("_quarantineService.RestoreAsync(");
        restoreMethod.IndexOf("QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync", StringComparison.Ordinal)
            .Should().BeLessThan(restoreMethod.IndexOf("new TimelineRestoreConfirmationWindow", StringComparison.Ordinal));
    }

    private sealed class RestoreFixture : IDisposable
    {
        private RestoreFixture(
            string root,
            string sourcePath,
            QuarantineRecord record,
            ActionTimelineEntry entry,
            FileQuarantineService quarantine,
            ActionTimelineStore timeline,
            IQuarantineCandidateIdentityReader identityReader)
        {
            Root = root;
            SourcePath = sourcePath;
            Record = record;
            Entry = entry;
            Quarantine = quarantine;
            Timeline = timeline;
            IdentityReader = identityReader;
        }

        public string Root { get; }
        public string SourcePath { get; }
        public QuarantineRecord Record { get; }
        public ActionTimelineEntry Entry { get; }
        public FileQuarantineService Quarantine { get; }
        public ActionTimelineStore Timeline { get; }
        public IQuarantineCandidateIdentityReader IdentityReader { get; }

        public static async Task<RestoreFixture> CreateAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "omnix-restore-pipeline-" + Guid.NewGuid().ToString("N"));
            var source = Path.Combine(root, "source", "cache.tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var quarantine = new FileQuarantineService(Path.Combine(root, "quarantine"));
            var record = await quarantine.QuarantineAsync(source, "fixture cleanup");
            var timeline = new ActionTimelineStore(Path.Combine(root, "timeline.db"));
            await timeline.AddAsync(new ActionTimelineEntry
            {
                OccurredAt = new DateTimeOffset(2026, 7, 16, 13, 0, 0, TimeSpan.Zero),
                Source = OperationSource.Manual,
                Title = "清理临时文件",
                EvidenceSummary = "已移动到隔离区",
                AffectedPaths = [record.OriginalPath],
                RestoreState = RestoreState.Restorable,
                RestoreOperationKind = QuarantineRestoreOperationPolicy.OperationKind,
                RestoreManifestPaths = [record.ManifestPath]
            });
            var entry = (await timeline.LoadRecentAsync(1)).Single();
            return new RestoreFixture(
                root,
                source,
                record,
                entry,
                quarantine,
                timeline,
                new FixtureIdentityReader());
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class FixtureIdentityReader : IQuarantineCandidateIdentityReader
    {
        public QuarantineCandidateInspection Inspect(string path)
        {
            if (!QuarantineCandidatePathPolicy.TryInspectCurrentPath(
                    path,
                    out var canonical,
                    out var kind,
                    out var error))
            {
                return QuarantineCandidateInspection.Refused(error);
            }

            var creation = kind == QuarantineCandidateKind.File
                ? File.GetCreationTimeUtc(canonical)
                : Directory.GetCreationTimeUtc(canonical);
            var lastWrite = kind == QuarantineCandidateKind.File
                ? File.GetLastWriteTimeUtc(canonical)
                : Directory.GetLastWriteTimeUtc(canonical);
            var length = kind == QuarantineCandidateKind.File
                ? new FileInfo(canonical).Length
                : 0;
            return QuarantineCandidateInspection.Accepted(new QuarantineCandidateEvidence
            {
                CanonicalPath = canonical,
                Kind = kind,
                VolumeSerialNumber = 1,
                FileId = StableFileId(canonical),
                CreationTimeUtcTicks = creation.Ticks,
                LastWriteTimeUtcTicks = lastWrite.Ticks,
                LengthBytes = length
            });
        }

        private static ulong StableFileId(string path)
        {
            ulong value = 14695981039346656037;
            foreach (var character in path.ToUpperInvariant())
            {
                value ^= character;
                value *= 1099511628211;
            }
            return value == 0 ? 1 : value;
        }
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(segments));
    }
}
