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
        return new OperationsFile($"{file.FileName} ({file.Currency})", operations);
    }

    public static OperationsFile Convert(PkoFile file)
    {
        List<BaseOperation> operations = new List<BaseOperation>();
        if (file.BlockedOperations != null)
            operations.AddRange(file.BlockedOperations.Select(PkoOperation.Convert));
        operations.AddRange(file.NonBlockedOperations.Select(operation => (BaseOperation)PkoOperation.Convert(operation)));
        return new OperationsFile(file.FileName, operations);
    }

    public static IReadOnlyList<BaseOperation> ConvertBlockedOperations(IReadOnlyList<PkoBlockedOperation> blockedOperations)
        => blockedOperations.Select(PkoOperation.Convert).ToList();
}