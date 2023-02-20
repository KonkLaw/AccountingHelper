using System.Text;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Views;

namespace AccountHelperWpf.ViewModels;

class FileSortingViewModel : BaseNotifyProperty, ISummaryChangedListener
{
    private readonly AccountFile accountFile;
    private readonly CategoriesViewModel categoriesViewModel;
    private readonly TabInfo tabInfo;

    private string summary = string.Empty;

    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    public IReadOnlyList<SortedOperationsGroupVM> OperationsGroups { get; }

    public ICommand SetForAllCommand { get; }
    public ICommand ResetFilters { get; }
    public ICommand RemoveFile { get; }

    public FileSortingViewModel(AccountFile accountFile,
        CategoriesViewModel categoriesViewModel, Action<object> removeHandler)
    {
        this.accountFile = accountFile;
        this.categoriesViewModel = categoriesViewModel;
        tabInfo = new TabInfo(accountFile.Description.Name, this);
        categoriesViewModel.Changed += UpdateSummary;
        OperationsGroups = accountFile.OperationsGroups.Select(
            operationGroup => new SortedOperationsGroupVM(operationGroup, categoriesViewModel.GetCategories(), this)).ToList();
        SetForAllCommand = new DelegateCommand(SetForAllHandler);
        ResetFilters = new DelegateCommand(ResetFiltersHandler);
        RemoveFile = new DelegateCommand(() => removeHandler(this));
        UpdateSummary();
    }

    public TabInfo GetTabItem() => tabInfo;

    public void Changed() => UpdateSummary();

    private void SetForAllHandler()
    {
        ObjectSelectorWindow window = new (categoriesViewModel.GetCategories());
        window.ShowDialog();
        if (window.SelectedItem == null)
            return;
        CategoryViewModel selectedItem = (CategoryViewModel)window.SelectedItem;
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
        categoriesViewModel.Changed -= UpdateSummary;
        foreach (SortedOperationsGroupVM sortedOperationsGroup in OperationsGroups)
            sortedOperationsGroup.ResetFilter();
        Changed();
    }

    private void UpdateSummary()
    {
        CategorySummary notAssigned = new ("Not Assigned");
        Dictionary<CategoryViewModel, CategorySummary> categoriesSummary = categoriesViewModel.GetCategories().ToDictionary(c => c, c => new CategorySummary(c.Name));
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

        tabInfo.IsSorted = allSorted;

        StringBuilder stringBuilder = new();
        foreach (CategorySummary categorySummary in categoriesSummary.Values)
        {
            stringBuilder.Append(categorySummary.GetSummary());
            stringBuilder.AppendLine();
        }
        stringBuilder.Append(notAssigned.GetSummary());

        Summary = stringBuilder.ToString();
    }
}

class CategorySummary
{
    private readonly string categoryName;
    private readonly List<OperationViewModel> operations = new ();

    public CategorySummary(string categoryName) => this.categoryName = categoryName;

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
        result.Append(sum.ToString(sum % 1 == 0 ? "G0" : "G"));

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

interface ISummaryChangedListener
{
    void Changed();
}