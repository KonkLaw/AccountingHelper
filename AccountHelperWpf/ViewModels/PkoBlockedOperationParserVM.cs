using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class PkoBlockedOperationParserVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    public OperationsGroup? OperationsGroup { get; set; }

    private OperationsGroupVM? operations;
    public OperationsGroupVM? Operations
    {
        get => operations;
        private set => SetProperty(ref operations, value);
    }

    private string? text;
    public string? Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    public ICommand TryParse { get; }
    public ICommand Clear { get; }

    public PkoBlockedOperationParserVM(IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        TryParse = new DelegateCommand(TryParseHandler);
        Clear = new DelegateCommand(ClearOperations);
    }

    private void ClearOperations()
    {
        Operations = null;
        OperationsGroup = null;
    }

    private void TryParseHandler()
    {
        if (string.IsNullOrEmpty(text))
        {
            ClearOperations();
            return;
        }

        PkoParser.TryParseBlocked(text, out OperationsGroup? operationsBlocked, out string? errorMessage);
        if (errorMessage != null)
            viewResolver.ShowWarning(errorMessage);
            
        OperationsGroup = operationsBlocked;
        if (OperationsGroup != null && OperationsGroup.Value.Operations.Count > 0)
        {
            Operations = new OperationsGroupVM(
                OperationsGroup.Value,
                new CategoriesVM(new ObservableCollection<CategoryVM>(), viewResolver),
                SummaryChanged, null);
        }
    }

    private void SummaryChanged() { }
}