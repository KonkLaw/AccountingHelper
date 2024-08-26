using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Models;

public record BaseOperation(DateTime TransactionDateTime, decimal Amount, string Description)
{
	public sealed override string ToString() => $"{TransactionDateTime:dd-MM-yyyy}|{Amount}|{Description}";
};

record PkoOperation(DateTime TransactionDateTime, decimal Amount, string Description,
    [property: StringFormat("dd-MM-yyyy")]
    DateOnly? DateAccounting,
    [property: Width(160)]
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
    DateTime TransactionDateTime, decimal Amount, string Description,
    [property: Width(150)] string CategoryName,
    decimal? Fee,
    decimal InitialAmount,
    [property: StringFormat("dd-MM-yyyy")] DateOnly? AccountDate,
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



public record OperationsFile(string Name, IReadOnlyList<BaseOperation> Operations, string Currency)
{
    public string GetTitle() => $"({Currency}) {Name}";
}