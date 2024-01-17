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
    private readonly AssociationStorage associationStorage;
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
    public ICommand SetLastOperationCommand { get; }
    public ICommand SetFirstOperationCommand { get; }
    public ICommand ApplyCategoryForSimilarOperationsCommand { get; }
    public ICommand ExcludeFromAssociations { get; }
    public ICommand ApproveCommand { get; }

    public OperationsVM(
        IReadOnlyList<BaseOperation> baseOperations,
        CategoriesVM categoriesVM,
        Action summaryChanged,
        AssociationStorage associationStorage)
    {
        Categories = categoriesVM.GetCategories();
        this.summaryChanged = summaryChanged;
        this.associationStorage = associationStorage;
        allOperations = GetAllOperations(baseOperations);
        UpdateByFilter();

        categoriesVM.OnCategoryRemoving += CategoriesVMOnOnCategoryRemoving;
        categoriesVM.OnCategoryRemoved += CategoriesVMOnOnCategoryRemoved;
        associationStorage.AssociationRemoved += AssociationStorageAssociationRemoved;
        associationStorage.AssociationsChanged += AssociationStorageOnAssociationsChanged;

        SearchInfoCommand = new DelegateCommand(SearchInfo);
        SetNullCategoryCommand = new DelegateCommand(SetCategoryToNull);
        SetLastOperationCommand = new DelegateCommand(SetLastOperation);
        SetFirstOperationCommand = new DelegateCommand(SetFirstOperation);
        ApplyCategoryForSimilarOperationsCommand = new DelegateCommand(ApplyCategoryForSimilarOperations);
        ExcludeFromAssociations = new DelegateCommand(ExcludeFromAssociationHandler);
        ApproveCommand = new DelegateCommand(Approve);

        isOnRemoving = true;
        AssociationStorageOnAssociationsChanged();
        isOnRemoving = false;
    }

    private void AssociationStorageOnAssociationsChanged()
    {
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
                    CategoryVM? category = associationStorage.TryGetBestCategory(operation.Operation.Description);
                    operation.AssociationStatus = operation.Category == category
                        ? AssociationStatus.None
                        : AssociationStatus.NotMatch;
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
            CategoryVM? categoryVM = associationStorage.TryGetBestCategory(operation.Description);
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
                associationStorage.UpdateAssociation(vm.Operation.Description, vm.Category!);
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
        // Set null for all similar categories
        isOnRemoving = true;
        foreach (OperationVM operation in allOperations)
        {
            if (StringDistance.GetDistancePercent(operation.Operation.Description, operationDescription).HasValue)
                operation.Category = null;
        }
        isOnRemoving = false;
        summaryChanged();
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

    private void ApplyCategoryForSimilarOperations()
    {
        OperationVM selectedOperation = GetSelectedOperation();
        string description = selectedOperation.Operation.Description;
        foreach (OperationVM operation in Operations)
        {
            if (StringDistance.GetDistancePercent(operation.Operation.Description, description).HasValue)
                operation.Category = selectedOperation.Category;
        }
    }

    private void ExcludeFromAssociationHandler()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            associationStorage.AddSimilarToExcluded(operationViewModel.Operation.Description);
    }

    private OperationVM GetSelectedOperation() => (OperationVM)selectedItems![0]!;

    private void Approve()
    {
        foreach (OperationVM operationViewModel in SelectedItems.CheckNull())
            operationViewModel.IsApproved = true;
    }
}