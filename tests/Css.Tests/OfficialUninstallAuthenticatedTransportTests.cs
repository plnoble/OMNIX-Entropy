using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Elevated.Uninstall;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class OfficialUninstallAuthenticatedTransportTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 11, 14, 0, 0, TimeSpan.Zero);
    private static readonly byte[] SessionKey = Enumerable.Range(1, 32)
        .Select(value => (byte)value)
        .ToArray();

    [Fact]
    public async Task Valid_authenticated_message_reaches_endpoint_and_returns_correlated_response()
    {
        var calls = 0;
        var draft = ReadyDraft();
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
        {
            calls++;
            return Task.FromResult(SuccessResponse(request.RequestId!));
        });

        var result = await client.SendAsync(draft, endpoint, Now);

        result.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        result.Response.Should().NotBeNull();
        result.Response!.RequestId.Should().Be(draft.RequestId);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task Wrong_key_is_rejected_before_endpoint_handler()
    {
        var calls = 0;
        var wrongKey = SessionKey.Select(value => (byte)(value ^ 0x5A)).ToArray();
        var client = CreateClient(wrongKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
        {
            calls++;
            return Task.FromResult(SuccessResponse(request.RequestId!));
        });

        var result = await client.SendAsync(ReadyDraft(), endpoint, Now);

        result.Status.Should().Be(OfficialUninstallTransportStatus.AuthenticationFailed);
        result.Response.Should().BeNull();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Endpoint_rejects_stale_and_replayed_messages()
    {
        var calls = 0;
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
        {
            calls++;
            return Task.FromResult(SuccessResponse(request.RequestId!));
        });
        var stale = client.CreateMessage(ReadyDraft("stale-request"), Now.AddMinutes(-3));
        var replayable = client.CreateMessage(ReadyDraft("replay-request"), Now);

        var staleResult = await endpoint.HandleAsync(stale, Now);
        var first = await endpoint.HandleAsync(replayable, Now);
        var replay = await endpoint.HandleAsync(replayable, Now.AddSeconds(1));

        staleResult.Status.Should().Be(OfficialUninstallTransportStatus.Expired);
        first.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        replay.Status.Should().Be(OfficialUninstallTransportStatus.ReplayRejected);
        calls.Should().Be(1);
    }

    [Theory]
    [InlineData(-16)]
    [InlineData(1)]
    public async Task Endpoint_rejects_stale_or_future_prepared_consent_before_handler(
        int preparedAtMinuteOffset)
    {
        var calls = 0;
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
        {
            calls++;
            return Task.FromResult(SuccessResponse(request.RequestId!));
        });
        var preparedAt = preparedAtMinuteOffset > 0
            ? Now.AddSeconds(31)
            : Now.AddMinutes(preparedAtMinuteOffset);
        var request = WithPreparedAt(ReadyDraft("prepared-age-request"), preparedAt);

        var result = await client.SendAsync(request, endpoint, Now);

        result.Status.Should().Be(OfficialUninstallTransportStatus.Expired);
        result.Response.Should().BeNull();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Changing_preparation_time_after_authentication_is_rejected()
    {
        var calls = 0;
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
        {
            calls++;
            return Task.FromResult(SuccessResponse(request.RequestId!));
        });
        var original = ReadyDraft("prepared-time-tamper");
        var message = client.CreateMessage(original, Now);
        var changed = message with
        {
            Request = WithPreparedAt(original, Now.AddSeconds(-1))
        };

        var result = await endpoint.HandleAsync(changed, Now);

        result.Status.Should().Be(OfficialUninstallTransportStatus.AuthenticationFailed);
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Endpoint_recomputes_descriptor_hash_and_rejects_mutated_arguments()
    {
        var calls = 0;
        var draft = ReadyDraft("tampered-request");
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
        {
            calls++;
            return Task.FromResult(SuccessResponse(request.RequestId!));
        });
        var message = client.CreateMessage(draft, Now);
        var arguments = draft.Operation!.Arguments.Should()
            .BeOfType<Dictionary<string, object?>>().Subject;
        arguments["arguments"] = "/quiet";

        var result = await endpoint.HandleAsync(message, Now);

        result.Status.Should().Be(OfficialUninstallTransportStatus.InvalidRequest);
        result.Response.Should().BeNull();
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Cancellation_propagates_without_becoming_a_completed_or_failed_response()
    {
        using var cancellation = new CancellationTokenSource();
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("unreachable");
        });
        cancellation.Cancel();

        var action = () => client.SendAsync(
            ReadyDraft("cancel-request"),
            endpoint,
            Now,
            cancellation.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Mismatched_response_is_rejected_instead_of_displayed()
    {
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (_, _) =>
            Task.FromResult(SuccessResponse("different-request")));

        var result = await client.SendAsync(ReadyDraft(), endpoint, Now);

        result.Status.Should().Be(OfficialUninstallTransportStatus.ResponseRejected);
        result.Response.Should().BeNull();
    }

    [Fact]
    public async Task Client_and_endpoint_zero_owned_session_keys_and_refuse_reuse_after_dispose()
    {
        var draft = ReadyDraft("dispose-request");
        var client = CreateClient(SessionKey);
        var endpoint = CreateEndpoint(SessionKey, (request, _) =>
            Task.FromResult(SuccessResponse(request.RequestId!)));
        var message = client.CreateMessage(draft, Now);

        client.Dispose();
        endpoint.Dispose();

        var clientAction = () => client.CreateMessage(draft, Now);
        var endpointAction = () => endpoint.HandleAsync(message, Now);
        clientAction.Should().Throw<ObjectDisposedException>();
        await endpointAction.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public void Transport_is_unregistered_and_has_no_process_or_mutation_authority()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallAuthenticatedInMemoryTransport.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));
        var program = File.ReadAllText(FindRepositoryFile("src", "Css.Elevated", "Program.cs"));

        source.Should().Contain("HMACSHA256");
        source.Should().Contain("FixedTimeEquals");
        source.Should().Contain("ComputeDescriptorSha256");
        source.Should().Contain("PreparedAtUtc");
        source.Should().Contain("official-uninstall-in-memory-v2");
        source.Should().Contain("CryptographicOperations.ZeroMemory");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        app.Should().Contain("#if DEBUG");
        app.Should().Contain("OfficialUninstallAuthenticatedInMemoryEndpoint");
        app.Should().NotContain("OfficialUninstallOperationHandler");
        app.Should().NotContain("WindowsOfficialUninstallerLauncher");
        program.Should().NotContain("OfficialUninstallAuthenticatedInMemory");
    }

    private static OfficialUninstallAuthenticatedInMemoryClient CreateClient(byte[] key) =>
        new(
            sessionId: "session-1",
            sessionKey: key,
            messageIdFactory: () => Guid.NewGuid().ToString("N"),
            nonceFactory: () => Guid.NewGuid().ToString("N"));

    private static OfficialUninstallAuthenticatedInMemoryEndpoint CreateEndpoint(
        byte[] key,
        Func<OfficialUninstallElevatedRequestDraft, CancellationToken,
            Task<OfficialUninstallElevatedResponseEnvelope>> handler) =>
        new("session-1", key, handler);

    private static OfficialUninstallElevatedRequestDraft ReadyDraft(
        string requestId = "transport-request")
    {
        var operation = new OperationDescriptor
        {
            Kind = "uninstall.official.run",
            Title = "Example App official uninstaller",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "snapshot-transport",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "verified transport evidence",
            ConfirmationText = "Run Example App official uninstaller?",
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "Example App",
                ["executablePath"] = @"D:\Software\Example\Uninstall.exe",
                ["arguments"] = "/remove",
                ["snapshotManifestPath"] = @"D:\Evidence\snapshot.json",
                ["snapshotSha256"] = new string('A', 64),
                ["snapshotCanRestoreApplication"] = false,
                ["recoveryMethod"] = OfficialUninstallRecoveryMethod.ReinstallSource.ToString(),
                ["recoveryReference"] = @"D:\Installers\ExampleSetup.exe"
            }
        };
        var gate = new OfficialUninstallExecutionGateResult
        {
            CanRequestExecution = true,
            PrimaryButtonText = "confirm",
            BlockingReasons = [],
            CommandTrust = OfficialUninstallCommandTrustResult.NotEvaluated(),
            Operation = operation
        };
        return OfficialUninstallElevatedRequestComposer.Create(
            gate,
            new OfficialUninstallVisualGateReceipt
            {
                UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
                ScreenshotSha256 = new string('B', 64),
                CapturedAtUtc = Now.AddMinutes(-1),
                RecoveryTruthVisible = true,
                FinalConfirmationVisible = true,
                TechnicalDetailsCollapsedByDefault = true,
                NoExecutionControlDuringPreparation = true
            },
            new OfficialUninstallFinalUserConsent
            {
                ConfirmationText = operation.ConfirmationText,
                ConfirmedAtUtc = Now,
                OfficialCommandConfirmed = true,
                AppsClosedConfirmed = true,
                NoAutomaticUndoAcknowledged = true,
                PostUninstallRescanConfirmed = true,
                ExecutionRequested = true
            },
            requestId,
            Now);
    }

    private static OfficialUninstallElevatedResponseEnvelope SuccessResponse(string requestId) =>
        new()
        {
            RequestId = requestId,
            Result = OperationResult.Ok(payload: new object())
        };

    private static OfficialUninstallElevatedRequestDraft WithPreparedAt(
        OfficialUninstallElevatedRequestDraft source,
        DateTimeOffset preparedAtUtc) =>
        new()
        {
            Status = source.Status,
            MissingRequirements = source.MissingRequirements,
            PreparedAtUtc = preparedAtUtc,
            RequestId = source.RequestId,
            DescriptorSha256 = source.DescriptorSha256,
            Operation = source.Operation
        };

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
}
