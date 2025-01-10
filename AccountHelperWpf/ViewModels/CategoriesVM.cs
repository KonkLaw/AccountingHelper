using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

    public int SelectedIndex { get; set; }

    public event Action<CategoryVM>? OnCategoryRemoving;
    public event Action? OnCategoryRemoved;
    public event Action? CategoryOrListChanged;

    public CategoriesVM(ObservableCollection<CategoryVM> loadedCategories, IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        Categories = loadedCategories;
        collection = new ReadOnlyObservableCollection<CategoryVM>(Categories);
        RemoveCommand = new DelegateCommand(RemoveCategory);
        MoveUpCommand = new DelegateCommand(MoveUp);
        MoveDownCommand = new DelegateCommand(MoveDown);

        Categories.CollectionChanged += CategoriesCollectionChanged;
        foreach (CategoryVM? categoryViewModel in Categories)
        {
            categoryViewModel.PropertyChanged += CategoryChanged;
        }
    }

    private void RemoveCategory()
    {
        if (SelectedIndex == 0)
            return;

        if (viewResolver.ShowQuestion("Are you sure? All saved associations for this category will be removed", MessageBoxButton.YesNo) == MessageBoxResult.No)
            return;
        CategoryVM categoryToRemove = Categories[SelectedIndex];
        OnCategoryRemoving?.Invoke(categoryToRemove);
        Categories.RemoveAt(SelectedIndex);
        OnCategoryRemoved?.Invoke();
    }

    private void MoveUp()
    {
        if (SelectedIndex == 0)
            return;

        if (SelectedIndex < 2)
            return;
        Categories.Move(SelectedIndex, SelectedIndex - 1);
    }

    private void MoveDown()
    {
        if (SelectedIndex == 0)
            return;

        if (SelectedIndex > Categories.Count - 2)
            return;
        Categories.Move(SelectedIndex, SelectedIndex + 1);
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
                    }
                if (e.OldItems != null)
                    foreach (CategoryVM categoryViewModel in e.OldItems)
                    {
                        categoryViewModel.PropertyChanged -= CategoryChanged;
                    }
                break;
            case NotifyCollectionChangedAction.Reset:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Move:
                break;
        }
        Notify();
    }

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e) => Notify();

    private void Notify() => CategoryOrListChanged?.Invoke();

    public ReadOnlyObservableCollection<CategoryVM> GetCategories() => collection;
}

class CategoryVM : BaseNotifyProperty, IComparable<CategoryVM>, IComparable
{
    public static CategoryVM Default { get; } = new()
    {
        Name = "# Not assigned",
        Description = "Default category for all not assigned operations"
    };

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

    public bool IsDefault => ReferenceEquals(this, Default);

    public int CompareTo(CategoryVM? other)
        => other == null ? 1 : string.Compare(Name, other.Name, StringComparison.Ordinal);

    public int CompareTo(object? obj) => CompareTo(obj as CategoryVM);

    public override string ToString() => Name;
}