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
        CategorySummary notAssigned = new ("Not Assigned");
        Dictionary<CategoryVm, CategorySummary> categoriesSummary = categories.ToDictionary(c => c, c => new CategorySummary(c.Name));
        bool allSorted = true;

        foreach (SortedOperationsGroup operationsGroup in OperationsGroups)
        {
            foreach (SortedOperation operation in operationsGroup.Operations)
            {
                if (operation.Category == null)
                {
                    notAssigned.Add(operation);
                    allSorted = false;
                }
                else
                    categoriesSummary[operation.Category].Add(operation);
            }
        }

        IsSorted = allSorted;

        StringBuilder stringBuilder = new();
        foreach (CategorySummary categorySummary in categoriesSummary.Values)
        {
            stringBuilder.Append(categorySummary.GetSummary());
            stringBuilder.AppendLine();
        }
        stringBuilder.Append(notAssigned.GetSummary());

        Summary = stringBuilder.ToString();
    }

    class CategorySummary
    {
        private readonly string categoryName;
        private readonly List<SortedOperation> operations = new ();

        public CategorySummary(string categoryName)
        {
            this.categoryName = categoryName;
        }

        public void Add(SortedOperation operation) => operations.Add(operation);

        public StringBuilder GetSummary()
        {
            StringBuilder result = new();
            result.Append("#");
            result.Append(categoryName);
            result.Append(" ");

            decimal sum = 0;
            StringBuilder detailed = new();
            foreach (SortedOperation operation in operations)
            {
                sum += operation.Operation.AccountAmount;
                if (string.IsNullOrEmpty(operation.Description))
                    continue;

                detailed.Append(operation.Operation.AccountAmount);
                detailed.Append(" ");
                detailed.Append(operation.Description);
                detailed.Append(",");
            }
            result.Append(sum);

            if (detailed.Length != 0)
            {
                detailed.Remove(detailed.Length - 1, 1);
                result.Append(" = (");
                result.Append(detailed);
                result.Append(")");
            }
            
            return result;
        }
    }
}

interface ICategoryChangedListener
{
    void CategoryChanged();
}