using System.Text;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Parsing.Pko;

namespace AccountHelperWpf.Models;

static class OperationDescriptionHelper
{
    delegate void GetOperationDescriptionDelegate(
        SortedDictionary<string, string> tagsToContents,
        out string displayName, out string comparisonKeyBeforeProcess);

    private static readonly Dictionary<string, GetOperationDescriptionDelegate> BankIdToConverters = new()
    {
        { PkoParser.BankId, GetPkoOperationDescription },
        { PriorParser.BankId, GetPriorOperationDescription }
    };

    public static void GetOperationDisplayName(
        string bankId, SortedDictionary<string, string> tagsToContents,
        out string displayName, out string comparisonKey)
    {
        static string ConvertToComparisonString(string value)
        {
            StringBuilder stringBuilder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (char.IsDigit(c))
                    continue;
                stringBuilder.Append(char.ToLower(c));
            }
            return stringBuilder.ToString();
        }

        if (BankIdToConverters.TryGetValue(bankId, out GetOperationDescriptionDelegate? converter))
        {
            converter(tagsToContents, out displayName, out comparisonKey);
            comparisonKey = ConvertToComparisonString(comparisonKey);
        }
        else
            throw new ArgumentException($"Unknown bank id: {bankId}");
    }

    private static void GetPkoOperationDescription(
        SortedDictionary<string, string> tagsToContents,
        out string displayName, out string comparisonString)
    {
        StringBuilder displayBuilder = new();
        StringBuilder comparisonBuilder = new();

        bool isFirst = true;
        foreach (KeyValuePair<string, string> tagsToContent in tagsToContents)
        {
            string tagName = tagsToContent.Key;
            string displayPart;
            string comparisonKey;

            if (isFirst)
                isFirst = false;
            else
            {
                const string displaySeparator = " || ";
                displayBuilder.Append(displaySeparator);
                const char comparisonSeparator = ' ';
                comparisonBuilder.Append(comparisonSeparator);
            }

            switch (tagName)
            {
                case PkoDescriptionParser.TitleTagName:
                    displayPart = TitleParsingHelper.CorrectIfFromToTitleType(tagsToContent.Value);
                    comparisonKey = displayPart;
                    break;
                case PkoDescriptionParser.LocationTagName:
                    LocationParsingHelper.Parse(tagsToContent.Value,
                            out string address, out (string city, string country)? info);

                    displayPart = info.HasValue 
                        ? $"{address} ({info.Value.city}, {info.Value.country})"
                        : address;
                    comparisonKey = address;
                    break;
                case PkoDescriptionParser.RecipientTagName:
                    displayPart = $"TO: {tagsToContent.Value}";
                    comparisonKey = tagsToContent.Value;
                    break;
                case PkoDescriptionParser.SenderTagName:
                    displayPart = $"FROM: {tagsToContent.Value}";
                    comparisonKey = tagsToContent.Value;
                    break;
                default:
                    displayPart = tagsToContent.Value;
                    comparisonKey = tagsToContent.Value;
                    break;
            }

            displayBuilder.Append(displayPart);
            comparisonBuilder.Append(comparisonKey);
        }

        displayName = displayBuilder.ToString();
        comparisonString = comparisonBuilder.ToString();
    }

    private static void GetPriorOperationDescription(
        SortedDictionary<string, string> tagsToContents,
        out string displayName, out string comparisonString)
    {
        displayName = tagsToContents[PriorParser.DescriptionTagName];
        comparisonString = displayName;
    }
}