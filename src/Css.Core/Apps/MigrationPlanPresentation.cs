using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Css.Core;
using Css.Core.Migration;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class MigrationPlanDecisionSummaryViewModel
{
    public required string StatusLabel { get; init; }
    public required string Conclusion { get; init; }
    public required string TargetSummary { get; init; }
    public required string NextStep { get; init; }
    public required string RollbackSummary { get; init; }
    public required string SpaceSummary { get; init; }
}

public sealed class MigrationPlanPreviewViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string SafetyBanner { get; init; }
    public required string DestinationLine { get; init; }
    public required string ScoreLine { get; init; }
    public required string RollbackManifestLine { get; init; }
    public required string SuggestedRollbackManifestPath { get; init; }
    public required string DestinationSpaceLine { get; init; }
    public required bool CanRunMigration { get; init; }
    public required bool RequiresSnapshot { get; init; }
    public required bool IsRecommended { get; init; }
    public required bool IsAlreadyReasonable { get; init; }
    public required IReadOnlyList<string> BlockingReasons { get; init; }
    public required MigrationPreflightChecklistViewModel ReadinessChecklist { get; init; }
    public required IReadOnlyList<MigrationPlanSectionViewModel> Sections { get; init; }
    public required string PrimaryActionText { get; init; }
    public required string FinalReminder { get; init; }
    public MigrationPlanDecisionSummaryViewModel DecisionSummary =>
        MigrationPlanDecisionSummaryPresenter.Create(this);
}

public sealed class MigrationPlanSectionViewModel
{
    public required string Title { get; init; }
    public required string StatusLabel { get; init; }
    public required string Detail { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
}

public sealed class MigrationPlanPresentationOptions
{
    public DateTimeOffset? Now { get; init; }
    public string? SnapshotId { get; init; }
    public string? RollbackRoot { get; init; }
    public Func<string, long>? AvailableBytesProvider { get; init; }
    public MigrationExecutionReadiness? Readiness { get; init; }
    public Func<string, bool>? RollbackManifestExists { get; init; }
}

public static class MigrationPlanDecisionSummaryPresenter
{
    public static MigrationPlanDecisionSummaryViewModel Create(MigrationPlanPreviewViewModel preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        if (preview.IsAlreadyReasonable)
        {
            return new MigrationPlanDecisionSummaryViewModel
            {
                StatusLabel = "位置合理",
                Conclusion = "主程序已经在 D 盘，不需要再搬。接下来只观察它是否还往 C 盘写数据。",
                TargetSummary = "保持现在的 D 盘位置",
                NextStep = "观察 C 盘是否继续增长",
                RollbackSummary = "没有迁移动作，因此不需要回滚",
                SpaceSummary = "不需要额外占用 D 盘空间"
            };
        }

        if (!preview.IsRecommended)
        {
            return new MigrationPlanDecisionSummaryViewModel
            {
                StatusLabel = "不建议迁移",
                Conclusion = "不建议移动这个应用。改动位置可能影响服务、驱动或系统组件。",
                TargetSummary = "保持当前安装位置",
                NextStep = "优先使用软件官方的修复、更新或重装选项",
                RollbackSummary = "OMNIX 已阻止迁移，不会创建需要回滚的改动",
                SpaceSummary = "暂时不检查 D 盘空间"
            };
        }

        var canRun = preview.CanRunMigration && preview.ReadinessChecklist.CanRequestExecution;
        return new MigrationPlanDecisionSummaryViewModel
        {
            StatusLabel = canRun ? "可以进入确认" : "需要先准备",
            Conclusion = canRun
                ? "可以迁移，安全检查已经通过；开始前仍要逐项确认。"
                : "可以规划迁移，但现在还不能开始。OMNIX 会先准备安全证据。",
            TargetSummary = "目标是 D 盘的分类文件夹",
            NextStep = canRun
                ? "查看最终确认，核对影响和恢复办法"
                : "先创建快照和回滚清单，再重新检查",
            RollbackSummary = preview.RequiresSnapshot
                ? "可以后悔，但必须先建立快照和回滚清单"
                : "执行前仍会说明恢复办法",
            SpaceSummary = SpaceSummary(preview.DestinationSpaceLine)
        };
    }

