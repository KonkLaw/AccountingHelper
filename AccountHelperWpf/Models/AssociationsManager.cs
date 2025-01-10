using System.Collections.ObjectModel;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class AssociationsManager : IAssociationsManager
{
    private readonly HashSet<IAssociationStorageListener> listeners = new();
    private readonly AssociationStorage storage;

    public ObservableCollection<AssociationVM> Associations { get; }
    public ObservableCollection<AssociationVM> Exceptions { get; set; }

    public AssociationsManager(AssociationStorage storage)
    {
        this.storage = storage;

        Associations = new();
        Exceptions = new();
        foreach (AssociationVM association in storage.Associations)
        {
            if (association.CategoryVM.IsDefault)
                Exceptions.Add(association);
            else
                Associations.Add(association);
        }
    }

    public void AddListener(IAssociationStorageListener listener) => listeners.Add(listener);

    public void RemoveListener(IAssociationStorageListener listener) => listeners.Remove(listener);

    private void ExecuteForAll<TArg>(TArg arg, Action<IAssociationStorageListener, TArg> handler)
    {
        foreach (IAssociationStorageListener listener in listeners)
        {
            handler(listener, arg);
        }
    }


    void IAssociationsManager.AddOrUpdateAssociation(string operationDescription, CategoryVM category)
    {
        // Default means - not associated category by default = exception
        if (string.IsNullOrEmpty(operationDescription))
            return;

        AssociationVM? associationVM = storage.TryGetBestMatch(operationDescription, out int index);
        if (associationVM is { CategoryVM.IsDefault: true })
            return;

        Association? oldAssociation = associationVM == null ? null : new Association(associationVM);
        AssociationVM newAssociationVM;
        if (category.IsDefault)
        {
            if (associationVM != null)
            {
                storage.DeleteAt(index);
                Associations.Remove(associationVM);
                ExecuteForAll(
                    new Association(associationVM),
                    static (l, d) => l.AssociationRemoved(d));
            }
            newAssociationVM = new AssociationVM(operationDescription, category, true);
            storage.Add(newAssociationVM);
            Exceptions.Add(newAssociationVM);
        }
        else
        {
            if (associationVM == null)
            {
                newAssociationVM = new(operationDescription, category, true);
                storage.Add(newAssociationVM);
                Associations.Add(newAssociationVM);
            }
            else
            {
                if (associationVM.CategoryVM.IsDefault)
                {
                    associationVM.CategoryVM = category;
                    associationVM.IsNew = true;
                    Associations.Add(associationVM);
                    Exceptions.Remove(associationVM);
                }
                else
                {
                    associationVM.CategoryVM = category;
                    associationVM.IsNew = true;
                }
                newAssociationVM = associationVM;
            }
            
        }
        Association newAssociation = new Association(newAssociationVM);
        ExecuteForAll(
            (oldAssociation, newAssociation),
            static (l, arg) => l.AssociationChanged(arg.oldAssociation, arg.newAssociation));
    }
    
    Association? IAssociationsManager.TryGetBestAssociation(string operationDescription)
    {
        AssociationVM? associationVM = storage.TryGetBestMatch(operationDescription, out _);
        return associationVM == null ? null : new Association(associationVM);
    }

    void IAssociationsManager.RemoveAssociation(CategoryVM category)
    {
        List<string> deleted = storage.Remove(category);
        foreach (string operationDescription in deleted)
        {
            ExecuteForAll(
                (operationDescription, category),
                static (l, arg) => l.AssociationRemoved(new Association(arg.operationDescription, arg.category)));
        }
    }

    public void DeleteAssociation(int selectedIndex)
    {
        AssociationVM associationVM = Associations[selectedIndex];
        Associations.RemoveAt(selectedIndex);
        storage.Remove(associationVM);
        ExecuteForAll(new Association(associationVM), static (l, d) => l.AssociationRemoved(d));
    }
    
    public void DeleteException(int selectedIndex)
    {
        AssociationVM associationVM = Exceptions[selectedIndex];
        Exceptions.RemoveAt(selectedIndex);
        storage.Remove(associationVM);
        ExecuteForAll(new Association(associationVM), static (l, d) => l.AssociationRemoved(d));
    }
}

interface IAssociationsManager
{
    Association? TryGetBestAssociation(string operationDescription);
    void AddOrUpdateAssociation(string operationDescription, CategoryVM category);
    void RemoveAssociation(CategoryVM category);
}

interface IAssociationStorageListener
{
    void AssociationChanged(Association? oldAssociation, Association newAssociation);
    void AssociationRemoved(Association association);
}

record Association(string OperationDescription, CategoryVM Category)
{
    public Association(AssociationVM associationVM)
        : this(associationVM.OperationDescription, associationVM.CategoryVM) { }
}