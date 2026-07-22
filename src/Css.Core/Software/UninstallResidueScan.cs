using System;
using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Software;

public enum UninstallResidueKind
{
    CacheDirectory,
    LogDirectory,
    DataDirectory,
    InstallDirectory,
    StartupEntry,
    Service,
    ScheduledTask
}

public sealed class UninstallResidueCandidate
{
    public required UninstallResidueKind Kind { get; init; }
    public string? Path { get; init; }
    public string? Identifier { get; init; }
    public long EstimatedBytes { get; init; }
    public bool RequiresConfirmation { get; init; } = true;
}

public sealed class UninstallResidueGroup
{
    public required string Title { get; init; }
    public required RiskLevel Risk { get; init; }
    public required bool CanMoveToQuarantine { get; init; }
    public required IReadOnlyList<UninstallResidueCandidate> Candidates { get; init; }
}

public sealed class UninstallResidueScanReport
{
    public required string SoftwareName { get; init; }
    public required string Summary { get; init; }
    public bool OfficialUninstallAppearsComplete { get; init; }
    public bool WouldDeleteAutomatically { get; init; }
    public IReadOnlyList<UninstallResidueGroup> Groups { get; init; } = [];
}

public static class UninstallResidueScanBuilder
{
    public static UninstallResidueScanReport Build(
        SoftwareProfile before,
        IReadOnlyList<SoftwareProfile> afterProfiles,
        Func<string, bool> pathExists,
        Func<string, long>? sizeResolver = null)
    {
        var stillInstalled = afterProfiles.Any(profile => SameSoftware(before, profile));
        if (stillInstalled)
        {
            return new UninstallResidueScanReport
            {
                SoftwareName = before.Name,
                Summary = "仍然检测到这个软件，暂不建议清理残留，避免误删正在使用的文件。",
                OfficialUninstallAppearsComplete = false,
                WouldDeleteAutomatically = false
            };
        }

        var low = new List<UninstallResidueCandidate>();
        AddPathCandidates(low, before.CachePaths, UninstallResidueKind.CacheDirectory, pathExists, sizeResolver);
        AddPathCandidates(low, before.LogPaths, UninstallResidueKind.LogDirectory, pathExists, sizeResolver);

        var medium = new List<UninstallResidueCandidate>();
        AddPathCandidates(medium, before.DataPaths, UninstallResidueKind.DataDirectory, pathExists, sizeResolver);
        if (!string.IsNullOrWhiteSpace(before.InstallPath))
            AddPathCandidate(medium, before.InstallPath, UninstallResidueKind.InstallDirectory, pathExists, sizeResolver);

        var high = new List<UninstallResidueCandidate>();
        AddIdentifierCandidates(high, before.StartupEntries, UninstallResidueKind.StartupEntry);
        AddIdentifierCandidates(high, before.Services, UninstallResidueKind.Service);
        AddIdentifierCandidates(high, before.ScheduledTasks, UninstallResidueKind.ScheduledTask);

        var groups = new List<UninstallResidueGroup>();
        if (low.Count > 0)
        {
            groups.Add(new UninstallResidueGroup
            {
                Title = "低风险缓存/日志残留",
                Risk = RiskLevel.Low,
                CanMoveToQuarantine = true,
                Candidates = low
            });
        }

        if (medium.Count > 0)
        {
            groups.Add(new UninstallResidueGroup
            {
                Title = "中风险数据/安装目录残留",
                Risk = RiskLevel.Medium,
                CanMoveToQuarantine = false,
                Candidates = medium
            });
        }

        if (high.Count > 0)
        {
            groups.Add(new UninstallResidueGroup
            {
                Title = "高风险启动项/服务/计划任务残留",
                Risk = RiskLevel.High,
                CanMoveToQuarantine = false,
                Candidates = high
            });
        }

        return new UninstallResidueScanReport
        {
            SoftwareName = before.Name,
            Summary = groups.Count == 0
                ? "官方卸载后暂未发现残留候选；只读扫描完成。"
                : $"官方卸载后发现 {groups.Sum(group => group.Candidates.Count)} 项残留候选；只生成残留清单，不会自动删除。",
            OfficialUninstallAppearsComplete = true,
            WouldDeleteAutomatically = false,
            Groups = groups
        };
    }

    private static void AddPathCandidates(
        List<UninstallResidueCandidate> candidates,
        IEnumerable<string> paths,
        UninstallResidueKind kind,
        Func<string, bool> pathExists,
        Func<string, long>? sizeResolver)
    {
        foreach (var path in paths)
            AddPathCandidate(candidates, path, kind, pathExists, sizeResolver);
    }

    private static void AddPathCandidate(
        List<UninstallResidueCandidate> candidates,
        string path,
        UninstallResidueKind kind,
        Func<string, bool> pathExists,
        Func<string, long>? sizeResolver)
    {
        if (string.IsNullOrWhiteSpace(path) || !pathExists(path))
            return;

        candidates.Add(new UninstallResidueCandidate
        {
            Kind = kind,
            Path = path,
            EstimatedBytes = sizeResolver?.Invoke(path) ?? 0,
            RequiresConfirmation = true
        });
    }

    private static void AddIdentifierCandidates(
        List<UninstallResidueCandidate> candidates,
        IEnumerable<string> values,
        UninstallResidueKind kind)
    {
        foreach (var value in values.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            candidates.Add(new UninstallResidueCandidate
            {
                Kind = kind,
                Identifier = value,
                RequiresConfirmation = true
            });
        }
    }

    private static bool SameSoftware(SoftwareProfile before, SoftwareProfile after)
    {
        if (!before.Name.Equals(after.Name, StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.IsNullOrWhiteSpace(before.InstallPath) || string.IsNullOrWhiteSpace(after.InstallPath))
            return true;

        return before.InstallPath.Equals(after.InstallPath, StringComparison.OrdinalIgnoreCase);
    }
}
