using System.Windows;
using Css.Core.Quarantine;

namespace Css.App;

public partial class QuarantinePurgeConfirmationWindow : Window
{
    public QuarantinePurgeConfirmationWindow(
        QuarantinePurgeConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Acknowledgement_Changed(object sender, RoutedEventArgs e)
    {
        if (QuarantinePurgeConfirmButton is not null)
        {
            QuarantinePurgeConfirmButton.IsEnabled =
                QuarantinePurgeAcknowledgementCheckBox.IsChecked == true;
        }
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (QuarantinePurgeAcknowledgementCheckBox.IsChecked != true)
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
