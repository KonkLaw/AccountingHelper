using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Utils;
using AccountHelperWpf.Views;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationsVM : BaseNotifyProperty, IAssociationStorageListener
{
    private readonly IAssociationsManager associationsManager;
    private readonly List<OperationVM> allOperations;
    private readonly BatchProcessingMutex mutex = new ();

    private readonly Action summaryChanged;
    private readonly Action updateIsSorted;

    private OperationVM? firstIncluded;
    private OperationVM? lastIncluded;

    public IEnumerable<CategoryVM> Categories { get; }

    public IEnumerable<ColumnDescription> ColumnDescriptions { get; }

    private IReadOnlyList<OperationVM> operations = null!;
    public IReadOnlyList<OperationVM> Operations
    {
        get => operations;
        set => SetProperty(ref operations, value);
    }

    private IList? selectedItems;
    public IList? SelectedItems
    {
        get => selectedItems;
        set
        {
            SetProperty(ref selectedItems, value);
            // We don't check equality as reference to collection is the same
            // however count is different
            IsSingleSelection = selectedItems is { Count: 1 };
        }
    }

    private bool isSingleSelection;
    public bool IsSingleSelection
    {
        get => isSingleSelection;
        set => SetProperty(ref isSingleSelection, value);
    }

    public ICommand SearchInfoCommand { get; }
    public ICommand SetLastOperationCommand { get; }
    public ICommand SetFirstOperationCommand { get; }
    public ICommand ApplyCategoryForSimilarOperationsCommand { get; }
    public ICommand AddExceptionCommand { get; }
    public ICommand ApproveSelectedCommand { get; }
    public ICommand AddCommand { get; }

    public OperationsVM(IReadOnlyList<BaseOperation> baseOperations,
        IEnumerable<ColumnDescription> columnDescriptions,
        CategoriesVM categoriesVM,
        Action summaryChanged, Action updateIsSorted,
        IAssociationsManager associationsManager)
    {
        Categories = categoriesVM.GetCategories();
        ColumnDescriptions = columnDescriptions;
        this.summaryChanged = summaryChanged;
        this.updateIsSorted = updateIsSorted;
        this.associationsManager = associationsManager;
        allOperations = GetAllOperations(baseOperations);
        UpdateByFilter();

        categoriesVM.OnCategoryRemoving += CategoriesVMOnOnCategoryRemoving;
        categoriesVM.OnCategoryRemoved += CategoriesVMOnOnCategoryRemoved;
        
        SearchInfoCommand = new DelegateCommand(SearchInfo);
        SetLastOperationCommand = new DelegateCommand(SetLastOperation);
        SetFirstOperationCommand = new DelegateCommand(SetFirstOperation);
        ApplyCategoryForSimilarOperationsCommand = new DelegateCommand(ApplyCategoryForSimilarOperations);
        AddExceptionCommand = new DelegateCommand(AddException);
        ApproveSelectedCommand = new DelegateCommand(ApproveSelectedHandler);
        AddCommand = new DelegateCommand<OperationVM>(AddAssociation);
    }

    private List<OperationVM> GetAllOperations(IReadOnlyList<BaseOperation> baseOperations)
    {
        var result = new List<OperationVM>(baseOperations.Count);
        foreach (BaseOperation operation in baseOperations)
        {
            Association? association = associationsManager.TryGetBestAssociation(operation.Description);
            CategoryVM category = association == null ? CategoryVM.Default : association.Category;
            OperationVM operationVM = new(operation, category)
            {
                // regardless existing of prediction - it's auto-mapped
                IsAutoMappedNotApproved = true,
                Association = association
            };
            result.Add(operationVM);
            operationVM.PropertyChanged += OperationViewModelOnPropertyChanged;
        }
        return result;
    }

    private void OperationViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (mutex.IsBatchProcessing)
            return;

        switch (e.PropertyName)
        {
            case nameof(OperationVM.Category):
            {
                using var _ = mutex.EnterBatchProcessing();
                CategoryVM newCategory = ((OperationVM)sender!).Category;
                foreach (OperationVM operationVM in SelectedItems.CheckNull())
                {
                    operationVM.Category = newCategory;
                    operationVM.IsAutoMappedNotApproved = false;
                }
                summaryChanged();
                updateIsSorted();
                break;
            }
            case nameof(OperationVM.Comment):
                summaryChanged();
                break;
            case nameof(OperationVM.AssociationStatus):
            case nameof(OperationVM.IsAutoMappedNotApproved):
                updateIsSorted();
                break;
        }
    }

    private void CategoriesVMOnOnCategoryRemoving(CategoryVM categoryToRemove)
    {
        using var _ = mutex.EnterBatchProcessing();
        foreach (OperationVM operation in allOperations)
        {
            if (operation.Category == categoryToRemove)
            {
                operation.Category = CategoryVM.Default;
            }
        }
        associationsManager.RemoveAssociation(categoryToRemove);
    }

    private void CategoriesVMOnOnCategoryRemoved()
    {
        summaryChanged();
        updateIsSorted();
    }

    private void UpdateByFilter()
    {
        var filteredOperations = new List<OperationVM>(allOperations.Count);
        bool skip = lastIncluded != null;
        foreach (OperationVM operationVM in allOperations)
        {
            if (skip && (skip = operationVM != lastIncluded))
                continue;
            filteredOperations.Add(operationVM);
            if (operationVM == firstIncluded)
                break;
        }
        Operations = filteredOperations;
    }

    public void ResetFilters()
    {
        firstIncluded = null;
        lastIncluded = null;
        UpdateByFilter();
    }

    private void SearchInfo()
    {
        string searchQuery = GetSelectedOperation().Operation.Description;
        string encodedQuery = Uri.EscapeDataString(searchQuery);
        string searchUrl = $"https://www.google.com/search?q={encodedQuery}";
        // Starts the default web browser with the search URL
        Process.Start(new ProcessStartInfo
        {
            FileName = searchUrl,
            UseShellExecute = true
        });
    }

    private void SetFirstOperation()
    {
        firstIncluded = GetSelectedOperation();
        UpdateByFilter();
        summaryChanged();
    }

    private void SetLastOperation()
    {
        lastIncluded = GetSelectedOperation();
        UpdateByFilter();
        summaryChanged();
    }

    private void ApplyCategoryForSimilarOperations()
    {
        OperationVM selectedOperation = GetSelectedOperation();
        string description = selectedOperation.Operation.Description;
        IReadOnlyList<OperationVM> collection = CollectionSearchHelper.FindAll(description, Operations, op => op.Operation.Description);
        foreach (OperationVM operation in collection)
        {
            operation.Category = selectedOperation.Category;
            operation.IsAutoMappedNotApproved = true;
        }
    }

    private void AddException()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            associationsManager.AddOrUpdateAssociation(operationViewModel.Operation.Description, CategoryVM.Default);
    }

    private OperationVM GetSelectedOperation() => (OperationVM)selectedItems![0]!;

    private void ApproveSelectedHandler()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsAutoMappedNotApproved = false;
    }

    private void AddAssociation(OperationVM? obj)
    {
        OperationVM operationVM = obj!;
        associationsManager.AddOrUpdateAssociation(operationVM.Operation.Description, operationVM.Category);
    }


    void IAssociationStorageListener.AssociationChanged(Association? oldAssociation, Association newAssociation)
    {
        using var _ = mutex.EnterBatchProcessing();

        if (oldAssociation == null)
        {
            IReadOnlyList<OperationVM> collection = CollectionSearchHelper.FindAll(
                newAssociation.OperationDescription, allOperations, op => op.Operation.Description);
            foreach (OperationVM operation in collection)
            {
                operation.Association = newAssociation;
                if (operation.IsAutoMappedNotApproved)
                {
                    operation.Category = newAssociation.Category;
                }
            }
        }
        else
        {
            foreach (OperationVM operation in allOperations)
            {
                if (operation.Association != null && operation.Association.Equals(oldAssociation))
                {
                    operation.Association = newAssociation;
                    if (operation.IsAutoMappedNotApproved)
                    {
                        operation.Category = newAssociation.Category;
                    }
                }
            }
        }

        summaryChanged();
        updateIsSorted();
    }

    void IAssociationStorageListener.AssociationRemoved(Association association)
    {
        using var _ = mutex.EnterBatchProcessing();
        foreach (OperationVM operation in allOperations)
        {
            if (operation.Association != null && operation.Association.Equals(association))
            {
                Association? newAssociation = associationsManager.TryGetBestAssociation(operation.Operation.Description);
                operation.Association = newAssociation;
                if (operation.IsAutoMappedNotApproved)
                {
                    if (newAssociation == null)
                        operation.Category = CategoryVM.Default;
                    else
                        operation.Category = newAssociation.Category;
                    ;
                } // else leave user choice
            }
        }
        summaryChanged();
        updateIsSorted();
    }
}

public class BatchProcessingMutex
{
    public bool IsBatchProcessing { get; private set; }

    public Guard EnterBatchProcessing() => new(this);

    public readonly struct Guard : IDisposable
    {
        private readonly BatchProcessingMutex mutex;

        public Guard(BatchProcessingMutex mutex)
        {
            this.mutex = mutex;
            mutex.IsBatchProcessing = true;
        }

        public void Dispose()
        {
            mutex.IsBatchProcessing = false;
        }
    }
}