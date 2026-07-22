using Css.Core.Uninstall;
using Css.Ipc.Uninstall;

namespace Css.App;

public enum OfficialUninstallWorkerResultTone
{
    Normal,
    Notice,
    Warning
}

public sealed class OfficialUninstallWorkerResultViewModel
{
    public required OfficialUninstallWorkerResultTone Tone { get; init; }
    public required string Title { get; init; }
    public required string StatusLabel { get; init; }
    public required string Conclusion { get; init; }
    public required string AgentAdvice { get; init; }
    public required string SafetyText { get; init; }
    public required string CloseButtonText { get; init; }
    public string ReturnToApplicationButtonText => "返回并重新检查";
    public bool CanExecuteDirectly => false;

    public string VisibleText => string.Join(
        Environment.NewLine,
        Title,
        StatusLabel,
        Conclusion,
        AgentAdvice,
        SafetyText,
        CloseButtonText);
}

public static class OfficialUninstallWorkerResultPresenter
{
    public static OfficialUninstallWorkerResultViewModel CreateUnknownAttempt() =>
        View(
            OfficialUninstallWorkerResultTone.Warning,
            "\u5378\u8f7d\u7ed3\u679c\u6ca1\u6709\u5b8c\u6574\u786e\u8ba4",
            "\u9700\u8981\u91cd\u65b0\u626b\u63cf",
            "\u8bf7\u6c42\u5df2\u7ecf\u4ea4\u7ed9\u5b89\u5168\u52a9\u624b\uff0c\u4f46\u8fd4\u56de\u7ed3\u679c\u4e0d\u5b8c\u6574\uff0c\u6211\u4e0d\u4f1a\u628a\u5b83\u8bf4\u6210\u5378\u8f7d\u6210\u529f\u3002",
            "\u5173\u95ed\u8fd9\u4e2a\u7a97\u53e3\u540e\uff0c\u6211\u4f1a\u91cd\u65b0\u626b\u63cf\u5e94\u7528\uff1b\u5728\u7ed3\u679c\u51fa\u6765\u524d\uff0c\u4e0d\u8981\u624b\u52a8\u5220\u9664\u8f6f\u4ef6\u76ee\u5f55\u3002",
            "\u5f53\u524d\u65b9\u6848\u5df2\u9501\u5b9a\uff0c\u4e0d\u4f1a\u81ea\u52a8\u91cd\u8bd5\u5378\u8f7d\u6216\u6e05\u7406\u6b8b\u7559\u3002");

    public static OfficialUninstallWorkerResultViewModel Create(
        OfficialUninstallWorkerLifecycleResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Status switch
        {
            OfficialUninstallWorkerLifecycleStatus.CompletedFake => View(
                OfficialUninstallWorkerResultTone.Normal,
                "安全连接测试已完成",
                "测试通过",
                "OMNIX 已确认安全助手的身份，并完成了一次不修改电脑的连接测试。",
                "现在可以确认通信和自动关闭机制工作正常。真正卸载软件之前，仍需要单独启用并再次确认。",
                "本次没有运行卸载器，也没有删除、迁移或修改任何系统内容。"),
            OfficialUninstallWorkerLifecycleStatus.CompletedProduction =>
                ProductionCompleted(result),
            OfficialUninstallWorkerLifecycleStatus.ProductionLauncherRejected => SecurityStopped(
                "正式执行身份没有通过",
                "当前启动器没有携带可信发布版本的完整证明，OMNIX 已拒绝进入正式卸载。"),
            OfficialUninstallWorkerLifecycleStatus.UserCanceledElevation => View(
                OfficialUninstallWorkerResultTone.Notice,
                "你取消了 Windows 确认",
                "没有开始",
                "安全助手没有获得管理员权限，因此这次操作没有开始。",
                "这是安全的取消，不需要修复。需要时可以重新生成方案并再次确认。",
                "电脑没有因为这次取消发生改变。"),
            OfficialUninstallWorkerLifecycleStatus.WorkerImageRejected => SecurityStopped(
                "安全助手文件不一致",
                "Windows 启动程序后，OMNIX 复查到的文件与刚才确认的安全助手不一致，因此已经停止。"),
            OfficialUninstallWorkerLifecycleStatus.PeerRejected => SecurityStopped(
                "安全助手身份不一致",
                "OMNIX 发现连接到的程序不是刚刚启动的那个安全助手，因此已经停止。"),
            OfficialUninstallWorkerLifecycleStatus.BootstrapFailed => SecurityStopped(
                "安全验证没有完成",
                "OMNIX 无法建立受保护的连接，已经关闭本次操作。"),
            OfficialUninstallWorkerLifecycleStatus.ResponseTimedOut => View(
                OfficialUninstallWorkerResultTone.Warning,
                "等待安全助手超时",
                "已停止",
                "安全助手没有在规定时间内返回结果，OMNIX 已结束等待并尝试关闭它。",
                "先不要连续重试。关闭 OMNIX 后重新打开，再检查一次；如果仍出现，请保留当前记录。",
                "OMNIX 不会把超时说成成功，也不会继续执行后续处理。"),
            OfficialUninstallWorkerLifecycleStatus.WorkerExitFailed => View(
                OfficialUninstallWorkerResultTone.Warning,
                "安全助手没有正常关闭",
                "需要留意",
                "OMNIX 已停止继续操作，但无法确认安全助手已经完全退出。",
                "请先关闭 OMNIX 并重新打开，暂时不要再次执行同一个操作。",
                "本页不会继续运行卸载、清理或系统修改。"),
            OfficialUninstallWorkerLifecycleStatus.InvalidRequest => SecurityStopped(
                "安全清单已经失效",
                "当前确认信息不完整或已经变化，OMNIX 拒绝继续。"),
            OfficialUninstallWorkerLifecycleStatus.Canceled => View(
                OfficialUninstallWorkerResultTone.Notice,
                "操作已取消",
                "没有继续",
                "本次请求在安全助手完成前被取消。",
                "需要时请回到软件页面重新检查，再生成新的处理方案。",
                "取消后不会自动继续执行。"),
            _ => View(
                OfficialUninstallWorkerResultTone.Warning,
                "安全助手没有启动",
                "没有开始",
                "OMNIX 无法启动或联系安全助手，因此已经停止。",
                "重新打开 OMNIX 后再试一次；如果仍失败，请不要自行寻找或替换程序文件。",
                "没有确认安全连接时，OMNIX 不会运行卸载器。")
        };
    }

