using System.Security.Cryptography;
using Css.Core.Migration;

namespace Css.Win32.Migration;

public interface IWindowsDirectoryRedirector
{
    void Create(string originalPath, string destinationPath);
    bool TryGetTarget(string originalPath, out string? destinationPath);
    void Remove(string originalPath);
}

public interface IWindowsDirectoryCopyVerifier
{
    Task CopyVerifiedAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default);

    Task VerifyEqualAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default);
}

public sealed class WindowsDirectorySymbolicLinkRedirector : IWindowsDirectoryRedirector
{
    public void Create(string originalPath, string destinationPath)
    {
        var original = Path.GetFullPath(originalPath);
        var destination = Path.GetFullPath(destinationPath);
        if (Directory.Exists(original) || File.Exists(original))
            throw new IOException("The redirect source path already exists.");
        if (!Directory.Exists(destination))
            throw new DirectoryNotFoundException("The redirect destination is missing.");
        var parent = Path.GetDirectoryName(original);
        if (!string.IsNullOrWhiteSpace(parent))
            Directory.CreateDirectory(parent);
        Directory.CreateSymbolicLink(original, destination);
    }

    public bool TryGetTarget(string originalPath, out string? destinationPath)
    {
        destinationPath = null;
        try
        {
            var info = new DirectoryInfo(Path.GetFullPath(originalPath));
            if (!info.Exists && string.IsNullOrWhiteSpace(info.LinkTarget))
                return false;
            var target = info.LinkTarget;
            if (string.IsNullOrWhiteSpace(target))
                return false;
            destinationPath = Path.GetFullPath(
                Path.IsPathRooted(target)
                    ? target
                    : Path.Combine(info.Parent?.FullName ?? string.Empty, target));
            return true;
        }
        catch
        {
            destinationPath = null;
            return false;
        }
    }

    public void Remove(string originalPath)
    {
        var full = Path.GetFullPath(originalPath);
        var attributes = File.GetAttributes(full);
        if (!attributes.HasFlag(FileAttributes.Directory)
            || !attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException("The path is not a directory redirect.");
        Directory.Delete(full, recursive: false);
    }
}

public sealed class WindowsDirectoryCopyVerifier : IWindowsDirectoryCopyVerifier
{
    private const int MaximumFiles = 100_000;
    private const int MaximumDirectories = 25_000;
    private const long MaximumBytes = 2L * 1024 * 1024 * 1024 * 1024;

    public async Task CopyVerifiedAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var source = RequireRealDirectory(sourcePath, "source");
        var destination = Path.GetFullPath(destinationPath);
        if (Directory.Exists(destination) || File.Exists(destination))
            throw new IOException("The copy destination already exists.");
        var parent = Path.GetDirectoryName(destination)
            ?? throw new IOException("The copy destination parent is unavailable.");
        Directory.CreateDirectory(parent);
        Directory.CreateDirectory(destination);

        try
        {
            var tree = EnumerateTree(source);
            foreach (var directory in tree.Directories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(Path.Combine(destination, directory));
            }
            foreach (var file in tree.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourceFile = Path.Combine(source, file.RelativePath);
                var destinationFile = Path.Combine(destination, file.RelativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrWhiteSpace(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);
                await using var input = new FileStream(
                    sourceFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    1024 * 1024,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                await using var output = new FileStream(
                    destinationFile,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    1024 * 1024,
                    FileOptions.Asynchronous | FileOptions.WriteThrough);
                await input.CopyToAsync(output, 1024 * 1024, cancellationToken);
                await output.FlushAsync(cancellationToken);
            }
            await VerifyEqualAsync(source, destination, cancellationToken);
        }
        catch
        {
            DeleteCreatedDirectory(destination);
            throw;
        }
    }

    public async Task VerifyEqualAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var source = RequireRealDirectory(sourcePath, "source");
        var destination = RequireRealDirectory(destinationPath, "destination");
        var sourceTree = EnumerateTree(source);
        var destinationTree = EnumerateTree(destination);
        if (!sourceTree.Directories.SequenceEqual(
                destinationTree.Directories,
                StringComparer.OrdinalIgnoreCase)
            || sourceTree.Files.Count != destinationTree.Files.Count)
            throw new IOException("Copied directory structure does not match the source.");

        for (var index = 0; index < sourceTree.Files.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceFile = sourceTree.Files[index];
            var destinationFile = destinationTree.Files[index];
            if (!string.Equals(
                    sourceFile.RelativePath,
                    destinationFile.RelativePath,
                    StringComparison.OrdinalIgnoreCase)
                || sourceFile.Length != destinationFile.Length)
                throw new IOException("Copied file inventory does not match the source.");

            var sourceHash = await HashFileAsync(
                Path.Combine(source, sourceFile.RelativePath),
                cancellationToken);
            var destinationHash = await HashFileAsync(
                Path.Combine(destination, destinationFile.RelativePath),
                cancellationToken);
            try
            {
                if (!CryptographicOperations.FixedTimeEquals(sourceHash, destinationHash))
                    throw new IOException("Copied file hash does not match the source.");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(sourceHash);
                CryptographicOperations.ZeroMemory(destinationHash);
            }
        }
    }

