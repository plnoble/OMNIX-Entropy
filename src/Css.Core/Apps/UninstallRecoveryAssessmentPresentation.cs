using System.Collections.Generic;
using Css.Core.Software;

namespace Css.Core.Apps;

public sealed class UninstallRecoveryAssessmentViewModel
{
    public required string AgentConclusion { get; init; }
    public required string UndoHeadline { get; init; }
    public required IReadOnlyList<string> ProtectionLines { get; init; }
    public required IReadOnlyList<string> SimpleSteps { get; init; }
    public required string NextAction { get; init; }
    public required string SafetyBoundary { get; init; }
    public bool CanUndoOfficialUninstall { get; init; }
    public bool CanRestoreQuarantinedResidue { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class UninstallRecoveryAssessmentPresenter
{
    public static UninstallRecoveryAssessmentViewModel Create(SoftwareProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var hasOfficialUninstaller = !string.IsNullOrWhiteSpace(profile.UninstallCommand);
        var hasRestorableResidue = profile.CachePaths.Count > 0 || profile.LogPaths.Count > 0;

        return new UninstallRecoveryAssessmentViewModel
        {
            AgentConclusion = hasOfficialUninstaller
                ? "Agent 判断：可以准备官方卸载，但现在不会直接执行。"
                : "Agent 判断：暂时不能安全准备卸载，因为没有找到官方卸载入口。",
            UndoHeadline = "先说清楚：软件卸载本身不能靠隔离区一键恢复；后悔时通常需要重新安装。",
            ProtectionLines =
            [
                "个人数据默认不处理；卸载前仍建议备份重要内容。",
                "中/高风险残留只解释，不自动删除服务、启动项或系统配置。",
                hasRestorableResidue
                    ? "低风险缓存和日志只有在你再次确认后才进隔离区，并可在后悔药中心还原。"
                    : "当前没有预先发现可进入隔离区的低风险缓存或日志。"
            ],
            SimpleSteps =
            [
                hasOfficialUninstaller
                    ? "1. 确认官方卸载入口可信，并关闭软件。"
                    : "1. 先从 Windows 设置或软件官网确认官方卸载入口。",
                "2. 卸载完成后重新扫描，确认软件确实已不在。",
                "3. 只处理确认过的低风险残留，其余内容保持原样。"
            ],
            NextAction = hasOfficialUninstaller
                ? "下一步：先备份重要数据，并确认以后能从哪里重新安装这个软件。"
                : "下一步：先找到可信的官方卸载入口；OMNIX-Entropy 不会猜命令。",
            SafetyBoundary = "当前仍是只读方案：没有运行卸载器，没有删除文件，也没有修改服务、启动项或注册表。",
            CanUndoOfficialUninstall = false,
            CanRestoreQuarantinedResidue = hasRestorableResidue,
            CanExecuteDirectly = false
        };
    }
}
