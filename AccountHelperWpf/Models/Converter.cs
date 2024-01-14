using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.Models;

public static class Converter
{
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
        return new OperationsFile($"{file.FileName} ({file.Currency})", operations, file.Currency);
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
        foreach (Parsing.PkoOperation operation in file.NonBlockedOperations)
        {
            CheckCurrency(operation.Currency);
            operations.Add(PkoOperation.Convert(operations.Count, operation));
        }

        return new OperationsFile(file.FileName, operations, fixedCurrent);
    }
}