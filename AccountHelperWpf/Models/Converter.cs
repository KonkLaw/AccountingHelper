using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.Models;

public static class Converter
{
    private const string DateFormat = "dd-MM-yyyy";

    public static OperationsFile Convert(PriorFile file)
    {
        List<BaseOperation> operations = new List<BaseOperation>();
        foreach (PriorBlockedOperationsGroup group in file.BlockedOperationsGroups)
        {
            operations.AddRange(group.Operations.Select(operation => PriorOperation.Convert(operation, group.Name)));
        }
        foreach (PriorOperationsGroup group in file.NonBlockedOperationsGroups)
        {
            operations.AddRange(group.Operations.Select(operation => PriorOperation.Convert(operation, group.Name)));
        }
        return new OperationsFile(
            $"{file.FileName} ({file.Currency})",
            operations,
            GetPriorDescription(),
            file.Currency);
    }

    private static IReadOnlyCollection<ColumnDescription> GetPriorDescription()
    {
        return
        [
            new ColumnDescription(nameof(PriorOperation.CategoryName), 150, null),
            new ColumnDescription(nameof(PriorOperation.AccountDate), null, DateFormat),
        ];
    }

    public static OperationsFile Convert(PkoFile file)
    {
        List<BaseOperation> operations = new List<BaseOperation>();
        string fixedCurrent = file.NonBlockedOperations[0].Currency;

        void CheckCurrency(string currency)
        {
            if (fixedCurrent != currency)
                throw new Exception("File contains operations with different currencies.");
        }
        
        if (file.BlockedOperations != null)
        {
            foreach (PkoBlockedOperation operation in file.BlockedOperations)
            {
                CheckCurrency(operation.Currency);
                operations.Add(PkoOperation.Convert(operation, operations.Count));
            }
        }
        // Operations may be reordered by sorting, so use each operation's original file position
        // (offset past the blocked operations) for its Id rather than its index in the sorted list.
        int nonBlockedIdOffset = operations.Count;
        foreach (Parsing.PkoOperation operation in file.NonBlockedOperations)
        {
            CheckCurrency(operation.Currency);
            operations.Add(PkoOperation.Convert(nonBlockedIdOffset + operation.FileIndex, operation));
        }

        return new OperationsFile(file.FileName, operations, GetPkoDescription(file.WithSaldo, file.Sorted), fixedCurrent);
    }

    private static IReadOnlyCollection<ColumnDescription> GetPkoDescription(bool isSaldoVisible, bool isSorted)
    {
        return
        [
            new ColumnDescription(nameof(PkoOperation.DateAccounting), null, DateFormat),
            new ColumnDescription(nameof(PkoOperation.OperationType), 160, null),
            new ColumnDescription(nameof(PkoOperation.SaldoAfterTransaction), null, null, null, !isSaldoVisible),
			new ColumnDescription(nameof(PkoOperation.IsLinked), null, null, null, true), // hide
			new ColumnDescription(nameof(PkoOperation.LinkedToPrev), null, null, null, !isSorted),
            new ColumnDescription(nameof(PkoOperation.OtherDescription), null, null,
                $"{nameof(PkoOperation.OtherDescription)}.{nameof(PkoOtherDescription.FullDescription)}", false)
        ];
    }
}