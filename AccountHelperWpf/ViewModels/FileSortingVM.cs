using System.ComponentModel;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class FileSortingVM : BaseNotifyProperty, ISummaryChangedListener
{
    public readonly OperationsFile File;
    private readonly CategoriesVM categoriesVM;
    private readonly ISaveController saveController;
    private readonly ISummaryNotifier summaryNotifier;
    public TabInfo TabInfo { get; }

    public OperationsVM OperationsVM { get; }

    public SingleCurrencyTextSummaryVM TextSummaryVM { get; }

    public FileSortingVM(
        OperationsFile file,
        CategoriesVM categoriesVM,
        IAssociationsManager associationsManager,
        ISaveController saveController,
        ISummaryNotifier summaryNotifier)
    {
        File = file;
        this.categoriesVM = categoriesVM;
        this.saveController = saveController;
        this.summaryNotifier = summaryNotifier;
        OperationsVM = new OperationsVM(file.Operations, file.ColumnDescriptions, categoriesVM, associationsManager, this);
        TabInfo = new TabInfo(TabInfo.TabTypeEnum.File, file.GetTitle(), this);
        TextSummaryVM = new SingleCurrencyTextSummaryVM();
        TextSummaryVM.PropertyChanged += TextSummaryVMOnPropertyChanged;

        UpdateSummary();
        UpdateIsSorted();

        categoriesVM.CategoryOrListChanged += CategoriesVMOnCategoryOrListChanged;
    }

    void TextSummaryVMOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BaseTextSummaryVM.GroupByComment))
            UpdateSummary();
    }

    public void SetCategoryForAllNonEmpty(Category selectedItem)
    {
        foreach (OperationVM operation in OperationsVM.Operations)
        {
            if (operation.Category.IsDefault)
            {
                operation.Category = selectedItem;
            }
        }
    }

    public void UpdateSummary()
    {
        SummaryHelperSingleCurrency.PrepareSummary(
            categoriesVM.GetCategories(), OperationsVM.Operations, TextSummaryVM.GroupByComment,
            out ICollection<CategoryDetails> collection);
        TextSummaryVM.Update(collection);
        summaryNotifier.NotifySummaryChanged();
    }

    public void UpdateIsSorted()
        => TabInfo.IsHighlighted = !SummaryHelperSingleCurrency.GetIsSorted(OperationsVM.Operations);

    private void CategoriesVMOnCategoryOrListChanged()
    {
        saveController.MarkChanged();
        UpdateSummary();
    }
}

interface ISummaryChangedListener
{
    void UpdateSummary();
    void UpdateIsSorted();
}