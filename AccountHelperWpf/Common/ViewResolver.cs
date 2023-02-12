using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Common;

class ViewResolver : IViewResolver
{
    private readonly Dictionary<Type, Func<FrameworkElement>> viewModelToView = new();
    private readonly Dictionary<Type, Func<Window>> viewModelToWindow = new();
    private Window? dialogWindow;

    public FrameworkElement ResolveView(object viewModel)
    {
        Func<FrameworkElement> viewCreator = viewModelToView[viewModel.GetType()];
        FrameworkElement view = viewCreator();
        view.DataContext = viewModel;
        return view;
    }

    public TabItem ResolveTabItem(string header, object contentViewModel)
    {
        return new TabItem
        {
            Header = header,
            Content = ResolveView(contentViewModel)
        };
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
    {
        MessageBox.Show(dialogWindow ?? Application.Current!.MainWindow!,
            message, string.Empty, MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    public void RegisterView<TView, TViewModel>(Func<TView> creator) where TView : FrameworkElement
        => viewModelToView.Add(typeof(TViewModel), creator);

    public void RegisterWindow<TView, TViewModel>(Func<TView> creator) where TView : Window
        => viewModelToWindow.Add(typeof(TViewModel), creator);
}