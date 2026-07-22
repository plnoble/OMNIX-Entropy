using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Agent;

public sealed class AgentSkillCardViewModel
{
    public required AgentSkillCategory Category { get; init; }
    public required string Title { get; init; }
    public required string CategoryLabel { get; init; }
    public required string Description { get; init; }
    public required string ModeLabel { get; init; }
    public required string RiskLabel { get; init; }
    public required string NextStepLabel { get; init; }
    public required string SafetyHint { get; init; }
}

public static class AgentSkillCardPresenter
{
    public static IReadOnlyList<AgentSkillCardViewModel> CreateDefault() =>
        AgentSkillCatalog.CreateDefault().Skills.Select(Create).ToList();

    public static AgentSkillCardViewModel Create(AgentSkill skill)
    {
        var category = CategoryLabel(skill.Category);
        return new AgentSkillCardViewModel
        {
            Category = skill.Category,
            CategoryLabel = category,
            Title = $"{category} / {SkillName(skill.Category)}",
            Description = Description(skill.Category),
            ModeLabel = ModeLabel(skill.ExecutionMode),
            RiskLabel = RiskLabel(skill.Risk),
            NextStepLabel = NextStepLabel(skill.Category, skill.ExecutionMode),
            SafetyHint = SafetyHint(skill.Category, skill.ExecutionMode)
        };
    }

    private static string CategoryLabel(AgentSkillCategory category) =>
        category switch
        {
            AgentSkillCategory.SystemDiagnosis => "\u7cfb\u7edf\u8bca\u65ad\u4e0e\u4f53\u68c0",
            AgentSkillCategory.SystemSettings => "\u7cfb\u7edf\u8bbe\u7f6e\u4e0e\u4f18\u5316",
            AgentSkillCategory.Troubleshooting => "\u6545\u969c\u6392\u67e5\u4e0e\u4fee\u590d",
            AgentSkillCategory.WindowAndDesktop => "\u7a97\u53e3\u4e0e\u684c\u9762\u7ba1\u7406",
            AgentSkillCategory.ProcessAndServiceManagement => "\u8fdb\u7a0b\u4e0e\u670d\u52a1\u7ba1\u7406",
            AgentSkillCategory.HardwareInfo => "\u786c\u4ef6\u4fe1\u606f\u67e5\u8be2",
            AgentSkillCategory.SystemTools => "\u7cfb\u7edf\u5de5\u5177\u76f4\u8fbe",
            AgentSkillCategory.InputAndSession => "\u8f93\u5165\u4e0e\u4f1a\u8bdd\u63a7\u5236",
            _ => "\u5176\u4ed6\u80fd\u529b"
        };

    private static string SkillName(AgentSkillCategory category) =>
        category switch
        {
            AgentSkillCategory.SystemDiagnosis => "\u7535\u8111\u4f53\u68c0",
            AgentSkillCategory.SystemSettings => "\u8bbe\u7f6e\u4f18\u5316\u5efa\u8bae",
            AgentSkillCategory.Troubleshooting => "\u95ee\u9898\u5b9a\u4f4d",
            AgentSkillCategory.WindowAndDesktop => "\u684c\u9762\u548c\u7a97\u53e3\u6574\u7406",
            AgentSkillCategory.ProcessAndServiceManagement => "\u540e\u53f0\u548c\u81ea\u542f\u68c0\u67e5",
            AgentSkillCategory.HardwareInfo => "\u7535\u8111\u914d\u7f6e\u67e5\u8be2",
            AgentSkillCategory.SystemTools => "\u5e38\u7528\u5de5\u5177\u6253\u5f00",
            AgentSkillCategory.InputAndSession => "\u9501\u5c4f/\u4f11\u7720/\u91cd\u542f\u786e\u8ba4",
            _ => "\u80fd\u529b"
        };

    private static string Description(AgentSkillCategory category) =>
        category switch
        {
            AgentSkillCategory.SystemDiagnosis => "\u67e5\u770b\u78c1\u76d8\u7a7a\u95f4\u3001\u5185\u5b58\u3001\u8fdb\u7a0b\u3001\u81ea\u542f\u52a8\u548c\u4f7f\u7528\u4e60\u60ef\uff0c\u5148\u7ed9\u51fa\u7ed3\u8bba\u3002",
            AgentSkillCategory.SystemSettings => "\u628a\u7f51\u7edc\u3001\u58f0\u97f3\u3001\u663e\u793a\u3001\u7535\u6e90\u7b49\u8bbe\u7f6e\u7ffb\u8bd1\u6210\u53ef\u7406\u89e3\u7684\u5efa\u8bae\u3002",
            AgentSkillCategory.Troubleshooting => "\u5e2e\u4f60\u628a\u7f51\u7edc\u3001\u9a71\u52a8\u3001\u5e94\u7528\u95ea\u9000\u3001\u84dd\u5c4f\u3001\u97f3\u9891\u6216\u663e\u793a\u95ee\u9898\u5148\u5206\u7c7b\u3002",
            AgentSkillCategory.WindowAndDesktop => "\u6574\u7406\u684c\u9762\u56fe\u6807\u3001\u7a97\u53e3\u5e03\u5c40\u548c\u591a\u5c4f\u72b6\u6001\uff0c\u5148\u751f\u6210\u65b9\u6848\u3002",
            AgentSkillCategory.ProcessAndServiceManagement => "\u770b\u61c2\u54ea\u4e9b\u7a0b\u5e8f\u5728\u540e\u53f0\u5e38\u9a7b\uff0c\u54ea\u4e9b\u53ef\u80fd\u5f71\u54cd\u5f00\u673a\u901f\u5ea6\u3002",
            AgentSkillCategory.HardwareInfo => "\u67e5\u770b CPU\u3001\u663e\u5361\u3001\u5185\u5b58\u548c Windows \u7248\u672c\uff1b\u5224\u65ad\u5177\u4f53\u8f6f\u4ef6\u6216\u6e38\u620f\u65f6\u8fd8\u9700\u8981\u5b98\u65b9\u914d\u7f6e\u8981\u6c42\u3002",
            AgentSkillCategory.SystemTools => "\u5e2e\u4f60\u627e\u5230\u4efb\u52a1\u7ba1\u7406\u5668\u3001\u78c1\u76d8\u7ba1\u7406\u3001\u8bbe\u5907\u7ba1\u7406\u5668\u7b49\u5de5\u5177\u5165\u53e3\u3002",
            AgentSkillCategory.InputAndSession => "\u5bf9\u9501\u5c4f\u3001\u4f11\u7720\u3001\u5173\u673a\u3001\u91cd\u542f\u8fd9\u7c7b\u5f71\u54cd\u5f53\u524d\u4f1a\u8bdd\u7684\u52a8\u4f5c\u505a\u786e\u8ba4\u524d\u89e3\u91ca\u3002",
            _ => skillDescriptionFallback
        };

