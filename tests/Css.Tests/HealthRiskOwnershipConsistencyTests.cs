using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using FluentAssertions;

namespace Css.Tests;

public sealed class HealthRiskOwnershipConsistencyTests
{
    [Fact]
    public void C_drive_agent_counts_low_and_higher_risk_clean_findings_separately()
    {
        var health = HealthWithCleanFindings();

        var reply = AgentConversationPresenter.Answer("我的 C 盘为什么快满了", health, []);

        reply.EvidenceLines.Should().Contain(line =>
            line.Contains("低风险清理发现 1 项")
            && line.Contains("风险偏高清理提醒 1 项"));
        reply.NextSteps.Should().Contain(line => line.Contains("只选择低风险项目"));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void C_drive_agent_with_only_higher_risk_clean_advises_observation_not_cleanup()
    {
        var health = HealthWithCleanFindings();
        health = new HealthCheckSummary
        {
            OverallScore = health.OverallScore,
            Dimensions = health.Dimensions,
            KeyFindings = health.KeyFindings.Where(item => item.Risk == RiskLevel.High).ToArray()
        };

        var reply = AgentConversationPresenter.Answer("我的 C 盘为什么快满了", health, []);

        reply.Answer.Should().Contain("当前没有低风险清理候选")
            .And.Contain("只观察");
        reply.NextSteps.Should().Contain(line =>
            line.Contains("风险偏高")
            && line.Contains("不进入隔离处理"));
        reply.NextSteps.Should().NotContain(line => line.Contains("只选择低风险项目"));
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Stored_digest_never_counts_higher_risk_clean_as_low_risk()
    {
        var health = HealthWithCleanFindings();
        var snapshot = new ScanSnapshot(
            new DateTimeOffset(2026, 7, 16, 9, 0, 0, TimeSpan.Zero),
            []);

        var digest = HealthDigestBuilder.Create(@"C:\", snapshot, health, []);

        digest.Summary.Should().Contain("低风险清理 1 项")
            .And.Contain("风险偏高清理 1 项");
        digest.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Higher_risk_clean_recommendation_uses_observation_and_rollback_copy()
    {
        var recommendations = new[]
        {
            Recommendation("Low cache", RiskLevel.Low, 10 * 1024 * 1024),
            Recommendation("Risky cache", RiskLevel.High, 20 * 1024 * 1024)
        };

        var summary = HealthCheckSummaryBuilder.Build(DriveResult(), recommendations);
        var low = summary.KeyFindings.Single(item => item.Text.Contains("Low cache"));
        var high = summary.KeyFindings.Single(item => item.Text.Contains("Risky cache"));
        var disk = summary.Dimensions.Single(item => item.Name == "磁盘健康");

        low.Text.Should().Contain("低风险")
            .And.Contain("确认后");
        high.Text.Should().Contain("风险偏高")
            .And.Contain("快照")
            .And.Contain("回滚")
            .And.NotContain("建议确认后清理");
        disk.Result.Should().Contain("10.0 MB")
            .And.NotContain("30.0 MB");
    }

    [Fact]
    public void Startup_dimension_separates_ordinary_system_and_ownership_pending_clues()
    {
        var ordinary = new SoftwareProfile
        {
            Name = "Ordinary",
            Category = SoftwareCategory.Normal,
            StartupEntries = ["Ordinary Startup"]
        };
        var system = new SoftwareProfile
        {
            Name = "System",
            Category = SoftwareCategory.SystemTool,
            StartupEntries = ["System Startup"]
        };
        var ownershipPending = new SoftwareProfile
        {
            Name = "Ownership Pending",
            Category = SoftwareCategory.Unknown,
            InstallPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "SystemApps",
                "OwnershipPending"),
            StartupEntries = ["Ownership Pending Startup"]
        };

        var mixed = HealthCheckSummaryBuilder.Build(
            DriveResult(),
            [],
            softwareProfiles: [ordinary, system, ownershipPending]);
        var protectedOnly = HealthCheckSummaryBuilder.Build(
            DriveResult(),
            [],
            softwareProfiles: [system, ownershipPending]);
        var mixedDimension = mixed.Dimensions.Single(item => item.Name == "自启动线索");
        var protectedDimension = protectedOnly.Dimensions.Single(item => item.Name == "自启动线索");

        mixedDimension.Result.Should().Contain("1 个普通应用")
            .And.Contain("1 个系统组件线索")
            .And.Contain("1 个归属待确认线索");
        mixedDimension.Rating.Should().Be("建议查看");
        protectedDimension.Result.Should().Contain("0 个普通应用")
            .And.Contain("仅供查看");
        protectedDimension.Rating.Should().Be("仅供查看");
    }

    private static HealthCheckSummary HealthWithCleanFindings() =>
        new()
        {
            OverallScore = 70,
            Dimensions =
            [
                new HealthDimensionResult
                {
                    Name = "磁盘健康",
                    Result = "C 盘需要检查",
                    Rating = "需要关注"
                }
            ],
            KeyFindings =
            [
                new HealthFinding
                {
                    Text = "Low clean",
                    Action = RecommendationAction.Clean,
                    Risk = RiskLevel.Low
                },
                new HealthFinding
                {
                    Text = "High clean",
                    Action = RecommendationAction.Clean,
                    Risk = RiskLevel.High
                }
            ]
        };

    private static Recommendation Recommendation(string title, RiskLevel risk, long bytes) =>
        new()
        {
            Title = title,
            Finding = title + " finding",
            Reason = title + " reason",
            Action = RecommendationAction.Clean,
            Risk = risk,
            Reversibility = risk == RiskLevel.Low
                ? ReversibilityLevel.Reversible
                : ReversibilityLevel.PartiallyReversible,
            EstimatedImpactBytes = bytes,
            Evidence = [title + " evidence"]
        };

    private static DriveScanResult DriveResult() =>
        new()
        {
            Drive = @"C:\",
            TotalBytes = 100L * 1024 * 1024 * 1024,
            FreeBytes = 20L * 1024 * 1024 * 1024,
            TopLevel = []
        };
}
