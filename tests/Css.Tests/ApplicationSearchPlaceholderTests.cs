using System.Xml.Linq;
using FluentAssertions;

namespace Css.Tests;

public sealed class ApplicationSearchPlaceholderTests
{
    [Fact]
    public void Search_uses_an_empty_value_with_a_noninteractive_overlay_hint()
    {
        var xamlPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Css.App",
            "MainWindow.xaml");
        var document = XDocument.Load(xamlPath, LoadOptions.PreserveWhitespace);
        var search = NamedElement(document, "AppSearchTextBox");
        var hint = NamedElement(document, "AppSearchPlaceholderTextBlock");
        var searchHost = search.Parent;

        search.Name.LocalName.Should().Be("TextBox");
        search.Attribute("Text")?.Value.Should().BeNullOrEmpty();
        AutomationId(search).Should().Be("AppSearchTextBox");
        hint.Name.LocalName.Should().Be("TextBlock");
        hint.Attribute("Text")?.Value.Should().Be("\u641c\u7d22\u5e94\u7528");
        hint.Attribute("IsHitTestVisible")?.Value.Should().Be("False");
        AutomationId(hint).Should().Be("AppSearchPlaceholderTextBlock");
        searchHost.Should().NotBeNull();
        searchHost!.Name.LocalName.Should().Be("Grid");
        hint.Parent.Should().BeSameAs(searchHost);
        searchHost.Attribute("Width")?.Value.Should().Be("160");
        searchHost.Attribute("Height")?.Value.Should().Be("34");
    }

    [Fact]
    public void Text_change_updates_the_hint_before_filtering_the_catalog()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var handler = SourceMethodExtractor.Extract(
            code,
            "private void AppSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)");

        handler.Should().Contain("AppSearchPlaceholderTextBlock.Visibility")
            .And.Contain("string.IsNullOrEmpty(AppSearchTextBox.Text)")
            .And.Contain("RefreshAppCatalog();");
        handler.IndexOf("AppSearchPlaceholderTextBlock.Visibility", StringComparison.Ordinal)
            .Should().BeLessThan(handler.IndexOf("RefreshAppCatalog();", StringComparison.Ordinal));
    }

    private static XElement NamedElement(XDocument document, string name) =>
        document.Descendants().Single(element =>
            element.Attributes().Any(attribute =>
                attribute.Name.LocalName == "Name" && attribute.Value == name));

    private static string? AutomationId(XElement element) =>
        element.Attributes().SingleOrDefault(attribute =>
            attribute.Name.LocalName.EndsWith(".AutomationId", StringComparison.Ordinal))?.Value;

    private static string Read(params string[] segments) =>
        File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. segments]));

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
