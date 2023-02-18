using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using AccountHelperWpf.Common;

namespace AccountHelperWpf.ViewModels;

class CategoriesViewModel
{
    private readonly ReadOnlyObservableCollection<CategoryViewModel> collection;
    public ObservableCollection<CategoryViewModel> Categories { get; }
    public event Action? Changed;

    public CategoriesViewModel(List<CategoryViewModel> loadedCategories)
    {
        Categories = new (loadedCategories);
        collection = new ReadOnlyObservableCollection<CategoryViewModel>(Categories);


        Categories.CollectionChanged += CategoriesCollectionChanged;
        foreach (CategoryViewModel categoryViewModel in Categories)
        {
            categoryViewModel.PropertyChanged += CategoryChanged;
        }
    }

    private void CategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems!= null)
                    foreach (CategoryViewModel categoryViewModel in e.NewItems)
                    {
                        categoryViewModel.PropertyChanged += CategoryChanged;
                    }
                if (e.OldItems != null)
                    foreach (CategoryViewModel categoryViewModel in e.OldItems)
                    {
                        categoryViewModel.PropertyChanged -= CategoryChanged;
                    }
                break;
            // TODO: handle clear;
        }
        Notify();
    }

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e) => Notify();

    private void Notify() => Changed?.Invoke();

    public ReadOnlyObservableCollection<CategoryViewModel> GetCategories() => collection;
}

class CategoryViewModel : BaseNotifyProperty, IComparable<CategoryViewModel>, IComparable
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

    public int CompareTo(CategoryViewModel? other)
        => other == null ? 1 : string.Compare(Name, other.Name, StringComparison.Ordinal);

    public int CompareTo(object? obj) => CompareTo(obj as CategoryViewModel);

    public override string ToString() => Name;
}