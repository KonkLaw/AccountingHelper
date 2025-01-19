using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for AssociationsView.xaml
/// </summary>
public partial class AssociationsView : UserControl
{
    public AssociationsView()
    {
        InitializeComponent();
    }

    private void FrameworkElement_OnUnloaded(object sender, RoutedEventArgs e)
    {
        // otherwise
        // System.InvalidOperationException: ''DeferRefresh' is not allowed during an AddNew or EditItem transaction.'
        DataGrid grid = ((DataGrid)sender);
        grid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
        {
            dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            var row = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem) as DataGridRow;
            if (row == null)
            {
                // If the row is not generated yet, delay action until it is
                dataGrid.Dispatcher.Invoke(() =>
                {
                    row = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem) as DataGridRow;
                    if (row != null) row.IsSelected = true;
                }, DispatcherPriority.Loaded);
            }
            else
            {
                row.IsSelected = true;
            }
        }
    }
}
