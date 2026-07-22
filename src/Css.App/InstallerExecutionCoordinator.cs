using Css.Core.Operations;
using Css.Core.Software;
using Css.InstallGuard.Installers;

namespace Css.App;

public enum InstallerExecutionStatus
{
    InitialPostScanCompleted,
    LaunchRefused,
    InstallerWaitInterrupted,
    PostScanFailed
}

public sealed record InstallerExecutionResult
{
    public required InstallerExecutionStatus Status { get; init; }
    public InstallSystemSnapshot? AfterSnapshot { get; init; }
    public InstallSnapshotDiffReport? Report { get; init; }
    public int? InstallerExitCode { get; init; }
    public string? Error { get; init; }
}

public sealed record InstallerExecutionResultViewModel
{
    public required string Title { get; init; }
    public required string StatusLabel { get; init; }
    public required string Conclusion { get; init; }
    public required string AgentAdvice { get; init; }
    public required IReadOnlyList<string> NextSteps { get; init; }
    public required string SafetyText { get; init; }
    public required string CloseButtonText { get; init; }
    public string PostScanRetryButtonText { get; init; } = "我已完成安装，重新扫描";
    public bool CanRequestPostScanRetry { get; init; }
    public bool CanExecuteDirectly { get; init; }
}

public static class InstallerExecutionResultPresenter
{
    public static InstallerExecutionResultViewModel Create(InstallerExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Status == InstallerExecutionStatus.InitialPostScanCompleted
            && result.Report is not null)
        {
            var agent = InstallSnapshotDiffAgentPresenter.Create(result.Report);
            return new InstallerExecutionResultViewModel
            {
                Title = "安装后的初步检查",
                StatusLabel = "已完成初步扫描",
                Conclusion = result.Report.Summary,
                AgentAdvice = agent.Headline + " " + agent.WhatThisMeans,
                NextSteps = agent.NextSteps,
                SafetyText = "启动器退出码不会被当作安装成功；如果安装器仍有子窗口或后台步骤，请关闭后回到安装管控页重新扫描。",
                CloseButtonText = "我知道了",
                CanExecuteDirectly = false
            };
        }

        var (status, conclusion, advice, nextStep) = result.Status switch
        {
            InstallerExecutionStatus.LaunchRefused => (
                "没有打开安装器",
                "安全检查没有通过，OMNIX 没有启动安装包。",
                "请重新选择官网下载的安装包并再次分析，不要绕过签名或文件变化提示。",
                "重新分析安装包"),
            InstallerExecutionStatus.InstallerWaitInterrupted => (
                "安装器状态未确认",
                "安装界面可能仍在运行，当前不能生成可靠的安装后报告。",
                "先完成或关闭安装界面，再回到安装管控页捕获安装后快照。",
                "安装完成后重新扫描"),
            _ => (
                "安装后扫描未完成",
                "安装界面已经关闭，但 OMNIX 没有拿到完整的安装后清单。",
                "不要根据这个结果判断安装成功或失败；请在安装管控页重新捕获安装后快照。",
                "重新捕获安装后快照")
        };
        return new InstallerExecutionResultViewModel
        {
            Title = "安装状态",
            StatusLabel = status,
            Conclusion = conclusion,
            AgentAdvice = advice,
            NextSteps = [nextStep],
            SafetyText = "OMNIX 没有自动清理、迁移、卸载或修改这次安装产生的内容。",
            CloseButtonText = "关闭",
            CanRequestPostScanRetry = result.Status is
                InstallerExecutionStatus.InstallerWaitInterrupted
                or InstallerExecutionStatus.PostScanFailed,
            CanExecuteDirectly = false
        };
    }
}

public sealed class InstallerPostScanCoordinator
{
    private readonly Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> _scanSoftware;
    private readonly Func<CancellationToken, Task<InstallFootprintCapture>> _scanCDriveFootprint;
    private readonly Func<DateTimeOffset> _now;

    public InstallerPostScanCoordinator(
        Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> scanSoftware,
        Func<CancellationToken, Task<InstallFootprintCapture>> scanCDriveFootprint,
        Func<DateTimeOffset>? now = null)
    {
        _scanSoftware = scanSoftware ?? throw new ArgumentNullException(nameof(scanSoftware));
        _scanCDriveFootprint = scanCDriveFootprint
            ?? throw new ArgumentNullException(nameof(scanCDriveFootprint));
        _now = now ?? (() => DateTimeOffset.UtcNow);
    }

    public static InstallerPostScanCoordinator CreateProduction(
        Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> scanSoftware) =>
        new(
            scanSoftware,
            cancellationToken => Task.Run(
                () => new WindowsInstallFootprintProbe().Capture(),
                cancellationToken));

    public async Task<InstallerExecutionResult> CaptureAsync(
        InstallSystemSnapshot beforeSnapshot,
        int? installerExitCode = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(beforeSnapshot);
        try
        {
            var afterProfiles = await _scanSoftware(cancellationToken);
            var afterFootprint = await _scanCDriveFootprint(cancellationToken);
            var after = new InstallSystemSnapshot(_now(), afterProfiles, afterFootprint);
            var report = InstallSnapshotDiffBuilder.Build(beforeSnapshot, after);
            return new InstallerExecutionResult
            {
                Status = InstallerExecutionStatus.InitialPostScanCompleted,
                AfterSnapshot = after,
                Report = report,
                InstallerExitCode = installerExitCode
            };
        }
        catch (OperationCanceledException)
        {
            return new InstallerExecutionResult
            {
                Status = InstallerExecutionStatus.PostScanFailed,
                InstallerExitCode = installerExitCode,
                Error = "Post-install scan was canceled."
            };
        }
        catch
        {
            return new InstallerExecutionResult
            {
                Status = InstallerExecutionStatus.PostScanFailed,
                InstallerExitCode = installerExitCode,
                Error = "Post-install scan failed."
            };
        }
    }
}

