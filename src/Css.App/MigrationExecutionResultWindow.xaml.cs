using System.Windows;
using System.Windows.Media;
using Css.Core.Apps;

namespace Css.App;

public partial class MigrationExecutionResultWindow : Window
{
    public MigrationExecutionResultWindow(MigrationExecutionResultViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        if (viewModel.CanExecuteDirectly)
            throw new InvalidOperationException("A migration result window cannot execute an operation.");
        InitializeComponent();
        DataContext = viewModel;
        ApplyTone(viewModel.Tone);
    }

    private void ApplyTone(MigrationExecutionResultTone tone)
    {
        var (background, border, foreground) = tone switch
        {
            MigrationExecutionResultTone.Normal => ("#DCFCE7", "#22C55E", "#166534"),
            MigrationExecutionResultTone.Notice => ("#EFF6FF", "#60A5FA", "#1D4ED8"),
            _ => ("#FEF3C7", "#F59E0B", "#78350F")
        };
        MigrationExecutionResultStatusBorder.Background = BrushFrom(background);
        MigrationExecutionResultStatusBorder.BorderBrush = BrushFrom(border);
        MigrationExecutionResultStatusTextBlock.Foreground = BrushFrom(foreground);
    }

    private static Brush BrushFrom(string color) =>
        (Brush)new BrushConverter().ConvertFromString(color)!;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
