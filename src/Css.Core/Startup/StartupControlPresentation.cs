using Css.Core.Operations;

namespace Css.Core.Startup;

public sealed class StartupControlConfirmationViewModel
{
    public required string Title { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<string> OutcomeLines { get; init; }
    public required string SafetyText { get; init; }
    public required string FirstAcknowledgementText { get; init; }
    public required string SecondAcknowledgementText { get; init; }
    public required string ConfirmButtonText { get; init; }
    public required IReadOnlyList<string> TechnicalDetails { get; init; }
}

public static class StartupControlConfirmationPresenter
{
    public static StartupControlConfirmationViewModel Create(
        StartupControlPreparation preparation,
        OperationDescriptor operation)
    {
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(operation);
        if (!preparation.CanContinue)
            throw new InvalidOperationException("A ready startup preparation is required.");

        var validation = StartupEntryControlOperationPolicy.ValidateCandidate(operation);
        if (!validation.Success)
            throw new InvalidOperationException(validation.Error);

        return new StartupControlConfirmationViewModel
        {
            Title = "确认关闭自启动",
            Headline = $"{preparation.SoftwareName} 下次登录时不再自动启动",
            Summary = "OMNIX 只会关闭刚刚核对过的 1 个普通自启动入口。",
            OutcomeLines =
            [
                "不会关闭现在正在运行的软件。",
                "不会修改服务、计划任务或其他应用的自启动。",
                "会保存原始设置；完成后可在后悔药中心还原。"
            ],
            SafetyText = "执行前还会重新核对原始值和权限；任何一项变化都会停止。",
            FirstAcknowledgementText = "我知道这里只关闭 1 个普通自启动入口，不会退出或卸载软件。",
            SecondAcknowledgementText = "我知道应用更新或再次设置后，可能重新创建这个自启动入口。",
            ConfirmButtonText = "确认关闭自启动",
            TechnicalDetails =
            [
                $"风险等级：{operation.Risk}",
                $"快照：{operation.SnapshotId}",
                $"影响注册表：{operation.AffectedRegistryKeys.Single()}",
                "不处理：服务、计划任务、正在运行的进程、StartupApproved 状态"
            ]
        };
    }
}
