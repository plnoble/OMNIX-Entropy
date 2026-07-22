using System;
using System.Collections.Generic;
using Css.Core.Operations;
using Css.Core.Software;

namespace Css.Core.Apps;

public enum OfficialUninstallPreflightStepState
{
    Complete,
    Waiting,
    Blocked
}

public sealed class OfficialUninstallPreflightStepViewModel
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required OfficialUninstallPreflightStepState State { get; init; }
    public required string StatusLabel { get; init; }
    public required string Detail { get; init; }
}

public sealed class OfficialUninstallPreflightChecklistViewModel
{
    public required bool CanRequestExecution { get; init; }
    public required string PrimaryActionText { get; init; }
    public required string NextActionText { get; init; }
    public required OfficialUninstallExecutionGateResult ExecutionGate { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public required IReadOnlyList<OfficialUninstallPreflightStepViewModel> Steps { get; init; }
}

public static class OfficialUninstallPreflightChecklistBuilder
{
    public static OfficialUninstallPreflightChecklistViewModel Create(
        SoftwareProfile profile,
        OfficialUninstallExecutionReadiness readiness,
        Func<string, bool> uninstallerExists,
        Func<string, string?>? snapshotHashResolver = null,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(readiness);
        ArgumentNullException.ThrowIfNull(uninstallerExists);

        var effectiveNow = now ?? DateTimeOffset.UtcNow;
        var hashResolver = snapshotHashResolver ?? (_ => null);
        var gate = OfficialUninstallExecutionGate.Evaluate(
            profile,
            readiness,
            uninstallerExists,
            hashResolver,
            effectiveNow);
        var snapshotValidation = OfficialUninstallSnapshotEvidenceValidator.Validate(
            profile,
            readiness.SnapshotEvidence,
            hashResolver,
            effectiveNow);
        var snapshotReady = snapshotValidation.IsValid;
        var parsed = ParseCommand(profile.UninstallCommand);
        var uninstallerFileExists = parsed is not null && ExistsSafely(parsed.Value.ExecutablePath, uninstallerExists);
        var recoveryEvidenceReady = OfficialUninstallRecoveryEvidenceValidator.IsUsable(readiness.RecoveryEvidence);
        var userDataBackupReady = profile.DataPaths.Count == 0 || readiness.RecoveryEvidence?.UserDataBackupConfirmed == true;
        var steps = new List<OfficialUninstallPreflightStepViewModel>
        {
            Step(
                "feature-enabled",
                "\u5b89\u5168\u6267\u884c\u5165\u53e3",
                readiness.FeatureEnabled ? OfficialUninstallPreflightStepState.Complete : OfficialUninstallPreflightStepState.Blocked,
                readiness.FeatureEnabled ? "\u5df2\u5c31\u7eea" : "\u7b49\u5f85\u51c6\u5907",
                readiness.FeatureEnabled
                    ? "\u5df2\u5141\u8bb8\u8fdb\u5165\u5b98\u65b9\u5378\u8f7d\u8bf7\u6c42\u6d41\u7a0b\uff0c\u4f46\u4ecd\u9700\u6700\u540e\u786e\u8ba4\u3002"
                    : "\u8fd9\u4efd\u65b9\u6848\u5c1a\u672a\u5b8c\u6210\u5feb\u7167\u3001\u6062\u590d\u8bc1\u636e\u548c\u6700\u7ec8\u786e\u8ba4\u51c6\u5907\uff0c\u4e0d\u4f1a\u542f\u52a8\u5378\u8f7d\u5668\u3002"),
            Step(
                "command-trust",
                "\u547d\u4ee4\u53ef\u4fe1\u5ea6",
                gate.CommandTrust.IsTrusted ? OfficialUninstallPreflightStepState.Complete : OfficialUninstallPreflightStepState.Blocked,
                gate.CommandTrust.IsTrusted ? "\u53ef\u4fe1" : "\u5df2\u963b\u6b62",
                gate.CommandTrust.Summary),
            Step(
                "uninstaller-file",
                "\u5378\u8f7d\u5668\u6587\u4ef6",
                uninstallerFileExists ? OfficialUninstallPreflightStepState.Complete : OfficialUninstallPreflightStepState.Blocked,
                uninstallerFileExists ? "\u5df2\u627e\u5230" : "\u672a\u627e\u5230",
                uninstallerFileExists
                    ? "\u5df2\u627e\u5230\u547d\u4ee4\u91cc\u7684\u5378\u8f7d\u5668\u6587\u4ef6\u3002"
                    : "\u6ca1\u6709\u627e\u5230\u547d\u4ee4\u91cc\u7684\u5378\u8f7d\u5668\u6587\u4ef6\uff0c\u4e0d\u80fd\u7ee7\u7eed\u3002"),
            Step(
                "snapshot",
                "\u5378\u8f7d\u524d\u5feb\u7167",
                !snapshotReady
                    ? OfficialUninstallPreflightStepState.Waiting
                    : OfficialUninstallPreflightStepState.Complete,
                !snapshotReady ? "\u9700\u8981" : "\u5df2\u5c31\u7eea",
                !snapshotReady
                    ? snapshotValidation.Reasons.FirstOrDefault()
                        ?? "\u9700\u8981\u5148\u521b\u5efa\u5378\u8f7d\u524d\u8bc1\u636e\u5feb\u7167\u3002"
                    : "\u5df2\u9a8c\u8bc1\u5378\u8f7d\u524d\u8bc1\u636e\u5feb\u7167\uff1b\u5b83\u7528\u4e8e\u5ba1\u8ba1\u548c\u5378\u8f7d\u540e\u5bf9\u6bd4\uff0c\u4e0d\u80fd\u6062\u590d\u8f6f\u4ef6\u3002"),
            Step(
                "official-command-confirmation",
                "\u786e\u8ba4\u5378\u8f7d\u547d\u4ee4",
                readiness.UserConfirmedOfficialCommand
                    ? OfficialUninstallPreflightStepState.Complete
                    : OfficialUninstallPreflightStepState.Waiting,
                readiness.UserConfirmedOfficialCommand ? "\u5df2\u786e\u8ba4" : "\u9700\u8981",
                readiness.UserConfirmedOfficialCommand
                    ? "\u5df2\u786e\u8ba4\u8fc7\u5378\u8f7d\u547d\u4ee4\u548c\u53c2\u6570\u3002"
                    : "\u9700\u8981\u5148\u770b\u6e05\u5e76\u786e\u8ba4\u5378\u8f7d\u547d\u4ee4\u548c\u53c2\u6570\u3002"),
            Step(
                "no-automatic-undo",
                "确认不能一键恢复",
                readiness.UserAcknowledgedNoAutomaticUndo
                    ? OfficialUninstallPreflightStepState.Complete
                    : OfficialUninstallPreflightStepState.Waiting,
                readiness.UserAcknowledgedNoAutomaticUndo ? "已确认" : "需要",
                readiness.UserAcknowledgedNoAutomaticUndo
                    ? "已确认官方卸载本身不能由隔离区一键恢复。"
                    : "需要先确认：后悔时通常要重新安装，隔离区只能还原之后处理的低风险残留。"),
            Step(
                "recovery-evidence",
                "恢复方式",
                recoveryEvidenceReady
                    ? OfficialUninstallPreflightStepState.Complete
                    : OfficialUninstallPreflightStepState.Waiting,
                recoveryEvidenceReady ? "已准备" : "需要",
                recoveryEvidenceReady
                    ? DescribeRecoveryEvidence(readiness.RecoveryEvidence!)
                    : "需要准备系统还原点或可信的重新安装来源，不能只填写一个快照编号。"),
            Step(
                "user-data-backup",
                "个人数据备份",
                userDataBackupReady
                    ? OfficialUninstallPreflightStepState.Complete
                    : OfficialUninstallPreflightStepState.Waiting,
                userDataBackupReady ? "已确认" : "需要",
                profile.DataPaths.Count == 0
                    ? "当前画像没有发现明确的个人数据位置；卸载前仍建议确认重要文件。"
                    : userDataBackupReady
                        ? "已确认重要个人数据已经备份。"
                        : "检测到个人数据位置，需要先确认重要内容已经备份。"),
            Step(
                "close-apps",
                "\u5173\u95ed\u5e94\u7528",
                readiness.UserConfirmedAppsClosed
                    ? OfficialUninstallPreflightStepState.Complete
                    : OfficialUninstallPreflightStepState.Waiting,
                readiness.UserConfirmedAppsClosed ? "\u5df2\u786e\u8ba4" : "\u9700\u8981",
                readiness.UserConfirmedAppsClosed
                    ? "\u5df2\u786e\u8ba4\u8f6f\u4ef6\u548c\u76f8\u5173\u6258\u76d8\u7a97\u53e3\u5df2\u5173\u95ed\u3002"
                    : BuildCloseAppsDetail(profile)),
            Step(
                "post-uninstall-rescan",
                "\u5378\u8f7d\u540e\u91cd\u626b",
                readiness.UserConfirmedPostUninstallRescan
                    ? OfficialUninstallPreflightStepState.Complete
                    : OfficialUninstallPreflightStepState.Waiting,
                readiness.UserConfirmedPostUninstallRescan ? "\u5df2\u786e\u8ba4" : "\u9700\u8981",
                readiness.UserConfirmedPostUninstallRescan
                    ? "\u5df2\u786e\u8ba4 OMNIX-Entropy \u4f1a\u5728\u5378\u8f7d\u540e\u91cd\u65b0\u626b\u63cf\u6b8b\u7559\u3002"
                    : "\u9700\u8981\u786e\u8ba4\u5378\u8f7d\u540e\u5fc5\u987b\u91cd\u65b0\u626b\u63cf\u6b8b\u7559\uff0c\u518d\u51b3\u5b9a\u80fd\u5426\u6e05\u7406\u3002")
        };

        return new OfficialUninstallPreflightChecklistViewModel
        {
            CanRequestExecution = gate.CanRequestExecution,
            PrimaryActionText = BuildPrimaryActionText(readiness, gate),
            NextActionText = BuildNextActionText(steps, gate),
            ExecutionGate = gate,
            Operation = gate.Operation,
            Steps = steps
        };
    }

