using System.Xml.Linq;
using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class UninstallPlanDecisionHierarchyTests
{
    [Fact]
    public void Uninstall_preview_is_explained_as_a_path_free_beginner_decision()
    {
        var preview = UninstallPlanPresentationBuilder.Create(new SoftwareProfile
        {
            Name = "Example Notes",
            Publisher = "Example Publisher",
            InstallPath = @"C:\Private\Example",
            UninstallCommand = @"""C:\Private\Example\uninstall.exe"""
        });

        var decision = UninstallPlanDecisionSummaryPresenter.Create(preview);

        decision.Conclusion.Should().Contain("不会卸载软件")
            .And.NotContain("\\");
        decision.ProcessSummary.Should().Contain("官方卸载器")
            .And.Contain("重新扫描")
            .And.NotContain("\\");
        decision.ResidueSummary.Should().Contain("低风险")
            .And.Contain("不会自动处理")
            .And.NotContain("\\");
        decision.UndoSummary.Should().NotBeNullOrWhiteSpace()
            .And.NotContain("\\");
        decision.NextStep.Should().NotBeNullOrWhiteSpace()
            .And.NotContain("\\");
    }

    [Fact]
    public void Uninstall_window_puts_decision_before_collapsed_preparation_and_evidence()
    {
        var document = XDocument.Load(
            Path.Combine(FindRepositoryRoot(), "src", "Css.App", "UninstallPlanWindow.xaml"),
            LoadOptions.PreserveWhitespace);
        var conclusion = NamedElement(document, "UninstallPlanAgentConclusionTextBlock");
        var residue = NamedElement(document, "UninstallPlanDecisionResidueTextBlock");
        var undo = NamedElement(document, "UninstallPlanDecisionUndoTextBlock");
        var nextStep = NamedElement(document, "UninstallPlanDecisionNextStepTextBlock");
        var preparation = NamedElement(document, "UninstallPlanPreparationExpander");
        var workflow = NamedElement(document, "UninstallPlanWorkflowExpander");
        var technical = NamedElement(document, "UninstallPlanTechnicalDetailsExpander");

        AutomationId(conclusion).Should().Be("UninstallPlanAgentConclusionTextBlock");
        AutomationId(residue).Should().Be("UninstallPlanDecisionResidueTextBlock");
        AutomationId(undo).Should().Be("UninstallPlanDecisionUndoTextBlock");
        AutomationId(nextStep).Should().Be("UninstallPlanDecisionNextStepTextBlock");
        AttributeValue(preparation, "IsExpanded").Should().Be("False");
        AttributeValue(workflow, "IsExpanded").Should().Be("False");
        AttributeValue(technical, "IsExpanded").Should().Be("False");
        conclusion.IsBefore(preparation).Should().BeTrue();
        residue.IsBefore(preparation).Should().BeTrue();
        undo.IsBefore(preparation).Should().BeTrue();
        nextStep.IsBefore(preparation).Should().BeTrue();
        preparation.Descendants().Any(element =>
                Name(element) == "UninstallPlanChooseInstallerButton")
            .Should().BeTrue();
        preparation.Descendants().Any(element =>
                Name(element) == "UninstallPlanBuildFinalChecklistButton")
            .Should().BeTrue();
        workflow.Descendants().Any(element =>
                Name(element) == "UninstallPlanSimpleStepsListBox")
            .Should().BeTrue();
        technical.Descendants().Any(element =>
                Name(element) == "UninstallPlanOfficialUninstallerTextBlock")
            .Should().BeTrue();
    }

    [Fact]
    public void Unsigned_preview_hides_unavailable_preparation_controls()
    {
        var code = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "Css.App", "UninstallPlanWindow.xaml.cs"));
        var method = SourceMethodExtractor.Extract(
            code,
            "private void ApplyProductionReadiness()");

        method.Should().Contain("UninstallPlanPreparationExpander.Visibility")
            .And.Contain("_productionReadiness.CanPrepareExecution")
            .And.Contain("Visibility.Visible")
            .And.Contain("Visibility.Collapsed")
            .And.Contain("UninstallPlanDecisionNextStepTextBlock.Text");
        method.Should().NotContain("ExecuteAsync")
            .And.NotContain("Process.Start");
    }

    private static XElement NamedElement(XDocument document, string name) =>
        document.Descendants().Single(element => Name(element) == name);

    private static string? Name(XElement element) =>
        element.Attributes().SingleOrDefault(attribute =>
            attribute.Name.LocalName == "Name")?.Value;

    private static string? AutomationId(XElement element) =>
        element.Attributes().SingleOrDefault(attribute =>
            attribute.Name.LocalName.EndsWith(".AutomationId", StringComparison.Ordinal))?.Value;

    private static string? AttributeValue(XElement element, string name) =>
        element.Attribute(name)?.Value;

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
