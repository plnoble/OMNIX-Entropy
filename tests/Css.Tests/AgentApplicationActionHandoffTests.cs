using Css.Core.Agent;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AgentApplicationActionHandoffTests
{
    [Theory]
    [InlineData("卸载微信", AgentApplicationHandoff.UninstallReview, "查看卸载安全方案")]
    [InlineData("把微信迁移到 D 盘", AgentApplicationHandoff.MigrationReview, "查看迁移方案")]
    [InlineData("清理微信缓存", AgentApplicationHandoff.CacheCleanupReview, "查看缓存方案")]
    [InlineData("关闭微信开机自启动", AgentApplicationHandoff.StartupControlReview, "查看自启动方案")]
    public void Exact_explicit_app_action_prepares_one_typed_review_without_execution(
        string question,
        AgentApplicationHandoff expected,
        string label)
    {
        var reply = AgentConversationPresenter.Answer(question, null, [Profile()]);

        reply.TargetAppName.Should().Be("微信");
        reply.TargetAppHandoff.Should().Be(expected);
        reply.NavigationTargetPage.Should().Be("Apps");
        reply.NavigationLabel.Should().Be(label);
        reply.CanNavigate.Should().BeTrue();
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData("微信装在哪里")]
    [InlineData("微信闪退了")]
    [InlineData("微信最近有点奇怪")]
    public void Location_troubleshooting_and_general_app_questions_remain_details_only(
        string question)
    {
        var reply = AgentConversationPresenter.Answer(question, null, [Profile()]);

        reply.TargetAppName.Should().Be("微信");
        reply.TargetAppHandoff.Should().Be(AgentApplicationHandoff.Details);
        reply.NavigationLabel.Should().Be("打开这个应用");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData("卸载微信")]
    [InlineData("把微信迁移到 D 盘")]
    [InlineData("清理微信缓存")]
    [InlineData("关闭微信开机自启动")]
    public void Unavailable_or_untrusted_app_action_remains_details_only(string question)
    {
        var reply = AgentConversationPresenter.Answer(
            question,
            null,
            [new SoftwareProfile
            {
                Name = "微信",
                Category = SoftwareCategory.SystemTool,
                InstallPath = @"D:\SystemTools\Wechat",
                UninstallCommand = @"D:\SystemTools\Wechat\uninstall.exe",
                CachePaths = [@"C:\ProgramData\Wechat\Cache"],
                StartupEntries = ["Wechat Startup"]
            }]);

        reply.TargetAppHandoff.Should().Be(AgentApplicationHandoff.Details);
        reply.NavigationLabel.Should().Be("打开这个应用");
        reply.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Wpf_handoff_re_resolves_identity_and_reuses_manual_preview_methods_only()
    {
        var main = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "MainWindow.xaml.cs"));

        var navigation = SourceMethodExtractor.Extract(
            main,
            "private async void AgentConversationNavigate_Click(object sender, RoutedEventArgs e)");
        navigation.Should().Contain("ResolveAndOpenAppTargetAsync(reply.TargetAppName)");
        navigation.Should().Contain(
            "OpenAgentApplicationHandoffAsync(reply.TargetAppHandoff, resolution.Profile)");

        var handoff = SourceMethodExtractor.Extract(
            main,
            "private async Task OpenAgentApplicationHandoffAsync(");
        handoff.Should().Contain("AgentApplicationHandoff.UninstallReview")
            .And.Contain("AgentApplicationHandoff.MigrationReview")
            .And.Contain("AgentApplicationHandoff.CacheCleanupReview")
            .And.Contain("AgentApplicationHandoff.StartupControlReview")
            .And.Contain("ShowUninstallPlanAsync(profile)")
            .And.Contain("ShowMigrationPlanAsync(profile)")
            .And.Contain("ShowCacheCleanupPreview(profile)")
            .And.Contain("ShowStartupControlPreviewAsync(profile)");
        handoff.Should().NotContain("SafetyOperationPipeline")
            .And.NotContain("OperationDescriptor")
            .And.NotContain("Process.Start")
            .And.NotContain("File.Move")
            .And.NotContain("Directory.Move")
            .And.NotContain("File.Delete")
            .And.NotContain("Directory.Delete");

        var manualActions = Extract(
            main,
            "private async void PreviewUninstall_Click",
            "private void ApplyDrawerActionHost");
        manualActions.Should().Contain("ShowUninstallPlanAsync(selected.Profile)")
            .And.Contain("ShowMigrationPlanAsync(selected.Profile)")
            .And.Contain("ShowCacheCleanupPreview(selected.Profile)")
            .And.Contain("ShowStartupControlPreviewAsync(selected.Profile)");
    }

    private static SoftwareProfile Profile() =>
        new()
        {
            Name = "微信",
            InstallPath = @"C:\Program Files\Wechat",
            UninstallCommand = @"D:\Software\Wechat\Install\uninstall.exe",
            CachePaths = [@"C:\Users\Fixture\AppData\Local\Wechat\Cache"],
            StartupEntries = ["Wechat Startup"]
        };

    private static string Extract(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }

    private static string FindRepositoryFile(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. parts]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException("Repository file was not found.", Path.Combine(parts));
    }
}
