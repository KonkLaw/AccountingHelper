using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationsGroupVM : BaseNotifyProperty
{
    private readonly OperationsGroup operationGroup;
    private readonly ReadOnlyObservableCollection<CategoryVM> categories;
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
                IsSingleSelection = selectedItems != null && selectedItems.Count > 0;
        }
    }

    private bool isSingleSelection;
    public bool IsSingleSelection
    {
        get => isSingleSelection;
        set => SetProperty(ref isSingleSelection, value);
    }

    public string Name => operationGroup.Name;

    public ICommand ExcludeFromAssociations { get; }
    public ICommand SetLastOperationCommand { get; }
    public ICommand SetFirstOperationCommand { get; }
    public ICommand SetNullCategoryCommand { get; }
    public ICommand ApplyCategoryForSameOperationsCommand { get; }
    public ICommand ApproveCommand { get; }

    public OperationsGroupVM(
        OperationsGroup operationGroup,
        CategoriesVM categoriesVM,
        Action summaryChanged,
        AssociationStorage? associationStorage)
    {
        this.operationGroup = operationGroup;
        categories = categoriesVM.GetCategories();
        categoriesVM.OnCategoryRemoving += CategoriesVMOnOnCategoryRemoving;
        categoriesVM.OnCategoryRemoved += CategoriesVMOnOnCategoryRemoved;
        this.summaryChanged = summaryChanged;
        this.associationStorage = associationStorage;
        if (associationStorage != null)
            associationStorage.OnAssociationRemoved += AssociationStorageOnOnAssociationRemoved;
        SetLastOperationCommand = new DelegateCommand(SetLastOperation);
        SetFirstOperationCommand = new DelegateCommand(SetFirstOperation);
        ExcludeFromAssociations = new DelegateCommand(ExcludeFromAssociationHandler);
        SetNullCategoryCommand = new DelegateCommand(SetCategoryToNull);
        ApplyCategoryForSameOperationsCommand = new DelegateCommand(ApplyCategoryForSameOperations);
        ApproveCommand = new DelegateCommand(Approve);
        allOperations = GetAllOperations();
        UpdateByFilter();
    }

    private List<OperationVM> GetAllOperations()
    {
        var result = new List<OperationVM>(operationGroup.Operations.Count);
        foreach (BaseOperation operation in operationGroup.Operations)
        {
            OperationVM operationVM = new(operation, categories);
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

    private void CategoriesVMOnOnCategoryRemoving() => isOnRemoving = true;

    private void CategoriesVMOnOnCategoryRemoved()
    {
        isOnRemoving = false;
        summaryChanged();
    }

    private void OperationViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (isOnRemoving)
            return;

        if (e.PropertyName == nameof(OperationVM.Category))
        {
            OperationVM vm = (OperationVM)sender!;
            foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
                operationViewModel.Category = vm.Category;
            associationStorage?.Update(vm.Operation.Description, vm.Category!);
            summaryChanged();
        }
        else if (e.PropertyName == nameof(OperationVM.Description) || e.PropertyName == nameof(OperationVM.IsApproved))
        {
            summaryChanged();
        }
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

    public void ResetFilters()
    {
        firstIncluded = null;
        lastIncluded = null;
        UpdateByFilter();
    }

    private void ExcludeFromAssociationHandler()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            associationStorage?.AddToExcludedOperations(operationViewModel.Operation.Description);
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

    private void SetCategoryToNull()
    {
        foreach (OperationVM operationVM in SelectedItems.CheckNull())
            operationVM.Category = null;
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

    private void Approve()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsApproved = true;
    }

    private OperationVM GetSelectedOperation() => (OperationVM)selectedItems![0]!;
}