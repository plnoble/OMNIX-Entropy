using Css.Core.Software;
using Microsoft.Win32;

namespace Css.Scanner.Software;

/// <summary>
/// Reads bounded Windows integration locations. It never changes registry
/// values, file associations, browser policy, services, tasks, or files.
/// </summary>
public sealed class WindowsSoftwareSystemFootprintScanner
{
    private static readonly string[] ContextMenuRoots =
    [
        @"Software\Classes\*\shell",
        @"Software\Classes\*\shellex\ContextMenuHandlers",
        @"Software\Classes\AllFilesystemObjects\shell",
        @"Software\Classes\AllFilesystemObjects\shellex\ContextMenuHandlers",
        @"Software\Classes\Directory\shell",
        @"Software\Classes\Directory\shellex\ContextMenuHandlers",
        @"Software\Classes\Directory\Background\shell",
        @"Software\Classes\Directory\Background\shellex\ContextMenuHandlers",
        @"Software\Classes\Drive\shell",
        @"Software\Classes\Drive\shellex\ContextMenuHandlers",
        @"Software\Classes\Folder\shell",
        @"Software\Classes\Folder\shellex\ContextMenuHandlers"
    ];

    private static readonly string[] ExplorerNamespaceRoots =
    [
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace",
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace",
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\NetworkNeighborhood\NameSpace"
    ];

    private static readonly string[] BrowserIntegrationRoots =
    [
        @"Software\Google\Chrome\NativeMessagingHosts",
        @"Software\Microsoft\Edge\NativeMessagingHosts",
        @"Software\Mozilla\NativeMessagingHosts"
    ];

    private static readonly string[] CommonFileExtensions =
    [
        ".7z", ".doc", ".docx", ".jpg", ".jpeg", ".pdf", ".png",
        ".ppt", ".pptx", ".rar", ".txt", ".xls", ".xlsx", ".zip"
    ];

    public IReadOnlyList<SoftwareSystemFootprintEntry> Scan()
    {
        var entries = new List<SoftwareSystemFootprintEntry>();
        ScanView(entries, RegistryHive.CurrentUser, RegistryView.Registry64, "HKCU64");
        ScanView(entries, RegistryHive.LocalMachine, RegistryView.Registry64, "HKLM64");
        ScanView(entries, RegistryHive.LocalMachine, RegistryView.Registry32, "HKLM32");

        return entries
            .Where(entry =>
                !string.IsNullOrWhiteSpace(entry.DisplayName)
                && !string.IsNullOrWhiteSpace(entry.SourceLocator)
                && !string.IsNullOrWhiteSpace(entry.Evidence))
            .GroupBy(
                entry => entry.Kind + "|" + entry.SourceLocator,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static void ScanView(
        List<SoftwareSystemFootprintEntry> entries,
        RegistryHive hive,
        RegistryView view,
        string viewName)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(hive, view);
            ScanContextMenus(entries, root, viewName);
            ScanExplorerEntries(entries, root, viewName);
            ScanBrowserIntegrations(entries, root, viewName);
            ScanFileAssociations(entries, root, viewName);
        }
        catch
        {
            // A restricted registry view makes this scan partial, never writable.
        }
    }

    private static void ScanContextMenus(
        List<SoftwareSystemFootprintEntry> entries,
        RegistryKey root,
        string viewName)
    {
        foreach (var parentPath in ContextMenuRoots)
        {
            using var parent = TryOpen(root, parentPath);
            if (parent is null)
                continue;

            foreach (var childName in SafeSubKeyNames(parent))
            {
                using var child = TryOpen(parent, childName);
                if (child is null)
                    continue;

                var title = FirstText(
                    ReadString(child, "MUIVerb"),
                    ReadString(child, string.Empty),
                    childName);
                var evidence = JoinEvidence(
                    childName,
                    ReadString(child, string.Empty),
                    ReadString(child, "MUIVerb"),
                    ReadChildDefault(child, "command"),
                    ResolveClsidEvidence(root, ReadString(child, string.Empty)),
                    ResolveClsidEvidence(root, childName));
                Add(
                    entries,
                    SoftwareSystemFootprintKind.ContextMenu,
                    title,
                    viewName + "\\" + parentPath + "\\" + childName,
                    evidence);
            }
        }
    }

    private static void ScanExplorerEntries(
        List<SoftwareSystemFootprintEntry> entries,
        RegistryKey root,
        string viewName)
    {
        foreach (var parentPath in ExplorerNamespaceRoots)
        {
            using var parent = TryOpen(root, parentPath);
            if (parent is null)
                continue;

            foreach (var childName in SafeSubKeyNames(parent))
            {
                var clsidEvidence = ResolveClsidEvidence(root, childName);
                Add(
                    entries,
                    SoftwareSystemFootprintKind.ExplorerEntry,
                    FriendlyClsidName(root, childName) ?? childName,
                    viewName + "\\" + parentPath + "\\" + childName,
                    JoinEvidence(childName, clsidEvidence));
            }
        }
    }

