using System.ComponentModel;
using System.Diagnostics;
using Css.Elevated.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class WindowsOfficialUninstallerLauncherTests
{
    [Fact]
    public async Task Launcher_builds_exact_elevated_shell_execute_request_and_captures_exit_code()
    {
        var runner = new FakeProcessRunner(exitCode: 0);
        var launcher = new WindowsOfficialUninstallerLauncher(runner);
        var executable = @"D:\Software\Example\Install\Uninstall.exe";

        var result = await launcher.LaunchAsync(new OfficialUninstallerLaunchRequest
        {
            ExecutablePath = executable,
            Arguments = "/remove",
            RequiresElevation = true
        }, CancellationToken.None);

        result.Started.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        runner.CallCount.Should().Be(1);
        runner.LastStartInfo!.FileName.Should().Be(executable);
        runner.LastStartInfo.Arguments.Should().Be("/remove");
        runner.LastStartInfo.UseShellExecute.Should().BeTrue();
        runner.LastStartInfo.Verb.Should().Be("runas");
        runner.LastStartInfo.WorkingDirectory.Should().Be(@"D:\Software\Example\Install");
    }

    [Fact]
    public async Task Launcher_omits_runas_when_elevation_is_not_requested()
    {
        var runner = new FakeProcessRunner(exitCode: 5);
        var launcher = new WindowsOfficialUninstallerLauncher(runner);

        var result = await launcher.LaunchAsync(new OfficialUninstallerLaunchRequest
        {
            ExecutablePath = @"D:\Software\Example\Install\Uninstall.exe",
            Arguments = string.Empty,
            RequiresElevation = false
        }, CancellationToken.None);

        result.Started.Should().BeTrue();
        result.ExitCode.Should().Be(5);
        runner.LastStartInfo!.Verb.Should().BeEmpty();
    }

    [Fact]
    public async Task Launcher_maps_windows_uac_cancellation_to_user_cancelled_result()
    {
        var runner = new FakeProcessRunner(new Win32Exception(1223, "The operation was canceled by the user."));
        var launcher = new WindowsOfficialUninstallerLauncher(runner);

        var result = await launcher.LaunchAsync(new OfficialUninstallerLaunchRequest
        {
            ExecutablePath = @"D:\Software\Example\Install\Uninstall.exe",
            Arguments = "/remove",
            RequiresElevation = true
        }, CancellationToken.None);

        result.Started.Should().BeFalse();
        result.UserCancelled.Should().BeTrue();
        result.ExitCode.Should().BeNull();
    }

    [Fact]
    public async Task Launcher_reports_runner_failure_without_throwing_or_claiming_start()
    {
        var runner = new FakeProcessRunner(new InvalidOperationException("start failed"));
        var launcher = new WindowsOfficialUninstallerLauncher(runner);

        var result = await launcher.LaunchAsync(new OfficialUninstallerLaunchRequest
        {
            ExecutablePath = @"D:\Software\Example\Install\Uninstall.exe",
            Arguments = "/remove",
            RequiresElevation = true
        }, CancellationToken.None);

        result.Started.Should().BeFalse();
        result.UserCancelled.Should().BeFalse();
        result.Error.Should().Contain("start failed");
    }

    [Fact]
    public async Task Launcher_preserves_operation_cancellation()
    {
        var runner = new FakeProcessRunner(new OperationCanceledException("cancelled"));
        var launcher = new WindowsOfficialUninstallerLauncher(runner);

        var action = () => launcher.LaunchAsync(new OfficialUninstallerLaunchRequest
        {
            ExecutablePath = @"D:\Software\Example\Install\Uninstall.exe",
            Arguments = "/remove",
            RequiresElevation = true
        }, CancellationToken.None);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Real_process_api_is_isolated_behind_the_elevated_production_worker()
    {
        var runnerSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall", "SystemProcessRunner.cs"));
        var launcherSource = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall", "WindowsOfficialUninstallerLauncher.cs"));
        var worker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "OfficialUninstallProductionWorker.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        runnerSource.Should().Contain("Process.Start");
        runnerSource.Should().Contain("WaitForExitAsync");
        launcherSource.Should().NotContain("Process.Start");
        launcherSource.Should().Contain("IWindowsProcessRunner");
        worker.Should().Contain("WindowsOfficialUninstallerLauncher");
        worker.Should().Contain("SystemProcessRunner");
        worker.Should().Contain("WindowsOfficialUninstallProductionPackageAuthorizer");
        app.Should().NotContain("WindowsOfficialUninstallerLauncher");
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
                return path;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", Path.Combine(segments));
    }

    private sealed class FakeProcessRunner : IWindowsProcessRunner
    {
        private readonly int _exitCode;
        private readonly Exception? _exception;

        public FakeProcessRunner(int exitCode)
        {
            _exitCode = exitCode;
        }

        public FakeProcessRunner(Exception exception)
        {
            _exception = exception;
        }

        public int CallCount { get; private set; }
        public ProcessStartInfo? LastStartInfo { get; private set; }

        public Task<int> RunAsync(
            ProcessStartInfo startInfo,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastStartInfo = startInfo;
            if (_exception is not null)
                throw _exception;
            return Task.FromResult(_exitCode);
        }
    }
}