    private static OfficialUninstallPreflightStepViewModel Step(
        string key,
        string title,
        OfficialUninstallPreflightStepState state,
        string statusLabel,
        string detail) =>
        new()
        {
            Key = key,
            Title = title,
            State = state,
            StatusLabel = statusLabel,
            Detail = detail
        };

    private static string BuildPrimaryActionText(
        OfficialUninstallExecutionReadiness readiness,
        OfficialUninstallExecutionGateResult gate)
    {
        if (!readiness.FeatureEnabled)
            return "\u5148\u5b8c\u6210\u6062\u590d\u51c6\u5907";

        return gate.CanRequestExecution
            ? "\u8bf7\u6c42\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\u5668"
            : "\u5b8c\u6210\u5378\u8f7d\u524d\u68c0\u67e5";
    }

    private static string BuildNextActionText(
        IReadOnlyList<OfficialUninstallPreflightStepViewModel> steps,
        OfficialUninstallExecutionGateResult gate)
    {
        if (gate.CanRequestExecution)
            return "\u6240\u6709\u5378\u8f7d\u524d\u68c0\u67e5\u90fd\u901a\u8fc7\u4e86\u3002\u4e0b\u4e00\u6b65\u4ecd\u7136\u5fc5\u987b\u5f39\u51fa\u6700\u7ec8\u786e\u8ba4\u3002";

        foreach (var step in steps)
        {
            if (step.State != OfficialUninstallPreflightStepState.Complete)
                return step.Detail;
        }

        return "\u4ecd\u88ab\u5b89\u5168\u95e8\u963b\u6b62\uff1a" + string.Join(" ", gate.BlockingReasons);
    }

    private static string BuildCloseAppsDetail(SoftwareProfile profile)
    {
        if (profile.RunningProcesses.Count == 0)
            return "\u8bf7\u786e\u8ba4\u8f6f\u4ef6\u548c\u76f8\u5173\u6258\u76d8\u7a97\u53e3\u5df2\u7ecf\u5173\u95ed\u3002";

        return "\u8bf7\u5148\u5173\u95ed\u6216\u786e\u8ba4\u8fd9\u4e9b\u8fd0\u884c\u4e2d\u7684\u8fdb\u7a0b\uff1a" + string.Join(", ", profile.RunningProcesses);
    }

    private static string DescribeRecoveryEvidence(OfficialUninstallRecoveryEvidence evidence) =>
        evidence.Method switch
        {
            OfficialUninstallRecoveryMethod.WindowsRestorePoint => "已记录可用的 Windows 系统还原点。",
            OfficialUninstallRecoveryMethod.ReinstallSource => "已记录可信的重新安装来源。",
            _ => "恢复方式尚未准备好。"
        };

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
