using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class BaseTextSummaryVM : BaseNotifyProperty
{
    private readonly ObservableCollection<CategoryDetails> collection;
    public IReadOnlyCollection<CategoryDetails> CategoriesDetails => collection;

    private string textSummary;
    public string TextSummary
    {
        get => textSummary;
        set => SetProperty(ref textSummary, value);
    }

    private decimal amountSelected;
    public decimal AmountSelected
    {
        get => amountSelected;
        private set => SetProperty(ref amountSelected, value);
    }

    private decimal amountNonSelected;
    public decimal AmountNonSelected
    {
        get => amountNonSelected;
        private set => SetProperty(ref amountNonSelected, value);
    }

    private bool groupByComment = true;
    public bool GroupByComment
    {
        get => groupByComment;
        set => SetProperty(ref groupByComment, value);
    }

    private bool isGroupingUsed;
    public bool IsGroupingUsed
    {
        get => isGroupingUsed;
        set => SetProperty(ref isGroupingUsed, value);
    }

    public ICommand UnselectCommand { get; }
    public ICommand SelectCommand { get; }
    public ICommand InvertCommand { get; }

    public BaseTextSummaryVM()
    {
        collection = new ObservableCollection<CategoryDetails>();
        textSummary = string.Empty;
        UnselectCommand = new DelegateCommand(Unselect);
        SelectCommand = new DelegateCommand(Select);
        InvertCommand = new DelegateCommand(Invert);
    }

    private void CategoryDetailsChanged(object? sender, PropertyChangedEventArgs e)
        => UpdateAmount();

    private void UpdateAmount()
    {
        AmountSelected = CategoriesDetails.Where(c => c.IsSelected).Sum(c => c.Amount);
        AmountNonSelected = CategoriesDetails.Where(c => !c.IsSelected).Sum(c => c.Amount);
    }

    private void Unselect()
    {
        foreach (CategoryDetails categoryDetails in collection)
            categoryDetails.IsSelected = false;
    }

    private void Select()
    {
        foreach (CategoryDetails categoryDetails in collection)
            categoryDetails.IsSelected = true;
    }

    private void Invert()
    {
        foreach (CategoryDetails categoryDetails in collection)
            categoryDetails.IsSelected = !categoryDetails.IsSelected;
    }

    protected void UpdateCollection(ICollection<CategoryDetails> newCollection, string report)
    {
        if (collection.All(c => c.IsSelected))
        {
            foreach (CategoryDetails categoryDetails in newCollection)
                categoryDetails.IsSelected = true;
        }
        else if (collection.All(c => !c.IsSelected))
        {
            foreach (CategoryDetails categoryDetails in newCollection)
                categoryDetails.IsSelected = false;
        }
        else
        {
            foreach (CategoryDetails oldItem in collection)
            {
                CategoryDetails? details = newCollection.FirstOrDefault(c => c.Name == oldItem.Name);
                if (details != null)
                    details.IsSelected = oldItem.IsSelected;
            }
        }
        collection.Clear();

        foreach (CategoryDetails categoryDetails in newCollection)
        {
            if (categoryDetails.Amount == 0)
                continue;
            collection.Add(categoryDetails);
            categoryDetails.PropertyChanged += CategoryDetailsChanged;
        }
        UpdateAmount();
        TextSummary = report;
    }
}


class MultiCurrencyTextSummaryVM : BaseTextSummaryVM
{
    internal readonly record struct CategoriesInfo(IReadOnlyCollection<CategoryDetails> Collection, string Currency, decimal Course);

    public MultiCurrencyTextSummaryVM()
    {
        IsGroupingUsed = false;
    }

    public void Update(IReadOnlyCollection<CategoriesInfo> collection)
    {
        SummaryHelperMultiCurrency.Prepare(collection, out List<CategoryDetails> categories, out string textSummary);
        UpdateCollection(categories, textSummary);
    }
}

class SingleCurrencyTextSummaryVM : BaseTextSummaryVM
{
    public SingleCurrencyTextSummaryVM()
    {
        IsGroupingUsed = true;
    }

    public void Update(ICollection<CategoryDetails> newCollection)
        => UpdateCollection(newCollection, GetTextDescription(newCollection));

    private static string GetTextDescription(ICollection<CategoryDetails> newCollection)
    {
        decimal sum = 0;
        StringBuilder result = new();
        foreach (CategoryDetails categoryDetails in newCollection)
        {
            if (categoryDetails.Amount == 0)
                continue;
            result.Append('#');
            result.Append(categoryDetails.Name);
            result.Append(' ');
            result.Append(categoryDetails.Amount.ToGoodString());
            if (!string.IsNullOrEmpty(categoryDetails.AdditionalDescription))
            {
                result.Append(" (");
                result.Append(categoryDetails.AdditionalDescription);
                result.Append(')');
            }
            sum += categoryDetails.Amount;
            result.AppendLine();
        }

        result.AppendLine($"Total = {sum.ToGoodString()}");
        return result.ToString();
    }
}