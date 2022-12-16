using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for CategoriesView.xaml
/// </summary>
public partial class CategoriesView : UserControl
{
    public CategoriesView()
    {
        InitializeComponent();
    }
    
    private void DataGrid_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        DataGrid dataGrid = (DataGrid)sender;


        

        object? qwe = dataGrid.SelectedItem;

        dataGrid.CancelEdit(DataGridEditingUnit.Row);
    }
}
