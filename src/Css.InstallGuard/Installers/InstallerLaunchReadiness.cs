namespace Css.InstallGuard.Installers;

public enum InstallerLaunchReadinessState
{
    Ready,
    UnsupportedPlatform,
    DisabledByOperator
}

public sealed record InstallerLaunchReadiness
{
    public required InstallerLaunchReadinessState State { get; init; }
    public required string StatusText { get; init; }
    public bool IsAvailable => State == InstallerLaunchReadinessState.Ready;
}

public enum InstallerLaunchPreparationState
{
    Ready,
    RuntimeUnavailable,
    WindowsManagedStorageHandoff,
    PackageCapabilityUnavailable,
    TargetUnavailable
}

public sealed record InstallerLaunchPreparationReadiness
{
    public required InstallerLaunchPreparationState State { get; init; }
    public required string StatusText { get; init; }
    public bool CanPrepare => State == InstallerLaunchPreparationState.Ready;
}

public static class InstallerLaunchReadinessPolicy
{
    public const string DisableEnvironmentVariable =
        "OMNIX_ENTROPY_DISABLE_INSTALLER_LAUNCH";

    public static InstallerLaunchReadiness Evaluate(
        bool isWindows,
        string? disableOverride)
    {
        if (!isWindows)
        {
            return new InstallerLaunchReadiness
            {
                State = InstallerLaunchReadinessState.UnsupportedPlatform,
                StatusText = "安装启动只在 Windows 上可用，当前不会运行安装包。"
            };
        }

        if (IsEnabledOverride(disableOverride))
        {
            return new InstallerLaunchReadiness
            {
                State = InstallerLaunchReadinessState.DisabledByOperator,
                StatusText = "安装启动已由安全开关暂停；分析和路径建议仍可使用。"
            };
        }

        return new InstallerLaunchReadiness
        {
            State = InstallerLaunchReadinessState.Ready,
            StatusText = "已通过安全验收；准备安装时会先做快照和最后确认。"
        };
    }

    private static bool IsEnabledOverride(string? value) =>
        value is not null
        && (value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Equals("on", StringComparison.OrdinalIgnoreCase));
}

public static class InstallerLaunchPreparationPolicy
{
    public static InstallerLaunchPreparationReadiness Evaluate(
        InstallerLaunchReadiness launchReadiness,
        InstallerRoutingCapability capability,
        IInstallerTargetPathPolicy targetPathPolicy)
    {
        ArgumentNullException.ThrowIfNull(launchReadiness);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(targetPathPolicy);

        if (capability.Mode == InstallerRoutingCapabilityMode.WindowsManagedStorage
            && string.Equals(
                capability.SettingsShortcutId,
                InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId,
                StringComparison.Ordinal))
        {
            return new InstallerLaunchPreparationReadiness
            {
                State = InstallerLaunchPreparationState.WindowsManagedStorageHandoff,
                StatusText = "Windows 管理这类应用的位置；可以先打开“新应用保存位置”，OMNIX 不会运行安装包。"
            };
        }

        if (!launchReadiness.IsAvailable)
        {
            return new InstallerLaunchPreparationReadiness
            {
                State = InstallerLaunchPreparationState.RuntimeUnavailable,
                StatusText = launchReadiness.StatusText
            };
        }

        if (!capability.CanRequestInstallerLaunch)
        {
            return new InstallerLaunchPreparationReadiness
            {
                State = InstallerLaunchPreparationState.PackageCapabilityUnavailable,
                StatusText = "当前证据不足，OMNIX 不会替你打开这个安装包。"
            };
        }

        if (!targetPathPolicy.IsAllowed(capability.TargetInstallPath, out _))
        {
            return new InstallerLaunchPreparationReadiness
            {
                State = InstallerLaunchPreparationState.TargetUnavailable,
                StatusText = "推荐的非系统盘位置当前不可用；请检查 D 盘后重新分析。"
            };
        }

        return new InstallerLaunchPreparationReadiness
        {
            State = InstallerLaunchPreparationState.Ready,
            StatusText = launchReadiness.StatusText
        };
    }
}
