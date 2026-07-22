using Css.Core.Migration;

namespace Css.Win32.Migration;

public sealed class WindowsMigrationSnapshotSourceReader : IMigrationSnapshotSourceReader
{
    private const int MaximumFiles = 100_000;
    private const int MaximumDirectories = 25_000;
    private const long MaximumBytes = 2L * 1024 * 1024 * 1024 * 1024;

    public Task<MigrationSnapshotSourceEvidence> ObserveAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = Path.GetFullPath(path);
        if (!Directory.Exists(full))
        {
            return Task.FromResult(new MigrationSnapshotSourceEvidence
            {
                Path = full,
                Exists = File.Exists(full),
                IsDirectory = false
            });
        }

        var rootAttributes = File.GetAttributes(full);
        if (rootAttributes.HasFlag(FileAttributes.ReparsePoint))
        {
            return Task.FromResult(new MigrationSnapshotSourceEvidence
            {
                Path = full,
                Exists = true,
                IsDirectory = true,
                IsRedirect = true
            });
        }

        var pending = new Queue<string>();
        pending.Enqueue(full);
        var directoryCount = 1;
        var fileCount = 0;
        long bytes = 0;
        DateTimeOffset? latestWrite = Directory.GetLastWriteTimeUtc(full);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = pending.Dequeue();
            foreach (var entry in Directory.EnumerateFileSystemEntries(
                         current,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attributes = File.GetAttributes(entry);
                if (attributes.HasFlag(FileAttributes.ReparsePoint))
                    throw new IOException("Migration snapshot does not follow reparse points.");
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    directoryCount++;
                    if (directoryCount > MaximumDirectories)
                        throw new IOException("Migration snapshot directory count exceeds the safety limit.");
                    pending.Enqueue(entry);
                    latestWrite = Latest(latestWrite, Directory.GetLastWriteTimeUtc(entry));
                }
                else
                {
                    fileCount++;
                    if (fileCount > MaximumFiles)
                        throw new IOException("Migration snapshot file count exceeds the safety limit.");
                    var info = new FileInfo(entry);
                    bytes = checked(bytes + info.Length);
                    if (bytes > MaximumBytes)
                        throw new IOException("Migration snapshot size exceeds the safety limit.");
                    latestWrite = Latest(latestWrite, info.LastWriteTimeUtc);
                }
            }
        }

        return Task.FromResult(new MigrationSnapshotSourceEvidence
        {
            Path = full,
            Exists = true,
            IsDirectory = true,
            IsRedirect = false,
            ObservedBytes = bytes,
            LastWriteUtc = latestWrite
        });
    }

    private static DateTimeOffset? Latest(DateTimeOffset? current, DateTime candidate) =>
        !current.HasValue || candidate > current.Value.UtcDateTime
            ? new DateTimeOffset(candidate, TimeSpan.Zero)
            : current;
}
