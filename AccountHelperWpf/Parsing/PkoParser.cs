using System.Globalization;
using System.IO;
using System.Text;

namespace AccountHelperWpf.Parsing;

static class PkoParser
{
    public static IReadOnlyList<PkoOperation>? TryParse(StreamReader reader)
    {
        List<PkoOperation> operations = new();
        string firstLine = reader.ReadLine()!;
        const string knownFirstString = "\"Data operacji\",\"Data waluty\",\"Typ transakcji\",\"Kwota\",\"Waluta\",\"Saldo po transakcji\",\"Opis transakcji\"";
        if (!firstLine.AsSpan(0, knownFirstString.Length).SequenceEqual(knownFirstString))
            return null;
        do
        {
            string line = reader.ReadLine()!;
            operations.Add(ParseString(line));
        } while (!reader.EndOfStream);

        List<(int orderId, PkoOperation operation)> list = operations.Select((operation, index) => (index, operation)).ToList();
        var comparer = new DelegateComparer<(int, PkoOperation)>((i1, i2) =>
        {
            int result = Comparer<DateTime>.Default.Compare(i2.Item2.TransactionDateTime, i1.Item2.TransactionDateTime);
            return result == 0 ? Comparer<int>.Default.Compare(i1.Item1, i2.Item1) : result;
        });
        list.Sort(comparer);
        operations.Clear();
        foreach ((_, PkoOperation operation) in list)
            operations.Add(operation);

        return operations;
    }

