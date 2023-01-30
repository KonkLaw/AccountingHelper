using System.Windows;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for ObjectSelectorWindow.xaml
/// </summary>
public partial class ObjectSelectorWindow : Window
{
    public object? SelectedItem => ListBox.SelectedItem;

    public ObjectSelectorWindow(IEnumerable<object> items)
    {
        InitializeComponent();
        ListBox.ItemsSource = items;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}