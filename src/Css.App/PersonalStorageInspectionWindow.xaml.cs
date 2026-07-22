using System.Windows;
using Css.Scanner.Experience;

namespace Css.App;

public partial class PersonalStorageInspectionWindow : Window
{
    public PersonalStorageInspectionWindow(PersonalStorageFindingViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
        PersonalStorageInspectionPathsListBox.ItemsSource = viewModel.EvidencePaths;
        if (viewModel.EvidencePaths.Count > 0)
            PersonalStorageInspectionPathsListBox.SelectedIndex = 0;
    }

    public string? RequestedEvidencePath { get; private set; }

    private void PathsListBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        PersonalStorageInspectionOpenButton.IsEnabled =
            PersonalStorageInspectionPathsListBox.SelectedItem is string;
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (PersonalStorageInspectionPathsListBox.SelectedItem is not string path)
            return;

        RequestedEvidencePath = path;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        RequestedEvidencePath = null;
        Close();
    }
}
