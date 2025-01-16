using System.ComponentModel;
using AccountHelperWpf.ViewUtils;
using static AccountHelperWpf.ViewModels.MultiCurrencyTextSummaryVM;

namespace AccountHelperWpf.ViewModels;

class SummaryVM : BaseNotifyProperty, ISummaryFiles, ISummaryNotifier
{
    private readonly HashSet<FileSortingVM> viewModels = new();
    private List<CurrencyInfo> currenciesInfo = new();
    public List<CurrencyInfo> CurrenciesInfo
    {
        get => currenciesInfo;
        private set => SetProperty(ref currenciesInfo, value);
    }
    public MultiCurrencyTextSummaryVM TextSummaryVM { get; } = new ();

    public SummaryVM()
    {
        TextSummaryVM.PropertyChanged += TextSummaryVMOnPropertyChanged;
    }

    private void TextSummaryVMOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BaseTextSummaryVM.GroupByComment))
            UpdateSummary();
    }

    public void Register(FileSortingVM fileSortingVM)
    {
        viewModels.Add(fileSortingVM);
        UpdateCurrencies();
    }

    public void Unregister(FileSortingVM fileSortingVM)
    {
        if (viewModels.Remove(fileSortingVM))
            UpdateCurrencies();
    }

    public void NotifySummaryChanged() => UpdateSummary();

    private void UpdateSummary()
    {
        if (CurrenciesInfo.Any(info => !info.Course.HasValue))
        {
            TextSummaryVM.Update(Array.Empty<CategoriesInfo>());
            return;
        }

        List<CategoriesInfo> infos = new List<CategoriesInfo>();
        foreach (FileSortingVM fileSortingVM in viewModels)
        {
            string currency = fileSortingVM.File.Currency;
            CurrencyInfo currencyInfo = CurrenciesInfo.First(ci => ci.Currency == currency);
            infos.Add(new CategoriesInfo(
                fileSortingVM.TextSummaryVM.CategoriesDetails,
                currency,
                currencyInfo.Course!.Value));
        }
        TextSummaryVM.Update(infos);
    }

    private void UpdateCurrencies()
    {
        List<CurrencyInfo> newList = new();
        foreach (FileSortingVM file in viewModels)
        {
            string currency = file.File.Currency;
            CurrencyInfo? sameOldItem = CurrenciesInfo.FirstOrDefault(item => item.Currency == currency);
            string courseText = sameOldItem != null ? sameOldItem.CourseText : string.Empty;
            newList.Add(new CurrencyInfo(currency, courseText, this));
        }
        CurrenciesInfo = newList;
        UpdateSummary();
    }
}

interface ISummaryFiles
{
    void Register(FileSortingVM fileSortingVM);
    void Unregister(FileSortingVM fileSortingVM);
}

interface ISummaryNotifier
{
    void NotifySummaryChanged();
}

class CurrencyInfo : BaseNotifyProperty
{
    private string currency;
    private readonly ISummaryNotifier summaryNotifier;

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
                summaryNotifier.NotifySummaryChanged();
        }
    }

    public CurrencyInfo(string currency, string courseText, ISummaryNotifier summaryNotifier)
    {
        this.currency = currency;
        this.courseText = courseText;
        if (decimal.TryParse(courseText, out decimal res))
            course = res;
        this.summaryNotifier = summaryNotifier;
    }
}