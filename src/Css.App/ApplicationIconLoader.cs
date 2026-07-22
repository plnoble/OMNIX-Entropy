using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Css.App;

internal static class ApplicationIconLoader
{
    private const int MaximumPathLength = 1024;
    private const long MaximumRasterBytes = 16L * 1024 * 1024;
    private const int MaximumCacheEntries = 256;
    private static readonly object CacheGate = new();
    private static readonly Dictionary<IconCacheKey, ImageSource?> Cache = new();
    private static readonly HashSet<string> NativeIconExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe",
        ".dll",
        ".ico"
    };
    private static readonly HashSet<string> RasterIconExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif"
    };

    public static ImageSource? TryLoad(string? path, int resourceIndex)
    {
        var localPath = ResolveSafeLocalPath(path);
        if (localPath is null)
            return null;

        IconCacheKey cacheKey;
        try
        {
            var file = new FileInfo(localPath);
            cacheKey = new IconCacheKey(
                localPath,
                resourceIndex,
                file.Length,
                file.LastWriteTimeUtc.Ticks);
        }
        catch
        {
            return null;
        }

        lock (CacheGate)
        {
            if (Cache.TryGetValue(cacheKey, out var cached))
                return cached;
        }

        var extension = Path.GetExtension(localPath);
        ImageSource? loaded;
        if (NativeIconExtensions.Contains(extension))
            loaded = TryLoadNativeIcon(localPath, resourceIndex);
        else if (RasterIconExtensions.Contains(extension))
            loaded = TryLoadRaster(localPath);
        else
            loaded = null;

        lock (CacheGate)
        {
            if (Cache.Count >= MaximumCacheEntries && !Cache.ContainsKey(cacheKey))
                Cache.Clear();
            Cache[cacheKey] = loaded;
        }

        return loaded;
    }

    private static string? ResolveSafeLocalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)
            || path.Length > MaximumPathLength
            || path.Any(char.IsControl)
            || !IsLocalDrivePath(path))
        {
            return null;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath);
            if (string.IsNullOrWhiteSpace(root)
                || new DriveInfo(root).DriveType != DriveType.Fixed
                || !File.Exists(fullPath)
                || HasReparsePoint(fullPath))
            {
                return null;
            }
        }
        catch
        {
            return null;
        }

        return fullPath;
    }

    private static bool HasReparsePoint(string fullPath)
    {
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
            return true;

        var directory = Directory.GetParent(fullPath);
        while (directory is not null)
        {
            if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                return true;
            directory = directory.Parent;
        }

        return false;
    }

    private static ImageSource? TryLoadRaster(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length <= 0 || fileInfo.Length > MaximumRasterBytes)
                return null;

            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 64;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    private static ImageSource? TryLoadNativeIcon(string path, int resourceIndex)
    {
        var largeIcons = new IntPtr[1];
        try
        {
            var count = ExtractIconEx(path, resourceIndex, largeIcons, null, 1);
            if (count == 0 || largeIcons[0] == IntPtr.Zero)
                return null;

            var source = Imaging.CreateBitmapSourceFromHIcon(
                largeIcons[0],
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(56, 56));
            source.Freeze();
            return source;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (largeIcons[0] != IntPtr.Zero)
                DestroyIcon(largeIcons[0]);
        }
    }

    private static bool IsLocalDrivePath(string path) =>
        path.Length >= 3
        && char.IsAsciiLetter(path[0])
        && path[1] == ':'
        && (path[2] == '\\' || path[2] == '/');

    private readonly record struct IconCacheKey(
        string Path,
        int ResourceIndex,
        long FileLength,
        long LastWriteTicks);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint ExtractIconEx(
        string fileName,
        int iconIndex,
        [Out] IntPtr[] largeIcons,
        [Out] IntPtr[]? smallIcons,
        uint iconCount);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr iconHandle);
}
