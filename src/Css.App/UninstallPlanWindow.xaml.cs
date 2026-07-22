using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using Css.Core;
using Css.Core.Apps;
using Css.Core.Recovery;
using Css.Core.Software;
using Css.Core.Uninstall;
using Css.Scanner.Software;
using Css.Snapshot.Uninstall;
using Microsoft.Win32;

namespace Css.App;

public partial class UninstallPlanWindow : Window
{
    private readonly SoftwareProfile _profile;
    private readonly UninstallRecoveryPreparationSession _preparationSession;
    private readonly IOfficialUninstallProductionExecutionCoordinator? _executionCoordinator;
    private readonly ProductionExecutionReadinessViewModel _productionReadiness;
    private bool _isApplyingPreparation;
    private UninstallFinalConfirmationDraft? _latestFinalDraft;
    private OfficialUninstallElevatedRequestDraft? _preparedRequest;

    public UninstallPlanWindow(
        SoftwareProfile profile,
        UninstallPlanPreviewViewModel viewModel,
        IReadOnlyList<WindowsRestorePointInfo> restorePoints,
        WindowsRestorePointScanState restorePointScanState,
        IOfficialUninstallProductionExecutionCoordinator? executionCoordinator = null,
        ProductionExecutionReadinessViewModel? productionReadiness = null)
    {
        InitializeComponent();
        _profile = profile;
        _executionCoordinator = executionCoordinator;
        _productionReadiness = productionReadiness
            ?? ProductionExecutionReadinessPresenter.Unavailable(
                ProductionExecutionCapability.OfficialUninstall);
        _preparationSession = new UninstallRecoveryPreparationSession(
            profile,
            viewModel.ReinstallReadiness,
            restorePoints,
            restorePointScanState);
        DataContext = viewModel;
        ApplyProductionReadiness();
        ApplyRecoveryPreparation();
    }

    public bool ProductionExecutionAttempted { get; private set; }
    public bool ProductionCompleted { get; private set; }
    public bool ProductionResidueReviewRecommended { get; private set; }
    public OfficialUninstallPostScanAction ProductionPostScanActionRequested { get; private set; } =
        OfficialUninstallPostScanAction.Close;
    public string? LastExecutionConclusion { get; private set; }

    private void ApplyProductionReadiness()
    {
        UninstallPlanProductionReadinessTitleTextBlock.Text = _productionReadiness.Title;
        UninstallPlanProductionReadinessStatusTextBlock.Text = _productionReadiness.StatusLabel;
        UninstallPlanProductionReadinessConclusionTextBlock.Text = _productionReadiness.Conclusion;
        UninstallPlanProductionReadinessNextStepTextBlock.Text = _productionReadiness.NextStep;
        UninstallPlanProductionReadinessSafetyTextBlock.Text = _productionReadiness.SafetyText;
        UninstallPlanPreparationExpander.Visibility = _productionReadiness.CanPrepareExecution
            ? Visibility.Visible
            : Visibility.Collapsed;
        if (!_productionReadiness.CanPrepareExecution)
        {
            UninstallPlanDecisionNextStepTextBlock.Text =
                "下一步：当前开发验证版只供查看；请使用可信发布者签名的正式版本。";
        }
    }

    private void ChooseInstaller_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "\u9009\u62e9\u8fd9\u4e2a\u8f6f\u4ef6\u7684\u5b98\u65b9\u5b89\u88c5\u5305",
            Filter = "\u5b98\u65b9\u5b89\u88c5\u5305 (*.exe;*.msi)|*.exe;*.msi",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
            return;

