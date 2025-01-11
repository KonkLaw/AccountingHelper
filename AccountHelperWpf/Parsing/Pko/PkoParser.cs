using System.Globalization;
using System.IO;
using AccountHelperWpf.Models;

namespace AccountHelperWpf.Parsing.Pko;

static class PkoParser
{
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
        DescriptionParserCache cache = new();
        do
        {
            string line = reader.ReadLine()!;
            try
            {
                operations.Add(ParseRecord(line, cache, withSaldo));
            }
            catch (Exception ex)
            {
                var exception = new Exception($"Problem with following line {line}", ex);
                throw exception;
            }
        } while (!reader.EndOfStream);
        return operations;
    }

    private static PkoOperation ParseRecord(string record, DescriptionParserCache cache, bool withSaldo)
    {
        RecordIterator iterator = new RecordIterator(record);

        DateOnly dateAccounting = DateOnly.Parse(iterator.GetNextSpan());
        DateTime dateOperation = DateTime.ParseExact(iterator.GetNextSpan(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        string operationType = iterator.GetNextSpan().ToString();
        decimal amount = decimal.Parse(iterator.GetNextSpan());
        string currency = iterator.GetNextSpan().ToString();
        decimal saldoBeforeTransaction = withSaldo ? decimal.Parse(iterator.GetNextSpan()) : 0;

        new DescriptionParser(iterator, cache).Parse(
            out KeyValue[] main, out KeyValue[] other, out string? originalAmount);

        var shortDescription = string.Join(" || ", main.Select(kvp => $"{kvp.Key} : {kvp.Value}"));
        var otherDetails = string.Join(" || ", other.Select(kvp => $"{kvp.Key} : {kvp.Value}"));

        return new PkoOperation(
            dateOperation, amount, shortDescription,
            dateAccounting, currency, operationType, originalAmount,
            saldoBeforeTransaction, otherDetails);
    }
}