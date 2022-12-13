using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Common;

class ViewResolver : IViewResolver
{
    private readonly Dictionary<Type, Func<FrameworkElement>> viewModelToView = new();

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

    public void RegisterViewModel<TView, TViewModel>(Func<TView> creator) where TView : FrameworkElement
    {
        viewModelToView.Add(typeof(TViewModel), creator);
    }
}