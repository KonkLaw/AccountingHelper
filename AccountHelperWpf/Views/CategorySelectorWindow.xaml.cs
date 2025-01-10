using System.Windows;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Views;

partial class CategorySelectorWindow : Window
{
    public object? SelectedItem => ListBox.SelectedItem;

    public CategorySelectorWindow(IEnumerable<CategoryVM> items)
    {
        InitializeComponent();
        ListBox.ItemsSource = items;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}