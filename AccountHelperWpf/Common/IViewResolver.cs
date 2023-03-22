namespace AccountHelperWpf.Common;

internal interface IViewResolver
{
    void ResolveAndShowDialog(object viewModel);
    void ShowWarning(string message);
    bool ShowYesNoDialog(string question);
}