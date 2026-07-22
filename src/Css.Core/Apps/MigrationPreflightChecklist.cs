using System;
using System.Collections.Generic;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Software;

namespace Css.Core.Apps;

public enum MigrationPreflightStepState
{
    Complete,
    Waiting,
    Blocked
}

public sealed class MigrationPreflightStepViewModel
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required MigrationPreflightStepState State { get; init; }
    public required string StatusLabel { get; init; }
    public required string Detail { get; init; }
}

public sealed class MigrationPreflightChecklistViewModel
{
    public required bool CanRequestExecution { get; init; }
    public required string PrimaryActionText { get; init; }
    public required string NextActionText { get; init; }
    public required MigrationExecutionGateResult ExecutionGate { get; init; }
    public OperationDescriptor? Operation { get; init; }
    public required IReadOnlyList<MigrationPreflightStepViewModel> Steps { get; init; }
}

public static class MigrationPreflightChecklistBuilder
{
    public static MigrationPreflightChecklistViewModel Create(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationExecutionReadiness readiness,
        Func<string, bool> rollbackManifestExists)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(readiness);
        ArgumentNullException.ThrowIfNull(rollbackManifestExists);

        var gate = MigrationExecutionGate.Evaluate(profile, plan, readiness, rollbackManifestExists);
        var manifestExists = !string.IsNullOrWhiteSpace(readiness.RollbackManifestPath)
            && ExistsSafely(readiness.RollbackManifestPath, rollbackManifestExists);
        var snapshotExists = !string.IsNullOrWhiteSpace(readiness.SnapshotEvidencePath)
            && ExistsSafely(readiness.SnapshotEvidencePath, rollbackManifestExists);
        var snapshotReady = !string.IsNullOrWhiteSpace(readiness.SnapshotId)
            && snapshotExists
            && IsSha256(readiness.SnapshotEvidenceSha256);
        var requiredBytes = gate.RequiredBytes;

