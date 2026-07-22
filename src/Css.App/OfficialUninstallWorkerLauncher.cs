using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using Css.Win32.Processes;

namespace Css.App;

internal enum WindowsOfficialUninstallWorkerMode
{
    DevelopmentVerification,
    Production,
    MigrationProduction
}

public sealed class WindowsOfficialUninstallWorkerLauncher
    : IOfficialUninstallWorkerLauncher
{
    private const int ErrorCancelled = 1223;
    private readonly string _workerExecutablePath;
    private readonly string _expectedWorkerSha256;
    private readonly WindowsOfficialUninstallWorkerMode _mode;

    public WindowsOfficialUninstallWorkerLauncher(
        string workerExecutablePath,
        string expectedWorkerSha256)
        : this(
            workerExecutablePath,
            expectedWorkerSha256,
            WindowsOfficialUninstallWorkerMode.DevelopmentVerification)
    {
    }

    internal WindowsOfficialUninstallWorkerLauncher(
        string workerExecutablePath,
        string expectedWorkerSha256,
        WindowsOfficialUninstallWorkerMode mode)
    {
        if (string.IsNullOrWhiteSpace(workerExecutablePath))
            throw new ArgumentException("The worker executable path is required.", nameof(workerExecutablePath));
        _workerExecutablePath = Path.GetFullPath(workerExecutablePath);
        _expectedWorkerSha256 = NormalizeSha256(expectedWorkerSha256);
        _mode = mode;
    }

    public ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
        OfficialUninstallWorkerLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        if (!WorkerHashMatches())
            return ValueTask.FromResult(Failed());
        var start = new ProcessStartInfo
        {
            FileName = _workerExecutablePath,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden
        };
        foreach (var value in WorkerArguments(request, _mode))
            start.ArgumentList.Add(value);

        try
        {
            var process = Process.Start(start);
            return ValueTask.FromResult(process is null
                ? Failed()
                : new OfficialUninstallWorkerLaunchResult
                {
                    Status = OfficialUninstallWorkerLaunchStatus.Started,
                    Process = new WindowsOfficialUninstallWorkerProcess(process),
                    ImageExpectation = new OfficialUninstallWorkerImageExpectation
                    {
                        ExecutablePath = _workerExecutablePath,
                        Sha256 = _expectedWorkerSha256
                    }
                });
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == ErrorCancelled)
        {
            return ValueTask.FromResult(new OfficialUninstallWorkerLaunchResult
            {
                Status = OfficialUninstallWorkerLaunchStatus.UserCanceled
            });
        }
        catch
        {
            return ValueTask.FromResult(Failed());
        }
    }

    internal static IReadOnlyList<string> WorkerArguments(
        OfficialUninstallWorkerLaunchRequest request,
        WindowsOfficialUninstallWorkerMode mode =
            WindowsOfficialUninstallWorkerMode.DevelopmentVerification) =>
        [
            mode switch
            {
                WindowsOfficialUninstallWorkerMode.Production =>
                    "official-uninstall-production-worker",
                WindowsOfficialUninstallWorkerMode.MigrationProduction =>
                    "migration-production-worker",
                _ => "official-uninstall-fake-worker"
            },
            "--pipe-name", request.PipeName,
            "--session-id", request.SessionId,
            "--client-sid", request.Client.UserSid,
            "--client-pid", request.Client.ProcessId.ToString(CultureInfo.InvariantCulture),
            "--client-windows-session",
            request.Client.WindowsSessionId.ToString(CultureInfo.InvariantCulture),
            "--timeout-ms", request.TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture)
        ];

    private static OfficialUninstallWorkerLaunchResult Failed() =>
        new() { Status = OfficialUninstallWorkerLaunchStatus.Failed };

    private bool WorkerHashMatches()
    {
        try
        {
            using var stream = File.Open(
                _workerExecutablePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            var actual = SHA256.HashData(stream);
            var expected = Convert.FromHexString(_expectedWorkerSha256);
            try
            {
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(actual);
                CryptographicOperations.ZeroMemory(expected);
            }
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeSha256(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("The expected worker hash is required.", nameof(value));
        if (value.Length != 64 || !value.All(Uri.IsHexDigit))
            throw new ArgumentException("The expected worker hash is invalid.", nameof(value));
        return value.ToUpperInvariant();
    }
}

public sealed class WindowsOfficialUninstallProductionWorkerLauncher
    : IOfficialUninstallProductionWorkerLauncher
{
    private readonly WindowsOfficialUninstallWorkerLauncher _inner;

    private WindowsOfficialUninstallProductionWorkerLauncher(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        if (!trust.CanLaunchProduction
            || string.IsNullOrWhiteSpace(trust.WorkerExecutablePath)
            || !HasSha256(trust.WorkerEvidence.FileSha256))
        {
            throw new InvalidOperationException(
                "Production worker launch requires trusted package evidence and an exact worker hash.");
        }

        _inner = new WindowsOfficialUninstallWorkerLauncher(
            trust.WorkerExecutablePath,
            trust.WorkerEvidence.FileSha256!,
            WindowsOfficialUninstallWorkerMode.Production);
    }

    public static WindowsOfficialUninstallProductionWorkerLauncher Create(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        ArgumentNullException.ThrowIfNull(trust);
        return new WindowsOfficialUninstallProductionWorkerLauncher(trust);
    }

    public ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
        OfficialUninstallWorkerLaunchRequest request,
        CancellationToken cancellationToken = default) =>
        _inner.LaunchAsync(request, cancellationToken);

    private static bool HasSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed class WindowsMigrationProductionWorkerLauncher
    : IMigrationProductionWorkerLauncher
{
    private readonly WindowsOfficialUninstallWorkerLauncher _inner;

    private WindowsMigrationProductionWorkerLauncher(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        if (!trust.CanLaunchProduction
            || string.IsNullOrWhiteSpace(trust.WorkerExecutablePath)
            || !HasSha256(trust.WorkerEvidence.FileSha256))
        {
            throw new InvalidOperationException(
                "Migration launch requires a same-signer trusted package and exact worker hash.");
        }

        _inner = new WindowsOfficialUninstallWorkerLauncher(
            trust.WorkerExecutablePath,
            trust.WorkerEvidence.FileSha256!,
            WindowsOfficialUninstallWorkerMode.MigrationProduction);
    }

    public static WindowsMigrationProductionWorkerLauncher Create(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        ArgumentNullException.ThrowIfNull(trust);
        return new WindowsMigrationProductionWorkerLauncher(trust);
    }

    public ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
        OfficialUninstallWorkerLaunchRequest request,
        CancellationToken cancellationToken = default) =>
        _inner.LaunchAsync(request, cancellationToken);

    private static bool HasSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public sealed class WindowsOfficialUninstallWorkerImageInspector
    : IOfficialUninstallWorkerImageInspector
{
    private readonly IWindowsProcessImagePathResolver _pathResolver;

    public WindowsOfficialUninstallWorkerImageInspector(
        IWindowsProcessImagePathResolver? pathResolver = null)
    {
        _pathResolver = pathResolver ?? new WindowsProcessImagePathResolver();
    }

    public ValueTask<OfficialUninstallWorkerImageEvidence> InspectAsync(
        IOfficialUninstallWorkerProcess process,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(process);
        cancellationToken.ThrowIfCancellationRequested();
        if (process.ProcessId <= 0 || process.HasExited)
            throw new InvalidOperationException("The worker process is not available for inspection.");

        cancellationToken.ThrowIfCancellationRequested();
        var executablePath = _pathResolver.Resolve(process.ProcessId);
        using var stream = File.Open(
            executablePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read | FileShare.Delete);
        var hash = SHA256.HashData(stream);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new OfficialUninstallWorkerImageEvidence
            {
                ExecutablePath = executablePath,
                Sha256 = Convert.ToHexString(hash)
            });
        }
        finally
        {
            CryptographicOperations.ZeroMemory(hash);
        }
    }

}

internal sealed class WindowsOfficialUninstallWorkerProcess(Process process)
    : IOfficialUninstallWorkerProcess
{
    private bool _disposed;

    public int ProcessId { get; } = process.Id;
    public int WindowsSessionId { get; } = process.SessionId;
    public bool HasExited => process.HasExited;

    public async Task<int> WaitForExitAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);
        await process.WaitForExitAsync(deadline.Token);
        return process.ExitCode;
    }

    public async Task TerminateTreeAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
        _ = await WaitForExitAsync(timeout, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        if (!process.HasExited)
        {
            try
            {
                await TerminateTreeAsync(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Lifecycle client already reported cleanup failure.
            }
        }
        process.Dispose();
        _disposed = true;
    }
}
