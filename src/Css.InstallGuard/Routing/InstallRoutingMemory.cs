using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Css.Core.Software;
using Css.InstallGuard.Installers;

namespace Css.InstallGuard.Routing;

public enum InstallRoutingMemoryScope
{
    Software,
    Category
}

public sealed class InstallRoutingMemoryRule
{
    public string? SoftwareName { get; init; }
    public SoftwareCategory? Category { get; init; }
    public required string TargetRoot { get; init; }
}

public sealed class InstallRoutingMemory
{
    public static InstallRoutingMemory Empty { get; } = new([]);

    public InstallRoutingMemory(IReadOnlyList<InstallRoutingMemoryRule> rules)
    {
        Rules = rules;
    }

    public IReadOnlyList<InstallRoutingMemoryRule> Rules { get; }

    public InstallRoutingMemory RememberSoftware(
        string softwareName,
        SoftwareCategory category,
        string targetRoot) =>
        Replace(rule =>
                !string.IsNullOrWhiteSpace(rule.SoftwareName) &&
                Normalize(rule.SoftwareName) == Normalize(softwareName),
            new InstallRoutingMemoryRule
            {
                SoftwareName = softwareName.Trim(),
                Category = category,
                TargetRoot = CleanTargetRoot(targetRoot)
            });

    public InstallRoutingMemory RememberCategory(SoftwareCategory category, string targetRoot) =>
        Replace(rule =>
                string.IsNullOrWhiteSpace(rule.SoftwareName) &&
                rule.Category == category,
            new InstallRoutingMemoryRule
            {
                SoftwareName = null,
                Category = category,
                TargetRoot = CleanTargetRoot(targetRoot)
            });

    public InstallRoutingMemory RememberRoute(InstallRoute route) =>
        RememberSoftware(route.SoftwareName, route.Category, InferTargetRoot(route));

    public InstallRoutingMemory RememberRouteForCategory(InstallRoute route) =>
        RememberCategory(route.Category, InferTargetRoot(route));

