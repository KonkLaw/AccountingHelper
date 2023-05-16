using System.Collections.ObjectModel;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class AssociationStorage
{
    private readonly ObservableCollection<AssociationVM> associations;
    private readonly ObservableCollection<string> excludedOperations;
    private readonly ISaveController saveController;

    public AssociationStorage(
        ObservableCollection<AssociationVM> associations,
        ObservableCollection<string> excludedOperations,
        ISaveController saveController)
    {
        this.associations = associations;
        this.excludedOperations = excludedOperations;
        this.saveController = saveController;
    }

    public void Update(string operationDescription, CategoryVM category)
    {
        if (string.IsNullOrEmpty(operationDescription) || excludedOperations.Any(o => o == operationDescription))
            return;

        AssociationVM? associationVM = associations.FirstOrDefault(a => a.OperationDescription == operationDescription);
        if (associationVM == null)
        {
            AssociationVM newAssociation = new (operationDescription, category);
            associations.PrependBeforeBigger(newAssociation,
                (o1, o2) => string.Compare(o1.OperationDescription, o2.OperationDescription, StringComparison.InvariantCulture));
        }
        else
            associationVM.CategoryVM = category;
        saveController.MarkChanged();
    }

    public void ExcludeFromAssociations(string operationDescription)
    {
        for (int i = 0; i < associations.Count; i++)
        {
            if (associations[i].OperationDescription == operationDescription)
            {
                associations.RemoveAt(i);
                break;
            }
        }
        excludedOperations.PrependBeforeBigger(operationDescription, string.Compare);
        saveController.MarkChanged();
    }

    public CategoryVM? TryGetCategory(string operationDescription)
    {
        AssociationVM? associationVM = associations.FirstOrDefault(a => a.OperationDescription == operationDescription);
        return associationVM?.CategoryVM;
    }
}