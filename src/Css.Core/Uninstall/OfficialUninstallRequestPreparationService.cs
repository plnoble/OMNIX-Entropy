using Css.Core.Apps;
using Css.Core.Software;

namespace Css.Core.Uninstall;

public static class OfficialUninstallRequestPreparationService
{
    public static OfficialUninstallElevatedRequestDraft Create(
        SoftwareProfile profile,
        OfficialUninstallSnapshotEvidence snapshotEvidence,
        OfficialUninstallRecoveryEvidence recoveryEvidence,
        OfficialUninstallFinalUserConsent consent,
        OfficialUninstallVisualGateReceiptIssuer visualGateIssuer,
        string visualTicketId,
        string requestId,
        DateTimeOffset now,
        Func<string, bool> uninstallerExists,
        Func<string, string?> snapshotHashResolver)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(snapshotEvidence);
        ArgumentNullException.ThrowIfNull(recoveryEvidence);
        ArgumentNullException.ThrowIfNull(consent);
        ArgumentNullException.ThrowIfNull(visualGateIssuer);
        ArgumentNullException.ThrowIfNull(uninstallerExists);
        ArgumentNullException.ThrowIfNull(snapshotHashResolver);

        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            new OfficialUninstallExecutionReadiness
            {
                FeatureEnabled = true,
                SnapshotId = snapshotEvidence.SnapshotId,
                SnapshotEvidence = snapshotEvidence,
                UserConfirmedOfficialCommand = consent.OfficialCommandConfirmed,
                UserConfirmedAppsClosed = consent.AppsClosedConfirmed,
                UserConfirmedPostUninstallRescan = consent.PostUninstallRescanConfirmed,
                UserAcknowledgedNoAutomaticUndo = consent.NoAutomaticUndoAcknowledged,
                RecoveryEvidence = recoveryEvidence
            },
            uninstallerExists,
            snapshotHashResolver,
            now);

        return new OfficialUninstallElevatedRequestSession(visualGateIssuer).Create(
            gate,
            visualTicketId,
            consent,
            requestId,
            now);
    }
}
