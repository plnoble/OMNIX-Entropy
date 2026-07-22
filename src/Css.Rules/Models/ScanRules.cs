using System.Collections.Generic;

namespace Css.Rules.Models;

/// <summary>
/// C: root-cause scan rules. Drives the category classifier and the unexpected-root-folder detector.
/// Loaded from rules.scan.json; env-style %Var% tokens in patterns are expanded by the loader.
/// </summary>
public sealed class ScanRules
{
    public int Version { get; set; } = 1;

    /// <summary>Legitimate C:\ top-level entries. Anything else is flagged as unexpected (pain point #2).</summary>
    public List<string> ExpectedRootDirs { get; set; } = [];

    /// <summary>Maps a UsageCategory name to glob patterns matched against full paths.</summary>
    public Dictionary<string, List<string>> CategoryPatterns { get; set; } = [];
}
