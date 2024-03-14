using System.Windows.Input;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class PkoBlockedOperationParserVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    public IReadOnlyList<PkoBlockedOperation>? BlockedOperations { get; set; }

    private IReadOnlyList<PkoBlockedOperation>? operations;
    public IReadOnlyList<PkoBlockedOperation>? Operations
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

    private bool closeTrigger;
    public bool CloseTrigger
    {
        get => closeTrigger;
        set => SetProperty(ref closeTrigger, value);
    }

    public ICommand TryParse { get; }
    public ICommand Clear { get; }
    public ICommand Accept { get; }
    public ICommand Skip { get; }

    public PkoBlockedOperationParserVM(IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        TryParse = new DelegateCommand(TryParseHandler);
        Clear = new DelegateCommand(ClearOperations);
        Accept = new DelegateCommand(AcceptHandler);
        Skip = new DelegateCommand(SkipOperations);
    }

    private void TryParseHandler()
    {
        if (string.IsNullOrEmpty(text))
        {
            ClearOperations();
            return;
        }

        PkoBlockedParser.TryParse(text, out IReadOnlyList<PkoBlockedOperation>? operationsBlocked, out string? errorMessage);
        if (errorMessage != null)
            viewResolver.ShowWarning(errorMessage);

        BlockedOperations = operationsBlocked;
        if (BlockedOperations is { Count: > 0 })
        {
            Operations = BlockedOperations;
        }
    }

    private void AcceptHandler() => Close();

    private void ClearOperations()
    {
        Operations = null;
        BlockedOperations = null;
    }

    private void SkipOperations()
    {
        ClearOperations();
        Close();
    }

    private void Close() => CloseTrigger = true;
}