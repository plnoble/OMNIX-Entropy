using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Css.Core.Agent;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;

namespace Css.Core.Apps;

public enum AppTileStatus
{
    Normal,
    Warning,
    Attention,
    System
}

public sealed class AppTileViewModel
{
    public required string Name { get; init; }
    public string? IconPath { get; init; }
    public int IconIndex { get; init; }
    public required SoftwareCategory Category { get; init; }
    public required AppTileStatus Status { get; init; }
    public required string ShortTag { get; init; }
    public required RiskLevel Risk { get; init; }
    public required string VisibleText { get; init; }
    public required string AccessibilityName { get; init; }
}

public sealed class AppDrawerViewModel
{
    public required string Name { get; init; }
    public required string CategorySummary { get; init; }
    public required string InstallLocationSummary { get; init; }
    public required string SizeSummary { get; init; }
    public required string ResidencySummary { get; init; }
    public required AgentRecommendation AgentAdvice { get; init; }
    public required IReadOnlyList<AppActionViewModel> AvailableActions { get; init; }
    public required AppResidueReviewAvailabilityViewModel UninstallResidueReview { get; init; }
    public required IReadOnlyList<string> UninstallPreviewLines { get; init; }
    public required string CacheCleanupSummary { get; init; }
    public required IReadOnlyList<string> CacheCleanupPreviewLines { get; init; }
    public required bool CacheCleanupCanExecuteDirectly { get; init; }
    public required string StartupControlSummary { get; init; }
    public required IReadOnlyList<string> StartupControlPreviewLines { get; init; }
    public required bool StartupControlCanExecuteDirectly { get; init; }
    public required string MigrationSummary { get; init; }
    public required IReadOnlyList<string> MigrationPreviewLines { get; init; }
    public required bool TechnicalDetailsHiddenByDefault { get; init; }
    public required IReadOnlyList<string> TechnicalDetails { get; init; }
}

public sealed class AppResidueReviewAvailabilityViewModel
{
    public required bool IsEnabled { get; init; }
    public required string Reason { get; init; }
}

public sealed class AppActionViewModel
{
    public required AppActionKind Kind { get; init; }
    public required string Label { get; init; }
    public required bool IsEnabled { get; init; }
    public required string Reason { get; init; }
}

public sealed class AppActionEntryDecision
{
    public required bool IsAllowed { get; init; }
    public required string Reason { get; init; }
}

public static class AppActionEntryPolicy
{
    public static AppActionEntryDecision Evaluate(
        AppDrawerViewModel drawer,
        AppActionKind kind)
    {
        ArgumentNullException.ThrowIfNull(drawer);
        var action = drawer.AvailableActions.FirstOrDefault(candidate => candidate.Kind == kind);
        return new AppActionEntryDecision
        {
            IsAllowed = action?.IsEnabled == true,
            Reason = action?.Reason ?? "当前应用没有这项可用操作。"
        };
    }
}

public enum AppActionKind
{
    Uninstall,
    Migration,
    CacheCleanup,
    StartupControl,
    TechnicalDetails
}

public enum AppCatalogFilter
{
    All,
    NormalApplications,
    DevelopmentTools,
    GamesEntertainment,
    SystemApps,
    CDrive,
    Resident,
    Uninstallable
}

public enum AppCatalogSort
{
    Risk,
    Name,
    Size,
    RecentInstall,
    RecentGrowth
}

public sealed class AppCatalogQuery
{
    public AppCatalogFilter Filter { get; init; } = AppCatalogFilter.All;
    public AppCatalogSort Sort { get; init; } = AppCatalogSort.Risk;
    public string? SearchText { get; init; }
    public int Limit { get; init; } = 120;
}

public static class AppCatalogPresenter
{
    public static IReadOnlyList<SoftwareProfile> Apply(
        IEnumerable<SoftwareProfile> profiles,
        AppCatalogQuery query)
    {
        var filtered = profiles
            .Where(profile => MatchesFilter(profile, query.Filter))
            .Where(profile => MatchesSearch(profile, query.SearchText));

        var sorted = query.Sort switch
        {
            AppCatalogSort.Name => filtered.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            AppCatalogSort.Size => filtered
                .OrderByDescending(TotalSizeBytes)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            AppCatalogSort.RecentInstall => filtered
                .OrderByDescending(p => p.InstallDate.HasValue)
                .ThenByDescending(p => p.InstallDate)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            AppCatalogSort.RecentGrowth => filtered
                .OrderByDescending(p => p.RecentGrowthBytes)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            _ => filtered
                .OrderByDescending(RiskOrder)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
        };

        return sorted
            .Take(query.Limit <= 0 ? 120 : query.Limit)
            .ToList();
    }

    private static bool MatchesFilter(SoftwareProfile profile, AppCatalogFilter filter) =>
        filter switch
        {
            AppCatalogFilter.NormalApplications => profile.Category == SoftwareCategory.Normal,
            AppCatalogFilter.DevelopmentTools => profile.Category == SoftwareCategory.DevelopmentTool,
            AppCatalogFilter.GamesEntertainment => profile.Category == SoftwareCategory.Game,
            AppCatalogFilter.SystemApps => profile.Category == SoftwareCategory.SystemTool,
            AppCatalogFilter.CDrive => AppPresentationBuilder.HasCDriveFootprint(profile),
            AppCatalogFilter.Resident => AppPresentationBuilder.IsResident(profile),
            AppCatalogFilter.Uninstallable => AppPresentationBuilder.CanReviewUninstall(profile),
            _ => true
        };

