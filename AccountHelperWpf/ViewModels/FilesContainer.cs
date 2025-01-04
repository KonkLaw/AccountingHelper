using System.Collections.ObjectModel;

namespace AccountHelperWpf.ViewModels;

class FilesContainer
{
    private readonly ObservableCollection<TabInfo> tabCollection;
    private readonly SummaryVM summaryVM;
    private readonly int rightTabCount;
    private readonly Dictionary<string, FileSortingVM> filesToVm = new();

    public FilesContainer(ObservableCollection<TabInfo> tabCollection, SummaryVM summaryVM)
    {
        this.tabCollection = tabCollection;
        this.summaryVM = summaryVM;
        rightTabCount = tabCollection.Count;
    }

    public bool HasFile(string fullPath) => filesToVm.ContainsKey(fullPath);

    public void Add(string fullPath, FileSortingVM fileSortingVM)
    {
        tabCollection.Insert(tabCollection.Count - rightTabCount, fileSortingVM.TabInfo);
        filesToVm.Add(fullPath, fileSortingVM);
        summaryVM.Register(fileSortingVM);
        UpdateCurrencies();
    }

    public void CloseFile(FileSortingVM viewModel)
    {
        TabInfo tabToRemove = tabCollection.First(tab => tab.Content == viewModel);
        tabCollection.Remove(tabToRemove);
        string fileToDell = filesToVm.First(p => p.Value == viewModel).Key;
        filesToVm.Remove(fileToDell);
        summaryVM.Unregister(viewModel);
        UpdateCurrencies();
    }

    private void UpdateCurrencies() => summaryVM.UpdateCurrencies(GetAllCurrencies());

    public IReadOnlyList<string> GetAllCurrencies()
    {
        List<string> currencies = filesToVm
            .Select(pair => pair.Value.File.Currency)
            .GroupBy(currency => currency)
            .Select(grouping => grouping.Key).ToList();
        return currencies;
    }
}