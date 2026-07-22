using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Apps;
using Css.Core.Operations;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public static class HealthCheckSummaryBuilder
{
    public static HealthCheckSummary Build(
        DriveScanResult result,
        IReadOnlyList<Recommendation> recommendations,
        IReadOnlyList<GrowthFinding>? growthFindings = null,
        PersonalStorageAnalysis? personalStorage = null,
        MachineHealthObservation? machineHealth = null,
        IReadOnlyList<SoftwareProfile>? softwareProfiles = null,
        int observedSnapshotCount = 1)
    {
        growthFindings ??= [];
        personalStorage ??= new PersonalStorageAnalysis();
        var usedPercent = result.TotalBytes <= 0
            ? 0
            : (double)(result.TotalBytes - result.FreeBytes) / result.TotalBytes * 100;
        var reclaimable = SaturatingSum(recommendations
            .Where(r => HealthFindingRiskPolicy.IsLowRiskClean(r.Action, r.Risk))
            .Select(r => r.EstimatedImpactBytes));
        var score = Math.Clamp(100 - (int)Math.Round(Math.Max(0, usedPercent - 50)), 0, 100);

        return new HealthCheckSummary
        {
            OverallScore = score,
            Hardware = machineHealth?.Hardware,
            Dimensions = BuildDimensions(
                score,
                usedPercent,
                reclaimable,
                machineHealth,
                softwareProfiles,
                growthFindings,
                observedSnapshotCount),
            KeyFindings = BuildFindings(
                result,
                recommendations,
                growthFindings,
                personalStorage)
        };
    }

    private static IReadOnlyList<HealthDimensionResult> BuildDimensions(
        int score,
        double cDriveUsedPercent,
        long reclaimableBytes,
        MachineHealthObservation? machineHealth,
        IReadOnlyList<SoftwareProfile>? softwareProfiles,
        IReadOnlyList<GrowthFinding> growthFindings,
        int observedSnapshotCount)
    {
        var dimensions = new List<HealthDimensionResult>
        {
            new()
            {
                Name = "综合评分",
                Result = score + " 分（当前按磁盘空间）",
                Rating = score >= 85 ? "良好" : score >= 70 ? "良好，有优化空间" : "需要关注"
            },
            new()
            {
                Name = "磁盘健康",
                Result = $"C 盘 {cDriveUsedPercent:0.0}%，可安全处理约 {FormatBytes(reclaimableBytes)}",
                Rating = cDriveUsedPercent >= 85 ? "需要关注" : "有优化空间"
            }
        };

        dimensions.AddRange(MachineHealthPresentationBuilder.CreateDimensions(machineHealth));

        dimensions.Add(StartupDimension(softwareProfiles));
        dimensions.Add(UsageTrendDimension(growthFindings, observedSnapshotCount));
        return dimensions;
    }

    private static HealthDimensionResult StartupDimension(
        IReadOnlyList<SoftwareProfile>? softwareProfiles)
    {
        if (softwareProfiles is null)
            return UnavailableDimension("自启动线索", "应用尚未扫描，本次不判断自启动");

        var ordinary = softwareProfiles.Count(profile =>
            AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile)
            && profile.StartupEntries.Count > 0);
        var system = softwareProfiles.Count(profile =>
            profile.Category == SoftwareCategory.SystemTool
            && profile.StartupEntries.Count > 0);
        var ownershipPending = softwareProfiles.Count(profile =>
            profile.StartupEntries.Count > 0
            && profile.Category != SoftwareCategory.SystemTool
            && !AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile));
        var systemText = system > 0 ? $"；另有 {system} 个系统组件线索" : string.Empty;
        var ownershipText = ownershipPending > 0
            ? $"；另有 {ownershipPending} 个归属待确认线索"
            : string.Empty;
        var readOnlyText = system + ownershipPending > 0 ? "，仅供查看" : string.Empty;
        return new HealthDimensionResult
        {
            Name = "自启动线索",
            Result = $"{ordinary} 个普通应用包含自启动线索{systemText}{ownershipText}{readOnlyText}",
            Rating = ordinary == 0
                ? system + ownershipPending > 0 ? "仅供查看" : "未发现"
                : ordinary <= 3 ? "建议查看" : "线索较多"
        };
    }

    private static HealthDimensionResult UsageTrendDimension(
        IReadOnlyList<GrowthFinding> growthFindings,
        int observedSnapshotCount)
    {
        var snapshots = Math.Clamp(observedSnapshotCount, 1, 100);
        if (snapshots < 3)
        {
            return new HealthDimensionResult
            {
                Name = "使用趋势",
                Result = $"已有 {snapshots} 次手动体检，至少 3 次后再判断持续增长",
                Rating = "历史不足"
            };
        }

        var sustained = growthFindings.Count(finding => finding.IsSustainedGrowth);
        return new HealthDimensionResult
        {
            Name = "使用趋势",
            Result = sustained == 0
                ? $"最近 {snapshots} 次手动体检未发现持续增长来源"
                : $"最近 {snapshots} 次手动体检发现 {sustained} 个持续增长来源",
            Rating = sustained == 0 ? "正常" : "需要关注"
        };
    }

    private static HealthDimensionResult UnavailableDimension(
        string name,
        string result,
        string rating = "未检测") =>
        new()
        {
            Name = name,
            Result = result,
            Rating = rating
        };

    private static IReadOnlyList<HealthFinding> BuildFindings(
        DriveScanResult result,
        IReadOnlyList<Recommendation> recommendations,
        IReadOnlyList<GrowthFinding> growthFindings,
        PersonalStorageAnalysis personalStorage)
    {
        var findings = new List<HealthFinding>();

        var sustainedGrowth = GrowthFindingPresenter.CreateList(growthFindings)
            .Select(item => item.Finding)
            .Where(item => item?.IsSustainedGrowth == true)
            .Cast<GrowthFinding>()
            .Take(2);
        foreach (var growth in sustainedGrowth)
        {
            var owner = GrowthFindingPresenter.OwnerLabel(growth);
            findings.Add(new HealthFinding
            {
                Text = $"{owner} 近期多次变大，累计增加 {FormatBytes(growth.TrendGrowthBytes)}；建议先查明是哪类内容",
                Kind = HealthFindingKind.SustainedGrowth,
                TargetAppName = growth.SourceKind == GrowthSourceKind.Software
                    ? growth.OwnerSoftware
                    : null,
                Action = RecommendationAction.Observe,
                Risk = RiskLevel.Medium
            });
        }

        var largeFiles = personalStorage.Findings
            .Where(item => item.Kind == PersonalStorageFindingKind.LongUnusedLargeFile)
            .ToArray();
        if (largeFiles.Length > 0 && findings.Count < 5)
        {
            findings.Add(new HealthFinding
            {
                Text = $"发现 {largeFiles.Length} 个长期未用的大文件，共约 {FormatBytes(SaturatingSum(largeFiles.Select(item => item.CandidateBytes)))}；建议先确认用途或归档",
                Kind = HealthFindingKind.PersonalStorage,
                Action = RecommendationAction.Observe,
                Risk = RiskLevel.Medium
            });
        }

        var duplicateGroups = personalStorage.Findings
            .Where(item => item.Kind == PersonalStorageFindingKind.PossibleDuplicateGroup)
            .ToArray();
        if (duplicateGroups.Length > 0 && findings.Count < 5)
        {
            findings.Add(new HealthFinding
            {
                Text = $"发现 {duplicateGroups.Length} 组疑似重复文件，最多可能重复占用 {FormatBytes(SaturatingSum(duplicateGroups.Select(item => item.CandidateBytes)))}；尚未比对文件内容",
                Kind = HealthFindingKind.PersonalStorage,
                Action = RecommendationAction.Observe,
                Risk = RiskLevel.Medium
            });
        }

        foreach (var recommendation in recommendations.Take(Math.Max(0, 5 - findings.Count)))
        {
            var sizeText = recommendation.EstimatedImpactBytes > 0
                ? " " + FormatBytes(recommendation.EstimatedImpactBytes)
                : string.Empty;
            findings.Add(new HealthFinding
            {
                Text = $"{recommendation.Title}{sizeText}，{ActionText(recommendation.Action, recommendation.Risk)}",
                Action = recommendation.Action,
                Risk = recommendation.Risk
            });
        }

        if (findings.Count == 0)
        {
            findings.Add(new HealthFinding
            {
                Text = $"扫描到 {result.TopLevel.Count} 个顶层来源，暂未发现可立即处理项",
                Action = RecommendationAction.Observe,
                Risk = RiskLevel.None
            });
        }

        return findings;
    }

    private static string ActionText(RecommendationAction action, RiskLevel risk) =>
        action switch
        {
            RecommendationAction.Clean when HealthFindingRiskPolicy.IsLowRiskClean(action, risk) =>
                "低风险，可确认后进入隔离区",
            RecommendationAction.Clean =>
                "风险偏高，先观察并补齐快照和回滚",
            RecommendationAction.Migrate => "建议评估迁移",
            RecommendationAction.DisableStartup => "建议评估关闭自启动",
            RecommendationAction.Uninstall => "建议评估卸载",
            RecommendationAction.Keep => "建议保留",
            _ => "建议先观察"
        };

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0) return "0 B";
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

    private static long SaturatingSum(IEnumerable<long> values)
    {
        long total = 0;
        foreach (var value in values.Where(value => value > 0))
        {
            if (value > long.MaxValue - total)
                return long.MaxValue;
            total += value;
        }
        return total;
    }
}
