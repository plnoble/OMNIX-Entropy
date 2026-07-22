using Css.Core.Software;
using Microsoft.Win32;

namespace Css.Elevated.Uninstall;

public sealed class SystemOfficialUninstallInventoryReader
{
    private static readonly (RegistryKey Root, string Path)[] Locations =
    [
        (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall"),
        (Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Uninstall"),
        (Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")
    ];

    public Task<IReadOnlyList<SoftwareProfile>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var profiles = new List<SoftwareProfile>();
        foreach (var (root, path) in Locations)
            ReadLocation(root, path, profiles, cancellationToken);
        return Task.FromResult<IReadOnlyList<SoftwareProfile>>(profiles);
    }

    private static void ReadLocation(
        RegistryKey root,
        string path,
        ICollection<SoftwareProfile> profiles,
        CancellationToken cancellationToken)
    {
        using var location = root.OpenSubKey(path, writable: false);
        if (location is null)
            return;
        foreach (var subKeyName in location.GetSubKeyNames())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var item = location.OpenSubKey(subKeyName, writable: false)
                ?? throw new InvalidOperationException(
                    "An installed-software registry entry could not be opened.");
            var name = item.GetValue("DisplayName") as string;
            if (string.IsNullOrWhiteSpace(name))
                continue;
            profiles.Add(new SoftwareProfile
            {
                Name = name,
                Publisher = item.GetValue("Publisher") as string,
                InstallPath = item.GetValue("InstallLocation") as string,
                UninstallCommand = item.GetValue("UninstallString") as string
            });
        }
    }
}
