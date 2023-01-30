using System.Globalization;
using System.IO;

namespace AccountHelperWpf.Parsing;

static class PriorParser
{
    private const string ColumnSeparator = ";";

    public static AccountFile? TryParse(StreamReader reader, string name)
    {
        string firstLine = reader.ReadLine()!;
        if (firstLine != "Выписка по контракту")
            return null;

        var lines = new List<string>();
        while (true)
        {
            string? line = reader.ReadLine();
            if (line == null)
                break;
            lines.Add(line);
        }

        List<OperationsGroup> records = ParseLines(lines, out string currency);
        return new AccountFile(new AccountDescription(name, currency), records);
    }

    private static List<OperationsGroup> ParseLines(List<string> lines, out string currency)
    {
        const string currencyLinePrefix = "Валюта счета: ";
        const string operationsGroupBegin = "Операции по";
        const string operationsGroupEnd = "Всего по контракту";

        int lineIndex = 0;
        List<OperationsGroup> groups = new();
        int? currencyIndex = FindNextLine(lines, lineIndex, currencyLinePrefix);
        if (currencyIndex == null)
            throw new ParsingException();

        currency = lines[currencyIndex.Value].Split(ColumnSeparator)[1];
        while (true)
        {
            int? groupBeginIndex = FindNextLine(lines, lineIndex, operationsGroupBegin);
            if (!groupBeginIndex.HasValue)
                break;

            lineIndex = groupBeginIndex.Value;
            string groupName = lines[lineIndex];
            lineIndex++; // skip one line with header
            List<BaseOperation> operationsGroup = new();
            while (true)
            {
                lineIndex++;
                string line = lines[lineIndex];
                if (line.StartsWith(operationsGroupEnd))
                    break;
                operationsGroup.Add(ParseOperationLine(line));
            }
            groups.Add(new OperationsGroup(groupName, operationsGroup));
        }
        return groups;
    }

    private static int? FindNextLine(List<string> lines, int startSearchLineIndex, string lineBeginString)
    {
        for (int lineIndex = startSearchLineIndex; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            if (line.StartsWith(lineBeginString))
            {
                return lineIndex;
            }
        }
        return null;
    }

    private static PriorOperation ParseOperationLine(string line)
    {
        string[] parts = line.Split(ColumnSeparator);
        return new PriorOperation(
            DateTime.ParseExact(parts[0], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            decimal.Parse(parts[6], NumberFormatHelper.NumberFormat),
            parts[1],
            parts[8],
            parts[3],
            decimal.Parse(parts[5], NumberFormatHelper.NumberFormat),
            decimal.Parse(parts[2], NumberFormatHelper.NumberFormat),
            DateOnly.ParseExact(parts[4], "dd.MM.yyyy", CultureInfo.InvariantCulture));
    }
}

file class NumberFormatHelper
{
    public static readonly NumberFormatInfo NumberFormat;

    static NumberFormatHelper()
    {
        NumberFormat = (NumberFormatInfo)CultureInfo.InstalledUICulture.NumberFormat.Clone();
        NumberFormat.NumberDecimalSeparator = ",";
        NumberFormat.NumberGroupSeparator = " ";
    }
}

class ParsingException : Exception { }