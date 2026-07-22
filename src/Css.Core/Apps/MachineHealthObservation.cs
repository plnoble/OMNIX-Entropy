namespace Css.Core.Apps;

public enum MachineMetricAvailability
{
    Available,
    NotPresent,
    Unavailable
}

public sealed class LocalDriveHealthObservation
{
    public required MachineMetricAvailability Availability { get; init; }
    public long TotalBytes { get; init; }
    public long FreeBytes { get; init; }
}

public sealed class MemoryHealthObservation
{
    public required MachineMetricAvailability Availability { get; init; }
    public long TotalBytes { get; init; }
    public long AvailableBytes { get; init; }
    public int LoadPercent { get; init; }
}

public sealed class BatteryHealthObservation
{
    public required MachineMetricAvailability Availability { get; init; }
    public int? ChargePercent { get; init; }
    public bool? IsOnAcPower { get; init; }
    public bool? IsCharging { get; init; }
}

public sealed class MachineHealthObservation
{
    public required DateTimeOffset ObservedAtUtc { get; init; }
    public required LocalDriveHealthObservation SecondaryDrive { get; init; }
    public required MemoryHealthObservation Memory { get; init; }
    public required BatteryHealthObservation Battery { get; init; }
    public HardwareSummaryObservation? Hardware { get; init; }
    public int? ProcessCount { get; init; }
    public bool CanExecuteDirectly => false;
}

public interface IMachineHealthProbe
{
    MachineHealthObservation Observe();
}
