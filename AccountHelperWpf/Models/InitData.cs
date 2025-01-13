using System.Collections.ObjectModel;

namespace AccountHelperWpf.Models;

record InitData
{
    public readonly ObservableCollection<Category> Categories;
    public readonly AssociationStorage AssociationStorage;

    public InitData(List<Category> categories, List<IAssociation> associations)
    {
        Categories = new ObservableCollection<Category>(categories);
        AssociationStorage = new AssociationStorage(associations);
    }
}