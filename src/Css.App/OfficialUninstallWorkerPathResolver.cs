using System.IO;

namespace Css.App;

public enum OfficialUninstallWorkerAvailabilityStatus
{
    ReadyForVerification,
    Missing,
    UnsafePath,
    ProbeFailed
}

public sealed class OfficialUninstallWorkerAvailability
{
    public required OfficialUninstallWorkerAvailabilityStatus Status { get; init; }
    public string? ExecutablePath { get; init; }
    public bool CanLaunchVerification =>
        Status == OfficialUninstallWorkerAvailabilityStatus.ReadyForVerification
        && !string.IsNullOrWhiteSpace(ExecutablePath);
}

public static class OfficialUninstallWorkerPathResolver
{
    public const string WorkerFileName = "Css.Elevated.exe";

    public static OfficialUninstallWorkerAvailability Resolve(
        string applicationBaseDirectory,
        Func<string, bool>? fileExists = null,
        Func<string, FileAttributes>? attributes = null)
    {
        if (string.IsNullOrWhiteSpace(applicationBaseDirectory))
            return Result(OfficialUninstallWorkerAvailabilityStatus.UnsafePath);

        string baseDirectory;
        string workerPath;
        try
        {
            baseDirectory = Path.GetFullPath(applicationBaseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            workerPath = Path.GetFullPath(Path.Combine(baseDirectory, WorkerFileName));
        }
        catch
        {
            return Result(OfficialUninstallWorkerAvailabilityStatus.UnsafePath);
        }

        if (!string.Equals(
                Path.GetDirectoryName(workerPath)?.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar),
                baseDirectory,
                StringComparison.OrdinalIgnoreCase)
            || !string.Equals(
                Path.GetFileName(workerPath),
                WorkerFileName,
                StringComparison.Ordinal))
        {
            return Result(OfficialUninstallWorkerAvailabilityStatus.UnsafePath);
        }

        try
        {
            if (!(fileExists ?? File.Exists)(workerPath))
                return Result(OfficialUninstallWorkerAvailabilityStatus.Missing);
            var workerAttributes = (attributes ?? File.GetAttributes)(workerPath);
            if ((workerAttributes & FileAttributes.ReparsePoint) != 0)
                return Result(OfficialUninstallWorkerAvailabilityStatus.UnsafePath);
        }
        catch
        {
            return Result(OfficialUninstallWorkerAvailabilityStatus.ProbeFailed);
        }

        return new OfficialUninstallWorkerAvailability
        {
            Status = OfficialUninstallWorkerAvailabilityStatus.ReadyForVerification,
            ExecutablePath = workerPath
        };
    }

    private static OfficialUninstallWorkerAvailability Result(
        OfficialUninstallWorkerAvailabilityStatus status) =>
        new() { Status = status };
}
