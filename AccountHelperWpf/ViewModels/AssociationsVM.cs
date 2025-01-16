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

    private IAssociation? selectedAssociation;
    public IAssociation? SelectedAssociation
    {
        get => selectedAssociation;
        set => SetProperty(ref selectedAssociation, value);
    }

    private IAssociation? selectedException;
    public IAssociation? SelectedException
    {
        get => selectedException;
        set => SetProperty(ref selectedException, value);
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
    {
        if (SelectedAssociation is null)
            return;
        storage.DeleteAssociation(SelectedAssociation);
    }

    private void DeleteException()
    {
        if (SelectedException is null)
            return;
        storage.DeleteException(SelectedException);
    }
}