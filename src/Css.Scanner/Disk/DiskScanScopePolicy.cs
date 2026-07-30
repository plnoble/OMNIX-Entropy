using System;
using System.Collections.Generic;
using System.IO;

namespace Css.Scanner.Disk;

public static class DiskScanScopePolicy
{
    public static bool IsSystemDrive(string scanRoot, string windowsDirectory)
    {
        var scanDrive = NormalizeDriveRoot(scanRoot);
        var systemDrive = NormalizeDriveRoot(windowsDirectory);
        return scanDrive is not null
            && systemDrive is not null
            && string.Equals(
                scanDrive,
                systemDrive,
                StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string>? ResolvePersonalStorageRoots(
        string selectedDriveRoot,
        string scanRoot,
        string windowsDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedDriveRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(scanRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(windowsDirectory);

        return IsSystemDrive(selectedDriveRoot, windowsDirectory)
            && IsSystemDrive(scanRoot, windowsDirectory)
            ? null
            : [Path.GetFullPath(scanRoot)];
    }

    private static string? NormalizeDriveRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            return string.IsNullOrWhiteSpace(root)
                ? null
                : root.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }
        catch
        {
            return null;
        }
    }
}
