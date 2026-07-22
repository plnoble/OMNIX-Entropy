using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Startup;
using Css.Core.Timeline;
using FluentAssertions;

namespace Css.Tests;

public sealed class StartupEntryControlTests
{
    [Fact]
    public async Task Preparation_accepts_one_fresh_current_user_run_value()
    {
        var now = DateTimeOffset.UtcNow;
        var observation = StartupObservation(now);
        var state = StartupEntryStateFactory.Create(
            observation,
            StartupRegistryValueKind.String,
            "\"D:\\Software\\Fixture\\fixture.exe\" --background",
            Sha('B'),
            now);
        var store = new FakeStartupStore(state);

        var result = await StartupControlPreparationService.PrepareAsync(
            Profile(observation),
            store,
            now);

        result.Status.Should().Be(StartupControlPreparationStatus.Ready);
        result.CanContinue.Should().BeTrue();
        result.State.Should().BeSameAs(state);
        result.Summary.Should().NotContain(@"D:\Software");
        result.Lines.Should().Contain(line => line.Contains("服务") && line.Contains("计划任务"));
        store.Captured.Should().ContainSingle().Which.Should().BeSameAs(observation);
    }

    [Theory]
    [InlineData(SoftwareCategory.SystemTool, StartupEntryControlPolicy.SupportedSourceLocator)]
    [InlineData(SoftwareCategory.Normal, @"HKLM64\Software\Microsoft\Windows\CurrentVersion\Run")]
    public async Task Preparation_refuses_system_or_machine_wide_candidates(
        SoftwareCategory category,
        string sourceLocator)
    {
        var now = DateTimeOffset.UtcNow;
        var observation = StartupObservation(now, sourceLocator: sourceLocator);
        var store = new FakeStartupStore(null);

        var result = await StartupControlPreparationService.PrepareAsync(
            Profile(observation, category),
            store,
            now);

        result.CanContinue.Should().BeFalse();
        store.Captured.Should().BeEmpty();
    }

    [Fact]
    public async Task Preparation_refuses_multiple_or_drifted_startup_candidates()
    {
        var now = DateTimeOffset.UtcNow;
        var first = StartupObservation(now, name: "Fixture A");
        var second = StartupObservation(now, name: "Fixture B");
        var state = StartupEntryStateFactory.Create(
            first,
            StartupRegistryValueKind.String,
            "fixture.exe",
            Sha('C'),
            now) with { ObservationFingerprint = Sha('D') };

        var multiple = await StartupControlPreparationService.PrepareAsync(
            Profile(first, additional: [second]),
            new FakeStartupStore(state),
            now);
        var drifted = await StartupControlPreparationService.PrepareAsync(
            Profile(first),
            new FakeStartupStore(state),
            now);

        multiple.Status.Should().Be(StartupControlPreparationStatus.Ambiguous);
        drifted.Status.Should().Be(StartupControlPreparationStatus.Stale);
        multiple.CanContinue.Should().BeFalse();
        drifted.CanContinue.Should().BeFalse();
    }