    private static DirectoryTree EnumerateTree(string root)
    {
        var directories = new List<string>();
        var files = new List<DirectoryFile>();
        var pending = new Queue<string>();
        pending.Enqueue(root);
        long totalBytes = 0;

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var path in Directory.EnumerateFileSystemEntries(
                         current,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                var attributes = File.GetAttributes(path);
                if (attributes.HasFlag(FileAttributes.ReparsePoint))
                    throw new IOException("Migration does not follow reparse points.");
                var relative = Path.GetRelativePath(root, path);
                if (relative.StartsWith("..", StringComparison.Ordinal)
                    || Path.IsPathRooted(relative))
                    throw new IOException("Migration traversal escaped the source root.");
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    directories.Add(relative);
                    if (directories.Count > MaximumDirectories)
                        throw new IOException("Migration directory count exceeds the safety limit.");
                    pending.Enqueue(path);
                }
                else
                {
                    var length = new FileInfo(path).Length;
                    totalBytes = checked(totalBytes + length);
                    if (totalBytes > MaximumBytes || files.Count >= MaximumFiles)
                        throw new IOException("Migration file count or size exceeds the safety limit.");
                    files.Add(new DirectoryFile(relative, length));
                }
            }
        }

        directories.Sort(StringComparer.OrdinalIgnoreCase);
        files.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(
            left.RelativePath,
            right.RelativePath));
        return new DirectoryTree(directories, files);
    }

    private static async Task<byte[]> HashFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            1024 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await SHA256.HashDataAsync(stream, cancellationToken);
    }

    private static string RequireRealDirectory(string path, string role)
    {
        var full = Path.GetFullPath(path);
        if (!Directory.Exists(full))
            throw new DirectoryNotFoundException($"The migration {role} directory is missing.");
        var attributes = File.GetAttributes(full);
        if (!attributes.HasFlag(FileAttributes.Directory)
            || attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException($"The migration {role} must be a real directory.");
        return full;
    }

    private static void DeleteCreatedDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return;
            var attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new IOException("Refusing to recursively delete a redirect.");
            Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Preserve the original copy/verification exception.
        }
    }

    private sealed record DirectoryFile(string RelativePath, long Length);
    private sealed record DirectoryTree(
        IReadOnlyList<string> Directories,
        IReadOnlyList<DirectoryFile> Files);
}

public sealed class WindowsMigrationPathObserver : IMigrationPathObserver
{
    public Task<MigrationPathObservation> ObserveAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = Path.GetFullPath(path);
        var directory = new DirectoryInfo(full);

        try
        {
            var linkTarget = directory.LinkTarget;
            if (!string.IsNullOrWhiteSpace(linkTarget))
            {
                var resolvedTarget = Path.GetFullPath(
                    Path.IsPathRooted(linkTarget)
                        ? linkTarget
                        : Path.Combine(directory.Parent?.FullName ?? string.Empty, linkTarget));
                return Task.FromResult(new MigrationPathObservation
                {
                    Path = full,
                    Exists = true,
                    IsDirectory = true,
                    IsRedirect = true,
                    RedirectTarget = resolvedTarget
                });
            }
        }
        catch (IOException)
        {
            // Continue with existence checks so an unreadable redirect is never reported healthy.
        }
        catch (UnauthorizedAccessException)
        {
            // Continue with existence checks so an unreadable redirect is never reported healthy.
        }

        if (Directory.Exists(full))
        {
            var attributes = File.GetAttributes(full);
            return Task.FromResult(new MigrationPathObservation
            {
                Path = full,
                Exists = true,
                IsDirectory = true,
                IsRedirect = attributes.HasFlag(FileAttributes.ReparsePoint)
            });
        }

