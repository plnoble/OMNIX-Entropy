using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;

internal static class OfficialUninstallIpcWorker
{
    private const int MaximumTimeoutMilliseconds = 30_000;
    private const int MinimumTimeoutMilliseconds = 100;
    private static readonly JsonSerializerOptions ReceiptJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = WorkerOptions.Parse(args);
            var receipt = await ServeOnceAsync(options);
            Console.WriteLine(JsonSerializer.Serialize(receipt, ReceiptJsonOptions));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"official-uninstall-ipc-worker failed: {exception.GetType().Name}");
            return 1;
        }
    }

    private static async Task<WorkerReceipt> ServeOnceAsync(WorkerOptions options)
    {
        var timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
        var expectedClient = new OfficialUninstallPipePeerIdentity
        {
            UserSid = options.ClientSid,
            ProcessId = options.ClientProcessId,
            WindowsSessionId = options.ClientWindowsSessionId
        };
        var serverIdentity = CurrentProcessIdentity();

        await using var pipe = new NamedPipeServerStream(
            options.PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
            inBufferSize: OfficialUninstallPipeCodec.MaximumPayloadBytes + sizeof(int),
            outBufferSize: OfficialUninstallPipeCodec.MaximumPayloadBytes + sizeof(int));

        using (var startup = new CancellationTokenSource(timeout))
            await pipe.WaitForConnectionAsync(startup.Token);

        var actualClient = new WindowsOfficialUninstallPipePeerIdentityReader()
            .ReadClientPeer(pipe);
        RequireIdentity(expectedClient, actualClient, "client");

        var context = new OfficialUninstallSessionBootstrapContext
        {
            PipeName = options.PipeName,
            SessionId = options.SessionId,
            Client = actualClient,
            Server = serverIdentity
        };

        using var responseDeadline = new CancellationTokenSource(timeout);
        using var sessionKey = await new OfficialUninstallSessionBootstrapServer(
                context,
                new OfficialUninstallSessionBootstrapReplayGuard(),
                timeout: timeout)
            .EstablishAsync(pipe, responseDeadline.Token);

        var keyCopy = sessionKey.ExportCopy();
        try
        {
            using var endpoint = new OfficialUninstallAuthenticatedInMemoryEndpoint(
                options.SessionId,
                keyCopy,
                CreateFakeResponseAsync);
            var requestPayload = await OfficialUninstallPipeFrame.ReadAsync(
                pipe,
                responseDeadline.Token);
            var message = OfficialUninstallPipeCodec.DeserializeRequest(requestPayload);
            var result = await endpoint.HandleAsync(
                message,
                DateTimeOffset.UtcNow,
                responseDeadline.Token);
            var responsePayload = OfficialUninstallPipeCodec.SerializeResponse(message, result);
            await OfficialUninstallPipeFrame.WriteAsync(
                pipe,
                responsePayload,
                responseDeadline.Token);

            return new WorkerReceipt
            {
                Mode = "official-uninstall-ipc-worker",
                SessionId = options.SessionId,
                WorkerProcessId = serverIdentity.ProcessId,
                ClientProcessId = actualClient.ProcessId,
                RequestId = message.RequestId,
                Status = result.Status.ToString(),
                TranscriptSha256 = sessionKey.TranscriptSha256
            };
        }
        finally
        {
            CryptographicOperations.ZeroMemory(keyCopy);
        }
    }

    private static Task<OfficialUninstallElevatedResponseEnvelope> CreateFakeResponseAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new OfficialUninstallElevatedResponseEnvelope
        {
            RequestId = request.RequestId!,
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
                    Summary = "Test-only fake post-scan result."
                }
            })
        });
    }

    private static OfficialUninstallPipePeerIdentity CurrentProcessIdentity()
    {
        using var windowsIdentity = WindowsIdentity.GetCurrent();
        var sid = windowsIdentity.User?.Value
            ?? throw new InvalidOperationException("The worker SID is unavailable.");
        using var process = Process.GetCurrentProcess();
        return new OfficialUninstallPipePeerIdentity
        {
            UserSid = sid,
            ProcessId = Environment.ProcessId,
            WindowsSessionId = process.SessionId
        };
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
            throw new InvalidOperationException($"The {role} pipe identity does not match.");
        }
    }

    private sealed class WorkerOptions
    {
        private static readonly string[] RequiredNames =
        [
            "--pipe-name",
            "--session-id",
            "--client-sid",
            "--client-pid",
            "--client-windows-session",
            "--timeout-ms"
        ];

        public required string PipeName { get; init; }
        public required string SessionId { get; init; }
        public required string ClientSid { get; init; }
        public required int ClientProcessId { get; init; }
        public required int ClientWindowsSessionId { get; init; }
        public required int TimeoutMilliseconds { get; init; }

        public static WorkerOptions Parse(string[] args)
        {
            if (args.Length != RequiredNames.Length * 2)
                throw new ArgumentException("The worker metadata is incomplete.", nameof(args));

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var index = 0; index < args.Length; index += 2)
            {
                var name = args[index];
                if (!RequiredNames.Contains(name, StringComparer.Ordinal)
                    || !values.TryAdd(name, args[index + 1]))
                {
                    throw new ArgumentException("The worker metadata is invalid.", nameof(args));
                }
            }

            var pipeName = Token(values["--pipe-name"], "pipe name");
            if (pipeName.IndexOfAny(['\\', '/']) >= 0)
                throw new ArgumentException("The pipe name is invalid.", nameof(args));
            var sessionId = Token(values["--session-id"], "session id");
            var sid = values["--client-sid"];
            _ = new SecurityIdentifier(sid);
            var clientPid = PositiveInt(values["--client-pid"], "client pid");
            var clientSession = NonNegativeInt(
                values["--client-windows-session"],
                "client Windows session");
            var timeoutMilliseconds = PositiveInt(values["--timeout-ms"], "timeout");
            if (timeoutMilliseconds is < MinimumTimeoutMilliseconds or > MaximumTimeoutMilliseconds)
                throw new ArgumentOutOfRangeException(nameof(args), "The worker timeout is invalid.");

            return new WorkerOptions
            {
                PipeName = pipeName,
                SessionId = sessionId,
                ClientSid = sid,
                ClientProcessId = clientPid,
                ClientWindowsSessionId = clientSession,
                TimeoutMilliseconds = timeoutMilliseconds
            };
        }

        private static string Token(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
                throw new ArgumentException($"The {name} is invalid.");
            return value;
        }

        private static int PositiveInt(string value, string name)
        {
            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                || parsed <= 0)
            {
                throw new ArgumentException($"The {name} is invalid.");
            }
            return parsed;
        }

        private static int NonNegativeInt(string value, string name)
        {
            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                || parsed < 0)
            {
                throw new ArgumentException($"The {name} is invalid.");
            }
            return parsed;
        }
    }

    private sealed class WorkerReceipt
    {
        public required string Mode { get; init; }
        public required string SessionId { get; init; }
        public required int WorkerProcessId { get; init; }
        public required int ClientProcessId { get; init; }
        public required string RequestId { get; init; }
        public required string Status { get; init; }
        public required string TranscriptSha256 { get; init; }
    }
}
