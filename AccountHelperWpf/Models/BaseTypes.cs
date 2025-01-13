using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;
using System.Collections.ObjectModel;

namespace AccountHelperWpf.Models;

public record BaseOperation(DateTime TransactionDateTime, decimal Amount, OperationDescription Description)
{
	public sealed override string ToString() => $"{TransactionDateTime:dd-MM-yyyy}|{Amount}|{Description}";
}

record PkoOperation(DateTime TransactionDateTime, decimal Amount, OperationDescription Description,
    DateOnly? DateAccounting,
    string? OperationType,
    string? OriginalAmount,
    decimal? SaldoBeforeTransaction,
    int Id,
    string OtherDescription)
    :
    BaseOperation(TransactionDateTime, Amount, Description)
{
    public static PkoOperation Convert(int id, Parsing.PkoOperation operation) => new PkoOperation(
            operation.TransactionDateTime, operation.Amount, operation.Description,
            DateAccounting: operation.DateAccounting,
            OperationType: operation.OperationType,
            OriginalAmount: operation.OriginalAmount,
            SaldoBeforeTransaction: operation.SaldoBeforeTransaction,
            Id: id,
            OtherDescription: operation.OtherDescription
        );

    public static PkoOperation Convert(PkoBlockedOperation operation, int id) => new PkoOperation(
            operation.TransactionDateTime, operation.Amount, operation.Description,
            DateAccounting: null,
            OperationType: null,
            OriginalAmount: null,
            SaldoBeforeTransaction: null,
            Id: id,
            OtherDescription: operation.OtherDescription
        );
}


public record PriorOperation(
    DateTime TransactionDateTime, decimal Amount, OperationDescription Description,
    string CategoryName,
    decimal? Fee,
    decimal InitialAmount,
    DateOnly? AccountDate,
    string? InitialCurrency,
    string OperationGroupName)
    :
        BaseOperation(TransactionDateTime, Amount, Description)
{
    public static PriorOperation Convert(Parsing.PriorOperation operation, string groupName) =>
        new PriorOperation(
            TransactionDateTime: operation.TransactionDateTime,
            Amount: operation.Amount,
            Description: operation.Description,
            CategoryName: operation.CategoryName,
            Fee: operation.Fee,
            InitialAmount: operation.InitialAmount,
            AccountDate: operation.AccountDate,
            InitialCurrency: null,
            OperationGroupName: groupName);

    public static PriorOperation Convert(PriorBlockedOperation operation, string groupName) =>
        new PriorOperation(
            TransactionDateTime: operation.TransactionDateTime,
            Amount: operation.Amount,
            Description: operation.Description,
            CategoryName: operation.CategoryName,
            Fee: null,
            InitialAmount: operation.InitialAmount,
            AccountDate: null,
            InitialCurrency: operation.InitialCurrency,
            OperationGroupName: groupName);
}

public record OperationsFile(string Name, IReadOnlyList<BaseOperation> Operations, IReadOnlyCollection<ColumnDescription> ColumnDescriptions, string Currency)
{
    public string GetTitle() => $"({Currency}) {Name}";
}

public class OperationDescription
{
    private readonly string bankId;
    private readonly SortedDictionary<string, string> tagsContents;

    public string DisplayName { get; }
    public string ComparisonKey { get; }

    public static OperationDescription Create(
        string bankId, SortedDictionary<string, string> tagsContents)
    {
        return new OperationDescription(bankId, tagsContents);
    }

    private OperationDescription(
        string bankId, SortedDictionary<string, string> tagsContents)
    {
        this.bankId = bankId;
        this.tagsContents = tagsContents;
        OperationDescriptionHelper.GetOperationDisplayName(
            bankId, tagsContents,
            out string displayName, out string comparisonKey);
        DisplayName = displayName;
        ComparisonKey = comparisonKey;
    }

    public void GetData(out string bankIdOut, out ReadOnlyDictionary<string, string> tagContentsOut)
    {
        bankIdOut = bankId;
        // wrap to prevent accidental modification
        tagContentsOut = new ReadOnlyDictionary<string, string>(tagsContents);
    }

    public override string ToString() => DisplayName;
}