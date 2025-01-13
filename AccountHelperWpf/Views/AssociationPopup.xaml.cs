using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for AssociationPopup.xaml
/// </summary>
public partial class AssociationPopup : Popup
{
    public AssociationPopup()
    {
        InitializeComponent();
        Opened += OnOpened;
        KeyUp += OnKeyUp;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        ;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        TextBox.Focus();
    }
}
