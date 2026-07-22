using System.IO.Pipes;
using System.Security.Cryptography;
using System.Security.Principal;
using Css.Core.Uninstall;

namespace Css.Ipc.Uninstall;

public enum OfficialUninstallWorkerLaunchStatus
{
    Started,
    UserCanceled,
    Failed
}

public sealed record OfficialUninstallWorkerLaunchRequest
{
    public required string PipeName { get; init; }
    public required string SessionId { get; init; }
    public required OfficialUninstallPipePeerIdentity Client { get; init; }
    public required int TimeoutMilliseconds { get; init; }
}

public sealed class OfficialUninstallWorkerLaunchResult
{
    public required OfficialUninstallWorkerLaunchStatus Status { get; init; }
    public IOfficialUninstallWorkerProcess? Process { get; init; }
    public OfficialUninstallWorkerImageExpectation? ImageExpectation { get; init; }
}

public sealed record OfficialUninstallWorkerImageExpectation
{
    public required string ExecutablePath { get; init; }
    public required string Sha256 { get; init; }
}

public sealed record OfficialUninstallWorkerImageEvidence
{
    public required string ExecutablePath { get; init; }
    public required string Sha256 { get; init; }
}

public interface IOfficialUninstallWorkerLauncher
{
    ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
        OfficialUninstallWorkerLaunchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IOfficialUninstallProductionWorkerLauncher
    : IOfficialUninstallWorkerLauncher
{
}

public interface IOfficialUninstallWorkerProcess : IAsyncDisposable
{
    int ProcessId { get; }
    int WindowsSessionId { get; }
    bool HasExited { get; }

    Task<int> WaitForExitAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    Task TerminateTreeAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}

public interface IOfficialUninstallWorkerImageInspector
{
    ValueTask<OfficialUninstallWorkerImageEvidence> InspectAsync(
        IOfficialUninstallWorkerProcess process,
        CancellationToken cancellationToken = default);
}

public interface IOfficialUninstallCurrentProcessIdentityProvider
{
    OfficialUninstallPipePeerIdentity ReadCurrent();
}

public sealed class WindowsOfficialUninstallCurrentProcessIdentityProvider
    : IOfficialUninstallCurrentProcessIdentityProvider
{
    public OfficialUninstallPipePeerIdentity ReadCurrent()
    {
        using var identity = WindowsIdentity.GetCurrent();
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return new OfficialUninstallPipePeerIdentity
        {
            UserSid = identity.User?.Value
                ?? throw new InvalidOperationException("The current Windows SID is unavailable."),
            ProcessId = Environment.ProcessId,
            WindowsSessionId = process.SessionId
        };
    }
}

public enum OfficialUninstallWorkerLifecycleStatus
{
    CompletedFake,
    CompletedProduction,
    ProductionLauncherRejected,
    InvalidRequest,
    UserCanceledElevation,
    LaunchFailed,
    WorkerImageRejected,
    PeerRejected,
    BootstrapFailed,
    ResponseTimedOut,
    TransportFailed,
    WorkerExitFailed,
    Canceled
}

public sealed class OfficialUninstallWorkerLifecycleResult
{
    public required OfficialUninstallWorkerLifecycleStatus Status { get; init; }
    public OfficialUninstallElevatedResponseEnvelope? Response { get; init; }
    public OfficialUninstallSessionBootstrapStatus? BootstrapStatus { get; init; }
    public OfficialUninstallTransportStatus? TransportStatus { get; init; }
    public bool ChildExited { get; init; }
}

public sealed class OfficialUninstallWorkerLifecycleClient
{
    private readonly IOfficialUninstallWorkerLauncher _launcher;
    private readonly IOfficialUninstallWorkerImageInspector _imageInspector;
    private readonly IOfficialUninstallCurrentProcessIdentityProvider _identityProvider;
    private readonly IOfficialUninstallPipePeerIdentityReader _peerIdentityReader;
    private readonly Func<OfficialUninstallSessionBootstrapContext, TimeSpan,
        OfficialUninstallSessionBootstrapClient> _bootstrapFactory;
    private readonly Func<DateTimeOffset> _clock;
    private readonly TimeSpan _startupTimeout;
    private readonly TimeSpan _bootstrapTimeout;
    private readonly TimeSpan _responseTimeout;
    private readonly TimeSpan _shutdownTimeout;

