using Css.Core.Migration;
using Css.Core.Operations;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using System.Security.Cryptography;

namespace Css.Elevated.Migration;

public sealed class MigrationProductionWorkerSession
{
    private static readonly TimeSpan MaximumRequestAge = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromSeconds(30);

    private readonly IMigrationOneShotWorkerServer _server;
    private readonly IMigrationProductionPackageAuthorizer _authorizer;
    private readonly SafetyOperationPipeline _pipeline;
    private readonly Func<DateTimeOffset> _clock;

    public MigrationProductionWorkerSession(
        IMigrationOneShotWorkerServer server,
        IMigrationProductionPackageAuthorizer authorizer,
        MigrationOperationHandler handler,
        Func<DateTimeOffset>? clock = null)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        ArgumentNullException.ThrowIfNull(handler);
        _pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public Task<MigrationTransportResult> ServeOnceAsync(
        MigrationOneShotWorkerOptions options,
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

    private async Task<MigrationElevatedResponseEnvelope> ExecuteAsync(
        MigrationElevatedRequestDraft request,
        CancellationToken cancellationToken)
    {
        if (!request.CanSubmit
            || request.Operation is null
            || string.IsNullOrWhiteSpace(request.RequestId)
            || !IsFresh(request.PreparedAtUtc)
            || !HashesMatch(
                request.DescriptorSha256,
                ComputeHashSafely(request.Operation)))
        {
            return Refused(request.RequestId);
        }

        return new MigrationElevatedResponseEnvelope
        {
            RequestId = request.RequestId,
            Result = await _pipeline.ExecuteAsync(request.Operation, cancellationToken)
        };
    }

    private bool IsFresh(DateTimeOffset? preparedAtUtc)
    {
        if (!preparedAtUtc.HasValue)
            return false;
        var age = _clock().ToUniversalTime() - preparedAtUtc.Value.ToUniversalTime();
        return age <= MaximumRequestAge && age >= -MaximumClockSkew;
    }

    private static string? ComputeHashSafely(OperationDescriptor operation)
    {
        try
        {
            return MigrationElevatedRequestComposer.ComputeDescriptorSha256(operation);
        }
        catch
        {
            return null;
        }
    }

    private static bool HashesMatch(string? expected, string? actual)
    {
        if (expected is not { Length: 64 }
            || actual is not { Length: 64 }
            || !expected.All(Uri.IsHexDigit)
            || !actual.All(Uri.IsHexDigit))
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

    private static MigrationElevatedResponseEnvelope Refused(string? requestId) =>
        new()
        {
            RequestId = string.IsNullOrWhiteSpace(requestId) ? "invalid-request" : requestId,
            Result = new OperationResult
            {
                Success = false,
                Error = "Migration request is invalid or stale.",
                Payload = new MigrationExecutionResult
                {
                    Status = MigrationExecutionStatus.Refused,
                    Summary = "Migration did not start.",
                    RollbackSucceeded = true
                }
            }
        };
}
