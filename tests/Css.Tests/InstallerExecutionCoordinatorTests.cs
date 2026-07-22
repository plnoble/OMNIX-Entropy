using System.Security.Cryptography;
using System.Text;
using Css.App;
using Css.Core.Software;
using Css.InstallGuard.Installers;
using Css.Win32.Security;
using FluentAssertions;

namespace Css.Tests;

public sealed class InstallerExecutionCoordinatorTests : IDisposable
{
    private readonly string _fixtureRoot = Path.Combine(
        Path.GetTempPath(),
        "omnix-installer-coordinator-fixture-" + Guid.NewGuid().ToString("N"));

    public InstallerExecutionCoordinatorTests() => Directory.CreateDirectory(_fixtureRoot);

    [Fact]
    public async Task Confirmed_flow_waits_then_returns_initial_post_scan_without_claiming_success()
    {
        var fixture = await CreateFixtureAsync(confirm: true);
        var launcher = new FakeLauncher(new FakeSession(exitCode: 0));
        var scanCount = 0;
        var delayCount = 0;
        var coordinator = new InstallerExecutionCoordinator(
            _ =>
            {
                scanCount++;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>(
                [
                    Profile("Existing"),
                    Profile("New App", cDrivePaths: [@"C:\Users\Fixture\AppData\Local\NewApp"])
                ]);
            },
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now.AddMinutes(1),
            (_, _) =>
            {
                delayCount++;
                return Task.CompletedTask;
            });

        var result = await coordinator.ExecuteConfirmedAsync(
            fixture.Plan,
            fixture.Before);
        var view = InstallerExecutionResultPresenter.Create(result);

        result.Status.Should().Be(InstallerExecutionStatus.InitialPostScanCompleted);
        result.Report.Should().NotBeNull();
        result.Report!.HasCDriveWrites.Should().BeTrue();
        result.InstallerExitCode.Should().Be(0);
        launcher.Requests.Should().ContainSingle();
        scanCount.Should().Be(1);
        delayCount.Should().Be(1);
        view.Title.Should().Contain("初步检查");
        view.SafetyText.Should().Contain("不会被当作安装成功");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public async Task Unconfirmed_plan_stops_before_launcher_wait_delay_and_post_scan()
    {
        var fixture = await CreateFixtureAsync(confirm: false);
        var launcher = new FakeLauncher(new FakeSession(exitCode: 0));
        var scanCount = 0;
        var delayCount = 0;
        var coordinator = new InstallerExecutionCoordinator(
            _ =>
            {
                scanCount++;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>([]);
            },
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now,
            (_, _) =>
            {
                delayCount++;
                return Task.CompletedTask;
            });

        var result = await coordinator.ExecuteConfirmedAsync(
            fixture.Plan,
            fixture.Before);

        result.Status.Should().Be(InstallerExecutionStatus.LaunchRefused);
        launcher.Requests.Should().BeEmpty();
        scanCount.Should().Be(0);
        delayCount.Should().Be(0);
    }

    [Fact]
    public async Task Footprint_mismatch_stops_before_launcher_and_post_scan()
    {
        var fixture = await CreateFixtureAsync(confirm: true);
        var launcher = new FakeLauncher(new FakeSession(exitCode: 0));
        var scanCount = 0;
        var coordinator = new InstallerExecutionCoordinator(
            _ =>
            {
                scanCount++;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>([]);
            },
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now,
            (_, _) => Task.CompletedTask);
        var mismatchedBefore = fixture.Before with
        {
            CDriveFootprint = new InstallFootprintCapture
            {
                Status = InstallFootprintCaptureStatus.Complete,
                Paths = [@"C:\ProgramData\ChangedAfterConsent"]
            }
        };

        var result = await coordinator.ExecuteConfirmedAsync(
            fixture.Plan,
            mismatchedBefore);

        result.Status.Should().Be(InstallerExecutionStatus.LaunchRefused);
        result.Error.Should().Contain("before inventory");
        launcher.Requests.Should().BeEmpty();
        scanCount.Should().Be(0);
    }

    [Fact]
    public async Task Confirmed_flow_uses_bounded_footprint_in_initial_post_scan()
    {
        var fixture = await CreateFixtureAsync(confirm: true);
        var launcher = new FakeLauncher(new FakeSession(exitCode: 0));
        var footprintScanCount = 0;
        var coordinator = new InstallerExecutionCoordinator(
            _ => Task.FromResult<IReadOnlyList<SoftwareProfile>>([Profile("Existing")]),
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now.AddMinutes(1),
            (_, _) => Task.CompletedTask,
            _ =>
            {
                footprintScanCount++;
                return Task.FromResult(new InstallFootprintCapture
                {
                    Status = InstallFootprintCaptureStatus.Complete,
                    Paths = [@"C:\ProgramData\UnregisteredTool"]
                });
            });

        var result = await coordinator.ExecuteConfirmedAsync(
            fixture.Plan,
            fixture.Before);

        result.Status.Should().Be(InstallerExecutionStatus.InitialPostScanCompleted);
        result.Report!.NewCDrivePaths.Should().Contain(@"C:\ProgramData\UnregisteredTool");
        result.Report.CDriveFootprintStatus.Should().Be(InstallFootprintCaptureStatus.Complete);
        footprintScanCount.Should().Be(1);
        launcher.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Interrupted_installer_wait_does_not_run_post_scan_or_claim_installation_result()
    {
        var fixture = await CreateFixtureAsync(confirm: true);
        var launcher = new FakeLauncher(new FakeSession(
            exitCode: null,
            waitError: new InvalidOperationException("fixture wait failure")));
        var scanCount = 0;
        var coordinator = new InstallerExecutionCoordinator(
            _ =>
            {
                scanCount++;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>([]);
            },
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now.AddMinutes(1),
            (_, _) => Task.CompletedTask);

        var result = await coordinator.ExecuteConfirmedAsync(
            fixture.Plan,
            fixture.Before);
        var view = InstallerExecutionResultPresenter.Create(result);

        result.Status.Should().Be(InstallerExecutionStatus.InstallerWaitInterrupted);
        scanCount.Should().Be(0);
        view.Conclusion.Should().Contain("可能仍在运行");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Theory]
    [InlineData(InstallerExecutionStatus.InstallerWaitInterrupted, true)]
    [InlineData(InstallerExecutionStatus.PostScanFailed, true)]
    [InlineData(InstallerExecutionStatus.LaunchRefused, false)]
    [InlineData(InstallerExecutionStatus.InitialPostScanCompleted, false)]
    public void Result_offers_user_driven_post_scan_only_for_uncertain_observation_states(
        InstallerExecutionStatus status,
        bool expected)
    {
        var result = new InstallerExecutionResult
        {
            Status = status,
            Report = status == InstallerExecutionStatus.InitialPostScanCompleted
                ? InstallSnapshotDiffBuilder.Build(
                    new InstallSystemSnapshot(DateTimeOffset.UtcNow, []),
                    new InstallSystemSnapshot(DateTimeOffset.UtcNow.AddMinutes(1), []))
                : null
        };

        var view = InstallerExecutionResultPresenter.Create(result);

        view.CanRequestPostScanRetry.Should().Be(expected);
        view.PostScanRetryButtonText.Should().Be("我已完成安装，重新扫描");
        view.CanExecuteDirectly.Should().BeFalse();
    }

    [Fact]
    public async Task User_driven_post_scan_reuses_before_snapshot_without_relaunching_installer()
    {
        var fixture = await CreateFixtureAsync(confirm: true);
        var launcher = new FakeLauncher(new FakeSession(exitCode: 0));
        var softwareScanCount = 0;
        var footprintScanCount = 0;
        var coordinator = new InstallerExecutionCoordinator(
            _ =>
            {
                softwareScanCount++;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>(
                [
                    Profile("Existing"),
                    Profile("Recovered App", cDrivePaths: [@"C:\Users\Fixture\AppData\Local\Recovered"])
                ]);
            },
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now.AddMinutes(2),
            (_, _) => Task.CompletedTask,
            _ =>
            {
                footprintScanCount++;
                return Task.FromResult(new InstallFootprintCapture
                {
                    Status = InstallFootprintCaptureStatus.Complete,
                    Paths = [@"C:\Users\Fixture\AppData\Local\Recovered"]
                });
            });

        var result = await coordinator.CapturePostInstallSnapshotAsync(
            fixture.Before,
            installerExitCode: 23);

        result.Status.Should().Be(InstallerExecutionStatus.InitialPostScanCompleted);
        result.AfterSnapshot.Should().NotBeNull();
        result.Report.Should().NotBeNull();
        result.InstallerExitCode.Should().Be(23);
        softwareScanCount.Should().Be(1);
        footprintScanCount.Should().Be(1);
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Failed_user_driven_post_scan_remains_retryable_without_relaunching_installer()
    {
        var fixture = await CreateFixtureAsync(confirm: true);
        var launcher = new FakeLauncher(new FakeSession(exitCode: 0));
        var coordinator = new InstallerExecutionCoordinator(
            _ => Task.FromException<IReadOnlyList<SoftwareProfile>>(
                new IOException("fixture scan failure")),
            new FakeInspector(fixture.Package),
            new InstallBeforeSnapshotEvidenceReader(),
            new AllowTargetPolicy(),
            launcher,
            () => fixture.Now.AddMinutes(2),
            (_, _) => Task.CompletedTask);

        var result = await coordinator.CapturePostInstallSnapshotAsync(
            fixture.Before,
            installerExitCode: null);
        var view = InstallerExecutionResultPresenter.Create(result);

        result.Status.Should().Be(InstallerExecutionStatus.PostScanFailed);
        result.AfterSnapshot.Should().BeNull();
        result.Report.Should().BeNull();
        view.CanRequestPostScanRetry.Should().BeTrue();
        view.Conclusion.Should().NotContain("fixture scan failure");
        view.AgentAdvice.Should().NotContain("fixture scan failure");
        launcher.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Dedicated_post_scan_coordinator_reads_once_without_holding_launch_authority()
    {
        var softwareScanCount = 0;
        var footprintScanCount = 0;
        var now = DateTimeOffset.UtcNow;
        var before = new InstallSystemSnapshot(now, [Profile("Existing")]);
        var coordinator = new InstallerPostScanCoordinator(
            _ =>
            {
                softwareScanCount++;
                return Task.FromResult<IReadOnlyList<SoftwareProfile>>(
                    [Profile("Existing"), Profile("Later App")]);
            },
            _ =>
            {
                footprintScanCount++;
                return Task.FromResult(InstallFootprintCapture.EmptyComplete);
            },
            () => now.AddMinutes(3));

        var result = await coordinator.CaptureAsync(before, installerExitCode: 7);

        result.Status.Should().Be(InstallerExecutionStatus.InitialPostScanCompleted);
        result.Report.Should().NotBeNull();
        result.InstallerExitCode.Should().Be(7);
        softwareScanCount.Should().Be(1);
        footprintScanCount.Should().Be(1);

        var source = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerExecutionCoordinator.cs"));
        var start = source.IndexOf(
            "public sealed class InstallerPostScanCoordinator",
            StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        var end = source.IndexOf(
            "public sealed class InstallerExecutionCoordinator",
            start,
            StringComparison.Ordinal);
        end.Should().BeGreaterThan(start);
        var postScanSource = source[start..end];
        postScanSource.Should().NotContain("IInteractiveInstallerProcessLauncher");
        postScanSource.Should().NotContain("SafetyOperationPipeline");
        postScanSource.Should().NotContain("Process.Start");
        var executionSource = source[end..];
        executionSource.Should().Contain("new InstallerPostScanCoordinator(");
        executionSource.Should().Contain("_now).CaptureAsync(");
    }

    [Fact]
    public void Main_window_uses_typed_installer_readiness_and_stays_outside_launch_authority()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var coordinator = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerExecutionCoordinator.cs"));

        main.Should().Contain("InstallerLaunchReadinessPolicy.Evaluate");
        main.Should().Contain("InstallerLaunchPreparationPolicy.Evaluate");
        main.Should().Contain("preparation.CanPrepare");
        main.Should().NotContain("InstallerLaunchFeatureEnabled");
        main.Should().Contain("InstallerExecutionCoordinator.CreateProduction");
        main.Should().Contain("InstallerLaunchFinalConsentService.Confirm");
        main.Should().NotContain("WindowsInteractiveInstallerProcessLauncher");
        coordinator.Should().Contain("new SafetyOperationPipeline(handler.ExecuteAsync)");
        coordinator.Should().Contain("new WindowsInteractiveInstallerProcessLauncher()");
        coordinator.Should().NotContain("Process.Start");
    }

    [Fact]
    public void Main_window_reuses_verified_after_snapshot_for_application_catalog()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var prepareStart = main.IndexOf(
            "private async void PrepareInstaller_Click",
            StringComparison.Ordinal);
        prepareStart.Should().BeGreaterThanOrEqualTo(0);
        var prepareEnd = main.IndexOf(
            "private async Task<InstallerExecutionResult> PresentInstallerExecutionResultsAsync",
            prepareStart,
            StringComparison.Ordinal);
        prepareEnd.Should().BeGreaterThan(prepareStart);
        var prepare = main[prepareStart..prepareEnd];
        var presenterEnd = main.IndexOf(
            "private async void PersistentInstallPostScan_Click",
            prepareEnd,
            StringComparison.Ordinal);
        presenterEnd.Should().BeGreaterThan(prepareEnd);
        var presenter = main[prepareEnd..presenterEnd];

        const string successGate =
            "if (execution.AfterSnapshot is not null && execution.Report is not null)";
        const string catalogUpdate =
            "SetSoftwareProfiles(execution.AfterSnapshot.SoftwareProfiles);";
        prepare.Should().Contain("PresentInstallerExecutionResultsAsync");
        presenter.Should().Contain(successGate);
        presenter.Should().Contain(catalogUpdate);
        presenter.IndexOf(successGate, StringComparison.Ordinal)
            .Should().BeLessThan(presenter.IndexOf(catalogUpdate, StringComparison.Ordinal));
        presenter.IndexOf(catalogUpdate, StringComparison.Ordinal)
            .Should().BeLessThan(presenter.IndexOf("ApplyInstallDiffPresentation", StringComparison.Ordinal));
        presenter.Should().Contain("InstallerExecutionResultPresenter.Create(execution)");
    }

    [Theory]
    [InlineData(true, null, InstallerLaunchReadinessState.Ready, true)]
    [InlineData(true, "0", InstallerLaunchReadinessState.Ready, true)]
    [InlineData(true, "1", InstallerLaunchReadinessState.DisabledByOperator, false)]
    [InlineData(true, "TRUE", InstallerLaunchReadinessState.DisabledByOperator, false)]
    [InlineData(false, null, InstallerLaunchReadinessState.UnsupportedPlatform, false)]
    public void Installer_readiness_is_explicit_and_has_a_fail_closed_override(
        bool isWindows,
        string? disableOverride,
        InstallerLaunchReadinessState expectedState,
        bool expectedAvailability)
    {
        var readiness = InstallerLaunchReadinessPolicy.Evaluate(
            isWindows,
            disableOverride);

        readiness.State.Should().Be(expectedState);
        readiness.IsAvailable.Should().Be(expectedAvailability);
        readiness.StatusText.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Preparation_readiness_refuses_an_unavailable_target_before_snapshot_work()
    {
        var launchReadiness = InstallerLaunchReadinessPolicy.Evaluate(
            isWindows: true,
            disableOverride: null);
        var capability = new InstallerRoutingCapability
        {
            Mode = InstallerRoutingCapabilityMode.GuidedInteractiveRoute,
            Title = "fixture",
            AgentConclusion = "fixture",
            NextStep = "fixture",
            SafetyText = "fixture",
            TargetInstallPath = @"D:\Software\Fixture\Install",
            CanRequestInstallerLaunch = true,
            CanApplyTargetAutomatically = false
        };

        var preparation = InstallerLaunchPreparationPolicy.Evaluate(
            launchReadiness,
            capability,
            new DenyTargetPolicy());

        preparation.State.Should().Be(InstallerLaunchPreparationState.TargetUnavailable);
        preparation.CanPrepare.Should().BeFalse();
        preparation.StatusText.Should().Contain("不可用");
    }

    [Fact]
    public void Msix_preparation_is_a_windows_managed_storage_handoff_not_an_installer_launch()
    {
        var launchReadiness = InstallerLaunchReadinessPolicy.Evaluate(
            isWindows: true,
            disableOverride: null);
        var capability = new InstallerRoutingCapability
        {
            Mode = InstallerRoutingCapabilityMode.WindowsManagedStorage,
            Title = "fixture",
            AgentConclusion = "fixture",
            NextStep = "fixture",
            SafetyText = "fixture",
            TargetInstallPath = @"D:\Software\Fixture\Install",
            SettingsShortcutId = InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId,
            CanRequestInstallerLaunch = false,
            CanApplyTargetAutomatically = false
        };

        var preparation = InstallerLaunchPreparationPolicy.Evaluate(
            launchReadiness,
            capability,
            new AllowTargetPolicy());

        preparation.State.Should().Be(InstallerLaunchPreparationState.WindowsManagedStorageHandoff);
        preparation.CanPrepare.Should().BeFalse();
        preparation.StatusText.Should().Contain("Windows").And.Contain("新应用保存位置");
        capability.CanRequestInstallerLaunch.Should().BeFalse();
    }

    [Fact]
    public void Final_consent_puts_four_automation_backed_acknowledgements_before_confirm()
    {
        var xaml = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerFinalConsentWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerFinalConsentWindow.xaml.cs"));
        var ids = new[]
        {
            "InstallerConsentPackageCheckBox",
            "InstallerConsentLocationCheckBox",
            "InstallerConsentInteractionCheckBox",
            "InstallerConsentReportCheckBox"
        };

        foreach (var id in ids)
        {
            xaml.Should().Contain($"x:Name=\"{id}\"");
            xaml.Should().Contain($"AutomationProperties.AutomationId=\"{id}\"");
            xaml.IndexOf(id, StringComparison.Ordinal).Should().BeLessThan(
                xaml.IndexOf("InstallerFinalConsentConfirmButton", StringComparison.Ordinal));
        }
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerFinalConsentReadinessTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerFinalConsentConfirmButton\"");
        code.Should().Contain("remaining == 0");
        code.Should().Contain("InstallerLaunchFinalConsentDecision");
    }

    [Fact]
    public void Beginner_result_conclusion_is_first_view_and_has_no_execution_authority()
    {
        var xaml = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerExecutionResultWindow.xaml"));
        var code = File.ReadAllText(FindRepositoryFile(
            "src", "Css.App", "InstallerExecutionResultWindow.xaml.cs"));

        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerExecutionResultTitleTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerExecutionResultStatusTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerExecutionResultConclusionTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerExecutionResultAgentAdviceTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerExecutionResultSafetyTextBlock\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"InstallerExecutionResultPostScanRetryButton\"");
        xaml.IndexOf("InstallerExecutionResultConclusionTextBlock", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallerExecutionResultNextStepsListBox", StringComparison.Ordinal));
        code.Should().Contain("if (viewModel.CanExecuteDirectly)");
        code.Should().Contain("PostScanRetryRequested = true");
        code.Should().Contain("viewModel.CanRequestPostScanRetry");
        code.Should().NotContain("SafetyOperationPipeline");
        code.Should().NotContain("Process.Start");
    }

    [Fact]
public void Main_window_retries_post_scan_from_original_baseline_without_relaunching()
    {
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));
        var prepareStart = main.IndexOf(
            "private async void PrepareInstaller_Click",
            StringComparison.Ordinal);
        prepareStart.Should().BeGreaterThanOrEqualTo(0);
        var prepareEnd = main.IndexOf(
            "private async Task<InstallerExecutionResult> PresentInstallerExecutionResultsAsync",
            prepareStart,
            StringComparison.Ordinal);
        prepareEnd.Should().BeGreaterThan(prepareStart);
        var prepare = main[prepareStart..prepareEnd];
        var presenterEnd = main.IndexOf(
            "private async void PersistentInstallPostScan_Click",
            prepareEnd,
            StringComparison.Ordinal);
        presenterEnd.Should().BeGreaterThan(prepareEnd);
        var presenter = main[prepareEnd..presenterEnd];

        presenter.Should().Contain("while (true)");
        presenter.Should().Contain("resultWindow.PostScanRetryRequested");
        presenter.Should().Contain("capturePostScan(execution.InstallerExitCode)");
        presenter.Should().Contain("SetSoftwareProfiles(execution.AfterSnapshot.SoftwareProfiles);");
        presenter.Should().Contain("正在重新读取安装后的应用和 C 盘变化");
        prepare.Should().Contain("coordinator.CapturePostInstallSnapshotAsync(before, exitCode)");
        prepare.Should().Contain("PresentInstallerExecutionResultsAsync");
        prepare.Split(
            "coordinator.ExecuteConfirmedAsync(plan, before)",
            StringSplitOptions.None).Length.Should().Be(2);
    }

    [Fact]
    public void Install_page_exposes_session_bound_later_rescan_outside_advanced_diagnostics()
    {
        var xaml = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml"));
        var main = File.ReadAllText(FindRepositoryFile("src", "Css.App", "MainWindow.xaml.cs"));

        xaml.Should().Contain("x:Name=\"PersistentInstallPostScanButton\"");
        xaml.Should().Contain("AutomationProperties.AutomationId=\"PersistentInstallPostScanButton\"");
        xaml.Should().Contain("Click=\"PersistentInstallPostScan_Click\"");
        xaml.Should().Contain("Content=\"安装界面都关了，重新扫描\"");
        xaml.Should().Contain("Visibility=\"Collapsed\"");
        xaml.IndexOf("PersistentInstallPostScanButton", StringComparison.Ordinal)
            .Should().BeLessThan(xaml.IndexOf("InstallManualComparisonExpander", StringComparison.Ordinal));

        main.Should().Contain("private InstallSystemSnapshot? _activeInstallObservationBaseline;");
        main.Should().Contain("private int? _activeInstallObservationExitCode;");

        var resetStart = main.IndexOf("private void ResetInstallerAnalysis", StringComparison.Ordinal);
        var resetEnd = main.IndexOf("private void RememberInstallRoute_Click", resetStart, StringComparison.Ordinal);
        var reset = main[resetStart..resetEnd];
        reset.Should().Contain("_activeInstallObservationBaseline = null;");
        reset.Should().Contain("PersistentInstallPostScanButton.Visibility = Visibility.Collapsed;");

        var prepareStart = main.IndexOf(
            "private async void PrepareInstaller_Click",
            StringComparison.Ordinal);
        var prepareEnd = main.IndexOf(
            "private async Task<InstallerExecutionResult> PresentInstallerExecutionResultsAsync",
            prepareStart,
            StringComparison.Ordinal);
        prepareStart.Should().BeGreaterThanOrEqualTo(0);
        prepareEnd.Should().BeGreaterThan(prepareStart);
        var prepare = main[prepareStart..prepareEnd];
        const string launchGate =
            "if (execution.Status != InstallerExecutionStatus.LaunchRefused)";
        prepare.Should().Contain(launchGate);
        prepare.IndexOf(launchGate, StringComparison.Ordinal).Should().BeLessThan(
            prepare.IndexOf("_activeInstallObservationBaseline = before;", StringComparison.Ordinal));
        prepare.IndexOf("_activeInstallObservationBaseline = before;", StringComparison.Ordinal)
            .Should().BeLessThan(prepare.IndexOf(
                "PersistentInstallPostScanButton.Visibility = Visibility.Visible;",
                StringComparison.Ordinal));

        var handlerStart = main.IndexOf(
            "private async void PersistentInstallPostScan_Click",
            StringComparison.Ordinal);
        var handlerEnd = main.IndexOf(
            "private async void CaptureBeforeInstall_Click",
            handlerStart,
            StringComparison.Ordinal);
        handlerStart.Should().BeGreaterThanOrEqualTo(0);
        handlerEnd.Should().BeGreaterThan(handlerStart);
        var handler = main[handlerStart..handlerEnd];
        handler.Should().Contain("InstallerPostScanCoordinator.CreateProduction");
        handler.Should().Contain("_activeInstallObservationBaseline");
        handler.Should().Contain("PresentInstallerExecutionResultsAsync");
        handler.Should().NotContain("ExecuteConfirmedAsync");
        handler.Should().NotContain("SafetyOperationPipeline");
        handler.Should().NotContain("Process.Start");
    }

    public void Dispose()
    {
        if (Directory.Exists(_fixtureRoot))
            Directory.Delete(_fixtureRoot, recursive: true);
    }

    private async Task<Fixture> CreateFixtureAsync(bool confirm)
    {
        var now = DateTimeOffset.UtcNow;
        var packagePath = Path.Combine(_fixtureRoot, "ExampleTool.exe");
        await File.WriteAllTextAsync(packagePath, "Inno Setup Setup Data", Encoding.UTF8);
        var info = new FileInfo(packagePath);
        var package = new InstallerPackageEvidence
        {
            Status = InstallerPackageInspectionStatus.Ready,
            PackagePath = packagePath,
            FileName = info.Name,
            LengthBytes = info.Length,
            LastWriteUtc = new DateTimeOffset(info.LastWriteTimeUtc, TimeSpan.Zero),
            Sha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(packagePath))),
            SignatureStatus = AuthenticodeSignatureStatus.Trusted,
            SignerSubject = "CN=Fixture Publisher",
            DetectedKind = InstallerKind.InnoSetup,
            KindConfidence = InstallerKindConfidence.High,
            KindEvidence = ["fixture marker"]
        };
        var before = new InstallSystemSnapshot(now, [Profile("Existing")]);
        var evidence = await InstallBeforeSnapshotEvidenceService.CreateAsync(
            package,
            before,
            Path.Combine(_fixtureRoot, Guid.NewGuid().ToString("N") + ".json"),
            now);
        var analysis = InstallerAnalyzer.AnalyzePackage(package);
        var capability = InstallerRoutingCapabilityPolicy.Evaluate(analysis, package);
        var plan = InstallerLaunchOperationPlanner.Create(
            analysis,
            package,
            capability,
            evidence,
            new AllowTargetPolicy());
        if (confirm)
        {
            plan = plan with
            {
                Operation = InstallerLaunchFinalConsentService.Confirm(
                    plan.Operation,
                    new InstallerLaunchFinalConsentDecision
                    {
                        PackagePublisherAccepted = true,
                        LocationLimitAccepted = true,
                        InteractiveReviewAccepted = true,
                        PostScanLimitAccepted = true
                    },
                    now)
            };
        }
        return new Fixture(now, package, before, plan);
    }

    private static SoftwareProfile Profile(
        string name,
        IReadOnlyList<string>? cDrivePaths = null) =>
        new()
        {
            Name = name,
            Publisher = "Fixture",
            InstallPath = @"D:\Software\Fixture\Install",
            CDriveWritePaths = cDrivePaths ?? []
        };

    private static string FindRepositoryFile(params string[] segments) =>
        Path.Combine([FindRepositoryRoot(), .. segments]);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private sealed record Fixture(
        DateTimeOffset Now,
        InstallerPackageEvidence Package,
        InstallSystemSnapshot Before,
        InstallerLaunchOperationPlan Plan);

    private sealed class FakeInspector(InstallerPackageEvidence package)
        : IInstallerPackageInspector
    {
        public InstallerPackageEvidence Inspect(string packagePath) => package;
    }

    private sealed class AllowTargetPolicy : IInstallerTargetPathPolicy
    {
        public bool IsAllowed(string targetPath, out string? reason)
        {
            reason = null;
            return true;
        }
    }

    private sealed class DenyTargetPolicy : IInstallerTargetPathPolicy
    {
        public bool IsAllowed(string targetPath, out string? reason)
        {
            reason = "fixture target unavailable";
            return false;
        }
    }

    private sealed class FakeLauncher(FakeSession session)
        : IInteractiveInstallerProcessLauncher
    {
        public List<InteractiveInstallerLaunchRequest> Requests { get; } = [];

        public ValueTask<InteractiveInstallerLaunchResult> LaunchAsync(
            InteractiveInstallerLaunchRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return ValueTask.FromResult(new InteractiveInstallerLaunchResult
            {
                Status = InteractiveInstallerLaunchStatus.Started,
                Session = session
            });
        }
    }

    private sealed class FakeSession(int? exitCode, Exception? waitError = null)
        : IInteractiveInstallerProcessSession
    {
        public int? ExitCode => exitCode;

        public Task WaitForExitAsync(CancellationToken cancellationToken = default) =>
            waitError is null ? Task.CompletedTask : Task.FromException(waitError);

        public void Dispose()
        {
        }
    }
}
