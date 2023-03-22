using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class MainWindowModel : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    private readonly CategoriesViewModel categoriesViewModel;
    private readonly AssociationViewModel associationViewModel;

    private readonly Dictionary<string, FileSortingViewModel> filesVm = new ();

    public ICommand LoadOperationFile { get; }
    public ICommand LoadAssociation { get; }
    public ICommand WindowClosing { get; }

    public ObservableCollection<TabInfo> Tabs { get; } = new ();
    
    public MainWindowModel(IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        LoadOperationFile = new DelegateCommand(LoadOperationFileHandler);
        LoadAssociation = new DelegateCommand(LoadAssociationHandler);
        WindowClosing = new DelegateCommand(WindowClosingHandler);

        List<CategoryViewModel> defaultCategoriesList =  new()
        {
            new() { Name = "Пополнения", Description = "Пополнения" },
            new() { Name = "Подарки", Description = "Подарки" },
            new() { Name = "Здоровье", Description = "Траты на здоровье" }
        };
        categoriesViewModel = new CategoriesViewModel(defaultCategoriesList);
        Tabs.Add(new TabInfo("Categories", categoriesViewModel) { IsSorted = true });


        associationViewModel = new AssociationViewModel();
    }

    private void LoadOperationFileHandler()
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

    private void LoadAssociationHandler()
    {
        OpenFileDialog fileDialog = new() { Filter = "zip|*.zip" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            associationViewModel.Load(fileDialog.FileName);
        }
    }

    private void WindowClosingHandler()
    {
        
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