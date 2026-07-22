using Css.Core.Software;
using Css.InstallGuard.Installers;
using Css.InstallGuard.Routing;
using FluentAssertions;

namespace Css.Tests;

public class InstallerAnalyzerTests
{
    [Theory]
    [InlineData(@"C:\Downloads\Notion Setup 3.0.0.exe", SoftwareCategory.Normal, @"D:\Software\Notion\Install")]
    [InlineData(@"C:\Downloads\SteamSetup.exe", SoftwareCategory.Game, @"D:\Game\Steam\Install")]
    [InlineData(@"C:\Downloads\OllamaSetup.exe", SoftwareCategory.Ai, @"D:\Agent\Ollama\Install")]
    [InlineData(@"C:\Downloads\Docker Desktop Installer.exe", SoftwareCategory.DevelopmentTool, @"D:\Development\Docker Desktop\Install")]
    public void Analyzer_recommends_user_install_layout_from_installer_name(
        string installerPath,
        SoftwareCategory expectedCategory,
        string expectedTarget)
    {
        var result = InstallerAnalyzer.AnalyzePath(installerPath);

        result.Category.Should().Be(expectedCategory);
        result.RecommendedRoute.TargetInstallPath.Should().Be(expectedTarget);
        result.RequiresUserConfirmation.Should().BeTrue();
        result.Evidence.Should().NotBeEmpty();
    }

    [Fact]
    public void Analyzer_detects_msi_and_returns_candidate_arguments()
    {
        var result = InstallerAnalyzer.AnalyzePath(@"C:\Downloads\ExampleTool.msi");

        result.Kind.Should().Be(InstallerKind.Msi);
        result.CandidateInstallArguments.Should().Contain(a => a.Contains("TARGETDIR"));
        result.CandidateInstallArguments.Should().Contain(a => a.Contains("INSTALLDIR"));
    }

    [Fact]
    public void Analyzer_detects_inno_and_nsis_hints_without_running_installer()
    {
        var inno = InstallerAnalyzer.AnalyzePath(@"C:\Downloads\PhotoTool-inno-setup.exe");
        var nsis = InstallerAnalyzer.AnalyzePath(@"C:\Downloads\PhotoTool-nsis-installer.exe");

        inno.Kind.Should().Be(InstallerKind.InnoSetup);
        nsis.Kind.Should().Be(InstallerKind.Nsis);
        inno.WillRunInstaller.Should().BeFalse();
        nsis.WillRunInstaller.Should().BeFalse();
    }

