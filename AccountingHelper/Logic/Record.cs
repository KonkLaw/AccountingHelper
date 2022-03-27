namespace AccountingHelper.Logic;

public record Record(
    DateTime TransactionDateTime,
    string OperationName,
    decimal Amount,
    string Curency,
    DateTime AccountData,
    float MoneyBack,
    float AccountAmount,
    string Category);

public record AccountDescription(string Name, string Currency);

public readonly record struct RecordGroup(
    string Name,
    IReadOnlyList<Record> Records);

public record class AccountFile(
    AccountDescription Description,
    IReadOnlyList<RecordGroup> RecordGroups);

// =========== Selection

public class RecordSelectionGroup
{
    public readonly RecordGroup Group;
    public Record LastSelected { get; private set; }

    public RecordSelectionGroup(RecordGroup group)
    {
        Group = group;
        LastSelected = group.Records[group.Records.Count - 1];
    }

    public void SetLastSelected(Record lastSelected)
    {
        LastSelected = lastSelected;
    }
}