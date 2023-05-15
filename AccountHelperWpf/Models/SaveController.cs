using System.Windows;
using AccountHelperWpf.HistoryFile;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Models;

class SaveController : ISaveController
{
    private readonly IViewResolver viewResolver;
    private readonly InitData initData;
    private string? lastSavePath;
    private bool wasSaved;

    public SaveController(IViewResolver viewResolver, InitData initData)
    {
        this.viewResolver = viewResolver;
        this.initData = initData;
    }

    public void MarkChanged() => wasSaved = false;

    public bool RequestForClose()
    {
        if (wasSaved)
            return true;

        MessageBoxResult result = viewResolver.ShowQuestion("There are unsaved changes of associations. Save them or cancel exit?", MessageBoxButton.YesNoCancel);
        return result switch
        {
            MessageBoxResult.Cancel => true,
            MessageBoxResult.No => false,
            MessageBoxResult.Yes => TrySave(),
            _ => throw new InvalidOperationException()
        };
    }

    private bool TrySave()
    {
        if (wasSaved)
            return true;

        string savePath;
        if (lastSavePath == null)
        {
            string? filePath = viewResolver.SaveFileDialogTryGetPath(HistoryStorageHelper.DefaultExtension);
            if (filePath == null)
                return false;
            savePath = filePath;
        }
        else
            savePath = lastSavePath;

        HistoryHelper.Save(savePath, initData);
        lastSavePath = savePath;
        wasSaved = true;
        return true;
    }

    public void Save() => TrySave();
}

interface ISaveController
{
    void MarkChanged();
}