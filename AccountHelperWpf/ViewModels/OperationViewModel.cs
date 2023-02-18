using System.Collections.ObjectModel;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class OperationViewModel : BaseNotifyProperty
{
    private readonly ISummaryChangedListener summaryChangedListener;
    private readonly Action<CategoryViewModel?> categoryChanged;
    public BaseOperation Operation { get; }

    private CategoryViewModel? category;
    public CategoryViewModel? Category
    {
        get => category;
        set
        {
            if (!SetProperty(ref category, value))
                return;
            categoryChanged(value);
            summaryChangedListener.Changed();
        }
    }

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set
        {
            if (SetProperty(ref description, value))
                summaryChangedListener.Changed();
        }
    }

    public ReadOnlyObservableCollection<CategoryViewModel> Categories { get; }

    public OperationViewModel(BaseOperation operation,
        ReadOnlyObservableCollection<CategoryViewModel> categories,
        ISummaryChangedListener summaryChangedListener,
        Action<CategoryViewModel?> categoryChanged)
    {
        Operation = operation;
        Categories = categories;
        this.summaryChangedListener = summaryChangedListener;
        this.categoryChanged = categoryChanged;
    }
}