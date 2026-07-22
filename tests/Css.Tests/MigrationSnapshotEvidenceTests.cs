using Css.Core.Migration;
using Css.Core.Operations;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationSnapshotEvidenceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 13, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Evidence_round_trips_and_validates_against_manifest_and_operation()
    {
        using var fixture = await Fixture.CreateAsync();

        fixture.Snapshot.Sha256.Should().MatchRegex("^[0-9A-F]{64}$");
        var verified = await MigrationSnapshotEvidenceStore.ReadVerifiedAsync(
            fixture.Snapshot.EvidencePath,
            fixture.Snapshot.Sha256);

        verified.IsValid.Should().BeTrue();
        verified.Evidence!.Sources.Should().ContainSingle();
        MigrationSnapshotEvidenceService.ValidateForOperation(
                verified.Evidence,
                fixture.Manifest,
                fixture.Operation(),
                Now)
            .Should().BeNull();
        Directory.GetFiles(fixture.Root, "*.tmp-*", SearchOption.AllDirectories)
            .Should().BeEmpty();
    }

    [Fact]
    public async Task Hash_tamper_and_unknown_json_fields_are_rejected()
    {
        using var fixture = await Fixture.CreateAsync();
        await File.AppendAllTextAsync(fixture.Snapshot.EvidencePath, " ");

        var tampered = await MigrationSnapshotEvidenceStore.ReadVerifiedAsync(
            fixture.Snapshot.EvidencePath,
            fixture.Snapshot.Sha256);

        tampered.IsValid.Should().BeFalse();
        tampered.Error.Should().Contain("hash");

        var strictPath = Path.Combine(fixture.Root, "snapshot-with-unknown-field.json");
        var originalJson = await File.ReadAllTextAsync(fixture.OriginalSnapshotPath);
        var objectStart = originalJson.IndexOf('{');
        objectStart.Should().BeGreaterThanOrEqualTo(0);
        var withUnknownField = originalJson.Insert(
            objectStart + 1,
            "\n  \"UnexpectedField\": true,");
        await File.WriteAllTextAsync(strictPath, withUnknownField);
        var strictHash = await MigrationSnapshotEvidenceStore.ComputeSha256Async(strictPath);

        var strict = await MigrationSnapshotEvidenceStore.ReadVerifiedAsync(
            strictPath,
            strictHash);

        strict.IsValid.Should().BeFalse();
        strict.Error.Should().Contain("could not be verified");
    }

    [Fact]
    public async Task Validation_rejects_stale_identity_and_manifest_binding_mismatches()
    {
        using var fixture = await Fixture.CreateAsync();
        var operation = fixture.Operation();

        MigrationSnapshotEvidenceService.ValidateForOperation(
                fixture.Snapshot.Evidence with { CreatedAtUtc = Now.AddMinutes(-31) },
                fixture.Manifest,
                operation,
                Now)
            .Should().Contain("stale");
        MigrationSnapshotEvidenceService.ValidateForOperation(
                fixture.Snapshot.Evidence with { SnapshotId = "different-snapshot" },
                fixture.Manifest,
                operation,
                Now)
            .Should().Contain("identity");
        MigrationSnapshotEvidenceService.ValidateForOperation(
                fixture.Snapshot.Evidence with
                {
                    RollbackManifestSha256 = new string('F', 64)
                },
                fixture.Manifest,
                operation,
                Now)
            .Should().Contain("verified rollback manifest");
    }

    [Fact]
    public async Task Creation_rejects_missing_redirected_and_wrong_path_sources()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "omnix-migration-snapshot-reject-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var manifest = Fixture.CreateManifest(root);
            var manifestPath = Path.Combine(root, "rollback.json");
            await MigrationRollbackManifestStore.WriteAsync(manifest, manifestPath);
            var manifestHash = await MigrationRollbackManifestStore.ComputeSha256Async(manifestPath);

            foreach (var unsafeObservation in new[]
                     {
                         new MigrationSnapshotSourceEvidence
                         {
                             Path = manifest.Entries[0].OriginalPath,
                             Exists = false,
                             IsDirectory = false
                         },
                         new MigrationSnapshotSourceEvidence
                         {
                             Path = manifest.Entries[0].OriginalPath,
                             Exists = true,
                             IsDirectory = true,
                             IsRedirect = true
                         },
                         new MigrationSnapshotSourceEvidence
                         {
                             Path = Path.Combine(root, "different-source"),
                             Exists = true,
                             IsDirectory = true
                         }
                     })
            {
                Func<Task> action = () => MigrationSnapshotEvidenceService.CreateAsync(
                    manifest,
                    manifestPath,
                    manifestHash,
                    Path.Combine(root, Guid.NewGuid().ToString("N") + ".json"),
                    new StaticSourceReader(unsafeObservation),
                    Now);

                await action.Should().ThrowAsync<InvalidOperationException>();
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private sealed class Fixture : IDisposable
    {
        private Fixture(
            string root,
            string originalSnapshotPath,
            MigrationRollbackManifest manifest,
            MigrationRollbackManifestCreationResult rollback,
            MigrationSnapshotEvidenceCreationResult snapshot)
        {
            Root = root;
            OriginalSnapshotPath = originalSnapshotPath;
            Manifest = manifest;
            Rollback = rollback;
            Snapshot = snapshot;
        }

        public string Root { get; }
        public string OriginalSnapshotPath { get; }
        public MigrationRollbackManifest Manifest { get; }
        public MigrationRollbackManifestCreationResult Rollback { get; }
        public MigrationSnapshotEvidenceCreationResult Snapshot { get; }

        public static async Task<Fixture> CreateAsync()
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "omnix-migration-snapshot-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var manifest = CreateManifest(root);
            var manifestPath = Path.Combine(root, "rollback.json");
            await MigrationRollbackManifestStore.WriteAsync(manifest, manifestPath);
            var manifestHash = await MigrationRollbackManifestStore.ComputeSha256Async(manifestPath);
            var rollback = new MigrationRollbackManifestCreationResult
            {
                ManifestPath = manifestPath,
                Sha256 = manifestHash,
                Manifest = manifest
            };
            var observation = new MigrationSnapshotSourceEvidence
            {
                Path = manifest.Entries[0].OriginalPath,
                Exists = true,
                IsDirectory = true,
                IsRedirect = false,
                ObservedBytes = 42,
                LastWriteUtc = Now.AddSeconds(-5)
            };
            var snapshotPath = Path.Combine(root, "snapshot.json");
            var snapshot = await MigrationSnapshotEvidenceService.CreateAsync(
                manifest,
                manifestPath,
                manifestHash,
                snapshotPath,
                new StaticSourceReader(observation),
                Now);
            var originalSnapshotPath = Path.Combine(root, "snapshot-original.json");
            File.Copy(snapshotPath, originalSnapshotPath);
            return new Fixture(root, originalSnapshotPath, manifest, rollback, snapshot);
        }

        public static MigrationRollbackManifest CreateManifest(string root)
        {
            var source = Path.Combine(root, "source");
            var destinationRoot = Path.Combine(root, "destination");
            return new MigrationRollbackManifest
            {
                Id = "snapshot-manifest",
                CreatedAt = Now,
                SoftwareName = "Snapshot Fixture",
                SnapshotId = "snapshot-fixture",
                DestinationRoot = destinationRoot,
                IsPlanOnly = true,
                Entries =
                [
                    new MigrationRollbackManifestEntry
                    {
                        OriginalPath = source,
                        PlannedDestinationPath = Path.Combine(destinationRoot, "source"),
                        RestorePath = source,
                        Reason = "fixture"
                    }
                ],
                ServicesToRestore = [],
                StartupEntriesToRestore = [],
                ScheduledTasksToRestore = [],
                MonitorPaths = [source],
                VerificationSteps = ["verify"],
                RollbackSteps = ["restore"]
            };
        }

        public OperationDescriptor Operation() =>
            new()
            {
                Kind = "migration.execute",
                Title = "Snapshot fixture migration",
                Source = OperationSource.Manual,
                Risk = RiskLevel.High,
                IsDestructive = true,
                RequiresElevation = true,
                RequiresSnapshot = true,
                SnapshotId = Manifest.SnapshotId,
                RollbackRequired = true,
                ConfirmationAccepted = true,
                AffectedPaths = Manifest.Entries.Select(entry => entry.OriginalPath).ToArray(),
                Arguments = new Dictionary<string, object?>
                {
                    ["destinationRoot"] = Manifest.DestinationRoot,
                    ["rollbackManifestPath"] = Rollback.ManifestPath,
                    ["rollbackManifestSha256"] = Rollback.Sha256,
                    ["snapshotEvidencePath"] = Snapshot.EvidencePath,
                    ["snapshotEvidenceSha256"] = Snapshot.Sha256
                }
            };

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class StaticSourceReader(MigrationSnapshotSourceEvidence observation)
        : IMigrationSnapshotSourceReader
    {
        public Task<MigrationSnapshotSourceEvidence> ObserveAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(observation);
        }
    }
}
