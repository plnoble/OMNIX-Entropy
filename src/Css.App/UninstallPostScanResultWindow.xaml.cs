using System.Windows;
using System.Windows.Media;
using Css.Core.Uninstall;

namespace Css.App;

public partial class UninstallPostScanResultWindow : Window
{
    private readonly OfficialUninstallPostScanViewModel _viewModel;

    public UninstallPostScanResultWindow(OfficialUninstallPostScanViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        if (viewModel.CanExecuteDirectly)
            throw new InvalidOperationException("A post-scan result window cannot execute an operation.");

        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        UninstallPostScanPrimaryActionButton.Content = viewModel.PrimaryActionText;
        UninstallPostScanPrimaryActionButton.Visibility = viewModel.HasPrimaryAction
            ? Visibility.Visible
            : Visibility.Collapsed;
        UninstallPostScanTechnicalHintTextBlock.Visibility = viewModel.TechnicalDetailsAvailable
            ? Visibility.Visible
            : Visibility.Collapsed;
        ApplyStatusColor(viewModel.State);
    }

    public OfficialUninstallPostScanAction RequestedAction { get; private set; } =
        OfficialUninstallPostScanAction.Close;

    private void ApplyStatusColor(OfficialUninstallPostScanState state)
    {
        var (background, border, foreground) = state switch
        {
            OfficialUninstallPostScanState.NoVisibleResidue => ("#DCFCE7", "#22C55E", "#166534"),
            OfficialUninstallPostScanState.ScanFailed => ("#FEE2E2", "#EF4444", "#991B1B"),
            _ => ("#FEF3C7", "#F59E0B", "#78350F")
        };

        UninstallPostScanStatusBorder.Background = BrushFrom(background);
        UninstallPostScanStatusBorder.BorderBrush = BrushFrom(border);
        UninstallPostScanStatusTextBlock.Foreground = BrushFrom(foreground);
    }

    private static Brush BrushFrom(string color) =>
        (Brush)new BrushConverter().ConvertFromString(color)!;

    private void PrimaryAction_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.PrimaryAction == OfficialUninstallPostScanAction.Close)
            return;

        RequestedAction = _viewModel.PrimaryAction;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        RequestedAction = OfficialUninstallPostScanAction.Close;
        Close();
    }
}
