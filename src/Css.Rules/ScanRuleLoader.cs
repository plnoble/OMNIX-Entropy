using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Css.Rules.Models;

namespace Css.Rules;

/// <summary>
/// Loads <see cref="ScanRules"/> from a JSON file, expanding %EnvVar% tokens in patterns.
/// Throws <see cref="FileNotFoundException"/> if the file is missing so callers fail fast
/// (least-privilege/config-fails-fast principle).
/// </summary>
public sealed class ScanRuleLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip };

    public ScanRules Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Scan rules file not found: " + path, path);

        var json = File.ReadAllText(path);
        var rules = JsonSerializer.Deserialize<ScanRules>(json, JsonOpts)
                    ?? throw new InvalidDataException("Scan rules file is empty or invalid: " + path);

        rules.ExpectedRootDirs = ExpandAll(rules.ExpectedRootDirs);
        var expanded = new Dictionary<string, List<string>>();
        foreach (var (cat, patterns) in rules.CategoryPatterns)
            expanded[cat] = ExpandAll(patterns);
        rules.CategoryPatterns = expanded;
        return rules;
    }

    private static List<string> ExpandAll(List<string> values)
    {
        var result = new List<string>(values.Count);
        foreach (var v in values) result.Add(Environment.ExpandEnvironmentVariables(v));
        return result;
    }
}
