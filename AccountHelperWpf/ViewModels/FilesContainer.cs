using System.Collections.ObjectModel;

namespace AccountHelperWpf.ViewModels;

class FilesContainer
{
    private readonly ObservableCollection<TabInfo> tabCollection;
    private readonly ISummaryFiles summaryFiles;
    private readonly int rightTabCount;
    private readonly Dictionary<string, FileSortingVM> filesToVm = new();

    public FilesContainer(ObservableCollection<TabInfo> tabCollection, ISummaryFiles summaryFiles)
    {
        this.tabCollection = tabCollection;
        this.summaryFiles = summaryFiles;
        rightTabCount = tabCollection.Count;
    }

    public bool HasFile(string fullPath) => filesToVm.ContainsKey(fullPath);

    public void Add(string fullPath, FileSortingVM fileSortingVM)
    {
        tabCollection.Insert(tabCollection.Count - rightTabCount, fileSortingVM.TabInfo);
        filesToVm.Add(fullPath, fileSortingVM);
        summaryFiles.Register(fileSortingVM);
    }

    public void CloseFile(FileSortingVM viewModel)
    {
        TabInfo tabToRemove = tabCollection.First(tab => tab.Content == viewModel);
        tabCollection.Remove(tabToRemove);
        string fileToDell = filesToVm.First(p => p.Value == viewModel).Key;
        filesToVm.Remove(fileToDell);
        summaryFiles.Unregister(viewModel);
    }
}