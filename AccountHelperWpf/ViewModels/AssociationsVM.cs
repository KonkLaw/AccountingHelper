using System.Windows.Controls;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationsVM : BaseNotifyProperty
{
    private readonly AssociationStorage storage;
    public IEnumerable<AssociationVM> Associations { get; }
    public IEnumerable<string> ExcludedOperations { get; }
    public ICommand DeleteAssociationCommand { get; }
    public ICommand DeleteAndClearOperationCommand { get; }
    public ICommand DeleteExceptionCommand { get; }

    private int selectedAssociationIndex;
    public int SelectedAssociationIndex
    {
        get => selectedAssociationIndex;
        set => SetProperty(ref selectedAssociationIndex, value);
    }

    private int selectedExceptionIndex;
    public int SelectedExceptionIndex
    {
        get => selectedExceptionIndex;
        set => SetProperty(ref selectedExceptionIndex, value);
    }

    private DataGridCellInfo selectedAssociation;
    public DataGridCellInfo SelectedAssociation
    {
        get => selectedAssociation;
        set => SetProperty(ref selectedAssociation, value);
    }

    public AssociationsVM(AssociationStorage storage)
    {
        this.storage = storage;
        DeleteAssociationCommand = new DelegateCommand(DeleteAssociation);
        DeleteAndClearOperationCommand = new DelegateCommand(DeleteAndClearOperation);
        DeleteExceptionCommand = new DelegateCommand(DeleteException);

        Associations = storage.Associations;
        ExcludedOperations = storage.ExcludedOperations;
    }

    private void DeleteAndClearOperation()
        => storage.DeleteAssociationAndClearOperations(SelectedAssociationIndex);

    private void DeleteAssociation()
        => storage.DeleteAssociation(SelectedAssociationIndex);

    private void DeleteException()
        => storage.DeleteException(SelectedExceptionIndex);
}