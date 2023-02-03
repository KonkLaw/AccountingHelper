using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for OperationsGrid.xaml
/// </summary>
public partial class OperationsGrid : UserControl
{
    private bool isReadOnly;
    public bool IsReadOnly
    {
        get => isReadOnly;
        set
        {
            isReadOnly = value;
            if (isReadOnly)
            {
                CategoriesColumn.Visibility = Visibility.Hidden;
                CommentColumn.Visibility = Visibility.Hidden;
            }
            else
            {
                CategoriesColumn.Visibility = Visibility.Visible;
                CommentColumn.Visibility = Visibility.Visible;
            }
        }
    }

    public OperationsGrid()
    {
        InitializeComponent();
    }
}
