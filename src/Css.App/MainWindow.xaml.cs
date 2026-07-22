using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Css.Core.Agent;
using Css.Core.Apps;
using Css.Core;
using Css.Core.Migration;
using Css.Core.Operations;
using Css.Core.Quarantine;
using Css.Core.Recommendations;
using Css.Core.Software;
using Css.Core.Startup;
using Css.Core.Timeline;
using Css.Core.Uninstall;
using Css.Scanner.Disk;
using Css.Scanner.Experience;
using Css.Scanner.Persistence;
using Css.Scanner.Recovery;
using Css.Scanner.Software;
using Css.InstallGuard.Installers;
using Css.InstallGuard.Routing;
using Css.Win32.Migration;
using Css.Win32.Quarantine;
using Css.Win32.Startup;
using Css.Win32.SystemHealth;
using Microsoft.Win32;

namespace Css.App;

public partial class MainWindow : Window
{
    private static readonly InstallerLaunchReadiness InstallerLaunchAvailability =
        InstallerLaunchReadinessPolicy.Evaluate(
            OperatingSystem.IsWindows(),
            Environment.GetEnvironmentVariable(
                InstallerLaunchReadinessPolicy.DisableEnvironmentVariable));
    private readonly DiskScanner _scanner = new();
    private readonly SoftwareInventoryScanner _softwareScanner = new();
    private readonly SoftwareInventoryFixtureScanner? _softwareFixtureScanner =
        SoftwareInventoryFixtureScanner.TryCreate(AppDevelopmentPathResolver.ResolveSoftwareInventoryFixturePath());
    private readonly ScanSnapshotStore _snapshotStore = new(DefaultDatabasePath());
    private readonly HealthDigestStore _healthDigestStore = new(DefaultDatabasePath());
    private readonly ActionTimelineStore _timelineStore = new(DefaultDatabasePath());
    private readonly FileQuarantineService _quarantineService = new(DefaultQuarantineRoot());
    private readonly PersonalStorageExplorerLauncher _personalStorageExplorerLauncher =
        PersonalStorageExplorerLauncher.CreateDefault();
    private readonly IQuarantineCandidateIdentityReader _quarantineIdentityReader =
        new WindowsQuarantineCandidateIdentityReader();
    private readonly IStartupEntryControlStore _startupEntryStore = CreateStartupEntryControlStore();
    private readonly StartupRollbackManifestStore _startupManifestStore =
        new(DefaultStartupRollbackRoot());
    private readonly SoftwareInventoryLoadGate _softwareInventoryLoadGate = new();
    private readonly ReadOnlyEvidenceLoadGate _healthScanLoadGate = new();
    private readonly ReadOnlyEvidenceLoadGate _machineObservationLoadGate = new();
    private readonly ReadOnlyEvidenceLoadGate _timelineLoadGate = new();
    private CancellationTokenSource? _scanCts;
    private InstallSystemSnapshot? _beforeInstallSnapshot;
    private InstallSystemSnapshot? _afterInstallSnapshot;
    private InstallSnapshotDiffReport? _lastInstallDiffReport;
    private InstallSystemSnapshot? _activeInstallObservationBaseline;
    private int? _activeInstallObservationExitCode;
    private InstallerDetectionResult? _lastInstallerAnalysis;
    private InstallerPackageEvidence? _lastInstallerPackageEvidence;
    private InstallerRoutingCapability? _lastInstallerCapability;
    private IReadOnlyList<SoftwareProfile> _softwareProfiles = [];
    private IReadOnlyList<GrowthFinding> _latestGrowthFindings = [];
    private IReadOnlyCollection<string> _personalStorageEvidencePaths = [];
    private IReadOnlyDictionary<string, MigrationClosureSummaryViewModel> _migrationClosureBySoftware =
        new Dictionary<string, MigrationClosureSummaryViewModel>(StringComparer.OrdinalIgnoreCase);
    private OperationDescriptor? _pendingDrawerOperation;
    private string? _pendingDrawerTargetAppName;
    private string? _pendingStartupTargetAppName;
    private HealthCheckSummary? _baseHealthSummary;
    private HealthCheckSummary? _lastHealthSummary;
    private bool _healthDigestHistoryHasEvidence;
    private bool _isOpeningHealthDigestEvidence;
    private MachineHealthObservation? _latestMachineHealthObservation;
    private int _latestObservedSnapshotCount;
    private AppCatalogFilter _appCatalogFilter = AppCatalogFilter.All;
    private AppCatalogSort _appCatalogSort = AppCatalogSort.Risk;

    public MainWindow()
    {
        InitializeComponent();
        InitializeDriveOptions();
        ApplyCDrivePageChrome();
        ShowPage("Home");
        LoadAgentSkills();
        LoadSystemToolShortcuts();
        LoadWindowsSettingsShortcuts();
        LoadAgentNextSteps();
        LoadInstallRoutingMemoryRules();
        SetAppFilterSelected();
        _ = LoadHealthDigestHistoryAsync();
    }