    private static bool MatchesSearch(SoftwareProfile profile, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || IsSearchPlaceholder(searchText))
            return true;

        return profile.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
            || (!string.IsNullOrWhiteSpace(profile.Publisher)
                && profile.Publisher.Contains(searchText, StringComparison.CurrentCultureIgnoreCase));
    }

    private static bool IsSearchPlaceholder(string searchText)
    {
        var text = searchText.Trim();
        return text.Equals("Search apps", StringComparison.CurrentCultureIgnoreCase)
            || text.Equals("搜索应用", StringComparison.CurrentCultureIgnoreCase)
            || text.Equals("搜索软件", StringComparison.CurrentCultureIgnoreCase);
    }

    private static long TotalSizeBytes(SoftwareProfile profile) =>
        profile.InstalledSizeBytes + profile.DataSizeBytes + profile.CacheSizeBytes;

    private static int RiskOrder(SoftwareProfile profile) =>
        AppPresentationBuilder.CreateTile(profile).Status switch
        {
            AppTileStatus.Attention => 3,
            AppTileStatus.Warning => 2,
            AppTileStatus.Normal => 1,
            _ => 0
        };
}

public sealed class AppCatalogSummaryViewModel
{
    public required int TotalCount { get; init; }
    public required int VisibleCount { get; init; }
    public required int MainProgramOnCCount { get; init; }
    public required int MainProgramOnDCount { get; init; }
    public required int CDriveDataAppCount { get; init; }
    public required int CDriveFootprintCount { get; init; }
    public required int OrdinaryCDriveFootprintCount { get; init; }
    public required int SystemCDriveFootprintCount { get; init; }
    public required int OwnershipPendingCDriveFootprintCount { get; init; }
    public required int ResidentAppCount { get; init; }
    public required int OrdinaryResidentAppCount { get; init; }
    public required int SystemResidentAppCount { get; init; }
    public required int OwnershipPendingResidentAppCount { get; init; }
    public required int RunningAppCount { get; init; }
    public required int StartupAppCount { get; init; }
    public required int ServiceAppCount { get; init; }
    public required int ScheduledTaskAppCount { get; init; }
    public required string Text { get; init; }
}

public static class AppCatalogSummaryPresenter
{
    public static AppCatalogSummaryViewModel Create(
        IEnumerable<SoftwareProfile> profiles,
        int visibleCount)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        var items = profiles.ToList();
        var mainProgramOnC = items.Count(AppPresentationBuilder.IsMainProgramOnC);
        var mainProgramOnD = items.Count(AppPresentationBuilder.IsMainProgramOnD);
        var cDriveDataApps = items.Count(profile =>
            AppPresentationBuilder.CDriveDataLocationCount(profile) > 0);
        var cDriveOwnership = CDriveApplicationOwnershipCatalog.Create(items);
        var cDriveFootprint = cDriveOwnership.AllProfiles.Count;
        var backgroundOwnership = BackgroundApplicationOwnershipCatalog.Create(items);
        var running = items.Count(profile => profile.RunningProcesses.Count > 0);
        var startup = items.Count(profile => profile.StartupEntries.Count > 0);
        var services = items.Count(profile => profile.Services.Count > 0);
        var tasks = items.Count(profile => profile.ScheduledTasks.Count > 0);
        var visible = Math.Clamp(visibleCount, 0, items.Count);
        var backgroundParts = new List<string>();
        if (running > 0) backgroundParts.Add($"正在运行 {running} 个");
        if (startup > 0) backgroundParts.Add($"有自启动 {startup} 个");
        if (services > 0) backgroundParts.Add($"有后台服务 {services} 个");
        if (tasks > 0) backgroundParts.Add($"有计划任务 {tasks} 个");
        var backgroundText = backgroundOwnership.AllProfiles.Count == 0
            ? "暂未发现后台常驻线索。"
            : $"后台线索：{backgroundOwnership.BeginnerSummary}。其中{string.Join("，", backgroundParts)}。";

        return new AppCatalogSummaryViewModel
        {
            TotalCount = items.Count,
            VisibleCount = visible,
            MainProgramOnCCount = mainProgramOnC,
            MainProgramOnDCount = mainProgramOnD,
            CDriveDataAppCount = cDriveDataApps,
            CDriveFootprintCount = cDriveFootprint,
            OrdinaryCDriveFootprintCount = cDriveOwnership.OrdinaryProfiles.Count,
            SystemCDriveFootprintCount = cDriveOwnership.SystemProfiles.Count,
            OwnershipPendingCDriveFootprintCount = cDriveOwnership.OwnershipPendingProfiles.Count,
            ResidentAppCount = backgroundOwnership.AllProfiles.Count,
            OrdinaryResidentAppCount = backgroundOwnership.OrdinaryProfiles.Count,
            SystemResidentAppCount = backgroundOwnership.SystemProfiles.Count,
            OwnershipPendingResidentAppCount = backgroundOwnership.OwnershipPendingProfiles.Count,
            RunningAppCount = running,
            StartupAppCount = startup,
            ServiceAppCount = services,
            ScheduledTaskAppCount = tasks,
            Text = $"扫描到 {items.Count} 个应用：主程序在 C 盘 {mainProgramOnC} 个，D 盘 {mainProgramOnD} 个；另有 C 盘数据或缓存线索 {cDriveDataApps} 个；“占 C 盘”共 {cDriveFootprint} 个：{cDriveOwnership.BeginnerSummary}。{backgroundText} 当前显示 {visible} 个。"
        };
    }
}

