using System.Xml.Linq;
using Css.InstallGuard.Routing;
using FluentAssertions;

namespace Css.Tests;

public sealed class InstallFirstViewHierarchyTests
{
    [Fact]
    public void Empty_routing_memory_is_a_state_not_a_fake_rule()
    {
        var view = InstallRoutingMemoryPresenter.Create(InstallRoutingMemory.Empty);

        view.Summary.Should().Contain("还没有");
        view.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Installation_first_view_hides_empty_rule_and_report_controls()
    {
        var document = XDocument.Load(
            Path.Combine(FindRepositoryRoot(), "src", "Css.App", "MainWindow.xaml"),
            LoadOptions.PreserveWhitespace);

        VisibilityOf(NamedElement(document, "InstallRoutingMemoryListBox"))
            .Should().Be("Collapsed");
        VisibilityOf(NamedElement(document, "ForgetInstallRoutingRuleButton"))
            .Should().Be("Collapsed");
        VisibilityOf(NamedElement(document, "InstallDiffCardsListBox"))
            .Should().Be("Collapsed");
        VisibilityOf(NamedElement(document, "InstallDiffAgentExplainButton"))
            .Should().Be("Collapsed");
        VisibilityOf(NamedElement(document, "InstallDiffTechnicalDetailsExpander"))
            .Should().Be("Collapsed");
    }

    [Fact]
    public void Installation_result_controls_follow_current_presenter_rows_and_cards()
    {
        var code = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "Css.App", "MainWindow.xaml.cs"));
        var rules = SourceMethodExtractor.Extract(
            code,
            "private void LoadInstallRoutingMemoryRules()");
        var diff = SourceMethodExtractor.Extract(
            code,
            "private void ApplyInstallDiffPresentation(InstallSnapshotDiffViewModel view)");

        rules.Should().Contain("view.Rows.Count > 0")
            .And.Contain("InstallRoutingMemoryListBox.Visibility")
            .And.Contain("ForgetInstallRoutingRuleButton.Visibility");
        diff.Should().Contain("view.Cards.Count > 0")
            .And.Contain("InstallDiffCardsListBox.Visibility")
            .And.Contain("InstallDiffAgentExplainButton.Visibility = Visibility.Visible")
            .And.Contain("InstallDiffTechnicalDetailsExpander.Visibility = Visibility.Visible");
        rules.Should().NotContain("Process.Start")
            .And.NotContain("OperationPipeline");
        diff.Should().NotContain("Process.Start")
            .And.NotContain("OperationPipeline");
    }

    private static XElement NamedElement(XDocument document, string name) =>
        document.Descendants().Single(element =>
            element.Attributes().Any(attribute =>
                attribute.Name.LocalName == "Name" && attribute.Value == name));

    private static string? VisibilityOf(XElement element) =>
        element.Attribute("Visibility")?.Value;

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
