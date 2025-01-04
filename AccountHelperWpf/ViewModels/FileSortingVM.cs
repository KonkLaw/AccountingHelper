using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;
using System.Windows.Input;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class FileSortingVM : BaseNotifyProperty
{
    public readonly OperationsFile File;
    private readonly CategoriesVM categoriesVM;
    private readonly ISaveController saveController;
    private readonly ISummaryNotifier summaryNotifier;
    public TabInfo TabInfo { get; }

    private bool groupByComment = true;
    public bool GroupByComment
    {
        get => groupByComment;
        set
        {
            if (SetProperty(ref groupByComment, value))
                UpdateSummary();
        }
    }

    public OperationsVM OperationsVM { get; }

    public ICommand SetForAllCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand ApproveAllCommand { get; }

    public SingleCurrencyTextSummaryVM TextSummaryVM { get; }

    public FileSortingVM(
        OperationsFile file,
        CategoriesVM categoriesVM,
        AssociationStorage associationStorage,
        Action<FileSortingVM> removeHandler,
        ISaveController saveController,
        ISummaryNotifier summaryNotifier)
    {
        File = file;
        this.categoriesVM = categoriesVM;
        this.saveController = saveController;
        this.summaryNotifier = summaryNotifier;
        OperationsVM = new OperationsVM(file.Operations, file.ColumnDescriptions, categoriesVM, UpdateSummary, associationStorage);
        TabInfo = new TabInfo(file.GetTitle(), this);
        TextSummaryVM = new SingleCurrencyTextSummaryVM();

        SetForAllCommand = new DelegateCommand(SetForAllHandler);
        ResetFiltersCommand = new DelegateCommand(ResetFiltersHandler);
        RemoveFileCommand = new DelegateCommand(() => removeHandler(this));
        ApproveAllCommand = new DelegateCommand(ApproveHandler);

        UpdateSummary();

        categoriesVM.CategoryOrListChanged += CategoriesVMOnCategoryOrListChanged;
    }

    private void UpdateSummary()
    {
        SummaryHelperSingleCurrency.PrepareSummary(
            categoriesVM.GetCategories(), OperationsVM.Operations, groupByComment,
            out bool isSorted, out ICollection<CategoryDetails> collection);
        TabInfo.IsHighlighted = !isSorted;
        TextSummaryVM.Update(collection);
        summaryNotifier.NotifySummaryChanged();
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