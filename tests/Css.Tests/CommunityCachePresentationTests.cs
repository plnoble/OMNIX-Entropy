using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class CommunityCachePresentationTests
{
    [Fact]
    public void Rule_only_cache_becomes_a_plain_language_observation_not_a_cleanup_action()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            InstallPath = @"D:\Software\Example\Install",
            CommunityCacheEvidence =
            [
                new CommunityRuleCacheEvidence
                {
                    RuleName = "Example Cache",
                    RulePackSource = "Community rules",
                    RulePackVersion = "2026-08",
                    RulePackSha256 = new string('A', 64),
                    FileCount = 40,
                    SizeBytes = 1536L * 1024 * 1024,
                    StaleFileCount = 30,
                    StaleSizeBytes = 1229L * 1024 * 1024,
                    IsSizeLowerBound = true,
                    Warning = "Signing in again may be required.",
                    SamplePaths = [@"C:\Users\Fixture\Private\Cache\one.bin"]
                }
            ]
        };

        var tile = AppPresentationBuilder.CreateTile(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var cacheAction = drawer.AvailableActions.Single(action => action.Kind == AppActionKind.CacheCleanup);
        var beginnerText = string.Join(
            "\n",
            tile.VisibleText,
            drawer.SizeSummary,
            drawer.CommunityCacheSummary,
            drawer.AgentAdvice.Text,
            drawer.AgentAdvice.Reason,
            cacheAction.Reason);

        tile.Status.Should().Be(AppTileStatus.Warning);
        tile.ShortTag.Should().Be("发现缓存");
        drawer.CommunityCacheSummary.Should().Contain("至少 1.5 GB")
            .And.Contain("30 天以上")
            .And.Contain("1.2 GB")
            .And.Contain("只读发现");
        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Observe);
        drawer.AgentAdvice.Text.Should().Contain("先预览");
        cacheAction.IsEnabled.Should().BeFalse("rule matches are not approved cleanup paths");
        cacheAction.Reason.Should().Contain("还不能直接清理");
        beginnerText.Should().NotContain(@"C:\")
            .And.NotContain("SHA-256")
            .And.NotContain(new string('A', 64));
        drawer.TechnicalDetails.Should().Contain(line => line.Contains("Community rule sample:") && line.Contains(@"C:\"));
        drawer.TechnicalDetails.Should().Contain(line => line.Contains("Community rule pack:") && line.Contains("2026-08"));
    }

    [Fact]
    public void Multiple_rule_sizes_use_the_largest_observation_instead_of_an_overlap_sum()
    {
        var profile = new SoftwareProfile
        {
            Name = "Overlapping App",
            CommunityCacheEvidence =
            [
                Evidence("Broad Cache", 2L * 1024 * 1024 * 1024),
                Evidence("Nested Cache", 1536L * 1024 * 1024)
            ]
        };

        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        drawer.CommunityCacheSummary.Should().Contain("至少 2.0 GB")
            .And.Contain("2 条规则")
            .And.NotContain("3.5 GB");
    }

    [Fact]
    public void Candidate_assessment_tells_a_beginner_whether_the_agent_advanced_or_stopped()
    {
        var eligibleFile = new CommunityRuleFileEvidence
        {
            Path = @"C:\Users\Fixture\AppData\Local\Example\Cache\old.tmp",
            SizeBytes = 2048,
            LastWriteTimeUtc = DateTimeOffset.UtcNow.AddDays(-60),
            Attributes = FileAttributes.Normal
        };
        var eligible = Evidence("Eligible Cache", 2048, new CommunityRuleCandidateAssessment
        {
            Disposition = CommunityRuleCandidateDisposition.EligibleForSafePreview,
            Summary = "1 个旧缓存文件通过第一轮筛选，可进入安全预演。",
            Explanation = "仍需重新核验并准备隔离回滚。",
            Reasons = [CommunityRuleCandidateReason.EligibleStaleCacheFiles],
            EligibleFiles = [eligibleFile],
            EligibleBytes = 2048
        });
        var refused = Evidence("Refused Cache", 1024, new CommunityRuleCandidateAssessment
        {
            Disposition = CommunityRuleCandidateDisposition.Refused,
            Summary = "文件位于应用安装目录，已拒绝。",
            Explanation = "主程序文件不能因为目录名像缓存就进入清理候选。",
            Reasons = [CommunityRuleCandidateReason.InsideInstallLocation],
            EligibleFiles = []
        });

        var eligibleDrawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Eligible App",
            CommunityCacheEvidence = [eligible]
        });
        var refusedDrawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Refused App",
            CommunityCacheEvidence = [refused]
        });
        var eligibleAction = eligibleDrawer.AvailableActions.Single(action => action.Kind == AppActionKind.CacheCleanup);

        eligibleDrawer.CommunityCacheSummary.Should().Contain("通过第一轮筛选")
            .And.Contain("安全预演")
            .And.Contain("暂不能直接清理");
        eligibleDrawer.AgentAdvice.Text.Should().Contain("1 个旧缓存文件")
            .And.Contain("目前不会清理");
        eligibleAction.IsEnabled.Should().BeTrue();
        eligibleAction.Reason.Should().Contain("安全预演")
            .And.Contain("隔离回滚");
        refusedDrawer.CommunityCacheSummary.Should().Contain("已拒绝晋级");
        refusedDrawer.AgentAdvice.Text.Should().Contain("已停止晋级")
            .And.Contain("只保留查看");
    }

    [Fact]
    public void App_drawer_places_the_automation_backed_cache_conclusion_before_actions_and_details()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"DrawerCommunityCacheSummaryTextBlock\"");
        xaml.IndexOf("DrawerAdviceTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("DrawerCommunityCacheSummaryTextBlock", StringComparison.Ordinal));
        xaml.IndexOf("DrawerAdviceTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("DrawerCleanCacheButton", StringComparison.Ordinal));
        xaml.IndexOf("DrawerCommunityCacheSummaryTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("DrawerCleanCacheButton", StringComparison.Ordinal));
        code.Should().Contain("DrawerCommunityCacheSummaryTextBlock.Text = drawer.CommunityCacheSummary");
        code.Should().Contain("Winapp2SoftwareProfileEnricher");
        code.Should().Contain("LoadActiveCatalog");
    }

    private static CommunityRuleCacheEvidence Evidence(
        string rule,
        long bytes,
        CommunityRuleCandidateAssessment? assessment = null) =>
        new()
        {
            RuleName = rule,
            RulePackSource = "Fixture",
            RulePackVersion = "1",
            RulePackSha256 = new string('B', 64),
            FileCount = 1,
            SizeBytes = bytes,
            CandidateAssessment = assessment
        };

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
