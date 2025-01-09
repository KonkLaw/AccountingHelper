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
            ObservableCollection<CategoryVM> categories = ((CategoriesVM)DataContext).Categories;
            string[] names = categories.Select(c => c.Name).ToArray();

            // for some reason wpf have different behaviour for new record and for editing existing row
            // while editing existing row, category is not updated
            names[DataGrid.SelectedIndex] = textBox.Text;
            textBox.Text = ProcessUniqueness(names, textBox.Text);
        }
    }

    private static string ProcessUniqueness(string[] names, string text)
    {
        if (names.Count(n => n == text) > 1)
        {
            return GetUniqueName(names, text);
        }
        return text;
    }

    private static string GetUniqueName(string[] names, string text)
    {
        string original = text;
        int counter = 2;
        do
        {
            string toTest = $"{original}({counter})";
            if (names.Any(n => n == toTest))
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
