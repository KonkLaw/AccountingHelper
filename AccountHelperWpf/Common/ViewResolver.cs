using AccountHelperWpf.ViewModels;
using AccountHelperWpf.Views;
using System.Windows;

namespace AccountHelperWpf.Common;

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

    public bool ShowYesNoDialog(string question)
    {
        MessageBoxResult result = MessageBox.Show(
            GetWindow(), question, string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public void RegisterWindow<TView, TViewModel>(Func<TView> creator) where TView : Window
        => viewModelToWindow.Add(typeof(TViewModel), creator);

    private Window GetWindow() => dialogWindow ?? Application.Current!.MainWindow!;
}