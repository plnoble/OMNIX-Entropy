using System.Collections.Generic;
using Css.Rules.Models;
using Css.Rules;
using Css.Core.Recommendations;
using Css.Scanner.Disk;
using Css.Scanner.Persistence;
using FluentAssertions;

namespace Css.Tests;

public class DiskScannerTests
{
    private static ScanRules SampleRules() => new()
    {
        ExpectedRootDirs = ["Windows", "Program Files", "Users", "$Recycle.Bin"],
        CategoryPatterns = new()
        {
            ["System"] = ["C:\\Windows"],
            ["Programs"] = ["C:\\Program Files"],
            ["AppData"] = ["**\\AppData\\Local\\*"]
        }
    };

    [Fact]
    public void Classifier_flags_top_level_folders_not_in_allowlist()
    {
        var rules = SampleRules();
        var nodes = new List<CategoryNode>
        {
            new() { Name = "Windows", Path = "C:\\Windows" },
            new() { Name = "Program Files", Path = "C:\\Program Files" },
            new() { Name = " SETUP539", Path = "C:\\ SETUP539" },
            new() { Name = "KRECYCLE", Path = "C:\\KRECYCLE" }
        };
        new CategoryClassifier(rules).Classify(nodes, "C:\\");

        nodes[0].IsUnexpectedRoot.Should().BeFalse();
        nodes[2].IsUnexpectedRoot.Should().BeTrue("leading-space folder is not in allowlist");
        nodes[3].IsUnexpectedRoot.Should().BeTrue();
    }

    [Fact]
    public void Classifier_maps_paths_to_categories_via_globs()
    {
        var rules = SampleRules();
        var c = new CategoryClassifier(rules);
        c.ClassifyPath("C:\\Windows\\System32").Should().Be(UsageCategory.System);
        c.ClassifyPath("C:\\Users\\Me\\AppData\\Local\\Docker").Should().Be(UsageCategory.AppData);
        c.ClassifyPath("C:\\SomeUnknown").Should().Be(UsageCategory.Other);
    }

    [Fact]
    public void App_rules_classify_top_level_temp_roots_for_cleanup_fixture()
    {
        var rules = new ScanRuleLoader().Load(FindRepositoryFile("src", "Css.App", "rules.scan.json"));
        var classifier = new CategoryClassifier(rules);

        classifier.ClassifyPath(@"C:\Temp").Should().Be(UsageCategory.Temp);
        classifier.ClassifyPath(@"C:\tmp").Should().Be(UsageCategory.Temp);
        classifier.ClassifyPath(@"D:\Agent\Project\OMNIX-Entropy\.omx\qa-cdrive-cleanup-scan-root\Temp")
            .Should().Be(UsageCategory.Temp);
    }

