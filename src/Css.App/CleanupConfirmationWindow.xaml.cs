using System.Windows;
using Css.Core.Apps;

namespace Css.App;

public partial class CleanupConfirmationWindow : Window
{
    public CleanupConfirmationWindow(CleanupConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
