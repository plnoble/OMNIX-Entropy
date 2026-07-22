using Css.Core.Apps;
using Css.Core.Software;
using FluentAssertions;

namespace Css.Tests;

public sealed class AppStartupSettingsHandoffTests
{
    [Fact]
    public void Normal_startup_entry_gets_open_only_windows_settings_handoff()
    {
        var profile = new SoftwareProfile
        {
            Name = "Example",
            Category = SoftwareCategory.Normal,
            StartupEntries = ["ExampleStartup"],
            Services = ["ExampleService"],
            ScheduledTasks = ["ExampleTask"]
        };

        var handoff = AppStartupSettingsHandoffPresenter.Create(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var host = AppDrawerActionHostPresenter.ShowStartupControl(drawer, handoff);

        handoff.CanOpenStartupSettings.Should().BeTrue();
        handoff.SettingsShortcutId.Should().Be("startup-apps");
        handoff.Lines.Should().Contain(line => line.Contains("服务/计划任务") && line.Contains("不处理"));
        handoff.Lines.Should().Contain(line =>
            line.Contains("Windows 官方页面") && line.Contains("不根据内部字节猜测"));
        handoff.SafetyText.Should().Contain("只打开设置页");
        host.PrimaryActionText.Should().Be("在 Windows 中查看");
        host.PrimaryActionKey.Should().Be("StartupSettings");
        host.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public void Service_task_only_and_system_profiles_remain_explanation_only()
    {
        var backgroundOnly = new SoftwareProfile
        {
            Name = "Background Example",
            Services = ["ExampleService"],
            ScheduledTasks = ["ExampleTask"]
        };
        var system = new SoftwareProfile
        {
            Name = "System Example",
            Category = SoftwareCategory.SystemTool,
            StartupEntries = ["SystemStartup"]
        };

        var backgroundHandoff = AppStartupSettingsHandoffPresenter.Create(backgroundOnly);
        var systemHandoff = AppStartupSettingsHandoffPresenter.Create(system);

        backgroundHandoff.CanOpenStartupSettings.Should().BeFalse();
        backgroundHandoff.SettingsShortcutId.Should().BeNull();
        backgroundHandoff.Summary.Should().Contain("后台服务或计划任务");
        systemHandoff.CanOpenStartupSettings.Should().BeFalse();
        systemHandoff.Summary.Should().Contain("系统相关应用");
    }

    [Fact]
    public void Drawer_uses_allowlisted_confirmed_settings_launcher_without_mutation_authority()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var presenter = File.ReadAllText(FindRepositoryFile("src", "Css.Core", "Apps", "AppStartupSettingsHandoff.cs"));
        var primary = Extract(
            main,
            "private async void DrawerActionPreviewPrimary_Click",
            "private async Task ExecutePendingAppCacheCleanupAsync()");
        var launcher = Extract(
            main,
            "private void OpenAllowlistedWindowsSettings",
            "private Task EnsureHealthScanLoadedAsync()");

        primary.Should().Contain("case \"StartupSettings\":");
        xaml.Should().Contain("Content=\"&#x7BA1;&#x7406;&#x81EA;&#x542F;&#x52A8;\"");
        primary.Should().Contain("OpenAllowlistedWindowsSettings(AppStartupSettingsHandoffPresenter.StartupSettingsShortcutId)");
        primary.Should().NotContain("Process.Start");
        launcher.Should().Contain("WindowsSettingsShortcutCatalog.FindById");
        launcher.Should().Contain("shortcut.IsOpenOnly");
        launcher.Should().Contain("shortcut.RequiresConfirmation");
        launcher.Should().Contain("UseShellExecute = true");
        launcher.Should().Contain("Process.Start(startInfo)");
        launcher.Should().NotContain("Registry.SetValue");
        launcher.Should().NotContain("Set-Service");
        launcher.Should().NotContain("schtasks");
        presenter.Should().NotContain("OperationDescriptor");
        presenter.Should().NotContain("Registry");
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine([current.FullName, .. segments]);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        throw new FileNotFoundException("Repository file was not found.", Path.Combine(segments));
    }

    private static string Extract(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        end.Should().BeGreaterThan(start);
        return source[start..end];
    }
}
