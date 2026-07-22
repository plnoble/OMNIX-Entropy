using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentApplicationGrowthAdviceTests
{
    [Fact]
    public void Growth_observation_is_aggregate_path_free_and_non_executable()
    {
        var propertyNames = typeof(ApplicationGrowthObservation)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        propertyNames.Should().Contain([
            "Availability",
            "SoftwareName",
            "ObservedSnapshotCount",
            "RecentGrowthBytes",
            "CDriveWriteLocationCount",
            "CacheLocationCount",
            "CanExecuteDirectly"
        ]);
        propertyNames.Should().NotContain(name =>
            name.Contains("Path", StringComparison.OrdinalIgnoreCase)
            || name.Contains("File", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Operation", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Registry", StringComparison.OrdinalIgnoreCase));
        Observation(ApplicationGrowthObservationAvailability.Available)
            .CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Exact_unique_app_growth_question_selects_one_read_only_target()
    {
        var profile = Profile("微信");

        AgentConversationPresenter.ApplicationGrowthObservationTarget(
                "微信为什么越来越大",
                [profile])
            .Should().BeSameAs(profile);
        AgentConversationPresenter.ApplicationGrowthObservationTarget(
                "微信为什么还在写 C 盘",
                [profile])
            .Should().BeSameAs(profile);

        AgentConversationPresenter.ApplicationGrowthObservationTarget(
                "哪些软件增长最快",
                [profile])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationGrowthObservationTarget(
                "清理微信缓存，因为它越来越大",
                [profile])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationGrowthObservationTarget(
                "把微信迁移到 D 盘，别再增长",
                [profile])
            .Should().BeNull();
        AgentConversationPresenter.ApplicationGrowthObservationTarget(
                "微信为什么越来越大",
                [profile, Profile("微信")])
            .Should().BeNull();
    }

    [Fact]
    public void Available_growth_separates_immediate_relief_from_prevention_without_paths()
    {
        var profile = Profile("微信", 1536L * 1024 * 1024);
        var observation = Observation(
            ApplicationGrowthObservationAvailability.Available,
            growthBytes: profile.RecentGrowthBytes,
            snapshotCount: 4);

        var reply = AgentConversationPresenter.Answer(
            "微信为什么越来越大",
            null,
            [profile],
            applicationGrowthObservation: observation);

        reply.Headline.Should().Contain("微信").And.Contain("增长");
        reply.Answer.Should().Contain("1.5 GB").And.Contain("不能单独证明");
        reply.EvidenceLines.Should().Contain(line => line.Contains("4 次") && line.Contains("1.5 GB"));
        reply.EvidenceLines.Should().Contain(line => line.Contains("C 盘写入线索") && line.Contains("2"));
        reply.NextSteps.Should().Contain(step => step.StartsWith("现在腾空间："));
        reply.NextSteps.Should().Contain(step => step.StartsWith("以后防止继续增长："));
        reply.TargetAppHandoff.Should().Be(AgentApplicationHandoff.Details);
        reply.NavigationLabel.Should().Be("查看增长详情");
        reply.CanExecuteDirectly.Should().BeFalse();
        Visible(reply).Should().NotContain(@"C:\Users\Fixture")
            .And.NotContain(@"D:\Software\Wechat");
    }

    [Fact]
    public void One_snapshot_is_a_baseline_not_a_growth_claim()
    {
        var reply = AgentConversationPresenter.Answer(
            "微信为什么越来越大",
            null,
            [Profile("微信")],
            applicationGrowthObservation: Observation(
                ApplicationGrowthObservationAvailability.InsufficientBaseline,
                snapshotCount: 1));

        reply.Answer.Should().Contain("只有 1 次").And.Contain("不能判断");
        reply.EvidenceLines.Should().Contain(line => line.Contains("基线"));
        reply.NextSteps.Should().Contain(step => step.StartsWith("现在腾空间："));
        reply.NextSteps.Should().Contain(step => step.StartsWith("以后防止继续增长："));
    }

    [Fact]
    public void Available_zero_growth_is_a_bounded_comparison_not_a_forever_stable_claim()
    {
        var reply = AgentConversationPresenter.Answer(
            "微信为什么还在写 C 盘",
            null,
            [Profile("微信")],
            applicationGrowthObservation: Observation(
                ApplicationGrowthObservationAvailability.Available,
                growthBytes: 0,
                snapshotCount: 3));

        reply.Answer.Should().Contain("最近一次对比没有确认到增长");
        reply.Answer.Should().Contain("不代表以后不会增长");
        reply.Answer.Should().NotContain("没有问题");
    }

    [Fact]
    public void Missing_or_mismatched_observation_remains_unknown()
    {
        var profile = Profile("微信");
        var unavailable = AgentConversationPresenter.Answer(
            "微信为什么越来越大",
            null,
            [profile],
            applicationGrowthObservation: Observation(
                ApplicationGrowthObservationAvailability.Unavailable));
        var mismatched = AgentConversationPresenter.Answer(
            "微信为什么越来越大",
            null,
            [profile],
            applicationGrowthObservation: Observation(
                ApplicationGrowthObservationAvailability.Available,
                softwareName: "QQ",
                growthBytes: 1024));

        unavailable.Answer.Should().Contain("没有形成可用的增长对比");
        mismatched.Answer.Should().Contain("没有形成可用的增长对比");
        unavailable.Answer.Should().NotContain("没有问题").And.NotContain("一切正常");
        mismatched.Answer.Should().NotContain("1.0 KB");
    }

    [Fact]
    public void MainWindow_loads_growth_evidence_after_inventory_and_re_resolves_target()
    {
        var main = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "MainWindow.xaml.cs"));
        var ask = Extract(
            main,
            "private async void AskComputerAgent_Click",
            "private void ApplyAgentConversationReply");

        ask.Should().Contain("ApplicationGrowthObservationTarget")
            .And.Contain("await EnsureHealthScanLoadedAsync()")
            .And.Contain("CreateApplicationGrowthObservation")
            .And.Contain("applicationGrowthObservation");
        ask.Split("ApplicationGrowthObservationTarget", StringSplitOptions.None)
            .Should().HaveCount(3, "the target must be resolved before and after an on-demand scan");
        ask.IndexOf("EnsureSoftwareInventoryLoadedAsync", StringComparison.Ordinal)
            .Should().BeLessThan(ask.IndexOf("ApplicationGrowthObservationTarget", StringComparison.Ordinal));
        ask.IndexOf("await EnsureHealthScanLoadedAsync()", StringComparison.Ordinal)
            .Should().BeLessThan(ask.LastIndexOf("ApplicationGrowthObservationTarget", StringComparison.Ordinal));
        ask.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("Process.Start")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Move")
            .And.NotContain("Directory.Delete");

        main.Should().Contain("private int _latestObservedSnapshotCount;");
        main.Should().Contain("_latestObservedSnapshotCount = observedSnapshotCount;");
    }

    private static SoftwareProfile Profile(string name, long recentGrowthBytes = 0) =>
        new()
        {
            Name = name,
            InstallPath = @"D:\Software\Wechat\Install",
            RecentGrowthBytes = recentGrowthBytes,
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Wechat\Cache"],
            CDriveWritePaths =
            [
                @"C:\Users\Fixture\AppData\Local\Wechat",
                @"C:\Users\Fixture\AppData\Roaming\Wechat"
            ]
        };

    private static ApplicationGrowthObservation Observation(
        ApplicationGrowthObservationAvailability availability,
        string softwareName = "微信",
        long growthBytes = 0,
        int snapshotCount = 0) =>
        new()
        {
            Availability = availability,
            SoftwareName = softwareName,
            ObservedSnapshotCount = snapshotCount,
            RecentGrowthBytes = growthBytes,
            CDriveWriteLocationCount = 2,
            CacheLocationCount = 1
        };

    private static string Visible(AgentConversationReply reply) =>
        string.Join("\n", new[] { reply.Headline, reply.Answer }
            .Concat(reply.EvidenceLines)
            .Concat(reply.NextSteps));

    private static string Extract(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static string FindRepositoryFile(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. parts]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException("Repository file was not found.", Path.Combine(parts));
    }
}
