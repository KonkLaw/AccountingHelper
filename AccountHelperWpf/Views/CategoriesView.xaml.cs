using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AccountHelperWpf.Models;
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
        // we are interested only in finishing of edit.
        // this check is important for canceling functionality
        bool isCommit = e.EditAction == DataGridEditAction.Commit;
        if (isCommit && frameworkElement is TextBox textBox && e.Column.DisplayIndex == 0)
        {
            ObservableCollection<Category> categories = ((CategoriesVM)DataContext).Categories;

            // if last row and is empty string - don't add
            if (e.Row.Item == categories[^1] && textBox.Text.Trim() == string.Empty)
            {
                e.Cancel = true; // prevent commit
                ((DataGrid)sender).CancelEdit(); // cancel editing
            }
            else
            {
                string[] names = categories.Select(c => c.Name).ToArray();
                // for some reason wpf have different behaviour for new record and for editing existing row
                // while editing existing row, category is not updated
                names[DataGrid.SelectedIndex] = textBox.Text;
                textBox.Text = ProcessUniqueness(names, textBox.Text);
            }
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

    private void BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        // it makes some row not editable
        if (((Category)e.Row.Item).IsDefault)
        {
            e.Cancel = true;
        }
    }
}
