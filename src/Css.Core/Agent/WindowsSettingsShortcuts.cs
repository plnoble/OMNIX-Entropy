using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Agent;

public sealed class WindowsSettingsShortcut
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Uri { get; init; }
    public required RiskLevel Risk { get; init; }
    public required bool RequiresConfirmation { get; init; }
    public bool IsOpenOnly { get; init; } = true;
    public required string SafetyHint { get; init; }
}

public static class WindowsSettingsShortcutCatalog
{
    private static readonly IReadOnlyList<WindowsSettingsShortcut> DefaultShortcuts =
    [
        new()
        {
            Id = "storage",
            Name = "\u5b58\u50a8",
            Description = "\u6253\u5f00 Windows \u5b58\u50a8\u8bbe\u7f6e\uff0c\u7528\u6765\u67e5\u770b\u7a7a\u95f4\u5360\u7528\u548c\u5b58\u50a8\u611f\u77e5\u5165\u53e3\u3002",
            Uri = "ms-settings:storagesense",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "\u53ea\u6253\u5f00\u5b58\u50a8\u8bbe\u7f6e\uff1b\u4e0d\u4f1a\u66ff\u4f60\u5220\u9664\u6587\u4ef6\u6216\u5f00\u542f\u81ea\u52a8\u6e05\u7406\u3002"
        },
        new()
        {
            Id = "installed-apps",
            Name = "\u5df2\u5b89\u88c5\u5e94\u7528",
            Description = "\u6253\u5f00 Windows \u5e94\u7528\u7ba1\u7406\u5165\u53e3\uff0c\u7528\u6765\u67e5\u770b\u7cfb\u7edf\u8ba4\u4e3a\u5df2\u5b89\u88c5\u7684\u5e94\u7528\u3002",
            Uri = "ms-settings:appsfeatures",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "\u53ea\u6253\u5f00\u5df2\u5b89\u88c5\u5e94\u7528\u9875\uff1bOMNIX-Entropy \u4e0d\u4f1a\u66ff\u4f60\u5378\u8f7d\u8f6f\u4ef6\u3002"
        },
        new()
        {
            Id = "default-save-locations",
            Name = "新应用保存位置",
            Description = "打开 Windows 的默认保存位置设置，用来选择以后新应用默认保存到哪个盘。",
            Uri = "ms-settings:savelocations",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "只打开新应用保存位置设置；OMNIX-Entropy 不会替你安装应用、更改保存盘或把应用放进任意文件夹。"
        },
        new()
        {
            Id = "startup-apps",
            Name = "启动应用",
            Description = "打开 Windows 启动应用设置，用来查看哪些普通应用会在登录时启动。",
            Uri = "ms-settings:startupapps",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "只打开启动应用设置；OMNIX-Entropy 不会替你切换开关，也不会修改服务或计划任务。"
        },
        new()
        {
            Id = "power",
            Name = "\u7535\u6e90 / \u7761\u7720",
            Description = "\u6253\u5f00\u7535\u6e90\u548c\u7761\u7720\u8bbe\u7f6e\uff0c\u7528\u6765\u67e5\u770b\u8017\u7535\u548c\u5f85\u673a\u884c\u4e3a\u3002",
            Uri = "ms-settings:powersleep",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "\u53ea\u6253\u5f00\u7535\u6e90\u8bbe\u7f6e\uff1b\u4e0d\u4f1a\u66ff\u4f60\u4fee\u6539\u7761\u7720\u3001\u4f11\u7720\u6216\u7535\u6e90\u8ba1\u5212\u3002"
        },
        new()
        {
            Id = "network",
            Name = "\u7f51\u7edc / Wi-Fi",
            Description = "\u6253\u5f00 Windows \u7f51\u7edc\u8bbe\u7f6e\uff0c\u7528\u6765\u67e5\u770b Wi-Fi\u3001\u4ee5\u592a\u7f51\u548c\u7f51\u7edc\u72b6\u6001\u3002",
            Uri = "ms-settings:network",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00\u7f51\u7edc\u8bbe\u7f6e\uff1bOMNIX-Entropy \u4e0d\u4f1a\u66ff\u4f60\u5207\u6362\u7f51\u7edc\u6216\u4fee\u6539\u914d\u7f6e\u3002"
        },
        new()
        {
            Id = "bluetooth",
            Name = "\u84dd\u7259 / \u8bbe\u5907",
            Description = "\u6253\u5f00\u84dd\u7259\u548c\u5916\u8bbe\u5165\u53e3\uff0c\u9002\u5408\u67e5\u770b\u952e\u76d8\u3001\u9f20\u6807\u3001\u8033\u673a\u7b49\u72b6\u6001\u3002",
            Uri = "ms-settings:bluetooth",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00\u84dd\u7259\u8bbe\u7f6e\uff1b\u4e0d\u4f1a\u66ff\u4f60\u914d\u5bf9\u3001\u5220\u9664\u6216\u5173\u95ed\u8bbe\u5907\u3002"
        },
        new()
        {
            Id = "sound",
            Name = "\u58f0\u97f3",
            Description = "\u6253\u5f00\u58f0\u97f3\u8bbe\u7f6e\uff0c\u7528\u6765\u68c0\u67e5\u8f93\u5165\u3001\u8f93\u51fa\u8bbe\u5907\u548c\u97f3\u91cf\u3002",
            Uri = "ms-settings:sound",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00\u58f0\u97f3\u8bbe\u7f6e\uff1b\u4e0d\u4f1a\u66ff\u4f60\u66f4\u6539\u9ea6\u514b\u98ce\u6216\u626c\u58f0\u5668\u3002"
        },
        new()
        {
            Id = "display",
            Name = "\u663e\u793a",
            Description = "\u6253\u5f00\u663e\u793a\u8bbe\u7f6e\uff0c\u7528\u6765\u67e5\u770b\u5206\u8fa8\u7387\u3001\u7f29\u653e\u548c\u591a\u5c4f\u5e03\u5c40\u3002",
            Uri = "ms-settings:display",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00\u663e\u793a\u8bbe\u7f6e\uff1b\u4e0d\u4f1a\u66ff\u4f60\u6539\u5206\u8fa8\u7387\u6216\u591a\u5c4f\u5e03\u5c40\u3002"
        }
    ];

    public static IReadOnlyList<WindowsSettingsShortcut> CreateDefault() => DefaultShortcuts;

    public static WindowsSettingsShortcut? FindById(string id) =>
        DefaultShortcuts.FirstOrDefault(shortcut => shortcut.Id == id);
}
