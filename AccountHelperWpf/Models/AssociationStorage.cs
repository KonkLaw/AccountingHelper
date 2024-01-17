using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class AssociationStorage
{
    public event Action<string>? AssociationRemoved;
    public event Action? AssociationsChanged;

    private readonly ObservableDictionary associations;
    private readonly ObservableHashset excludedOperations;
    private readonly ISaveController saveController;

    public IEnumerable<AssociationVM> Associations => associations.Collection;
    public IEnumerable<string> ExcludedOperations => excludedOperations.Collection;

    public AssociationStorage(
        ObservableDictionary associations,
        ObservableHashset excludedOperations,
        ISaveController saveController)
    {
        this.associations = associations;
        this.excludedOperations = excludedOperations;
        this.saveController = saveController;
    }

    public CategoryVM? TryGetBestCategory(string operationDescription) => associations.TryGetBestMatch(operationDescription)?.CategoryVM;

    public bool IsExcluded(string operationDescription) => excludedOperations.ContainsSimilar(operationDescription);

    public void UpdateAssociation(string operationDescription, CategoryVM category)
    {
        if (string.IsNullOrEmpty(operationDescription) || excludedOperations.ContainsSimilar(operationDescription))
            return;

        AssociationVM? associationVM = associations.TryGetBestMatch(operationDescription);
        if (associationVM == null)
        {
            AssociationVM newAssociation = new (operationDescription, category, true);
            associations.Add(newAssociation);
        }
        else
        {
            associationVM.CategoryVM = category;
            associationVM.IsNew = true;
        }
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void AddSimilarToExcluded(string operationDescription)
    {
        associations.DeleteBetsMatch(operationDescription);

        if (excludedOperations.ContainsSimilar(operationDescription))
            return;
        excludedOperations.Add(operationDescription);
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void DeleteAssociationAndClearOperations(int selectedAssociationIndex)
    {
        AssociationVM associationVM = associations.GetByIndexAt(selectedAssociationIndex);
        associations.DeleteAt(selectedAssociationIndex);
        AssociationRemoved?.Invoke(associationVM.OperationDescription);
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void DeleteAssociation(int index)
    {
        associations.DeleteAt(index);
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void DeleteException(int selectedExceptionIndex)
    {
        excludedOperations.RemoveAt(selectedExceptionIndex);
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    private void OnAssociationChanged() => AssociationsChanged?.Invoke();
}