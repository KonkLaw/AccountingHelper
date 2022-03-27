using AccountingHelper.Logic;

namespace AccountingHelper.ViewModels;

class RecordSelectionTabVM : IRecordSelectionTabVM
{
    public IList<RecordSelectionGroup> RecordGroups { get; }
    public string Currency { get; }
    public string FileName { get; }

    public RecordSelectionTabVM(AccountFile fileInfo)
    {
        RecordGroups = Map(fileInfo);
        Currency = fileInfo.Description.Currency;
        FileName = fileInfo.Description.Name;
    }

    private static List<RecordSelectionGroup> Map(AccountFile fileInfo)
    {
        var result = new List<RecordSelectionGroup>();
        foreach (RecordGroup group in fileInfo.RecordGroups)
        {
            result.Add(new RecordSelectionGroup(group));
        }
        return result;
    }
}

public interface IRecordSelectionTabVM
{
    IList<RecordSelectionGroup> RecordGroups { get; }
    string Currency { get; }
    string FileName { get; }
}