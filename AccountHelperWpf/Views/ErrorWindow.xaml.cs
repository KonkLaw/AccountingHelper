using System.Windows;

namespace AccountHelperWpf.Views;
/// <summary>
/// Interaction logic for ErrorWindow.xaml
/// </summary>
public partial class ErrorWindow : Window
{
	private readonly string? message;

	protected ErrorWindow()
	{
		InitializeComponent();
	}

	public ErrorWindow(string message) : this()
	{
		this.message = message;
		Text.Text = message;
	}

	private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
	{
		Clipboard.SetText(message!);
	}
}
