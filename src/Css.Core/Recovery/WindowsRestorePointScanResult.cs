namespace Css.Core.Recovery;

public enum WindowsRestorePointScanState
{
    Completed,
    TimedOut,
    Failed
}

public sealed class WindowsRestorePointScanResult
{
    public required WindowsRestorePointScanState State { get; init; }
    public required IReadOnlyList<WindowsRestorePointInfo> Points { get; init; }
    public bool IsComplete => State == WindowsRestorePointScanState.Completed;
}
