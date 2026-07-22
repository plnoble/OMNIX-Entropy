using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Software;

namespace Css.InstallGuard.Installers;

public sealed record InstallSystemSnapshot(
    DateTimeOffset CapturedAt,
    IReadOnlyList<SoftwareProfile> SoftwareProfiles,
    InstallFootprintCapture? CDriveFootprint = null);

public sealed class InstallSnapshotDiffReport
{
    public required DateTimeOffset BeforeCapturedAt { get; init; }
    public required DateTimeOffset AfterCapturedAt { get; init; }
    public IReadOnlyList<SoftwareProfile> AddedSoftware { get; init; } = [];
    public IReadOnlyList<string> NewStartupEntries { get; init; } = [];
    public IReadOnlyList<string> NewServices { get; init; } = [];
    public IReadOnlyList<string> NewScheduledTasks { get; init; } = [];
    public IReadOnlyList<string> NewCDrivePaths { get; init; } = [];
    public InstallFootprintCaptureStatus CDriveFootprintStatus { get; init; } =
        InstallFootprintCaptureStatus.Complete;
    public bool HasCDriveWrites { get; init; }
    public required string Summary { get; init; }
}

public sealed class InstallSnapshotDiffViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string SafetyText { get; init; }
    public bool CanExecuteDirectly { get; init; }
    public IReadOnlyList<InstallSnapshotDiffCardViewModel> Cards { get; init; } = [];
    public IReadOnlyList<string> TechnicalDetails { get; init; } = [];
}

public sealed class InstallSnapshotDiffCardViewModel
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required string Detail { get; init; }
}