    [Fact]
    public void Routing_memory_prefers_exact_software_rule_then_category_rule_and_persists_json()
    {
        var memory = InstallRoutingMemory.Empty
            .RememberCategory(SoftwareCategory.Ai, @"E:\AI")
            .RememberSoftware("Ollama", SoftwareCategory.Ai, @"D:\Agent\LocalAI");
        var engine = InstallRoutingEngine.CreateDefault();

        var exact = engine.Recommend("Ollama", SoftwareCategory.Ai, memory);
        var category = engine.Recommend("LM Studio", SoftwareCategory.Ai, memory);
        var normal = engine.Recommend("Notion", SoftwareCategory.Normal, memory);

        exact.TargetInstallPath.Should().Be(@"D:\Agent\LocalAI\Ollama\Install");
        exact.FromUserMemory.Should().BeTrue();
        exact.MemoryScope.Should().Be("Software");
        category.TargetInstallPath.Should().Be(@"E:\AI\LM Studio\Install");
        category.FromUserMemory.Should().BeTrue();
        category.MemoryScope.Should().Be("Category");
        normal.TargetInstallPath.Should().Be(@"D:\Software\Notion\Install");
        normal.FromUserMemory.Should().BeFalse();

        var path = Path.Combine(Path.GetTempPath(), "omnix-install-routing-memory.json");
        try
        {
            InstallRoutingMemoryStore.Save(path, memory);
            var loaded = InstallRoutingMemoryStore.Load(path);

            loaded.Rules.Should().HaveCount(2);
            engine.Recommend("Ollama", SoftwareCategory.Ai, loaded).TargetInstallPath
                .Should().Be(@"D:\Agent\LocalAI\Ollama\Install");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Analyzer_uses_install_routing_memory_without_running_installer()
    {
        var memory = InstallRoutingMemory.Empty
            .RememberCategory(SoftwareCategory.Ai, @"E:\AIApps");

        var result = InstallerAnalyzer.AnalyzePath(@"C:\Downloads\OllamaSetup.exe", routingMemory: memory);

        result.RecommendedRoute.TargetInstallPath.Should().Be(@"E:\AIApps\Ollama\Install");
        result.RecommendedRoute.FromUserMemory.Should().BeTrue();
        result.RecommendedRoute.MemoryScope.Should().Be("Category");
        result.RequiresUserConfirmation.Should().BeTrue();
        result.WillRunInstaller.Should().BeFalse();
        result.Evidence.Should().Contain(line => line.Contains("E:\\AIApps\\Ollama\\Install"));
    }

    [Fact]
    public void Routing_memory_can_remember_a_confirmed_route_for_the_same_software()
    {
        var route = new InstallRoute
        {
            SoftwareName = "Notion",
            Category = SoftwareCategory.Normal,
            TargetInstallPath = @"E:\Apps\Notion\Install",
            Reason = "User confirmed route"
        };

        var memory = InstallRoutingMemory.Empty.RememberRoute(route);
        var remembered = InstallRoutingEngine.CreateDefault().Recommend("Notion", SoftwareCategory.Normal, memory);

        remembered.TargetInstallPath.Should().Be(@"E:\Apps\Notion\Install");
        remembered.FromUserMemory.Should().BeTrue();
        remembered.MemoryScope.Should().Be("Software");
    }

    [Fact]
    public void Routing_memory_can_remember_a_confirmed_route_for_the_whole_category()
    {
        var route = new InstallRoute
        {
            SoftwareName = "Ollama",
            Category = SoftwareCategory.Ai,
            TargetInstallPath = @"E:\AgentTools\Ollama\Install",
            Reason = "User confirmed route"
        };

        var memory = InstallRoutingMemory.Empty.RememberRouteForCategory(route);
        var remembered = InstallRoutingEngine.CreateDefault().Recommend("LM Studio", SoftwareCategory.Ai, memory);

        remembered.TargetInstallPath.Should().Be(@"E:\AgentTools\LM Studio\Install");
        remembered.FromUserMemory.Should().BeTrue();
        remembered.MemoryScope.Should().Be("Category");
    }

    [Fact]
    public void Install_route_memory_choice_presenter_explains_scope_without_running_installer()
    {
        var result = InstallerAnalyzer.AnalyzePath(@"C:\Downloads\OllamaSetup.exe");

        var choice = InstallRouteMemoryChoicePresenter.Create(result);

        choice.Title.Should().Contain("\u8bb0\u4f4f");
        choice.Summary.Should().Contain("Ollama");
        choice.RecommendedTarget.Should().Be(@"D:\Agent\Ollama\Install");
        choice.SoftwareOptionText.Should().Contain("Ollama");
        choice.CategoryOptionText.Should().Contain("AI");
        choice.SafetyText.Should().Contain("\u4e0d\u4f1a\u8fd0\u884c\u5b89\u88c5\u5668");
        choice.SafetyText.Should().Contain("\u53ea\u8bb0\u4f4f\u89c4\u5219");
    }

    [Fact]
    public void Install_routing_memory_presenter_shows_plain_learned_rules_without_json()
    {
        var memory = InstallRoutingMemory.Empty
            .RememberSoftware("Ollama", SoftwareCategory.Ai, @"D:\Agent")
            .RememberCategory(SoftwareCategory.Game, @"D:\Game");

        var view = InstallRoutingMemoryPresenter.Create(memory);

        view.Rows.Should().HaveCount(2);
        view.Rows[0].Title.Should().Contain("Ollama");
        view.Rows[0].Summary.Should().Contain("\u53ea\u9488\u5bf9\u8fd9\u4e2a\u8f6f\u4ef6");
        view.Rows[1].Title.Should().Contain("\u6e38\u620f");
        view.Rows[1].Summary.Should().Contain("\u540c\u7c7b\u8f6f\u4ef6");
        view.Rows.Should().OnlyContain(row => row.SafetyText.Contains("\u4e0d\u4f1a\u8fd0\u884c\u5b89\u88c5\u5668"));

        var visibleText = string.Join("\n", view.Rows.Select(row => row.Title + row.Summary + row.SafetyText));
        visibleText.Should().NotContain("SoftwareName");
        visibleText.Should().NotContain("TargetRoot");
        visibleText.Should().NotContain("{");
        visibleText.Should().NotContain("}");
    }

    [Fact]
    public void Install_routing_memory_can_forget_a_presented_rule_by_key()
    {
        var memory = InstallRoutingMemory.Empty
            .RememberSoftware("Ollama", SoftwareCategory.Ai, @"D:\Agent")
            .RememberCategory(SoftwareCategory.Game, @"D:\Game");
        var view = InstallRoutingMemoryPresenter.Create(memory);

        var updated = memory.ForgetRule(view.Rows[0].RuleKey);

        updated.FindSoftwareRule("Ollama").Should().BeNull();
        updated.FindCategoryRule(SoftwareCategory.Game).Should().NotBeNull();
        InstallRoutingMemoryPresenter.Create(updated).Rows.Should().OnlyContain(row => row.CanForget);
    }
}
