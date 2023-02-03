using System.Globalization;
using System.IO;

namespace AccountHelperWpf.Parsing;

static class PkoParser
{
    public static OperationsGroup? TryParse(StreamReader reader)
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

        return new OperationsGroup("Non Blocked operations", operations);
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

    public static OperationsGroup? TryParseBlocked(string? text)
        => string.IsNullOrEmpty(text) ? null : PkoBlockedParser.Parse(text);

    class PkoBlockedParser
    {
        private static readonly string BigLineSeparator = Environment.NewLine + '\t' + Environment.NewLine;
        private static readonly string EndSearchLine = BigLineSeparator + "Sum:";
        private static readonly string[] Days = Enum.GetNames(typeof(DayOfWeek));
        private readonly string text;
        private int index;

        private PkoBlockedParser(string text) => this.text = text;

        public static OperationsGroup? Parse(string text)
        {
            List<BaseOperation> operations = new PkoBlockedParser(text).Parse();
            if (operations.Count == 0)
                return null;
            return new OperationsGroup("Blocked operations", operations);
        }

        private bool TryMoveToNewDay(out int currentDayIndex)
        {
            currentDayIndex = 0;
            int startIndex = int.MaxValue;
            for (int i = 0; i < Days.Length; i++)
            {
                string day = Days[i];
                int newIndex = text.IndexOf(day, index, StringComparison.InvariantCulture);
                if (newIndex >= 0 && newIndex < startIndex)
                {
                    startIndex = newIndex;
                    currentDayIndex = i;
                }
            }

            if (startIndex == int.MaxValue)
                return false;
            index = startIndex;
            return true;
        }

        private List<BaseOperation> Parse()
        {
            List<BaseOperation> operations = new ();
            while (true)
            {
                if (!TryMoveToNewDay(out int currentDayIndex))
                    break;

                string day = Days[currentDayIndex];
                index += day.Length;
                ReadDate(out DateTime dateTime);

                while (true)
                {
                    PkoBlockedOperation operation = ReadRecord(text, ref index, dateTime);
                    operations.Add(operation);

                    if (text.AsSpan(index, EndSearchLine.Length).Equals(EndSearchLine.AsSpan(), StringComparison.InvariantCulture))
                        return operations;

                    if (!text.AsSpan(index, BigLineSeparator.Length).Equals(BigLineSeparator.AsSpan(), StringComparison.InvariantCulture))
                        break;
                }
            }

            return operations;
        }

        private void ReadDate(out DateTime dateTime)
        {
            index++;
            int endIndex = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            dateTime = DateTime.ParseExact(text.AsSpan(index, endIndex - index), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            index = endIndex;
        }

        private static PkoBlockedOperation ReadRecord(string text, ref int index, DateTime dateTime)
        {
            index += BigLineSeparator.Length;

            int endIndex = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            string description = text.Substring(index, endIndex - index);
            index = endIndex;

            index += BigLineSeparator.Length;

            endIndex = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> otherDescription1 = text.AsSpan(index, endIndex - index);
            index = endIndex + Environment.NewLine.Length;

            endIndex = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> otherDescription2 = text.AsSpan(index, endIndex - index);
            index = endIndex;

            index += BigLineSeparator.Length;

            endIndex = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> otherDescription3 = text.AsSpan(index, endIndex - index);
            index = endIndex;

            index += BigLineSeparator.Length;

            endIndex = text.IndexOf(' ', index);
            decimal amount = decimal.Parse(text.AsSpan(index, endIndex - index), NumberFormatHelper.NumberFormat);
            index = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            
            return new PkoBlockedOperation(dateTime, amount, description, string.Concat(otherDescription1.ToString(), " ", otherDescription2.ToString(), " ", otherDescription3.ToString()));
        }
    }
}