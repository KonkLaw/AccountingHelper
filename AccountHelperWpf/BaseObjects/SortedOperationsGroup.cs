using System.Collections.Generic;
using System.Collections.ObjectModel;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.BaseObjects;

class SortedOperationsGroup
{
    public List<SortedOperation> Operations { get; }
    public string Name { get; }

    public SortedOperationsGroup(
        OperationsGroup operationGroup, ReadOnlyObservableCollection<CategoryVm> categories, ICategoryChangedListener categoryChangedListener)
    {
        Name = operationGroup.Name;
        Operations = new List<SortedOperation>(operationGroup.Operations.Count);
        foreach (Operation operation in operationGroup.Operations)
        {
            Operations.Add(new SortedOperation(operation, categories, categoryChangedListener));
        }
    }
}