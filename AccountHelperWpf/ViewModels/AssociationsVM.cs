using System.Collections.ObjectModel;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationsVM : BaseNotifyProperty
{
    public ObservableCollection<AssociationVM> Associations { get; }
    public ObservableCollection<string> ExcludedOperations { get; }

    public AssociationsVM(InitData initData)
    {
        Associations = initData.Associations;
        ExcludedOperations = initData.ExcludedOperations;
    }
}