    private static void ScanBrowserIntegrations(
        List<SoftwareSystemFootprintEntry> entries,
        RegistryKey root,
        string viewName)
    {
        foreach (var parentPath in BrowserIntegrationRoots)
        {
            using var parent = TryOpen(root, parentPath);
            if (parent is null)
                continue;

            foreach (var childName in SafeSubKeyNames(parent))
            {
                using var child = TryOpen(parent, childName);
                if (child is null)
                    continue;

                var manifestPath = ReadString(child, string.Empty);
                Add(
                    entries,
                    SoftwareSystemFootprintKind.BrowserIntegration,
                    childName,
                    viewName + "\\" + parentPath + "\\" + childName,
                    JoinEvidence(childName, manifestPath));
            }
        }
    }

    private static void ScanFileAssociations(
        List<SoftwareSystemFootprintEntry> entries,
        RegistryKey root,
        string viewName)
    {
        foreach (var extension in CommonFileExtensions)
        {
            var extensionPath = @"Software\Classes\" + extension;
            using var extensionKey = TryOpen(root, extensionPath);
            if (extensionKey is null)
                continue;

            var programIds = new List<string>();
            AddDistinct(programIds, ReadString(extensionKey, string.Empty));
            using (var openWith = TryOpen(extensionKey, "OpenWithProgids"))
            {
                if (openWith is not null)
                {
                    foreach (var valueName in SafeValueNames(openWith))
                        AddDistinct(programIds, valueName);
                }
            }

            foreach (var programId in programIds)
            {
                var command = ReadClassOpenCommand(root, programId);
                Add(
                    entries,
                    SoftwareSystemFootprintKind.FileAssociation,
                    extension + " 由 " + programId + " 打开",
                    viewName + "\\" + extensionPath + "::" + programId,
                    JoinEvidence(extension, programId, command));
            }
        }
    }

    private static string? FriendlyClsidName(RegistryKey root, string candidate)
    {
        if (!TryNormalizeClsid(candidate, out var clsid))
            return null;

        using var key = TryOpen(root, @"Software\Classes\CLSID\" + clsid);
        return key is null ? null : ReadString(key, string.Empty);
    }

    private static string ResolveClsidEvidence(RegistryKey root, string? candidate)
    {
        if (!TryNormalizeClsid(candidate, out var clsid))
            return string.Empty;

        using var key = TryOpen(root, @"Software\Classes\CLSID\" + clsid);
        if (key is null)
            return string.Empty;

        return JoinEvidence(
            ReadString(key, string.Empty),
            ReadChildDefault(key, "InprocServer32"),
            ReadChildDefault(key, "LocalServer32"));
    }

    private static bool TryNormalizeClsid(string? value, out string clsid)
    {
        clsid = string.Empty;
        if (!Guid.TryParse(value, out var parsed))
            return false;

        clsid = parsed.ToString("B");
        return true;
    }

    private static string ReadClassOpenCommand(RegistryKey root, string programId)
    {
        if (string.IsNullOrWhiteSpace(programId)
            || programId.Length > 260
            || programId.Any(char.IsControl)
            || programId.Contains('\\'))
        {
            return string.Empty;
        }

        using var key = TryOpen(
            root,
            @"Software\Classes\" + programId + @"\shell\open\command");
        return key is null ? string.Empty : ReadString(key, string.Empty);
    }

    private static RegistryKey? TryOpen(RegistryKey root, string subKey)
    {
        try
        {
            return root.OpenSubKey(subKey, writable: false);
        }
        catch
        {
            return null;
        }
    }

    private static string[] SafeSubKeyNames(RegistryKey key)
    {
        try
        {
            return key.GetSubKeyNames();
        }
        catch
        {
            return [];
        }
    }

    private static string[] SafeValueNames(RegistryKey key)
    {
        try
        {
            return key.GetValueNames();
        }
        catch
        {
            return [];
        }
    }

    private static string ReadString(RegistryKey key, string name)
    {
        try
        {
            return Convert.ToString(
                key.GetValue(name, string.Empty, RegistryValueOptions.DoNotExpandEnvironmentNames))
                ?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ReadChildDefault(RegistryKey key, string childName)
    {
        using var child = TryOpen(key, childName);
        return child is null ? string.Empty : ReadString(child, string.Empty);
    }

    private static void Add(
        List<SoftwareSystemFootprintEntry> entries,
        SoftwareSystemFootprintKind kind,
        string? displayName,
        string sourceLocator,
        string evidence)
    {
        if (string.IsNullOrWhiteSpace(displayName)
            || string.IsNullOrWhiteSpace(evidence))
        {
            return;
        }

        entries.Add(new SoftwareSystemFootprintEntry(
            kind,
            displayName.Trim(),
            sourceLocator,
            evidence));
    }

    private static string FirstText(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
        ?? string.Empty;

    private static string JoinEvidence(params string?[] values) =>
        string.Join(
            " ",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

    private static void AddDistinct(List<string> values, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)
            || candidate.Length > 260
            || candidate.Any(char.IsControl)
            || values.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        values.Add(candidate.Trim());
    }
}
