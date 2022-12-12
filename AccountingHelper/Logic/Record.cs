namespace AccountingHelper.Logic;



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