using AccountHelperWpf.HistoryFile;
using AccountHelperWpf.Views;
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

        ExitState? result = viewResolver.ShowExitWindow();
        return result switch
        {
            null => false,
            ExitState.ExitNoSave => true,
            ExitState.SaveAndExit => TrySave(),
            ExitState.Cancel => false,
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