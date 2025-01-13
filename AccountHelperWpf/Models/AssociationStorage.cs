namespace AccountHelperWpf.Models;

class AssociationStorage
{
    private readonly List<IAssociation> list;

    public event Action? Changed;

    public IEnumerable<IAssociation> Associations => list;

    public AssociationStorage(List<IAssociation> associations)
    {
        list = associations;
    }

    private void RaiseChanged() => Changed?.Invoke();

    public IAssociation? TryFindBestMatch(OperationDescription description)
    {
        IAssociation? bestMath = CollectionSearchHelper.FindBest(
            description.ComparisonKey, list, association => association.Description.ComparisonKey);
        return bestMath;
    }

    public void Add(IAssociation association)
    {
        list.Add(association);
        RaiseChanged();
    }

    public IAssociation? DeleteByOperation(OperationDescription description)
    {
        int index = list.FindIndex(association => association.Description == description);
        if (index > 0)
        {
            IAssociation deleted = list[index];
            list.RemoveAt(index);
            RaiseChanged();
            return deleted;
        }
        return null;
    }

    public void Remove(IAssociation association)
    {
        list.Remove(association);
        RaiseChanged();
    }

    public List<IAssociation> Remove(Category category)
    {
        List<IAssociation> deleted = new();
        for (var i = list.Count - 1; i >= 0; i--)
        {
            IAssociation association = list[i];
            if (association.Category == category)
            {
                deleted.Add(association);
                list.RemoveAt(i);
            }
        }
        RaiseChanged();
        return deleted;
    }
}