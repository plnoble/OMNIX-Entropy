using Css.Core.Software;

namespace Css.Scanner.Disk;

public static class ScanSnapshotBuilder
{
    public const string SharedSoftwareOwner = "多个软件";
    private const int MaximumTreeNodes = 250_000;
    private const int MaximumKnownPaths = 8_192;
    public const int MaximumSnapshotItems = 2_048;

    public static ScanSnapshot Build(
        DriveScanResult result,
        DateTimeOffset capturedAt,
        IReadOnlyList<SoftwareProfile>? softwareProfiles = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        softwareProfiles ??= [];
        var items = new Dictionary<string, ScanSnapshotItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in result.TopLevel
                     .OrderByDescending(node => node.SizeBytes)
                     .Take(MaximumSnapshotItems))
        {
            if (TryCanonicalPath(node.Path, out var path))
            {
                items[path] = new ScanSnapshotItem(
                    path,
                    node.Category.ToString(),
                    Math.Max(0, node.SizeBytes));
            }
        }

        var nodesByPath = FlattenNodes(result.TopLevel);
        var claims = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var knownPathCount = 0;
        foreach (var profile in softwareProfiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Name))
                continue;
            foreach (var knownPath in KnownCDrivePaths(profile, result.Drive))
            {
                if (++knownPathCount > MaximumKnownPaths)
                    break;
                if (!nodesByPath.ContainsKey(knownPath))
                    continue;
                if (!claims.TryGetValue(knownPath, out var owners))
                {
                    owners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    claims[knownPath] = owners;
                }
                owners.Add(profile.Name.Trim());
            }
            if (knownPathCount > MaximumKnownPaths)
                break;
        }

        foreach (var claim in claims
                     .Select(pair => new
                     {
                         Path = pair.Key,
                         Owners = pair.Value,
                         Node = nodesByPath[pair.Key]
                     })
                     .OrderByDescending(item => item.Node.SizeBytes)
                     .Take(Math.Max(0, MaximumSnapshotItems - items.Count)))
        {
            var owner = claim.Owners.Count == 1
                ? claim.Owners.Single()
                : SharedSoftwareOwner;
            items[claim.Path] = new ScanSnapshotItem(
                claim.Path,
                owner,
                Math.Max(0, claim.Node.SizeBytes));
        }

        return new ScanSnapshot(
            capturedAt,
            items.Values
                .OrderByDescending(item => item.SizeBytes)
                .ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    private static IReadOnlyDictionary<string, CategoryNode> FlattenNodes(
        IReadOnlyList<CategoryNode> roots)
    {
        var result = new Dictionary<string, CategoryNode>(StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<CategoryNode>(roots.Reverse());
        var observed = 0;
        while (pending.Count > 0 && observed++ < MaximumTreeNodes)
        {
            var node = pending.Pop();
            if (TryCanonicalPath(node.Path, out var path))
                result.TryAdd(path, node);
            for (var index = node.Children.Count - 1; index >= 0; index--)
                pending.Push(node.Children[index]);
        }
        return result;
    }

    private static IEnumerable<string> KnownCDrivePaths(
        SoftwareProfile profile,
        string driveRoot)
    {
        var candidates = new List<string?> { profile.InstallPath };
        candidates.AddRange(profile.DataPaths);
        candidates.AddRange(profile.CachePaths);
        candidates.AddRange(profile.LogPaths);
        candidates.AddRange(profile.CDriveWritePaths);
        var expectedRoot = NormalizeRoot(driveRoot);
        return candidates
            .Where(path => TryCanonicalPath(path, out _))
            .Select(path => Path.GetFullPath(path!))
            .Where(path => string.Equals(
                NormalizeRoot(path),
                expectedRoot,
                StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string? NormalizeRoot(string path)
    {
        try
        {
            return Path.GetPathRoot(Path.GetFullPath(path))?.TrimEnd('\\', '/');
        }
        catch
        {
            return null;
        }
    }

    private static bool TryCanonicalPath(string? path, out string canonical)
    {
        canonical = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            canonical = Path.GetFullPath(path);
            var root = Path.GetPathRoot(canonical);
            if (!string.Equals(canonical, root, StringComparison.OrdinalIgnoreCase))
                canonical = canonical.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
