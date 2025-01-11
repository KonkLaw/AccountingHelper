namespace AccountHelperWpf.Parsing;


// ==================== Prior

public record PriorOperation(
    DateTime TransactionDateTime, decimal Amount, string Description,
    string CategoryName,
    string Currency,
    decimal Fee,
    decimal InitialAmount,
    DateOnly AccountDate);

public record PriorBlockedOperation(
    DateTime TransactionDateTime, decimal Amount, string Description,
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

public record PkoOperation(DateTime TransactionDateTime, decimal Amount, string Description,
        DateOnly DateAccounting,
        string Currency,
        string OperationType,
        string? OriginalAmount,
        decimal SaldoBeforeTransaction,
        string OtherDescription);

public record PkoBlockedOperation(
    DateTime TransactionDateTime, decimal Amount, string Description,
    string Currency, string OtherDescription);

public record PkoFile(
    string FileName,
    IReadOnlyList<PkoOperation> NonBlockedOperations,
    IReadOnlyList<PkoBlockedOperation>? BlockedOperations,
    bool WithSaldo);