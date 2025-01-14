﻿using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;
using System.Windows.Input;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class FileSortingVM : BaseNotifyProperty, ISummaryChangedListener
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

    public ICommand ResetFiltersCommand { get; }
    public ICommand ApproveAllCommand { get; }

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
        TabInfo = new TabInfo(file.GetTitle(), this);
        TextSummaryVM = new SingleCurrencyTextSummaryVM();

        ResetFiltersCommand = new DelegateCommand(ResetFiltersHandler);
        ApproveAllCommand = new DelegateCommand(ApproveAllHandler);

        UpdateSummary();

        categoriesVM.CategoryOrListChanged += CategoriesVMOnCategoryOrListChanged;
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

    private void UpdateSummary()
    {
        SummaryHelperSingleCurrency.PrepareSummary(
            categoriesVM.GetCategories(), OperationsVM.Operations, groupByComment,
            out ICollection<CategoryDetails> collection);
        TextSummaryVM.Update(collection);
        summaryNotifier.NotifySummaryChanged();
    }

    private void CategoriesVMOnCategoryOrListChanged()
    {
        saveController.MarkChanged();
        UpdateSummary();
    }

    private void ResetFiltersHandler()
    {
        OperationsVM.ResetFilters();
        UpdateSummary();
    }

    private void ApproveAllHandler()
    {
        foreach (OperationVM operationVM in OperationsVM.Operations)
        {
            if (operationVM is { IsAutoMappedNotApproved: true, Category.IsDefault: false })
                operationVM.IsAutoMappedNotApproved = false;
        }
    }

    void ISummaryChangedListener.SummaryDescriptionChanged() => UpdateSummary();

    void ISummaryChangedListener.IsSortedChanged()
        => TabInfo.IsHighlighted = !SummaryHelperSingleCurrency.GetIsSorted(OperationsVM.Operations);
}

interface ISummaryChangedListener
{
    void SummaryDescriptionChanged();
    void IsSortedChanged();
}