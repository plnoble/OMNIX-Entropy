using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Windows;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using Css.Win32.Security;

namespace Css.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

#if DEBUG
        if (e.Args.Any(argument => string.Equals(
                argument,
                "--smoke-uninstall-worker-lifecycle",
                StringComparison.Ordinal)))
        {
            _ = RunWorkerLifecycleSmokeAsync();
            return;
        }

        if (e.Args.Any(argument => string.Equals(
                argument,
                "--smoke-uninstall-final-consent",
                StringComparison.Ordinal)))
        {
            _ = RunFinalConsentSmokeFlowAsync();
            return;
        }

        if (e.Args.Any(argument => string.Equals(
                argument,
                "--smoke-uninstall-post-scan-review",
                StringComparison.Ordinal)))
        {
            var smokeWindow = new UninstallPostScanResultWindow(CreatePostScanSmokeViewModel());
            MainWindow = smokeWindow;
            smokeWindow.Show();
            return;
        }
#endif

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

#if DEBUG
    private async Task RunWorkerLifecycleSmokeAsync()
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var availability = OfficialUninstallWorkerPathResolver.Resolve(
            AppContext.BaseDirectory);
        var trust = OfficialUninstallWorkerTrustPolicy.Evaluate(
            Environment.ProcessPath ?? string.Empty,
            availability,
            new WindowsAuthenticodeSignatureVerifier());
        OfficialUninstallWorkerResultViewModel viewModel;
        if (!trust.CanLaunchDevelopmentVerification)
        {
            viewModel = OfficialUninstallWorkerTrustPresenter.Create(trust);
        }
        else
        {
            OfficialUninstallWorkerLifecycleResult result;
            try
            {
                var client = new OfficialUninstallWorkerLifecycleClient(
                    new WindowsOfficialUninstallWorkerLauncher(
                        availability.ExecutablePath!,
                        trust.WorkerEvidence.FileSha256
                            ?? throw new InvalidOperationException(
                                "Development worker trust evidence has no hash.")),
                    new WindowsOfficialUninstallWorkerImageInspector(),
                    new WindowsOfficialUninstallCurrentProcessIdentityProvider(),
                    new WindowsOfficialUninstallPipePeerIdentityReader(),
                    startupTimeout: TimeSpan.FromSeconds(20),
                    bootstrapTimeout: TimeSpan.FromSeconds(15),
                    responseTimeout: TimeSpan.FromSeconds(15),
                    shutdownTimeout: TimeSpan.FromSeconds(5));
                result = await client.RunFakeOnceAsync(
                    CreateWorkerLifecycleSmokeDraft());
            }
            catch
            {
                result = new OfficialUninstallWorkerLifecycleResult
                {
                    Status = OfficialUninstallWorkerLifecycleStatus.LaunchFailed,
                    ChildExited = false
                };
            }
            viewModel = OfficialUninstallWorkerResultPresenter.Create(result);
        }

        var window = new OfficialUninstallWorkerResultWindow(viewModel);
        MainWindow = window;
        window.ShowDialog();
        Shutdown();
    }

    private static OfficialUninstallElevatedRequestDraft CreateWorkerLifecycleSmokeDraft()
    {
        var operation = new OperationDescriptor
        {
            Kind = "uninstall.official.run",
            Title = "OMNIX worker lifecycle verification",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "worker-lifecycle-smoke-snapshot",
            RollbackRequired = true,
            ConfirmationAccepted = true,
            EvidenceSummary = "fake-only worker lifecycle verification",
            ConfirmationText = "Verify the fake-only worker lifecycle?",
            AffectedPaths = [@"D:\OMNIX-Smoke\WorkerFixture"],
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "OMNIX Worker Fixture",
                ["executablePath"] = @"D:\OMNIX-Smoke\NeverRun.exe",
                ["arguments"] = string.Empty,
                ["snapshotManifestPath"] = @"D:\OMNIX-Smoke\NeverRead.json",
                ["snapshotSha256"] = new string('A', 64),
                ["snapshotCanRestoreApplication"] = false,
                ["recoveryMethod"] = OfficialUninstallRecoveryMethod.ReinstallSource.ToString(),
                ["recoveryReference"] = @"D:\OMNIX-Smoke\NeverRunSetup.exe"
            }
        };
        return new OfficialUninstallElevatedRequestDraft
        {
            Status = OfficialUninstallElevatedRequestStatus.Ready,
            MissingRequirements = [],
            PreparedAtUtc = DateTimeOffset.UtcNow,
            RequestId = $"worker-smoke-{Guid.NewGuid():N}",
            Operation = operation,
            DescriptorSha256 = OfficialUninstallElevatedRequestComposer
                .ComputeDescriptorSha256(operation)
        };
    }

    private async Task RunFinalConsentSmokeFlowAsync()
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var operation = CreateFinalConsentSmokeOperation();
        var consentView = OfficialUninstallFinalConsentPresenter.Create(operation);
        var visualGateIssuer = new OfficialUninstallVisualGateReceiptIssuer();
        var consentWindow = new OfficialUninstallFinalConsentWindow(
            consentView,
            visualGateIssuer,
            new OfficialUninstallFinalConsentVisualCapture(),
            () => DateTimeOffset.UtcNow);
        MainWindow = consentWindow;

        var accepted = consentWindow.ShowDialog() == true
            && consentWindow.Consent is not null
            && !string.IsNullOrWhiteSpace(consentWindow.VisualTicketId);
        if (accepted)
        {
            OfficialUninstallPostScanViewModel resultView;
            try
            {
                resultView = await RunFinalConsentFakePipeAsync(
                    operation,
                    consentWindow.Consent!,
                    visualGateIssuer,
                    consentWindow.VisualTicketId!);
            }
            catch
            {
                resultView = CreateFakePipeFailureViewModel();
            }

            var resultWindow = new UninstallPostScanResultWindow(resultView);
            MainWindow = resultWindow;
            resultWindow.ShowDialog();
        }

        Shutdown();
    }

    private static OperationDescriptor CreateFinalConsentSmokeOperation() =>
        new()
        {
            Kind = "uninstall.official.run",
            Title = "OMNIX Smoke App \u5b98\u65b9\u5378\u8f7d\u5668",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = "smoke-snapshot",
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "smoke-only verified evidence",
            ConfirmationText = "\u8fd0\u884c OMNIX Smoke App \u7684\u5b98\u65b9\u5378\u8f7d\u5668\uff1f",
            AffectedPaths = [@"D:\OMNIX-Smoke\App"],
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = "OMNIX Smoke App",
                ["executablePath"] = @"D:\OMNIX-Smoke\Uninstall.exe",
                ["arguments"] = "/remove",
                ["snapshotManifestPath"] = @"D:\OMNIX-Smoke\snapshot.json",
                ["snapshotSha256"] = new string('A', 64),
                ["snapshotCanRestoreApplication"] = false,
                ["recoveryMethod"] = OfficialUninstallRecoveryMethod.ReinstallSource.ToString(),
                ["recoveryReference"] = @"D:\OMNIX-Smoke\Setup.exe"
            }
        };

    private static async Task<OfficialUninstallPostScanViewModel> RunFinalConsentFakePipeAsync(
        OperationDescriptor operation,
        OfficialUninstallFinalUserConsent consent,
        OfficialUninstallVisualGateReceiptIssuer visualGateIssuer,
        string visualTicketId)
    {
        var now = consent.ConfirmedAtUtc.ToUniversalTime();
        var draft = new OfficialUninstallElevatedRequestSession(visualGateIssuer).Create(
            new OfficialUninstallExecutionGateResult
            {
                CanRequestExecution = true,
                PrimaryButtonText = "DEBUG fake pipe only",
                BlockingReasons = [],
                CommandTrust = OfficialUninstallCommandTrustResult.NotEvaluated(),
                Operation = operation
            },
            visualTicketId,
            consent,
            $"smoke-request-{Guid.NewGuid():N}",
            now);
        if (!draft.CanSubmit)
            return CreateFakePipeFailureViewModel();

        var sessionId = $"smoke-session-{Guid.NewGuid():N}";
        var sessionKey = RandomNumberGenerator.GetBytes(32);
        using var authenticatedClient = new OfficialUninstallAuthenticatedInMemoryClient(
            sessionId,
            sessionKey);
        var message = authenticatedClient.CreateMessage(draft, now);
        using var endpoint = new OfficialUninstallAuthenticatedInMemoryEndpoint(
            sessionId,
            sessionKey,
            (request, _) => Task.FromResult(CreateFakePipeResponse(request.RequestId!)));

        using var windowsIdentity = WindowsIdentity.GetCurrent();
        using var currentProcess = Process.GetCurrentProcess();
        var peer = new OfficialUninstallPipePeerIdentity
        {
            UserSid = windowsIdentity.User?.Value
                ?? throw new InvalidOperationException("Current Windows SID is unavailable."),
            ProcessId = Environment.ProcessId,
            WindowsSessionId = currentProcess.SessionId
        };
        var identityReader = new WindowsOfficialUninstallPipePeerIdentityReader();
        var pipeName = $"omnix-uninstall-smoke-{Guid.NewGuid():N}";
        var server = new OfficialUninstallFakeNamedPipeServer(
            pipeName,
            peer,
            identityReader,
            endpoint,
            () => now,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
        var client = new OfficialUninstallFakeNamedPipeClient(
            pipeName,
            peer,
            identityReader,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));

        var serverTask = server.ServeOnceAsync();
        var clientResult = await client.SendAsync(message);
        var serverResult = await serverTask;
        if (serverResult.Status != OfficialUninstallTransportStatus.Completed
            || clientResult.Status != OfficialUninstallTransportStatus.Completed
            || clientResult.Response is null)
        {
            return CreateFakePipeFailureViewModel();
        }

        var responseView = OfficialUninstallElevatedResponsePresenter.Create(
            draft,
            clientResult.Response);
        return responseView.PostScan ?? CreateFakePipeFailureViewModel();
    }

    private static OfficialUninstallElevatedResponseEnvelope CreateFakePipeResponse(
        string requestId) =>
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
                    ResidueCandidateCount = 3,
                    PathResidueCandidateCount = 2,
                    VerifiedBackgroundResidueCount = 1,
                    UnverifiedBackgroundHintCount = 0,
                    RequiresBackgroundRescan = false,
                    Summary = @"DEBUG private path must not cross IPC: C:\Users\Example\Secret"
                }
            })
        };

    private static OfficialUninstallPostScanViewModel CreateFakePipeFailureViewModel() =>
        OfficialUninstallPostScanPresenter.Create(
            "OMNIX Smoke App",
            OfficialUninstallPostScanResult.NotRun(
                "The DEBUG fake pipe did not return a verified result."));

    private static OfficialUninstallPostScanViewModel CreatePostScanSmokeViewModel() =>
        new()
        {
            State = OfficialUninstallPostScanState.ReviewNeeded,
            Title = "\u53d1\u73b0\u5378\u8f7d\u540e\u7684\u5f85\u68c0\u67e5\u5185\u5bb9",
            StatusLabel = "\u9700\u8981\u68c0\u67e5",
            Conclusion = "\u53d1\u73b0\u4e00\u4e9b\u53ef\u80fd\u7684\u6b8b\u7559\uff0c\u76ee\u524d\u6ca1\u6709\u81ea\u52a8\u5904\u7406\u3002",
            Facts =
            [
                "\u53d1\u73b0 2 \u9879\u4f4e\u98ce\u9669\u7f13\u5b58\u6216\u65e5\u5fd7\u3002",
                "\u53d1\u73b0 1 \u9879\u9700\u8981\u4fdd\u7559\u7684\u9ad8\u98ce\u9669\u540e\u53f0\u8bb0\u5f55\u3002",
                "\u6240\u6709\u5185\u5bb9\u90fd\u8fd8\u5728\u539f\u4f4d\u3002"
            ],
            AgentAdvice = "\u5efa\u8bae\u5148\u67e5\u770b\u5206\u7ec4\u6e05\u5355\u3002\u53ea\u6709\u4f4e\u98ce\u9669\u5185\u5bb9\u624d\u80fd\u5728\u4f60\u518d\u6b21\u786e\u8ba4\u540e\u8fdb\u5165\u9694\u79bb\u533a\u3002",
            PrimaryActionText = "\u67e5\u770b\u6b8b\u7559\u6e05\u5355",
            PrimaryAction = OfficialUninstallPostScanAction.ReviewResidue,
            CanReviewResidue = true,
            TechnicalDetailsAvailable = true
        };
#endif
}
