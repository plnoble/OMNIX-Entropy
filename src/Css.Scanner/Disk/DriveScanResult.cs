using System.Collections.Generic;

namespace Css.Scanner.Disk;

/// <summary>Result of a full drive root-cause scan.</summary>
public sealed class DriveScanResult
{
    public string Drive { get; init; } = "";
    public long TotalBytes { get; init; }
    public long FreeBytes { get; init; }
    public long UsedBytes => TotalBytes - FreeBytes;

    /// <summary>Top-level directory nodes (classified, unexpected flagged).</summary>
    public List<CategoryNode> TopLevel { get; init; } = [];

    /// <summary>System files/stores probed without crawling.</summary>
    public List<BigRock> BigRocks { get; init; } = [];

    /// <summary>Convenience: top-level nodes flagged as unexpected roots.</summary>
    public List<CategoryNode> UnexpectedRoots =>
        TopLevel.FindAll(n => n.IsUnexpectedRoot);
}
