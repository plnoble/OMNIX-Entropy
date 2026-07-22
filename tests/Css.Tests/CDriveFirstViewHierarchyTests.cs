using System.Xml.Linq;
using FluentAssertions;

namespace Css.Tests;

public sealed class CDriveFirstViewHierarchyTests
{
    [Fact]
    public void Cdrive_first_view_shows_truthful_state_copy_instead_of_empty_result_lists()
    {
        var document = XDocument.Load(
            Path.Combine(FindRepositoryRoot(), "src", "Css.App", "MainWindow.xaml"),
            LoadOptions.PreserveWhitespace);
        var rootState = NamedElement(document, "CDriveRootCauseStateTextBlock");
        var rootCauses = NamedElement(document, "CDriveRootCauseListBox");
        var growth = NamedElement(document, "GrowthListBox");
        var personalStorage = NamedElement(document, "PersonalStorageFindingsListBox");
        var recommendationState = NamedElement(document, "RecommendationsEmptyStateTextBlock");
        var recommendations = NamedElement(document, "RecommendationsListBox");
        var actionText = NamedElement(document, "RecommendationActionTextBlock");
        var actionPanel = NamedElement(document, "RecommendationActionPanel");
        var continueButton = NamedElement(document, "ExecuteRecommendationButton");

        AutomationId(rootState).Should().Be("CDriveRootCauseStateTextBlock");
        rootState.Attribute("Text")?.Value.Should().Contain("开始体检");
        AutomationId(recommendationState).Should().Be("RecommendationsEmptyStateTextBlock");
        recommendationState.Attribute("Text")?.Value.Should().Contain("体检");
        rootCauses.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        growth.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        personalStorage.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        recommendations.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        actionText.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        actionPanel.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        continueButton.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        rootState.IsBefore(rootCauses).Should().BeTrue();
        recommendationState.IsBefore(recommendations).Should().BeTrue();
    }

    [Fact]
    public void Cdrive_scan_switches_result_surfaces_only_from_current_collection_counts()
    {
        var code = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "Css.App", "MainWindow.xaml.cs"));
        var scan = SourceMethodExtractor.Extract(
            code,
            "private async Task<bool> RunHealthScanCoreAsync()");
        var apply = SourceMethodExtractor.Extract(
            code,
            "private HealthCheckSummary ApplySession(");
        var visibility = SourceMethodExtractor.Extract(
            code,
            "private void SetCDriveResultVisibility(");

        scan.Should().Contain("SetCDriveResultVisibility(false, false, false, false);")
            .And.Contain("正在只读扫描");
        apply.Should().Contain("rootCauseSummary.Cards.Count > 0")
            .And.Contain("growthItems.Count > 0")
            .And.Contain("personalStorage.Items.Count > 0")
            .And.Contain("recommendationList.Cards.Count > 0")
            .And.Contain("SetCDriveResultVisibility(");
        visibility.Should().Contain("CDriveRootCauseListBox.Visibility")
            .And.Contain("GrowthListBox.Visibility")
            .And.Contain("PersonalStorageFindingsListBox.Visibility")
            .And.Contain("RecommendationsListBox.Visibility")
            .And.Contain("RecommendationActionTextBlock.Visibility")
            .And.Contain("RecommendationActionPanel.Visibility")
            .And.Contain("ExecuteRecommendationButton.Visibility");
        visibility.Should().NotContain("OperationPipeline")
            .And.NotContain("Quarantine")
            .And.NotContain("Delete");
    }

    private static XElement NamedElement(XDocument document, string name) =>
        document.Descendants().Single(element =>
            element.Attributes().Any(attribute =>
                attribute.Name.LocalName == "Name" && attribute.Value == name));

    private static string? AutomationId(XElement element) =>
        element.Attributes().SingleOrDefault(attribute =>
            attribute.Name.LocalName.EndsWith(".AutomationId", StringComparison.Ordinal))?.Value;

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
