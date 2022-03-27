using AccountingHelper.Logic;

namespace AccountingHelper.ViewModels;

class AccountPageVM : IAccountPageVM
{
    public string Currency { get; }
    public string FileName { get; }
    public IReadOnlyList<RecordGroup> RecordGroups { get; }

    public string[] Tests { get; }
    public string TestBody { get; set; }

    public AccountPageVM()
    {
        RecordGroups = new  List<RecordGroup>();
        Currency = string.Empty;
        FileName = string.Empty;

        Tests = new string[]
        {
            "Op1",
            "Op2",
            "Op3",
        };
        TestBody = string.Empty;
    }

    public AccountPageVM(AccountFile fileInfo)
    {
        RecordGroups = fileInfo.RecordGroups;
        Currency = fileInfo.Description.Currency;
        FileName = fileInfo.Description.Name;

        Tests = new string[]
        {
            "Op1",
            "Op2",
            "Op3",
        };
        TestBody = string.Empty;
    }

    public void OnClick()
    {
        TestBody = Report();
    }

    string Report()
    {
        var map = new Dictionary<string, decimal>();
        foreach (string record in Tests)
        {
            map.Add(record, 0);
        }

        foreach (RecordGroup recordGroup in RecordGroups)
        {
            foreach (Record rec in recordGroup.Records)
            {
                //if (rec.Test == null)
                //    rec.Test = Tests[0];
                //map[rec.Test] += rec.Amount;
            }
        }

        string asd = string.Empty;
        foreach (var mm in map)
        {
            asd += mm.Key + "  " + mm.Value + " ||| ";
        }
        return asd;
    }
}

public interface IAccountPageVM
{
    string Currency { get; }
    string FileName { get; }
    IReadOnlyList<RecordGroup> RecordGroups { get; }

    string[] Tests { get; }
    string TestBody { get; set; }

    void OnClick();
}