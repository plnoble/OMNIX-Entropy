namespace Css.Core.Apps;

public sealed class HardwareSummaryObservation
{
    public required MachineMetricAvailability Availability { get; init; }
    public string? CpuName { get; init; }
    public int? LogicalProcessorCount { get; init; }
    public string? GpuName { get; init; }
    public string? OperatingSystem { get; init; }
    public string? Architecture { get; init; }
    public bool CanExecuteDirectly => false;
}