    private static string SpaceSummary(string source)
    {
        if (source.Contains("空间不足", StringComparison.Ordinal))
            return "D 盘空间不足";
        if (source.Contains("空间足够", StringComparison.Ordinal))
            return "D 盘空间够用";
        return "D 盘空间还需要检查";
    }
}

public static class MigrationPlanPresentationBuilder
{
    public static MigrationPlanPreviewViewModel Create(SoftwareProfile profile) =>
        Create(profile, null);

    public static MigrationPlanPreviewViewModel Create(
        SoftwareProfile profile,
        MigrationPlanPresentationOptions? options)
    {
        if (IsOnDrive(profile.InstallPath, "D"))
            return CreateAlreadyOnD(profile, options);

        var plan = CreatePlan(profile);
        var destination = plan.DestinationRoot;
        if (plan.Score.Band == MigrationRiskBand.NotRecommended)
            return CreateNotRecommended(profile, plan, options);

        var blockers = new List<string>
        {
            "\u5f53\u524d\u53ea\u751f\u6210\u8fc1\u79fb\u9884\u89c8\uff0c\u4e0d\u6267\u884c\u8fc1\u79fb\u3002",
            "\u771f\u6b63\u8fc1\u79fb\u524d\u5fc5\u987b\u5148\u6709\u7cfb\u7edf\u5feb\u7167\u548c\u56de\u6eda\u6e05\u5355\u3002"
        };
        if (HasBackgroundActivity(profile))
            blockers.Add("\u8fc1\u79fb\u524d\u9700\u8981\u5148\u5173\u95ed\u5e94\u7528\u548c\u76f8\u5173\u540e\u53f0\u7ec4\u4ef6\u3002");
        if (plan.Score.Band == MigrationRiskBand.CacheOnly)
            blockers.Add("\u4e3b\u7a0b\u5e8f\u4f4d\u7f6e\u672a\u77e5\uff0c\u53ea\u5efa\u8bae\u8fc1\u79fb\u5df2\u77e5\u7f13\u5b58\u3001\u6a21\u578b\u6216\u4e0b\u8f7d\u8def\u5f84\u3002");
        var readiness = BuildDefaultReadinessChecklist(profile, plan, options);
        if (readiness.CanRequestExecution)
            blockers.Clear();

        return new MigrationPlanPreviewViewModel
        {
            Title = profile.Name + " \u8fc1\u79fb\u65b9\u6848",
            Summary = plan.Score.Band == MigrationRiskBand.CacheOnly
                ? "\u8fd9\u662f\u7f13\u5b58\u8fc1\u79fb\u9884\u89c8\uff1aOMNIX-Entropy \u4e0d\u4f1a\u5728\u8fd9\u91cc\u79fb\u52a8\u4e3b\u7a0b\u5e8f\u3002"
                : "\u8fd9\u662f\u8fc1\u79fb\u9884\u89c8\uff1a\u5148\u8bf4\u6e05\u8981\u68c0\u67e5\u4ec0\u4e48\uff0c\u518d\u51b3\u5b9a\u8981\u4e0d\u8981\u52a8\u3002",
            SafetyBanner = readiness.CanRequestExecution
                ? "\u8fc1\u79fb\u524d\u68c0\u67e5\u5df2\u901a\u8fc7\uff1b\u4ecd\u9700\u8981\u4f60\u5728\u6700\u7ec8\u786e\u8ba4\u9875\u9010\u9879\u540c\u610f\u3002"
                : "\u53ea\u9884\u89c8\uff1a\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6\u3001\u5feb\u6377\u65b9\u5f0f\u3001\u670d\u52a1\u3001\u81ea\u542f\u52a8\u9879\u6216\u6ce8\u518c\u8868\u3002",
            DestinationLine = "\u5efa\u8bae\u76ee\u6807\u4f4d\u7f6e\uff1a" + destination,
            ScoreLine = "\u8fc1\u79fb\u8bc4\u5206\uff1a" + FormatScore(plan.Score),
            RollbackManifestLine = BuildRollbackManifestLine(profile, plan, options),
            SuggestedRollbackManifestPath = BuildRollbackManifestPath(profile, plan, options),
            DestinationSpaceLine = BuildDestinationSpaceLine(profile, plan, options),
            CanRunMigration = readiness.CanRequestExecution,
            RequiresSnapshot = plan.RequiresSnapshot,
            IsRecommended = true,
            IsAlreadyReasonable = false,
            BlockingReasons = blockers,
            ReadinessChecklist = readiness,
            Sections =
            [
                BuildPreflightSection(profile),
                BuildStepsSection(plan),
                BuildRollbackSection(plan),
                BuildMonitoringSection(profile, plan)
            ],
            PrimaryActionText = readiness.PrimaryActionText,
            FinalReminder = readiness.CanRequestExecution
                ? "\u4e0b\u4e00\u6b65\u4f1a\u5148\u663e\u793a\u6700\u7ec8\u786e\u8ba4\uff1b\u53d6\u6d88\u6216\u4efb\u4f55\u5b89\u5168\u68c0\u67e5\u5931\u8d25\u90fd\u4e0d\u4f1a\u5f00\u59cb\u8fc1\u79fb\u3002"
                : "\u8fd9\u4e2a\u9875\u9762\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6\u3002\u771f\u6b63\u8fc1\u79fb\u9700\u8981\u5feb\u7167\u3001\u56de\u6eda\u65b9\u6848\u3001\u7528\u6237\u786e\u8ba4\u548c\u8fc1\u79fb\u540e\u89c2\u5bdf\u3002"
        };
    }

