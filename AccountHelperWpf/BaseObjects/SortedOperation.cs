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

    public ObservableCollection<CategoryVm> Categories { get; }

    public SortedOperation(
        Operation operation,
        ObservableCollection<CategoryVm> categories,
        ICategoryChangedListener categoryChangedListener)
    {
        Operation = operation;
        Categories = categories;
        this.categoryChangedListener = categoryChangedListener;
    }
}