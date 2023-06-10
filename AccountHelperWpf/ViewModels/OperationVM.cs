using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationVM : BaseNotifyProperty
{
    public BaseOperation Operation { get; }

    private CategoryVM? category;
    public CategoryVM? Category
    {
        get => category;
        set
        {
            SetProperty(ref category, value);
            IsApproved = true;
        }
    }

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

    public OperationVM(BaseOperation operation)
    {
        Operation = operation;
    }
}