    private static OfficialUninstallWorkerResultViewModel ProductionCompleted(
        OfficialUninstallWorkerLifecycleResult result)
    {
        if (result.Response?.Result.Payload is not OfficialUninstallHandlerPayload payload)
        {
            return SecurityStopped(
                "无法确认卸载结果",
                "安全助手返回的内容不完整，OMNIX 不会把它当成卸载成功。");
        }

        if (!payload.UninstallerStarted)
        {
            return View(
                OfficialUninstallWorkerResultTone.Warning,
                "官方卸载器没有启动",
                "没有开始",
                "本次没有开始卸载，软件仍按原状保留。",
                "请重新检查软件状态，再生成一份新的处理方案。",
                "没有启动官方卸载器时，OMNIX 不会继续处理残留。");
        }

        if (!payload.UninstallerCompleted)
        {
            return View(
                OfficialUninstallWorkerResultTone.Warning,
                "官方卸载没有完成",
                "需要检查",
                "卸载器已经结束，但目前不能确认软件已经移除。",
                "先让 Agent 重新扫描，不要手动删除软件文件。",
                "OMNIX 不会在结果不确定时自动清理残留。");
        }

        if (!payload.PostScan.Success)
        {
            return View(
                OfficialUninstallWorkerResultTone.Warning,
                "卸载已结束，复查未完成",
                "需要重新扫描",
                "官方卸载器已经结束，但卸载后的只读复查没有得到完整结果。",
                "请重新扫描一次；在复查成功前，不处理任何残留。",
                "本次没有自动删除或移动卸载后的内容。");
        }

        if (payload.PostScan.SoftwareStillPresent)
        {
            return View(
                OfficialUninstallWorkerResultTone.Warning,
                "软件可能仍然存在",
                "需要检查",
                "卸载器已经结束，但复查仍能发现这个软件。",
                "建议先重新启动电脑并再次扫描，不要直接删除安装目录。",
                "OMNIX 已保留现场，没有自动处理残留。");
        }

        var residueCount = payload.PostScan.ResidueCandidateCount;
        return residueCount > 0
            ? View(
                OfficialUninstallWorkerResultTone.Notice,
                "卸载完成，发现待检查内容",
                "已完成卸载",
                $"软件已经移除，复查发现 {residueCount} 项可能的残留。",
                "Agent 会先按风险分组；只有低风险内容才能在你确认后移入隔离区。",
                "目前所有残留都还在原位，没有自动删除。")
            : View(
                OfficialUninstallWorkerResultTone.Normal,
                "卸载和复查已完成",
                "处理完成",
                "软件已经移除，复查没有发现需要处理的残留。",
                "这次不需要继续操作；后悔药中心仍会保留本次记录。",
                "OMNIX 没有执行额外的残留删除或系统修改。");
    }

    private static OfficialUninstallWorkerResultViewModel SecurityStopped(
        string title,
        string conclusion) =>
        View(
            OfficialUninstallWorkerResultTone.Warning,
            title,
            "安全停止",
            conclusion,
            "这不是需要你判断的技术问题。OMNIX 已替你停止，重新打开软件后再检查即可。",
            "身份或安全验证不通过时，不会运行卸载器，也不会修改电脑。" );

    private static OfficialUninstallWorkerResultViewModel View(
        OfficialUninstallWorkerResultTone tone,
        string title,
        string status,
        string conclusion,
        string advice,
        string safety) =>
        new()
        {
            Tone = tone,
            Title = title,
            StatusLabel = status,
            Conclusion = conclusion,
            AgentAdvice = advice,
            SafetyText = safety,
            CloseButtonText = "我知道了"
        };
}
