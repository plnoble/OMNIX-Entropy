using System.Diagnostics;
using System.IO;

namespace Css.App;

public sealed class PersonalStorageLocationOpenResult
{
    public required bool Opened { get; init; }
    public required string Message { get; init; }
}

public sealed class PersonalStorageExplorerLauncher
{
    private readonly string _windowsDirectory;
    private readonly Func<string, bool> _fileExists;
    private readonly Action<ProcessStartInfo> _startProcess;

    public PersonalStorageExplorerLauncher(
        string windowsDirectory,
        Func<string, bool> fileExists,
        Action<ProcessStartInfo> startProcess)
    {
        _windowsDirectory = windowsDirectory;
        _fileExists = fileExists ?? throw new ArgumentNullException(nameof(fileExists));
        _startProcess = startProcess ?? throw new ArgumentNullException(nameof(startProcess));
    }

    public static PersonalStorageExplorerLauncher CreateDefault() =>
        new(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            File.Exists,
            startInfo => Process.Start(startInfo));

    public PersonalStorageLocationOpenResult TryOpenSelectedLocation(
        string? requestedPath,
        IReadOnlyCollection<string> currentEvidencePaths)
    {
        ArgumentNullException.ThrowIfNull(currentEvidencePaths);

        var selected = TryNormalizeLocalFilePath(requestedPath);
        var belongsToCurrentEvidence = selected is not null
            && currentEvidencePaths
                .Select(TryNormalizeLocalFilePath)
                .Any(path => string.Equals(path, selected, StringComparison.OrdinalIgnoreCase));
        if (!belongsToCurrentEvidence || !ExistsSafely(selected!))
            return Refused();

        var windowsDirectory = TryNormalizeLocalDirectory(_windowsDirectory);
        if (windowsDirectory is null)
            return Unavailable();
        var explorerPath = Path.Combine(windowsDirectory, "explorer.exe");
        if (!ExistsSafely(explorerPath))
            return Unavailable();

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = explorerPath,
                UseShellExecute = true
            };
            startInfo.ArgumentList.Add("/select," + selected);
            _startProcess(startInfo);
            return new PersonalStorageLocationOpenResult
            {
                Opened = true,
                Message = "\u5df2\u8ba9 Windows \u8d44\u6e90\u7ba1\u7406\u5668\u5b9a\u4f4d\u8fd9\u4e2a\u6587\u4ef6\uff1bOMNIX-Entropy \u6ca1\u6709\u6253\u5f00\u3001\u79fb\u52a8\u6216\u5220\u9664\u5b83\u3002"
            };
        }
        catch
        {
            return Unavailable();
        }
    }

    private bool ExistsSafely(string path)
    {
        try { return _fileExists(path); }
        catch { return false; }
    }

    private static string? TryNormalizeLocalFilePath(string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)
                || path.StartsWith("\\\\", StringComparison.Ordinal)
                || !Path.IsPathFullyQualified(path))
            {
                return null;
            }

            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath);
            if (string.IsNullOrWhiteSpace(root)
                || string.Equals(
                    fullPath.TrimEnd(Path.DirectorySeparatorChar),
                    root.TrimEnd(Path.DirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase)
                || fullPath[root.Length..].Contains(':'))
            {
                return null;
            }

            return fullPath;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryNormalizeLocalDirectory(string? path)
    {
        var normalized = TryNormalizeLocalFilePath(
            string.IsNullOrWhiteSpace(path) ? null : Path.Combine(path, "placeholder"));
        return normalized is null ? null : Path.GetDirectoryName(normalized);
    }

    private static PersonalStorageLocationOpenResult Refused() =>
        new()
        {
            Opened = false,
            Message = "\u8fd9\u4e2a\u4f4d\u7f6e\u5df2\u53d8\u5316\u6216\u4e0d\u5c5e\u4e8e\u672c\u6b21\u4f53\u68c0\uff0c\u5df2\u505c\u6b62\u6253\u5f00\u3002\u8bf7\u91cd\u65b0\u4f53\u68c0\u540e\u518d\u67e5\u770b\u3002"
        };

    private static PersonalStorageLocationOpenResult Unavailable() =>
        new()
        {
            Opened = false,
            Message = "\u6682\u65f6\u65e0\u6cd5\u6253\u5f00 Windows \u8d44\u6e90\u7ba1\u7406\u5668\uff1b\u6ca1\u6709\u6253\u5f00\u3001\u79fb\u52a8\u6216\u5220\u9664\u6587\u4ef6\u3002"
        };
}
