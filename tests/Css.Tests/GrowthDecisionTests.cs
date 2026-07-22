using Css.Core.Apps;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class GrowthDecisionTests
{
    [Fact]
    public void Software_growth_enrichment_deduplicates_nested_paths_and_sums_siblings()
    {
        var profile = new SoftwareProfile
        {
            Name = "Docker Desktop",
            Publisher = "Docker Inc.",
            Category = SoftwareCategory.DevelopmentTool,
            InstallPath = @"D:\Software\Docker\Install",
            InstallDate = new DateOnly(2026, 7, 20),
            DisplayIconPath = @"D:\Software\Docker\Docker.exe",
            DisplayIconIndex = -1,
            CachePaths = [@"C:\Fixture\Docker\Cache"],
            Services = ["DockerService"]
        };
        var findings = new[]
        {
            Growth("Docker Desktop", @"C:\Fixture\Docker\Cache", 100),
            Growth("Docker Desktop", @"C:\Fixture\Docker\Cache\Nested", 80),
            Growth("Docker Desktop", @"C:\Fixture\Docker\Logs", 40)
        };

        var enriched = SoftwareGrowthProfileEnricher.Apply([profile], findings).Single();
        var tile = AppPresentationBuilder.CreateTile(enriched);
        var drawer = AppPresentationBuilder.CreateDrawer(enriched);

        enriched.Should().NotBeSameAs(profile);
        enriched.RecentGrowthBytes.Should().Be(140);
        enriched.Publisher.Should().Be(profile.Publisher);
        enriched.InstallPath.Should().Be(profile.InstallPath);
        enriched.InstallDate.Should().Be(profile.InstallDate);
        enriched.DisplayIconPath.Should().Be(profile.DisplayIconPath);
        enriched.DisplayIconIndex.Should().Be(-1);
        enriched.Services.Should().Equal(profile.Services);
        tile.Status.Should().Be(AppTileStatus.Warning);
        tile.ShortTag.Should().Be("最近变大");
        drawer.SizeSummary.Should().Contain("最近增长 140 B");
    }

    [Fact]
    public void Software_growth_enrichment_refuses_ambiguous_first_or_shared_attribution()
    {
        var profiles = new[]
        {
            new SoftwareProfile { Name = "Same App", RecentGrowthBytes = 999 },
            new SoftwareProfile { Name = "same app", RecentGrowthBytes = 999 },
            new SoftwareProfile { Name = "Baseline App", RecentGrowthBytes = 999 }
        };
        var findings = new[]
        {
            Growth("Same App", @"C:\Fixture\Same", 100),
            Growth(
                "Baseline App",
                @"C:\Fixture\Baseline",
                100,
                isNewObservation: true),
            Growth(
                ScanSnapshotBuilder.SharedSoftwareOwner,
                @"C:\Fixture\Shared",
                100,
                sourceKind: GrowthSourceKind.SharedSoftware)
        };

        var enriched = SoftwareGrowthProfileEnricher.Apply(profiles, findings);

        enriched.Should().OnlyContain(profile => profile.RecentGrowthBytes == 0);
    }

    [Fact]
    public void Snapshot_builder_applies_one_global_item_limit()
    {
        var roots = Enumerable.Range(0, 2_100)
            .Select(index => Node(
                $"Root{index:D4}",
                $@"C:\Fixture\Root{index:D4}",
                index + 1,
                UsageCategory.Other))
            .ToList();
        var result = new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 1_000_000,
            FreeBytes = 500_000,
            TopLevel = roots
        };

        var snapshot = ScanSnapshotBuilder.Build(result, DateTimeOffset.UtcNow);

        snapshot.Items.Should().HaveCount(2_048);
        snapshot.Items.Should().Contain(item => item.Path == @"C:\Fixture\Root2099");
        snapshot.Items.Should().NotContain(item => item.Path == @"C:\Fixture\Root0000");
    }

    [Fact]
    public void Snapshot_builder_keeps_broad_root_and_exact_software_known_path()
    {
        var result = ResultWithPath(
            @"C:\Users\Fixture\AppData\Local\Docker",
            8L * 1024 * 1024 * 1024);
        var profile = new SoftwareProfile
        {
            Name = "Docker Desktop",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Docker"]
        };

        var snapshot = ScanSnapshotBuilder.Build(
            result,
            DateTimeOffset.UtcNow,
            [profile]);

        snapshot.Items.Should().Contain(item =>
            item.Path.Equals(@"C:\Users", StringComparison.OrdinalIgnoreCase)
            && item.OwnerSoftware == UsageCategory.UserProfiles.ToString());
        snapshot.Items.Should().Contain(item =>
            item.Path.Equals(
                @"C:\Users\Fixture\AppData\Local\Docker",
                StringComparison.OrdinalIgnoreCase)
            && item.OwnerSoftware == "Docker Desktop"
            && item.SizeBytes == 8L * 1024 * 1024 * 1024);
    }

    [Fact]
    public void Shared_exact_path_is_not_forced_onto_one_software_profile()
    {
        var sharedPath = @"C:\Users\Fixture\AppData\Local\SharedCache";
        var result = ResultWithPath(sharedPath, 2L * 1024 * 1024 * 1024);
        var first = new SoftwareProfile { Name = "App A", CachePaths = [sharedPath] };
        var second = new SoftwareProfile { Name = "App B", DataPaths = [sharedPath] };

        var snapshot = ScanSnapshotBuilder.Build(
            result,
            DateTimeOffset.UtcNow,
            [first, second]);

        snapshot.Items.Single(item =>
                item.Path.Equals(sharedPath, StringComparison.OrdinalIgnoreCase))
            .OwnerSoftware.Should().Be(ScanSnapshotBuilder.SharedSoftwareOwner);
    }

    [Fact]
    public void Profile_path_absent_from_scanned_tree_is_not_invented()
    {
        var result = ResultWithPath(
            @"C:\Users\Fixture\AppData\Local\Present",
            1024);
        var profile = new SoftwareProfile
        {
            Name = "Missing App",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\NotObserved"]
        };

        var snapshot = ScanSnapshotBuilder.Build(
            result,
            DateTimeOffset.UtcNow,
            [profile]);

        snapshot.Items.Should().NotContain(item => item.OwnerSoftware == "Missing App");
    }

    [Fact]
    public void First_software_observation_is_labeled_as_baseline_not_historical_growth()
    {
        var previous = new ScanSnapshot(
            new DateTimeOffset(2026, 7, 12, 8, 0, 0, TimeSpan.Zero),
            [new ScanSnapshotItem(@"C:\Users", "UserProfiles", 1000)]);
        var current = new ScanSnapshot(
            new DateTimeOffset(2026, 7, 13, 8, 0, 0, TimeSpan.Zero),
            [
                new ScanSnapshotItem(@"C:\Users", "UserProfiles", 1200),
                new ScanSnapshotItem(
                    @"C:\Users\Fixture\AppData\Local\Docker",
                    "Docker Desktop",
                    8L * 1024 * 1024 * 1024)
            ]);

        var finding = GrowthAnalyzer.Compare(previous, current)
            .Single(item => item.OwnerSoftware == "Docker Desktop");
        var item = GrowthFindingPresenter.Create(finding);
        var decision = GrowthDecisionPresenter.Create(finding);

        finding.IsNewObservation.Should().BeTrue();
        finding.SourceKind.Should().Be(GrowthSourceKind.Software);
        item.Title.Should().StartWith("首次记录：Docker Desktop");
        item.Summary.Should().StartWith("当前 ");
        item.Detail.Should().Contain("还不能判断增长速度");
        decision.RequiresMoreObservation.Should().BeTrue();
        decision.EvidenceText.Should().Contain("不能证明它一直在增长");
        decision.CanExecuteDirectly.Should().BeFalse();
        VisibleText(item, decision).Should().NotContain(@"C:\");
    }

    [Fact]
    public void Repeated_software_growth_gets_separate_now_and_prevention_advice()
    {
        var path = @"C:\Users\Fixture\AppData\Local\Docker\cache";
        var previous = new ScanSnapshot(
            new DateTimeOffset(2026, 7, 12, 8, 0, 0, TimeSpan.Zero),
            [new ScanSnapshotItem(path, "Docker Desktop", 2L * 1024 * 1024 * 1024)]);
        var current = new ScanSnapshot(
            new DateTimeOffset(2026, 7, 13, 8, 0, 0, TimeSpan.Zero),
            [new ScanSnapshotItem(path, "Docker Desktop", 5L * 1024 * 1024 * 1024)]);

        var finding = GrowthAnalyzer.Compare(previous, current).Single();
        var item = GrowthFindingPresenter.Create(finding);
        var decision = GrowthDecisionPresenter.Create(finding);

        finding.IsNewObservation.Should().BeFalse();
        finding.SourceKind.Should().Be(GrowthSourceKind.Software);
        finding.ObservationInterval.Should().Be(TimeSpan.FromDays(1));
        item.Title.Should().Contain("最近增长：Docker Desktop");
        item.Summary.Should().Contain("+3.0 GB");
        decision.OneTimeAction.Should().StartWith("现在：");
        decision.PreventionAction.Should().StartWith("以后：");
        decision.PreventionAction.Should().Contain("D 盘");
        decision.TargetAppName.Should().Be("Docker Desktop");
        decision.CanOpenApp.Should().BeTrue();
        decision.CanExecuteDirectly.Should().BeFalse();
        VisibleText(item, decision).Should().NotContain(@"C:\");
    }

    [Fact]
    public void Three_or_more_recent_observations_are_required_for_sustained_growth()
    {
        var path = @"C:\Users\Fixture\AppData\Local\Docker\cache";
        var start = new DateTimeOffset(2026, 7, 10, 8, 0, 0, TimeSpan.Zero);
        var snapshots = new[]
        {
            Snapshot(start, path, 1_000),
            Snapshot(start.AddDays(1), path, 2_000),
            Snapshot(start.AddDays(2), path, 3_000),
            Snapshot(start.AddDays(3), path, 4_000)
        };
        var latest = GrowthAnalyzer.Compare(snapshots[^2], snapshots[^1]);

        var finding = GrowthTrendAnalyzer.Enrich(latest, snapshots).Single();
        var item = GrowthFindingPresenter.Create(finding);
        var decision = GrowthDecisionPresenter.Create(finding);

        finding.IsSustainedGrowth.Should().BeTrue();
        finding.ObservedSnapshots.Should().Be(4);
        finding.PositiveGrowthIntervals.Should().Be(3);
        finding.TrendGrowthBytes.Should().Be(3_000);
        item.Title.Should().StartWith("持续增长：Docker Desktop");
        decision.Headline.Should().Contain("近期多次");
        decision.EvidenceText.Should().Contain("最近 4 次观察中有 3 次变大");
        decision.RequiresMoreObservation.Should().BeFalse();
        decision.CanExecuteDirectly.Should().BeFalse();
        VisibleText(item, decision).Should().NotContain(@"C:\");
    }

    [Fact]
    public void One_recent_increase_is_not_presented_as_sustained_growth()
    {
        var path = @"C:\Users\Fixture\AppData\Local\Docker\cache";
        var start = new DateTimeOffset(2026, 7, 10, 8, 0, 0, TimeSpan.Zero);
        var snapshots = new[]
        {
            Snapshot(start, path, 3_000),
            Snapshot(start.AddDays(1), path, 2_000),
            Snapshot(start.AddDays(2), path, 1_000),
            Snapshot(start.AddDays(3), path, 4_000)
        };
        var latest = GrowthAnalyzer.Compare(snapshots[^2], snapshots[^1]);

        var finding = GrowthTrendAnalyzer.Enrich(latest, snapshots).Single();
        var item = GrowthFindingPresenter.Create(finding);
        var decision = GrowthDecisionPresenter.Create(finding);

        finding.IsSustainedGrowth.Should().BeFalse();
        finding.PositiveGrowthIntervals.Should().Be(1);
        item.Title.Should().StartWith("最近增长：Docker Desktop");
        decision.EvidenceText.Should().Contain("目前只有一次变化");
        decision.RequiresMoreObservation.Should().BeTrue();
    }

    [Fact]
    public void Scan_session_enriches_latest_delta_from_recent_history()
    {
        var path = @"C:\Users\Fixture\AppData\Local\Docker";
        var start = new DateTimeOffset(2026, 7, 10, 8, 0, 0, TimeSpan.Zero);
        var oldest = Snapshot(start, path, 1_000);
        var middle = Snapshot(start.AddDays(1), path, 2_000);
        var latest = Snapshot(start.AddDays(2), path, 3_000);
        var profile = new SoftwareProfile
        {
            Name = "Docker Desktop",
            CachePaths = [path]
        };

        var session = DiskScanSessionBuilder.Build(
            ResultWithPath(path, 4_000),
            latest,
            start.AddDays(3),
            [profile],
            [latest, middle, oldest]);

        var finding = session.GrowthFindings.Single(item =>
            item.OwnerSoftware == "Docker Desktop");
        finding.IsSustainedGrowth.Should().BeTrue();
        finding.ObservedSnapshots.Should().Be(4);
        finding.TrendGrowthBytes.Should().Be(3_000);
    }

    [Fact]
    public void Duplicate_snapshot_paths_are_reduced_before_growth_comparison()
    {
        var previous = new ScanSnapshot(
            DateTimeOffset.UtcNow.AddDays(-1),
            [
                new ScanSnapshotItem(@"C:\Temp", "Temp", 100),
                new ScanSnapshotItem(@"C:\Temp", "Temp", 200)
            ]);
        var current = new ScanSnapshot(
            DateTimeOffset.UtcNow,
            [
                new ScanSnapshotItem(@"C:\Temp", "Temp", 500),
                new ScanSnapshotItem(@"C:\Temp", "Temp", 400)
            ]);

        var findings = GrowthAnalyzer.Compare(previous, current);

        findings.Should().ContainSingle();
        findings[0].PreviousBytes.Should().Be(200);
        findings[0].CurrentBytes.Should().Be(500);
    }

    [Fact]
    public void Display_ranking_prefers_attributed_child_over_the_same_historical_parent_delta()
    {
        var parent = new GrowthFinding
        {
            Path = @"C:\Users",
            OwnerSoftware = "UserProfiles",
            PreviousBytes = 10_000,
            CurrentBytes = 20_000,
            SourceKind = GrowthSourceKind.UserArea,
            IsNewObservation = false,
            Reason = "Grew since previous scan."
        };
        var child = new GrowthFinding
        {
            Path = @"C:\Users\Fixture\AppData\Local\Docker",
            OwnerSoftware = "Docker Desktop",
            PreviousBytes = 5_000,
            CurrentBytes = 15_000,
            SourceKind = GrowthSourceKind.Software,
            IsNewObservation = false,
            Reason = "Grew since previous scan."
        };

        var items = GrowthFindingPresenter.CreateList([parent, child]);

        items.Should().ContainSingle();
        items[0].Finding.Should().BeSameAs(child);
        items[0].Title.Should().Contain("Docker Desktop");
    }

    [Fact]
    public void Display_ranking_keeps_parent_when_software_child_explains_only_a_small_share()
    {
        var parent = new GrowthFinding
        {
            Path = @"C:\Users",
            OwnerSoftware = "UserProfiles",
            PreviousBytes = 10_000,
            CurrentBytes = 30_000,
            SourceKind = GrowthSourceKind.UserArea,
            Reason = "Grew since previous scan."
        };
        var child = new GrowthFinding
        {
            Path = @"C:\Users\Fixture\AppData\Local\Docker",
            OwnerSoftware = "Docker Desktop",
            PreviousBytes = 5_000,
            CurrentBytes = 7_000,
            SourceKind = GrowthSourceKind.Software,
            Reason = "Grew since previous scan."
        };

        var items = GrowthFindingPresenter.CreateList([parent, child]);

        items.Should().HaveCount(2);
        items.Should().Contain(item => item.Finding == parent);
        items.Should().Contain(item => item.Finding == child);
    }

    [Fact]
    public void C_drive_page_places_automation_backed_agent_decision_before_growth_list()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var ids = new[]
        {
            "GrowthDecisionHeadlineTextBlock",
            "GrowthDecisionEvidenceTextBlock",
            "GrowthDecisionOneTimeTextBlock",
            "GrowthDecisionPreventionTextBlock",
            "GrowthDecisionSafetyTextBlock"
        };

        foreach (var id in ids)
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"OpenGrowthAppButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"GrowthListBox\"");
        xaml.IndexOf("GrowthDecisionHeadlineTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("GrowthListBox", StringComparison.Ordinal));
        code.Should().Contain("DiskScanSessionBuilder.Build(");
        code.Should().Contain("attributionProfiles");
        code.Should().Contain("LoadRecentAsync(");
        code.Should().Contain("previousSnapshots,");
        code.Should().Contain("GrowthDecisionPresenter.Create(firstGrowth?.Finding)");
        code.Should().Contain("private void ApplyGrowthDecision");
        code.Should().Contain("OpenGrowthAppButton.Tag = decision.TargetAppName");
        code.Should().NotContain("GrowthDecisionViewModel decision)\r\n    {\r\n        Process.Start");
    }

    private static string VisibleText(
        GrowthFindingViewModel item,
        GrowthDecisionViewModel decision) =>
        string.Join(
            "\n",
            item.Title,
            item.Summary,
            item.Detail,
            item.AgentSuggestion,
            decision.Headline,
            decision.EvidenceText,
            decision.OneTimeAction,
            decision.PreventionAction,
            decision.SafetyText);

    private static DriveScanResult ResultWithPath(string leafPath, long leafSize)
    {
        var users = Node("Users", @"C:\Users", leafSize, UsageCategory.UserProfiles);
        var fixture = Node("Fixture", @"C:\Users\Fixture", leafSize, UsageCategory.UserProfiles);
        var appData = Node("AppData", @"C:\Users\Fixture\AppData", leafSize, UsageCategory.AppData);
        var local = Node("Local", @"C:\Users\Fixture\AppData\Local", leafSize, UsageCategory.AppData);
        var leafName = Path.GetFileName(leafPath);
        var leaf = Node(leafName, leafPath, leafSize, UsageCategory.AppData);
        if (!leafPath.Equals(local.Path, StringComparison.OrdinalIgnoreCase))
            local.Children.Add(leaf);
        appData.Children.Add(local);
        fixture.Children.Add(appData);
        users.Children.Add(fixture);
        return new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 50L * 1024 * 1024 * 1024,
            TopLevel = [users]
        };
    }

    private static ScanSnapshot Snapshot(
        DateTimeOffset capturedAt,
        string path,
        long bytes) =>
        new(
            capturedAt,
            [new ScanSnapshotItem(path, "Docker Desktop", bytes)]);

    private static GrowthFinding Growth(
        string owner,
        string path,
        long growthBytes,
        bool isNewObservation = false,
        GrowthSourceKind sourceKind = GrowthSourceKind.Software) =>
        new()
        {
            Path = path,
            OwnerSoftware = owner,
            PreviousBytes = isNewObservation ? 0 : 100,
            CurrentBytes = isNewObservation ? growthBytes : 100 + growthBytes,
            IsNewObservation = isNewObservation,
            SourceKind = sourceKind,
            Reason = isNewObservation
                ? "First observation."
                : "Grew since previous scan."
        };

    private static CategoryNode Node(
        string name,
        string path,
        long size,
        UsageCategory category) =>
        new()
        {
            Name = name,
            Path = path,
            SizeBytes = size,
            Category = category
        };

    private static string FindRepositoryFile(params string[] segments) =>
        Path.Combine([FindRepositoryRoot(), .. segments]);

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
