using System;

namespace Css.Core.Recovery;

public sealed class WindowsRestorePointInfo
{
    public required long SequenceNumber { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required int RestorePointType { get; init; }
    public required int EventType { get; init; }
    public bool IsReadOnlyEvidence => true;
}
