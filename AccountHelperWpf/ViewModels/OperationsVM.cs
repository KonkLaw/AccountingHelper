using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationsVM : BaseNotifyProperty
{
    private readonly Action summaryChanged;
    private readonly AssociationStorage? associationStorage;
    private readonly List<OperationVM> allOperations;
    private bool isOnRemoving;
    private OperationVM? firstIncluded;
    private OperationVM? lastIncluded;

    public IEnumerable<CategoryVM> Categories { get; }

    private IReadOnlyList<OperationVM> operations = null!;
    public IReadOnlyList<OperationVM> Operations
    {
        get => operations;
        set => SetProperty(ref operations, value);
    }

    private IList? selectedItems;
    public IList? SelectedItems
    {
        get => selectedItems;
        set
        {
            SetProperty(ref selectedItems, value);
            // We don't check equality as reference to collection is the same
            // however count is different
            IsSingleSelection = selectedItems is { Count: 1 };
        }
    }

    private bool isSingleSelection;
    public bool IsSingleSelection
    {
        get => isSingleSelection;
        set => SetProperty(ref isSingleSelection, value);
    }

    public ICommand SearchInfoCommand { get; }
    public ICommand SetNullCategoryCommand { get; }
    public ICommand SetFirstOperationCommand { get; }
    public ICommand SetLastOperationCommand { get; }
    public ICommand ApplyCategoryForSameOperationsCommand { get; }
    public ICommand ExcludeFromAssociations { get; }
    public ICommand ApproveCommand { get; }

    public OperationsVM(
        IReadOnlyList<BaseOperation> baseOperations,
        CategoriesVM categoriesVM,
        Action summaryChanged,
        AssociationStorage? associationStorage)
    {
        Categories = categoriesVM.GetCategories();
        this.summaryChanged = summaryChanged;
        this.associationStorage = associationStorage;
        allOperations = GetAllOperations(baseOperations);
        UpdateByFilter();

        categoriesVM.OnCategoryRemoving += CategoriesVMOnOnCategoryRemoving;
        categoriesVM.OnCategoryRemoved += CategoriesVMOnOnCategoryRemoved;
        if (associationStorage != null)
        {
            associationStorage.AssociationRemoved += AssociationStorageAssociationRemoved;
            associationStorage.AssociationsChanged += AssociationStorageOnAssociationsChanged;
        }

        SearchInfoCommand = new DelegateCommand(SearchInfo);
        SetNullCategoryCommand = new DelegateCommand(SetCategoryToNull);
        SetLastOperationCommand = new DelegateCommand(SetLastOperation);
        SetFirstOperationCommand = new DelegateCommand(SetFirstOperation);
        ApplyCategoryForSameOperationsCommand = new DelegateCommand(ApplyCategoryForSameOperations);
        ExcludeFromAssociations = new DelegateCommand(ExcludeFromAssociationHandler);
        ApproveCommand = new DelegateCommand(Approve);

        isOnRemoving = true;
        AssociationStorageOnAssociationsChanged();
        isOnRemoving = false;
    }

    private void AssociationStorageOnAssociationsChanged()
    {
        if (associationStorage == null)
            return;

        foreach (OperationVM operation in allOperations)
        {
            if (associationStorage.IsExcluded(operation.Operation.Description))
            {
                operation.AssociationStatus = AssociationStatus.Excluded;
            }
            else
            {
                if (operation.Category == null)
                {
                    operation.AssociationStatus = AssociationStatus.None;
                }
                else
                {
                    CategoryVM? category = associationStorage!.TryGetCategory(operation.Operation.Description);
                    operation.AssociationStatus = operation.Category == category
                        ? AssociationStatus.None
                        : AssociationStatus.NotCorrespond;
                }
            }
        }
    }

    private List<OperationVM> GetAllOperations(IReadOnlyList<BaseOperation> baseOperations)
    {
        var result = new List<OperationVM>(baseOperations.Count);
        foreach (BaseOperation operation in baseOperations)
        {
            OperationVM operationVM = new(operation);
            CategoryVM? categoryVM = associationStorage?.TryGetCategory(operation.Description);
            if (categoryVM != null)
            {
                operationVM.Category = categoryVM;
                operationVM.IsApproved = false;
            }
            result.Add(operationVM);
            operationVM.PropertyChanged += OperationViewModelOnPropertyChanged;
        }
        return result;
    }

    private void OperationViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (isOnRemoving)
            return;

        switch (e.PropertyName)
        {
            case nameof(OperationVM.Category):
            {
                OperationVM vm = (OperationVM)sender!;
                foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
                    operationViewModel.Category = vm.Category;
                associationStorage?.Update(vm.Operation.Description, vm.Category!);
                summaryChanged();
                break;
            }
            case nameof(OperationVM.AssociationStatus):
            case nameof(OperationVM.Comment):
            case nameof(OperationVM.IsApproved):
                summaryChanged();
                break;
        }
    }

    private void CategoriesVMOnOnCategoryRemoving() => isOnRemoving = true;

    private void CategoriesVMOnOnCategoryRemoved()
    {
        isOnRemoving = false;
        summaryChanged();
    }

    private void AssociationStorageAssociationRemoved(string operationDescription)
    {
        isOnRemoving = true;
        foreach (OperationVM operation in allOperations)
        {
            if (operation.Operation.Description == operationDescription)
                operation.Category = null;
        }
        isOnRemoving = false;
    }

    private void UpdateByFilter()
    {
        var filteredOperations = new List<OperationVM>(allOperations.Count);
        bool skip = lastIncluded != null;
        foreach (OperationVM operationVM in allOperations)
        {
            if (skip && (skip = operationVM != lastIncluded))
                continue;
            filteredOperations.Add(operationVM);
            if (operationVM == firstIncluded)
                break;
        }
        Operations = filteredOperations;
    }

    public void ResetFilters()
    {
        firstIncluded = null;
        lastIncluded = null;
        UpdateByFilter();
    }

    private void SearchInfo()
    {
        string searchQuery = GetSelectedOperation().Operation.Description;
        string encodedQuery = Uri.EscapeDataString(searchQuery);
        string searchUrl = $"https://www.google.com/search?q={encodedQuery}";
        // Starts the default web browser with the search URL
        Process.Start(new ProcessStartInfo
        {
            FileName = searchUrl,
            UseShellExecute = true
        });
    }

    private void SetCategoryToNull()
    {
        foreach (OperationVM operationVM in SelectedItems.CheckNull())
            operationVM.Category = null;
    }

    private void SetFirstOperation()
    {
        firstIncluded = GetSelectedOperation();
        UpdateByFilter();
        summaryChanged();
    }

    private void SetLastOperation()
    {
        lastIncluded = GetSelectedOperation();
        UpdateByFilter();
        summaryChanged();
    }

    private void ApplyCategoryForSameOperations()
    {
        OperationVM selectedOperation = GetSelectedOperation();
        foreach (OperationVM operation in Operations)
        {
            if (operation.Operation.Description == selectedOperation.Operation.Description)
                operation.Category = selectedOperation.Category;
        }
    }

    private void ExcludeFromAssociationHandler()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            associationStorage?.AddToExcludedOperations(operationViewModel.Operation.Description);
    }

    private OperationVM GetSelectedOperation() => (OperationVM)selectedItems![0]!;

    private void Approve()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsApproved = true;
    }
}