using System.Windows;
using System.Windows.Threading;
using AccountHelperWpf.Updater;
using AccountHelperWpf.Views;

namespace AccountHelperWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
        UpdateController.CheckUpdates();
		base.OnStartup(e);
    }

    private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
	    var window = new ErrorWindow(e.Exception.ToString());
        window.ShowDialog();
    }
}
