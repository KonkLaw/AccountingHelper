using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class FilesSortingViewModel
{
    private readonly IViewResolver viewResolver;
    private readonly CategoriesViewModel categoriesViewModel;
    private readonly Dictionary<string, FileSortingViewModel> filesVm = new ();

    public ObservableCollection<TabInfo> Tabs { get; } = new ();
    public ICommand LoadFile { get; }

    public FilesSortingViewModel(IViewResolver viewResolver, List<CategoryViewModel> categories)
    {
        this.viewResolver = viewResolver;
        LoadFile = new DelegateCommand(LoadFileHandler);
        categoriesViewModel = new CategoriesViewModel(categories);
        Tabs.Add(new TabInfo("Categories", categoriesViewModel) { IsSorted = true });
    }

    private void LoadFileHandler()
    {
        OpenFileDialog fileDialog = new() { Filter = "csv|*.csv" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string fullPath = fileDialog.FileName;
            if (filesVm.ContainsKey(fullPath))
            {
                viewResolver.ShowWarning("File already added");
                return;
            }
            AccountFile? accountFile = ParserChooser.ParseFile(fullPath, viewResolver);
            if (accountFile == null)
            {
                viewResolver.ShowWarning("Sorry, the fle wasn't recognized as any known bank report.");
                return;
            }
            FileSortingViewModel fileSortingViewModel = new (accountFile, categoriesViewModel, RemoveHandler);
            Tabs.Add(fileSortingViewModel.GetTabItem());
            filesVm.Add(fullPath, fileSortingViewModel);
        }
    }

    private void RemoveHandler(object viewModel)
    {
        if (viewResolver.ShowYesNoDialog("Are you sure you want to remove current file from sorting?"))
        {
            TabInfo tabToRemove = Tabs.First(tab => tab.Content == viewModel);
            Tabs.Remove(tabToRemove);
            string fileToDell = filesVm.First(p => p.Value == viewModel).Key;
            filesVm.Remove(fileToDell);
        }
    }
}