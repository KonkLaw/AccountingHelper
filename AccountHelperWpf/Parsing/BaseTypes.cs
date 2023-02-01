namespace AccountHelperWpf.Parsing;

public record BaseOperation(DateTime TransactionDateTime, decimal Amount, string Description);

public record PriorOperation(DateTime TransactionDateTime, decimal Amount, string Description,
    string CategoryName,
    string Currency,
    decimal Fee,
    decimal InitialAmount,
    DateOnly AccountDate)
    : BaseOperation(TransactionDateTime, Amount, Description);

record PkoOperation(DateTime TransactionDateTime, decimal Amount, string Description,
    DateOnly DateAccounting,
    string Currency,
    string OperationType,
    decimal SaldoBeforeTransaction,
    string OtherDescription)
    : BaseOperation(TransactionDateTime, Amount, Description);

public readonly record struct OperationsGroup(
    string Name,
    IReadOnlyList<BaseOperation> Operations);

public record AccountDescription(string Name, string Currency);

public record AccountFile(
    AccountDescription Description,
    IReadOnlyList<OperationsGroup> OperationsGroups);