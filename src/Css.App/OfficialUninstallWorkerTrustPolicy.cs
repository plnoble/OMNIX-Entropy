using Css.Win32.Security;

namespace Css.App;

public enum OfficialUninstallWorkerTrustStatus
{
    TrustedForProduction,
    WorkerUnavailable,
    AppNotSigned,
    WorkerNotSigned,
    AppUntrusted,
    WorkerUntrusted,
    SignerMismatch,
    ProbeFailed
}

public sealed class OfficialUninstallWorkerTrustAssessment
{
    public required OfficialUninstallWorkerTrustStatus Status { get; init; }
    public required AuthenticodeSignatureEvidence AppEvidence { get; init; }
    public required AuthenticodeSignatureEvidence WorkerEvidence { get; init; }
    public string? WorkerExecutablePath { get; init; }
    public bool CanLaunchProduction =>
        Status == OfficialUninstallWorkerTrustStatus.TrustedForProduction;
    public bool CanLaunchDevelopmentVerification =>
        !string.IsNullOrWhiteSpace(WorkerExecutablePath)
        && (CanLaunchProduction
            || (AppEvidence.Status == AuthenticodeSignatureStatus.NotSigned
                && WorkerEvidence.Status == AuthenticodeSignatureStatus.NotSigned
                && HasHash(AppEvidence.FileSha256)
                && HasHash(WorkerEvidence.FileSha256)));

    private static bool HasHash(string? value) =>
        value is { Length: 64 } && value.All(Uri.IsHexDigit);
}

public static class OfficialUninstallWorkerTrustPolicy
{
    public static OfficialUninstallWorkerTrustAssessment Evaluate(
        string applicationExecutablePath,
        OfficialUninstallWorkerAvailability availability,
        IAuthenticodeSignatureVerifier verifier)
    {
        ArgumentNullException.ThrowIfNull(availability);
        ArgumentNullException.ThrowIfNull(verifier);
        var empty = Evidence(AuthenticodeSignatureStatus.Missing);
        if (!availability.CanLaunchVerification
            || string.IsNullOrWhiteSpace(availability.ExecutablePath))
        {
            return Assessment(
                OfficialUninstallWorkerTrustStatus.WorkerUnavailable,
                empty,
                empty);
        }

        AuthenticodeSignatureEvidence app;
        AuthenticodeSignatureEvidence worker;
        try
        {
            app = verifier.Verify(applicationExecutablePath);
            worker = verifier.Verify(availability.ExecutablePath);
        }
        catch
        {
            return Assessment(
                OfficialUninstallWorkerTrustStatus.ProbeFailed,
                empty,
                empty,
                availability.ExecutablePath);
        }

        var status = DetermineStatus(app, worker);
        return Assessment(status, app, worker, availability.ExecutablePath);
    }

    private static OfficialUninstallWorkerTrustStatus DetermineStatus(
        AuthenticodeSignatureEvidence app,
        AuthenticodeSignatureEvidence worker)
    {
        if (app.Status == AuthenticodeSignatureStatus.ProbeFailed
            || worker.Status == AuthenticodeSignatureStatus.ProbeFailed)
            return OfficialUninstallWorkerTrustStatus.ProbeFailed;
        if (app.Status == AuthenticodeSignatureStatus.NotSigned)
            return OfficialUninstallWorkerTrustStatus.AppNotSigned;
        if (worker.Status == AuthenticodeSignatureStatus.NotSigned)
            return OfficialUninstallWorkerTrustStatus.WorkerNotSigned;
        if (!app.IsTrusted)
            return OfficialUninstallWorkerTrustStatus.AppUntrusted;
        if (!worker.IsTrusted)
            return OfficialUninstallWorkerTrustStatus.WorkerUntrusted;
        if (!string.Equals(
                app.SignerThumbprint,
                worker.SignerThumbprint,
                StringComparison.OrdinalIgnoreCase))
            return OfficialUninstallWorkerTrustStatus.SignerMismatch;
        return OfficialUninstallWorkerTrustStatus.TrustedForProduction;
    }

    private static OfficialUninstallWorkerTrustAssessment Assessment(
        OfficialUninstallWorkerTrustStatus status,
        AuthenticodeSignatureEvidence app,
        AuthenticodeSignatureEvidence worker,
        string? workerPath = null) =>
        new()
        {
            Status = status,
            AppEvidence = app,
            WorkerEvidence = worker,
            WorkerExecutablePath = workerPath
        };

    private static AuthenticodeSignatureEvidence Evidence(
        AuthenticodeSignatureStatus status) =>
        new() { Status = status };
}

public static class OfficialUninstallWorkerTrustPresenter
{
    public static OfficialUninstallWorkerResultViewModel Create(
        OfficialUninstallWorkerTrustAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        if (assessment.CanLaunchProduction)
        {
            return View(
                OfficialUninstallWorkerResultTone.Normal,
                "安全组件身份已确认",
                "可以继续确认",
                "OMNIX 主程序和安全助手来自同一个可信发布者。",
                "真正执行前仍会重新核对文件，并再次显示操作影响。",
                "身份检查通过不代表已经执行；当前没有修改电脑。" );
        }

        if (assessment.CanLaunchDevelopmentVerification)
        {
            return View(
                OfficialUninstallWorkerResultTone.Notice,
                "当前是开发验证版本",
                "仅允许测试",
                "这个本地版本尚未签名，只能验证安全连接和自动关闭机制。",
                "正式卸载必须使用经过签名并验证为同一发布者的发布版本。",
                "开发验证不会获得真实卸载、清理或系统修改权限。" );
        }

        var (title, conclusion) = assessment.Status switch
        {
            OfficialUninstallWorkerTrustStatus.WorkerUnavailable =>
                ("安全组件不完整", "OMNIX 找不到随主程序发布的安全助手。"),
            OfficialUninstallWorkerTrustStatus.SignerMismatch =>
                ("安全组件来源不一致", "主程序和安全助手不是由同一个可信身份签名。"),
            OfficialUninstallWorkerTrustStatus.AppNotSigned
                or OfficialUninstallWorkerTrustStatus.WorkerNotSigned =>
                ("这个版本不能正式执行", "主程序或安全助手缺少正式发布所需的可信签名。"),
            _ => ("安全组件验证失败", "Windows 无法确认主程序和安全助手的可信身份。")
        };
        return View(
            OfficialUninstallWorkerResultTone.Warning,
            title,
            "已停止",
            conclusion,
            "请使用 OMNIX 的正式发布版本，不要自行下载或替换安全助手文件。",
            "安全身份不完整或不一致时，不会启动真实卸载和系统修改。" );
    }

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
