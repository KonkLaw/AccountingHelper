using System.Collections.ObjectModel;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.BaseObjects;

class SortedOperation : BaseNotifyProperty
{
    private readonly ICategoryChangedListener categoryChangedListener;
    public Operation Operation { get; }

    private CategoryVm? category;
    public CategoryVm? Category
    {
        get => category;
        set
        {
            if (SetProperty(ref category, value))
                categoryChangedListener.CategoryChanged();
        }
    }

    public ReadOnlyObservableCollection<CategoryVm> Categories { get; }

    public SortedOperation(
        Operation operation,
        ReadOnlyObservableCollection<CategoryVm> categories,
        ICategoryChangedListener categoryChangedListener)
    {
        Operation = operation;
        Categories = categories;
        this.categoryChangedListener = categoryChangedListener;
    }
}