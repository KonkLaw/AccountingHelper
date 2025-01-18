using System.Globalization;
using System.IO;
using AccountHelperWpf.Models;

namespace AccountHelperWpf.Parsing.Pko;

static class PkoParser
{
    public const string BankId = "pkopl";

    public static IReadOnlyList<PkoOperation>? TryParseFile(StreamReader reader, out bool withSaldo)
    {
        string firstLine = reader.ReadLine()!;

        const string knownStartWithSaldo = "\"Data operacji\",\"Data waluty\",\"Typ transakcji\",\"Kwota\",\"Waluta\",\"Saldo po transakcji\",\"Opis transakcji\"";
        const string knownStartWithoutSaldo = "\"Data operacji\",\"Data waluty\",\"Typ transakcji\",\"Kwota\",\"Waluta\",\"Opis transakcji\"";
        if (firstLine.AsSpan().StartsWith(knownStartWithSaldo))
            withSaldo = true;
        else if (firstLine.AsSpan().StartsWith(knownStartWithoutSaldo))
            withSaldo = false;
        else
        {
            withSaldo = false;
            return null;
        }

        List<PkoOperation> operations = [];
        do
        {
            string line = reader.ReadLine()!;
            try
            {
                operations.Add(ParseRecord(line, withSaldo));
            }
            catch (Exception ex)
            {
                var exception = new Exception($"Problem with following line {line}", ex);
                throw exception;
            }
        } while (!reader.EndOfStream);
        return operations;
    }

    private static PkoOperation ParseRecord(string record, bool withSaldo)
    {
        RecordIterator iterator = new RecordIterator(record);

        DateOnly dateAccounting = DateOnly.Parse(iterator.GetNextSpan());
        DateTime dateOperation = DateTime.ParseExact(iterator.GetNextSpan(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        string operationType = iterator.GetNextSpan().ToString();
        decimal amount = decimal.Parse(iterator.GetNextSpan());
        string currency = iterator.GetNextSpan().ToString();
        decimal saldoBeforeTransaction = withSaldo ? decimal.Parse(iterator.GetNextSpan()) : 0;

        new PkoDescriptionParser(iterator).Parse(
            out SortedDictionary<string, string> main,
            out SortedDictionary<string, string> other,
            out string? originalAmount);

        return new PkoOperation(
            dateOperation, amount, OperationDescription.Create(BankId, main), 
            dateAccounting, currency, operationType, originalAmount,
            saldoBeforeTransaction, other);
    }
}