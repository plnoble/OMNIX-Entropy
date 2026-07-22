using System;
using System.Collections.Generic;
using Css.Core.Recovery;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class UninstallRecoveryPreparationViewModel
{
    public required ReinstallSourceReadinessViewModel ReinstallReadiness { get; init; }
    public required string RestorePointStatus { get; init; }
    public required bool HasRestorePointHint { get; init; }
    public required bool RequiresPersonalDataBackup { get; init; }
    public required bool PersonalDataBackupAcknowledged { get; init; }
    public required string BackupStatus { get; init; }
    public required bool IsPreparationComplete { get; init; }
    public required string Summary { get; init; }
    public required string SafetyBoundary { get; init; }
    public required bool CanRequestExecution { get; init; }
}

public static class UninstallRecoveryPreparationPresenter
{
    public static UninstallRecoveryPreparationViewModel Create(
        SoftwareProfile profile,
        ReinstallSourceReadinessViewModel reinstallReadiness,
        IReadOnlyList<WindowsRestorePointInfo> restorePoints,
        bool personalDataBackupAcknowledged,
        WindowsRestorePointScanState restorePointScanState = WindowsRestorePointScanState.Completed)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(reinstallReadiness);
        ArgumentNullException.ThrowIfNull(restorePoints);

        var requiresBackup = profile.DataPaths.Count > 0;
        var backupReady = !requiresBackup || personalDataBackupAcknowledged;
        var reinstallReady = reinstallReadiness.CanUseAsRecoveryEvidence;
        var complete = reinstallReady && backupReady;
        var hasRestorePoint = restorePoints.Count > 0;

        return new UninstallRecoveryPreparationViewModel
        {
            ReinstallReadiness = reinstallReadiness,
            RestorePointStatus = BuildRestorePointStatus(
                restorePoints.Count,
                restorePointScanState),
            HasRestorePointHint = hasRestorePoint,
            RequiresPersonalDataBackup = requiresBackup,
            PersonalDataBackupAcknowledged = personalDataBackupAcknowledged,
            BackupStatus = requiresBackup
                ? personalDataBackupAcknowledged
                    ? "\u4e2a\u4eba\u6570\u636e\uff1a\u5df2\u786e\u8ba4\u91cd\u8981\u6587\u4ef6\u7531\u4f60\u53e6\u884c\u5907\u4efd\u3002"
                    : "\u4e2a\u4eba\u6570\u636e\uff1a\u672a\u786e\u8ba4\u5907\u4efd\uff0c\u8fd9\u4e00\u9879\u4e0d\u80fd\u7531\u8f6f\u4ef6\u66ff\u4f60\u5047\u8bbe\u5b8c\u6210\u3002"
                : "\u4e2a\u4eba\u6570\u636e\uff1a\u672a\u8bc6\u522b\u5230\u660e\u786e\u6570\u636e\u4f4d\u7f6e\uff0c\u6267\u884c\u524d\u4ecd\u5efa\u8bae\u786e\u8ba4\u91cd\u8981\u6587\u4ef6\u3002",
            IsPreparationComplete = complete,
            Summary = complete
                ? "\u6062\u590d\u51c6\u5907\u5df2\u9f50\uff1a\u5b89\u88c5\u5305\u5df2\u9a8c\u8bc1\uff0c\u4e2a\u4eba\u6570\u636e\u5907\u4efd\u5df2\u786e\u8ba4\u3002\u4e0b\u4e00\u6b65\u4ecd\u9700\u5355\u72ec\u7684\u5378\u8f7d\u786e\u8ba4\u3002"
                : BuildMissingSummary(reinstallReady, backupReady),
            SafetyBoundary = "\u8fd9\u4e9b\u6b65\u9aa4\u53ea\u68c0\u67e5\u6062\u590d\u51c6\u5907\uff1a\u4e0d\u4f1a\u8fd0\u884c\u5b89\u88c5\u5305\uff0c\u4e0d\u4f1a\u8fd0\u884c\u5378\u8f7d\u5668\uff0c\u4e5f\u4e0d\u4f1a\u521b\u5efa\u6216\u4f7f\u7528\u8fd8\u539f\u70b9\u3002",
            CanRequestExecution = false
        };
    }

    private static string BuildRestorePointStatus(
        int restorePointCount,
        WindowsRestorePointScanState scanState) =>
        scanState switch
        {
            WindowsRestorePointScanState.TimedOut =>
                "\u7cfb\u7edf\u8fd8\u539f\u70b9\uff1a\u8bfb\u53d6\u8d85\u65f6\uff0c\u6682\u65f6\u4e0d\u80fd\u786e\u8ba4\u662f\u5426\u5b58\u5728\uff1b\u8fd9\u4e0d\u4f1a\u963b\u6b62\u67e5\u770b\u5b89\u5168\u65b9\u6848\u3002",
            WindowsRestorePointScanState.Failed =>
                "\u7cfb\u7edf\u8fd8\u539f\u70b9\uff1a\u672c\u6b21\u65e0\u6cd5\u8bfb\u53d6\uff0c\u6682\u65f6\u4e0d\u80fd\u786e\u8ba4\u662f\u5426\u5b58\u5728\u3002",
            _ when restorePointCount > 0 =>
                $"\u7cfb\u7edf\u8fd8\u539f\u70b9\uff1a\u53d1\u73b0 {restorePointCount} \u4e2a\u540e\u5907\u7ebf\u7d22\uff0c\u4f46\u4e0d\u80fd\u66ff\u4ee3\u5b98\u65b9\u5b89\u88c5\u5305\u6216\u4e2a\u4eba\u6587\u4ef6\u5907\u4efd\u3002",
            _ => "\u7cfb\u7edf\u8fd8\u539f\u70b9\uff1a\u672a\u53d1\u73b0\u53ef\u7528\u7684\u540e\u5907\u7ebf\u7d22\u3002"
        };

    private static string BuildMissingSummary(bool reinstallReady, bool backupReady)
    {
        if (!reinstallReady && !backupReady)
            return "\u6062\u590d\u51c6\u5907\u8fd8\u5dee\u4e24\u9879\uff1a\u53ef\u9a8c\u8bc1\u7684\u5b98\u65b9\u5b89\u88c5\u5305\uff0c\u4ee5\u53ca\u4e2a\u4eba\u6570\u636e\u5907\u4efd\u786e\u8ba4\u3002";
        if (!reinstallReady)
            return "\u6062\u590d\u51c6\u5907\u8fd8\u5dee\u4e00\u9879\uff1a\u53ef\u9a8c\u8bc1\u7684\u5b98\u65b9\u5b89\u88c5\u5305\u3002";
        return "\u6062\u590d\u51c6\u5907\u8fd8\u5dee\u4e00\u9879\uff1a\u4e2a\u4eba\u6570\u636e\u5907\u4efd\u786e\u8ba4\u3002";
    }
}

