using Css.Core.Software;

namespace Css.Core.Apps;

public enum ApplicationRuntimeObservationAvailability
{
    Available,
    NotRunning,
    Unavailable
}

public enum ApplicationCpuActivity
{
    Unknown,
    Idle,
    Low,
    Moderate,
    High
}

public sealed class ApplicationRuntimeObservation
{
    public required ApplicationRuntimeObservationAvailability Availability { get; init; }
    public required string SoftwareName { get; init; }
    public required DateTimeOffset ObservedAtUtc { get; init; }
    public int MatchedProcessCount { get; init; }
    public long TotalWorkingSetBytes { get; init; }
    public ApplicationCpuActivity CpuActivity { get; init; } = ApplicationCpuActivity.Unknown;
    public int SampleDurationMilliseconds { get; init; }
    public bool CanExecuteDirectly => false;
}

public interface IApplicationRuntimeProbe
{
    ApplicationRuntimeObservation Observe(SoftwareProfile profile);
}
