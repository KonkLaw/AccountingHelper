using System.Collections.ObjectModel;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

record InitData
{
    public ObservableCollection<CategoryVM> Categories;
    public ObservableDictionary Associations;
    public ObservableHashset ExcludedOperations;

    public InitData(ObservableCollection<CategoryVM> categories, ObservableCollection<AssociationVM> associations, ObservableCollection<string> excludedOperations)
    {
        Categories = categories;
        Associations = new ObservableDictionary(associations);
        ExcludedOperations = new ObservableHashset(excludedOperations);
    }
}

class ObservableDictionary
{
    private readonly ObservableCollection<AssociationVM> collection;
    public IEnumerable<AssociationVM> Collection;

    public ObservableDictionary(ObservableCollection<AssociationVM> collection)
    {
        this.collection = collection;;
        Collection = collection;
    }

    public AssociationVM? TryGetBestMatch(string description)
        => TryGetBestMatch(description, out _);

    private AssociationVM? TryGetBestMatch(string description, out int index)
    {
        AssociationVM? bestMath = CollectionSearchHelper.FindBest(description, collection, assoc => assoc.OperationDescription);
        index = bestMath == null ? -1 : collection.IndexOf(bestMath);
        return bestMath;
    }

    public void DeleteBetsMatch(string description)
    {
        if (TryGetBestMatch(description, out int index) != null)
            collection.RemoveAt(index);
    }

    public void Add(AssociationVM associationVM) =>
        collection.PrependBeforeBigger(associationVM,
            (o1, o2) => string.Compare(o1.OperationDescription, o2.OperationDescription, StringComparison.InvariantCulture));

    public AssociationVM GetByIndexAt(int index) => collection[index];

    public void DeleteAt(int index) => collection.RemoveAt(index);
}

class ObservableHashset
{
    private readonly ObservableCollection<string> collection;
    public IEnumerable<string> Collection => collection;

    public ObservableHashset(ObservableCollection<string> collection)
    {
        this.collection = collection;
    }

    public bool ContainsSimilar(string description)
        => CollectionSearchHelper.FindAny(description, collection, str => str);

    public void RemoveAt(int index) => collection.RemoveAt(index);

    public void Add(string description)
        => collection.PrependBeforeBigger(description, string.Compare);
}