    private void LoadInstallRoutingMemoryRules()
    {
        var view = InstallRoutingMemoryPresenter.Create(
            InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath()));
        InstallRoutingMemorySummaryTextBlock.Text = view.Summary;
        InstallRoutingMemoryListBox.ItemsSource = view.Rows;
        var hasRules = view.Rows.Count > 0;
        InstallRoutingMemoryListBox.Visibility = hasRules
            ? Visibility.Visible
            : Visibility.Collapsed;
        ForgetInstallRoutingRuleButton.Visibility = hasRules
            ? Visibility.Visible
            : Visibility.Collapsed;
        if (!hasRules)
        {
            InstallRoutingMemoryListBox.SelectedItem = null;
            ForgetInstallRoutingRuleButton.IsEnabled = false;
        }
    }

    private async void StartScan_Click(object sender, RoutedEventArgs e)
    {
        await RefreshHealthScanAsync();
    }

    private void InstallRoutingMemoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var row = InstallRoutingMemoryListBox.SelectedItem as InstallRoutingMemoryRuleRowViewModel;
        ForgetInstallRoutingRuleButton.IsEnabled = row is not null && row.CanForget;
    }

    private void ForgetInstallRoutingRule_Click(object sender, RoutedEventArgs e)
    {
        if (InstallRoutingMemoryListBox.SelectedItem is not InstallRoutingMemoryRuleRowViewModel row || !row.CanForget)
        {
            StatusTextBlock.Text = "请先选择一条已经记住的安装规则。";
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "确认忘记这条安装推荐规则吗？\n\n" +
            row.Title +
            "\n\n只会影响以后的安装建议；不会卸载软件、不会移动文件、不会运行安装器。",
            "忘记安装规则",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);
        if (confirmation != MessageBoxResult.OK)
        {
            StatusTextBlock.Text = "已取消忘记规则，安装建议没有变化。";
            return;
        }

        var memory = InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath());
        var updated = memory.ForgetRule(row.RuleKey);
        InstallRoutingMemoryStore.Save(DefaultInstallRoutingMemoryPath(), updated);
        LoadInstallRoutingMemoryRules();
        ForgetInstallRoutingRuleButton.IsEnabled = false;
        StatusTextBlock.Text = "已忘记这条安装推荐规则；只会影响以后的安装分析建议。";
    }

    private void CancelScan_Click(object sender, RoutedEventArgs e)
    {
        _scanCts?.Cancel();
        StatusTextBlock.Text = "正在取消扫描...";
    }

    private async void ScanSoftware_Click(object sender, RoutedEventArgs e)
    {
        await RefreshSoftwareInventoryAsync();
    }

    private void PickInstaller_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择安装包",
            Filter = "安装包 (*.exe;*.msi;*.msix;*.appx)|*.exe;*.msi;*.msix;*.appx|所有文件 (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            InstallerPathTextBox.Text = dialog.FileName;
            ResetInstallerAnalysis();
        }
    }

    private void InstallerPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsInitialized)
            ResetInstallerAnalysis();
    }

    private async void AnalyzeInstaller_Click(object sender, RoutedEventArgs e)
    {
        var path = InstallerPathTextBox.Text;
        if (string.IsNullOrWhiteSpace(path))
        {
            InstallerCapabilityPanel.Visibility = Visibility.Visible;
            InstallerCapabilityTitleTextBlock.Text = "请先选择安装包";
            InstallerCapabilityConclusionTextBlock.Text = "建议从软件官网下载，不要使用来历不明的安装包。";
            InstallerCapabilityTargetTextBlock.Text = string.Empty;
            InstallerCapabilityNextStepTextBlock.Text = "选择文件后再让 Agent 分析。";
            InstallerCapabilitySafetyTextBlock.Text = "未读取文件，也没有运行任何程序。";
            InstallerCapabilityStatusTextBlock.Text = string.Empty;
            return;
        }

        AnalyzeInstallerButton.IsEnabled = false;
        PrepareInstallerButton.IsEnabled = false;
        InstallerCapabilityPanel.Visibility = Visibility.Visible;
        InstallerCapabilityTitleTextBlock.Text = "正在核验安装包...";
        InstallerCapabilityConclusionTextBlock.Text = "正在读取文件指纹、发布者签名和安装器类型。";
        InstallerCapabilityTargetTextBlock.Text = string.Empty;
        InstallerCapabilityNextStepTextBlock.Text = string.Empty;
        InstallerCapabilitySafetyTextBlock.Text = "只读检查，不会运行安装包。";
        InstallerCapabilityStatusTextBlock.Text = string.Empty;
        try
        {
            var package = await Task.Run(() => new WindowsInstallerPackageInspector().Inspect(path));
            var routingMemory = InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath());
            var result = InstallerAnalyzer.AnalyzePackage(package, routingMemory: routingMemory);
            var capability = InstallerRoutingCapabilityPolicy.Evaluate(result, package);
            _lastInstallerPackageEvidence = package;
            _lastInstallerAnalysis = result;
            _lastInstallerCapability = capability;
            ApplyInstallerCapability(result, package, capability);
        }
        finally
        {
            AnalyzeInstallerButton.IsEnabled = true;
        }
    }

    private void ApplyInstallerCapability(
        InstallerDetectionResult analysis,
        InstallerPackageEvidence package,
        InstallerRoutingCapability capability)
    {
        var preparation = EvaluateInstallerPreparation(capability);
        var routeSource = analysis.RecommendedRoute.FromUserMemory
            ? "使用你记住的规则"
            : "使用 OMNIX 默认分类";
        InstallerCapabilityTitleTextBlock.Text = capability.Title;
        InstallerCapabilityConclusionTextBlock.Text = capability.AgentConclusion;
        InstallerCapabilityTargetTextBlock.Text =
            capability.Mode == InstallerRoutingCapabilityMode.WindowsManagedStorage
                ? "位置管理: 由 Windows 决定，可在设置中选择默认保存盘"
                : "推荐位置: " + capability.TargetInstallPath;
        InstallerCapabilityNextStepTextBlock.Text = capability.NextStep;
        InstallerCapabilitySafetyTextBlock.Text = capability.SafetyText;
        InstallerCapabilityStatusTextBlock.Text = preparation.StatusText;
        InstallerAnalysisTextBlock.Text =
            $"软件: {analysis.SoftwareName}\n" +
            $"真实类型: {package.DetectedKind} / 可信度: {package.KindConfidence}\n" +
            $"发布者状态: {package.SignatureStatus} / {package.SignerSubject ?? "未知"}\n" +
            $"文件 SHA-256: {package.Sha256 ?? "不可用"}\n" +
            $"路径规则: {routeSource}\n" +
            $"交互参数: {(capability.InteractiveArguments.Count == 0 ? "不传目录参数" : string.Join(" ; ", capability.InteractiveArguments))}";
        InstallRememberRouteButton.IsEnabled = package.HasStableIdentity
            && capability.Mode is InstallerRoutingCapabilityMode.AutomaticInteractiveRoute
                or InstallerRoutingCapabilityMode.GuidedInteractiveRoute;
        PrepareInstallerButton.IsEnabled = preparation.CanPrepare;
        var canOpenStorageSettings =
            capability.Mode == InstallerRoutingCapabilityMode.WindowsManagedStorage
            && string.Equals(
                capability.SettingsShortcutId,
                InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId,
                StringComparison.Ordinal);
        OpenInstallerStorageSettingsButton.Visibility = canOpenStorageSettings
            ? Visibility.Visible
            : Visibility.Collapsed;
        OpenInstallerStorageSettingsButton.IsEnabled = canOpenStorageSettings;
    }

    private static InstallerLaunchPreparationReadiness EvaluateInstallerPreparation(
        InstallerRoutingCapability capability) =>
        InstallerLaunchPreparationPolicy.Evaluate(
            InstallerLaunchAvailability,
            capability,
            new WindowsInstallerTargetPathPolicy());

    private void ResetInstallerAnalysis()
    {
        _lastInstallerAnalysis = null;
        _lastInstallerPackageEvidence = null;
        _lastInstallerCapability = null;
        _activeInstallObservationBaseline = null;
        _activeInstallObservationExitCode = null;
        PersistentInstallPostScanButton.IsEnabled = false;
        PersistentInstallPostScanButton.Visibility = Visibility.Collapsed;
        InstallRememberRouteButton.IsEnabled = false;
        PrepareInstallerButton.IsEnabled = false;
        OpenInstallerStorageSettingsButton.IsEnabled = false;
        OpenInstallerStorageSettingsButton.Visibility = Visibility.Collapsed;
        InstallerCapabilityPanel.Visibility = Visibility.Collapsed;
    }

    private void RememberInstallRoute_Click(object sender, RoutedEventArgs e)
    {
        if (_lastInstallerAnalysis is null)
        {
            InstallerAnalysisTextBlock.Text = "请先分析一个安装包，再记住安装位置。";
            return;
        }

        var choiceWindow = new InstallRouteMemoryChoiceWindow(InstallRouteMemoryChoicePresenter.Create(_lastInstallerAnalysis))
        {
            Owner = this
        };
        if (choiceWindow.ShowDialog() != true || choiceWindow.SelectedScope is null)
        {
            StatusTextBlock.Text = "已取消记住安装位置，没有修改规则。";
            return;
        }

        var memory = InstallRoutingMemoryStore.Load(DefaultInstallRoutingMemoryPath());
        var updated = choiceWindow.SelectedScope == InstallRoutingMemoryScope.Category
            ? memory.RememberRouteForCategory(_lastInstallerAnalysis.RecommendedRoute)
            : memory.RememberRoute(_lastInstallerAnalysis.RecommendedRoute);
        InstallRoutingMemoryStore.Save(DefaultInstallRoutingMemoryPath(), updated);
        LoadInstallRoutingMemoryRules();
        StatusTextBlock.Text = choiceWindow.SelectedScope == InstallRoutingMemoryScope.Category
            ? "已记住这一类软件的安装位置；下次分析同类安装包会优先推荐它。"
            : "已记住这个软件的安装位置；下次分析同名安装包会优先推荐它。";
    }

    private async void PrepareInstaller_Click(object sender, RoutedEventArgs e)
    {
        if (_lastInstallerAnalysis is null
            || _lastInstallerPackageEvidence is null
            || _lastInstallerCapability is null)
        {
            StatusTextBlock.Text = "请先让 Agent 完成安装包分析。";
            return;
        }
        var preparation = EvaluateInstallerPreparation(_lastInstallerCapability);
        if (!preparation.CanPrepare)
        {
            StatusTextBlock.Text = preparation.StatusText;
            return;
        }

        PrepareInstallerButton.IsEnabled = false;
        AnalyzeInstallerButton.IsEnabled = false;
        PickInstallerButton.IsEnabled = false;
        SetInstallSnapshotButtonsEnabled(false);
        _activeInstallObservationBaseline = null;
        _activeInstallObservationExitCode = null;
        PersistentInstallPostScanButton.IsEnabled = false;
        PersistentInstallPostScanButton.Visibility = Visibility.Collapsed;
        StatusTextBlock.Text = "正在自动捕获安装前只读快照...";
        try
        {
            var now = DateTimeOffset.UtcNow;
            var beforeProfiles = await ScanSoftwareProfilesAsync();
            var beforeFootprint = await CaptureInstallFootprintAsync();
            var before = new InstallSystemSnapshot(now, beforeProfiles, beforeFootprint);
            _beforeInstallSnapshot = before;
            var evidencePath = Path.Combine(
                DefaultInstallEvidenceRoot(),
                "before-" + Guid.NewGuid().ToString("N") + ".json");
            var snapshotEvidence = await InstallBeforeSnapshotEvidenceService.CreateAsync(
                _lastInstallerPackageEvidence,
                before,
                evidencePath,
                now);
            var plan = InstallerLaunchOperationPlanner.Create(
                _lastInstallerAnalysis,
                _lastInstallerPackageEvidence,
                _lastInstallerCapability,
                snapshotEvidence,
                new WindowsInstallerTargetPathPolicy());
            var consentWindow = new InstallerFinalConsentWindow(
                InstallerLaunchFinalConsentPresenter.Create(plan))
            {
                Owner = this
            };
            if (consentWindow.ShowDialog() != true || consentWindow.Decision is null)
            {
                StatusTextBlock.Text = "已取消安装，没有运行安装包。";
                return;
            }

            plan = plan with
            {
                Operation = InstallerLaunchFinalConsentService.Confirm(
                    plan.Operation,
                    consentWindow.Decision,
                    DateTimeOffset.UtcNow)
            };
            StatusTextBlock.Text = "安全检查通过后会打开安装界面；OMNIX 不会替你点击安装。";
            var coordinator = InstallerExecutionCoordinator.CreateProduction(
                _ => ScanSoftwareProfilesAsync());
            var execution = await coordinator.ExecuteConfirmedAsync(plan, before);
            if (execution.Status != InstallerExecutionStatus.LaunchRefused)
            {
                _activeInstallObservationBaseline = before;
                _activeInstallObservationExitCode = execution.InstallerExitCode;
                PersistentInstallPostScanButton.IsEnabled = true;
                PersistentInstallPostScanButton.Visibility = Visibility.Visible;
            }
            execution = await PresentInstallerExecutionResultsAsync(
                execution,
                exitCode => coordinator.CapturePostInstallSnapshotAsync(before, exitCode));
            StatusTextBlock.Text = execution.Status == InstallerExecutionStatus.InitialPostScanCompleted
                ? "安装后的初步扫描已完成；这不等于确认安装成功。"
                : "安装流程没有形成完整报告，请查看 Agent 结论。";
        }
        catch
        {
            StatusTextBlock.Text = "安装准备没有完成，OMNIX 没有报告安装成功。";
        }
        finally
        {
            AnalyzeInstallerButton.IsEnabled = true;
            PickInstallerButton.IsEnabled = true;
            SetInstallSnapshotButtonsEnabled(true);
            InstallRememberRouteButton.IsEnabled = _lastInstallerPackageEvidence?.HasStableIdentity == true;
            PrepareInstallerButton.IsEnabled = _lastInstallerCapability is not null
                && EvaluateInstallerPreparation(_lastInstallerCapability).CanPrepare;
        }
    }

    private async Task<InstallerExecutionResult> PresentInstallerExecutionResultsAsync(
        InstallerExecutionResult execution,
        Func<int?, Task<InstallerExecutionResult>> capturePostScan)
    {
        while (true)
        {
            if (execution.AfterSnapshot is not null && execution.Report is not null)
            {
                _afterInstallSnapshot = execution.AfterSnapshot;
                _lastInstallDiffReport = execution.Report;
                _activeInstallObservationExitCode = execution.InstallerExitCode;
                SetSoftwareProfiles(execution.AfterSnapshot.SoftwareProfiles);
                ApplyInstallDiffPresentation(
                    InstallSnapshotDiffPresenter.Create(execution.Report));
                InstallDiffAgentExplainButton.IsEnabled = true;
            }
            var resultWindow = new InstallerExecutionResultWindow(
                InstallerExecutionResultPresenter.Create(execution))
            {
                Owner = this
            };
            resultWindow.ShowDialog();
            if (!resultWindow.PostScanRetryRequested)
                return execution;

            StatusTextBlock.Text = "正在重新读取安装后的应用和 C 盘变化...";
            execution = await capturePostScan(execution.InstallerExitCode);
        }
    }

    private async void PersistentInstallPostScan_Click(object sender, RoutedEventArgs e)
    {
        var before = _activeInstallObservationBaseline;
        if (before is null)
        {
            PersistentInstallPostScanButton.IsEnabled = false;
            PersistentInstallPostScanButton.Visibility = Visibility.Collapsed;
            StatusTextBlock.Text = "当前没有可继续比较的安装前记录，请重新分析安装包。";
            return;
        }

        PersistentInstallPostScanButton.IsEnabled = false;
        InstallerPathTextBox.IsEnabled = false;
        AnalyzeInstallerButton.IsEnabled = false;
        PickInstallerButton.IsEnabled = false;
        PrepareInstallerButton.IsEnabled = false;
        SetInstallSnapshotButtonsEnabled(false);
        StatusTextBlock.Text = "正在重新读取安装后的应用和 C 盘变化...";
        try
        {
            var postScan = InstallerPostScanCoordinator.CreateProduction(
                _ => ScanSoftwareProfilesAsync());
            var execution = await postScan.CaptureAsync(
                before,
                _activeInstallObservationExitCode);
            if (!ReferenceEquals(before, _activeInstallObservationBaseline))
            {
                StatusTextBlock.Text = "安装包已经变化，这次扫描没有合并到旧报告。";
                return;
            }

            execution = await PresentInstallerExecutionResultsAsync(
                execution,
                exitCode => postScan.CaptureAsync(before, exitCode));
            StatusTextBlock.Text = execution.Status == InstallerExecutionStatus.InitialPostScanCompleted
                ? "已重新读取安装后的变化；这仍是初步报告，不代表安装成功。"
                : "重新扫描没有形成完整报告；保留上一次有效结果，可以稍后再试。";
        }
        catch
        {
            StatusTextBlock.Text = "重新扫描没有完成；保留上一次有效结果，没有修改电脑。";
        }
        finally
        {
            InstallerPathTextBox.IsEnabled = true;
            AnalyzeInstallerButton.IsEnabled = true;
            PickInstallerButton.IsEnabled = true;
            SetInstallSnapshotButtonsEnabled(true);
            PrepareInstallerButton.IsEnabled = _lastInstallerCapability is not null
                && EvaluateInstallerPreparation(_lastInstallerCapability).CanPrepare;
            PersistentInstallPostScanButton.IsEnabled =
                ReferenceEquals(before, _activeInstallObservationBaseline);
        }
    }

    private async void CaptureBeforeInstall_Click(object sender, RoutedEventArgs e)
    {
        await CaptureInstallSnapshotAsync(isBefore: true);
    }

    private async void CaptureAfterInstall_Click(object sender, RoutedEventArgs e)
    {
        await CaptureInstallSnapshotAsync(isBefore: false);
    }

    private void BuildInstallDiff_Click(object sender, RoutedEventArgs e)
    {
        if (_beforeInstallSnapshot is null || _afterInstallSnapshot is null)
        {
            const string message = "\u8bf7\u5148\u5206\u522b\u6355\u83b7\u5b89\u88c5\u524d\u548c\u5b89\u88c5\u540e\u5feb\u7167\u3002";
            _lastInstallDiffReport = null;
            InstallDiffAgentExplainButton.IsEnabled = false;
            InstallDiffAgentExplainButton.Visibility = Visibility.Collapsed;
            InstallDiffAgentPanel.Visibility = Visibility.Collapsed;
            InstallDiffActionPlanPanel.Visibility = Visibility.Collapsed;
            InstallDiffSummaryTextBlock.Text = message;
            InstallDiffCardsListBox.ItemsSource = Array.Empty<InstallSnapshotDiffCardViewModel>();
            InstallDiffCardsListBox.Visibility = Visibility.Collapsed;
            InstallDiffTechnicalDetailsExpander.Visibility = Visibility.Collapsed;
            InstallDiffTextBox.Text = message;
            return;
        }

        var report = InstallSnapshotDiffBuilder.Build(_beforeInstallSnapshot, _afterInstallSnapshot);
        _lastInstallDiffReport = report;
        InstallDiffAgentExplainButton.IsEnabled = true;
        InstallDiffAgentPanel.Visibility = Visibility.Collapsed;
        InstallDiffActionPlanPanel.Visibility = Visibility.Collapsed;
        var view = InstallSnapshotDiffPresenter.Create(report);
        ApplyInstallDiffPresentation(view);
        StatusTextBlock.Text = report.HasCDriveWrites
            ? "安装变化报告完成：发现新增 C 盘路径。"
            : "安装变化报告完成：未发现新增 C 盘路径。";
    }

    private void ExplainInstallDiff_Click(object sender, RoutedEventArgs e)
    {
        if (_lastInstallDiffReport is null)
        {
            StatusTextBlock.Text = "\u8bf7\u5148\u751f\u6210\u5b89\u88c5\u53d8\u5316\u62a5\u544a\u3002";
            return;
        }

        var advice = InstallSnapshotDiffAgentPresenter.Create(_lastInstallDiffReport);
        ApplyInstallDiffAgentAdvice(advice);
        InstallDiffActionPlanPanel.Visibility = Visibility.Collapsed;
        StatusTextBlock.Text = "Computer Agent \u5df2\u89e3\u91ca\u8fd9\u6b21\u5b89\u88c5\u53d8\u5316\uff1b\u5c1a\u672a\u6267\u884c\u4efb\u4f55\u5904\u7406\u3002";
    }

    private void ApplyInstallDiffAgentAdvice(InstallSnapshotDiffAgentViewModel advice)
    {
        InstallDiffAgentTitleTextBlock.Text = advice.Title;
        InstallDiffAgentHeadlineTextBlock.Text = advice.Headline;
        InstallDiffAgentMeaningTextBlock.Text = advice.WhatThisMeans;
        InstallDiffAgentStepsListBox.ItemsSource = advice.NextSteps;
        InstallDiffAgentSafetyTextBlock.Text = advice.SafetyBoundary;
        InstallDiffAgentPanel.Visibility = Visibility.Visible;
    }

    private void GenerateInstallDiffActionPlan_Click(object sender, RoutedEventArgs e)
    {
        if (_lastInstallDiffReport is null)
        {
            StatusTextBlock.Text = "\u8bf7\u5148\u751f\u6210\u5b89\u88c5\u53d8\u5316\u62a5\u544a\u3002";
            return;
        }

        var plan = InstallSnapshotDiffActionPlanPresenter.Create(_lastInstallDiffReport);
        ApplyInstallDiffActionPlan(plan);
        StatusTextBlock.Text = "Computer Agent \u5df2\u751f\u6210\u5904\u7406\u65b9\u6848\uff1b\u5c1a\u672a\u6267\u884c\u4efb\u4f55\u5904\u7406\u3002";
    }

    private void ApplyInstallDiffActionPlan(InstallSnapshotDiffActionPlanViewModel plan)
    {
        InstallDiffActionPlanSummaryTextBlock.Text = plan.Summary;
        InstallDiffActionPlanReviewSummaryTextBlock.Text = plan.ReviewSummary;
        InstallDiffEvidenceReviewExpander.IsExpanded = false;
        InstallDiffCDriveEvidenceReviewListBox.ItemsSource = plan.EvidenceReview.CDriveItems;
        InstallDiffBackgroundEvidenceReviewListBox.ItemsSource = plan.EvidenceReview.BackgroundItems;
        InstallDiffEligibleActionsListBox.ItemsSource = plan.EvidenceReview.EligibleActions;
        InstallDiffEvidenceReviewSafetyTextBlock.Text = plan.EvidenceReview.SafetyBoundary;
        InstallDiffCandidatePreviewPanel.Visibility = Visibility.Collapsed;
        InstallDiffActionPlanListBox.ItemsSource = plan.Items;
        InstallDiffActionPlanSafetyTextBlock.Text = plan.SafetyBoundary;
        InstallDiffActionPlanPanel.Visibility = Visibility.Visible;
    }

    private void PreviewInstallDiffCandidate_Click(object sender, RoutedEventArgs e)
    {
        if (_lastInstallDiffReport is null)
        {
            StatusTextBlock.Text = "请先生成安装变化报告。";
            return;
        }

        if (sender is not Button { Tag: InstallSnapshotEligibleActionKind kind })
        {
            StatusTextBlock.Text = "无法识别要预览的方案类型。";
            return;
        }

        var preview = InstallSnapshotCandidatePreviewPresenter.Create(_lastInstallDiffReport, kind);
        ApplyInstallDiffCandidatePreview(preview);
        StatusTextBlock.Text = preview.Status == InstallSnapshotCandidatePreviewStatus.Refused
            ? "Computer Agent 因证据不足拒绝生成具体预览；未执行任何处理。"
            : "Computer Agent 已生成只读方案预览；尚未执行任何处理。";
    }

    private void ApplyInstallDiffCandidatePreview(InstallSnapshotCandidatePreviewViewModel preview)
    {
        InstallDiffCandidatePreviewTitleTextBlock.Text = preview.Title;
        InstallDiffCandidatePreviewStatusTextBlock.Text = preview.StatusLabel;
        InstallDiffCandidatePreviewSummaryTextBlock.Text = preview.Summary;
        InstallDiffCandidatePreviewAgentTextBlock.Text = preview.AgentTakeaway;
        InstallDiffCandidatePreviewLinesListBox.ItemsSource = preview.Lines;
        InstallDiffCandidatePreviewMissingEvidenceListBox.ItemsSource = preview.MissingEvidence;
        InstallDiffCandidatePreviewSafetyTextBlock.Text = preview.SafetyBoundary;
        InstallDiffCandidateOpenAppButton.Content = preview.NavigationLabel;
        InstallDiffCandidateOpenAppButton.Tag = preview.TargetAppName;
        InstallDiffCandidateOpenAppButton.Visibility = preview.CanNavigateToApp
            ? Visibility.Visible
            : Visibility.Collapsed;
        InstallDiffCandidateOpenAppButton.IsEnabled = preview.CanNavigateToApp;
        InstallDiffCandidatePreviewPanel.Visibility = Visibility.Visible;
    }

    private async void OpenInstallDiffCandidateApp_Click(object sender, RoutedEventArgs e)
    {
        var targetAppName = (sender as FrameworkElement)?.Tag?.ToString();
        var resolution = await ResolveAndOpenAppTargetAsync(targetAppName);
        if (!resolution.CanOpen)
        {
            InstallDiffCandidatePreviewStatusTextBlock.Text = resolution.Headline;
            InstallDiffCandidatePreviewSummaryTextBlock.Text = resolution.Explanation;
            StatusTextBlock.Text = resolution.Headline;
            return;
        }

        StatusTextBlock.Text =
            "已打开对应应用详情；安装报告没有执行任何处理，请从应用抽屉重新审核。";
    }

    private void ApplyInstallDiffPresentation(InstallSnapshotDiffViewModel view)
    {
        InstallDiffSummaryTextBlock.Text = view.Summary;
        InstallDiffSafetyTextBlock.Text = view.SafetyText;
        InstallDiffCardsListBox.ItemsSource = view.Cards;
        InstallDiffCardsListBox.Visibility = view.Cards.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        InstallDiffAgentExplainButton.Visibility = Visibility.Visible;
        InstallDiffTechnicalDetailsExpander.Visibility = Visibility.Visible;
        InstallDiffTextBox.Text = string.Join(Environment.NewLine, view.TechnicalDetails);
    }

    private async void LoadTimeline_Click(object sender, RoutedEventArgs e)
    {
        await LoadTimelineAsync();
    }

    private async void RestoreTimeline_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not ActionTimelineItemViewModel item || !item.CanRestore)
        {
            StatusTextBlock.Text = "当前记录没有可用的还原入口。";
            return;
        }

        if (item.RestoreOperationKind?.Equals(
                StartupEntryControlOperationPolicy.RestoreKind,
                StringComparison.OrdinalIgnoreCase) == true)
        {
            await RestoreStartupTimelineItemAsync(item);
            return;
        }

        await RestoreQuarantineTimelineItemAsync(item);
    }

    private async void ExecuteRecommendation_Click(object sender, RoutedEventArgs e)
    {
        await ExecuteSelectedRecommendationAsync();
    }

    private void RecommendationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selection = RecommendationSelectionPresenter.Create(RecommendationsListBox.SelectedItem as RecommendationCardViewModel);
        ApplyRecommendationSelection(selection);
    }

    private void ApplyRecommendationSelection(RecommendationSelectionViewModel selection)
    {
        ExecuteRecommendationButton.IsEnabled = selection.CanContinue;
        ExecuteRecommendationButton.Content = selection.ButtonText;
        RecommendationActionTextBlock.Text = selection.ExplanationText;
        RecommendationActionTakeawayTextBlock.Text = selection.AgentTakeaway;
        RecommendationActionNextStepTextBlock.Text = selection.NextStepText;
        RecommendationActionSafetyTextBlock.Text = selection.SafetyBoundary;
        RecommendationActionPlanListBox.ItemsSource = selection.PlanLines;
    }

    private void ExplainHealthFinding_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not HealthFinding finding)
        {
            StatusTextBlock.Text = "\u8bf7\u5148\u9009\u62e9\u4e00\u6761\u5173\u952e\u53d1\u73b0\u3002";
            return;
        }

        var response = HomeAgentResponsePresenter.Explain(finding);
        ShowHomeAgentResponse(response);
    }

    private async void ShowHealthFindingDetails_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not HealthFinding finding)
        {
            StatusTextBlock.Text = "\u8bf7\u5148\u9009\u62e9\u4e00\u6761\u5173\u952e\u53d1\u73b0\u3002";
            return;
        }

        if (!string.IsNullOrWhiteSpace(finding.TargetAppName))
        {
            var resolution = await ResolveAndOpenAppTargetAsync(finding.TargetAppName);
            if (resolution.CanOpen)
                return;
            ShowHomeAgentResponse(HomeAgentResponsePresenter.AppTargetUnavailable(resolution));
            return;
        }

        var response = HomeAgentResponsePresenter.ShowDetails(finding);
        ShowHomeAgentResponse(response);
    }

    private async void HomeAgentResponseNavigate_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not HomeAgentResponseViewModel response
            || !response.CanNavigate)
        {
            StatusTextBlock.Text = "这条 Agent 回答没有安全的下一步入口。";
            return;
        }

        if (!string.IsNullOrWhiteSpace(response.TargetAppName))
        {
            if (response.NavigationDestination != HomeAgentNavigationDestination.Applications
                || response.TargetAppFilter is not null)
            {
                StatusTextBlock.Text = "Agent 的应用目标和页面不一致，已拦截。";
                return;
            }

            var resolution = await ResolveAndOpenAppTargetAsync(response.TargetAppName);
            if (!resolution.CanOpen)
                ShowHomeAgentResponse(HomeAgentResponsePresenter.AppTargetUnavailable(resolution));
            return;
        }

        if (response.TargetAppFilter is { } appFilter)
        {
            if (response.NavigationDestination != HomeAgentNavigationDestination.Applications)
            {
                StatusTextBlock.Text = "Agent 的应用筛选上下文与页面不一致，已拦截。";
                return;
            }

            await OpenAgentAppCatalogFilterAsync(appFilter);
            return;
        }

        var targetPage = response.NavigationDestination switch
        {
            HomeAgentNavigationDestination.CDrive => "CDrive",
            HomeAgentNavigationDestination.CDrivePersonalStorage => "CDrive",
            HomeAgentNavigationDestination.Applications => "Apps",
            _ => null
        };
        if (targetPage is null || !IsAgentNavigationTarget(targetPage))
        {
            StatusTextBlock.Text = "Agent 的下一步不在内部页面白名单中，已拦截。";
            return;
        }

        ShowPage(targetPage);
        if (response.NavigationDestination == HomeAgentNavigationDestination.CDrivePersonalStorage)
        {
            CDrivePage.UpdateLayout();
            PersonalStorageFindingsListBox.UpdateLayout();
            PersonalStorageFindingsListBox.BringIntoView();
            StatusTextBlock.Text = "已打开个人大文件和疑似重复文件的只读候选；没有执行删除或移动。";
            return;
        }
        StatusTextBlock.Text = "Agent 已打开建议页面；没有自动执行任何处理。";
    }

    private void CreateHealthFindingPlan_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not HealthFinding finding)
        {
            StatusTextBlock.Text = "\u8bf7\u5148\u9009\u62e9\u4e00\u6761\u5173\u952e\u53d1\u73b0\u3002";
            return;
        }

        var response = HomeAgentResponsePresenter.CreatePlan(finding);
        ShowHomeAgentResponse(response);
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as FrameworkElement)?.Tag?.ToString();
        if (!string.IsNullOrWhiteSpace(tag))
            ShowPage(tag);
    }

    private void AppFilter_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as FrameworkElement)?.Tag?.ToString();
        if (Enum.TryParse<AppCatalogFilter>(tag, out var filter))
        {
            _appCatalogFilter = filter;
            SetAppFilterSelected();
            RefreshAppCatalog();
        }
    }

    private void AppSortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AppSortComboBox?.SelectedItem is ComboBoxItem item
            && Enum.TryParse<AppCatalogSort>(item.Tag?.ToString(), out var sort))
        {
            _appCatalogSort = sort;
            if (!IsInitialized || AppTilesListBox is null)
                return;

            RefreshAppCatalog();
        }
    }

    private void AppSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (AppSearchPlaceholderTextBlock is not null)
        {
            AppSearchPlaceholderTextBlock.Visibility =
                string.IsNullOrEmpty(AppSearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        if (!IsInitialized || AppTilesListBox is null)
            return;

        RefreshAppCatalog();
    }

    private void AppTilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AppTilesListBox.SelectedItem is AppTileUi selected)
            ShowAppDrawer(selected.Profile);
    }

    private async void PreviewUninstall_Click(object sender, RoutedEventArgs e)
    {
        if (AppTilesListBox.SelectedItem is not AppTileUi selected)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.NoSelectionForUninstall());
            return;
        }

        await ShowUninstallPlanAsync(selected.Profile);
    }

    private void OpenUpdate_Click(object sender, RoutedEventArgs e)
    {
        var window = new UpdateWindow { Owner = this };
        window.ShowDialog();
    }

    private async Task<IReadOnlyList<SoftwareProfile>?> TryScanSoftwareProfilesAfterProductionAttemptAsync()
    {
        try
        {
            return await ScanSoftwareProfilesAsync();
        }
        catch
        {
            return null;
        }
    }

    private async Task ShowUninstallPlanAsync(SoftwareProfile profile)
    {
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var entry = AppActionEntryPolicy.Evaluate(drawer, AppActionKind.Uninstall);
        if (!entry.IsAllowed)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.UninstallRefused(entry.Reason));
            return;
        }

        ApplyDrawerActionHost(AppDrawerActionHostPresenter.ShowUninstall(drawer));
        StatusTextBlock.Text = "\u6b63\u5728\u53ea\u8bfb\u68c0\u67e5\u6062\u590d\u51c6\u5907...";
        var restorePointScanner = new WindowsRestorePointScanner();
        var restorePointScan = await restorePointScanner.ScanWithStatusAsync();
        var preview = UninstallPlanPresentationBuilder.Create(
            profile,
            reinstallSourceFileExists: File.Exists,
            reinstallSourceDirectoryExists: Directory.Exists,
            reinstallSourceSignatureResolver: SignatureInspector.GetSignatureSubject,
            restorePoints: restorePointScan.Points,
            restorePointScanState: restorePointScan.State);
        var uninstallTrust = CurrentPackageWorkerTrustProvider.Assess();
        var uninstallReadiness = ProductionExecutionReadinessPresenter.Create(
            uninstallTrust,
            ProductionExecutionCapability.OfficialUninstall);
        var window = new UninstallPlanWindow(
            profile,
            preview,
            restorePointScan.Points,
            restorePointScan.State,
            OfficialUninstallProductionExecutionCoordinator.CreateForCurrentPackage(),
            uninstallReadiness)
        {
            Owner = this
        };
        window.ShowDialog();
        if (window.ProductionExecutionAttempted)
        {
            var refreshedProfiles = await TryScanSoftwareProfilesAfterProductionAttemptAsync();
            if (refreshedProfiles is null)
            {
                var conclusion = window.LastExecutionConclusion
                    ?? "\u5378\u8f7d\u7ed3\u679c\u6ca1\u6709\u5b8c\u6574\u786e\u8ba4";
                StatusTextBlock.Text = conclusion
                    + "\uff1b\u5e94\u7528\u590d\u67e5\u6682\u65f6\u65e0\u6cd5\u5b8c\u6210\u3002\u8bf7\u4e0d\u8981\u624b\u52a8\u5220\u9664\u8f6f\u4ef6\u76ee\u5f55\uff0c\u7a0d\u540e\u91cd\u65b0\u626b\u63cf\u3002";
                return;
            }
            if (window.ProductionCompleted && window.ProductionResidueReviewRecommended
                && window.ProductionPostScanActionRequested ==
                    OfficialUninstallPostScanAction.ReviewResidue)
            {
                await ReviewUninstallResidueAsync(profile, refreshedProfiles);
                return;
            }

            if (window.ProductionPostScanActionRequested ==
                OfficialUninstallPostScanAction.RetryReadOnlyScan)
            {
                ShowReadOnlyUninstallResidueReviewAfterRetry(profile, refreshedProfiles);
                return;
            }

            SetSoftwareProfiles(refreshedProfiles);
        }

        StatusTextBlock.Text = window.LastExecutionConclusion is not null
            ? "卸载流程结果：" + window.LastExecutionConclusion
            : "已查看卸载安全方案；尚未运行卸载器，也没有删除残留。";
    }

    private async void PreviewMigration_Click(object sender, RoutedEventArgs e)
    {
        if (AppTilesListBox.SelectedItem is not AppTileUi selected)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.NoSelectionForMigration());
            return;
        }

        await ShowMigrationPlanAsync(selected.Profile);
    }

    private async Task ShowMigrationPlanAsync(SoftwareProfile profile)
    {
        var migrationClosure = FindMigrationClosure(profile);
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var migrationState = MigrationClosureDrawerStatePresenter.Create(profile, drawer, migrationClosure);
        if (!migrationState.CanOpenPlan)
        {
            StatusTextBlock.Text = migrationState.ButtonReason;
            return;
        }

        ApplyDrawerActionHost(AppDrawerActionHostPresenter.ShowMigration(drawer, migrationClosure));
        var preview = MigrationPlanPresentationBuilder.AddClosureReview(
            MigrationPlanPresentationBuilder.Create(profile),
            migrationClosure);
        var migrationTrust = CurrentPackageWorkerTrustProvider.Assess();
        var migrationReadiness = ProductionExecutionReadinessPresenter.Create(
            migrationTrust,
            ProductionExecutionCapability.Migration);
        var window = new MigrationPlanWindow(
            preview,
            async () => MigrationPlanPresentationBuilder.AddClosureReview(
                await CreateRollbackManifestPreviewAsync(profile),
                migrationClosure),
            MigrationProductionExecutionCoordinator.CreateForCurrentPackage(),
            migrationReadiness)
        {
            Owner = this
        };
        window.ShowDialog();
        if (window.ProductionExecutionAttempted)
        {
            var refreshedProfiles = await TryScanSoftwareProfilesAfterProductionAttemptAsync();
            if (refreshedProfiles is null)
            {
                var conclusion = window.LastExecutionConclusion
                    ?? "\u8fc1\u79fb\u7ed3\u679c\u6ca1\u6709\u5b8c\u6574\u786e\u8ba4";
                StatusTextBlock.Text = conclusion
                    + "\uff1b\u5e94\u7528\u4f4d\u7f6e\u590d\u67e5\u6682\u65f6\u65e0\u6cd5\u5b8c\u6210\uff0c\u4e0d\u4f1a\u81ea\u52a8\u7ee7\u7eed\u642c\u52a8\u3002\u8bf7\u7a0d\u540e\u91cd\u65b0\u626b\u63cf\u3002";
                return;
            }
            SetSoftwareProfiles(
                refreshedProfiles,
                refreshCatalog: false,
                refreshAgent: false);
            var closureAvailable = await RefreshMigrationClosureAsync(refreshUi: true);
            if (window.ProductionCompleted)
            {
                StatusTextBlock.Text = closureAvailable
                    ? "迁移已完成，已重新扫描应用位置、C 盘写入证据和迁移闭环。"
                    : "迁移已完成，但闭环记录暂时无法读取；请不要手动继续搬动目录。";
            }
            else
            {
                var conclusion = window.LastExecutionConclusion
                    ?? "迁移结果没有完整确认";
                StatusTextBlock.Text = closureAvailable
                    ? conclusion + "；已重新扫描应用位置和迁移闭环，不会自动继续搬动。"
                    : conclusion + "；已重新扫描应用位置，但闭环记录暂时无法读取，不会自动继续搬动。";
            }
        }
        else
        {
            StatusTextBlock.Text = "\u5df2\u67e5\u770b\u8fc1\u79fb\u65b9\u6848\uff1b\u672c\u6b21\u6ca1\u6709\u5b8c\u6210\u8fc1\u79fb\u3002";
        }
    }

    private void PreviewCacheCleanup_Click(object sender, RoutedEventArgs e)
    {
        if (AppTilesListBox.SelectedItem is not AppTileUi selected)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.NoSelectionForCacheCleanup());
            return;
        }

        ShowCacheCleanupPreview(selected.Profile);
    }

    private void ShowCacheCleanupPreview(SoftwareProfile profile)
    {
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var entry = AppActionEntryPolicy.Evaluate(drawer, AppActionKind.CacheCleanup);
        if (!entry.IsAllowed)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(entry.Reason));
            return;
        }

        var plan = AppCacheCleanupPlanBuilder.Create(
            profile,
            CurrentUserDataRoots(),
            Directory.Exists,
            IsReparsePoint,
            EstimateExistingPathSize);
        ApplyDrawerActionHost(AppDrawerActionHostPresenter.ShowCacheCleanup(drawer, plan));
        _pendingDrawerOperation = plan.Operation;
        _pendingDrawerTargetAppName = plan.CanContinue ? profile.Name : null;
    }

    private async void PreviewStartupControl_Click(object sender, RoutedEventArgs e)
    {
        if (AppTilesListBox.SelectedItem is not AppTileUi selected)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.NoSelectionForStartupControl());
            return;
        }

        await ShowStartupControlPreviewAsync(selected.Profile);
    }

    private async Task ShowStartupControlPreviewAsync(SoftwareProfile profile)
    {
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var entry = AppActionEntryPolicy.Evaluate(drawer, AppActionKind.StartupControl);
        if (!entry.IsAllowed)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlRefused(entry.Reason));
            return;
        }

        var handoff = AppStartupSettingsHandoffPresenter.Create(profile);
        var preparation = await StartupControlPreparationService.PrepareAsync(
            profile,
            _startupEntryStore);
        ApplyDrawerActionHost(AppDrawerActionHostPresenter.ShowStartupControl(
            drawer,
            preparation,
            handoff));
        _pendingStartupTargetAppName = preparation.CanContinue ? profile.Name : null;
    }

    private void ApplyDrawerActionHost(AppDrawerActionHostViewModel state)
    {
        _pendingDrawerOperation = null;
        _pendingDrawerTargetAppName = null;
        _pendingStartupTargetAppName = null;
        DrawerActionPreviewPanel.Visibility = state.IsVisible
            ? Visibility.Visible
            : Visibility.Collapsed;
        DrawerActionPreviewTitleTextBlock.Text = state.Title;
        DrawerActionPreviewSummaryTextBlock.Text = state.Summary;
        DrawerActionPreviewAgentTextBlock.Text = state.AgentTakeaway;
        DrawerActionPreviewNextStepTextBlock.Text = state.NextStepText;
        DrawerActionPreviewSafetyTextBlock.Text = state.SafetyText;
        DrawerActionPreviewListBox.ItemsSource = state.Lines;
        DrawerActionPreviewPrimaryButton.Content = state.PrimaryActionText;
        DrawerActionPreviewPrimaryButton.Tag = state.PrimaryActionKey;
        DrawerActionPreviewPrimaryButton.Visibility = string.IsNullOrWhiteSpace(state.PrimaryActionText)
            ? Visibility.Collapsed
            : Visibility.Visible;
        DrawerActionPreviewPrimaryButton.IsEnabled = !string.IsNullOrWhiteSpace(state.PrimaryActionText);

        if (!string.IsNullOrWhiteSpace(state.StatusText))
            StatusTextBlock.Text = state.StatusText;

        if (state.IsVisible)
            DrawerActionPreviewPanel.BringIntoView();
    }

    private async void DrawerActionPreviewPrimary_Click(object sender, RoutedEventArgs e)
    {
        switch ((sender as FrameworkElement)?.Tag?.ToString())
        {
            case "CacheCleanup":
                await ExecutePendingAppCacheCleanupAsync();
                break;
            case "StartupSettings":
                OpenAllowlistedWindowsSettings(AppStartupSettingsHandoffPresenter.StartupSettingsShortcutId);
                break;
            case "StartupDisableReview":
                await ReviewAndExecutePendingStartupDisableAsync();
                break;
            case "Timeline":
                ShowPage("Timeline");
                StatusTextBlock.Text = "\u5df2\u6253\u5f00\u540e\u6094\u836f\u4e2d\u5fc3\uff1b\u8fd9\u91cc\u53ea\u67e5\u770b\u8bb0\u5f55\uff0c\u4e0d\u4f1a\u81ea\u52a8\u8fd8\u539f\u3002";
                break;
            default:
                StatusTextBlock.Text = "\u8fd9\u4e2a\u7ed3\u679c\u52a8\u4f5c\u53ea\u652f\u6301\u5b89\u5168\u5bfc\u822a\uff0c\u4e0d\u4f1a\u6267\u884c\u7cfb\u7edf\u4fee\u6539\u3002";
                break;
        }
    }

    private async Task RefreshStartupStateAfterAttemptAsync()
    {
        try
        {
            SetSoftwareProfiles(await ScanSoftwareProfilesAsync());
        }
        catch
        {
            // Keep the last application list when the read-only refresh is unavailable.
        }

        try
        {
            await LoadTimelineAsync();
        }
        catch
        {
            // The startup-operation conclusion remains authoritative when history cannot reload.
        }
    }

    private async Task ReviewAndExecutePendingStartupDisableAsync()
    {
        var targetAppName = _pendingStartupTargetAppName;
        if (string.IsNullOrWhiteSpace(targetAppName))
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlRefused(
                "自启动方案已经失效，请重新选择应用并生成方案。"));
            return;
        }

        DrawerActionPreviewPrimaryButton.IsEnabled = false;
        StartupRollbackManifestEvidence? manifestEvidence = null;
        var confirmed = false;
        var pipelineAttempted = false;
        var stateSynchronized = false;
        StatusTextBlock.Text = "正在重新核对这个应用的自启动证据...";
        try
        {
            var currentProfiles = await ScanSoftwareProfilesAsync();
            SetSoftwareProfiles(currentProfiles);
            var resolution = AppDrawerTargetResolver.Resolve(targetAppName, currentProfiles);
            if (!resolution.CanOpen || resolution.Profile is null)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlRefused(
                    "无法唯一确认对应应用，旧方案已停止。"));
                return;
            }

            var profile = resolution.Profile;
            var handoff = AppStartupSettingsHandoffPresenter.Create(profile);
            var preparation = await StartupControlPreparationService.PrepareAsync(
                profile,
                _startupEntryStore);
            if (!preparation.CanContinue)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlRefused(
                    preparation.Summary,
                    handoff));
                return;
            }

            manifestEvidence = await _startupManifestStore.CreateAsync(preparation);
            var operation = StartupEntryControlOperationPolicy.CreateDisablePlan(
                preparation,
                manifestEvidence);
            var confirmation = StartupControlConfirmationPresenter.Create(preparation, operation);
            var confirmationWindow = new StartupControlConfirmationWindow(confirmation)
            {
                Owner = this
            };
            if (confirmationWindow.ShowDialog() != true)
            {
                await _startupManifestStore.DeleteUncommittedAsync(manifestEvidence);
                manifestEvidence = null;
                StatusTextBlock.Text = "已取消关闭自启动，没有修改任何设置。";
                return;
            }

            confirmed = true;
            var descriptor = StartupEntryControlOperationPolicy.ConfirmForExecution(operation);
            var handler = new StartupEntryControlOperationHandler(
                _startupEntryStore,
                _startupManifestStore,
                _timelineStore);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            StatusTextBlock.Text = "正在执行已确认的自启动方案...";
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(descriptor);
            await RefreshStartupStateAfterAttemptAsync();
            stateSynchronized = true;
            if (!result.Success)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlRefused(
                    "自启动处理没有确认完成；请重新扫描应用并到后悔药中心核对当前状态。",
                    handoff));
                return;
            }

            ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlCompleted(
                preparation.SoftwareName));
        }
        catch (Exception)
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await RefreshStartupStateAfterAttemptAsync();
                stateSynchronized = true;
            }
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.StartupControlRefused(
                "自启动方案暂时无法完成；没有把异常当作成功。"));
        }
        finally
        {
            if (!confirmed && manifestEvidence is not null)
            {
                try
                {
                    await _startupManifestStore.DeleteUncommittedAsync(manifestEvidence);
                }
                catch
                {
                    // A verified rollback snapshot is retained when safe cleanup cannot be proven.
                }
            }
            DrawerActionPreviewPrimaryButton.IsEnabled = true;
        }
    }

    private async Task RefreshCacheCleanupStateAfterAttemptAsync()
    {
        try
        {
            await LoadTimelineAsync();
        }
        catch
        {
            // The cache-cleanup conclusion remains authoritative when history cannot reload.
        }

        try
        {
            SetSoftwareProfiles(await ScanSoftwareProfilesAsync());
        }
        catch
        {
            // Keep the last application list when the read-only refresh is unavailable.
        }
    }

    private async Task ExecutePendingAppCacheCleanupAsync()
    {
        var operation = _pendingDrawerOperation;
        var targetAppName = _pendingDrawerTargetAppName;
        var pipelineAttempted = false;
        var stateSynchronized = false;
        if (operation is null || string.IsNullOrWhiteSpace(targetAppName))
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                "缓存方案已经失效，请重新选择应用并生成方案。"));
            return;
        }

        var candidatePolicy = QuarantineOperationPolicy.ValidateCandidate(operation);
        var pathPolicy = AppCacheCleanupPlanBuilder.ValidateForExecution(
            operation,
            CurrentUserDataRoots(),
            Directory.Exists,
            IsReparsePoint);
        if (!candidatePolicy.Success || !pathPolicy.Success)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                "缓存位置没有通过执行前安全校验，请重新扫描。"));
            return;
        }

        var quarantinePreparation = QuarantineOperationPolicy.PrepareForConfirmation(
            operation,
            DefaultQuarantineRoot(),
            _quarantineIdentityReader);
        if (!quarantinePreparation.Success || quarantinePreparation.Operation is null)
        {
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                "缓存位置无法绑定当前文件身份，请重新扫描。"));
            return;
        }
        var preparedOperation = quarantinePreparation.Operation;

        var confirmation = CleanupConfirmationPresenter.Create(preparedOperation, DefaultQuarantineRoot());
        var confirmationWindow = new CleanupConfirmationWindow(confirmation)
        {
            Owner = this
        };
        if (confirmationWindow.ShowDialog() != true)
        {
            StatusTextBlock.Text = "已取消缓存处理，没有移动任何文件。";
            return;
        }

        DrawerActionPreviewPrimaryButton.IsEnabled = false;
        StatusTextBlock.Text = "正在重新确认应用状态和缓存证据...";
        try
        {
            var currentProfiles = await ScanSoftwareProfilesAsync();
            SetSoftwareProfiles(currentProfiles);
            var resolution = AppDrawerTargetResolver.Resolve(targetAppName, _softwareProfiles);
            if (!resolution.CanOpen || resolution.Profile is null)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                    "无法唯一确认对应应用，旧方案已停止。"));
                return;
            }

            if (resolution.Profile.RunningProcesses.Count > 0)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                    "应用仍在运行，请先正常关闭它，再重新生成缓存方案。"));
                return;
            }

            if (!AppCacheCleanupPlanBuilder.MatchesCurrentProfile(resolution.Profile, preparedOperation))
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                    "应用画像中的缓存证据已经变化，旧方案已停止。"));
                return;
            }

            pathPolicy = AppCacheCleanupPlanBuilder.ValidateForExecution(
                preparedOperation,
                CurrentUserDataRoots(),
                Directory.Exists,
                IsReparsePoint);
            if (!pathPolicy.Success)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                    "缓存位置在确认后发生了变化，旧方案已停止。"));
                return;
            }

            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparedOperation);
            var handler = new AppCacheCleanupOperationHandler(
                _quarantineService,
                _timelineStore,
                CurrentUserDataRoots(),
                Directory.Exists,
                IsReparsePoint,
                _quarantineIdentityReader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(descriptor);
            await RefreshCacheCleanupStateAfterAttemptAsync();
            stateSynchronized = true;
            if (!result.Success)
            {
                ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                    "本地安全管线没有完整完成；请到后悔药中心复查记录。"));
                return;
            }

            ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupCompleted(
                result.Summary ?? "缓存已移动到隔离区。"));
            StatusTextBlock.Text = "缓存已移动到隔离区，可以在后悔药中心还原。";
        }
        catch
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await RefreshCacheCleanupStateAfterAttemptAsync();
                stateSynchronized = true;
            }
            ApplyDrawerActionHost(AppDrawerActionHostPresenter.CacheCleanupRefused(
                "处理过程中出现异常，未确认完成；请重新扫描并查看后悔药中心。"));
        }
        finally
        {
            if (DrawerActionPreviewPrimaryButton.Visibility == Visibility.Visible)
                DrawerActionPreviewPrimaryButton.IsEnabled = true;
        }
    }

    private async void ReviewUninstallResidue_Click(object sender, RoutedEventArgs e)
    {
        await ReviewSelectedUninstallResidueAsync();
    }

    private void ToggleTechnicalDetails_Click(object sender, RoutedEventArgs e)
    {
        var state = AppDrawerTechnicalDetailsPresenter.Toggle(DrawerTechnicalListBox.Visibility == Visibility.Visible);
        ApplyDrawerTechnicalDetailsState(state, sender as Button);
    }

    private void ApplyDrawerTechnicalDetailsState(AppDrawerTechnicalDetailsState state, Button? sourceButton)
    {
        DrawerTechnicalListBox.Visibility = state.IsVisible
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (sourceButton is not null)
            sourceButton.Content = state.ButtonText;

        if (!string.IsNullOrWhiteSpace(state.StatusText))
            StatusTextBlock.Text = state.StatusText;
    }

    private void ToggleTechnicalReport_Click(object sender, RoutedEventArgs e)
    {
        var isVisible = ReportTextBox.Visibility == Visibility.Visible;
        ReportTextBox.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;
        ToggleTechnicalReportButton.Content = isVisible
            ? "\u663e\u793a\u6280\u672f\u62a5\u544a"
            : "\u9690\u85cf\u6280\u672f\u62a5\u544a";
    }

    private void ShowPage(string page)
    {
        HomePage.Visibility = page == "Home" ? Visibility.Visible : Visibility.Collapsed;
        AppsPage.Visibility = page == "Apps" ? Visibility.Visible : Visibility.Collapsed;
        CDrivePage.Visibility = page == "CDrive" ? Visibility.Visible : Visibility.Collapsed;
        InstallPage.Visibility = page == "Install" ? Visibility.Visible : Visibility.Collapsed;
        TimelinePage.Visibility = page == "Timeline" ? Visibility.Visible : Visibility.Collapsed;
        AgentPage.Visibility = page == "Agent" ? Visibility.Visible : Visibility.Collapsed;

        SetNavSelected(HomeNavButton, page == "Home");
        SetNavSelected(AppsNavButton, page == "Apps");
        SetNavSelected(CDriveNavButton, page == "CDrive");
        SetNavSelected(InstallNavButton, page == "Install");
        SetNavSelected(TimelineNavButton, page == "Timeline");
        SetNavSelected(AgentNavButton, page == "Agent");

        (PageTitleTextBlock.Text, PageSubtitleTextBlock.Text) = page switch
        {
            "Apps" => ("应用管理", "像应用中心一样查看软件，点击图标后再看结论和可做动作。"),
            "CDrive" => ("C盘清理", "先看谁占空间，再让 Agent 生成安全处理方案。"),
            "Install" => ("安装管控", "新软件优先建议安装到 D 盘指定目录，V1 不强行拦截安装器。"),
            "Timeline" => ("后悔药中心", "所有改动都应该留下证据、影响范围和还原入口。"),
            "Agent" => ("AI Agent", "Computer Agent 负责解释和判断，真正执行仍必须经过本地安全管线。"),
            _ => ("首页体检", "先看结论，再决定要不要处理。")
        };

        if (page == "Timeline")
            _ = EnsureTimelineLoadedAsync();
        if (page == "Apps")
            _ = EnsureSoftwareInventoryLoadedAsync();
    }

    private static void SetNavSelected(Button button, bool selected)
    {
        button.Background = BrushFrom(selected ? "#1F2937" : "Transparent");
        button.Foreground = BrushFrom(selected ? "#FFFFFF" : "#D1D5DB");
        button.FontWeight = selected ? FontWeights.SemiBold : FontWeights.Normal;
    }

    private void LoadAgentSkills()
    {
        AgentSkillListBox.ItemsSource = AgentSkillCardPresenter.CreateDefault()
            .Select(AgentSkillView.From)
            .ToList();
    }

    private void LoadSystemToolShortcuts()
    {
        AgentSystemToolListBox.ItemsSource = SystemToolShortcutCatalog.CreateDefault()
            .Select(SystemToolShortcutView.From)
            .ToList();
    }

    private void LoadWindowsSettingsShortcuts()
    {
        AgentWindowsSettingsListBox.ItemsSource = WindowsSettingsShortcutCatalog.CreateDefault()
            .Select(WindowsSettingsShortcutView.From)
            .ToList();
    }

    private void LoadAgentNextSteps()
    {
        var panel = AgentNextStepPresenter.Create(_lastHealthSummary, _softwareProfiles);
        AgentNextStepTitleTextBlock.Text = panel.Title;
        AgentNextStepSummaryTextBlock.Text = panel.Summary;
        AgentNextStepReasonsListBox.ItemsSource = panel.Reasons;
        AgentNextStepActionsListBox.ItemsSource = panel.SafeNextActions;
        AgentNextStepActionButtonsItemsControl.ItemsSource = panel.NavigationActions;
        AgentBlockedActionsListBox.ItemsSource = panel.BlockedActions;
        AgentNextStepSafetyTextBlock.Text = panel.SafetyBoundary;
        AgentNextStepPrivacyTextBlock.Text = panel.PrivacyLine;

        var backgroundReview = AgentBackgroundReviewPresenter.Create(_softwareProfiles);
        AgentBackgroundReviewPanel.Visibility = backgroundReview.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        AgentBackgroundReviewSummaryTextBlock.Text = backgroundReview.Summary;
        AgentBackgroundReviewItemsListBox.ItemsSource = backgroundReview.Items;
        AgentBackgroundReviewSafetyTextBlock.Text = backgroundReview.SafetyLine;

        var startupServicePlan = AgentStartupServicePlanPresenter.Create(_softwareProfiles);
        AgentStartupServicePlanPanel.Visibility = startupServicePlan.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        AgentStartupServicePlanTitleTextBlock.Text = startupServicePlan.Title;
        AgentStartupServicePlanSummaryTextBlock.Text = startupServicePlan.Summary;
        AgentStartupServicePlanStepsListBox.ItemsSource = startupServicePlan.PlanSteps;
        AgentStartupServicePlanSafetyTextBlock.Text = startupServicePlan.SafetyLine;
    }

    private async void OpenAgentBackgroundApp_Click(object sender, RoutedEventArgs e)
    {
        var targetAppName = (sender as FrameworkElement)?.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(targetAppName))
        {
            StatusTextBlock.Text =
                "\u8fd9\u6761\u540e\u53f0\u7ebf\u7d22\u6ca1\u6709\u53ef\u9760\u7684\u5e94\u7528\u5f52\u5c5e\uff0c\u5df2\u4fdd\u6301\u53ea\u8bfb\u3002";
            return;
        }

        var resolution = await ResolveAndOpenAppTargetAsync(targetAppName);
        if (!resolution.CanOpen)
        {
            StatusTextBlock.Text =
                "\u6682\u65f6\u65e0\u6cd5\u552f\u4e00\u5b9a\u4f4d\u8fd9\u4e2a\u5e94\u7528\uff1b\u6ca1\u6709\u6253\u5f00\u6216\u5904\u7406\u4efb\u4f55\u540e\u53f0\u9879\u3002";
        }
    }

    private async void AskComputerAgent_Click(object sender, RoutedEventArgs e)
    {
        var question = AgentQuestionTextBox.Text;
        ApplicationCrashObservation? applicationCrashObservation = null;
        ApplicationRuntimeObservation? applicationRuntimeObservation = null;
        ApplicationGrowthObservation? applicationGrowthObservation = null;
        AskComputerAgentButton.IsEnabled = false;
        try
        {
            if (AgentConversationPresenter.QuestionNeedsSoftwareInventory(question, _lastHealthSummary))
            {
                StatusTextBlock.Text = "Computer Agent 正在自动读取应用和后台线索...";
                await EnsureSoftwareInventoryLoadedAsync();
            }

            if (AgentConversationPresenter.QuestionNeedsFullHealthScan(question, _lastHealthSummary))
            {
                StatusTextBlock.Text = "Computer Agent 正在自动完成只读电脑体检，请稍候...";
                await EnsureHealthScanLoadedAsync();
            }

            if (AgentConversationPresenter.QuestionNeedsMachineObservation(
                    question,
                    _lastHealthSummary,
                    _latestMachineHealthObservation))
            {
                StatusTextBlock.Text = "Computer Agent 正在只读读取电脑状态...";
                await EnsureMachineObservationLoadedAsync();
            }

            var applicationGrowthTarget =
                AgentConversationPresenter.ApplicationGrowthObservationTarget(
                    question,
                    _softwareProfiles);
            if (applicationGrowthTarget is not null)
            {
                if (_latestObservedSnapshotCount == 0)
                {
                    StatusTextBlock.Text = "Computer Agent 正在建立只读增长基线，请稍候...";
                    await EnsureHealthScanLoadedAsync();
                }

                applicationGrowthTarget =
                    AgentConversationPresenter.ApplicationGrowthObservationTarget(
                        question,
                        _softwareProfiles);
                if (applicationGrowthTarget is not null)
                {
                    applicationGrowthObservation = CreateApplicationGrowthObservation(
                        applicationGrowthTarget,
                        _latestObservedSnapshotCount);
                }
            }

            var applicationCrashTarget =
                AgentConversationPresenter.ApplicationCrashObservationTarget(
                    question,
                    _softwareProfiles);
            if (applicationCrashTarget is not null)
            {
                StatusTextBlock.Text = "Computer Agent 正在只读查看最近的应用错误记录...";
                applicationCrashObservation =
                    await ObserveApplicationCrashAsync(applicationCrashTarget);
            }

            var applicationRuntimeTarget =
                AgentConversationPresenter.ApplicationRuntimeObservationTarget(
                    question,
                    _softwareProfiles);
            if (applicationRuntimeTarget is not null)
            {
                StatusTextBlock.Text = "Computer Agent 正在只读采样这个应用的当前资源状态...";
                applicationRuntimeObservation =
                    await ObserveApplicationRuntimeAsync(applicationRuntimeTarget);
            }

            var reply = AgentConversationPresenter.Answer(
                question,
                _lastHealthSummary,
                _softwareProfiles,
                _latestMachineHealthObservation,
                applicationCrashObservation,
                applicationRuntimeObservation,
                applicationGrowthObservation: applicationGrowthObservation);
            ApplyAgentConversationReply(reply);
        }
        finally
        {
            AskComputerAgentButton.IsEnabled = true;
        }
    }

    private void ApplyAgentConversationReply(AgentConversationReply reply)
    {
        AgentConversationHeadlineTextBlock.Text = reply.Headline;
        AgentConversationAnswerTextBlock.Text = reply.Answer;
        AgentConversationEvidenceListBox.ItemsSource = reply.EvidenceLines;
        AgentConversationNextStepsListBox.ItemsSource = reply.NextSteps;
        AgentConversationSafetyTextBlock.Text = reply.SafetyBoundary;
        AgentConversationPrivacyTextBlock.Text = reply.PrivacyLine;
        AgentConversationNavigateButton.Content = reply.NavigationLabel;
        AgentConversationNavigateButton.Tag = reply;
        AgentConversationNavigateButton.Visibility = reply.CanNavigate
            && !string.IsNullOrWhiteSpace(reply.NavigationLabel)
                ? Visibility.Visible
                : Visibility.Collapsed;
        AgentConversationResponsePanel.Visibility = Visibility.Visible;
        AgentConversationScrollViewer.ScrollToTop();
        StatusTextBlock.Text = "Computer Agent 已根据当前本地摘要回答；没有执行系统修改。";
    }

    private async void AgentSkillAction_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not AgentSkillCategory category)
        {
            StatusTextBlock.Text = "这张技能卡片没有可识别的本地能力。";
            return;
        }

        var sourceButton = sender as Button;
        if (sourceButton is not null)
            sourceButton.IsEnabled = false;
        try
        {
            if (AgentConversationPresenter.SkillNeedsSoftwareInventory(category))
            {
                StatusTextBlock.Text = "Computer Agent 正在自动读取应用和后台线索...";
                await EnsureSoftwareInventoryLoadedAsync();
            }

            if (AgentConversationPresenter.SkillNeedsHealthScan(category, _lastHealthSummary))
            {
                StatusTextBlock.Text = "Computer Agent 正在自动完成只读电脑体检，请稍候...";
                await EnsureHealthScanLoadedAsync();
            }

            if (AgentConversationPresenter.SkillNeedsMachineObservation(
                    category,
                    _lastHealthSummary,
                    _latestMachineHealthObservation))
            {
                StatusTextBlock.Text = "Computer Agent 正在只读读取电脑配置...";
                await EnsureMachineObservationLoadedAsync();
            }

            var reply = AgentConversationPresenter.ExplainSkill(
                category,
                _lastHealthSummary,
                _softwareProfiles,
                _latestMachineHealthObservation);
            ApplyAgentConversationReply(reply);
        }
        finally
        {
            if (sourceButton is not null)
                sourceButton.IsEnabled = true;
        }
    }

    private async void AgentConversationNavigate_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not AgentConversationReply reply
            || !reply.CanNavigate)
        {
            StatusTextBlock.Text = "这条回答没有安全的下一步入口。";
            return;
        }

        if (!string.IsNullOrWhiteSpace(reply.TargetAppName))
        {
            var resolution = await ResolveAndOpenAppTargetAsync(reply.TargetAppName);
            if (!resolution.CanOpen || resolution.Profile is null)
            {
                ApplyAgentConversationReply(AgentConversationPresenter.TargetUnavailable());
                return;
            }

            await OpenAgentApplicationHandoffAsync(reply.TargetAppHandoff, resolution.Profile);
            return;
        }

        if (reply.ShortcutKind is not null)
        {
            if (string.IsNullOrWhiteSpace(reply.ShortcutId))
            {
                StatusTextBlock.Text = "Agent 的工具入口证据不完整，已拦截。";
                return;
            }

            switch (reply.ShortcutKind)
            {
                case AgentShortcutKind.WindowsSettings:
                    OpenAllowlistedWindowsSettings(reply.ShortcutId);
                    return;
                case AgentShortcutKind.SystemTool:
                    OpenAllowlistedSystemTool(reply.ShortcutId);
                    return;
                default:
                    StatusTextBlock.Text = "Agent 的工具入口不在白名单中，已拦截。";
                    return;
            }
        }

        if (reply.TargetAppFilter is { } appFilter)
        {
            if (!string.Equals(reply.NavigationTargetPage, "Apps", StringComparison.Ordinal))
            {
                StatusTextBlock.Text =
                    "Agent \u7684\u5e94\u7528\u7b5b\u9009\u4e0a\u4e0b\u6587\u4e0e\u9875\u9762\u4e0d\u4e00\u81f4\uff0c\u5df2\u62e6\u622a\u3002";
                return;
            }

            await OpenAgentAppCatalogFilterAsync(appFilter);
            return;
        }

        if (string.IsNullOrWhiteSpace(reply.NavigationTargetPage)
            || !IsAgentNavigationTarget(reply.NavigationTargetPage))
        {
            StatusTextBlock.Text = "Agent 的下一步不在内部页面白名单中，已拦截。";
            return;
        }

        ShowPage(reply.NavigationTargetPage);
        StatusTextBlock.Text = "Agent 已打开建议页面；没有自动执行任何处理。";
    }

    private async Task OpenAgentAppCatalogFilterAsync(AppCatalogFilter filter)
    {
        if (filter is not (AppCatalogFilter.Resident
            or AppCatalogFilter.CDrive
            or AppCatalogFilter.Uninstallable))
        {
            StatusTextBlock.Text =
                "Agent \u7684\u5e94\u7528\u7b5b\u9009\u4e0d\u5728\u5f53\u524d\u767d\u540d\u5355\u4e2d\uff0c\u5df2\u62e6\u622a\u3002";
            return;
        }

        ShowPage("Apps");
        _appCatalogFilter = filter;
        AppSearchTextBox.Text = string.Empty;
        SetAppFilterSelected();
        await EnsureSoftwareInventoryLoadedAsync();
        RefreshAppCatalog();
        AppTilesListBox.BringIntoView();
        if (!_softwareInventoryLoadGate.HasCompletedLoad)
        {
            StatusTextBlock.Text = "暂时无法读取应用列表；没有修改电脑，可以稍后重新扫描。";
            return;
        }

        var hasVisibleApplications = AppTilesListBox.Items.Count > 0;
        StatusTextBlock.Text = filter switch
        {
            AppCatalogFilter.Resident when !hasVisibleApplications =>
                "应用读取已完成，当前没有发现后台常驻应用。",
            AppCatalogFilter.Resident =>
                "已打开后台常驻应用；请先选择应用查看原因，没有自动关闭任何内容。",
            AppCatalogFilter.CDrive when !hasVisibleApplications =>
                "应用读取已完成，当前没有发现占 C 盘的应用线索。",
            AppCatalogFilter.CDrive =>
                "已打开占 C 盘应用；请选择一个应用，看它适合迁主程序、只迁缓存，还是不建议移动。没有自动迁移任何内容。",
            AppCatalogFilter.Uninstallable when !hasVisibleApplications =>
                "应用读取已完成，当前没有发现可在 OMNIX 中审核卸载的普通应用。",
            AppCatalogFilter.Uninstallable =>
                "已打开具备可审核官方卸载入口的普通应用；请逐个查看保留或卸载建议。没有自动卸载任何软件。",
            _ => "Agent 的应用筛选不在当前白名单中，已拦截。"
        };
    }

    private async Task OpenAgentApplicationHandoffAsync(
        AgentApplicationHandoff handoff,
        SoftwareProfile profile)
    {
        switch (handoff)
        {
            case AgentApplicationHandoff.Details:
                return;
            case AgentApplicationHandoff.UninstallReview:
                await ShowUninstallPlanAsync(profile);
                return;
            case AgentApplicationHandoff.MigrationReview:
                await ShowMigrationPlanAsync(profile);
                return;
            case AgentApplicationHandoff.CacheCleanupReview:
                ShowCacheCleanupPreview(profile);
                return;
            case AgentApplicationHandoff.StartupControlReview:
                await ShowStartupControlPreviewAsync(profile);
                return;
            default:
                StatusTextBlock.Text = "Agent 的应用方案类型不在安全白名单中，已停止。";
                return;
        }
    }

    private async void AgentNextAction_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not AgentNextActionViewModel action
            || !action.IsNavigationOnly
            || !IsAgentNavigationTarget(action.TargetPage))
        {
            StatusTextBlock.Text = "Agent 只会打开 OMNIX-Entropy 内部页面，不会执行系统修改。";
            return;
        }

        if (action.TargetAppFilter is { } appFilter)
        {
            if (!string.Equals(action.TargetPage, "Apps", StringComparison.Ordinal))
            {
                StatusTextBlock.Text = "Agent 的应用筛选上下文与页面不一致，已拦截。";
                return;
            }

            await OpenAgentAppCatalogFilterAsync(appFilter);
            return;
        }

        ShowPage(action.TargetPage);
        StatusTextBlock.Text = "Agent 已打开建议页面。这里仍然只是查看和确认，不会自动修改系统。";
    }

    private static bool IsAgentNavigationTarget(string targetPage) =>
        targetPage is "Home" or "Apps" or "CDrive" or "Install" or "Timeline" or "Agent";

    private async void OpenCDriveRootCauseAction_Click(object sender, RoutedEventArgs e)
    {
        var element = sender as FrameworkElement;
        if (element?.DataContext is not CDriveRootCauseCard card
            || element.Tag is not CDriveRootCauseAction action
            || action == CDriveRootCauseAction.None
            || card.Action != action
            || !card.HasAction)
        {
            StatusTextBlock.Text = "这个 C 盘建议没有可用的安全查看入口，已拦截。";
            return;
        }

        switch (action)
        {
            case CDriveRootCauseAction.OpenRecycleBin:
                OpenAllowlistedSystemTool(SystemToolShortcutCatalog.RecycleBinId);
                return;

            case CDriveRootCauseAction.OpenCDriveApps:
                await OpenAgentAppCatalogFilterAsync(AppCatalogFilter.CDrive);
                return;

            case CDriveRootCauseAction.ReviewPersonalStorage:
                PersonalStorageFindingsListBox.BringIntoView();
                PersonalStorageFindingsListBox.Focus();
                StatusTextBlock.Text = PersonalStorageFindingsListBox.Items.Count == 0
                    ? "本次没有发现达到阈值的大文件或疑似重复文件；没有生成个人文件处理操作。"
                    : "已定位到大文件和疑似重复文件候选；这里只读查看，不会删除或移动。";
                return;

            case CDriveRootCauseAction.ReviewCleanupRecommendations:
                var candidate = RecommendationsListBox.Items
                    .OfType<RecommendationCardViewModel>()
                    .FirstOrDefault(item => item.CanExecute && item.Operation is not null);
                RecommendationsListBox.BringIntoView();
                if (candidate is null)
                {
                    RecommendationsListBox.Focus();
                    StatusTextBlock.Text = "本次没有通过安全策略的可清理项；不会把普通临时目录说明当成清理授权。";
                    return;
                }

                RecommendationsListBox.SelectedItem = candidate;
                RecommendationsListBox.ScrollIntoView(candidate);
                RecommendationsListBox.Focus();
                StatusTextBlock.Text = "已选中一条可安全审核的清理建议；现在只是预览，继续处理仍需二次确认。";
                return;

            default:
                StatusTextBlock.Text = "这个 C 盘建议不在内部安全入口白名单中，已拦截。";
                return;
        }
    }

    private void OpenSystemTool_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not string toolId)
        {
            StatusTextBlock.Text = "\u672a\u8bc6\u522b\u7684\u7cfb\u7edf\u5de5\u5177\u5165\u53e3\u3002";
            return;
        }

        OpenAllowlistedSystemTool(toolId);
    }

    private void InspectPersonalStorageFinding_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not PersonalStorageFindingViewModel finding
            || !finding.CanInspectLocations)
        {
            StatusTextBlock.Text =
                "\u8fd9\u6761\u5019\u9009\u6ca1\u6709\u53ef\u590d\u6838\u7684\u6587\u4ef6\u4f4d\u7f6e\uff1b\u6ca1\u6709\u6253\u5f00\u6216\u4fee\u6539\u4efb\u4f55\u5185\u5bb9\u3002";
            return;
        }

        var window = new PersonalStorageInspectionWindow(finding)
        {
            Owner = this
        };
        window.ShowDialog();
        if (window.RequestedEvidencePath is null)
        {
            StatusTextBlock.Text =
                "\u5df2\u5173\u95ed\u4f4d\u7f6e\u67e5\u770b\uff1b\u6ca1\u6709\u6253\u5f00\u3001\u79fb\u52a8\u6216\u5220\u9664\u6587\u4ef6\u3002";
            return;
        }

        var result = _personalStorageExplorerLauncher.TryOpenSelectedLocation(
            window.RequestedEvidencePath,
            _personalStorageEvidencePaths);
        StatusTextBlock.Text = result.Message;
    }

    private void OpenAllowlistedSystemTool(string toolId)
    {

        var shortcut = SystemToolShortcutCatalog.FindById(toolId);
        if (shortcut is null)
        {
            StatusTextBlock.Text = "\u8fd9\u4e2a\u7cfb\u7edf\u5de5\u5177\u4e0d\u5728 OMNIX-Entropy \u767d\u540d\u5355\u4e2d\uff0c\u5df2\u62e6\u622a\u3002";
            return;
        }

        if (shortcut.RequiresConfirmation)
        {
            var confirm = MessageBox.Show(
                this,
                $"{shortcut.Name}\n\n{shortcut.SafetyHint}\n\nOMNIX-Entropy \u53ea\u4f1a\u6253\u5f00\u8fd9\u4e2a Windows \u5de5\u5177\uff0c\u4e0d\u4f1a\u4ee3\u66ff\u4f60\u70b9\u51fb\u6216\u4fee\u6539\u7cfb\u7edf\u3002",
                "\u786e\u8ba4\u6253\u5f00\u7cfb\u7edf\u5de5\u5177",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.OK)
            {
                StatusTextBlock.Text = "\u5df2\u53d6\u6d88\uff0c\u6ca1\u6709\u6253\u5f00\u7cfb\u7edf\u5de5\u5177\u3002";
                return;
            }
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = shortcut.Command,
                Arguments = shortcut.Arguments ?? string.Empty,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            StatusTextBlock.Text = $"\u5df2\u6253\u5f00 {shortcut.Name}\uff1bOMNIX-Entropy \u6ca1\u6709\u4fee\u6539\u7cfb\u7edf\u3002";
        }
        catch (Exception)
        {
            StatusTextBlock.Text =
                $"打开 {shortcut.Name} 失败；没有修改系统。可以稍后重试，或从 Windows 设置中手动打开。";
        }
    }

    private void OpenInstallerStorageSettings_Click(object sender, RoutedEventArgs e)
    {
        if (_lastInstallerCapability is not { Mode: InstallerRoutingCapabilityMode.WindowsManagedStorage }
            || !string.Equals(
                _lastInstallerCapability.SettingsShortcutId,
                InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId,
                StringComparison.Ordinal))
        {
            StatusTextBlock.Text = "当前没有可信的 Windows 应用包分析结果，已停止打开设置。";
            return;
        }

        OpenAllowlistedWindowsSettings(
            InstallerRoutingCapabilityPolicy.WindowsManagedStorageShortcutId);
    }

    private void OpenWindowsSettings_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not string settingId)
        {
            StatusTextBlock.Text = "\u672a\u8bc6\u522b\u7684 Windows \u8bbe\u7f6e\u5165\u53e3\u3002";
            return;
        }

        OpenAllowlistedWindowsSettings(settingId);
    }

    private void OpenAllowlistedWindowsSettings(string settingId)
    {
        var shortcut = WindowsSettingsShortcutCatalog.FindById(settingId);
        if (shortcut is null)
        {
            StatusTextBlock.Text = "\u8fd9\u4e2a Windows \u8bbe\u7f6e\u5165\u53e3\u4e0d\u5728 OMNIX-Entropy \u767d\u540d\u5355\u4e2d\uff0c\u5df2\u62e6\u622a\u3002";
            return;
        }

        OpenWindowsSettingsShortcut(shortcut);
    }

    private void OpenWindowsSettingsShortcut(WindowsSettingsShortcut shortcut)
    {
        if (!shortcut.IsOpenOnly
            || !shortcut.Uri.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase))
        {
            StatusTextBlock.Text = "已拦截不是纯打开操作的 Windows 设置入口。";
            return;
        }

        if (shortcut.RequiresConfirmation)
        {
            var confirm = MessageBox.Show(
                this,
                $"{shortcut.Name}\n\n{shortcut.SafetyHint}\n\nOMNIX-Entropy \u53ea\u4f1a\u6253\u5f00\u8fd9\u4e2a Windows \u8bbe\u7f6e\u9875\uff0c\u4e0d\u4f1a\u4ee3\u66ff\u4f60\u70b9\u51fb\u3001\u5378\u8f7d\u3001\u5220\u9664\u6216\u4fee\u6539\u914d\u7f6e\u3002",
                "\u786e\u8ba4\u6253\u5f00 Windows \u8bbe\u7f6e",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.OK)
            {
                StatusTextBlock.Text = "\u5df2\u53d6\u6d88\uff0c\u6ca1\u6709\u6253\u5f00 Windows \u8bbe\u7f6e\u9875\u3002";
                return;
            }
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = shortcut.Uri,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            StatusTextBlock.Text = $"\u5df2\u6253\u5f00 {shortcut.Name}\uff1bOMNIX-Entropy \u6ca1\u6709\u4fee\u6539\u4efb\u4f55\u8bbe\u7f6e\u3002";
        }
        catch
        {
            StatusTextBlock.Text = $"打开 {shortcut.Name} 失败；没有修改任何设置。";
        }
    }

    private Task EnsureHealthScanLoadedAsync() =>
        _healthScanLoadGate.EnsureLoadedAsync(RunHealthScanCoreAsync);

    private Task RefreshHealthScanAsync() =>
        _healthScanLoadGate.RefreshAsync(RunHealthScanCoreAsync);

    private async Task<bool> RunHealthScanCoreAsync()
    {
        var driveRoot = NormalizeDriveRoot(DriveRootComboBox.SelectedItem?.ToString() ?? "C:\\");
        var scanRoot = AppDevelopmentPathResolver.ResolveCDriveScanRoot(driveRoot);
        var rulesPath = Path.Combine(AppContext.BaseDirectory, "rules.scan.json");
        _scanCts?.Dispose();
        _scanCts = new CancellationTokenSource();
        var ct = _scanCts.Token;

        StartScanButton.IsEnabled = false;
        CancelScanButton.IsEnabled = true;
        ReportTextBox.Text = "扫描中，请稍候...";
        CDriveRootCauseListBox.ItemsSource = null;
        RecommendationsListBox.ItemsSource = null;
        ApplyRecommendationSelection(RecommendationSelectionPresenter.Create(null));
        RecommendationActionTextBlock.Text = "\u6b63\u5728\u626b\u63cf\uff0c\u5b8c\u6210\u540e\u4f1a\u663e\u793a\u53ef\u7406\u89e3\u7684\u5904\u7406\u9884\u6848\u3002";
        GrowthListBox.ItemsSource = null;
        _personalStorageEvidencePaths = [];
        PersonalStorageSummaryTextBlock.Text = "正在分析个人文件夹中的大文件和疑似重复文件...";
        PersonalStorageFindingsListBox.ItemsSource = null;
        CDriveRootCauseStateTextBlock.Text = "正在只读扫描空间占用，不会清理文件。";
        RecommendationsEmptyStateTextBlock.Text = "正在只读分析，完成后才会显示处理建议。";
        SetCDriveResultVisibility(false, false, false, false);
        StatusTextBlock.Text = "开始只读扫描 " + driveRoot;

        try
        {
            var previousSnapshots = await _snapshotStore.LoadRecentAsync(
                scanRoot,
                ScanSnapshotStore.DefaultTrendSnapshotCount,
                ct);
            var previous = previousSnapshots.FirstOrDefault();
            var progress = new Progress<string>(message => StatusTextBlock.Text = "扫描中: " + message);
            var result = await _scanner.ScanAsync(scanRoot, File.Exists(rulesPath) ? rulesPath : null, progress, ct);
            var attributionProfiles = _softwareProfiles;
            var softwareInventoryAvailable = _softwareInventoryLoadGate.HasCompletedLoad;
            if (!softwareInventoryAvailable)
            {
                StatusTextBlock.Text = "正在只读关联软件与 C 盘写入位置...";
                try
                {
                    attributionProfiles = await ScanSoftwareProfilesAsync();
                    SetSoftwareProfiles(
                        attributionProfiles,
                        refreshCatalog: false,
                        refreshAgent: false);
                    _softwareInventoryLoadGate.MarkLoaded();
                    softwareInventoryAvailable = true;
                }
                catch
                {
                    attributionProfiles = [];
                    softwareInventoryAvailable = false;
                }
            }
            var personalStorageFixtureRoot =
                AppDevelopmentPathResolver.ResolvePersonalStorageFixtureRoot();
            IReadOnlyList<string>? personalStorageRoots = personalStorageFixtureRoot is null
                ? null
                : [personalStorageFixtureRoot];
            var personalStorageOptions = personalStorageFixtureRoot is null
                ? null
                : new PersonalStorageAnalysisOptions
                {
                    MinimumLargeFileBytes = 8 * 1024,
                    MinimumDuplicateFileBytes = 4 * 1024,
                    UnusedDays = 30
                };
            var session = DiskScanSessionBuilder.Build(
                result,
                previous,
                DateTimeOffset.Now,
                attributionProfiles,
                previousSnapshots,
                personalStorageRoots,
                personalStorageOptions);
            await _snapshotStore.SaveAsync(scanRoot, session.CurrentSnapshot, ct);

            StatusTextBlock.Text = "正在只读读取 D 盘、内存、进程、电池和硬件配置...";
            await RefreshMachineObservationAsync();
            ct.ThrowIfCancellationRequested();
            var machineHealth = _latestMachineHealthObservation
                ?? UnavailableMachineObservation();
            var closureAvailable = await RefreshMigrationClosureAsync(refreshUi: false, ct);
            var observedSnapshotCount = previousSnapshots
                .Select(snapshot => snapshot.CapturedAt)
                .Distinct()
                .Count() + 1;
            _latestObservedSnapshotCount = observedSnapshotCount;
            var summary = ApplySession(
                session,
                machineHealth,
                softwareInventoryAvailable ? attributionProfiles : null,
                observedSnapshotCount);
            var digestSaved = await TrySaveHealthDigestAsync(
                scanRoot,
                session.CurrentSnapshot,
                summary,
                ct);
            StatusTextBlock.Text = $"扫描完成: {session.CurrentSnapshot.Items.Count} 个监测位置，{session.Recommendations.Count} 张决策卡片，{session.PersonalStorage.Findings.Count} 条个人文件候选。" +
                (digestSaved ? " 体检摘要已保存。" : " 体检完成，但历史摘要暂未保存。") +
                (closureAvailable ? "" : " 迁移闭环记录暂时无法读取。");
            return true;
        }
        catch (OperationCanceledException)
        {
            StatusTextBlock.Text = "扫描已取消。";
            ReportTextBox.Text = "扫描已取消，没有执行任何清理或迁移动作。";
            CDriveRootCauseStateTextBlock.Text = "扫描已取消，没有清理任何文件；可以稍后重新体检。";
            RecommendationsEmptyStateTextBlock.Text = "扫描已取消，没有生成处理建议。";
            return false;
        }
        catch (Exception)
        {
            StatusTextBlock.Text = "体检没有完成；没有执行任何清理或迁移动作，可以稍后重试。";
            ReportTextBox.Text = "体检失败。底层错误和本机路径未显示；请稍后重试。";
            CDriveRootCauseStateTextBlock.Text = "体检没有完成，当前没有可用结果；可以稍后重试。";
            RecommendationsEmptyStateTextBlock.Text = "体检没有完成，没有生成处理建议。";
            return false;
        }
        finally
        {
            StartScanButton.IsEnabled = true;
            CancelScanButton.IsEnabled = false;
        }
    }

    private void SetSoftwareProfiles(
        IReadOnlyList<SoftwareProfile> profiles,
        bool refreshCatalog = true,
        bool refreshAgent = true)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        _softwareProfiles = SoftwareGrowthProfileEnricher.Apply(
            profiles,
            _latestGrowthFindings);
        AppHealthTextBlock.Text = $"{_softwareProfiles.Count} 个应用";
        if (refreshCatalog)
            RefreshAppCatalog();
        if (refreshAgent)
            LoadAgentNextSteps();
    }

    private HealthCheckSummary ApplySession(
        DiskScanSession session,
        MachineHealthObservation machineHealth,
        IReadOnlyList<SoftwareProfile>? healthProfiles,
        int observedSnapshotCount)
    {
        ReportTextBox.Text = session.Report;
        _latestGrowthFindings = session.GrowthFindings;
        SetSoftwareProfiles(_softwareProfiles, refreshAgent: false);
        var rootCauseSummary = CDriveRootCauseSummaryBuilder.Build(session.Result);
        CDriveSummaryHeadlineTextBlock.Text = rootCauseSummary.Headline;
        CDriveSummarySubheadlineTextBlock.Text = rootCauseSummary.Subheadline;
        CDriveRootCauseListBox.ItemsSource = rootCauseSummary.Cards;
        var hasRootCauseItems = rootCauseSummary.Cards.Count > 0;
        CDriveRootCauseStateTextBlock.Text =
            "本次体检完成，没有发现需要单独列出的主要占用来源。";

        _baseHealthSummary = HealthCheckSummaryBuilder.Build(
            session.Result,
            session.Recommendations,
            session.GrowthFindings,
            session.PersonalStorage,
            machineHealth,
            healthProfiles,
            observedSnapshotCount);
        var summary = RefreshHealthSummaryFromBase();
        if (summary is null)
            throw new InvalidOperationException("The health summary could not be created.");

        var growthItems = GrowthFindingPresenter.CreateList(session.GrowthFindings);
        GrowthListBox.ItemsSource = growthItems;
        var hasGrowthItems = growthItems.Count > 0;
        var firstGrowth = growthItems.FirstOrDefault(item => item.Finding is not null);
        ApplyGrowthDecision(GrowthDecisionPresenter.Create(firstGrowth?.Finding));
        GrowthListBox.SelectedItem = firstGrowth;
        var personalStorage = PersonalStorageFindingPresenter.Create(session.PersonalStorage);
        _personalStorageEvidencePaths = personalStorage.Items
            .SelectMany(item => item.EvidencePaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        PersonalStorageSummaryTextBlock.Text = personalStorage.Summary;
        PersonalStorageFindingsListBox.ItemsSource = personalStorage.Items;
        var hasPersonalStorageItems = personalStorage.Items.Count > 0;
        var recommendationList = RecommendationListPresenter.Create(session.Recommendations);
        RecommendationsListBox.ItemsSource = recommendationList.Cards;
        var hasRecommendationItems = recommendationList.Cards.Count > 0;
        RecommendationsEmptyStateTextBlock.Text =
            "本次体检没有发现可进入处理预案的项目。";
        SetCDriveResultVisibility(
            hasRootCauseItems,
            hasGrowthItems,
            hasPersonalStorageItems,
            hasRecommendationItems);
        ApplyRecommendationSelection(RecommendationSelectionPresenter.Create(null));
        RecommendationActionTextBlock.Text = recommendationList.ActionExplanationText;
        LoadAgentNextSteps();
        ExecuteRecommendationButton.Content = "选择可清理项后继续";
        return summary;
    }

    private void SetCDriveResultVisibility(
        bool hasRootCauseItems,
        bool hasGrowthItems,
        bool hasPersonalStorageItems,
        bool hasRecommendationItems)
    {
        CDriveRootCauseStateTextBlock.Visibility = hasRootCauseItems
            ? Visibility.Collapsed
            : Visibility.Visible;
        CDriveRootCauseListBox.Visibility = hasRootCauseItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        GrowthListBox.Visibility = hasGrowthItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        PersonalStorageFindingsListBox.Visibility = hasPersonalStorageItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        RecommendationsEmptyStateTextBlock.Visibility = hasRecommendationItems
            ? Visibility.Collapsed
            : Visibility.Visible;
        RecommendationsListBox.Visibility = hasRecommendationItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        RecommendationActionTextBlock.Visibility = hasRecommendationItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        RecommendationActionPanel.Visibility = hasRecommendationItems
            ? Visibility.Visible
            : Visibility.Collapsed;
        ExecuteRecommendationButton.Visibility = hasRecommendationItems
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private HealthCheckSummary? RefreshHealthSummaryFromBase()
    {
        if (_baseHealthSummary is null)
            return null;

        var summary = MigrationClosureHealthEnricher.Apply(
            _baseHealthSummary,
            _migrationClosureBySoftware.Values,
            ResolveMigrationClosureTargetDisposition);
        _lastHealthSummary = summary;
        OverallScoreTextBlock.Text = summary.OverallScore + " 分";
        HealthDimensionListView.ItemsSource = summary.Dimensions;
        KeyFindingsListBox.ItemsSource = summary.KeyFindings;
        var hasKeyFindings = summary.KeyFindings.Count > 0;
        KeyFindingsEmptyStateTextBlock.Text =
            "本次体检没有发现需要优先处理的项目。";
        KeyFindingsEmptyStateTextBlock.Visibility = hasKeyFindings
            ? Visibility.Collapsed
            : Visibility.Visible;
        KeyFindingsListBox.Visibility = hasKeyFindings
            ? Visibility.Visible
            : Visibility.Collapsed;
        DiskHealthTextBlock.Text = summary.Dimensions.FirstOrDefault(d => d.Name == "磁盘健康")?.Rating ?? "已完成";
        LoadAgentNextSteps();
        return summary;
    }

    private async Task<bool> TrySaveHealthDigestAsync(
        string driveRoot,
        ScanSnapshot snapshot,
        HealthCheckSummary summary,
        CancellationToken ct)
    {
        try
        {
            var digest = HealthDigestBuilder.Create(
                driveRoot,
                snapshot,
                summary,
                _softwareProfiles);
            await _healthDigestStore.SaveAsync(digest, ct);
            await LoadHealthDigestHistoryAsync(ct);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            await LoadHealthDigestHistoryAsync(CancellationToken.None);
            return false;
        }
    }

    private async Task LoadHealthDigestHistoryAsync(CancellationToken ct = default)
    {
        try
        {
            var digests = await _healthDigestStore.LoadRecentAsync(14, ct);
            ApplyHealthDigestHistory(
                HealthDigestHistoryPresenter.Create(digests, DateTimeOffset.Now));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            ApplyHealthDigestHistory(HealthDigestHistoryPresenter.Create([], DateTimeOffset.Now));
            HealthDigestLatestHeadlineTextBlock.Text = "体检历史暂时无法读取";
        }
    }

    private void ApplyHealthDigestHistory(HealthDigestHistoryViewModel history)
    {
        _healthDigestHistoryHasEvidence = history.HasEvidence;
        HealthDigestLatestHeadlineTextBlock.Text = history.LatestHeadline;
        HealthDigestLatestSummaryTextBlock.Text = history.LatestSummary;
        HealthDigestWeeklySummaryTextBlock.Text = history.WeeklySummary;
        HealthDigestMonitoringNoticeTextBlock.Text = history.MonitoringNotice;
        HealthDigestHistoryListBox.ItemsSource = history.DailyRows;
        OpenHealthDigestEvidenceButton.Content = _lastHealthSummary is null
            ? "\u91cd\u65b0\u4f53\u68c0\u5e76\u67e5\u770b\u5f53\u524d\u8bc1\u636e"
            : "\u67e5\u770b\u5f53\u524d C \u76d8\u8bc1\u636e";
        OpenHealthDigestEvidenceButton.IsEnabled =
            history.HasEvidence && !_isOpeningHealthDigestEvidence;
    }

    private async void OpenHealthDigestEvidence_Click(object sender, RoutedEventArgs e)
    {
        if (!OpenHealthDigestEvidenceButton.IsEnabled || _isOpeningHealthDigestEvidence)
            return;

        _isOpeningHealthDigestEvidence = true;
        OpenHealthDigestEvidenceButton.IsEnabled = false;
        OpenHealthDigestEvidenceButton.Content =
            "\u6b63\u5728\u8bfb\u53d6\u5f53\u524d C \u76d8\u8bc1\u636e";
        ShowPage("CDrive");
        StatusTextBlock.Text =
            "\u6b63\u5728\u8fdb\u884c\u53ea\u8bfb\u4f53\u68c0\uff0c\u5b8c\u6210\u540e\u518d\u663e\u793a\u5f53\u524d C \u76d8\u8bc1\u636e...";

        try
        {
            await EnsureHealthScanLoadedAsync();
            if (!_healthScanLoadGate.HasCompletedLoad || _lastHealthSummary is null)
            {
                StatusTextBlock.Text =
                    "\u5f53\u524d C \u76d8\u8bc1\u636e\u6ca1\u6709\u66f4\u65b0\u5b8c\u6210\uff1b\u9996\u9875\u4ecd\u53ef\u67e5\u770b\u5386\u53f2\u6458\u8981\uff0c\u4f46\u4e0d\u80fd\u628a\u5b83\u5f53\u4f5c\u5f53\u524d\u660e\u7ec6\u3002";
                return;
            }

            StatusTextBlock.Text =
                "\u5f53\u524d C \u76d8\u8bc1\u636e\u5df2\u6253\u5f00\uff1b\u8fd9\u662f\u53ea\u8bfb\u4f53\u68c0\u7ed3\u679c\uff0c\u6ca1\u6709\u81ea\u52a8\u6267\u884c\u4efb\u4f55\u5904\u7406\u3002";
        }
        catch
        {
            StatusTextBlock.Text =
                "\u5f53\u524d C \u76d8\u8bc1\u636e\u6682\u65f6\u65e0\u6cd5\u8bfb\u53d6\uff1b\u5386\u53f2\u6458\u8981\u4ecd\u4fdd\u7559\uff0c\u6ca1\u6709\u4fee\u6539\u7535\u8111\u3002";
        }
        finally
        {
            _isOpeningHealthDigestEvidence = false;
            OpenHealthDigestEvidenceButton.Content = _lastHealthSummary is null
                ? "\u91cd\u65b0\u4f53\u68c0\u5e76\u67e5\u770b\u5f53\u524d\u8bc1\u636e"
                : "\u67e5\u770b\u5f53\u524d C \u76d8\u8bc1\u636e";
            OpenHealthDigestEvidenceButton.IsEnabled = _healthDigestHistoryHasEvidence;
        }
    }

    private void GrowthListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var finding = (GrowthListBox.SelectedItem as GrowthFindingViewModel)?.Finding;
        ApplyGrowthDecision(GrowthDecisionPresenter.Create(finding));
    }

    private void ApplyGrowthDecision(GrowthDecisionViewModel decision)
    {
        GrowthDecisionHeadlineTextBlock.Text = decision.Headline;
        GrowthDecisionEvidenceTextBlock.Text = decision.EvidenceText;
        GrowthDecisionOneTimeTextBlock.Text = decision.OneTimeAction;
        GrowthDecisionPreventionTextBlock.Text = decision.PreventionAction;
        GrowthDecisionSafetyTextBlock.Text = decision.SafetyText;
        OpenGrowthAppButton.Tag = decision.TargetAppName;
        OpenGrowthAppButton.Visibility = decision.CanOpenApp
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void OpenGrowthApp_Click(object sender, RoutedEventArgs e)
    {
        var targetAppName = (sender as FrameworkElement)?.Tag?.ToString();
        var resolution = await ResolveAndOpenAppTargetAsync(targetAppName);
        if (!resolution.CanOpen)
            ShowGrowthAppTargetFailure(resolution);
    }

    private void ShowGrowthAppTargetFailure(AppDrawerTargetResolution resolution)
    {
        GrowthDecisionHeadlineTextBlock.Text = resolution.Headline;
        GrowthDecisionEvidenceTextBlock.Text = resolution.Explanation;
        GrowthDecisionOneTimeTextBlock.Text = "现在：保留当前增长证据，不根据名称猜测应用。";
        GrowthDecisionPreventionTextBlock.Text = "以后：重新扫描应用画像后，可以再次尝试定位。";
        GrowthDecisionSafetyTextBlock.Text = resolution.SafetyBoundary;
        OpenGrowthAppButton.Tag = null;
        OpenGrowthAppButton.Visibility = Visibility.Collapsed;
        StatusTextBlock.Text = resolution.Headline;
    }

    private async Task<AppDrawerTargetResolution> ResolveAndOpenAppTargetAsync(
        string? targetAppName)
    {
        var resolution = AppDrawerTargetResolver.Resolve(targetAppName, _softwareProfiles);
        if (resolution.Status == AppDrawerTargetStatus.NotFound)
        {
            StatusTextBlock.Text = "正在只读刷新应用画像，再确认对应应用...";
            await RefreshSoftwareInventoryAsync();
            if (!_softwareInventoryLoadGate.HasCompletedLoad)
                return AppDrawerTargetResolver.InventoryUnavailable();
            resolution = AppDrawerTargetResolver.Resolve(targetAppName, _softwareProfiles);
        }

        if (!resolution.CanOpen || resolution.Profile is null)
            return resolution;

        OpenAppDrawerTarget(resolution.Profile);
        return resolution;
    }

    private void OpenAppDrawerTarget(SoftwareProfile profile)
    {
        ShowPage("Apps");
        _appCatalogFilter = AppCatalogFilter.All;
        SetAppFilterSelected();
        AppSearchTextBox.Text = profile.Name;
        RefreshAppCatalog(profile);
        DrawerTitleTextBlock.BringIntoView();
        StatusTextBlock.Text = $"已打开 {profile.Name} 的应用详情；这里只展示结论和预案，不会直接处理。";
    }

    private static Task<ApplicationRuntimeObservation> ObserveApplicationRuntimeAsync(
        SoftwareProfile profile) =>
        Task.Run(() => new WindowsApplicationRuntimeProbe().Observe(profile));

    private static ApplicationGrowthObservation CreateApplicationGrowthObservation(
        SoftwareProfile profile,
        int observedSnapshotCount) =>
        new()
        {
            Availability = observedSnapshotCount switch
            {
                <= 0 => ApplicationGrowthObservationAvailability.Unavailable,
                1 => ApplicationGrowthObservationAvailability.InsufficientBaseline,
                _ => ApplicationGrowthObservationAvailability.Available
            },
            SoftwareName = profile.Name,
            ObservedSnapshotCount = Math.Max(0, observedSnapshotCount),
            RecentGrowthBytes = Math.Max(0, profile.RecentGrowthBytes),
            CDriveWriteLocationCount = profile.CDriveWritePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            CacheLocationCount = profile.CachePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
        };

    private static Task<ApplicationCrashObservation> ObserveApplicationCrashAsync(
        SoftwareProfile profile) =>
        Task.Run(() => new WindowsApplicationCrashProbe().Observe(profile));

    private Task EnsureMachineObservationLoadedAsync() =>
        _machineObservationLoadGate.EnsureLoadedAsync(RunMachineObservationCoreAsync);

    private Task RefreshMachineObservationAsync() =>
        _machineObservationLoadGate.RefreshAsync(RunMachineObservationCoreAsync);

    private async Task<bool> RunMachineObservationCoreAsync()
    {
        try
        {
            _latestMachineHealthObservation = await Task.Run(
                () => new WindowsMachineHealthProbe().Observe());
            return true;
        }
        catch
        {
            _latestMachineHealthObservation = UnavailableMachineObservation();
            StatusTextBlock.Text = "这次没有读到电脑状态；没有修改任何设置，可以稍后重试。";
            return true;
        }
    }

    private static MachineHealthObservation UnavailableMachineObservation() =>
        new()
        {
            ObservedAtUtc = DateTimeOffset.UtcNow,
            SecondaryDrive = new LocalDriveHealthObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            },
            Memory = new MemoryHealthObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            },
            Battery = new BatteryHealthObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            },
            Hardware = new HardwareSummaryObservation
            {
                Availability = MachineMetricAvailability.Unavailable
            }
        };

    private Task EnsureSoftwareInventoryLoadedAsync() =>
        _softwareInventoryLoadGate.EnsureLoadedAsync(RunSoftwareScanCoreAsync);

    private Task RefreshSoftwareInventoryAsync() =>
        _softwareInventoryLoadGate.RefreshAsync(RunSoftwareScanCoreAsync);

    private async Task<bool> RunSoftwareScanCoreAsync()
    {
        var hadPreviousInventory = _softwareInventoryLoadGate.HasCompletedLoad
            || _softwareProfiles.Count > 0;
        ScanSoftwareButton.IsEnabled = false;
        AppTilesListBox.ItemsSource = new[] { AppTileUi.Message("扫描中") };
        AppsSummaryTextBlock.Text = "正在只读扫描应用、后台进程、服务、自启动和计划任务...";
        StatusTextBlock.Text = "正在只读扫描应用...";

        try
        {
            var profiles = await ScanSoftwareProfilesAsync();
            SetSoftwareProfiles(
                profiles,
                refreshCatalog: false,
                refreshAgent: false);
            var closureAvailable = await RefreshMigrationClosureAsync(refreshUi: true);
            StatusTextBlock.Text = closureAvailable
                ? $"应用扫描完成: {profiles.Count} 个应用，迁移闭环也已复查。"
                : $"应用扫描完成: {profiles.Count} 个应用；迁移闭环记录暂时无法读取。";
            if (profiles.Count == 0)
            {
                AppTilesListBox.ItemsSource = Array.Empty<AppTileUi>();
                AppsSummaryTextBlock.Text = "应用读取已完成，当前没有发现可管理的应用。";
            }
            return true;
        }
        catch
        {
            if (hadPreviousInventory)
            {
                RefreshAppCatalog();
                AppsSummaryTextBlock.Text = "重新扫描没有完成，已保留上一次应用列表。";
                StatusTextBlock.Text = "重新扫描没有完成；上一次结果仍然保留。";
            }
            else
            {
                AppsSummaryTextBlock.Text = "暂时无法读取应用列表；没有修改电脑，可以稍后重新扫描。";
                AppTilesListBox.ItemsSource = Array.Empty<AppTileUi>();
                StatusTextBlock.Text = "应用读取没有完成；没有把失败当作扫描成功。";
            }
            return false;
        }
        finally
        {
            ScanSoftwareButton.IsEnabled = true;
        }
    }

    private Task<IReadOnlyList<SoftwareProfile>> ScanSoftwareProfilesAsync() =>
        _softwareFixtureScanner is null
            ? _softwareScanner.ScanAsync()
            : _softwareFixtureScanner.ScanAsync();

    private async Task<bool> RefreshMigrationClosureAsync(
        bool refreshUi,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var monitoringRoot = Path.Combine(DefaultMigrationRollbackRoot(), "Monitoring");
            var findings = await MigrationClosureMonitor.ScanLatestAsync(
                new JsonMigrationMonitoringStore(monitoringRoot),
                new WindowsMigrationPathObserver(),
                maximumSoftwareCount: 64,
                cancellationToken: cancellationToken);
            _migrationClosureBySoftware = MigrationClosurePresenter.CreateLatest(findings)
                .ToDictionary(
                    summary => summary.SoftwareName,
                    summary => summary,
                    StringComparer.OrdinalIgnoreCase);

            if (refreshUi)
            {
                RefreshAppCatalog();
                RefreshHealthSummaryFromBase();
                LoadAgentNextSteps();
            }
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            _migrationClosureBySoftware =
                new Dictionary<string, MigrationClosureSummaryViewModel>(StringComparer.OrdinalIgnoreCase);
            if (refreshUi)
            {
                RefreshAppCatalog();
                RefreshHealthSummaryFromBase();
                LoadAgentNextSteps();
            }
            return false;
        }
    }

    private MigrationClosureTargetDisposition ResolveMigrationClosureTargetDisposition(
        string softwareName)
    {
        var resolution = AppDrawerTargetResolver.Resolve(softwareName, _softwareProfiles);
        if (!resolution.CanOpen || resolution.Profile is null)
            return MigrationClosureTargetDisposition.Unavailable;

        return AppPresentationBuilder.CanReviewMigrationClosure(resolution.Profile)
            ? MigrationClosureTargetDisposition.Reviewable
            : MigrationClosureTargetDisposition.ProtectedHistorical;
    }

    private bool HasUniqueSoftwareName(string softwareName) =>
        _softwareProfiles.Count(profile =>
            profile.Name.Equals(softwareName, StringComparison.OrdinalIgnoreCase)) == 1;

    private MigrationClosureSummaryViewModel? FindMigrationClosure(SoftwareProfile profile)
    {
        if (!HasUniqueSoftwareName(profile.Name))
            return null;
        return _migrationClosureBySoftware.TryGetValue(profile.Name, out var summary)
            ? summary
            : null;
    }

    private void ShowAppDrawer(SoftwareProfile profile)
    {
        var drawer = AppPresentationBuilder.CreateDrawer(profile);
        var migrationClosure = FindMigrationClosure(profile);
        var migrationState = MigrationClosureDrawerStatePresenter.Create(profile, drawer, migrationClosure);
        DrawerTitleTextBlock.Text = drawer.Name;
        DrawerPublisherTextBlock.Text = string.IsNullOrWhiteSpace(profile.Publisher)
            ? "发布者未知"
            : profile.Publisher;
        DrawerCategorySummaryTextBlock.Text = drawer.CategorySummary;
        DrawerLocationTextBlock.Text = drawer.InstallLocationSummary;
        DrawerSizeTextBlock.Text = drawer.SizeSummary;
        DrawerResidencyTextBlock.Text = drawer.ResidencySummary;
        DrawerAdviceTextBlock.Text = migrationState.AdviceText;
        ApplyDrawerActionHost(AppDrawerActionHostPresenter.Collapsed());
        DrawerTechnicalListBox.ItemsSource = drawer.TechnicalDetails.Count == 0
            ? new[] { "暂无可展示的技术细节。" }
            : drawer.TechnicalDetails;
        ApplyDrawerTechnicalDetailsState(AppDrawerTechnicalDetailsPresenter.Collapsed(), DrawerTechnicalDetailsButton);
        DrawerTechnicalDetailsButton.IsEnabled = true;
        DrawerTechnicalDetailsButton.ToolTip = "查看路径、后台组件和分类依据等技术证据。";

        ApplyActionState(DrawerUninstallButton, drawer, AppActionKind.Uninstall);
        ApplyActionState(DrawerMigrateButton, drawer, AppActionKind.Migration);
        ApplyActionState(DrawerCleanCacheButton, drawer, AppActionKind.CacheCleanup);
        ApplyActionState(DrawerDisableStartupButton, drawer, AppActionKind.StartupControl);
        DrawerMigrateButton.Content = migrationState.ButtonText;
        DrawerMigrateButton.IsEnabled = migrationState.CanOpenPlan;
        DrawerMigrateButton.ToolTip = migrationState.ButtonReason;
        ApplyResidueReviewState(DrawerResidueReviewButton, drawer.UninstallResidueReview);
    }

    private static void ApplyActionState(
        Button button,
        AppDrawerViewModel drawer,
        AppActionKind kind)
    {
        var action = drawer.AvailableActions.FirstOrDefault(a => a.Kind == kind);
        button.IsEnabled = action?.IsEnabled == true;
        button.ToolTip = action?.Reason ?? "当前不可用";
    }

    private static void ApplyResidueReviewState(
        Button button,
        AppResidueReviewAvailabilityViewModel availability)
    {
        button.IsEnabled = availability.IsEnabled;
        button.ToolTip = availability.Reason;
    }

    private void RefreshAppCatalog(SoftwareProfile? preferredProfile = null)
    {
        if (_softwareProfiles.Count == 0)
        {
            AppTilesListBox.ItemsSource = Array.Empty<AppTileUi>();
            var emptyStateTitle = _softwareInventoryLoadGate.HasCompletedLoad
                ? "应用读取已完成，当前没有发现可管理的应用。"
                : "正在准备应用列表...";
            AppsSummaryTextBlock.Text = emptyStateTitle;
            ClearAppDrawer(emptyStateTitle);
            return;
        }

        var selectedName = preferredProfile?.Name
            ?? (AppTilesListBox.SelectedItem as AppTileUi)?.Profile.Name;
        var filtered = AppCatalogPresenter.Apply(_softwareProfiles, new AppCatalogQuery
        {
            Filter = _appCatalogFilter,
            Sort = _appCatalogSort,
            SearchText = AppSearchTextBox.Text
        });

        var filteredList = filtered
            .OrderByDescending(profile =>
                MigrationClosureTileStatePresenter.ShouldPrioritize(profile, FindMigrationClosure(profile)))
            .ToList();
        AppTilesListBox.ItemsSource = filteredList
            .Select(profile => AppTileUi.From(profile, FindMigrationClosure(profile)))
            .ToList();
        AppsSummaryTextBlock.Text =
            AppCatalogSummaryPresenter.Create(_softwareProfiles, filtered.Count).Text +
            BuildMigrationClosureCatalogSummary();

        if (filtered.Count == 0)
        {
            ClearAppDrawer("没有符合条件的应用");
            return;
        }

        var index = selectedName is null
            ? 0
            : filteredList.FindIndex(profile =>
                ReferenceEquals(profile, preferredProfile)
                || profile.Name.Equals(selectedName, StringComparison.CurrentCultureIgnoreCase));
        AppTilesListBox.SelectedIndex = index >= 0 ? index : 0;
    }

    private string BuildMigrationClosureCatalogSummary()
    {
        return MigrationClosureCatalogSummaryPresenter.Create(_softwareProfiles, FindMigrationClosure).Text;
    }

    private void ClearAppDrawer(string reason)
    {
        var empty = AppDrawerEmptyStatePresenter.Create(reason);
        AppTilesListBox.SelectedIndex = -1;
        DrawerTitleTextBlock.Text = empty.Title;
        DrawerPublisherTextBlock.Text = empty.SupportingText;
        DrawerCategorySummaryTextBlock.Text = empty.CategorySummary;
        DrawerLocationTextBlock.Text = empty.InstallLocationSummary;
        DrawerSizeTextBlock.Text = empty.SizeSummary;
        DrawerResidencyTextBlock.Text = empty.ResidencySummary;
        DrawerAdviceTextBlock.Text = empty.AgentAdviceText;
        ApplyDrawerActionHost(AppDrawerActionHostPresenter.Collapsed());
        DrawerTechnicalListBox.ItemsSource = Array.Empty<string>();
        ApplyDrawerTechnicalDetailsState(AppDrawerTechnicalDetailsPresenter.Collapsed(), DrawerTechnicalDetailsButton);
        DrawerUninstallButton.IsEnabled = false;
        DrawerMigrateButton.IsEnabled = false;
        DrawerMigrateButton.Content = "迁移到 D 盘";
        DrawerCleanCacheButton.IsEnabled = false;
        DrawerDisableStartupButton.IsEnabled = false;
        DrawerResidueReviewButton.IsEnabled = false;
        DrawerTechnicalDetailsButton.IsEnabled = false;
        DrawerUninstallButton.ToolTip = empty.DisabledActionReason;
        DrawerMigrateButton.ToolTip = empty.DisabledActionReason;
        DrawerCleanCacheButton.ToolTip = empty.DisabledActionReason;
        DrawerDisableStartupButton.ToolTip = empty.DisabledActionReason;
        DrawerResidueReviewButton.ToolTip = empty.DisabledActionReason;
        DrawerTechnicalDetailsButton.ToolTip = empty.DisabledActionReason;
    }

    private void SetAppFilterSelected()
    {
        foreach (var button in AppFilterButtons())
        {
            var selected = button.Tag?.ToString() == _appCatalogFilter.ToString();
            button.Background = BrushFrom(selected ? "#111827" : "#FFFFFF");
            button.Foreground = BrushFrom(selected ? "#FFFFFF" : "#374151");
            button.BorderBrush = BrushFrom(selected ? "#111827" : "#D1D5DB");
        }
    }

    private IEnumerable<Button> AppFilterButtons()
    {
        yield return AllAppsFilterButton;
        yield return NormalAppsFilterButton;
        yield return DevelopmentAppsFilterButton;
        yield return GameAppsFilterButton;
        yield return SystemAppsFilterButton;
        yield return CDriveAppsFilterButton;
        yield return ResidentAppsFilterButton;
        yield return UninstallableAppsFilterButton;
    }

    private async Task CaptureInstallSnapshotAsync(bool isBefore)
    {
        SetInstallSnapshotButtonsEnabled(false);
        _lastInstallDiffReport = null;
        InstallDiffAgentExplainButton.IsEnabled = false;
        InstallDiffAgentExplainButton.Visibility = Visibility.Collapsed;
        InstallDiffCardsListBox.Visibility = Visibility.Collapsed;
        InstallDiffTechnicalDetailsExpander.Visibility = Visibility.Collapsed;
        InstallDiffAgentPanel.Visibility = Visibility.Collapsed;
        InstallDiffActionPlanPanel.Visibility = Visibility.Collapsed;
        var label = isBefore ? "安装前" : "安装后";
        InstallDiffTextBox.Text = $"正在捕获{label}快照...";
        StatusTextBlock.Text = $"正在只读捕获{label}软件画像快照...";

        try
        {
            var profiles = await ScanSoftwareProfilesAsync();
            var footprint = await CaptureInstallFootprintAsync();
            var snapshot = new InstallSystemSnapshot(DateTimeOffset.Now, profiles, footprint);

            if (isBefore)
                _beforeInstallSnapshot = snapshot;
            else
                _afterInstallSnapshot = snapshot;

            InstallDiffTextBox.Text =
                $"已捕获{label}快照: {snapshot.SoftwareProfiles.Count} 个软件\n" +
                $"C 盘落地点观察: {snapshot.CDriveFootprint?.Paths.Count ?? 0} 个，状态 {FootprintStatusText(snapshot.CDriveFootprint)}\n" +
                $"时间: {snapshot.CapturedAt:yyyy-MM-dd HH:mm:ss}\n" +
                "状态: 只读扫描完成，没有运行安装包，也没有修改系统。";
            StatusTextBlock.Text = $"{label}快照捕获完成: {snapshot.SoftwareProfiles.Count} 个软件。";
        }
        catch (Exception)
        {
            InstallDiffTextBox.Text =
                $"{label}快照没有完成；没有运行安装包，也没有修改系统。请稍后重新捕获。";
            StatusTextBlock.Text = $"{label}快照没有完成；没有把未知状态当作成功。";
        }
        finally
        {
            SetInstallSnapshotButtonsEnabled(true);
        }
    }

    private static Task<InstallFootprintCapture> CaptureInstallFootprintAsync(
        CancellationToken cancellationToken = default) =>
        Task.Run(
            () => new WindowsInstallFootprintProbe().Capture(),
            cancellationToken);

    private static string FootprintStatusText(InstallFootprintCapture? footprint) =>
        (footprint ?? InstallFootprintCapture.EmptyComplete).Status switch
        {
            InstallFootprintCaptureStatus.Complete => "完整",
            InstallFootprintCaptureStatus.Truncated => "超过上限",
            _ => "部分位置不可读取"
        };

    private void SetInstallSnapshotButtonsEnabled(bool isEnabled)
    {
        CaptureBeforeInstallButton.IsEnabled = isEnabled;
        CaptureAfterInstallButton.IsEnabled = isEnabled;
        BuildInstallDiffButton.IsEnabled = isEnabled;
    }

    private static string FormatInstallDiffReport(InstallSnapshotDiffReport report)
    {
        var lines = new List<string>
        {
            report.Summary,
            $"安装前: {report.BeforeCapturedAt:yyyy-MM-dd HH:mm:ss}",
            $"安装后: {report.AfterCapturedAt:yyyy-MM-dd HH:mm:ss}",
            string.Empty
        };

        AddProfileLines(lines, "新增软件", report.AddedSoftware);
        AddTextLines(lines, "新增自启动", report.NewStartupEntries);
        AddTextLines(lines, "新增服务", report.NewServices);
        AddTextLines(lines, "新增计划任务", report.NewScheduledTasks);
        AddTextLines(lines, "新增 C 盘路径", report.NewCDrivePaths);

        return string.Join(Environment.NewLine, lines);
    }

    private static void AddProfileLines(List<string> lines, string title, IReadOnlyList<SoftwareProfile> profiles)
    {
        lines.Add(title + ":");
        if (profiles.Count == 0)
        {
            lines.Add("  无");
            lines.Add(string.Empty);
            return;
        }

        foreach (var profile in profiles.Take(20))
            lines.Add($"  - {profile.Name} | {profile.InstallPath ?? "未知位置"}");

        if (profiles.Count > 20)
            lines.Add($"  ... 还有 {profiles.Count - 20} 个");

        lines.Add(string.Empty);
    }

    private static void AddTextLines(List<string> lines, string title, IReadOnlyList<string> values)
    {
        lines.Add(title + ":");
        if (values.Count == 0)
        {
            lines.Add("  无");
            lines.Add(string.Empty);
            return;
        }

        foreach (var value in values.Take(30))
            lines.Add("  - " + value);

        if (values.Count > 30)
            lines.Add($"  ... 还有 {values.Count - 30} 项");

        lines.Add(string.Empty);
    }

    private Task EnsureTimelineLoadedAsync() =>
        _timelineLoadGate.EnsureLoadedAsync(LoadTimelineCoreAsync);

    private async Task LoadTimelineAsync()
    {
        await _timelineLoadGate.RefreshAsync(LoadTimelineCoreAsync);
    }

    private async Task<bool> LoadTimelineCoreAsync()
    {
        LoadTimelineButton.IsEnabled = false;
        ReviewQuarantineCleanupButton.IsEnabled = false;
        ReviewQuarantineCleanupButton.Visibility = Visibility.Collapsed;
        TimelineQuarantineCandidateListBox.ItemsSource = null;
        TimelineQuarantineCandidateListBox.Visibility = Visibility.Collapsed;
        TimelineQuarantinePolicyTextBlock.Text = "正在自动读取隔离区容量和保留期建议。";
        ShowTimelineState("正在读取最近的安全操作，不会修改任何文件或设置。");
        StatusTextBlock.Text = "正在读取后悔药时间线...";

        try
        {
            var entries = await _timelineStore.LoadRecentAsync(30);
            await RefreshQuarantinePolicyAsync();
            if (entries.Count == 0)
            {
                ShowTimelineState("目前没有可以还原的操作。以后由 OMNIX 完成的安全变更会记录在这里。");
            }
            else
            {
                ShowTimelineEntries(entries);
            }
            StatusTextBlock.Text = entries.Count == 0
                ? "后悔药时间线为空。"
                : $"后悔药时间线已加载: {entries.Count} 条。";
            return true;
        }
        catch (Exception)
        {
            ShowTimelineState("暂时无法读取后悔药记录。可以稍后重新加载；本次没有修改任何文件或设置。");
            StatusTextBlock.Text = "后悔药记录暂时无法读取；没有修改任何文件或设置。";
            return false;
        }
        finally
        {
            LoadTimelineButton.IsEnabled = true;
        }
    }

    private void ShowTimelineState(string message)
    {
        TimelineStateTextBlock.Text = message;
        TimelineStateTextBlock.Visibility = Visibility.Visible;
        TimelineListBox.ItemsSource = null;
        TimelineListBox.Visibility = Visibility.Collapsed;
    }

    private void ShowTimelineEntries(IReadOnlyList<ActionTimelineEntry> entries)
    {
        TimelineListBox.ItemsSource = entries.Select(ActionTimelinePresenter.CreateItem).ToList();
        TimelineStateTextBlock.Visibility = Visibility.Collapsed;
        TimelineListBox.Visibility = Visibility.Visible;
    }

    private async Task RefreshQuarantinePolicyAsync()
    {
        var records = await _quarantineService.LoadRecordsAsync();
        var plan = QuarantineRetentionPlanner.Build(records, new QuarantineRetentionOptions());
        var presentation = QuarantineRetentionPresenter.Create(plan);
        ApplyQuarantineRetentionPresentation(presentation);
    }

    private void ApplyQuarantineRetentionPresentation(QuarantineRetentionViewModel presentation)
    {
        TimelineQuarantinePolicyTextBlock.Text =
            presentation.Headline + "\n" +
            presentation.UsageText + "\n" +
            presentation.ImpactText + "\n" +
            presentation.SafetyText;
        TimelineQuarantineCandidateListBox.ItemsSource = presentation.Candidates;
        var hasCandidates = presentation.Candidates.Count > 0;
        TimelineQuarantineCandidateListBox.Visibility = hasCandidates
            ? Visibility.Visible
            : Visibility.Collapsed;
        ReviewQuarantineCleanupButton.Visibility = hasCandidates
            ? Visibility.Visible
            : Visibility.Collapsed;
        ReviewQuarantineCleanupButton.IsEnabled = hasCandidates;
    }

    private async void ReviewQuarantineCleanup_Click(object sender, RoutedEventArgs e)
    {
        var pipelineAttempted = false;
        var stateSynchronized = false;
        ReviewQuarantineCleanupButton.IsEnabled = false;
        StatusTextBlock.Text = "正在重新核对隔离区永久整理方案...";
        try
        {
            var records = await _quarantineService.LoadRecordsAsync();
            var plan = QuarantineRetentionPlanner.Build(records, new QuarantineRetentionOptions());
            if (plan.Candidates.Count == 0)
            {
                StatusTextBlock.Text = "隔离区当前没有需要永久整理的项目。";
                await RefreshQuarantinePolicyAsync();
                return;
            }

            var descriptor = QuarantinePurgeOperationPolicy.CreatePlan(plan);
            var validation = QuarantinePurgeOperationPolicy.ValidateCandidate(descriptor);
            if (!validation.Success)
            {
                StatusTextBlock.Text =
                    "永久整理方案未通过安全检查；没有永久删除任何隔离内容。请重新加载后再生成方案。";
                await RefreshQuarantinePolicyAsync();
                return;
            }

            var confirmation = QuarantinePurgeConfirmationPresenter.Create(descriptor, plan);
            var window = new QuarantinePurgeConfirmationWindow(confirmation) { Owner = this };
            if (window.ShowDialog() != true)
            {
                StatusTextBlock.Text = "已取消，没有永久整理任何隔离内容。";
                await RefreshQuarantinePolicyAsync();
                return;
            }

            var confirmed = QuarantinePurgeOperationPolicy.ConfirmForExecution(descriptor);
            var handler = new QuarantinePurgeOperationHandler(_quarantineService, _timelineStore);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            StatusTextBlock.Text = "正在永久整理已确认的隔离项目...";
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(confirmed);
            await LoadTimelineAsync();
            stateSynchronized = true;
            StatusTextBlock.Text = result.Success
                ? result.Summary ?? "隔离区永久整理已完成。"
                : "永久整理没有确认完成；请重新加载后悔药中心核对当前状态。";
        }
        catch (Exception)
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await LoadTimelineAsync();
                stateSynchronized = true;
            }
            StatusTextBlock.Text =
                "永久整理没有确认完成；请重新加载后悔药中心核对当前状态，再决定是否重试。";
            if (!pipelineAttempted)
            {
                try
                {
                    await RefreshQuarantinePolicyAsync();
                }
                catch
                {
                    ReviewQuarantineCleanupButton.IsEnabled = true;
                }
            }
        }
    }

    private async Task RestoreQuarantineTimelineItemAsync(ActionTimelineItemViewModel item)
    {
        var pipelineAttempted = false;
        var stateSynchronized = false;
        LoadTimelineButton.IsEnabled = false;
        StatusTextBlock.Text = "正在重新核对后悔药记录和隔离内容...";

        try
        {
            var preparation = await QuarantineRestoreOperationPolicy.PrepareForConfirmationAsync(
                item.Id,
                _timelineStore,
                _quarantineService,
                _quarantineIdentityReader);
            if (!preparation.Success
                || preparation.Operation is null
                || preparation.CurrentEntry is null)
            {
                StatusTextBlock.Text =
                    "还原记录没有通过当前安全核对；没有移动任何内容，请重新加载后再试。";
                return;
            }

            var currentItem = ActionTimelinePresenter.CreateItem(preparation.CurrentEntry);
            var confirmation = TimelineRestoreConfirmationPresenter.Create(currentItem);
            var confirmationWindow = new TimelineRestoreConfirmationWindow(confirmation)
            {
                Owner = this
            };
            if (confirmationWindow.ShowDialog() != true)
            {
                StatusTextBlock.Text = "已取消还原，没有修改任何文件。";
                return;
            }

            var descriptor = QuarantineRestoreOperationPolicy.ConfirmForExecution(
                preparation.Operation);
            var handler = new QuarantineRestoreOperationHandler(
                _quarantineService,
                _timelineStore,
                _quarantineIdentityReader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            StatusTextBlock.Text = "正在通过安全管线还原已确认的隔离内容...";
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(descriptor);
            await LoadTimelineAsync();
            stateSynchronized = true;
            StatusTextBlock.Text = result.Success
                ? result.Summary ?? "已还原，后悔药时间线已更新。"
                : "还原没有确认完成；请重新加载后悔药中心核对当前状态。";
        }
        catch (Exception)
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await LoadTimelineAsync();
                stateSynchronized = true;
            }
            StatusTextBlock.Text =
                "还原没有确认完成；请重新加载后悔药中心核对当前状态，再决定是否重试。";
        }
        finally
        {
            LoadTimelineButton.IsEnabled = true;
        }
    }

    private async Task ReviewSelectedUninstallResidueAsync()
    {
        if (AppTilesListBox.SelectedItem is not AppTileUi selected)
        {
            StatusTextBlock.Text = "请先选择一个应用。";
            return;
        }

        await ReviewUninstallResidueAsync(selected.Profile);
    }

    private async Task RefreshUninstallResidueStateAfterAttemptAsync()
    {
        try
        {
            await LoadTimelineAsync();
        }
        catch
        {
            // The residue-operation conclusion remains authoritative when history cannot reload.
        }

        try
        {
            SetSoftwareProfiles(await ScanSoftwareProfilesAsync());
        }
        catch
        {
            RefreshAppCatalog();
        }
    }

    private void ShowReadOnlyUninstallResidueReviewAfterRetry(
        SoftwareProfile before,
        IReadOnlyList<SoftwareProfile> afterProfiles)
    {
        try
        {
            var report = UninstallResidueScanBuilder.Build(
                before,
                afterProfiles,
                ExistingPathExists,
                EstimateExistingPathSize);
            var review = UninstallResidueReviewPresentationBuilder.Create(report);

            SetSoftwareProfiles(afterProfiles, refreshCatalog: false);
            RefreshAppCatalog();
            ShowResidueReviewInline(review);
            StatusTextBlock.Text =
                "已重新扫描并生成只读残留结论；没有移动或删除任何内容。";
        }
        catch
        {
            SetSoftwareProfiles(afterProfiles);
            StatusTextBlock.Text =
                "残留复查没有完成；不能据此判断没有残留。没有移动或删除任何内容。";
        }
    }

    private async Task ReviewUninstallResidueAsync(
        SoftwareProfile before,
        IReadOnlyList<SoftwareProfile>? knownAfterProfiles = null)
    {
        var pipelineAttempted = false;
        var stateSynchronized = false;
        var availability = AppPresentationBuilder.CreateUninstallResidueReviewAvailability(before);
        if (!availability.IsEnabled)
        {
            StatusTextBlock.Text = availability.Reason;
            return;
        }

        DrawerResidueReviewButton.IsEnabled = false;
        StatusTextBlock.Text = $"正在重新扫描 {before.Name} 的卸载后残留...";

        try
        {
            var afterProfiles = knownAfterProfiles ?? await ScanSoftwareProfilesAsync();
            var report = UninstallResidueScanBuilder.Build(
                before,
                afterProfiles,
                ExistingPathExists,
                EstimateExistingPathSize);
            var review = UninstallResidueReviewPresentationBuilder.Create(report);

            SetSoftwareProfiles(afterProfiles, refreshCatalog: false);

            var lowRiskOperation = review.LowRiskOperation;
            if (!review.CanMoveLowRiskToQuarantine || lowRiskOperation is null)
            {
                StatusTextBlock.Text = review.Summary;
                RefreshAppCatalog();
                ShowResidueReviewInline(review);
                return;
            }

            var policy = QuarantineOperationPolicy.ValidateCandidate(lowRiskOperation);
            if (!policy.Success)
            {
                MessageBox.Show(
                    this,
                    "安全策略没有批准这项残留处理；没有移动或删除任何残留。\n\n请重新扫描应用后再生成方案。",
                    review.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                StatusTextBlock.Text = "安全策略没有批准这项残留处理；没有移动或删除任何残留。";
                RefreshAppCatalog();
                ShowResidueReviewInline(review);
                return;
            }


            var quarantinePreparation = QuarantineOperationPolicy.PrepareForConfirmation(
                lowRiskOperation,
                DefaultQuarantineRoot(),
                _quarantineIdentityReader);
            if (!quarantinePreparation.Success || quarantinePreparation.Operation is null)
            {
                StatusTextBlock.Text = "残留位置无法绑定当前文件身份，旧方案已停止。";
                RefreshAppCatalog();
                ShowResidueReviewInline(review);
                return;
            }
            var preparedOperation = quarantinePreparation.Operation;

            var confirmation = CleanupConfirmationPresenter.Create(preparedOperation, DefaultQuarantineRoot());
            var confirmationWindow = new CleanupConfirmationWindow(confirmation)
            {
                Owner = this
            };
            if (confirmationWindow.ShowDialog() != true)
            {
                StatusTextBlock.Text = "已取消残留处理，没有移动任何文件。";
                RefreshAppCatalog();
                ShowResidueOutcomeInline(UninstallResidueDrawerReviewPresenter.CreateCanceled(review));
                return;
            }

            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparedOperation);
            var handler = new QuarantineOperationHandler(
                _quarantineService,
                _timelineStore,
                _quarantineIdentityReader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(descriptor);
            await RefreshUninstallResidueStateAfterAttemptAsync();
            stateSynchronized = true;

            if (!result.Success)
            {
                MessageBox.Show(
                    this,
                    "残留处理没有确认完成；请到后悔药中心核对记录，并重新扫描应用。",
                    review.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                StatusTextBlock.Text = "残留处理没有确认完成；请到后悔药中心核对记录，并重新扫描应用。";
                ShowResidueReviewInline(review);
                return;
            }

            StatusTextBlock.Text = result.Summary ?? "低风险残留已移动到隔离区。";
            MessageBox.Show(
                this,
                (result.Summary ?? "低风险残留已移动到隔离区。") + "\n\n可以在后悔药中心按记录还原。",
                review.Title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            ShowResidueOutcomeInline(UninstallResidueDrawerReviewPresenter.CreateQuarantined(review, result.Summary));
        }
        catch (Exception)
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await RefreshUninstallResidueStateAfterAttemptAsync();
                stateSynchronized = true;
            }
            StatusTextBlock.Text = "残留复查没有完成；不能据此判断没有残留。";
            MessageBox.Show(
                this,
                "残留复查没有完成；不能据此判断没有残留。\n\n请重新扫描应用后再试。",
                "卸载后检查残留",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            RefreshResidueReviewButtonForCurrentSelection();
        }
    }

    private void RefreshResidueReviewButtonForCurrentSelection()
    {
        if (AppTilesListBox.SelectedItem is not AppTileUi selected)
        {
            DrawerResidueReviewButton.IsEnabled = false;
            DrawerResidueReviewButton.ToolTip = "请先选择应用。";
            return;
        }

        var availability = AppPresentationBuilder.CreateUninstallResidueReviewAvailability(selected.Profile);
        ApplyResidueReviewState(DrawerResidueReviewButton, availability);
    }

    private void ShowResidueReviewInline(UninstallResidueReviewViewModel review)
    {
        var drawerReview = UninstallResidueDrawerReviewPresenter.Create(review);
        ApplyDrawerActionHost(new AppDrawerActionHostViewModel
        {
            IsVisible = true,
            Title = drawerReview.SectionTitle,
            Summary = review.Summary,
            AgentTakeaway = "Agent \u5224\u65ad\uff1a\u5378\u8f7d\u540e\u6b8b\u7559\u8981\u5148\u786e\u8ba4\u8f6f\u4ef6\u5df2\u7ecf\u4e0d\u5728\uff0c\u518d\u5206\u98ce\u9669\u5904\u7406\u3002",
            NextStepText = "\u4e0b\u4e00\u6b65\uff1a\u53ea\u6709\u4f4e\u98ce\u9669\u7f13\u5b58\u6216\u65e5\u5fd7\u624d\u80fd\u8fdb\u9694\u79bb\u533a\u65b9\u6848\u3002",
            SafetyText = "\u5b89\u5168\u8fb9\u754c\uff1a\u4e0d\u4f1a\u76f4\u63a5\u5220\u9664\u6b8b\u7559\uff0c\u9ad8\u98ce\u9669\u9879\u53ea\u89e3\u91ca\u4e0d\u5904\u7406\u3002",
            Lines = drawerReview.Lines,
            CanExecuteDirectly = review.CanExecuteDirectly,
            StatusText = review.Summary
        });
        StatusTextBlock.Text = review.Summary;
    }

    private void ShowResidueOutcomeInline(UninstallResidueDrawerReviewViewModel outcome)
    {
        ApplyDrawerActionHost(new AppDrawerActionHostViewModel
        {
            IsVisible = true,
            Title = outcome.SectionTitle,
            Summary = outcome.Lines.FirstOrDefault() ?? outcome.SectionTitle,
            AgentTakeaway = "Agent \u5224\u65ad\uff1a\u8fd9\u6b21\u6b8b\u7559\u5904\u7406\u5df2\u7ecf\u6709\u7ed3\u679c\uff0c\u5148\u770b\u7ed3\u8bba\u518d\u770b\u6280\u672f\u660e\u7ec6\u3002",
            NextStepText = "\u4e0b\u4e00\u6b65\uff1a" + outcome.PrimaryButtonText,
            SafetyText = "\u5b89\u5168\u8fb9\u754c\uff1a\u53ea\u5c55\u793a\u672c\u6b21\u7ed3\u679c\uff1b\u6ca1\u6709\u989d\u5916\u6267\u884c\u5378\u8f7d\u3001\u6ce8\u518c\u8868\u6216\u670d\u52a1\u64cd\u4f5c\u3002",
            Lines = outcome.Lines,
            CanExecuteDirectly = false,
            StatusText = outcome.Lines.FirstOrDefault() ?? outcome.SectionTitle,
            PrimaryActionText = outcome.PrimaryActionText,
            PrimaryActionKey = outcome.PrimaryActionKey
        });
        StatusTextBlock.Text = outcome.Lines.FirstOrDefault() ?? outcome.SectionTitle;
    }

    private void ShowHomeAgentResponse(HomeAgentResponseViewModel response)
    {
        HomeAgentResponseTitleTextBlock.Text = response.Title;
        HomeAgentResponseBodyTextBlock.Text = response.Body;
        HomeAgentResponseSafetyTextBlock.Text = response.SafetyBoundary;
        HomeAgentResponseNavigateButton.Content = response.NavigationLabel;
        HomeAgentResponseNavigateButton.Tag = response;
        HomeAgentResponseNavigateButton.Visibility = response.CanNavigate
            ? Visibility.Visible
            : Visibility.Collapsed;
        StatusTextBlock.Text = response.SafetyBoundary;
    }

    private static IReadOnlyList<string> CurrentUserDataRoots()
    {
        var roots = new List<string>();
        AddExistingUserDataRoot(roots, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        AddExistingUserDataRoot(roots, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
            AddExistingUserDataRoot(roots, Path.Combine(userProfile, "AppData", "LocalLow"));
        return roots.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddExistingUserDataRoot(List<string> roots, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            roots.Add(path);
    }

    private static bool IsReparsePoint(string path)
    {
        try
        {
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }
        catch
        {
            return true;
        }
    }

    private async Task RestoreStartupTimelineItemAsync(ActionTimelineItemViewModel item)
    {
        var pipelineAttempted = false;
        var stateSynchronized = false;
        LoadTimelineButton.IsEnabled = false;
        StatusTextBlock.Text = "正在核对自启动还原证据...";
        try
        {
            var preparation = await StartupRestoreOperationPolicy.PrepareForConfirmationAsync(
                item.Id,
                _timelineStore,
                _startupManifestStore);
            if (!preparation.Success
                || preparation.Operation is null
                || preparation.CurrentEntry is null)
            {
                StatusTextBlock.Text =
                    "自启动还原记录没有通过当前安全核对；没有修改启动设置，请重新加载后再试。";
                return;
            }

            var currentItem = ActionTimelinePresenter.CreateItem(preparation.CurrentEntry);
            var confirmation = TimelineRestoreConfirmationPresenter.Create(currentItem);
            var confirmationWindow = new TimelineRestoreConfirmationWindow(confirmation)
            {
                Owner = this
            };
            if (confirmationWindow.ShowDialog() != true)
            {
                StatusTextBlock.Text = "已取消还原，没有修改文件或启动设置。";
                return;
            }

            var descriptor = StartupRestoreOperationPolicy.ConfirmForExecution(
                preparation.Operation);
            var handler = new StartupRestoreOperationHandler(
                _startupEntryStore,
                _startupManifestStore,
                _timelineStore);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            StatusTextBlock.Text = "正在通过安全管线恢复已确认的自启动设置...";
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(descriptor);
            await RefreshStartupStateAfterAttemptAsync();
            stateSynchronized = true;
            StatusTextBlock.Text = result.Success
                ? "自启动已按原始设置还原；没有启动软件。"
                : "自启动还原没有确认完成；请重新扫描应用并重新加载后悔药中心。";
        }
        catch (Exception)
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await RefreshStartupStateAfterAttemptAsync();
                stateSynchronized = true;
            }
            StatusTextBlock.Text =
                "自启动还原没有确认完成；请重新扫描应用并重新加载后悔药中心核对当前状态。";
        }
        finally
        {
            LoadTimelineButton.IsEnabled = true;
        }
    }

    private static bool ExistingPathExists(string path)
    {
        try
        {
            return File.Exists(path) || Directory.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    private static long EstimateExistingPathSize(string path)
    {
        try
        {
            if (File.Exists(path))
                return new FileInfo(path).Length;

            if (!Directory.Exists(path))
                return 0;

            long total = 0;
            foreach (var file in EnumerateFilesForSize(path, maxFiles: 5_000))
            {
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // Ignore files that disappear or cannot be read during the estimate.
                }
            }

            return total;
        }
        catch
        {
            return 0;
        }
    }

    private static IEnumerable<string> EnumerateFilesForSize(string root, int maxFiles)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        var yielded = 0;

        while (pending.Count > 0 && yielded < maxFiles)
        {
            var current = pending.Pop();
            string[] files;
            try
            {
                files = Directory.GetFiles(current);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                if (yielded >= maxFiles)
                    yield break;

                yielded++;
                yield return file;
            }

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var directory in directories)
            {
                try
                {
                    if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(directory);
                }
                catch
                {
                    // Ignore unreadable directories while estimating residue size.
                }
            }
        }
    }

    private async Task RefreshRecommendationCleanupStateAfterAttemptAsync()
    {
        try
        {
            await LoadTimelineAsync();
        }
        catch
        {
            // The cleanup conclusion remains authoritative when history cannot reload.
        }

        try
        {
            await RefreshHealthScanAsync();
        }
        catch
        {
            // Keep the last health view when the read-only rescan is unavailable.
        }
    }

    private async Task ExecuteSelectedRecommendationAsync()
    {
        var pipelineAttempted = false;
        var stateSynchronized = false;
        if (RecommendationsListBox.SelectedItem is not RecommendationCardViewModel selected || selected.Operation is null)
        {
            RecommendationActionTextBlock.Text = "请先选择一张带清理操作的低风险决策卡。";
            return;
        }

        var policy = QuarantineOperationPolicy.ValidateCandidate(selected.Operation);
        if (!policy.Success)
        {
            RecommendationActionTextBlock.Text =
                "这张清理卡没有通过安全检查；没有移动任何文件。请重新扫描后再选择。";
            return;
        }


        var quarantinePreparation = QuarantineOperationPolicy.PrepareForConfirmation(
            selected.Operation,
            DefaultQuarantineRoot(),
            _quarantineIdentityReader);
        if (!quarantinePreparation.Success || quarantinePreparation.Operation is null)
        {
            RecommendationActionTextBlock.Text =
                "当前文件状态无法安全确认，请重新扫描后再生成方案。";
            return;
        }
        var preparedOperation = quarantinePreparation.Operation;

        var confirmation = CleanupConfirmationPresenter.Create(preparedOperation, DefaultQuarantineRoot());
        var confirmationWindow = new CleanupConfirmationWindow(confirmation)
        {
            Owner = this
        };
        if (confirmationWindow.ShowDialog() != true)
        {
            RecommendationActionTextBlock.Text = "已取消，没有移动任何文件。";
            return;
        }

        ExecuteRecommendationButton.IsEnabled = false;
        RecommendationActionTextBlock.Text = "正在通过安全管线移动到隔离区...";

        try
        {
            var descriptor = QuarantineOperationPolicy.ConfirmForExecution(preparedOperation);
            var handler = new QuarantineOperationHandler(
                _quarantineService,
                _timelineStore,
                _quarantineIdentityReader);
            var pipeline = new SafetyOperationPipeline(handler.ExecuteAsync);
            pipelineAttempted = true;
            var result = await pipeline.ExecuteAsync(descriptor);
            await RefreshRecommendationCleanupStateAfterAttemptAsync();
            stateSynchronized = true;

            if (!result.Success)
            {
                RecommendationActionTextBlock.Text =
                    "清理没有确认完成；请到后悔药中心核对当前记录，再重新扫描。";
                StatusTextBlock.Text = "清理状态需要复查；已重新读取后悔药和 C 盘状态。";
                return;
            }

            RecommendationActionTextBlock.Text = result.Summary ?? "已移动到隔离区。";
            StatusTextBlock.Text = "清理建议已执行，文件已移动到隔离区。";
        }
        catch (Exception)
        {
            if (pipelineAttempted && !stateSynchronized)
            {
                await RefreshRecommendationCleanupStateAfterAttemptAsync();
                stateSynchronized = true;
            }
            RecommendationActionTextBlock.Text =
                "清理没有确认完成；请到后悔药中心核对当前记录，再重新扫描。";
            StatusTextBlock.Text = "清理状态需要复查；没有把未知状态当作成功。";
        }
        finally
        {
            ExecuteRecommendationButton.IsEnabled = RecommendationSelectionPresenter.Create(
                RecommendationsListBox.SelectedItem as RecommendationCardViewModel).CanContinue;
        }
    }

    private static string NormalizeDriveRoot(string raw)
    {
        var value = string.IsNullOrWhiteSpace(raw) ? "C:\\" : raw.Trim();
        var root = Path.GetPathRoot(value);
        if (string.IsNullOrWhiteSpace(root))
            return value.EndsWith('\\') ? value : value + "\\";
        return root.EndsWith('\\') ? root : root + "\\";
    }

    private void InitializeDriveOptions()
    {
        var drives = Directory.GetLogicalDrives()
            .Select(NormalizeDriveRoot)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (drives.Count == 0)
            drives.Add("C:\\");

        DriveRootComboBox.ItemsSource = drives;
        DriveRootComboBox.SelectedItem = drives.FirstOrDefault(d => d.Equals("C:\\", StringComparison.OrdinalIgnoreCase))
            ?? drives.First();
    }

    private void ApplyCDrivePageChrome()
    {
        var driveRoot = NormalizeDriveRoot(DriveRootComboBox.SelectedItem?.ToString() ?? "C:\\");
        var chrome = CDrivePageChromePresenter.Create(driveRoot);

        DriveTargetLabelTextBlock.Text = chrome.ScanTargetLabel;
        DriveTargetHintTextBlock.Text = chrome.ScanTargetHint;
        DriveTargetPanel.ToolTip = chrome.ScanTargetHint;
        ToggleTechnicalReportButton.Content = chrome.TechnicalReportToggleText;
        TechnicalReportHintTextBlock.Text = chrome.TechnicalReportHint;
        ReportTextBox.Visibility = chrome.IsTechnicalReportVisibleByDefault ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string CategoryLabel(SoftwareCategory category) =>
        category switch
        {
            SoftwareCategory.Normal => "普通应用",
            SoftwareCategory.Game => "游戏娱乐",
            SoftwareCategory.Ai => "AI 工具",
            SoftwareCategory.DevelopmentTool => "开发工具",
            SoftwareCategory.SystemTool => "系统应用",
            _ => "未知类型"
        };

    private static Brush BrushFrom(string color)
    {
        if (color.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            return Brushes.Transparent;
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }

    private static string DefaultDatabasePath()
    {
        return AppStoragePathResolver.Resolve().DatabasePath;
    }

    private static string DefaultInstallRoutingMemoryPath()
    {
        return AppStoragePathResolver.Resolve().InstallRoutingMemoryPath;
    }

    private static string DefaultInstallEvidenceRoot()
    {
        var databasePath = AppStoragePathResolver.Resolve().DatabasePath;
        return Path.Combine(
            Path.GetDirectoryName(databasePath)
                ?? throw new InvalidOperationException("Application data root is unavailable."),
            "InstallEvidence");
    }

    private static async Task<MigrationPlanPreviewViewModel> CreateRollbackManifestPreviewAsync(
        SoftwareProfile profile)
    {
        var now = DateTimeOffset.Now;
        var plan = MigrationPlanPresentationBuilder.CreatePlan(profile);
        var snapshotId = "migration-snapshot-" + Guid.NewGuid().ToString("N");
        var root = DefaultMigrationRollbackRoot();
        var pathOptions = new MigrationPlanPresentationOptions
        {
            Now = now,
            SnapshotId = snapshotId,
            RollbackRoot = root
        };
        var manifestPath = MigrationPlanPresentationBuilder.BuildRollbackManifestPath(profile, plan, pathOptions);

        var manifestEvidence = await MigrationRollbackManifestCreationService.CreateAsync(
            profile,
            plan,
            manifestPath,
            snapshotId,
            now);
        var snapshotEvidencePath = Path.Combine(
            root,
            "Snapshots",
            snapshotId + ".json");
        var snapshotEvidence = await MigrationSnapshotEvidenceService.CreateAsync(
            manifestEvidence.Manifest,
            manifestEvidence.ManifestPath,
            manifestEvidence.Sha256,
            snapshotEvidencePath,
            new WindowsMigrationSnapshotSourceReader(),
            now);

        var destinationSpace = MigrationDestinationSpaceProbe.CheckCurrentMachine(
            plan.DestinationRoot,
            EstimateMigrationRequiredBytes(profile));
        var refreshedOptions = new MigrationPlanPresentationOptions
        {
            Now = now,
            SnapshotId = snapshotId,
            RollbackRoot = root,
            Readiness = new MigrationExecutionReadiness
            {
                FeatureEnabled = true,
                SnapshotId = snapshotId,
                RollbackManifestPath = manifestPath,
                RollbackManifestSha256 = manifestEvidence.Sha256,
                SnapshotEvidencePath = snapshotEvidence.EvidencePath,
                SnapshotEvidenceSha256 = snapshotEvidence.Sha256,
                DestinationAvailableBytes = destinationSpace.AvailableBytes
            },
            RollbackManifestExists = File.Exists
        };

        return MigrationPlanPresentationBuilder.Create(profile, refreshedOptions);
    }

    private static long EstimateMigrationRequiredBytes(SoftwareProfile profile)
    {
        var total = profile.InstalledSizeBytes + profile.DataSizeBytes + profile.CacheSizeBytes;
        return total > 0 ? total : 1;
    }

    private static string DefaultMigrationRollbackRoot() =>
        AppStoragePathResolver.Resolve().MigrationRollbackRoot;

    private static string DefaultQuarantineRoot()
    {
        return AppStoragePathResolver.Resolve().QuarantineRoot;
    }

    private static string DefaultStartupRollbackRoot()
    {
        var databasePath = AppStoragePathResolver.Resolve().DatabasePath;
        return Path.Combine(
            Path.GetDirectoryName(databasePath)
                ?? throw new InvalidOperationException("Application data root is unavailable."),
            "StartupRollback",
            "Manifests");
    }

    private static IStartupEntryControlStore CreateStartupEntryControlStore() =>
        (IStartupEntryControlStore?)StartupEntryControlFixtureStore.TryCreate(
            AppDevelopmentPathResolver.ResolveStartupEntryFixturePath())
        ?? new WindowsCurrentUserRunStartupEntryStore();

    private sealed class AppTileUi
    {
        public required SoftwareProfile Profile { get; init; }
        public required string Name { get; init; }
        public required string ShortTag { get; init; }
        public required string AccessibilityName { get; init; }
        public required string IconText { get; init; }
        public required ImageSource? IconSource { get; init; }
        public required Visibility IconVisibility { get; init; }
        public required Visibility IconFallbackVisibility { get; init; }
        public required Brush IconBackground { get; init; }
        public required Brush StatusBrush { get; init; }

        public static AppTileUi From(
            SoftwareProfile profile,
            MigrationClosureSummaryViewModel? migrationClosure = null)
        {
            var tile = AppPresentationBuilder.CreateTile(profile);
            var iconSource = ApplicationIconLoader.TryLoad(tile.IconPath, tile.IconIndex);
            var closureState = MigrationClosureTileStatePresenter.Create(profile, tile, migrationClosure);
            return new AppTileUi
            {
                Profile = profile,
                Name = tile.Name,
                ShortTag = closureState.ShortTag,
                AccessibilityName = tile.Name + ", " + closureState.ShortTag,
                IconText = BuildIconText(tile.Name),
                IconSource = iconSource,
                IconVisibility = iconSource is null ? Visibility.Collapsed : Visibility.Visible,
                IconFallbackVisibility = iconSource is null ? Visibility.Visible : Visibility.Collapsed,
                IconBackground = BrushFrom(CategoryColor(tile.Category)),
                StatusBrush = BrushFrom(StatusColor(closureState.Status))
            };
        }

        public static AppTileUi Message(string message)
        {
            var profile = new SoftwareProfile { Name = message };
            return new AppTileUi
            {
                Profile = profile,
                Name = message,
                ShortTag = "请稍候",
                AccessibilityName = message,
                IconText = "…",
                IconSource = null,
                IconVisibility = Visibility.Collapsed,
                IconFallbackVisibility = Visibility.Visible,
                IconBackground = BrushFrom("#6B7280"),
                StatusBrush = BrushFrom("#9CA3AF")
            };
        }

        private static string BuildIconText(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";
            var first = name.Trim()[0];
            return char.IsLetter(first)
                ? char.ToUpperInvariant(first).ToString()
                : first.ToString();
        }

        private static string CategoryColor(SoftwareCategory category) =>
            category switch
            {
                SoftwareCategory.Ai => "#7C3AED",
                SoftwareCategory.DevelopmentTool => "#2563EB",
                SoftwareCategory.Game => "#F97316",
                SoftwareCategory.SystemTool => "#64748B",
                SoftwareCategory.Normal => "#059669",
                _ => "#475569"
            };

        private static string StatusColor(AppTileStatus status) =>
            status switch
            {
                AppTileStatus.Normal => "#10B981",
                AppTileStatus.Warning => "#F59E0B",
                AppTileStatus.Attention => "#EF4444",
                AppTileStatus.System => "#9CA3AF",
                _ => "#9CA3AF"
            };
    }

    private sealed class SystemToolShortcutView
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string SafetyHint { get; init; }

        public static SystemToolShortcutView From(SystemToolShortcut shortcut) =>
            new()
            {
                Id = shortcut.Id,
                Name = shortcut.RequiresConfirmation
                    ? $"{shortcut.Name} · \u9700\u786e\u8ba4"
                    : shortcut.Name,
                Description = shortcut.Description,
                SafetyHint = shortcut.SafetyHint
            };
    }

    private sealed class WindowsSettingsShortcutView
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string SafetyHint { get; init; }

        public static WindowsSettingsShortcutView From(WindowsSettingsShortcut shortcut) =>
            new()
            {
                Id = shortcut.Id,
                Name = shortcut.RequiresConfirmation
                    ? $"{shortcut.Name} - \u9700\u786e\u8ba4"
                    : shortcut.Name,
                Description = shortcut.Description,
                SafetyHint = shortcut.SafetyHint
            };
    }

    private sealed class AgentSkillView
    {
        public required AgentSkillCategory Category { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string NextStepLabel { get; init; }
        public required string SafetyHint { get; init; }
        public required string SafetyLine { get; init; }

        public static AgentSkillView From(AgentSkillCardViewModel skill) =>
            new()
            {
                Category = skill.Category,
                Title = skill.Title,
                Description = skill.Description,
                NextStepLabel = $"\u4e0b\u4e00\u6b65\uff1a{skill.NextStepLabel}",
                SafetyHint = skill.SafetyHint,
                SafetyLine = $"\u6267\u884c\u8fb9\u754c\uff1a{skill.ModeLabel} / \u98ce\u9669\uff1a{skill.RiskLabel}"
            };

        private static string AgentCategoryLabel(AgentSkillCategory category) =>
            category switch
            {
                AgentSkillCategory.SystemDiagnosis => "系统诊断与体检",
                AgentSkillCategory.SystemSettings => "系统设置与优化",
                AgentSkillCategory.Troubleshooting => "故障排查与修复",
                AgentSkillCategory.WindowAndDesktop => "窗口与桌面管理",
                AgentSkillCategory.ProcessAndServiceManagement => "进程与服务管理",
                AgentSkillCategory.HardwareInfo => "硬件信息查询",
                AgentSkillCategory.SystemTools => "系统工具直达",
                AgentSkillCategory.InputAndSession => "输入与会话控制",
                _ => "其他能力"
            };

        private static string ModeLabel(AgentExecutionMode mode) =>
            mode switch
            {
                AgentExecutionMode.ReadOnly => "只读查询",
                AgentExecutionMode.ExplainOnly => "只解释",
                AgentExecutionMode.PlanOnly => "只生成方案",
                AgentExecutionMode.OpenSystemTool => "只打开系统工具",
                _ => mode.ToString()
            };

        private static string AgentRiskLabel(RiskLevel risk) =>
            risk switch
            {
                RiskLevel.None => "无",
                RiskLevel.Low => "低",
                RiskLevel.Medium => "中，需确认",
                RiskLevel.High => "高，必须走安全管线",
                _ => risk.ToString()
            };
    }

    private sealed class RecommendationCardView
    {
        public required string Title { get; init; }
        public required string Finding { get; init; }
        public required string Reason { get; init; }
        public required string SafetyLine { get; init; }
        public OperationDescriptor? Operation { get; init; }

        public static RecommendationCardView From(Recommendation recommendation) =>
            new()
            {
                Title = HumanRecommendationTitle(recommendation),
                Finding = "发现：" + recommendation.Finding,
                Reason = "建议：" + recommendation.Reason,
                SafetyLine = $"能不能动：{ActionLabel(recommendation.Action)} / 风险：{RiskLabel(recommendation.Risk)} / 后悔药：{ReversibilityLabel(recommendation.Reversibility)} / 预计影响：{RootCauseReportBuilder.Fmt(recommendation.EstimatedImpactBytes)}",
                Operation = recommendation.Operation
            };

        private static string HumanRecommendationTitle(Recommendation recommendation)
        {
            const string unexpectedPrefix = "非预期根目录: ";
            const string tempPrefix = "可清理临时目录: ";
            if (recommendation.Title.StartsWith(unexpectedPrefix, StringComparison.OrdinalIgnoreCase))
                return "先别删，先确认来源：" + recommendation.Title[unexpectedPrefix.Length..];
            if (recommendation.Title.StartsWith(tempPrefix, StringComparison.OrdinalIgnoreCase))
                return "可试清理：" + recommendation.Title[tempPrefix.Length..];
            return recommendation.Title;
        }

        private static string ActionLabel(RecommendationAction action) =>
            action switch
            {
                RecommendationAction.Clean => "可以确认后移入隔离区",
                RecommendationAction.Observe => "先观察，不建议直接动",
                RecommendationAction.Migrate => "可规划迁移，尚需验证",
                RecommendationAction.Keep => "保留",
                RecommendationAction.DisableStartup => "可考虑禁用自启动",
                RecommendationAction.Uninstall => "可考虑卸载",
                RecommendationAction.RepairInstallLocation => "可修复安装路径",
                _ => action.ToString()
            };

        private static string RiskLabel(RiskLevel risk) =>
            risk switch
            {
                RiskLevel.None => "无风险",
                RiskLevel.Low => "低",
                RiskLevel.Medium => "中，先确认来源",
                RiskLevel.High => "高，当前不开放执行",
                _ => risk.ToString()
            };

        private static string ReversibilityLabel(ReversibilityLevel reversibility) =>
            reversibility switch
            {
                ReversibilityLevel.Reversible => "可还原",
                ReversibilityLevel.PartiallyReversible => "部分可还原",
                ReversibilityLevel.NotReversible => "不可还原",
                _ => reversibility.ToString()
            };
    }

    private sealed class SoftwareProfileView
    {
        public required string Title { get; init; }
        public required string Detail { get; init; }
        public required string SafetyLine { get; init; }

        public static SoftwareProfileView From(SoftwareProfile profile)
        {
            var installPath = string.IsNullOrWhiteSpace(profile.InstallPath) ? "未知安装位置" : profile.InstallPath;
            var cDrive = profile.CDriveWritePaths.Count == 0 ? "未发现 C 盘安装/写入点" : "关注：仍在 C 盘或会写 C 盘 - " + string.Join("; ", profile.CDriveWritePaths.Take(2));
            var signature = string.IsNullOrWhiteSpace(profile.SignatureSubject) ? "签名: 未知/未签名" : "签名: " + profile.SignatureSubject;
            var advice = profile.CDriveWritePaths.Count > 0
                ? "用途: 后续可判断是否需要清缓存或迁移"
                : "用途: 作为安装后变化和自启动检查基线";
            return new SoftwareProfileView
            {
                Title = $"{profile.Name} | {CategoryLabel(profile.Category)}",
                Detail = $"发布者: {profile.Publisher ?? "未知"} / 安装位置: {installPath}",
                SafetyLine = $"自启动 {profile.StartupEntries.Count} / 服务 {profile.Services.Count} / 计划任务 {profile.ScheduledTasks.Count} / {cDrive} / {signature} / {advice}"
            };
        }

        private static string CategoryLabel(SoftwareCategory category) =>
            category switch
            {
                SoftwareCategory.Normal => "普通软件",
                SoftwareCategory.Game => "游戏",
                SoftwareCategory.Ai => "AI 工具",
                SoftwareCategory.DevelopmentTool => "开发工具",
                SoftwareCategory.SystemTool => "系统组件",
                _ => "未知类型"
            };
    }

}
