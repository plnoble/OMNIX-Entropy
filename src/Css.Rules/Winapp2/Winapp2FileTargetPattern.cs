namespace Css.Rules.Winapp2;

public sealed class Winapp2FileTargetPattern
{
    private const int MaxFileNamePatterns = 64;
    private readonly Winapp2PathPattern _directoryPattern;

    private Winapp2FileTargetPattern(
        string rawExpression,
        Winapp2PathPattern directoryPattern,
        IReadOnlyList<string> fileNamePatterns,
        bool recurse,
        bool removeSelf)
    {
        RawExpression = rawExpression;
        _directoryPattern = directoryPattern;
        FileNamePatterns = fileNamePatterns;
        Recurse = recurse;
        RemoveSelf = removeSelf;
    }

    public string RawExpression { get; }
    public IReadOnlyList<string> FileNamePatterns { get; }
    public bool Recurse { get; }
    public bool RemoveSelf { get; }

    public bool IsOwnedBy(string profilePath) => _directoryPattern.IsOwnedBy(profilePath);

    public bool MatchesDirectory(string directoryPath) =>
        _directoryPattern.MatchesPath(directoryPath, allowDescendants: Recurse);

    public bool CanMatchWithin(string directoryPath) =>
        _directoryPattern.CanMatchWithin(directoryPath, allowDescendants: Recurse);

    public bool MatchesFileName(string fileName) =>
        FileNamePatterns.Any(pattern => Winapp2Wildcard.Matches(pattern, fileName));

