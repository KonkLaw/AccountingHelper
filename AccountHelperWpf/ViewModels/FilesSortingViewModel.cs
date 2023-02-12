using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class FilesSortingViewModel
{
    private readonly IViewResolver viewResolver;
    private readonly CategoriesViewModel categoriesViewModel;

    public ObservableCollection<TabItem> Tabs { get; } = new ();
    public DelegateCommand Next { get; }

    public ICommand LoadFile { get; }

    private readonly List<(string fullPath, FileSortingViewModel vm)> filesVm = new ();

    public FilesSortingViewModel(IViewResolver viewResolver, List<CategoryVm> categories)
    {
        this.viewResolver = viewResolver;
        Next = new DelegateCommand(NextHandler);
        LoadFile = new DelegateCommand(LoadFileHandler);
        categoriesViewModel = new CategoriesViewModel(categories);
        Tabs.Add(viewResolver.ResolveTabItem("Categories", categoriesViewModel));
        //Tabs.Add(viewResolver.ResolveTabItem("History", new HistoryViewModel()));
        UpdateNextButtonState();
    }

    private void LoadFileHandler()
    {
        OpenFileDialog fileDialog = new() { Filter = "csv|*.csv" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string fullPath = fileDialog.FileName;
            if (filesVm.Any(f => f.fullPath == fullPath))
            {
                viewResolver.ShowWarning("File already added");
                return;
            }
            AccountFile accountFile = ParserChooser.ParseFile(fullPath, viewResolver);
            FileSortingViewModel fileSortingViewModel = new (accountFile, categoriesViewModel.GetCategories(), SortedChangedHandler, RemoveHandler);
            Tabs.Add(viewResolver.ResolveTabItem(accountFile.Description.Name, fileSortingViewModel));
            filesVm.Add((fullPath, fileSortingViewModel));
        }
    }

    private void RemoveHandler(object viewModel)
    {
        if (viewResolver.ShowYesNoDialog("Are you sure you want to remove current file from sorting?"))
            Tabs.Remove(Tabs.First(tab => ((FrameworkElement)tab.Content).DataContext == viewModel));
    }

    private void UpdateNextButtonState() => Next.IsEnabled = filesVm.All(f => f.vm.IsSorted);

    private void SortedChangedHandler() { }
    private void NextHandler() { }
}