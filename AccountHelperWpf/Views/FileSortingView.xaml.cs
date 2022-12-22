using System.Windows.Controls;
using System.Windows.Input;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for SortingView.xaml
/// </summary>
public partial class FileSortingView : UserControl
{
    public FileSortingView()
    {
        InitializeComponent();
    }

    private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        ScrollViewer scv = (ScrollViewer)sender;
        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}
