using System.Windows;
using Css.InstallGuard.Installers;

namespace Css.App;

public partial class InstallerFinalConsentWindow : Window
{
    public InstallerFinalConsentWindow(InstallerLaunchFinalConsentViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
        UpdateReadiness();
    }

    public InstallerLaunchFinalConsentDecision? Decision { get; private set; }

    private void Acknowledgement_Changed(object sender, RoutedEventArgs e) =>
        UpdateReadiness();

    private void UpdateReadiness()
    {
        if (!IsInitialized)
            return;
        var remaining = new[]
        {
            InstallerConsentPackageCheckBox.IsChecked == true,
            InstallerConsentLocationCheckBox.IsChecked == true,
            InstallerConsentInteractionCheckBox.IsChecked == true,
            InstallerConsentReportCheckBox.IsChecked == true
        }.Count(value => !value);
        InstallerFinalConsentConfirmButton.IsEnabled = remaining == 0;
        InstallerFinalConsentReadinessTextBlock.Text = remaining == 0
            ? "4 项都已确认，可以打开安装界面"
            : $"还需确认 {remaining} 项";
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Decision = null;
        DialogResult = false;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (!InstallerFinalConsentConfirmButton.IsEnabled)
            return;
        Decision = new InstallerLaunchFinalConsentDecision
        {
            PackagePublisherAccepted = true,
            LocationLimitAccepted = true,
            InteractiveReviewAccepted = true,
            PostScanLimitAccepted = true
        };
        DialogResult = true;
    }
}
