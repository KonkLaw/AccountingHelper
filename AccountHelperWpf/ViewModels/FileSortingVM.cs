﻿using AccountHelperWpf.Models;
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

    public OperationsVM OperationsVM
    {
        get => operationsVM;
        set => SetProperty(ref operationsVM, value);
    }

    public ICommand SetForAllCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand ApproveAllCommand { get; }

    public SummaryVM SummaryVM { get; }

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
        SummaryVM = new SummaryVM();

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
            categoriesVM.GetCategories(), operationsVM.Operations, groupByComment,
            out bool isSorted, out ICollection<CategoryDetails> collection);
        TabInfo.IsHighlighted = !isSorted;
        SummaryVM.Update(collection);
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