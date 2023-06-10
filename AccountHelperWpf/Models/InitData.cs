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
    private readonly Dictionary<string, AssociationVM> dictionary;

    public ObservableDictionary(ObservableCollection<AssociationVM> collection)
    {
        this.collection = collection;;
        Collection = collection;
        dictionary = new Dictionary<string, AssociationVM>(collection.Select(
            a => new KeyValuePair<string, AssociationVM>(a.OperationDescription, a)));
    }

    public AssociationVM? TryGet(string operationDescription)
        => dictionary.TryGetValue(operationDescription, out AssociationVM? association) ? association : null;

    public void RemoveAt(int index, out string oldDescription)
    {
        oldDescription = collection[index].OperationDescription;
        collection.RemoveAt(index);
        dictionary.Remove(oldDescription);
    }

    public void Delete(string description)
    {
        dictionary.Remove(description);
        for (int i = 0; i < collection.Count; i++)
        {
            if (collection[i].OperationDescription == description)
            {
                collection.RemoveAt(i);
                break;
            }
        }
    }

    public void Insert(AssociationVM associationVM)
    {
        dictionary.Add(associationVM.OperationDescription, associationVM);
        collection.PrependBeforeBigger(associationVM,
            (o1, o2) => string.Compare(o1.OperationDescription, o2.OperationDescription, StringComparison.InvariantCulture));
    }
}

class ObservableHashset
{
    private readonly ObservableCollection<string> collection;
    public IEnumerable<string> Collection => collection;
    private readonly HashSet<string> hashSet;

    public ObservableHashset(ObservableCollection<string> collection)
    {
        this.collection = collection;
        hashSet = new HashSet<string>(collection);
    }

    public bool Contains(string operationDescription)
        => hashSet.Contains(operationDescription);

    public void RemoveAt(int index)
    {
        hashSet.Remove(collection[index]);
        collection.RemoveAt(index);
    }

    public void Insert(string description)
    {
        collection.PrependBeforeBigger(description, string.Compare);
        hashSet.Add(description);

    }
}