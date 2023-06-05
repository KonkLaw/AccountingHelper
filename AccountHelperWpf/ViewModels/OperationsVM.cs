using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationsVM : BaseNotifyProperty
{
    private readonly CategoriesVM categoriesVM;
    private readonly Action summaryChanged;
    private readonly AssociationStorage? associationStorage;
    private readonly List<OperationVM> allOperations;
    private bool isOnRemoving;
    private OperationVM? firstIncluded;
    private OperationVM? lastIncluded;

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
            if (SetProperty(ref selectedItems, value))
                IsSingleSelection = selectedItems is { Count: > 0 };
        }
    }

    private bool isSingleSelection;
    public bool IsSingleSelection
    {
        get => isSingleSelection;
        set => SetProperty(ref isSingleSelection, value);
    }

    public ICommand SearchInfoCommand { get; }
    public ICommand SetNullCategoryCommand { get; }
    public ICommand SetFirstOperationCommand { get; }
    public ICommand SetLastOperationCommand { get; }
    public ICommand ApplyCategoryForSameOperationsCommand { get; }
    public ICommand ExcludeFromAssociations { get; }
    public ICommand ApproveCommand { get; }
    

    public OperationsVM(
        IReadOnlyList<BaseOperation> baseOperations,
        CategoriesVM categoriesVM,
        Action summaryChanged,
        AssociationStorage? associationStorage)
    {
        this.categoriesVM = categoriesVM;
        this.summaryChanged = summaryChanged;
        this.associationStorage = associationStorage;
        allOperations = GetAllOperations(baseOperations);
        UpdateByFilter();

        categoriesVM.OnCategoryRemoving += CategoriesVMOnOnCategoryRemoving;
        categoriesVM.OnCategoryRemoved += CategoriesVMOnOnCategoryRemoved;
        if (associationStorage != null)
            associationStorage.OnAssociationRemoved += AssociationStorageOnOnAssociationRemoved;

        SearchInfoCommand = new DelegateCommand(SearchInfo);
        SetNullCategoryCommand = new DelegateCommand(SetCategoryToNull);
        SetLastOperationCommand = new DelegateCommand(SetLastOperation);
        SetFirstOperationCommand = new DelegateCommand(SetFirstOperation);
        ApplyCategoryForSameOperationsCommand = new DelegateCommand(ApplyCategoryForSameOperations);
        ExcludeFromAssociations = new DelegateCommand(ExcludeFromAssociationHandler);
        ApproveCommand = new DelegateCommand(Approve);
    }

    private List<OperationVM> GetAllOperations(IReadOnlyList<BaseOperation> baseOperations)
    {
        var result = new List<OperationVM>(baseOperations.Count);
        foreach (BaseOperation operation in baseOperations)
        {
            OperationVM operationVM = new(operation, categoriesVM.GetCategories());
            CategoryVM? categoryVM = associationStorage?.TryGetCategory(operation.Description);
            if (categoryVM != null)
            {
                operationVM.Category = categoryVM;
                operationVM.IsApproved = false;
            }
            result.Add(operationVM);
            operationVM.PropertyChanged += OperationViewModelOnPropertyChanged;
        }
        return result;
    }

    private void OperationViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (isOnRemoving)
            return;

        switch (e.PropertyName)
        {
            case nameof(OperationVM.Category):
            {
                OperationVM vm = (OperationVM)sender!;
                foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
                    operationViewModel.Category = vm.Category;
                associationStorage?.Update(vm.Operation.Description, vm.Category!);
                summaryChanged();
                break;
            }
            case nameof(OperationVM.Description):
            case nameof(OperationVM.IsApproved):
                summaryChanged();
                break;
        }
    }

    private void CategoriesVMOnOnCategoryRemoving() => isOnRemoving = true;

    private void CategoriesVMOnOnCategoryRemoved()
    {
        isOnRemoving = false;
        summaryChanged();
    }

    private void AssociationStorageOnOnAssociationRemoved(string operationDescription)
    {
        isOnRemoving = true;
        foreach (OperationVM operation in allOperations)
        {
            if (operation.Operation.Description == operationDescription)
                operation.Category = null;
        }
        isOnRemoving = false;
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

    private void SetCategoryToNull()
    {
        foreach (OperationVM operationVM in SelectedItems.CheckNull())
            operationVM.Category = null;
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

    private void ApplyCategoryForSameOperations()
    {
        OperationVM selectedOperation = GetSelectedOperation();
        foreach (OperationVM operation in Operations)
        {
            if (operation.Operation.Description == selectedOperation.Operation.Description)
                operation.Category = selectedOperation.Category;
        }
    }

    private void ExcludeFromAssociationHandler()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            associationStorage?.AddToExcludedOperations(operationViewModel.Operation.Description);
    }

    private OperationVM GetSelectedOperation() => (OperationVM)selectedItems![0]!;

    private void Approve()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsApproved = true;
    }
}