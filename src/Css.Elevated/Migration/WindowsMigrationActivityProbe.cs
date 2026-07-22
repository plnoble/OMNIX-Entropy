using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Css.Core.Migration;
using Microsoft.Win32.SafeHandles;

namespace Css.Elevated.Migration;

public interface IWindowsMigrationProcessStateReader
{
    bool IsRunning(string processName);
}

public interface IWindowsMigrationServiceStateReader
{
    bool IsRunningOrTransitioning(string serviceName);
}

public interface IWindowsMigrationScheduledTaskStateReader
{
    bool IsEnabledOrRunning(string taskPath);
}

public sealed class WindowsMigrationActivityProbe : IMigrationActivityProbe
{
    private const int MaximumItemsPerKind = 256;

    private readonly IWindowsMigrationProcessStateReader _processes;
    private readonly IWindowsMigrationServiceStateReader _services;
    private readonly IWindowsMigrationScheduledTaskStateReader _tasks;

    public WindowsMigrationActivityProbe(
        IWindowsMigrationProcessStateReader? processes = null,
        IWindowsMigrationServiceStateReader? services = null,
        IWindowsMigrationScheduledTaskStateReader? tasks = null)
    {
        _processes = processes ?? new SystemMigrationProcessStateReader();
        _services = services ?? new WindowsMigrationServiceStateReader();
        _tasks = tasks ?? new WindowsMigrationScheduledTaskStateReader();
    }

    public Task<IReadOnlyList<string>> FindActiveAsync(
        MigrationActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var findings = new List<string>();
        Probe(
            request.ProcessNames,
            "process",
            _processes.IsRunning,
            findings,
            cancellationToken);
        Probe(
            request.ServiceNames,
            "service",
            _services.IsRunningOrTransitioning,
            findings,
            cancellationToken);
        Probe(
            request.ScheduledTasks,
            "scheduled task",
            _tasks.IsEnabledOrRunning,
            findings,
            cancellationToken);
        return Task.FromResult<IReadOnlyList<string>>(findings);
    }

    private static void Probe(
        IReadOnlyList<string> names,
        string kind,
        Func<string, bool> isActive,
        ICollection<string> findings,
        CancellationToken cancellationToken)
    {
        if (names.Count > MaximumItemsPerKind)
        {
            findings.Add($"{kind} list exceeds the safety limit");
            return;
        }

        foreach (var name in names
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (name.Length > 4096)
            {
                findings.Add($"{kind} identity is invalid");
                continue;
            }

            try
            {
                if (isActive(name))
                    findings.Add($"{kind}: {name}");
            }
            catch
            {
                findings.Add($"{kind} status unavailable: {name}");
            }
        }
    }
}

public sealed class SystemMigrationProcessStateReader : IWindowsMigrationProcessStateReader
{
    public bool IsRunning(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
            throw new ArgumentException("Process name is required.", nameof(processName));
        var normalized = Path.GetFileNameWithoutExtension(processName.Trim());
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Process name is invalid.", nameof(processName));
        }

