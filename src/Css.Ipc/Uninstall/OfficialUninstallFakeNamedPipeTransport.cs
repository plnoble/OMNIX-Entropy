using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Css.Core.Uninstall;
using Microsoft.Win32.SafeHandles;

namespace Css.Ipc.Uninstall;

public sealed record OfficialUninstallPipePeerIdentity
{
    public required string UserSid { get; init; }
    public required int ProcessId { get; init; }
    public required int WindowsSessionId { get; init; }
}

public interface IOfficialUninstallPipePeerIdentityReader
{
    OfficialUninstallPipePeerIdentity ReadClientPeer(NamedPipeServerStream pipe);
    OfficialUninstallPipePeerIdentity ReadServerPeer(NamedPipeClientStream pipe);
}

public sealed class WindowsOfficialUninstallPipePeerIdentityReader
    : IOfficialUninstallPipePeerIdentityReader
{
    public OfficialUninstallPipePeerIdentity ReadClientPeer(NamedPipeServerStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        if (!pipe.IsConnected)
            throw new InvalidOperationException("The server pipe is not connected.");
        if (!GetNamedPipeClientProcessId(pipe.SafePipeHandle, out var processId)
            || processId == 0
            || processId > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Could not read the named-pipe client process id ({Marshal.GetLastWin32Error()}).");
        }

        string? sid = null;
        pipe.RunAsClient(() =>
        {
            using var identity = WindowsIdentity.GetCurrent();
            sid = identity.User?.Value;
        });
        if (string.IsNullOrWhiteSpace(sid))
            throw new InvalidOperationException("Could not read the named-pipe client SID.");

        return CreateIdentity(sid, checked((int)processId));
    }

    public OfficialUninstallPipePeerIdentity ReadServerPeer(NamedPipeClientStream pipe)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        if (!pipe.IsConnected)
            throw new InvalidOperationException("The client pipe is not connected.");
        if (!GetNamedPipeServerProcessId(pipe.SafePipeHandle, out var processId)
            || processId == 0
            || processId > int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Could not read the named-pipe server process id ({Marshal.GetLastWin32Error()}).");
        }

        // CurrentUserOnly rejects a different Windows user before this read. The
        // server PID is then correlated with the process started for this session.
        using var identity = WindowsIdentity.GetCurrent();
        var sid = identity.User?.Value;
        if (string.IsNullOrWhiteSpace(sid))
            throw new InvalidOperationException("Could not read the current Windows SID.");
        return CreateIdentity(sid, checked((int)processId));
    }

    private static OfficialUninstallPipePeerIdentity CreateIdentity(string sid, int processId)
    {
        using var process = Process.GetProcessById(processId);
        return new OfficialUninstallPipePeerIdentity
        {
            UserSid = sid,
            ProcessId = processId,
            WindowsSessionId = process.SessionId
        };
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(
        SafePipeHandle pipe,
        out uint clientProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetNamedPipeServerProcessId(
        SafePipeHandle pipe,
        out uint serverProcessId);
}

public sealed class OfficialUninstallFakeNamedPipeServer
{
    private readonly string _pipeName;
    private readonly OfficialUninstallPipePeerIdentity _expectedClient;
    private readonly IOfficialUninstallPipePeerIdentityReader _identityReader;
    private readonly OfficialUninstallAuthenticatedInMemoryEndpoint _endpoint;
    private readonly Func<DateTimeOffset> _clock;
    private readonly TimeSpan _startupTimeout;
    private readonly TimeSpan _responseTimeout;

    public OfficialUninstallFakeNamedPipeServer(
        string pipeName,
        OfficialUninstallPipePeerIdentity expectedClient,
        IOfficialUninstallPipePeerIdentityReader identityReader,
        OfficialUninstallAuthenticatedInMemoryEndpoint endpoint,
        Func<DateTimeOffset>? clock = null,
        TimeSpan? startupTimeout = null,
        TimeSpan? responseTimeout = null)
    {
        _pipeName = ValidatePipeName(pipeName);
        _expectedClient = ValidateIdentity(expectedClient);
        _identityReader = identityReader ?? throw new ArgumentNullException(nameof(identityReader));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _startupTimeout = ValidateTimeout(startupTimeout ?? TimeSpan.FromSeconds(15), "startup");
        _responseTimeout = ValidateTimeout(responseTimeout ?? TimeSpan.FromMinutes(2), "response");
    }

    public async Task<OfficialUninstallTransportResult> ServeOnceAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var pipe = new NamedPipeServerStream(
            _pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
            inBufferSize: OfficialUninstallPipeCodec.MaximumPayloadBytes + sizeof(int),
            outBufferSize: OfficialUninstallPipeCodec.MaximumPayloadBytes + sizeof(int));

        using (var startup = CreateTimeout(cancellationToken, _startupTimeout))
        {
            try
            {
                await pipe.WaitForConnectionAsync(startup.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Result(OfficialUninstallTransportStatus.StartupTimedOut);
            }
        }

        OfficialUninstallPipePeerIdentity actualClient;
        try
        {
            actualClient = _identityReader.ReadClientPeer(pipe);
        }
        catch
        {
            return Result(OfficialUninstallTransportStatus.PeerRejected);
        }
        if (!IdentityMatches(_expectedClient, actualClient))
            return Result(OfficialUninstallTransportStatus.PeerRejected);

        OfficialUninstallAuthenticatedMessage? message = null;
        using var response = CreateTimeout(cancellationToken, _responseTimeout);
        try
        {
            var payload = await OfficialUninstallPipeFrame.ReadAsync(pipe, response.Token);
            message = OfficialUninstallPipeCodec.DeserializeRequest(payload);
            var endpointResult = await _endpoint.HandleAsync(message, _clock(), response.Token);
            var responsePayload = OfficialUninstallPipeCodec.SerializeResponse(message, endpointResult);
            try
            {
                await OfficialUninstallPipeFrame.WriteAsync(pipe, responsePayload, response.Token);
            }
            catch (Exception exception) when (
                endpointResult.Status != OfficialUninstallTransportStatus.Completed
                && exception is IOException or EndOfStreamException or ObjectDisposedException)
            {
                // Preserve the security rejection even if an untrusted caller closes
                // before reading it; the endpoint was never invoked for execution.
                return endpointResult;
            }
            return endpointResult;
        }
        catch (OfficialUninstallPipeProtocolException exception)
        {
            return Result(exception.Status);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            if (message is not null)
                await TryWriteTimeoutResponseAsync(pipe, message);
            return Result(OfficialUninstallTransportStatus.ResponseTimedOut);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException
                or EndOfStreamException
                or ObjectDisposedException)
        {
            return Result(OfficialUninstallTransportStatus.ConnectionFailed);
        }
        catch
        {
            return Result(OfficialUninstallTransportStatus.EndpointFailed);
        }
    }

    private static async Task TryWriteTimeoutResponseAsync(
        NamedPipeServerStream pipe,
        OfficialUninstallAuthenticatedMessage message)
    {
        if (!pipe.IsConnected)
            return;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            var payload = OfficialUninstallPipeCodec.SerializeResponse(
                message,
                Result(OfficialUninstallTransportStatus.ResponseTimedOut));
            await OfficialUninstallPipeFrame.WriteAsync(pipe, payload, timeout.Token);
        }
        catch
        {
            // The caller already receives a truthful connection/timeout result.
        }
    }

    private static OfficialUninstallTransportResult Result(
        OfficialUninstallTransportStatus status) =>
        new() { Status = status };

    internal static bool IdentityMatches(
        OfficialUninstallPipePeerIdentity expected,
        OfficialUninstallPipePeerIdentity actual) =>
        string.Equals(expected.UserSid, actual.UserSid, StringComparison.OrdinalIgnoreCase)
        && expected.ProcessId == actual.ProcessId
        && expected.WindowsSessionId == actual.WindowsSessionId;

    internal static string ValidatePipeName(string pipeName)
    {
        if (!TransportAuthentication.IsValidToken(pipeName)
            || pipeName.IndexOfAny(['\\', '/']) >= 0)
        {
            throw new ArgumentException("The pipe name is invalid.", nameof(pipeName));
        }
        return pipeName;
    }

    internal static OfficialUninstallPipePeerIdentity ValidateIdentity(
        OfficialUninstallPipePeerIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (string.IsNullOrWhiteSpace(identity.UserSid)
            || identity.UserSid.Length > 256
            || identity.ProcessId <= 0
            || identity.WindowsSessionId < 0)
        {
            throw new ArgumentException("The expected pipe identity is invalid.", nameof(identity));
        }
        return identity;
    }

    internal static TimeSpan ValidateTimeout(TimeSpan timeout, string name)
    {
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromMinutes(5))
            throw new ArgumentOutOfRangeException(name, "The timeout must be between zero and five minutes.");
        return timeout;
    }

    internal static CancellationTokenSource CreateTimeout(
        CancellationToken cancellationToken,
        TimeSpan timeout)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        return linked;
    }
}