    [Fact]
    public async Task Manifest_round_trip_is_bounded_and_tamper_evident()
    {
        var root = TempRoot();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var observation = StartupObservation(now);
            var state = StartupEntryStateFactory.Create(
                observation,
                StartupRegistryValueKind.ExpandString,
                @"%LOCALAPPDATA%\Fixture\fixture.exe",
                Sha('E'),
                now);
            var preparation = await StartupControlPreparationService.PrepareAsync(
                Profile(observation),
                new FakeStartupStore(state),
                now);
            var manifests = new StartupRollbackManifestStore(root);

            var evidence = await manifests.CreateAsync(preparation, now);
            var loaded = await manifests.LoadVerifiedAsync(evidence);

            loaded.Manifest.State.ValueData.Should().Be(state.ValueData);
            loaded.Manifest.State.ValueKind.Should().Be(StartupRegistryValueKind.ExpandString);
            loaded.Manifest.State.KeyAclSha256.Should().Be(Sha('E'));
            evidence.Sha256.Should().HaveLength(64);
            evidence.ManifestPath.Should().StartWith(root);

            await File.AppendAllTextAsync(evidence.ManifestPath, " ");
            var action = () => manifests.LoadVerifiedAsync(evidence);
            await action.Should().ThrowAsync<InvalidDataException>();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Cancelled_uncommitted_manifest_is_deleted_only_after_verification()
    {
        var root = TempRoot();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var observation = StartupObservation(now);
            var state = StartupEntryStateFactory.Create(
                observation,
                StartupRegistryValueKind.String,
                "fixture.exe",
                Sha('9'),
                now);
            var preparation = await StartupControlPreparationService.PrepareAsync(
                Profile(observation),
                new FakeStartupStore(state),
                now);
            var manifests = new StartupRollbackManifestStore(root);
            var evidence = await manifests.CreateAsync(preparation, now);

            var deleted = await manifests.DeleteUncommittedAsync(evidence);

            deleted.Should().BeTrue();
            File.Exists(evidence.ManifestPath).Should().BeFalse();

            var tampered = await manifests.CreateAsync(preparation, now);
            await File.AppendAllTextAsync(tampered.ManifestPath, " ");
            var action = () => manifests.DeleteUncommittedAsync(tampered);
            await action.Should().ThrowAsync<InvalidDataException>();
            File.Exists(tampered.ManifestPath).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Confirmed_operation_disables_exact_value_and_records_restorable_timeline()
    {
        var root = TempRoot();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var observation = StartupObservation(now);
            var state = StartupEntryStateFactory.Create(
                observation,
                StartupRegistryValueKind.String,
                "fixture.exe --background",
                Sha('F'),
                now);
            var store = new FakeStartupStore(state);
            var preparation = await StartupControlPreparationService.PrepareAsync(
                Profile(observation),
                store,
                now);
            var manifests = new StartupRollbackManifestStore(Path.Combine(root, "manifests"));
            var evidence = await manifests.CreateAsync(preparation, now);
            var candidate = StartupEntryControlOperationPolicy.CreateDisablePlan(preparation, evidence);
            var timeline = new ActionTimelineStore(Path.Combine(root, "data.db"));
            var handler = new StartupEntryControlOperationHandler(store, manifests, timeline, () => now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var refused = await pipeline.ExecuteAsync(candidate);
            var result = await pipeline.ExecuteAsync(
                StartupEntryControlOperationPolicy.ConfirmForExecution(candidate));

            refused.Success.Should().BeFalse();
            result.Success.Should().BeTrue();
            store.Disabled.Should().ContainSingle().Which.StateFingerprint.Should().Be(state.StateFingerprint);
            var entry = (await timeline.LoadRecentAsync(10)).Should().ContainSingle().Subject;
            entry.RestoreState.Should().Be(RestoreState.Restorable);
            entry.RestoreOperationKind.Should().Be(StartupEntryControlOperationPolicy.RestoreKind);
            entry.RestoreManifestPaths.Should().Equal(evidence.ManifestPath);
            entry.AffectedRegistryKeys.Should().Equal(StartupEntryControlPolicy.SupportedSourceLocator);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Timeline_failure_restores_the_value_before_reporting_failure()
    {
        var root = TempRoot();
        try
        {
            var now = DateTimeOffset.UtcNow;
            var observation = StartupObservation(now);
            var state = StartupEntryStateFactory.Create(
                observation,
                StartupRegistryValueKind.String,
                "fixture.exe",
                Sha('A'),
                now);
            var store = new FakeStartupStore(state);
            var preparation = await StartupControlPreparationService.PrepareAsync(
                Profile(observation),
                store,
                now);
            var manifests = new StartupRollbackManifestStore(Path.Combine(root, "manifests"));
            var evidence = await manifests.CreateAsync(preparation, now);
            var descriptor = StartupEntryControlOperationPolicy.ConfirmForExecution(
                StartupEntryControlOperationPolicy.CreateDisablePlan(preparation, evidence));
            var unwritableDatabase = Path.Combine(root, "database-directory");
            Directory.CreateDirectory(unwritableDatabase);
            var handler = new StartupEntryControlOperationHandler(
                store,
                manifests,
                new ActionTimelineStore(unwritableDatabase),
                () => now);

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(descriptor);

            result.Success.Should().BeFalse();
            store.Disabled.Should().ContainSingle();
            store.Restored.Should().ContainSingle();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static BackgroundComponentObservation StartupObservation(
        DateTimeOffset now,
        string name = "Fixture Startup",
        string sourceLocator = StartupEntryControlPolicy.SupportedSourceLocator) =>
        BackgroundComponentObservationFactory.Startup(
            name,
            sourceLocator,
            "fixture.exe --background",
            now,
            StartupApprovalObservationFactory.FromRegistryValue(
                StartupEntryControlPolicy.SupportedApprovalLocator,
                name,
                new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

    private static SoftwareProfile Profile(
        BackgroundComponentObservation startup,
        SoftwareCategory category = SoftwareCategory.Normal,
        IReadOnlyList<BackgroundComponentObservation>? additional = null) =>
        new()
        {
            Name = "Fixture App",
            Category = category,
            StartupEntries = [startup.Identity.DisplayName],
            BackgroundComponents = [startup, .. additional ?? []]
        };

    private static string Sha(char value) => new(value, 64);

    private static string TempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-startup-control-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed class FakeStartupStore(StartupEntryState? captured) : IStartupEntryControlStore
    {
        public List<BackgroundComponentObservation> Captured { get; } = [];
        public List<StartupEntryState> Disabled { get; } = [];
        public List<StartupEntryState> Restored { get; } = [];

        public Task<StartupEntryCaptureResult> CaptureAsync(
            BackgroundComponentObservation observation,
            CancellationToken cancellationToken = default)
        {
            Captured.Add(observation);
            return Task.FromResult(captured is null
                ? StartupEntryCaptureResult.Refused("not available")
                : StartupEntryCaptureResult.Completed(captured));
        }

        public Task<StartupEntryMutationResult> DisableAsync(
            StartupEntryState expected,
            CancellationToken cancellationToken = default)
        {
            Disabled.Add(expected);
            return Task.FromResult(StartupEntryMutationResult.Completed("disabled"));
        }

        public Task<StartupEntryMutationResult> RestoreAsync(
            StartupEntryState expected,
            CancellationToken cancellationToken = default)
        {
            Restored.Add(expected);
            return Task.FromResult(StartupEntryMutationResult.Completed("restored"));
        }
    }
}