    public OfficialUninstallWorkerLifecycleClient(
        IOfficialUninstallWorkerLauncher launcher,
        IOfficialUninstallWorkerImageInspector imageInspector,
        IOfficialUninstallCurrentProcessIdentityProvider identityProvider,
        IOfficialUninstallPipePeerIdentityReader peerIdentityReader,
        Func<OfficialUninstallSessionBootstrapContext, TimeSpan,
            OfficialUninstallSessionBootstrapClient>? bootstrapFactory = null,
        Func<DateTimeOffset>? clock = null,
        TimeSpan? startupTimeout = null,
        TimeSpan? bootstrapTimeout = null,
        TimeSpan? responseTimeout = null,
        TimeSpan? shutdownTimeout = null)
    {
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _imageInspector = imageInspector ?? throw new ArgumentNullException(nameof(imageInspector));
        _identityProvider = identityProvider ?? throw new ArgumentNullException(nameof(identityProvider));
        _peerIdentityReader = peerIdentityReader ?? throw new ArgumentNullException(nameof(peerIdentityReader));
        _bootstrapFactory = bootstrapFactory
            ?? ((context, timeout) => new OfficialUninstallSessionBootstrapClient(
                context,
                timeout: timeout));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _startupTimeout = ValidateTimeout(startupTimeout ?? TimeSpan.FromSeconds(15));
        _bootstrapTimeout = ValidateTimeout(bootstrapTimeout ?? TimeSpan.FromSeconds(15));
        _responseTimeout = ValidateTimeout(responseTimeout ?? TimeSpan.FromMinutes(2));
        _shutdownTimeout = ValidateTimeout(shutdownTimeout ?? TimeSpan.FromSeconds(5));
    }

    public async Task<OfficialUninstallWorkerLifecycleResult> RunFakeOnceAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken = default)
    {
        if (_launcher is IOfficialUninstallProductionWorkerLauncher)
            return Result(OfficialUninstallWorkerLifecycleStatus.ProductionLauncherRejected);
        return await RunOnceAsync(
            request,
            OfficialUninstallWorkerLifecycleStatus.CompletedFake,
            cancellationToken);
    }

