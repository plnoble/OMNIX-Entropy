using Css.Core.Migration;
using Css.Core.Operations;

namespace Css.Core.Apps;

public enum MigrationExecutionResultTone
{
    Normal,
    Notice,
    Warning
}

public sealed class MigrationExecutionResultViewModel
{
    public required MigrationExecutionResultTone Tone { get; init; }
    public required string Title { get; init; }
    public required string StatusLabel { get; init; }
    public required string Conclusion { get; init; }
    public required string AgentAdvice { get; init; }
    public required string SafetyText { get; init; }
    public required string CloseButtonText { get; init; }
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

public static class MigrationExecutionResultPresenter
{
    public static MigrationExecutionResultViewModel Create(
        MigrationElevatedRequestDraft request,
        MigrationElevatedResponseEnvelope response)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);
        return request.CanSubmit
            && string.Equals(request.RequestId, response.RequestId, StringComparison.Ordinal)
                ? Create(response.Result)
                : Create(OperationResult.Fail("Migration response correlation failed."));
    }

    public static MigrationExecutionResultViewModel Create(OperationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Payload is not MigrationExecutionResult migration)
        {
            return View(
                MigrationExecutionResultTone.Warning,
                "无法确认迁移结果",
                "已停止",
                "返回的信息不完整，OMNIX 不会把它当成迁移成功。",
                "先不要打开相关软件；重新扫描后再生成方案。",
                "结果不明确时不会继续移动、删除或修改其他内容。");
        }

        return migration.Status switch
        {
            MigrationExecutionStatus.Completed => View(
                MigrationExecutionResultTone.Normal,
                "迁移完成，开始观察 C 盘",
                "已完成",
                $"已迁移 {migration.MovedPathCount} 个目录，原位置现在会把访问转到 D 盘。",
                "你可以正常打开软件。OMNIX 会继续观察原位置，发现软件重新写回 C 盘时提醒你。",
                "回滚清单和监控记录已保留；本页不会继续执行其他操作。"),
            MigrationExecutionStatus.Refused => View(
                MigrationExecutionResultTone.Notice,
                "迁移没有开始",
                "没有改动",
                "安全检查没有全部通过，因此没有移动任何软件目录。",
                "按迁移方案中的待办逐项处理，再重新生成一次确认清单。",
                "没有通过证据、空间、活动组件和路径检查时，不会开始迁移。"),
            MigrationExecutionStatus.FailedRolledBack => View(
                MigrationExecutionResultTone.Warning,
                "迁移失败，已还原",
                "已回滚",
                "迁移中途没有完成，已把尝试移动的目录恢复到原位置。",
                "先重新扫描软件是否正常；确认无误前不要重复迁移。",
                "没有把失败说成成功，迁移监控也不会登记为已完成。"),
            MigrationExecutionStatus.FailedRollbackIncomplete => View(
                MigrationExecutionResultTone.Warning,
                "迁移未完成，需要人工检查",
                "回滚不完整",
                "迁移失败后仍有目录没有完成恢复。OMNIX 已停止继续处理。",
                "暂时不要打开或重装这个软件，请保留现有文件并使用技术详情进行恢复检查。",
                "不会自动删除任何剩余副本，也不会继续创建重定向。"),
            _ => View(
                MigrationExecutionResultTone.Warning,
                "无法确认迁移结果",
                "已停止",
                "当前结果不在可识别范围内。",
                "重新扫描后再生成迁移方案。",
                "OMNIX 不会依据未知结果继续操作。")
        };
    }

    private static MigrationExecutionResultViewModel View(
        MigrationExecutionResultTone tone,
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
            CloseButtonText = "返回并重新检查"
        };
}
