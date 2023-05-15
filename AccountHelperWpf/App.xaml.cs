using System.Windows;
using System.Windows.Threading;

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
        => MessageBox.Show(e.Exception.ToString());
}
