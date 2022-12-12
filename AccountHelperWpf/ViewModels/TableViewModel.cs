using System.Collections.Generic;
using System.Collections.ObjectModel;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class TableViewModel
{
    public IEnumerable<OperationCategory> Operations { get; set; }

    public TableViewModel(AccountFile[] accountFiles)
    {
        IReadOnlyList<Operation> operations = accountFiles[0].OperationsGroups[1].Operations;
        List<OperationCategory> operationCategories = new List<OperationCategory>(operations.Count);

        ObservableCollection<string> categories = new ObservableCollection<string>
        {
            "Qwe1",
            "Asd2",
            "Zxc3"
        };

        foreach (Operation operation in operations)
        {
            operationCategories.Add(new OperationCategory(operation, categories));
        }
        Operations = operationCategories;
    }
}