using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class MainWindowVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    private readonly FilesContainer filesContainer;
    private readonly SaveController saveController;
    private readonly AssociationStorage associationStorage;
    private readonly CategoriesVM categoriesVM;

    public ICommand LoadOperationFileCommand { get; }
    public ICommand SaveAssociation { get; }
    public ICommand WindowClosing { get; }

    public ObservableCollection<TabInfo> Tabs { get; } = new ();

    public MainWindowVM(IViewResolver viewResolver, InitData initData)
    {
        this.viewResolver = viewResolver;
        saveController = new SaveController(viewResolver, initData);
        associationStorage = new AssociationStorage(initData.Associations, initData.ExcludedOperations, saveController);
        InitCategories(viewResolver, associationStorage, Tabs, initData, out categoriesVM, out var generalSummaryVM);
        filesContainer = new FilesContainer(Tabs, generalSummaryVM);

        LoadOperationFileCommand = new DelegateCommand(LoadOperationFile);
        SaveAssociation = new DelegateCommand(saveController.Save);
        WindowClosing = new DelegateCommand<CancelEventArgs>(WindowClosingHandler);
    }

    private void LoadOperationFile()
    {
        OpenFileDialog fileDialog = new() { Filter = "csv|*.csv" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string fullPath = fileDialog.FileName;
            if (filesContainer.HasFile(fullPath))
            {
                viewResolver.ShowWarning("File already added");
                return;
            }
            OperationsFile? operationsFile = ParserChooser.ParseFile(fullPath, viewResolver);
            if (operationsFile == null)
                return;
            var fileSortingVM = new FileSortingVM(operationsFile, categoriesVM, associationStorage, RemoveHandler, saveController);
            filesContainer.Add(fullPath, fileSortingVM);
        }
    }

    private void WindowClosingHandler(CancelEventArgs? arg) => arg!.Cancel = !saveController.RequestForClose();

    private void RemoveHandler(object viewModel)
    {
        if (viewResolver.ShowYesNoQuestion("Are you sure you want to remove current file from sorting?"))
        {
            filesContainer.CloseFile(viewModel);
        }
    }

    private static void InitCategories(
        IViewResolver viewResolver,
        AssociationStorage storage,
        ObservableCollection<TabInfo> tabs,
        InitData initData,
        out CategoriesVM categoriesVM,
        out GeneralSummaryVM generalSummaryVM)
    {
        categoriesVM = new CategoriesVM(initData.Categories, viewResolver);
        tabs.Add(new TabInfo("Categories", categoriesVM));
        var associationVM = new AssociationsVM(storage);
        tabs.Add(new TabInfo("Associations", associationVM));
        generalSummaryVM = new GeneralSummaryVM();
        tabs.Add(new TabInfo("Summary", generalSummaryVM));
    }
}