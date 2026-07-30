using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Css.Scanner.Experience;

public sealed record LocalDriveScanObservation(
    string Root,
    bool IsReady,
    DriveType DriveType,
    long TotalBytes,
    long FreeBytes);

public sealed class DriveScanTargetViewModel
{
    public required string Root { get; init; }
    public required string DisplayName { get; init; }
    public required string SpaceSummary { get; init; }
    public required string AccessibilityName { get; init; }
    public required bool IsSystemDrive { get; init; }
}

public static class DriveScanTargetPresenter
{
    public static IReadOnlyList<DriveScanTargetViewModel> Create(
        IEnumerable<LocalDriveScanObservation> observations,
        string systemDriveRoot)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentException.ThrowIfNullOrWhiteSpace(systemDriveRoot);

        var normalizedSystemRoot = NormalizeRoot(systemDriveRoot);
        var targets = observations
            .Where(observation =>
                observation.IsReady
                && observation.DriveType == DriveType.Fixed)
            .Select(observation => CreateTarget(
                NormalizeRoot(observation.Root),
                observation.TotalBytes,
                observation.FreeBytes,
                normalizedSystemRoot))
            .GroupBy(target => target.Root, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(target => target.IsSystemDrive)
            .ThenBy(target => target.Root, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return targets.Length > 0
            ? targets
            : [CreateTarget(normalizedSystemRoot, 0, 0, normalizedSystemRoot)];
    }

    public static string DriveLabel(string driveRoot)
    {
        var root = NormalizeRoot(driveRoot);
        return char.ToUpperInvariant(root[0]) + " 盘";
    }

    private static DriveScanTargetViewModel CreateTarget(
        string root,
        long totalBytes,
        long freeBytes,
        string systemDriveRoot)
    {
        var isSystemDrive = string.Equals(
            root,
            systemDriveRoot,
            StringComparison.OrdinalIgnoreCase);
        var driveLabel = DriveLabel(root);
        var displayName = isSystemDrive
            ? "系统盘 " + driveLabel
            : driveLabel;
        var spaceSummary = totalBytes > 0
            ? $"剩余 {FormatBytes(Math.Clamp(freeBytes, 0, totalBytes))}"
            : "容量暂时不可用";

        return new DriveScanTargetViewModel
        {
            Root = root,
            DisplayName = displayName,
            SpaceSummary = spaceSummary,
            AccessibilityName = $"{displayName}，{spaceSummary}",
            IsSystemDrive = isSystemDrive
        };
    }

    private static string NormalizeRoot(string path)
    {
        var root = Path.GetPathRoot(Path.GetFullPath(path));
        if (string.IsNullOrWhiteSpace(root)
            || root.Length < 2
            || root[1] != ':')
        {
            throw new ArgumentException("A local drive root is required.", nameof(path));
        }

        return char.ToUpperInvariant(root[0]) + @":\";
    }

    private static string FormatBytes(long bytes)
    {
        const double unit = 1024;
        if (bytes >= unit * unit * unit)
            return (bytes / (unit * unit * unit)).ToString("0.#", CultureInfo.InvariantCulture) + " GB";
        if (bytes >= unit * unit)
            return (bytes / (unit * unit)).ToString("0.#", CultureInfo.InvariantCulture) + " MB";
        return (bytes / unit).ToString("0.#", CultureInfo.InvariantCulture) + " KB";
    }
}
