using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Css.Rules.Models;

namespace Css.Scanner.Disk;

/// <summary>
/// Maps scanned <see cref="CategoryNode"/>s to <see cref="UsageCategory"/> using the glob patterns
/// in <see cref="ScanRules"/>, and flags C:\ top-level folders that are not in the expected-root
/// allowlist — the core of pain point #2 ("I don't know what's eating C:").
/// </summary>
public sealed class CategoryClassifier
{
    private readonly ScanRules _rules;
    private readonly ConcurrentDictionary<string, Regex> _regexCache = new();

    public CategoryClassifier(ScanRules rules) => _rules = rules;

    /// <summary>Classifies a full tree of top-level nodes in place, marking unexpected roots.</summary>
    public void Classify(IEnumerable<CategoryNode> topLevelNodes, string driveRoot)
    {
        var expected = new HashSet<string>(_rules.ExpectedRootDirs, StringComparer.OrdinalIgnoreCase);
        foreach (var node in topLevelNodes)
        {
            node.IsUnexpectedRoot = !expected.Contains(node.Name);
            node.Category = ClassifyPath(node.Path ?? node.Name);
            ReclassifyChildren(node);
        }
    }

    private void ReclassifyChildren(CategoryNode node)
    {
        foreach (var child in node.Children)
        {
            child.Category = ClassifyPath(child.Path ?? child.Name);
            ReclassifyChildren(child);
        }
    }

    /// <summary>Resolves a path to a category via first-match against the rules' category patterns.</summary>
    public UsageCategory ClassifyPath(string path)
    {
        foreach (var (catName, patterns) in _rules.CategoryPatterns)
        {
            foreach (var pattern in patterns)
            {
                if (GlobMatch(pattern, path))
                    return ParseCategory(catName);
            }
        }
        return UsageCategory.Other;
    }

    private static UsageCategory ParseCategory(string name) =>
        Enum.TryParse<UsageCategory>(name, true, out var c) ? c : UsageCategory.Other;

    /// <summary>
    /// Glob matcher. Patterns may contain <c>**</c> (any depth, crosses separators) and <c>*</c>
    /// (single path segment). A pattern with no wildcard is a directory prefix match (so
    /// <c>C:\Windows</c> matches <c>C:\Windows\System32</c>). Case-insensitive, backslash-normalized.
    /// </summary>
    private bool GlobMatch(string pattern, string path)
    {
        var p = pattern.Replace('/', '\\').TrimEnd('\\');
        var t = (path ?? "").Replace('/', '\\').TrimEnd('\\');

        if (p.Length == 0) return false;

        // No wildcards → directory prefix match (with separator boundary).
        if (!p.Contains('*'))
        {
            return t.Equals(p, StringComparison.OrdinalIgnoreCase)
                || t.StartsWith(p + "\\", StringComparison.OrdinalIgnoreCase);
        }

        // Wildcard pattern → compiled regex. ** => .* (crosses \), * => [^\]* (single segment).
        var re = _regexCache.GetOrAdd(p, BuildRegex);
        return re.IsMatch(t);
    }

    private static Regex BuildRegex(string pattern)
    {
        // Escape everything, then turn escaped "\*\*" into ".*" and remaining "\*" into "[^\]*".
        var escaped = Regex.Escape(pattern);
        escaped = escaped.Replace("\\*\\*", ".*").Replace("\\*", "[^\\\\]*");
        return new Regex("^" + escaped + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