    private const string skillDescriptionFallback = "\u89e3\u91ca\u80fd\u529b\u8fb9\u754c\u5e76\u751f\u6210\u5efa\u8bae\u3002";

    private static string ModeLabel(AgentExecutionMode mode) =>
        mode switch
        {
            AgentExecutionMode.ReadOnly => "\u53ea\u8bfb\u8bca\u65ad",
            AgentExecutionMode.ExplainOnly => "\u53ea\u89e3\u91ca",
            AgentExecutionMode.PlanOnly => "\u53ea\u751f\u6210\u65b9\u6848",
            AgentExecutionMode.OpenSystemTool => "\u53ea\u6253\u5f00\u5de5\u5177",
            _ => mode.ToString()
        };

    private static string RiskLabel(RiskLevel risk) =>
        risk switch
        {
            RiskLevel.None => "\u65e0\u98ce\u9669",
            RiskLevel.Low => "\u4f4e\u98ce\u9669",
            RiskLevel.Medium => "\u4e2d\u98ce\u9669\uff0c\u9700\u786e\u8ba4",
            RiskLevel.High => "\u9ad8\u98ce\u9669\uff0c\u5fc5\u987b\u8d70\u5b89\u5168\u7ba1\u7ebf",
            _ => risk.ToString()
        };

    private static string NextStepLabel(AgentSkillCategory category, AgentExecutionMode mode) =>
        category switch
        {
            AgentSkillCategory.SystemDiagnosis => "\u5f00\u59cb\u672c\u5730\u4f53\u68c0",
            AgentSkillCategory.SystemSettings => "\u751f\u6210\u8bbe\u7f6e\u4f18\u5316\u65b9\u6848",
            AgentSkillCategory.Troubleshooting => "\u5148\u5b9a\u4f4d\u539f\u56e0",
            AgentSkillCategory.WindowAndDesktop => "\u751f\u6210\u684c\u9762\u6574\u7406\u65b9\u6848",
            AgentSkillCategory.ProcessAndServiceManagement => "\u5148\u67e5\u770b\u540e\u53f0\u9879",
            AgentSkillCategory.HardwareInfo => "\u67e5\u770b\u7535\u8111\u914d\u7f6e",
            AgentSkillCategory.SystemTools => "\u6253\u5f00\u7cfb\u7edf\u5de5\u5177",
            AgentSkillCategory.InputAndSession => "\u51c6\u5907\u786e\u8ba4\u5165\u53e3",
            _ when mode == AgentExecutionMode.OpenSystemTool => "\u6253\u5f00\u5de5\u5177",
            _ => "\u751f\u6210\u5efa\u8bae"
        };

    private static string SafetyHint(AgentSkillCategory category, AgentExecutionMode mode) =>
        category switch
        {
            AgentSkillCategory.ProcessAndServiceManagement => "\u4e0d\u4f1a\u76f4\u63a5\u7ed3\u675f\u8fdb\u7a0b\u3001\u7981\u7528\u670d\u52a1\u6216\u6539\u81ea\u542f\u52a8\uff1b\u8981\u6267\u884c\u5fc5\u987b\u6709\u8bc1\u636e\u3001\u786e\u8ba4\u548c\u56de\u6eda\u8bb0\u5f55\u3002",
            AgentSkillCategory.SystemTools => "\u53ea\u6253\u5f00\u5de5\u5177\u5165\u53e3\uff0c\u4e0d\u4f1a\u66ff\u4f60\u70b9\u51fb\u5371\u9669\u6309\u94ae\u3002",
            AgentSkillCategory.InputAndSession => "\u4e0d\u4f1a\u76f4\u63a5\u9501\u5c4f\u3001\u4f11\u7720\u3001\u5173\u673a\u6216\u91cd\u542f\uff1b\u53ea\u51c6\u5907\u9700\u8981\u4f60\u786e\u8ba4\u7684\u5165\u53e3\u3002",
            _ when mode == AgentExecutionMode.ReadOnly => "\u53ea\u8bfb\u53d6\u672c\u5730\u6458\u8981\uff0c\u4e0d\u4fee\u6539\u7cfb\u7edf\u3002",
            _ when mode == AgentExecutionMode.OpenSystemTool => "\u53ea\u6253\u5f00\u5de5\u5177\u5165\u53e3\uff0c\u4e0d\u4ee3\u66ff\u4f60\u64cd\u4f5c\u3002",
            _ => "\u53ea\u751f\u6210\u65b9\u6848\uff1b\u771f\u6b63\u52a8\u4f5c\u5fc5\u987b\u8fdb\u5165\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u3002"
        };
}