    public static MigrationPlanPreviewViewModel AddClosureReview(
        MigrationPlanPreviewViewModel preview,
        MigrationClosureSummaryViewModel? closure)
    {
        ArgumentNullException.ThrowIfNull(preview);
        if (closure?.NeedsAttention != true)
            return preview;

        var closureSection = new MigrationPlanSectionViewModel
        {
            Title = "先复查迁移闭环",
            StatusLabel = "需要检查",
            Detail = closure.Headline,
            Items =
            [
                closure.Detail,
                "重新扫描应用，确认它当前是否正在运行。",
                "旧监控记录不能直接授权移动；必须重新生成快照和回滚清单。",
                "处理后再次确认原 C 盘位置没有恢复写入。"
            ]
        };

        return new MigrationPlanPreviewViewModel
        {
            Title = preview.Title,
            Summary = closure.Headline + "。" + closure.Detail + "\n" + preview.Summary,
            SafetyBanner = "闭环异常只用于提示和重新规划，不会根据旧记录直接移动文件。\n" + preview.SafetyBanner,
            DestinationLine = preview.DestinationLine,
            ScoreLine = "闭环复查：需要检查。" + preview.ScoreLine,
            RollbackManifestLine = preview.RollbackManifestLine,
            SuggestedRollbackManifestPath = preview.SuggestedRollbackManifestPath,
            DestinationSpaceLine = preview.DestinationSpaceLine,
            CanRunMigration = preview.CanRunMigration,
            RequiresSnapshot = true,
            IsRecommended = preview.IsRecommended,
            IsAlreadyReasonable = false,
            BlockingReasons =
            [
                "迁移闭环状态已经变化，旧监控记录不能直接用于再次移动。",
                .. preview.BlockingReasons
            ],
            ReadinessChecklist = preview.ReadinessChecklist,
            Sections = [closureSection, .. preview.Sections],
            PrimaryActionText = preview.CanRunMigration
                ? preview.PrimaryActionText
                : "先重新生成安全证据",
            FinalReminder = "闭环异常不会自动修复。" + preview.FinalReminder
        };
    }

    public static MigrationPlan CreatePlan(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var destination = IsOnDrive(profile.InstallPath, "D")
            ? profile.InstallPath ?? RecommendMigrationDestination(profile)
            : RecommendMigrationDestination(profile);

        return MigrationPlanner.CreatePlan(profile, destination, snapshotAvailable: false);
    }

