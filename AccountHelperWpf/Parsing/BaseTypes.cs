using System;
using System.Collections.Generic;

namespace AccountHelperWpf.Parsing;

public record Operation(
    DateTime TransactionDateTime,
    string OperationName,
    decimal Amount,
    string Currency,
    DateTime AccountDate,
    float Fee,
    decimal AccountAmount,
    string Category);

public readonly record struct OperationsGroup(
    string Name,
    IReadOnlyList<Operation> Operations);

public record AccountDescription(string Name, string Currency);

public record AccountFile(
    AccountDescription Description,
    IReadOnlyList<OperationsGroup> OperationsGroups);