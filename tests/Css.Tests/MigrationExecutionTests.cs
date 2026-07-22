using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationExecutionTests
{
    [Fact]
    public async Task Manifest_hash_is_verified_and_tampering_is_rejected()
    {
        var fixture = await Fixture.CreateAsync();
        try
        {
            fixture.Evidence.Sha256.Should().MatchRegex("^[0-9A-F]{64}$");
            var verified = await MigrationRollbackManifestStore.ReadVerifiedAsync(
                fixture.Evidence.ManifestPath,
                fixture.Evidence.Sha256);
            verified.IsValid.Should().BeTrue();
            verified.Manifest!.SnapshotId.Should().Be(fixture.SnapshotId);

            await File.AppendAllTextAsync(fixture.Evidence.ManifestPath, " ");
            var tampered = await MigrationRollbackManifestStore.ReadVerifiedAsync(
                fixture.Evidence.ManifestPath,
                fixture.Evidence.Sha256);
            tampered.IsValid.Should().BeFalse();
            tampered.Error.Should().Contain("hash");
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Active_component_refuses_before_any_path_mutation()
    {
        var fixture = await Fixture.CreateAsync();
        try
        {
            var adapter = FakePathAdapter.ForManifest(fixture.Evidence.Manifest);
            var handler = fixture.Handler(
                adapter,
                new FakeActivityProbe(["process: fixture"]));
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(fixture.ConfirmedOperation());

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("still active");
            adapter.MoveOrder.Should().BeEmpty();
            adapter.RollbackOrder.Should().BeEmpty();
            fixture.MonitorStore.Saved.Should().BeNull();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Source_changed_after_snapshot_is_refused_before_any_path_mutation()
    {
        var fixture = await Fixture.CreateAsync();
        try
        {
            var adapter = FakePathAdapter.ForManifest(fixture.Evidence.Manifest);
            var handler = fixture.Handler(
                adapter,
                new FakeActivityProbe([]),
                snapshotSourceReader: new ChangedSnapshotSourceReader(fixture.SnapshotEvidence));

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(fixture.ConfirmedOperation());

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("changed after the snapshot");
            adapter.MoveOrder.Should().BeEmpty();
            adapter.RollbackOrder.Should().BeEmpty();
            fixture.MonitorStore.Saved.Should().BeNull();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Successful_migration_records_redirects_and_detects_write_return()
    {
        var fixture = await Fixture.CreateAsync();
        try
        {
            var adapter = FakePathAdapter.ForManifest(fixture.Evidence.Manifest);
            var handler = fixture.Handler(adapter, new FakeActivityProbe([]));
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(fixture.ConfirmedOperation());

            result.Success.Should().BeTrue();
            var execution = result.Payload.Should().BeOfType<MigrationExecutionResult>().Subject;
            execution.Status.Should().Be(MigrationExecutionStatus.Completed);
            execution.MovedPathCount.Should().Be(fixture.Evidence.Manifest.Entries.Count);
            execution.MonitoringRecord.Should().NotBeNull();
            fixture.MonitorStore.Saved.Should().BeSameAs(execution.MonitoringRecord);

            var healthy = await MigrationClosureMonitor.ScanAsync(
                execution.MonitoringRecord!,
                adapter);
            healthy.Should().OnlyContain(finding =>
                finding.Kind == MigrationClosureFindingKind.RedirectHealthy);

            adapter.ReplaceRedirectWithRealDirectory(
                execution.MonitoringRecord!.Paths[0].OriginalPath);
            var returned = await MigrationClosureMonitor.ScanAsync(
                execution.MonitoringRecord,
                adapter);
            returned.Should().Contain(finding =>
                finding.Kind == MigrationClosureFindingKind.OriginalWriteReturned
                && finding.NeedsAttention);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Partial_failure_rolls_back_attempted_paths_in_reverse_order()
    {
        var fixture = await Fixture.CreateAsync();
        try
        {
            var adapter = FakePathAdapter.ForManifest(fixture.Evidence.Manifest);
            adapter.FailMovePath = fixture.Evidence.Manifest.Entries[1].OriginalPath;
            var handler = fixture.Handler(adapter, new FakeActivityProbe([]));

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(fixture.ConfirmedOperation());

            result.Success.Should().BeFalse();
            var execution = result.Payload.Should().BeOfType<MigrationExecutionResult>().Subject;
            execution.Status.Should().Be(MigrationExecutionStatus.FailedRolledBack);
            execution.RollbackAttempted.Should().BeTrue();
            execution.RollbackSucceeded.Should().BeTrue();
            adapter.RollbackOrder.Should().Equal(
                fixture.Evidence.Manifest.Entries[1].OriginalPath,
                fixture.Evidence.Manifest.Entries[0].OriginalPath);
            fixture.MonitorStore.Saved.Should().BeNull();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Rollback_failure_is_never_reported_as_recovered()
    {
        var fixture = await Fixture.CreateAsync();
        try
        {
            var adapter = FakePathAdapter.ForManifest(fixture.Evidence.Manifest);
            adapter.FailMovePath = fixture.Evidence.Manifest.Entries[1].OriginalPath;
            adapter.FailRollbackPath = fixture.Evidence.Manifest.Entries[0].OriginalPath;
            var handler = fixture.Handler(adapter, new FakeActivityProbe([]));

            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(fixture.ConfirmedOperation());

            result.Success.Should().BeFalse();
            var execution = result.Payload.Should().BeOfType<MigrationExecutionResult>().Subject;
            execution.Status.Should().Be(MigrationExecutionStatus.FailedRollbackIncomplete);
            execution.RollbackSucceeded.Should().BeFalse();
            execution.Summary.Should().Contain("incomplete");
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Stale_manifest_and_unsafe_policy_are_refused_before_move()
    {
        var stale = await Fixture.CreateAsync(createdAt: DateTimeOffset.UtcNow.AddHours(-2));
        try
        {
            var adapter = FakePathAdapter.ForManifest(stale.Evidence.Manifest);
            var staleResult = await new SafetyOperationPipeline(
                    stale.Handler(adapter, new FakeActivityProbe([])).ExecuteAsync)
                .ExecuteAsync(stale.ConfirmedOperation());
            staleResult.Success.Should().BeFalse();
            staleResult.Error.Should().Contain("stale");
            adapter.MoveOrder.Should().BeEmpty();
        }
        finally
        {
            stale.Dispose();
        }

        var unsafeFixture = await Fixture.CreateAsync();
        try
        {
            var adapter = FakePathAdapter.ForManifest(unsafeFixture.Evidence.Manifest);
            var handler = unsafeFixture.Handler(
                adapter,
                new FakeActivityProbe([]),
                new FakePathPolicy("Source path is outside the allowed migration roots."));
            var result = await new SafetyOperationPipeline(handler.ExecuteAsync)
                .ExecuteAsync(unsafeFixture.ConfirmedOperation());
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("allowed migration roots");
            adapter.MoveOrder.Should().BeEmpty();
        }
        finally
        {
            unsafeFixture.Dispose();
        }
    }

    [Fact]
    public void Windows_path_policy_allows_user_data_to_approved_d_root_and_blocks_protected_paths()
    {
        var policy = new WindowsMigrationPathPolicy();
        var manifest = PolicyManifest(@"D:\Agent\Example\Install");
        var allowed = PolicyEntry(
            @"C:\Users\Me\AppData\Local\Example",
            @"D:\Agent\Example\Install\MigratedData\Example");
        var protectedEntry = PolicyEntry(
            @"C:\Program Files\Example",
            @"D:\Agent\Example\Install");
        var outside = PolicyEntry(
            @"C:\Users\Me\AppData\Local\Example",
            @"D:\Other\Example");

        policy.Validate(manifest, allowed).Should().BeNull();
        policy.Validate(manifest, protectedEntry).Should().Contain("Protected");
        policy.Validate(PolicyManifest(@"D:\Other\Example"), outside)
            .Should().Contain("allowlist");
    }

    [Fact]
    public async Task Json_monitor_store_reloads_records_for_later_closure_scans()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "omnix-migration-monitor-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonMigrationMonitoringStore(root);
            var record = new MigrationMonitoringRecord
            {
                Id = "migration-monitor-fixture",
                SoftwareName = "Fixture",
                SnapshotId = "snapshot",
                RollbackManifestPath = @"D:\Evidence\fixture.json",
                RollbackManifestSha256 = new string('A', 64),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                Paths =
                [
                    new MigrationMonitoringPath
                    {
                        OriginalPath = @"C:\Users\Me\AppData\Local\Fixture",
                        ExpectedRedirectTarget = @"D:\Software\Fixture\MigratedData\Fixture"
                    }
                ]
            };

            await store.SaveAsync(record);
            var loaded = await new JsonMigrationMonitoringStore(root).LoadAsync();

            loaded.Should().ContainSingle();
            loaded[0].Id.Should().Be(record.Id);
            loaded[0].Paths[0].ExpectedRedirectTarget.Should()
                .Be(record.Paths[0].ExpectedRedirectTarget);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static MigrationRollbackManifest PolicyManifest(string destinationRoot) =>
        new()
        {
            Id = "policy",
            CreatedAt = DateTimeOffset.UtcNow,
            SoftwareName = "Policy Fixture",
            SnapshotId = "snapshot",
            DestinationRoot = destinationRoot,
            Entries = [],
            ServicesToRestore = [],
            StartupEntriesToRestore = [],
            ScheduledTasksToRestore = [],
            MonitorPaths = [],
            VerificationSteps = [],
            RollbackSteps = []
        };

    private static MigrationRollbackManifestEntry PolicyEntry(
        string original,
        string destination) =>
        new()
        {
            OriginalPath = original,
            PlannedDestinationPath = destination,
            RestorePath = original,
            Reason = "fixture"
        };

    private sealed class Fixture : IDisposable
    {
        private Fixture(
            string root,
            SoftwareProfile profile,
            MigrationPlan plan,
            MigrationRollbackManifestCreationResult evidence,
            MigrationSnapshotEvidenceCreationResult snapshotEvidence,
            string snapshotId,
            DateTimeOffset now)
        {
            Root = root;
            Profile = profile;
            Plan = plan;
            Evidence = evidence;
            SnapshotEvidence = snapshotEvidence;
            SnapshotId = snapshotId;
            Now = now;
        }

        public string Root { get; }
        public SoftwareProfile Profile { get; }
        public MigrationPlan Plan { get; }
        public MigrationRollbackManifestCreationResult Evidence { get; }
        public MigrationSnapshotEvidenceCreationResult SnapshotEvidence { get; }
        public string SnapshotId { get; }
        public DateTimeOffset Now { get; }
        public FakeMonitoringStore MonitorStore { get; } = new();

        public static async Task<Fixture> CreateAsync(DateTimeOffset? createdAt = null)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "omnix-migration-execution-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var install = Path.Combine(root, "source", "Install");
            var cache = Path.Combine(root, "source", "Cache");
            Directory.CreateDirectory(install);
            Directory.CreateDirectory(cache);
            var destination = Path.Combine(root, "destination", "Fixture");
            var profile = new SoftwareProfile
            {
                Name = "Migration Fixture",
                Category = SoftwareCategory.Normal,
                InstallPath = install,
                CachePaths = [cache],
                CDriveWritePaths = [install, cache],
                RunningProcesses = ["fixture-process"]
            };
            var plan = MigrationPlanner.CreatePlan(profile, destination, snapshotAvailable: true);
            var snapshotId = "migration-fixture-snapshot";
            var now = createdAt ?? DateTimeOffset.UtcNow;
            var manifestPath = Path.Combine(root, "evidence", "migration.rollback.json");
            var evidence = await MigrationRollbackManifestCreationService.CreateAsync(
                profile,
                plan,
                manifestPath,
                snapshotId,
                now);
            var snapshotEvidence = await MigrationSnapshotEvidenceService.CreateAsync(
                evidence.Manifest,
                evidence.ManifestPath,
                evidence.Sha256,
                Path.Combine(root, "evidence", "migration.snapshot.json"),
                new FixtureSnapshotSourceReader(now),
                now);
            return new Fixture(
                root,
                profile,
                plan,
                evidence,
                snapshotEvidence,
                snapshotId,
                now);
        }

        public MigrationOperationHandler Handler(
            IMigrationPathAdapter paths,
            IMigrationActivityProbe activity,
            IMigrationPathPolicy? policy = null,
            IMigrationSnapshotSourceReader? snapshotSourceReader = null) =>
            new(
                activity,
                paths,
                policy ?? new FakePathPolicy(),
                snapshotSourceReader ?? new FixtureSnapshotSourceReader(Now),
                MonitorStore,
                () => DateTimeOffset.UtcNow);

        public OperationDescriptor ConfirmedOperation()
        {
            var readiness = new MigrationExecutionReadiness
            {
                FeatureEnabled = true,
                SnapshotId = SnapshotId,
                UserConfirmedPlan = true,
                UserConfirmedAppsClosed = true,
                RollbackManifestPath = Evidence.ManifestPath,
                RollbackManifestSha256 = Evidence.Sha256,
                SnapshotEvidencePath = SnapshotEvidence.EvidencePath,
                SnapshotEvidenceSha256 = SnapshotEvidence.Sha256,
                DestinationAvailableBytes = long.MaxValue,
                UserConfirmedPostMigrationMonitoring = true
            };
            var gate = MigrationExecutionGate.Evaluate(
                Profile,
                Plan,
                readiness,
                File.Exists);
            gate.CanRequestExecution.Should().BeTrue();
            var source = gate.Operation!;
            return new OperationDescriptor
            {
                Kind = source.Kind,
                Title = source.Title,
                Source = source.Source,
                Risk = source.Risk,
                IsDestructive = source.IsDestructive,
                RequiresElevation = source.RequiresElevation,
                RequiresSnapshot = source.RequiresSnapshot,
                SnapshotId = source.SnapshotId,
                RollbackRequired = source.RollbackRequired,
                ConfirmationAccepted = true,
                EvidenceSummary = source.EvidenceSummary,
                EstimatedImpactBytes = source.EstimatedImpactBytes,
                ConfirmationText = source.ConfirmationText,
                AffectedPaths = source.AffectedPaths,
                AffectedRegistryKeys = source.AffectedRegistryKeys,
                AffectedServices = source.AffectedServices,
                Arguments = source.Arguments
            };
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class FakeActivityProbe(IReadOnlyList<string> active)
        : IMigrationActivityProbe
    {
        public Task<IReadOnlyList<string>> FindActiveAsync(
            MigrationActivityRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(active);
        }
    }

    private sealed class FixtureSnapshotSourceReader(DateTimeOffset? observedAt = null)
        : IMigrationSnapshotSourceReader
    {
        public Task<MigrationSnapshotSourceEvidence> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new MigrationSnapshotSourceEvidence
            {
                Path = path,
                Exists = Directory.Exists(path),
                IsDirectory = Directory.Exists(path),
                IsRedirect = false,
                ObservedBytes = 0,
                LastWriteUtc = observedAt ?? DateTimeOffset.UtcNow
            });
        }
    }

    private sealed class ChangedSnapshotSourceReader(
        MigrationSnapshotEvidenceCreationResult creation)
        : IMigrationSnapshotSourceReader
    {
        public Task<MigrationSnapshotSourceEvidence> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var original = creation.Evidence.Sources.Single(source =>
                string.Equals(
                    Path.GetFullPath(source.Path),
                    Path.GetFullPath(path),
                    StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(original with
            {
                ObservedBytes = original.ObservedBytes + 1
            });
        }
    }

    private sealed class FakePathPolicy(string? error = null) : IMigrationPathPolicy
    {
        public string? Validate(
            MigrationRollbackManifest manifest,
            MigrationRollbackManifestEntry entry) => error;
    }

    private sealed class FakeMonitoringStore : IMigrationMonitoringStore
    {
        public MigrationMonitoringRecord? Saved { get; private set; }

        public Task SaveAsync(
            MigrationMonitoringRecord record,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Saved = record;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<MigrationMonitoringRecord>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<MigrationMonitoringRecord>>(
                Saved is null ? [] : [Saved]);
        }
    }

    private sealed class FakePathAdapter : IMigrationPathAdapter
    {
        private readonly Dictionary<string, MigrationPathObservation> _observations =
            new(StringComparer.OrdinalIgnoreCase);

        public string? FailMovePath { get; set; }
        public string? FailRollbackPath { get; set; }
        public List<string> MoveOrder { get; } = [];
        public List<string> RollbackOrder { get; } = [];

        public static FakePathAdapter ForManifest(MigrationRollbackManifest manifest)
        {
            var adapter = new FakePathAdapter();
            foreach (var entry in manifest.Entries)
            {
                adapter.Set(new MigrationPathObservation
                {
                    Path = entry.OriginalPath,
                    Exists = true,
                    IsDirectory = true
                });
                adapter.Set(new MigrationPathObservation
                {
                    Path = entry.PlannedDestinationPath,
                    Exists = false,
                    IsDirectory = true
                });
            }
            return adapter;
        }

        public Task<MigrationPathObservation> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_observations.TryGetValue(Key(path), out var value)
                ? value
                : new MigrationPathObservation
                {
                    Path = path,
                    Exists = false,
                    IsDirectory = true
                });
        }

        public Task<MigrationMoveResult> MoveAndRedirectAsync(
            MigrationRollbackManifestEntry entry,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MoveOrder.Add(entry.OriginalPath);
            if (Same(FailMovePath, entry.OriginalPath))
                throw new IOException("Injected move failure.");
            Set(new MigrationPathObservation
            {
                Path = entry.OriginalPath,
                Exists = true,
                IsDirectory = true,
                IsRedirect = true,
                RedirectTarget = entry.PlannedDestinationPath
            });
            Set(new MigrationPathObservation
            {
                Path = entry.PlannedDestinationPath,
                Exists = true,
                IsDirectory = true
            });
            return Task.FromResult(new MigrationMoveResult
            {
                OriginalPath = entry.OriginalPath,
                DestinationPath = entry.PlannedDestinationPath,
                RedirectCreated = true
            });
        }

        public Task RollbackAsync(
            MigrationRollbackManifestEntry entry,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RollbackOrder.Add(entry.OriginalPath);
            if (Same(FailRollbackPath, entry.OriginalPath))
                throw new IOException("Injected rollback failure.");
            Set(new MigrationPathObservation
            {
                Path = entry.OriginalPath,
                Exists = true,
                IsDirectory = true
            });
            Set(new MigrationPathObservation
            {
                Path = entry.PlannedDestinationPath,
                Exists = false,
                IsDirectory = true
            });
            return Task.CompletedTask;
        }

        public void ReplaceRedirectWithRealDirectory(string path) =>
            Set(new MigrationPathObservation
            {
                Path = path,
                Exists = true,
                IsDirectory = true,
                IsRedirect = false,
                ObservedBytes = 1024,
                LastWriteUtc = DateTimeOffset.UtcNow
            });

        private void Set(MigrationPathObservation value) =>
            _observations[Key(value.Path)] = value;

        private static string Key(string path) => Path.GetFullPath(path);

        private static bool Same(string? left, string right) =>
            !string.IsNullOrWhiteSpace(left)
            && string.Equals(Key(left), Key(right), StringComparison.OrdinalIgnoreCase);
    }
}