public static class AppPresentationBuilder
{
    public static AppTileViewModel CreateTile(SoftwareProfile profile)
    {
        var status = DetermineStatus(profile);
        var shortTag = status switch
        {
            AppTileStatus.System => "\u7cfb\u7edf\u7ec4\u4ef6",
            AppTileStatus.Attention => CDriveAttentionTag(profile),
            AppTileStatus.Warning when profile.RecentGrowthBytes > 0 => "最近变大",
            AppTileStatus.Warning when IsResident(profile) => "\u540e\u53f0\u5e38\u9a7b",
            AppTileStatus.Warning => "\u6709\u5efa\u8bae",
            _ => "\u6b63\u5e38"
        };

        return new AppTileViewModel
        {
            Name = profile.Name,
            IconPath = profile.DisplayIconPath,
            IconIndex = profile.DisplayIconIndex,
            Category = profile.Category,
            Status = status,
            ShortTag = shortTag,
            Risk = status == AppTileStatus.Attention ? RiskLevel.Medium : RiskLevel.Low,
            VisibleText = profile.Name + " " + shortTag,
            AccessibilityName = profile.Name + ", " + shortTag
        };
    }

    private static string CDriveAttentionTag(SoftwareProfile profile)
    {
        if (RequiresSystemOwnershipReview(profile))
            return "系统归属待确认";

        if (IsOnDrive(profile.InstallPath, "C"))
            return "主程序在 C 盘";

        if (IsOnDrive(profile.InstallPath, "D")
            && profile.CDriveWritePaths.Any(path => IsOnDrive(path, "C")))
            return "数据写入 C 盘";

        return "C 盘线索待确认";
    }

    public static AppDrawerViewModel CreateDrawer(SoftwareProfile profile)
    {
        var requiresOwnershipReview = RequiresSystemOwnershipReview(profile);
        var cachePreview = AppCacheCleanupPreviewPresenter.Create(profile);
        var startupPreview = AppStartupControlPreviewPresenter.Create(profile);

        return new AppDrawerViewModel
        {
            Name = profile.Name,
            CategorySummary = CategorySummary(profile),
            InstallLocationSummary = LocationSummary(profile),
            SizeSummary = SizeSummary(profile),
            ResidencySummary = ResidencySummary(profile),
            AgentAdvice = CreateAgentAdvice(profile),
            AvailableActions = CreateActions(profile),
            UninstallResidueReview = CreateUninstallResidueReviewAvailability(profile),
            UninstallPreviewLines = CreateUninstallPreview(profile),
            CacheCleanupSummary = requiresOwnershipReview
                ? "系统归属待确认，暂不生成普通应用缓存方案。"
                : cachePreview.Summary,
            CacheCleanupPreviewLines = requiresOwnershipReview
                ? ["先确认它是否属于 Windows 管理组件；当前不会处理缓存。"]
                : cachePreview.Lines,
            CacheCleanupCanExecuteDirectly = cachePreview.CanExecuteDirectly,
            StartupControlSummary = requiresOwnershipReview
                ? "系统归属待确认，暂不生成普通应用自启动方案。"
                : startupPreview.Summary,
            StartupControlPreviewLines = requiresOwnershipReview
                ? ["先确认组件归属；当前不会关闭自启动、服务或计划任务。"]
                : startupPreview.Lines,
            StartupControlCanExecuteDirectly = startupPreview.CanExecuteDirectly,
            MigrationSummary = CreateMigrationSummary(profile),
            MigrationPreviewLines = CreateMigrationPreview(profile),
            TechnicalDetailsHiddenByDefault = true,
            TechnicalDetails = CreateTechnicalDetails(profile)
        };
    }

    public static bool IsResident(SoftwareProfile profile) =>
        profile.RunningProcesses.Count > 0
        || profile.Services.Count > 0
        || profile.StartupEntries.Count > 0
        || profile.ScheduledTasks.Count > 0;

    public static bool IsMainProgramOnC(SoftwareProfile profile) =>
        IsOnDrive(profile.InstallPath, "C");

    public static bool IsMainProgramOnD(SoftwareProfile profile) =>
        IsOnDrive(profile.InstallPath, "D");

    public static bool HasCDriveFootprint(SoftwareProfile profile) =>
        IsMainProgramOnC(profile) || CDriveDataLocationCount(profile) > 0;

    public static bool CanUseOrdinaryApplicationActions(SoftwareProfile profile) =>
        profile.Category != SoftwareCategory.SystemTool
        && !RequiresSystemOwnershipReview(profile);

    public static bool CanReviewUninstall(SoftwareProfile profile) =>
        CanUseOrdinaryApplicationActions(profile)
        && !string.IsNullOrWhiteSpace(profile.UninstallCommand);

