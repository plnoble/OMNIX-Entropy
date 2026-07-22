using System.Windows;
using Css.Core.Startup;

namespace Css.App;

public partial class StartupControlConfirmationWindow : Window
{
    public StartupControlConfirmationWindow(StartupControlConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        UpdateConfirmState();
    }

    private void Acknowledgement_Changed(object sender, RoutedEventArgs e) =>
        UpdateConfirmState();

    private void UpdateConfirmState()
    {
        if (ConfirmButton is null)
            return;

        ConfirmButton.IsEnabled = FirstAcknowledgementCheckBox.IsChecked == true
            && SecondAcknowledgementCheckBox.IsChecked == true;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmButton.IsEnabled)
            return;

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
