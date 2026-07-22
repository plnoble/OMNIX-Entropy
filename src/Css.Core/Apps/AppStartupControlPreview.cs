using System.Collections.Generic;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class AppStartupControlPreviewViewModel
{
    public required string Summary { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public required bool CanExecuteDirectly { get; init; }
}

public static class AppStartupControlPreviewPresenter
{
    public static AppStartupControlPreviewViewModel Create(SoftwareProfile profile)
    {
        var decision = DetermineDecision(profile);
        var parts = BuildEvidenceParts(profile);

        if (parts.Count == 0)
        {
            return new AppStartupControlPreviewViewModel
            {
                Summary = "\u6682\u672a\u53d1\u73b0\u660e\u786e\u7684\u81ea\u542f\u52a8\u3001\u540e\u53f0\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u3002",
                Lines =
                [
                    "\u6682\u65f6\u4e0d\u751f\u6210\u5173\u95ed\u65b9\u6848\uff1a\u6ca1\u6709\u8db3\u591f\u8bc1\u636e\u8bf4\u660e\u5b83\u4f1a\u5f00\u673a\u5e38\u9a7b\u3002",
                    "\u5982\u679c\u4f60\u89c9\u5f97\u5b83\u5f71\u54cd\u5f00\u673a\u6216\u5360\u7528\u8d44\u6e90\uff0c\u53ef\u4ee5\u91cd\u65b0\u626b\u63cf\u5e94\u7528\u548c\u8fdb\u7a0b\u3002",
                    "\u8fd9\u91cc\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\u542f\u52a8\u9879\u3001\u670d\u52a1\u6216\u8ba1\u5212\u4efb\u52a1\u3002"
                ],
                CanExecuteDirectly = false
            };
        }

        return new AppStartupControlPreviewViewModel
        {
            Summary = BuildSummary(decision, parts),
            Lines = BuildLines(decision),
            CanExecuteDirectly = false
        };
    }

    private enum StartupPlanDecision
    {
        Keep,
        Observe,
        FutureDisable
    }

    private static StartupPlanDecision DetermineDecision(SoftwareProfile profile)
    {
        if (profile.Category == SoftwareCategory.SystemTool)
            return StartupPlanDecision.Keep;

        if (profile.StartupEntries.Count > 0 || profile.Services.Count > 0 || profile.ScheduledTasks.Count > 0)
            return StartupPlanDecision.FutureDisable;

        return StartupPlanDecision.Observe;
    }

    private static List<string> BuildEvidenceParts(SoftwareProfile profile)
    {
        var parts = new List<string>();
        if (profile.RunningProcesses.Count > 0)
            parts.Add(profile.RunningProcesses.Count + " \u4e2a\u6b63\u5728\u8fd0\u884c\u7684\u8fdb\u7a0b");
        if (profile.StartupEntries.Count > 0)
            parts.Add(profile.StartupEntries.Count + " \u4e2a\u81ea\u542f\u52a8\u9879");
        if (profile.Services.Count > 0)
            parts.Add(profile.Services.Count + " \u4e2a\u540e\u53f0\u670d\u52a1");
        if (profile.ScheduledTasks.Count > 0)
            parts.Add(profile.ScheduledTasks.Count + " \u4e2a\u8ba1\u5212\u4efb\u52a1");

        return parts;
    }

    private static string BuildSummary(StartupPlanDecision decision, IReadOnlyList<string> parts)
    {
        var evidence = string.Join("\u3001", parts);
        return decision switch
        {
            StartupPlanDecision.Keep =>
                "\u5efa\u8bae\u4fdd\u7559\uff1a\u53d1\u73b0\u540e\u53f0\u5e38\u9a7b\u8bc1\u636e\uff08" + evidence + "\uff09\uff0c\u7cfb\u7edf\u6216\u9a71\u52a8\u76f8\u5173\u9879\u4e0d\u5efa\u8bae\u76f4\u63a5\u52a8\u3002",
            StartupPlanDecision.Observe =>
                "\u5148\u89c2\u5bdf\uff1a\u53d1\u73b0\u540e\u53f0\u6d3b\u52a8\uff08" + evidence + "\uff09\uff0c\u4f46\u8fd8\u6ca1\u6709\u8db3\u591f\u8bc1\u636e\u8bf4\u5b83\u9700\u8981\u7981\u7528\u3002",
            _ =>
                "\u672a\u6765\u53ef\u7981\u7528\u5019\u9009\uff1a\u53d1\u73b0\u53ef\u68c0\u67e5\u7684\u5f00\u673a\u5e38\u9a7b\u7ec4\u4ef6\uff08" + evidence + "\uff09\uff0c\u4f46\u73b0\u5728\u53ea\u751f\u6210\u65b9\u6848\u3002"
        };
    }

    private static IReadOnlyList<string> BuildLines(StartupPlanDecision decision)
    {
        return decision switch
        {
            StartupPlanDecision.Keep =>
            [
                "\u7ed3\u8bba\uff1a\u5efa\u8bae\u4fdd\u7559\u3002\u7cfb\u7edf\u3001\u9a71\u52a8\u3001\u5b89\u5168\u548c\u8f93\u5165\u8bbe\u5907\u76f8\u5173\u540e\u53f0\u80fd\u529b\u9ed8\u8ba4\u4e0d\u52a8\u3002",
                "\u53ea\u751f\u6210\u65b9\u6848\uff1aAgent \u53ea\u89e3\u91ca\u8fd9\u4e2a\u540e\u53f0\u4e3a\u4ec0\u4e48\u4e0d\u5efa\u8bae\u5904\u7406\u3002",
                "\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\uff1a\u6ca1\u6709\u5feb\u7167\u3001\u56de\u6eda\u548c\u4f60\u7684\u786e\u8ba4\u65f6\uff0c\u4e0d\u4f1a\u52a8\u670d\u52a1\u3001\u81ea\u542f\u52a8\u6216\u8ba1\u5212\u4efb\u52a1\u3002",
                "\u53ef\u4ee5\u540e\u6094\uff1a\u771f\u6b63\u5904\u7406\u65f6\u5fc5\u987b\u8fdb\u5165\u540e\u6094\u836f\u4e2d\u5fc3\u65f6\u95f4\u7ebf\uff0c\u5e76\u6807\u51fa\u8fd8\u539f\u65b9\u5f0f\u3002",
                "\u6280\u672f\u540d\u79f0\u4e0d\u5728\u4e3b\u754c\u9762\u5806\u51fa\u6765\uff1a\u9700\u8981\u65f6\u518d\u70b9\u51fb\u201c\u6280\u672f\u8be6\u60c5\u201d\u3002"
            ],
            StartupPlanDecision.Observe =>
            [
                "\u7ed3\u8bba\uff1a\u5148\u89c2\u5bdf\u3002\u6b63\u5728\u8fd0\u884c\u4e0d\u4ee3\u8868\u53ef\u4ee5\u7981\u7528\uff0c\u4e5f\u4e0d\u4ee3\u8868\u5b83\u4f1a\u5f00\u673a\u81ea\u542f\u52a8\u3002",
                "\u53ea\u751f\u6210\u65b9\u6848\uff1a\u5efa\u8bae\u7ee7\u7eed\u89c2\u5bdf\uff0c\u6216\u91cd\u65b0\u626b\u63cf\u5e94\u7528\u3001\u81ea\u542f\u52a8\u548c\u8fdb\u7a0b\u540e\u518d\u5224\u65ad\u3002",
                "\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\uff1a\u53ea\u6709\u8bc1\u636e\u8bf4\u660e\u5b83\u4f1a\u5f00\u673a\u5e38\u9a7b\uff0c\u624d\u4f1a\u751f\u6210\u7981\u7528\u5019\u9009\u65b9\u6848\u3002",
                "\u53ef\u4ee5\u540e\u6094\uff1a\u672a\u6765\u771f\u8981\u5904\u7406\u65f6\uff0c\u4ecd\u9700\u5feb\u7167\u3001\u56de\u6eda\u548c\u540e\u6094\u836f\u4e2d\u5fc3\u8bb0\u5f55\u3002",
                "\u6280\u672f\u540d\u79f0\u4e0d\u5728\u4e3b\u754c\u9762\u5806\u51fa\u6765\uff1a\u9700\u8981\u65f6\u518d\u70b9\u51fb\u201c\u6280\u672f\u8be6\u60c5\u201d\u3002"
            ],
            _ =>
            [
                "\u7ed3\u8bba\uff1a\u672a\u6765\u53ef\u7981\u7528\u5019\u9009\u3002\u73b0\u5728\u53ea\u751f\u6210\u65b9\u6848\uff0c\u4e0d\u76f4\u63a5\u6539\u7cfb\u7edf\u3002",
                "\u5148\u5224\u65ad\u662f\u5426\u5fc5\u8981\uff1a\u540c\u6b65\u3001\u66f4\u65b0\u3001\u8f93\u5165\u3001\u5b89\u5168\u548c\u9a71\u52a8\u7c7b\u540e\u53f0\u80fd\u529b\u8981\u66f4\u4fdd\u5b88\u3002",
                "\u4e0d\u4f1a\u76f4\u63a5\u7981\u7528\uff1a\u81ea\u542f\u52a8\u9879\u3001\u670d\u52a1\u548c\u8ba1\u5212\u4efb\u52a1\u90fd\u9700\u8981\u8bc1\u636e\u3001\u5feb\u7167\u548c\u4f60\u7684\u786e\u8ba4\u3002",
                "\u9700\u8981\u56de\u6eda\uff1a\u771f\u6b63\u5904\u7406\u65f6\u5fc5\u987b\u8fdb\u5165\u540e\u6094\u836f\u4e2d\u5fc3\u65f6\u95f4\u7ebf\uff0c\u5e76\u6807\u51fa\u8fd8\u539f\u65b9\u5f0f\u3002",
                "\u6280\u672f\u540d\u79f0\u4e0d\u5728\u4e3b\u754c\u9762\u5806\u51fa\u6765\uff1a\u9700\u8981\u65f6\u518d\u70b9\u51fb\u201c\u6280\u672f\u8be6\u60c5\u201d\u3002"
            ]
        };
    }
}
