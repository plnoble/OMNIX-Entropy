using Css.Core.Apps;
using Css.Core.Migration;
using Css.Ipc.Migration;
using Css.Ipc.Uninstall;
using Css.Win32.Security;

namespace Css.App;

public interface IMigrationProductionExecutionCoordinator
{
    Task<MigrationProductionExecutionOutcome> ExecuteAsync(
        MigrationElevatedRequestDraft request,
        CancellationToken cancellationToken = default);
}

public interface IMigrationProductionLifecycleRunner
{
    Task<MigrationWorkerLifecycleResult> RunAsync(
        MigrationElevatedRequestDraft request,
        CancellationToken cancellationToken = default);
}

public sealed class MigrationProductionExecutionOutcome
{
    public required MigrationExecutionResultViewModel Summary { get; init; }
    public MigrationWorkerLifecycleResult? Lifecycle { get; init; }
    public bool ProductionAttempted { get; init; }
    public bool CompletedProduction { get; init; }
}

public sealed class MigrationProductionExecutionCoordinator
    : IMigrationProductionExecutionCoordinator
{
    private readonly Func<OfficialUninstallWorkerTrustAssessment> _assessTrust;
    private readonly Func<OfficialUninstallWorkerTrustAssessment,
        IMigrationProductionLifecycleRunner> _createRunner;

    public MigrationProductionExecutionCoordinator(
        Func<OfficialUninstallWorkerTrustAssessment> assessTrust,
        Func<OfficialUninstallWorkerTrustAssessment,
            IMigrationProductionLifecycleRunner> createRunner)
    {
        _assessTrust = assessTrust ?? throw new ArgumentNullException(nameof(assessTrust));
        _createRunner = createRunner ?? throw new ArgumentNullException(nameof(createRunner));
    }

    public static MigrationProductionExecutionCoordinator CreateForCurrentPackage() =>
        new(CurrentPackageWorkerTrustProvider.Assess, CreateWindowsRunner);

    public async Task<MigrationProductionExecutionOutcome> ExecuteAsync(
        MigrationElevatedRequestDraft request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.CanSubmit)
        {
            return Outcome(
                Lifecycle(MigrationWorkerLifecycleStatus.InvalidRequest),
                request,
                attempted: false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        OfficialUninstallWorkerTrustAssessment trust;
        try
        {
            trust = _assessTrust();
        }
        catch
        {
            trust = CurrentPackageWorkerTrustProvider.ProbeFailed();
        }

        if (!trust.CanLaunchProduction)
        {
            return new MigrationProductionExecutionOutcome
            {
                Summary = TrustRefusal(trust),
                ProductionAttempted = false
            };
        }

        MigrationWorkerLifecycleResult lifecycle;
        try
        {
            var runner = _createRunner(trust)
                ?? throw new InvalidOperationException("Migration lifecycle runner is unavailable.");
            lifecycle = await runner.RunAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            lifecycle = Lifecycle(MigrationWorkerLifecycleStatus.Canceled);
        }
        catch
        {
            lifecycle = Lifecycle(MigrationWorkerLifecycleStatus.LaunchFailed);
        }

        return Outcome(lifecycle, request, attempted: true);
    }

    private static MigrationProductionExecutionOutcome Outcome(
        MigrationWorkerLifecycleResult lifecycle,
        MigrationElevatedRequestDraft request,
        bool attempted)
    {
        var completedTransport = lifecycle.Status
                == MigrationWorkerLifecycleStatus.CompletedProduction
            && lifecycle.Response is not null;
        var acceptedCompletion = completedTransport
            && request.CanSubmit
            && string.Equals(
                request.RequestId,
                lifecycle.Response!.RequestId,
                StringComparison.Ordinal)
            && lifecycle.Response.Result.Success
            && lifecycle.Response.Result.Payload is MigrationExecutionResult
            {
                Status: MigrationExecutionStatus.Completed
            };
        var summary = completedTransport
                ? MigrationExecutionResultPresenter.Create(request, lifecycle.Response!)
                : LifecycleSummary(lifecycle.Status);
        return new MigrationProductionExecutionOutcome
        {
            Summary = summary,
            Lifecycle = lifecycle,
            ProductionAttempted = attempted,
            CompletedProduction = acceptedCompletion
        };
    }

    private static MigrationExecutionResultViewModel TrustRefusal(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        var detail = trust.Status switch
        {
            OfficialUninstallWorkerTrustStatus.WorkerUnavailable =>
                "OMNIX 没有找到随主程序发布的安全助手。",
            OfficialUninstallWorkerTrustStatus.SignerMismatch =>
                "主程序和安全助手不是由同一个可信发布者签名。",
            OfficialUninstallWorkerTrustStatus.AppNotSigned
                or OfficialUninstallWorkerTrustStatus.WorkerNotSigned =>
                "当前是未签名的开发版本，不能获得真实迁移权限。",
            _ => "Windows 无法确认主程序和安全助手的可信身份。"
        };
        return View(
            MigrationExecutionResultTone.Notice,
            "迁移没有开始",
            "没有改动",
            detail,
            "请使用经过签名的正式版本；不要手动替换安全助手文件。",
            "身份不完整时不会启动提权、移动目录或创建重定向。");
    }

    private static MigrationExecutionResultViewModel LifecycleSummary(
        MigrationWorkerLifecycleStatus status) =>
        status switch
        {
            MigrationWorkerLifecycleStatus.UserCanceledElevation => View(
                MigrationExecutionResultTone.Notice,
                "迁移没有开始",
                "你取消了系统确认",
                "Windows 的管理员确认没有通过，因此没有移动任何目录。",
                "需要迁移时重新生成方案即可。",
                "取消管理员确认不会留下半成品。"),
            MigrationWorkerLifecycleStatus.InvalidRequest => View(
                MigrationExecutionResultTone.Notice,
                "迁移方案需要重新确认",
                "没有改动",
                "当前方案缺少确认、快照或回滚证据。",
                "请返回应用详情重新生成迁移方案。",
                "证据不完整时不会启动安全助手。"),
            _ => View(
                MigrationExecutionResultTone.Warning,
                "迁移没有开始",
                "安全连接失败",
                "OMNIX 无法确认安全助手和本次请求完整对应。",
                "先不要手动移动软件；重新打开 OMNIX 后再试。",
                "连接、身份、响应或子进程状态不明确时不会继续操作。")
        };

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
            CloseButtonText = "我知道了"
        };

    private static MigrationWorkerLifecycleResult Lifecycle(
        MigrationWorkerLifecycleStatus status) =>
        new() { Status = status, ChildExited = false };

    private static IMigrationProductionLifecycleRunner CreateWindowsRunner(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        var launcher = WindowsMigrationProductionWorkerLauncher.Create(trust);
        var lifecycle = new MigrationWorkerLifecycleClient(
            launcher,
            new WindowsOfficialUninstallWorkerImageInspector(),
            new WindowsOfficialUninstallCurrentProcessIdentityProvider(),
            new WindowsOfficialUninstallPipePeerIdentityReader(),
            startupTimeout: TimeSpan.FromSeconds(20),
            bootstrapTimeout: TimeSpan.FromSeconds(15),
            responseTimeout: TimeSpan.FromMinutes(5),
            shutdownTimeout: TimeSpan.FromSeconds(5));
        return new MigrationProductionLifecycleRunner(lifecycle);
    }

}

internal sealed class MigrationProductionLifecycleRunner(
    MigrationWorkerLifecycleClient lifecycle)
    : IMigrationProductionLifecycleRunner
{
    public Task<MigrationWorkerLifecycleResult> RunAsync(
        MigrationElevatedRequestDraft request,
        CancellationToken cancellationToken = default) =>
        lifecycle.RunProductionOnceAsync(request, cancellationToken);
}
