using System.Text;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class CategorySummary
{
    private readonly string categoryName;
    private readonly List<OperationVM> operations = new();

    public CategorySummary(string categoryName) => this.categoryName = categoryName;

    public void Add(OperationVM operationVM) => operations.Add(operationVM);

    public StringBuilder GetSummary()
    {
        StringBuilder result = new();
        result.Append('#');
        result.Append(categoryName);
        result.Append(' ');

        decimal sum = 0;
        StringBuilder detailed = new();
        foreach (OperationVM operation in operations)
        {
            sum += operation.Operation.Amount;
            if (string.IsNullOrEmpty(operation.Comment))
                continue;
            if (detailed.Length != 0)
                detailed.Append(' ');
            detailed.Append(operation.Operation.Amount.ToGoodString());
            detailed.Append(' ');
            detailed.Append(operation.Comment);
            detailed.Append(',');
        }
        result.Append(sum.ToGoodString());

        if (detailed.Length != 0)
        {
            detailed.Remove(detailed.Length - 1, 1);
            result.Append(" (");
            result.Append(detailed);
            result.Append(')');
        }

        return result;
    }

    public static void PrepareSummary(IEnumerable<CategoryVM> categoriesVM, IEnumerable<OperationVM> operationsVM,
        out bool isSorted, out string summary)
    {
        CategorySummary notAssigned = new("Not Assigned");
        Dictionary<CategoryVM, CategorySummary> categoriesSummary = categoriesVM.
            ToDictionary(c => c, c => new CategorySummary(c.Name));
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
        }
        StringBuilder stringBuilder = new();
        foreach (CategorySummary categorySummary in categoriesSummary.Values)
        {
            stringBuilder.Append(categorySummary.GetSummary());
            stringBuilder.AppendLine();
        }
        stringBuilder.Append(notAssigned.GetSummary());
        summary = stringBuilder.ToString();
    }
}