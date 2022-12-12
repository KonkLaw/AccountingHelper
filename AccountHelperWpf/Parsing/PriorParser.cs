using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AccountHelperWpf.Parsing;

static class PriorParser
{
    private const string ColumnSeparator = ";";

    public static AccountFile ParseFile(string filePath)
    {
        var lines = new List<string>();
        using (Stream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
        using (StreamReader reader = new (fs, EncodingHelper.RusEncoding))
        {
            while (true)
            {
                string? line = reader.ReadLine();
                if (line == null)
                    break;
                lines.Add(line);
            }
        }
        List<OperationsGroup> records = ParseLines(lines, out string currency);
        return new AccountFile(new AccountDescription(Path.GetFileName(filePath), currency), records);
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
            List<Operation> operationsGroup = new();
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

    private static Operation ParseOperationLine(string line)
    {
        string[] parts = line.Split(ColumnSeparator);
        return new Operation(
            DateTime.ParseExact(parts[0], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            parts[1],
            decimal.Parse(parts[2], NumberFormatHelper.NumberFormat),
            parts[3],
            DateTime.ParseExact(parts[4], "dd.MM.yyyy", CultureInfo.InvariantCulture),
            float.Parse(parts[5], NumberFormatHelper.NumberFormat),
            float.Parse(parts[6], NumberFormatHelper.NumberFormat),
            parts[8]);
    }
}

file static class EncodingHelper
{
    public static readonly Encoding RusEncoding;

    static EncodingHelper()
    {
        // Required to for loading of Russian encoding.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        RusEncoding = Encoding.GetEncoding("windows-1251");
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