    private static MigrationPlanPreviewViewModel CreateAlreadyOnD(
        SoftwareProfile profile,
        MigrationPlanPresentationOptions? options)
    {
        var plan = CreatePlan(profile);
        var sections = new List<MigrationPlanSectionViewModel>
        {
            new()
            {
                Title = "\u5df2\u7ecf\u5728 D \u76d8",
                StatusLabel = "\u4e0d\u9700\u8fc1\u79fb",
                Detail = "\u4e3b\u7a0b\u5e8f\u5df2\u7ecf\u4e0d\u5728 C \u76d8\u3002",
                Items = [profile.InstallPath ?? "\u5b89\u88c5\u8def\u5f84\u5df2\u5728 D \u76d8\u3002"]
            },
            BuildMonitoringOnlySection(profile)
        };

        return new MigrationPlanPreviewViewModel
        {
            Title = profile.Name + " \u8fc1\u79fb\u65b9\u6848",
            Summary = "\u8fd9\u4e2a\u5e94\u7528\u5df2\u7ecf\u5728 D \u76d8\uff0c\u4e0d\u9700\u8981\u8fc1\u79fb\uff1b\u63a5\u4e0b\u6765\u89c2\u5bdf\u5b83\u662f\u5426\u8fd8\u5728 C \u76d8\u5199\u6570\u636e\u3002",
            SafetyBanner = "\u53ea\u9884\u89c8\uff1aOMNIX-Entropy \u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6\u3002",
            DestinationLine = "\u5f53\u524d\u4f4d\u7f6e\uff1a" + (profile.InstallPath ?? "D \u76d8"),
            ScoreLine = "\u8fc1\u79fb\u8bc4\u5206\uff1a\u5df2\u5728 D \u76d8\uff0c\u4f4d\u7f6e\u5408\u7406\u3002",
            RollbackManifestLine = "\u56de\u6eda\u6e05\u5355\u8349\u7a3f\uff1a\u4e0d\u9700\u8981\uff0c\u56e0\u4e3a\u5e94\u7528\u5df2\u7ecf\u5728 D \u76d8\u3002",
            SuggestedRollbackManifestPath = string.Empty,
            DestinationSpaceLine = BuildDestinationSpaceLine(profile, plan, options),
            CanRunMigration = false,
            RequiresSnapshot = false,
            IsRecommended = false,
            IsAlreadyReasonable = true,
            BlockingReasons = ["\u5df2\u7ecf\u5b89\u88c5\u5728 D \u76d8\u3002"],
            ReadinessChecklist = BuildDefaultReadinessChecklist(profile, plan, options),
            Sections = sections,
            PrimaryActionText = "\u4e0d\u9700\u8fc1\u79fb",
            FinalReminder = "\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6\u3002\u66f4\u6709\u7528\u7684\u4e0b\u4e00\u6b65\u662f\u89c2\u5bdf C \u76d8\u589e\u957f\u3002"
        };
    }

