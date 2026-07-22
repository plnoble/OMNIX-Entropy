using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.InstallGuard.Routing;
using Css.Scanner.Disk;
using FluentAssertions;

namespace Css.Tests;

public class V1FoundationTests
{
    [Fact]
    public async Task Pipeline_blocks_destructive_snapshot_required_operation_without_snapshot()
    {
        var pipeline = SafetyOperationPipeline.DryRun(_ => OperationResult.Ok("executed"));
        var descriptor = new OperationDescriptor
        {
            Kind = "clean.cache",
            Title = "清理浏览器缓存",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresSnapshot = true,
            RollbackRequired = true,
            ConfirmationAccepted = true,
            EvidenceSummary = "将移动缓存到隔离区",
            EstimatedImpactBytes = 1024
        };

        var result = await pipeline.ExecuteAsync(descriptor);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("snapshot");
    }

    [Fact]
    public void Recommendation_requires_decision_card_evidence_before_execution()
    {
        var operation = new OperationDescriptor
        {
            Kind = "clean.cache",
            Title = "清理微信缓存",
            Risk = RiskLevel.Medium,
            IsDestructive = true,
            RequiresSnapshot = true,
            RollbackRequired = true,
            ConfirmationText = "确认将缓存移动到隔离区",
            EvidenceSummary = "缓存目录 2.0 GB，30 天未访问",
            EstimatedImpactBytes = 2L * 1024 * 1024 * 1024,
            AffectedPaths = [@"C:\Users\Me\AppData\Roaming\Tencent\Cache"]
        };

        var card = new Recommendation
        {
            Title = "微信缓存增长过快",
            Finding = "缓存目录占用 2.0 GB",
            Reason = "属于可重建缓存，清理后不影响聊天记录",
            Action = RecommendationAction.Clean,
            Risk = RiskLevel.Medium,
            Reversibility = ReversibilityLevel.Reversible,
            EstimatedImpactBytes = operation.EstimatedImpactBytes,
            Evidence = ["路径位于 AppData 缓存目录", "30 天未访问"],
            Operation = operation
        };

        card.HasDecisionCardMinimums.Should().BeTrue();
        card.Operation!.AffectedPaths.Should().Contain(@"C:\Users\Me\AppData\Roaming\Tencent\Cache");
    }

    [Fact]
    public void Growth_analyzer_reports_sorted_positive_growth()
    {
        var previous = new ScanSnapshot(
            new DateTimeOffset(2026, 6, 29, 8, 0, 0, TimeSpan.Zero),
            [new ScanSnapshotItem(@"C:\Users\Me\AppData\Local\Docker", "Docker", 4_000)]);
        var current = new ScanSnapshot(
            new DateTimeOffset(2026, 6, 30, 8, 0, 0, TimeSpan.Zero),
            [
                new ScanSnapshotItem(@"C:\Users\Me\AppData\Local\Docker", "Docker", 9_000),
                new ScanSnapshotItem(@"C:\Temp", "Unknown", 6_000)
            ]);

        var growth = GrowthAnalyzer.Compare(previous, current);

        growth.Should().HaveCount(2);
        growth[0].Path.Should().Be(@"C:\Temp");
        growth[0].GrowthBytes.Should().Be(6_000);
        growth[1].OwnerSoftware.Should().Be("Docker");
    }

    [Theory]
    [InlineData("Notion", SoftwareCategory.Normal, @"D:\Software\Notion\Install")]
    [InlineData("Steam Game", SoftwareCategory.Game, @"D:\Game\Steam Game\Install")]
    [InlineData("Ollama", SoftwareCategory.Ai, @"D:\Agent\Ollama\Install")]
    [InlineData("Docker Desktop", SoftwareCategory.DevelopmentTool, @"D:\Development\Docker Desktop\Install")]
    public void Install_router_maps_categories_to_user_storage_layout(string softwareName, SoftwareCategory category, string expected)
    {
        var route = InstallRoutingEngine.CreateDefault().Recommend(softwareName, category);

        route.TargetInstallPath.Should().Be(expected);
        route.RequiresUserConfirmation.Should().BeTrue();
    }

    [Fact]
    public void Migration_plan_requires_verification_and_rollback_steps()
    {
        var profile = new SoftwareProfile
        {
            Name = "Ollama",
            Category = SoftwareCategory.Ai,
            InstallPath = @"C:\Users\Me\AppData\Local\Programs\Ollama",
            CachePaths = [@"C:\Users\Me\.ollama\models"],
            Services = ["OllamaService"]
        };

        var plan = MigrationPlanner.CreatePlan(profile, @"D:\Agent\Ollama", snapshotAvailable: true);

        plan.Score.Band.Should().Be(MigrationRiskBand.NeedsStopAndVerify);
        plan.VerificationSteps.Should().NotBeEmpty();
        plan.Rollback.Steps.Should().NotBeEmpty();
        plan.RequiresSnapshot.Should().BeTrue();
    }
}
