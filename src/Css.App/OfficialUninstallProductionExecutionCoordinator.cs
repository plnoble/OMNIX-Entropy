using Css.Core.Uninstall;
using Css.Ipc.Uninstall;
using Css.Win32.Security;

namespace Css.App;

public interface IOfficialUninstallProductionExecutionCoordinator
{
    Task<OfficialUninstallProductionExecutionOutcome> ExecuteAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken = default);
}

public interface IOfficialUninstallProductionLifecycleRunner
{
    Task<OfficialUninstallWorkerLifecycleResult> RunAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken = default);
}

public sealed class OfficialUninstallProductionExecutionOutcome
{
    public required OfficialUninstallWorkerResultViewModel Summary { get; init; }
    public OfficialUninstallWorkerLifecycleResult? Lifecycle { get; init; }
    public OfficialUninstallElevatedResponseViewModel? Response { get; init; }
    public OfficialUninstallPostScanViewModel? PostScan => Response?.PostScan;
    public bool ProductionAttempted { get; init; }
    public bool CompletedProduction =>
        Lifecycle?.Status == OfficialUninstallWorkerLifecycleStatus.CompletedProduction;
}

public sealed class OfficialUninstallProductionExecutionCoordinator
    : IOfficialUninstallProductionExecutionCoordinator
{
    private readonly Func<OfficialUninstallWorkerTrustAssessment> _assessTrust;
    private readonly Func<OfficialUninstallWorkerTrustAssessment,
        IOfficialUninstallProductionLifecycleRunner> _createRunner;

    public OfficialUninstallProductionExecutionCoordinator(
        Func<OfficialUninstallWorkerTrustAssessment> assessTrust,
        Func<OfficialUninstallWorkerTrustAssessment,
            IOfficialUninstallProductionLifecycleRunner> createRunner)
    {
        _assessTrust = assessTrust ?? throw new ArgumentNullException(nameof(assessTrust));
        _createRunner = createRunner ?? throw new ArgumentNullException(nameof(createRunner));
    }

    public static OfficialUninstallProductionExecutionCoordinator CreateForCurrentPackage() =>
        new(
            CurrentPackageWorkerTrustProvider.Assess,
            CreateWindowsRunner);

    public async Task<OfficialUninstallProductionExecutionOutcome> ExecuteAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.CanSubmit)
        {
            var invalid = new OfficialUninstallWorkerLifecycleResult
            {
                Status = OfficialUninstallWorkerLifecycleStatus.InvalidRequest,
                ChildExited = false
            };
            return LifecycleOutcome(request, invalid, attempted: false);
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
            return new OfficialUninstallProductionExecutionOutcome
            {
                Summary = OfficialUninstallWorkerTrustPresenter.Create(trust),
                ProductionAttempted = false
            };
        }

        OfficialUninstallWorkerLifecycleResult lifecycle;
        try
        {
            var runner = _createRunner(trust)
                ?? throw new InvalidOperationException("The production lifecycle runner is unavailable.");
            lifecycle = await runner.RunAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            lifecycle = new OfficialUninstallWorkerLifecycleResult
            {
                Status = OfficialUninstallWorkerLifecycleStatus.Canceled,
                ChildExited = false
            };
        }
        catch
        {
            lifecycle = new OfficialUninstallWorkerLifecycleResult
            {
                Status = OfficialUninstallWorkerLifecycleStatus.LaunchFailed,
                ChildExited = false
            };
        }

        return LifecycleOutcome(request, lifecycle, attempted: true);
    }

    private static OfficialUninstallProductionExecutionOutcome LifecycleOutcome(
        OfficialUninstallElevatedRequestDraft request,
        OfficialUninstallWorkerLifecycleResult lifecycle,
        bool attempted)
    {
        var response = lifecycle.Status == OfficialUninstallWorkerLifecycleStatus.CompletedProduction
            && lifecycle.Response is not null
                ? OfficialUninstallElevatedResponsePresenter.Create(request, lifecycle.Response)
                : null;
        return new OfficialUninstallProductionExecutionOutcome
        {
            Summary = OfficialUninstallWorkerResultPresenter.Create(lifecycle),
            Lifecycle = lifecycle,
            Response = response,
            ProductionAttempted = attempted
        };
    }

    private static IOfficialUninstallProductionLifecycleRunner CreateWindowsRunner(
        OfficialUninstallWorkerTrustAssessment trust)
    {
        var launcher = WindowsOfficialUninstallProductionWorkerLauncher.Create(trust);
        var lifecycle = new OfficialUninstallWorkerLifecycleClient(
            launcher,
            new WindowsOfficialUninstallWorkerImageInspector(),
            new WindowsOfficialUninstallCurrentProcessIdentityProvider(),
            new WindowsOfficialUninstallPipePeerIdentityReader(),
            startupTimeout: TimeSpan.FromSeconds(20),
            bootstrapTimeout: TimeSpan.FromSeconds(15),
            responseTimeout: TimeSpan.FromMinutes(2),
            shutdownTimeout: TimeSpan.FromSeconds(5));
        return new OfficialUninstallProductionLifecycleRunner(lifecycle);
    }

}

internal sealed class OfficialUninstallProductionLifecycleRunner(
    OfficialUninstallWorkerLifecycleClient lifecycle)
    : IOfficialUninstallProductionLifecycleRunner
{
    public Task<OfficialUninstallWorkerLifecycleResult> RunAsync(
        OfficialUninstallElevatedRequestDraft request,
        CancellationToken cancellationToken = default) =>
        lifecycle.RunProductionOnceAsync(request, cancellationToken);
}
