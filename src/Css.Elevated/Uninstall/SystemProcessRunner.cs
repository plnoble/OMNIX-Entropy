using System.Diagnostics;

namespace Css.Elevated.Uninstall;

public sealed class SystemProcessRunner : IWindowsProcessRunner
{
    public async Task<int> RunAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startInfo);
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Windows did not create the requested process.");
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }
}
