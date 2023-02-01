using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class SortedOperationsGroupVM : BaseNotifyProperty
{
    private readonly OperationsGroup operationGroup;
    private readonly ReadOnlyObservableCollection<CategoryVm> categories;
    private readonly ISummaryChangedListener listener;

    public string Name => operationGroup.Name;

    private IReadOnlyList<OperationViewModel> operations;
    public IReadOnlyList<OperationViewModel> Operations
    {
        get => operations;
        set => SetProperty(ref operations, value);
    }

    private OperationViewModel? selectedOperation;
    public OperationViewModel? SelectedOperation
    {
        get => selectedOperation;
        set => SetProperty(ref selectedOperation, value);
    }

    public ICommand SetLastCommand { get; }

    public SortedOperationsGroupVM(
        OperationsGroup operationGroup,
        ReadOnlyObservableCollection<CategoryVm> categories,
        ISummaryChangedListener listener)
    {
        this.operationGroup = operationGroup;
        this.categories = categories;
        this.listener = listener;
        SetLastCommand = new DelegateCommand(SetLast);
        operations = GetFiltered(null);
    }

    private List<OperationViewModel> GetFiltered(BaseOperation? lastIncluded)
    {
        List<OperationViewModel> filteredOperations = new (operationGroup.Operations.Count);
        foreach (BaseOperation operation in operationGroup.Operations)
        {
            filteredOperations.Add(new OperationViewModel(operation, categories, listener));
            if (operation == lastIncluded)
                break;
        }

        return filteredOperations;
    }

    private void SetLast()
    {
        Operations = GetFiltered(selectedOperation!.Operation);
        SelectedOperation = null;
        listener.Changed();
    }

    public void ResetFilter()
    {
        Operations = GetFiltered(null);
        listener.Changed();
    }
}