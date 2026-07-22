using System.Xml.Linq;
using FluentAssertions;

namespace Css.Tests;

public sealed class TimelineFirstViewHierarchyTests
{
    [Fact]
    public void Undo_center_first_view_uses_compact_state_instead_of_empty_action_surfaces()
    {
        var document = XDocument.Load(
            Path.Combine(FindRepositoryRoot(), "src", "Css.App", "MainWindow.xaml"),
            LoadOptions.PreserveWhitespace);
        var quarantineCandidates = NamedElement(document, "TimelineQuarantineCandidateListBox");
        var cleanupButton = NamedElement(document, "ReviewQuarantineCleanupButton");
        var timelineState = NamedElement(document, "TimelineStateTextBlock");
        var timeline = NamedElement(document, "TimelineListBox");

        AutomationId(timelineState).Should().Be("TimelineStateTextBlock");
        timelineState.Attribute("Text")?.Value.Should().Contain("读取");
        quarantineCandidates.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        cleanupButton.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        timeline.Attribute("Visibility")?.Value.Should().Be("Collapsed");
        timelineState.IsBefore(timeline).Should().BeTrue();
    }

    [Fact]
    public void Undo_center_result_surfaces_follow_current_entries_and_retention_candidates()
    {
        var code = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "src", "Css.App", "MainWindow.xaml.cs"));
        var state = SourceMethodExtractor.Extract(
            code,
            "private void ShowTimelineState(");
        var entries = SourceMethodExtractor.Extract(
            code,
            "private void ShowTimelineEntries(");
        var retention = SourceMethodExtractor.Extract(
            code,
            "private void ApplyQuarantineRetentionPresentation(");

        state.Should().Contain("TimelineStateTextBlock.Visibility = Visibility.Visible")
            .And.Contain("TimelineListBox.Visibility = Visibility.Collapsed")
            .And.Contain("TimelineListBox.ItemsSource = null");
        entries.Should().Contain("entries.Select(ActionTimelinePresenter.CreateItem)")
            .And.Contain("TimelineStateTextBlock.Visibility = Visibility.Collapsed")
            .And.Contain("TimelineListBox.Visibility = Visibility.Visible");
        retention.Should().Contain("presentation.Candidates.Count > 0")
            .And.Contain("TimelineQuarantineCandidateListBox.Visibility")
            .And.Contain("ReviewQuarantineCleanupButton.Visibility")
            .And.Contain("ReviewQuarantineCleanupButton.IsEnabled = hasCandidates");
        state.Should().NotContain("OperationPipeline")
            .And.NotContain("RestoreAsync")
            .And.NotContain("PurgeAsync");
        entries.Should().NotContain("OperationPipeline")
            .And.NotContain("RestoreAsync")
            .And.NotContain("PurgeAsync");
        retention.Should().NotContain("OperationPipeline")
            .And.NotContain("RestoreAsync")
            .And.NotContain("PurgeAsync");
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
