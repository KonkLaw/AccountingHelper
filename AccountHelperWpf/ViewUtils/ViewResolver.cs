using System.Windows;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewModels;
using AccountHelperWpf.Views;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewUtils;

class ViewResolver : IViewResolver
{
    private readonly Dictionary<Type, Func<Window>> viewModelToWindow = new();
    private Window? dialogWindow;

    public ViewResolver()
    {
        RegisterWindow<PkoBlockedOperationsWindow, PkoBlockedOperationParserVM>(() => new PkoBlockedOperationsWindow());
    }

    public void ResolveAndShowDialog(object viewModel)
    {
        Window window = viewModelToWindow[viewModel.GetType()]();
        window.DataContext = viewModel;
        dialogWindow = window;
        window.ShowDialog();
        dialogWindow = null;
    }

    public void ShowWarning(string message)
        => MessageBox.Show(GetWindow(), message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);

    public MessageBoxResult ShowQuestion(string question, MessageBoxButton messageBoxButton)
    {
        MessageBoxResult result = MessageBox.Show(
            GetWindow(), question, string.Empty, messageBoxButton, MessageBoxImage.Question);
        return result;
    }

    public bool ShowYesNoQuestion(string question)
        => ShowQuestion(question, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

    public string? SaveFileDialogTryGetPath(string? defaultExtension)
    {
        var dialog = new SaveFileDialog();
        if (defaultExtension != null) { }
            dialog.DefaultExt = defaultExtension;
        bool? result = dialog.ShowDialog();
        if (result.HasValue && result.Value)
            return dialog.FileName;
        return null;
    }

    public string? OpenFileDialogTryGetPath(string filer)
    {
        OpenFileDialog fileDialog = new() { Filter = filer };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
            return fileDialog.FileName;
        return null;
    }

    public void RegisterWindow<TView, TViewModel>(Func<TView> creator) where TView : Window
        => viewModelToWindow.Add(typeof(TViewModel), creator);

    public void ShowMain(InitData initData)
    {
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowVM(this, initData)
        };
        Application.Current.MainWindow?.Close();
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
    }

    public ExitState? ShowExitWindow()
    {
        var dialog = new ExitDialog();
        dialog.ShowDialog();
        return dialog.Result;
    }

    private Window GetWindow() => dialogWindow ?? Application.Current!.MainWindow!;
}