    private static MigrationPlanPreviewViewModel CreateNotRecommended(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationPlanPresentationOptions? options)
    {
        return new MigrationPlanPreviewViewModel
        {
            Title = profile.Name + " \u8fc1\u79fb\u65b9\u6848",
            Summary = "\u4e0d\u5efa\u8bae\u8fc1\u79fb\u8fd9\u4e2a\u5e94\u7528\u3002",
            SafetyBanner = "\u5df2\u963b\u6b62\uff1aOMNIX-Entropy \u4e0d\u4f1a\u8fc1\u79fb\u8fd9\u7c7b\u7cfb\u7edf\u654f\u611f\u5e94\u7528\u3002",
            DestinationLine = "\u76ee\u6807\u4f4d\u7f6e\uff1a\u4e0d\u5efa\u8bae\u8fc1\u79fb",
            ScoreLine = "\u8fc1\u79fb\u8bc4\u5206\uff1a" + FormatScore(plan.Score),
            RollbackManifestLine = BuildRollbackManifestLine(profile, plan, options),
            SuggestedRollbackManifestPath = BuildRollbackManifestPath(profile, plan, options),
            DestinationSpaceLine = "\u76ee\u6807\u76d8\u7a7a\u95f4\uff1a\u56e0\u4e3a\u4e0d\u5efa\u8bae\u8fc1\u79fb\uff0c\u6682\u4e0d\u68c0\u67e5\u3002",
            CanRunMigration = false,
            RequiresSnapshot = true,
            IsRecommended = false,
            IsAlreadyReasonable = false,
            BlockingReasons =
            [
                "\u7cfb\u7edf\u5de5\u5177\u4e0d\u5e94\u7531 OMNIX-Entropy \u8fc1\u79fb\u3002",
                "\u6539\u52a8\u5b83\u4eec\u7684\u8def\u5f84\u53ef\u80fd\u7834\u574f\u670d\u52a1\u3001\u9a71\u52a8\u6216\u5168\u5c40\u914d\u7f6e\u3002"
            ],
            ReadinessChecklist = BuildDefaultReadinessChecklist(profile, plan, options),
            Sections =
            [
                new MigrationPlanSectionViewModel
                {
                    Title = "\u4e0d\u8981\u8fc1\u79fb\u7cfb\u7edf\u5de5\u5177",
                    StatusLabel = "\u5df2\u963b\u6b62",
                    Detail = "\u8fd9\u4e2a\u5e94\u7528\u88ab\u8bc6\u522b\u4e3a\u7cfb\u7edf\u5de5\u5177\u6216\u8fd0\u884c\u65f6\u3002",
                    Items =
                    [
                        "\u9664\u975e\u5b98\u65b9\u5b89\u88c5\u5668\u652f\u6301\u66f4\u6539\u4f4d\u7f6e\uff0c\u5426\u5219\u4fdd\u6301\u539f\u6837\u3002",
                        "\u6280\u672f\u8be6\u60c5\u53ea\u7528\u4e8e\u8bca\u65ad\u3002"
                    ]
                },
                BuildMonitoringOnlySection(profile)
            ],
            PrimaryActionText = "\u4e0d\u8981\u8fc1\u79fb",
            FinalReminder = "\u4e0d\u4f1a\u79fb\u52a8\u6587\u4ef6\u3002\u7cfb\u7edf\u5de5\u5177\u5e94\u4f18\u5148\u4f7f\u7528\u5b98\u65b9\u4fee\u590d\u6216\u91cd\u88c5\u9009\u9879\u3002"
        };
    }

    private static MigrationPlanSectionViewModel BuildPreflightSection(SoftwareProfile profile)
    {
        var items = new List<string>
        {
            "\u5148\u521b\u5efa\u7cfb\u7edf\u5feb\u7167\u548c\u8fc1\u79fb\u6e05\u5355\u3002",
            "\u51c6\u5907\u79fb\u52a8\u524d\u5148\u5173\u95ed\u5e94\u7528\u3002"
        };
        if (profile.RunningProcesses.Count > 0)
            items.Add("\u9700\u5173\u95ed\u7684\u8fdb\u7a0b\uff1a" + string.Join(", ", profile.RunningProcesses.Take(5)));
        if (profile.Services.Count > 0)
            items.Add("\u9700\u505c\u6b62\u7684\u670d\u52a1\uff08\u5fc5\u987b\u786e\u8ba4\u540e\uff09\uff1a" + string.Join(", ", profile.Services.Take(5)));
        if (profile.StartupEntries.Count > 0)
            items.Add("\u8fc1\u79fb\u671f\u95f4\u9700\u8981\u6682\u505c\u76f8\u5173\u81ea\u542f\u52a8\u9879\u3002");
        if (profile.ScheduledTasks.Count > 0)
            items.Add("\u8fc1\u79fb\u671f\u95f4\u9700\u8981\u6682\u505c\u76f8\u5173\u8ba1\u5212\u4efb\u52a1\u3002");

        return new MigrationPlanSectionViewModel
        {
            Title = "\u8fc1\u79fb\u524d\u68c0\u67e5",
            StatusLabel = "\u5fc5\u987b\u5b8c\u6210",
            Detail = "\u771f\u6b63\u7533\u8bf7\u8fc1\u79fb\u524d\uff0c\u8fd9\u4e9b\u68c0\u67e5\u5fc5\u987b\u5148\u901a\u8fc7\u3002",
            Items = items
        };
    }

