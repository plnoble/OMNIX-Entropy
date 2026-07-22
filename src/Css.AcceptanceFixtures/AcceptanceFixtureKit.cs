using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace Css.AcceptanceFixtures;

public enum AcceptanceFixtureRole
{
    Uninstall,
    Migration
}

public sealed record AcceptanceFixturePayloadFile(
    string SourcePath,
    string RelativePath);

public sealed class AcceptanceFixturePayload
{
    private static readonly HashSet<string> RequiredFiles = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "Css.AcceptanceFixtures.exe",
        "Css.AcceptanceFixtures.dll",
        "Css.AcceptanceFixtures.deps.json",
        "Css.AcceptanceFixtures.runtimeconfig.json"
    };

    public AcceptanceFixturePayload(IReadOnlyList<AcceptanceFixturePayloadFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        var normalized = new List<AcceptanceFixturePayloadFile>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.SourcePath) ||
                string.IsNullOrWhiteSpace(file.RelativePath) ||
                Path.IsPathRooted(file.RelativePath) ||
                file.RelativePath.Contains(Path.DirectorySeparatorChar) ||
                file.RelativePath.Contains(Path.AltDirectorySeparatorChar) ||
                !RequiredFiles.Contains(file.RelativePath) ||
                !names.Add(file.RelativePath))
            {
                throw new ArgumentException("Fixture payload inventory is invalid.", nameof(files));
            }
            normalized.Add(file);
        }
        if (!RequiredFiles.SetEquals(names))
            throw new ArgumentException("Fixture payload inventory is incomplete.", nameof(files));
        Files = normalized;
    }

    public IReadOnlyList<AcceptanceFixturePayloadFile> Files { get; }

    public static AcceptanceFixturePayload Discover(string directory)
    {
        var root = Path.GetFullPath(directory);
        return new AcceptanceFixturePayload(RequiredFiles
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(name => new AcceptanceFixturePayloadFile(
                Path.Combine(root, name),
                name))
            .ToArray());
    }
}

public sealed class AcceptanceFixtureLayout
{
    private const string RootName = "OMNIX-Entropy-Acceptance";
    private const string OwnerFileName = ".omnix-acceptance-owner.json";

    private AcceptanceFixtureLayout(
        string sessionId,
        string systemDriveRoot,
        string dataDriveRoot,
        string localAppDataRoot,
        string cleanupTempRoot)
    {
        SessionId = sessionId;
        ShortId = sessionId[..8];
        UninstallDisplayName = $"OMNIX Acceptance Uninstall Fixture {ShortId}";
        MigrationDisplayName = $"OMNIX Acceptance Migration Fixture {ShortId}";

        CSessionRoot = Path.Combine(systemDriveRoot, RootName, sessionId);
        UninstallInstallRoot = Path.Combine(CSessionRoot, "UninstallFixture", "Install");
        MigrationInstallRoot = Path.Combine(CSessionRoot, "MigrationFixture", "Install");
        LocalDataRoot = Path.Combine(localAppDataRoot, UninstallDisplayName);
        CacheRoot = Path.Combine(LocalDataRoot, "Cache");
        TempRoot = cleanupTempRoot;
        MigrationDestinationInstallRoot = Path.Combine(
            dataDriveRoot,
            "Software",
            MigrationDisplayName,
            "Install");

        CSessionOwnershipMarker = Path.Combine(CSessionRoot, OwnerFileName);
        UninstallInstallOwnershipMarker = Path.Combine(UninstallInstallRoot, OwnerFileName);
        MigrationInstallOwnershipMarker = Path.Combine(MigrationInstallRoot, OwnerFileName);
        LocalDataOwnershipMarker = Path.Combine(LocalDataRoot, OwnerFileName);
        TempOwnershipMarker = Path.Combine(TempRoot, OwnerFileName);
        MigrationDestinationOwnershipMarker = Path.Combine(
            MigrationDestinationInstallRoot,
            OwnerFileName);

        UninstallExecutable = Path.Combine(
            UninstallInstallRoot,
            "Css.AcceptanceFixtures.exe");
        MigrationExecutable = Path.Combine(
            MigrationInstallRoot,
            "Css.AcceptanceFixtures.exe");
        CacheFixtureFile = Path.Combine(CacheRoot, "cache-fixture.bin");
        TempFixtureFile = Path.Combine(TempRoot, "cleanup-fixture.tmp");
        FailureLockFile = Path.Combine(MigrationInstallRoot, "failure-lock.bin");
        UninstallResidueMarker = Path.Combine(
            UninstallInstallRoot,
            "uninstalled-residue.json");

        UninstallRegistryKeyName = $"OMNIX-Entropy-Acceptance-Uninstall-{ShortId}";
        MigrationRegistryKeyName = $"OMNIX-Entropy-Acceptance-Migration-{ShortId}";
        StartupValueName = $"OMNIX-Entropy-Acceptance-Startup-{ShortId}";
    }

