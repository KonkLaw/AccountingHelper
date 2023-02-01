using System.Globalization;
using System.IO;

namespace AccountHelperWpf.Parsing;

static class PkoParser
{
    public static AccountFile? TryParse(StreamReader reader, string name)
    {
        List<BaseOperation> operations = new();

        string firstLine = reader.ReadLine()!;
        const string knownFirstString = "\"Data operacji\",\"Data waluty\",\"Typ transakcji\",\"Kwota\",\"Waluta\",\"Saldo po transakcji\",\"Opis transakcji\",\"\",\"\",\"\",\"\"";
        if (firstLine != knownFirstString)
            return null;

        do
        {
            string line = reader.ReadLine()!;
            operations.Add(ParseString(line));
        } while (!reader.EndOfStream);

        return new AccountFile(new AccountDescription(name, "zl"), new List<OperationsGroup>
        {
            new OperationsGroup("zl", operations)
        });
    }

    private static PkoOperation ParseString(string record)
    {
        string[] lines = record.Replace("\"", "").Split(",");

        DateOnly dateAccounting = DateOnly.Parse(lines[0]);
        DateTime dateOperation = DateTime.ParseExact(lines[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
        string operationType = lines[2];
        decimal amount = decimal.Parse(lines[3]);
        string currency = lines[4];
        decimal saldoBeforeTransaction = decimal.Parse(lines[5]);
        string otherDescription = string.Concat(lines[6], " ; ", lines[8], " ; ", lines[9], " ; ", lines[10]);
        string description = lines[7];

        return new PkoOperation(
            dateOperation, amount, description, dateAccounting, currency, operationType, saldoBeforeTransaction, otherDescription);
    }
}