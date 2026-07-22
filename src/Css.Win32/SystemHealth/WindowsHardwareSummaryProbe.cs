using System.Management;
using System.Runtime.InteropServices;
using Css.Core.Apps;
using Microsoft.Win32;

namespace Css.Win32.SystemHealth;

public sealed class WindowsHardwareSummaryProbe
{
    public HardwareSummaryObservation Observe()
    {
        if (!OperatingSystem.IsWindows())
            return Unavailable();

        var (cpuName, logicalProcessors) = ReadCpu();
        if (cpuName is null)
            (cpuName, logicalProcessors) = ReadCpuFallback();
        var gpuName = ReadGpu() ?? ReadGpuFallback();
        var operatingSystem = ReadOperatingSystem();
        var architecture = ArchitectureLabel();
        var available = cpuName is not null
            || gpuName is not null
            || operatingSystem is not null
            || architecture is not null;

        return new HardwareSummaryObservation
        {
            Availability = available
                ? MachineMetricAvailability.Available
                : MachineMetricAvailability.Unavailable,
            CpuName = cpuName,
            LogicalProcessorCount = logicalProcessors,
            GpuName = gpuName,
            OperatingSystem = operatingSystem,
            Architecture = architecture
        };
    }

    private static (string? Name, int? LogicalProcessors) ReadCpu()
    {
        try
        {
            using var searcher = CreateSearcher(
                "SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor");
            using var results = searcher.Get();
            foreach (var item in results.Cast<ManagementBaseObject>().Take(1))
            {
                using (item)
                {
                    var name = SafeLabel(item["Name"]?.ToString(), 120);
                    var logical = ToBoundedInt(item["NumberOfLogicalProcessors"]);
                    return (name, logical);
                }
            }
        }
        catch
        {
            // A missing or unhealthy WMI provider makes this field unavailable.
        }

        return (null, null);
    }

    private static string? ReadGpu()
    {
        try
        {
            using var searcher = CreateSearcher("SELECT Name FROM Win32_VideoController");
            using var results = searcher.Get();
            var names = results
                .Cast<ManagementBaseObject>()
                .Take(4)
                .Select(item =>
                {
                    using (item)
                        return SafeLabel(item["Name"]?.ToString(), 120);
                })
                .Where(name => name is not null)
                .Cast<string>()
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .Take(2)
                .ToArray();
            return names.Length == 0 ? null : string.Join("、", names);
        }
        catch
        {
            return null;
        }
    }

    private static (string? Name, int? LogicalProcessors) ReadCpuFallback()
    {
        try
        {
            var name = SafeLabel(
                Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                    "ProcessorNameString",
                    null)?.ToString(),
                120);
            var logical = Math.Clamp(Environment.ProcessorCount, 1, 4096);
            return (name, logical);
        }
        catch
        {
            return (null, null);
        }
    }

    private static string? ReadGpuFallback()
    {
        try
        {
            var names = new List<string>();
            for (uint index = 0; index < 16; index++)
            {
                var device = DisplayDevice.Create();
                if (!EnumDisplayDevices(null, index, ref device, 0))
                    break;

                var name = SafeLabel(device.Description, 120);
                if (name is not null
                    && !names.Contains(name, StringComparer.CurrentCultureIgnoreCase))
                {
                    names.Add(name);
                }
                if (names.Count == 2)
                    break;
            }

            return names.Count == 0 ? null : string.Join("、", names);
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadOperatingSystem()
    {
        try
        {
            using var searcher = CreateSearcher(
                "SELECT Caption, Version FROM Win32_OperatingSystem");
            using var results = searcher.Get();
            foreach (var item in results.Cast<ManagementBaseObject>().Take(1))
            {
                using (item)
                {
                    var caption = SafeLabel(item["Caption"]?.ToString(), 100);
                    var version = SafeLabel(item["Version"]?.ToString(), 40);
                    if (caption is null)
                        return version;
                    return version is null ? caption : $"{caption} {version}";
                }
            }
        }
        catch
        {
            // Runtime information below remains available even when WMI is not.
        }

        return SafeLabel(RuntimeInformation.OSDescription, 120);
    }

    private static string? ArchitectureLabel()
    {
        try
        {
            var bits = Environment.Is64BitOperatingSystem ? "64 位操作系统" : "32 位操作系统";
            return $"{RuntimeInformation.OSArchitecture}（{bits}）";
        }
        catch
        {
            return null;
        }
    }

    private static ManagementObjectSearcher CreateSearcher(string query) =>
        new(
            new ManagementScope(@"\\.\root\CIMV2"),
            new ObjectQuery(query),
            new System.Management.EnumerationOptions
            {
                ReturnImmediately = true,
                Rewindable = false,
                Timeout = TimeSpan.FromSeconds(2)
            });

    private static int? ToBoundedInt(object? value)
    {
        try
        {
            var parsed = Convert.ToInt64(value);
            return parsed is > 0 and <= 4096 ? (int)parsed : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeLabel(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleaned = string.Join(
            " ",
            new string(value.Where(character => !char.IsControl(character)).ToArray())
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (cleaned.Contains(":\\", StringComparison.Ordinal)
            || cleaned.Contains(":/", StringComparison.Ordinal)
            || cleaned.StartsWith("\\\\", StringComparison.Ordinal))
        {
            return null;
        }

        return cleaned.Length <= maximumLength
            ? cleaned
            : cleaned[..maximumLength].TrimEnd();
    }

    private static HardwareSummaryObservation Unavailable() =>
        new() { Availability = MachineMetricAvailability.Unavailable };

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(
        string? device,
        uint index,
        ref DisplayDevice displayDevice,
        uint flags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public int Size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Description;

        public uint StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string ReservedIdentifier;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string ReservedKey;

        public static DisplayDevice Create() =>
            new()
            {
                Size = Marshal.SizeOf<DisplayDevice>(),
                Name = string.Empty,
                Description = string.Empty,
                ReservedIdentifier = string.Empty,
                ReservedKey = string.Empty
            };
    }
}
