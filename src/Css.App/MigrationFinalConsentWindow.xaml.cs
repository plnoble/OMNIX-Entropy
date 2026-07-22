using System.Windows;
using Css.Core.Apps;
using Css.Core.Migration;

namespace Css.App;

public partial class MigrationFinalConsentWindow : Window
{
    private readonly MigrationFinalConsentViewModel _viewModel;
    private readonly Func<DateTimeOffset> _utcNow;

    public MigrationFinalConsentWindow(
        MigrationFinalConsentViewModel viewModel,
        Func<DateTimeOffset>? utcNow = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        InitializeComponent();
        DataContext = viewModel;
        UpdateReadiness();
    }

    public MigrationFinalUserConsent? Consent { get; private set; }

    private void Acknowledgement_Changed(object sender, RoutedEventArgs e) =>
        UpdateReadiness();

    private void UpdateReadiness()
    {
        if (!IsInitialized)
            return;
        var confirmed = new[]
        {
            MigrationFinalConsentPlanCheckBox.IsChecked == true,
            MigrationFinalConsentClosedCheckBox.IsChecked == true,
            MigrationFinalConsentRollbackCheckBox.IsChecked == true,
            MigrationFinalConsentMonitoringCheckBox.IsChecked == true
        };
        var remaining = confirmed.Count(value => !value);
        MigrationFinalConsentConfirmButton.IsEnabled = remaining == 0;
        MigrationFinalConsentReadinessTextBlock.Text = remaining == 0
            ? "4 项都已确认，可以继续"
            : $"还需确认 {remaining} 项";
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Consent = null;
        DialogResult = false;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (!MigrationFinalConsentConfirmButton.IsEnabled)
            return;
        Consent = new MigrationFinalUserConsent
        {
            ConfirmationText = _viewModel.ConfirmationText,
            PlanReviewedConfirmed = true,
            AppComponentsClosedConfirmed = true,
            RollbackAcknowledged = true,
            MonitoringConfirmed = true,
            ExecutionRequested = true,
            ConfirmedAtUtc = _utcNow().ToUniversalTime()
        };
        DialogResult = true;
    }
}