    public static bool CanReviewMigrationClosure(SoftwareProfile profile) =>
        CanUseOrdinaryApplicationActions(profile);

    public static bool CanReviewMigration(SoftwareProfile profile) =>
        CanUseOrdinaryApplicationActions(profile)
        && !IsOnDrive(profile.InstallPath, "D")
        && (!string.IsNullOrWhiteSpace(profile.InstallPath)
            || profile.CachePaths.Count > 0
            || profile.DataPaths.Count > 0);

    public static AppResidueReviewAvailabilityViewModel CreateUninstallResidueReviewAvailability(
        SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
        {
            return new AppResidueReviewAvailabilityViewModel
            {
                IsEnabled = false,
                Reason = "这是系统组件；普通应用抽屉不会检查或处理它的卸载残留。"
            };
        }

        if (RequiresSystemOwnershipReview(profile))
        {
            return new AppResidueReviewAvailabilityViewModel
            {
                IsEnabled = false,
                Reason = "系统归属未确认，暂不检查或处理卸载残留。"
            };
        }

        return new AppResidueReviewAvailabilityViewModel
        {
            IsEnabled = true,
            Reason = "可在官方或外部卸载后复查；如果软件仍在，OMNIX 不会处理残留。"
        };
    }

    private static AppTileStatus DetermineStatus(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
            return AppTileStatus.System;
        if (RequiresSystemOwnershipReview(profile))
            return AppTileStatus.Attention;
        if (HasCDriveFootprint(profile))
            return AppTileStatus.Attention;
        if (profile.RecentGrowthBytes > 0)
            return AppTileStatus.Warning;
        if (IsResident(profile) || profile.DataSizeBytes >= 1024L * 1024 * 1024)
            return AppTileStatus.Warning;
        return AppTileStatus.Normal;
    }

    private static string LocationSummary(SoftwareProfile profile)
    {
        if (RequiresSystemOwnershipReview(profile))
            return "主程序位于 Windows 管理位置；系统归属待确认，暂不提供普通应用操作。";

        var cDriveDataLocations = CDriveDataLocationCount(profile);
        if (string.IsNullOrWhiteSpace(profile.InstallPath))
        {
            return cDriveDataLocations > 0
                ? $"主程序位置未知；仍发现 {cDriveDataLocations} 个 C 盘数据或缓存写入线索，先确认来源再决定。"
                : "主程序位置未知；暂未发现已归属的 C 盘写入线索。";
        }
        if (IsOnDrive(profile.InstallPath, "D"))
        {
            return cDriveDataLocations > 0
                ? $"主程序在 D 盘；但仍发现 {cDriveDataLocations} 个 C 盘数据或缓存写入线索。装在 D 盘不等于 C 盘不会增长。"
                : "主程序在 D 盘；暂未发现已归属的 C 盘写入线索。";
        }
        if (IsOnDrive(profile.InstallPath, "C"))
        {
            return cDriveDataLocations > 0
                ? $"主程序在 C 盘；另发现 {cDriveDataLocations} 个 C 盘数据或缓存写入线索，需要分别判断主程序和数据位置。"
                : "主程序在 C 盘；建议先评估主程序应迁移、重装还是保留。";
        }
        return cDriveDataLocations > 0
            ? $"主程序在非标准 C/D 盘位置；另有 {cDriveDataLocations} 个 C 盘数据或缓存写入线索，需要分别确认。"
            : "主程序在非标准 C/D 盘位置；需要先确认用途。";
    }

    private static string CategorySummary(SoftwareProfile profile)
    {
        var categoryLabel = CategoryLabel(profile.Category);
        var assessment = profile.CategoryAssessment;
        if (profile.Category == SoftwareCategory.Unknown)
            return "未知类型 · 扫描资料不足，OMNIX 不猜。";

        if (assessment.Category != profile.Category)
            return $"{categoryLabel} · 当前资料没有保留可靠的分类依据。";

        if (assessment.IsFallback)
            return "普通应用 · 没发现明确分类线索，先按普通应用展示。";

        var sources = assessment.Evidence
            .Select(evidence => evidence.Source switch
            {
                SoftwareCategoryEvidenceSource.ProductName => "应用名称",
                SoftwareCategoryEvidenceSource.Publisher => "发布者信息",
                SoftwareCategoryEvidenceSource.InstallLocation => "安装位置线索",
                _ => "扫描线索"
            })
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (sources.Count == 0)
            return $"{categoryLabel} · 当前资料没有保留分类依据。";

        var sourceText = sources.Count == 1
            ? sources[0]
            : string.Join("、", sources.Take(sources.Count - 1)) + "和" + sources[^1];
        var confidence = assessment.Confidence switch
        {
            SoftwareCategoryConfidence.High => "把握较高",
            SoftwareCategoryConfidence.Medium => "把握一般",
            SoftwareCategoryConfidence.Low => "仅供参考",
            _ => "尚未评估"
        };
        return $"{categoryLabel} · 根据{sourceText}判断（{confidence}）。";
    }

    private static string CategoryLabel(SoftwareCategory category) =>
        category switch
        {
            SoftwareCategory.Normal => "普通应用",
            SoftwareCategory.Game => "游戏娱乐",
            SoftwareCategory.Ai => "AI 工具",
            SoftwareCategory.DevelopmentTool => "开发工具",
            SoftwareCategory.SystemTool => "系统应用",
            _ => "未知类型"
        };