public sealed class UninstallRecoveryPreparationSession
{
    private readonly SoftwareProfile _profile;
    private readonly IReadOnlyList<WindowsRestorePointInfo> _restorePoints;
    private readonly WindowsRestorePointScanState _restorePointScanState;
    private ReinstallSourceReadinessViewModel _reinstallReadiness;
    private bool _personalDataBackupAcknowledged;

    public UninstallRecoveryPreparationSession(
        SoftwareProfile profile,
        ReinstallSourceReadinessViewModel initialReinstallReadiness,
        IReadOnlyList<WindowsRestorePointInfo> restorePoints,
        WindowsRestorePointScanState restorePointScanState = WindowsRestorePointScanState.Completed)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(initialReinstallReadiness);
        ArgumentNullException.ThrowIfNull(restorePoints);
        _profile = profile;
        _reinstallReadiness = initialReinstallReadiness;
        _restorePoints = restorePoints;
        _restorePointScanState = restorePointScanState;
        Current = BuildCurrent();
    }

    public UninstallRecoveryPreparationViewModel Current { get; private set; }

    public void SelectOfficialInstaller(
        string selectedPath,
        Func<string, bool> fileExists,
        Func<string, string?> signatureResolver)
    {
        _reinstallReadiness = ReinstallSourceReadinessPresenter.CreateForSelectedInstaller(
            _profile,
            selectedPath,
            fileExists,
            signatureResolver);
        Current = BuildCurrent();
    }

    public void SetPersonalDataBackupAcknowledged(bool acknowledged)
    {
        _personalDataBackupAcknowledged = acknowledged;
        Current = BuildCurrent();
    }

    private UninstallRecoveryPreparationViewModel BuildCurrent() =>
        UninstallRecoveryPreparationPresenter.Create(
            _profile,
            _reinstallReadiness,
            _restorePoints,
            _personalDataBackupAcknowledged,
            _restorePointScanState);
}
