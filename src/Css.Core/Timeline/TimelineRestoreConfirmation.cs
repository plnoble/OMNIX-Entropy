namespace Css.Core.Timeline;

public sealed class TimelineRestoreConfirmationViewModel
{
    public required string Title { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public required string SafetyText { get; init; }
    public required string ConfirmButtonText { get; init; }
}

public static class TimelineRestoreConfirmationPresenter
{
    private const string StartupRestoreKind = "startup.restore.hkcu.run";

    public static TimelineRestoreConfirmationViewModel Create(ActionTimelineItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!item.CanRestore || item.RestoreManifestPaths.Count == 0)
            throw new InvalidOperationException("A restorable timeline item is required.");

        if (item.RestoreOperationKind?.Equals(
                StartupRestoreKind,
                StringComparison.OrdinalIgnoreCase) == true)
        {
            return new TimelineRestoreConfirmationViewModel
            {
                Title = "确认恢复自启动",
                Headline = item.Title,
                Summary = "恢复后，这个应用可以在下次登录时通过原来的普通入口自动启动。",
                SafetyText = "不会立即启动软件，不会修改服务或计划任务；如果同名入口已存在或权限变化，会拒绝覆盖。",
                ConfirmButtonText = "确认恢复自启动"
            };
        }

        return new TimelineRestoreConfirmationViewModel
        {
            Title = "确认还原隔离内容",
            Headline = item.Title,
            Summary = "OMNIX 会按记录把隔离内容放回原位置。",
            SafetyText = "如果原位置已经有内容，会拒绝覆盖；不会永久删除任何文件。",
            ConfirmButtonText = "确认还原"
        };
    }
}
