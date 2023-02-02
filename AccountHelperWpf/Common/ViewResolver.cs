using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Common;

class ViewResolver : IViewResolver
{
    private readonly Dictionary<Type, Func<FrameworkElement>> viewModelToView = new();
    private readonly Dictionary<Type, Func<Window>> viewModelToWindow = new();

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
        window.ShowDialog();
    }

    public void RegisterView<TView, TViewModel>(Func<TView> creator) where TView : FrameworkElement
        => viewModelToView.Add(typeof(TViewModel), creator);

    public void RegisterWindow<TView, TViewModel>(Func<TView> creator) where TView : Window
        => viewModelToWindow.Add(typeof(TViewModel), creator);
}