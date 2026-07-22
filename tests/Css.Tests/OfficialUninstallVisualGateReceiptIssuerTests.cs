using System.Security.Cryptography;
using Css.Core.Uninstall;
using FluentAssertions;

namespace Css.Tests;

public class OfficialUninstallVisualGateReceiptIssuerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 11, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Issuer_hashes_valid_png_evidence_and_consumes_the_receipt_once()
    {
        var issuer = new OfficialUninstallVisualGateReceiptIssuer(
            ticketIdFactory: () => "visual-ticket-1");
        var png = MinimalPngEvidence();

        var issued = issuer.Issue(CompleteRequest(png), Now);
        var consumed = issuer.Consume(issued.TicketId!, Now.AddMinutes(1));
        var replay = issuer.Consume(issued.TicketId!, Now.AddMinutes(2));

        issued.Status.Should().Be(OfficialUninstallVisualGateIssueStatus.Issued);
        issued.TicketId.Should().Be("visual-ticket-1");
        issued.MissingRequirements.Should().BeEmpty();
        consumed.Status.Should().Be(OfficialUninstallVisualGateConsumeStatus.Consumed);
        consumed.Receipt.Should().NotBeNull();
        consumed.Receipt!.ScreenshotSha256.Should().Be(
            Convert.ToHexString(SHA256.HashData(png)));
        consumed.Receipt.CapturedAtUtc.Should().Be(Now.AddSeconds(-2));
        replay.Status.Should().Be(OfficialUninstallVisualGateConsumeStatus.AlreadyConsumed);
        replay.Receipt.Should().BeNull();
    }

    [Fact]
    public void Issuer_refuses_invalid_png_contract_or_incomplete_visible_state()
    {
        var issuer = new OfficialUninstallVisualGateReceiptIssuer();
        var request = CompleteRequest([1, 2, 3]) with
        {
            UiContractVersion = "old-contract",
            FinalConfirmationVisible = false
        };

        var result = issuer.Issue(request, Now);

        result.Status.Should().Be(OfficialUninstallVisualGateIssueStatus.Refused);
        result.TicketId.Should().BeNull();
        result.MissingRequirements.Should().HaveCountGreaterThanOrEqualTo(3);
        issuer.OutstandingTicketCount.Should().Be(0);
    }

    [Fact]
    public void Issuer_refuses_stale_or_future_capture_and_expired_ticket_cannot_be_consumed()
    {
        var issuer = new OfficialUninstallVisualGateReceiptIssuer(
            ticketIdFactory: () => "visual-ticket-2");
        var stale = issuer.Issue(
            CompleteRequest(MinimalPngEvidence()) with { CapturedAtUtc = Now.AddMinutes(-11) },
            Now);
        var future = issuer.Issue(
            CompleteRequest(MinimalPngEvidence()) with { CapturedAtUtc = Now.AddMinutes(2) },
            Now);
        var issued = issuer.Issue(CompleteRequest(MinimalPngEvidence()), Now);

        var expired = issuer.Consume(issued.TicketId!, Now.AddMinutes(11));

        stale.Status.Should().Be(OfficialUninstallVisualGateIssueStatus.Refused);
        future.Status.Should().Be(OfficialUninstallVisualGateIssueStatus.Refused);
        expired.Status.Should().Be(OfficialUninstallVisualGateConsumeStatus.Expired);
        expired.Receipt.Should().BeNull();
    }

    [Fact]
    public void Issuer_refuses_unknown_ticket_and_duplicate_ticket_ids()
    {
        var issuer = new OfficialUninstallVisualGateReceiptIssuer(
            ticketIdFactory: () => "duplicate-ticket");
        var first = issuer.Issue(CompleteRequest(MinimalPngEvidence()), Now);
        var duplicate = issuer.Issue(CompleteRequest(MinimalPngEvidence()), Now.AddSeconds(1));
        var unknown = issuer.Consume("missing-ticket", Now.AddSeconds(2));

        first.Status.Should().Be(OfficialUninstallVisualGateIssueStatus.Issued);
        duplicate.Status.Should().Be(OfficialUninstallVisualGateIssueStatus.Refused);
        unknown.Status.Should().Be(OfficialUninstallVisualGateConsumeStatus.UnknownTicket);
        unknown.Receipt.Should().BeNull();
    }

    [Fact]
    public void Issued_hash_is_not_changed_when_the_callers_png_buffer_is_mutated()
    {
        var issuer = new OfficialUninstallVisualGateReceiptIssuer(
            ticketIdFactory: () => "immutable-hash-ticket");
        var png = MinimalPngEvidence();
        var expectedHash = Convert.ToHexString(SHA256.HashData(png));

        var issued = issuer.Issue(CompleteRequest(png), Now);
        png[^1] ^= 0xFF;
        var consumed = issuer.Consume(issued.TicketId!, Now.AddSeconds(1));

        consumed.Status.Should().Be(OfficialUninstallVisualGateConsumeStatus.Consumed);
        consumed.Receipt!.ScreenshotSha256.Should().Be(expectedHash);
        consumed.Receipt.ScreenshotSha256.Should().NotBe(
            Convert.ToHexString(SHA256.HashData(png)));
    }

    [Fact]
    public void Issuer_source_is_in_core_in_memory_and_non_executable()
    {
        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.Core", "Uninstall", "OfficialUninstallVisualGateReceiptIssuer.cs"));
        var app = File.ReadAllText(FindRepositoryFile("src", "Css.App", "App.xaml.cs"));
        var program = File.ReadAllText(FindRepositoryFile("src", "Css.Elevated", "Program.cs"));

        source.Should().NotContain("File.Write");
        source.Should().NotContain("FileStream");
        source.Should().NotContain("Process.Start");
        source.Should().NotContain("OperationPipeline");
        source.Should().NotContain("ExecuteAsync");
        app.Should().Contain("OfficialUninstallVisualGateReceiptIssuer");
        app.Should().Contain("OfficialUninstallElevatedRequestSession");
        program.Should().NotContain("OfficialUninstallVisualGateReceiptIssuer");
    }

    private static OfficialUninstallVisualGateIssueRequest CompleteRequest(byte[] png) =>
        new()
        {
            UiContractVersion = OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
            ScreenshotPng = png,
            CapturedAtUtc = Now.AddSeconds(-2),
            RecoveryTruthVisible = true,
            FinalConfirmationVisible = true,
            TechnicalDetailsCollapsedByDefault = true,
            NoExecutionControlDuringPreparation = true
        };

    private static byte[] MinimalPngEvidence() =>
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52
    ];

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
