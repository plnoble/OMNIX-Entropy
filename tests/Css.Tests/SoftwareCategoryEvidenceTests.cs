using Css.Core.Apps;
using Css.Core.Software;
using Css.Scanner.Experience;
using Css.Scanner.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class SoftwareCategoryEvidenceTests
{
    [Fact]
    public void Product_name_signal_is_preserved_as_high_confidence_evidence()
    {
        var profile = Build("Marvis", "Tencent", @"D:\Software\Marvis\Install");
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        profile.Category.Should().Be(SoftwareCategory.Ai);
        profile.CategoryAssessment.Category.Should().Be(SoftwareCategory.Ai);
        profile.CategoryAssessment.Confidence.Should().Be(SoftwareCategoryConfidence.High);
        profile.CategoryAssessment.IsFallback.Should().BeFalse();
        profile.CategoryAssessment.Evidence.Should().ContainSingle(evidence =>
            evidence.Source == SoftwareCategoryEvidenceSource.ProductName
            && evidence.MatchedRule == "marvis");
        drawer.CategorySummary.Should().Contain("AI 工具")
            .And.Contain("应用名称")
            .And.Contain("把握较高")
            .And.NotContain(@"D:\Software");
    }

    [Fact]
    public void Publisher_only_signal_is_advisory_instead_of_proof()
    {
        var profile = Build("Acme Assistant", "OpenAI Labs", @"D:\Software\Acme\Install");
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        profile.Category.Should().Be(SoftwareCategory.Ai);
        profile.CategoryAssessment.Confidence.Should().Be(SoftwareCategoryConfidence.Medium);
        profile.CategoryAssessment.Evidence.Should().ContainSingle(evidence =>
            evidence.Source == SoftwareCategoryEvidenceSource.Publisher
            && evidence.MatchedRule == "openai");
        drawer.CategorySummary.Should().Contain("发布者信息")
            .And.Contain("把握一般")
            .And.NotContain("OpenAI Labs");
    }

    [Fact]
    public void Install_location_only_signal_is_low_confidence_and_path_free_in_drawer()
    {
        var profile = Build("Acme Workbench", "Acme", @"D:\Agent\Ollama\Install");
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        profile.Category.Should().Be(SoftwareCategory.Ai);
        profile.CategoryAssessment.Confidence.Should().Be(SoftwareCategoryConfidence.Low);
        profile.CategoryAssessment.Evidence.Should().ContainSingle(evidence =>
            evidence.Source == SoftwareCategoryEvidenceSource.InstallLocation
            && evidence.MatchedRule == "ollama");
        drawer.CategorySummary.Should().Contain("安装位置线索")
            .And.Contain("仅供参考")
            .And.NotContain(@"D:\Agent")
            .And.NotContain("Ollama");
    }

    [Fact]
    public void Normal_fallback_says_that_no_explicit_category_signal_was_found()
    {
        var profile = Build("Acme Notes", "Acme", @"D:\Software\Acme Notes\Install");
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        profile.Category.Should().Be(SoftwareCategory.Normal);
        profile.CategoryAssessment.Category.Should().Be(SoftwareCategory.Normal);
        profile.CategoryAssessment.Confidence.Should().Be(SoftwareCategoryConfidence.Low);
        profile.CategoryAssessment.IsFallback.Should().BeTrue();
        profile.CategoryAssessment.Evidence.Should().BeEmpty();
        drawer.CategorySummary.Should().Contain("普通应用")
            .And.Contain("没发现明确分类线索")
            .And.Contain("先按普通应用展示");
    }

    [Fact]
    public void Missing_scanner_observation_keeps_unknown_explicit_instead_of_guessing()
    {
        var profile = new SoftwareProfile { Name = "Mystery Utility" };
        var drawer = AppPresentationBuilder.CreateDrawer(profile);

        profile.Category.Should().Be(SoftwareCategory.Unknown);
        profile.CategoryAssessment.Category.Should().Be(SoftwareCategory.Unknown);
        profile.CategoryAssessment.Confidence.Should().Be(SoftwareCategoryConfidence.Unknown);
        profile.CategoryAssessment.Evidence.Should().BeEmpty();
        drawer.CategorySummary.Should().Contain("未知类型")
            .And.Contain("资料不足")
            .And.Contain("不猜");
    }

    [Fact]
    public void Growth_enrichment_preserves_the_scanner_owned_category_observation()
    {
        var profile = Build("Docker Desktop", "Docker Inc.", @"D:\Development\Docker\Install");

        var enriched = SoftwareGrowthProfileEnricher.Apply([profile], []).Single();

        enriched.Category.Should().Be(SoftwareCategory.DevelopmentTool);
        enriched.CategoryAssessment.Should().BeSameAs(profile.CategoryAssessment);
        enriched.CategoryAssessment.Evidence.Should().Contain(evidence =>
            evidence.Source == SoftwareCategoryEvidenceSource.ProductName
            && evidence.MatchedRule == "docker");
    }

    private static SoftwareProfile Build(string name, string publisher, string path) =>
        SoftwareInventoryBuilder.Build(
            [new InstalledSoftwareRecord(name, publisher, path, null, null, "HKCU")],
            [],
            [],
            []).Single();
}
