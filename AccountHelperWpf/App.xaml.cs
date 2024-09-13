using System.Windows;
using System.Windows.Threading;
using AccountHelperWpf.Views;

namespace AccountHelperWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
    }

    private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
	    var window = new ErrorWindow(e.Exception.ToString());
        window.ShowDialog();
    }
}
