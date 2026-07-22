using System.IO.Pipes;
using System.Security.Cryptography;
using System.Security.Principal;
using Css.Core.Migration;
using Css.Ipc.Uninstall;

namespace Css.Ipc.Migration;

public interface IMigrationWorkerLauncher
{
    ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
        OfficialUninstallWorkerLaunchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IMigrationProductionWorkerLauncher : IMigrationWorkerLauncher
{
}

public enum MigrationWorkerLifecycleStatus
{
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

public sealed class MigrationWorkerLifecycleResult
{
    public required MigrationWorkerLifecycleStatus Status { get; init; }
    public MigrationElevatedResponseEnvelope? Response { get; init; }
    public OfficialUninstallSessionBootstrapStatus? BootstrapStatus { get; init; }
    public MigrationTransportStatus? TransportStatus { get; init; }
    public bool ChildExited { get; init; }
}

public sealed class MigrationWorkerLifecycleClient
{
    private readonly IMigrationWorkerLauncher _launcher;
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

    public MigrationWorkerLifecycleClient(
        IMigrationWorkerLauncher launcher,
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
        _responseTimeout = ValidateTimeout(responseTimeout ?? TimeSpan.FromMinutes(5));
        _shutdownTimeout = ValidateTimeout(shutdownTimeout ?? TimeSpan.FromSeconds(5));
    }

    public async Task<MigrationWorkerLifecycleResult> RunProductionOnceAsync(
        MigrationElevatedRequestDraft request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (_launcher is not IMigrationProductionWorkerLauncher)
            return Result(MigrationWorkerLifecycleStatus.ProductionLauncherRejected);
        if (!request.CanSubmit)
            return Result(MigrationWorkerLifecycleStatus.InvalidRequest);

        cancellationToken.ThrowIfCancellationRequested();
        var clientIdentity = _identityProvider.ReadCurrent();
        var launchRequest = new OfficialUninstallWorkerLaunchRequest
        {
            PipeName = $"omnix-migration-{Guid.NewGuid():N}",
            SessionId = $"migration-session-{Guid.NewGuid():N}",
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
            return Result(MigrationWorkerLifecycleStatus.Canceled);
        }
        catch
        {
            return Result(MigrationWorkerLifecycleStatus.LaunchFailed);
        }

        if (launch.Status == OfficialUninstallWorkerLaunchStatus.UserCanceled)
            return Result(MigrationWorkerLifecycleStatus.UserCanceledElevation);
        if (launch.Status != OfficialUninstallWorkerLaunchStatus.Started || launch.Process is null)
            return Result(MigrationWorkerLifecycleStatus.LaunchFailed);

        await using var process = launch.Process;
        MigrationWorkerLifecycleResult exchange;
        try
        {
            exchange = await WorkerImageMatchesAsync(launch, process, cancellationToken)
                ? await ExchangeAsync(
                    request,
                    launchRequest,
                    process,
                    cancellationToken)
                : Result(MigrationWorkerLifecycleStatus.WorkerImageRejected);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            exchange = Result(MigrationWorkerLifecycleStatus.Canceled);
        }
        catch
        {
            exchange = Result(MigrationWorkerLifecycleStatus.TransportFailed);
        }

        var childExited = await EnsureExitAsync(process);
        if (!childExited)
        {
            return new MigrationWorkerLifecycleResult
            {
                Status = MigrationWorkerLifecycleStatus.WorkerExitFailed,
                Response = exchange.Response,
                BootstrapStatus = exchange.BootstrapStatus,
                TransportStatus = exchange.TransportStatus,
                ChildExited = false
            };
        }

        return new MigrationWorkerLifecycleResult
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

    private async Task<MigrationWorkerLifecycleResult> ExchangeAsync(
        MigrationElevatedRequestDraft request,
        OfficialUninstallWorkerLaunchRequest launch,
        IOfficialUninstallWorkerProcess process,
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
                    MigrationWorkerLifecycleStatus.TransportFailed,
                    transport: MigrationTransportStatus.StartupTimedOut);
            }
        }

        OfficialUninstallPipePeerIdentity actualServer;
        try
        {
            actualServer = _peerIdentityReader.ReadServerPeer(pipe);
        }
        catch
        {
            return Result(MigrationWorkerLifecycleStatus.PeerRejected);
        }
        var expectedServer = new OfficialUninstallPipePeerIdentity
        {
            UserSid = launch.Client.UserSid,
            ProcessId = process.ProcessId,
            WindowsSessionId = process.WindowsSessionId
        };
        if (!IdentityMatches(expectedServer, actualServer))
            return Result(MigrationWorkerLifecycleStatus.PeerRejected);

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
                MigrationWorkerLifecycleStatus.BootstrapFailed,
                bootstrap: exception.Status);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result(
                MigrationWorkerLifecycleStatus.BootstrapFailed,
                bootstrap: OfficialUninstallSessionBootstrapStatus.TimedOut);
        }

        using (sessionKey)
        {
            var keyCopy = sessionKey.ExportCopy();
            try
            {
                using var authenticated = new MigrationAuthenticatedClient(
                    launch.SessionId,
                    keyCopy);
                var message = authenticated.CreateMessage(request, _clock());
                using var response = CreateTimeout(cancellationToken, _responseTimeout);
                try
                {
                    await OfficialUninstallPipeFrame.WriteAsync(
                        pipe,
                        MigrationPipeCodec.SerializeRequest(message),
                        response.Token);
                    var payload = await OfficialUninstallPipeFrame.ReadAsync(pipe, response.Token);
                    var transport = MigrationPipeCodec.DeserializeResponse(payload, message);
                    return transport.Status == MigrationTransportStatus.Completed
                        ? new MigrationWorkerLifecycleResult
                        {
                            Status = MigrationWorkerLifecycleStatus.CompletedProduction,
                            Response = transport.Response,
                            TransportStatus = transport.Status,
                            ChildExited = false
                        }
                        : Result(
                            MigrationWorkerLifecycleStatus.TransportFailed,
                            transport: transport.Status);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    return Result(
                        MigrationWorkerLifecycleStatus.ResponseTimedOut,
                        transport: MigrationTransportStatus.ResponseTimedOut);
                }
                catch (MigrationPipeProtocolException exception)
                {
                    return Result(
                        MigrationWorkerLifecycleStatus.TransportFailed,
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

    private static MigrationWorkerLifecycleResult Result(
        MigrationWorkerLifecycleStatus status,
        OfficialUninstallSessionBootstrapStatus? bootstrap = null,
        MigrationTransportStatus? transport = null) =>
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
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(10))
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
