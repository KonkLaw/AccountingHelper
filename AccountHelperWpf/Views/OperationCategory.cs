using System.Collections.ObjectModel;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.Views;

class OperationCategory : BaseNotifyProperty
{
    public Operation Operation { get; }

    private string? category;
    public string? Category
    {
        get => category;
        set => SetProperty(ref category, value);
    }

    private readonly ObservableCollection<string> categories;
    public ObservableCollection<string> Categories => categories;

    public OperationCategory(Operation operation, ObservableCollection<string> categories)
    {
        Operation = operation;
        this.categories = categories;
    }
}