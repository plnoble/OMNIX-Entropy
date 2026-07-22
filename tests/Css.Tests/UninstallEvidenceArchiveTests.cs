using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Software;
using Css.Core.Timeline;
using Css.Snapshot.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class UninstallEvidenceArchiveTests
{
    [Fact]
    public async Task Confirmed_archive_operation_moves_manifest_through_pipeline_and_can_restore()
    {
        var root = CreateTempRoot();
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        try
        {
            var snapshotRoot = Path.Combine(root, "snapshots");
            var store = new UninstallEvidenceSnapshotStore(snapshotRoot, () => now.AddDays(-10));
            var profile = new SoftwareProfile { Name = "Example App" };
            var evidence = await store.CreateAsync(profile, Recovery());
            var plan = await new UninstallEvidenceRetentionPlanner(snapshotRoot).PlanAsync(
                new UninstallEvidenceRetentionPolicy(TimeSpan.FromDays(1), 10),
                now);
            var candidate = plan.Candidates.Should().ContainSingle().Subject;
            candidate.Sha256.Should().Be(evidence.Sha256);
            var quarantine = new FileQuarantineService(Path.Combine(root, "archive"));
            var timeline = new ActionTimelineStore(Path.Combine(root, "timeline.db"));
            var policy = new UninstallEvidenceArchiveOperationPolicy(snapshotRoot);
            var handler = new UninstallEvidenceArchiveOperationHandler(
                snapshotRoot,
                quarantine,
                timeline);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            var preview = policy.CreatePreview(plan, [candidate.SnapshotId]);

            var blocked = await pipeline.ExecuteAsync(preview);

            blocked.Success.Should().BeFalse();
            File.Exists(evidence.ManifestPath).Should().BeTrue();

            var confirmed = policy.ConfirmForExecution(preview);
            var result = await pipeline.ExecuteAsync(confirmed);
            result.Success.Should().BeTrue(result.Error);
            var records = result.Payload.Should().BeAssignableTo<IReadOnlyList<QuarantineRecord>>().Subject;
            var entries = await timeline.LoadRecentAsync(5);

            File.Exists(evidence.ManifestPath).Should().BeFalse();
            records.Should().ContainSingle();
            entries.Should().ContainSingle();
            entries[0].RestoreState.Should().Be(RestoreState.Restorable);
            entries[0].RestoreOperationKind.Should().Be("quarantine.restore");

            var restored = await quarantine.RestoreAsync(records[0].ManifestPath);

            restored.Success.Should().BeTrue();
            File.Exists(evidence.ManifestPath).Should().BeTrue();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Archive_handler_refuses_a_manifest_changed_after_planning()
    {
        var root = CreateTempRoot();
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        try
        {
            var fixture = await CreateCandidateAsync(root, "one", now);
            var timeline = new ActionTimelineStore(Path.Combine(root, "timeline.db"));
            var policy = new UninstallEvidenceArchiveOperationPolicy(fixture.SnapshotRoot);
            var preview = policy.CreatePreview(fixture.Plan, [fixture.Candidate.SnapshotId]);
            await File.AppendAllTextAsync(fixture.Candidate.ManifestPath, "tampered");
            var pipeline = new SafetyOperationPipeline(
                new UninstallEvidenceArchiveOperationHandler(
                    fixture.SnapshotRoot,
                    new FileQuarantineService(Path.Combine(root, "archive")),
                    timeline).ExecuteAsync);

            var result = await pipeline.ExecuteAsync(policy.ConfirmForExecution(preview));
            var entries = await timeline.LoadRecentAsync(5);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("\u54c8\u5e0c");
            File.Exists(fixture.Candidate.ManifestPath).Should().BeTrue();
            entries.Should().BeEmpty();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Archive_handler_validates_entire_batch_before_moving_any_manifest()
    {
        var root = CreateTempRoot();
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        try
        {
            var snapshotRoot = Path.Combine(root, "snapshots");
            var current = now.AddDays(-10);
            var store = new UninstallEvidenceSnapshotStore(snapshotRoot, () => current);
            await store.CreateAsync(new SoftwareProfile { Name = "App One" }, Recovery());
            current = now.AddDays(-9);
            await store.CreateAsync(new SoftwareProfile { Name = "App Two" }, Recovery());
            var plan = await new UninstallEvidenceRetentionPlanner(snapshotRoot).PlanAsync(
                new UninstallEvidenceRetentionPolicy(TimeSpan.FromDays(1), 10),
                now);
            plan.Candidates.Should().HaveCount(2);
            var changed = plan.Candidates[1];
            await File.AppendAllTextAsync(changed.ManifestPath, "tampered");
            var policy = new UninstallEvidenceArchiveOperationPolicy(snapshotRoot);
            var preview = policy.CreatePreview(plan, plan.Candidates.Select(item => item.SnapshotId).ToList());
            var pipeline = new SafetyOperationPipeline(
                new UninstallEvidenceArchiveOperationHandler(
                    snapshotRoot,
                    new FileQuarantineService(Path.Combine(root, "archive")),
                    new ActionTimelineStore(Path.Combine(root, "timeline.db"))).ExecuteAsync);

            var result = await pipeline.ExecuteAsync(policy.ConfirmForExecution(preview));

            result.Success.Should().BeFalse();
            plan.Candidates.Should().OnlyContain(item => File.Exists(item.ManifestPath));
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Archive_handler_rolls_back_prior_moves_when_a_later_move_fails()
    {
        var root = CreateTempRoot();
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        try
        {
            var snapshotRoot = Path.Combine(root, "snapshots");
            var current = now.AddDays(-10);
            var store = new UninstallEvidenceSnapshotStore(snapshotRoot, () => current);
            await store.CreateAsync(new SoftwareProfile { Name = "App One" }, Recovery());
            current = now.AddDays(-9);
            await store.CreateAsync(new SoftwareProfile { Name = "App Two" }, Recovery());
            var plan = await new UninstallEvidenceRetentionPlanner(snapshotRoot).PlanAsync(
                new UninstallEvidenceRetentionPolicy(TimeSpan.FromDays(1), 10),
                now);
            var policy = new UninstallEvidenceArchiveOperationPolicy(snapshotRoot);
            var preview = policy.CreatePreview(plan, plan.Candidates.Select(item => item.SnapshotId).ToList());
            var fakeArchiveRoot = Path.Combine(root, "fake-archive");
            var movedByManifest = new Dictionary<string, QuarantineRecord>(StringComparer.OrdinalIgnoreCase);
            var moveCount = 0;

            async Task<QuarantineRecord> MoveAsync(string source, string reason, CancellationToken _)
            {
                moveCount++;
                if (moveCount == 2)
                    throw new IOException("simulated second move failure");

                Directory.CreateDirectory(fakeArchiveRoot);
                var quarantined = Path.Combine(fakeArchiveRoot, Path.GetFileName(source));
                File.Move(source, quarantined);
                var manifestPath = quarantined + ".restore.json";
                await File.WriteAllTextAsync(manifestPath, "restore marker");
                var record = new QuarantineRecord
                {
                    Id = Guid.NewGuid().ToString("N"),
                    OriginalPath = source,
                    QuarantinedPath = quarantined,
                    ManifestPath = manifestPath,
                    Reason = reason,
                    SizeBytes = new FileInfo(quarantined).Length
                };
                movedByManifest[manifestPath] = record;
                return record;
            }

            Task<QuarantineRestoreResult> RestoreAsync(string manifestPath, CancellationToken _)
            {
                var record = movedByManifest[manifestPath];
                File.Move(record.QuarantinedPath, record.OriginalPath);
                return Task.FromResult(new QuarantineRestoreResult
                {
                    Success = true,
                    RestoreState = RestoreState.Restored,
                    Summary = "restored",
                    Record = record
                });
            }

            var timeline = new ActionTimelineStore(Path.Combine(root, "timeline.db"));
            var handler = new UninstallEvidenceArchiveOperationHandler(
                snapshotRoot,
                MoveAsync,
                RestoreAsync,
                timeline);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(policy.ConfirmForExecution(preview));
            var entries = await timeline.LoadRecentAsync(5);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("simulated second move failure");
            plan.Candidates.Should().OnlyContain(item => File.Exists(item.ManifestPath));
            entries.Should().BeEmpty();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Archive_policy_rejects_candidate_outside_snapshot_root()
    {
        var root = CreateTempRoot();
        try
        {
            var snapshotRoot = Path.Combine(root, "snapshots");
            var outside = Path.Combine(root, "outside", "uninstall-outside.json");
            var item = new UninstallEvidenceRetentionItem
            {
                SnapshotId = "uninstall-outside",
                ManifestPath = outside,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-40),
                Reason = "expired",
                Sha256 = "A1B2"
            };
            var plan = new UninstallEvidenceRetentionPlan
            {
                Keep = [],
                Candidates = [item],
                PreservedUnknown = [],
                CanApplyDirectly = false
            };
            var policy = new UninstallEvidenceArchiveOperationPolicy(snapshotRoot);

            var action = () => policy.CreatePreview(plan, [item.SnapshotId]);

            action.Should().Throw<InvalidOperationException>();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Archive_handler_source_has_no_permanent_delete_path()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Snapshot", "Uninstall", "UninstallEvidenceArchiveOperationHandler.cs"));

        source.Should().Contain("RestoreAsync");
        source.Should().Contain("ActionTimelineStore");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("Directory.Delete");
    }

    private static async Task<(string SnapshotRoot, UninstallEvidenceRetentionPlan Plan, UninstallEvidenceRetentionItem Candidate)> CreateCandidateAsync(
        string root,
        string suffix,
        DateTimeOffset now)
    {
        var snapshotRoot = Path.Combine(root, "snapshots-" + suffix);
        var store = new UninstallEvidenceSnapshotStore(snapshotRoot, () => now.AddDays(-10));
        await store.CreateAsync(new SoftwareProfile { Name = "Example " + suffix }, Recovery());
        var plan = await new UninstallEvidenceRetentionPlanner(snapshotRoot).PlanAsync(
            new UninstallEvidenceRetentionPolicy(TimeSpan.FromDays(1), 10),
            now);
        return (snapshotRoot, plan, plan.Candidates.Single());
    }

    private static OfficialUninstallRecoveryEvidence Recovery() =>
        new()
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = "verified-installer",
            CanRecoverApplication = true
        };

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-uninstall-archive-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
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

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }
}
