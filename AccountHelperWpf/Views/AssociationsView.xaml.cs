using System.Windows;
using System.Windows.Controls;

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
}
