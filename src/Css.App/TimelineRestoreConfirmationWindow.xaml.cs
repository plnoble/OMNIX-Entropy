using System.Windows;
using Css.Core.Timeline;

namespace Css.App;

public partial class TimelineRestoreConfirmationWindow : Window
{
    public TimelineRestoreConfirmationWindow(TimelineRestoreConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
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
