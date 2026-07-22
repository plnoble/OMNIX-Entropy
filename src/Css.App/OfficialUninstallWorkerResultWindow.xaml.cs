using System.Windows;
using System.Windows.Media;

namespace Css.App;

public partial class OfficialUninstallWorkerResultWindow : Window
{
    public OfficialUninstallWorkerResultWindow(
        OfficialUninstallWorkerResultViewModel viewModel,
        bool returnsToApplication = false)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        if (viewModel.CanExecuteDirectly)
            throw new InvalidOperationException("A worker result window cannot execute an operation.");

        InitializeComponent();
        DataContext = viewModel;
        if (returnsToApplication)
            OfficialUninstallWorkerResultCloseButton.Content =
                viewModel.ReturnToApplicationButtonText;
        ApplyTone(viewModel.Tone);
    }

    private void ApplyTone(OfficialUninstallWorkerResultTone tone)
    {
        var (background, border, foreground) = tone switch
        {
            OfficialUninstallWorkerResultTone.Normal => ("#DCFCE7", "#22C55E", "#166534"),
            OfficialUninstallWorkerResultTone.Notice => ("#EFF6FF", "#60A5FA", "#1E3A8A"),
            _ => ("#FEF3C7", "#F59E0B", "#78350F")
        };
        OfficialUninstallWorkerResultStatusBorder.Background = BrushFrom(background);
        OfficialUninstallWorkerResultStatusBorder.BorderBrush = BrushFrom(border);
        OfficialUninstallWorkerResultStatusTextBlock.Foreground = BrushFrom(foreground);
    }

    private static Brush BrushFrom(string color) =>
        (Brush)new BrushConverter().ConvertFromString(color)!;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
