using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.BaseObjects;

class LoadedFileInfo
{
    public ICommand Remove { get; }
    public string Description => $"{AccountFile.Description.Name} :=: {AccountFile.Description.Currency}";
    public readonly string FullPath;
    public readonly AccountFile AccountFile;
    private readonly IFileRemover remover;

    public LoadedFileInfo(string fullPath, AccountFile accountFile, IFileRemover remover)
    {
        FullPath = fullPath;
        AccountFile = accountFile;
        this.remover = remover;
        Remove = new DelegateCommand(OnRemove);
    }

    private void OnRemove() => remover.RemoveFile(this);
}