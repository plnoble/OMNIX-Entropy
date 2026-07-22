using Css.Core.Software;
using Css.Scanner.Disk;

namespace Css.Scanner.Experience;

public static class SoftwareGrowthProfileEnricher
{
    public static IReadOnlyList<SoftwareProfile> Apply(
        IReadOnlyList<SoftwareProfile> profiles,
        IReadOnlyList<GrowthFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(findings);
        var nameCounts = profiles
            .Where(profile => !string.IsNullOrWhiteSpace(profile.Name))
            .GroupBy(profile => profile.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        return profiles
            .Select(profile => CloneWithGrowth(
                profile,
                UniqueProfileGrowth(profile, nameCounts, findings)))
            .ToArray();
    }

    private static long UniqueProfileGrowth(
        SoftwareProfile profile,
        IReadOnlyDictionary<string, int> nameCounts,
        IReadOnlyList<GrowthFinding> findings)
    {
        var name = profile.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)
            || !nameCounts.TryGetValue(name, out var count)
            || count != 1)
        {
            return 0;
        }

        var candidates = findings
            .Where(finding =>
                !finding.IsNewObservation
                && finding.SourceKind == GrowthSourceKind.Software
                && finding.GrowthBytes > 0
                && string.Equals(
                    finding.OwnerSoftware?.Trim(),
                    name,
                    StringComparison.OrdinalIgnoreCase))
            .Select(finding => TryCanonicalPath(finding.Path, out var path)
                ? new GrowthPath(path, finding.GrowthBytes)
                : null)
            .Where(candidate => candidate is not null)
            .Cast<GrowthPath>()
            .GroupBy(candidate => candidate.Path, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(candidate => candidate.Bytes).First())
            .ToArray();
        if (candidates.Length == 0)
            return 0;

        var roots = candidates
            .Where(candidate => !candidates.Any(other =>
                !other.Path.Equals(candidate.Path, StringComparison.OrdinalIgnoreCase)
                && IsDescendantPath(other.Path, candidate.Path)))
            .ToArray();
        var total = 0L;
        foreach (var root in roots)
        {
            var contribution = candidates
                .Where(candidate =>
                    candidate.Path.Equals(root.Path, StringComparison.OrdinalIgnoreCase)
                    || IsDescendantPath(root.Path, candidate.Path))
                .Max(candidate => candidate.Bytes);
            total = SaturatingAdd(total, contribution);
        }
        return total;
    }

    private static SoftwareProfile CloneWithGrowth(SoftwareProfile source, long recentGrowthBytes) =>
        new()
        {
            Name = source.Name,
            Publisher = source.Publisher,
            SignatureSubject = source.SignatureSubject,
            Category = source.Category,
            CategoryAssessment = source.CategoryAssessment,
            InstallPath = source.InstallPath,
            UninstallCommand = source.UninstallCommand,
            DisplayIconPath = source.DisplayIconPath,
            DisplayIconIndex = source.DisplayIconIndex,
            ReinstallSource = source.ReinstallSource,
            IsWindowsInstaller = source.IsWindowsInstaller,
            WindowsInstallerProductCode = source.WindowsInstallerProductCode,
            InstallDate = source.InstallDate,
            InstalledSizeBytes = source.InstalledSizeBytes,
            DataSizeBytes = source.DataSizeBytes,
            CacheSizeBytes = source.CacheSizeBytes,
            RecentGrowthBytes = recentGrowthBytes,
            DataPaths = source.DataPaths,
            CachePaths = source.CachePaths,
            LogPaths = source.LogPaths,
            CDriveWritePaths = source.CDriveWritePaths,
            RunningProcesses = source.RunningProcesses,
            StartupEntries = source.StartupEntries,
            Services = source.Services,
            ScheduledTasks = source.ScheduledTasks,
            BackgroundComponents = source.BackgroundComponents
        };

    private static bool TryCanonicalPath(string? path, out string canonical)
    {
        canonical = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            canonical = Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return !string.IsNullOrWhiteSpace(canonical);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsDescendantPath(string parent, string child)
    {
        var prefix = parent.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return child.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static long SaturatingAdd(long left, long right) =>
        right >= long.MaxValue - left ? long.MaxValue : left + right;

    private sealed record GrowthPath(string Path, long Bytes);
}
