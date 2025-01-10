using System.Collections.ObjectModel;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

record InitData
{
    public readonly ObservableCollection<CategoryVM> Categories;
    public readonly AssociationStorage AssociationStorage;

    public InitData(
        List<CategoryVM> categories,
        List<AssociationVM> associations)
    {
        Categories = new ObservableCollection<CategoryVM>(categories);
        AssociationStorage = new AssociationStorage(associations);
    }
}