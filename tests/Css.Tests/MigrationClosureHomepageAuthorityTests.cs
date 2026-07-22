using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Recommendations;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationClosureHomepageAuthorityTests
{
    [Fact]
    public void Homepage_findings_separate_reviewable_protected_and_unavailable_closure_targets()
    {
        var health = BaseHealth();
        var summaries = new[]
        {
            Closure("Ordinary App"),
            Closure("Windows Component"),
            Closure("Ambiguous App")
        };

        var enriched = MigrationClosureHealthEnricher.Apply(
            health,
            summaries,
            name => name switch
            {
                "Ordinary App" => MigrationClosureTargetDisposition.Reviewable,
                "Windows Component" => MigrationClosureTargetDisposition.ProtectedHistorical,
                _ => MigrationClosureTargetDisposition.Unavailable
            });

        var dimension = enriched.Dimensions.Single(item => item.Name == "迁移闭环");
        dimension.Result.Should().Contain("1 个普通应用需要复查")
            .And.Contain("2 条旧记录仅供查看");
        dimension.Rating.Should().Be("需关注");

        var findings = enriched.KeyFindings
            .Where(item => item.Kind == HealthFindingKind.MigrationClosure)
            .ToList();
        findings.Should().HaveCount(3);

        var reviewable = findings.Single(item => item.TargetAppName == "Ordinary App");
        var protectedHistorical = findings.Single(item =>
            item.Text.Contains("系统相关旧迁移记录", StringComparison.Ordinal));
        var unavailable = findings.Single(item =>
            item.Text.Contains("无法唯一对应应用", StringComparison.Ordinal));

        findings[0].Should().BeSameAs(reviewable);
        reviewable.Action.Should().Be(RecommendationAction.Migrate);
        reviewable.Text.Should().NotContain("仅供查看");

        protectedHistorical.Action.Should().Be(RecommendationAction.Observe);
        protectedHistorical.TargetAppName.Should().BeNull();
        protectedHistorical.Text.Should().Contain("系统相关旧迁移记录")
            .And.Contain("仅供查看");

        unavailable.Action.Should().Be(RecommendationAction.Observe);
        unavailable.TargetAppName.Should().BeNull();
        unavailable.Text.Should().Contain("无法唯一对应应用")
            .And.Contain("仅供查看");

        string.Join("\n", findings.Select(item => item.Text))
            .Should().NotContain(@"C:\")
            .And.NotContain(@"D:\");
    }

    [Fact]
    public void Read_only_closure_finding_uses_observation_copy_and_c_drive_app_navigation()
    {
        var finding = new HealthFinding
        {
            Text = "系统相关旧迁移记录，仅供查看：迁移状态需要检查。",
            Kind = HealthFindingKind.MigrationClosure,
            Action = RecommendationAction.Observe,
            Risk = RiskLevel.Medium
        };

        var explanation = HealthFindingAgentExplanationBuilder.Create(finding);
        var explainResponse = HomeAgentResponsePresenter.Explain(finding);
        var detailResponse = HomeAgentResponsePresenter.ShowDetails(finding);
        var planResponse = HomeAgentResponsePresenter.CreatePlan(finding);
        var filterProperty = typeof(HomeAgentResponseViewModel).GetProperty("TargetAppFilter");

        filterProperty.Should().NotBeNull();

        explanation.RecommendedNextStep.Should().Contain("仅供查看")
            .And.Contain("应用管理")
            .And.NotContain("生成新的快照和迁移方案");
        explanation.NextSteps.Should().Contain(step => step.Contains("确认系统归属"));
        explanation.NextSteps.Should().NotContain(step => step.Contains("生成新的快照和迁移方案"));
        planResponse.Body.Should().Contain("不生成迁移动作")
            .And.NotContain("可以准备迁移方案");

        new[] { explainResponse, detailResponse, planResponse }
            .Should().OnlyContain(response =>
                response.NavigationDestination == HomeAgentNavigationDestination.Applications
                && response.NavigationLabel == "打开应用管理"
                && response.TargetAppName == null
                && response.CanNavigate
                && !response.CanExecuteDirectly);
        foreach (var response in new[] { explainResponse, detailResponse, planResponse })
            filterProperty!.GetValue(response).Should().Be(AppCatalogFilter.CDrive);
    }

    [Fact]
    public void Reviewable_closure_finding_keeps_exact_app_navigation_and_plan_review_copy()
    {
        var finding = new HealthFinding
        {
            Text = "Ordinary App 的迁移没有闭环。",
            Kind = HealthFindingKind.MigrationClosure,
            TargetAppName = "Ordinary App",
            Action = RecommendationAction.Migrate,
            Risk = RiskLevel.Medium
        };

        var explanation = HealthFindingAgentExplanationBuilder.Create(finding);
        var response = HomeAgentResponsePresenter.CreatePlan(finding);
        var filterProperty = typeof(HomeAgentResponseViewModel).GetProperty("TargetAppFilter");

        explanation.RecommendedNextStep.Should().Contain("对应应用详情")
            .And.Contain("新快照和回滚方案");
        response.NavigationDestination.Should().Be(HomeAgentNavigationDestination.Applications);
        response.NavigationLabel.Should().Be("打开对应应用");
        response.TargetAppName.Should().Be("Ordinary App");
        filterProperty.Should().NotBeNull();
        filterProperty!.GetValue(response).Should().BeNull();
        response.Body.Should().Contain("迁移方案");
        response.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Main_window_resolves_closure_target_with_current_profile_authority()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var refresh = ExtractMethod(
            code,
            "private HealthCheckSummary? RefreshHealthSummaryFromBase",
            "private async Task<bool> TrySaveHealthDigestAsync");
        var resolver = ExtractMethod(
            code,
            "private MigrationClosureTargetDisposition ResolveMigrationClosureTargetDisposition",
            "private bool HasUniqueSoftwareName");

        refresh.Should().Contain("ResolveMigrationClosureTargetDisposition");
        resolver.Should().Contain("AppDrawerTargetResolver.Resolve(softwareName, _softwareProfiles)")
            .And.Contain("if (!resolution.CanOpen || resolution.Profile is null)")
            .And.Contain("MigrationClosureTargetDisposition.Unavailable")
            .And.Contain("AppPresentationBuilder.CanReviewMigrationClosure(resolution.Profile)")
            .And.Contain("MigrationClosureTargetDisposition.Reviewable")
            .And.Contain("MigrationClosureTargetDisposition.ProtectedHistorical");
    }

    [Fact]
    public void Home_navigation_uses_only_the_bounded_catalog_handoff_for_aggregate_closure()
    {
        var code = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var handler = SourceMethodExtractor.Extract(
            code,
            "private async void HomeAgentResponseNavigate_Click(object sender, RoutedEventArgs e)");

        handler.Should().Contain("response.TargetAppFilter is { } appFilter")
            .And.Contain("HomeAgentNavigationDestination.Applications")
            .And.Contain("await OpenAgentAppCatalogFilterAsync(appFilter)")
            .And.NotContain("ShowMigrationPlanAsync")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("Process.Start")
            .And.NotContain("Registry")
            .And.NotContain("ServiceController");
    }

    private static HealthCheckSummary BaseHealth() =>
        new()
        {
            OverallScore = 80,
            Dimensions = [new HealthDimensionResult { Name = "磁盘健康", Result = "正常", Rating = "正常" }],
            KeyFindings = []
        };

    private static MigrationClosureSummaryViewModel Closure(string softwareName) =>
        new()
        {
            SoftwareName = softwareName,
            DisplayName = softwareName,
            TargetAppNameCandidate = softwareName,
            State = MigrationClosureFindingKind.OriginalWriteReturned,
            Headline = softwareName + " 的迁移没有闭环",
            Detail = "它可能继续写入原位置。",
            ObservedPathCount = 1,
            MonitoringStartedAtUtc = DateTimeOffset.UtcNow
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

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }
}