        var steps = new List<MigrationPreflightStepViewModel>
        {
            Step(
                "feature-enabled",
                "\u5b89\u5168\u6267\u884c\u5165\u53e3",
                readiness.FeatureEnabled ? MigrationPreflightStepState.Complete : MigrationPreflightStepState.Blocked,
                readiness.FeatureEnabled ? "\u5df2\u5c31\u7eea" : "\u7b49\u5f85\u51c6\u5907",
                readiness.FeatureEnabled
                    ? "\u56de\u6eda\u548c\u5feb\u7167\u8bc1\u636e\u5df2\u751f\u6210\uff0c\u53ef\u4ee5\u8fdb\u5165\u5355\u72ec\u7684\u6700\u7ec8\u786e\u8ba4\u3002"
                    : "\u8fd9\u4efd\u65b9\u6848\u5c1a\u672a\u751f\u6210\u56de\u6eda\u548c\u5feb\u7167\u8bc1\u636e\uff0c\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6\u3002"),
            Step(
                "migration-score",
                "\u8fc1\u79fb\u8bc4\u5206",
                plan.Score.Band == MigrationRiskBand.NotRecommended
                    ? MigrationPreflightStepState.Blocked
                    : MigrationPreflightStepState.Complete,
                plan.Score.Band == MigrationRiskBand.NotRecommended ? "\u5df2\u963b\u6b62" : "\u5df2\u68c0\u67e5",
                FormatScore(plan.Score)),
            Step(
                "snapshot",
                "\u8fc1\u79fb\u524d\u5feb\u7167",
                !snapshotReady
                    ? MigrationPreflightStepState.Waiting
                    : MigrationPreflightStepState.Complete,
                snapshotReady ? "\u5df2\u5c31\u7eea" : "\u9700\u8981",
                snapshotReady
                    ? "\u5feb\u7167\u8bc1\u636e\u5df2\u4fdd\u5b58\uff1a" + readiness.SnapshotId
                    : "\u8fc1\u79fb\u524d\u5fc5\u987b\u5148\u521b\u5efa\u5e76\u6821\u9a8c\u5feb\u7167\u8bc1\u636e\u3002"),
            Step(
                "plan-confirmation",
                "\u786e\u8ba4\u65b9\u6848",
                readiness.UserConfirmedPlan
                    ? MigrationPreflightStepState.Complete
                    : MigrationPreflightStepState.Waiting,
                readiness.UserConfirmedPlan ? "\u5df2\u786e\u8ba4" : "\u6700\u7ec8\u786e\u8ba4",
                readiness.UserConfirmedPlan
                    ? "\u7528\u6237\u5df2\u786e\u8ba4\u76ee\u6807\u4f4d\u7f6e\u3001\u53d7\u5f71\u54cd\u8def\u5f84\u3001\u56de\u6eda\u65b9\u6848\u548c\u8fc1\u79fb\u540e\u89c2\u5bdf\u3002"
                    : "\u4e0b\u4e00\u6b65\u4f1a\u5728\u6700\u7ec8\u786e\u8ba4\u9875\u6838\u5bf9\u76ee\u6807\u4f4d\u7f6e\u3001\u53d7\u5f71\u54cd\u5185\u5bb9\u548c\u56de\u6eda\u65b9\u6848\u3002"),
            Step(
                "close-apps",
                "\u5173\u95ed\u5e94\u7528",
                readiness.UserConfirmedAppsClosed
                    ? MigrationPreflightStepState.Complete
                    : MigrationPreflightStepState.Waiting,
                readiness.UserConfirmedAppsClosed ? "\u5df2\u786e\u8ba4" : "\u6700\u7ec8\u786e\u8ba4",
                readiness.UserConfirmedAppsClosed
                    ? "\u7528\u6237\u5df2\u786e\u8ba4\u7a97\u53e3\u3001\u6258\u76d8\u8fdb\u7a0b\u3001\u670d\u52a1\u548c\u8ba1\u5212\u4efb\u52a1\u5df2\u5904\u7406\u3002"
                    : BuildFinalCloseAppsDetail(profile)),
            Step(
                "rollback-manifest",
                "\u56de\u6eda\u6e05\u5355",
                ManifestState(readiness, manifestExists),
                manifestExists ? "\u5df2\u5c31\u7eea" : string.IsNullOrWhiteSpace(readiness.RollbackManifestPath) ? "\u9700\u8981" : "\u7f3a\u5931",
                manifestExists
                    ? "\u56de\u6eda\u6e05\u5355\uff1a" + readiness.RollbackManifestPath
                    : string.IsNullOrWhiteSpace(readiness.RollbackManifestPath)
                        ? "\u8fc1\u79fb\u524d\u5fc5\u987b\u751f\u6210\u56de\u6eda\u6e05\u5355\u3002"
                        : "\u5df2\u6709\u56de\u6eda\u6e05\u5355\u8def\u5f84\uff0c\u4f46\u6587\u4ef6\u7f3a\u5931\u6216\u65e0\u6cd5\u8bfb\u53d6\u3002"),
            Step(
                "destination-space",
                "\u76ee\u6807\u76d8\u7a7a\u95f4",
                DestinationSpaceState(readiness, requiredBytes),
                DestinationSpaceLabel(readiness, requiredBytes),
                DestinationSpaceDetail(readiness, requiredBytes)),
            Step(
                "post-migration-monitoring",
                "\u8fc1\u79fb\u540e\u89c2\u5bdf",
                readiness.UserConfirmedPostMigrationMonitoring
                    ? MigrationPreflightStepState.Complete
                    : MigrationPreflightStepState.Waiting,
                readiness.UserConfirmedPostMigrationMonitoring ? "\u5df2\u786e\u8ba4" : "\u6700\u7ec8\u786e\u8ba4",
                readiness.UserConfirmedPostMigrationMonitoring
                    ? "\u7528\u6237\u5df2\u786e\u8ba4 OMNIX-Entropy \u4f1a\u7ee7\u7eed\u89c2\u5bdf\u539f C \u76d8\u8def\u5f84\u3002"
                    : "\u4e0b\u4e00\u6b65\u4f1a\u8981\u6c42\u786e\u8ba4\u8fc1\u79fb\u540e\u7ee7\u7eed\u89c2\u5bdf\u539f C \u76d8\u4f4d\u7f6e\u3002")
        };

