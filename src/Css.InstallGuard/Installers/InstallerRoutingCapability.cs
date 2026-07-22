using Css.Win32.Security;

namespace Css.InstallGuard.Installers;

public enum InstallerRoutingCapabilityMode
{
    AutomaticInteractiveRoute,
    GuidedInteractiveRoute,
    WindowsManagedStorage,
    Refused
}

public sealed class InstallerRoutingCapability
{
    public required InstallerRoutingCapabilityMode Mode { get; init; }
    public required string Title { get; init; }
    public required string AgentConclusion { get; init; }
    public required string NextStep { get; init; }
    public required string SafetyText { get; init; }
    public required string TargetInstallPath { get; init; }
    public string? SettingsShortcutId { get; init; }
    public IReadOnlyList<string> InteractiveArguments { get; init; } = [];
    public bool CanRequestInstallerLaunch { get; init; }
    public bool CanApplyTargetAutomatically { get; init; }
    public bool RequiresBeforeSnapshot { get; init; } = true;
    public bool RequiresFinalConfirmation { get; init; } = true;
}

public static class InstallerRoutingCapabilityPolicy
{
    public const string WindowsManagedStorageShortcutId = "default-save-locations";

    public static InstallerRoutingCapability Evaluate(
        InstallerDetectionResult analysis,
        InstallerPackageEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(evidence);
        var target = analysis.RecommendedRoute.TargetInstallPath;

        if (!evidence.HasStableIdentity
            || !PathsEqual(analysis.InstallerPath, evidence.PackagePath))
        {
            return Refused(
                target,
                "还不能安全打开这个安装包",
                "文件不存在、读取失败，或分析结果与当前选中的文件不一致。",
                "请重新选择安装包并再次分析。");
        }

        if (evidence.SignatureStatus != AuthenticodeSignatureStatus.Trusted)
        {
            var reason = evidence.SignatureStatus switch
            {
                AuthenticodeSignatureStatus.NotSigned => "安装包没有可验证的发布者签名。",
                AuthenticodeSignatureStatus.Invalid => "安装包签名无效，文件可能已损坏或被修改。",
                AuthenticodeSignatureStatus.Untrusted => "Windows 不能信任这个安装包的发布者证书。",
                _ => "暂时无法确认安装包发布者。"
            };
            return Refused(
                target,
                "OMNIX 不建议替你打开",
                reason,
                "请从软件官网重新下载安装包，确认发布者后再试。");
        }

        if (evidence.DetectedKind == InstallerKind.Unknown)
        {
            return Refused(
                target,
                "还不能确认安装器类型",
                "当前证据不足以判断怎样安全打开这个安装包。",
                "请重新选择官网提供的 EXE、MSI 或 MSIX 安装包。");
        }

        if (evidence.DetectedKind is InstallerKind.InnoSetup or InstallerKind.Nsis
            && evidence.KindConfidence == InstallerKindConfidence.High)
        {
            var arguments = evidence.DetectedKind == InstallerKind.InnoSetup
                ? new[] { $"/DIR={target}" }
                : new[] { $"/D={target}" };
            return new InstallerRoutingCapability
            {
                Mode = InstallerRoutingCapabilityMode.AutomaticInteractiveRoute,
                Title = "可以帮你预填 D 盘位置",
                AgentConclusion = "安装器类型已经确认，OMNIX 可以在打开安装界面时预填推荐目录。",
                NextStep = "确认后仍会显示安装器界面，请检查位置后再点击安装。",
                SafetyText = "不会使用静默安装参数；不会替你点击安装，也不会修改 Windows 全局安装目录。",
                TargetInstallPath = target,
                InteractiveArguments = arguments,
                CanRequestInstallerLaunch = true,
                CanApplyTargetAutomatically = true
            };
        }

        if (evidence.DetectedKind == InstallerKind.Msix)
        {
            return new InstallerRoutingCapability
            {
                Mode = InstallerRoutingCapabilityMode.WindowsManagedStorage,
                Title = "安装位置由 Windows 管理",
                AgentConclusion = "这是 Windows 应用包，不能可靠地指定到任意 D 盘文件夹。",
                NextStep = "可以先打开 Windows 的“新应用保存位置”，选择以后新应用默认保存到哪个盘。",
                SafetyText = "这里只打开设置，不会更改保存盘、运行安装包、伪造目录参数或修改 ProgramFilesDir。",
                TargetInstallPath = target,
                SettingsShortcutId = WindowsManagedStorageShortcutId,
                CanRequestInstallerLaunch = false,
                CanApplyTargetAutomatically = false
            };
        }

        var kindLabel = evidence.DetectedKind switch
        {
            InstallerKind.Msi => "MSI",
            InstallerKind.Burn => "WiX Burn",
            _ => "普通 EXE"
        };
        return new InstallerRoutingCapability
        {
            Mode = InstallerRoutingCapabilityMode.GuidedInteractiveRoute,
            Title = "需要在安装界面里选择位置",
            AgentConclusion = $"这是 {kindLabel} 安装包，但没有发现可以安全套用的目录参数。",
            NextStep = "OMNIX 可以打开安装界面并把推荐位置展示在旁边；看到“安装位置”时请选择该目录。",
            SafetyText = "不会尝试未知参数，不会静默安装，也不会把“打开安装器”说成“安装成功”。",
            TargetInstallPath = target,
            CanRequestInstallerLaunch = true,
            CanApplyTargetAutomatically = false
        };
    }

    private static InstallerRoutingCapability Refused(
        string target,
        string title,
        string conclusion,
        string nextStep) =>
        new()
        {
            Mode = InstallerRoutingCapabilityMode.Refused,
            Title = title,
            AgentConclusion = conclusion,
            NextStep = nextStep,
            SafetyText = "证据不完整或发布者不可信时，OMNIX 不会运行安装包。",
            TargetInstallPath = target,
            CanRequestInstallerLaunch = false,
            CanApplyTargetAutomatically = false
        };

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
