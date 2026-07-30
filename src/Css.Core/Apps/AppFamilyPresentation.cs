using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class AppFamilyContextViewModel
{
    public required string FamilyName { get; init; }
    public required int RelatedEntryCount { get; init; }
    public required int OfficialUninstallEntryCount { get; init; }
    public required string Summary { get; init; }
    public required string CurrentEntrySummary { get; init; }
    public required long CDriveProgramBytes { get; init; }
    public required long OtherCDriveProgramBytes { get; init; }
    public required long CDriveDataBytes { get; init; }
}

public static partial class AppFamilyPresentationBuilder
{
    public static AppFamilyContextViewModel Create(
        SoftwareProfile current,
        IReadOnlyList<SoftwareProfile>? inventory = null)
    {
        ArgumentNullException.ThrowIfNull(current);
        var familyName = NormalizeFamilyName(current.Name);
        var related = (inventory ?? [current])
            .Where(profile =>
                NormalizeFamilyName(profile.Name)
                    .Equals(familyName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (!related.Contains(current))
            related.Insert(0, current);

        var officialUninstallCount = related.Count(profile =>
            !string.IsNullOrWhiteSpace(profile.UninstallCommand));

        return new AppFamilyContextViewModel
        {
            FamilyName = familyName,
            RelatedEntryCount = related.Count,
            OfficialUninstallEntryCount = officialUninstallCount,
            Summary = FamilySummary(related.Count, officialUninstallCount),
            CurrentEntrySummary = CurrentEntrySummary(current),
            CDriveProgramBytes = SumDistinctCDrivePrograms(related),
            OtherCDriveProgramBytes = SumDistinctCDrivePrograms(
                related,
                IsOnDrive(current.InstallPath, "C")
                    ? CanonicalPath(current.InstallPath)
                    : null),
            CDriveDataBytes = SumDistinctCDriveData(related)
        };
    }

    public static string NormalizeFamilyName(string displayName)
    {
        var value = displayName.Trim();
        value = UserScopeSuffixRegex().Replace(value, string.Empty);
        value = VersionSuffixRegex().Replace(value, string.Empty);
        value = WhitespaceRegex().Replace(value, " ").Trim();
        return value.Length == 0 ? displayName.Trim() : value;
    }

    private static string FamilySummary(int relatedCount, int officialUninstallCount)
    {
        if (relatedCount <= 1)
            return "只发现这一条应用记录；下面的操作只针对它。";

        var withoutOfficialUninstaller = relatedCount - officialUninstallCount;
        return $"发现同类 {relatedCount} 条记录：{officialUninstallCount} 条有官方卸载入口，"
            + $"{withoutOfficialUninstaller} 条没有。它们只会一起解释，不会合并卸载。";
    }

    private static string CurrentEntrySummary(SoftwareProfile profile)
    {
        var version = string.IsNullOrWhiteSpace(profile.DisplayVersion)
            ? "版本未识别"
            : "版本 " + profile.DisplayVersion;
        var location = LocationLabel(profile.InstallPath);
        var source = InventorySourceLabel(profile.InventorySource);
        var hasOfficialUninstaller = !string.IsNullOrWhiteSpace(profile.UninstallCommand);

        if (!string.IsNullOrWhiteSpace(profile.InstallPath))
        {
            return hasOfficialUninstaller
                ? $"当前这条：{version}，主程序在{location}，有官方卸载入口（{source}）。卸载只针对这条记录。"
                : $"当前这条：{version}，主程序或副本在{location}，但没有官方卸载入口；OMNIX 不会直接删除目录。";
        }

        if (profile.CDriveWritePaths.Count > 0 || profile.CDriveDataSizeBytes > 0)
        {
            return $"当前这条：{version}，只识别到 C 盘数据、缓存或更新线索，没有官方卸载入口；它不是可直接卸载的主程序。";
        }

        return hasOfficialUninstaller
            ? $"当前这条：{version}，有官方卸载入口，但主程序位置仍需确认（{source}）。"
            : $"当前这条：{version}，只有不完整扫描线索，没有官方卸载入口。";
    }

    private static long SumDistinctCDrivePrograms(
        IReadOnlyList<SoftwareProfile> profiles,
        string? excludedInstallPath = null)
    {
        return profiles
            .Where(profile => IsOnDrive(profile.InstallPath, "C"))
            .GroupBy(
                profile => CanonicalPath(profile.InstallPath) ?? profile.Name,
                StringComparer.OrdinalIgnoreCase)
            .Where(group =>
                excludedInstallPath is null
                || !group.Key.Equals(
                    excludedInstallPath,
                    StringComparison.OrdinalIgnoreCase))
            .Select(group => group.Max(profile => Math.Max(0, profile.InstalledSizeBytes)))
            .Aggregate(0L, SaturatingAdd);
    }

    private static long SumDistinctCDriveData(IReadOnlyList<SoftwareProfile> profiles)
    {
        return profiles
            .Where(profile => profile.CDriveDataSizeBytes > 0)
            .GroupBy(DataIdentity, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Max(profile => profile.CDriveDataSizeBytes))
            .Aggregate(0L, SaturatingAdd);
    }

    private static string DataIdentity(SoftwareProfile profile)
    {
        var paths = profile.CDriveWritePaths
            .Where(path => IsOnDrive(path, "C"))
            .Select(CanonicalPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Where(path => !IsSamePath(path, profile.InstallPath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return paths.Count > 0
            ? string.Join("|", paths)
            : profile.Name + "|" + profile.InventorySource;
    }

    private static bool IsSamePath(string candidate, string? other)
    {
        var canonicalOther = CanonicalPath(other);
        return canonicalOther is not null
            && candidate.Equals(canonicalOther, StringComparison.OrdinalIgnoreCase);
    }

    private static string LocationLabel(string? path)
    {
        if (IsOnDrive(path, "C"))
            return " C 盘";
        if (IsOnDrive(path, "D"))
            return " D 盘";
        return string.IsNullOrWhiteSpace(path) ? "未知位置" : "其他磁盘";
    }

    private static string InventorySourceLabel(string? source)
    {
        if (source?.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase) == true)
            return "当前用户安装登记";
        if (source?.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase) == true)
            return "全体用户安装登记";
        return "扫描来源未确认";
    }

    private static bool IsOnDrive(string? path, string driveLetter) =>
        !string.IsNullOrWhiteSpace(path)
        && path.StartsWith(driveLetter + @":\", StringComparison.OrdinalIgnoreCase);

    private static string? CanonicalPath(string? path)
    {
        try
        {
            return string.IsNullOrWhiteSpace(path)
                ? null
                : Path.GetFullPath(path)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return null;
        }
    }

    private static long SaturatingAdd(long left, long right) =>
        right > long.MaxValue - left ? long.MaxValue : left + right;

    [GeneratedRegex(
        @"\s*(?:\((?:user|current\s+user|all\s+users|machine)\)|（用户）)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UserScopeSuffixRegex();

    [GeneratedRegex(
        @"\s+v?\d+(?:\.\d+){1,3}(?:[-+][a-z0-9._-]+)?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionSuffixRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
