using System.Collections.ObjectModel;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class OperationViewModel : BaseNotifyProperty
{
    private readonly ISummaryChangedListener summaryChangedListener;
    public BaseOperation Operation { get; }

    private CategoryVm? category;
    public CategoryVm? Category
    {
        get => category;
        set
        {
            if (SetProperty(ref category, value))
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

    public ReadOnlyObservableCollection<CategoryVm> Categories { get; }

    public OperationViewModel(
        BaseOperation operation,
        ReadOnlyObservableCollection<CategoryVm> categories,
        ISummaryChangedListener summaryChangedListener)
    {
        Operation = operation;
        Categories = categories;
        this.summaryChangedListener = summaryChangedListener;
    }
}