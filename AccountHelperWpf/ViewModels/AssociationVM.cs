using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationVM : BaseNotifyProperty
{
    public string OperationDescription { get; }

    private CategoryVM categoryVM;
    public CategoryVM CategoryVM
    {
        get => categoryVM;
        set => SetProperty(ref categoryVM, value);
    }

    private bool isNew;
    public bool IsNew
    {
        get => isNew;
        set => SetProperty(ref isNew, value);
    }

    public AssociationVM(
        string operationDescription, CategoryVM categoryVM, bool isNew)
    {
        OperationDescription = operationDescription;
        this.categoryVM = categoryVM;
        IsNew = isNew;
    }
}