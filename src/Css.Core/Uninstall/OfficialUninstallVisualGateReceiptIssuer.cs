using System.Security.Cryptography;

namespace Css.Core.Uninstall;

public sealed record OfficialUninstallVisualGateIssueRequest
{
    public required string UiContractVersion { get; init; }
    public required byte[] ScreenshotPng { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public bool RecoveryTruthVisible { get; init; }
    public bool FinalConfirmationVisible { get; init; }
    public bool TechnicalDetailsCollapsedByDefault { get; init; }
    public bool NoExecutionControlDuringPreparation { get; init; }
}

public enum OfficialUninstallVisualGateIssueStatus
{
    Refused,
    Issued
}

public sealed class OfficialUninstallVisualGateIssueResult
{
    public required OfficialUninstallVisualGateIssueStatus Status { get; init; }
    public required IReadOnlyList<string> MissingRequirements { get; init; }
    public string? TicketId { get; init; }
}

public enum OfficialUninstallVisualGateConsumeStatus
{
    Consumed,
    AlreadyConsumed,
    Expired,
    UnknownTicket,
    InvalidTime
}

public sealed class OfficialUninstallVisualGateConsumeResult
{
    public required OfficialUninstallVisualGateConsumeStatus Status { get; init; }
    public OfficialUninstallVisualGateReceipt? Receipt { get; init; }
}

public sealed class OfficialUninstallVisualGateReceiptIssuer
{
    private const int MaximumScreenshotBytes = 20 * 1024 * 1024;
    private const int MaximumOutstandingTickets = 32;
    private static readonly TimeSpan MaximumCaptureAge = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MaximumClockSkew = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan TicketLifetime = TimeSpan.FromMinutes(10);
    private static readonly byte[] PngSignature =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly object _sync = new();
    private readonly Dictionary<string, TicketEntry> _tickets = new(StringComparer.Ordinal);
    private readonly Func<string> _ticketIdFactory;

    public OfficialUninstallVisualGateReceiptIssuer(Func<string>? ticketIdFactory = null)
    {
        _ticketIdFactory = ticketIdFactory ?? (() => Guid.NewGuid().ToString("N"));
    }

    public int OutstandingTicketCount
    {
        get
        {
            lock (_sync)
                return _tickets.Values.Count(ticket => !ticket.Consumed && !ticket.Expired);
        }
    }

    public OfficialUninstallVisualGateIssueResult Issue(
        OfficialUninstallVisualGateIssueRequest request,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(request);

        var missing = Validate(request, now);
        if (missing.Count > 0)
            return Refused(missing);

        var ticketId = _ticketIdFactory();
        if (string.IsNullOrWhiteSpace(ticketId) || ticketId.Length > 128)
            return Refused(["\u89c6\u89c9\u9a8c\u8bc1\u51ed\u8bc1\u7f16\u53f7\u65e0\u6548\u3002"]);

        var receipt = new OfficialUninstallVisualGateReceipt
        {
            UiContractVersion = request.UiContractVersion,
            ScreenshotSha256 = Convert.ToHexString(SHA256.HashData(request.ScreenshotPng)),
            CapturedAtUtc = request.CapturedAtUtc.ToUniversalTime(),
            RecoveryTruthVisible = request.RecoveryTruthVisible,
            FinalConfirmationVisible = request.FinalConfirmationVisible,
            TechnicalDetailsCollapsedByDefault = request.TechnicalDetailsCollapsedByDefault,
            NoExecutionControlDuringPreparation = request.NoExecutionControlDuringPreparation
        };

        lock (_sync)
        {
            if (_tickets.Count >= MaximumOutstandingTickets)
                return Refused(["\u5f53\u524d\u7b49\u5f85\u786e\u8ba4\u7684\u51ed\u8bc1\u8fc7\u591a\uff0c\u8bf7\u5148\u53d6\u6d88\u65e7\u65b9\u6848\u3002"]);
            if (_tickets.ContainsKey(ticketId))
                return Refused(["\u89c6\u89c9\u9a8c\u8bc1\u51ed\u8bc1\u7f16\u53f7\u91cd\u590d\uff0c\u5df2\u62d2\u7edd\u3002"]);

            _tickets.Add(ticketId, new TicketEntry
            {
                Receipt = receipt,
                IssuedAtUtc = now.ToUniversalTime(),
                ExpiresAtUtc = now.ToUniversalTime().Add(TicketLifetime)
            });
        }

        return new OfficialUninstallVisualGateIssueResult
        {
            Status = OfficialUninstallVisualGateIssueStatus.Issued,
            MissingRequirements = [],
            TicketId = ticketId
        };
    }

