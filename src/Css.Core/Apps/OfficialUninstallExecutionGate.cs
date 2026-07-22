using System;
using System.Collections.Generic;
using Css.Core.Operations;
using Css.Core.Software;

namespace Css.Core.Apps;

public enum OfficialUninstallRecoveryMethod
{
    None,
    ReinstallSource,
    WindowsRestorePoint
}

public sealed class OfficialUninstallRecoveryEvidence
{
    public OfficialUninstallRecoveryMethod Method { get; init; }
    public string? Reference { get; init; }
    public bool CanRecoverApplication { get; init; }
    public bool UserDataBackupConfirmed { get; init; }
}

public static class OfficialUninstallRecoveryEvidenceValidator
{
    public static bool IsUsable(OfficialUninstallRecoveryEvidence? evidence) =>
        evidence is
        {
            Method: not OfficialUninstallRecoveryMethod.None,
            CanRecoverApplication: true
        }
        && !string.IsNullOrWhiteSpace(evidence.Reference);
}

public sealed class OfficialUninstallExecutionReadiness
{
    public bool FeatureEnabled { get; init; }
    public string? SnapshotId { get; init; }
    public OfficialUninstallSnapshotEvidence? SnapshotEvidence { get; init; }
    public bool UserConfirmedOfficialCommand { get; init; }
    public bool UserConfirmedAppsClosed { get; init; }
    public bool UserConfirmedPostUninstallRescan { get; init; }
    public bool UserAcknowledgedNoAutomaticUndo { get; init; }
    public OfficialUninstallRecoveryEvidence? RecoveryEvidence { get; init; }
}

public sealed class OfficialUninstallExecutionGateResult
{
    public required bool CanRequestExecution { get; init; }
    public required string PrimaryButtonText { get; init; }
    public required IReadOnlyList<string> BlockingReasons { get; init; }
    public required OfficialUninstallCommandTrustResult CommandTrust { get; init; }
    public OperationDescriptor? Operation { get; init; }
}

public static class OfficialUninstallExecutionGate
{
    public static OfficialUninstallExecutionGateResult Evaluate(
        SoftwareProfile profile,
        OfficialUninstallExecutionReadiness readiness,
        Func<string, bool> uninstallerExists,
        Func<string, string?>? snapshotHashResolver = null,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(readiness);
        ArgumentNullException.ThrowIfNull(uninstallerExists);

        if (!readiness.FeatureEnabled)
        {
            return Blocked(
                "等待恢复准备",
                ["这份方案尚未完成恢复证据和最终确认准备。"]);
        }

        var parsed = ParseCommand(profile.UninstallCommand);
        var commandTrust = OfficialUninstallCommandTrustResult.NotEvaluated();
        var blockers = new List<string>();
        var snapshotValidation = OfficialUninstallSnapshotEvidenceValidator.Validate(
            profile,
            readiness.SnapshotEvidence,
            snapshotHashResolver ?? (_ => null),
            now ?? DateTimeOffset.UtcNow);

        if (parsed is null)
        {
            blockers.Add("未发现官方卸载命令。");
        }
        else
        {
            commandTrust = OfficialUninstallCommandTrustEvaluator.Evaluate(
                parsed.Value.ExecutablePath,
                profile.InstallPath,
                parsed.Value.Arguments,
                profile.Publisher,
                profile.SignatureSubject);

            if (!commandTrust.IsTrusted)
                blockers.Add(commandTrust.Summary);

            if (!ExistsSafely(parsed.Value.ExecutablePath, uninstallerExists))
                blockers.Add("未找到官方卸载器文件。");
        }

        if (string.IsNullOrWhiteSpace(readiness.SnapshotId))
            blockers.Add("需要先创建卸载前快照。");

        if (!snapshotValidation.IsValid)
            blockers.AddRange(snapshotValidation.Reasons);

        if (snapshotValidation.IsValid
            && !string.Equals(
                readiness.SnapshotId,
                readiness.SnapshotEvidence!.SnapshotId,
                StringComparison.Ordinal))
        {
            blockers.Add("\u754c\u9762\u4e2d\u7684\u5feb\u7167\u7f16\u53f7\u4e0e\u5feb\u7167\u8bc1\u636e\u7f16\u53f7\u4e0d\u4e00\u81f4\u3002");
        }

        if (!readiness.UserConfirmedOfficialCommand)
            blockers.Add("需要先确认官方卸载命令。");

        if (!readiness.UserConfirmedAppsClosed)
            blockers.Add(BuildCloseAppsBlocker(profile));

        if (!readiness.UserConfirmedPostUninstallRescan)
            blockers.Add("需要确认卸载后会重扫残留。");

        if (!readiness.UserAcknowledgedNoAutomaticUndo)
            blockers.Add("需要确认：官方卸载本身不能一键恢复，后悔时通常需要重新安装。");

        if (!OfficialUninstallRecoveryEvidenceValidator.IsUsable(readiness.RecoveryEvidence))
            blockers.Add("需要准备可验证的恢复方式：系统还原点或可信的重新安装来源。");

        if (profile.DataPaths.Count > 0 && readiness.RecoveryEvidence?.UserDataBackupConfirmed != true)
            blockers.Add("检测到个人数据位置，需要先确认重要个人数据已经备份。");

        if (blockers.Count > 0 || parsed is null)
            return Blocked("前置检查未完成", blockers, commandTrust);

        return new OfficialUninstallExecutionGateResult
        {
            CanRequestExecution = true,
            PrimaryButtonText = "确认运行官方卸载器",
            BlockingReasons = [],
            CommandTrust = commandTrust,
            Operation = CreateOperation(
                profile,
                parsed.Value,
                readiness.SnapshotEvidence!,
                readiness.RecoveryEvidence!)
        };
    }