    public string SessionId { get; }
    public string ShortId { get; }
    public string UninstallDisplayName { get; }
    public string MigrationDisplayName { get; }
    public string CSessionRoot { get; }
    public string UninstallInstallRoot { get; }
    public string MigrationInstallRoot { get; }
    public string LocalDataRoot { get; }
    public string CacheRoot { get; }
    public string TempRoot { get; }
    public string MigrationDestinationInstallRoot { get; }
    public string CSessionOwnershipMarker { get; }
    public string UninstallInstallOwnershipMarker { get; }
    public string MigrationInstallOwnershipMarker { get; }
    public string LocalDataOwnershipMarker { get; }
    public string TempOwnershipMarker { get; }
    public string MigrationDestinationOwnershipMarker { get; }
    public string UninstallExecutable { get; }
    public string MigrationExecutable { get; }
    public string CacheFixtureFile { get; }
    public string TempFixtureFile { get; }
    public string FailureLockFile { get; }
    public string UninstallResidueMarker { get; }
    public string UninstallRegistryKeyName { get; }
    public string MigrationRegistryKeyName { get; }
    public string StartupValueName { get; }

    public string ExpectedStartupCommand =>
        Quote(UninstallExecutable) + " status --session-id " + SessionId;

    public string ExpectedUninstallCommand(AcceptanceFixtureRole role)
    {
        var executable = role == AcceptanceFixtureRole.Uninstall
            ? UninstallExecutable
            : MigrationExecutable;
        return Quote(executable) +
            " uninstall --session-id " + SessionId +
            " --role " + role.ToString().ToLowerInvariant() +
            " --attestation " + Quote(AcceptanceFixtureAuthority.RequiredAttestation);
    }