        _preparationSession.SelectOfficialInstaller(
            dialog.FileName,
            File.Exists,
            SignatureInspector.GetSignatureSubject);
        ApplyRecoveryPreparation();
    }

    private void BackupAcknowledgement_Changed(object sender, RoutedEventArgs e)
    {
        if (_isApplyingPreparation)
            return;

        _preparationSession.SetPersonalDataBackupAcknowledged(
            UninstallPlanBackupCheckBox.IsChecked == true);
        ApplyRecoveryPreparation();
    }

    private void ApplyRecoveryPreparation()
    {
        _isApplyingPreparation = true;
        try
        {
            var preparation = _preparationSession.Current;
            UninstallPlanReinstallReadinessTextBlock.Text =
                "\u6062\u590d\u51c6\u5907\uff1a" + preparation.ReinstallReadiness.StatusLabel;
            UninstallPlanReinstallNextActionTextBlock.Text = preparation.ReinstallReadiness.NextAction;
            UninstallPlanRestorePointStatusTextBlock.Text = preparation.RestorePointStatus;
            UninstallPlanPreparationSummaryTextBlock.Text = preparation.Summary;
            UninstallPlanBackupCheckBox.IsEnabled = preparation.RequiresPersonalDataBackup;
            UninstallPlanBackupCheckBox.IsChecked = preparation.PersonalDataBackupAcknowledged;
            UninstallPlanBackupCheckBox.Content = preparation.RequiresPersonalDataBackup
                ? "\u6211\u5df2\u786e\u8ba4\u91cd\u8981\u4e2a\u4eba\u6587\u4ef6\u5df2\u5907\u4efd"
                : "\u672a\u8bc6\u522b\u5230\u660e\u786e\u4e2a\u4eba\u6570\u636e\u4f4d\u7f6e";
            UninstallPlanReinstallTechnicalListBox.ItemsSource =
                preparation.ReinstallReadiness.TechnicalDetails;
            ResetFinalChecklist();
            UninstallPlanBuildFinalChecklistButton.IsEnabled =
                _productionReadiness.CanPrepareExecution;
        }
        finally
        {
            _isApplyingPreparation = false;
        }
    }

    private async void BuildFinalChecklist_Click(object sender, RoutedEventArgs e)
    {
        if (!_productionReadiness.CanPrepareExecution)
        {
            UninstallPlanProductionReadinessSafetyTextBlock.BringIntoView();
            return;
        }

        UninstallPlanBuildFinalChecklistButton.IsEnabled = false;
        UninstallPlanFinalChecklistPanel.Visibility = Visibility.Visible;
        UninstallPlanFinalChecklistStatusTextBlock.Text =
            "\u6b63\u5728\u68c0\u67e5\u6062\u590d\u3001\u5907\u4efd\u548c\u5feb\u7167\u8bc1\u636e...";
        UninstallPlanFinalChecklistSummaryTextBlock.Text = string.Empty;

        try
        {
            var defaultRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppIdentity.LocalDataFolderName,
                "Snapshots",
                "Uninstall");
            var evidenceRoot = AppDevelopmentPathResolver.ResolveUninstallEvidenceRoot(defaultRoot);
            var service = new UninstallFinalConfirmationDraftService(
                new UninstallEvidenceSnapshotStore(evidenceRoot));
            var draft = await service.CreateAsync(_profile, _preparationSession.Current);
            ApplyFinalChecklist(draft);
        }
        catch
        {
            UninstallPlanFinalChecklistStatusTextBlock.Text =
                "\u6700\u7ec8\u786e\u8ba4\u6e05\u5355\u6682\u65f6\u65e0\u6cd5\u751f\u6210";
            UninstallPlanFinalChecklistSummaryTextBlock.Text =
                "Agent \u5df2\u505c\u6b62\u7ee7\u7eed\uff0c\u6ca1\u6709\u8fd0\u884c\u5378\u8f7d\u5668\u3002";
            UninstallPlanFinalChecklistReadyListBox.ItemsSource = null;
            UninstallPlanFinalChecklistPendingListBox.ItemsSource = null;
            UninstallPlanFinalChecklistMissingListBox.ItemsSource =
                new[] { "\u8bf7\u91cd\u65b0\u68c0\u67e5\u6062\u590d\u51c6\u5907\u540e\u518d\u8bd5\u3002" };
        }
        finally
        {
            UninstallPlanBuildFinalChecklistButton.IsEnabled =
                _productionReadiness.CanPrepareExecution;
            UninstallPlanFinalChecklistStatusTextBlock.BringIntoView();
        }
    }

    private void ApplyFinalChecklist(UninstallFinalConfirmationDraft draft)
    {
        _latestFinalDraft = draft;
        _preparedRequest = null;
        UninstallPlanFinalChecklistStatusTextBlock.Text = draft.Status switch
        {
            UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation =>
                "\u6062\u590d\u548c\u5feb\u7167\u8bc1\u636e\u5df2\u51c6\u5907\uff0c\u7b49\u5f85\u6700\u7ec8\u786e\u8ba4",
            UninstallFinalConfirmationDraftStatus.SnapshotVerificationFailed =>
                "\u5feb\u7167\u8bc1\u636e\u9a8c\u8bc1\u5931\u8d25\uff0c\u5df2\u505c\u6b62",
            _ => "\u6062\u590d\u51c6\u5907\u8fd8\u4e0d\u5b8c\u6574"
        };
        UninstallPlanFinalChecklistSummaryTextBlock.Text = draft.Summary;
        UninstallPlanFinalChecklistReadyListBox.ItemsSource = draft.ReadyFacts;
        UninstallPlanFinalChecklistPendingListBox.ItemsSource = draft.PendingConfirmations;
        UninstallPlanFinalChecklistMissingListBox.ItemsSource = draft.MissingRequirements;
        UninstallPlanFinalChecklistSafetyTextBlock.Text = draft.CanExecuteDirectly
            ? "\u5b89\u5168\u72b6\u6001\u5f02\u5e38\uff1a\u5df2\u963b\u6b62\u7ee7\u7eed\u3002"
            : "\u8fd9\u4efd\u6e05\u5355\u53ea\u505a\u786e\u8ba4\u51c6\u5907\uff0c\u4e0d\u4f1a\u8fd0\u884c\u5378\u8f7d\u5668\u3002";
        var canContinue = draft.Status == UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation
            && draft.SnapshotEvidence is not null
            && draft.RecoveryEvidence is not null
            && draft.SnapshotValidation?.IsValid == true
            && !draft.CanExecuteDirectly
            && _productionReadiness.CanPrepareExecution;
        UninstallPlanContinueFinalConsentButton.Visibility = canContinue
            ? Visibility.Visible
            : Visibility.Collapsed;
        UninstallPlanContinueFinalConsentButton.IsEnabled = canContinue;
        UninstallPlanContinueFinalConsentButton.Content = "\u7ee7\u7eed\u6700\u7ec8\u786e\u8ba4";
        UninstallPlanRequestStatusTextBlock.Visibility = Visibility.Collapsed;
        UninstallPlanRequestStatusTextBlock.Text = string.Empty;
    }

    private async void ContinueFinalConsent_Click(object sender, RoutedEventArgs e)
    {
        if (ProductionExecutionAttempted)
            return;

        if (!_productionReadiness.CanPrepareExecution)
        {
            ShowRequestStatus(
                "当前版本没有通过正式执行身份检查，已停止进入最终确认。",
                false);
            return;
        }

        var draft = _latestFinalDraft;
        if (draft is null
            || draft.Status != UninstallFinalConfirmationDraftStatus.ReadyForFinalConfirmation
            || draft.SnapshotEvidence is null
            || draft.RecoveryEvidence is null
            || draft.SnapshotValidation?.IsValid != true)
        {
            ShowRequestStatus("\u5feb\u7167\u6216\u6062\u590d\u8bc1\u636e\u5df2\u4e0d\u5b8c\u6574\uff0c\u8bf7\u91cd\u65b0\u751f\u6210\u6700\u7ec8\u786e\u8ba4\u6e05\u5355\u3002", false);
            return;
        }

        var issuer = new OfficialUninstallVisualGateReceiptIssuer();
        var confirmationText = "\u8fd0\u884c " + _profile.Name + " \u7684\u5b98\u65b9\u5378\u8f7d\u5668\uff1f";
        var consentWindow = new OfficialUninstallFinalConsentWindow(
            OfficialUninstallFinalConsentPresenter.CreatePending(
                _profile.Name,
                confirmationText),
            issuer,
            new OfficialUninstallFinalConsentVisualCapture())
        {
            Owner = this
        };
        if (consentWindow.ShowDialog() != true
            || consentWindow.Consent is null
            || string.IsNullOrWhiteSpace(consentWindow.VisualTicketId))
        {
            ShowRequestStatus("\u5df2\u53d6\u6d88\u6700\u7ec8\u786e\u8ba4\uff0c\u6ca1\u6709\u751f\u6210\u5378\u8f7d\u8bf7\u6c42\u3002", false);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var recoveryEvidence = RevalidateRecoveryEvidence(draft.RecoveryEvidence);
        var request = OfficialUninstallRequestPreparationService.Create(
            _profile,
            draft.SnapshotEvidence,
            recoveryEvidence ?? new OfficialUninstallRecoveryEvidence
            {
                Method = OfficialUninstallRecoveryMethod.None,
                CanRecoverApplication = false,
                UserDataBackupConfirmed = false
            },
            consentWindow.Consent,
            issuer,
            consentWindow.VisualTicketId,
            $"uninstall-request-{Guid.NewGuid():N}",
            now,
            File.Exists,
            ComputeFileSha256);
        _preparedRequest = request.CanSubmit ? request : null;

        if (_preparedRequest is null)
        {
            UninstallPlanContinueFinalConsentButton.IsEnabled = false;
            ShowRequestStatus(
                request.MissingRequirements.FirstOrDefault()
                ?? "\u6700\u7ec8\u5b89\u5168\u68c0\u67e5\u672a\u901a\u8fc7\uff0c\u5df2\u505c\u6b62\u3002",
                false);
            return;
        }

        UninstallPlanContinueFinalConsentButton.IsEnabled = false;
        UninstallPlanContinueFinalConsentButton.Content = "\u5df2\u5b8c\u6210\u786e\u8ba4";
        if (_executionCoordinator is null)
        {
            ShowRequestStatus(
                "\u5df2\u751f\u6210\u4e00\u6b21\u6027\u5b89\u5168\u8bf7\u6c42\u3002\u5f53\u524d\u5c1a\u672a\u542f\u52a8\u5378\u8f7d\u5668\uff0c\u4e5f\u6ca1\u6709\u5220\u9664\u4efb\u4f55\u5185\u5bb9\u3002",
                true);
            return;
        }

        UninstallPlanContinueFinalConsentButton.Content = "\u6b63\u5728\u5b89\u5168\u68c0\u67e5";
        ShowRequestStatus("\u6b63\u5728\u6838\u5bf9 OMNIX \u5b89\u5168\u52a9\u624b\u7684\u53d1\u5e03\u8eab\u4efd...", true);
        ProductionExecutionAttempted = true;
        OfficialUninstallProductionExecutionOutcome outcome;
        try
        {
            outcome = await _executionCoordinator.ExecuteAsync(_preparedRequest)
                ?? throw new InvalidOperationException("The uninstall outcome is unavailable.");
        }
        catch
        {
            var unknown = OfficialUninstallWorkerResultPresenter.CreateUnknownAttempt();
            ProductionCompleted = false;
            ProductionResidueReviewRecommended = false;
            ProductionPostScanActionRequested = OfficialUninstallPostScanAction.Close;
            LastExecutionConclusion = unknown.Title;
            var unknownWindow = new OfficialUninstallWorkerResultWindow(
                unknown,
                returnsToApplication: true)
            {
                Owner = this
            };
            unknownWindow.ShowDialog();
            ShowRequestStatus(unknown.Conclusion, false);
            Close();
            return;
        }

        ProductionCompleted = outcome.CompletedProduction;
        ProductionResidueReviewRecommended =
            outcome.CompletedProduction && outcome.PostScan?.CanReviewResidue == true;
        LastExecutionConclusion = outcome.Summary.Title;

        if (outcome.PostScan is not null)
        {
            var postScanWindow = new UninstallPostScanResultWindow(outcome.PostScan)
            {
                Owner = this
            };
            postScanWindow.ShowDialog();
            ProductionPostScanActionRequested = postScanWindow.RequestedAction;
            if (ProductionPostScanActionRequested != OfficialUninstallPostScanAction.Close)
            {
                Close();
                return;
            }
        }
        else
        {
            var resultWindow = new OfficialUninstallWorkerResultWindow(
                outcome.Summary,
                returnsToApplication: true)
            {
                Owner = this
            };
            resultWindow.ShowDialog();
        }

        ShowRequestStatus(
            outcome.Summary.Conclusion,
            outcome.CompletedProduction);
        Close();
    }

    private OfficialUninstallRecoveryEvidence? RevalidateRecoveryEvidence(
        OfficialUninstallRecoveryEvidence evidence)
    {
        if (evidence.Method != OfficialUninstallRecoveryMethod.ReinstallSource
            || string.IsNullOrWhiteSpace(evidence.Reference))
        {
            return null;
        }

        var readiness = ReinstallSourceReadinessPresenter.CreateForSelectedInstaller(
            _profile,
            evidence.Reference,
            File.Exists,
            SignatureInspector.GetSignatureSubject);
        if (!readiness.CanUseAsRecoveryEvidence || readiness.RecoveryEvidence is null)
            return null;
        return new OfficialUninstallRecoveryEvidence
        {
            Method = readiness.RecoveryEvidence.Method,
            Reference = readiness.RecoveryEvidence.Reference,
            CanRecoverApplication = readiness.RecoveryEvidence.CanRecoverApplication,
            UserDataBackupConfirmed = evidence.UserDataBackupConfirmed
        };
    }

    private static string? ComputeFileSha256(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
        catch
        {
            return null;
        }
    }

    private void ShowRequestStatus(string text, bool success)
    {
        UninstallPlanRequestStatusTextBlock.Text = text;
        UninstallPlanRequestStatusTextBlock.Foreground = success
            ? System.Windows.Media.Brushes.DarkGreen
            : System.Windows.Media.Brushes.DarkRed;
        UninstallPlanRequestStatusTextBlock.Visibility = Visibility.Visible;
        UninstallPlanRequestStatusTextBlock.BringIntoView();
    }

    private void ResetFinalChecklist()
    {
        _latestFinalDraft = null;
        _preparedRequest = null;
        ProductionExecutionAttempted = false;
        ProductionCompleted = false;
        ProductionResidueReviewRecommended = false;
        ProductionPostScanActionRequested = OfficialUninstallPostScanAction.Close;
        LastExecutionConclusion = null;
        UninstallPlanFinalChecklistPanel.Visibility = Visibility.Collapsed;
        UninstallPlanFinalChecklistReadyListBox.ItemsSource = null;
        UninstallPlanFinalChecklistPendingListBox.ItemsSource = null;
        UninstallPlanFinalChecklistMissingListBox.ItemsSource = null;
        UninstallPlanContinueFinalConsentButton.Visibility = Visibility.Collapsed;
        UninstallPlanContinueFinalConsentButton.IsEnabled = false;
        UninstallPlanRequestStatusTextBlock.Visibility = Visibility.Collapsed;
        UninstallPlanRequestStatusTextBlock.Text = string.Empty;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
