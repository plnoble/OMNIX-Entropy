using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Css.Core.Software;
using Microsoft.Win32;

namespace Css.Scanner.Software;

/// <summary>
/// Read-only software inventory scanner. It reads uninstall registry keys,
/// Run startup entries, and Win32_Service executable paths; it does not change
/// registry, services, tasks, or files.
/// </summary>
public sealed class SoftwareInventoryScanner
{
    public Task<IReadOnlyList<SoftwareProfile>> ScanAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var installed = ReadInstalledSoftware();
        var startup = ReadStartupEntries();
        var services = ReadServices();
        var scheduledTasks = ReadScheduledTasks();
        var processes = ReadRunningProcesses();
        var observedAtUtc = DateTimeOffset.UtcNow;
        return Task.FromResult(SoftwareInventoryBuilder.Build(
            installed,
            startup,
            services,
            scheduledTasks,
            signatureResolver: SignatureInspector.GetSignatureSubject,
            runningProcesses: processes,
            installSizeResolver: EstimateDirectorySize,
            userDataRoots: GetUserDataRoots(),
            pathExists: Directory.Exists,
            cacheSizeResolver: EstimateDirectorySize,
            observedAtUtc: observedAtUtc));
    }

    private static IReadOnlyList<string> GetUserDataRoots()
    {
        var roots = new List<string>();
        AddExistingFolder(roots, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        AddExistingFolder(roots, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
            AddExistingFolder(roots, Path.Combine(userProfile, "AppData", "LocalLow"));

        return roots
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddExistingFolder(List<string> roots, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            roots.Add(path);
    }

    private static IReadOnlyList<InstalledSoftwareRecord> ReadInstalledSoftware()
    {
        var records = new List<InstalledSoftwareRecord>();
        ReadUninstallKey(records, Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall", "HKCU");
        ReadUninstallKey(records, Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Uninstall", "HKLM");
        ReadUninstallKey(records, Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", "HKLM32");
        return records;
    }

    private static void ReadUninstallKey(List<InstalledSoftwareRecord> records, RegistryKey root, string subKeyPath, string hiveName)
    {
        using var key = root.OpenSubKey(subKeyPath, false);
        if (key is null) return;

        foreach (var subKeyName in key.GetSubKeyNames())
        {
            using var sub = key.OpenSubKey(subKeyName, false);
            if (sub is null) continue;

            var displayName = sub.GetValue("DisplayName") as string;
            if (string.IsNullOrWhiteSpace(displayName)) continue;

            records.Add(InstalledSoftwareRegistryRecordFactory.Create(
                displayName,
                sub.GetValue("Publisher") as string,
                sub.GetValue("InstallLocation") as string,
                sub.GetValue("UninstallString") as string,
                sub.GetValue("DisplayIcon") as string,
                hiveName + "\\" + subKeyPath + "\\" + subKeyName,
                subKeyName,
                sub.GetValue("InstallSource") as string,
                sub.GetValue("WindowsInstaller"),
                sub.GetValue("InstallDate")));
        }
    }

    private static IReadOnlyList<StartupEntry> ReadStartupEntries()
    {
        var entries = new List<StartupEntry>();
        using var currentUser64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        using var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        ReadRunKey(
            entries,
            currentUser64,
            currentUser64,
            @"Software\Microsoft\Windows\CurrentVersion\Run",
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
            "HKCU64",
            "HKCU64");
        ReadRunKey(
            entries,
            localMachine64,
            localMachine64,
            @"Software\Microsoft\Windows\CurrentVersion\Run",
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
            "HKLM64",
            "HKLM64");
        ReadRunKey(
            entries,
            localMachine32,
            localMachine64,
            @"Software\Microsoft\Windows\CurrentVersion\Run",
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32",
            "HKLM32",
            "HKLM64");
        return entries;
    }

    private static void ReadRunKey(
        List<StartupEntry> entries,
        RegistryKey runRoot,
        RegistryKey approvalRoot,
        string subKeyPath,
        string approvalSubKeyPath,
        string runHiveViewName,
        string approvalHiveViewName)
    {
        using var key = runRoot.OpenSubKey(subKeyPath, false);
        if (key is null) return;

        RegistryKey? approvalKey = null;
        var approvalKeyReadable = true;
        try
        {
            approvalKey = approvalRoot.OpenSubKey(approvalSubKeyPath, false);
        }
        catch
        {
            approvalKeyReadable = false;
        }

        using (approvalKey)
        {
            foreach (var name in key.GetValueNames())
            {
                var command = key.GetValue(name)?.ToString();
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                entries.Add(new StartupEntry(
                    name,
                    command,
                    runHiveViewName + "\\" + subKeyPath,
                    ReadStartupApproval(
                        approvalKey,
                        approvalKeyReadable,
                        approvalHiveViewName + "\\" + approvalSubKeyPath,
                        name)));
            }
        }
    }

    private static StartupApprovalObservation ReadStartupApproval(
        RegistryKey? approvalKey,
        bool approvalKeyReadable,
        string approvalKeyLocator,
        string valueName)
    {
        if (!approvalKeyReadable)
            return StartupApprovalObservationFactory.Unreadable(approvalKeyLocator, valueName);

        if (approvalKey is null)
            return StartupApprovalObservationFactory.FromRegistryValue(
                approvalKeyLocator,
                valueName,
                value: null);

        try
        {
            var value = approvalKey.GetValue(
                valueName,
                null,
                RegistryValueOptions.DoNotExpandEnvironmentNames);
            return StartupApprovalObservationFactory.FromRegistryValue(
                approvalKeyLocator,
                valueName,
                value);
        }
        catch
        {
            return StartupApprovalObservationFactory.Unreadable(approvalKeyLocator, valueName);
        }
    }

    private static IReadOnlyList<ServiceEntry> ReadServices()
    {
        var services = new List<ServiceEntry>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, DisplayName, PathName, State, StartMode FROM Win32_Service");
            foreach (var item in searcher.Get())
            {
                var name = item["Name"]?.ToString();
                var displayName = item["DisplayName"]?.ToString();
                var pathName = item["PathName"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(pathName))
                    services.Add(new ServiceEntry(
                        name,
                        displayName ?? name,
                        pathName,
                        item["StartMode"]?.ToString(),
                        item["State"]?.ToString()));
            }
        }
        catch
        {
            // WMI can be unavailable or restricted; software inventory still works without services.
        }

        ReadServiceRegistryEntries(services);

        return services
            .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    private static void ReadServiceRegistryEntries(List<ServiceEntry> services)
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", false);
        if (key is null) return;

        foreach (var subKeyName in key.GetSubKeyNames())
        {
            using var sub = key.OpenSubKey(subKeyName, false);
            if (sub is null) continue;

            var entry = ServiceEntryFactory.FromRegistryValues(
                subKeyName,
                sub.GetValue("DisplayName") as string,
                sub.GetValue("ImagePath")?.ToString(),
                sub.GetValue("Start"));
            if (entry is not null)
                services.Add(entry);
        }
    }

    private static IReadOnlyList<ScheduledTaskEntry> ReadScheduledTasks()
    {
        var entries = new List<ScheduledTaskEntry>();
        var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (string.IsNullOrWhiteSpace(windowsDir))
            windowsDir = Environment.GetEnvironmentVariable("WINDIR") ?? @"C:\Windows";

        var tasksRoot = Path.Combine(windowsDir, "System32", "Tasks");
        if (!Directory.Exists(tasksRoot))
            return entries;

        foreach (var path in EnumerateTaskFiles(tasksRoot))
        {
            try
            {
                var xml = File.ReadAllText(path);
                var taskName = ToTaskName(tasksRoot, path);
                var entry = ScheduledTaskXmlParser.Parse(taskName, xml);
                if (entry is not null)
                    entries.Add(entry);
            }
            catch
            {
                // Some task files are protected or malformed; keep the scan read-only and partial.
            }
        }

        return entries;
    }

    private static IReadOnlyList<ProcessEntry> ReadRunningProcesses()
    {
        var entries = new List<ProcessEntry>();
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                entries.Add(new ProcessEntry(process.ProcessName, TryGetProcessPath(process)));
            }
            catch
            {
                // Process may exit during enumeration; keep the scan partial.
            }
            finally
            {
                process.Dispose();
            }
        }

        return entries;
    }

    private static string? TryGetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<string> EnumerateTaskFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            string[] files;
            try
            {
                files = Directory.GetFiles(current);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
                yield return file;

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var directory in directories)
            {
                try
                {
                    if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(directory);
                }
                catch
                {
                    // Skip directories whose attributes cannot be read.
                }
            }
        }
    }

    private static string ToTaskName(string tasksRoot, string path)
    {
        try
        {
            var relative = Path.GetRelativePath(tasksRoot, path)
                .Replace(Path.DirectorySeparatorChar, '\\')
                .Replace(Path.AltDirectorySeparatorChar, '\\');
            return relative.StartsWith('\\') ? relative : "\\" + relative;
        }
        catch
        {
            return "\\" + Path.GetFileName(path);
        }
    }

    private static long EstimateDirectorySize(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return 0;

        const int maxFiles = 20000;
        long total = 0;
        var seenFiles = 0;
        var pending = new Stack<string>();
        pending.Push(path);

        while (pending.Count > 0 && seenFiles < maxFiles)
        {
            var current = pending.Pop();

            FileInfo[] files;
            try
            {
                files = new DirectoryInfo(current).GetFiles();
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                total += Math.Max(0, file.Length);
                seenFiles++;
                if (seenFiles >= maxFiles)
                    break;
            }

            if (seenFiles >= maxFiles)
                break;

            DirectoryInfo[] directories;
            try
            {
                directories = new DirectoryInfo(current).GetDirectories();
            }
            catch
            {
                continue;
            }

            foreach (var directory in directories)
            {
                try
                {
                    if ((directory.Attributes & FileAttributes.ReparsePoint) == 0)
                        pending.Push(directory.FullName);
                }
                catch
                {
                    // Skip directories whose attributes cannot be read.
                }
            }
        }

        return total;
    }
}
