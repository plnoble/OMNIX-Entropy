using Css.Core.Software;

namespace Css.Core.Apps;

public enum ApplicationCrashObservationAvailability
{
    Available,
    NotFound,
    Unavailable
}

public sealed class ApplicationCrashObservation
{
    public required ApplicationCrashObservationAvailability Availability { get; init; }
    public required string SoftwareName { get; init; }
    public required DateTimeOffset ObservedAtUtc { get; init; }
    public required DateTimeOffset WindowStartUtc { get; init; }
    public int MatchCount { get; init; }
    public DateTimeOffset? LatestOccurrenceUtc { get; init; }
    public bool CanExecuteDirectly => false;
}

public interface IApplicationCrashProbe
{
    ApplicationCrashObservation Observe(SoftwareProfile profile);
}
