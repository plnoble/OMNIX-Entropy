using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class SystemCleanupOpportunityTests
{
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
        userCard.Title.Should().Be("可评估清理");
        userCard.PrimaryText.Should().Contain("至少");
        userCard.AgentSuggestion.Should().Contain("先检查");

        var windowsCard = cards.Single(card => card.PrimaryText.Contains("Windows 更新下载缓存"));
        windowsCard.Title.Should().Be("交给 Windows 清理");
        windowsCard.Explanation.Should().Contain("Windows");
        windowsCard.Action.Should().Be(CDriveRootCauseAction.None);
        windowsCard.AgentSuggestion.Should().Contain("不会自行处理");
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
            .And.Contain("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"")
            .And.Contain("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"");
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
}
