using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class OfficialUninstallElevatedBoundaryTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 11, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Request_composer_refuses_without_visual_proof_and_exact_final_consent()
    {
        var gate = ReadyGate(out _);

        var draft = OfficialUninstallElevatedRequestComposer.Create(
            gate,
            visualGate: null,
            finalConsent: null,
            requestId: "request-1",
            now: Now);

        draft.Status.Should().Be(OfficialUninstallElevatedRequestStatus.Refused);
        draft.CanSubmit.Should().BeFalse();
        draft.Operation.Should().BeNull();
        draft.DescriptorSha256.Should().BeNull();
        draft.MissingRequirements.Should().Contain(item => item.Contains("界面", StringComparison.Ordinal));
        draft.MissingRequirements.Should().Contain(item => item.Contains("最终确认", StringComparison.Ordinal));
    }

    [Fact]
    public void Request_composer_binds_fresh_visual_proof_exact_consent_and_descriptor_copy()
    {
        var gate = ReadyGate(out var mutableArguments);

        var draft = OfficialUninstallElevatedRequestComposer.Create(
            gate,
            CompleteVisualGate(),
            CompleteConsent(),
            requestId: "request-2",
            now: Now);

        mutableArguments["arguments"] = "/quiet";

        draft.Status.Should().Be(OfficialUninstallElevatedRequestStatus.Ready);
        draft.CanSubmit.Should().BeTrue();
        draft.RequestId.Should().Be("request-2");
        draft.PreparedAtUtc.Should().Be(CompleteConsent().ConfirmedAtUtc);
        draft.DescriptorSha256.Should().MatchRegex("^[A-F0-9]{64}$");
        draft.Operation.Should().NotBeNull();
        draft.Operation!.ConfirmationAccepted.Should().BeTrue();
        draft.Operation.Arguments["arguments"].Should().Be("/remove");
        draft.Operation.Kind.Should().Be("uninstall.official.run");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Request_composer_refuses_stale_visual_proof_or_changed_confirmation(
        bool staleVisualProof,
        bool changedConfirmation)
    {
        var gate = ReadyGate(out _);
        var visualGate = CompleteVisualGate() with
        {
            CapturedAtUtc = staleVisualProof ? Now.AddDays(-2) : Now.AddMinutes(-2)
        };
        var consent = CompleteConsent() with
        {
            ConfirmationText = changedConfirmation ? "卸载另一个软件？" : "运行 Example App 的官方卸载器？"
        };

        var draft = OfficialUninstallElevatedRequestComposer.Create(
            gate,
            visualGate,
            consent,
            requestId: "request-3",
            now: Now);

        draft.Status.Should().Be(OfficialUninstallElevatedRequestStatus.Refused);
        draft.CanSubmit.Should().BeFalse();
        draft.Operation.Should().BeNull();
    }

    [Fact]
    public void Response_presenter_correlates_request_and_returns_path_free_post_scan_conclusion()
    {
        var request = OfficialUninstallElevatedRequestComposer.Create(
            ReadyGate(out _),
            CompleteVisualGate(),
            CompleteConsent(),
            requestId: "request-4",
            now: Now);
        var payload = new OfficialUninstallHandlerPayload
        {
            UninstallerStarted = true,
            UninstallerCompleted = true,
            ExitCode = 0,
            RequiresPostScanRetry = false,
            PostScan = new OfficialUninstallPostScanResult
            {
                Success = true,
                SoftwareStillPresent = false,
                ResidueCandidateCount = 0,
                Summary = @"scan source C:\Users\Example\AppData\Secret"
            }
        };
        var response = new OfficialUninstallElevatedResponseEnvelope
        {
            RequestId = "request-4",
            Result = OperationResult.Ok(payload: payload)
        };

        var view = OfficialUninstallElevatedResponsePresenter.Create(request, response);

        view.State.Should().Be(OfficialUninstallElevatedResponseState.PostScanReady);
        view.CanExecuteDirectly.Should().BeFalse();
        view.PostScan.Should().NotBeNull();
        view.PostScan!.State.Should().Be(OfficialUninstallPostScanState.NoVisibleResidue);
        view.VisibleText.Should().NotContain(@"C:\Users\Example");
        view.VisibleText.Should().NotContain("Secret");
    }

    [Fact]
    public void Response_presenter_refuses_mismatched_or_untyped_response()
    {
        var request = OfficialUninstallElevatedRequestComposer.Create(
            ReadyGate(out _),
            CompleteVisualGate(),
            CompleteConsent(),
            requestId: "request-5",
            now: Now);
        var mismatched = new OfficialUninstallElevatedResponseEnvelope
        {
            RequestId = "another-request",
            Result = OperationResult.Ok(payload: new object())
        };

        var view = OfficialUninstallElevatedResponsePresenter.Create(request, mismatched);

        view.State.Should().Be(OfficialUninstallElevatedResponseState.InvalidResponse);
        view.CanExecuteDirectly.Should().BeFalse();
        view.PostScan.Should().BeNull();
        view.VisibleText.Should().Contain("无法确认");
    }

    [Fact]
    public void Request_session_consumes_visual_ticket_once_before_composing_request()
    {
        var issuer = new OfficialUninstallVisualGateReceiptIssuer(
            ticketIdFactory: () => "visual-session-ticket");
        var issue = issuer.Issue(new OfficialUninstallVisualGateIssueRequest
        {
            UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
            ScreenshotPng =
            [
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52
            ],
            CapturedAtUtc = Now.AddMinutes(-2),
            RecoveryTruthVisible = true,
            FinalConfirmationVisible = true,
            TechnicalDetailsCollapsedByDefault = true,
            NoExecutionControlDuringPreparation = true
        }, Now);
        var session = new OfficialUninstallElevatedRequestSession(issuer);

        var first = session.Create(
            ReadyGate(out _),
            issue.TicketId!,
            CompleteConsent(),
            "session-request-1",
            Now.AddSeconds(1));
        var replay = session.Create(
            ReadyGate(out _),
            issue.TicketId!,
            CompleteConsent(),
            "session-request-2",
            Now.AddSeconds(2));

        first.Status.Should().Be(OfficialUninstallElevatedRequestStatus.Ready);
        first.CanSubmit.Should().BeTrue();
        replay.Status.Should().Be(OfficialUninstallElevatedRequestStatus.Refused);
        replay.CanSubmit.Should().BeFalse();
        replay.MissingRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public void Boundary_is_unregistered_and_contains_no_execution_or_mutation_path()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallElevatedBoundary.cs"));
        var program = File.ReadAllText(FindRepositoryFile("src", "Css.Elevated", "Program.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));
        var appProject = File.ReadAllText(FindRepositoryFile("src", "Css.App", "Css.App.csproj"));

        source.Should().NotContain("ExecuteAsync");
        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
        source.Should().NotContain("Quarantine");
        program.Should().NotContain("OfficialUninstallElevatedRequestComposer");
        app.Should().Contain("#if DEBUG");
        app.Should().Contain("OfficialUninstallElevatedRequestSession");
        app.Should().Contain("consentWindow.VisualTicketId");
        app.Should().NotContain("OfficialUninstallOperationHandler");
        app.Should().NotContain("WindowsOfficialUninstallerLauncher");
        app.Should().NotContain("SafetyOperationPipeline");
        appProject.Should().NotContain("<ProjectReference Include=\"..\\Css.Elevated");
    }

    private static OfficialUninstallExecutionGateResult ReadyGate(
        out Dictionary<string, object?> mutableArguments)
    {
        mutableArguments = new Dictionary<string, object?>
        {
            ["executablePath"] = @"D:\Software\Example\Uninstall.exe",
            ["arguments"] = "/remove",
            ["snapshotManifestPath"] = @"D:\Evidence\snapshot.json",
            ["snapshotSha256"] = new string('A', 64),
            ["snapshotCanRestoreApplication"] = false,
            ["recoveryMethod"] = OfficialUninstallRecoveryMethod.ReinstallSource.ToString(),
            ["recoveryReference"] = @"D:\Installers\ExampleSetup.exe"
        };
        return new OfficialUninstallExecutionGateResult
        {
            CanRequestExecution = true,
            PrimaryButtonText = "确认运行官方卸载器",
            BlockingReasons = [],
            CommandTrust = OfficialUninstallCommandTrustResult.NotEvaluated(),
            Operation = new OperationDescriptor
            {
                Kind = "uninstall.official.run",
                Title = "Example App 官方卸载器",
                Source = OperationSource.Manual,
                Risk = RiskLevel.High,
                IsDestructive = true,
                RequiresElevation = true,
                RequiresSnapshot = true,
                SnapshotId = "snapshot-1",
                RollbackRequired = true,
                ConfirmationAccepted = false,
                EvidenceSummary = "verified evidence",
                ConfirmationText = "运行 Example App 的官方卸载器？",
                AffectedPaths = [@"D:\Software\Example"],
                AffectedServices = ["ExampleService"],
                Arguments = mutableArguments
            }
        };
    }

    private static OfficialUninstallVisualGateReceipt CompleteVisualGate() =>
        new()
        {
            UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
            ScreenshotSha256 = new string('B', 64),
            CapturedAtUtc = Now.AddMinutes(-2),
            RecoveryTruthVisible = true,
            FinalConfirmationVisible = true,
            TechnicalDetailsCollapsedByDefault = true,
            NoExecutionControlDuringPreparation = true
        };

    private static OfficialUninstallFinalUserConsent CompleteConsent() =>
        new()
        {
            ConfirmationText = "运行 Example App 的官方卸载器？",
            ConfirmedAtUtc = Now.AddMinutes(-1),
            OfficialCommandConfirmed = true,
            AppsClosedConfirmed = true,
            NoAutomaticUndoAcknowledged = true,
            PostUninstallRescanConfirmed = true,
            ExecutionRequested = true
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
