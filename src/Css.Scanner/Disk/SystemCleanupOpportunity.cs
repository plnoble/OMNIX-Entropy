using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Css.Scanner.Disk;

public enum KnownCleanupStoreKind
{
    UserTemporaryFiles,
    WindowsTemporaryFiles,
    WindowsUpdateDownloads,
    DeliveryOptimizationCache,
    DirectXShaderCache,
    CrashDumps
}

public enum CleanupHandling
{
    UserReview,
    WindowsManaged,
    ProtectedSystem
}

public sealed record KnownCleanupStoreLocation(
    KnownCleanupStoreKind Kind,
    string Title,
    string Path,
    CleanupHandling Handling);

public sealed class SystemCleanupOpportunity
{
    public required KnownCleanupStoreKind Kind { get; init; }
    public required string Title { get; init; }
    public required string Path { get; init; }
    public required CleanupHandling Handling { get; init; }
    public long SizeBytes { get; init; }
    public int FileCount { get; init; }
    public bool IsSizeLowerBound { get; init; }
    public bool IsAccessible { get; init; }
}

/// <summary>
/// Performs a bounded, read-only inventory of well-known cleanup stores.
/// It does not decide that a file is disposable and never mutates the store.
/// </summary>
public sealed class KnownCleanupStoreProbe
{
    private readonly int _maxFilesPerLocation;
    private readonly int _maxDirectoriesPerLocation;
    private readonly EnumerationOptions _options = new()
    {
        AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.Device,
        IgnoreInaccessible = false,
        RecurseSubdirectories = false,
        ReturnSpecialDirectories = false,
        BufferSize = 8192
    };

    public KnownCleanupStoreProbe(
        int maxFilesPerLocation = 20_000,
        int maxDirectoriesPerLocation = 20_000)
    {
        _maxFilesPerLocation = Math.Max(1, maxFilesPerLocation);
        _maxDirectoriesPerLocation = Math.Max(1, maxDirectoriesPerLocation);
    }

    public IReadOnlyList<SystemCleanupOpportunity> ProbeDefault(
        CancellationToken cancellationToken = default) =>
        Probe(ResolveDefaultLocations(), cancellationToken);

    public IReadOnlyList<SystemCleanupOpportunity> Probe(
        IEnumerable<KnownCleanupStoreLocation> locations,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locations);

        return locations
            .Where(location => !string.IsNullOrWhiteSpace(location.Path))
            .GroupBy(location => CanonicalPath(location.Path), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(location => ProbeLocation(location, cancellationToken))
            .ToList();
    }

