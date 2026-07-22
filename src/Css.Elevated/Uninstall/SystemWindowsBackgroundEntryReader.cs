using Microsoft.Win32;

namespace Css.Elevated.Uninstall;

public sealed class SystemWindowsBackgroundEntryReader : IWindowsBackgroundEntryReader
{
    private static readonly (RegistryKey Root, string Path)[] StartupLocations =
    [
        (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run"),
        (Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run"),
        (Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run")
    ];

    private readonly string _tasksRoot;

    public SystemWindowsBackgroundEntryReader(string? tasksRoot = null)
    {
        _tasksRoot = Path.GetFullPath(tasksRoot ?? DefaultTasksRoot());
    }

    public WindowsBackgroundEntryState ProbeStartupEntry(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.IndexOf('\0') >= 0)
            return WindowsBackgroundEntryState.Unknown;

        var accessFailed = false;
        foreach (var (root, path) in StartupLocations)
        {
            try
            {
                using var key = root.OpenSubKey(path, writable: false);
                if (key?.GetValueNames().Contains(name, StringComparer.OrdinalIgnoreCase) == true)
                    return WindowsBackgroundEntryState.Exists;
            }
            catch
            {
                accessFailed = true;
            }
        }

        return accessFailed
            ? WindowsBackgroundEntryState.Unknown
            : WindowsBackgroundEntryState.Missing;
    }

    public WindowsBackgroundEntryState ProbeService(string name)
    {
        if (!IsSafeRegistrySubKeyName(name))
            return WindowsBackgroundEntryState.Unknown;

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\" + name,
                writable: false);
            return key is null
                ? WindowsBackgroundEntryState.Missing
                : WindowsBackgroundEntryState.Exists;
        }
        catch
        {
            return WindowsBackgroundEntryState.Unknown;
        }
    }

    public WindowsBackgroundEntryState ProbeScheduledTask(string name)
    {
        if (!TryResolveTaskPath(name, out var taskPath))
            return WindowsBackgroundEntryState.Unknown;

        try
        {
            if (!Directory.Exists(_tasksRoot))
                return WindowsBackgroundEntryState.Unknown;

            var relative = Path.GetRelativePath(_tasksRoot, taskPath);
            var segments = relative.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);
            var current = _tasksRoot;
            for (var index = 0; index < segments.Length; index++)
            {
                current = Path.Combine(current, segments[index]);
                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(current);
                }
                catch (FileNotFoundException)
                {
                    return WindowsBackgroundEntryState.Missing;
                }
                catch (DirectoryNotFoundException)
                {
                    return WindowsBackgroundEntryState.Missing;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    return WindowsBackgroundEntryState.Unknown;
            }

            return WindowsBackgroundEntryState.Exists;
        }
        catch
        {
            return WindowsBackgroundEntryState.Unknown;
        }
    }

    private bool TryResolveTaskPath(string name, out string taskPath)
    {
        taskPath = string.Empty;
        if (string.IsNullOrWhiteSpace(name) || name.IndexOf('\0') >= 0)
            return false;

        var segments = name
            .TrimStart('\\', '/')
            .Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
            return false;

        try
        {
            var candidate = Path.GetFullPath(Path.Combine([_tasksRoot, .. segments]));
            var rootPrefix = _tasksRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            taskPath = candidate;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsSafeRegistrySubKeyName(string name) =>
        !string.IsNullOrWhiteSpace(name)
        && name.IndexOfAny(['\\', '/', '\0']) < 0;

    private static string DefaultTasksRoot()
    {
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (string.IsNullOrWhiteSpace(windows))
            windows = Environment.GetEnvironmentVariable("WINDIR") ?? @"C:\Windows";
        return Path.Combine(windows, "System32", "Tasks");
    }
}
