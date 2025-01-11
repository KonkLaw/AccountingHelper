using System.Globalization;

namespace AccountHelperWpf.Parsing.Pko;

class PkoBlockedParser
{
    public static void TryParse(string textToParse, out IReadOnlyList<PkoBlockedOperation>? result, out string? errorMessage)
    {
        BlockedIterator iterator = new BlockedIterator(textToParse);
        List<PkoBlockedOperation> operations = new();
        try
        {
            DateTime dateTime;

            while (true)
            {
                if (iterator.IsEnded)
                {
                    errorMessage = "There were no any date in text.";
                    result = null;
                    return;
                }
                if (iterator.TryReadDate(out dateTime))
                    break;
            }

            do
            {
                iterator.SkipEmptyLines();
                iterator.ReadNewLine(out ReadOnlySpan<char> description);
                iterator.SkipEmptyLines();
                iterator.ReadNewLine(out ReadOnlySpan<char> additionalDescription1);
                iterator.ReadNewLine(out ReadOnlySpan<char> additionalDescription2);
                iterator.SkipEmptyLines();
                iterator.ReadNewLine(out ReadOnlySpan<char> details);
                iterator.SkipEmptyLines();

                if (iterator.IsEnded) // do we ended text. if so amount  is not correct
                    break;

                iterator.ReadNewLine(out ReadOnlySpan<char> amountAndCurrency);
                ReadAmountAndCurrency(amountAndCurrency, out decimal amount, out string currency);

                string otherDetails = $"{details}; {additionalDescription1} {additionalDescription2}";
                var operation = new PkoBlockedOperation(dateTime, amount, description.ToString(),
                    currency, otherDetails);
                operations.Add(operation);

                iterator.SkipEmptyLines();
                if (iterator.IsEnded) // do we ended text
                    break;
                iterator.TryReadDate(out dateTime); // trying update data. ok - if not.
            }
            while (true);
        }
        catch
        {
            errorMessage = "Some exception during parsing. Result may be not full.";
            result = operations;
            return;
        }

        errorMessage = null;
        result = operations;
    }

    private static void ReadAmountAndCurrency(ReadOnlySpan<char> line, out decimal amount, out string currency)
    {
        int index = 0;
        int endIndex;
        do
        {
            endIndex = index + line[index..].IndexOf(new ReadOnlySpan<char>(' '));
            if (!char.IsDigit(line[endIndex + 1]))
                break;
            index = endIndex + 1; // continue looking for end of amount number
        } while (true);
        amount = decimal.Parse(line[..endIndex], NumberFormatHelper.NumberFormat);
        currency = line[(endIndex + 1)..].ToString();
    }

    struct BlockedIterator
    {
        private static readonly string[] Days = Enum.GetNames(typeof(DayOfWeek));
        private static string EmptyChars = " \t";

        private readonly string text;
        private int index;

        public bool IsEnded => index == text.Length;

        public BlockedIterator(string text)
        {
            this.text = text;
            index = 0;
        }

        public bool TryReadDate(out DateTime dateTime)
        {
            ReadNewLine(out ReadOnlySpan<char> line, out _);
            for (int dayIndex = 0; dayIndex < Days.Length; dayIndex++)
            {
                string day = Days[dayIndex];
                if (line.StartsWith(day))
                {
                    ReadNewLine(out ReadOnlySpan<char> line2, out int oldIndex);
                    if (!line2.SequenceEqual(line))
                        index = oldIndex;
                    int dateIndex = Days[dayIndex].Length + 1;
                    dateTime = DateTime.ParseExact(line.Slice(dateIndex), "dd.MM.yyyy",
                        CultureInfo.InvariantCulture);
                    return true;
                }
            }
            dateTime = default;
            return false;
        }

        private void ReadNewLine(out ReadOnlySpan<char> line, out int oldIndex)
        {
            oldIndex = index;
            int endIndex = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);
            if (endIndex >= 0)
            {
                line = text.AsSpan(index, endIndex - index);
                index = endIndex + Environment.NewLine.Length;
            }
            else
            {
                line = text.AsSpan(index);
                index = text.Length;
            }
        }

        public void ReadNewLine(out ReadOnlySpan<char> line) => ReadNewLine(out line, out _);

        public void SkipEmptyLines()
        {
            while (true)
            {
                ReadNewLine(out ReadOnlySpan<char> line, out int oldIndex);
                ReadOnlySpan<char> trimmedLine = line.Trim(EmptyChars);
                if (trimmedLine.Length != 0)
                {
                    index = oldIndex;
                    break;
                }
                if (IsEnded)
                    return;
            }
        }
    }
}