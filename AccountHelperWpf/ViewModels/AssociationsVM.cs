using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationsVM : BaseNotifyProperty
{
    private readonly AssociationsManager storage;
    public IEnumerable<IAssociation> Associations { get; }
    public IEnumerable<IAssociation> Exceptions { get; }
    public ICommand DeleteAssociationCommand { get; }
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

    public AssociationsVM(AssociationsManager storage)
    {
        this.storage = storage;
        DeleteAssociationCommand = new DelegateCommand(DeleteAssociation);
        DeleteExceptionCommand = new DelegateCommand(DeleteException);

        Associations = storage.Associations;
        Exceptions = storage.Exceptions;
    }

    private void DeleteAssociation()
        => storage.DeleteAssociation(SelectedAssociationIndex);

    private void DeleteException()
        => storage.DeleteException(SelectedExceptionIndex);
}