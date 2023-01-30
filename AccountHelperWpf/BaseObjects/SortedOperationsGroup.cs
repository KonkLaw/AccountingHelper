using System.Collections.ObjectModel;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.BaseObjects;

class SortedOperationsGroup
{
    public List<OperationViewModel> Operations { get; }
    public string Name { get; }

    public SortedOperationsGroup(
        OperationsGroup operationGroup, ReadOnlyObservableCollection<CategoryVm> categories, ICategoryChangedListener categoryChangedListener)
    {
        Name = operationGroup.Name;
        Operations = new List<OperationViewModel>(operationGroup.Operations.Count);
        foreach (BaseOperation operation in operationGroup.Operations)
        {
            Operations.Add(new OperationViewModel(operation, categories, categoryChangedListener));
        }
    }
}