using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class CategorySummaryTemp
{
    private readonly string name;
    private readonly bool groupWithSameComment;
    private decimal totalAmount;
    private readonly List<(string, decimal)> tags = new();

    public CategorySummaryTemp(string name, bool groupWithSameComment)
    {
        this.name = name;
        this.groupWithSameComment = groupWithSameComment;
    }

    public void Add(OperationVM operationVM)
    {
        decimal amount = operationVM.Operation.Amount;
        string comment = operationVM.Comment;

        totalAmount += amount;

        if (string.IsNullOrEmpty(comment))
            return;

        if (groupWithSameComment)
        {
            int index = tags.FindIndex(tag => tag.Item1 == comment);
            if (index < 0)
            {
                tags.Add((comment, amount));
            }
            else
            {
                (string, decimal) oldValue = tags[index];
                oldValue.Item2 += amount;
                tags[index] = oldValue;
            }
        }
        else
        {
            tags.Add((comment, amount));
        }
    }

    public CategoryDetails GetDetails() => new CategoryDetails(name, totalAmount, tags);
}

class SummaryHelper
{
    public static void PrepareSummary(
        IEnumerable<CategoryVM> categoriesVM,
        IEnumerable<OperationVM> operationsVM,
        bool groupWithSameComment,
        out bool isSorted, out ICollection<CategoryDetails> details)
    {
        CategorySummaryTemp notAssigned = new("Not assigned", groupWithSameComment);
        Dictionary<CategoryVM, CategorySummaryTemp> dictionary = categoriesVM.
            ToDictionary(c => c, c => new CategorySummaryTemp(c.Name, groupWithSameComment));
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

            if (operation.AssociationStatus == AssociationStatus.NotMatch)
                isSorted = false;
        }
        List<CategoryDetails> list = new (dictionary.Values.Select(c => c.GetDetails()))
        {
            notAssigned.GetDetails()
        };
        details = list;
    }
}