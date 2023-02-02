using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Common;

internal interface IViewResolver
{
    FrameworkElement ResolveView(object viewModel);
    TabItem ResolveTabItem(string header, object contentViewModel);
    void ResolveAndShowDialog(object viewModel);
}