using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;
using System.Windows.Input;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class FileSortingVM : BaseNotifyProperty
{
    private readonly CategoriesVM categoriesVM;
    private readonly ISaveController saveController;
    public TabInfo TabInfo { get; }
    private OperationsVM operationsVM;

    public OperationsVM OperationsVM
    {
        get => operationsVM;
        set => SetProperty(ref operationsVM, value);
    }

    private string summary = string.Empty;
    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    public ICommand SetForAllCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand ApproveAllCommand { get; }

    public FileSortingVM(
        OperationsFile file,
        CategoriesVM categoriesVM,
        AssociationStorage associationStorage,
        Action<object> removeHandler,
        ISaveController saveController)
    {
        this.categoriesVM = categoriesVM;
        this.saveController = saveController;
        operationsVM = new OperationsVM(file.Operations, categoriesVM, UpdateSummary, associationStorage);
        TabInfo = new TabInfo(file.Name, this);

        SetForAllCommand = new DelegateCommand(SetForAllHandler);
        ResetFiltersCommand = new DelegateCommand(ResetFiltersHandler);
        RemoveFileCommand = new DelegateCommand(() => removeHandler(this));
        ApproveAllCommand = new DelegateCommand(ApproveHandler);

        UpdateSummary();

        categoriesVM.CategoryOrListChanged += CategoriesVMOnCategoryOrListChanged;
    }

    private void UpdateSummary()
    {
        SummaryHelper.PrepareSummary(
        categoriesVM.GetCategories(), operationsVM.Operations,
            out bool isSorted, out SummaryVM summaryVM);
        TabInfo.IsSorted = isSorted; ;
        Summary = summaryVM.GetDescription().ToString();
    }

    private void CategoriesVMOnCategoryOrListChanged()
    {
        saveController.MarkChanged();
        UpdateSummary();
    }

    private void SetForAllHandler()
    {
        ObjectSelectorWindow window = new(categoriesVM.GetCategories());
        window.ShowDialog();
        if (window.SelectedItem == null)
            return;
        CategoryVM selectedItem = (CategoryVM)window.SelectedItem;
        foreach (OperationVM operation in OperationsVM.Operations)
        {
            operation.Category ??= selectedItem;
        }
    }

    private void ResetFiltersHandler()
    {
        OperationsVM.ResetFilters();
        UpdateSummary();
    }

    private void ApproveHandler()
    {
        foreach (OperationVM operationVM in OperationsVM.Operations)
            operationVM.IsApproved = true;
    }
}