using System.Collections.Generic;
using System.IO;
using System.Linq;
using Css.Core.Software;

namespace Css.InstallGuard.Routing;

public sealed class InstallRoute
{
    public required string SoftwareName { get; init; }
    public SoftwareCategory Category { get; init; }
    public required string TargetInstallPath { get; init; }
    public bool RequiresUserConfirmation { get; init; } = true;
    public required string Reason { get; init; }
    public bool FromUserMemory { get; init; }
    public string MemoryScope { get; init; } = "";
}

public sealed class InstallRoutingRule
{
    public SoftwareCategory Category { get; init; }
    public required string TargetRoot { get; init; }
    public IReadOnlyList<string> NameHints { get; init; } = [];
}

public sealed class InstallRoutingEngine
{
    private readonly IReadOnlyDictionary<SoftwareCategory, InstallRoutingRule> _rules;

    public InstallRoutingEngine(IEnumerable<InstallRoutingRule> rules)
    {
        _rules = rules.ToDictionary(r => r.Category);
    }

    public static InstallRoutingEngine CreateDefault() =>
        new(
        [
            new InstallRoutingRule { Category = SoftwareCategory.Normal, TargetRoot = @"D:\Software" },
            new InstallRoutingRule { Category = SoftwareCategory.Game, TargetRoot = @"D:\Game" },
            new InstallRoutingRule { Category = SoftwareCategory.Ai, TargetRoot = @"D:\Agent" },
            new InstallRoutingRule { Category = SoftwareCategory.DevelopmentTool, TargetRoot = @"D:\Development" },
            new InstallRoutingRule { Category = SoftwareCategory.SystemTool, TargetRoot = @"D:\Development" },
            new InstallRoutingRule { Category = SoftwareCategory.Unknown, TargetRoot = @"D:\Software" }
        ]);

    public InstallRoute Recommend(
        string softwareName,
        SoftwareCategory category,
        InstallRoutingMemory? memory = null)
    {
        var effectiveCategory = _rules.ContainsKey(category) ? category : SoftwareCategory.Unknown;
        var safeName = SanitizePathSegment(softwareName);
        var rememberedSoftware = memory?.FindSoftwareRule(softwareName);
        var rememberedCategory = rememberedSoftware is null
            ? memory?.FindCategoryRule(effectiveCategory)
            : null;
        var rememberedRule = rememberedSoftware ?? rememberedCategory;
        var targetRoot = rememberedRule?.TargetRoot ?? _rules[effectiveCategory].TargetRoot;

        return new InstallRoute
        {
            SoftwareName = safeName,
            Category = effectiveCategory,
            TargetInstallPath = Path.Combine(targetRoot, safeName, "Install"),
            RequiresUserConfirmation = true,
            Reason = rememberedRule is null
                ? "Recommended from the default OMNIX-Entropy storage layout."
                : "Recommended from your remembered install routing rule.",
            FromUserMemory = rememberedRule is not null,
            MemoryScope = rememberedSoftware is not null
                ? "Software"
                : rememberedCategory is not null
                    ? "Category"
                    : ""
        };
    }

    private static string SanitizePathSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Trim().Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var result = new string(chars);
        return string.IsNullOrWhiteSpace(result) ? "UnknownSoftware" : result;
    }
}
