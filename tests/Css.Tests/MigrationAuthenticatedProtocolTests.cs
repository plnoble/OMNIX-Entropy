using System.IO.Pipes;
using System.Text;
using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class MigrationAuthenticatedProtocolTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 13, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Composer_BindsFinalConsentAndEveryStringListValue()
    {
        var first = CreateRequest();
        var changedGate = CreateGate(processes: ["demo-helper"]);
        var second = MigrationElevatedRequestComposer.Create(
            changedGate,
            CreateConsent(changedGate.Operation!),
            "migration-request-2",
            Now);

        first.CanSubmit.Should().BeTrue();
        first.Operation!.ConfirmationAccepted.Should().BeTrue();
        first.PreparedAtUtc.Should().Be(Now);
        first.DescriptorSha256.Should().NotBe(second.DescriptorSha256);
    }

    [Fact]
    public void Composer_RefusesIncompleteOrStaleConsent()
    {
        var gate = CreateGate();
        var consent = CreateConsent(gate.Operation!) with
        {
            MonitoringConfirmed = false,
            ConfirmedAtUtc = Now.AddMinutes(-16)
        };

        var request = MigrationElevatedRequestComposer.Create(
            gate,
            consent,
            "migration-request-refused",
            Now);

        request.CanSubmit.Should().BeFalse();
        request.MissingRequirements.Should().HaveCount(2);
    }

    [Fact]
    public async Task CodecAndEndpoint_RoundTripListArgumentsAndTypedResult()
    {
        var key = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();
        var request = CreateRequest();
        using var client = new MigrationAuthenticatedClient("migration-session-1", key);
        var message = client.CreateMessage(request, Now);
        var decoded = MigrationPipeCodec.DeserializeRequest(
            MigrationPipeCodec.SerializeRequest(message));
        var calls = 0;
        using var endpoint = new MigrationAuthenticatedEndpoint(
            "migration-session-1",
            key,
            (draft, _) =>
            {
                calls++;
                ((string[])draft.Operation!.Arguments["affectedProcesses"]!)
                    .Should().Equal("demo", "demo-agent");
                return Task.FromResult(Completed(draft.RequestId!));
            });

        var handled = await endpoint.HandleAsync(decoded, Now);
        var response = MigrationPipeCodec.DeserializeResponse(
            MigrationPipeCodec.SerializeResponse(decoded, handled),
            decoded);

        calls.Should().Be(1);
        response.Status.Should().Be(MigrationTransportStatus.Completed);
        response.Response!.Result.Payload.Should().BeOfType<MigrationExecutionResult>()
            .Which.Status.Should().Be(MigrationExecutionStatus.Completed);
    }

    [Fact]
    public async Task Endpoint_RejectsDescriptorTamperBeforeHandler()
    {
        var key = Enumerable.Repeat((byte)9, 32).ToArray();
        var request = CreateRequest();
        using var client = new MigrationAuthenticatedClient("migration-session-2", key);
        var message = client.CreateMessage(request, Now);
        ((string[])request.Operation!.Arguments["affectedProcesses"]!)[0] = "tampered";
        var calls = 0;
        using var endpoint = new MigrationAuthenticatedEndpoint(
            "migration-session-2",
            key,
            (draft, _) =>
            {
                calls++;
                return Task.FromResult(Completed(draft.RequestId!));
            });

        var result = await endpoint.HandleAsync(message, Now);

        result.Status.Should().Be(MigrationTransportStatus.InvalidRequest);
        calls.Should().Be(0);
    }

    [Fact]
    public async Task Endpoint_RejectsReplayAndStaleRequest()
    {
        var key = Enumerable.Repeat((byte)7, 32).ToArray();
        var request = CreateRequest();
        using var client = new MigrationAuthenticatedClient("migration-session-3", key);
        var message = client.CreateMessage(request, Now);
        var calls = 0;
        using var endpoint = new MigrationAuthenticatedEndpoint(
            "migration-session-3",
            key,
            (draft, _) =>
            {
                calls++;
                return Task.FromResult(Completed(draft.RequestId!));
            });

        (await endpoint.HandleAsync(message, Now)).Status
            .Should().Be(MigrationTransportStatus.Completed);
        (await endpoint.HandleAsync(message, Now)).Status
            .Should().Be(MigrationTransportStatus.ReplayRejected);

        var staleRequest = CreateRequest(Now.AddMinutes(-16));
        using var staleClient = new MigrationAuthenticatedClient("migration-session-4", key);
        var stale = staleClient.CreateMessage(staleRequest, Now);
        using var staleEndpoint = new MigrationAuthenticatedEndpoint(
            "migration-session-4",
            key,
            (draft, _) => Task.FromResult(Completed(draft.RequestId!)));
        (await staleEndpoint.HandleAsync(stale, Now)).Status
            .Should().Be(MigrationTransportStatus.Expired);
        calls.Should().Be(1);
    }

    [Fact]
    public void Codec_ResponseOmitsPathsRawErrorsAndMonitoringRecord()
    {
        var key = Enumerable.Repeat((byte)3, 32).ToArray();
        var request = CreateRequest();
        using var client = new MigrationAuthenticatedClient("migration-session-5", key);
        var message = client.CreateMessage(request, Now);
        const string secretPath = @"C:\Users\Private\Secret";
        var result = new MigrationTransportResult
        {
            Status = MigrationTransportStatus.Completed,
            Response = new MigrationElevatedResponseEnvelope
            {
                RequestId = request.RequestId!,
                Result = new OperationResult
                {
                    Success = false,
                    Error = "failed at " + secretPath,
                    Payload = new MigrationExecutionResult
                    {
                        Status = MigrationExecutionStatus.FailedRollbackIncomplete,
                        Summary = "failed at " + secretPath,
                        RollbackAttempted = true,
                        RollbackSucceeded = false,
                        Errors = [secretPath],
                        MonitoringRecord = new MigrationMonitoringRecord
                        {
                            Id = "monitor-1",
                            SoftwareName = "Demo",
                            SnapshotId = "snapshot-1",
                            RollbackManifestPath = secretPath,
                            RollbackManifestSha256 = new string('A', 64),
                            Paths =
                            [
                                new MigrationMonitoringPath
                                {
                                    OriginalPath = secretPath,
                                    ExpectedRedirectTarget = @"D:\Software\Demo"
                                }
                            ]
                        }
                    }
                }
            }
        };

        var payload = MigrationPipeCodec.SerializeResponse(message, result);
        var json = Encoding.UTF8.GetString(payload);
        var decoded = MigrationPipeCodec.DeserializeResponse(payload, message);

        json.Should().NotContain(secretPath);
        json.Should().NotContain("Private");
        var execution = decoded.Response!.Result.Payload.Should()
            .BeOfType<MigrationExecutionResult>().Subject;
        execution.Errors.Should().BeEmpty();
        execution.MonitoringRecord.Should().BeNull();
    }

    [Fact]
    public void Codec_RejectsUnknownFieldsAndOversizedPayloads()
    {
        var key = Enumerable.Repeat((byte)5, 32).ToArray();
        using var client = new MigrationAuthenticatedClient("migration-session-6", key);
        var message = client.CreateMessage(CreateRequest(), Now);
        var json = Encoding.UTF8.GetString(MigrationPipeCodec.SerializeRequest(message));
        var unknown = Encoding.UTF8.GetBytes(json.Insert(json.Length - 1, ",\"unexpected\":true"));

        FluentActions.Invoking(() => MigrationPipeCodec.DeserializeRequest(unknown))
            .Should().Throw<MigrationPipeProtocolException>();
        FluentActions.Invoking(() => MigrationPipeCodec.DeserializeRequest(
                new byte[MigrationPipeCodec.MaximumPayloadBytes + 1]))
            .Should().Throw<MigrationPipeProtocolException>();
    }

    [Fact]
    public async Task OneShotServer_AuthorizesBeforeBootstrapOrHandler()
    {
        var identity = new OfficialUninstallPipePeerIdentity
        {
            UserSid = "S-1-5-21-fixture",
            ProcessId = 100,
            WindowsSessionId = 1
        };
        var worker = identity with { ProcessId = 200 };
        var pipeName = "omnix-migration-test-" + Guid.NewGuid().ToString("N");
        var reader = new FixedPeerReader(identity, worker);
        var server = new MigrationOneShotWorkerServer(reader);
        var authorizationCalls = 0;
        var handlerCalls = 0;
        var serve = server.ServeOnceAsync(
            new MigrationOneShotWorkerOptions
            {
                PipeName = pipeName,
                SessionId = "migration-session-denied",
                ExpectedClient = identity,
                Worker = worker,
                Timeout = TimeSpan.FromSeconds(5)
            },
            (_, _, _) =>
            {
                authorizationCalls++;
                return ValueTask.FromResult(false);
            },
            (draft, _) =>
            {
                handlerCalls++;
                return Task.FromResult(Completed(draft.RequestId!));
            });
        await using var pipe = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        await pipe.ConnectAsync(5000);

        var result = await serve;

        result.Status.Should().Be(MigrationTransportStatus.AuthorizationFailed);
        authorizationCalls.Should().Be(1);
        handlerCalls.Should().Be(0);
    }

    private static MigrationElevatedRequestDraft CreateRequest(DateTimeOffset? confirmedAt = null)
    {
        var gate = CreateGate();
        var time = confirmedAt ?? Now;
        return MigrationElevatedRequestComposer.Create(
            gate,
            CreateConsent(gate.Operation!) with { ConfirmedAtUtc = time },
            "migration-request-1",
            time);
    }

    private static MigrationExecutionGateResult CreateGate(
        IReadOnlyList<string>? processes = null)
    {
        var operation = new OperationDescriptor
        {
            Kind = "migration.execute",
            Title = "Demo migration",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "snapshot-1",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "Verified migration plan and rollback manifest.",
            EstimatedImpactBytes = 1024,
            ConfirmationText = "Migrate Demo to D drive?",
            AffectedPaths = [@"C:\Users\Fixture\AppData\Local\Demo"],
            AffectedServices = ["DemoService"],
            Arguments = new Dictionary<string, object?>
            {
                ["destinationRoot"] = @"D:\Software\Demo",
                ["rollbackManifestPath"] = @"C:\Users\Fixture\AppData\Local\OMNIX\manifest.json",
                ["rollbackManifestSha256"] = new string('A', 64),
                ["snapshotEvidencePath"] = @"C:\Users\Fixture\AppData\Local\OMNIX\snapshot.json",
                ["snapshotEvidenceSha256"] = new string('B', 64),
                ["affectedProcesses"] = (processes ?? ["demo", "demo-agent"]).ToArray(),
                ["scheduledTasks"] = new[] { "DemoTask" },
                ["startupEntries"] = Array.Empty<string>(),
                ["monitorPaths"] = new[] { @"C:\Users\Fixture\AppData\Local\Demo" }
            }
        };
        return new MigrationExecutionGateResult
        {
            CanRequestExecution = true,
            PrimaryButtonText = "Request migration",
            BlockingReasons = [],
            RequiredBytes = 1024,
            Operation = operation
        };
    }

    private static MigrationFinalUserConsent CreateConsent(OperationDescriptor operation) =>
        new()
        {
            ConfirmationText = operation.ConfirmationText!,
            PlanReviewedConfirmed = true,
            AppComponentsClosedConfirmed = true,
            RollbackAcknowledged = true,
            MonitoringConfirmed = true,
            ExecutionRequested = true,
            ConfirmedAtUtc = Now
        };

    private static MigrationElevatedResponseEnvelope Completed(string requestId) =>
        new()
        {
            RequestId = requestId,
            Result = OperationResult.Ok(
                "internal summary",
                new MigrationExecutionResult
                {
                    Status = MigrationExecutionStatus.Completed,
                    Summary = "internal summary",
                    MovedPathCount = 1,
                    RollbackSucceeded = true
                })
        };

    private sealed class FixedPeerReader(
        OfficialUninstallPipePeerIdentity client,
        OfficialUninstallPipePeerIdentity server)
        : IOfficialUninstallPipePeerIdentityReader
    {
        public OfficialUninstallPipePeerIdentity ReadClientPeer(NamedPipeServerStream pipe) => client;
        public OfficialUninstallPipePeerIdentity ReadServerPeer(NamedPipeClientStream pipe) => server;
    }
}
