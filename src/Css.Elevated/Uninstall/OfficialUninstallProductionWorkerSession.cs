using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;

namespace Css.Elevated.Uninstall;

public sealed class OfficialUninstallProductionWorkerSession
{
    private static readonly TimeSpan MaximumRequestAge = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromSeconds(30);

    private readonly IOfficialUninstallOneShotWorkerServer _server;
    private readonly IOfficialUninstallProductionPackageAuthorizer _authorizer;
    private readonly SafetyOperationPipeline _pipeline;
    private readonly Func<DateTimeOffset> _clock;

    public OfficialUninstallProductionWorkerSession(
        IOfficialUninstallOneShotWorkerServer server,
        IOfficialUninstallProductionPackageAuthorizer authorizer,
        OfficialUninstallOperationHandler handler,
        Func<DateTimeOffset>? clock = null)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        ArgumentNullException.ThrowIfNull(handler);
        _pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public Task<OfficialUninstallTransportResult> ServeOnceAsync(
        OfficialUninstallOneShotWorkerOptions options,
        CancellationToken cancellationToken = default) =>
        _server.ServeOnceAsync(
            options,
            AuthorizeAsync,
            ExecuteAsync,
            cancellationToken);

    private ValueTask<bool> AuthorizeAsync(
        OfficialUninstallPipePeerIdentity actualClient,
        OfficialUninstallPipePeerIdentity worker,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(
            _authorizer.Authorize(actualClient, worker).CanAuthorize);
    }

    private async Task<OfficialUninstallElevatedResponseEnvelope> ExecuteAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken)
    {
        if (!request.CanSubmit
            || request.Operation is null
            || string.IsNullOrWhiteSpace(request.RequestId)
            || !IsFresh(request.PreparedAtUtc)
            || !string.Equals(
                request.DescriptorSha256,
                OfficialUninstallElevatedRequestComposer.ComputeDescriptorSha256(request.Operation),
                StringComparison.OrdinalIgnoreCase))
        {
            return new OfficialUninstallElevatedResponseEnvelope
            {
                RequestId = request.RequestId ?? "invalid-request",
                Result = Refused("The official uninstall request is invalid or stale.")
            };
        }

        return new OfficialUninstallElevatedResponseEnvelope
        {
            RequestId = request.RequestId,
            Result = await _pipeline.ExecuteAsync(request.Operation, cancellationToken)
        };
    }

    private bool IsFresh(DateTimeOffset? preparedAtUtc)
    {
        if (!preparedAtUtc.HasValue)
            return false;
        var age = _clock().ToUniversalTime()
            - preparedAtUtc.Value.ToUniversalTime();
        return age <= MaximumRequestAge && age >= -MaximumClockSkew;
    }

    private static OperationResult Refused(string error) =>
        new()
        {
            Success = false,
            Error = error,
            Payload = new OfficialUninstallHandlerPayload
            {
                UninstallerStarted = false,
                UninstallerCompleted = false,
                RequiresPostScanRetry = false,
                PostScan = OfficialUninstallPostScanResult.NotRun(
                    "The official uninstaller was not started.")
            }
        };
}