    public async Task<OfficialUninstallWorkerLifecycleResult> RunProductionOnceAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken = default)
    {
        if (_launcher is not IOfficialUninstallProductionWorkerLauncher)
            return Result(OfficialUninstallWorkerLifecycleStatus.ProductionLauncherRejected);
        return await RunOnceAsync(
            request,
            OfficialUninstallWorkerLifecycleStatus.CompletedProduction,
            cancellationToken);
    }

    private async Task<OfficialUninstallWorkerLifecycleResult> RunOnceAsync(
        OfficialUninstallElevatedRequestDraft request,
        OfficialUninstallWorkerLifecycleStatus completedStatus,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.CanSubmit)
            return Result(OfficialUninstallWorkerLifecycleStatus.InvalidRequest);

        cancellationToken.ThrowIfCancellationRequested();
        var clientIdentity = _identityProvider.ReadCurrent();
        var launchRequest = new OfficialUninstallWorkerLaunchRequest
        {
            PipeName = $"omnix-uninstall-{Guid.NewGuid():N}",
            SessionId = $"uninstall-session-{Guid.NewGuid():N}",
            Client = clientIdentity,
            TimeoutMilliseconds = checked((int)Math.Ceiling(
                Math.Max(_startupTimeout.TotalMilliseconds, _responseTimeout.TotalMilliseconds)))
        };

        OfficialUninstallWorkerLaunchResult launch;
        try
        {
            launch = await _launcher.LaunchAsync(launchRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result(OfficialUninstallWorkerLifecycleStatus.Canceled);
        }
        catch
        {
            return Result(OfficialUninstallWorkerLifecycleStatus.LaunchFailed);
        }

        if (launch.Status == OfficialUninstallWorkerLaunchStatus.UserCanceled)
            return Result(OfficialUninstallWorkerLifecycleStatus.UserCanceledElevation);
        if (launch.Status != OfficialUninstallWorkerLaunchStatus.Started || launch.Process is null)
            return Result(OfficialUninstallWorkerLifecycleStatus.LaunchFailed);

        await using var process = launch.Process;
        OfficialUninstallWorkerLifecycleResult exchange;
        try
        {
            exchange = await WorkerImageMatchesAsync(launch, process, cancellationToken)
                ? await ExchangeAsync(
                    request,
                    launchRequest,
                    process,
                    completedStatus,
                    cancellationToken)
                : Result(OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            exchange = Result(OfficialUninstallWorkerLifecycleStatus.Canceled);
        }
        catch
        {
            exchange = Result(OfficialUninstallWorkerLifecycleStatus.TransportFailed);
        }

        var childExited = await EnsureExitAsync(process);
        if (!childExited)
        {
            return new OfficialUninstallWorkerLifecycleResult
            {
                Status = OfficialUninstallWorkerLifecycleStatus.WorkerExitFailed,
                Response = exchange.Response,
                BootstrapStatus = exchange.BootstrapStatus,
                TransportStatus = exchange.TransportStatus,
                ChildExited = false
            };
        }

        return new OfficialUninstallWorkerLifecycleResult
        {
            Status = exchange.Status,
            Response = exchange.Response,
            BootstrapStatus = exchange.BootstrapStatus,
            TransportStatus = exchange.TransportStatus,
            ChildExited = true
        };
    }

    private async ValueTask<bool> WorkerImageMatchesAsync(
        OfficialUninstallWorkerLaunchResult launch,
        IOfficialUninstallWorkerProcess process,
        CancellationToken cancellationToken)
    {
        try
        {
            var expected = launch.ImageExpectation;
            if (expected is null || process.HasExited)
                return false;

            var actual = await _imageInspector.InspectAsync(process, cancellationToken);
            return !process.HasExited
                && PathsMatch(expected.ExecutablePath, actual.ExecutablePath)
                && HashesMatch(expected.Sha256, actual.Sha256);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private async Task<OfficialUninstallWorkerLifecycleResult> ExchangeAsync(
        OfficialUninstallElevatedRequestDraft request,
        OfficialUninstallWorkerLaunchRequest launch,
        IOfficialUninstallWorkerProcess process,
        OfficialUninstallWorkerLifecycleStatus completedStatus,
        CancellationToken cancellationToken)
    {
        await using var pipe = new NamedPipeClientStream(
            ".",
            launch.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous,
            TokenImpersonationLevel.Impersonation,
            HandleInheritability.None);
        using (var startup = CreateTimeout(cancellationToken, _startupTimeout))
        {
            try
            {
                await pipe.ConnectAsync(startup.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Result(
                    OfficialUninstallWorkerLifecycleStatus.TransportFailed,
                    transport: OfficialUninstallTransportStatus.StartupTimedOut);
            }
        }

        OfficialUninstallPipePeerIdentity actualServer;
        try
        {
            actualServer = _peerIdentityReader.ReadServerPeer(pipe);
        }
        catch
        {
            return Result(OfficialUninstallWorkerLifecycleStatus.PeerRejected);
        }
        var expectedServer = new OfficialUninstallPipePeerIdentity
        {
            UserSid = launch.Client.UserSid,
            ProcessId = process.ProcessId,
            WindowsSessionId = process.WindowsSessionId
        };
        if (!IdentityMatches(expectedServer, actualServer))
            return Result(OfficialUninstallWorkerLifecycleStatus.PeerRejected);

        var context = new OfficialUninstallSessionBootstrapContext
        {
            PipeName = launch.PipeName,
            SessionId = launch.SessionId,
            Client = launch.Client,
            Server = actualServer
        };
        OfficialUninstallSessionKey sessionKey;
        try
        {
            sessionKey = await _bootstrapFactory(context, _bootstrapTimeout)
                .EstablishAsync(pipe, cancellationToken);
        }
        catch (OfficialUninstallSessionBootstrapException exception)
        {
            return Result(
                OfficialUninstallWorkerLifecycleStatus.BootstrapFailed,
                bootstrap: exception.Status);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result(
                OfficialUninstallWorkerLifecycleStatus.BootstrapFailed,
                bootstrap: OfficialUninstallSessionBootstrapStatus.TimedOut);
        }

        using (sessionKey)
        {
            var keyCopy = sessionKey.ExportCopy();
            try
            {
                using var authenticated = new OfficialUninstallAuthenticatedInMemoryClient(
                    launch.SessionId,
                    keyCopy);
                var message = authenticated.CreateMessage(request, _clock());
                using var response = CreateTimeout(cancellationToken, _responseTimeout);
                try
                {
                    await OfficialUninstallPipeFrame.WriteAsync(
                        pipe,
                        OfficialUninstallPipeCodec.SerializeRequest(message),
                        response.Token);
                    var payload = await OfficialUninstallPipeFrame.ReadAsync(pipe, response.Token);
                    var transport = OfficialUninstallPipeCodec.DeserializeResponse(payload, message);
                    return transport.Status == OfficialUninstallTransportStatus.Completed
                        ? new OfficialUninstallWorkerLifecycleResult
                        {
                            Status = completedStatus,
                            Response = transport.Response,
                            TransportStatus = transport.Status,
                            ChildExited = false
                        }
                        : Result(
                            OfficialUninstallWorkerLifecycleStatus.TransportFailed,
                            transport: transport.Status);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    return Result(
                        OfficialUninstallWorkerLifecycleStatus.ResponseTimedOut,
                        transport: OfficialUninstallTransportStatus.ResponseTimedOut);
                }
                catch (OfficialUninstallPipeProtocolException exception)
                {
                    return Result(
                        OfficialUninstallWorkerLifecycleStatus.TransportFailed,
                        transport: exception.Status);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(keyCopy);
            }
        }
    }

    private async Task<bool> EnsureExitAsync(IOfficialUninstallWorkerProcess process)
    {
        if (process.HasExited)
            return true;
        try
        {
            _ = await process.WaitForExitAsync(_shutdownTimeout);
            return process.HasExited;
        }
        catch
        {
            try
            {
                await process.TerminateTreeAsync(_shutdownTimeout);
                return process.HasExited;
            }
            catch
            {
                return false;
            }
        }
    }

    private static OfficialUninstallWorkerLifecycleResult Result(
        OfficialUninstallWorkerLifecycleStatus status,
        OfficialUninstallSessionBootstrapStatus? bootstrap = null,
        OfficialUninstallTransportStatus? transport = null) =>
        new()
        {
            Status = status,
            BootstrapStatus = bootstrap,
            TransportStatus = transport,
            ChildExited = false
        };

    private static bool IdentityMatches(
        OfficialUninstallPipePeerIdentity expected,
        OfficialUninstallPipePeerIdentity actual) =>
        string.Equals(expected.UserSid, actual.UserSid, StringComparison.OrdinalIgnoreCase)
        && expected.ProcessId == actual.ProcessId
        && expected.WindowsSessionId == actual.WindowsSessionId;

    private static bool PathsMatch(string expected, string actual)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(expected),
                Path.GetFullPath(actual),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool HashesMatch(string expected, string actual)
    {
        if (expected.Length != 64 || actual.Length != 64
            || !expected.All(Uri.IsHexDigit) || !actual.All(Uri.IsHexDigit))
        {
            return false;
        }

        var expectedBytes = Convert.FromHexString(expected);
        var actualBytes = Convert.FromHexString(actual);
        try
        {
            return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(expectedBytes);
            CryptographicOperations.ZeroMemory(actualBytes);
        }
    }

    private static TimeSpan ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(5))
            throw new ArgumentOutOfRangeException(nameof(timeout));
        return timeout;
    }

    private static CancellationTokenSource CreateTimeout(
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        return linked;
    }
}
