using System.Windows;
using AccountHelperWpf.Models;

namespace AccountHelperWpf.ViewUtils;

interface IViewResolver
{
    void ResolveAndShowDialog(object viewModel);
    void ShowWarning(string message);
    MessageBoxResult ShowQuestion(string question, MessageBoxButton messageBoxButton);
    bool ShowYesNoQuestion(string question);
    string? SaveFileDialogTryGetPath(string? defaultExtension);
    string? OpenFileDialogTryGetPath(string filer);
    void ShowMain(InitData initData);
}