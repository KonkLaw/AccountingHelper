using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AccountHelperWpf.BaseObjects;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class FileSortingViewModel : BaseNotifyProperty, ICategoryChangedListener
{
    private readonly string currency;
    private readonly ReadOnlyObservableCollection<CategoryVm> categories;
    private readonly Action updatedHandler;
    public IReadOnlyList<SortedOperationsGroup> OperationsGroups { get; }

    private string summary = string.Empty;
    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    private bool isSorted;
    public bool IsSorted
    {
        get => isSorted;
        private set
        {
            if (isSorted == value)
                return;
            isSorted = value;
            updatedHandler();
        }
    }

    public ICommand SetForAllCommand { get; }

    public FileSortingViewModel(AccountFile accountFile, ReadOnlyObservableCollection<CategoryVm> categories, Action updatedHandler)
    {
        currency = accountFile.Description.Currency;
        this.categories = categories;
        this.updatedHandler = updatedHandler;
        OperationsGroups = accountFile.OperationsGroups.Select(
            og => new SortedOperationsGroup(og, this.categories, this)).ToList();
        SetForAllCommand = new DelegateCommand(SetForAllHandler);
        UpdateSummary();
    }

    private void SetForAllHandler()
    {
        // TODO
        var window = new ObjectSelectorWindow(categories);
        window.ShowDialog();
        if (window.SelectedItem == null)
            return;
        CategoryVm selectedItem = (CategoryVm)window.SelectedItem;
        foreach (SortedOperationsGroup operationsGroup in OperationsGroups)
        {
            foreach (SortedOperation operation in operationsGroup.Operations)
            {
                operation.Category ??= selectedItem;
            }
        }
    }

    public void CategoryChanged() => UpdateSummary();

    private void UpdateSummary()
    {
        decimal notDefinedAmount = 0;
        Dictionary<CategoryVm, Sum> categoryToSum = categories.ToDictionary(c => c, _ => new Sum());
        bool allSorted = true;
        foreach (SortedOperationsGroup operationsGroup in OperationsGroups)
        {
            foreach (SortedOperation operation in operationsGroup.Operations)
            {
                if (operation.Category == null)
                {
                    notDefinedAmount += operation.Operation.AccountAmount;
                    allSorted = false;
                }
                else
                    categoryToSum[operation.Category].Amount += operation.Operation.AccountAmount;
            }
        }

        IsSorted = allSorted;

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