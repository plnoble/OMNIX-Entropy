namespace Css.Core.Apps;

public enum ApplicationGrowthObservationAvailability
{
    Available,
    InsufficientBaseline,
    Unavailable
}

public sealed class ApplicationGrowthObservation
{
    public required ApplicationGrowthObservationAvailability Availability { get; init; }
    public required string SoftwareName { get; init; }
    public int ObservedSnapshotCount { get; init; }
    public long RecentGrowthBytes { get; init; }
    public int CDriveWriteLocationCount { get; init; }
    public int CacheLocationCount { get; init; }
    public bool CanExecuteDirectly => false;
}
