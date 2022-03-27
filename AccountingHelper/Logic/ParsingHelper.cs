using System.Globalization;

namespace AccountingHelper.Logic;

static class ParsingHelper
{
    private static readonly NumberFormatInfo NumberFormat;

    private const string Separator = ";";
    private const string GroupBegin = "Операции по";
    private const string GroupEnd = "Всего по контракту";
    private const string CurrencyPefix = "Валюта счета: ";

    static ParsingHelper()
    {
        NumberFormat = (NumberFormatInfo)CultureInfo.InstalledUICulture.NumberFormat.Clone();
        NumberFormat.NumberDecimalSeparator = ",";
        NumberFormat.NumberGroupSeparator = " ";
    }

    static int? FindNextLine(List<string> lines, int startIndex, string lineBeginString)
    {
        for (int lineIndex = startIndex; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            if (line.StartsWith(lineBeginString))
            {
                return lineIndex;
            }
        }
        return null;
    }

    static Record? TryParseLine(string line)
    {
        string[] parts = line.Split(Separator);

        if (line.StartsWith(GroupEnd))
            return null;

        return new Record(
            DateTime.ParseExact(parts[0], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            parts[1],
            decimal.Parse(parts[2], NumberFormat),
            parts[3],
            DateTime.ParseExact(parts[4], "dd.MM.yyyy", CultureInfo.InvariantCulture),
            float.Parse(parts[5], NumberFormat),
            float.Parse(parts[6], NumberFormat),
            parts[8]);
    }

    public static List<RecordGroup> ParseLines(List<string> lines, out string currency)
    {
        int lineIndex = 0;
        List<RecordGroup> groups = new();
        int? curencyIndex = FindNextLine(lines, lineIndex, CurrencyPefix);
        if (curencyIndex == null)
            throw new Exception("Can't parse.");
        currency = lines[curencyIndex.Value].Split(Separator)[1];
        while (true)
        {
            int? index = FindNextLine(lines, lineIndex, GroupBegin);
            if (index.HasValue)
            {
                lineIndex = index.Value;
                string groupName = lines[lineIndex];
                lineIndex += 2;
                List<Record> groupRecords = new();
                while (true)
                {
                    Record? recordOrNot = TryParseLine(lines[lineIndex]);
                    lineIndex++;
                    if (recordOrNot != null)
                        groupRecords.Add(recordOrNot);
                    else
                        break;
                }
                groups.Add(new RecordGroup(groupName, groupRecords));
            }
            else
            {
                break;
            }
        }
        return groups;
    }
}

class Storage
{
    public List<AccountFile> list;
}