public sealed class InstallerExecutionCoordinator
{
    private readonly Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> _scanSoftware;
    private readonly IInstallerPackageInspector _packageInspector;
    private readonly IInstallBeforeSnapshotEvidenceReader _snapshotReader;
    private readonly IInstallerTargetPathPolicy _targetPathPolicy;
    private readonly IInteractiveInstallerProcessLauncher _launcher;
    private readonly Func<DateTimeOffset> _now;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly Func<CancellationToken, Task<InstallFootprintCapture>> _scanCDriveFootprint;

    public InstallerExecutionCoordinator(
        Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> scanSoftware,
        IInstallerPackageInspector packageInspector,
        IInstallBeforeSnapshotEvidenceReader snapshotReader,
        IInstallerTargetPathPolicy targetPathPolicy,
        IInteractiveInstallerProcessLauncher launcher,
        Func<DateTimeOffset>? now = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        Func<CancellationToken, Task<InstallFootprintCapture>>? scanCDriveFootprint = null)
    {
        _scanSoftware = scanSoftware ?? throw new ArgumentNullException(nameof(scanSoftware));
        _packageInspector = packageInspector ?? throw new ArgumentNullException(nameof(packageInspector));
        _snapshotReader = snapshotReader ?? throw new ArgumentNullException(nameof(snapshotReader));
        _targetPathPolicy = targetPathPolicy ?? throw new ArgumentNullException(nameof(targetPathPolicy));
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _now = now ?? (() => DateTimeOffset.UtcNow);
        _delay = delay ?? Task.Delay;
        _scanCDriveFootprint = scanCDriveFootprint
            ?? (_ => Task.FromResult(InstallFootprintCapture.EmptyComplete));
    }

    public static InstallerExecutionCoordinator CreateProduction(
        Func<CancellationToken, Task<IReadOnlyList<SoftwareProfile>>> scanSoftware) =>
        new(
            scanSoftware,
            new WindowsInstallerPackageInspector(),
            new InstallBeforeSnapshotEvidenceReader(),
            new WindowsInstallerTargetPathPolicy(),
            new WindowsInteractiveInstallerProcessLauncher(),
            scanCDriveFootprint: cancellationToken => Task.Run(
                () => new WindowsInstallFootprintProbe().Capture(),
                cancellationToken));

    public async Task<InstallerExecutionResult> ExecuteConfirmedAsync(
        InstallerLaunchOperationPlan plan,
        InstallSystemSnapshot beforeSnapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(beforeSnapshot);
        if (!plan.Operation.ConfirmationAccepted
            || !string.Equals(
                plan.BeforeSnapshot.Evidence.InventoryFingerprintSha256,
                InstallBeforeSnapshotEvidenceService.ComputeInventoryFingerprint(
                    beforeSnapshot.SoftwareProfiles),
                StringComparison.OrdinalIgnoreCase)
            || plan.BeforeSnapshot.Evidence.SoftwareCount != beforeSnapshot.SoftwareProfiles.Count
            || !string.Equals(
                plan.BeforeSnapshot.Evidence.FootprintFingerprintSha256,
                InstallBeforeSnapshotEvidenceService.ComputeFootprintFingerprint(
                    beforeSnapshot.CDriveFootprint),
                StringComparison.OrdinalIgnoreCase)
            || plan.BeforeSnapshot.Evidence.FootprintPathCount
                != (beforeSnapshot.CDriveFootprint ?? InstallFootprintCapture.EmptyComplete).Paths.Count
            || plan.BeforeSnapshot.Evidence.FootprintStatus
                != (beforeSnapshot.CDriveFootprint ?? InstallFootprintCapture.EmptyComplete).Status)
        {
            return Refused("The confirmed operation is not bound to the before inventory.");
        }

        var handler = new InstallerLaunchOperationHandler(
            _packageInspector,
            _snapshotReader,
            _targetPathPolicy,
            _launcher,
            _now);
        var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
        var operationResult = await pipeline.ExecuteAsync(plan.Operation, cancellationToken);
        if (!operationResult.Success
            || operationResult.Payload is not InteractiveInstallerLaunchResult
            {
                Status: InteractiveInstallerLaunchStatus.Started,
                Session: not null
            } launch)
        {
            return Refused(operationResult.Error ?? "Installer launch was refused.");
        }

        using var session = launch.Session;
        try
        {
            await session.WaitForExitAsync(cancellationToken);
            await _delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return new InstallerExecutionResult
            {
                Status = InstallerExecutionStatus.InstallerWaitInterrupted,
                InstallerExitCode = session.ExitCode,
                Error = "Installer wait was interrupted."
            };
        }
        catch
        {
            return new InstallerExecutionResult
            {
                Status = InstallerExecutionStatus.InstallerWaitInterrupted,
                InstallerExitCode = session.ExitCode,
                Error = "Installer state could not be observed."
            };
        }

        return await CapturePostInstallSnapshotAsync(
            beforeSnapshot,
            session.ExitCode,
            cancellationToken);
    }

    public Task<InstallerExecutionResult> CapturePostInstallSnapshotAsync(
        InstallSystemSnapshot beforeSnapshot,
        int? installerExitCode = null,
        CancellationToken cancellationToken = default)
        => new InstallerPostScanCoordinator(
            _scanSoftware,
            _scanCDriveFootprint,
            _now).CaptureAsync(
                beforeSnapshot,
                installerExitCode,
                cancellationToken);

    private static InstallerExecutionResult Refused(string error) =>
        new()
        {
            Status = InstallerExecutionStatus.LaunchRefused,
            Error = error
        };
}
