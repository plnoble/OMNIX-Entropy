using System.Globalization;
using System.IO;
using Css.Core.Software;

namespace Css.Scanner.Software;

public sealed record InstalledSoftwareRecord(
    string DisplayName,
    string? Publisher,
    string? InstallLocation,
    string? UninstallCommand,
    string? DisplayIcon,
    string RegistryKeyPath,
    string? InstallSource = null,
    bool IsWindowsInstaller = false,
    string? WindowsInstallerProductCode = null,
    DateOnly? InstallDate = null);

public sealed record DisplayIconReference(string Path, int ResourceIndex);

public static class DisplayIconReferenceParser
{
    private const int MaximumReferenceLength = 1024;
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe",
        ".dll",
        ".ico",
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif"
    };

    public static DisplayIconReference? Parse(
        string? value,
        Func<string, string>? environmentExpander = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();
        if (text.Length > MaximumReferenceLength || text.Any(char.IsControl))
            return null;

        if (!TrySplitPathAndIndex(text, out var rawPath, out var resourceIndex))
            return null;

        string expanded;
        try
        {
            expanded = (environmentExpander ?? Environment.ExpandEnvironmentVariables)(rawPath).Trim();
        }
        catch
        {
            return null;
        }

        if (expanded.Length == 0
            || expanded.Length > MaximumReferenceLength
            || expanded.Contains('%')
            || expanded.Any(char.IsControl)
            || !IsLocalDrivePath(expanded))
        {
            return null;
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(expanded);
        }
        catch
        {
            return null;
        }

        if (!IsLocalDrivePath(fullPath)
            || !SupportedExtensions.Contains(Path.GetExtension(fullPath)))
        {
            return null;
        }

        return new DisplayIconReference(fullPath, resourceIndex);
    }

    private static bool TrySplitPathAndIndex(
        string text,
        out string path,
        out int resourceIndex)
    {
        path = string.Empty;
        resourceIndex = 0;

        if (text.StartsWith('"'))
        {
            var closingQuote = text.IndexOf('"', 1);
            if (closingQuote <= 1)
                return false;

            path = text[1..closingQuote].Trim();
            var suffix = text[(closingQuote + 1)..].Trim();
            if (suffix.Length == 0)
                return path.Length > 0;

            return TryParseIndexSuffix(suffix, out resourceIndex) && path.Length > 0;
        }

        var lastComma = text.LastIndexOf(',');
        if (lastComma > 0
            && int.TryParse(
                text[(lastComma + 1)..].Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsedIndex))
        {
            path = text[..lastComma].Trim();
            resourceIndex = parsedIndex;
            return path.Length > 0;
        }

        path = text;
        return path.Length > 0 && !path.Contains('"');
    }

    private static bool TryParseIndexSuffix(string suffix, out int resourceIndex)
    {
        resourceIndex = 0;
        if (!suffix.StartsWith(','))
            return false;

        return int.TryParse(
            suffix[1..].Trim(),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out resourceIndex);
    }

    private static bool IsLocalDrivePath(string path) =>
        path.Length >= 3
        && char.IsAsciiLetter(path[0])
        && path[1] == ':'
        && (path[2] == '\\' || path[2] == '/');
}

public static class InstalledSoftwareRegistryRecordFactory
{
    public static InstalledSoftwareRecord Create(
        string displayName,
        string? publisher,
        string? installLocation,
        string? uninstallCommand,
        string? displayIcon,
        string registryKeyPath,
        string registrySubKeyName,
        string? installSource,
        object? windowsInstallerValue,
        object? installDateValue = null)
    {
        var isWindowsInstaller = IsEnabled(windowsInstallerValue);
        var productCode = isWindowsInstaller && Guid.TryParse(registrySubKeyName, out _)
            ? registrySubKeyName
            : null;

        return new InstalledSoftwareRecord(
            displayName,
            publisher,
            installLocation,
            uninstallCommand,
            displayIcon,
            registryKeyPath,
            installSource,
            isWindowsInstaller,
            productCode,
            ParseInstallDate(installDateValue));
    }

    private static DateOnly? ParseInstallDate(object? value)
    {
        var text = value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        string[] formats = ["yyyyMMdd", "yyyy-MM-dd"];
        return DateOnly.TryParseExact(
            text,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? date
            : null;
    }

    private static bool IsEnabled(object? value)
    {
        try
        {
            return value is not null && Convert.ToInt32(value) == 1;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public sealed record StartupEntry(
    string Name,
    string Command,
    string SourceLocator,
    StartupApprovalObservation? ApprovalEvidence = null);

public sealed record ServiceEntry(
    string Name,
    string DisplayName,
    string PathName,
    string? StartMode = null,
    string? RuntimeState = null);

public sealed record ScheduledTaskEntry(string Name, string ActionPath, bool? IsEnabled = null);

public sealed record ProcessEntry(string Name, string? Path);
