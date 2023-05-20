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
    public int SelectedAssociationIndex { get; set; }
    public int SelectedExceptionIndex { get; set; }

    public AssociationsVM(InitData initData)
    {
        Associations = initData.Associations;
        ExcludedOperations = initData.ExcludedOperations;
        DeleteAssociationCommand = new DelegateCommand(DeleteAssociation);
        DeleteExceptionCommand = new DelegateCommand(DeleteException);
    }

    private void DeleteAssociation() => Associations.RemoveAt(SelectedAssociationIndex);

    private void DeleteException() => ExcludedOperations.RemoveAt(SelectedExceptionIndex!);
}