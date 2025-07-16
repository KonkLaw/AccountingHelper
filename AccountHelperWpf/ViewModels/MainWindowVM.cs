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

class MainWindowVM : BaseNotifyProperty, INavigationHelper, ISortedInfoOwner
{
    private readonly IViewResolver viewResolver;
    private readonly FilesContainer filesContainer;
    private readonly SaveController saveController;
    private readonly AssociationsManager associationsManager;
    private readonly CategoriesVM categoriesVM;
    private readonly TabInfo associationsTab;
    private readonly AssociationsVM associationsVM;
    private readonly SummaryVM summaryVM;

    private FileSortingVM? fileSortingVM;

    public ICommand LoadOperationFileCommand { get; }
    public ICommand SaveAssociationCommand { get; }
    public ICommand SetCategoryForAllCommand { get; }
    public ICommand ResetTimeFilterCommand { get; }
    public ICommand ApproveAllCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand WindowClosingCommand { get; }

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

    private bool highlightNotSorted = true;

    public bool HighlightNotSorted
    {
        get => highlightNotSorted;
        set => SetProperty(ref highlightNotSorted, value);
    }

    public MainWindowVM(IViewResolver viewResolver, InitData initData, string? optionalFile)
    {
        this.viewResolver = viewResolver;
        saveController = new SaveController(viewResolver, initData);
        associationsManager = new AssociationsManager(initData.AssociationStorage);
        InitCategories(viewResolver, associationsManager, Tabs, initData,
            out categoriesVM, out associationsTab, out associationsVM, out summaryVM);
        filesContainer = new FilesContainer(Tabs, summaryVM);

        LoadOperationFileCommand = new DelegateCommand(LoadOperationFile);
        SaveAssociationCommand = new DelegateCommand(saveController.Save);
        SetCategoryForAllCommand = new DelegateCommand(SetCategoryForAll);
        ResetTimeFilterCommand = new DelegateCommand(ResetTimeFilters);
        ApproveAllCommand = new DelegateCommand(ApproveAll);
        RemoveFileCommand = new DelegateCommand(RemoveFile);
        AboutCommand = new DelegateCommand(ShowAbout);
        WindowClosingCommand = new DelegateCommand<CancelEventArgs>(WindowClosing);

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
            operationsFile, categoriesVM, associationsManager,
            saveController, summaryVM, this, this);
        associationsManager.AddListener(newFileSortingVM.OperationsVM);
        filesContainer.Add(fullPath, newFileSortingVM);
        SelectedTab = newFileSortingVM.TabInfo;
    }

    private void WindowClosing(CancelEventArgs arg) => arg.Cancel = !saveController.RequestForClose();

    private void SetCategoryForAll()
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

    private void ResetTimeFilters()
    {
        FileSortingVM viewModel = fileSortingVM!;
        viewModel.OperationsVM.ResetFilters();
    }

    private void ApproveAll()
    {
        FileSortingVM viewModel = fileSortingVM!;
        foreach (OperationVM operationVM in viewModel.OperationsVM.Operations)
        {
            if (operationVM is { IsAutoMappedNotApproved: true, Category.IsDefault: false })
                operationVM.IsAutoMappedNotApproved = false;
        }
    }

    private void RemoveFile()
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
        out TabInfo associationsTab,
        out AssociationsVM associationsVM,
        out SummaryVM summaryVM)
    {
        categoriesVM = new CategoriesVM(initData.Categories, viewResolver);
        tabs.Add(new TabInfo(TabInfo.TabTypeEnum.Category, "Categories", categoriesVM));
        associationsVM = new AssociationsVM(storage);
        associationsTab = new TabInfo(TabInfo.TabTypeEnum.Associations, "Associations", associationsVM);
        tabs.Add(associationsTab);
        summaryVM = new SummaryVM();
        tabs.Add(new TabInfo(TabInfo.TabTypeEnum.Summary, "Summary", summaryVM));
    }

    public void NavigateAndSelect(IAssociation association)
    {
        SelectedTab = associationsTab;
        if (association.Category.IsDefault)
            associationsVM.SelectedException = association;
        else
            associationsVM.SelectedAssociation = association;
    }
}

interface INavigationHelper
{
    void NavigateAndSelect(IAssociation association);
}

interface ISortedInfoOwner : INotifyPropertyChanged
{
    bool HighlightNotSorted { get; }
}