using System.Text;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.Utils;
using AccountHelperWpf.Views;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class FileSortingViewModel : BaseNotifyProperty
{
    private readonly CategoriesVM categoriesVM;
    private readonly ISaveController saveController;
    private readonly TabInfo tabInfo;

    private string summary = string.Empty;

    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    public IReadOnlyList<OperationsGroupVM> OperationsGroups { get; }

    public ICommand SetForAllCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand ApproveAllCommand { get; }

    public FileSortingViewModel(
        AccountFile accountFile,
        CategoriesVM categoriesVM,
        Action<object> removeHandler,
        AssociationStorage associationsStorage,
        ISaveController saveController)
    {
        this.categoriesVM = categoriesVM;
        this.saveController = saveController;
        tabInfo = new TabInfo(accountFile.Description.Name, this);
        categoriesVM.CategoryOrListChanged += CategoriesVMOnCategoryOrListChanged;
        OperationsGroups = accountFile.OperationsGroups.Select(
            operationGroup => new OperationsGroupVM(
                operationGroup, categoriesVM, UpdateSummary, associationsStorage)).ToList();
        SetForAllCommand = new DelegateCommand(SetForAllHandler);
        ResetFiltersCommand = new DelegateCommand(ResetFiltersHandler);
        RemoveFileCommand = new DelegateCommand(() => removeHandler(this));
        ApproveAllCommand = new DelegateCommand(ApproveHandler);
        UpdateSummary();
    }

    private void CategoriesVMOnCategoryOrListChanged()
    {
        saveController.MarkChanged();
        UpdateSummary();
    }

    public TabInfo GetTabItem() => tabInfo;

    private void SetForAllHandler()
    {
        ObjectSelectorWindow window = new (categoriesVM.GetCategories());
        window.ShowDialog();
        if (window.SelectedItem == null)
            return;
        CategoryVM selectedItem = (CategoryVM)window.SelectedItem;
        foreach (OperationsGroupVM operationsGroup in OperationsGroups)
        {
            foreach (OperationVM operation in operationsGroup.Operations)
            {
                operation.Category ??= selectedItem;
            }
        }
    }

    private void ResetFiltersHandler()
    {
        categoriesVM.CategoryOrListChanged -= UpdateSummary;
        foreach (OperationsGroupVM sortedOperationsGroup in OperationsGroups)
            sortedOperationsGroup.ResetFilters();
        UpdateSummary();
    }

    private void ApproveHandler()
    {
        foreach (OperationsGroupVM operationsGroupVM in OperationsGroups)
            foreach (OperationVM operationVM in operationsGroupVM.Operations)
                operationVM.IsApproved = true;
    }

    private void UpdateSummary()
    {
        CategorySummary notAssigned = new ("Not Assigned");
        Dictionary<CategoryVM, CategorySummary> categoriesSummary = categoriesVM.
            GetCategories().ToDictionary(c => c, c => new CategorySummary(c!.Name));
        bool allSorted = true;

        foreach (OperationsGroupVM operationsGroup in OperationsGroups)
        {
            foreach (OperationVM operation in operationsGroup.Operations)
            {
                if (!operation.IsApproved)
                    allSorted = false;

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
    private readonly List<OperationVM> operations = new ();

    public CategorySummary(string categoryName) => this.categoryName = categoryName;

    public void Add(OperationVM operationVM) => operations.Add(operationVM);

    public StringBuilder GetSummary()
    {
        StringBuilder result = new();
        result.Append('#');
        result.Append(categoryName);
        result.Append(' ');

        decimal sum = 0;
        StringBuilder detailed = new();
        foreach (OperationVM operation in operations)
        {
            sum += operation.Operation.Amount;
            if (string.IsNullOrEmpty(operation.Description))
                continue;
            if (detailed.Length != 0)
                detailed.Append(' ');
            detailed.Append(operation.Operation.Amount.ToGoodString());
            detailed.Append(' ');
            detailed.Append(operation.Description);
            detailed.Append(',');
        }
        result.Append(sum.ToGoodString());

        if (detailed.Length != 0)
        {
            detailed.Remove(detailed.Length - 1, 1);
            result.Append(" (");
            result.Append(detailed);
            result.Append(')');
        }
            
        return result;
    }
}