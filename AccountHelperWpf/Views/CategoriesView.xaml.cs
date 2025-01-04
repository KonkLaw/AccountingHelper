using System.Collections.ObjectModel;
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

    private void DataGrid_OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        FrameworkElement frameworkElement = e.EditingElement;
        if (frameworkElement is TextBox textBox && e.Column.DisplayIndex == 0)
        {
            textBox.Text = ProcessUniqueness(textBox.Text);
        }
    }

    private string ProcessUniqueness(string text)
    {
        var categoryVm = (CategoryVM)DataGrid.SelectedItem;
        if (categoryVm.Name == text)
            return text;

        ObservableCollection<CategoryVM> categories = ((CategoriesVM)DataContext).Categories;
        if (categories.Any(c => c.Name == text))
        {
            return GetUniqueName(categories, text);
        }
        return text;
    }

    private static string GetUniqueName(ObservableCollection<CategoryVM> categories, string text)
    {
        string original = text;
        int counter = 2;
        do
        {
            string toTest = $"{original}({counter})";
            if (categories.Any(c => c.Name == toTest))
            {
                counter++;
            }
            else
            {
                return toTest;
            }

        } while (true);
    }
}
