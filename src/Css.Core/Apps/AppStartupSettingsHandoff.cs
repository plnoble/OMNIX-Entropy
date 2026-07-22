using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class AppStartupSettingsHandoff
{
    public required string Summary { get; init; }
    public required string AgentTakeaway { get; init; }
    public required string NextStepText { get; init; }
    public required string SafetyText { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
    public bool CanOpenStartupSettings { get; init; }
    public string? SettingsShortcutId { get; init; }
}

public static class AppStartupSettingsHandoffPresenter
{
    public const string StartupSettingsShortcutId = "startup-apps";

    public static AppStartupSettingsHandoff Create(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!AppPresentationBuilder.CanUseOrdinaryApplicationActions(profile))
        {
            return Refused(
                profile.Category == SoftwareCategory.SystemTool
                    ? "这是系统相关应用，建议保留当前启动行为。"
                    : "这个应用的系统归属待确认，当前只能查看启动线索。",
                profile.Category == SoftwareCategory.SystemTool
                    ? "系统、驱动、安全和输入设备相关后台能力不会交给快捷操作处理。"
                    : "确认组件归属前，不生成本地审核或 Windows 设置交接。");
        }

        if (profile.StartupEntries.Count == 0)
        {
            var hasOtherBackgroundComponents = profile.Services.Count > 0
                || profile.ScheduledTasks.Count > 0;
            return Refused(
                hasOtherBackgroundComponents
                    ? "发现了后台服务或计划任务，但没有确认到普通启动应用。"
                    : "暂未发现可以在 Windows“启动应用”页面管理的项目。",
                hasOtherBackgroundComponents
                    ? "服务和计划任务需要各自的身份、当前状态和回滚证据，当前只观察。"
                    : "可以先重新扫描应用；没有证据时不提供关闭入口。");
        }

        var excludedComponents = profile.Services.Count + profile.ScheduledTasks.Count;
        return new AppStartupSettingsHandoff
        {
            Summary = $"发现 {profile.StartupEntries.Count} 个普通自启动项，可以到 Windows 官方页面查看。",
            AgentTakeaway = "Agent 判断：可以检查是否需要开机启动，但由你在 Windows 页面中做最后选择。",
            NextStepText = "下一步：确认后打开“启动应用”；先看名称和影响，再决定是否关闭。",
            SafetyText = "OMNIX-Entropy 只打开设置页，不会替你切换开关，也不会修改注册表、服务或计划任务。",
            Lines = excludedComponents > 0
                ?
                [
                    $"普通自启动项：{profile.StartupEntries.Count} 个，可在 Windows 页面查看。",
                    $"另有 {excludedComponents} 个服务/计划任务，本次不处理。",
                    "当前是否开启：请在 Windows 官方页面确认，OMNIX 不根据内部字节猜测。",
                    "关闭后若功能异常，可以回到同一 Windows 页面重新开启。"
                ]
                :
                [
                    $"普通自启动项：{profile.StartupEntries.Count} 个，可在 Windows 页面查看。",
                    "当前是否开启：请在 Windows 官方页面确认，OMNIX 不根据内部字节猜测。",
                    "打开页面本身不会改变任何设置。",
                    "关闭后若功能异常，可以回到同一 Windows 页面重新开启。"
                ],
            CanOpenStartupSettings = true,
            SettingsShortcutId = StartupSettingsShortcutId
        };
    }

    private static AppStartupSettingsHandoff Refused(string summary, string nextStep) =>
        new()
        {
            Summary = summary,
            AgentTakeaway = "Agent 判断：当前不适合给出关闭按钮，先保留或观察。",
            NextStepText = nextStep,
            SafetyText = "没有生成修改操作，也不会打开服务、任务计划或注册表编辑入口。",
            Lines = ["证据不足或风险偏高时，OMNIX-Entropy 会选择停止。"],
            CanOpenStartupSettings = false
        };
}