    private static MigrationPlanSectionViewModel BuildStepsSection(MigrationPlan plan)
    {
        var items = plan.Steps.Count == 0
            ? ["\u6682\u65f6\u65e0\u6cd5\u751f\u6210\u79fb\u52a8\u6b65\u9aa4\u3002"]
            : plan.Steps.Take(8).Select(LocalizePlanStep).ToList();

        return new MigrationPlanSectionViewModel
        {
            Title = "\u8fc1\u79fb\u6b65\u9aa4\u9884\u89c8",
            StatusLabel = "\u53ea\u9884\u89c8",
            Detail = "\u8fd9\u53ea\u662f\u53ef\u8bfb\u65b9\u6848\uff0c\u4e0d\u4f1a\u6267\u884c\u3002",
            Items = items
        };
    }

    private static MigrationPlanSectionViewModel BuildRollbackSection(MigrationPlan plan)
    {
        return new MigrationPlanSectionViewModel
        {
            Title = "\u56de\u6eda\u65b9\u6848",
            StatusLabel = "\u5fc5\u987b\u5b8c\u6210",
            Detail = "\u5982\u679c\u4e0d\u80fd\u540e\u6094\uff0c\u8fc1\u79fb\u5c31\u4e0d\u5b89\u5168\u3002",
            Items = plan.Rollback.Steps.Count == 0
                ? ["\u6ca1\u6709\u751f\u6210\u56de\u6eda\u6b65\u9aa4\uff0c\u6240\u4ee5\u5fc5\u987b\u7ee7\u7eed\u963b\u6b62\u8fc1\u79fb\u3002"]
                : plan.Rollback.Steps.Select(LocalizeRollbackStep).ToList()
        };
    }

    private static MigrationPlanSectionViewModel BuildMonitoringSection(SoftwareProfile profile, MigrationPlan plan)
    {
        var items = new List<string>();
        items.AddRange(plan.VerificationSteps.Select(LocalizeVerificationStep));
        items.AddRange(CDriveMonitoringItems(profile));

        return new MigrationPlanSectionViewModel
        {
            Title = "\u8fc1\u79fb\u540e\u89c2\u5bdf",
            StatusLabel = "\u5fc5\u987b\u5b8c\u6210",
            Detail = "\u53ea\u6709\u539f C \u76d8\u8def\u5f84\u4e0d\u518d\u589e\u957f\uff0c\u8fc1\u79fb\u624d\u7b97\u95ed\u73af\u3002",
            Items = items
        };
    }

    private static MigrationPlanSectionViewModel BuildMonitoringOnlySection(SoftwareProfile profile)
    {
        return new MigrationPlanSectionViewModel
        {
            Title = "\u53ea\u89c2\u5bdf",
            StatusLabel = "\u89c2\u5bdf",
            Detail = "\u5b89\u88c5\u4f4d\u7f6e\u4e0d\u662f\u4e3b\u8981\u95ee\u9898\uff1b\u9700\u8981\u89c2\u5bdf\u6570\u636e\u662f\u5426\u8fd8\u5728 C \u76d8\u589e\u957f\u3002",
            Items = CDriveMonitoringItems(profile)
        };
    }

    private static IReadOnlyList<string> CDriveMonitoringItems(SoftwareProfile profile)
    {
        if (profile.CDriveWritePaths.Count == 0)
            return ["\u6682\u65f6\u6ca1\u6709\u5df2\u77e5 C \u76d8\u5199\u5165\u8def\u5f84\uff1b\u5efa\u8bae\u4fdd\u6301\u6bcf\u65e5\u589e\u957f\u8ffd\u8e2a\u3002"];

        return profile.CDriveWritePaths
            .Take(8)
            .Select(path => "\u89c2\u5bdf C \u76d8\u8def\u5f84\uff1a" + path)
            .ToList();
    }

    private static bool HasBackgroundActivity(SoftwareProfile profile) =>
        profile.RunningProcesses.Count > 0
        || profile.Services.Count > 0
        || profile.StartupEntries.Count > 0
        || profile.ScheduledTasks.Count > 0;