    private static string SizeSummary(SoftwareProfile profile)
    {
        var program = profile.InstalledSizeBytes > 0
            ? "主程序安装 " + FormatBytes(profile.InstalledSizeBytes)
            : "主程序大小未统计";
        var data = profile.DataSizeBytes > 0
            ? "数据 " + FormatBytes(profile.DataSizeBytes)
            : profile.DataPaths.Count > 0
                ? "数据位置已识别，大小未统计"
                : "未识别单独数据位置";
        var cache = profile.CacheSizeBytes > 0
            ? "可识别缓存 " + FormatBytes(profile.CacheSizeBytes)
            : profile.CachePaths.Count > 0
                ? "缓存位置已识别，大小未统计"
                : "未识别缓存位置";
        var growth = profile.RecentGrowthBytes > 0
            ? "最近增长 " + FormatBytes(profile.RecentGrowthBytes)
            : "最近增长暂无可用数值";

        return string.Join("；", program, data, cache, growth) + "。";
    }

    private static string ResidencySummary(SoftwareProfile profile)
    {
        if (!IsResident(profile))
            return "\u672a\u53d1\u73b0\u81ea\u542f\u52a8\u3001\u540e\u53f0\u670d\u52a1\u3001\u8ba1\u5212\u4efb\u52a1\u6216\u6b63\u5728\u8fd0\u884c\u7684\u8fdb\u7a0b\u3002";

        var parts = new List<string>();
        if (profile.RunningProcesses.Count > 0) parts.Add(profile.RunningProcesses.Count + " \u4e2a\u6b63\u5728\u8fd0\u884c\u7684\u8fdb\u7a0b");
        if (profile.Services.Count > 0) parts.Add("\u540e\u53f0\u670d\u52a1");
        if (profile.StartupEntries.Count > 0) parts.Add("\u81ea\u542f\u52a8\u9879");
        if (profile.ScheduledTasks.Count > 0) parts.Add("\u8ba1\u5212\u4efb\u52a1");
        return string.Join("\uff0c", parts);
    }

    private static AgentRecommendation CreateAgentAdvice(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
        {
            return new AgentRecommendation
            {
                Text = "这是系统相关应用，建议保留。OMNIX 不会从普通应用抽屉卸载、迁移、清理缓存或关闭它的后台组件。",
                Reason = "系统组件可能依赖 Windows 服务、驱动、更新或系统范围配置。",
                Risk = RiskLevel.Medium,
                RequiresUserConfirmation = false,
                Action = RecommendationAction.Keep
            };
        }

        if (RequiresSystemOwnershipReview(profile))
        {
            return new AgentRecommendation
            {
                Text = "这个程序位于 Windows 管理位置，但系统归属未确认。OMNIX 只允许查看详情，不会提供普通应用操作。",
                Reason = "类别证据不足时，卸载、迁移、缓存和后台项都需要先确认组件归属。",
                Risk = RiskLevel.Medium,
                RequiresUserConfirmation = false,
                Action = RecommendationAction.Observe
            };
        }

        var cDriveDataLocations = CDriveDataLocationCount(profile);
        if (IsOnDrive(profile.InstallPath, "D") && cDriveDataLocations > 0)
        {
            return new AgentRecommendation
            {
                Text = "主程序已经在 D 盘，不要重复迁移主程序。先确认 C 盘内容是缓存、日志、配置还是必要数据；只清理一次可能还会继续增长。",
                Reason = $"主程序在 D 盘，但仍发现 {cDriveDataLocations} 个 C 盘数据或缓存写入线索。",
                Risk = RiskLevel.Medium,
                RequiresUserConfirmation = true,
                Action = RecommendationAction.RepairInstallLocation
            };
        }

        if (string.IsNullOrWhiteSpace(profile.InstallPath) && cDriveDataLocations > 0)
        {
            return new AgentRecommendation
            {
                Text = "先确认主程序和数据分别属于哪里；主程序位置未知时不直接迁移，也不把所有 C 盘内容当成缓存。",
                Reason = $"主程序位置未知，但发现 {cDriveDataLocations} 个 C 盘数据或缓存写入线索。",
                Risk = RiskLevel.Medium,
                RequiresUserConfirmation = false,
                Action = RecommendationAction.Observe
            };
        }

        if (HasCDriveFootprint(profile))
        {
            return new AgentRecommendation
            {
                Text = "\u5148\u751f\u6210\u8fc1\u79fb\u65b9\u6848\uff0c\u4e0d\u8981\u76f4\u63a5\u79fb\u52a8\u6587\u4ef6\uff1b\u8fd9\u4e2a\u5e94\u7528\u4ecd\u5728 C \u76d8\u7559\u4e0b\u5360\u7528\u3002",
                Reason = "\u53d1\u73b0 C \u76d8\u5b89\u88c5\u4f4d\u7f6e\u6216\u5199\u5165\u8def\u5f84\u3002",
                Risk = RiskLevel.Medium,
                RequiresUserConfirmation = true,
                Action = RecommendationAction.Migrate
            };
        }

        if (IsResident(profile) || profile.DataSizeBytes >= 1024L * 1024 * 1024)
        {
            return new AgentRecommendation
            {
                Text = "\u5148\u89c2\u5bdf\uff1a\u5b83\u6ca1\u6709\u88c5\u5728 C \u76d8\uff0c\u4f46\u6709\u540e\u53f0\u6d3b\u52a8\u6216\u672c\u5730\u6570\u636e\u3002",
                Reason = "\u53d1\u73b0\u540e\u53f0\u8fdb\u7a0b\u3001\u670d\u52a1\u3001\u8ba1\u5212\u4efb\u52a1\u6216\u8f83\u5927\u7684\u672c\u5730\u6570\u636e\u3002",
                Risk = RiskLevel.Low,
                RequiresUserConfirmation = false,
                Action = RecommendationAction.Observe
            };
        }

        return new AgentRecommendation
        {
            Text = "\u76ee\u524d\u6b63\u5e38\uff0c\u4e0d\u9700\u8981\u5904\u7406\u3002",
            Reason = "\u6ca1\u6709\u53d1\u73b0\u660e\u663e C \u76d8\u5360\u7528\u6216\u540e\u53f0\u5e38\u9a7b\u95ee\u9898\u3002",
            Risk = RiskLevel.None,
            RequiresUserConfirmation = false,
            Action = RecommendationAction.Keep
        };
    }