        return new MigrationPreflightChecklistViewModel
        {
            CanRequestExecution = gate.CanRequestExecution,
            PrimaryActionText = BuildPrimaryActionText(readiness, gate),
            NextActionText = BuildNextActionText(steps, gate),
            ExecutionGate = gate,
            Operation = gate.Operation,
            Steps = steps
        };
    }

    private static MigrationPreflightStepViewModel Step(
        string key,
        string title,
        MigrationPreflightStepState state,
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

    private static MigrationPreflightStepState ManifestState(
        MigrationExecutionReadiness readiness,
        bool manifestExists)
    {
        if (manifestExists)
            return MigrationPreflightStepState.Complete;

        return string.IsNullOrWhiteSpace(readiness.RollbackManifestPath)
            ? MigrationPreflightStepState.Waiting
            : MigrationPreflightStepState.Blocked;
    }

    private static MigrationPreflightStepState DestinationSpaceState(
        MigrationExecutionReadiness readiness,
        long requiredBytes)
    {
        if (readiness.DestinationAvailableBytes is null)
            return MigrationPreflightStepState.Waiting;

        return readiness.DestinationAvailableBytes.Value >= requiredBytes
            ? MigrationPreflightStepState.Complete
            : MigrationPreflightStepState.Blocked;
    }

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);

    private static string DestinationSpaceLabel(MigrationExecutionReadiness readiness, long requiredBytes)
    {
        if (readiness.DestinationAvailableBytes is null)
            return "\u9700\u8981\u68c0\u67e5";

        return readiness.DestinationAvailableBytes.Value >= requiredBytes ? "\u7a7a\u95f4\u8db3\u591f" : "\u7a7a\u95f4\u4e0d\u8db3";
    }

    private static string DestinationSpaceDetail(MigrationExecutionReadiness readiness, long requiredBytes)
    {
        if (readiness.DestinationAvailableBytes is null)
            return "\u8fc1\u79fb\u524d\u9700\u8981\u68c0\u67e5\u76ee\u6807\u76d8\u5269\u4f59\u7a7a\u95f4\u3002";

        return readiness.DestinationAvailableBytes.Value >= requiredBytes
            ? "\u76ee\u6807\u76d8\u5269\u4f59\u7a7a\u95f4\u8db3\u591f\u5bb9\u7eb3\u4f30\u7b97\u7684\u8fc1\u79fb\u5185\u5bb9\u3002"
            : "\u76ee\u6807\u76d8\u5269\u4f59\u7a7a\u95f4\u4f4e\u4e8e\u4f30\u7b97\u7684\u8fc1\u79fb\u5927\u5c0f\u3002";
    }

    private static string BuildPrimaryActionText(
        MigrationExecutionReadiness readiness,
        MigrationExecutionGateResult gate)
    {
        if (!readiness.FeatureEnabled)
            return "\u5148\u751f\u6210\u56de\u6eda\u8bc1\u636e";

        return gate.CanRequestExecution
            ? "\u8bf7\u6c42\u8fc1\u79fb"
            : "\u5b8c\u6210\u8fc1\u79fb\u524d\u68c0\u67e5";
    }

    private static string BuildNextActionText(
        IReadOnlyList<MigrationPreflightStepViewModel> steps,
        MigrationExecutionGateResult gate)
    {
        if (gate.CanRequestExecution)
            return "\u8fc1\u79fb\u8bc1\u636e\u68c0\u67e5\u5df2\u901a\u8fc7\uff0c\u4e0b\u4e00\u6b65\u4ecd\u9700\u8981\u7528\u6237\u9010\u9879\u6700\u7ec8\u786e\u8ba4\u3002";

        foreach (var step in steps)
        {
            if (step.State != MigrationPreflightStepState.Complete)
                return step.Detail;
        }

        return "\u8fc1\u79fb\u5b89\u5168\u95e8\u4ecd\u5728\u963b\u6b62\u6267\u884c\uff1a" + string.Join(" ", gate.BlockingReasons);
    }

    private static string BuildFinalCloseAppsDetail(SoftwareProfile profile)
    {
        if (profile.RunningProcesses.Count == 0
            && profile.Services.Count == 0
            && profile.ScheduledTasks.Count == 0
            && profile.StartupEntries.Count == 0)
            return "\u6700\u7ec8\u786e\u8ba4\u9875\u4f1a\u8981\u6c42\u786e\u8ba4\u5e94\u7528\u548c\u76f8\u5173\u6258\u76d8\u7a97\u53e3\u5df2\u5173\u95ed\u3002";

        return "\u68c0\u6d4b\u5230\u540e\u53f0\u7ec4\u4ef6\uff1b\u6700\u7ec8\u786e\u8ba4\u9875\u4f1a\u8981\u6c42\u5148\u5173\u95ed\u8f6f\u4ef6\u7a97\u53e3\u548c\u6258\u76d8\u7a0b\u5e8f\u3002";
    }

    private static string FormatScore(MigrationScore score) =>
        LocalizeBand(score.Band) + "\uff1a" + LocalizeReason(score);

    private static string LocalizeBand(MigrationRiskBand band) =>
        band switch
        {
            MigrationRiskBand.Safe => "\u8f83\u5b89\u5168",
            MigrationRiskBand.NeedsStopAndVerify => "\u9700\u5173\u95ed\u540e\u9a8c\u8bc1",
            MigrationRiskBand.CacheOnly => "\u53ea\u5efa\u8bae\u8fc1\u79fb\u7f13\u5b58",
            MigrationRiskBand.NotRecommended => "\u4e0d\u5efa\u8bae\u8fc1\u79fb",
            _ => band.ToString()
        };

    private static string LocalizeReason(MigrationScore score) =>
        score.Band switch
        {
            MigrationRiskBand.NotRecommended => "\u7cfb\u7edf\u5de5\u5177\u53ef\u80fd\u4f9d\u8d56\u670d\u52a1\u3001\u9a71\u52a8\u6216\u5168\u5c40\u8def\u5f84\u3002",
            MigrationRiskBand.NeedsStopAndVerify => "\u53d1\u73b0\u540e\u53f0\u7ec4\u4ef6\uff0c\u8fc1\u79fb\u524d\u5fc5\u987b\u5173\u95ed\uff0c\u8fc1\u79fb\u540e\u5fc5\u987b\u9a8c\u8bc1\u3002",
            MigrationRiskBand.CacheOnly => "\u53ea\u77e5\u9053\u7f13\u5b58\u3001\u6a21\u578b\u6216\u4e0b\u8f7d\u8def\u5f84\uff0c\u4e0d\u5efa\u8bae\u79fb\u52a8\u4e3b\u7a0b\u5e8f\u3002",
            MigrationRiskBand.Safe => "\u8f6f\u4ef6\u753b\u50cf\u4e2d\u6ca1\u6709\u53d1\u73b0\u540e\u53f0\u7ec4\u4ef6\u3002",
            _ => score.Reason
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
}
