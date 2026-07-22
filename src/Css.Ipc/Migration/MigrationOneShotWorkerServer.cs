using System.IO.Pipes;
using System.Security.Cryptography;
using Css.Core.Migration;
using Css.Ipc.Uninstall;

namespace Css.Ipc.Migration;

public sealed record MigrationOneShotWorkerOptions
{
    public required string PipeName { get; init; }
    public required string SessionId { get; init; }
    public required OfficialUninstallPipePeerIdentity ExpectedClient { get; init; }
    public required OfficialUninstallPipePeerIdentity Worker { get; init; }
    public required TimeSpan Timeout { get; init; }
}

public delegate ValueTask<bool> MigrationOneShotWorkerAuthorization(
    OfficialUninstallPipePeerIdentity actualClient,
    OfficialUninstallPipePeerIdentity worker,
    CancellationToken cancellationToken);

public interface IMigrationOneShotWorkerServer
{
    Task<MigrationTransportResult> ServeOnceAsync(
        MigrationOneShotWorkerOptions options,
        MigrationOneShotWorkerAuthorization authorization,
        Func<MigrationElevatedRequestDraft, CancellationToken,
            Task<MigrationElevatedResponseEnvelope>> responseFactory,
        CancellationToken cancellationToken = default);
}

public sealed class MigrationOneShotWorkerServer : IMigrationOneShotWorkerServer
{
    private readonly IOfficialUninstallPipePeerIdentityReader _identityReader;

    public MigrationOneShotWorkerServer(IOfficialUninstallPipePeerIdentityReader identityReader)
    {
        _identityReader = identityReader ?? throw new ArgumentNullException(nameof(identityReader));
    }

    public async Task<MigrationTransportResult> ServeOnceAsync(
        MigrationOneShotWorkerOptions options,
        MigrationOneShotWorkerAuthorization authorization,
        Func<MigrationElevatedRequestDraft, CancellationToken,
            Task<MigrationElevatedResponseEnvelope>> responseFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(authorization);
        ArgumentNullException.ThrowIfNull(responseFactory);
        var pipeName = ValidateToken(options.PipeName, nameof(options.PipeName));
        var sessionId = ValidateToken(options.SessionId, nameof(options.SessionId));
        var expectedClient = ValidateIdentity(options.ExpectedClient, nameof(options.ExpectedClient));
        var worker = ValidateIdentity(options.Worker, nameof(options.Worker));
        var timeout = ValidateTimeout(options.Timeout);

        await using var pipe = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
            inBufferSize: MigrationPipeCodec.MaximumPayloadBytes + sizeof(int),
            outBufferSize: MigrationPipeCodec.MaximumPayloadBytes + sizeof(int));

        using (var startup = new CancellationTokenSource(timeout))
        using (var linked = CancellationTokenSource.CreateLinkedTokenSource(
                   cancellationToken,
                   startup.Token))
        {
            await pipe.WaitForConnectionAsync(linked.Token);
        }

        var actualClient = _identityReader.ReadClientPeer(pipe);
        RequireIdentity(expectedClient, actualClient, "client");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);

        bool authorized;
        try
        {
            authorized = await authorization(actualClient, worker, deadline.Token);
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            authorized = false;
        }
        if (!authorized)
            return new MigrationTransportResult { Status = MigrationTransportStatus.AuthorizationFailed };

        var context = new OfficialUninstallSessionBootstrapContext
        {
            PipeName = pipeName,
            SessionId = sessionId,
            Client = actualClient,
            Server = worker
        };
        using var sessionKey = await new OfficialUninstallSessionBootstrapServer(
                context,
                new OfficialUninstallSessionBootstrapReplayGuard(),
                timeout: timeout)
            .EstablishAsync(pipe, deadline.Token);
        var keyCopy = sessionKey.ExportCopy();
        try
        {
            using var endpoint = new MigrationAuthenticatedEndpoint(
                sessionId,
                keyCopy,
                responseFactory);
            var requestPayload = await OfficialUninstallPipeFrame.ReadAsync(pipe, deadline.Token);
            var message = MigrationPipeCodec.DeserializeRequest(requestPayload);
            var result = await endpoint.HandleAsync(message, DateTimeOffset.UtcNow, deadline.Token);
            var responsePayload = MigrationPipeCodec.SerializeResponse(message, result);
            await OfficialUninstallPipeFrame.WriteAsync(pipe, responsePayload, deadline.Token);
            return result;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(keyCopy);
        }
    }

    private static string ValidateToken(string value, string name)
    {
        if (!MigrationTransportAuthentication.IsToken(value))
            throw new ArgumentException("The migration worker token is invalid.", name);
        return value;
    }

    private static OfficialUninstallPipePeerIdentity ValidateIdentity(
        OfficialUninstallPipePeerIdentity identity,
        string name)
    {
        ArgumentNullException.ThrowIfNull(identity, name);
        if (string.IsNullOrWhiteSpace(identity.UserSid)
            || identity.ProcessId <= 0
            || identity.WindowsSessionId < 0)
        {
            throw new ArgumentException("The migration worker identity is invalid.", name);
        }
        return identity;
    }

    private static TimeSpan ValidateTimeout(TimeSpan timeout)
    {
        if (timeout < TimeSpan.FromMilliseconds(100) || timeout > TimeSpan.FromMinutes(5))
            throw new ArgumentOutOfRangeException(nameof(timeout));
        return timeout;
    }

    private static void RequireIdentity(
        OfficialUninstallPipePeerIdentity expected,
        OfficialUninstallPipePeerIdentity actual,
        string role)
    {
        if (!string.Equals(expected.UserSid, actual.UserSid, StringComparison.OrdinalIgnoreCase)
            || expected.ProcessId != actual.ProcessId
            || expected.WindowsSessionId != actual.WindowsSessionId)
        {
            throw new InvalidOperationException($"The {role} migration pipe identity does not match.");
        }
    }
}