    private static IReadOnlyList<AppActionViewModel> CreateActions(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
        {
            return
            [
                new() { Kind = AppActionKind.Uninstall, Label = "\u5378\u8f7d\u5e72\u51c0\u70b9", IsEnabled = false, Reason = "系统组件不从普通应用抽屉卸载。" },
                new() { Kind = AppActionKind.Migration, Label = "\u8fc1\u79fb\u5230 D \u76d8", IsEnabled = false, Reason = "系统组件不建议迁移。" },
                new() { Kind = AppActionKind.CacheCleanup, Label = "\u6e05\u7406\u7f13\u5b58", IsEnabled = false, Reason = "系统缓存需要专用规则，普通应用抽屉不会处理。" },
                new() { Kind = AppActionKind.StartupControl, Label = "\u7ba1\u7406\u81ea\u542f\u52a8", IsEnabled = false, Reason = "系统后台组件不从普通应用抽屉关闭。" },
                new() { Kind = AppActionKind.TechnicalDetails, Label = "\u6280\u672f\u8be6\u60c5", IsEnabled = true, Reason = "\u5c55\u5f00\u8def\u5f84\u3001\u670d\u52a1\u3001\u81ea\u542f\u52a8\u548c\u8ba1\u5212\u4efb\u52a1\u660e\u7ec6\u3002" }
            ];
        }

        if (RequiresSystemOwnershipReview(profile))
        {
            return
            [
                new() { Kind = AppActionKind.Uninstall, Label = "\u5378\u8f7d\u5e72\u51c0\u70b9", IsEnabled = false, Reason = "系统归属未确认，暂不提供卸载。" },
                new() { Kind = AppActionKind.Migration, Label = "\u8fc1\u79fb\u5230 D \u76d8", IsEnabled = false, Reason = "系统归属未确认，暂不提供迁移。" },
                new() { Kind = AppActionKind.CacheCleanup, Label = "\u6e05\u7406\u7f13\u5b58", IsEnabled = false, Reason = "系统归属未确认，暂不处理缓存。" },
                new() { Kind = AppActionKind.StartupControl, Label = "\u7ba1\u7406\u81ea\u542f\u52a8", IsEnabled = false, Reason = "系统归属未确认，暂不处理后台项。" },
                new() { Kind = AppActionKind.TechnicalDetails, Label = "\u6280\u672f\u8be6\u60c5", IsEnabled = true, Reason = "\u5c55\u5f00\u8def\u5f84\u3001\u670d\u52a1\u3001\u81ea\u542f\u52a8\u548c\u8ba1\u5212\u4efb\u52a1\u660e\u7ec6\u3002" }
            ];
        }

        return
        [
            new() { Kind = AppActionKind.Uninstall, Label = "\u5378\u8f7d\u5e72\u51c0\u70b9", IsEnabled = CanReviewUninstall(profile), Reason = "\u5148\u8fd0\u884c\u5b98\u65b9\u5378\u8f7d\u5668\uff0c\u518d\u626b\u63cf\u6b8b\u7559\u3002" },
            new() { Kind = AppActionKind.Migration, Label = "\u8fc1\u79fb\u5230 D \u76d8", IsEnabled = CanReviewMigration(profile), Reason = MigrationActionReason(profile) },
            new() { Kind = AppActionKind.CacheCleanup, Label = "\u6e05\u7406\u7f13\u5b58", IsEnabled = profile.CachePaths.Count > 0 || profile.CacheSizeBytes > 0, Reason = "\u53ea\u751f\u6210\u4f4e\u98ce\u9669\u7f13\u5b58\u65b9\u6848\uff0c\u6267\u884c\u524d\u4ecd\u9700\u786e\u8ba4\u3002" },
            new() { Kind = AppActionKind.StartupControl, Label = "\u7ba1\u7406\u81ea\u542f\u52a8", IsEnabled = HasStartupControlSignals(profile), Reason = "\u4f1a\u5148\u751f\u6210\u786e\u8ba4\u65b9\u6848\uff0c\u4e0d\u4f1a\u76f4\u63a5\u6539\u7cfb\u7edf\u3002" },
            new() { Kind = AppActionKind.TechnicalDetails, Label = "\u6280\u672f\u8be6\u60c5", IsEnabled = true, Reason = "\u5c55\u5f00\u8def\u5f84\u3001\u670d\u52a1\u3001\u81ea\u542f\u52a8\u548c\u8ba1\u5212\u4efb\u52a1\u660e\u7ec6\u3002" }
        ];
    }

