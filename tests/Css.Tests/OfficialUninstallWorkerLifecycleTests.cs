using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Principal;
using Css.App;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallWorkerLifecycleTests
{
    [Fact]
    public async Task Elevated_fake_worker_returns_one_truthful_response_and_exits()
    {
        var launcher = new TestElevatedWorkerLauncher(ElevatedWorkerExecutable());
        var client = CreateClient(launcher);

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.CompletedFake);
        result.ChildExited.Should().BeTrue();
        result.TransportStatus.Should().Be(OfficialUninstallTransportStatus.Completed);
        var payload = result.Response!.Result.Payload
            .Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;
        payload.UninstallerStarted.Should().BeFalse();
        payload.UninstallerCompleted.Should().BeFalse();
        payload.PostScan.Success.Should().BeFalse();
        launcher.LastProcess!.ExitedObserved.Should().BeTrue();
        launcher.LastProcess.TerminateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Injected_trusted_production_launcher_returns_a_distinct_typed_response()
    {
        var inner = new TestElevatedWorkerLauncher(ElevatedWorkerExecutable());
        var launcher = new TestProductionWorkerLauncher(inner);
        var client = CreateClient(launcher);

        var result = await client.RunProductionOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.CompletedProduction);
        result.ChildExited.Should().BeTrue();
        result.TransportStatus.Should().Be(OfficialUninstallTransportStatus.Completed);
        result.Response.Should().NotBeNull();
        result.Response!.Result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>();
        inner.LastProcess!.ExitedObserved.Should().BeTrue();
    }

    [Fact]
    public async Task Production_run_rejects_a_development_launcher_before_process_start()
    {
        var launcher = new TestElevatedWorkerLauncher(ElevatedWorkerExecutable());
        var client = CreateClient(launcher);

        var result = await client.RunProductionOnceAsync(ReadyDraft());

        result.Status.Should().Be(
            OfficialUninstallWorkerLifecycleStatus.ProductionLauncherRejected);
        result.ChildExited.Should().BeFalse();
        launcher.LastProcess.Should().BeNull();
    }

    [Fact]
    public async Task Fake_run_rejects_a_production_launcher_before_process_start()
    {
        var inner = new TestElevatedWorkerLauncher(ElevatedWorkerExecutable());
        var client = CreateClient(new TestProductionWorkerLauncher(inner));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(
            OfficialUninstallWorkerLifecycleStatus.ProductionLauncherRejected);
        result.ChildExited.Should().BeFalse();
        inner.LastProcess.Should().BeNull();
    }

    [Fact]
    public async Task Uac_cancel_is_reported_without_creating_a_process()
    {
        var launcher = new TestElevatedWorkerLauncher(
            ElevatedWorkerExecutable(),
            launchStatus: OfficialUninstallWorkerLaunchStatus.UserCanceled);
        var client = CreateClient(launcher);

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(
            OfficialUninstallWorkerLifecycleStatus.UserCanceledElevation);
        result.ChildExited.Should().BeFalse();
        launcher.LastProcess.Should().BeNull();
    }

    [Fact]
    public async Task Started_worker_with_a_different_image_path_is_rejected_before_transport()
    {
        var executable = ElevatedWorkerExecutable();
        var launcher = new TestElevatedWorkerLauncher(executable);
        var inspector = new TestWorkerImageInspector(
            executable,
            executablePath: Path.Combine(Path.GetDirectoryName(executable)!, "OtherWorker.exe"));
        var client = CreateClient(
            launcher,
            inspector,
            shutdownTimeout: TimeSpan.FromMilliseconds(150));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected);
        result.ChildExited.Should().BeTrue();
        inspector.InspectCount.Should().Be(1);
        launcher.LastProcess!.TerminateCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Started_worker_without_expected_image_evidence_is_rejected_before_transport()
    {
        var executable = ElevatedWorkerExecutable();
        var launcher = new TestElevatedWorkerLauncher(
            executable,
            omitImageExpectation: true);
        var inspector = new TestWorkerImageInspector(executable);
        var client = CreateClient(
            launcher,
            inspector,
            shutdownTimeout: TimeSpan.FromMilliseconds(150));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected);
        result.ChildExited.Should().BeTrue();
        inspector.InspectCount.Should().Be(0);
        launcher.LastProcess!.TerminateCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Started_worker_with_a_different_image_hash_is_rejected_before_transport()
    {
        var executable = ElevatedWorkerExecutable();
        var launcher = new TestElevatedWorkerLauncher(executable);
        var inspector = new TestWorkerImageInspector(
            executable,
            sha256: new string('0', 64));
        var client = CreateClient(
            launcher,
            inspector,
            shutdownTimeout: TimeSpan.FromMilliseconds(150));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected);
        result.ChildExited.Should().BeTrue();
        inspector.InspectCount.Should().Be(1);
        launcher.LastProcess!.TerminateCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Worker_image_inspection_failure_is_rejected_and_the_child_is_cleaned_up()
    {
        var executable = ElevatedWorkerExecutable();
        var launcher = new TestElevatedWorkerLauncher(executable);
        var inspector = new TestWorkerImageInspector(executable, fail: true);
        var client = CreateClient(
            launcher,
            inspector,
            shutdownTimeout: TimeSpan.FromMilliseconds(150));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected);
        result.ChildExited.Should().BeTrue();
        inspector.InspectCount.Should().Be(1);
        launcher.LastProcess!.TerminateCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Windows_image_inspector_reads_the_actual_current_process_path_and_hash()
    {
        var processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("The test process path is unavailable.");
        var inspector = new WindowsOfficialUninstallWorkerImageInspector();

        var evidence = await inspector.InspectAsync(new CurrentProcessProbe());

        File.Exists(evidence.ExecutablePath).Should().BeTrue();
        Path.GetFileName(evidence.ExecutablePath)
            .Should().BeEquivalentTo(Path.GetFileName(processPath));
        evidence.Sha256.Should().Be(FileSha256(processPath));
    }

    [Fact]
    public async Task Connected_worker_with_wrong_launched_pid_is_rejected_and_cleaned_up()
    {
        var launcher = new TestElevatedWorkerLauncher(
            ElevatedWorkerExecutable(),
            exposedProcessIdOffset: 1);
        var client = CreateClient(launcher, shutdownTimeout: TimeSpan.FromSeconds(1));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.PeerRejected);
        result.ChildExited.Should().BeTrue();
        launcher.LastProcess!.ExitedObserved.Should().BeTrue();
    }

    [Fact]
    public async Task Bootstrap_session_mismatch_is_distinct_and_child_is_cleaned_up()
    {
        var launcher = new TestElevatedWorkerLauncher(
            ElevatedWorkerExecutable(),
            overrideSessionId: "mismatched-worker-session");
        var client = CreateClient(launcher, shutdownTimeout: TimeSpan.FromSeconds(1));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.BootstrapFailed);
        result.BootstrapStatus.Should().NotBeNull();
        result.ChildExited.Should().BeTrue();
        launcher.LastProcess!.ExitedObserved.Should().BeTrue();
    }

    [Fact]
    public async Task Response_timeout_terminates_the_delayed_child_tree()
    {
        var launcher = new TestElevatedWorkerLauncher(
            ElevatedWorkerExecutable(),
            responseDelayMilliseconds: 2_000);
        var client = CreateClient(
            launcher,
            responseTimeout: TimeSpan.FromMilliseconds(150),
            shutdownTimeout: TimeSpan.FromMilliseconds(150));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.ResponseTimedOut);
        result.TransportStatus.Should().Be(OfficialUninstallTransportStatus.ResponseTimedOut);
        result.ChildExited.Should().BeTrue();
        launcher.LastProcess!.TerminateCalled.Should().BeTrue();
        launcher.LastProcess.ExitedObserved.Should().BeTrue();
    }

    [Fact]
    public async Task Successful_response_still_terminates_a_worker_that_does_not_exit_in_time()
    {
        var launcher = new TestElevatedWorkerLauncher(
            ElevatedWorkerExecutable(),
            exitDelayMilliseconds: 2_000);
        var client = CreateClient(
            launcher,
            shutdownTimeout: TimeSpan.FromMilliseconds(150));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.CompletedFake);
        result.ChildExited.Should().BeTrue();
        launcher.LastProcess!.TerminateCalled.Should().BeTrue();
        launcher.LastProcess.ExitedObserved.Should().BeTrue();
    }

    [Fact]
    public async Task Current_unsigned_production_worker_self_denies_before_bootstrap_and_exits()
    {
        var launcher = new TestElevatedWorkerLauncher(
            ElevatedWorkerExecutable(),
            workerMode: "official-uninstall-production-worker");
        var client = CreateClient(
            launcher,
            shutdownTimeout: TimeSpan.FromSeconds(1));

        var result = await client.RunFakeOnceAsync(ReadyDraft());

        result.Status.Should().Be(OfficialUninstallWorkerLifecycleStatus.BootstrapFailed);
        result.ChildExited.Should().BeTrue();
        launcher.LastProcess!.ExitedObserved.Should().BeTrue();
        launcher.LastProcess.TerminateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Production_worker_command_line_rejects_fake_only_options_before_pipe_startup()
    {
        using var identity = WindowsIdentity.GetCurrent();
        using var process = Process.GetCurrentProcess();
        var start = new ProcessStartInfo
        {
            FileName = ElevatedWorkerExecutable(),
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var value in new[]
                 {
                     "official-uninstall-production-worker",
                     "--pipe-name", $"production-parser-{Guid.NewGuid():N}",
                     "--session-id", $"production-parser-{Guid.NewGuid():N}",
                     "--client-sid", identity.User!.Value,
                     "--client-pid", Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                     "--client-windows-session", process.SessionId.ToString(CultureInfo.InvariantCulture),
                     "--timeout-ms", "1000",
                     "--fake-response-delay-ms", "1"
                 })
        {
            start.ArgumentList.Add(value);
        }

        using var worker = Process.Start(start)
            ?? throw new InvalidOperationException("Production parser test worker did not start.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await worker.WaitForExitAsync(timeout.Token);

        worker.ExitCode.Should().Be(1);
    }

    [Fact]
    public void Production_fake_worker_boundary_has_no_real_uninstall_authority_or_secret_channel()
    {
        var lifecycle = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallWorkerLifecycle.cs"));
        var server = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Ipc", "Uninstall", "OfficialUninstallOneShotWorkerServer.cs"));
        var appLauncher = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "OfficialUninstallWorkerLauncher.cs"));
        var processResolver = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Win32", "Processes", "WindowsProcessImagePathResolver.cs"));
        var fakeWorker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "OfficialUninstallFakeWorker.cs"));
        var program = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Program.cs"));
        var planWindow = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "UninstallPlanWindow.xaml.cs"));

        lifecycle.Should().Contain("ReadServerPeer");
        lifecycle.Should().Contain("process.ProcessId");
        lifecycle.Should().Contain("process.WindowsSessionId");
        lifecycle.Should().Contain("WorkerImageMatchesAsync");
        lifecycle.Should().Contain("FixedTimeEquals");
        lifecycle.Should().Contain("TerminateTreeAsync");
        lifecycle.IndexOf("WorkerImageMatchesAsync(launch", StringComparison.Ordinal)
            .Should().BeLessThan(
                lifecycle.IndexOf("? await ExchangeAsync", StringComparison.Ordinal));
        server.Should().Contain("PipeOptions.CurrentUserOnly");
        server.Should().Contain("ReadClientPeer");
        server.Should().Contain("OfficialUninstallSessionBootstrapServer");
        appLauncher.Should().Contain("Verb = \"runas\"");
        appLauncher.Should().Contain("ErrorCancelled = 1223");
        appLauncher.Should().Contain("Process.Start(start)");
        appLauncher.Should().Contain("WindowsProcessImagePathResolver");
        processResolver.Should().Contain("QueryFullProcessImageName");
        processResolver.Should().Contain("ProcessQueryLimitedInformation");
        appLauncher.Should().NotContain("--session-key");
        appLauncher.Should().NotContain("--authentication-tag");
        fakeWorker.Should().Contain("UninstallerStarted = false");
        fakeWorker.Should().NotContain("OfficialUninstallOperationHandler");
        fakeWorker.Should().NotContain("WindowsOfficialUninstallerLauncher");
        fakeWorker.Should().NotContain("InventoryOfficialUninstallPostScanner");
        fakeWorker.Should().NotContain("SafetyOperationPipeline");
        fakeWorker.Should().NotContain("Registry.");
        fakeWorker.Should().NotContain("File.Delete");
        fakeWorker.Should().NotContain("File.Move");
        program.Should().Contain("official-uninstall-fake-worker");
        program.Should().NotContain("OfficialUninstallOperationHandler");
        planWindow.Should().NotContain("WindowsOfficialUninstallWorkerLauncher");
        planWindow.Should().NotContain("OfficialUninstallWorkerLifecycleClient");
    }

    private static OfficialUninstallWorkerLifecycleClient CreateClient(
        IOfficialUninstallWorkerLauncher launcher,
        IOfficialUninstallWorkerImageInspector? imageInspector = null,
        TimeSpan? responseTimeout = null,
        TimeSpan? shutdownTimeout = null) =>
        new(
            launcher,
            imageInspector ?? new TestWorkerImageInspector(ElevatedWorkerExecutable()),
            new WindowsOfficialUninstallCurrentProcessIdentityProvider(),
            new WindowsOfficialUninstallPipePeerIdentityReader(),
            startupTimeout: TimeSpan.FromSeconds(3),
            bootstrapTimeout: TimeSpan.FromSeconds(3),
            responseTimeout: responseTimeout ?? TimeSpan.FromSeconds(3),
            shutdownTimeout: shutdownTimeout ?? TimeSpan.FromSeconds(2));

    private static OfficialUninstallElevatedRequestDraft ReadyDraft()
    {
        var operation = new OperationDescriptor
        {
            Kind = "uninstall.official.run",
            Title = "Lifecycle fake official uninstaller",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "snapshot-lifecycle-fake",
            RollbackRequired = true,
            ConfirmationAccepted = true,
            EvidenceSummary = "verified fake-worker lifecycle evidence",
            ConfirmationText = "Run the lifecycle fake official uninstaller?",
            AffectedPaths = [@"D:\Software\LifecycleFake"],
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "Lifecycle Fake",
                ["executablePath"] = @"D:\Software\LifecycleFake\Uninstall.exe",
                ["arguments"] = "/remove",
                ["snapshotManifestPath"] = @"D:\Evidence\lifecycle-snapshot.json",
                ["snapshotSha256"] = new string('A', 64),
                ["snapshotCanRestoreApplication"] = false,
                ["recoveryMethod"] = "ReinstallSource",
                ["recoveryReference"] = @"D:\Installers\LifecycleFakeSetup.exe"
            }
        };
        return new OfficialUninstallElevatedRequestDraft
        {
            Status = OfficialUninstallElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = DateTimeOffset.UtcNow,
            RequestId = $"lifecycle-request-{Guid.NewGuid():N}",
            Operation = operation,
            DescriptorSha256 = OfficialUninstallElevatedRequestComposer
                .ComputeDescriptorSha256(operation)
        };
    }

    private static string ElevatedWorkerExecutable()
    {
        var project = Path.GetDirectoryName(FindRepositoryFile(
            "src", "Css.Elevated", "Css.Elevated.csproj"))!;
        var configuration = new DirectoryInfo(AppContext.BaseDirectory)
            .Parent?.Name ?? "Debug";
        var executable = Path.Combine(
            project,
            "bin",
            configuration,
            "net8.0-windows",
            "Css.Elevated.exe");
        File.Exists(executable).Should().BeTrue(
            $"the fake elevated worker should be built at {executable}");
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
        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(segments));
    }

    private sealed class TestElevatedWorkerLauncher : IOfficialUninstallWorkerLauncher
    {
        private readonly string _executablePath;
        private readonly OfficialUninstallWorkerLaunchStatus _launchStatus;
        private readonly int _exposedProcessIdOffset;
        private readonly string? _overrideSessionId;
        private readonly int _responseDelayMilliseconds;
        private readonly int _exitDelayMilliseconds;
        private readonly bool _omitImageExpectation;
        private readonly string _workerMode;

        public TestElevatedWorkerLauncher(
            string executablePath,
            OfficialUninstallWorkerLaunchStatus launchStatus =
                OfficialUninstallWorkerLaunchStatus.Started,
            int exposedProcessIdOffset = 0,
            string? overrideSessionId = null,
            int responseDelayMilliseconds = 0,
            int exitDelayMilliseconds = 0,
            bool omitImageExpectation = false,
            string workerMode = "official-uninstall-fake-worker")
        {
            _executablePath = executablePath;
            _launchStatus = launchStatus;
            _exposedProcessIdOffset = exposedProcessIdOffset;
            _overrideSessionId = overrideSessionId;
            _responseDelayMilliseconds = responseDelayMilliseconds;
            _exitDelayMilliseconds = exitDelayMilliseconds;
            _omitImageExpectation = omitImageExpectation;
            _workerMode = workerMode;
        }

        public TestWorkerProcess? LastProcess { get; private set; }

        public ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
            OfficialUninstallWorkerLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_launchStatus != OfficialUninstallWorkerLaunchStatus.Started)
            {
                return ValueTask.FromResult(new OfficialUninstallWorkerLaunchResult
                {
                    Status = _launchStatus
                });
            }

            var start = new ProcessStartInfo
            {
                FileName = _executablePath,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var value in Arguments(request))
                start.ArgumentList.Add(value);
            var process = Process.Start(start)
                ?? throw new InvalidOperationException("The fake elevated worker did not start.");
            LastProcess = new TestWorkerProcess(process, _exposedProcessIdOffset);
            return ValueTask.FromResult(new OfficialUninstallWorkerLaunchResult
            {
                Status = OfficialUninstallWorkerLaunchStatus.Started,
                Process = LastProcess,
                ImageExpectation = _omitImageExpectation
                    ? null
                    : new OfficialUninstallWorkerImageExpectation
                    {
                        ExecutablePath = _executablePath,
                        Sha256 = FileSha256(_executablePath)
                    }
            });
        }

        private IReadOnlyList<string> Arguments(OfficialUninstallWorkerLaunchRequest request)
        {
            var values = new List<string>
            {
                _workerMode,
                "--pipe-name", request.PipeName,
                "--session-id", _overrideSessionId ?? request.SessionId,
                "--client-sid", request.Client.UserSid,
                "--client-pid", request.Client.ProcessId.ToString(CultureInfo.InvariantCulture),
                "--client-windows-session",
                request.Client.WindowsSessionId.ToString(CultureInfo.InvariantCulture),
                "--timeout-ms", request.TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture)
            };
            if (_responseDelayMilliseconds > 0)
            {
                values.Add("--fake-response-delay-ms");
                values.Add(_responseDelayMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
            if (_exitDelayMilliseconds > 0)
            {
                values.Add("--fake-exit-delay-ms");
                values.Add(_exitDelayMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
            return values;
        }
    }

    private sealed class TestProductionWorkerLauncher(
        IOfficialUninstallWorkerLauncher inner)
        : IOfficialUninstallProductionWorkerLauncher
    {
        public ValueTask<OfficialUninstallWorkerLaunchResult> LaunchAsync(
            OfficialUninstallWorkerLaunchRequest request,
            CancellationToken cancellationToken = default) =>
            inner.LaunchAsync(request, cancellationToken);
    }

    private sealed class TestWorkerImageInspector : IOfficialUninstallWorkerImageInspector
    {
        private readonly string _executablePath;
        private readonly string _sha256;
        private readonly bool _fail;

        public TestWorkerImageInspector(
            string sourceExecutablePath,
            string? executablePath = null,
            string? sha256 = null,
            bool fail = false)
        {
            _executablePath = executablePath ?? sourceExecutablePath;
            _sha256 = sha256 ?? FileSha256(sourceExecutablePath);
            _fail = fail;
        }

        public int InspectCount { get; private set; }

        public ValueTask<OfficialUninstallWorkerImageEvidence> InspectAsync(
            IOfficialUninstallWorkerProcess process,
            CancellationToken cancellationToken = default)
        {
            InspectCount++;
            cancellationToken.ThrowIfCancellationRequested();
            if (_fail)
                throw new IOException("Injected private image inspection failure.");
            return ValueTask.FromResult(new OfficialUninstallWorkerImageEvidence
            {
                ExecutablePath = _executablePath,
                Sha256 = _sha256
            });
        }
    }

    private sealed class CurrentProcessProbe : IOfficialUninstallWorkerProcess
    {
        public int ProcessId => Environment.ProcessId;
        public int WindowsSessionId => Process.GetCurrentProcess().SessionId;
        public bool HasExited => false;
        public Task<int> WaitForExitAsync(TimeSpan timeout, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task TerminateTreeAsync(TimeSpan timeout, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class TestWorkerProcess : IOfficialUninstallWorkerProcess
    {
        private readonly Process _process;
        private readonly int _exposedProcessIdOffset;
        private bool _disposed;

        public TestWorkerProcess(Process process, int exposedProcessIdOffset)
        {
            _process = process;
            _exposedProcessIdOffset = exposedProcessIdOffset;
            ProcessId = process.Id + exposedProcessIdOffset;
            WindowsSessionId = process.SessionId;
        }

        public int ProcessId { get; }
        public int WindowsSessionId { get; }
        public bool ExitedObserved { get; private set; }
        public bool TerminateCalled { get; private set; }
        public bool HasExited
        {
            get
            {
                if (ExitedObserved)
                    return true;
                if (_disposed)
                    return ExitedObserved;
                try
                {
                    ExitedObserved = _process.HasExited;
                    return ExitedObserved;
                }
                catch
                {
                    return ExitedObserved;
                }
            }
        }

        public async Task<int> WaitForExitAsync(
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(timeout);
            await _process.WaitForExitAsync(deadline.Token);
            ExitedObserved = true;
            return _process.ExitCode;
        }

        public async Task TerminateTreeAsync(
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            TerminateCalled = true;
            if (!_process.HasExited)
                _process.Kill(entireProcessTree: true);
            _ = await WaitForExitAsync(timeout, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            if (!HasExited)
                await TerminateTreeAsync(TimeSpan.FromSeconds(2));
            _process.Dispose();
            _disposed = true;
        }
    }

    private static string FileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
