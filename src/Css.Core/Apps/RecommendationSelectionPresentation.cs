using Css.Core.Operations;
using System.Collections.Generic;

namespace Css.Core.Apps;

public sealed class RecommendationSelectionViewModel
{
    public required bool CanContinue { get; init; }
    public required bool CanExecuteDirectly { get; init; }
    public required string ButtonText { get; init; }
    public required string ExplanationText { get; init; }
    public required string AgentTakeaway { get; init; }
    public required string NextStepText { get; init; }
    public required string SafetyBoundary { get; init; }
    public required IReadOnlyList<string> PlanLines { get; init; }
}

public static class RecommendationSelectionPresenter
{
    public static RecommendationSelectionViewModel Create(RecommendationCardViewModel? selected)
    {
        if (selected is null)
        {
            return new RecommendationSelectionViewModel
            {
                CanContinue = false,
                CanExecuteDirectly = false,
                ButtonText = "\u9009\u62e9\u53ef\u6e05\u7406\u9879\u540e\u7ee7\u7eed",
                ExplanationText = "\u5148\u9009\u62e9\u4e00\u5f20\u5efa\u8bae\u5361\u3002\u53ea\u6709\u4f4e\u98ce\u9669\u3001\u6709\u56de\u6eda\u8bc1\u636e\u7684\u6e05\u7406\u9879\u624d\u4f1a\u5f00\u653e\u4e0b\u4e00\u6b65\u3002",
                AgentTakeaway = "Agent \u5224\u65ad\uff1a\u8bf7\u5148\u9009\u62e9\u4e00\u5f20\u5efa\u8bae\u5361\u3002",
                NextStepText = "\u4e0b\u4e00\u6b65\uff1a\u9009\u4e2d\u53ef\u6e05\u7406\u9879\u540e\uff0c\u6211\u4f1a\u5148\u7ed9\u4f60\u770b\u5904\u7406\u9884\u6848\u3002",
                SafetyBoundary = "\u5b89\u5168\u8fb9\u754c\uff1a\u73b0\u5728\u4e0d\u4f1a\u76f4\u63a5\u6267\u884c\u4efb\u4f55\u6e05\u7406\u3002",
                PlanLines =
                [
                    "\u5148\u9009\u4e2d\u4e00\u5f20\u5361\u7247\u3002",
                    "\u4f4e\u98ce\u9669\u9879\u624d\u4f1a\u5f00\u653e\u9694\u79bb\u533a\u9884\u6848\u3002",
                    "\u4e2d\u9ad8\u98ce\u9669\u9879\u53ea\u80fd\u89e3\u91ca\u6216\u89c2\u5bdf\u3002"
                ]
            };
        }

        if (selected is { CanExecute: true, Operation: not null })
            return CreateActionable(selected, selected.Operation);

        return new RecommendationSelectionViewModel
        {
            CanContinue = false,
            CanExecuteDirectly = false,
            ButtonText = "\u5f53\u524d\u5361\u7247\u4e0d\u80fd\u6267\u884c",
            ExplanationText = "\u8fd9\u5f20\u5361\u4e0d\u80fd\u76f4\u63a5\u6267\u884c\uff1a\u5b83\u53ea\u662f\u89c2\u5bdf\u3001\u89e3\u91ca\u6216\u65b9\u6848\u5efa\u8bae\u3002\u9700\u8981\u66f4\u591a\u8bc1\u636e\u65f6\uff0c\u5148\u67e5\u770b\u6280\u672f\u62a5\u544a\u6216\u8ba9 Agent \u89e3\u91ca\u3002",
            AgentTakeaway = "Agent \u5224\u65ad\uff1a\u8fd9\u5f20\u5361\u4e0d\u9002\u5408\u76f4\u63a5\u5904\u7406\u3002",
            NextStepText = "\u4e0b\u4e00\u6b65\uff1a\u5148\u67e5\u6765\u6e90\u3001\u770b\u8be6\u60c5\uff0c\u6216\u8ba9 Agent \u751f\u6210\u89c2\u5bdf\u65b9\u6848\u3002",
            SafetyBoundary = "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u5220\u9664\u3001\u8fc1\u79fb\u6216\u7981\u7528\u4efb\u4f55\u5185\u5bb9\u3002",
            PlanLines =
            [
                "\u6682\u65f6\u4e0d\u8fdb\u5165\u9694\u79bb\u533a\u5904\u7406\u3002",
                "\u5982\u679c\u662f\u4e0d\u660e\u6765\u6e90\uff0c\u5148\u67e5\u8f6f\u4ef6\u5f52\u5c5e\u3002",
                "\u9700\u8981\u5feb\u7167\u6216\u56de\u6eda\u8bc1\u636e\u7684\u9879\u76ee\uff0cV1 \u4e0d\u76f4\u63a5\u52a8\u3002"
            ]
        };
    }