    public OfficialUninstallVisualGateConsumeResult Consume(
        string ticketId,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
            return ConsumeResult(OfficialUninstallVisualGateConsumeStatus.UnknownTicket);

        lock (_sync)
        {
            if (!_tickets.TryGetValue(ticketId, out var ticket))
                return ConsumeResult(OfficialUninstallVisualGateConsumeStatus.UnknownTicket);

            var utcNow = now.ToUniversalTime();
            if (utcNow < ticket.IssuedAtUtc.Subtract(MaximumClockSkew))
                return ConsumeResult(OfficialUninstallVisualGateConsumeStatus.InvalidTime);
            if (ticket.Expired || utcNow > ticket.ExpiresAtUtc)
            {
                ticket.Expired = true;
                return ConsumeResult(OfficialUninstallVisualGateConsumeStatus.Expired);
            }
            if (ticket.Consumed)
                return ConsumeResult(OfficialUninstallVisualGateConsumeStatus.AlreadyConsumed);

            ticket.Consumed = true;
            return new OfficialUninstallVisualGateConsumeResult
            {
                Status = OfficialUninstallVisualGateConsumeStatus.Consumed,
                Receipt = ticket.Receipt
            };
        }
    }

    private static List<string> Validate(
        OfficialUninstallVisualGateIssueRequest request,
        DateTimeOffset now)
    {
        var missing = new List<string>();
        if (!string.Equals(
                request.UiContractVersion,
                OfficialUninstallElevatedRequestComposer.RequiredUiContractVersion,
                StringComparison.Ordinal))
        {
            missing.Add("\u754c\u9762\u5b89\u5168\u5951\u7ea6\u7248\u672c\u4e0d\u5339\u914d\u3002");
        }

        if (!IsPng(request.ScreenshotPng))
            missing.Add("\u7f3a\u5c11\u53ef\u9a8c\u8bc1\u7684 PNG \u754c\u9762\u8bc1\u636e\u3002");
        if (request.ScreenshotPng.Length > MaximumScreenshotBytes)
            missing.Add("\u754c\u9762\u8bc1\u636e\u8d85\u8fc7\u5141\u8bb8\u5927\u5c0f\u3002");
        if (!request.RecoveryTruthVisible
            || !request.FinalConfirmationVisible
            || !request.TechnicalDetailsCollapsedByDefault
            || !request.NoExecutionControlDuringPreparation)
        {
            missing.Add("\u6062\u590d\u771f\u76f8\u3001\u6700\u7ec8\u786e\u8ba4\u6216\u5b89\u5168\u9ed8\u8ba4\u72b6\u6001\u6ca1\u6709\u5168\u90e8\u53ef\u89c1\u3002");
        }

        var age = now.ToUniversalTime() - request.CapturedAtUtc.ToUniversalTime();
        if (age > MaximumCaptureAge || age < -MaximumClockSkew)
            missing.Add("\u754c\u9762\u8bc1\u636e\u5df2\u8fc7\u671f\u6216\u65f6\u95f4\u65e0\u6548\u3002");

        return missing;
    }

    private static bool IsPng(byte[] screenshot) =>
        screenshot.Length >= PngSignature.Length
        && screenshot.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature);

    private static OfficialUninstallVisualGateIssueResult Refused(
        IReadOnlyList<string> missing) =>
        new()
        {
            Status = OfficialUninstallVisualGateIssueStatus.Refused,
            MissingRequirements = missing
        };

    private static OfficialUninstallVisualGateConsumeResult ConsumeResult(
        OfficialUninstallVisualGateConsumeStatus status) =>
        new() { Status = status };

    private sealed class TicketEntry
    {
        public required OfficialUninstallVisualGateReceipt Receipt { get; init; }
        public required DateTimeOffset IssuedAtUtc { get; init; }
        public required DateTimeOffset ExpiresAtUtc { get; init; }
        public bool Consumed { get; set; }
        public bool Expired { get; set; }
    }
}