public sealed class OfficialUninstallFakeNamedPipeClient
{
    private readonly string _pipeName;
    private readonly OfficialUninstallPipePeerIdentity _expectedServer;
    private readonly IOfficialUninstallPipePeerIdentityReader _identityReader;
    private readonly TimeSpan _startupTimeout;
    private readonly TimeSpan _responseTimeout;

    public OfficialUninstallFakeNamedPipeClient(
        string pipeName,
        OfficialUninstallPipePeerIdentity expectedServer,
        IOfficialUninstallPipePeerIdentityReader identityReader,
        TimeSpan? startupTimeout = null,
        TimeSpan? responseTimeout = null)
    {
        _pipeName = OfficialUninstallFakeNamedPipeServer.ValidatePipeName(pipeName);
        _expectedServer = OfficialUninstallFakeNamedPipeServer.ValidateIdentity(expectedServer);
        _identityReader = identityReader ?? throw new ArgumentNullException(nameof(identityReader));
        _startupTimeout = OfficialUninstallFakeNamedPipeServer.ValidateTimeout(
            startupTimeout ?? TimeSpan.FromSeconds(15),
            "startup");
        _responseTimeout = OfficialUninstallFakeNamedPipeServer.ValidateTimeout(
            responseTimeout ?? TimeSpan.FromMinutes(2),
            "response");
    }

