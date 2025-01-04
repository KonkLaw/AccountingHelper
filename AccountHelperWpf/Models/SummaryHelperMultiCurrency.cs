using System.Globalization;
using AccountHelperWpf.Utils;
using System.Text;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class SummaryHelperMultiCurrency
{
    class CategorySummaryTemp
    {
        private readonly string name;
        private readonly List<(CategoryDetails category, string currency)> categories = new();
        private decimal sum;
        public decimal Sum => sum;

        public CategorySummaryTemp(CategoryDetails categoryDetails, string currency, decimal course)
        {
            name = categoryDetails.Name;
            Add(categoryDetails, currency, course);
        }

        public void Add(CategoryDetails categoryDetails, string currency, decimal course)
        {
            sum += categoryDetails.Amount * course;
            categories.Add((categoryDetails, currency));
        }

        public CategoryDetails GetCategory()
            => new CategoryDetails(name, sum, Array.Empty<CategoryDetails.OperationInfo>());

        public void AppendDescription(StringBuilder result)
        {
            result.Append('#');
            result.Append(name);
            result.Append(' ');
            result.Append(sum);
            result.Append(" = ");

            for (int i = 0; i < categories.Count; i++)
            {
                if (i > 0)
                    result.Append("; ");
                (CategoryDetails category, string currency) = categories[i];
                result.Append(category.Amount.ToGoodString());
                result.Append(' ');
                result.Append(ConvertSymbol(currency));
                if (!string.IsNullOrEmpty(category.AdditionalDescription))
                {
                    result.Append(" (");
                    result.Append(category.AdditionalDescription);
                    result.Append(')');
                }
            }
        }
    }

    private static string ConvertSymbol(string currencyFullName)
    {
        switch (currencyFullName)
        {
            case "BYN":
                return "Br";
            case "PLN":
                return "Zł";
            case "USD":
                return "$";
            case "EUR":
                return "€";
            case "RUB":
                return "₽";
            default:
                return currencyFullName;
        }
    }

    public static void Prepare(
        IReadOnlyCollection<MultiCurrencyTextSummaryVM.CategoriesInfo> collection,
        out List<CategoryDetails> categories,
        out string details)
    {
        Dictionary<string, CategorySummaryTemp> dict = new();
        foreach (MultiCurrencyTextSummaryVM.CategoriesInfo categoriesInfo in collection)
        {
            foreach (CategoryDetails categoryDetails in categoriesInfo.Collection)
            {
                if (dict.TryGetValue(categoryDetails.Name, out CategorySummaryTemp? categorySummary))
                {
                    categorySummary.Add(categoryDetails, categoriesInfo.Currency, categoriesInfo.Course);
                }
                else
                {
                    dict.Add(categoryDetails.Name, new CategorySummaryTemp(categoryDetails, categoriesInfo.Currency, categoriesInfo.Course));
                }
            }
        }

        categories = dict.Values.Select(c => c.GetCategory()).ToList();
        details = GetText(dict);
    }

    private static string GetText(Dictionary<string, CategorySummaryTemp> dict)
    {
        var result = new StringBuilder();

        decimal sum = 0;
        foreach (CategorySummaryTemp categorySummaryTemp in dict.Values)
        {
            categorySummaryTemp.AppendDescription(result);
            sum += categorySummaryTemp.Sum;
            result.AppendLine();
        }
        result.Append("Total = ");
        result.AppendLine(sum.ToString(CultureInfo.InvariantCulture));
        return result.ToString();
    }
}