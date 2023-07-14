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
    public int SelectedExceptionIndex { get; set; }

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

        Associations = storage.GetAssociations();
        ExcludedOperations = storage.GetExcludedOperations();
    }

    private void DeleteAndClearOperation()
        => storage.DeleteAssociationAndClearOperations(
            ((AssociationVM)selectedAssociation.Item).OperationDescription);

    private void DeleteAssociation()
        => storage.DeleteAssociation(
            ((AssociationVM)selectedAssociation.Item).OperationDescription);

    private void DeleteException()
        => storage.DeleteException(SelectedExceptionIndex);
}