using System.Collections.ObjectModel;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.BaseObjects;

class OperationViewModel : BaseNotifyProperty
{
    private readonly ICategoryChangedListener categoryChangedListener;
    public BaseOperation Operation { get; }

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

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set
        {
            if (SetProperty(ref description, value))
                categoryChangedListener.CategoryChanged();
        }
    }

    public ReadOnlyObservableCollection<CategoryVm> Categories { get; }

    public OperationViewModel(
        BaseOperation operation,
        ReadOnlyObservableCollection<CategoryVm> categories,
        ICategoryChangedListener categoryChangedListener)
    {
        Operation = operation;
        Categories = categories;
        this.categoryChangedListener = categoryChangedListener;
    }
}