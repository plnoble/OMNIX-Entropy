using System.Collections.Generic;
using System.Linq;
using Css.Core.Operations;

namespace Css.Core.Agent;

public sealed class SystemToolShortcut
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Command { get; init; }
    public string? Arguments { get; init; }
    public required RiskLevel Risk { get; init; }
    public required bool RequiresConfirmation { get; init; }
    public bool IsOpenOnly { get; init; } = true;
    public required string SafetyHint { get; init; }
}

public static class SystemToolShortcutCatalog
{
    public const string RecycleBinId = "recycle-bin";

    private static readonly IReadOnlyList<SystemToolShortcut> DefaultShortcuts =
    [
        new()
        {
            Id = "task-manager",
            Name = "\u4efb\u52a1\u7ba1\u7406\u5668",
            Description = "\u67e5\u770b\u6b63\u5728\u8fd0\u884c\u7684\u7a0b\u5e8f\u3001CPU\u3001\u5185\u5b58\u548c\u542f\u52a8\u9879\u3002",
            Command = "taskmgr.exe",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00\u4efb\u52a1\u7ba1\u7406\u5668\uff1bOMNIX-Entropy \u4e0d\u4f1a\u66ff\u4f60\u7ed3\u675f\u8fdb\u7a0b\u3002"
        },
        new()
        {
            Id = RecycleBinId,
            Name = "回收站",
            Description = "查看当前回收站中的文件，确认有没有需要恢复的内容。",
            Command = "explorer.exe",
            Arguments = "shell:RecycleBinFolder",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "只打开回收站供你查看；OMNIX-Entropy 不会清空、删除或还原任何文件。"
        },
        new()
        {
            Id = "device-manager",
            Name = "\u8bbe\u5907\u7ba1\u7406\u5668",
            Description = "\u67e5\u770b\u9a71\u52a8\u548c\u786c\u4ef6\u72b6\u6001\uff0c\u9002\u5408\u6392\u67e5\u5916\u8bbe\u6216\u9a71\u52a8\u95ee\u9898\u3002",
            Command = "devmgmt.msc",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "\u53ea\u6253\u5f00\u8bbe\u5907\u7ba1\u7406\u5668\uff1b\u4e0d\u4f1a\u66ff\u4f60\u5378\u8f7d\u9a71\u52a8\u6216\u7981\u7528\u8bbe\u5907\u3002"
        },
        new()
        {
            Id = "disk-management",
            Name = "\u78c1\u76d8\u7ba1\u7406",
            Description = "\u67e5\u770b\u5206\u533a\u3001\u76d8\u7b26\u548c\u78c1\u76d8\u72b6\u6001\uff0c\u9002\u5408\u786e\u8ba4 C/D \u76d8\u5bb9\u91cf\u3002",
            Command = "diskmgmt.msc",
            Risk = RiskLevel.Medium,
            RequiresConfirmation = true,
            SafetyHint = "\u53ea\u6253\u5f00\u78c1\u76d8\u7ba1\u7406\uff1b\u4e0d\u4f1a\u66ff\u4f60\u521b\u5efa\u3001\u5220\u9664\u6216\u683c\u5f0f\u5316\u5206\u533a\u3002"
        },
        new()
        {
            Id = "event-viewer",
            Name = "\u4e8b\u4ef6\u67e5\u770b\u5668",
            Description = "\u67e5\u770b Windows \u9519\u8bef\u548c\u5d29\u6e83\u8bb0\u5f55\uff0c\u9002\u5408\u6545\u969c\u6392\u67e5\u3002",
            Command = "eventvwr.msc",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00\u4e8b\u4ef6\u67e5\u770b\u5668\uff1b\u4e0d\u4f1a\u5220\u9664\u65e5\u5fd7\u6216\u4fee\u6539\u7cfb\u7edf\u3002"
        },
        new()
        {
            Id = "windows-security",
            Name = "Windows \u5b89\u5168\u4e2d\u5fc3",
            Description = "\u6253\u5f00\u9632\u75c5\u6bd2\u3001\u9632\u706b\u5899\u548c\u8bbe\u5907\u5b89\u5168\u72b6\u6001\u5165\u53e3\u3002",
            Command = "windowsdefender:",
            Risk = RiskLevel.Low,
            RequiresConfirmation = false,
            SafetyHint = "\u53ea\u6253\u5f00 Windows \u5b89\u5168\u4e2d\u5fc3\uff1b\u4e0d\u4f1a\u66ff\u4f60\u5173\u95ed\u4efb\u4f55\u9632\u62a4\u3002"
        },
        new()
        {
            Id = "registry-editor",
            Name = "\u6ce8\u518c\u8868\u7f16\u8f91\u5668",
            Description = "\u9ad8\u98ce\u9669\u5de5\u5177\uff0c\u53ea\u5728\u9700\u8981\u67e5\u770b\u6216\u5bfc\u51fa\u8bc1\u636e\u65f6\u6253\u5f00\u3002",
            Command = "regedit.exe",
            Risk = RiskLevel.High,
            RequiresConfirmation = true,
            SafetyHint = "\u9700\u8981\u4f60\u786e\u8ba4\u540e\u624d\u6253\u5f00\uff1bOMNIX-Entropy \u4e0d\u4f1a\u66ff\u4f60\u4fee\u6539\u6ce8\u518c\u8868\u3002"
        }
    ];

    public static IReadOnlyList<SystemToolShortcut> CreateDefault() => DefaultShortcuts;

    public static SystemToolShortcut? FindById(string id) =>
        DefaultShortcuts.FirstOrDefault(shortcut => shortcut.Id == id);
}
