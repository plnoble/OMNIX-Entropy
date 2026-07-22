using Css.Core.Migration;

namespace Css.Core.Apps;

public sealed class MigrationFinalConsentViewModel
{
    public required string Title { get; init; }
    public required string SoftwareName { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> ImpactLines { get; init; }
    public required string SafetyText { get; init; }
    public required string ConfirmationText { get; init; }
}

public static class MigrationFinalConsentPresenter
{
    public static MigrationFinalConsentViewModel Create(
        MigrationExecutionGateResult gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        var operation = gate.Operation;
        if (!gate.CanRequestExecution
            || gate.BlockingReasons.Count > 0
            || operation is null
            || string.IsNullOrWhiteSpace(operation.ConfirmationText))
        {
            throw new InvalidOperationException(
                "Migration final consent requires a complete preflight operation.");
        }

        var softwareName = operation.Title.EndsWith(" migration", StringComparison.Ordinal)
            ? operation.Title[..^" migration".Length]
            : operation.Title;
        var impact = new List<string>
        {
            $"将处理 {operation.AffectedPaths.Count} 个已经确认的目录，并把访问转到建议的 D 盘位置。",
            "迁移前快照、回滚清单和文件完整性校验都必须再次通过。",
            "相关窗口、后台进程、服务或计划任务仍在活动时，迁移会拒绝开始。",
            "迁移完成后会继续观察原 C 盘位置，发现重新写入时提醒你。"
        };
        if (operation.EstimatedImpactBytes > 0)
        {
            impact.Insert(
                1,
                "预计处理约 " + FormatBytes(operation.EstimatedImpactBytes) + " 数据。");
        }

        return new MigrationFinalConsentViewModel
        {
            Title = "迁移前最后确认",
            SoftwareName = softwareName,
            Summary = "请只确认你已经看懂的内容。任何一项不确定，都可以取消并让 Agent 重新解释。",
            ImpactLines = impact,
            SafetyText = "确认后仍会重新校验身份、快照、路径和活动组件；检查不通过就不会移动目录。",
            ConfirmationText = operation.ConfirmationText
        };
    }

    private static string FormatBytes(long bytes)
    {
        var value = (double)Math.Max(0, bytes);
        var units = new[] { "B", "KB", "MB", "GB", "TB" };
        var index = 0;
        while (value >= 1024 && index < units.Length - 1)
        {
            value /= 1024;
            index++;
        }
        return value.ToString(value >= 100 || index == 0 ? "0" : "0.0") + " " + units[index];
    }
}
