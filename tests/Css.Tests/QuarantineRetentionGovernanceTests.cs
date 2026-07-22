using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Timeline;
using FluentAssertions;

namespace Css.Tests;

public sealed class QuarantineRetentionGovernanceTests
{
    [Fact]
    public void Retention_plan_saturates_bytes_bounds_candidates_and_rejects_invalid_options()
    {
        var now = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);
        var records = new[]
        {
            Record("one", long.MaxValue, now.AddDays(-90)),
            Record("two", long.MaxValue, now.AddDays(-80)),
            Record("three", -50, now.AddDays(-70))
        };

        var plan = QuarantineRetentionPlanner.Build(
            records,
            new QuarantineRetentionOptions
            {
                Now = now,
                RetentionDays = 30,
                MaxTotalBytes = 1,
                MaximumCandidates = 2
            });

        plan.TotalBytes.Should().Be(long.MaxValue);
        plan.ActiveBytes.Should().Be(long.MaxValue);
        plan.ReclaimableBytes.Should().Be(long.MaxValue);
        plan.ProjectedActiveBytes.Should().Be(0);
        plan.Candidates.Should().HaveCount(2);
        plan.WasTruncated.Should().BeTrue();
        plan.WouldDeleteAutomatically.Should().BeFalse();
        FluentActions.Invoking(() => QuarantineRetentionPlanner.Build(
                [],
                new QuarantineRetentionOptions { RetentionDays = 0 }))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => QuarantineRetentionPlanner.Build(
                [],
                new QuarantineRetentionOptions { MaxTotalBytes = 0 }))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Retention_presentation_is_path_free_and_non_executable()
    {
        var record = Record(
            "old",
            1024,
            new DateTimeOffset(2026, 5, 1, 8, 0, 0, TimeSpan.Zero));
        var plan = QuarantineRetentionPlanner.Build(
            [record],
            new QuarantineRetentionOptions
            {
                Now = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.Zero),
                RetentionDays = 30,
                MaxTotalBytes = 20L * 1024 * 1024 * 1024
            });

        var view = QuarantineRetentionPresenter.Create(plan);
        var visible = string.Join(
            "\n",
            new[] { view.Headline, view.UsageText, view.ImpactText, view.SafetyText }
                .Concat(view.Candidates.SelectMany(item =>
                    new[] { item.Title, item.Summary, item.SafetyText })));

        visible.Should().Contain("不会自动删除").And.Contain("不能");
        visible.Should().NotContain(@"C:\").And.NotContain(@"D:\");
        view.CanExecuteDirectly.Should().BeFalse();
        view.Candidates.Should().OnlyContain(item => !item.CanExecuteDirectly);
    }

    [Fact]
    public async Task Manifest_inspection_refuses_record_that_points_outside_its_item_root()
    {
        var root = CreateTempRoot();
        try
        {
            var quarantineRoot = Path.Combine(root, "quarantine");
            var id = "forged-record";
            var itemRoot = Path.Combine(quarantineRoot, "20260714", id);
            Directory.CreateDirectory(itemRoot);
            var manifest = Path.Combine(itemRoot, "manifest.json");
            var outsidePayload = Path.Combine(root, "outside.tmp");
            await File.WriteAllTextAsync(outsidePayload, "must stay");
            var forged = new QuarantineRecord
            {
                Id = id,
                MovedAt = DateTimeOffset.Now.AddDays(-40),
                OriginalPath = Path.Combine(root, "original.tmp"),
                QuarantinedPath = outsidePayload,
                ManifestPath = manifest,
                Reason = "forged fixture",
                SizeBytes = 9,
                RestoreState = RestoreState.Restorable
            };
            await File.WriteAllTextAsync(manifest, JsonSerializer.Serialize(forged));
            var service = new FileQuarantineService(quarantineRoot);

            var inspection = await service.InspectManifestAsync(manifest);
            var restore = await service.RestoreAsync(manifest);

            inspection.Success.Should().BeFalse();
            restore.Success.Should().BeFalse();
            File.Exists(outsidePayload).Should().BeTrue();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Confirmed_manual_purge_uses_pipeline_and_records_not_restorable_timeline()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var quarantine = new FileQuarantineService(Path.Combine(root, "quarantine"));
            var timeline = new ActionTimelineStore(Path.Combine(root, "timeline.db"));
            var record = await quarantine.QuarantineAsync(source, "test cleanup");
            var plan = QuarantineRetentionPlanner.Build(
                [record],
                new QuarantineRetentionOptions
                {
                    Now = record.MovedAt.AddDays(40),
                    RetentionDays = 30,
                    MaxTotalBytes = 20L * 1024 * 1024 * 1024
                });
            var unconfirmed = QuarantinePurgeOperationPolicy.CreatePlan(plan);
            var handler = new QuarantinePurgeOperationHandler(quarantine, timeline);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var blocked = await pipeline.ExecuteAsync(unconfirmed);
            var confirmed = QuarantinePurgeOperationPolicy.ConfirmForExecution(unconfirmed);
            var result = await pipeline.ExecuteAsync(confirmed);
            var entries = await timeline.LoadRecentAsync(5);

            blocked.Success.Should().BeFalse();
            result.Success.Should().BeTrue();
            File.Exists(record.QuarantinedPath).Should().BeFalse();
            File.Exists(record.ManifestPath).Should().BeFalse();
            File.Exists(record.OriginalPath).Should().BeFalse();
            entries.Should().ContainSingle();
            entries[0].RestoreState.Should().Be(RestoreState.NotRestorable);
            entries[0].RestoreOperationKind.Should().BeNull();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Purge_policy_refuses_agent_source_and_fake_rollback_claims()
    {
        var baseDescriptor = new OperationDescriptor
        {
            Kind = QuarantinePurgeOperationPolicy.OperationKind,
            Title = "永久整理隔离区",
            Source = OperationSource.Agent,
            Risk = RiskLevel.Medium,
            IsDestructive = true,
            EvidenceSummary = "one item",
            ConfirmationText = "确认永久整理？整理后不能还原。",
            AffectedPaths = [@"D:\OMNIX-Entropy\Quarantine\20260714\id\manifest.json"]
        };
        var fakeRollback = new OperationDescriptor
        {
            Kind = baseDescriptor.Kind,
            Title = baseDescriptor.Title,
            Source = OperationSource.Manual,
            Risk = baseDescriptor.Risk,
            IsDestructive = true,
            RollbackRequired = true,
            EvidenceSummary = baseDescriptor.EvidenceSummary,
            ConfirmationText = baseDescriptor.ConfirmationText,
            AffectedPaths = baseDescriptor.AffectedPaths
        };

        QuarantinePurgeOperationPolicy.ValidateCandidate(baseDescriptor).Success.Should().BeFalse();
        QuarantinePurgeOperationPolicy.ValidateCandidate(fakeRollback).Success.Should().BeFalse();
    }

    [Fact]
    public void Wpf_requires_visible_acknowledgement_and_never_runs_purge_on_load()
    {
        var mainXaml = Read("src", "Css.App", "MainWindow.xaml");
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var confirmXaml = Read("src", "Css.App", "QuarantinePurgeConfirmationWindow.xaml");
        var confirmCode = Read("src", "Css.App", "QuarantinePurgeConfirmationWindow.xaml.cs");

        mainXaml.Should().Contain("AutomationProperties.AutomationId=\"ReviewQuarantineCleanupButton\"");
        mainXaml.Should().Contain("Click=\"ReviewQuarantineCleanup_Click\"");
        confirmXaml.Should().Contain("AutomationProperties.AutomationId=\"QuarantinePurgeWarningTextBlock\"");
        confirmXaml.Should().Contain("AutomationProperties.AutomationId=\"QuarantinePurgeAcknowledgementCheckBox\"");
        confirmXaml.Should().Contain("AutomationProperties.AutomationId=\"QuarantinePurgeConfirmButton\"");
        confirmXaml.Should().Contain("IsEnabled=\"False\"");
        confirmXaml.IndexOf("QuarantinePurgeWarningTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(confirmXaml.IndexOf("QuarantinePurgeAcknowledgementCheckBox", StringComparison.Ordinal));
        confirmCode.Should().Contain("QuarantinePurgeAcknowledgementCheckBox.IsChecked == true");
        confirmCode.Should().Contain("QuarantinePurgeAcknowledgementCheckBox.IsChecked != true");
        main.Should().Contain("new SafetyOperationPipeline(handler.ExecuteAsync)");
        main.Should().Contain("QuarantinePurgeOperationPolicy.ConfirmForExecution(descriptor)");

        var loadStart = main.IndexOf("private async Task LoadTimelineAsync()", StringComparison.Ordinal);
        var refreshStart = main.IndexOf("private async Task RefreshQuarantinePolicyAsync()", StringComparison.Ordinal);
        var loadMethod = main[loadStart..refreshStart];
        loadMethod.Should().NotContain("PurgeAsync").And.NotContain("pipeline.ExecuteAsync");
    }

    private static QuarantineRecord Record(string id, long bytes, DateTimeOffset movedAt) =>
        new()
        {
            Id = id,
            MovedAt = movedAt,
            OriginalPath = $@"C:\Temp\{id}.tmp",
            QuarantinedPath = $@"D:\OMNIX-Entropy\Quarantine\20260714\{id}\{id}.tmp",
            ManifestPath = $@"D:\OMNIX-Entropy\Quarantine\20260714\{id}\manifest.json",
            Reason = "test",
            SizeBytes = bytes,
            RestoreState = RestoreState.Restorable
        };

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-quarantine-governance-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
