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
    CrashDumps,
    InstallerDownloadRemnants,
    BrowserCache,
    DeveloperToolCache,
    WindowsDiagnosticReports
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
    CleanupHandling Handling)
{
    public int ReviewAgeDays { get; init; }
    public bool RecurseSubdirectories { get; init; } = true;
    public IReadOnlyList<string> IncludedExtensions { get; init; } = [];
}

public sealed class SystemCleanupOpportunity
{
    public required KnownCleanupStoreKind Kind { get; init; }
    public required string Title { get; init; }
    public required string Path { get; init; }
    public required CleanupHandling Handling { get; init; }
    public long SizeBytes { get; init; }
    public int FileCount { get; init; }
    public long ReviewableSizeBytes { get; init; }
    public int ReviewableFileCount { get; init; }
    public int RecentFileCount { get; init; }
    public int AgeUnknownFileCount { get; init; }
    public int ReviewAgeDays { get; init; }
    public bool HasAgeFilteredLocations { get; init; }
    public int LocationCount { get; init; } = 1;
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

        var normalizedLocations = locations
            .Where(location => !string.IsNullOrWhiteSpace(location.Path))
            .Select(location => location with { Path = CanonicalPath(location.Path) })
            .GroupBy(location => CanonicalPath(location.Path), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        return normalizedLocations
            .Select(location => ProbeLocation(
                location,
                normalizedLocations
                    .Where(other => IsStrictDescendant(other.Path, location.Path))
                    .Select(other => other.Path)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase),
                cancellationToken))
            .GroupBy(
                item => (item.Kind, item.Title, item.Handling),
                EqualityComparer<(KnownCleanupStoreKind, string, CleanupHandling)>.Default)
            .Select(Aggregate)
            .ToList();
    }

    public static IReadOnlyList<KnownCleanupStoreLocation> ResolveDefaultLocations()
    {
        var localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var locations = new List<KnownCleanupStoreLocation>();

        Add(
            locations,
            KnownCleanupStoreKind.UserTemporaryFiles,
            "用户临时文件",
            Path.GetTempPath(),
            CleanupHandling.UserReview,
            reviewAgeDays: 7);
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
            CleanupHandling.UserReview,
            reviewAgeDays: 7);
        Add(
            locations,
            KnownCleanupStoreKind.CrashDumps,
            "应用崩溃转储",
            CombineIfPresent(localAppData, "CrashDumps"),
            CleanupHandling.UserReview,
            reviewAgeDays: 7);
        Add(
            locations,
            KnownCleanupStoreKind.InstallerDownloadRemnants,
            "旧安装包候选",
            CombineIfPresent(userProfile, "Downloads"),
            CleanupHandling.UserReview,
            reviewAgeDays: 30,
            recurseSubdirectories: false,
            includedExtensions:
            [
                ".exe", ".msi", ".msix", ".msixbundle", ".appx", ".appxbundle"
            ]);
        AddBrowserCacheLocations(locations, localAppData);
        AddDeveloperCacheLocations(locations, localAppData);
        Add(
            locations,
            KnownCleanupStoreKind.WindowsDiagnosticReports,
            "Windows 诊断报告",
            CombineIfPresent(localAppData, "Microsoft", "Windows", "WER"),
            CleanupHandling.WindowsManaged);

        return locations
            .GroupBy(location => CanonicalPath(location.Path), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private SystemCleanupOpportunity ProbeLocation(
        KnownCleanupStoreLocation location,
        IReadOnlySet<string> excludedNestedRoots,
        CancellationToken cancellationToken)
    {
        var root = CanonicalPath(location.Path);
        if (!Directory.Exists(root))
        {
            return Create(
                location,
                root,
                0,
                0,
                0,
                0,
                0,
                0,
                isLowerBound: false,
                isAccessible: false);
        }

        try
        {
            if ((new DirectoryInfo(root).Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return Create(
                    location,
                    root,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    isLowerBound: true,
                    isAccessible: false);
            }
        }
        catch (Exception ex) when (IsExpectedFileSystemFailure(ex))
        {
            return Create(
                location,
                root,
                0,
                0,
                0,
                0,
                0,
                0,
                isLowerBound: true,
                isAccessible: false);
        }

        var directories = new Stack<string>();
        directories.Push(root);
        long sizeBytes = 0;
        long reviewableSizeBytes = 0;
        var fileCount = 0;
        var inspectedFileCount = 0;
        var reviewableFileCount = 0;
        var recentFileCount = 0;
        var ageUnknownFileCount = 0;
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
                    if (inspectedFileCount >= _maxFilesPerLocation)
                    {
                        reachedLimit = true;
                        break;
                    }

                    inspectedFileCount++;
                    if (!IncludesFile(location, file))
                        continue;

                    fileCount++;
                    var length = SafeLength(file, ref hadAccessErrors);
                    sizeBytes = SaturatingAdd(sizeBytes, length);
                    ClassifyAge(
                        location,
                        file,
                        length,
                        ref reviewableSizeBytes,
                        ref reviewableFileCount,
                        ref recentFileCount,
                        ref ageUnknownFileCount,
                        ref hadAccessErrors);
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

            if (!location.RecurseSubdirectories)
                continue;

            try
            {
                foreach (var directory in new DirectoryInfo(current).EnumerateDirectories("*", _options))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var directoryPath = CanonicalPath(directory.FullName);
                    if ((directory.Attributes & FileAttributes.ReparsePoint) == 0
                        && !excludedNestedRoots.Contains(directoryPath))
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
            reviewableSizeBytes,
            reviewableFileCount,
            recentFileCount,
            ageUnknownFileCount,
            isLowerBound: reachedLimit || hadAccessErrors,
            isAccessible: rootAccessible);
    }

    private static SystemCleanupOpportunity Create(
        KnownCleanupStoreLocation location,
        string path,
        long sizeBytes,
        int fileCount,
        long reviewableSizeBytes,
        int reviewableFileCount,
        int recentFileCount,
        int ageUnknownFileCount,
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
            ReviewableSizeBytes = Math.Max(0, reviewableSizeBytes),
            ReviewableFileCount = Math.Max(0, reviewableFileCount),
            RecentFileCount = Math.Max(0, recentFileCount),
            AgeUnknownFileCount = Math.Max(0, ageUnknownFileCount),
            ReviewAgeDays = Math.Max(0, location.ReviewAgeDays),
            HasAgeFilteredLocations = location.ReviewAgeDays > 0,
            IsSizeLowerBound = isLowerBound,
            IsAccessible = isAccessible
        };

    private static SystemCleanupOpportunity Aggregate(
        IGrouping<(KnownCleanupStoreKind Kind, string Title, CleanupHandling Handling), SystemCleanupOpportunity> group)
    {
        var items = group.ToArray();
        return new SystemCleanupOpportunity
        {
            Kind = group.Key.Kind,
            Title = group.Key.Title,
            Path = items.Length == 1 ? items[0].Path : string.Empty,
            Handling = group.Key.Handling,
            SizeBytes = SaturatingSum(items.Select(item => item.SizeBytes)),
            FileCount = SaturatingSum(items.Select(item => item.FileCount)),
            ReviewableSizeBytes = SaturatingSum(items.Select(item => item.ReviewableSizeBytes)),
            ReviewableFileCount = SaturatingSum(items.Select(item => item.ReviewableFileCount)),
            RecentFileCount = SaturatingSum(items.Select(item => item.RecentFileCount)),
            AgeUnknownFileCount = SaturatingSum(items.Select(item => item.AgeUnknownFileCount)),
            ReviewAgeDays = items.Any(item => item.ReviewAgeDays == 0)
                ? 0
                : items.Min(item => item.ReviewAgeDays),
            HasAgeFilteredLocations = items.Any(item =>
                item.HasAgeFilteredLocations || item.ReviewAgeDays > 0),
            LocationCount = items.Length,
            IsSizeLowerBound = items.Any(item => item.IsSizeLowerBound),
            IsAccessible = items.Any(item => item.IsAccessible)
        };
    }

    private static bool IncludesFile(KnownCleanupStoreLocation location, FileInfo file) =>
        location.IncludedExtensions.Count == 0
        || location.IncludedExtensions.Any(extension =>
            file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase));

    private static void ClassifyAge(
        KnownCleanupStoreLocation location,
        FileInfo file,
        long length,
        ref long reviewableSizeBytes,
        ref int reviewableFileCount,
        ref int recentFileCount,
        ref int ageUnknownFileCount,
        ref bool hadAccessErrors)
    {
        if (location.Handling != CleanupHandling.UserReview)
            return;

        if (location.ReviewAgeDays <= 0)
        {
            reviewableSizeBytes = SaturatingAdd(reviewableSizeBytes, length);
            reviewableFileCount++;
            return;
        }

        DateTime lastWriteTimeUtc;
        try
        {
            lastWriteTimeUtc = file.LastWriteTimeUtc;
        }
        catch (Exception ex) when (IsExpectedFileSystemFailure(ex))
        {
            hadAccessErrors = true;
            ageUnknownFileCount++;
            return;
        }

        if (lastWriteTimeUtc == DateTime.MinValue)
        {
            ageUnknownFileCount++;
            return;
        }

        if (lastWriteTimeUtc <= DateTime.UtcNow.AddDays(-location.ReviewAgeDays))
        {
            reviewableSizeBytes = SaturatingAdd(reviewableSizeBytes, length);
            reviewableFileCount++;
        }
        else
        {
            recentFileCount++;
        }
    }

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

    private static long SaturatingSum(IEnumerable<long> values)
    {
        long total = 0;
        foreach (var value in values)
            total = SaturatingAdd(total, Math.Max(0, value));
        return total;
    }

    private static int SaturatingSum(IEnumerable<int> values)
    {
        var total = 0;
        foreach (var value in values)
        {
            var normalized = Math.Max(0, value);
            if (total > int.MaxValue - normalized)
                return int.MaxValue;
            total += normalized;
        }

        return total;
    }

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
        CleanupHandling handling,
        int reviewAgeDays = 0,
        bool recurseSubdirectories = true,
        IReadOnlyList<string>? includedExtensions = null)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            locations.Add(new KnownCleanupStoreLocation(kind, title, path, handling)
            {
                ReviewAgeDays = Math.Max(0, reviewAgeDays),
                RecurseSubdirectories = recurseSubdirectories,
                IncludedExtensions = includedExtensions ?? []
            });
        }
    }

    private static void AddBrowserCacheLocations(
        List<KnownCleanupStoreLocation> locations,
        string? localAppData)
    {
        AddChromiumBrowserCaches(
            locations,
            CombineIfPresent(localAppData, "Google", "Chrome", "User Data"));
        AddChromiumBrowserCaches(
            locations,
            CombineIfPresent(localAppData, "Microsoft", "Edge", "User Data"));
        AddChromiumBrowserCaches(
            locations,
            CombineIfPresent(localAppData, "BraveSoftware", "Brave-Browser", "User Data"));
    }

    private static void AddChromiumBrowserCaches(
        List<KnownCleanupStoreLocation> locations,
        string? userDataRoot)
    {
        if (string.IsNullOrWhiteSpace(userDataRoot))
            return;

        foreach (var cachePath in DiscoverChromiumProfileCacheLocations(userDataRoot))
        {
            Add(
                locations,
                KnownCleanupStoreKind.BrowserCache,
                "浏览器缓存",
                cachePath,
                CleanupHandling.UserReview,
                reviewAgeDays: 7);
        }
    }

    public static IReadOnlyList<string> DiscoverChromiumProfileCacheLocations(
        string userDataRoot)
    {
        if (string.IsNullOrWhiteSpace(userDataRoot))
            return [];

        var canonicalRoot = CanonicalPath(userDataRoot);
        var profiles = new List<string> { "Default" };
        if (Directory.Exists(canonicalRoot))
        {
            try
            {
                profiles.AddRange(new DirectoryInfo(canonicalRoot)
                    .EnumerateDirectories("*", new EnumerationOptions
                    {
                        AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.Device,
                        IgnoreInaccessible = false,
                        RecurseSubdirectories = false,
                        ReturnSpecialDirectories = false
                    })
                    .Where(directory =>
                        directory.Name.Equals("Guest Profile", StringComparison.OrdinalIgnoreCase)
                        || IsNumberedChromiumProfile(directory.Name))
                    .OrderBy(directory => directory.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(16)
                    .Select(directory => directory.Name));
            }
            catch (Exception ex) when (IsExpectedFileSystemFailure(ex))
            {
                // The default profile remains useful even when profile discovery is unavailable.
            }
        }

        return profiles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(profile => Path.Combine(canonicalRoot, profile, "Cache"))
            .ToArray();
    }

    private static bool IsNumberedChromiumProfile(string name) =>
        name.StartsWith("Profile ", StringComparison.OrdinalIgnoreCase)
        && int.TryParse(name.AsSpan("Profile ".Length), out var number)
        && number >= 0;

    private static void AddDeveloperCacheLocations(
        List<KnownCleanupStoreLocation> locations,
        string? localAppData)
    {
        Add(
            locations,
            KnownCleanupStoreKind.DeveloperToolCache,
            "开发工具下载缓存",
            CombineIfPresent(localAppData, "npm-cache", "_cacache"),
            CleanupHandling.UserReview,
            reviewAgeDays: 14);
        Add(
            locations,
            KnownCleanupStoreKind.DeveloperToolCache,
            "开发工具下载缓存",
            CombineIfPresent(localAppData, "pip", "Cache"),
            CleanupHandling.UserReview,
            reviewAgeDays: 14);
        Add(
            locations,
            KnownCleanupStoreKind.DeveloperToolCache,
            "开发工具下载缓存",
            CombineIfPresent(localAppData, "NuGet", "v3-cache"),
            CleanupHandling.UserReview,
            reviewAgeDays: 14);
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
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }
        catch
        {
            return path.Trim();
        }
    }

    private static bool IsStrictDescendant(string candidate, string parent)
    {
        if (candidate.Equals(parent, StringComparison.OrdinalIgnoreCase))
            return false;

        var parentWithSeparator = parent.EndsWith(Path.DirectorySeparatorChar)
            || parent.EndsWith(Path.AltDirectorySeparatorChar)
                ? parent
                : parent + Path.DirectorySeparatorChar;
        return candidate.StartsWith(parentWithSeparator, StringComparison.OrdinalIgnoreCase);
    }
}
