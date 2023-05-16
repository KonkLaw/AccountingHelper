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

    public string Name => operationGroup.Name;

    private IReadOnlyList<OperationVM> operations;
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

    public ICommand SetLastCommand { get; }

    public ICommand ExcludeFromAssociations { get; }

    public OperationsGroupVM(
        OperationsGroup operationGroup,
        ReadOnlyObservableCollection<CategoryVM> categories,
        Action summaryChanged,
        AssociationStorage? associationStorage)
    {
        this.operationGroup = operationGroup;
        this.categories = categories;
        this.summaryChanged = summaryChanged;
        this.associationStorage = associationStorage;
        SetLastCommand = new DelegateCommand(SetLast);
        ExcludeFromAssociations = new DelegateCommand(ExcludeFromAssociationHandler);
        operations = GetFiltered(null);
    }

    private List<OperationVM> GetFiltered(BaseOperation? lastIncluded)
    {
        List<OperationVM> filteredOperations = new (operationGroup.Operations.Count);
        foreach (BaseOperation operation in operationGroup.Operations)
        {
            OperationVM operationVM = new(operation, categories, Approve);

            if (associationStorage != null)
            {
                CategoryVM? categoryVM = associationStorage.TryGetCategory(operation.Description);
                if (categoryVM != null)
                {
                    operationVM.Category = categoryVM;
                    operationVM.IsApproved = false;
                }
            }
            filteredOperations.Add(operationVM);
            operationVM.PropertyChanged += OperationViewModelOnPropertyChanged;
            if (operation == lastIncluded)
                break;
        }
        return filteredOperations;
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
        if (e.PropertyName == nameof(OperationVM.Category))
        {
            OperationVM vm = (OperationVM)sender!;
            if (SelectedItems == null)
                return;
            foreach (OperationVM operationViewModel in SelectedItems)
                operationViewModel.Category = vm.Category;
            associationStorage?.Update(vm.Operation.Description, vm.Category!);
        }
        summaryChanged();
    }

    private void SetLast()
    {
        Operations = GetFiltered(selectedOperation!.Operation);
        SelectedOperation = null;
        summaryChanged();
    }

    private void ExcludeFromAssociationHandler()
        => associationStorage?.ExcludeFromAssociations(selectedOperation!.Operation.Description);

    public void ResetFilter() => Operations = GetFiltered(null);
}