    private static MigrationPreflightChecklistViewModel BuildDefaultReadinessChecklist(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationPlanPresentationOptions? options) =>
        MigrationPreflightChecklistBuilder.Create(
            profile,
            plan,
            options?.Readiness ?? new MigrationExecutionReadiness(),
            options?.RollbackManifestExists ?? (_ => false));

    private static string BuildRollbackManifestLine(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationPlanPresentationOptions? options)
    {
        var manifestPath = BuildRollbackManifestPath(profile, plan, options);
        var now = options?.Now ?? DateTimeOffset.Now;
        var snapshotId = string.IsNullOrWhiteSpace(options?.SnapshotId)
            ? "snapshot-required"
            : options!.SnapshotId!;
        var manifest = MigrationRollbackManifestBuilder.Build(profile, plan, snapshotId, now);

        if (!string.IsNullOrWhiteSpace(options?.Readiness?.RollbackManifestPath)
            && ExistsSafely(options.Readiness.RollbackManifestPath, options.RollbackManifestExists))
        {
            return "\u56de\u6eda\u6e05\u5355\u5df2\u5c31\u7eea\uff1a" + options.Readiness.RollbackManifestPath +
                "\uff08" + manifest.Entries.Count + " \u4e2a\u8def\u5f84\uff0cJSON \u8bc1\u636e\u5df2\u5199\u5165\uff09\u3002";
        }

        return "\u56de\u6eda\u6e05\u5355\u8349\u7a3f\uff1a" + manifestPath +
            "\uff08" + manifest.Entries.Count + " \u4e2a\u8def\u5f84\uff0c\u5c1a\u672a\u5199\u5165\uff09\u3002";
    }

    public static string BuildRollbackManifestPath(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationPlanPresentationOptions? options)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(plan);

        var now = options?.Now ?? DateTimeOffset.Now;
        var root = string.IsNullOrWhiteSpace(options?.RollbackRoot)
            ? DefaultRollbackRoot()
            : options!.RollbackRoot!;

        return Path.Combine(
            root,
            SafePathName(profile.Name),
            now.ToString("yyyyMMddHHmmss"),
            "migration.rollback.json");
    }

