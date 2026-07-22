using System.Windows;
using System;
using System.Threading.Tasks;
using Css.Core.Apps;
using Css.Core.Migration;
using Css.Core.Operations;

namespace Css.App;

public partial class MigrationPlanWindow : Window
{
    private readonly Func<Task<MigrationPlanPreviewViewModel?>>? _createRollbackManifestAsync;
    private readonly IMigrationProductionExecutionCoordinator? _executionCoordinator;
    private readonly ProductionExecutionReadinessViewModel _productionReadiness;
    private readonly Func<DateTimeOffset> _utcNow;
    private MigrationPlanPreviewViewModel _viewModel;
    private bool _evidenceCreated;

    public bool ProductionExecutionAttempted { get; private set; }
    public bool ProductionCompleted { get; private set; }
    public string? LastExecutionConclusion { get; private set; }

    private void ApplyProductionReadiness()
    {
        MigrationPlanProductionReadinessTitleTextBlock.Text = _productionReadiness.Title;
        MigrationPlanProductionReadinessStatusTextBlock.Text = _productionReadiness.StatusLabel;
        MigrationPlanProductionReadinessConclusionTextBlock.Text = _productionReadiness.Conclusion;
        MigrationPlanProductionReadinessNextStepTextBlock.Text = _productionReadiness.NextStep;
        MigrationPlanProductionReadinessSafetyTextBlock.Text = _productionReadiness.SafetyText;
    }

    public MigrationPlanWindow(
        MigrationPlanPreviewViewModel viewModel,
        Func<Task<MigrationPlanPreviewViewModel?>>? createRollbackManifestAsync = null,
        IMigrationProductionExecutionCoordinator? executionCoordinator = null,
        ProductionExecutionReadinessViewModel? productionReadiness = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _executionCoordinator = executionCoordinator;
        _productionReadiness = productionReadiness
            ?? ProductionExecutionReadinessPresenter.Unavailable(
                ProductionExecutionCapability.Migration);
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        InitializeComponent();
        DataContext = viewModel;
        _createRollbackManifestAsync = createRollbackManifestAsync;
        ApplyProductionReadiness();
        UpdateActionAvailability();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void CreateRollbackManifest_Click(object sender, RoutedEventArgs e)
    {
        if (_createRollbackManifestAsync is null
            || !_productionReadiness.CanPrepareExecution)
            return;

        var answer = MessageBox.Show(
            this,
            "OMNIX-Entropy \u53ea\u4f1a\u5199\u5165 JSON \u56de\u6eda\u8bc1\u636e\u6587\u4ef6\uff0c\u4e0d\u4f1a\u79fb\u52a8\u8f6f\u4ef6\u6587\u4ef6\u6216\u4fee\u6539\u7cfb\u7edf\u8bbe\u7f6e\u3002",
            "\u751f\u6210\u56de\u6eda\u6e05\u5355",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        CreateRollbackManifestButton.IsEnabled = false;
        try
        {
            var updated = await _createRollbackManifestAsync();
            if (updated is not null)
            {
                _viewModel = updated;
                _evidenceCreated = true;
                DataContext = updated;
                CreateRollbackManifestButton.Content = "\u56de\u6eda\u6e05\u5355\u5df2\u4fdd\u5b58";
                UpdateActionAvailability();
            }
        }
        catch (Exception)
        {
            MessageBox.Show(
                this,
                "\u65e0\u6cd5\u751f\u6210\u56de\u6eda\u8bc1\u636e\uff0c\u6ca1\u6709\u79fb\u52a8\u4efb\u4f55\u5185\u5bb9\u3002\u8bf7\u5148\u5173\u95ed\u8f6f\u4ef6\u540e\u91cd\u8bd5\uff0c\u6216\u8ba9 Agent \u89e3\u91ca\u539f\u56e0\u3002",
                "\u56de\u6eda\u6e05\u5355",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            UpdateActionAvailability();
        }
    }

    private async void RequestMigration_Click(object sender, RoutedEventArgs e)
    {
        var gate = _viewModel.ReadinessChecklist.ExecutionGate;
        if (_executionCoordinator is null
            || !_productionReadiness.CanPrepareExecution
            || !gate.CanRequestExecution
            || gate.Operation is null
            || ProductionExecutionAttempted)
            return;

        MigrationFinalConsentViewModel consentView;
        try
        {
            consentView = MigrationFinalConsentPresenter.Create(gate);
        }
        catch
        {
            ShowExecutionResult(OperationResult.Fail(
                "Migration preflight is no longer complete."));
            return;
        }

        var consentWindow = new MigrationFinalConsentWindow(consentView, _utcNow)
        {
            Owner = this
        };
        if (consentWindow.ShowDialog() != true || consentWindow.Consent is null)
            return;

        var now = _utcNow().ToUniversalTime();
        var request = MigrationElevatedRequestComposer.Create(
            gate,
            consentWindow.Consent,
            "migration-request-" + Guid.NewGuid().ToString("N"),
            now);
        if (!request.CanSubmit)
        {
            ShowExecutionResult(OperationResult.Fail(
                "Migration request did not pass the final safety checks."));
            return;
        }

        RequestMigrationButton.IsEnabled = false;
        ProductionExecutionAttempted = true;
        try
        {
            var outcome = await _executionCoordinator.ExecuteAsync(request);
            ProductionCompleted = outcome.CompletedProduction;
            LastExecutionConclusion = outcome.Summary.Title;
            var resultWindow = new MigrationExecutionResultWindow(outcome.Summary)
            {
                Owner = this
            };
            resultWindow.ShowDialog();
            Close();
        }
        catch
        {
            ProductionCompleted = false;
            LastExecutionConclusion = "\u8fc1\u79fb\u7ed3\u679c\u6ca1\u6709\u5b8c\u6574\u786e\u8ba4";
            ShowExecutionResult(OperationResult.Fail(
                "Migration execution outcome is unknown."));
            Close();
        }
        finally
        {
            UpdateActionAvailability();
        }
    }

    private void ShowExecutionResult(OperationResult result)
    {
        var window = new MigrationExecutionResultWindow(
            MigrationExecutionResultPresenter.Create(result))
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void UpdateActionAvailability()
    {
        var canPrepareExecution = _productionReadiness.CanPrepareExecution;
        CreateRollbackManifestButton.Visibility =
            canPrepareExecution ? Visibility.Visible : Visibility.Collapsed;
        RequestMigrationButton.Visibility =
            canPrepareExecution ? Visibility.Visible : Visibility.Collapsed;
        CreateRollbackManifestButton.IsEnabled = _createRollbackManifestAsync is not null
            && canPrepareExecution
            && !_evidenceCreated
            && !string.IsNullOrWhiteSpace(_viewModel.SuggestedRollbackManifestPath);
        RequestMigrationButton.IsEnabled = _executionCoordinator is not null
            && canPrepareExecution
            && _viewModel.CanRunMigration
            && _viewModel.ReadinessChecklist.CanRequestExecution
            && !ProductionExecutionAttempted;
    }
}
