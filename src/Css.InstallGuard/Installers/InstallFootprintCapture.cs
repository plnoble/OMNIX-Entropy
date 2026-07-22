namespace Css.InstallGuard.Installers;

public enum InstallFootprintCaptureStatus
{
    Complete,
    Truncated,
    Unavailable
}

public sealed record InstallFootprintCapture
{
    public required InstallFootprintCaptureStatus Status { get; init; }
    public required IReadOnlyList<string> Paths { get; init; }

    public static InstallFootprintCapture EmptyComplete { get; } =
        new() { Status = InstallFootprintCaptureStatus.Complete, Paths = [] };
}

public sealed class WindowsInstallFootprintProbe
{
    public const int MaximumEntries = 4096;
    private const int MaximumRoots = 8;

    public InstallFootprintCapture Capture()
    {
        if (!OperatingSystem.IsWindows())
            return Unavailable();

        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        }
            .Where(root => TryNormalizeLocalCPath(root, out _))
            .ToArray();
        return CaptureRoots(roots);
    }

    public InstallFootprintCapture CaptureRoots(
        IEnumerable<string> roots,
        int maximumEntries = MaximumEntries)
    {
        ArgumentNullException.ThrowIfNull(roots);
        if (maximumEntries is < 1 or > MaximumEntries)
            throw new ArgumentOutOfRangeException(nameof(maximumEntries));

        var normalizedRoots = new List<string>();
        var observedRoots = 0;
        try
        {
            foreach (var root in roots)
            {
                if (++observedRoots > MaximumRoots)
                    return Unavailable();
                if (string.IsNullOrWhiteSpace(root))
                    continue;
                if (!TryNormalizeLocalCPath(root, out var normalized))
                    return Unavailable();
                if (!normalizedRoots.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                    normalizedRoots.Add(normalized);
            }
        }
        catch
        {
            return Unavailable();
        }
        if (normalizedRoots.Count == 0)
            return Unavailable();

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var incomplete = false;
        foreach (var root in normalizedRoots)
        {
            try
            {
                if (!Directory.Exists(root)
                    || File.GetAttributes(root).HasFlag(FileAttributes.ReparsePoint))
                {
                    incomplete = true;
                    continue;
                }

                foreach (var entry in Directory.EnumerateFileSystemEntries(
                             root,
                             "*",
                             SearchOption.TopDirectoryOnly))
                {
                    if (paths.Count >= maximumEntries)
                    {
                        return new InstallFootprintCapture
                        {
                            Status = InstallFootprintCaptureStatus.Truncated,
                            Paths = paths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()
                        };
                    }

                    try
                    {
                        var attributes = File.GetAttributes(entry);
                        if (attributes.HasFlag(FileAttributes.ReparsePoint)
                            || !TryNormalizeLocalCPath(entry, out var normalizedEntry))
                            continue;
                        paths.Add(normalizedEntry);
                    }
                    catch
                    {
                        incomplete = true;
                    }
                }
            }
            catch
            {
                incomplete = true;
            }
        }

        return new InstallFootprintCapture
        {
            Status = incomplete
                ? InstallFootprintCaptureStatus.Unavailable
                : InstallFootprintCaptureStatus.Complete,
            Paths = paths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()
        };
    }

    private static bool TryNormalizeLocalCPath(string path, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(path)
            || path.Length > 1024
            || path.StartsWith(@"\\", StringComparison.Ordinal))
            return false;

        try
        {
            var full = Path.GetFullPath(path);
            if (!string.Equals(Path.GetPathRoot(full), @"C:\", StringComparison.OrdinalIgnoreCase))
                return false;
            normalized = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return !string.Equals(normalized, "C:", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static InstallFootprintCapture Unavailable() =>
        new() { Status = InstallFootprintCaptureStatus.Unavailable, Paths = [] };
}