    private static RecommendationSelectionViewModel CreateActionable(
        RecommendationCardViewModel selected,
        OperationDescriptor operation)
    {
        return new RecommendationSelectionViewModel
        {
            CanContinue = true,
            CanExecuteDirectly = false,
            ButtonText = "\u67e5\u770b\u5e76\u786e\u8ba4\u79fb\u52a8\u5230\u9694\u79bb\u533a",
            ExplanationText =
                "\u5df2\u9009\u62e9\uff1a" + selected.Title +
                "\u3002\u4e0b\u4e00\u6b65\u4e0d\u4f1a\u9a6c\u4e0a\u6e05\u7406\uff1b\u70b9\u51fb\u540e\u4f1a\u5148\u8fdb\u5165\u4e8c\u6b21\u786e\u8ba4\uff0c\u5217\u51fa\u8bc1\u636e\u3001\u5f71\u54cd\u8303\u56f4\u3001\u9884\u8ba1\u91ca\u653e " +
                FormatBytes(operation.EstimatedImpactBytes) +
                "\u548c\u9694\u79bb\u533a\u4f4d\u7f6e\u3002\u786e\u8ba4\u540e\u53ea\u4f1a\u79fb\u52a8\u5230 OMNIX-Entropy \u9694\u79bb\u533a\uff0c\u4e0d\u662f\u6c38\u4e45\u5220\u9664\uff1b\u9700\u8981\u65f6\u53ef\u4ee5\u5728\u540e\u6094\u836f\u4e2d\u5fc3\u8fd8\u539f\u3002",
            AgentTakeaway = "Agent \u5224\u65ad\uff1a\u8fd9\u662f\u4f4e\u98ce\u9669\u9879\uff0c\u53ef\u4ee5\u5904\u7406\uff0c\u4f46\u5148\u79fb\u5230\u9694\u79bb\u533a\u3002",
            NextStepText = "\u4e0b\u4e00\u6b65\uff1a\u70b9\u51fb\u6309\u94ae\u540e\u5148\u770b\u4e8c\u6b21\u786e\u8ba4\uff0c\u9884\u8ba1\u91ca\u653e " + FormatBytes(operation.EstimatedImpactBytes) + "\u3002",
            SafetyBoundary = "\u5b89\u5168\u8fb9\u754c\uff1a\u8fd9\u4e0d\u662f\u6c38\u4e45\u5220\u9664\uff1b\u786e\u8ba4\u540e\u53ea\u8fdb\u9694\u79bb\u533a\uff0c\u540e\u6094\u836f\u4e2d\u5fc3\u4f1a\u4fdd\u7559\u8fd8\u539f\u8bb0\u5f55\u3002",
            PlanLines =
            [
                "\u67e5\u770b\u8bc1\u636e\u548c\u5f71\u54cd\u8303\u56f4\uff1a" + operation.AffectedPaths.Count + " \u4e2a\u4f4d\u7f6e\u3002",
                "\u901a\u8fc7\u672c\u5730\u5b89\u5168\u7ba1\u7ebf\u79fb\u5230\u9694\u79bb\u533a\uff0c\u9884\u8ba1\u91ca\u653e " + FormatBytes(operation.EstimatedImpactBytes) + "\u3002",
                "\u540e\u6094\u836f\u4e2d\u5fc3\u4f1a\u8bb0\u5f55 manifest\uff0c\u9700\u8981\u65f6\u53ef\u6309\u8bb0\u5f55\u8fd8\u539f\u3002",
                "\u9ad8\u98ce\u9669\u9879\u4e0d\u4f1a\u5728\u8fd9\u91cc\u81ea\u52a8\u5904\u7406\u3002"
            ]
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? value.ToString("0") + " " + units[unit]
            : value.ToString("0.0") + " " + units[unit];
    }
}
