using Css.Core.Apps;
using Css.Core.Software;
using Css.Core.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public sealed class OfficialUninstallRequestPreparationServiceTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private const string SnapshotHash =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public void Ready_evidence_and_exact_consent_create_one_request()
    {
        var issuer = Issuer(out var ticketId);

        var request = Create(issuer, ticketId, Consent());
        var replay = Create(issuer, ticketId, Consent(), requestId: "request-replay");

        request.CanSubmit.Should().BeTrue();
        request.Operation.Should().NotBeNull();
        request.Operation!.ConfirmationAccepted.Should().BeTrue();
        request.Operation.Kind.Should().Be("uninstall.official.run");
        replay.CanSubmit.Should().BeFalse();
        replay.MissingRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public void Missing_apps_closed_confirmation_is_refused_and_burns_ticket()
    {
        var issuer = Issuer(out var ticketId);
        var consent = Consent() with { AppsClosedConfirmed = false };

        var request = Create(issuer, ticketId, consent);
        var replay = Create(issuer, ticketId, Consent(), requestId: "request-after-refusal");

        request.CanSubmit.Should().BeFalse();
        request.Operation.Should().BeNull();
        replay.CanSubmit.Should().BeFalse();
    }

    [Fact]
    public void Changed_snapshot_hash_is_refused_before_a_request_can_submit()
    {
        var issuer = Issuer(out var ticketId);

        var request = Create(
            issuer,
            ticketId,
            Consent(),
            hashResolver: _ => new string('B', 64));

        request.CanSubmit.Should().BeFalse();
        request.Operation.Should().BeNull();
        request.MissingRequirements.Should().NotBeEmpty();
    }

    [Fact]
    public void Preparation_source_has_no_execution_or_mutation_authority()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallRequestPreparationService.cs"));

        source.Should().Contain("OfficialUninstallExecutionGate.Evaluate");
        source.Should().Contain("OfficialUninstallElevatedRequestSession");
        source.Should().Contain("UserConfirmedAppsClosed = consent.AppsClosedConfirmed");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("ExecuteAsync");
        source.Should().NotContain("SafetyOperationPipeline");
        source.Should().NotContain("File.Delete");
        source.Should().NotContain("File.Move");
    }

    private static OfficialUninstallElevatedRequestDraft Create(
        OfficialUninstallVisualGateReceiptIssuer issuer,
        string ticketId,
        OfficialUninstallFinalUserConsent consent,
        string requestId = "request-ready",
        Func<string, string?>? hashResolver = null) =>
        OfficialUninstallRequestPreparationService.Create(
            Profile(),
            Snapshot(),
            Recovery(),
            consent,
            issuer,
            ticketId,
            requestId,
            Now,
            _ => true,
            hashResolver ?? (_ => SnapshotHash));

    private static OfficialUninstallVisualGateReceiptIssuer Issuer(out string ticketId)
    {
        const string issuedTicketId = "visual-ticket";
        ticketId = issuedTicketId;
        var issuer = new OfficialUninstallVisualGateReceiptIssuer(() => issuedTicketId);
        var issued = issuer.Issue(new OfficialUninstallVisualGateIssueRequest
        {
            UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
            ScreenshotPng =
            [
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52
            ],
            CapturedAtUtc = Now.AddSeconds(-1),
            RecoveryTruthVisible = true,
            FinalConfirmationVisible = true,
            TechnicalDetailsCollapsedByDefault = true,
            NoExecutionControlDuringPreparation = true
        }, Now);
        issued.TicketId.Should().Be(issuedTicketId);
        return issuer;
    }

    private static SoftwareProfile Profile() =>
        new()
        {
            Name = "Example App",
            Publisher = "Example Publisher",
            SignatureSubject = "Example Publisher",
            InstallPath = @"D:\Software\Example",
            UninstallCommand = @"""D:\Software\Example\Uninstall.exe"" /remove"
        };

    private static OfficialUninstallSnapshotEvidence Snapshot() =>
        new()
        {
            SnapshotId = "snapshot-ready",
            ManifestPath = @"D:\Evidence\snapshot.json",
            SoftwareName = "Example App",
            CreatedAtUtc = Now.AddMinutes(-1),
            Sha256 = SnapshotHash,
            CanRestoreApplication = false
        };

    private static OfficialUninstallRecoveryEvidence Recovery() =>
        new()
        {
            Method = OfficialUninstallRecoveryMethod.ReinstallSource,
            Reference = @"D:\Installers\ExampleSetup.exe",
            CanRecoverApplication = true,
            UserDataBackupConfirmed = true
        };

    private static OfficialUninstallFinalUserConsent Consent() =>
        new()
        {
            ConfirmationText = "\u8fd0\u884c Example App \u7684\u5b98\u65b9\u5378\u8f7d\u5668\uff1f",
            ConfirmedAtUtc = Now,
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
