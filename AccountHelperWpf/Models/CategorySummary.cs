using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewModels;
using System.Text;
using System.Threading;

namespace AccountHelperWpf.Models;

class CategorySummaryTemp
{
    private readonly CategoryVM? categoryVM;
    private readonly List<OperationVM> operations = new();

    public CategorySummaryTemp(CategoryVM? categoryVM)
    {
        this.categoryVM = categoryVM;
    }

    public void Add(OperationVM operationVM) => operations.Add(operationVM);

    public (decimal amount, IReadOnlyList<CommentInfo> list) GetSummary()
    {
        List<CommentInfo> list = new();
        decimal sum = 0;
        foreach (OperationVM operation in operations)
        {
            sum += operation.Operation.Amount;
            if (string.IsNullOrEmpty(operation.Comment))
                continue;
            list.Add(new CommentInfo(operation.Comment, operation.Operation.Amount));
        }
        return (sum, list);
    }
}

class SummaryHelper
{
    public static void PrepareSummary(IEnumerable<CategoryVM> categoriesVM, IEnumerable<OperationVM> operationsVM,
        out bool isSorted, out SummaryVM summaryVM)
    {
        CategorySummaryTemp notAssigned = new(null);
        Dictionary<CategoryVM, CategorySummaryTemp> categoriesSummary = categoriesVM.
            ToDictionary(c => c, c => new CategorySummaryTemp(c));
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
                categoriesSummary[operation.Category].Add(operation);

            if (operation.AssociationStatus == AssociationStatus.NotCorrespond)
                isSorted = false;
        }

        List<SummaryItem> items = new ();
        foreach (KeyValuePair<CategoryVM, CategorySummaryTemp> categorySummaryTemp in categoriesSummary)
        {
            (decimal amount, IReadOnlyList<CommentInfo> list) = categorySummaryTemp.Value.GetSummary();
            items.Add(new SummaryItem(categorySummaryTemp.Key.Name, amount, list));
        }

        (decimal amount, IReadOnlyList<CommentInfo> list) notAssignedData = notAssigned.GetSummary();
        const string nameForNoAssociatedOperations = "Not Assigned";
        items.Add(new SummaryItem(nameForNoAssociatedOperations, notAssignedData.amount, notAssignedData.list));
        summaryVM = new SummaryVM(items);
    }
}

record CommentInfo(string Comment, decimal Amount);

class SummaryItem
{
    private readonly string name;
    private readonly decimal amount;
    private readonly IReadOnlyList<CommentInfo> commentedOperations;

    public SummaryItem(
        string name,
        decimal amount,
        IReadOnlyList<CommentInfo> commentedOperations)
    {
        this.name = name;
        this.amount = amount;
        this.commentedOperations = commentedOperations;
    }

    public StringBuilder GetDescription()
    {
        StringBuilder result = new();
        result.Append('#');
        result.Append(name);
        result.Append(' ');

        StringBuilder detailed = new();
        foreach (CommentInfo commentInfo in commentedOperations)
        {
            if (detailed.Length != 0)
                detailed.Append(' ');
            detailed.Append(commentInfo.Amount.ToGoodString());
            detailed.Append(' ');
            detailed.Append(commentInfo.Comment);
            detailed.Append(',');
        }
        result.Append(amount.ToGoodString());
        if (detailed.Length != 0)
        {
            detailed.Remove(detailed.Length - 1, 1);
            result.Append(" (");
            result.Append(detailed);
            result.Append(')');
        }
        return result;
    }
}

class SummaryVM
{
    private readonly IReadOnlyList<SummaryItem> summaryItems;

    public SummaryVM(IReadOnlyList<SummaryItem> summaryItems)
    {
        this.summaryItems = summaryItems;
    }

    public StringBuilder GetDescription()
    {
        StringBuilder stringBuilder = new();
        foreach (SummaryItem summaryItem in summaryItems)
        {
            stringBuilder.Append(summaryItem.GetDescription());
            stringBuilder.AppendLine();
        }
        return stringBuilder;
    }
}