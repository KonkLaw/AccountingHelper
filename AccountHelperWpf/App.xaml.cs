using System.IO;
using System.Windows;
using System.Windows.Threading;
using AccountHelperWpf.HistoryFile;
using AccountHelperWpf.Updater;
using AccountHelperWpf.Views;
using AccountHelperWpf.ViewUtils;

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

        //const bool fastStart = true;
        const bool fastStart = false;
        if (fastStart)
        {
            string testPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.csv");
            if (File.Exists(testPath))
            {
                new ViewResolver().ShowMain(HistoryHelper.GetEmpty(), testPath);
            }
        }
        else
        {
            StartupUri = new Uri("Views/Start.xaml", UriKind.Relative);
        }
    }

    private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
	    var window = new ErrorWindow(e.Exception.ToString());
        window.ShowDialog();
    }
}
