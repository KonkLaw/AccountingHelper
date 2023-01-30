using System.Collections.ObjectModel;
using AccountHelperWpf.Common;

namespace AccountHelperWpf.ViewModels;

class CategoriesViewModel
{
    private readonly ReadOnlyObservableCollection<CategoryVm> collection;
    public ObservableCollection<CategoryVm> Categories { get; }

    public CategoriesViewModel(List<CategoryVm> loadedCategories)
    {
        ObservableCollection<CategoryVm> categories = new (loadedCategories);
        Categories = categories;
        collection = new ReadOnlyObservableCollection<CategoryVm>(categories);
    }

    public ReadOnlyObservableCollection<CategoryVm> GetCategories() => collection;
}

class CategoryVm : BaseNotifyProperty, IComparable<CategoryVm>, IComparable
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

    public int CompareTo(CategoryVm? other)
    {
        if (other == null)
            return 1;
        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }

    public int CompareTo(object? obj) => CompareTo(obj as CategoryVm);

    public override string ToString() => Name;
}