using System.Windows;
using Css.InstallGuard.Routing;

namespace Css.App;

public partial class InstallRouteMemoryChoiceWindow : Window
{
    public InstallRouteMemoryChoiceWindow(InstallRouteMemoryChoiceViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public InstallRoutingMemoryScope? SelectedScope { get; private set; }

    private void RememberSoftware_Click(object sender, RoutedEventArgs e)
    {
        SelectedScope = InstallRoutingMemoryScope.Software;
        DialogResult = true;
    }

    private void RememberCategory_Click(object sender, RoutedEventArgs e)
    {
        SelectedScope = InstallRoutingMemoryScope.Category;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        SelectedScope = null;
        DialogResult = false;
    }
}
