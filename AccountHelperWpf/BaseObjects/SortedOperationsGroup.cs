using System.Collections.Generic;
using System.Collections.ObjectModel;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.BaseObjects;

class SortedOperationsGroup
{
    public ObservableCollection<CategoryVm> Categories { get; }
    public List<SortedOperation> Operations { get; }
    public string Name { get; }

    public SortedOperationsGroup(
        OperationsGroup operationGroup, ObservableCollection<CategoryVm> categories, ICategoryChangedListener categoryChangedListener)
    {
        Categories = categories;
        Name = operationGroup.Name;
        Operations = new List<SortedOperation>(operationGroup.Operations.Count);
        foreach (Operation operation in operationGroup.Operations)
        {
            Operations.Add(new SortedOperation(operation, categories, categoryChangedListener));
        }
    }
}