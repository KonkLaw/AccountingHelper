using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;
using AccountHelperWpf.ViewUtils;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class MainWindowVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    private readonly FilesContainer filesContainer;
    private readonly SaveController saveController;
    private readonly AssociationsManager associationsManager;
    private readonly CategoriesVM categoriesVM;
    private readonly SummaryVM summaryVM;

    private FileSortingVM? fileSortingVM;

    public ICommand LoadOperationFileCommand { get; }
    public ICommand SaveAssociation { get; }
    public ICommand RemoveFile { get; }
    public ICommand SetForAll { get; }
    public ICommand About { get; }
    public ICommand WindowClosing { get; }

    private TabInfo? selectedTab;
    public TabInfo? SelectedTab
    {
        get => selectedTab;
        set
        {
            if (SetProperty(ref selectedTab, value))
            {
                fileSortingVM = selectedTab?.Content as FileSortingVM;
                OnPropertyChanged(nameof(IsFileTab));
            }
        }
    }

    public bool IsFileTab => fileSortingVM != null;

    public ObservableCollection<TabInfo> Tabs { get; } = new ();

    public MainWindowVM(IViewResolver viewResolver, InitData initData, string? optionalFile)
    {
        this.viewResolver = viewResolver;
        saveController = new SaveController(viewResolver, initData);
        associationsManager = new AssociationsManager(initData.AssociationStorage);
        InitCategories(viewResolver, associationsManager, Tabs, initData, out categoriesVM, out summaryVM);
        filesContainer = new FilesContainer(Tabs, summaryVM);

        LoadOperationFileCommand = new DelegateCommand(LoadOperationFile);
        SaveAssociation = new DelegateCommand(saveController.Save);
        RemoveFile = new DelegateCommand(RemoveFileHandler);
        SetForAll = new DelegateCommand(SetForAllHandler);
        About = new DelegateCommand(ShowAbout);
        WindowClosing = new DelegateCommand<CancelEventArgs>(WindowClosingHandler);

        if (optionalFile != null)
            LoadFile(optionalFile);
    }

    private void ShowAbout()
    {
        Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        string info = version == null ? "" : version.ToString();
        string content = "Account Helper v" + info + Environment.NewLine +
                         "Made by KonkLaw" + Environment.NewLine +
                         "https://github.com/KonkLaw/AccountingHelper";
        viewResolver.ShowInfo(content, "About");
    }

    private void LoadOperationFile()
    {
        OpenFileDialog fileDialog = new() { Filter = "csv|*.csv" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            LoadFile(fileDialog.FileName);
        }
    }

    private void LoadFile(string fullPath)
    {
        if (filesContainer.HasFile(fullPath))
        {
            viewResolver.ShowWarning("File already added");
            return;
        }
        OperationsFile? operationsFile = ParserChooser.ParseFile(fullPath, viewResolver);
        if (operationsFile == null)
            return;
        var newFileSortingVM = new FileSortingVM(
            operationsFile, categoriesVM, associationsManager, saveController, summaryVM);
        associationsManager.AddListener(newFileSortingVM.OperationsVM);
        filesContainer.Add(fullPath, newFileSortingVM);
        SelectedTab = newFileSortingVM.TabInfo;
    }

    private void WindowClosingHandler(CancelEventArgs arg) => arg.Cancel = !saveController.RequestForClose();

    private void SetForAllHandler()
    {
        // default for select is not supported
        CategorySelectorWindow window = new(
            categoriesVM.GetCategories().Where(c => !c.IsDefault).ToList());

        window.ShowDialog();
        if (window.SelectedItem == null)
            return;

        Category selectedItem = (Category)window.SelectedItem;
        fileSortingVM!.SetCategoryForAllNonEmpty(selectedItem);
    }

    private void RemoveFileHandler()
    {
        FileSortingVM viewModel = fileSortingVM!;
        if (viewResolver.ShowYesNoQuestion("Are you sure you want to remove current file from sorting?"))
        {
            filesContainer.CloseFile(viewModel);
            associationsManager.RemoveListener(viewModel.OperationsVM);
        }
    }

    private static void InitCategories(
        IViewResolver viewResolver,
        AssociationsManager storage,
        ObservableCollection<TabInfo> tabs,
        InitData initData,
        out CategoriesVM categoriesVM,
        out SummaryVM summaryVM)
    {
        categoriesVM = new CategoriesVM(initData.Categories, viewResolver);
        tabs.Add(new TabInfo("Categories", categoriesVM));
        var associationsVM = new AssociationsVM(storage);
        tabs.Add(new TabInfo("Associations", associationsVM));
        summaryVM = new SummaryVM();
        tabs.Add(new TabInfo("Summary", summaryVM));
    }
}