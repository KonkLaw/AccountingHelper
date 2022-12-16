using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AccountHelperWpf.BaseObjects;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class FileSortingViewModel : BaseNotifyProperty, ICategoryChangedListener
{
    private readonly string currency;
    private readonly ReadOnlyObservableCollection<CategoryVm> categories;
    public IReadOnlyList<SortedOperationsGroup> OperationsGroups { get; }

    private string summary = string.Empty;
    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    public FileSortingViewModel(AccountFile accountFile, ReadOnlyObservableCollection<CategoryVm> categories)
    {
        currency = accountFile.Description.Currency;
        this.categories = categories;
        OperationsGroups = accountFile.OperationsGroups.Select(
            og => new SortedOperationsGroup(og, this.categories, this)).ToList();
        UpdateSummary();
    }

    public void CategoryChanged() => UpdateSummary();

    private void UpdateSummary()
    {
        decimal notDefinedAmount = 0;
        Dictionary<CategoryVm, Sum> categoryToSum = categories.ToDictionary(c => c, _ => new Sum());

        foreach (SortedOperationsGroup operationsGroup in OperationsGroups)
        {
            foreach (SortedOperation operation in operationsGroup.Operations)
            {
                if (operation.Category == null)
                    notDefinedAmount += operation.Operation.AccountAmount;
                else
                    categoryToSum[operation.Category].Amount += operation.Operation.AccountAmount;
            }
        }

        StringBuilder stringBuilder = new();
        foreach (KeyValuePair<CategoryVm, Sum> categoryAndSum in categoryToSum)
        {
            stringBuilder.Append(categoryAndSum.Key);
            stringBuilder.Append(" = ");
            stringBuilder.Append(categoryAndSum.Value.Amount);
            stringBuilder.Append(' ');
            stringBuilder.Append(currency);
            stringBuilder.Append("; ");
        }
        stringBuilder.Append("Not Assigned = ");
        stringBuilder.Append(notDefinedAmount);

        Summary = stringBuilder.ToString();
    }

    class Sum { public decimal Amount; }
}

interface ICategoryChangedListener
{
    void CategoryChanged();
}