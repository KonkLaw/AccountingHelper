using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AccountHelperWpf.Common;

namespace AccountHelperWpf.ViewModels;

class CategoriesViewModel
{
    public ObservableCollection<CategoryVm> Categories { get; } = new ();


    public CategoriesViewModel()
    {
        Categories.CollectionChanged += CaterogiesOnCollectionChanged;
    }

    private void CaterogiesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ;
    }
}

class CategoryVm : BaseNotifyProperty
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

    public override string ToString() => name;
}