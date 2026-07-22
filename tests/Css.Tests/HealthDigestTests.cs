using System;
using System.IO;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class HealthDigestTests
{
    [Fact]
    public void Builder_creates_stable_path_free_non_executable_digest()
    {
        var captured = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);
        var privatePath = @"C:\Users\Me\Downloads\private.zip";
        var health = new HealthCheckSummary
        {
            OverallScore = 76,
            Dimensions =
            [
                new HealthDimensionResult
                {
                    Name = "磁盘健康",
                    Result = privatePath + " 占用较大",
                    Rating = "需要关注"
                }
            ],
            KeyFindings =
            [
                new HealthFinding
                {
                    Text = "发现 " + privatePath + " 长期增长",
                    Kind = HealthFindingKind.SustainedGrowth,
                    Action = RecommendationAction.Observe,
                    Risk = RiskLevel.Medium
                }
            ]
        };
        var snapshot = new ScanSnapshot(captured, []);
        var profiles = new[]
        {
            new SoftwareProfile
            {
                Name = "Example",
                CDriveWritePaths = [@"C:\Users\Me\AppData\Local\Example"]
            }
        };

        var first = HealthDigestBuilder.Create(@"C:\", snapshot, health, profiles);
        var same = HealthDigestBuilder.Create(@"c:\", snapshot, health, profiles);
        var later = HealthDigestBuilder.Create(
            @"C:\",
            snapshot with { CapturedAt = captured.AddMinutes(1) },
            health,
            profiles);
        var visible = first.Headline + "\n" + first.Summary + "\n" +
            string.Join("\n", first.KeyFindings);

        first.ScanIdentity.Should().Be(same.ScanIdentity);
        later.ScanIdentity.Should().NotBe(first.ScanIdentity);
        visible.Should().Contain("某个本机位置").And.NotContain(@"C:\");
        first.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public async Task Store_upserts_scan_identity_and_refuses_visible_local_paths()
    {
        var root = CreateTempRoot();
        try
        {
            var store = new HealthDigestStore(Path.Combine(root, "state.db"));
            var captured = new DateTimeOffset(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);
            await store.SaveAsync(Digest("same", captured, 70, "first"));
            await store.SaveAsync(Digest("same", captured, 80, "updated"));

            var loaded = await store.LoadRecentAsync();

            loaded.Should().ContainSingle();
            loaded[0].OverallScore.Should().Be(80);
            loaded[0].Summary.Should().Contain("updated");
            await FluentActions.Awaiting(() => store.SaveAsync(new HealthDigest
                {
                    ScanIdentity = "unsafe",
                    CapturedAt = captured,
                    OverallScore = 80,
                    Headline = "unsafe",
                    Summary = @"C:\Users\Me\secret.txt",
                    KeyFindings = ["unsafe"]
                }))
                .Should().ThrowAsync<InvalidDataException>();
        }
        finally
        {
            DeleteTempRoot(root);
        }
    }

    [Fact]
    public void History_uses_one_real_scan_per_day_and_explains_weekly_trend()
    {
        var now = new DateTimeOffset(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);
        var digests = new[]
        {
            Digest("today-new", now.AddHours(-1), 82, "today new"),
            Digest("today-old", now.AddHours(-4), 78, "today old"),
            Digest("yesterday", now.AddDays(-1), 74, "yesterday"),
            Digest("old", now.AddDays(-8), 90, "outside week")
        };

        var view = HealthDigestHistoryPresenter.Create(digests, now);
        var empty = HealthDigestHistoryPresenter.Create([], now);

        view.DailyRows.Should().HaveCount(3);
        view.DailyRows[0].Summary.Should().Contain("today new");
        view.WeeklySummary.Should().Contain("2 次").And.Contain("提高 8 分");
        view.MonitoringNotice.Should().Contain("手动").And.Contain("没有自动执行");
        view.HasEvidence.Should().BeTrue();
        view.CanExecuteDirectly.Should().BeFalse();
        empty.HasEvidence.Should().BeFalse();
        empty.MonitoringNotice.Should().Contain("没有后台定时扫描");
    }

    [Fact]
    public void Main_window_saves_digest_only_after_successful_manual_scan_and_navigates_internally()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var main = Read("src", "Css.App", "MainWindow.xaml.cs");
        var store = Read("src", "Css.Core", "Apps", "HealthDigest.cs");

        foreach (var id in new[]
        {
            "HealthDigestLatestHeadlineTextBlock",
            "HealthDigestLatestSummaryTextBlock",
            "HealthDigestWeeklySummaryTextBlock",
            "HealthDigestMonitoringNoticeTextBlock",
            "HealthDigestHistoryListBox",
            "OpenHealthDigestEvidenceButton"
        })
        {
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
        }

        main.IndexOf("await _snapshotStore.SaveAsync", StringComparison.Ordinal)
            .Should().BeLessThan(main.IndexOf("TrySaveHealthDigestAsync", StringComparison.Ordinal));
        main.Should().Contain("HealthDigestBuilder.Create");
        main.Should().Contain("await _healthDigestStore.SaveAsync");
        main.Should().Contain("ShowPage(\"CDrive\")");
        main.Should().NotContain("TaskScheduler").And.NotContain("schtasks");
        store.Should().Contain("MaximumDigests = 90");
        store.Should().Contain("ON CONFLICT(scan_identity) DO UPDATE");
        store.Should().NotContain("OperationDescriptor").And.NotContain("SafetyOperationPipeline");
    }

    private static HealthDigest Digest(
        string identity,
        DateTimeOffset captured,
        int score,
        string text) =>
        new()
        {
            ScanIdentity = identity,
            CapturedAt = captured,
            OverallScore = score,
            Headline = "状态摘要",
            Summary = text,
            KeyFindings = [text]
        };

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-health-digest-" + Guid.NewGuid().ToString("N"));
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
