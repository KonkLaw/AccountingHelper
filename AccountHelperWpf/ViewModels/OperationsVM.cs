using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Utils;
using AccountHelperWpf.Views;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationsVM : BaseNotifyProperty, IAssociationStorageListener
{
    private readonly IAssociationsManager associationsManager;
    private readonly ISummaryChangedListener summaryChangedListener;
    private readonly List<OperationVM> allOperations;
    private readonly BatchProcessingMutex mutex = new ();

    private OperationVM? firstIncluded;
    private OperationVM? lastIncluded;

    public IEnumerable<Category> Categories { get; }

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
    public ICommand HighlightSimilarOperations { get; }
    public ICommand HighlightSameCategory { get; }
    public ICommand ResetHighlight { get; }

    public OperationsVM(IReadOnlyList<BaseOperation> baseOperations,
        IEnumerable<ColumnDescription> columnDescriptions,
        CategoriesVM categoriesVM,
        IAssociationsManager associationsManager,
        ISummaryChangedListener summaryChangedListener)
    {
        Categories = categoriesVM.GetCategories();
        ColumnDescriptions = columnDescriptions;
        this.associationsManager = associationsManager;
        this.summaryChangedListener = summaryChangedListener;
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
        AddCommand = new DelegateCommand<UIElement, OperationVM>(AddAssociation);
        HighlightSimilarOperations = new DelegateCommand(HighlightSimilarOperationsHandler);
        HighlightSameCategory = new DelegateCommand(HighlightSameCategoryHandler);
        ResetHighlight = new DelegateCommand(ResetHighlightHandler);
    }

    private List<OperationVM> GetAllOperations(IReadOnlyList<BaseOperation> baseOperations)
    {
        var result = new List<OperationVM>(baseOperations.Count);
        foreach (BaseOperation operation in baseOperations)
        {
            IAssociation? association = associationsManager.TryFindBestMatch(operation.Description);
            Category category = association == null ? Category.Default : association.Category;
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
                Category newCategory = ((OperationVM)sender!).Category;
                foreach (OperationVM operationVM in SelectedItems.CheckNull())
                {
                    operationVM.Category = newCategory;
                    operationVM.IsAutoMappedNotApproved = false;
                }
                summaryChangedListener.SummaryDescriptionChanged();
                summaryChangedListener.IsSortedChanged();
                break;
            }
            case nameof(OperationVM.Comment):
                summaryChangedListener.SummaryDescriptionChanged();
                break;
            case nameof(OperationVM.AssociationStatus):
            case nameof(OperationVM.IsAutoMappedNotApproved):
                summaryChangedListener.IsSortedChanged();
                break;
        }
    }

    private void CategoriesVMOnOnCategoryRemoving(Category categoryToRemove)
    {
        using var _ = mutex.EnterBatchProcessing();
        foreach (OperationVM operation in allOperations)
        {
            if (operation.Category == categoryToRemove)
            {
                operation.Category = Category.Default;
            }
        }
        associationsManager.RemoveAssociations(categoryToRemove);
    }

    private void CategoriesVMOnOnCategoryRemoved()
    {
        summaryChangedListener.SummaryDescriptionChanged();
        summaryChangedListener.IsSortedChanged();
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
        string searchQuery = GetSelectedOperation().Operation.Description.DisplayName;
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
        summaryChangedListener.SummaryDescriptionChanged();
    }

    private void SetLastOperation()
    {
        lastIncluded = GetSelectedOperation();
        UpdateByFilter();
        summaryChangedListener.SummaryDescriptionChanged();
    }

    private void ApplyCategoryForSimilarOperations()
    {
        OperationVM selectedOperation = GetSelectedOperation();
        IReadOnlyList<OperationVM> collection = GetSimilarOperations(selectedOperation.Operation.Description);
        foreach (OperationVM operation in collection)
        {
            operation.Category = selectedOperation.Category;
            operation.IsAutoMappedNotApproved = true;
        }
    }

    private void AddException()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            associationsManager.AddException(operationViewModel.Operation.Description);
    }

    private void ApproveSelectedHandler()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsAutoMappedNotApproved = false;
    }

    private void AddAssociation(UIElement button, OperationVM operation)
    {
        OperationVM operationVM = operation;
        var viewModel = new AssociationPopupVM(
            operationVM.Operation.Description, operationVM.Category,
            associationsManager);
        _ = new AssociationPopup
        {
            DataContext = viewModel,
            PlacementTarget = button,
            Focusable = true
        };
    }

    private void HighlightSimilarOperationsHandler()
    {
        OperationVM operation = GetSelectedOperation();
        IReadOnlyList<OperationVM> collection = GetSimilarOperations(operation.Operation.Description);

        ResetHighlightHandler();
        
        foreach (OperationVM operationVM in collection)
        {
            operationVM.IsHighlighted = true;
        }
    }

    private void HighlightSameCategoryHandler()
    {
        Category category = GetSelectedOperation().Category;
        foreach (OperationVM operationVM in allOperations)
        {
            operationVM.IsHighlighted = operationVM.Category == category;
        }
    }

    private void ResetHighlightHandler()
    {
        foreach (OperationVM operationVM in allOperations)
        {
            operationVM.IsHighlighted = false;
        }
    }

    void IAssociationStorageListener.AssociationAdded(IAssociation newAssociation)
    {
        using var _ = mutex.EnterBatchProcessing();
        IReadOnlyList<OperationVM> collection = GetSimilarOperations(newAssociation.Description);
        foreach (OperationVM operation in collection)
        {
            operation.Association = newAssociation;
            if (operation.IsAutoMappedNotApproved)
            {
                operation.Category = newAssociation.Category;
            }
        }

        summaryChangedListener.SummaryDescriptionChanged();
        summaryChangedListener.IsSortedChanged();
    }

    void IAssociationStorageListener.AssociationRemoved(IAssociation association)
    {
        using var _ = mutex.EnterBatchProcessing();
        foreach (OperationVM operation in allOperations)
        {
            if (operation.Association != null && operation.Association == association)
            {
                IAssociation? newAssociation = associationsManager.TryFindBestMatch(operation.Operation.Description);
                operation.Association = newAssociation;
                if (operation.IsAutoMappedNotApproved)
                {
                    operation.Category = newAssociation == null
                        ? Category.Default
                        : newAssociation.Category;
                } // else leave user choice
            }
        }

        summaryChangedListener.SummaryDescriptionChanged();
        summaryChangedListener.IsSortedChanged();
    }

    private OperationVM GetSelectedOperation() => (OperationVM)selectedItems![0]!;

    private IReadOnlyList<OperationVM> GetSimilarOperations(OperationDescription description)
    {
        IReadOnlyList<OperationVM> collection = CollectionSearchHelper.FindAll(
            description.ComparisonKey, allOperations, operationVM => operationVM.Operation.Description.ComparisonKey);
        return collection;
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