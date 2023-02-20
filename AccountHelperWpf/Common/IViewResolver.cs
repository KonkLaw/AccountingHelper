using System.Windows;
using System.Windows.Controls;

namespace AccountHelperWpf.Common;

internal interface IViewResolver
{
    FrameworkElement ResolveView(object viewModel);
    void ResolveAndShowDialog(object viewModel);
    void ShowWarning(string message);
    bool ShowYesNoDialog(string question);
}