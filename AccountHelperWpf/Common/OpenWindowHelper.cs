using System.Windows;
using AccountHelperWpf.ViewModels;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.Common;

class OpenWindowHelper
{
    public static void OpenMainWindow(MainWindowModel mainWindowModel)
    {
        Window window = new MainWindow();
        window.DataContext = mainWindowModel;
        Application.Current!.MainWindow!.Close();
        Application.Current.MainWindow = window;
        window.ShowDialog();
    }
}