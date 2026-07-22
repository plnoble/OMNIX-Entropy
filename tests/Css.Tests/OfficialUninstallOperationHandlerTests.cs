using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Software;
using Css.Core.Timeline;
using Css.Core.Uninstall;
using Css.Elevated.Uninstall;
using Css.Ipc.Uninstall;
using Css.Snapshot.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class OfficialUninstallOperationHandlerTests
{
    [Fact]
    public async Task Authenticated_fake_transport_runs_fake_launcher_and_returns_beginner_result()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(new OfficialUninstallPostScanResult
            {
                Success = true,
                SoftwareStillPresent = false,
                ResidueCandidateCount = 2,
                Summary = @"fake technical scan C:\Users\Secret"
            });
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            var gate = new OfficialUninstallExecutionGateResult
            {
                CanRequestExecution = true,
                PrimaryButtonText = "confirm",
                BlockingReasons = [],
                CommandTrust = OfficialUninstallCommandTrustResult.NotEvaluated(),
                Operation = fixture.Operation
            };
            var request = OfficialUninstallElevatedRequestComposer.Create(
                gate,
                new OfficialUninstallVisualGateReceipt
                {
                    UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
                    ScreenshotSha256 = new string('B', 64),
                    CapturedAtUtc = fixture.Now.AddMinutes(-2),
                    RecoveryTruthVisible = true,
                    FinalConfirmationVisible = true,
                    TechnicalDetailsCollapsedByDefault = true,
                    NoExecutionControlDuringPreparation = true
                },
                new OfficialUninstallFinalUserConsent
                {
                    ConfirmationText = fixture.Operation.ConfirmationText
                        ?? throw new InvalidOperationException("Fixture confirmation text is required."),
                    ConfirmedAtUtc = fixture.Now.AddMinutes(-1),
                    OfficialCommandConfirmed = true,
                    AppsClosedConfirmed = true,
                    NoAutomaticUndoAcknowledged = true,
                    PostUninstallRescanConfirmed = true,
                    ExecutionRequested = true
                },
                "fake-end-to-end-request",
                fixture.Now);
            var key = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();
            var endpoint = new OfficialUninstallAuthenticatedInMemoryEndpoint(
                "fake-session",
                key,
                async (draft, token) => new OfficialUninstallElevatedResponseEnvelope
                {
                    RequestId = draft.RequestId!,
                    Result = await pipeline.ExecuteAsync(draft.Operation!, token)
                });
            var client = new OfficialUninstallAuthenticatedInMemoryClient(
                "fake-session",
                key,
                () => "fake-message",
                () => "fake-nonce");

            var transported = await client.SendAsync(request, endpoint, fixture.Now);
            var view = OfficialUninstallElevatedResponsePresenter.Create(
                request,
                transported.Response!);

            transported.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
            launcher.CallCount.Should().Be(1);
            scanner.CallCount.Should().Be(1);
            view.State.Should().Be(OfficialUninstallElevatedResponseState.PostScanReady);
            view.PostScan.Should().NotBeNull();
            view.PostScan!.State.Should().Be(OfficialUninstallPostScanState.ReviewNeeded);
            view.VisibleText.Should().NotContain(@"C:\Users\Secret");
            view.CanExecuteDirectly.Should().BeFalse();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Production_session_denies_before_the_operation_pipeline_when_package_trust_fails()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0));
            var server = new FakeOneShotWorkerServer(ReadyDraft(fixture.Operation));
            var session = new OfficialUninstallProductionWorkerSession(
                server,
                new StaticPackageAuthorizer(canAuthorize: false),
                new OfficialUninstallOperationHandler(
                    launcher,
                    scanner,
                    new ActionTimelineStore(fixture.TimelinePath),
                    File.Exists,
                    UninstallEvidenceSnapshotStore.ComputeSha256,
                    () => fixture.Now));

            var result = await session.ServeOnceAsync(WorkerOptions());

            result.Status.Should().Be(OfficialUninstallTransportStatus.AuthorizationFailed);
            server.AuthorizationCallCount.Should().Be(1);
            server.ResponseFactoryCallCount.Should().Be(0);
            launcher.CallCount.Should().Be(0);
            scanner.CallCount.Should().Be(0);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Authorized_production_session_executes_once_through_pipeline_and_post_scan()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0));
            var server = new FakeOneShotWorkerServer(ReadyDraft(fixture.Operation));
            var session = new OfficialUninstallProductionWorkerSession(
                server,
                new StaticPackageAuthorizer(canAuthorize: true),
                new OfficialUninstallOperationHandler(
                    launcher,
                    scanner,
                    new ActionTimelineStore(fixture.TimelinePath),
                    File.Exists,
                    UninstallEvidenceSnapshotStore.ComputeSha256,
                    () => fixture.Now));

            var result = await session.ServeOnceAsync(WorkerOptions());
            var payload = result.Response!.Result.Payload
                .Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;

            result.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
            result.Response.Result.Success.Should().BeTrue();
            payload.UninstallerCompleted.Should().BeTrue();
            payload.PostScan.Success.Should().BeTrue();
            server.AuthorizationCallCount.Should().Be(1);
            server.ResponseFactoryCallCount.Should().Be(1);
            launcher.CallCount.Should().Be(1);
            scanner.CallCount.Should().Be(1);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Production_session_rejects_stale_preparation_even_after_package_authorization()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0));
            var server = new FakeOneShotWorkerServer(
                ReadyDraft(fixture.Operation, fixture.Now.AddMinutes(-16)));
            var session = new OfficialUninstallProductionWorkerSession(
                server,
                new StaticPackageAuthorizer(canAuthorize: true),
                new OfficialUninstallOperationHandler(
                    launcher,
                    scanner,
                    new ActionTimelineStore(fixture.TimelinePath),
                    File.Exists,
                    UninstallEvidenceSnapshotStore.ComputeSha256,
                    () => fixture.Now),
                () => fixture.Now);

            var result = await session.ServeOnceAsync(WorkerOptions());

            result.Status.Should().Be(OfficialUninstallTransportStatus.Completed);
            result.Response!.Result.Success.Should().BeFalse();
            result.Response.Result.Error.Should().Contain("stale");
            launcher.CallCount.Should().Be(0);
            scanner.CallCount.Should().Be(0);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_runs_only_after_pipeline_confirmation_and_successful_post_scan()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(new OfficialUninstallPostScanResult
            {
                Success = true,
                SoftwareStillPresent = false,
                ResidueCandidateCount = 2,
                Summary = "official uninstall completed; residue review required"
            });
            var timeline = new ActionTimelineStore(fixture.TimelinePath);
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                timeline,
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var blocked = await pipeline.ExecuteAsync(fixture.Operation);

            blocked.Success.Should().BeFalse();
            launcher.CallCount.Should().Be(0);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));
            var payload = result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;
            var entries = await timeline.LoadRecentAsync(5);

            result.Success.Should().BeTrue();
            launcher.CallCount.Should().Be(1);
            scanner.CallCount.Should().Be(1);
            launcher.LastRequest!.ExecutablePath.Should().Be(fixture.UninstallerPath);
            launcher.LastRequest.RequiresElevation.Should().BeFalse();
            payload.UninstallerCompleted.Should().BeTrue();
            payload.ExitCode.Should().Be(0);
            payload.PostScan.Success.Should().BeTrue();
            payload.RequiresPostScanRetry.Should().BeFalse();
            entries.Should().ContainSingle();
            entries[0].RestoreState.Should().Be(RestoreState.NotRestorable);
            entries[0].RestoreOperationKind.Should().BeNull();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_creates_post_scanner_from_the_exact_validated_manifest()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            UninstallEvidenceSnapshotManifest? receivedManifest = null;
            var scanner = new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0));
            var handler = new OfficialUninstallOperationHandler(
                new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0)),
                manifest =>
                {
                    receivedManifest = manifest;
                    return scanner;
                },
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));

            result.Success.Should().BeTrue(result.Error);
            receivedManifest.Should().NotBeNull();
            receivedManifest!.SnapshotId.Should().Be(fixture.SnapshotEvidence.SnapshotId);
            receivedManifest.SoftwareName.Should().Be("Example App");
            scanner.CallCount.Should().Be(1);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_rejects_tampered_snapshot_before_calling_launcher()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            await File.AppendAllTextAsync(fixture.SnapshotEvidence.ManifestPath, "tampered");
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0));
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("\u54c8\u5e0c");
            launcher.CallCount.Should().Be(0);
            scanner.CallCount.Should().Be(0);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_does_not_post_scan_when_the_official_uninstaller_never_started()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(
                OfficialUninstallerLaunchResult.NotStarted("start refused"));
            var scanner = new FakePostScanner(
                OfficialUninstallPostScanResult.Completed(false, 0));
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));
            var payload = result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;

            result.Success.Should().BeFalse();
            payload.UninstallerStarted.Should().BeFalse();
            payload.PostScan.Success.Should().BeFalse();
            payload.RequiresPostScanRetry.Should().BeFalse();
            scanner.CallCount.Should().Be(0);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_reports_nonzero_exit_and_still_runs_the_mandatory_read_only_post_scan()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(1602));
            var scanner = new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0));
            var timeline = new ActionTimelineStore(fixture.TimelinePath);
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                timeline,
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));
            var payload = result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;
            var entries = await timeline.LoadRecentAsync(5);

            result.Success.Should().BeFalse();
            payload.UninstallerCompleted.Should().BeFalse();
            payload.ExitCode.Should().Be(1602);
            payload.PostScan.Success.Should().BeTrue();
            payload.RequiresPostScanRetry.Should().BeFalse();
            scanner.CallCount.Should().Be(1);
            entries.Should().ContainSingle();
            entries[0].RestoreState.Should().Be(RestoreState.NotRestorable);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Nonzero_exit_with_failed_post_scan_requires_a_read_only_rescan_retry()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(5));
            var scanner = new FakePostScanner(new OfficialUninstallPostScanResult
            {
                Success = false,
                Summary = "read-only scan failed"
            });
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));
            var payload = result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;

            result.Success.Should().BeFalse();
            payload.UninstallerStarted.Should().BeTrue();
            payload.UninstallerCompleted.Should().BeFalse();
            payload.PostScan.Success.Should().BeFalse();
            payload.RequiresPostScanRetry.Should().BeTrue();
            scanner.CallCount.Should().Be(1);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Caller_cancellation_during_mandatory_post_scan_is_propagated()
    {
        var fixture = await CreateFixtureAsync();
        using var cancellation = new CancellationTokenSource();
        try
        {
            var scanner = new CancelingPostScanner();
            var handler = new OfficialUninstallOperationHandler(
                new CancelAfterLaunch(OfficialUninstallerLaunchResult.Completed(0), cancellation),
                scanner,
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var action = () => pipeline.ExecuteAsync(
                Confirm(fixture.Operation),
                cancellation.Token);

            await action.Should().ThrowAsync<OperationCanceledException>();
            scanner.CallCount.Should().Be(1);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_reports_completed_uninstall_when_mandatory_post_scan_fails()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var scanner = new FakePostScanner(new OfficialUninstallPostScanResult
            {
                Success = false,
                Summary = "scan failed"
            });
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                scanner,
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));
            var payload = result.Payload.Should().BeOfType<OfficialUninstallHandlerPayload>().Subject;

            result.Success.Should().BeFalse();
            payload.UninstallerCompleted.Should().BeTrue();
            payload.RequiresPostScanRetry.Should().BeTrue();
            scanner.CallCount.Should().Be(1);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_accepts_verified_install_root_uninstaller_with_no_arguments()
    {
        var fixture = await CreateFixtureAsync(arguments: string.Empty);
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0)),
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation));

            result.Success.Should().BeTrue(result.Error);
            launcher.CallCount.Should().Be(1);
            launcher.LastRequest!.Arguments.Should().BeEmpty();
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public async Task Handler_rejects_command_arguments_changed_after_gate_creation()
    {
        var fixture = await CreateFixtureAsync();
        try
        {
            var launcher = new FakeLauncher(OfficialUninstallerLaunchResult.Completed(0));
            var handler = new OfficialUninstallOperationHandler(
                launcher,
                new FakePostScanner(OfficialUninstallPostScanResult.Completed(false, 0)),
                new ActionTimelineStore(fixture.TimelinePath),
                File.Exists,
                UninstallEvidenceSnapshotStore.ComputeSha256,
                () => fixture.Now);
            var changedArguments = fixture.Operation.Arguments.ToDictionary(pair => pair.Key, pair => pair.Value);
            changedArguments["arguments"] = "/remove /quiet";
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);

            var result = await pipeline.ExecuteAsync(Confirm(fixture.Operation, changedArguments));

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("no longer matches");
            launcher.CallCount.Should().Be(0);
        }
        finally
        {
            fixture.Dispose();
        }
    }

    [Fact]
    public void Elevated_handler_is_registered_only_behind_the_production_worker_gates()
    {
        var program = File.ReadAllText(FindRepositoryFile("src", "Css.Elevated", "Program.cs"));
        var handler = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "Uninstall", "OfficialUninstallOperationHandler.cs"));
        var worker = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Elevated", "OfficialUninstallProductionWorker.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));

        program.Should().Contain("official-uninstall-production-worker");
        worker.Should().Contain("OfficialUninstallProductionWorkerSession");
        worker.Should().Contain("OfficialUninstallOperationHandler");
        worker.Should().Contain("WindowsOfficialUninstallProductionPackageAuthorizer");
        app.Should().NotContain("official-uninstall-production-worker");
        app.Should().NotContain("OfficialUninstallOperationHandler");
        handler.Should().NotContain("Process.Start");
        handler.Should().NotContain("ProcessStartInfo");
        handler.Should().Contain("IOfficialUninstallerLauncher");
        handler.Should().Contain("IOfficialUninstallPostScanner");
    }

    private static async Task<HandlerFixture> CreateFixtureAsync(string arguments = "/remove")
    {
        var root = Path.Combine(Path.GetTempPath(), "omnix-official-uninstall-handler-" + Guid.NewGuid().ToString("N"));
        var installRoot = Path.Combine(root, "install");
        Directory.CreateDirectory(installRoot);
        var uninstallerPath = Path.Combine(installRoot, "Uninstall.exe");
        await File.WriteAllTextAsync(uninstallerPath, "not executable; fake launcher only");
        var now = new DateTimeOffset(2026, 7, 10, 20, 0, 0, TimeSpan.Zero);
        var profile = new SoftwareProfile
        {
            Name = "Example App",
            Publisher = "Example Inc.",
            InstallPath = installRoot,
            UninstallCommand = string.IsNullOrEmpty(arguments)
                ? $"\"{uninstallerPath}\""
                : $"\"{uninstallerPath}\" {arguments}",
            DataPaths = [Path.Combine(root, "data")]
        };
        var recovery = new OfficialUninstallRecoveryEvidence
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = Path.Combine(root, "ExampleSetup.exe"),
            CanRecoverApplication = true,
            UserDataBackupConfirmed = true
        };
        var snapshotStore = new UninstallEvidenceSnapshotStore(
            Path.Combine(root, "snapshots"),
            () => now);
        var snapshotEvidence = await snapshotStore.CreateAsync(profile, recovery);
        var readiness = new OfficialUninstallExecutionReadiness
        {
            FeatureEnabled = true,
            SnapshotId = snapshotEvidence.SnapshotId,
            SnapshotEvidence = snapshotEvidence,
            UserConfirmedOfficialCommand = true,
            UserConfirmedAppsClosed = true,
            UserConfirmedPostUninstallRescan = true,
            UserAcknowledgedNoAutomaticUndo = true,
            RecoveryEvidence = recovery
        };
        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            File.Exists,
            UninstallEvidenceSnapshotStore.ComputeSha256,
            now);
        gate.CanRequestExecution.Should().BeTrue();

        return new HandlerFixture(
            root,
            uninstallerPath,
            Path.Combine(root, "timeline.db"),
            now,
            snapshotEvidence,
            gate.Operation!);
    }

    private static OperationDescriptor Confirm(
        OperationDescriptor operation,
        IReadOnlyDictionary<string, object?>? arguments = null) =>
        new()
        {
            Kind = operation.Kind,
            Title = operation.Title,
            Source = operation.Source,
            Risk = operation.Risk,
            IsDestructive = operation.IsDestructive,
            RequiresElevation = operation.RequiresElevation,
            RequiresSnapshot = operation.RequiresSnapshot,
            SnapshotId = operation.SnapshotId,
            RollbackRequired = operation.RollbackRequired,
            ConfirmationAccepted = true,
            EvidenceSummary = operation.EvidenceSummary,
            EstimatedImpactBytes = operation.EstimatedImpactBytes,
            ConfirmationText = operation.ConfirmationText,
            AffectedPaths = operation.AffectedPaths,
            AffectedRegistryKeys = operation.AffectedRegistryKeys,
            AffectedServices = operation.AffectedServices,
            Arguments = arguments ?? operation.Arguments
        };

    private static OfficialUninstallElevatedRequestDraft ReadyDraft(
        OperationDescriptor operation,
        DateTimeOffset? preparedAtUtc = null)
    {
        var confirmed = Confirm(operation);
        return new OfficialUninstallElevatedRequestDraft
        {
            Status = OfficialUninstallElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = preparedAtUtc ?? DateTimeOffset.UtcNow,
            RequestId = $"production-session-{Guid.NewGuid():N}",
            Operation = confirmed,
            DescriptorSha256 = OfficialUninstallElevatedRequestComposer
                .ComputeDescriptorSha256(confirmed)
        };
    }

    private static OfficialUninstallOneShotWorkerOptions WorkerOptions()
    {
        var identity = new OfficialUninstallPipePeerIdentity
        {
            UserSid = "S-1-5-21-1-2-3-1001",
            ProcessId = Environment.ProcessId,
            WindowsSessionId = 0
        };
        return new OfficialUninstallOneShotWorkerOptions
        {
            PipeName = $"production-session-{Guid.NewGuid():N}",
            SessionId = $"production-session-{Guid.NewGuid():N}",
            ExpectedClient = identity,
            Worker = identity,
            Timeout = TimeSpan.FromSeconds(2)
        };
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

    private sealed class FakeLauncher(OfficialUninstallerLaunchResult result)
        : IOfficialUninstallerLauncher
    {
        public int CallCount { get; private set; }
        public OfficialUninstallerLaunchRequest? LastRequest { get; private set; }

        public Task<OfficialUninstallerLaunchResult> LaunchAsync(
            OfficialUninstallerLaunchRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(result);
        }
    }

    private sealed class FakePostScanner(OfficialUninstallPostScanResult result)
        : IOfficialUninstallPostScanner
    {
        public int CallCount { get; private set; }

        public Task<OfficialUninstallPostScanResult> ScanAsync(
            string softwareName,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    private sealed class CancelAfterLaunch(
        OfficialUninstallerLaunchResult result,
        CancellationTokenSource cancellation) : IOfficialUninstallerLauncher
    {
        public Task<OfficialUninstallerLaunchResult> LaunchAsync(
            OfficialUninstallerLaunchRequest request,
            CancellationToken cancellationToken)
        {
            cancellation.Cancel();
            return Task.FromResult(result);
        }
    }

    private sealed class CancelingPostScanner : IOfficialUninstallPostScanner
    {
        public int CallCount { get; private set; }

        public Task<OfficialUninstallPostScanResult> ScanAsync(
            string softwareName,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private sealed class StaticPackageAuthorizer(bool canAuthorize)
        : IOfficialUninstallProductionPackageAuthorizer
    {
        public OfficialUninstallProductionPackageTrustResult Authorize(
            OfficialUninstallPipePeerIdentity actualClient,
            OfficialUninstallPipePeerIdentity worker) =>
            new()
            {
                Status = canAuthorize
                    ? OfficialUninstallProductionPackageTrustStatus.Trusted
                    : OfficialUninstallProductionPackageTrustStatus.ClientNotTrusted
            };
    }

    private sealed class FakeOneShotWorkerServer(OfficialUninstallElevatedRequestDraft request)
        : IOfficialUninstallOneShotWorkerServer
    {
        public int AuthorizationCallCount { get; private set; }
        public int ResponseFactoryCallCount { get; private set; }

        public async Task<OfficialUninstallTransportResult> ServeOnceAsync(
            OfficialUninstallOneShotWorkerOptions options,
            OfficialUninstallOneShotWorkerAuthorization authorization,
            Func<OfficialUninstallElevatedRequestDraft, CancellationToken,
                Task<OfficialUninstallElevatedResponseEnvelope>> responseFactory,
            CancellationToken cancellationToken = default)
        {
            AuthorizationCallCount++;
            if (!await authorization(
                    options.ExpectedClient,
                    options.Worker,
                    cancellationToken))
            {
                return new OfficialUninstallTransportResult
                {
                    Status = OfficialUninstallTransportStatus.AuthorizationFailed
                };
            }

            ResponseFactoryCallCount++;
            return new OfficialUninstallTransportResult
            {
                Status = OfficialUninstallTransportStatus.Completed,
                Response = await responseFactory(request, cancellationToken)
            };
        }
    }

    private sealed record HandlerFixture(
        string Root,
        string UninstallerPath,
        string TimelinePath,
        DateTimeOffset Now,
        OfficialUninstallSnapshotEvidence SnapshotEvidence,
        OperationDescriptor Operation) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
