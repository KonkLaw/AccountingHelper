using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationsVM : BaseNotifyProperty
{
    public ObservableCollection<AssociationVM> Associations { get; }
    public ObservableCollection<string> ExcludedOperations { get; }

    public ICommand DeleteAssociationCommand { get; }
    public ICommand DeleteExceptionCommand { get; }

    private AssociationVM? selectedAssociation;
    public AssociationVM? SelectedAssociation
    {
        get => selectedAssociation;
        set => SetProperty(ref selectedAssociation, value);
    }

    private string? selectedException;
    public string? SelectedException
    {
        get => selectedException;
        set => SetProperty(ref selectedException, value);
    }

    public AssociationsVM(InitData initData)
    {
        Associations = initData.Associations;
        ExcludedOperations = initData.ExcludedOperations;
        DeleteAssociationCommand = new DelegateCommand(DeleteAssociation);
        DeleteExceptionCommand = new DelegateCommand(DeleteException);
        SelectedAssociation = Associations.FirstOrDefault();
    }

    private void DeleteAssociation() => Associations.Remove(SelectedAssociation!);

    private void DeleteException() => ExcludedOperations.Remove(SelectedException!);
}