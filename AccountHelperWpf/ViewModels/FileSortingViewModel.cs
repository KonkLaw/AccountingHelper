using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class FileSortingViewModel : BaseNotifyProperty, ISummaryChangedListener
{
    private readonly ReadOnlyObservableCollection<CategoryVm> categories;
    private readonly Action sortedChangedHandler;
    public IReadOnlyList<SortedOperationsGroupVM> OperationsGroups { get; set; }

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
            sortedChangedHandler();
        }
    }

    public ICommand SetForAllCommand { get; }
    public ICommand ResetFilters { get; }
    public ICommand RemoveFile { get; }

    public FileSortingViewModel(AccountFile accountFile,
        ReadOnlyObservableCollection<CategoryVm> categories,
        Action sortedChangedHandler, Action<object> removeHandler)
    {
        this.categories = categories;
        this.sortedChangedHandler = sortedChangedHandler;
        OperationsGroups = accountFile.OperationsGroups.Select(
            operationGroup => new SortedOperationsGroupVM(operationGroup, categories, this)).ToList();
        SetForAllCommand = new DelegateCommand(SetForAllHandler);
        ResetFilters = new DelegateCommand(ResetFiltersHandler);
        RemoveFile = new DelegateCommand(() => removeHandler(this));
        UpdateSummary();
    }

    private void SetForAllHandler()
    {
        ObjectSelectorWindow window = new (categories);
        window.ShowDialog();
        if (window.SelectedItem == null)
            return;
        CategoryVm selectedItem = (CategoryVm)window.SelectedItem;
        foreach (SortedOperationsGroupVM operationsGroup in OperationsGroups)
        {
            foreach (OperationViewModel operation in operationsGroup.Operations)
            {
                operation.Category ??= selectedItem;
            }
        }
    }

    private void ResetFiltersHandler()
    {
        foreach (SortedOperationsGroupVM sortedOperationsGroup in OperationsGroups)
            sortedOperationsGroup.ResetFilter();
        Changed();
    }

    public void Changed() => UpdateSummary();

    private void UpdateSummary()
    {
        CategorySummary notAssigned = new ("Not Assigned");
        Dictionary<CategoryVm, CategorySummary> categoriesSummary = categories.ToDictionary(c => c, c => new CategorySummary(c.Name));
        bool allSorted = true;

        foreach (SortedOperationsGroupVM operationsGroup in OperationsGroups)
        {
            foreach (OperationViewModel operation in operationsGroup.Operations)
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
        private readonly List<OperationViewModel> operations = new ();

        public CategorySummary(string categoryName)
        {
            this.categoryName = categoryName;
        }

        public void Add(OperationViewModel operationViewModel) => operations.Add(operationViewModel);

        public StringBuilder GetSummary()
        {
            StringBuilder result = new();
            result.Append("#");
            result.Append(categoryName);
            result.Append(" ");

            decimal sum = 0;
            StringBuilder detailed = new();
            foreach (OperationViewModel operation in operations)
            {
                sum += operation.Operation.Amount;
                if (string.IsNullOrEmpty(operation.Description))
                    continue;

                detailed.Append(operation.Operation.Amount);
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

interface ISummaryChangedListener
{
    void Changed();
}