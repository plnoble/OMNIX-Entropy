using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallSmokeWorkerProcessTests
{
    [Fact]
    public async Task Separate_worker_bootstraps_returns_typed_fake_result_and_exits()
    {
        ISmokeWorkerLauncher launcher = new SystemSmokeWorkerLauncher(SmokeWorkerExecutable());
        var parent = CurrentProcessIdentity();
        var pipeName = $"omnix-worker-{Guid.NewGuid():N}";
        var sessionId = $"session-{Guid.NewGuid():N}";
        var launch = new SmokeWorkerLaunch
        {
            PipeName = pipeName,
            SessionId = sessionId,
            Client = parent,
            TimeoutMilliseconds = 8_000
        };

        await using var worker = launcher.Launch(launch);
        var expectedServer = new OfficialUninstallPipePeerIdentity
        {
            UserSid = parent.UserSid,
            ProcessId = worker.ProcessId,
            WindowsSessionId = worker.WindowsSessionId
        };

        var roundTrip = await SendOneAsync(
            launch,
            expectedServer,
            ReadyDraft(),
            TimeSpan.FromSeconds(8));

        roundTrip.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
        var payload = roundTrip.Response!.Result.Payload
            .Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;
        payload.UninstallerStarted.Should().BeTrue();
        payload.UninstallerCompleted.Should().BeTrue();
        payload.PostScan.ResidueCandidateCount.Should().Be(2);

        var exit = await worker.WaitForExitAsync(TimeSpan.FromSeconds(5));
        exit.ExitCode.Should().Be(0);
        exit.StandardError.Should().BeEmpty();
        using var receipt = JsonDocument.Parse(exit.StandardOutput);
        receipt.RootElement.GetProperty("workerProcessId").GetInt32()
            .Should().Be(worker.ProcessId);
        receipt.RootElement.GetProperty("clientProcessId").GetInt32()
            .Should().Be(parent.ProcessId);
        receipt.RootElement.GetProperty("sessionId").GetString()
            .Should().Be(sessionId);
        receipt.RootElement.GetProperty("status").GetString()
            .Should().Be(nameof(OfficialUninstallTransportStatus.Completed));
        receipt.RootElement.GetProperty("transcriptSha256").GetString()
            .Should().MatchRegex("^[0-9A-F]{64}$");
        worker.HasExited.Should().BeTrue();
    }

    [Fact]
    public async Task Worker_startup_timeout_exits_without_an_orphan()
    {
        ISmokeWorkerLauncher launcher = new SystemSmokeWorkerLauncher(SmokeWorkerExecutable());
        var launch = new SmokeWorkerLaunch
        {
            PipeName = $"omnix-worker-timeout-{Guid.NewGuid():N}",
            SessionId = $"session-{Guid.NewGuid():N}",
            Client = CurrentProcessIdentity(),
            TimeoutMilliseconds = 250
        };

        await using var worker = launcher.Launch(launch);
        var exit = await worker.WaitForExitAsync(TimeSpan.FromSeconds(5));

        exit.ExitCode.Should().Be(1);
        exit.StandardError.Should().Contain("OperationCanceledException");
        worker.HasExited.Should().BeTrue();
    }

    [Fact]
    public async Task Disposing_a_waiting_worker_terminates_the_child_process()
    {
        ISmokeWorkerLauncher launcher = new SystemSmokeWorkerLauncher(SmokeWorkerExecutable());
        var launch = new SmokeWorkerLaunch
        {
            PipeName = $"omnix-worker-dispose-{Guid.NewGuid():N}",
            SessionId = $"session-{Guid.NewGuid():N}",
            Client = CurrentProcessIdentity(),
            TimeoutMilliseconds = 8_000
        };
        var worker = launcher.Launch(launch);
        var workerProcessId = worker.ProcessId;

        await worker.DisposeAsync();

        var lookup = () => Process.GetProcessById(workerProcessId);
        lookup.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Worker_mode_has_no_secret_argument_or_real_execution_authority()
    {
        var worker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.SmokeTools", "OfficialUninstallIpcWorker.cs"));
        var testLauncher = File.ReadAllText(FindRepositoryFile(
            "tests", "Css.Tests", "OfficialUninstallSmokeWorkerProcessTests.cs"));
        var program = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Program.cs"));

        worker.Should().Contain("PipeOptions.CurrentUserOnly");
        worker.Should().Contain("ReadClientPeer");
        worker.Should().Contain("OfficialUninstallSessionBootstrapServer");
        worker.Should().Contain("OfficialUninstallAuthenticatedInMemoryEndpoint");
        worker.Should().NotContain("--session-key");
        worker.Should().NotContain("--authentication-tag");
        worker.Should().NotContain("GetEnvironmentVariable");
        worker.Should().NotContain("Process.Start");
        worker.Should().NotContain("OfficialUninstallOperationHandler");
        worker.Should().NotContain("WindowsOfficialUninstallerLauncher");
        worker.Should().NotContain("SafetyOperationPipeline");
        worker.Should().NotContain("Registry.");
        worker.Should().NotContain("File.Delete");
        worker.Should().NotContain("File.Move");
        testLauncher.Should().Contain("ISmokeWorkerLauncher launcher");
        testLauncher.Should().Contain("UseShellExecute = false");
        testLauncher.Should().Contain("start.ArgumentList.Add(value)");
        testLauncher.Should().NotContain("\"--session-key\", launch");
        testLauncher.Should().NotContain("\"--authentication-tag\", launch");
        program.Should().NotContain("official-uninstall-ipc-worker");
        program.Should().NotContain("OfficialUninstallSessionBootstrap");
    }

    private static async Task<OfficialUninstallTransportResult> SendOneAsync(
        SmokeWorkerLaunch launch,
        OfficialUninstallPipePeerIdentity expectedServer,
        OfficialUninstallElevatedRequestDraft request,
        TimeSpan timeout)
    {
        using var deadline = new CancellationTokenSource(timeout);
        await using var pipe = new NamedPipeClientStream(
            ".",
            launch.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous,
            TokenImpersonationLevel.Impersonation,
            HandleInheritability.None);
        await pipe.ConnectAsync(deadline.Token);

        var actualServer = new WindowsOfficialUninstallPipePeerIdentityReader()
            .ReadServerPeer(pipe);
        actualServer.Should().BeEquivalentTo(expectedServer);

        var context = new OfficialUninstallSessionBootstrapContext
        {
            PipeName = launch.PipeName,
            SessionId = launch.SessionId,
            Client = launch.Client,
            Server = actualServer
        };
        using var sessionKey = await new OfficialUninstallSessionBootstrapClient(
                context,
                timeout: timeout)
            .EstablishAsync(pipe, deadline.Token);

        var keyCopy = sessionKey.ExportCopy();
        try
        {
            using var authenticated = new OfficialUninstallAuthenticatedInMemoryClient(
                launch.SessionId,
                keyCopy);
            var message = authenticated.CreateMessage(request, DateTimeOffset.UtcNow);
            var requestPayload = OfficialUninstallPipeCodec.SerializeRequest(message);
            await OfficialUninstallPipeFrame.WriteAsync(pipe, requestPayload, deadline.Token);
            var responsePayload = await OfficialUninstallPipeFrame.ReadAsync(pipe, deadline.Token);
            return OfficialUninstallPipeCodec.DeserializeResponse(responsePayload, message);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(keyCopy);
        }
    }

    private static OfficialUninstallElevatedRequestDraft ReadyDraft()
    {
        var now = DateTimeOffset.UtcNow;
        var operation = new OperationDescriptor
        {
            Kind = "uninstall.official.run",
            Title = "Smoke fixture official uninstaller",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "snapshot-smoke-worker",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "verified smoke-only evidence",
            ConfirmationText = "Run the smoke fixture official uninstaller?",
            AffectedPaths = [@"D:\Software\SmokeFixture"],
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "Smoke Fixture",
                ["executablePath"] = @"D:\Software\SmokeFixture\Uninstall.exe",
                ["arguments"] = "/remove",
                ["snapshotManifestPath"] = @"D:\Evidence\smoke-snapshot.json",
                ["snapshotSha256"] = new string('A', 64),
                ["snapshotCanRestoreApplication"] = false,
                ["recoveryMethod"] = OfficialUninstallRecoveryMethod.ReinstallSource.ToString(),
                ["recoveryReference"] = @"D:\Installers\SmokeFixtureSetup.exe"
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
                CapturedAtUtc = now.AddMinutes(-1),
                RecoveryTruthVisible = true,
                FinalConfirmationVisible = true,
                TechnicalDetailsCollapsedByDefault = true,
                NoExecutionControlDuringPreparation = true
            },
            new OfficialUninstallFinalUserConsent
            {
                ConfirmationText = operation.ConfirmationText!,
                ConfirmedAtUtc = now,
                OfficialCommandConfirmed = true,
                AppsClosedConfirmed = true,
                NoAutomaticUndoAcknowledged = true,
                PostUninstallRescanConfirmed = true,
                ExecutionRequested = true
            },
            $"smoke-request-{Guid.NewGuid():N}",
            now);
    }

    private static OfficialUninstallPipePeerIdentity CurrentProcessIdentity()
    {
        using var identity = WindowsIdentity.GetCurrent();
        using var process = Process.GetCurrentProcess();
        return new OfficialUninstallPipePeerIdentity
        {
            UserSid = identity.User!.Value,
            ProcessId = Environment.ProcessId,
            WindowsSessionId = process.SessionId
        };
    }

    private static string SmokeWorkerExecutable()
    {
        var project = Path.GetDirectoryName(FindRepositoryFile(
            "src", "Css.SmokeTools", "Css.SmokeTools.csproj"))!;
        var configuration = new DirectoryInfo(AppContext.BaseDirectory)
            .Parent?.Name ?? "Debug";
        var executable = Path.Combine(
            project,
            "bin",
            configuration,
            "net8.0-windows",
            "Css.SmokeTools.exe");
        File.Exists(executable).Should().BeTrue($"the smoke worker should be built at {executable}");
        return executable;
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

    private sealed class SmokeWorkerLaunch
    {
        public required string PipeName { get; init; }
        public required string SessionId { get; init; }
        public required OfficialUninstallPipePeerIdentity Client { get; init; }
        public required int TimeoutMilliseconds { get; init; }
    }

    private interface ISmokeWorkerLauncher
    {
        SmokeWorkerProcess Launch(SmokeWorkerLaunch launch);
    }

    private sealed class SystemSmokeWorkerLauncher(string executablePath) : ISmokeWorkerLauncher
    {
        public SmokeWorkerProcess Launch(SmokeWorkerLaunch launch)
        {
            var start = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var value in new[]
                     {
                         "official-uninstall-ipc-worker",
                         "--pipe-name", launch.PipeName,
                         "--session-id", launch.SessionId,
                         "--client-sid", launch.Client.UserSid,
                         "--client-pid", launch.Client.ProcessId.ToString(CultureInfo.InvariantCulture),
                         "--client-windows-session",
                         launch.Client.WindowsSessionId.ToString(CultureInfo.InvariantCulture),
                         "--timeout-ms", launch.TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture)
                     })
            {
                start.ArgumentList.Add(value);
            }

            var process = Process.Start(start)
                ?? throw new InvalidOperationException("The smoke worker did not start.");
            return new SmokeWorkerProcess(process);
        }
    }

    private sealed class SmokeWorkerProcess : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly Task<string> _standardOutput;
        private readonly Task<string> _standardError;

        public SmokeWorkerProcess(Process process)
        {
            _process = process;
            ProcessId = process.Id;
            WindowsSessionId = process.SessionId;
            _standardOutput = process.StandardOutput.ReadToEndAsync();
            _standardError = process.StandardError.ReadToEndAsync();
        }

        public int ProcessId { get; }
        public int WindowsSessionId { get; }
        public bool HasExited => _process.HasExited;

        public async Task<SmokeWorkerExit> WaitForExitAsync(TimeSpan timeout)
        {
            using var deadline = new CancellationTokenSource(timeout);
            await _process.WaitForExitAsync(deadline.Token);
            return new SmokeWorkerExit
            {
                ExitCode = _process.ExitCode,
                StandardOutput = (await _standardOutput).Trim(),
                StandardError = (await _standardError).Trim()
            };
        }

        public async ValueTask DisposeAsync()
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _process.WaitForExitAsync(deadline.Token);
            }
            _process.Dispose();
        }
    }

    private sealed class SmokeWorkerExit
    {
        public required int ExitCode { get; init; }
        public required string StandardOutput { get; init; }
        public required string StandardError { get; init; }
    }
}