    public static bool TryParse(
        string expression,
        Func<string, string> expandVariables,
        out Winapp2FileTargetPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(expandVariables);
        pattern = null!;
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var parts = expression.Split('|');
        if (parts.Length is < 2 or > 3)
            return false;
        if (!Winapp2PathPattern.TryCreate(parts[0], expandVariables, out var directoryPattern))
            return false;

        var filePatterns = parts[1]
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (filePatterns.Length is 0 or > MaxFileNamePatterns)
            return false;

        var flag = parts.Length == 3 ? parts[2].Trim() : string.Empty;
        if (flag.Length > 0
            && !flag.Equals("RECURSE", StringComparison.OrdinalIgnoreCase)
            && !flag.Equals("REMOVESELF", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        pattern = new Winapp2FileTargetPattern(
            expression,
            directoryPattern,
            filePatterns,
            flag.Equals("RECURSE", StringComparison.OrdinalIgnoreCase),
            flag.Equals("REMOVESELF", StringComparison.OrdinalIgnoreCase));
        return true;
    }
}

internal sealed class Winapp2PathPattern
{
    private Winapp2PathPattern(
        string root,
        IReadOnlyList<string> segments,
        string? canonicalPath)
    {
        Root = root;
        Segments = segments;
        CanonicalPath = canonicalPath;
    }

    private string Root { get; }
    private IReadOnlyList<string> Segments { get; }
    private string? CanonicalPath { get; }

    public bool IsOwnedBy(string owner)
    {
        if (!TryCanonical(owner, out var canonicalOwner))
            return false;
        if (CanonicalPath is not null)
            return IsSameOrDescendant(CanonicalPath, canonicalOwner);

        var ownerRoot = Path.GetPathRoot(canonicalOwner);
        if (!Root.Equals(ownerRoot, StringComparison.OrdinalIgnoreCase))
            return false;
        var ownerSegments = PathSegments(canonicalOwner);
        if (Segments.Count < ownerSegments.Count)
            return false;

        for (var index = 0; index < ownerSegments.Count; index++)
        {
            var patternSegment = Segments[index];
            if (patternSegment.IndexOfAny(['*', '?']) >= 0
                && patternSegment.All(character => character is '*' or '?'))
            {
                return false;
            }
            if (!Winapp2Wildcard.Matches(patternSegment, ownerSegments[index]))
                return false;
        }

        return true;
    }

    public bool MatchesPath(string path, bool allowDescendants)
    {
        if (!TryCanonical(path, out var canonical))
            return false;
        if (CanonicalPath is not null)
        {
            return allowDescendants
                ? IsSameOrDescendant(canonical, CanonicalPath)
                : canonical.Equals(CanonicalPath, StringComparison.OrdinalIgnoreCase);
        }

        var root = Path.GetPathRoot(canonical);
        if (!Root.Equals(root, StringComparison.OrdinalIgnoreCase))
            return false;
        var candidateSegments = PathSegments(canonical);
        if (candidateSegments.Count < Segments.Count
            || (!allowDescendants && candidateSegments.Count != Segments.Count))
        {
            return false;
        }

        for (var index = 0; index < Segments.Count; index++)
        {
            if (!Winapp2Wildcard.Matches(Segments[index], candidateSegments[index]))
                return false;
        }

        return true;
    }

    public bool CanMatchWithin(string path, bool allowDescendants)
    {
        if (!TryCanonical(path, out var canonical))
            return false;
        if (CanonicalPath is not null)
        {
            return IsSameOrDescendant(CanonicalPath, canonical)
                || (allowDescendants && IsSameOrDescendant(canonical, CanonicalPath));
        }

        var root = Path.GetPathRoot(canonical);
        if (!Root.Equals(root, StringComparison.OrdinalIgnoreCase))
            return false;
        var candidateSegments = PathSegments(canonical);
        if (candidateSegments.Count > Segments.Count && !allowDescendants)
            return false;

        var comparableCount = Math.Min(candidateSegments.Count, Segments.Count);
        for (var index = 0; index < comparableCount; index++)
        {
            if (!Winapp2Wildcard.Matches(Segments[index], candidateSegments[index]))
                return false;
        }

        return true;
    }

    public static bool TryCreate(
        string expression,
        Func<string, string> expandVariables,
        out Winapp2PathPattern pattern)
    {
        pattern = null!;
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        try
        {
            var expanded = expandVariables(expression).Trim().Trim('"');
            if (!Path.IsPathFullyQualified(expanded))
                return false;

            if (expanded.IndexOfAny(['*', '?']) < 0)
            {
                if (!TryCanonical(expanded, out var canonical))
                    return false;
                var canonicalRoot = Path.GetPathRoot(canonical);
                if (string.IsNullOrWhiteSpace(canonicalRoot))
                    return false;
                pattern = new Winapp2PathPattern(canonicalRoot, PathSegments(canonical), canonical);
                return true;
            }

            var root = Path.GetPathRoot(expanded);
            if (string.IsNullOrWhiteSpace(root) || root.IndexOfAny(['*', '?']) >= 0)
                return false;
            var segments = PathSegments(expanded);
            if (segments.Count == 0 || segments.Any(segment => segment is "." or ".."))
                return false;
            pattern = new Winapp2PathPattern(root, segments, canonicalPath: null);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static bool TryCanonical(string path, out string canonical)
    {
        canonical = string.Empty;
        try
        {
            if (!Path.IsPathFullyQualified(path) || path.IndexOfAny(['*', '?']) >= 0)
                return false;
            canonical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            return canonical.Length > 0;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static IReadOnlyList<string> PathSegments(string path)
    {
        var root = Path.GetPathRoot(path) ?? string.Empty;
        return path[root.Length..]
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
            .ToArray();
    }

    private static bool IsSameOrDescendant(string candidate, string owner)
    {
        if (candidate.Equals(owner, StringComparison.OrdinalIgnoreCase))
            return true;
        return candidate.Length > owner.Length
            && candidate.StartsWith(owner, StringComparison.OrdinalIgnoreCase)
            && IsDirectorySeparator(candidate[owner.Length]);
    }

    private static bool IsDirectorySeparator(char character) =>
        character == Path.DirectorySeparatorChar || character == Path.AltDirectorySeparatorChar;
}

internal static class Winapp2Wildcard
{
    public static bool Matches(string pattern, string value)
    {
        if (pattern.Equals("*.*", StringComparison.OrdinalIgnoreCase))
            pattern = "*";

        var patternIndex = 0;
        var valueIndex = 0;
        var starIndex = -1;
        var retryValueIndex = 0;
        while (valueIndex < value.Length)
        {
            if (patternIndex < pattern.Length
                && (pattern[patternIndex] == '?'
                    || char.ToUpperInvariant(pattern[patternIndex]) == char.ToUpperInvariant(value[valueIndex])))
            {
                patternIndex++;
                valueIndex++;
            }
            else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                starIndex = patternIndex++;
                retryValueIndex = valueIndex;
            }
            else if (starIndex >= 0)
            {
                patternIndex = starIndex + 1;
                valueIndex = ++retryValueIndex;
            }
            else
            {
                return false;
            }
        }

        while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            patternIndex++;
        return patternIndex == pattern.Length;
    }
}
