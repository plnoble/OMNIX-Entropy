using System.Windows;

namespace Css.App;

public partial class InstallerExecutionResultWindow : Window
{
    public bool PostScanRetryRequested { get; private set; }

    public InstallerExecutionResultWindow(InstallerExecutionResultViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        if (viewModel.CanExecuteDirectly)
            throw new InvalidOperationException("An installer result window cannot execute an operation.");
        InitializeComponent();
        DataContext = viewModel;
        InstallerExecutionResultPostScanRetryButton.Visibility =
            viewModel.CanRequestPostScanRetry
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void PostScanRetry_Click(object sender, RoutedEventArgs e)
    {
        PostScanRetryRequested = true;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
