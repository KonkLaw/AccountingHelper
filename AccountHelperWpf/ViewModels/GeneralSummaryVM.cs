using AccountHelperWpf.ViewUtils;
using static AccountHelperWpf.ViewModels.MultiCurrencySummaryVM;

namespace AccountHelperWpf.ViewModels;

class GeneralSummaryVM : BaseNotifyProperty
{
    private readonly HashSet<FileSortingVM> viewModels = new();
    private List<CurrencyInfo>? currenciesInfo;
    public List<CurrencyInfo>? CurrenciesInfo
    {
        get => currenciesInfo;
        private set => SetProperty(ref currenciesInfo, value);
    }
    public MultiCurrencySummaryVM SummaryVM { get; } = new ();

    public void UpdateCurrencies(IReadOnlyList<string> currencies)
    {
        List<CurrencyInfo> newList = new();
        foreach (string currency in currencies)
        {
            string courseText;
            if (CurrenciesInfo != null)
            {
                CurrencyInfo? sameOldItem = CurrenciesInfo.FirstOrDefault(item => item.Currency == currency);
                courseText = sameOldItem != null ? sameOldItem.CourseText : string.Empty;
            }
            else
                courseText = string.Empty;


            newList.Add(new CurrencyInfo(currency, courseText, this));
        }
        CurrenciesInfo = newList;
        UpdateSummary();
    }

    public void Register(FileSortingVM fileSortingVM) => viewModels.Add(fileSortingVM);

    public void Unregister(FileSortingVM fileSortingVM) => viewModels.Remove(fileSortingVM);

    public void SummaryChanged() => UpdateSummary();

    public void UpdateSummary()
    {
        if (CurrenciesInfo == null || CurrenciesInfo.Any(info => !info.Course.HasValue))
        {
            SummaryVM.Update(Array.Empty<CategoriesInfo>());
            return;
        }

        List<CategoriesInfo> infos = new List<CategoriesInfo>();
        foreach (FileSortingVM fileSortingVM in viewModels)
        {
            string currency = fileSortingVM.File.Currency;
            CurrencyInfo currencyInfo = CurrenciesInfo.First(ci => ci.Currency == currency);
            infos.Add(new CategoriesInfo(
                fileSortingVM.SummaryVM.CategoriesDetails,
                currency,
                currencyInfo.Course!.Value));
        }
        SummaryVM.Update(infos);
    }
}

class CurrencyInfo : BaseNotifyProperty
{
    private string currency;
    private readonly GeneralSummaryVM generalSummaryVM;

    public string Currency
    {
        get => currency;
        set => SetProperty(ref currency, value);
    }

    private string courseText;
    public string CourseText
    {
        get => courseText;
        set
        {
            if (!SetProperty(ref courseText, value))
                return;
            if (decimal.TryParse(courseText, out decimal newValue))
                Course = newValue;
            else
                Course = null;
        }
    }

    private decimal? course;
    public decimal? Course
    {
        get => course;
        private set
        {
            if (SetProperty(ref course, value))
                generalSummaryVM.SummaryChanged();
        }
    }

    public CurrencyInfo(string currency, string courseText, GeneralSummaryVM generalSummaryVM)
    {
        this.currency = currency;
        this.courseText = courseText;
        if (decimal.TryParse(courseText, out decimal res))
            course = res;
        this.generalSummaryVM = generalSummaryVM;
    }
}