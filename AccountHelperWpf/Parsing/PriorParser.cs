using System.Globalization;
using System.IO;

namespace AccountHelperWpf.Parsing;

static class PriorParser
{
    public const string BankId = "priorby";

    private const string ColumnSeparator = ";";

    public static PriorFile? TryParse(StreamReader reader, string name)
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

        var file = ParseLines(lines, name);
        return file;
    }

    private static PriorFile ParseLines(List<string> lines, string fileName)
    {
        const string currencyLinePrefix = "Валюта счета: ";
        const string operationsGroupBegin = "Операции по";
        const string operationsGroupEnd = "Всего по контракту";
        const string operationsBlockedBegin = "Заблокированные суммы по .....";

        int lineIndex = 0;
        List<PriorOperationsGroup> nonBlockedOperationsGroups = new();
        List<PriorBlockedOperationsGroup> blockedOperationsGroups = new();
        int currencyIndex = FindNextLine(lines, lineIndex, currencyLinePrefix) ?? throw new ParsingException();
        string currency = lines[currencyIndex].Split(ColumnSeparator)[1];
        while (true)
        {
            int? groupBeginIndex = FindNextLine(lines, lineIndex, operationsGroupBegin);
            if (!groupBeginIndex.HasValue)
                break;

            lineIndex = groupBeginIndex.Value;
            string groupName = lines[lineIndex];
            lineIndex++; // skip one line
            List<PriorOperation> operationsGroup = new();
            while (true)
            {
                lineIndex++;
                string line = lines[lineIndex];
                if (line.StartsWith(operationsGroupEnd))
                    break;
                operationsGroup.Add(ParseOperationLine(line));
            }
            nonBlockedOperationsGroups.Add(new PriorOperationsGroup(groupName, operationsGroup));
        }

        int? blockedIndex = FindNextLine(lines, lineIndex, operationsBlockedBegin);
        if (blockedIndex != null)
        {
            lineIndex = blockedIndex.Value;
            string groupName = lines[lineIndex];
            lineIndex++; // skip one line
            List<PriorBlockedOperation> operationsGroup = new();
            while (true)
            {
                lineIndex++;
                if (lineIndex >= lines.Count)
                    break;
                string line = lines[lineIndex];
                operationsGroup.Add(ParseBlockedOperationLine(line));
            }
            blockedOperationsGroups.Add(new PriorBlockedOperationsGroup(groupName,operationsGroup));
        }

        var file = new PriorFile(fileName, currency, nonBlockedOperationsGroups, blockedOperationsGroups);
        return file;
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
        DateTime transactionDateTime = DateTime.ParseExact(parts[0], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        string description = parts[1];
        decimal initialAmount = decimal.Parse(parts[2], NumberFormatHelper.NumberFormat);
        string currency = parts[3];
        DateOnly accountDateTime = DateOnly.ParseExact(parts[4], "dd.MM.yyyy", CultureInfo.InvariantCulture);
        decimal fee = decimal.Parse(parts[5], NumberFormatHelper.NumberFormat);
        decimal amount = decimal.Parse(parts[6], NumberFormatHelper.NumberFormat);
        string categoryName = parts[8];

        throw new NotImplementedException();
        //return new PriorOperation(
        //    transactionDateTime, amount, description, categoryName, currency, fee, initialAmount, accountDateTime);
    }

    private static PriorBlockedOperation ParseBlockedOperationLine(string line)
    {
        string[] parts = line.Split(ColumnSeparator);

        DateTime transactionDateTime = DateTime.ParseExact(parts[0], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        string description = parts[1];
        decimal initialAmount = decimal.Parse(parts[2], NumberFormatHelper.NumberFormat);
        string initialCurrency = parts[3];
        // sign inverted !!!
        decimal amount = -decimal.Parse(parts[4], NumberFormatHelper.NumberFormat);
        string currency = parts[5];
        string categoryName = parts[7];
        throw new NotImplementedException();
        //return new PriorBlockedOperation(
        //    transactionDateTime, amount, description, categoryName, currency, initialAmount, initialCurrency);
    }
}

class ParsingException : Exception { }