    private static bool HasStartupControlSignals(SoftwareProfile profile) =>
        profile.StartupEntries.Count > 0
        || profile.Services.Count > 0
        || profile.ScheduledTasks.Count > 0;

    private static string CreateMigrationSummary(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
            return "不建议迁移：这是系统相关应用，可能依赖服务、驱动或系统范围配置。";
        if (RequiresSystemOwnershipReview(profile))
            return "系统归属待确认：暂不生成普通应用迁移方案。";

        if (IsOnDrive(profile.InstallPath, "D"))
        {
            var cDriveDataLocations = CDriveDataLocationCount(profile);
            return cDriveDataLocations > 0
                ? $"主程序不需要迁移：它已经在 D 盘；需要单独复查 {cDriveDataLocations} 个 C 盘数据或缓存写入线索。当前没有可靠的数据位置重定向方案。"
                : "不需要迁移主程序：主程序已经在 D 盘，暂未发现已归属的 C 盘写入线索。";
        }

        var plan = MigrationPlanner.CreatePlan(profile, RecommendMigrationDestination(profile), snapshotAvailable: false);
        return plan.Score.Band switch
        {
            MigrationRiskBand.Safe => "可以先生成迁移方案：暂未发现后台组件，但仍需要快照、确认和迁移后验证。",
            MigrationRiskBand.NeedsStopAndVerify => "可以评估迁移，但要先关闭软件和相关后台组件，迁移后还要验证。",
            MigrationRiskBand.CacheOnly => "只建议迁移缓存、模型或下载目录：主程序安装位置还不明确。",
            MigrationRiskBand.NotRecommended => "不建议迁移：当前证据显示风险较高。",
            _ => "需要先补齐证据，再决定是否生成迁移方案。"
        };
    }

    private static IReadOnlyList<string> CreateMigrationPreview(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
        {
            return
            [
                "不要迁移这个应用：系统相关应用可能依赖服务、驱动或系统范围配置。",
                "这里只解释原因，不会移动任何文件。"
            ];
        }

        if (RequiresSystemOwnershipReview(profile))
        {
            return
            [
                "系统归属待确认，暂不迁移这个程序。",
                "这里只保留技术详情，确认组件归属前不会移动文件或建立链接。"
            ];
        }

        if (IsOnDrive(profile.InstallPath, "D"))
        {
            var cDriveDataLocations = CDriveDataLocationCount(profile);
            if (cDriveDataLocations > 0)
            {
                return
                [
                    "主程序不需要迁移：它已经在 D 盘；需要单独复查 C 盘数据或缓存位置。",
                    "当前没有可靠的数据位置重定向方案，不会猜测移动或建立链接。"
                ];
            }

            return
            [
                "不需要迁移：主程序已经在 D 盘。",
                "暂未发现已归属的 C 盘写入线索。"
            ];
        }

        var destination = RecommendMigrationDestination(profile);
        var plan = MigrationPlanner.CreatePlan(profile, destination, snapshotAvailable: false);
        var lines = new List<string>
        {
            "建议目标位置：" + destination,
            "Agent 判断：" + MigrationBandExplanation(plan.Score.Band),
            "这里只生成方案，不会从应用抽屉移动任何文件。"
        };

        if (plan.Score.Band == MigrationRiskBand.CacheOnly)
            lines.Insert(0, "只迁移已识别的缓存、模型或下载目录，不移动主程序。");

        if (plan.RequiresSnapshot)
            lines.Add("真正迁移前必须有快照和回滚清单。");

        lines.Add("验证：软件能从新位置正常启动。");
        lines.Add("验证：原 C 盘位置不再继续产生新内容。");

        return lines;
    }

    private static string MigrationBandExplanation(MigrationRiskBand band) =>
        band switch
        {
            MigrationRiskBand.Safe => "可以规划迁移，但仍需确认和验证。",
            MigrationRiskBand.NeedsStopAndVerify => "发现后台组件，需要先关闭并在迁移后复查。",
            MigrationRiskBand.CacheOnly => "主程序位置不明确，只评估缓存、模型或下载目录。",
            MigrationRiskBand.NotRecommended => "当前风险较高，不建议迁移。",
            _ => "证据不足，先保持观察。"
        };

    private static string MigrationActionReason(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
            return "\u7cfb\u7edf\u7ec4\u4ef6\u4e0d\u5efa\u8bae\u8fc1\u79fb\u3002";
        if (RequiresSystemOwnershipReview(profile))
            return "系统归属未确认，暂不提供迁移。";

        if (IsOnDrive(profile.InstallPath, "D"))
        {
            return CDriveDataLocationCount(profile) > 0
                ? "主程序已经在 D 盘；当前没有可靠的数据位置重定向方案，只能先复查 C 盘数据来源。"
                : "主程序已经在 D 盘，不需要迁移。";
        }

        return "\u53ea\u751f\u6210\u8fc1\u79fb\u65b9\u6848\uff0c\u4e0d\u76f4\u63a5\u79fb\u52a8\u6587\u4ef6\u3002";
    }

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