    public static AcceptanceFixtureLayout Create(
        string sessionId,
        string systemDriveRoot,
        string dataDriveRoot,
        string localAppDataRoot,
        string cleanupTempRoot)
    {
        if (!Guid.TryParseExact(sessionId, "D", out var parsed) ||
            !string.Equals(parsed.ToString("D"), sessionId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Session id must be a canonical lowercase GUID.",
                nameof(sessionId));
        }

        var system = RequireRoot(systemDriveRoot, nameof(systemDriveRoot));
        var data = RequireRoot(dataDriveRoot, nameof(dataDriveRoot));
        if (string.Equals(system, data, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("System and data drive roots must differ.");
        var local = RequireAbsoluteDirectory(localAppDataRoot, nameof(localAppDataRoot));
        var temp = RequireAbsoluteDirectory(cleanupTempRoot, nameof(cleanupTempRoot));
        var expectedTemp = Path.Combine(system, "Temp");
        if (!IsInside(system, local) ||
            !string.Equals(temp, expectedTemp, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Local data must be on the system drive and cleanup temp must be its exact Temp root.");
        }

        return new AcceptanceFixtureLayout(sessionId, system, data, local, temp);
    }

    public static AcceptanceFixtureLayout CreateForWindows(string sessionId)
    {
        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory)
            ?? throw new InvalidOperationException("Windows system drive is unavailable.");
        const string dataRoot = @"D:\";
        var systemDrive = new DriveInfo(systemRoot);
        var dataDrive = new DriveInfo(dataRoot);
        if (systemDrive.DriveType != DriveType.Fixed ||
            !dataDrive.IsReady ||
            dataDrive.DriveType != DriveType.Fixed)
        {
            throw new InvalidOperationException("Acceptance fixtures require fixed C and D drives.");
        }
        return Create(
            sessionId,
            systemRoot,
            dataRoot,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Path.Combine(systemRoot, "Temp"));
    }

    private static string RequireRoot(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathRooted(value))
            throw new ArgumentException("Drive root must be absolute.", parameterName);
        var full = Path.GetFullPath(value);
        if (!string.Equals(
                full.TrimEnd(Path.DirectorySeparatorChar),
                Path.GetPathRoot(full)?.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A drive root is required.", parameterName);
        }
        return full;
    }

    private static string RequireAbsoluteDirectory(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathRooted(value))
            throw new ArgumentException("Directory must be absolute.", parameterName);
        return Path.GetFullPath(value);
    }

    private static bool IsInside(string root, string path)
    {
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string Quote(string value) => "\"" + value + "\"";
}

public static class AcceptanceFixtureAuthority
{
    public const string RequiredAttestation =
        "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT";

    public static void RequireMutatingAuthority(string attestation)
    {
        if (!string.Equals(attestation, RequiredAttestation, StringComparison.Ordinal))
            throw new InvalidOperationException("Disposable environment attestation is invalid.");
    }

    public static string ValidateFailureLockTarget(
        AcceptanceFixtureLayout layout,
        string path,
        string attestation)
    {
        ArgumentNullException.ThrowIfNull(layout);
        RequireMutatingAuthority(attestation);
        var candidate = Path.GetFullPath(path);
        if (!string.Equals(
                candidate,
                layout.FailureLockFile,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Failure lock target is outside fixture authority.");
        }
        return candidate;
    }
}

public sealed record AcceptanceFixtureUninstallRecord(
    string KeyName,
    string SessionId,
    AcceptanceFixtureRole Role,
    string DisplayName,
    string Publisher,
    string InstallLocation,
    string UninstallCommand,
    string DisplayIcon);

public interface IAcceptanceFixtureFileSystem
{
    bool Exists(string path);
    void CreateDirectory(string path);
    void WriteAllTextExclusive(string path, string content);
    void CopyFileExclusive(string sourcePath, string destinationPath);
    string ReadAllText(string path);
    void DeleteTreeNoFollow(string path);
}

public interface IAcceptanceFixtureRegistry
{
    AcceptanceFixtureUninstallRecord? ReadUninstallRecord(string keyName);
    string? ReadStartupValue(string valueName);
    bool StartupApprovalValueExists(string valueName);
    void CreateUninstallRecord(AcceptanceFixtureUninstallRecord record);
    void CreateStartupValue(string valueName, string command);
    void DeleteUninstallRecord(string keyName);
    void DeleteStartupValue(string valueName);
    void DeleteStartupApprovalValue(string valueName);
}

public sealed record AcceptanceFixtureProvisionResult(
    string SessionId,
    IReadOnlyList<string> CreatedPaths);

public sealed record AcceptanceFixtureUninstallResult(
    string SessionId,
    AcceptanceFixtureRole Role,
    bool ResidueLeftForReview);

public sealed record AcceptanceFixtureResetResult(
    string SessionId,
    IReadOnlyList<string> RemovedRoots);

public sealed record AcceptanceFixtureStatus(
    string SessionId,
    bool CSessionExists,
    bool LocalDataExists,
    bool TempExists,
    bool MigrationDestinationExists,
    bool UninstallRegistered,
    bool MigrationRegistered,
    bool StartupRegistered);

public sealed class AcceptanceFixtureOperator
{
    private readonly IAcceptanceFixtureFileSystem _files;
    private readonly IAcceptanceFixtureRegistry _registry;
    private readonly Func<DateTimeOffset> _clock;

    public AcceptanceFixtureOperator(
        IAcceptanceFixtureFileSystem files,
        IAcceptanceFixtureRegistry registry,
        Func<DateTimeOffset>? clock = null)
    {
        _files = files ?? throw new ArgumentNullException(nameof(files));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public AcceptanceFixtureProvisionResult Provision(
        AcceptanceFixtureLayout layout,
        AcceptanceFixturePayload payload,
        string attestation)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(payload);
        AcceptanceFixtureAuthority.RequireMutatingAuthority(attestation);

        var collisionPaths = new[]
        {
            layout.CSessionRoot,
            layout.LocalDataRoot,
            layout.TempRoot,
            layout.MigrationDestinationInstallRoot
        };
        if (collisionPaths.Any(_files.Exists) ||
            _registry.ReadUninstallRecord(layout.UninstallRegistryKeyName) is not null ||
            _registry.ReadUninstallRecord(layout.MigrationRegistryKeyName) is not null ||
            _registry.ReadStartupValue(layout.StartupValueName) is not null ||
            _registry.StartupApprovalValueExists(layout.StartupValueName) ||
            payload.Files.Any(file => !_files.Exists(file.SourcePath)))
        {
            throw new InvalidOperationException("Fixture provision collision or payload failure.");
        }

        var createdRoots = new List<string>();
        var createdRegistry = new List<string>();
        var startupCreated = false;
        try
        {
            createdRoots.Add(layout.CSessionRoot);
            CreateOwnedRoot(layout.CSessionRoot, layout.CSessionOwnershipMarker, layout);
            createdRoots.Add(layout.LocalDataRoot);
            CreateOwnedRoot(layout.LocalDataRoot, layout.LocalDataOwnershipMarker, layout);
            createdRoots.Add(layout.TempRoot);
            CreateOwnedRoot(layout.TempRoot, layout.TempOwnershipMarker, layout);

            CreateInstallRoot(
                layout.UninstallInstallRoot,
                layout.UninstallInstallOwnershipMarker,
                layout,
                payload);
            CreateInstallRoot(
                layout.MigrationInstallRoot,
                layout.MigrationInstallOwnershipMarker,
                layout,
                payload);
            _files.CreateDirectory(layout.CacheRoot);
            _files.WriteAllTextExclusive(
                layout.CacheFixtureFile,
                "cache-fixture:" + layout.SessionId);
            _files.WriteAllTextExclusive(
                layout.TempFixtureFile,
                "cleanup-fixture:" + layout.SessionId);
            _files.WriteAllTextExclusive(
                layout.FailureLockFile,
                "migration-lock-fixture:" + layout.SessionId);

            var uninstallRecord = CreateRecord(layout, AcceptanceFixtureRole.Uninstall);
            var migrationRecord = CreateRecord(layout, AcceptanceFixtureRole.Migration);
            createdRegistry.Add(uninstallRecord.KeyName);
            _registry.CreateUninstallRecord(uninstallRecord);
            createdRegistry.Add(migrationRecord.KeyName);
            _registry.CreateUninstallRecord(migrationRecord);
            startupCreated = true;
            _registry.CreateStartupValue(
                layout.StartupValueName,
                layout.ExpectedStartupCommand);

            return new AcceptanceFixtureProvisionResult(
                layout.SessionId,
                createdRoots.ToArray());
        }
        catch
        {
            if (startupCreated && _registry.ReadStartupValue(layout.StartupValueName) is not null)
                _registry.DeleteStartupValue(layout.StartupValueName);
            foreach (var keyName in createdRegistry.AsEnumerable().Reverse())
            {
                if (_registry.ReadUninstallRecord(keyName) is not null)
                    _registry.DeleteUninstallRecord(keyName);
            }
            foreach (var root in createdRoots.AsEnumerable().Reverse())
            {
                if (_files.Exists(root))
                    _files.DeleteTreeNoFollow(root);
            }
            throw;
        }
    }

    public AcceptanceFixtureUninstallResult Uninstall(
        AcceptanceFixtureLayout layout,
        AcceptanceFixtureRole role,
        string attestation)
    {
        ArgumentNullException.ThrowIfNull(layout);
        AcceptanceFixtureAuthority.RequireMutatingAuthority(attestation);
        var record = ExpectedRecord(layout, role);
        var current = _registry.ReadUninstallRecord(record.KeyName)
            ?? throw new InvalidOperationException("Fixture uninstall registration is missing.");
        if (!RecordMatches(current, record))
            throw new InvalidOperationException("Fixture uninstall ownership has changed.");
        if (role == AcceptanceFixtureRole.Uninstall)
        {
            var startup = _registry.ReadStartupValue(layout.StartupValueName);
            if (!string.Equals(
                    startup,
                    layout.ExpectedStartupCommand,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Fixture startup ownership has changed.");
            }
            if (_files.Exists(layout.UninstallResidueMarker))
                throw new InvalidOperationException("Fixture uninstall residue marker already exists.");
        }

        if (role == AcceptanceFixtureRole.Uninstall)
        {
            _files.WriteAllTextExclusive(
                layout.UninstallResidueMarker,
                Serialize(new FixtureResidue(
                    layout.SessionId,
                    _clock().ToUniversalTime())));
            _registry.DeleteStartupValue(layout.StartupValueName);
            if (_registry.StartupApprovalValueExists(layout.StartupValueName))
                _registry.DeleteStartupApprovalValue(layout.StartupValueName);
        }
        _registry.DeleteUninstallRecord(record.KeyName);
        return new AcceptanceFixtureUninstallResult(layout.SessionId, role, true);
    }

    public AcceptanceFixtureResetResult Reset(
        AcceptanceFixtureLayout layout,
        string attestation)
    {
        ArgumentNullException.ThrowIfNull(layout);
        AcceptanceFixtureAuthority.RequireMutatingAuthority(attestation);
        var expectedRecords = new[]
        {
            ExpectedRecord(layout, AcceptanceFixtureRole.Uninstall),
            ExpectedRecord(layout, AcceptanceFixtureRole.Migration)
        };
        foreach (var expected in expectedRecords)
        {
            var current = _registry.ReadUninstallRecord(expected.KeyName);
            if (current is not null && !RecordMatches(current, expected))
                throw new InvalidOperationException("Fixture registry ownership has changed.");
        }
        var startup = _registry.ReadStartupValue(layout.StartupValueName);
        if (startup is not null && !string.Equals(
                startup,
                layout.ExpectedStartupCommand,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Fixture startup ownership has changed.");
        }

        var roots = new[]
        {
            new OwnedRoot(layout.CSessionRoot, layout.CSessionOwnershipMarker),
            new OwnedRoot(layout.LocalDataRoot, layout.LocalDataOwnershipMarker),
            new OwnedRoot(layout.TempRoot, layout.TempOwnershipMarker),
            new OwnedRoot(
                layout.MigrationDestinationInstallRoot,
                layout.MigrationDestinationOwnershipMarker)
        };
        var existingRoots = roots.Where(root => _files.Exists(root.Root)).ToArray();
        foreach (var root in existingRoots)
            RequireOwnership(root.Marker, layout.SessionId);

        foreach (var expected in expectedRecords)
        {
            if (_registry.ReadUninstallRecord(expected.KeyName) is not null)
                _registry.DeleteUninstallRecord(expected.KeyName);
        }
        if (startup is not null)
            _registry.DeleteStartupValue(layout.StartupValueName);
        if (_registry.StartupApprovalValueExists(layout.StartupValueName))
            _registry.DeleteStartupApprovalValue(layout.StartupValueName);
        foreach (var root in existingRoots)
            _files.DeleteTreeNoFollow(root.Root);

        return new AcceptanceFixtureResetResult(
            layout.SessionId,
            existingRoots.Select(root => root.Root).ToArray());
    }

    public AcceptanceFixtureStatus GetStatus(AcceptanceFixtureLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        return new AcceptanceFixtureStatus(
            layout.SessionId,
            _files.Exists(layout.CSessionRoot),
            _files.Exists(layout.LocalDataRoot),
            _files.Exists(layout.TempRoot),
            _files.Exists(layout.MigrationDestinationInstallRoot),
            _registry.ReadUninstallRecord(layout.UninstallRegistryKeyName) is not null,
            _registry.ReadUninstallRecord(layout.MigrationRegistryKeyName) is not null,
            _registry.ReadStartupValue(layout.StartupValueName) is not null);
    }

    private void CreateOwnedRoot(
        string root,
        string marker,
        AcceptanceFixtureLayout layout)
    {
        _files.CreateDirectory(root);
        _files.WriteAllTextExclusive(marker, SerializeOwner(layout));
    }

    private void CreateInstallRoot(
        string root,
        string marker,
        AcceptanceFixtureLayout layout,
        AcceptanceFixturePayload payload)
    {
        _files.CreateDirectory(root);
        _files.WriteAllTextExclusive(marker, SerializeOwner(layout));
        foreach (var file in payload.Files)
            _files.CopyFileExclusive(file.SourcePath, Path.Combine(root, file.RelativePath));
    }

    private AcceptanceFixtureUninstallRecord CreateRecord(
        AcceptanceFixtureLayout layout,
        AcceptanceFixtureRole role) => ExpectedRecord(layout, role);

    private static AcceptanceFixtureUninstallRecord ExpectedRecord(
        AcceptanceFixtureLayout layout,
        AcceptanceFixtureRole role)
    {
        var uninstall = role == AcceptanceFixtureRole.Uninstall;
        var installRoot = uninstall
            ? layout.UninstallInstallRoot
            : layout.MigrationInstallRoot;
        var executable = uninstall
            ? layout.UninstallExecutable
            : layout.MigrationExecutable;
        return new AcceptanceFixtureUninstallRecord(
            uninstall ? layout.UninstallRegistryKeyName : layout.MigrationRegistryKeyName,
            layout.SessionId,
            role,
            uninstall ? layout.UninstallDisplayName : layout.MigrationDisplayName,
            "OMNIX QA Fixtures",
            installRoot,
            layout.ExpectedUninstallCommand(role),
            executable);
    }

    private static bool RecordMatches(
        AcceptanceFixtureUninstallRecord current,
        AcceptanceFixtureUninstallRecord expected) =>
        current == expected;

    private void RequireOwnership(string markerPath, string sessionId)
    {
        if (!_files.Exists(markerPath))
            throw new InvalidOperationException("Fixture path ownership marker is missing.");
        try
        {
            var owner = JsonSerializer.Deserialize<FixtureOwner>(_files.ReadAllText(markerPath));
            if (owner is null ||
                !string.Equals(owner.SessionId, sessionId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Fixture path ownership has changed.");
            }
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Fixture path ownership marker is invalid.",
                exception);
        }
    }

    private static string SerializeOwner(AcceptanceFixtureLayout layout) =>
        Serialize(new FixtureOwner(
            layout.SessionId,
            "OMNIX-Entropy disposable acceptance fixture"));

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });

    private sealed record OwnedRoot(string Root, string Marker);
    private sealed record FixtureOwner(string SessionId, string Kind);
    private sealed record FixtureResidue(string SessionId, DateTimeOffset UninstalledAtUtc);
}

public sealed class SystemAcceptanceFixtureFileSystem : IAcceptanceFixtureFileSystem
{
    public bool Exists(string path) => File.Exists(path) || Directory.Exists(path);

    public void CreateDirectory(string path)
    {
        AssertSafeExistingAncestors(path);
        Directory.CreateDirectory(path);
    }

    public void WriteAllTextExclusive(string path, string content)
    {
        AssertSafeExistingAncestors(path);
        using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    public void CopyFileExclusive(string sourcePath, string destinationPath)
    {
        AssertSafeExistingAncestors(sourcePath);
        AssertSafeExistingAncestors(destinationPath);
        File.Copy(sourcePath, destinationPath, overwrite: false);
    }

    public string ReadAllText(string path)
    {
        AssertSafeExistingAncestors(path);
        return File.ReadAllText(path, Encoding.UTF8);
    }

    public void DeleteTreeNoFollow(string path)
    {
        var full = Path.GetFullPath(path);
        if (IsRoot(full))
            throw new InvalidOperationException("Refusing to delete a drive root.");
        AssertSafeExistingAncestors(Path.GetDirectoryName(full) ?? full);
        DeleteEntryNoFollow(full);
    }

    private static void DeleteEntryNoFollow(string path)
    {
        var attributes = File.GetAttributes(path);
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
        {
            if (attributes.HasFlag(FileAttributes.Directory))
                Directory.Delete(path, recursive: false);
            else
                File.Delete(path);
            return;
        }
        if (!attributes.HasFlag(FileAttributes.Directory))
        {
            File.Delete(path);
            return;
        }
        foreach (var child in Directory.EnumerateFileSystemEntries(path))
            DeleteEntryNoFollow(child);
        Directory.Delete(path, recursive: false);
    }

    private static void AssertSafeExistingAncestors(string path)
    {
        var full = Path.GetFullPath(path);
        FileSystemInfo? current;
        if (Directory.Exists(full))
            current = new DirectoryInfo(full);
        else if (File.Exists(full))
            current = new FileInfo(full);
        else
        {
            var parent = Path.GetDirectoryName(full);
            while (!string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
                parent = Path.GetDirectoryName(parent);
            current = string.IsNullOrWhiteSpace(parent) ? null : new DirectoryInfo(parent);
        }
        while (current is not null)
        {
            if (current.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidOperationException("Fixture paths cannot use reparse-point ancestors.");
            current = current switch
            {
                DirectoryInfo directory => directory.Parent,
                FileInfo file => file.Directory,
                _ => null
            };
        }
    }

    private static bool IsRoot(string path) =>
        string.Equals(
            path.TrimEnd(Path.DirectorySeparatorChar),
            Path.GetPathRoot(path)?.TrimEnd(Path.DirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);
}

public sealed class CurrentUserAcceptanceFixtureRegistry : IAcceptanceFixtureRegistry
{
    private const string UninstallPath =
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string RunPath =
        @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupApprovalPath =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    public AcceptanceFixtureUninstallRecord? ReadUninstallRecord(string keyName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            UninstallPath + "\\" + keyName,
            writable: false);
        if (key is null)
            return null;
        var sessionId = key.GetValue("FixtureSessionId") as string;
        var roleText = key.GetValue("FixtureRole") as string;
        if (!Enum.TryParse<AcceptanceFixtureRole>(roleText, ignoreCase: true, out var role))
            role = (AcceptanceFixtureRole)(-1);
        return new AcceptanceFixtureUninstallRecord(
            keyName,
            sessionId ?? string.Empty,
            role,
            key.GetValue("DisplayName") as string ?? string.Empty,
            key.GetValue("Publisher") as string ?? string.Empty,
            key.GetValue("InstallLocation") as string ?? string.Empty,
            key.GetValue("UninstallString") as string ?? string.Empty,
            key.GetValue("DisplayIcon") as string ?? string.Empty);
    }

    public string? ReadStartupValue(string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunPath, writable: false);
        return key?.GetValue(
            valueName,
            null,
            RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
    }

    public bool StartupApprovalValueExists(string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            StartupApprovalPath,
            writable: false);
        return key?.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames)
            is not null;
    }

    public void CreateUninstallRecord(AcceptanceFixtureUninstallRecord record)
    {
        using var parent = Registry.CurrentUser.CreateSubKey(UninstallPath, writable: true);
        using var existing = parent.OpenSubKey(record.KeyName, writable: false);
        if (existing is not null)
            throw new InvalidOperationException("Fixture uninstall key already exists.");
        using var key = parent.CreateSubKey(record.KeyName, writable: true)
            ?? throw new InvalidOperationException("Fixture uninstall key could not be created.");
        key.SetValue("FixtureSessionId", record.SessionId, RegistryValueKind.String);
        key.SetValue("FixtureRole", record.Role.ToString(), RegistryValueKind.String);
        key.SetValue("DisplayName", record.DisplayName, RegistryValueKind.String);
        key.SetValue("Publisher", record.Publisher, RegistryValueKind.String);
        key.SetValue("InstallLocation", record.InstallLocation, RegistryValueKind.String);
        key.SetValue("UninstallString", record.UninstallCommand, RegistryValueKind.String);
        key.SetValue("DisplayIcon", record.DisplayIcon, RegistryValueKind.String);
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
    }

    public void CreateStartupValue(string valueName, string command)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunPath, writable: true)
            ?? throw new InvalidOperationException("Fixture startup key could not be opened.");
        if (key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames)
            is not null)
        {
            throw new InvalidOperationException("Fixture startup value already exists.");
        }
        key.SetValue(valueName, command, RegistryValueKind.String);
    }

    public void DeleteUninstallRecord(string keyName)
    {
        using var parent = Registry.CurrentUser.OpenSubKey(UninstallPath, writable: true);
        parent?.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
    }

    public void DeleteStartupValue(string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunPath, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    public void DeleteStartupApprovalValue(string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            StartupApprovalPath,
            writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