    private static string BuildDestinationSpaceLine(
        SoftwareProfile profile,
        MigrationPlan plan,
        MigrationPlanPresentationOptions? options)
    {
        var requiredBytes = MigrationExecutionGate.EstimateRequiredBytes(profile);
        if (options?.Readiness?.DestinationAvailableBytes is long availableBytes)
        {
            var readinessState = availableBytes >= requiredBytes ? "\u7a7a\u95f4\u8db3\u591f" : "\u7a7a\u95f4\u4e0d\u8db3";
            return "\u76ee\u6807\u76d8\u7a7a\u95f4\uff1a" + readinessState +
                "\uff08\u53ef\u7528 " + availableBytes + " bytes / \u9700\u8981 " + requiredBytes + " bytes\uff09\u3002";
        }

        var probe = options?.AvailableBytesProvider is null
            ? MigrationDestinationSpaceProbe.CheckCurrentMachine(plan.DestinationRoot, requiredBytes)
            : MigrationDestinationSpaceProbe.Check(plan.DestinationRoot, requiredBytes, options.AvailableBytesProvider);

        if (!probe.CanCheck)
            return "\u76ee\u6807\u76d8\u7a7a\u95f4\uff1a\u6682\u65f6\u65e0\u6cd5\u68c0\u67e5\u3002" + (probe.Error ?? probe.Summary);

        var state = probe.HasEnoughSpace ? "\u7a7a\u95f4\u8db3\u591f" : "\u7a7a\u95f4\u4e0d\u8db3";
        return "\u76ee\u6807\u76d8\u7a7a\u95f4\uff1a" + state +
            "\uff08\u53ef\u7528 " + probe.AvailableBytes + " bytes / \u9700\u8981 " + probe.RequiredBytes + " bytes\uff09\u3002";
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

    private static string LocalizePlanStep(string step)
    {
        if (step.Equals("Stop related services before moving files.", StringComparison.OrdinalIgnoreCase))
            return "\u79fb\u52a8\u6587\u4ef6\u524d\u5148\u505c\u6b62\u76f8\u5173\u670d\u52a1\u3002";
        if (step.Equals("Disable related startup entries during migration.", StringComparison.OrdinalIgnoreCase))
            return "\u8fc1\u79fb\u671f\u95f4\u6682\u505c\u76f8\u5173\u81ea\u542f\u52a8\u9879\u3002";
        if (step.StartsWith("Move ", StringComparison.OrdinalIgnoreCase) && step.Contains(" to ", StringComparison.OrdinalIgnoreCase))
        {
            var withoutPrefix = step["Move ".Length..].TrimEnd('.');
            var split = withoutPrefix.Split(" to ", 2, StringSplitOptions.None);
            if (split.Length == 2)
                return "\u8ba1\u5212\u628a " + split[0] + " \u79fb\u5230 " + split[1] + "\u3002";
        }
        if (step.Equals("Create redirect or update configuration for moved paths.", StringComparison.OrdinalIgnoreCase))
            return "\u4e3a\u5df2\u79fb\u52a8\u7684\u8def\u5f84\u521b\u5efa\u91cd\u5b9a\u5411\u6216\u66f4\u65b0\u914d\u7f6e\u3002";

        return step;
    }

    private static string LocalizeVerificationStep(string step)
    {
        if (step.Equals("Launch the software and confirm it starts from the new location.", StringComparison.OrdinalIgnoreCase))
            return "\u542f\u52a8\u8f6f\u4ef6\uff0c\u786e\u8ba4\u5b83\u80fd\u4ece\u65b0\u4f4d\u7f6e\u6b63\u5e38\u6253\u5f00\u3002";
        if (step.Equals("Scan original C: paths to confirm no new writes happened after migration.", StringComparison.OrdinalIgnoreCase))
            return "\u626b\u63cf\u539f C \u76d8\u8def\u5f84\uff0c\u786e\u8ba4\u8fc1\u79fb\u540e\u6ca1\u6709\u7ee7\u7eed\u5199\u5165\u3002";

        return step;
    }

    private static string LocalizeRollbackStep(string step)
    {
        if (step.Equals("Stop the software and related background tasks.", StringComparison.OrdinalIgnoreCase))
            return "\u505c\u6b62\u8f6f\u4ef6\u548c\u76f8\u5173\u540e\u53f0\u4efb\u52a1\u3002";
        if (step.Equals("Remove redirects created during migration.", StringComparison.OrdinalIgnoreCase))
            return "\u79fb\u9664\u8fc1\u79fb\u65f6\u521b\u5efa\u7684\u91cd\u5b9a\u5411\u3002";
        if (step.Equals("Move files back to their original paths from the migration manifest.", StringComparison.OrdinalIgnoreCase))
            return "\u6309\u8fc1\u79fb\u6e05\u5355\u628a\u6587\u4ef6\u79fb\u56de\u539f\u8def\u5f84\u3002";

        return step;
    }

    private static string DefaultRollbackRoot() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppIdentity.LocalDataFolderName,
            "MigrationRollback");

    private static string RecommendMigrationDestination(SoftwareProfile profile)
    {
        var root = profile.Category switch
        {
            SoftwareCategory.Game => @"D:\Game",
            SoftwareCategory.Ai => @"D:\Agent",
            SoftwareCategory.DevelopmentTool => @"D:\Development",
            _ => @"D:\Software"
        };

        return root + "\\" + SafePathName(profile.Name) + "\\Install";
    }

    private static string SafePathName(string name)
    {
        var invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
        var cleaned = new string(name
            .Select(ch => invalid.Contains(ch) ? '_' : ch)
            .ToArray())
            .Trim();

        return string.IsNullOrWhiteSpace(cleaned) ? "UnknownApp" : cleaned;
    }

    private static bool IsOnDrive(string? path, string driveLetter) =>
        !string.IsNullOrWhiteSpace(path) &&
        path.StartsWith(driveLetter + ":\\", StringComparison.OrdinalIgnoreCase);

    private static bool ExistsSafely(string path, Func<string, bool>? exists)
    {
        if (exists is null)
            return false;

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