        return Task.FromResult(new MigrationPathObservation
        {
            Path = full,
            Exists = File.Exists(full),
            IsDirectory = false
        });
    }
}

public sealed class WindowsDirectoryMigrationPathAdapter : IMigrationPathAdapter
{
    private readonly WindowsMigrationPathPolicy _policy;
    private readonly IWindowsDirectoryRedirector _redirects;
    private readonly IWindowsDirectoryCopyVerifier _copies;

    public WindowsDirectoryMigrationPathAdapter(
        WindowsMigrationPathPolicy policy,
        IWindowsDirectoryRedirector redirects,
        IWindowsDirectoryCopyVerifier? copies = null)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _redirects = redirects ?? throw new ArgumentNullException(nameof(redirects));
        _copies = copies ?? new WindowsDirectoryCopyVerifier();
    }

    public Task<MigrationPathObservation> ObserveAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var full = Path.GetFullPath(path);
        if (_redirects.TryGetTarget(full, out var target))
        {
            return Task.FromResult(new MigrationPathObservation
            {
                Path = full,
                Exists = true,
                IsDirectory = true,
                IsRedirect = true,
                RedirectTarget = target
            });
        }
        if (Directory.Exists(full))
        {
            var attributes = File.GetAttributes(full);
            return Task.FromResult(new MigrationPathObservation
            {
                Path = full,
                Exists = true,
                IsDirectory = true,
                IsRedirect = attributes.HasFlag(FileAttributes.ReparsePoint)
            });
        }
        return Task.FromResult(new MigrationPathObservation
        {
            Path = full,
            Exists = File.Exists(full),
            IsDirectory = false
        });
    }

    public async Task<MigrationMoveResult> MoveAndRedirectAsync(
        MigrationRollbackManifestEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _ = _policy;
        var source = Path.GetFullPath(entry.OriginalPath);
        var destination = Path.GetFullPath(entry.PlannedDestinationPath);
        RequireNoCollision(destination);
        var staging = destination + ".omnix-stage-" + Guid.NewGuid().ToString("N");
        try
        {
            await _copies.CopyVerifiedAsync(source, staging, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            RequireNoCollision(destination);
            Directory.Move(staging, destination);
            await _copies.VerifyEqualAsync(source, destination, cancellationToken);
            Directory.Delete(source, recursive: true);
            _redirects.Create(source, destination);
            if (!_redirects.TryGetTarget(source, out var actualTarget)
                || !PathsEqual(actualTarget, destination))
                throw new IOException("The migration redirect could not be verified.");
            return new MigrationMoveResult
            {
                OriginalPath = source,
                DestinationPath = destination,
                RedirectCreated = true
            };
        }
        finally
        {
            DeleteStagingSafely(staging);
        }
    }

    public async Task RollbackAsync(
        MigrationRollbackManifestEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var original = Path.GetFullPath(entry.OriginalPath);
        var destination = Path.GetFullPath(entry.PlannedDestinationPath);
        if (_redirects.TryGetTarget(original, out _))
            _redirects.Remove(original);

        var restoreStaging = original + ".omnix-restore-" + Guid.NewGuid().ToString("N");
        try
        {
            if (!Directory.Exists(original))
            {
                if (!Directory.Exists(destination))
                    throw new IOException("Neither the original nor migrated directory is available for rollback.");
                await _copies.CopyVerifiedAsync(destination, restoreStaging, cancellationToken);
                var parent = Path.GetDirectoryName(original);
                if (!string.IsNullOrWhiteSpace(parent))
                    Directory.CreateDirectory(parent);
                Directory.Move(restoreStaging, original);
            }

            if (Directory.Exists(destination))
            {
                await _copies.VerifyEqualAsync(original, destination, cancellationToken);
                Directory.Delete(destination, recursive: true);
            }
        }
        finally
        {
            DeleteStagingSafely(restoreStaging);
        }
    }

    private static void RequireNoCollision(string path)
    {
        if (Directory.Exists(path) || File.Exists(path))
            throw new IOException("The migration destination or staging path already exists.");
    }

    private static void DeleteStagingSafely(string path)
    {
        if (!Directory.Exists(path))
            return;
        var attributes = File.GetAttributes(path);
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException("Refusing to recursively delete a staging redirect.");
        Directory.Delete(path, recursive: true);
    }

    private static bool PathsEqual(string? left, string right)
    {
        if (string.IsNullOrWhiteSpace(left))
            return false;
        return string.Equals(
            Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);
    }
}
