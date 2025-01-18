using System.Text;
using AccountHelperWpf.Models;

namespace AccountHelperWpf.Parsing;


// ==================== Prior

public record PriorOperation(
    DateTime TransactionDateTime, decimal Amount, OperationDescription Description,
    string CategoryName,
    string Currency,
    decimal Fee,
    decimal InitialAmount,
    DateOnly AccountDate);

public record PriorBlockedOperation(
    DateTime TransactionDateTime, decimal Amount, OperationDescription Description,
    string CategoryName,
    string Currency,
    decimal InitialAmount,
    string InitialCurrency);

public readonly record struct PriorOperationsGroup(
    string Name,
    IReadOnlyList<PriorOperation> Operations);

public readonly record struct PriorBlockedOperationsGroup(
    string Name,
    IReadOnlyList<PriorBlockedOperation> Operations);


public record PriorFile(
    string FileName, string Currency,
    IReadOnlyList<PriorOperationsGroup> NonBlockedOperationsGroups,
    IReadOnlyList<PriorBlockedOperationsGroup> BlockedOperationsGroups);





// ==================== PKO

public record PkoOperation(DateTime TransactionDateTime, decimal Amount, OperationDescription Description,
        DateOnly DateAccounting,
        string Currency,
        string OperationType,
        string? OriginalAmount,
        decimal SaldoBeforeTransaction,
        SortedDictionary<string, string> OtherDescription);

public record PkoOtherDescription
{
    private readonly SortedDictionary<string, string> description;
    private readonly string shortDescription;

    private string? fullDescription;
    public string FullDescription
    {
        get
        {
            if (fullDescription == null)
            {
                StringBuilder result = new();
                foreach (KeyValuePair<string, string> tagContent in description)
                {
                    result.AppendLine($"{tagContent.Key} : {tagContent.Value}");
                    result.AppendLine();
                }
                fullDescription = result.ToString();
            }
            return fullDescription;
        }
    }

    public PkoOtherDescription(SortedDictionary<string, string> description)
    {
        this.description = description;
        shortDescription = string.Join(" || ", description.Select(kvp => $"{kvp.Key} : {kvp.Value}"));
    }

    public override string ToString() => shortDescription;
}

public record PkoBlockedOperation(
    DateTime TransactionDateTime, decimal Amount, OperationDescription Description,
    string Currency, string OtherDescription);

public record PkoFile(
    string FileName,
    IReadOnlyList<PkoOperation> NonBlockedOperations,
    IReadOnlyList<PkoBlockedOperation>? BlockedOperations,
    bool WithSaldo);