    [Fact]
    public void Report_lists_unexpected_roots_and_formats_sizes()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 300L * 1024 * 1024 * 1024,
            FreeBytes = 100L * 1024 * 1024 * 1024,
            TopLevel =
            [
                new() { Name = "Windows", Path = "C:\\Windows", SizeBytes = 30L*1024*1024*1024, Category = UsageCategory.System },
                new() { Name = " SETUP539", Path = "C:\\ SETUP539", SizeBytes = 5L*1024*1024*1024, IsUnexpectedRoot = true }
            ],
            BigRocks = [ new() { Name = "Page file", SizeBytes = 8L*1024*1024*1024 } ]
        };

        var report = RootCauseReportBuilder.Build(result);
        report.Should().Contain("非预期根目录文件夹");
        report.Should().Contain(" SETUP539");
        report.Should().Contain("5.0 GB");
        report.Should().Contain("Page file");
    }

    [Fact]
    public void Recommendation_builder_creates_cards_for_unexpected_roots_and_temp_cleanup()
    {
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 300L * 1024 * 1024 * 1024,
            FreeBytes = 100L * 1024 * 1024 * 1024,
            TopLevel =
            [
                new() { Name = "Windows", Path = "C:\\Windows", SizeBytes = 30L*1024*1024*1024, Category = UsageCategory.System },
                new() { Name = "temp", Path = "C:\\temp", SizeBytes = 2L*1024*1024*1024, Category = UsageCategory.Temp, IsUnexpectedRoot = true },
                new() { Name = "Users", Path = "C:\\Users", SizeBytes = 120L*1024*1024*1024, Category = UsageCategory.UserProfiles }
            ]
        };

        var cards = DiskRecommendationBuilder.Build(result);

        cards.Should().Contain(c => c.Action == RecommendationAction.Observe && c.Title.Contains("非预期根目录"));
        var clean = cards.Should().ContainSingle(c => c.Action == RecommendationAction.Clean).Subject;
        clean.HasDecisionCardMinimums.Should().BeTrue();
        clean.Operation.Should().NotBeNull();
        clean.Operation!.AffectedPaths.Should().Contain("C:\\temp");
        clean.Operation.IsDestructive.Should().BeTrue();
    }

    [Fact]
    public void Scan_session_builder_combines_report_recommendations_and_growth()
    {
        var previous = new ScanSnapshot(
            new DateTimeOffset(2026, 6, 29, 8, 0, 0, TimeSpan.Zero),
            [new ScanSnapshotItem("C:\\temp", "Temp", 1024)]);
        var result = new DriveScanResult
        {
            Drive = "C:\\",
            TotalBytes = 100_000,
            FreeBytes = 60_000,
            TopLevel =
            [
                new() { Name = "temp", Path = "C:\\temp", SizeBytes = 4096, Category = UsageCategory.Temp, IsUnexpectedRoot = true }
            ]
        };

        var session = DiskScanSessionBuilder.Build(result, previous, new DateTimeOffset(2026, 6, 30, 8, 0, 0, TimeSpan.Zero));

        session.Report.Should().Contain("C盘根因报告");
        session.Recommendations.Should().NotBeEmpty();
        session.GrowthFindings.Should().ContainSingle().Which.GrowthBytes.Should().Be(3072);
        session.CurrentSnapshot.Items.Should().ContainSingle(i => i.Path == "C:\\temp" && i.SizeBytes == 4096);
    }

    [Fact]
    public async Task Scan_snapshot_store_persists_latest_snapshot()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "css-snapshot-" + Guid.NewGuid() + ".db");
        try
        {
            var store = new ScanSnapshotStore(dbPath);
            var snapshot = new ScanSnapshot(
                new DateTimeOffset(2026, 6, 30, 8, 0, 0, TimeSpan.Zero),
                [
                    new ScanSnapshotItem("C:\\temp", "Temp", 4096),
                    new ScanSnapshotItem("C:\\Users", "UserProfiles", 8192)
                ]);

            await store.SaveAsync("C:\\", snapshot);
            var loaded = await store.LoadLatestAsync("C:\\");

            loaded.Should().NotBeNull();
            loaded!.CapturedAt.Should().Be(snapshot.CapturedAt);
            loaded.Items.Should().HaveCount(2);
            loaded.Items.Should().Contain(i => i.Path == "C:\\Users" && i.OwnerSoftware == "UserProfiles");
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task Scan_snapshot_store_keeps_bounded_recent_history_per_drive()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "css-snapshot-history-" + Guid.NewGuid() + ".db");
        try
        {
            var store = new ScanSnapshotStore(dbPath);
            var start = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
            for (var index = 0; index < 95; index++)
            {
                await store.SaveAsync(
                    "C:\\",
                    new ScanSnapshot(
                        start.AddHours(index),
                        [new ScanSnapshotItem("C:\\Fixture", "Fixture", index)]));
            }
            await store.SaveAsync(
                "D:\\",
                new ScanSnapshot(
                    start,
                    [new ScanSnapshotItem("D:\\Fixture", "Fixture", 7)]));

            var cHistory = await store.LoadRecentAsync("C:\\", 200);
            var dHistory = await store.LoadRecentAsync("D:\\", 8);
            var latest = await store.LoadLatestAsync("C:\\");

            cHistory.Should().HaveCount(ScanSnapshotStore.MaximumSnapshotsPerDrive);
            cHistory.Select(snapshot => snapshot.CapturedAt).Should().Equal(
                cHistory.Select(snapshot => snapshot.CapturedAt).OrderByDescending(value => value));
            cHistory[0].Items.Single().SizeBytes.Should().Be(94);
            cHistory[^1].Items.Single().SizeBytes.Should().Be(5);
            dHistory.Should().ContainSingle();
            dHistory[0].Items.Single().SizeBytes.Should().Be(7);
            latest!.Items.Single().SizeBytes.Should().Be(94);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task Scan_snapshot_store_refuses_oversized_snapshot_before_opening_database()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "css-snapshot-oversize-" + Guid.NewGuid() + ".db");
        var snapshot = new ScanSnapshot(
            DateTimeOffset.UtcNow,
            Enumerable.Range(0, ScanSnapshotBuilder.MaximumSnapshotItems + 1)
                .Select(index => new ScanSnapshotItem(
                    $"C:\\Fixture\\Item{index:D4}",
                    "Fixture",
                    index))
                .ToArray());
        var store = new ScanSnapshotStore(dbPath);

        Func<Task> action = () => store.SaveAsync("C:\\", snapshot);

        await action.Should().ThrowAsync<InvalidDataException>();
        File.Exists(dbPath).Should().BeFalse();
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
