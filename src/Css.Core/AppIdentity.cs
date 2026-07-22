using System;
using System.IO;

namespace Css.Core;

public static class AppIdentity
{
    public const string ProductName = "OMNIX-Entropy";
    public const string LocalDataFolderName = "OMNIX-Entropy";
    public const string QuarantineFolderName = "Quarantine";
    public const string DefaultQuarantineRootOnD = @"D:\OMNIX-Entropy\Quarantine";
}

public sealed record AppStoragePaths(
    string DatabasePath,
    string QuarantineRoot,
    string MigrationRollbackRoot,
    string InstallRoutingMemoryPath);

public static class AppStoragePathResolver
{
    public const string DataRootEnvironmentVariable = "OMNIX_ENTROPY_DATA_ROOT";
    public const string QuarantineRootEnvironmentVariable = "OMNIX_ENTROPY_QUARANTINE_ROOT";

    public static AppStoragePaths Resolve(
        Func<string, string?>? getEnvironmentVariable = null,
        Func<string, bool>? directoryExists = null,
        string? localAppDataRoot = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;
        directoryExists ??= Directory.Exists;
        localAppDataRoot ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dataRoot = CleanRoot(getEnvironmentVariable(DataRootEnvironmentVariable))
            ?? Path.Combine(localAppDataRoot, AppIdentity.LocalDataFolderName);
        var quarantineRoot = CleanRoot(getEnvironmentVariable(QuarantineRootEnvironmentVariable))
            ?? (directoryExists(@"D:\")
                ? AppIdentity.DefaultQuarantineRootOnD
                : Path.Combine(dataRoot, AppIdentity.QuarantineFolderName));

        return new AppStoragePaths(
            Path.Combine(dataRoot, "data.db"),
            quarantineRoot,
            Path.Combine(dataRoot, "MigrationRollback"),
            Path.Combine(dataRoot, "install-routing-memory.json"));
    }

    private static string? CleanRoot(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}

public static class AppDevelopmentPathResolver
{
    public const string CDriveScanRootEnvironmentVariable = "OMNIX_ENTROPY_CDRIVE_SCAN_ROOT";
    public const string SoftwareInventoryFixtureEnvironmentVariable = "OMNIX_ENTROPY_SOFTWARE_FIXTURE";
    public const string StartupEntryFixtureEnvironmentVariable = "OMNIX_ENTROPY_STARTUP_FIXTURE";
    public const string UninstallEvidenceRootEnvironmentVariable = "OMNIX_ENTROPY_UNINSTALL_EVIDENCE_ROOT";
    public const string PersonalStorageFixtureRootEnvironmentVariable = "OMNIX_ENTROPY_PERSONAL_STORAGE_ROOT";

    public static string ResolveCDriveScanRoot(
        string defaultRoot,
        Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;

        var overrideRoot = CleanRoot(getEnvironmentVariable(CDriveScanRootEnvironmentVariable));
        return overrideRoot ?? defaultRoot;
    }

    public static string? ResolveSoftwareInventoryFixturePath(
        Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;
        return CleanRoot(getEnvironmentVariable(SoftwareInventoryFixtureEnvironmentVariable));
    }

    public static string? ResolveStartupEntryFixturePath(
        Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;
        return CleanRoot(getEnvironmentVariable(StartupEntryFixtureEnvironmentVariable));
    }

    public static string ResolveUninstallEvidenceRoot(
        string defaultRoot,
        Func<string, string?>? getEnvironmentVariable = null)
    {
        if (string.IsNullOrWhiteSpace(defaultRoot))
            throw new ArgumentException("Default uninstall evidence root is required.", nameof(defaultRoot));

        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;
        return CleanRoot(getEnvironmentVariable(UninstallEvidenceRootEnvironmentVariable))
            ?? defaultRoot;
    }

    public static string? ResolvePersonalStorageFixtureRoot(
        Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;
        var candidate = CleanRoot(getEnvironmentVariable(PersonalStorageFixtureRootEnvironmentVariable));
        if (candidate is null
            || candidate.StartsWith("\\\\", StringComparison.Ordinal)
            || !Path.IsPathFullyQualified(candidate))
        {
            return null;
        }

        try
        {
            var fullPath = Path.GetFullPath(candidate)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var root = Path.GetPathRoot(fullPath)?.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            return fullPath.Equals(root, StringComparison.OrdinalIgnoreCase)
                || fullPath.AsSpan(Math.Min(2, fullPath.Length)).Contains(':')
                    ? null
                    : fullPath;
        }
        catch
        {
            return null;
        }
    }

    private static string? CleanRoot(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}
