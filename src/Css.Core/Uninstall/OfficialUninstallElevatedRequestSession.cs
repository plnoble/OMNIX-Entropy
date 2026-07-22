using Css.Core.Apps;

namespace Css.Core.Uninstall;

public sealed class OfficialUninstallElevatedRequestSession
{
    private readonly OfficialUninstallVisualGateReceiptIssuer _visualGateIssuer;

    public OfficialUninstallElevatedRequestSession(
        OfficialUninstallVisualGateReceiptIssuer visualGateIssuer)
    {
        _visualGateIssuer = visualGateIssuer
            ?? throw new ArgumentNullException(nameof(visualGateIssuer));
    }

    public OfficialUninstallElevatedRequestDraft Create(
        OfficialUninstallExecutionGateResult gate,
        string visualTicketId,
        OfficialUninstallFinalUserConsent finalConsent,
        string requestId,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(gate);
        ArgumentNullException.ThrowIfNull(finalConsent);

        var consumed = _visualGateIssuer.Consume(visualTicketId, now);
        if (consumed.Status != OfficialUninstallVisualGateConsumeStatus.Consumed
            || consumed.Receipt is null)
        {
            return Refused(consumed.Status);
        }

        return OfficialUninstallElevatedRequestComposer.Create(
            gate,
            consumed.Receipt,
            finalConsent,
            requestId,
            now);
    }

    private static OfficialUninstallElevatedRequestDraft Refused(
        OfficialUninstallVisualGateConsumeStatus status) =>
        new()
        {
            Status = OfficialUninstallElevatedRequestStatus.Refused,
            MissingRequirements =
            [
                status switch
                {
                    OfficialUninstallVisualGateConsumeStatus.AlreadyConsumed =>
                        "\u672c\u6b21\u754c\u9762\u786e\u8ba4\u5df2\u4f7f\u7528\uff0c\u4e0d\u80fd\u91cd\u590d\u63d0\u4ea4\u3002",
                    OfficialUninstallVisualGateConsumeStatus.Expired =>
                        "\u672c\u6b21\u754c\u9762\u786e\u8ba4\u5df2\u8fc7\u671f\uff0c\u8bf7\u91cd\u65b0\u68c0\u67e5\u3002",
                    OfficialUninstallVisualGateConsumeStatus.InvalidTime =>
                        "\u672c\u6b21\u754c\u9762\u786e\u8ba4\u65f6\u95f4\u65e0\u6548\uff0c\u5df2\u62d2\u7edd\u3002",
                    _ => "\u627e\u4e0d\u5230\u672c\u6b21\u754c\u9762\u786e\u8ba4\u51ed\u8bc1\u3002"
                }
            ]
        };
}
