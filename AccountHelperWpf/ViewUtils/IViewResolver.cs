using System.Windows;
using AccountHelperWpf.Models;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewUtils;

interface IViewResolver
{
    void ResolveAndShowDialog(object viewModel);

    void ShowWarning(string message);
    void ShowInfo(string message, string caption);
    bool ShowYesNoQuestion(string question);
    MessageBoxResult ShowQuestion(string question, MessageBoxButton messageBoxButton);

    string? SaveFileDialogTryGetPath(string? defaultExtension);
    string? OpenFileDialogTryGetPath(string filer);
    void ShowMain(InitData initData, string? optionalFile = null);
    ExitState? ShowExitWindow();
}