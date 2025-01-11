using System.Text.RegularExpressions;
using AccountHelperWpf.Models;
using AccountHelperWpf.Utils;

namespace AccountHelperWpf.Parsing.Pko;

struct DescriptionParser
{
    private const string TagSeparator = ":";

    private const string TitleTagName = "Tytuł";
    private const string OriginalAmountTagName = "Oryginalna kwota operacji";
    private const string LocationTagName = "Lokalizacja";
    private const string NazwaOdbiorcyTagName = "Nazwa odbiorcy";
    private const string NazwaNadawcyTagName = "Nazwa nadawcy";

    private RecordIterator iterator;
    private readonly DescriptionParserCache cache;

    public DescriptionParser(RecordIterator iterator, DescriptionParserCache cache)
    {
        this.iterator = iterator;
        this.cache = cache;
    }

    private void GetTagAndContent(
        ReadOnlySpan<char> span, out ReadOnlySpan<char> tagName, out ReadOnlySpan<char> tagContent)
    {
        int indexOfTag = span.IndexOf(TagSeparator);
        if (indexOfTag < 0)
        {
            tagName = default;
            tagContent = span;
        }
        else
        {
            const char beginEndSymbol = ' ';
            tagName = span.Slice(0, indexOfTag).Trim(beginEndSymbol);
            tagContent = span[(indexOfTag + TagSeparator.Length)..];
        }
    }

    public void Parse(out KeyValue[] main, out KeyValue[] other, out string? originalAmount)
    {
        originalAmount = null;
        cache.Main.Clear();
        cache.Other.Clear();

        while (iterator.TryGetNextSpan(out ReadOnlySpan<char> span))
        {
            if (span.Length == 0) // there are empty spans: ""
                continue;

            GetTagAndContent(span, out ReadOnlySpan<char> tagName, out ReadOnlySpan<char> tagContent);

            if (tagName == OriginalAmountTagName)
            {
                originalAmount = tagContent.ToString();
                continue;
            }

            bool addMain = false;
            switch (tagName)
            {
                case TitleTagName:
                {
                    const string pattern = @"\s*(?<name>.*?)\s*OD:\s*(?<od>.*?)\s*DO:\s*(?<do>.*?)$";

                    Regex.ValueMatchEnumerator enumerator = Regex.EnumerateMatches(tagContent, pattern);
                    if (enumerator.MoveNext())
                    {
                        addMain = true;
                    }
                    else
                    {
                        int digitCount = tagContent.GetDigitsCount();
                        const float acceptablePercent = 0.3f;
                        bool tooMuchDigits = digitCount / (float)tagContent.Length > acceptablePercent;
                        addMain = !tooMuchDigits;
                    }

                    break;
                }
                case NazwaOdbiorcyTagName:
                case NazwaNadawcyTagName:
                case LocationTagName:
                    addMain = true;
                    break;
            }
            string tagNameString = tagName.ToString();
            string tagContentString = tagContent.ToString();
            cache.Other.Add(tagNameString, tagContentString);
            if (addMain)
                cache.Main.Add(tagNameString, tagContentString);
        }
        main = cache.Main.ToArray();
        other = cache.Other.ToArray();
    }
}

class DescriptionParserCache
{
    public readonly SortedDictionary<string, string> Main = new();
    public readonly SortedDictionary<string, string> Other = new();
}

static class ParsingHelpers
{
    public static int GetDigitsCount(this ReadOnlySpan<char> span)
    {
        int digitCount = 0;
        foreach (char c in span)
        {
            if (char.IsDigit(c))
                digitCount++;
        }
        return digitCount;
    }
}