    private static OfficialUninstallExecutionGateResult Blocked(
        string buttonText,
        IReadOnlyList<string> reasons,
        OfficialUninstallCommandTrustResult? commandTrust = null) =>
        new()
        {
            CanRequestExecution = false,
            PrimaryButtonText = buttonText,
            BlockingReasons = reasons,
            CommandTrust = commandTrust ?? OfficialUninstallCommandTrustResult.NotEvaluated(),
            Operation = null
        };

    private static OperationDescriptor CreateOperation(
        SoftwareProfile profile,
        ParsedUninstallCommand command,
        OfficialUninstallSnapshotEvidence snapshotEvidence,
        OfficialUninstallRecoveryEvidence recoveryEvidence)
    {
        var affectedPaths = new List<string>();
        if (!string.IsNullOrWhiteSpace(profile.InstallPath))
            affectedPaths.Add(profile.InstallPath);

        return new OperationDescriptor
        {
            Kind = "uninstall.official.run",
            Title = profile.Name + " \u5b98\u65b9\u5378\u8f7d\u5668",
            Source = OperationSource.Manual,
            Risk = RiskLevel.High,
            IsDestructive = true,
            RequiresElevation = true,
            RequiresSnapshot = true,
            SnapshotId = snapshotEvidence.SnapshotId,
            RollbackRequired = true,
            ConfirmationAccepted = false,
            EvidenceSummary = "\u5df2\u786e\u8ba4\u5b98\u65b9\u5378\u8f7d\u547d\u4ee4\uff1b\u5fc5\u987b\u5148\u6709\u5feb\u7167\uff0c\u5378\u8f7d\u540e\u5fc5\u987b\u91cd\u626b\u6b8b\u7559\u3002",
            EstimatedImpactBytes = profile.InstalledSizeBytes,
            ConfirmationText = "\u8fd0\u884c " + profile.Name + " \u7684\u5b98\u65b9\u5378\u8f7d\u5668\uff1f",
            AffectedPaths = affectedPaths,
            AffectedServices = profile.Services,
            Arguments = new Dictionary<string, object?>
            {
                ["softwareName"] = profile.Name,
                ["executablePath"] = command.ExecutablePath,
                ["arguments"] = command.Arguments,
                ["snapshotManifestPath"] = snapshotEvidence.ManifestPath,
                ["snapshotSha256"] = snapshotEvidence.Sha256,
                ["snapshotCanRestoreApplication"] = snapshotEvidence.CanRestoreApplication,
                ["recoveryMethod"] = recoveryEvidence.Method.ToString(),
                ["recoveryReference"] = recoveryEvidence.Reference
            }
        };
    }

    private static string BuildCloseAppsBlocker(SoftwareProfile profile)
    {
        if (profile.RunningProcesses.Count == 0)
            return "需要确认软件和相关托盘窗口已经关闭。";

        return "需要先关闭或确认关闭运行中进程: " + string.Join(", ", profile.RunningProcesses);
    }

    private static bool ExistsSafely(string path, Func<string, bool> exists)
    {
        try
        {
            return exists(path);
        }
        catch
        {
            return false;
        }
    }

    private static ParsedUninstallCommand? ParseCommand(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return null;

        var trimmed = command.Trim();
        if (trimmed.StartsWith('"'))
        {
            var closing = trimmed.IndexOf('"', 1);
            if (closing > 1)
            {
                var executable = trimmed[1..closing];
                var args = trimmed[(closing + 1)..].Trim();
                return new ParsedUninstallCommand(executable, args);
            }
        }

        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace < 0
            ? new ParsedUninstallCommand(trimmed, string.Empty)
            : new ParsedUninstallCommand(trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }

    private readonly record struct ParsedUninstallCommand(string ExecutablePath, string Arguments);
}
