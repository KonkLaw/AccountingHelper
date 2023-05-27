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
    public int SelectedAssociationIndex { get; set; }
    public int SelectedExceptionIndex { get; set; }

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
        => storage.DeleteAssociationAndClearOperations(SelectedAssociationIndex);

    private void DeleteAssociation()
        => storage.DeleteAssociation(SelectedAssociationIndex);


    private void DeleteException()
        => storage.DeleteException(SelectedExceptionIndex);
}