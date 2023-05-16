using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
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

    private CategoryVM? selectedItem;
    public CategoryVM? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }

    public event Action? Changed;

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
            categoryViewModel.NameChanging += CategoryViewModelOnNameChanging;
        }
    }

    private void RemoveCategory(object? qwe)
    {
        if (viewResolver.ShowQuestion("Are you sure? All associations will be removed", MessageBoxButton.YesNo) == MessageBoxResult.No)
            return;
        Categories.Remove(SelectedItem!);
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
                        categoryViewModel.NameChanging += CategoryViewModelOnNameChanging;
                        categoryViewModel.PropertyChanged += CategoryChanged;
                    }
                if (e.OldItems != null)
                    foreach (CategoryVM categoryViewModel in e.OldItems)
                    {
                        categoryViewModel.NameChanging -= CategoryViewModelOnNameChanging;
                        categoryViewModel.PropertyChanged -= CategoryChanged;
                    }
                break;
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                throw new NotImplementedException();
        }
        Notify();
    }

    private bool CategoryViewModelOnNameChanging(string newName) => Categories.All(c => c.Name != newName);

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e) => Notify();

    private void Notify() => Changed?.Invoke();

    public ReadOnlyObservableCollection<CategoryVM> GetCategories() => collection;
}

class CategoryVM : BaseNotifyProperty, IComparable<CategoryVM>, IComparable
{
    public event Func<string, bool>? NameChanging;

    private string name = string.Empty;
    public string Name
    {
        get => name;
        set
        {
            if (name == value)
                return;
            bool result = true;
            if (NameChanging != null)
                result = NameChanging!.Invoke(value);
            if (result)
                SetProperty(ref name, value);
        }
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