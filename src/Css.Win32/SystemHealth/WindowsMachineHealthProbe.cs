using System.Diagnostics;
using System.Runtime.InteropServices;
using Css.Core.Apps;

namespace Css.Win32.SystemHealth;

public sealed class WindowsMachineHealthProbe : IMachineHealthProbe
{
    public MachineHealthObservation Observe()
    {
        if (!OperatingSystem.IsWindows())
            return UnavailableObservation();

        return new MachineHealthObservation
        {
            ObservedAtUtc = DateTimeOffset.UtcNow,
            SecondaryDrive = ObserveSecondaryDrive(),
            Memory = ObserveMemory(),
            Battery = ObserveBattery(),
            Hardware = new WindowsHardwareSummaryProbe().Observe(),
            ProcessCount = ObserveProcessCount()
        };
    }

    private static LocalDriveHealthObservation ObserveSecondaryDrive()
    {
        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(candidate =>
                candidate.Name.Equals(@"D:\", StringComparison.OrdinalIgnoreCase));
            if (drive is null)
                return Drive(MachineMetricAvailability.NotPresent);
            if (drive.DriveType != DriveType.Fixed || !drive.IsReady)
                return Drive(MachineMetricAvailability.Unavailable);

            return new LocalDriveHealthObservation
            {
                Availability = MachineMetricAvailability.Available,
                TotalBytes = Math.Max(0, drive.TotalSize),
                FreeBytes = Math.Clamp(drive.AvailableFreeSpace, 0, Math.Max(0, drive.TotalSize))
            };
        }
        catch
        {
            return Drive(MachineMetricAvailability.Unavailable);
        }
    }

    private static MemoryHealthObservation ObserveMemory()
    {
        try
        {
            var status = new MemoryStatusEx
            {
                Length = (uint)Marshal.SizeOf<MemoryStatusEx>()
            };
            if (!GlobalMemoryStatusEx(ref status) || status.TotalPhysical == 0)
                return Memory(MachineMetricAvailability.Unavailable);

            var total = ToInt64(status.TotalPhysical);
            var available = Math.Clamp(ToInt64(status.AvailablePhysical), 0, total);
            return new MemoryHealthObservation
            {
                Availability = MachineMetricAvailability.Available,
                TotalBytes = total,
                AvailableBytes = available,
                LoadPercent = Math.Clamp((int)status.MemoryLoad, 0, 100)
            };
        }
        catch
        {
            return Memory(MachineMetricAvailability.Unavailable);
        }
    }

    private static BatteryHealthObservation ObserveBattery()
    {
        try
        {
            if (!GetSystemPowerStatus(out var status))
                return Battery(MachineMetricAvailability.Unavailable);
            if (status.BatteryFlag == byte.MaxValue)
                return Battery(MachineMetricAvailability.Unavailable);
            if ((status.BatteryFlag & 128) != 0)
                return Battery(MachineMetricAvailability.NotPresent);

            return new BatteryHealthObservation
            {
                Availability = MachineMetricAvailability.Available,
                ChargePercent = status.BatteryLifePercent == byte.MaxValue
                    ? null
                    : Math.Clamp((int)status.BatteryLifePercent, 0, 100),
                IsOnAcPower = status.AcLineStatus switch
                {
                    0 => false,
                    1 => true,
                    _ => null
                },
                IsCharging = (status.BatteryFlag & 8) != 0
            };
        }
        catch
        {
            return Battery(MachineMetricAvailability.Unavailable);
        }
    }

    private static int? ObserveProcessCount()
    {
        try
        {
            var processes = Process.GetProcesses();
            try
            {
                return Math.Clamp(processes.Length, 0, 100_000);
            }
            finally
            {
                foreach (var process in processes)
                    process.Dispose();
            }
        }
        catch
        {
            return null;
        }
    }

    private static MachineHealthObservation UnavailableObservation() =>
        new()
        {
            ObservedAtUtc = DateTimeOffset.UtcNow,
            SecondaryDrive = Drive(MachineMetricAvailability.Unavailable),
            Memory = Memory(MachineMetricAvailability.Unavailable),
            Battery = Battery(MachineMetricAvailability.Unavailable)
        };

    private static LocalDriveHealthObservation Drive(MachineMetricAvailability availability) =>
        new() { Availability = availability };

    private static MemoryHealthObservation Memory(MachineMetricAvailability availability) =>
        new() { Availability = availability };

    private static BatteryHealthObservation Battery(MachineMetricAvailability availability) =>
        new() { Availability = availability };

    private static long ToInt64(ulong value) =>
        value > long.MaxValue ? long.MaxValue : (long)value;

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemPowerStatus(out SystemPowerStatus status);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemPowerStatus
    {
        public byte AcLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }
}
