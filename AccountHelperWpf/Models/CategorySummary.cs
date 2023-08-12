using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewModels;
using System.Text;

namespace AccountHelperWpf.Models;

class CategorySummaryTemp
{
    private readonly string name;
    private decimal amount;
    private readonly Dictionary<string, decimal> tagsToAmount = new();
    private readonly StringBuilder descriptionFull = new();
    
    public CategorySummaryTemp(string name)
    {
        this.name = name;
    }

    public void Add(OperationVM operationVM)
    {
        amount += operationVM.Operation.Amount;
        if (string.IsNullOrEmpty(operationVM.Comment))
            return;
        if (descriptionFull.Length != 0)
            descriptionFull.Append(", ");
        descriptionFull.Append(operationVM.Operation.Amount.ToGoodString());
        descriptionFull.Append(' ');
        descriptionFull.Append(operationVM.Comment);

        tagsToAmount.TryGetValue(operationVM.Comment, out decimal oldAmount);
        tagsToAmount[operationVM.Comment] = oldAmount + operationVM.Operation.Amount;
    }

    public CategoryDetails GetDetails()
    {
        StringBuilder descriptionShort = new();
        foreach (KeyValuePair<string, decimal> keyValuePair in tagsToAmount)
        {
            if (descriptionShort.Length != 0)
                descriptionShort.Append(", ");
            descriptionShort.Append(keyValuePair.Value.ToGoodString());
            descriptionShort.Append(' ');
            descriptionShort.Append(keyValuePair.Key);
        }
        return new CategoryDetails(name, amount, descriptionFull.ToString(), descriptionShort.ToString());
    }
}

class SummaryHelper
{
    public static void PrepareSummary(
        IEnumerable<CategoryVM> categoriesVM, IEnumerable<OperationVM> operationsVM,
        out bool isSorted, out ICollection<CategoryDetails> details)
    {
        CategorySummaryTemp notAssigned = new("Not assigned");
        Dictionary<CategoryVM, CategorySummaryTemp> dictionary = categoriesVM.
            ToDictionary(c => c, c => new CategorySummaryTemp(c.Name));
        isSorted = true;
        foreach (OperationVM operation in operationsVM)
        {
            if (!operation.IsApproved)
                isSorted = false;
            if (operation.Category == null)
            {
                notAssigned.Add(operation);
                isSorted = false;
            }
            else
                dictionary[operation.Category].Add(operation);

            if (operation.AssociationStatus == AssociationStatus.NotCorrespond)
                isSorted = false;
        }
        List<CategoryDetails> list = new (dictionary.Values.Select(c => c.GetDetails()))
        {
            notAssigned.GetDetails()
        };
        details = list;
    }
}