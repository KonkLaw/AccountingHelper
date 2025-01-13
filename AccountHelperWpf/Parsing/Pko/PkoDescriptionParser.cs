using System.Text.RegularExpressions;
using System.Windows;

namespace AccountHelperWpf.Parsing.Pko;

struct PkoDescriptionParser
{
    private const string TagSeparator = ":";

    private const string OriginalAmountTagName = "Oryginalna kwota operacji";

    public const string TitleTagName = "Tytuł";
    public const string LocationTagName = "Lokalizacja";
    public const string RecipientTagName = "Nazwa odbiorcy";
    public const string SenderTagName = "Nazwa nadawcy";

    private RecordIterator iterator;

    public PkoDescriptionParser(RecordIterator iterator)
    {
        this.iterator = iterator;
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
            tagContent = span[(indexOfTag + TagSeparator.Length)..].Trim(beginEndSymbol);
        }
    }

    public void Parse(
        out SortedDictionary<string, string> main,
        out SortedDictionary<string, string> other,
        out string? originalAmount)
    {
        originalAmount = null;
        main = new SortedDictionary<string, string>();
        other = new SortedDictionary<string, string>();

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
                    if (TitleParsingHelper.IsFromToTitleType(tagContent))
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
                case RecipientTagName:
                case SenderTagName:
                case LocationTagName:
                    addMain = true;
                    break;
            }

            string tagNameString = tagName.ToString();
            string tagContentString = tagContent.ToString();
            other.Add(tagNameString, tagContentString);
            if (addMain)
                main.Add(tagNameString, tagContentString);
        }
    }
}

static class TitleParsingHelper
{
    private const string TitleTransferPattern = @"\s*(?<name>.*?)\s*OD:\s*(?<od>.*?)\s*DO:\s*(?<do>.*?)$";

    public static bool IsFromToTitleType(ReadOnlySpan<char> tagContent)
    {
        Regex.ValueMatchEnumerator enumerator = Regex.EnumerateMatches(tagContent, TitleTransferPattern);
        bool matchPatter = enumerator.MoveNext();
        return matchPatter;
    }

    public static string CorrectIfFromToTitleType(string tagContent)
    {
        Match match = Regex.Match(tagContent, TitleTransferPattern);
        if (match.Success)
        {
            string name = match.Groups["name"].Value;
            return name;
        }
        else
        {
            return tagContent;
        }
    }
}

static class LocationParsingHelper
{
    public static void Parse(string tagContent, out string address, out (string city, string country)? info)
    {
        const string begining = "Adres: ";
        if (tagContent.StartsWith(begining))
        {
            const string pattern = @"Adres:\s*(?<Adres>.*?)\s*Miasto:\s*(?<Miasto>.*?)\s*Kraj:\s*(?<Kraj>.*?)$";
            Match match = Regex.Match(tagContent, pattern);

            if (match.Success)
            {
                address = match.Groups["Adres"].Value;
                info = (match.Groups["Miasto"].Value, match.Groups["Kraj"].Value);
            }
            else
            {
                address = tagContent[begining.Length..];
                info = null;
            }
        }
        else
        {
            MessageBox.Show(tagContent, "Doesn't match typical rule");
            address = tagContent;
            info = null;
        }
    }
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