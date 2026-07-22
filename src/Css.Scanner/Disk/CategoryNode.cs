using System;
using System.Collections.Generic;

namespace Css.Scanner.Disk;

/// <summary>High-level usage category used for treemap coloring and grouping.</summary>
public enum UsageCategory
{
    System,
    Programs,
    UserProfiles,
    AppData,
    SystemFiles,
    ShadowStorage,
    RecycleBin,
    PackageCaches,
    Temp,
    Mystery,
    Other
}

/// <summary>
/// A node in the scanned directory/size tree. Used both for the full category tree and for
/// individual root-folder findings. SizeBytes is the recursive sum; children enable drill-down.
/// </summary>
public sealed class CategoryNode
{
    public string Name { get; init; } = "";
    public string? Path { get; init; }
    public bool IsFile { get; init; }
    public long SizeBytes { get; set; }
    public UsageCategory Category { get; set; } = UsageCategory.Other;
    public List<CategoryNode> Children { get; } = [];

    /// <summary>True when this is a C:\ top-level folder not in the expected-root allowlist.</summary>
    public bool IsUnexpectedRoot { get; set; }

    public DateTime? LastWriteUtc { get; set; }
}
