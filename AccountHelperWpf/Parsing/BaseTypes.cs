using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Parsing;

public record BaseOperation(DateTime TransactionDateTime, decimal Amount, string Description);

public record PriorOperation(DateTime TransactionDateTime, decimal Amount, string Description,
        [property: Width(150)]
        string CategoryName,
        string Currency,
        decimal Fee,
        decimal InitialAmount,
        [property: StringFormat("dd-MM-yyyy")]
        DateOnly AccountDate)
    : BaseOperation(TransactionDateTime, Amount, Description);

public record PriorBlockedOperation(DateTime TransactionDateTime, decimal Amount, string Description,
        [property: Width(150)]
        string CategoryName,
        string Currency,
        decimal InitialAmount,
        string InitialCurrency)
    : BaseOperation(TransactionDateTime, Amount, Description);

record PkoOperation(DateTime TransactionDateTime, decimal Amount, string Description,
        [property: StringFormat("dd-MM-yyyy")]
        DateOnly DateAccounting,
        string Currency,
        [property: Width(160)]
        string OperationType,
        string? OriginalAmount,
        decimal SaldoBeforeTransaction,
        string OtherDescription)
    : BaseOperation(TransactionDateTime, Amount, Description);

record PkoBlockedOperation(DateTime TransactionDateTime, decimal Amount, string Description,
    string OtherDescription)
    : BaseOperation(TransactionDateTime, Amount, Description);

public readonly record struct OperationsGroup(
    string Name,
    IReadOnlyList<BaseOperation> Operations);

public record AccountDescription(string Name, string Currency);

public record AccountFile(
    AccountDescription Description,
    IReadOnlyList<OperationsGroup> OperationsGroups);