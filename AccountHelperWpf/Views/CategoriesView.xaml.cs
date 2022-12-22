using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AccountHelperWpf.ViewModels;

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
        => DataGrid.CancelEdit(DataGridEditingUnit.Row);

    private void CellLostFocus(object sender, RoutedEventArgs e)
    {
        var dataGridCell = (DataGridCell)sender;
        if (dataGridCell.IsKeyboardFocusWithin || !dataGridCell.IsEditing)
            return;

        string text;
        bool newLineForced;
        if (dataGridCell.Content is TextBox textBox)
        {
            text = textBox.Text;
            newLineForced = false;
        }
        else if (dataGridCell.IsSelected && dataGridCell.Content is TextBlock textBlock)
        {
            text = textBlock.Text;
            newLineForced = true;
        }
        else
            return;


        if (dataGridCell.Column.DisplayIndex == 0)
            ProcessNameColumn(text, newLineForced);
        else if (dataGridCell.Column.DisplayIndex == 1)
            ProcessDescription(text, newLineForced);
    }

    private void ProcessNameColumn(string text, bool newLineForced)
    {
        if (string.IsNullOrEmpty(text))
            return;
            
        ObservableCollection<CategoryVm> categories = ((CategoriesViewModel)DataContext).Categories;

        if (categories.Any(c => c.Name == text))
        {
            var categoryVm = (CategoryVm)DataGrid.SelectedItem;
            categoryVm.Name = GetUniqueName(categories, text);
        }
        DataGrid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private void ProcessDescription(string text, bool newLineForced)
    {
        if (string.IsNullOrEmpty(text) && newLineForced)
            return;

        var categoryVm = (CategoryVm)DataGrid.SelectedItem;
        if (!string.IsNullOrEmpty(categoryVm.Name))
            return;

        ObservableCollection<CategoryVm> categories = ((CategoriesViewModel)DataContext).Categories;
        categoryVm.Name = GetUniqueName(categories,"Empty");
    }

    private static string GetUniqueName(ObservableCollection<CategoryVm> categories, string text)
    {
        string original = text;
        int counter = 1;
        while (true)
        {
            if (categories.Any(c => c.Name == text))
            {
                counter++;
                text = original + '(' + counter.ToString() + ')';
            }
            else
            {
                return text;
            }
        }
    }
}
