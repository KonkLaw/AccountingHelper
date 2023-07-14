using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class AssociationStorage
{
    public event Action<string>? AssociationRemoved;
    public event Action? AssociationsChanged;

    private readonly ObservableDictionary associations;
    private readonly ObservableHashset excludedOperations;
    private readonly ISaveController saveController;

    public AssociationStorage(
        ObservableDictionary associations,
        ObservableHashset excludedOperations,
        ISaveController saveController)
    {
        this.associations = associations;
        this.excludedOperations = excludedOperations;
        this.saveController = saveController;
    }

    public IEnumerable<AssociationVM> GetAssociations() => associations.Collection;

    public IEnumerable<string> GetExcludedOperations() => excludedOperations.Collection;

    public CategoryVM? TryGetCategory(string operationDescription) => associations.TryGet(operationDescription)?.CategoryVM;

    public bool IsExcluded(string operationDescription) => excludedOperations.Contains(operationDescription);

    public void Update(string operationDescription, CategoryVM category)
    {
        if (string.IsNullOrEmpty(operationDescription) || excludedOperations.Contains(operationDescription))
            return;

        
        AssociationVM? associationVM = associations.TryGet(operationDescription);
        if (associationVM == null)
        {
            AssociationVM newAssociation = new (operationDescription, category, true);
            associations.Insert(newAssociation);
        }
        else
            associationVM.CategoryVM = category;
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void AddToExcludedOperations(string operationDescription)
    {
        associations.Delete(operationDescription);
        if (excludedOperations.Contains(operationDescription))
            return;
        excludedOperations.Insert(operationDescription);
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void DeleteAssociationAndClearOperations(string operationDescription)
    {
        associations.Delete(operationDescription);
        AssociationRemoved?.Invoke(operationDescription);
        OnAssociationChanged();
        saveController.MarkChanged();
    }

    public void DeleteAssociation(string operationDescription)
    {
        associations.Delete(operationDescription);
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