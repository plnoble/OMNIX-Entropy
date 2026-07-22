using Css.Win32.Security;

namespace Css.App;

public enum ProductionExecutionCapability
{
    OfficialUninstall,
    Migration
}

public sealed class ProductionExecutionReadinessViewModel
{
    public required string Title { get; init; }
    public required string StatusLabel { get; init; }
    public required string Conclusion { get; init; }
    public required string NextStep { get; init; }
    public required string SafetyText { get; init; }
    public bool CanPrepareExecution { get; init; }
    public string VisibleText => string.Join(
        "\n",
        Title,
        StatusLabel,
        Conclusion,
        NextStep,
        SafetyText);
}

public static class ProductionExecutionReadinessPresenter
{
    public static ProductionExecutionReadinessViewModel Create(
        OfficialUninstallWorkerTrustAssessment trust,
        ProductionExecutionCapability capability)
    {
        ArgumentNullException.ThrowIfNull(trust);
        var action = capability == ProductionExecutionCapability.Migration
            ? "迁移"
            : "卸载";

        if (trust.CanLaunchProduction)
        {
            return View(
                $"正式{action}已准备",
                "身份已确认",
                $"当前版本具备正式{action}所需的可信安全组件。",
                "仍需完成恢复证据、最终确认和 Windows 管理员确认。",
                "现在没有修改电脑；执行前还会重新核对签名、文件身份和本次请求。",
                canPrepareExecution: true);
        }

        if (trust.CanLaunchDevelopmentVerification)
        {
            return View(
                "当前只能预览方案",
                "开发验证版",
                $"这个版本没有正式签名，不能获得真实{action}权限。",
                "你仍可以查看 Agent 的判断；正式执行需要同一可信发布者签名的发布版本。",
                "不会生成最终执行证据、不会进入最终确认，也不会请求管理员确认。",
                canPrepareExecution: false);
        }

        var conclusion = trust.Status switch
        {
            OfficialUninstallWorkerTrustStatus.WorkerUnavailable =>
                "OMNIX 找不到随主程序发布的安全组件。",
            OfficialUninstallWorkerTrustStatus.SignerMismatch =>
                "主程序和安全组件不是由同一个可信发布者签名。",
            OfficialUninstallWorkerTrustStatus.AppNotSigned
                or OfficialUninstallWorkerTrustStatus.WorkerNotSigned =>
                "主程序或安全组件缺少正式执行所需的可信签名。",
            _ => "Windows 无法确认主程序和安全组件的可信身份。"
        };
        return View(
            "安全组件未准备好",
            "已停止",
            conclusion,
            "请使用完整的 OMNIX 正式发布版本，不要自行替换安全组件。",
            "身份不完整或不一致时，不会生成最终执行证据或请求管理员确认。",
            canPrepareExecution: false);
    }

    public static ProductionExecutionReadinessViewModel Unavailable(
        ProductionExecutionCapability capability) =>
        Create(
            CurrentPackageWorkerTrustProvider.ProbeFailed(),
            capability);

    private static ProductionExecutionReadinessViewModel View(
        string title,
        string status,
        string conclusion,
        string nextStep,
        string safety,
        bool canPrepareExecution) =>
        new()
        {
            Title = title,
            StatusLabel = status,
            Conclusion = conclusion,
            NextStep = nextStep,
            SafetyText = safety,
            CanPrepareExecution = canPrepareExecution
        };
}

public static class CurrentPackageWorkerTrustProvider
{
    public static OfficialUninstallWorkerTrustAssessment Assess()
    {
        try
        {
            var availability = OfficialUninstallWorkerPathResolver.Resolve(
                AppContext.BaseDirectory);
            return OfficialUninstallWorkerTrustPolicy.Evaluate(
                Environment.ProcessPath ?? string.Empty,
                availability,
                new WindowsAuthenticodeSignatureVerifier());
        }
        catch
        {
            return ProbeFailed();
        }
    }

    public static OfficialUninstallWorkerTrustAssessment ProbeFailed() =>
        new()
        {
            Status = OfficialUninstallWorkerTrustStatus.ProbeFailed,
            AppEvidence = new AuthenticodeSignatureEvidence
            {
                Status = AuthenticodeSignatureStatus.ProbeFailed
            },
            WorkerEvidence = new AuthenticodeSignatureEvidence
            {
                Status = AuthenticodeSignatureStatus.ProbeFailed
            }
        };
}
