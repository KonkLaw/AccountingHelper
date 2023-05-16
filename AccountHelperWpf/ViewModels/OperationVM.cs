using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationVM : BaseNotifyProperty
{
    private readonly Action<OperationVM> approve;
    public BaseOperation Operation { get; }

    private CategoryVM? category;
    public CategoryVM? Category
    {
        get => category;
        set => SetProperty(ref category, value);
    }

    public ICommand Approve { get; }

    private bool isApproved = true;
    public bool IsApproved
    {
        get => isApproved;
        set => SetProperty(ref isApproved, value);
    }

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    public ReadOnlyObservableCollection<CategoryVM> Categories { get; }

    public OperationVM(
        BaseOperation operation,
        ReadOnlyObservableCollection<CategoryVM> categories,
        Action<OperationVM> approve)
    {
        this.approve = approve;
        Operation = operation;
        Categories = categories;
        Approve = new DelegateCommand(ApproveHandler);
    }

    private void ApproveHandler() => approve(this);
}