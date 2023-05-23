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
    private bool isOnRemoving;
    private BaseOperation? firstIncluded;
    private BaseOperation? lastIncluded;

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
        SetLastOperationCommand = new DelegateCommand(SetLastOperation);
        SetFirstOperationCommand = new DelegateCommand(SetFirstOperation);
        ExcludeFromAssociations = new DelegateCommand(ExcludeFromAssociationHandler);
        SetNullCategoryCommand = new DelegateCommand(SetCategoryToNull);
        ApplyCategoryForSameOperationsCommand = new DelegateCommand(ApplyCategoryForSameOperations);
        UpdateByFilter();
    }

    private void CategoriesVMOnOnCategoryRemoving() => isOnRemoving = true;

    private void CategoriesVMOnOnCategoryRemoved()
    {
        isOnRemoving = false;
        summaryChanged();
    }

    private void Approve(OperationVM operationVM)
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsApproved = true;
    }

    private void OperationViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OperationVM.Category))
        {
            if (isOnRemoving)
                return;

            OperationVM vm = (OperationVM)sender!;
            if (SelectedItems == null)
                return;
            foreach (OperationVM operationViewModel in SelectedItems)
                operationViewModel.Category = vm.Category;
            associationStorage?.Update(vm.Operation.Description, vm.Category!);
            summaryChanged();
        }
        else if (e.PropertyName == nameof(OperationVM.Description))
        {
            summaryChanged();
        }
    }

    private void SetFirstOperation()
    {
        firstIncluded = GetSelectedOperation().Operation;
        UpdateByFilter();
        summaryChanged();
    }

    private void SetLastOperation()
    {
        lastIncluded = GetSelectedOperation().Operation;
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
            associationStorage?.ExcludeFromAssociations(operationViewModel.Operation.Description);
    }

    private void UpdateByFilter()
    {
        var filteredOperations = new List<OperationVM>(operationGroup.Operations.Count);

        bool skip = lastIncluded != null;
        foreach (BaseOperation operation in operationGroup.Operations)
        {
            if (skip && (skip = operation != lastIncluded))
                continue;

            OperationVM operationVM = new(operation, categories, Approve);
            CategoryVM? categoryVM = associationStorage?.TryGetCategory(operation.Description);
            if (categoryVM != null)
            {
                operationVM.Category = categoryVM;
                operationVM.IsApproved = false;
            }
            filteredOperations.Add(operationVM);
            operationVM.PropertyChanged += OperationViewModelOnPropertyChanged;

            if (operation == firstIncluded)
                break;
        }
        Operations = filteredOperations;
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

    private void SetCategoryToNull()
    {
        foreach (OperationVM operationVM in SelectedItems.CheckNull())
            operationVM.Category = null;
    }

    private OperationVM GetSelectedOperation() => ((OperationVM)selectedItems![0]!);
}