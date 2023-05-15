using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationVM : BaseNotifyProperty
{
    private CategoryVM categoryVM;

    public AssociationVM(string operationDescription, CategoryVM categoryVM)
    {
        OperationDescription = operationDescription;
        this.categoryVM = categoryVM;
    }

    public string OperationDescription { get; }
    public CategoryVM CategoryVM
    {
        get => categoryVM;
        set => SetProperty(ref categoryVM, value);
    }
}