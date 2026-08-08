using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class SystemCleanupOpportunityTests
{
    [Fact]
    public void Download_probe_counts_only_matching_old_top_level_installers_as_reviewable()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "omnix-installer-remnants-" + Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(root, "nested");
        Directory.CreateDirectory(nested);

        try
        {
            var oldInstaller = Path.Combine(root, "old-setup.exe");
            var recentInstaller = Path.Combine(root, "recent.msi");
            var oldDocument = Path.Combine(root, "notes.txt");
            var nestedInstaller = Path.Combine(nested, "nested-old.exe");
            File.WriteAllBytes(oldInstaller, new byte[10]);
            File.WriteAllBytes(recentInstaller, new byte[20]);
            File.WriteAllBytes(oldDocument, new byte[40]);
            File.WriteAllBytes(nestedInstaller, new byte[80]);
            File.SetLastWriteTimeUtc(oldInstaller, DateTime.UtcNow.AddDays(-90));
            File.SetLastWriteTimeUtc(recentInstaller, DateTime.UtcNow.AddDays(-1));
            File.SetLastWriteTimeUtc(oldDocument, DateTime.UtcNow.AddDays(-90));
            File.SetLastWriteTimeUtc(nestedInstaller, DateTime.UtcNow.AddDays(-90));

            var result = new KnownCleanupStoreProbe().Probe(
            [
                new KnownCleanupStoreLocation(
                    KnownCleanupStoreKind.InstallerDownloadRemnants,
                    "旧安装包候选",
                    root,
                    CleanupHandling.UserReview)
                {
                    ReviewAgeDays = 30,
                    RecurseSubdirectories = false,
                    IncludedExtensions = [".exe", ".msi"]
                }
            ]).Single();

            result.FileCount.Should().Be(2);
            result.SizeBytes.Should().Be(30);
            result.ReviewableFileCount.Should().Be(1);
            result.ReviewableSizeBytes.Should().Be(10);
            result.RecentFileCount.Should().Be(1);
            result.AgeUnknownFileCount.Should().Be(0);
            result.ReviewAgeDays.Should().Be(30);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Nested_known_locations_are_scanned_once_without_double_counting_child_files()
    {
        var parent = Path.Combine(
            Path.GetTempPath(),
            "omnix-cleanup-overlap-" + Guid.NewGuid().ToString("N"));
        var child = Path.Combine(parent, "browser-cache");
        Directory.CreateDirectory(child);

        try
        {
            var parentFile = Path.Combine(parent, "parent.cache");
            var childFile = Path.Combine(child, "child.cache");
            File.WriteAllBytes(parentFile, new byte[20]);
            File.WriteAllBytes(childFile, new byte[100]);
            File.SetLastWriteTimeUtc(parentFile, DateTime.UtcNow.AddDays(-20));
            File.SetLastWriteTimeUtc(childFile, DateTime.UtcNow.AddDays(-20));

            var result = new KnownCleanupStoreProbe().Probe(
            [
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", parent, 7),
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", child, 7)
            ]).Single();

            result.LocationCount.Should().Be(2);
            result.FileCount.Should().Be(2);
            result.SizeBytes.Should().Be(120);
            result.ReviewableSizeBytes.Should().Be(120);
        }
        finally
        {
            Directory.Delete(parent, recursive: true);
        }
    }

    [Fact]
    public void Aggregated_age_label_uses_the_lowest_threshold_that_every_candidate_satisfies()
    {
        var first = Path.Combine(Path.GetTempPath(), "omnix-cleanup-age-a-" + Guid.NewGuid().ToString("N"));
        var second = Path.Combine(Path.GetTempPath(), "omnix-cleanup-age-b-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(first);
        Directory.CreateDirectory(second);

        try
        {
            var firstFile = Path.Combine(first, "old.cache");
            var secondFile = Path.Combine(second, "older.cache");
            File.WriteAllBytes(firstFile, new byte[10]);
            File.WriteAllBytes(secondFile, new byte[10]);
            File.SetLastWriteTimeUtc(firstFile, DateTime.UtcNow.AddDays(-10));
            File.SetLastWriteTimeUtc(secondFile, DateTime.UtcNow.AddDays(-40));

            var result = new KnownCleanupStoreProbe().Probe(
            [
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", first, 7),
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", second, 30)
            ]).Single();

            result.ReviewAgeDays.Should().Be(7);
            result.ReviewableFileCount.Should().Be(2);
        }
        finally
        {
            Directory.Delete(first, recursive: true);
            Directory.Delete(second, recursive: true);
        }
    }

    [Fact]
    public void Mixed_unfiltered_and_age_filtered_locations_keep_only_each_locations_reviewable_bytes()
    {
        var unfiltered = Path.Combine(Path.GetTempPath(), "omnix-cleanup-mixed-all-" + Guid.NewGuid().ToString("N"));
        var ageFiltered = Path.Combine(Path.GetTempPath(), "omnix-cleanup-mixed-old-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(unfiltered);
        Directory.CreateDirectory(ageFiltered);

        try
        {
            var unfilteredFile = Path.Combine(unfiltered, "all.cache");
            var oldFile = Path.Combine(ageFiltered, "old.cache");
            var recentFile = Path.Combine(ageFiltered, "recent.cache");
            File.WriteAllBytes(unfilteredFile, new byte[10]);
            File.WriteAllBytes(oldFile, new byte[20]);
            File.WriteAllBytes(recentFile, new byte[30]);
            File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow.AddDays(-20));
            File.SetLastWriteTimeUtc(recentFile, DateTime.UtcNow.AddDays(-1));

            var opportunity = new KnownCleanupStoreProbe().Probe(
            [
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", unfiltered, 0),
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", ageFiltered, 7)
            ]).Single();

            opportunity.SizeBytes.Should().Be(60);
            opportunity.ReviewableSizeBytes.Should().Be(30);
            opportunity.ReviewableFileCount.Should().Be(2);
            opportunity.RecentFileCount.Should().Be(1);
            opportunity.ReviewAgeDays.Should().Be(0);
            opportunity.HasAgeFilteredLocations.Should().BeTrue();

            var result = new DriveScanResult
            {
                Drive = @"C:\",
                TotalBytes = 100L * 1024 * 1024 * 1024,
                FreeBytes = 10L * 1024 * 1024 * 1024,
                SystemCleanupOpportunities = [opportunity]
            };
            var card = CDriveRootCauseSummaryBuilder.Build(result).Cards
                .Single(item => item.AutomationId == "CDriveRootCauseCard_SystemCleanup_BrowserCache");
            var summary = HealthCheckSummaryBuilder.Build(result, []);

            card.PrimaryText.Should().Contain("可先复核的文件").And.Contain("30 B");
            card.Explanation.Should().Contain("不同的复核条件")
                .And.Contain("不是整个目录里的文件都是垃圾");
            summary.Dimensions.Single(item => item.Name == "磁盘健康").Result
                .Should().Contain("30 B").And.NotContain("60 B");
            summary.KeyFindings.Should().Contain(item =>
                item.Text.Contains("不同位置按各自条件筛选")
                && item.Text.Contains("30 B"));
        }
        finally
        {
            Directory.Delete(unfiltered, recursive: true);
            Directory.Delete(ageFiltered, recursive: true);
        }
    }

    [Fact]
    public void Chromium_profile_discovery_is_bounded_to_recognized_non_reparse_profiles()
    {
        var userData = Path.Combine(
            Path.GetTempPath(),
            "omnix-chromium-profiles-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(userData, "Profile 2"));
        Directory.CreateDirectory(Path.Combine(userData, "Profile 1"));
        Directory.CreateDirectory(Path.Combine(userData, "Guest Profile"));
        Directory.CreateDirectory(Path.Combine(userData, "Random Folder"));

        try
        {
            var caches = KnownCleanupStoreProbe.DiscoverChromiumProfileCacheLocations(userData);

            caches.Should().HaveCount(4);
            caches.Should().Contain(Path.Combine(userData, "Default", "Cache"));
            caches.Should().Contain(Path.Combine(userData, "Profile 1", "Cache"));
            caches.Should().Contain(Path.Combine(userData, "Profile 2", "Cache"));
            caches.Should().Contain(Path.Combine(userData, "Guest Profile", "Cache"));
            caches.Should().NotContain(path => path.Contains("Random Folder"));
        }
        finally
        {
            Directory.Delete(userData, recursive: true);
        }
    }

    [Fact]
    public void Default_locations_cover_missing_cleanup_categories_without_broad_download_recursion()
    {
        var locations = KnownCleanupStoreProbe.ResolveDefaultLocations();

        var installers = locations.Single(item =>
            item.Kind == KnownCleanupStoreKind.InstallerDownloadRemnants);
        installers.RecurseSubdirectories.Should().BeFalse();
        installers.ReviewAgeDays.Should().BeGreaterThanOrEqualTo(30);
        installers.IncludedExtensions.Should().Contain(".exe").And.Contain(".msi");

        locations.Should().Contain(item => item.Kind == KnownCleanupStoreKind.BrowserCache);
        locations.Should().Contain(item => item.Kind == KnownCleanupStoreKind.DeveloperToolCache);
        locations.Should().Contain(item => item.Kind == KnownCleanupStoreKind.WindowsDiagnosticReports);
        locations.Where(item => item.Kind == KnownCleanupStoreKind.BrowserCache)
            .Should().OnlyContain(item =>
                item.Path.Contains("Cache", StringComparison.OrdinalIgnoreCase)
                && !item.Path.EndsWith("User Data", StringComparison.OrdinalIgnoreCase));
        locations.Where(item => item.Kind == KnownCleanupStoreKind.DeveloperToolCache)
            .Should().NotContain(item =>
                item.Path.EndsWith(
                    Path.Combine(".nuget", "packages"),
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Bounded_probe_reports_honest_lower_bound_and_handling_authority()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "omnix-system-cleanup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            File.WriteAllBytes(Path.Combine(root, "one.tmp"), new byte[16]);
            File.WriteAllBytes(Path.Combine(root, "two.tmp"), new byte[32]);
            File.WriteAllBytes(Path.Combine(root, "three.tmp"), new byte[64]);

            var result = new KnownCleanupStoreProbe(maxFilesPerLocation: 2).Probe(
            [
                new KnownCleanupStoreLocation(
                    KnownCleanupStoreKind.UserTemporaryFiles,
                    "用户临时文件",
                    root,
                    CleanupHandling.UserReview)
            ]).Single();

            result.IsAccessible.Should().BeTrue();
            result.FileCount.Should().Be(2);
            result.SizeBytes.Should().BeOneOf(48L, 80L, 96L);
            result.IsSizeLowerBound.Should().BeTrue();
            result.Handling.Should().Be(CleanupHandling.UserReview);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Directory_limit_is_also_reported_as_a_lower_bound()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "omnix-system-cleanup-dirs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "one"));
        Directory.CreateDirectory(Path.Combine(root, "two"));
        Directory.CreateDirectory(Path.Combine(root, "three"));

        try
        {
            var result = new KnownCleanupStoreProbe(
                maxFilesPerLocation: 100,
                maxDirectoriesPerLocation: 2)
                .Probe(
                [
                    new KnownCleanupStoreLocation(
                        KnownCleanupStoreKind.UserTemporaryFiles,
                        "用户临时文件",
                        root,
                        CleanupHandling.UserReview)
                ])
                .Single();

            result.IsAccessible.Should().BeTrue();
            result.IsSizeLowerBound.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Root_cause_summary_distinguishes_user_candidates_from_windows_managed_stores()
    {
        var result = new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 10L * 1024 * 1024 * 1024,
            SystemCleanupOpportunities =
            [
                new SystemCleanupOpportunity
                {
                    Kind = KnownCleanupStoreKind.UserTemporaryFiles,
                    Title = "用户临时文件",
                    Path = @"C:\Users\Me\AppData\Local\Temp",
                    Handling = CleanupHandling.UserReview,
                    SizeBytes = 4L * 1024 * 1024 * 1024,
                    FileCount = 20_000,
                    ReviewableSizeBytes = 1L * 1024 * 1024 * 1024,
                    ReviewableFileCount = 1_000,
                    RecentFileCount = 19_000,
                    ReviewAgeDays = 7,
                    IsSizeLowerBound = true,
                    IsAccessible = true
                },
                new SystemCleanupOpportunity
                {
                    Kind = KnownCleanupStoreKind.WindowsUpdateDownloads,
                    Title = "Windows 更新下载缓存",
                    Path = @"C:\Windows\SoftwareDistribution\Download",
                    Handling = CleanupHandling.WindowsManaged,
                    SizeBytes = 600L * 1024 * 1024,
                    FileCount = 400,
                    IsAccessible = true
                }
            ]
        };

        var cards = CDriveRootCauseSummaryBuilder.Build(result).Cards;

        var userCard = cards.Single(card => card.PrimaryText.Contains("用户临时文件"));
        userCard.Title.Should().Be("OMNIX 先筛选");
        userCard.PrimaryText.Should().Contain("旧文件").And.Contain("1.0 GB");
        userCard.Explanation.Should().Contain("近期文件")
            .And.Contain("不是整个目录里的文件都是垃圾")
            .And.NotContain("可以全部删除");
        userCard.AgentSuggestion.Should().Contain("OMNIX").And.Contain("隔离方案");
        userCard.AutomationId.Should().Be("CDriveRootCauseCard_SystemCleanup_UserTemporaryFiles");

        var windowsCard = cards.Single(card => card.PrimaryText.Contains("Windows 更新下载缓存"));
        windowsCard.Title.Should().Be("交给 Windows 清理");
        windowsCard.Explanation.Should().Contain("Windows");
        windowsCard.Action.Should().Be(CDriveRootCauseAction.OpenStorageSettings);
        windowsCard.ActionLabel.Should().Be("打开 Windows 存储设置");
        windowsCard.AgentSuggestion.Should().Contain("Windows 处理");
    }

    [Fact]
    public void Multiple_cache_locations_are_aggregated_by_kind_without_exposing_paths()
    {
        var first = Path.Combine(Path.GetTempPath(), "omnix-browser-a-" + Guid.NewGuid().ToString("N"));
        var second = Path.Combine(Path.GetTempPath(), "omnix-browser-b-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(first);
        Directory.CreateDirectory(second);

        try
        {
            var firstFile = Path.Combine(first, "old.cache");
            var secondFile = Path.Combine(second, "old.cache");
            File.WriteAllBytes(firstFile, new byte[11]);
            File.WriteAllBytes(secondFile, new byte[13]);
            File.SetLastWriteTimeUtc(firstFile, DateTime.UtcNow.AddDays(-20));
            File.SetLastWriteTimeUtc(secondFile, DateTime.UtcNow.AddDays(-20));

            var result = new KnownCleanupStoreProbe().Probe(
            [
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", first, 7),
                Location(KnownCleanupStoreKind.BrowserCache, "浏览器缓存", second, 7)
            ]).Single();

            result.LocationCount.Should().Be(2);
            result.Path.Should().BeEmpty();
            result.SizeBytes.Should().Be(24);
            result.ReviewableSizeBytes.Should().Be(24);
        }
        finally
        {
            Directory.Delete(first, recursive: true);
            Directory.Delete(second, recursive: true);
        }
    }

    [Fact]
    public void Home_summary_separates_safe_cleanup_from_known_store_candidates()
    {
        var result = new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 10L * 1024 * 1024 * 1024,
            SystemCleanupOpportunities =
            [
                new SystemCleanupOpportunity
                {
                    Kind = KnownCleanupStoreKind.UserTemporaryFiles,
                    Title = "用户临时文件",
                    Path = @"C:\Users\Me\AppData\Local\Temp",
                    Handling = CleanupHandling.UserReview,
                    SizeBytes = 4L * 1024 * 1024 * 1024,
                    FileCount = 20_000,
                    IsSizeLowerBound = true,
                    IsAccessible = true
                }
            ]
        };

        var summary = HealthCheckSummaryBuilder.Build(result, []);

        summary.Dimensions.Single(item => item.Name == "磁盘健康").Result
            .Should().Contain("可安全处理约 0 B")
            .And.Contain("待确认临时/系统缓存至少 4.0 GB");
        summary.KeyFindings.Should().Contain(item =>
            item.Text.Contains("用户临时文件")
            && item.Text.Contains("至少 4.0 GB")
            && item.Action == Css.Core.Recommendations.RecommendationAction.Observe);
    }

    [Fact]
    public void Home_summary_counts_only_age_reviewable_user_files_but_keeps_windows_totals()
    {
        var result = new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 10L * 1024 * 1024 * 1024,
            SystemCleanupOpportunities =
            [
                new SystemCleanupOpportunity
                {
                    Kind = KnownCleanupStoreKind.UserTemporaryFiles,
                    Title = "用户临时文件",
                    Path = @"C:\Users\Me\AppData\Local\Temp",
                    Handling = CleanupHandling.UserReview,
                    SizeBytes = 4L * 1024 * 1024 * 1024,
                    FileCount = 20_000,
                    ReviewableSizeBytes = 1L * 1024 * 1024 * 1024,
                    ReviewableFileCount = 1_000,
                    RecentFileCount = 19_000,
                    ReviewAgeDays = 7,
                    IsAccessible = true
                },
                new SystemCleanupOpportunity
                {
                    Kind = KnownCleanupStoreKind.WindowsUpdateDownloads,
                    Title = "Windows 更新下载缓存",
                    Path = @"C:\Windows\SoftwareDistribution\Download",
                    Handling = CleanupHandling.WindowsManaged,
                    SizeBytes = 600L * 1024 * 1024,
                    FileCount = 400,
                    IsAccessible = true
                }
            ]
        };

        var summary = HealthCheckSummaryBuilder.Build(result, []);
        var disk = summary.Dimensions.Single(item => item.Name == "磁盘健康");

        disk.Result.Should().Contain("1.6 GB").And.NotContain("4.6 GB");
        summary.KeyFindings.Should().Contain(item =>
            item.Text.Contains("7 天以上旧文件")
            && item.Text.Contains("1.0 GB")
            && item.Text.Contains("OMNIX"));
    }

    [Fact]
    public void Incomplete_zero_candidate_scan_is_visible_without_claiming_nothing_was_found()
    {
        var result = new DriveScanResult
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 10L * 1024 * 1024 * 1024,
            SystemCleanupOpportunities =
            [
                new SystemCleanupOpportunity
                {
                    Kind = KnownCleanupStoreKind.BrowserCache,
                    Title = "浏览器缓存",
                    Path = @"C:\Users\Me\AppData\Local\Browser\Cache",
                    Handling = CleanupHandling.UserReview,
                    SizeBytes = 200,
                    FileCount = 20_000,
                    ReviewableSizeBytes = 0,
                    ReviewableFileCount = 0,
                    RecentFileCount = 20_000,
                    ReviewAgeDays = 7,
                    IsSizeLowerBound = true,
                    IsAccessible = true
                }
            ]
        };

        var card = CDriveRootCauseSummaryBuilder.Build(result).Cards.Single();
        card.Title.Should().Be("扫描还没看完");
        card.PrimaryText.Should().Contain("暂时不能判断");
        card.AgentSuggestion.Should().Contain("先保留");

        var summary = HealthCheckSummaryBuilder.Build(result, []);
        summary.Dimensions.Single(item => item.Name == "磁盘健康").Result
            .Should().Contain("扫描未完成").And.Contain("可能低估");
        summary.KeyFindings.Should().Contain(item =>
            item.Text.Contains("当前不能判断这里没有旧文件"));
    }

    [Fact]
    public void Root_cause_list_wraps_cards_instead_of_requiring_horizontal_scroll()
    {
        var xaml = File.ReadAllText(
            FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var start = xaml.IndexOf(
            "<ListBox x:Name=\"CDriveRootCauseListBox\"",
            StringComparison.Ordinal);
        var end = xaml.IndexOf("</ListBox>", start, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        end.Should().BeGreaterThan(start);

        var list = xaml[start..end];
        list.Should().Contain("HorizontalContentAlignment=\"Stretch\"")
            .And.Contain("AutomationProperties.AutomationId\" Value=\"{Binding AutomationId}\"")
            .And.Contain("AutomationProperties.Name\" Value=\"{Binding PrimaryText}\"")
            .And.Contain("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"")
            .And.Contain("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"");
    }

    [Fact]
    public void Known_cleanup_probe_remains_read_only_and_has_no_operation_authority()
    {
        var source = File.ReadAllText(
            FindRepositoryFile("src", "Css.Scanner", "Disk", "SystemCleanupOpportunity.cs"));

        source.Should().NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("File.Move")
            .And.NotContain("Directory.Move")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry.SetValue")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("SafetyOperationPipeline");
    }

    [Fact]
    public void Home_gui_smoke_proves_age_handler_scope_and_zero_operation()
    {
        var smoke = File.ReadAllText(
            FindRepositoryFile(".omx", "gui-home-agent-next-action-smoke.ps1"));

        smoke.Should().Contain("CDriveRootCauseCard_SystemCleanup_UserTemporaryFiles")
            .And.Contain("OMNIX-old-temp-")
            .And.Contain("SetLastWriteTimeUtc")
            .And.Contain("initialSystemCleanupVisible = $true")
            .And.Contain("was not visible in the initial working area")
            .And.Contain("OMNIX_ENTROPY_QUARANTINE_ROOT")
            .And.Contain("quarantineManifestCount")
            .And.Contain("noOperationExecuted = ($quarantineManifestCount -eq 0)")
            .And.Contain("systemCleanupScreenshot = $systemCleanupScreenshotPath");
        smoke.Should().NotContain("File.Delete")
            .And.NotContain("Directory.Delete")
            .And.NotContain("SafetyOperationPipeline");
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

        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(segments));
    }

    private static KnownCleanupStoreLocation Location(
        KnownCleanupStoreKind kind,
        string title,
        string path,
        int reviewAgeDays) =>
        new(kind, title, path, CleanupHandling.UserReview)
        {
            ReviewAgeDays = reviewAgeDays
        };
}
