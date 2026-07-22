using Css.Core.Apps;
using Css.Core.Software;

namespace Css.Snapshot.Uninstall;

public enum UninstallFinalConfirmationDraftStatus
{
    Refused,
    SnapshotVerificationFailed,
    ReadyForFinalConfirmation
}

public sealed class UninstallFinalConfirmationDraft
{
    public required UninstallFinalConfirmationDraftStatus Status { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> ReadyFacts { get; init; }
    public required IReadOnlyList<string> PendingConfirmations { get; init; }
    public required IReadOnlyList<string> MissingRequirements { get; init; }
    public required bool CanExecuteDirectly { get; init; }
    public OfficialUninstallRecoveryEvidence? RecoveryEvidence { get; init; }
    public OfficialUninstallSnapshotEvidence? SnapshotEvidence { get; init; }
    public OfficialUninstallSnapshotValidationResult? SnapshotValidation { get; init; }
}

public sealed class UninstallFinalConfirmationDraftService
{
    private readonly UninstallEvidenceSnapshotStore _snapshotStore;

    public UninstallFinalConfirmationDraftService(UninstallEvidenceSnapshotStore snapshotStore)
    {
        ArgumentNullException.ThrowIfNull(snapshotStore);
        _snapshotStore = snapshotStore;
    }

    public async Task<UninstallFinalConfirmationDraft> CreateAsync(
        SoftwareProfile profile,
        UninstallRecoveryPreparationViewModel preparation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(preparation);

        if (!preparation.IsPreparationComplete
            || preparation.ReinstallReadiness.RecoveryEvidence is null)
        {
            return Refused(BuildMissingRequirements(preparation));
        }

        var sourceRecovery = preparation.ReinstallReadiness.RecoveryEvidence;
        var recoveryEvidence = new OfficialUninstallRecoveryEvidence
        {
            Method = sourceRecovery.Method,
            Reference = sourceRecovery.Reference,
            CanRecoverApplication = sourceRecovery.CanRecoverApplication,
            UserDataBackupConfirmed = preparation.PersonalDataBackupAcknowledged
        };
        var snapshotEvidence = await _snapshotStore.CreateAsync(
            profile,
            recoveryEvidence,
            cancellationToken);
        var snapshotValidation = await _snapshotStore.VerifyAsync(
            snapshotEvidence,
            profile,
            cancellationToken);

        if (!snapshotValidation.IsValid)
        {
            return new UninstallFinalConfirmationDraft
            {
                Status = UninstallFinalConfirmationDraftStatus.SnapshotVerificationFailed,
                Headline = "\u5378\u8f7d\u524d\u8bc1\u636e\u5feb\u7167\u9a8c\u8bc1\u5931\u8d25",
                Summary = "Agent \u5df2\u505c\u6b62\u751f\u6210\u6700\u7ec8\u786e\u8ba4\u8349\u7a3f\uff0c\u4e0d\u4f1a\u7ee7\u7eed\u3002",
                ReadyFacts = [],
                PendingConfirmations = [],
                MissingRequirements = snapshotValidation.Reasons,
                CanExecuteDirectly = false,
                RecoveryEvidence = recoveryEvidence,
                SnapshotEvidence = snapshotEvidence,
                SnapshotValidation = snapshotValidation
            };
        }

        var readyFacts = new List<string>
        {
            "\u5df2\u9a8c\u8bc1\u53ef\u7528\u4e8e\u91cd\u65b0\u5b89\u88c5\u7684\u5b98\u65b9\u5b89\u88c5\u5305\u3002",
            preparation.RequiresPersonalDataBackup
                ? "\u5df2\u786e\u8ba4\u91cd\u8981\u4e2a\u4eba\u6570\u636e\u7531\u7528\u6237\u53e6\u884c\u5907\u4efd\u3002"
                : "\u672a\u8bc6\u522b\u5230\u660e\u786e\u4e2a\u4eba\u6570\u636e\u4f4d\u7f6e\uff0c\u6267\u884c\u524d\u4ecd\u5efa\u8bae\u590d\u67e5\u3002",
            "\u5df2\u521b\u5efa\u5e76\u9a8c\u8bc1\u5378\u8f7d\u524d\u8bc1\u636e\u5feb\u7167\uff1b\u5b83\u7528\u4e8e\u5ba1\u8ba1\u548c\u5bf9\u6bd4\uff0c\u4e0d\u80fd\u6062\u590d\u8f6f\u4ef6\u3002"
        };

        return new UninstallFinalConfirmationDraft
        {
            Status = UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation,
            Headline = "\u6062\u590d\u548c\u5feb\u7167\u8bc1\u636e\u5df2\u51c6\u5907\uff0c\u7b49\u5f85\u6700\u7ec8\u786e\u8ba4",
            Summary = "\u8fd9\u53ea\u662f\u6700\u7ec8\u786e\u8ba4\u8349\u7a3f\uff1b\u5c1a\u672a\u8fd0\u884c\u5b89\u88c5\u5305\u6216\u5378\u8f7d\u5668\u3002",
            ReadyFacts = readyFacts,
            PendingConfirmations =
            [
                "\u786e\u8ba4\u5173\u95ed\u8f6f\u4ef6\u548c\u76f8\u5173\u6258\u76d8\u7a97\u53e3\u3002",
                "\u786e\u8ba4\u5df2\u9605\u8bfb\u5b98\u65b9\u5378\u8f7d\u547d\u4ee4\u4e0e\u53c2\u6570\u3002",
                "\u786e\u8ba4\u5b98\u65b9\u5378\u8f7d\u672c\u8eab\u4e0d\u80fd\u4e00\u952e\u6062\u590d\u3002",
                "\u786e\u8ba4\u5378\u8f7d\u540e\u91cd\u65b0\u626b\u63cf\u8f6f\u4ef6\u753b\u50cf\u548c\u6b8b\u7559\u3002",
                "\u6700\u7ec8\u786e\u8ba4\u662f\u5426\u8bf7\u6c42\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\u5668\u3002"
            ],
            MissingRequirements = [],
            CanExecuteDirectly = false,
            RecoveryEvidence = recoveryEvidence,
            SnapshotEvidence = snapshotEvidence,
            SnapshotValidation = snapshotValidation
        };
    }

