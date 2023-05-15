using System.Windows.Input;
using AccountHelperWpf.HistoryFile;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class StartVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    public ICommand StartWithEmptyCommand { get; }
    public ICommand LoadFileCommand { get; }

    public StartVM()
    {
        StartWithEmptyCommand = new DelegateCommand(StartWithEmpty);
        LoadFileCommand = new DelegateCommand(LoadFile);
        viewResolver = new ViewResolver();
    }

    private void StartWithEmpty() => viewResolver.ShowMain(HistoryHelper.GetEmpty());

    private void LoadFile()
    {
        string historyFileExtensionFilter = $"{HistoryStorageHelper.DefaultExtension}|*.{HistoryStorageHelper.DefaultExtension}";
        string? filePath = viewResolver.OpenFileDialogTryGetPath(historyFileExtensionFilter);
        if (filePath == null)
            return;

        if (HistoryHelper.Load(filePath, out InitData? initData, out string errorMessage))
        {
            viewResolver.ShowMain(initData!);
        }
        else
        {
            viewResolver.ShowWarning(errorMessage);
        }
    }
}
