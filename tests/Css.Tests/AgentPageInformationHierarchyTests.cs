using FluentAssertions;

namespace Css.Tests;

public sealed class AgentPageInformationHierarchyTests
{
    [Fact]
    public void Agent_page_defaults_to_consultation_and_keeps_tools_in_a_second_stable_tab()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var root = xaml.IndexOf(
            "<TabControl x:Name=\"AgentPage\"",
            StringComparison.Ordinal);
        var consultation = xaml.IndexOf(
            "x:Name=\"AgentConsultationTab\"",
            StringComparison.Ordinal);
        var conversation = xaml.IndexOf(
            "x:Name=\"AgentConversationScrollViewer\"",
            StringComparison.Ordinal);
        var capabilities = xaml.IndexOf(
            "x:Name=\"AgentCapabilitiesTab\"",
            StringComparison.Ordinal);
        var capabilityContent = xaml.IndexOf(
            "x:Name=\"AgentCapabilityScrollViewer\"",
            StringComparison.Ordinal);

        root.Should().BeGreaterThanOrEqualTo(0);
        consultation.Should().BeGreaterThan(root);
        conversation.Should().BeGreaterThan(consultation);
        capabilities.Should().BeGreaterThan(conversation);
        capabilityContent.Should().BeGreaterThan(capabilities);
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentPageTabControl\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentConsultationTab\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"AgentCapabilitiesTab\"");
        xaml.Should().Contain("Header=\"咨询与建议\"");
        xaml.Should().Contain("Header=\"能力与工具\"");
        xaml.Should().Contain("SelectedIndex=\"0\"");
    }

    [Fact]
    public void Agent_tabs_preserve_existing_content_identity_without_a_two_column_root()
    {
        var xaml = Read("src", "Css.App", "MainWindow.xaml");
        var start = xaml.IndexOf("x:Name=\"AgentPage\"", StringComparison.Ordinal);
        var end = xaml.IndexOf("x:Name=\"StatusTextBlock\"", start, StringComparison.Ordinal);
        var agentPage = xaml.Substring(start, end - start);

        agentPage.Should().Contain("AgentConversationScrollViewer");
        agentPage.Should().Contain("AgentCapabilityScrollViewer");
        agentPage.Should().Contain("AgentNextStepActionButtonsItemsControl");
        agentPage.Should().Contain("AgentWindowsSettingsListBox");
        agentPage.Should().Contain("AgentSkillListBox");
        agentPage.Should().Contain("AgentSystemToolListBox");
        agentPage.Should().NotContain("<ColumnDefinition Width=\"340\"/>");
        agentPage.Should().NotContain("<Border Grid.Column=\"1\" Style=\"{StaticResource SectionCardStyle}\">");
        agentPage.Should().NotContain("MaxWidth=\"780\"");
    }

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
