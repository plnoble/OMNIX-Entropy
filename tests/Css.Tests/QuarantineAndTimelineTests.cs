using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Timeline;
using Css.Win32.Quarantine;
using FluentAssertions;

namespace Css.Tests;

public class QuarantineAndTimelineTests
{
    [Fact]
    public async Task Quarantine_moves_file_to_manifested_location_and_restores_it()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var service = new FileQuarantineService(Path.Combine(root, "quarantine"));

            var record = await service.QuarantineAsync(source, "临时文件清理");

            File.Exists(source).Should().BeFalse();
            File.Exists(record.QuarantinedPath).Should().BeTrue();
            File.Exists(record.ManifestPath).Should().BeTrue();
            record.OriginalPath.Should().Be(source);
            record.SizeBytes.Should().BeGreaterThan(0);

            var restore = await service.RestoreAsync(record.ManifestPath);

            restore.RestoreState.Should().Be(RestoreState.Restored);
            File.Exists(source).Should().BeTrue();
            File.Exists(record.QuarantinedPath).Should().BeFalse();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Quarantine_service_loads_manifest_records_without_restoring_or_deleting()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var service = new FileQuarantineService(Path.Combine(root, "quarantine"));
            var moved = await service.QuarantineAsync(source, "临时文件清理");

            var records = await service.LoadRecordsAsync();

            records.Should().ContainSingle();
            records[0].OriginalPath.Should().Be(moved.OriginalPath);
            File.Exists(moved.QuarantinedPath).Should().BeTrue();
            File.Exists(source).Should().BeFalse();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Quarantine_retention_planner_flags_expired_and_over_limit_records_without_deleting()
    {
        var now = new DateTimeOffset(2026, 6, 30, 20, 0, 0, TimeSpan.Zero);
        var old = new QuarantineRecord
        {
            Id = "old",
            MovedAt = now.AddDays(-45),
            OriginalPath = @"C:\tmp\old.tmp",
            QuarantinedPath = @"D:\OMNIX-Entropy\Quarantine\old.tmp",
            ManifestPath = @"D:\OMNIX-Entropy\Quarantine\old.json",
            Reason = "旧临时文件",
            SizeBytes = 10,
            RestoreState = RestoreState.Restorable
        };
        var large = new QuarantineRecord
        {
            Id = "large",
            MovedAt = now.AddDays(-1),
            OriginalPath = @"C:\tmp\large.tmp",
            QuarantinedPath = @"D:\OMNIX-Entropy\Quarantine\large.tmp",
            ManifestPath = @"D:\OMNIX-Entropy\Quarantine\large.json",
            Reason = "大临时文件",
            SizeBytes = 80,
            RestoreState = RestoreState.Restorable
        };
        var restored = new QuarantineRecord
        {
            Id = "restored",
            MovedAt = now.AddDays(-2),
            OriginalPath = @"C:\tmp\restored.tmp",
            QuarantinedPath = @"D:\OMNIX-Entropy\Quarantine\restored.tmp",
            ManifestPath = @"D:\OMNIX-Entropy\Quarantine\restored.json",
            Reason = "已还原文件",
            SizeBytes = 5,
            RestoreState = RestoreState.Restored
        };

        var plan = QuarantineRetentionPlanner.Build(
            [old, large, restored],
            new QuarantineRetentionOptions
            {
                Now = now,
                RetentionDays = 30,
                MaxTotalBytes = 50
            });

        plan.WouldDeleteAutomatically.Should().BeFalse();
        plan.TotalBytes.Should().Be(95);
        plan.Candidates.Should().Contain(candidate => candidate.Record.Id == "old"
            && candidate.Reason == QuarantineCleanupReason.Expired);
        plan.Candidates.Should().Contain(candidate => candidate.Record.Id == "large"
            && candidate.Reason == QuarantineCleanupReason.OverCapacity);
        plan.Candidates.Should().Contain(candidate => candidate.Record.Id == "restored"
            && candidate.Reason == QuarantineCleanupReason.AlreadyRestored);
        plan.Candidates.Should().OnlyContain(candidate => candidate.RequiresConfirmation);
        plan.Summary.Should().Contain("只生成建议");
    }

