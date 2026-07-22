using System.ComponentModel;
using System.Diagnostics;

namespace Css.Elevated.Uninstall;

public interface IWindowsProcessRunner
{
    Task<int> RunAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken);
}

public sealed class WindowsOfficialUninstallerLauncher : IOfficialUninstallerLauncher
{
    private const int ErrorCancelled = 1223;
    private readonly IWindowsProcessRunner _runner;

    public WindowsOfficialUninstallerLauncher(IWindowsProcessRunner runner)
    {
        ArgumentNullException.ThrowIfNull(runner);
        _runner = runner;
    }

    public async Task<OfficialUninstallerLaunchResult> LaunchAsync(
        OfficialUninstallerLaunchRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ExecutablePath))
            return OfficialUninstallerLaunchResult.NotStarted("Executable path is required.");

        var workingDirectory = Path.GetDirectoryName(request.ExecutablePath) ?? string.Empty;
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            Arguments = request.Arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
            Verb = request.RequiresElevation ? "runas" : string.Empty
        };

        try
        {
            var exitCode = await _runner.RunAsync(startInfo, cancellationToken);
            return OfficialUninstallerLaunchResult.Completed(exitCode);
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == ErrorCancelled)
        {
            return OfficialUninstallerLaunchResult.NotStarted(
                "Windows elevation was cancelled by the user.",
                userCancelled: true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return OfficialUninstallerLaunchResult.NotStarted(exception.Message);
        }
    }
}