    public InstallRoutingMemory ForgetRule(string ruleKey)
    {
        if (string.IsNullOrWhiteSpace(ruleKey))
            return this;

        var normalizedKey = ruleKey.Trim();
        return new InstallRoutingMemory(
            Rules.Where(rule => !RuleKey(rule).Equals(normalizedKey, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    public InstallRoutingMemoryRule? FindSoftwareRule(string softwareName) =>
        Rules.FirstOrDefault(rule =>
            !string.IsNullOrWhiteSpace(rule.SoftwareName) &&
            Normalize(rule.SoftwareName) == Normalize(softwareName));

    public InstallRoutingMemoryRule? FindCategoryRule(SoftwareCategory category) =>
        Rules.FirstOrDefault(rule =>
            string.IsNullOrWhiteSpace(rule.SoftwareName) &&
            rule.Category == category);

    private InstallRoutingMemory Replace(
        Func<InstallRoutingMemoryRule, bool> match,
        InstallRoutingMemoryRule replacement)
    {
        var next = Rules.Where(rule => !match(rule)).ToList();
        next.Add(replacement);
        return new InstallRoutingMemory(next);
    }

    private static string Normalize(string value) =>
        value.Trim().ToUpperInvariant();

    private static string RuleKey(InstallRoutingMemoryRule rule) =>
        !string.IsNullOrWhiteSpace(rule.SoftwareName)
            ? "software:" + Normalize(rule.SoftwareName)
            : "category:" + (rule.Category?.ToString() ?? SoftwareCategory.Unknown.ToString());

    private static string CleanTargetRoot(string targetRoot)
    {
        var cleaned = targetRoot.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.IsNullOrWhiteSpace(cleaned) ? @"D:\Software" : cleaned;
    }

    private static string InferTargetRoot(InstallRoute route)
    {
        var installDirectory = Directory.GetParent(route.TargetInstallPath);
        var root = installDirectory?.Parent?.FullName;
        return string.IsNullOrWhiteSpace(root) ? Path.GetDirectoryName(route.TargetInstallPath) ?? @"D:\Software" : root;
    }
}

public sealed class InstallRouteMemoryChoiceViewModel
{
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string RecommendedTarget { get; init; }
    public required string SoftwareOptionText { get; init; }
    public required string CategoryOptionText { get; init; }
    public required string CancelText { get; init; }
    public required string SafetyText { get; init; }
}

public static class InstallRouteMemoryChoicePresenter
{
    public static InstallRouteMemoryChoiceViewModel Create(InstallerDetectionResult result)
    {
        var category = CategoryLabel(result.Category);
        return new InstallRouteMemoryChoiceViewModel
        {
            Title = "\u8bb0\u4f4f\u5b89\u88c5\u4f4d\u7f6e",
            Summary = $"Computer Agent \u5efa\u8bae\u5148\u8bb0\u4f4f {result.SoftwareName} \u7684\u5b89\u88c5\u89c4\u5219\u3002\u4f60\u53ef\u4ee5\u9009\u62e9\u53ea\u9488\u5bf9\u8fd9\u4e2a\u8f6f\u4ef6\uff0c\u6216\u8005\u540c\u7c7b\u8f6f\u4ef6\u90fd\u7528\u8fd9\u4e2a\u89c4\u5219\u3002",
            RecommendedTarget = result.RecommendedRoute.TargetInstallPath,
            SoftwareOptionText = $"\u53ea\u8bb0\u4f4f {result.SoftwareName}",
            CategoryOptionText = $"\u540c\u7c7b {category} \u90fd\u8fd9\u6837\u63a8\u8350",
            CancelText = "\u6682\u65f6\u4e0d\u8bb0",
            SafetyText = "\u53ea\u8bb0\u4f4f\u89c4\u5219\uff0c\u4e0d\u4f1a\u8fd0\u884c\u5b89\u88c5\u5668\uff0c\u4e0d\u4f1a\u66ff\u4f60\u70b9\u51fb\u5b89\u88c5\uff0c\u4e5f\u4e0d\u4f1a\u4fee\u6539 Windows \u9ed8\u8ba4\u5b89\u88c5\u76ee\u5f55\u3002"
        };
    }

    private static string CategoryLabel(SoftwareCategory category) =>
        category switch
        {
            SoftwareCategory.Game => "\u6e38\u620f",
            SoftwareCategory.Ai => "AI \u5de5\u5177",
            SoftwareCategory.DevelopmentTool => "\u5f00\u53d1\u5de5\u5177",
            SoftwareCategory.SystemTool => "\u7cfb\u7edf\u5de5\u5177",
            SoftwareCategory.Normal => "\u666e\u901a\u8f6f\u4ef6",
            _ => "\u672a\u77e5\u7c7b\u578b"
        };
}

public sealed class InstallRoutingMemoryListViewModel
{
    public required string Summary { get; init; }
    public required IReadOnlyList<InstallRoutingMemoryRuleRowViewModel> Rows { get; init; }
}

public sealed class InstallRoutingMemoryRuleRowViewModel
{
    public required string RuleKey { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string SafetyText { get; init; }
    public bool CanForget { get; init; }
}

public static class InstallRoutingMemoryPresenter
{
    public static InstallRoutingMemoryListViewModel Create(InstallRoutingMemory memory)
    {
        if (memory.Rules.Count == 0)
        {
            return new InstallRoutingMemoryListViewModel
            {
                Summary = "\u8fd8\u6ca1\u6709\u8bb0\u4f4f\u5b89\u88c5\u89c4\u5219\u3002",
                Rows = []
            };
        }

        var rows = memory.Rules.Select(CreateRow).ToList();
        return new InstallRoutingMemoryListViewModel
        {
            Summary = $"\u5df2\u8bb0\u4f4f {rows.Count} \u6761\u5b89\u88c5\u63a8\u8350\u89c4\u5219\u3002",
            Rows = rows
        };
    }

    private static InstallRoutingMemoryRuleRowViewModel CreateRow(InstallRoutingMemoryRule rule)
    {
        var isSoftwareRule = !string.IsNullOrWhiteSpace(rule.SoftwareName);
        var target = rule.TargetRoot;
        return new InstallRoutingMemoryRuleRowViewModel
        {
            RuleKey = !string.IsNullOrWhiteSpace(rule.SoftwareName)
                ? "software:" + rule.SoftwareName.Trim().ToUpperInvariant()
                : "category:" + (rule.Category?.ToString() ?? SoftwareCategory.Unknown.ToString()),
            Title = isSoftwareRule
                ? $"{rule.SoftwareName} -> {target}"
                : $"{CategoryLabel(rule.Category)} -> {target}",
            Summary = isSoftwareRule
                ? "\u53ea\u9488\u5bf9\u8fd9\u4e2a\u8f6f\u4ef6\u7684\u5b89\u88c5\u5305\u4f18\u5148\u63a8\u8350\u8fd9\u4e2a\u4f4d\u7f6e\u3002"
                : "\u540c\u7c7b\u8f6f\u4ef6\u7684\u5b89\u88c5\u5305\u4f18\u5148\u63a8\u8350\u8fd9\u4e2a\u4f4d\u7f6e\u3002",
            SafetyText = "\u53ea\u5f71\u54cd\u4e0b\u6b21\u5b89\u88c5\u5206\u6790\u7684\u5efa\u8bae\uff0c\u4e0d\u4f1a\u8fd0\u884c\u5b89\u88c5\u5668\uff0c\u4e0d\u4f1a\u79fb\u52a8\u5df2\u5b89\u88c5\u8f6f\u4ef6\u3002",
            CanForget = true
        };
    }

    private static string CategoryLabel(SoftwareCategory? category) =>
        category switch
        {
            SoftwareCategory.Game => "\u6e38\u620f",
            SoftwareCategory.Ai => "AI \u5de5\u5177",
            SoftwareCategory.DevelopmentTool => "\u5f00\u53d1\u5de5\u5177",
            SoftwareCategory.SystemTool => "\u7cfb\u7edf\u5de5\u5177",
            SoftwareCategory.Normal => "\u666e\u901a\u8f6f\u4ef6",
            _ => "\u672a\u77e5\u7c7b\u578b"
        };
}

public static class InstallRoutingMemoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static InstallRoutingMemory Load(string path)
    {
        if (!File.Exists(path))
            return InstallRoutingMemory.Empty;

        var json = File.ReadAllText(path);
        var rules = JsonSerializer.Deserialize<List<InstallRoutingMemoryRule>>(json, JsonOptions) ?? [];
        return new InstallRoutingMemory(rules);
    }

    public static void Save(string path, InstallRoutingMemory memory)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, JsonSerializer.Serialize(memory.Rules, JsonOptions));
    }
}
