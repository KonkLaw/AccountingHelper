using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Parsing;


// ==================== Prior

public record PriorOperation(DateTime TransactionDateTime, decimal Amount, string Description,
        [property: Width(150)]
        string CategoryName,
        string Currency,
        decimal Fee,
        decimal InitialAmount,
        [property: StringFormat("dd-MM-yyyy")]
        DateOnly AccountDate);

public record PriorBlockedOperation(DateTime TransactionDateTime, decimal Amount, string Description,
        [property: Width(150)]
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
        [property: StringFormat("dd-MM-yyyy")]
        DateOnly DateAccounting,
        string Currency,
        [property: Width(160)]
        string OperationType,
        string? OriginalAmount,
        decimal SaldoBeforeTransaction,
        string OtherDescription);

public record PkoBlockedOperation(DateTime TransactionDateTime, decimal Amount, string Description,
    string OtherDescription);

public readonly record struct OperationsGroup(
    string Name,
    IReadOnlyList<BaseOperation> Operations);

public record PkoFile(
    string FileName, 
    IReadOnlyList<PkoOperation> NonBlockedOperations,
    IReadOnlyList<PkoBlockedOperation>? BlockedOperations);