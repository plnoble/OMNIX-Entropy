using System.IO.Pipes;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Elevated.Uninstall;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallSerializedNamedPipeTransportTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 12, 8, 0, 0, TimeSpan.Zero);
    private static readonly byte[] SessionKey = Enumerable.Range(1, 32)
        .Select(value => (byte)value)
        .ToArray();
    private static readonly OfficialUninstallPipePeerIdentity ExpectedPeer = new()
    {
        UserSid = "S-1-5-21-1000",
        ProcessId = Environment.ProcessId,
        WindowsSessionId = 7
    };

    [Fact]
    public void Codec_round_trips_a_strict_request_without_losing_descriptor_types()
    {
        var draft = ReadyDraft();
        var message = CreateAuthenticatedClient().CreateMessage(draft, Now);

        var payload = OfficialUninstallPipeCodec.SerializeRequest(message);
        var decoded = OfficialUninstallPipeCodec.DeserializeRequest(payload);

        payload.Should().HaveCountLessThanOrEqualTo(OfficialUninstallPipeCodec.MaximumPayloadBytes);
        decoded.RequestId.Should().Be(message.RequestId);
        decoded.Request.PreparedAtUtc.Should().Be(message.Request.PreparedAtUtc);
        decoded.DescriptorSha256.Should().Be(message.DescriptorSha256);
        decoded.AuthenticationTag.Should().Equal(message.AuthenticationTag);
        decoded.Request.Operation!.Arguments["arguments"].Should().Be("/remove");
        decoded.Request.Operation.Arguments["snapshotCanRestoreApplication"].Should().Be(false);
    }

    [Fact]
    public void Codec_rejects_oversized_malformed_wrong_schema_and_unknown_argument_types()
    {
        var oversized = new byte[OfficialUninstallPipeCodec.MaximumPayloadBytes + 1];
        var malformed = Encoding.UTF8.GetBytes("{not-json");
        var wrongSchema = Encoding.UTF8.GetBytes(
            "{\"schemaVersion\":99,\"messageType\":\"official-uninstall-request\"}");
        var draft = ReadyDraft();
        var arguments = draft.Operation!.Arguments.Should()
            .BeOfType<Dictionary<string, object?>>().Subject;
        arguments["unsupported"] = DateTimeOffset.UtcNow;

        var oversizedAction = () => OfficialUninstallPipeCodec.DeserializeRequest(oversized);
        var malformedAction = () => OfficialUninstallPipeCodec.DeserializeRequest(malformed);
        var wrongSchemaAction = () => OfficialUninstallPipeCodec.DeserializeRequest(wrongSchema);
        var unknownTypeAction = () => OfficialUninstallPipeCodec.SerializeRequest(
            CreateAuthenticatedClient().CreateMessage(draft, Now));

        oversizedAction.Should().Throw<OfficialUninstallPipeProtocolException>()
            .Which.Status.Should().Be(OfficialUninstallTransportStatus.PayloadRejected);
        malformedAction.Should().Throw<OfficialUninstallPipeProtocolException>()
            .Which.Status.Should().Be(OfficialUninstallTransportStatus.ProtocolRejected);
        wrongSchemaAction.Should().Throw<OfficialUninstallPipeProtocolException>()
            .Which.Status.Should().Be(OfficialUninstallTransportStatus.ProtocolRejected);
        unknownTypeAction.Should().Throw<OfficialUninstallPipeProtocolException>()
            .Which.Status.Should().Be(OfficialUninstallTransportStatus.ProtocolRejected);
    }

    [Fact]
    public async Task Real_named_pipe_round_trip_reaches_only_the_injected_fake_endpoint()
    {
        var calls = 0;
        var pipeName = UniquePipeName();
        var identityReader = new FixedIdentityReader(ExpectedPeer, ExpectedPeer);
        var endpoint = CreateEndpoint((request, _) =>
        {
            calls++;
            return Task.FromResult(SafeResponse(request.RequestId!));
        });
        var server = CreateServer(pipeName, identityReader, endpoint);
        var client = CreatePipeClient(pipeName, identityReader);
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);

        var serverTask = server.ServeOnceAsync();
        var clientResult = await client.SendAsync(message);
        var serverResult = await serverTask;

        serverResult.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        clientResult.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        clientResult.Response!.RequestId.Should().Be(message.RequestId);
        clientResult.Response.Result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>();
        calls.Should().Be(1);
    }

    [Fact]
    public async Task Windows_reader_derives_current_sid_pid_and_session_from_the_live_pipe()
    {
        using var windowsIdentity = WindowsIdentity.GetCurrent();
        using var currentProcess = Process.GetCurrentProcess();
        var expected = new OfficialUninstallPipePeerIdentity
        {
            UserSid = windowsIdentity.User!.Value,
            ProcessId = Environment.ProcessId,
            WindowsSessionId = currentProcess.SessionId
        };
        var pipeName = UniquePipeName();
        var identityReader = new WindowsOfficialUninstallPipePeerIdentityReader();
        var endpoint = CreateEndpoint((request, _) =>
            Task.FromResult(SafeResponse(request.RequestId!)));
        var server = new OfficialUninstallFakeNamedPipeServer(
            pipeName,
            expected,
            identityReader,
            endpoint,
            () => Now,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(2));
        var client = new OfficialUninstallFakeNamedPipeClient(
            pipeName,
            expected,
            identityReader,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(2));
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);

        var serverTask = server.ServeOnceAsync();
        var clientResult = await client.SendAsync(message);
        var serverResult = await serverTask;

        serverResult.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        clientResult.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
    }

    [Theory]
    [InlineData("S-1-5-21-other", 0, 7)]
    [InlineData("S-1-5-21-1000", 1, 7)]
    [InlineData("S-1-5-21-1000", 0, 8)]
    public async Task Wrong_client_sid_pid_or_session_is_rejected_before_the_endpoint(
        string sid,
        int processIdDelta,
        int windowsSessionId)
    {
        var calls = 0;
        var pipeName = UniquePipeName();
        var actualPeer = new OfficialUninstallPipePeerIdentity
        {
            UserSid = sid,
            ProcessId = ExpectedPeer.ProcessId + processIdDelta,
            WindowsSessionId = windowsSessionId
        };
        var identityReader = new FixedIdentityReader(actualPeer, ExpectedPeer);
        var endpoint = CreateEndpoint((request, _) =>
        {
            calls++;
            return Task.FromResult(SafeResponse(request.RequestId!));
        });
        var server = CreateServer(pipeName, identityReader, endpoint);

        var serverTask = server.ServeOnceAsync();
        await ConnectAndCloseAsync(pipeName);
        var result = await serverTask;

        result.Status.Should().Be(OfficialUninstallTransportStatus.PeerRejected);
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Client_rejects_wrong_server_pid_before_sending_a_request()
    {
        var pipeName = UniquePipeName();
        var wrongServer = ExpectedPeer with { ProcessId = ExpectedPeer.ProcessId + 1 };
        var identityReader = new FixedIdentityReader(ExpectedPeer, wrongServer);
        var endpoint = CreateEndpoint((request, _) =>
            Task.FromResult(SafeResponse(request.RequestId!)));
        var server = CreateServer(pipeName, identityReader, endpoint);
        var client = CreatePipeClient(pipeName, identityReader);
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);

        var serverTask = server.ServeOnceAsync();
        var result = await client.SendAsync(message);
        await serverTask;

        result.Status.Should().Be(OfficialUninstallTransportStatus.PeerRejected);
    }

    [Fact]
    public async Task Tampered_serialized_operation_is_rejected_before_the_fake_endpoint()
    {
        var calls = 0;
        var pipeName = UniquePipeName();
        var identityReader = new FixedIdentityReader(ExpectedPeer, ExpectedPeer);
        var endpoint = CreateEndpoint((request, _) =>
        {
            calls++;
            return Task.FromResult(SafeResponse(request.RequestId!));
        });
        var server = CreateServer(pipeName, identityReader, endpoint);
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);
        var payload = OfficialUninstallPipeCodec.SerializeRequest(message);
        var tampered = Encoding.UTF8.GetBytes(
            Encoding.UTF8.GetString(payload).Replace("/remove", "/quiet", StringComparison.Ordinal));

        var serverTask = server.ServeOnceAsync();
        await SendRawFrameAndCloseAsync(pipeName, tampered);
        var result = await serverTask;

        result.Status.Should().Be(OfficialUninstallTransportStatus.InvalidRequest);
        calls.Should().Be(0);
    }

    [Fact]
    public void Tampered_or_mismatched_serialized_response_is_rejected()
    {
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);
        var result = new OfficialUninstallTransportResult
        {
            Status = OfficialUninstallTransportStatus.Completed,
            Response = SafeResponse(message.RequestId)
        };
        var payload = OfficialUninstallPipeCodec.SerializeResponse(message, result);
        var json = Encoding.UTF8.GetString(payload);
        json.Should().NotContain("Secret");
        json.Should().NotContain(@"C:\Users\Example");
        var tampered = Encoding.UTF8.GetBytes(
            json.Replace("\"exitCode\":0", "\"exitCode\":9", StringComparison.Ordinal));
        var otherMessage = message with { MessageId = "different-message" };

        var tamperedAction = () => OfficialUninstallPipeCodec.DeserializeResponse(tampered, message);
        var mismatchedAction = () => OfficialUninstallPipeCodec.DeserializeResponse(payload, otherMessage);

        tamperedAction.Should().Throw<OfficialUninstallPipeProtocolException>()
            .Which.Status.Should().Be(OfficialUninstallTransportStatus.ResponseRejected);
        mismatchedAction.Should().Throw<OfficialUninstallPipeProtocolException>()
            .Which.Status.Should().Be(OfficialUninstallTransportStatus.ResponseRejected);
    }

    [Fact]
    public async Task Oversized_frame_is_rejected_before_the_fake_endpoint()
    {
        var calls = 0;
        var pipeName = UniquePipeName();
        var identityReader = new FixedIdentityReader(ExpectedPeer, ExpectedPeer);
        var endpoint = CreateEndpoint((request, _) =>
        {
            calls++;
            return Task.FromResult(SafeResponse(request.RequestId!));
        });
        var server = CreateServer(pipeName, identityReader, endpoint);

        var serverTask = server.ServeOnceAsync();
        await SendRawLengthAndCloseAsync(
            pipeName,
            OfficialUninstallPipeCodec.MaximumPayloadBytes + 1);
        var result = await serverTask;

        result.Status.Should().Be(OfficialUninstallTransportStatus.PayloadRejected);
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Replay_across_two_pipe_connections_is_rejected()
    {
        var calls = 0;
        var pipeName = UniquePipeName();
        var identityReader = new FixedIdentityReader(ExpectedPeer, ExpectedPeer);
        var endpoint = CreateEndpoint((request, _) =>
        {
            calls++;
            return Task.FromResult(SafeResponse(request.RequestId!));
        });
        var client = CreatePipeClient(pipeName, identityReader);
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);

        var firstServer = CreateServer(pipeName, identityReader, endpoint);
        var firstServerTask = firstServer.ServeOnceAsync();
        var first = await client.SendAsync(message);
        await firstServerTask;

        var secondServer = CreateServer(pipeName, identityReader, endpoint);
        var secondServerTask = secondServer.ServeOnceAsync();
        var replay = await client.SendAsync(message);
        await secondServerTask;

        first.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        replay.Status.Should().Be(OfficialUninstallTransportStatus.ReplayRejected);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task Startup_and_response_timeouts_are_explicit_and_external_cancellation_propagates()
    {
        var identityReader = new FixedIdentityReader(ExpectedPeer, ExpectedPeer);
        var idleServer = CreateServer(
            UniquePipeName(),
            identityReader,
            CreateEndpoint((request, _) => Task.FromResult(SafeResponse(request.RequestId!))),
            startupTimeout: TimeSpan.FromMilliseconds(100));

        var startupResult = await idleServer.ServeOnceAsync();

        startupResult.Status.Should().Be(OfficialUninstallTransportStatus.StartupTimedOut);

        var slowPipeName = UniquePipeName();
        var slowEndpoint = CreateEndpoint(async (_, token) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
            throw new InvalidOperationException("unreachable");
        });
        var slowServer = CreateServer(
            slowPipeName,
            identityReader,
            slowEndpoint,
            responseTimeout: TimeSpan.FromMilliseconds(120));
        var client = CreatePipeClient(
            slowPipeName,
            identityReader,
            responseTimeout: TimeSpan.FromSeconds(2));
        var message = CreateAuthenticatedClient().CreateMessage(ReadyDraft(), Now);

        var slowServerTask = slowServer.ServeOnceAsync();
        var clientResult = await client.SendAsync(message);
        var slowServerResult = await slowServerTask;

        slowServerResult.Status.Should().Be(OfficialUninstallTransportStatus.ResponseTimedOut);
        clientResult.Status.Should().Be(OfficialUninstallTransportStatus.ResponseTimedOut);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelledClient = CreatePipeClient(UniquePipeName(), identityReader);
        var action = () => cancelledClient.SendAsync(message, cancellation.Token);
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Windows_identity_reader_and_transport_remain_unregistered_and_non_executing()
    {
        var protocol = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallSerializedPipeProtocol.cs"));
        var transport = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallFakeNamedPipeTransport.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));
        var appProject = File.ReadAllText(FindRepositoryFile("src", "Css.App", "Css.App.csproj"));
        var program = File.ReadAllText(FindRepositoryFile("src", "Css.Elevated", "Program.cs"));

        transport.Should().Contain("PipeOptions.CurrentUserOnly");
        transport.Should().Contain("GetNamedPipeClientProcessId");
        transport.Should().Contain("GetNamedPipeServerProcessId");
        transport.Should().Contain("RunAsClient");
        transport.Should().Contain("WindowsIdentity.GetCurrent");
        protocol.Should().NotContain("Process.Start");
        transport.Should().NotContain("Process.Start");
        protocol.Should().NotContain("SafetyOperationPipeline");
        transport.Should().NotContain("SafetyOperationPipeline");
        appProject.Should().NotContain("<ProjectReference Include=\"..\\Css.Elevated");
        appProject.Should().Contain("Css.Ipc");
        program.Should().NotContain("OfficialUninstallFakeNamedPipe");
        program.Should().NotContain("OfficialUninstallOperationHandler");
    }

    private static OfficialUninstallFakeNamedPipeServer CreateServer(
        string pipeName,
        IOfficialUninstallPipePeerIdentityReader identityReader,
        OfficialUninstallAuthenticatedInMemoryEndpoint endpoint,
        TimeSpan? startupTimeout = null,
        TimeSpan? responseTimeout = null) =>
        new(
            pipeName,
            ExpectedPeer,
            identityReader,
            endpoint,
            () => Now,
            startupTimeout ?? TimeSpan.FromSeconds(2),
            responseTimeout ?? TimeSpan.FromSeconds(2));

    private static OfficialUninstallFakeNamedPipeClient CreatePipeClient(
        string pipeName,
        IOfficialUninstallPipePeerIdentityReader identityReader,
        TimeSpan? responseTimeout = null) =>
        new(
            pipeName,
            ExpectedPeer,
            identityReader,
            startupTimeout: TimeSpan.FromSeconds(2),
            responseTimeout: responseTimeout ?? TimeSpan.FromSeconds(2));

    private static OfficialUninstallAuthenticatedInMemoryClient CreateAuthenticatedClient() =>
        new(
            "session-serialized-pipe",
            SessionKey,
            messageIdFactory: () => Guid.NewGuid().ToString("N"),
            nonceFactory: () => Guid.NewGuid().ToString("N"));

    private static OfficialUninstallAuthenticatedInMemoryEndpoint CreateEndpoint(
        Func<OfficialUninstallElevatedRequestDraft, CancellationToken,
            Task<OfficialUninstallElevatedResponseEnvelope>> handler) =>
        new("session-serialized-pipe", SessionKey, handler);

    private static OfficialUninstallElevatedRequestDraft ReadyDraft(
        string requestId = "serialized-pipe-request")
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
            SnapshotId = "snapshot-serialized-pipe",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "verified serialized transport evidence",
            ConfirmationText = "Run Example App official uninstaller?",
            AffectedPaths = [@"D:\Software\Example"],
            AffectedServices = ["ExampleService"],
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
                ConfirmationText = operation.ConfirmationText!,
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

    private static OfficialUninstallElevatedResponseEnvelope SafeResponse(string requestId) =>
        new()
        {
            RequestId = requestId,
            Result = OperationResult.Ok(payload: new OfficialUninstallHandlerPayload
            {
                UninstallerStarted = true,
                UninstallerCompleted = true,
                ExitCode = 0,
                RequiresPostScanRetry = false,
                PostScan = new OfficialUninstallPostScanResult
                {
                    Success = true,
                    SoftwareStillPresent = false,
                    ResidueCandidateCount = 2,
                    PathResidueCandidateCount = 2,
                    VerifiedBackgroundResidueCount = 0,
                    UnverifiedBackgroundHintCount = 0,
                    RequiresBackgroundRescan = false,
                    Summary = @"must not cross IPC: C:\Users\Example\Secret"
                }
            })
        };

    private static string UniquePipeName() =>
        $"omnix-uninstall-test-{Guid.NewGuid():N}";

    private static async Task ConnectAndCloseAsync(string pipeName)
    {
        await using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
    }

    private static async Task SendRawFrameAndCloseAsync(string pipeName, byte[] payload)
    {
        await using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
        await OfficialUninstallPipeFrame.WriteAsync(client, payload, CancellationToken.None);
    }

    private static async Task SendRawLengthAndCloseAsync(string pipeName, int length)
    {
        await using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
        await client.WriteAsync(BitConverter.GetBytes(length));
        await client.FlushAsync();
    }

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

    private sealed class FixedIdentityReader(
        OfficialUninstallPipePeerIdentity clientPeer,
        OfficialUninstallPipePeerIdentity serverPeer)
        : IOfficialUninstallPipePeerIdentityReader
    {
        public OfficialUninstallPipePeerIdentity ReadClientPeer(NamedPipeServerStream pipe) =>
            clientPeer;

        public OfficialUninstallPipePeerIdentity ReadServerPeer(NamedPipeClientStream pipe) =>
            serverPeer;
    }
}
