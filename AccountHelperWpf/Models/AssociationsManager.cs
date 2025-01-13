﻿using System.Collections.ObjectModel;
using AccountHelperWpf.HistoryFile;
using AccountHelperWpf.ViewModels;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Models;

class AssociationsManager : IAssociationsManager
{
    private readonly HashSet<IAssociationStorageListener> listeners = new();
    private readonly AssociationStorage storage;

    public ObservableCollection<IAssociation> Associations { get; }
    public ObservableCollection<IAssociation> Exceptions { get; set; }

    public AssociationsManager(AssociationStorage storage)
    {
        this.storage = storage;

        Associations = new();
        Exceptions = new();
        foreach (IAssociation association in storage.Associations)
        {
            if (association.Category.IsDefault)
                Exceptions.Add(association);
            else
                Associations.Add(association);
        }
    }

    public void AddListener(IAssociationStorageListener listener) => listeners.Add(listener);

    public void RemoveListener(IAssociationStorageListener listener) => listeners.Remove(listener);

    private void ExecuteForAllAdd(IAssociation newAssociation)
    {
        foreach (IAssociationStorageListener listener in listeners)
        {
            listener.AssociationAdded(newAssociation);
        }
    }

    private void ExecuteForAllRemove(IAssociation oldAssociation)
    {
        foreach (IAssociationStorageListener listener in listeners)
        {
            listener.AssociationRemoved(oldAssociation);
        }
    }


    public IAssociation? TryFindBestMatch(OperationDescription operationDescription)
        => storage.TryFindBestMatch(operationDescription);

    void IAssociationsManager.AddException(OperationDescription description)
    {
        IAssociation? oldAssociation = storage.DeleteByOperation(description);
        if (oldAssociation != null)
        {
            Associations.Remove(oldAssociation);
            ExecuteForAllRemove(oldAssociation);
        }
        IAssociation newAssociation = new Association(description, Category.Default, true);
        storage.Add(newAssociation);
        Exceptions.Add(newAssociation);
        ExecuteForAllAdd(newAssociation);
    }

    void IAssociationsManager.AddAssociation(OperationDescription description, Category category)
    {
        if (category.IsDefault)
            throw new InvalidOperationException("Can't add default category to associations");

        IAssociation? oldAssociation = storage.TryFindBestMatch(description);

        if (oldAssociation != null)
            throw new InvalidOperationException("Can't change existing association.");

        IAssociation newAssociation = new Association(description, category, true);
        storage.Add(newAssociation);
        Associations.Add(newAssociation);
        ExecuteForAllAdd(newAssociation);
    }
    
    void IAssociationsManager.RemoveAssociations(Category category)
    {
        List<IAssociation> associations = storage.Remove(category);
        foreach (IAssociation association in associations)
        {
            ExecuteForAllRemove(association);
        }
    }

    public void DeleteAssociation(int selectedIndex)
    {
        IAssociation association = Associations[selectedIndex];
        Associations.RemoveAt(selectedIndex);
        storage.Remove(association);
        ExecuteForAllRemove(association);
    }
    
    public void DeleteException(int selectedIndex)
    {
        IAssociation association = Exceptions[selectedIndex];
        Exceptions.RemoveAt(selectedIndex);
        storage.Remove(association);
        ExecuteForAllRemove(association);
    }



    

    public static InitData PrepareInitData(HistoryData historyData)
    {
        List<Category> categories = [Category.Default];
        categories.AddRange(historyData.Categories!.Select(c => new Category
        {
            Description = c.Description!,
            Name = c.Name!
        }));
        var dictionary = new Dictionary<string, Category>(categories.Select(c => new KeyValuePair<string, Category>(c.Name, c)));


        IAssociation Selector(AssociationRecord association)
        {
            OperationDescription operationDescription = OperationDescription.Create(bankId: association.BankId!,
                // recreate to be sure that dictionary is ordered by key
                tagsContents: new SortedDictionary<string, string>(association.TagsToContents!));
            Category category = association.Category == null ? Category.Default : dictionary[association.Category!];
            return new Association(operationDescription, category, false) { Comment = association.Comment };
        }

        List<IAssociation> associations = historyData.Associations!.Select(Selector).ToList();

        return new InitData(categories, associations);
    }

    class Association : BaseNotifyProperty, IAssociation
    {
        public OperationDescription Description { get; }

        public Category Category { get; }

        public bool IsNew { get; }

        private string? comment;
        public string? Comment
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

        public Association(OperationDescription description, Category category, bool isNew)
        {
            Description = description;
            Category = category;
            IsNew = isNew;
        }
    }
}

interface IAssociationsManager
{
    IAssociation? TryFindBestMatch(OperationDescription operationDescription);
    void AddException(OperationDescription description);
    void AddAssociation(OperationDescription description, Category category);
    void RemoveAssociations(Category category);
}

interface IAssociationStorageListener
{
    void AssociationAdded(IAssociation newAssociation);
    void AssociationRemoved(IAssociation association);
}