    private static PkoOperation ParseString(string record)
    {
        RecordIterator iterator = new RecordIterator(record);

        DateOnly dateAccounting = DateOnly.Parse(iterator.GetNextSpan());
        DateTime dateOperation = DateTime.ParseExact(iterator.GetNextSpan(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        string operationType = iterator.GetNextSpan().ToString();
        decimal amount = decimal.Parse(iterator.GetNextSpan());
        string currency = iterator.GetNextSpan().ToString();
        decimal saldoBeforeTransaction = decimal.Parse(iterator.GetNextSpan());
		
        new DescriptionParser(iterator).Parse(out string? originalAmount, out string shortDescription, out string otherDetails);
		
        return new PkoOperation(
            dateOperation, amount, shortDescription,
            dateAccounting, currency, operationType, originalAmount,
            saldoBeforeTransaction, otherDetails);
    }

    public static void TryParseBlocked(string textToParse, out IReadOnlyList<PkoBlockedOperation>? blockedOperations, out string? errorMessage)
        => PkoBlockedParser.TryParse(textToParse, out blockedOperations, out errorMessage);

    class PkoBlockedParser
    {
        private static readonly string BigLineSeparator = Environment.NewLine + '\t' + Environment.NewLine;
        private static readonly string[] Days = Enum.GetNames(typeof(DayOfWeek));
        private readonly string text;
        private int index;

        private PkoBlockedParser(string text) => this.text = text;

        public static void TryParse(string textToParse, out IReadOnlyList<PkoBlockedOperation>? result, out string? errorMessage)
        {
            List<PkoBlockedOperation> operations = new PkoBlockedParser(textToParse).TryParse(out errorMessage);
            result = operations.Count == 0 ? null : operations;
        }

        private bool TryMoveToNewDay(out int currentDayIndex)
        {
            currentDayIndex = 0;
            int startIndex = int.MaxValue;
            for (int i = 0; i < Days.Length; i++)
            {
                int newIndex = text.IndexOf(Days[i], index, StringComparison.InvariantCulture);
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

        private List<PkoBlockedOperation> TryParse(out string? errorMessage)
        {
            List<PkoBlockedOperation> operations = new ();

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
            string description = text[index..endIndex];
            index = endIndex + BigLineSeparator.Length;

            endIndex = text.IndexOf(BigLineSeparator, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> additionalDescription1 = text.AsSpan(index, endIndex - index);
            index = endIndex + BigLineSeparator.Length;

            endIndex = text.IndexOf(BigLineSeparator, index, StringComparison.InvariantCulture);
            ReadOnlySpan<char> additionalDescription2 = text.AsSpan(index, endIndex - index);
            index = endIndex;
            index += BigLineSeparator.Length;


            int amountStart = index;
            do
            {
                endIndex = text.IndexOf(' ', index);
                if (char.IsDigit(text[endIndex + 1])) // not end of amount number
                {
                    index = endIndex + 1; // continue looking for end of amount number
                    continue;
                }
                break;
            } while (true);
            decimal amount = decimal.Parse(text.AsSpan(amountStart, endIndex - amountStart), NumberFormatHelper.NumberFormat);

            int indexOfCurrencyEnd = text.IndexOf(Environment.NewLine, endIndex, StringComparison.InvariantCulture);
            if (indexOfCurrencyEnd < 0)
                indexOfCurrencyEnd = text.Length;
            string currency = text.Substring(endIndex + 1, indexOfCurrencyEnd - (endIndex + 1));

            index = text.IndexOf(Environment.NewLine, index, StringComparison.InvariantCulture);

            return new PkoBlockedOperation(dateTime, amount, description,
                currency,
                string.Concat(
                    additionalDescription1.ToString().Replace(Environment.NewLine, " "),
                    " ",
                    additionalDescription2.ToString().Replace(Environment.NewLine, " ")
                    ));
        }
    }

    class DelegateComparer<TKey> : IComparer<TKey>
    {
        private readonly Func<TKey?, TKey?, int> function;

        public DelegateComparer(Func<TKey?, TKey?, int> function) => this.function = function;

        public int Compare(TKey? x, TKey? y) => function(x, y);
    }
}

class DescriptionParser
{
	private const string TagSeparator = ":";
	private const string OriginalAmountTagName = "Oryginalna kwota operacji";
	private const string TitleTagName = "Tytuł";
	private static readonly HashSet<string> TagsForShortDescription = new()
	{
		"Lokalizacja",
		"Nazwa odbiorcy",
		"Nazwa nadawcy"
	};

	private readonly StringBuilder cacheForMain = new();
	private readonly StringBuilder cacheForOther = new();
	private RecordIterator iterator;

	public DescriptionParser(RecordIterator iterator) => this.iterator = iterator;

	private void GetTagAndContent(ReadOnlySpan<char> span, out ReadOnlySpan<char> tagName, out ReadOnlySpan<char> content)
	{
		int indexOfTag = span.IndexOf(TagSeparator);
		tagName = span.Slice(0, indexOfTag - 1);
		content = span.Slice(indexOfTag + 2, span.Length - indexOfTag - 2);
	}

	public void Parse(out string? originalAmount, out string shortDescription, out string otherDetails)
	{
		var descriptionResult = new DescriptionResult(cacheForMain, cacheForOther);
		originalAmount = null;

		while (iterator.TryGetNextSpan(out ReadOnlySpan<char> span))
		{
            if (span.Length == 0)
                continue;

			GetTagAndContent(span, out ReadOnlySpan<char> tagName, out ReadOnlySpan<char> tagContent);
			if (tagName.SequenceEqual(OriginalAmountTagName))
			{
				originalAmount = tagContent.ToString();
				continue;
			}

			// Trying to clarify: what kind of Title is that.
			bool wasAdded = false;
			if (tagName.SequenceEqual(TitleTagName))
			{
				int digitCount = 0;
				foreach (char c in tagContent)
				{
					if (char.IsDigit(c))
						digitCount++;
				}

				const float acceptablePercent = 0.3f;
				if (digitCount / (float)tagContent.Length < acceptablePercent)
				{
					descriptionResult.AddToDescription(TitleTagName, tagContent);
					wasAdded = true;
				}
			}

			foreach (string tag in TagsForShortDescription)
			{
				if (tagName.SequenceEqual(tag))
				{
					descriptionResult.AddToDescription(tag, tagContent);
					wasAdded = true;
					break;
				}
			}
			if (!wasAdded)
				descriptionResult.AddToOtherDetails(tagName, tagContent);
		}
		descriptionResult.TripEnd();
		shortDescription = cacheForMain.ToString();
		otherDetails = cacheForOther.ToString();
	}
}

struct RecordIterator
{
	private readonly string record;
	private int index;

	public RecordIterator(string record)
	{
		this.record = record;
		index = -1;
	}

	public bool TryGetNextSpan(out ReadOnlySpan<char> span)
	{
		const char bracketsSymbol = '"';
		int startIndex = record.IndexOf(bracketsSymbol, index + 1);
		if (startIndex < 0)
		{
			span = default;
			return false;
		}

		int stopIndex = record.IndexOf(bracketsSymbol, startIndex + 1);
		span = record.AsSpan(startIndex + 1, stopIndex - startIndex - 1);
		index = stopIndex;
		return true;
	}

	public ReadOnlySpan<char> GetNextSpan()
	{
		TryGetNextSpan(out ReadOnlySpan<char> span);
		return span;
	}
}

readonly ref struct DescriptionResult
{
	private readonly StringBuilder cacheForMain;
	private readonly StringBuilder cacheForOther;

	public DescriptionResult(StringBuilder cacheForMain, StringBuilder cacheForOther)
	{
		this.cacheForMain = cacheForMain;
		this.cacheForOther = cacheForOther;
		cacheForMain.Clear();
		cacheForOther.Clear();
	}

	public void AddToDescription(string tagName, ReadOnlySpan<char> tagContent)
		=> cacheForMain.Append($"{tagName}: {tagContent}; ");

	public void AddToOtherDetails(ReadOnlySpan<char> tagName, ReadOnlySpan<char> tagContent)
		=> cacheForOther.Append($"{tagName}: {tagContent} || ");

	public void TripEnd()
	{
		if (cacheForMain.Length != 0)
			cacheForMain.Remove(cacheForMain.Length - 2, 2);
		if (cacheForOther.Length != 0)
			cacheForOther.Remove(cacheForOther.Length - 4, 4);
	}
}