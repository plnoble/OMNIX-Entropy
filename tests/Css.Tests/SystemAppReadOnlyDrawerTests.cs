using Css.Core.Apps;
using Css.Core.Recommendations;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public class SystemAppReadOnlyDrawerTests
{
    [Fact]
    public void System_app_stays_read_only_even_when_mutating_evidence_fields_exist()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Windows Component",
            Category = SoftwareCategory.SystemTool,
            InstallPath = @"C:\Windows\System32\Component",
            UninstallCommand = "remove-system-component.exe",
            CachePaths = [@"C:\Windows\Temp\Component"],
            CacheSizeBytes = 128L * 1024 * 1024,
            StartupEntries = ["Component Startup"],
            Services = ["ComponentService"],
            ScheduledTasks = [@"\Microsoft\Windows\Component"]
        });

        drawer.AgentAdvice.Action.Should().Be(RecommendationAction.Keep);
        drawer.AgentAdvice.Text.Should().Contain("系统相关应用")
            .And.Contain("建议保留")
            .And.Contain("不会")
            .And.Contain("卸载")
            .And.Contain("迁移");
        drawer.AgentAdvice.RequiresUserConfirmation.Should().BeFalse();

        drawer.AvailableActions.Where(action => action.Kind != AppActionKind.TechnicalDetails)
            .Should().OnlyContain(action => !action.IsEnabled && action.Reason.Contains("系统"));
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.TechnicalDetails)
            .IsEnabled.Should().BeTrue();

        var visible = drawer.AgentAdvice.Text + "\n" + drawer.AgentAdvice.Reason + "\n"
            + string.Join("\n", drawer.AvailableActions.Select(action => action.Reason));
        visible.Should().NotContain(@"C:\Windows")
            .And.NotContain("ComponentService")
            .And.NotContain("remove-system-component.exe");
    }

    [Fact]
    public void Ordinary_app_with_the_same_evidence_keeps_existing_review_actions()
    {
        var drawer = AppPresentationBuilder.CreateDrawer(new SoftwareProfile
        {
            Name = "Ordinary App",
            Category = SoftwareCategory.Normal,
            InstallPath = @"C:\Program Files\Ordinary App",
            UninstallCommand = "ordinary-uninstall.exe",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Ordinary App\Cache"],
            CacheSizeBytes = 128L * 1024 * 1024,
            StartupEntries = ["Ordinary App Startup"]
        });

        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.Uninstall)
            .IsEnabled.Should().BeTrue();
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.Migration)
            .IsEnabled.Should().BeTrue();
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.CacheCleanup)
            .IsEnabled.Should().BeTrue();
        drawer.AvailableActions.Single(action => action.Kind == AppActionKind.StartupControl)
            .IsEnabled.Should().BeTrue();
    }
}