    private static IReadOnlyList<string> CreateUninstallPreview(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
        {
            return
            [
                "这是系统相关应用，不会生成普通应用卸载计划。",
                "OMNIX 只保留技术详情，不会从应用抽屉运行卸载器或处理残留。"
            ];
        }

        if (RequiresSystemOwnershipReview(profile))
        {
            return
            [
                "系统归属待确认，暂不生成普通应用卸载方案。",
                "确认组件归属前不会运行卸载器或扫描可处理残留。"
            ];
        }

        return UninstallWorkflowGuidePresenter.Create(profile).DrawerLines;
    }

    private static IReadOnlyList<string> CreateTechnicalDetails(SoftwareProfile profile)
    {
        var details = new List<string>();
        details.Add($"Category: {profile.Category}; confidence {profile.CategoryAssessment.Confidence}; fallback {profile.CategoryAssessment.IsFallback}");
        details.AddRange(profile.CategoryAssessment.Evidence.Select(evidence =>
            $"Category evidence: {evidence.Source}; rule {evidence.MatchedRule}"));
        if (!string.IsNullOrWhiteSpace(profile.InstallPath)) details.Add("Install path: " + profile.InstallPath);
        if (profile.InstallDate is not null) details.Add("Install date: " + profile.InstallDate.Value.ToString("yyyy-MM-dd"));
        if (!string.IsNullOrWhiteSpace(profile.UninstallCommand)) details.Add("Uninstall command: " + profile.UninstallCommand);
        details.AddRange(profile.RunningProcesses.Select(value => "Process: " + value));
        if (profile.BackgroundComponents.Count > 0)
        {
            var readiness = BackgroundComponentChangeReadinessPolicy.Evaluate(profile);
            details.Add($"Background evidence: {readiness.StructuredObservationCount} read-only observations; rollback evidence not captured");
            details.AddRange(profile.BackgroundComponents
                .Take(12)
                .Select(FormatBackgroundComponent));
            details.Add("Change readiness: " + readiness.Summary);
            details.AddRange(readiness.Reasons.Select(reason => "Safety: " + reason));
        }
        else
        {
            details.AddRange(profile.Services.Select(value => "Service name hint: " + value));
            details.AddRange(profile.StartupEntries.Select(value => "Startup name hint: " + value));
            details.AddRange(profile.ScheduledTasks.Select(value => "Scheduled task name hint: " + value));
        }
        details.AddRange(profile.CDriveWritePaths.Select(value => "C drive path: " + value));
        return details;
    }

    private static string FormatBackgroundComponent(BackgroundComponentObservation observation)
    {
        var state = observation.ActivationState == BackgroundComponentActivationState.Unknown
            ? "activation unknown"
            : "activation " + observation.ActivationState;
        var runtime = observation.RuntimeState is BackgroundComponentRuntimeState.NotApplicable
            ? ""
            : "; runtime " + observation.RuntimeState;
        var approval = observation.StartupApproval is null
            ? ""
            : $"; startup approval {observation.StartupApproval.Status}; effective state not decoded; approval source {observation.StartupApproval.ApprovalKeyLocator}";
        return $"{observation.Identity.Kind}: {observation.Identity.DisplayName}; {state}{runtime}{approval}; source {observation.Identity.SourceLocator}";
    }

    private static bool RequiresSystemOwnershipReview(SoftwareProfile profile)
    {
        if (profile.Category != SoftwareCategory.Unknown)
            return false;

        var installPath = TryCanonicalPath(profile.InstallPath);
        if (installPath is null)
            return false;

        var windowsRoot = TryCanonicalPath(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows));
        if (windowsRoot is not null && IsSameOrDescendant(windowsRoot, installPath))
            return true;

        var programFileRoots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };
        return programFileRoots
            .Select(TryCanonicalPath)
            .Where(root => root is not null)
            .Cast<string>()
            .Select(root => TryCanonicalPath(Path.Combine(root, "WindowsApps")))
            .Where(root => root is not null)
            .Cast<string>()
            .Any(root => IsSameOrDescendant(root, installPath));
    }

    public static int CDriveDataLocationCount(SoftwareProfile profile)
    {
        var installRoot = TryCanonicalPath(profile.InstallPath);
        return profile.CDriveWritePaths
            .Select(TryCanonicalPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Where(path => IsOnDrive(path, "C"))
            .Where(path => installRoot is null || !IsSameOrDescendant(installRoot, path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
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

    private static bool IsSameOrDescendant(string parent, string candidate)
    {
        if (parent.Equals(candidate, StringComparison.OrdinalIgnoreCase))
            return true;

        var prefix = parent + Path.DirectorySeparatorChar;
        return candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOnDrive(string? path, string driveLetter) =>
        !string.IsNullOrWhiteSpace(path) &&
        path.StartsWith(driveLetter + ":\\", StringComparison.OrdinalIgnoreCase);

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.0} {units[unit]}";
    }
}
