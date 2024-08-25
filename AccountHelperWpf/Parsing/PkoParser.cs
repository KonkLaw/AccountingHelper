using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

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

    class DelegateComparer<TKey> : IComparer<TKey>
    {
        private readonly Func<TKey?, TKey?, int> function;

        public DelegateComparer(Func<TKey?, TKey?, int> function) => this.function = function;

        public int Compare(TKey? x, TKey? y) => function(x, y);
    }
}

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
		if (indexOfTag < 0)
		{
			tagName = default;
            content = span;
		}
		else
		{
			const char beginEndSymbol = ' ';
			tagName = span.Slice(0, indexOfTag).Trim(beginEndSymbol);
			content = span.Slice(indexOfTag + TagSeparator.Length, span.Length - indexOfTag - TagSeparator.Length).Trim(beginEndSymbol);
		}
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

			if (tagName == default)
			{
				descriptionResult.AddToDescription(TitleTagName, tagContent);
                continue;
			}

			if (tagName.SequenceEqual(OriginalAmountTagName))
			{
				originalAmount = tagContent.ToString();
				continue;
			}

			// Trying to clarify: what kind of Title is that.
			
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
					continue;
				}
			}

			bool wasAdded = false;
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

public struct RecordIterator
{
	private readonly string record;
	private int index;

	public RecordIterator(string record)
	{
		this.record = record;
    }

	public bool TryGetNextSpan(out ReadOnlySpan<char> span)
	{
        const string separatorSet = "\",\"";
        int endIndex = record.Length - 1;

        if (index >= endIndex)
        {
            span = default;
            return false;
        }
        int textStartIndex = index == 0 ? 1 : index + separatorSet.Length;

        int textStopIndex = record.IndexOf(separatorSet, textStartIndex, StringComparison.InvariantCulture);
        if (textStopIndex < 0)
            textStopIndex = endIndex;

        span = record.AsSpan(textStartIndex, textStopIndex - textStartIndex);
        index = textStopIndex;
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

	public void AddToDescription(ReadOnlySpan<char> tagContent)
		=> cacheForMain.Append($"{tagContent}; ");

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