using Css.Core.Agent;
using Css.Core.Apps;
using FluentAssertions;

namespace Css.Tests;

public sealed class AutomaticDiagnosisSkillLoadingTests
{
    [Theory]
    [InlineData(AgentSkillCategory.SystemDiagnosis, true)]
    [InlineData(AgentSkillCategory.SystemSettings, false)]
    [InlineData(AgentSkillCategory.Troubleshooting, false)]
    [InlineData(AgentSkillCategory.WindowAndDesktop, false)]
    [InlineData(AgentSkillCategory.ProcessAndServiceManagement, false)]
    [InlineData(AgentSkillCategory.HardwareInfo, false)]
    [InlineData(AgentSkillCategory.SystemTools, false)]
    [InlineData(AgentSkillCategory.InputAndSession, false)]
    public void Only_system_diagnosis_skill_needs_a_missing_full_health_scan(
        AgentSkillCategory category,
        bool expected)
    {
        AgentConversationPresenter.SkillNeedsHealthScan(category, null)
            .Should().Be(expected);
        AgentConversationPresenter.SkillNeedsHealthScan(category, ExistingSummary())
            .Should().BeFalse();
    }

    [Fact]
    public void System_diagnosis_skill_awaits_shared_read_only_health_before_replying()
    {
        var code = Read("src", "Css.App", "MainWindow.xaml.cs");
        var skill = Method(
            code,
            "private async void AgentSkillAction_Click",
            "private async void AgentConversationNavigate_Click");
        var core = Method(
            code,
            "private async Task<bool> RunHealthScanCoreAsync",
            "private void SetSoftwareProfiles");

        skill.Should().Contain("SkillNeedsHealthScan");
        skill.Should().Contain("await EnsureHealthScanLoadedAsync()");
        AssertBefore(
            skill,
            "await EnsureHealthScanLoadedAsync()",
            "AgentConversationPresenter.ExplainSkill");
        core.Should().Contain("_scanner.ScanAsync");
        skill.Should().NotContain("RefreshHealthScanAsync")
            .And.NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("ExecuteSelectedRecommendationAsync")
            .And.NotContain("Process.Start")
            .And.NotContain("File.Move")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Move")
            .And.NotContain("Directory.Delete")
            .And.NotContain("Registry.SetValue");
    }

    private static HealthCheckSummary ExistingSummary() => new()
    {
        OverallScore = 80,
        Dimensions = [],
        KeyFindings = []
    };

    private static string Method(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static void AssertBefore(string source, string first, string second)
    {
        var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
        var secondIndex = source.IndexOf(second, StringComparison.Ordinal);
        firstIndex.Should().BeGreaterThanOrEqualTo(0);
        secondIndex.Should().BeGreaterThan(firstIndex);
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
