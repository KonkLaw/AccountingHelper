using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class AssociationStorage
{
    private readonly List<AssociationVM> list;

    public event Action? Changed;

    public ICollection<AssociationVM> Associations => list;

    public AssociationStorage(List<AssociationVM> associations)
    {
        list = associations;
    }

    private void RaiseChanged() => Changed?.Invoke();

    public AssociationVM? TryGetBestMatch(string description, out int index)
    {
        AssociationVM? bestMath = CollectionSearchHelper.FindBest(
            description, list, assoc => assoc.OperationDescription);
        index = bestMath == null ? -1 : list.IndexOf(bestMath);
        return bestMath;
    }

    public void Add(AssociationVM associationVM)
    {
        list.Add(associationVM);
        RaiseChanged();
    }

    public void Remove(AssociationVM associationVM)
    {
        list.Remove(associationVM);
        RaiseChanged();
    }

    public void DeleteAt(int index)
    {
        list.RemoveAt(index);
        RaiseChanged();
    }

    public List<string> Remove(CategoryVM category)
    {
        List<string> deleted = new();
        for (var i = list.Count - 1; i >= 0; i--)
        {
            AssociationVM associationVM = list[i];
            if (associationVM.CategoryVM == category)
            {
                deleted.Add(associationVM.OperationDescription);
                list.RemoveAt(i);
            }
        }
        RaiseChanged();
        return deleted;
    }
}