    public async Task<OfficialUninstallTransportResult> SendAsync(
        OfficialUninstallAuthenticatedMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        await using var pipe = new NamedPipeClientStream(
            ".",
            _pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous,
            TokenImpersonationLevel.Impersonation,
            HandleInheritability.None);

        using (var startup = OfficialUninstallFakeNamedPipeServer.CreateTimeout(
                   cancellationToken,
                   _startupTimeout))
        {
            try
            {
                await pipe.ConnectAsync(startup.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Result(OfficialUninstallTransportStatus.StartupTimedOut);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (IOException)
            {
                return Result(OfficialUninstallTransportStatus.ConnectionFailed);
            }
        }

        OfficialUninstallPipePeerIdentity actualServer;
        try
        {
            actualServer = _identityReader.ReadServerPeer(pipe);
        }
        catch
        {
            return Result(OfficialUninstallTransportStatus.PeerRejected);
        }
        if (!OfficialUninstallFakeNamedPipeServer.IdentityMatches(_expectedServer, actualServer))
            return Result(OfficialUninstallTransportStatus.PeerRejected);

        using var response = OfficialUninstallFakeNamedPipeServer.CreateTimeout(
            cancellationToken,
            _responseTimeout);
        try
        {
            var payload = OfficialUninstallPipeCodec.SerializeRequest(message);
            await OfficialUninstallPipeFrame.WriteAsync(pipe, payload, response.Token);
            var responsePayload = await OfficialUninstallPipeFrame.ReadAsync(pipe, response.Token);
            return OfficialUninstallPipeCodec.DeserializeResponse(responsePayload, message);
        }
        catch (OfficialUninstallPipeProtocolException exception)
        {
            return Result(exception.Status);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result(OfficialUninstallTransportStatus.ResponseTimedOut);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is IOException
                or EndOfStreamException
                or ObjectDisposedException)
        {
            return Result(OfficialUninstallTransportStatus.ConnectionFailed);
        }
    }

    private static OfficialUninstallTransportResult Result(
        OfficialUninstallTransportStatus status) =>
        new() { Status = status };
}
