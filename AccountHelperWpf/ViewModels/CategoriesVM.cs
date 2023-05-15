using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class CategoriesVM
{
    private readonly ISaveController saveController;
    private readonly ReadOnlyObservableCollection<CategoryVM> collection;
    public ObservableCollection<CategoryVM> Categories { get; }
    public event Action? Changed;

    public CategoriesVM(ObservableCollection<CategoryVM> loadedCategories, ISaveController saveController)
    {
        this.saveController = saveController;
        Categories = loadedCategories;
        collection = new ReadOnlyObservableCollection<CategoryVM>(Categories);

        Categories.CollectionChanged += CategoriesCollectionChanged;
        foreach (CategoryVM? categoryViewModel in Categories)
        {
            categoryViewModel.PropertyChanged += CategoryChanged;
        }
    }

    private void CategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        saveController.MarkChanged();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems!= null)
                    foreach (CategoryVM categoryViewModel in e.NewItems)
                    {
                        categoryViewModel.PropertyChanged += CategoryChanged;
                    }
                if (e.OldItems != null)
                    foreach (CategoryVM categoryViewModel in e.OldItems)
                    {
                        categoryViewModel.PropertyChanged -= CategoryChanged;
                    }
                break;
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                throw new NotImplementedException();
        }
        Notify();
    }

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        Notify();
        saveController.MarkChanged();
    }

    private void Notify() => Changed?.Invoke();

    public ReadOnlyObservableCollection<CategoryVM> GetCategories() => collection;
}

class CategoryVM : BaseNotifyProperty, IComparable<CategoryVM>, IComparable
{
    private string name = string.Empty;
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    public int CompareTo(CategoryVM? other)
        => other == null ? 1 : string.Compare(Name, other.Name, StringComparison.Ordinal);

    public int CompareTo(object? obj) => CompareTo(obj as CategoryVM);

    public override string ToString() => Name;
}