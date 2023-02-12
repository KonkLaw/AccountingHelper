using AccountHelperWpf.ViewModels;
using AccountHelperWpf.Views;
using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Common;

class ViewResolver : IViewResolver
{
    private readonly Dictionary<Type, Func<FrameworkElement>> viewModelToView = new();
    private readonly Dictionary<Type, Func<Window>> viewModelToWindow = new();
    private Window? dialogWindow;

    public ViewResolver()
    {
        RegisterView<FileSortingView, FileSortingViewModel>(() => new FileSortingView());
        RegisterView<FilesSortingView, FilesSortingViewModel>(() => new FilesSortingView());
        RegisterView<CategoriesView, CategoriesViewModel>(() => new CategoriesView());
        RegisterView<HistoryView, HistoryViewModel>(() => new HistoryView());
        RegisterWindow<PkoBlockedOperationsWindow, PkoBlockedOperationParserVM>(() => new PkoBlockedOperationsWindow());
    }

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
            Content = ResolveView(contentViewModel),
            Width = 150
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