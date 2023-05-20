using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
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

    private OperationVM? selectedOperation;
    public OperationVM? SelectedOperation
    {
        get => selectedOperation;
        set => SetProperty(ref selectedOperation, value);
    }

    private IList? selectedItems;
    public IList? SelectedItems
    {
        get => selectedItems;
        set => SetProperty(ref selectedItems, value);
    }
    public string Name => operationGroup.Name;

    public ICommand ExcludeFromAssociations { get; }
    public ICommand SetLastOperationCommand { get; }
    public ICommand SetFirstOperationCommand { get; }

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
        if (SelectedItems == null)
            return;
        foreach (OperationVM operationViewModel in SelectedItems)
            operationViewModel.IsApproved = true;
    }

    private void OperationViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(OperationVM.Category))
            return;

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

    private void SetFirstOperation()
    {
        firstIncluded = selectedOperation!.Operation;
        UpdateByFilter();
        summaryChanged();
    }

    private void SetLastOperation()
    {
        lastIncluded = selectedOperation!.Operation;
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
        => associationStorage?.ExcludeFromAssociations(selectedOperation!.Operation.Description);

    private void UpdateByFilter()
    {
        var filteredOperations = new List<OperationVM>(operationGroup.Operations.Count);

        bool skip = firstIncluded != null;
        foreach (BaseOperation operation in operationGroup.Operations)
        {
            if (skip && (skip = operation != firstIncluded))
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

            if (operation == lastIncluded)
                break;
        }
        Operations = filteredOperations;
    }
}