using System.Collections.Generic;
using Css.Core.Operations;

namespace Css.Core.Software;

public enum UninstallStepKind
{
    RunOfficialUninstaller,
    ScanResidue,
    ReviewResidue,
    MoveLowRiskResidueToQuarantine
}

public sealed class UninstallStep
{
    public required UninstallStepKind Kind { get; init; }
    public string? Command { get; init; }
    public required string Description { get; init; }
}

public sealed class ResidueGroup
{
    public required string Title { get; init; }
    public required RiskLevel Risk { get; init; }
    public required bool CanMoveToQuarantine { get; init; }
    public required IReadOnlyList<string> Candidates { get; init; }
}

public sealed class UninstallPlan
{
    public required string SoftwareName { get; init; }
    public required bool RequiresUserConfirmation { get; init; }
    public required IReadOnlyList<UninstallStep> Steps { get; init; }
    public required IReadOnlyList<ResidueGroup> ResidueGroups { get; init; }
}

public static class UninstallPlanBuilder
{
    public static UninstallPlan Create(SoftwareProfile profile)
    {
        var lowRisk = new List<string>();
        lowRisk.AddRange(profile.CachePaths);
        lowRisk.AddRange(profile.LogPaths);

        var highRisk = new List<string>();
        highRisk.AddRange(profile.Services);
        highRisk.AddRange(profile.ScheduledTasks);

        var residueGroups = new List<ResidueGroup>();
        if (lowRisk.Count > 0)
        {
            residueGroups.Add(new ResidueGroup
            {
                Title = "低风险缓存/日志残留",
                Risk = RiskLevel.Low,
                CanMoveToQuarantine = true,
                Candidates = lowRisk
            });
        }

        if (highRisk.Count > 0)
        {
            residueGroups.Add(new ResidueGroup
            {
                Title = "高风险服务/计划任务残留",
                Risk = RiskLevel.High,
                CanMoveToQuarantine = false,
                Candidates = highRisk
            });
        }

        return new UninstallPlan
        {
            SoftwareName = profile.Name,
            RequiresUserConfirmation = true,
            Steps =
            [
                new()
                {
                    Kind = UninstallStepKind.RunOfficialUninstaller,
                    Command = profile.UninstallCommand,
                    Description = "先运行软件官方卸载器。"
                },
                new()
                {
                    Kind = UninstallStepKind.ScanResidue,
                    Description = "卸载完成后扫描残留文件、启动项、服务和计划任务。"
                },
                new()
                {
                    Kind = UninstallStepKind.ReviewResidue,
                    Description = "按低/中/高风险展示残留清单。"
                },
                new()
                {
                    Kind = UninstallStepKind.MoveLowRiskResidueToQuarantine,
                    Description = "仅低风险残留可确认后移动到隔离区。"
                }
            ],
            ResidueGroups = residueGroups
        };
    }
}
