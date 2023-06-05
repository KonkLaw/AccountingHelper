using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class PkoBlockedOperationParserVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    public IReadOnlyList<PkoBlockedOperation>? BlockedOperations { get; set; }

    private OperationsVM? operations;
    public OperationsVM? Operations
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
        BlockedOperations = null;
    }

    private void TryParseHandler()
    {
        if (string.IsNullOrEmpty(text))
        {
            ClearOperations();
            return;
        }

        PkoParser.TryParseBlocked(text, out IReadOnlyList<PkoBlockedOperation>? operationsBlocked, out string? errorMessage);
        if (errorMessage != null)
            viewResolver.ShowWarning(errorMessage);

        BlockedOperations = operationsBlocked;
        if (BlockedOperations is { Count: > 0 })
        {
            Operations = new OperationsVM(
                Converter.ConvertBlockedOperations(BlockedOperations),
                new CategoriesVM(new ObservableCollection<CategoryVM>(), viewResolver),
                () => { }, null);
        }
    }
}