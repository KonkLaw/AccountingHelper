using System.Windows;
using System.Windows.Controls;
using AccountHelperWpf.ViewModels;

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

    private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.Column.SortMemberPath == nameof(AssociationVM.CategoryVM))
        {
            e.Column.Visibility = Visibility.Collapsed;
        }
    }
}