    public static IReadOnlyList<KnownCleanupStoreLocation> ResolveDefaultLocations()
    {
        var localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var locations = new List<KnownCleanupStoreLocation>();

        Add(
            locations,
            KnownCleanupStoreKind.UserTemporaryFiles,
            "用户临时文件",
            Path.GetTempPath(),
            CleanupHandling.UserReview);
        Add(
            locations,
            KnownCleanupStoreKind.WindowsTemporaryFiles,
            "Windows 临时文件",
            CombineIfPresent(windows, "Temp"),
            CleanupHandling.WindowsManaged);
        Add(
            locations,
            KnownCleanupStoreKind.WindowsUpdateDownloads,
            "Windows 更新下载缓存",
            CombineIfPresent(windows, "SoftwareDistribution", "Download"),
            CleanupHandling.WindowsManaged);
        Add(
            locations,
            KnownCleanupStoreKind.DeliveryOptimizationCache,
            "传递优化缓存",
            CombineIfPresent(
                windows,
                "ServiceProfiles",
                "NetworkService",
                "AppData",
                "Local",
                "Microsoft",
                "Windows",
                "DeliveryOptimization",
                "Cache"),
            CleanupHandling.WindowsManaged);
        Add(
            locations,
            KnownCleanupStoreKind.DirectXShaderCache,
            "DirectX 着色器缓存",
            CombineIfPresent(localAppData, "D3DSCache"),
            CleanupHandling.UserReview);
        Add(
            locations,
            KnownCleanupStoreKind.CrashDumps,
            "应用崩溃转储",
            CombineIfPresent(localAppData, "CrashDumps"),
            CleanupHandling.UserReview);

        return locations
            .GroupBy(location => CanonicalPath(location.Path), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private SystemCleanupOpportunity ProbeLocation(
        KnownCleanupStoreLocation location,
        CancellationToken cancellationToken)
    {
        var root = CanonicalPath(location.Path);
        if (!Directory.Exists(root))
            return Create(location, root, 0, 0, isLowerBound: false, isAccessible: false);

        var directories = new Stack<string>();
        directories.Push(root);
        long sizeBytes = 0;
        var fileCount = 0;
        var directoryCount = 0;
        var hadAccessErrors = false;
        var rootAccessible = true;
        var reachedLimit = false;

        while (directories.Count > 0 && !reachedLimit)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (directoryCount >= _maxDirectoriesPerLocation)
            {
                reachedLimit = true;
                break;
            }

            var current = directories.Pop();
            directoryCount++;

            try
            {
                foreach (var file in new DirectoryInfo(current).EnumerateFiles("*", _options))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (fileCount >= _maxFilesPerLocation)
                    {
                        reachedLimit = true;
                        break;
                    }

                    fileCount++;
                    sizeBytes = SaturatingAdd(sizeBytes, SafeLength(file, ref hadAccessErrors));
                }
            }
            catch (Exception ex) when (IsExpectedFileSystemFailure(ex))
            {
                hadAccessErrors = true;
                if (current.Equals(root, StringComparison.OrdinalIgnoreCase))
                    rootAccessible = false;
            }

            if (reachedLimit)
                break;

            try
            {
                foreach (var directory in new DirectoryInfo(current).EnumerateDirectories("*", _options))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if ((directory.Attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        if (directoryCount + directories.Count >= _maxDirectoriesPerLocation)
                        {
                            reachedLimit = true;
                            break;
                        }

                        directories.Push(directory.FullName);
                    }
                }
            }
            catch (Exception ex) when (IsExpectedFileSystemFailure(ex))
            {
                hadAccessErrors = true;
                if (current.Equals(root, StringComparison.OrdinalIgnoreCase))
                    rootAccessible = false;
            }
        }

        return Create(
            location,
            root,
            sizeBytes,
            fileCount,
            isLowerBound: reachedLimit || hadAccessErrors,
            isAccessible: rootAccessible);
    }

    private static SystemCleanupOpportunity Create(
        KnownCleanupStoreLocation location,
        string path,
        long sizeBytes,
        int fileCount,
        bool isLowerBound,
        bool isAccessible) =>
        new()
        {
            Kind = location.Kind,
            Title = location.Title,
            Path = path,
            Handling = location.Handling,
            SizeBytes = Math.Max(0, sizeBytes),
            FileCount = Math.Max(0, fileCount),
            IsSizeLowerBound = isLowerBound,
            IsAccessible = isAccessible
        };

    private static long SafeLength(FileInfo file, ref bool hadAccessErrors)
    {
        try
        {
            return Math.Max(0, file.Length);
        }
        catch (Exception ex) when (IsExpectedFileSystemFailure(ex))
        {
            hadAccessErrors = true;
            return 0;
        }
    }

    private static long SaturatingAdd(long left, long right) =>
        right > long.MaxValue - left ? long.MaxValue : left + right;

    private static bool IsExpectedFileSystemFailure(Exception exception) =>
        exception is UnauthorizedAccessException
            or DirectoryNotFoundException
            or FileNotFoundException
            or IOException
            or System.Security.SecurityException;

    private static void Add(
        List<KnownCleanupStoreLocation> locations,
        KnownCleanupStoreKind kind,
        string title,
        string? path,
        CleanupHandling handling)
    {
        if (!string.IsNullOrWhiteSpace(path))
            locations.Add(new KnownCleanupStoreLocation(kind, title, path, handling));
    }

    private static string? CombineIfPresent(string? root, params string[] parts)
    {
        if (string.IsNullOrWhiteSpace(root))
            return null;

        return parts.Aggregate(root, Path.Combine);
    }

    private static string CanonicalPath(string path)
    {
        try
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return path.Trim();
        }
    }
}