public sealed class InstallSnapshotDiffAgentViewModel
{
    public required string Title { get; init; }
    public required string Headline { get; init; }
    public required string WhatThisMeans { get; init; }
    public required IReadOnlyList<string> NextSteps { get; init; }
    public required string SafetyBoundary { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

internal enum InstallProgramPlacementKind
{
    NoUniqueSoftware,
    CDrive,
    DDrive,
    OtherOrUnknown
}

internal sealed record InstallProgramPlacementObservation(
    InstallProgramPlacementKind Kind,
    SoftwareProfile? UniqueSoftware,
    int CDriveCandidateCount,
    int OwnedSeparateCDriveCandidateCount,
    int UnattributedSeparateCDriveCandidateCount);

internal static class InstallProgramPlacementAnalyzer
{
    public static InstallProgramPlacementObservation Create(InstallSnapshotDiffReport report)
    {
        var cDriveCandidates = report.NewCDrivePaths
            .Select(TryCanonicalPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Where(path => IsOnDrive(path, "C"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (report.AddedSoftware.Count != 1)
        {
            return new InstallProgramPlacementObservation(
                InstallProgramPlacementKind.NoUniqueSoftware,
                null,
                cDriveCandidates.Length,
                0,
                cDriveCandidates.Length);
        }

        var profile = report.AddedSoftware[0];
        var installRoot = TryCanonicalPath(profile.InstallPath);
        var separateCandidates = cDriveCandidates
            .Where(path => installRoot is null || !IsSameOrDescendant(installRoot, path))
            .ToArray();
        var ownedHints = profile.CDriveWritePaths
            .Concat(profile.CachePaths)
            .Concat(profile.DataPaths)
            .Concat(profile.LogPaths)
            .Select(TryCanonicalPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Where(path => IsOnDrive(path, "C"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var ownedCount = separateCandidates.Count(candidate =>
            ownedHints.Any(hint => PathsOverlap(hint, candidate)));
        var kind = IsOnDrive(installRoot, "C")
            ? InstallProgramPlacementKind.CDrive
            : IsOnDrive(installRoot, "D")
                ? InstallProgramPlacementKind.DDrive
                : InstallProgramPlacementKind.OtherOrUnknown;

        return new InstallProgramPlacementObservation(
            kind,
            profile,
            cDriveCandidates.Length,
            ownedCount,
            separateCandidates.Length - ownedCount);
    }

    private static string? TryCanonicalPath(string? path)
    {
        try
        {
            return string.IsNullOrWhiteSpace(path)
                ? null
                : Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return null;
        }
    }

    private static bool PathsOverlap(string left, string right) =>
        IsSameOrDescendant(left, right) || IsSameOrDescendant(right, left);

    private static bool IsSameOrDescendant(string parent, string candidate)
    {
        if (parent.Equals(candidate, StringComparison.OrdinalIgnoreCase))
            return true;

        return candidate.StartsWith(
            parent + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOnDrive(string? path, string driveLetter) =>
        !string.IsNullOrWhiteSpace(path)
        && path.StartsWith(driveLetter + ":\\", StringComparison.OrdinalIgnoreCase);
}

public static class InstallSnapshotDiffAgentPresenter
{
    public static InstallSnapshotDiffAgentViewModel Create(InstallSnapshotDiffReport report)
    {
        var cDriveCount = report.NewCDrivePaths.Count;
        var backgroundCount = report.NewStartupEntries.Count
            + report.NewServices.Count
            + report.NewScheduledTasks.Count;
        var footprintStatus = report.CDriveFootprintStatus;
        var placement = InstallProgramPlacementAnalyzer.Create(report);

        return new InstallSnapshotDiffAgentViewModel
        {
            Title = "Computer Agent \u89e3\u91ca",
            Headline = BuildHeadline(cDriveCount, backgroundCount, footprintStatus, placement),
            WhatThisMeans = BuildMeaning(cDriveCount, backgroundCount, footprintStatus, placement),
            NextSteps = BuildNextSteps(cDriveCount, backgroundCount, footprintStatus, placement),
            SafetyBoundary = "\u5b89\u5168\u8fb9\u754c\uff1a\u6211\u4e0d\u4f1a\u76f4\u63a5\u6267\u884c\u6e05\u7406\u3001\u8fc1\u79fb\u3001\u5173\u95ed\u540e\u53f0\u9879\u6216\u4fee\u6539\u7cfb\u7edf\uff1b\u771f\u6b63\u52a8\u4f5c\u5fc5\u987b\u7531\u4f60\u786e\u8ba4\u5e76\u8fdb\u5165\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002",
            CanExecuteDirectly = false
        };
    }

    private static string BuildHeadline(
        int cDriveCount,
        int backgroundCount,
        InstallFootprintCaptureStatus footprintStatus,
        InstallProgramPlacementObservation placement)
    {
        if (footprintStatus != InstallFootprintCaptureStatus.Complete)
        {
            return cDriveCount > 0 || backgroundCount > 0
                ? "已经观察到一些安装前后变化，但 C 盘落地点观察未完成，建议先复查。"
                : "C 盘落地点观察未完成，现在还不能判断这次没有写入 C 盘。";
        }

        if (placement.Kind == InstallProgramPlacementKind.DDrive && cDriveCount > 0)
        {
            return placement.OwnedSeparateCDriveCandidateCount > 0
                ? backgroundCount > 0
                    ? "主程序装在 D 盘，但安装期间仍观察到 C 盘数据或写入线索，也有后台变化。"
                    : "主程序装在 D 盘，但安装期间仍观察到 C 盘数据或写入线索。"
                : backgroundCount > 0
                    ? "主程序装在 D 盘，同时有尚未归属的 C 盘变化候选和后台变化。"
                    : "主程序装在 D 盘，同时有尚未归属的 C 盘变化候选。";
        }

        if (placement.Kind == InstallProgramPlacementKind.CDrive)
        {
            return backgroundCount > 0
                ? "主程序装在 C 盘；还要分别查看程序位置、其他 C 盘数据和后台变化。"
                : "主程序装在 C 盘；还要分别查看程序位置和其他 C 盘数据。";
        }

        if (placement.Kind == InstallProgramPlacementKind.NoUniqueSoftware && cDriveCount > 0)
            return "观察到 C 盘变化，但没有唯一新增软件，暂时不能判断主程序落点。";

        if (cDriveCount > 0 && backgroundCount > 0)
            return "安装前后同时观察到 C 盘内容和后台项变化，建议先弄清用途。";

        if (cDriveCount > 0)
            return "安装前后观察到 C 盘内容变化，但不需要立刻删除。";

        if (backgroundCount > 0)
            return "安装前后观察到后台项变化，建议先确认是否必要。";

        return "\u8fd9\u6b21\u5b89\u88c5\u6ca1\u6709\u660e\u663e\u589e\u52a0\u7cfb\u7edf\u538b\u529b\uff0c\u4e0d\u7528\u6025\u7740\u5904\u7406\u3002";
    }

    private static string BuildMeaning(
        int cDriveCount,
        int backgroundCount,
        InstallFootprintCaptureStatus footprintStatus,
        InstallProgramPlacementObservation placement)
    {
        if (footprintStatus != InstallFootprintCaptureStatus.Complete)
        {
            return cDriveCount == 0 && backgroundCount == 0
                ? "本次观察被截断或有位置无法读取，没有发现候选不等于没有新增内容。"
                : $"目前已观察到 C 盘 {cDriveCount} 个新位置候选、{backgroundCount} 项后台变化；由于落地点观察不完整，结果可能仍有遗漏。";
        }

        if (placement.Kind == InstallProgramPlacementKind.DDrive && cDriveCount > 0)
        {
            var background = backgroundCount > 0
                ? $"，另有 {backgroundCount} 项后台变化"
                : string.Empty;
            return placement.OwnedSeparateCDriveCandidateCount > 0
                ? $"主程序本身在 D 盘；其中 {placement.OwnedSeparateCDriveCandidateCount} 个新位置候选可以归到这个软件的 C 盘数据或写入线索，但不代表主程序装错位置{background}。"
                : $"主程序本身在 D 盘；同时观察到 {cDriveCount} 个同期 C 盘变化候选，但不能确认属于这个软件{background}。";
        }

        if (placement.Kind == InstallProgramPlacementKind.CDrive)
        {
            var background = backgroundCount > 0
                ? $"，另有 {backgroundCount} 项后台变化"
                : string.Empty;
            return placement.OwnedSeparateCDriveCandidateCount > 0
                ? $"主程序本身在 C 盘；安装目录之外 {placement.OwnedSeparateCDriveCandidateCount} 个 C 盘数据或写入线索可以归到这个软件{background}。"
                : $"主程序本身在 C 盘；目前没有确认安装目录之外还有属于它的 C 盘数据线索{background}。";
        }

        if (placement.Kind == InstallProgramPlacementKind.NoUniqueSoftware && cDriveCount > 0)
            return $"没有唯一新增软件，{cDriveCount} 个 C 盘变化候选不能判断主程序装在 C 盘还是 D 盘，也不能安全归到某个软件。";

        if (cDriveCount == 0 && backgroundCount == 0)
            return "\u6ca1\u6709\u53d1\u73b0\u65b0\u7684 C \u76d8\u538b\u529b\uff0c\u4e5f\u6ca1\u6709\u65b0\u589e\u81ea\u542f\u52a8\u3001\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u3002";

        if (cDriveCount > 0 && backgroundCount > 0)
            return $"安装前后观察到 {cDriveCount} 个新位置候选和 {backgroundCount} 项后台变化。它们不一定都由这个安装器产生，需要先分清用途。";

        if (cDriveCount > 0)
            return $"安装前后在 C 盘观察到 {cDriveCount} 个新位置候选，可能是安装文件、缓存或配置数据，也可能是同期其他变化。";

        return $"安装前后观察到 {backgroundCount} 项后台变化，可能用于更新、同步或软件核心功能，也需要确认归属。";
    }

    private static IReadOnlyList<string> BuildNextSteps(
        int cDriveCount,
        int backgroundCount,
        InstallFootprintCaptureStatus footprintStatus,
        InstallProgramPlacementObservation placement)
    {
        if (footprintStatus != InstallFootprintCaptureStatus.Complete)
        {
            var incompleteSteps = new List<string>
            {
                "先重新进行一次安装落地点观察，确认固定目录都能完整读取。"
            };
            if (cDriveCount > 0)
                incompleteSteps.Add("已发现的 C 盘位置仍只是候选，先保留并确认用途。");
            if (backgroundCount > 0)
                incompleteSteps.Add("已发现的后台变化先保持现状，不要在证据不完整时关闭。");
            incompleteSteps.Add("复查完成前，只记录和解释，不生成可执行处理动作。");
            return incompleteSteps;
        }

        if (cDriveCount == 0 && backgroundCount == 0)
        {
            return
            [
                "\u7ee7\u7eed\u89c2\u5bdf C \u76d8\u548c\u540e\u53f0\u9879\u5728\u4e0b\u6b21\u4f53\u68c0\u4e2d\u662f\u5426\u589e\u957f\u3002",
                "\u5982\u679c\u8f6f\u4ef6\u51fa\u73b0\u5f02\u5e38\u6216 C \u76d8\u7ee7\u7eed\u53d8\u5927\uff0c\u518d\u8ba9 Agent \u91cd\u65b0\u5206\u6790\u3002"
            ];
        }

        var steps = new List<string>();
        if (placement.Kind == InstallProgramPlacementKind.DDrive && cDriveCount > 0)
        {
            steps.Add(placement.OwnedSeparateCDriveCandidateCount > 0
                ? "主程序已经在 D 盘，不要重复迁移主程序；先复查软件的数据、缓存、日志或模型位置。"
                : "不要把同期变化当成这个软件的数据；先用应用详情和后续增长对比确认归属。");
        }
        else if (placement.Kind == InstallProgramPlacementKind.CDrive)
        {
            steps.Add("先分别查看主程序安装位置和安装目录之外的数据线索，再决定是否重装、迁移或只调整数据设置。");
        }
        else if (placement.Kind == InstallProgramPlacementKind.NoUniqueSoftware && cDriveCount > 0)
        {
            steps.Add("先确认唯一新增软件，再判断主程序位置和 C 盘候选归属。");
        }

        if (cDriveCount > 0)
            steps.Add("\u5148\u5224\u65ad C \u76d8\u65b0\u589e\u5185\u5bb9\u662f\u5b89\u88c5\u6587\u4ef6\u3001\u7f13\u5b58\u8fd8\u662f\u914d\u7f6e\u6570\u636e\u3002");

        if (backgroundCount > 0)
            steps.Add("\u9010\u9879\u786e\u8ba4\u65b0\u540e\u53f0\u9879\u662f\u5426\u662f\u8f6f\u4ef6\u6b63\u5e38\u529f\u80fd\uff0c\u6682\u65f6\u4e0d\u8981\u76f4\u63a5\u5173\u95ed\u3002");

        steps.Add("\u8ba9 Agent \u751f\u6210\u5904\u7406\u65b9\u6848\uff0c\u5206\u522b\u6807\u660e\u4fdd\u7559\u3001\u6e05\u7406\u7f13\u5b58\u3001\u8fc1\u79fb\u6216\u89c2\u5bdf\u3002");
        steps.Add("\u4f60\u786e\u8ba4\u540e\uff0c\u771f\u6b63\u52a8\u4f5c\u624d\u53ef\u4ee5\u8fdb\u5165\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002");
        return steps;
    }
}

public sealed class InstallSnapshotDiffActionPlanViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string ReviewSummary { get; init; }
    public required InstallSnapshotDiffEvidenceReviewViewModel EvidenceReview { get; init; }
    public required IReadOnlyList<InstallSnapshotDiffActionPlanItemViewModel> Items { get; init; }
    public required string SafetyBoundary { get; init; }
    public bool RequiresUserConfirmation { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public sealed class InstallSnapshotDiffActionPlanItemViewModel
{
    public int Order { get; init; }
    public required string Title { get; init; }
    public required string Decision { get; init; }
    public required string Reason { get; init; }
    public required string EvidenceSummary { get; init; }
    public required string RiskLabel { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public enum InstallSnapshotCDriveContentKind
{
    InstallFiles,
    Cache,
    Configuration,
    Logs,
    ModelOrData,
    Unknown
}

public enum InstallSnapshotBackgroundKind
{
    Startup,
    Service,
    ScheduledTask
}

public enum InstallSnapshotEligibleActionKind
{
    CacheCleanupPlan,
    StorageSettingGuidance,
    ReinstallOrMigrationPlan,
    StartupDisablePlan,
    ObserveOnly
}

public sealed class InstallSnapshotDiffEvidenceReviewViewModel
{
    public required string Summary { get; init; }
    public required IReadOnlyList<InstallSnapshotCDriveReviewItemViewModel> CDriveItems { get; init; }
    public required IReadOnlyList<InstallSnapshotBackgroundReviewItemViewModel> BackgroundItems { get; init; }
    public required IReadOnlyList<InstallSnapshotEligibleActionViewModel> EligibleActions { get; init; }
    public required string SafetyBoundary { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public sealed class InstallSnapshotEligibleActionViewModel
{
    public InstallSnapshotEligibleActionKind Kind { get; init; }
    public required string Title { get; init; }
    public required string Reason { get; init; }
    public required string EvidenceSummary { get; init; }
    public required string NextEvidenceNeeded { get; init; }
    public required string SafetyLabel { get; init; }
    public bool RequiresUserConfirmation { get; init; }
    public bool RequiresRollback { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public enum InstallSnapshotCandidatePreviewStatus
{
    Ready,
    GuidanceOnly,
    Refused
}

public sealed class InstallSnapshotCandidatePreviewViewModel
{
    public InstallSnapshotEligibleActionKind Kind { get; init; }
    public InstallSnapshotCandidatePreviewStatus Status { get; init; }
    public required string StatusLabel { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string AgentTakeaway { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public required IReadOnlyList<string> MissingEvidence { get; init; }
    public required string SafetyBoundary { get; init; }
    public string? TargetAppName { get; init; }
    public required string NavigationLabel { get; init; }
    public bool CanNavigateToApp { get; init; }
    public bool RequiresUserConfirmation { get; init; }
    public bool RequiresSnapshot { get; init; }
    public bool RequiresRollback { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public sealed class InstallSnapshotCDriveReviewItemViewModel
{
    public int Index { get; init; }
    public required string DisplayName { get; init; }
    public InstallSnapshotCDriveContentKind Kind { get; init; }
    public required string KindLabel { get; init; }
    public required string Purpose { get; init; }
    public required string Advice { get; init; }
    public required string ConfidenceLabel { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public sealed class InstallSnapshotBackgroundReviewItemViewModel
{
    public int Index { get; init; }
    public required string DisplayName { get; init; }
    public InstallSnapshotBackgroundKind Kind { get; init; }
    public required string KindLabel { get; init; }
    public required string LikelyPurpose { get; init; }
    public required string Advice { get; init; }
    public required string RiskLabel { get; init; }
    public required string ConfidenceLabel { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class InstallSnapshotDiffEvidenceReviewPresenter
{
    public static InstallSnapshotDiffEvidenceReviewViewModel Create(InstallSnapshotDiffReport report)
    {
        var cDriveItems = report.NewCDrivePaths
            .Select((path, index) => CreateCDriveItem(path, index + 1))
            .ToArray();
        var backgroundItems = CreateBackgroundItems(report);

        return new InstallSnapshotDiffEvidenceReviewViewModel
        {
            Summary = BuildSummary(cDriveItems, backgroundItems, report.CDriveFootprintStatus),
            CDriveItems = cDriveItems,
            BackgroundItems = backgroundItems,
            EligibleActions = BuildEligibleActions(
                cDriveItems,
                backgroundItems,
                report.CDriveFootprintStatus),
            SafetyBoundary = "这是根据位置和名称特征得出的初步判断，不代表已经确认用途，也不会直接清理或关闭。",
            CanExecuteDirectly = false
        };
    }

    private static IReadOnlyList<InstallSnapshotEligibleActionViewModel> BuildEligibleActions(
        IReadOnlyList<InstallSnapshotCDriveReviewItemViewModel> cDriveItems,
        IReadOnlyList<InstallSnapshotBackgroundReviewItemViewModel> backgroundItems,
        InstallFootprintCaptureStatus footprintStatus)
    {
        var actions = new List<InstallSnapshotEligibleActionViewModel>();
        var cacheCount = cDriveItems.Count(item =>
            item.Kind is InstallSnapshotCDriveContentKind.Cache or InstallSnapshotCDriveContentKind.Logs);
        var storageCount = cDriveItems.Count(item =>
            item.Kind is InstallSnapshotCDriveContentKind.Configuration or InstallSnapshotCDriveContentKind.ModelOrData);
        var installCount = cDriveItems.Count(item => item.Kind == InstallSnapshotCDriveContentKind.InstallFiles);
        var startupCount = backgroundItems.Count(item => item.Kind == InstallSnapshotBackgroundKind.Startup);
        var observeCount = cDriveItems.Count(item => item.Kind == InstallSnapshotCDriveContentKind.Unknown)
            + backgroundItems.Count(item =>
                item.Kind is InstallSnapshotBackgroundKind.Service or InstallSnapshotBackgroundKind.ScheduledTask);
        var footprintIncomplete = footprintStatus != InstallFootprintCaptureStatus.Complete;

        if (cacheCount > 0)
        {
            actions.Add(new InstallSnapshotEligibleActionViewModel
            {
                Kind = InstallSnapshotEligibleActionKind.CacheCleanupPlan,
                Title = "生成可回滚的缓存清理方案",
                Reason = "这些内容可能是缓存或日志，但仍需要先确认是否正在被软件使用。",
                EvidenceSummary = $"发现 {cacheCount} 个可能的缓存/日志位置。",
                NextEvidenceNeeded = "还需要确认软件已关闭、内容持续增长，并准备隔离区回滚清单。",
                SafetyLabel = "只能先生成清理方案；未确认前不会移动或删除。",
                RequiresUserConfirmation = true,
                RequiresRollback = true,
                CanExecuteDirectly = false
            });
        }

        if (storageCount > 0)
        {
            actions.Add(new InstallSnapshotEligibleActionViewModel
            {
                Kind = InstallSnapshotEligibleActionKind.StorageSettingGuidance,
                Title = "查看软件里的存储位置设置",
                Reason = "配置、模型和数据应优先通过软件自带设置改位置。",
                EvidenceSummary = $"发现 {storageCount} 个可能的配置/模型/数据位置。",
                NextEvidenceNeeded = "还需要确认该软件是否提供数据、模型或下载目录设置。",
                SafetyLabel = "当前只提供设置引导，不直接移动数据。",
                RequiresUserConfirmation = false,
                RequiresRollback = false,
                CanExecuteDirectly = false
            });
        }

        if (installCount > 0)
        {
            actions.Add(new InstallSnapshotEligibleActionViewModel
            {
                Kind = InstallSnapshotEligibleActionKind.ReinstallOrMigrationPlan,
                Title = "评估重装或迁移到 D 盘",
                Reason = "程序文件不能直接拖走，需要判断安全迁移、只迁数据，还是重新安装。",
                EvidenceSummary = $"发现 {installCount} 个可能的程序安装位置。",
                NextEvidenceNeeded = "还需要快照、回滚清单、关闭软件/服务，并确认迁移后的验证步骤。",
                SafetyLabel = "没有快照和回滚证据时，不允许迁移或重装。",
                RequiresUserConfirmation = true,
                RequiresRollback = true,
                CanExecuteDirectly = false
            });
        }

        if (startupCount > 0)
        {
            actions.Add(new InstallSnapshotEligibleActionViewModel
            {
                Kind = InstallSnapshotEligibleActionKind.StartupDisablePlan,
                Title = "评估是否关闭开机启动",
                Reason = "开机启动可能只是为了快速启动或更新，也可能承载必需功能。",
                EvidenceSummary = $"发现 {startupCount} 项新的开机启动。",
                NextEvidenceNeeded = "还需要确认用途、启动影响，并准备恢复原启动状态的步骤。",
                SafetyLabel = "只能先生成禁用方案；不会直接改启动项。",
                RequiresUserConfirmation = true,
                RequiresRollback = true,
                CanExecuteDirectly = false
            });
        }

        if (observeCount > 0 || actions.Count == 0 || footprintIncomplete)
        {
            var noPressure = cDriveItems.Count == 0
                && backgroundItems.Count == 0
                && !footprintIncomplete;
            actions.Add(new InstallSnapshotEligibleActionViewModel
            {
                Kind = InstallSnapshotEligibleActionKind.ObserveOnly,
                Title = "先继续观察",
                Reason = footprintIncomplete
                    ? "C 盘落地点观察被截断或有位置无法读取，不能把空结果当成没有写入。"
                    : noPressure
                        ? "没有发现新的 C 盘位置或后台项，当前不需要处理。"
                        : "还有用途不明的位置、服务或定时任务，直接处理风险较高。",
                EvidenceSummary = footprintIncomplete
                    ? $"落地点证据不完整；目前另有 {observeCount} 项需要观察或人工确认。"
                    : noPressure
                        ? "当前没有需要继续分类的新证据。"
                        : $"有 {observeCount} 项证据仍需要观察或人工确认。",
                NextEvidenceNeeded = footprintIncomplete
                    ? "重新完成安装前后落地点观察，再结合下次快照确认增长。"
                    : "等待下次快照的增长证据，或确认软件官方说明。",
                SafetyLabel = footprintIncomplete
                    ? "证据补齐前不会生成可执行处理动作。"
                    : "观察不会改动系统。",
                RequiresUserConfirmation = false,
                RequiresRollback = false,
                CanExecuteDirectly = false
            });
        }

        return actions;
    }

    private static InstallSnapshotCDriveReviewItemViewModel CreateCDriveItem(string path, int index)
    {
        var kind = ClassifyCDrivePath(path);
        var (label, purpose, advice, confidence) = DescribeCDriveKind(kind);
        return new InstallSnapshotCDriveReviewItemViewModel
        {
            Index = index,
            DisplayName = $"C 盘新增位置 {index}",
            Kind = kind,
            KindLabel = label,
            Purpose = purpose,
            Advice = advice,
            ConfidenceLabel = confidence,
            CanExecuteDirectly = false
        };
    }

    private static InstallSnapshotCDriveContentKind ClassifyCDrivePath(string path)
    {
        var normalized = (path ?? string.Empty).Replace('/', '\\').Trim().ToLowerInvariant();
        var segments = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Any(segment => segment is "cache" or "caches" or "code cache" or "gpucache" or "temp" or "tmp"))
            return InstallSnapshotCDriveContentKind.Cache;

        if (segments.Any(segment => segment is "config" or "configuration" or "settings" or "preferences"))
            return InstallSnapshotCDriveContentKind.Configuration;

        if (segments.Any(segment => segment is "log" or "logs"))
            return InstallSnapshotCDriveContentKind.Logs;

        if (segments.Any(segment => segment is "model" or "models" or "data" or "database" or "databases") ||
            segments.Any(segment => segment.EndsWith(".db", StringComparison.OrdinalIgnoreCase)))
        {
            return InstallSnapshotCDriveContentKind.ModelOrData;
        }

        if (segments.Any(segment => segment is "program files" or "program files (x86)" or "programs" or "install" or "application"))
            return InstallSnapshotCDriveContentKind.InstallFiles;

        return InstallSnapshotCDriveContentKind.Unknown;
    }

    private static (string Label, string Purpose, string Advice, string Confidence) DescribeCDriveKind(
        InstallSnapshotCDriveContentKind kind) =>
        kind switch
        {
            InstallSnapshotCDriveContentKind.InstallFiles => (
                "安装文件",
                "可能是软件运行必需的程序文件。",
                "不要直接移动或删除，应优先评估重装或安全迁移。",
                "初步判断：较高把握"),
            InstallSnapshotCDriveContentKind.Cache => (
                "缓存",
                "可能是软件为了加快加载而产生的临时内容。",
                "先观察增长，再生成可回滚的缓存清理方案。",
                "初步判断：较高把握"),
            InstallSnapshotCDriveContentKind.Configuration => (
                "配置",
                "可能保存账号、偏好或软件设置。",
                "建议保留；迁移前需确认软件是否支持更改数据位置。",
                "初步判断：较高把握"),
            InstallSnapshotCDriveContentKind.Logs => (
                "日志",
                "可能记录软件运行或故障信息。",
                "先确认近期是否需要排查问题，再生成日志清理方案。",
                "初步判断：较高把握"),
            InstallSnapshotCDriveContentKind.ModelOrData => (
                "模型/数据",
                "可能是模型、数据库或需要长期保留的内容。",
                "优先查找软件内置的存储位置设置，不要强行移动。",
                "初步判断：较高把握"),
            _ => (
                "待确认",
                "暂时无法仅根据位置判断用途。",
                "先保留并收集增长证据，不要直接处理。",
                "初步判断：待确认")
        };

    private static IReadOnlyList<InstallSnapshotBackgroundReviewItemViewModel> CreateBackgroundItems(
        InstallSnapshotDiffReport report)
    {
        var items = new List<InstallSnapshotBackgroundReviewItemViewModel>();
        AddBackgroundItems(items, report.NewStartupEntries, InstallSnapshotBackgroundKind.Startup);
        AddBackgroundItems(items, report.NewServices, InstallSnapshotBackgroundKind.Service);
        AddBackgroundItems(items, report.NewScheduledTasks, InstallSnapshotBackgroundKind.ScheduledTask);
        return items;
    }

    private static void AddBackgroundItems(
        List<InstallSnapshotBackgroundReviewItemViewModel> items,
        IReadOnlyList<string> values,
        InstallSnapshotBackgroundKind kind)
    {
        foreach (var value in values)
        {
            var index = items.Count + 1;
            var kindLabel = kind switch
            {
                InstallSnapshotBackgroundKind.Startup => "开机启动",
                InstallSnapshotBackgroundKind.Service => "后台服务",
                _ => "定时任务"
            };
            var riskLabel = kind == InstallSnapshotBackgroundKind.Service
                ? "风险较高：关闭可能影响核心功能"
                : "中等风险：改动前需确认用途";

            items.Add(new InstallSnapshotBackgroundReviewItemViewModel
            {
                Index = index,
                DisplayName = $"新增{kindLabel} {index}",
                Kind = kind,
                KindLabel = kindLabel,
                LikelyPurpose = InferBackgroundPurpose(value, kind),
                Advice = kind == InstallSnapshotBackgroundKind.Service
                    ? "先保持现状，确认是否是软件必需功能。"
                    : "先确认是否必要，需要改动时再生成处理方案。",
                RiskLabel = riskLabel,
                ConfidenceLabel = "初步判断：仅根据名称和机制类型",
                CanExecuteDirectly = false
            });
        }
    }

    private static string InferBackgroundPurpose(string value, InstallSnapshotBackgroundKind kind)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Contains("update", StringComparison.Ordinal) || normalized.Contains("更新", StringComparison.Ordinal))
            return "可能用于自动更新。";

        if (normalized.Contains("sync", StringComparison.Ordinal) || normalized.Contains("同步", StringComparison.Ordinal))
            return "可能用于数据同步。";

        return kind switch
        {
            InstallSnapshotBackgroundKind.Startup => "可能用于快速启动或登录后自动运行。",
            InstallSnapshotBackgroundKind.Service => "可能用于后台核心功能或持续监听。",
            _ => "可能用于定时更新或维护。"
        };
    }

    private static string BuildSummary(
        IReadOnlyList<InstallSnapshotCDriveReviewItemViewModel> cDriveItems,
        IReadOnlyList<InstallSnapshotBackgroundReviewItemViewModel> backgroundItems,
        InstallFootprintCaptureStatus footprintStatus)
    {
        var parts = new List<string>();
        AddCDriveSummary(parts, cDriveItems, InstallSnapshotCDriveContentKind.InstallFiles, "安装文件");
        AddCDriveSummary(parts, cDriveItems, InstallSnapshotCDriveContentKind.Cache, "缓存");
        AddCDriveSummary(parts, cDriveItems, InstallSnapshotCDriveContentKind.Configuration, "配置");
        AddCDriveSummary(parts, cDriveItems, InstallSnapshotCDriveContentKind.Logs, "日志");
        AddCDriveSummary(parts, cDriveItems, InstallSnapshotCDriveContentKind.ModelOrData, "模型/数据");
        AddCDriveSummary(parts, cDriveItems, InstallSnapshotCDriveContentKind.Unknown, "待确认");

        AddBackgroundSummary(parts, backgroundItems, InstallSnapshotBackgroundKind.Startup, "开机启动");
        AddBackgroundSummary(parts, backgroundItems, InstallSnapshotBackgroundKind.Service, "后台服务");
        AddBackgroundSummary(parts, backgroundItems, InstallSnapshotBackgroundKind.ScheduledTask, "定时任务");

        if (parts.Count == 0)
        {
            return footprintStatus == InstallFootprintCaptureStatus.Complete
                ? "Agent 初步判断：没有需要分类的新位置或后台项。"
                : "Agent 初步判断：C 盘落地点观察未完成，暂时不能判断没有新增位置。";
        }

        var summary = "Agent 初步判断：" + string.Join("、", parts) + "。";
        return footprintStatus == InstallFootprintCaptureStatus.Complete
            ? summary
            : summary + " C 盘落地点观察未完成，结果可能仍有遗漏。";
    }

    private static void AddCDriveSummary(
        List<string> parts,
        IReadOnlyList<InstallSnapshotCDriveReviewItemViewModel> items,
        InstallSnapshotCDriveContentKind kind,
        string label)
    {
        var count = items.Count(item => item.Kind == kind);
        if (count > 0)
            parts.Add($"{label} {count} 个");
    }

    private static void AddBackgroundSummary(
        List<string> parts,
        IReadOnlyList<InstallSnapshotBackgroundReviewItemViewModel> items,
        InstallSnapshotBackgroundKind kind,
        string label)
    {
        var count = items.Count(item => item.Kind == kind);
        if (count > 0)
            parts.Add($"{label} {count} 项");
    }
}

public static class InstallSnapshotCandidatePreviewPresenter
{
    public static InstallSnapshotCandidatePreviewViewModel Create(
        InstallSnapshotDiffReport report,
        InstallSnapshotEligibleActionKind kind)
    {
        if (report.CDriveFootprintStatus != InstallFootprintCaptureStatus.Complete
            && kind != InstallSnapshotEligibleActionKind.ObserveOnly)
        {
            return Refused(
                kind,
                "C 盘落地点观察未完成，暂时不能生成具体处理方案。",
                ["需要先重新完成安装前后落地点观察。"],
                null);
        }

        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);
        var candidate = review.EligibleActions.FirstOrDefault(action => action.Kind == kind);
        if (candidate is null)
        {
            return Refused(
                kind,
                "这次报告没有足够证据支持这种方案。",
                ["需要先在这次安装变化中找到对应的候选证据。"],
                null);
        }

        return kind switch
        {
            InstallSnapshotEligibleActionKind.CacheCleanupPlan => CreateCachePreview(report, review, candidate),
            InstallSnapshotEligibleActionKind.StorageSettingGuidance => CreateStorageGuidance(candidate),
            InstallSnapshotEligibleActionKind.ReinstallOrMigrationPlan => CreateMigrationPreview(report, review, candidate),
            InstallSnapshotEligibleActionKind.StartupDisablePlan => CreateStartupPreview(report, candidate),
            _ => CreateObservation(candidate)
        };
    }

    private static InstallSnapshotCandidatePreviewViewModel CreateCachePreview(
        InstallSnapshotDiffReport report,
        InstallSnapshotDiffEvidenceReviewViewModel review,
        InstallSnapshotEligibleActionViewModel candidate)
    {
        var profile = UniqueAddedSoftware(report);
        if (profile is null)
            return RefusedForUniqueSoftware(candidate);

        var cachePaths = review.CDriveItems
            .Where(item => item.Kind is InstallSnapshotCDriveContentKind.Cache or InstallSnapshotCDriveContentKind.Logs)
            .Select(item => report.NewCDrivePaths[item.Index - 1])
            .ToArray();
        if (cachePaths.Length == 0 || cachePaths.Any(path => !ProfileOwnsPath(profile, path)))
        {
            return Refused(
                candidate.Kind,
                "这些缓存/日志位置还不能安全归属到唯一新软件。",
                ["需要软件画像确认每个候选位置确实属于同一个软件。"],
                candidate);
        }

        var source = AppCacheCleanupPreviewPresenter.Create(new SoftwareProfile
        {
            Name = profile.Name,
            Publisher = profile.Publisher,
            Category = profile.Category,
            CachePaths = cachePaths,
            CacheSizeBytes = profile.CacheSizeBytes
        });
        return Ready(
            candidate,
            "缓存清理方案预览",
            source.Summary,
            "Agent 判断：证据可以归属到一个新软件，可以先看可回滚的清理方案。",
            source.Lines,
            requiresSnapshot: false,
            profile.Name,
            "打开对应应用并审核缓存");
    }

    private static InstallSnapshotCandidatePreviewViewModel CreateStartupPreview(
        InstallSnapshotDiffReport report,
        InstallSnapshotEligibleActionViewModel candidate)
    {
        var profile = UniqueAddedSoftware(report);
        if (profile is null)
            return RefusedForUniqueSoftware(candidate);

        if (report.NewStartupEntries.Count == 0 ||
            report.NewStartupEntries.Any(entry =>
                !profile.StartupEntries.Any(value => Same(value, entry))))
        {
            return Refused(
                candidate.Kind,
                "新开机启动还不能安全归属到唯一新软件。",
                ["需要软件画像确认这些开机启动确实由该软件创建。"],
                candidate);
        }

        var source = AppStartupControlPreviewPresenter.Create(new SoftwareProfile
        {
            Name = profile.Name,
            Publisher = profile.Publisher,
            Category = profile.Category,
            StartupEntries = report.NewStartupEntries
        });
        return Ready(
            candidate,
            "开机启动方案预览",
            source.Summary,
            "Agent 判断：只预览该软件的新开机启动，不会动服务或定时任务。",
            source.Lines,
            requiresSnapshot: true,
            profile.Name,
            "打开对应应用并审核自启动");
    }

    private static InstallSnapshotCandidatePreviewViewModel CreateMigrationPreview(
        InstallSnapshotDiffReport report,
        InstallSnapshotDiffEvidenceReviewViewModel review,
        InstallSnapshotEligibleActionViewModel candidate)
    {
        var profile = UniqueAddedSoftware(report);
        if (profile is null)
            return RefusedForUniqueSoftware(candidate);

        var installPaths = review.CDriveItems
            .Where(item => item.Kind == InstallSnapshotCDriveContentKind.InstallFiles)
            .Select(item => report.NewCDrivePaths[item.Index - 1])
            .ToArray();
        if (string.IsNullOrWhiteSpace(profile.InstallPath) ||
            installPaths.Length == 0 ||
            !installPaths.Any(path => Same(path, profile.InstallPath)))
        {
            return Refused(
                candidate.Kind,
                "还没有证据证明新的 C 盘程序位置就是该软件的安装目录。",
                ["需要唯一新软件的安装路径与新程序位置一致。"],
                candidate);
        }

        var source = MigrationPlanPresentationBuilder.Create(profile);
        var lines = new List<string> { source.ScoreLine };
        lines.AddRange(source.BlockingReasons);
        lines.Add(source.FinalReminder);
        return Ready(
            candidate,
            "重装/迁移方案预览",
            source.Summary,
            "Agent 判断：可以展示迁移风险和准备项，但不展示或执行原始路径操作。",
            lines,
            source.RequiresSnapshot,
            profile.Name,
            "打开对应应用并审核迁移");
    }

    private static InstallSnapshotCandidatePreviewViewModel CreateStorageGuidance(
        InstallSnapshotEligibleActionViewModel candidate) =>
        Guidance(
            candidate,
            "存储位置设置引导",
            "先从软件自带设置里改模型、数据或下载位置。",
            [
                "打开软件的设置，查找存储、下载、模型或数据目录。",
                "如果软件支持，优先改到 D 盘并重启软件验证。",
                "如果没有内置设置，再评估重装或迁移方案，不手动拖走数据。"
            ]);

    private static InstallSnapshotCandidatePreviewViewModel CreateObservation(
        InstallSnapshotEligibleActionViewModel candidate) =>
        Guidance(
            candidate,
            "继续观察方案",
            "当前证据不支持系统修改，先看下次快照是否继续增长。",
            [
                "保留这次安装变化作为对比基线。",
                "下次体检时检查 C 盘占用和后台项是否继续增长。",
                "只有用途和影响更明确后，才生成其他处理方案。"
            ]);

    private static InstallSnapshotCandidatePreviewViewModel Ready(
        InstallSnapshotEligibleActionViewModel candidate,
        string title,
        string summary,
        string takeaway,
        IReadOnlyList<string> lines,
        bool requiresSnapshot,
        string targetAppName,
        string navigationLabel) =>
        new()
        {
            Kind = candidate.Kind,
            Status = InstallSnapshotCandidatePreviewStatus.Ready,
            StatusLabel = "已生成只读预览",
            Title = title,
            Summary = summary,
            AgentTakeaway = takeaway,
            Lines = lines,
            MissingEvidence = [candidate.NextEvidenceNeeded],
            SafetyBoundary = "安全边界：这只是预览，尚未执行任何系统修改。",
            TargetAppName = targetAppName,
            NavigationLabel = navigationLabel,
            CanNavigateToApp = true,
            RequiresUserConfirmation = candidate.RequiresUserConfirmation,
            RequiresSnapshot = requiresSnapshot,
            RequiresRollback = candidate.RequiresRollback,
            CanExecuteDirectly = false
        };

    private static InstallSnapshotCandidatePreviewViewModel Guidance(
        InstallSnapshotEligibleActionViewModel candidate,
        string title,
        string summary,
        IReadOnlyList<string> lines) =>
        new()
        {
            Kind = candidate.Kind,
            Status = InstallSnapshotCandidatePreviewStatus.GuidanceOnly,
            StatusLabel = "只读引导",
            Title = title,
            Summary = summary,
            AgentTakeaway = "Agent 判断：现在只需要引导或观察，不需要进入系统操作。",
            Lines = lines,
            MissingEvidence = [candidate.NextEvidenceNeeded],
            SafetyBoundary = "安全边界：只展示步骤，不会修改存储设置或系统状态。",
            TargetAppName = null,
            NavigationLabel = string.Empty,
            CanNavigateToApp = false,
            RequiresUserConfirmation = false,
            RequiresSnapshot = false,
            RequiresRollback = false,
            CanExecuteDirectly = false
        };

    private static InstallSnapshotCandidatePreviewViewModel RefusedForUniqueSoftware(
        InstallSnapshotEligibleActionViewModel candidate) =>
        Refused(
            candidate.Kind,
            "不能安全生成这个预览，因为这次变化没有识别到唯一新软件。",
            ["需要唯一新软件画像，才能确认证据归属。"],
            candidate);

    private static InstallSnapshotCandidatePreviewViewModel Refused(
        InstallSnapshotEligibleActionKind kind,
        string summary,
        IReadOnlyList<string> missingEvidence,
        InstallSnapshotEligibleActionViewModel? candidate) =>
        new()
        {
            Kind = kind,
            Status = InstallSnapshotCandidatePreviewStatus.Refused,
            StatusLabel = "证据不足，已拒绝预览",
            Title = CandidateTitle(kind),
            Summary = summary,
            AgentTakeaway = "Agent 判断：证据无法安全归属，不会猜测或跳过检查。",
            Lines = ["这个预览已停在证据检查阶段。"],
            MissingEvidence = missingEvidence,
            SafetyBoundary = "安全边界：已拒绝生成具体处理方案，也不会执行系统修改。",
            TargetAppName = null,
            NavigationLabel = string.Empty,
            CanNavigateToApp = false,
            RequiresUserConfirmation = candidate?.RequiresUserConfirmation ?? false,
            RequiresSnapshot = false,
            RequiresRollback = candidate?.RequiresRollback ?? false,
            CanExecuteDirectly = false
        };

    private static SoftwareProfile? UniqueAddedSoftware(InstallSnapshotDiffReport report) =>
        report.AddedSoftware.Count == 1 ? report.AddedSoftware[0] : null;

    private static bool ProfileOwnsPath(SoftwareProfile profile, string path) =>
        (!string.IsNullOrWhiteSpace(profile.InstallPath) && Same(profile.InstallPath, path))
        || profile.CDriveWritePaths.Any(value => Same(value, path))
        || profile.CachePaths.Any(value => Same(value, path))
        || profile.LogPaths.Any(value => Same(value, path));

    private static bool Same(string? left, string? right) =>
        string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static string CandidateTitle(InstallSnapshotEligibleActionKind kind) =>
        kind switch
        {
            InstallSnapshotEligibleActionKind.CacheCleanupPlan => "缓存清理方案预览",
            InstallSnapshotEligibleActionKind.StorageSettingGuidance => "存储位置设置引导",
            InstallSnapshotEligibleActionKind.ReinstallOrMigrationPlan => "重装/迁移方案预览",
            InstallSnapshotEligibleActionKind.StartupDisablePlan => "开机启动方案预览",
            _ => "继续观察方案"
        };
}

public static class InstallSnapshotDiffActionPlanPresenter
{
    public static InstallSnapshotDiffActionPlanViewModel Create(InstallSnapshotDiffReport report)
    {
        var review = InstallSnapshotDiffEvidenceReviewPresenter.Create(report);
        var items = new List<InstallSnapshotDiffActionPlanItemViewModel>();
        var backgroundCount = report.NewStartupEntries.Count
            + report.NewServices.Count
            + report.NewScheduledTasks.Count;

        if (report.NewCDrivePaths.Count > 0)
        {
            items.Add(new InstallSnapshotDiffActionPlanItemViewModel
            {
                Order = items.Count + 1,
                Title = "先分清 C 盘新内容",
                Decision = "先保留，不删除，由 Agent 继续判断是安装文件、缓存还是配置。",
                Reason = "用途还没确定时直接删除，可能让软件无法正常启动。",
                EvidenceSummary = $"安装前后对比观察到 C 盘 {report.NewCDrivePaths.Count} 个新位置候选。",
                RiskLabel = "需要先确认用途",
                CanExecuteDirectly = false
            });
        }

        if (backgroundCount > 0)
        {
            items.Add(new InstallSnapshotDiffActionPlanItemViewModel
            {
                Order = items.Count + 1,
                Title = "再核实新后台项",
                Decision = "先保持现状，由 Agent 判断哪些是必需功能，哪些只是更新或快速启动。",
                Reason = "后台项可能影响开机速度，也可能是同步、安全或更新所必需。",
                EvidenceSummary = $"本次安装发现 {backgroundCount} 项后台变化。",
                RiskLabel = "关闭前需确认",
                CanExecuteDirectly = false
            });
        }

        if (items.Count == 0)
        {
            var footprintIncomplete = report.CDriveFootprintStatus
                != InstallFootprintCaptureStatus.Complete;
            items.Add(new InstallSnapshotDiffActionPlanItemViewModel
            {
                Order = 1,
                Title = "继续观察",
                Decision = footprintIncomplete
                    ? "先重新观察，不处理任何 C 盘内容或后台项。"
                    : "现在不用处理，下次体检时再看是否出现新增长。",
                Reason = footprintIncomplete
                    ? "C 盘落地点证据不完整，不能据此判断没有新增位置。"
                    : "这次完整对比没有发现新的 C 盘位置或后台项。",
                EvidenceSummary = footprintIncomplete
                    ? "本次观察被截断或有位置无法读取，需要补齐证据。"
                    : "当前没有需要立即处理的安装变化。",
                RiskLabel = footprintIncomplete ? "证据不完整" : "低风险",
                CanExecuteDirectly = false
            });
        }
        else
        {
            var footprintIncomplete = report.CDriveFootprintStatus
                != InstallFootprintCaptureStatus.Complete;
            items.Add(new InstallSnapshotDiffActionPlanItemViewModel
            {
                Order = items.Count + 1,
                Title = "继续观察是否增长",
                Decision = "观察下次体检的 C 盘增长和后台状态，再决定是否清缓存、迁移或关闭自启动。",
                Reason = footprintIncomplete
                    ? "落地点观察不完整，已发现的变化也不能代表全部结果。"
                    : "一次安装快照只能证明发生了变化，连续增长才更能说明问题。",
                EvidenceSummary = footprintIncomplete
                    ? "已记录当前候选，但需要完整复查后才能继续判断。"
                    : "本次变化已记录，可作为下次对比依据。",
                RiskLabel = footprintIncomplete ? "先补齐证据" : "建议观察",
                CanExecuteDirectly = false
            });
        }

        return new InstallSnapshotDiffActionPlanViewModel
        {
            Title = "Agent 处理方案",
            Summary = $"我建议先做 {items.Count} 件事，已经按安全顺序排好。",
            ReviewSummary = review.Summary,
            EvidenceReview = review,
            Items = items,
            SafetyBoundary = "这只是处理方案，尚未执行删除、迁移、关闭后台项或任何系统修改。真正动作必须再由你确认并进入本地安全管线。",
            RequiresUserConfirmation = true,
            CanExecuteDirectly = false
        };
    }
}

public static class InstallSnapshotDiffPresenter
{
    public static InstallSnapshotDiffViewModel Create(InstallSnapshotDiffReport report)
    {
        var backgroundCount = report.NewStartupEntries.Count
            + report.NewServices.Count
            + report.NewScheduledTasks.Count;

        return new InstallSnapshotDiffViewModel
        {
            Title = "\u5b89\u88c5\u53d8\u5316\u6458\u8981",
            Summary = BuildPlainSummary(report, backgroundCount),
            SafetyText = "\u8fd9\u4e2a\u9875\u9762\u53ea\u89e3\u91ca\u5b89\u88c5\u524d\u540e\u53d8\u5316\uff0c\u4e0d\u4f1a\u81ea\u52a8\u5904\u7406\u3001\u5378\u8f7d\u3001\u8fc1\u79fb\u6216\u7981\u7528\u4efb\u4f55\u5185\u5bb9\u3002",
            CanExecuteDirectly = false,
            Cards =
            [
                BuildSoftwareCard(report),
                BuildCDriveCard(report),
                BuildBackgroundCard(report, backgroundCount),
                BuildAgentCard(report, backgroundCount)
            ],
            TechnicalDetails = BuildTechnicalDetails(report)
        };
    }

    private static string BuildPlainSummary(InstallSnapshotDiffReport report, int backgroundCount)
    {
        var placement = InstallProgramPlacementAnalyzer.Create(report);
        var mainProgram = placement.Kind switch
        {
            InstallProgramPlacementKind.CDrive => "主程序位置：C 盘。",
            InstallProgramPlacementKind.DDrive => "主程序位置：D 盘。",
            InstallProgramPlacementKind.OtherOrUnknown => "主程序位置：尚未确认。",
            _ => "没有唯一新增软件，主程序位置尚未确认。"
        };
        var summary = $"新增软件 {report.AddedSoftware.Count} 个，C 盘新位置候选 {report.NewCDrivePaths.Count} 个，后台变化 {backgroundCount} 项。{mainProgram}";
        return report.CDriveFootprintStatus == InstallFootprintCaptureStatus.Complete
            ? summary
            : summary + " C 盘落地点观察未完成。";
    }

    private static InstallSnapshotDiffCardViewModel BuildSoftwareCard(InstallSnapshotDiffReport report)
    {
        var placement = InstallProgramPlacementAnalyzer.Create(report);
        var names = string.Join("\u3001", report.AddedSoftware.Select(profile => profile.Name));
        var body = report.AddedSoftware.Count switch
        {
            0 => "没有确认新增软件，因此还不能判断主程序装到哪里。",
            1 => $"新增软件 1 个：{names}。{MainProgramLocationSentence(placement.Kind)}",
            _ => $"新增软件 {report.AddedSoftware.Count} 个：{names}。主程序位置需要逐个查看，不能混在一起判断。"
        };

        return new InstallSnapshotDiffCardViewModel
        {
            Title = "\u88c5\u4e86\u4ec0\u4e48",
            Body = body,
            Detail = report.AddedSoftware.Count == 1
                ? "这里只说明主程序落点；缓存、配置、日志和模型可能使用别的位置。"
                : "需要先确认唯一新增软件，才能把安装位置和后续变化安全归到一起。"
        };
    }

    private static InstallSnapshotDiffCardViewModel BuildCDriveCard(InstallSnapshotDiffReport report)
    {
        var incomplete = report.CDriveFootprintStatus != InstallFootprintCaptureStatus.Complete;
        var placement = InstallProgramPlacementAnalyzer.Create(report);
        return new()
        {
            Title = "\u6709\u6ca1\u6709\u5199 C \u76d8",
            Body = incomplete
                ? report.NewCDrivePaths.Count == 0
                    ? "落地点观察未完成，暂时不能判断没有新增位置。"
                    : $"已观察到 {report.NewCDrivePaths.Count} 个新位置候选，但结果可能仍有遗漏。"
                : BuildCompleteCDriveBody(report, placement),
            Detail = incomplete
                ? "需要重新完成观察；已看到的位置只是同期变化候选，不一定由这个安装器产生。"
                : BuildCompleteCDriveDetail(report, placement)
        };
    }

    private static string MainProgramLocationSentence(InstallProgramPlacementKind kind) =>
        kind switch
        {
            InstallProgramPlacementKind.CDrive => "主程序装在 C 盘。",
            InstallProgramPlacementKind.DDrive => "主程序装在 D 盘。",
            _ => "主程序位置还不能确认。"
        };

    private static string BuildCompleteCDriveBody(
        InstallSnapshotDiffReport report,
        InstallProgramPlacementObservation placement)
    {
        if (placement.Kind == InstallProgramPlacementKind.DDrive)
        {
            if (placement.OwnedSeparateCDriveCandidateCount > 0)
            {
                return $"主程序在 D 盘；另有 {placement.OwnedSeparateCDriveCandidateCount} 个 C 盘数据或写入线索可以归到这个软件。";
            }

            return report.NewCDrivePaths.Count == 0
                ? "主程序在 D 盘；未观察到新增 C 盘位置。"
                : $"主程序在 D 盘；另有 {report.NewCDrivePaths.Count} 个同期 C 盘变化候选，但不能确认属于这个软件。";
        }

        if (placement.Kind == InstallProgramPlacementKind.CDrive)
        {
            return placement.OwnedSeparateCDriveCandidateCount > 0
                ? $"主程序在 C 盘；安装目录之外 {placement.OwnedSeparateCDriveCandidateCount} 个 C 盘数据或写入线索可以归到这个软件。"
                : "主程序在 C 盘；暂未确认安装目录之外还有属于它的 C 盘数据或写入线索。";
        }

        if (report.NewCDrivePaths.Count == 0)
            return "未观察到新增 C 盘位置，但主程序位置仍需确认。";

        return $"观察到 {report.NewCDrivePaths.Count} 个 C 盘新位置候选，但没有唯一新增软件，不能归到某个主程序。";
    }

    private static string BuildCompleteCDriveDetail(
        InstallSnapshotDiffReport report,
        InstallProgramPlacementObservation placement)
    {
        if (placement.Kind == InstallProgramPlacementKind.DDrive
            && placement.OwnedSeparateCDriveCandidateCount > 0)
        {
            return "装在 D 盘不等于不会写 C 盘；不需要重复迁移主程序，应分别复查缓存、配置、日志或模型位置。";
        }

        if (placement.Kind == InstallProgramPlacementKind.DDrive
            && report.NewCDrivePaths.Count > 0)
        {
            return "不要把同期变化直接当成这个软件的数据；需要应用画像或后续增长证据确认归属。";
        }

        if (placement.Kind == InstallProgramPlacementKind.CDrive)
            return "先分别判断主程序文件和安装目录之外的数据；不要把它们当成同一种迁移或清理对象。";

        return report.NewCDrivePaths.Count == 0
            ? "完整观察未发现新的 C 盘位置，但这不能替代主程序位置确认。"
            : "技术路径放在下方详情里；候选不等于已经确认由这个安装器产生。";
    }

    private static InstallSnapshotDiffCardViewModel BuildBackgroundCard(
        InstallSnapshotDiffReport report,
        int backgroundCount) =>
        new()
        {
            Title = "\u6709\u6ca1\u6709\u65b0\u589e\u540e\u53f0\u9879",
            Body = backgroundCount == 0
                ? "\u672a\u53d1\u73b0\u65b0\u589e\u81ea\u542f\u52a8\u3001\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u3002"
                : $"\u53d1\u73b0 {backgroundCount} \u9879\u65b0\u540e\u53f0\u53d8\u5316\uff1a\u81ea\u542f\u52a8 {report.NewStartupEntries.Count} \u9879\uff0c\u670d\u52a1 {report.NewServices.Count} \u9879\uff0c\u8ba1\u5212\u4efb\u52a1 {report.NewScheduledTasks.Count} \u9879\u3002",
            Detail = "\u540e\u53f0\u9879\u4e0d\u4ee3\u8868\u4e00\u5b9a\u6709\u95ee\u9898\uff0c\u4f46\u5e94\u8be5\u8ba9 Agent \u89e3\u91ca\u662f\u5426\u9700\u8981\u4fdd\u7559\u3002"
        };

    private static InstallSnapshotDiffCardViewModel BuildAgentCard(
        InstallSnapshotDiffReport report,
        int backgroundCount)
    {
        var incomplete = report.CDriveFootprintStatus != InstallFootprintCaptureStatus.Complete;
        var needsReview = report.NewCDrivePaths.Count > 0 || backgroundCount > 0 || incomplete;
        return new InstallSnapshotDiffCardViewModel
        {
            Title = "Agent \u5efa\u8bae",
            Body = incomplete
                ? "先补齐 C 盘落地点观察；证据完整前只解释，不生成可执行处理动作。"
                : needsReview
                    ? "\u5148\u67e5\u770b\u53d8\u5316\u6458\u8981\uff0c\u518d\u51b3\u5b9a\u662f\u4fdd\u7559\u3001\u6e05\u7f13\u5b58\u3001\u8fc1\u79fb\u8fd8\u662f\u89c2\u5bdf\u3002"
                : "\u6ca1\u6709\u53d1\u73b0\u660e\u663e\u9700\u8981\u7acb\u523b\u5904\u7406\u7684\u5b89\u88c5\u53d8\u5316\u3002",
            Detail = "\u4e0b\u4e00\u6b65\u4ecd\u7136\u53ea\u80fd\u751f\u6210\u65b9\u6848\uff1b\u771f\u6b63\u5904\u7406\u5fc5\u987b\u7ecf\u8fc7\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002"
        };
    }

    private static IReadOnlyList<string> BuildTechnicalDetails(InstallSnapshotDiffReport report)
    {
        var lines = new List<string>
        {
            report.Summary,
            $"Before: {report.BeforeCapturedAt:yyyy-MM-dd HH:mm:ss}",
            $"After: {report.AfterCapturedAt:yyyy-MM-dd HH:mm:ss}",
            $"C-drive footprint status: {report.CDriveFootprintStatus}",
            string.Empty
        };

        AddProfileLines(lines, "Added software", report.AddedSoftware);
        AddTextLines(lines, "New startup entries", report.NewStartupEntries);
        AddTextLines(lines, "New services", report.NewServices);
        AddTextLines(lines, "New scheduled tasks", report.NewScheduledTasks);
        AddTextLines(lines, "New C-drive paths", report.NewCDrivePaths);
        return lines;
    }

    private static void AddProfileLines(List<string> lines, string title, IReadOnlyList<SoftwareProfile> profiles)
    {
        lines.Add(title + ":");
        if (profiles.Count == 0)
        {
            lines.Add("  none");
            lines.Add(string.Empty);
            return;
        }

        foreach (var profile in profiles)
        {
            lines.Add($"  - {profile.Name} / {profile.InstallPath}");
        }

        lines.Add(string.Empty);
    }

    private static void AddTextLines(List<string> lines, string title, IReadOnlyList<string> values)
    {
        lines.Add(title + ":");
        if (values.Count == 0)
        {
            lines.Add("  none");
            lines.Add(string.Empty);
            return;
        }

        foreach (var value in values)
        {
            lines.Add("  - " + value);
        }

        lines.Add(string.Empty);
    }
}

public static class InstallSnapshotDiffBuilder
{
    public static InstallSnapshotDiffReport Build(InstallSystemSnapshot before, InstallSystemSnapshot after)
    {
        var beforeProfiles = before.SoftwareProfiles ?? [];
        var afterProfiles = after.SoftwareProfiles ?? [];

        var beforeProfileKeys = new HashSet<string>(
            beforeProfiles.Select(ProfileKey),
            StringComparer.OrdinalIgnoreCase);

        var addedSoftware = DistinctProfiles(
            afterProfiles.Where(profile => !beforeProfileKeys.Contains(ProfileKey(profile))));

        var newStartupEntries = FindNewValues(
            beforeProfiles.SelectMany(profile => profile.StartupEntries),
            afterProfiles.SelectMany(profile => profile.StartupEntries));
        var newServices = FindNewValues(
            beforeProfiles.SelectMany(profile => profile.Services),
            afterProfiles.SelectMany(profile => profile.Services));
        var newScheduledTasks = FindNewValues(
            beforeProfiles.SelectMany(profile => profile.ScheduledTasks),
            afterProfiles.SelectMany(profile => profile.ScheduledTasks));
        var inventoryCDrivePaths = FindNewValues(
            beforeProfiles.SelectMany(CDrivePathsFor),
            afterProfiles.SelectMany(CDrivePathsFor));
        var beforeFootprint = before.CDriveFootprint ?? InstallFootprintCapture.EmptyComplete;
        var afterFootprint = after.CDriveFootprint ?? InstallFootprintCapture.EmptyComplete;
        var footprintStatus = CombineFootprintStatus(beforeFootprint.Status, afterFootprint.Status);
        var footprintCDrivePaths = footprintStatus == InstallFootprintCaptureStatus.Complete
            ? FindNewValues(beforeFootprint.Paths, afterFootprint.Paths)
            : [];
        var newCDrivePaths = FindNewValues(
            Array.Empty<string>(),
            inventoryCDrivePaths.Concat(footprintCDrivePaths));

        return new InstallSnapshotDiffReport
        {
            BeforeCapturedAt = before.CapturedAt,
            AfterCapturedAt = after.CapturedAt,
            AddedSoftware = addedSoftware,
            NewStartupEntries = newStartupEntries,
            NewServices = newServices,
            NewScheduledTasks = newScheduledTasks,
            NewCDrivePaths = newCDrivePaths,
            CDriveFootprintStatus = footprintStatus,
            HasCDriveWrites = newCDrivePaths.Count > 0,
            Summary = BuildSummary(
                addedSoftware,
                newStartupEntries,
                newServices,
                newScheduledTasks,
                newCDrivePaths,
                footprintStatus)
        };
    }

    private static InstallFootprintCaptureStatus CombineFootprintStatus(
        InstallFootprintCaptureStatus before,
        InstallFootprintCaptureStatus after)
    {
        if (before == InstallFootprintCaptureStatus.Unavailable
            || after == InstallFootprintCaptureStatus.Unavailable)
            return InstallFootprintCaptureStatus.Unavailable;

        return before == InstallFootprintCaptureStatus.Truncated
            || after == InstallFootprintCaptureStatus.Truncated
                ? InstallFootprintCaptureStatus.Truncated
                : InstallFootprintCaptureStatus.Complete;
    }

    private static IReadOnlyList<SoftwareProfile> DistinctProfiles(IEnumerable<SoftwareProfile> profiles)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<SoftwareProfile>();

        foreach (var profile in profiles)
        {
            if (seen.Add(ProfileKey(profile)))
            {
                result.Add(profile);
            }
        }

        return result;
    }

    private static IReadOnlyList<string> FindNewValues(IEnumerable<string> before, IEnumerable<string> after)
    {
        var beforeSet = new HashSet<string>(
            before.Select(NormalizeValue).Where(value => value.Length > 0),
            StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var value in after.Select(NormalizeValue).Where(value => value.Length > 0))
        {
            if (!beforeSet.Contains(value) && seen.Add(value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static IEnumerable<string> CDrivePathsFor(SoftwareProfile profile)
    {
        foreach (var path in profile.CDriveWritePaths)
        {
            if (IsCDrivePath(path))
            {
                yield return path;
            }
        }

        if (!string.IsNullOrWhiteSpace(profile.InstallPath) && IsCDrivePath(profile.InstallPath))
        {
            yield return profile.InstallPath;
        }
    }

    private static string BuildSummary(
        IReadOnlyList<SoftwareProfile> addedSoftware,
        IReadOnlyList<string> newStartupEntries,
        IReadOnlyList<string> newServices,
        IReadOnlyList<string> newScheduledTasks,
        IReadOnlyList<string> newCDrivePaths,
        InstallFootprintCaptureStatus footprintStatus)
    {
        if (addedSoftware.Count == 0
            && newStartupEntries.Count == 0
            && newServices.Count == 0
            && newScheduledTasks.Count == 0
            && newCDrivePaths.Count == 0)
        {
            return footprintStatus == InstallFootprintCaptureStatus.Complete
                ? "未发现安装后新增软件、自启动、服务、计划任务或 C 盘写入点。"
                : "C 盘落地点观察未完成，当前不能排除新增位置；未发现其他软件或后台变化。";
        }

        var softwareNames = addedSoftware.Count == 0
            ? string.Empty
            : "（" + string.Join("、", addedSoftware.Select(profile => profile.Name)) + "）";

        var summary = $"安装变化：新增软件 {addedSoftware.Count} 个{softwareNames}，新增自启动 {newStartupEntries.Count} 个，新增服务 {newServices.Count} 个，新增计划任务 {newScheduledTasks.Count} 个，新增 C 盘路径候选 {newCDrivePaths.Count} 个。";
        return footprintStatus == InstallFootprintCaptureStatus.Complete
            ? summary
            : summary + " C 盘落地点观察未完成，结果可能仍有遗漏。";
    }

    private static string ProfileKey(SoftwareProfile profile) =>
        NormalizeValue(profile.Name) + "|" + NormalizeValue(profile.InstallPath);

    private static bool IsCDrivePath(string path)
    {
        var value = NormalizeValue(path);
        return value.StartsWith(@"C:\", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("C:/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeValue(string? value) =>
        (value ?? string.Empty).Trim().Trim('"');
}
