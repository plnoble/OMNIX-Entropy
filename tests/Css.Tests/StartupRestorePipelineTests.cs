using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Startup;
using Css.Core.Timeline;
using FluentAssertions;

namespace Css.Tests;

public sealed class StartupRestorePipelineTests
{
    [Fact]
    public async Task Preparation_loads_current_startup_entry_and_binds_verified_manifest()
    {
        using var fixture = await RestoreFixture.CreateAsync();

        var prepared = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
            fixture.Entry.Id,
            fixture.Timeline,
            fixture.Manifests);

        prepared.Success.Should().BeTrue();
        prepared.CurrentEntry.Should().NotBeNull();
        prepared.Operation.Should().NotBeNull();
        prepared.Operation!.Kind.Should().Be(StartupEntryControlOperationPolicy.RestoreKind);
        prepared.Operation.Source.Should().Be(OperationSource.Manual);
        prepared.Operation.Risk.Should().Be(RiskLevel.Medium);
        prepared.Operation.IsDestructive.Should().BeTrue();
        prepared.Operation.RequiresSnapshot.Should().BeTrue();
        prepared.Operation.SnapshotId.Should().Be(fixture.Manifest.SnapshotId);
        prepared.Operation.ConfirmationAccepted.Should().BeFalse();
        prepared.Operation.AffectedRegistryKeys.Should()
            .Equal(StartupEntryControlPolicy.SupportedSourceLocator);
        prepared.Operation.EvidenceSummary.Should().NotContain(fixture.Root);
        prepared.Operation.ConfirmationText.Should().NotContain(fixture.Root);
    }

    [Fact]
    public async Task Confirmed_restore_runs_through_pipeline_and_updates_same_timeline_entry()
    {
        using var fixture = await RestoreFixture.CreateAsync();
        var prepared = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
            fixture.Entry.Id,
            fixture.Timeline,
            fixture.Manifests);
        var handler = new StartupRestoreOperationHandler(
            fixture.Store,
            fixture.Manifests,
            fixture.Timeline);
        var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

        var refused = await pipeline.ExecuteAsync(prepared.Operation!);
        var confirmed = StartupRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
        var restored = await pipeline.ExecuteAsync(confirmed);
        var current = await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id);

        refused.Success.Should().BeFalse();
        restored.Success.Should().BeTrue();
        fixture.Store.Restored.Should().ContainSingle()
            .Which.StateFingerprint.Should().Be(fixture.State.StateFingerprint);
        current.Should().NotBeNull();
        current!.RestoreState.Should().Be(RestoreState.Restored);
        current.RestoreOperationKind.Should().BeNull();
    }

    [Fact]
    public async Task Manifest_changed_after_confirmation_is_refused_before_registry_restore()
    {
        using var fixture = await RestoreFixture.CreateAsync();
        var prepared = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
            fixture.Entry.Id,
            fixture.Timeline,
            fixture.Manifests);
        var confirmed = StartupRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
        await File.AppendAllTextAsync(fixture.Manifest.ManifestPath, "changed after confirmation");
        var handler = new StartupRestoreOperationHandler(
            fixture.Store,
            fixture.Manifests,
            fixture.Timeline);

        var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
            .ExecuteAsync(confirmed);
        var current = await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id);

        result.Success.Should().BeFalse();
        fixture.Store.Restored.Should().BeEmpty();
        current!.RestoreState.Should().Be(RestoreState.Restorable);
        current.RestoreOperationKind.Should().Be(StartupEntryControlOperationPolicy.RestoreKind);
    }

    [Fact]
    public async Task Timeline_state_changed_after_confirmation_is_refused_before_registry_restore()
    {
        using var fixture = await RestoreFixture.CreateAsync();
        var prepared = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
            fixture.Entry.Id,
            fixture.Timeline,
            fixture.Manifests);
        var confirmed = StartupRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
        await fixture.Timeline.UpdateRestoreStateAsync(
            fixture.Entry.Id,
            RestoreState.NotRestorable,
            null);
        var handler = new StartupRestoreOperationHandler(
            fixture.Store,
            fixture.Manifests,
            fixture.Timeline);

        var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
            .ExecuteAsync(confirmed);

        result.Success.Should().BeFalse();
        fixture.Store.Restored.Should().BeEmpty();
        (await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id))!.RestoreState
            .Should().Be(RestoreState.NotRestorable);
    }

    [Fact]
    public async Task Mismatched_timeline_registry_scope_is_refused_during_preparation()
    {
        using var fixture = await RestoreFixture.CreateAsync(@"HKCU64\Software\Other\Run");

        var prepared = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
            fixture.Entry.Id,
            fixture.Timeline,
            fixture.Manifests);

        prepared.Success.Should().BeFalse();
        prepared.Operation.Should().BeNull();
        fixture.Store.Restored.Should().BeEmpty();
    }

    [Fact]
    public async Task Failed_registry_restore_marks_same_timeline_entry_partially_restorable()
    {
        using var fixture = await RestoreFixture.CreateAsync(restoreSucceeds: false);
        var prepared = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
            fixture.Entry.Id,
            fixture.Timeline,
            fixture.Manifests);
        var confirmed = StartupRestoreOperationPolicy.ConfirmForExecution(prepared.Operation!);
        var handler = new StartupRestoreOperationHandler(
            fixture.Store,
            fixture.Manifests,
            fixture.Timeline);

        var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
            .ExecuteAsync(confirmed);
        var current = await fixture.Timeline.LoadByIdAsync(fixture.Entry.Id);

        result.Success.Should().BeFalse();
        fixture.Store.Restored.Should().ContainSingle();
        current!.RestoreState.Should().Be(RestoreState.PartiallyRestorable);
        current.RestoreOperationKind.Should().Be(StartupEntryControlOperationPolicy.RestoreKind);
    }

    [Fact]
    public void Main_window_routes_startup_restore_through_preparation_and_safety_pipeline()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var start = main.IndexOf(
            "private async Task RestoreStartupTimelineItemAsync",
            StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = main.IndexOf(
            "private static bool ExistingPathExists",
            start,
            StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        var method = main[start..end];

        method.Should().Contain("StartupRestoreOperationPolicy.PrepareForConfirmationAsync")
            .And.Contain("StartupRestoreOperationPolicy.ConfirmForExecution")
            .And.Contain("new StartupRestoreOperationHandler(")
            .And.Contain("new SafetyOperationPipeline(handler.ExecuteAsync)")
            .And.NotContain("handler.RestoreAsync(")
            .And.NotContain("_timelineStore.UpdateRestoreStateAsync(");
        method.IndexOf("StartupRestoreOperationPolicy.PrepareForConfirmationAsync", StringComparison.Ordinal)
            .Should().BeLessThan(method.IndexOf("new TimelineRestoreConfirmationWindow", StringComparison.Ordinal));
    }

    private sealed class RestoreFixture : IDisposable
    {
        private RestoreFixture(
            string root,
            StartupEntryState state,
            StartupRollbackManifestEvidence manifest,
            ActionTimelineEntry entry,
            FakeStartupStore store,
            StartupRollbackManifestStore manifests,
            ActionTimelineStore timeline)
        {
            Root = root;
            State = state;
            Manifest = manifest;
            Entry = entry;
            Store = store;
            Manifests = manifests;
            Timeline = timeline;
        }

        public string Root { get; }
        public StartupEntryState State { get; }
        public StartupRollbackManifestEvidence Manifest { get; }
        public ActionTimelineEntry Entry { get; }
        public FakeStartupStore Store { get; }
        public StartupRollbackManifestStore Manifests { get; }
        public ActionTimelineStore Timeline { get; }

        public static async Task<RestoreFixture> CreateAsync(
            string? affectedRegistryKey = null,
            bool restoreSucceeds = true)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "omnix-startup-restore-pipeline-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var now = new DateTimeOffset(2026, 7, 16, 14, 0, 0, TimeSpan.Zero);
            const string valueName = "Fixture Startup";
            var approval = StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                valueName,
                new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            var observation = BackgroundComponentObservationFactory.Startup(
                valueName,
                StartupEntryControlPolicy.SupportedSourceLocator,
                "fixture.exe --background",
                now,
                approval);
            var state = StartupEntryStateFactory.Create(
                observation,
                StartupRegistryValueKind.String,
                "fixture.exe --background",
                new string('A', 64),
                now);
            var preparation = new StartupControlPreparation
            {
                Status = StartupControlPreparationStatus.Ready,
                SoftwareName = "Fixture App",
                Summary = "Fixture-only startup restore evidence.",
                Lines = ["Fixture-only startup restore evidence."],
                State = state
            };
            var manifests = new StartupRollbackManifestStore(Path.Combine(root, "manifests"));
            var manifest = await manifests.CreateAsync(preparation, now);
            var timeline = new ActionTimelineStore(Path.Combine(root, "timeline.db"));
            await timeline.AddAsync(new ActionTimelineEntry
            {
                OccurredAt = now,
                Source = OperationSource.Manual,
                Title = "Disable fixture startup",
                EvidenceSummary = "One startup entry was disabled with rollback evidence.",
                AffectedRegistryKeys =
                [
                    affectedRegistryKey ?? StartupEntryControlPolicy.SupportedSourceLocator
                ],
                RestoreState = RestoreState.Restorable,
                RestoreOperationKind = StartupEntryControlOperationPolicy.RestoreKind,
                RestoreManifestPaths = [manifest.ManifestPath]
            });
            var entry = (await timeline.LoadRecentAsync(1)).Single();
            return new RestoreFixture(
                root,
                state,
                manifest,
                entry,
                new FakeStartupStore(restoreSucceeds),
                manifests,
                timeline);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class FakeStartupStore(bool restoreSucceeds) : IStartupEntryControlStore
    {
        public List<StartupEntryState> Restored { get; } = [];

        public Task<StartupEntryCaptureResult> CaptureAsync(
            BackgroundComponentObservation observation,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(StartupEntryCaptureResult.Refused("not used"));

        public Task<StartupEntryMutationResult> DisableAsync(
            StartupEntryState expected,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(StartupEntryMutationResult.Refused("not used"));

        public Task<StartupEntryMutationResult> RestoreAsync(
            StartupEntryState expected,
            CancellationToken cancellationToken = default)
        {
            Restored.Add(expected);
            return Task.FromResult(restoreSucceeds
                ? StartupEntryMutationResult.Completed("restored")
                : StartupEntryMutationResult.Refused("restore refused"));
        }
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException(
            "Repository file was not found.",
            Path.Combine(segments));
    }
}
