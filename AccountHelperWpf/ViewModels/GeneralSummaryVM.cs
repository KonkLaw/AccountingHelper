using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class GeneralSummaryVM : BaseNotifyProperty
{
    private List<CurrencyInfo>? currenciesInfo;
    public List<CurrencyInfo>? CurrenciesInfo
    {
        get => currenciesInfo;
        private set => SetProperty(ref currenciesInfo, value);
    }

    public void UpdateCurrencies(IReadOnlyList<string> currencies)
    {
        List<CurrencyInfo> newList = currencies.Select(c => new CurrencyInfo(c)).ToList();
        if (CurrenciesInfo != null)
        {
            foreach (CurrencyInfo oldItem in CurrenciesInfo)
            {
                CurrencyInfo? sameNewItem = newList.FirstOrDefault(item => item.Currency == oldItem.Currency);
                if (sameNewItem != null)
                    sameNewItem.CourseText = oldItem.CourseText;
            }
        }
        CurrenciesInfo = newList;
    }
}

class CurrencyInfo : BaseNotifyProperty
{
    private string currency;
    public string Currency
    {
        get => currency;
        set => SetProperty(ref currency, value);
    }

    private string courseText = string.Empty;
    public string CourseText
    {
        get => courseText;
        set
        {
            if (SetProperty(ref courseText, value)
                && decimal.TryParse(courseText, out decimal newValue))
                Course = newValue;
        }
    }

    private decimal? course;
    public decimal? Course
    {
        get => course;
        private set => SetProperty(ref course, value);
    }

    public CurrencyInfo(string currency)
    {
        this.currency = currency;
    }
}