    [Fact]
    public async Task Quarantine_operation_is_executed_after_safety_pipeline_confirmation()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var service = new FileQuarantineService(Path.Combine(root, "quarantine"));
            var pipeline = new SafetyOperationPipeline(async (descriptor, ct) =>
            {
                var record = await service.QuarantineAsync(descriptor.AffectedPaths.Single(), descriptor.EvidenceSummary!, ct);
                return OperationResult.Ok("moved to quarantine", record);
            });
            var blocked = new OperationDescriptor
            {
                Kind = "clean.temp",
                Title = "清理临时文件",
                Risk = RiskLevel.Low,
                IsDestructive = true,
                RollbackRequired = true,
                EvidenceSummary = "测试临时文件",
                AffectedPaths = [source]
            };

            var blockedResult = await pipeline.ExecuteAsync(blocked);

            blockedResult.Success.Should().BeFalse();
            File.Exists(source).Should().BeTrue();

            var confirmed = new OperationDescriptor
            {
                Kind = "clean.temp",
                Title = "清理临时文件",
                Risk = RiskLevel.Low,
                IsDestructive = true,
                RollbackRequired = true,
                ConfirmationAccepted = true,
                EvidenceSummary = "测试临时文件",
                AffectedPaths = [source]
            };

            var confirmedResult = await pipeline.ExecuteAsync(confirmed);

