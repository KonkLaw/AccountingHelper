using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class CategoriesVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    private readonly ReadOnlyObservableCollection<CategoryVM> collection;
    public ObservableCollection<CategoryVM> Categories { get; }
    public ICommand RemoveCommand { get; }

    public int SelectedIndex { get; set; }

    public event Action? OnCategoryRemoving;
    public event Action? OnCategoryRemoved;
    public event Action? CategoryOrListChanged;

    public CategoriesVM(ObservableCollection<CategoryVM> loadedCategories, IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        Categories = loadedCategories;
        collection = new ReadOnlyObservableCollection<CategoryVM>(Categories);
        RemoveCommand = new DelegateCommand<object>(RemoveCategory);

        Categories.CollectionChanged += CategoriesCollectionChanged;
        foreach (CategoryVM? categoryViewModel in Categories)
        {
            categoryViewModel.PropertyChanged += CategoryChanged;
        }
    }

    private void RemoveCategory(object? qwe)
    {
        if (viewResolver.ShowQuestion("Are you sure? All associations will be removed", MessageBoxButton.YesNo) == MessageBoxResult.No)
            return;
        OnCategoryRemoving?.Invoke();
        Categories.RemoveAt(SelectedIndex);
        OnCategoryRemoved?.Invoke();
    }

    private void CategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems!= null)
                    foreach (CategoryVM categoryViewModel in e.NewItems)
                    {
                        categoryViewModel.PropertyChanged += CategoryChanged;
                        Debug.WriteLine("Add");
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

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e) => Notify();

    private void Notify() => CategoryOrListChanged?.Invoke();

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