        var matches = Process.GetProcessesByName(normalized);
        try
        {
            return matches.Any(process => !HasExited(process));
        }
        finally
        {
            foreach (var process in matches)
                process.Dispose();
        }
    }

    private static bool HasExited(Process process)
    {
        try
        {
            return process.HasExited;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class WindowsMigrationServiceStateReader : IWindowsMigrationServiceStateReader
{
    private const uint ScManagerConnect = 0x0001;
    private const uint ServiceQueryStatus = 0x0004;
    private const int ScStatusProcessInfo = 0;
    private const uint ServiceStopped = 0x00000001;
    private const int ErrorServiceDoesNotExist = 1060;

    public bool IsRunningOrTransitioning(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName) || serviceName.Length > 256)
            throw new ArgumentException("Service name is invalid.", nameof(serviceName));

        using var manager = OpenSCManager(null, null, ScManagerConnect);
        if (manager.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        using var service = OpenService(manager, serviceName, ServiceQueryStatus);
        if (service.IsInvalid)
        {
            var error = Marshal.GetLastWin32Error();
            if (error == ErrorServiceDoesNotExist)
                return false;
            throw new Win32Exception(error);
        }

        var size = Marshal.SizeOf<ServiceStatusProcess>();
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            if (!QueryServiceStatusEx(
                    service,
                    ScStatusProcessInfo,
                    buffer,
                    size,
                    out _))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            var status = Marshal.PtrToStructure<ServiceStatusProcess>(buffer);
            return status.CurrentState != ServiceStopped;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ServiceStatusProcess
    {
        public uint ServiceType;
        public uint CurrentState;
        public uint ControlsAccepted;
        public uint Win32ExitCode;
        public uint ServiceSpecificExitCode;
        public uint CheckPoint;
        public uint WaitHint;
        public uint ProcessId;
        public uint ServiceFlags;
    }

    private sealed class ServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private ServiceHandle() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle() => CloseServiceHandle(handle);
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ServiceHandle OpenSCManager(
        string? machineName,
        string? databaseName,
        uint desiredAccess);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ServiceHandle OpenService(
        ServiceHandle serviceControlManager,
        string serviceName,
        uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryServiceStatusEx(
        ServiceHandle service,
        int informationLevel,
        IntPtr buffer,
        int bufferSize,
        out int bytesNeeded);

    [DllImport("advapi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseServiceHandle(IntPtr serviceHandle);
}

public sealed class WindowsMigrationScheduledTaskStateReader
    : IWindowsMigrationScheduledTaskStateReader
{
    private const int TaskNotFoundHresult = unchecked((int)0x8004130F);
    private const int FileNotFoundHresult = unchecked((int)0x80070002);

    public bool IsEnabledOrRunning(string taskPath)
    {
        var normalized = NormalizeTaskPath(taskPath);
        var separator = normalized.LastIndexOf('\\');
        var folderPath = separator <= 0 ? "\\" : normalized[..separator];
        var taskName = normalized[(separator + 1)..];
        object? service = null;
        object? folder = null;
        object? task = null;
        try
        {
            var type = Type.GetTypeFromProgID("Schedule.Service", throwOnError: false)
                ?? throw new PlatformNotSupportedException("Task Scheduler COM is unavailable.");
            service = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException("Task Scheduler COM could not start.");
            dynamic dynamicService = service;
            dynamicService.Connect();
            folder = dynamicService.GetFolder(folderPath);
            dynamic dynamicFolder = folder;
            task = dynamicFolder.GetTask(taskName);
            dynamic dynamicTask = task;
            return Convert.ToBoolean(dynamicTask.Enabled)
                || Convert.ToInt32(dynamicTask.State) == 4;
        }
        catch (COMException exception) when (
            exception.HResult is TaskNotFoundHresult or FileNotFoundHresult)
        {
            return false;
        }
        finally
        {
            ReleaseCom(task);
            ReleaseCom(folder);
            ReleaseCom(service);
        }
    }

    private static string NormalizeTaskPath(string taskPath)
    {
        if (string.IsNullOrWhiteSpace(taskPath) || taskPath.Length > 4096)
            throw new ArgumentException("Scheduled task path is invalid.", nameof(taskPath));
        var normalized = taskPath.Trim().Replace('/', '\\');
        if (!normalized.StartsWith('\\'))
            normalized = "\\" + normalized;
        if (normalized.EndsWith('\\') || normalized.Contains("..", StringComparison.Ordinal))
            throw new ArgumentException("Scheduled task path is invalid.", nameof(taskPath));
        return normalized;
    }

    private static void ReleaseCom(object? instance)
    {
        if (instance is not null && Marshal.IsComObject(instance))
            Marshal.FinalReleaseComObject(instance);
    }
}