            confirmedResult.Success.Should().BeTrue();
            confirmedResult.Payload.Should().BeOfType<QuarantineRecord>();
            File.Exists(source).Should().BeFalse();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Quarantine_operation_handler_moves_paths_and_records_timeline_after_pipeline_gate()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            var dbPath = Path.Combine(root, "timeline.db");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var quarantineRoot = Path.Combine(root, "quarantine");
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var handler = new QuarantineOperationHandler(
                new FileQuarantineService(quarantineRoot),
                new ActionTimelineStore(dbPath),
                reader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            var candidate = new OperationDescriptor
            {
                Kind = "clean.temp",
                Title = "清理临时文件",
                Source = OperationSource.Manual,
                Risk = RiskLevel.Low,
                IsDestructive = true,
                RollbackRequired = true,
                EvidenceSummary = "测试临时文件",
                AffectedPaths = [source]
            };
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                candidate, quarantineRoot, reader);
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);

            var result = await pipeline.ExecuteAsync(descriptor);
            var entries = await new ActionTimelineStore(dbPath).LoadRecentAsync(5);

            result.Success.Should().BeTrue();
            File.Exists(source).Should().BeFalse();
            entries.Should().ContainSingle();
            entries[0].Title.Should().Be("清理临时文件");
            entries[0].RestoreState.Should().Be(RestoreState.Restorable);
            entries[0].RestoreOperationKind.Should().Be("quarantine.restore");
            entries[0].AffectedPaths.Should().Contain(source);
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Quarantine_operation_handler_refuses_missing_paths_before_moving_anything()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            var missing = Path.Combine(root, "source", "missing.tmp");
            var dbPath = Path.Combine(root, "timeline.db");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            await File.WriteAllTextAsync(missing, "will disappear");
            var quarantineRoot = Path.Combine(root, "quarantine");
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var handler = new QuarantineOperationHandler(
                new FileQuarantineService(quarantineRoot),
                new ActionTimelineStore(dbPath),
                reader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            var candidate = new OperationDescriptor
            {
                Kind = "clean.temp",
                Title = "清理临时文件",
                Risk = RiskLevel.Low,
                IsDestructive = true,
                RollbackRequired = true,
                EvidenceSummary = "测试临时文件",
                AffectedPaths = [source, missing]
            };
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                candidate, quarantineRoot, reader);
            preparation.Success.Should().BeTrue(preparation.Error);
            File.Delete(missing);
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);

            var result = await pipeline.ExecuteAsync(descriptor);
            var entries = await new ActionTimelineStore(dbPath).LoadRecentAsync(5);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("不存在");
            File.Exists(source).Should().BeTrue();
            entries.Should().BeEmpty();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Quarantine_operation_policy_only_confirms_low_risk_temp_cleanup()
    {
        var allowed = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "清理临时目录",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EvidenceSummary = "临时目录占用 1 GB",
            ConfirmationText = "确认将临时目录移动到隔离区",
            AffectedPaths = [@"C:\temp"]
        };
        var highRisk = new OperationDescriptor
        {
            Kind = "clean.temp",
            Title = "清理系统目录",
            Risk = RiskLevel.High,
            IsDestructive = true,
            RollbackRequired = true,
            EvidenceSummary = "系统目录",
            AffectedPaths = [@"C:\Windows"]
        };
        var wrongKind = new OperationDescriptor
        {
            Kind = "disable.service",
            Title = "禁用服务",
            Risk = RiskLevel.Low,
            IsDestructive = true,
            RollbackRequired = true,
            EvidenceSummary = "启动项",
            AffectedPaths = [@"C:\temp"]
        };

        QuarantineOperationPolicy.ValidateCandidate(allowed).Success.Should().BeTrue();
        QuarantineOperationPolicy.ValidateCandidate(highRisk).Success.Should().BeFalse();
        QuarantineOperationPolicy.ValidateCandidate(wrongKind).Success.Should().BeFalse();

        var confirm = () => QuarantineOperationPolicy.ConfirmForExecution(allowed);
        confirm.Should().Throw<InvalidOperationException>()
            .WithMessage("*候选身份*");
    }

    [Fact]
    public async Task Action_timeline_store_persists_recent_entries_with_restore_state()
    {
        var root = CreateTempRoot();
        var dbPath = Path.Combine(root, "timeline.db");
        try
        {
            var store = new ActionTimelineStore(dbPath);
            var entry = new ActionTimelineEntry
            {
                OccurredAt = new DateTimeOffset(2026, 6, 30, 18, 0, 0, TimeSpan.Zero),
                Source = OperationSource.Manual,
                Title = "清理临时文件",
                EvidenceSummary = "移动到隔离区",
                AffectedPaths = [@"C:\temp\cache.tmp"],
                RestoreState = RestoreState.Restorable,
                RestoreOperationKind = "quarantine.restore"
            };

            await store.AddAsync(entry);
            var loaded = await store.LoadRecentAsync(10);

            loaded.Should().ContainSingle();
            loaded[0].Title.Should().Be("清理临时文件");
            loaded[0].AffectedPaths.Should().Contain(@"C:\temp\cache.tmp");
            loaded[0].RestoreState.Should().Be(RestoreState.Restorable);
            loaded[0].RestoreOperationKind.Should().Be("quarantine.restore");
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public async Task Quarantine_timeline_records_manifest_paths_and_can_mark_restore_result()
    {
        var root = CreateTempRoot();
        try
        {
            var source = Path.Combine(root, "source", "cache.tmp");
            var dbPath = Path.Combine(root, "timeline.db");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            await File.WriteAllTextAsync(source, "temporary data");
            var quarantine = new FileQuarantineService(Path.Combine(root, "quarantine"));
            var store = new ActionTimelineStore(dbPath);
            var reader = new WindowsQuarantineCandidateIdentityReader();
            var handler = new QuarantineOperationHandler(quarantine, store, reader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            var candidate = new OperationDescriptor
            {
                Kind = "clean.temp",
                Title = "清理临时文件",
                Risk = RiskLevel.Low,
                IsDestructive = true,
                RollbackRequired = true,
                EvidenceSummary = "测试临时文件",
                AffectedPaths = [source]
            };
            var preparation = QuarantineOperationPolicy.PrepareForConfirmation(
                candidate, quarantine.QuarantineRoot, reader);
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparation.Operation!);

            await pipeline.ExecuteAsync(descriptor);
            var entries = await store.LoadRecentAsync(5);

            entries.Should().ContainSingle();
            entries[0].Id.Should().BeGreaterThan(0);
            entries[0].RestoreManifestPaths.Should().ContainSingle();
            File.Exists(entries[0].RestoreManifestPaths[0]).Should().BeTrue();

            var restore = await quarantine.RestoreAsync(entries[0].RestoreManifestPaths[0]);
            await store.UpdateRestoreStateAsync(entries[0].Id, restore.RestoreState, null);
            var reloaded = await store.LoadRecentAsync(5);

            restore.Success.Should().BeTrue();
            File.Exists(source).Should().BeTrue();
            reloaded[0].RestoreState.Should().Be(RestoreState.Restored);
            reloaded[0].RestoreOperationKind.Should().BeNull();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void Timeline_presentation_only_enables_restore_for_restorable_quarantine_entries()
    {
        var restorable = new ActionTimelineEntry
        {
            Id = 7,
            OccurredAt = new DateTimeOffset(2026, 6, 30, 19, 0, 0, TimeSpan.Zero),
            Source = OperationSource.Manual,
            Title = "清理临时文件",
            EvidenceSummary = "已移动到隔离区",
            AffectedPaths = [@"C:\tmp\cache.tmp"],
            RestoreState = RestoreState.Restorable,
            RestoreOperationKind = "quarantine.restore",
            RestoreManifestPaths = [@"D:\OMNIX-Entropy\Quarantine\manifest.json"]
        };
        var explainedOnly = new ActionTimelineEntry
        {
            Id = 8,
            Source = OperationSource.Agent,
            Title = "高风险残留",
            EvidenceSummary = "只解释，不处理",
            RestoreState = RestoreState.NotRestorable,
            RestoreOperationKind = "quarantine.restore"
        };

        var enabled = ActionTimelinePresenter.CreateItem(restorable);
        var disabled = ActionTimelinePresenter.CreateItem(explainedOnly);

        enabled.CanRestore.Should().BeTrue();
        enabled.RestoreButtonText.Should().Be("还原");
        enabled.RestoreHint.Should().Contain("不会覆盖");
        enabled.RestoreManifestPaths.Should().ContainSingle();
        disabled.CanRestore.Should().BeFalse();
        disabled.RestoreButtonText.Should().Be("不可还原");
    }

    [Fact]
    public void Timeline_presentation_summarizes_affected_paths_for_beginner_view()
    {
        var rawPath = @"C:\Users\Me\AppData\Local\Example\Cache\very-long-cache-file.tmp";
        var entry = new ActionTimelineEntry
        {
            Id = 9,
            OccurredAt = new DateTimeOffset(2026, 7, 9, 8, 0, 0, TimeSpan.Zero),
            Source = OperationSource.System,
            Title = "Undo center smoke seed",
            EvidenceSummary = "Temporary cache moved to quarantine",
            AffectedPaths = [rawPath],
            RestoreState = RestoreState.Restorable,
            RestoreOperationKind = "quarantine.restore",
            RestoreManifestPaths = [@"D:\OMNIX-Entropy\Quarantine\manifest.json"]
        };

        var item = ActionTimelinePresenter.CreateItem(entry);

        item.Detail.Should().Contain("1");
        item.Detail.Should().NotContain(rawPath);
        item.Detail.Should().NotContain(@"C:\Users\Me");
    }

    [Fact]
    public void Timeline_presentation_keeps_raw_paths_in_collapsed_technical_details()
    {
        var rawPath = @"C:\Users\Me\AppData\Local\Example\Cache\very-long-cache-file.tmp";
        var manifestPath = @"D:\OMNIX-Entropy\Quarantine\manifest.json";
        var entry = new ActionTimelineEntry
        {
            Id = 10,
            OccurredAt = new DateTimeOffset(2026, 7, 9, 9, 0, 0, TimeSpan.Zero),
            Source = OperationSource.System,
            Title = "Undo center details",
            EvidenceSummary = "Temporary cache moved to quarantine",
            AffectedPaths = [rawPath],
            RestoreState = RestoreState.Restorable,
            RestoreOperationKind = "quarantine.restore",
            RestoreManifestPaths = [manifestPath]
        };

        var item = ActionTimelinePresenter.CreateItem(entry);

        item.Detail.Should().NotContain(rawPath);
        item.TechnicalDetailsButtonText.Should().Be("\u67e5\u770b\u6280\u672f\u8be6\u60c5");
        item.TechnicalDetails.Should().Contain(line => line.Contains(rawPath, StringComparison.Ordinal));
        item.TechnicalDetails.Should().Contain(line => line.Contains(manifestPath, StringComparison.Ordinal));
        item.TechnicalDetails.Should().Contain(line => line.Contains("quarantine.restore", StringComparison.Ordinal));
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "css-quarantine-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTempRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
