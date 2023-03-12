using System.Globalization;
using System.IO;
using System.Text;

namespace AccountHelperWpf.Parsing;

static class PkoParser
{
    public static OperationsGroup? TryParse(StreamReader reader)
    {
        List<BaseOperation> operations = new();

        string firstLine = reader.ReadLine()!;
        const string knownFirstString = "\"Data operacji\",\"Data waluty\",\"Typ transakcji\",\"Kwota\",\"Waluta\",\"Saldo po transakcji\",\"Opis transakcji\",";
        if (!firstLine.AsSpan(0, knownFirstString.Length).SequenceEqual(knownFirstString))
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
        string description = lines[7];

        StringBuilder otherDescription = new(lines[6]);
        for (int i = 8; i < lines.Length; i++)
        {
            otherDescription.Append(" ; ");
            otherDescription.Append(lines[i]);
        }

        return new PkoOperation(
            dateOperation, amount, description, dateAccounting, currency, operationType, saldoBeforeTransaction, otherDescription.ToString());
    }

    public static void TryParseBlocked(string textToParse, out OperationsGroup? operationsGroup, out string? errorMessage)
        => PkoBlockedParser.TryParse(textToParse, out operationsGroup, out errorMessage);

    class PkoBlockedParser
    {
        private static readonly string BigLineSeparator = Environment.NewLine + '\t' + Environment.NewLine;
        private static readonly string[] Days = Enum.GetNames(typeof(DayOfWeek));
        private readonly string text;
        private int index;

        private PkoBlockedParser(string text) => this.text = text;

        public static void TryParse(string textToParse, out OperationsGroup? result, out string? errorMessage)
        {
            List<BaseOperation> operations = new PkoBlockedParser(textToParse).TryParse(out errorMessage);
            if (operations.Count == 0)
                result = null;
            else
                result = new OperationsGroup("Blocked operations", operations);
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

        private List<BaseOperation> TryParse(out string? errorMessage)
        {
            List<BaseOperation> operations = new ();

            try
            {
                errorMessage = null;

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

                        if (index < 0)
                            return operations;

                        if (!text.AsSpan(index, BigLineSeparator.Length).Equals(BigLineSeparator.AsSpan(),
                                StringComparison.InvariantCulture))
                            break;
                    }
                }
            }
            catch
            {
                errorMessage = "Some exception during parsing. Result may be not full.";
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

            int endIndex = text.IndexOf(BigLineSeparator, index, StringComparison.InvariantCulture);
            string description = text.Substring(index, endIndex - index);
            index = endIndex;
            index += BigLineSeparator.Length;

            endIndex = text.IndexOf(BigLineSeparator, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> additionalDescription1 = text.AsSpan(index, endIndex - index);
            index = endIndex;
            index += BigLineSeparator.Length;

            endIndex = text.IndexOf(BigLineSeparator, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> additionalDescription2 = text.AsSpan(index, endIndex - index);
            index = endIndex;
            index += BigLineSeparator.Length;

            endIndex = text.IndexOf(' ', index);
            decimal amount = decimal.Parse(text.AsSpan(index, endIndex - index), NumberFormatHelper.NumberFormat);
            index = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);

            
            return new PkoBlockedOperation(dateTime, amount, description,
                string.Concat(
                    additionalDescription1.ToString().Replace(Environment.NewLine, " "),
                    " ",
                    additionalDescription2.ToString().Replace(Environment.NewLine, " ")
                    ));
        }
    }
}