    private static UninstallFinalConfirmationDraft Refused(IReadOnlyList<string> missingRequirements) =>
        new()
        {
            Status = UninstallFinalConfirmationDraftStatus.Refused,
            Headline = "\u6682\u65f6\u4e0d\u80fd\u751f\u6210\u6700\u7ec8\u786e\u8ba4\u8349\u7a3f",
            Summary = "Agent \u4e0d\u4f1a\u5728\u6062\u590d\u51c6\u5907\u4e0d\u5b8c\u6574\u65f6\u521b\u5efa\u5feb\u7167\u6216\u7ee7\u7eed\u3002",
            ReadyFacts = [],
            PendingConfirmations = [],
            MissingRequirements = missingRequirements,
            CanExecuteDirectly = false
        };

    private static IReadOnlyList<string> BuildMissingRequirements(
        UninstallRecoveryPreparationViewModel preparation)
    {
        var missing = new List<string>();
        if (!preparation.ReinstallReadiness.CanUseAsRecoveryEvidence)
            missing.Add("\u8fd8\u7f3a\u5c11\u7b7e\u540d\u5339\u914d\u7684\u5b98\u65b9\u5b89\u88c5\u5305\u3002");
        if (preparation.RequiresPersonalDataBackup
            && !preparation.PersonalDataBackupAcknowledged)
        {
            missing.Add("\u8fd8\u7f3a\u5c11\u91cd\u8981\u4e2a\u4eba\u6570\u636e\u5907\u4efd\u786e\u8ba4\u3002");
        }

        if (missing.Count == 0)
            missing.Add("\u6062\u590d\u51c6\u5907\u72b6\u6001\u4e0d\u5b8c\u6574\uff0c\u9700\u8981\u91cd\u65b0\u68c0\u67e5\u3002");
        return missing;
    }
}
