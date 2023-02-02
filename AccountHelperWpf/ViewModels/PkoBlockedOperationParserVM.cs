using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class PkoBlockedOperationParserVM : BaseNotifyProperty
{
    public OperationsGroup? Operations
    {
        get => operations;
        private set => SetProperty(ref operations, value);
    }

    private string? text;
    private OperationsGroup? operations;

    public string? Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    public ICommand TryParse { get; }

    public PkoBlockedOperationParserVM()
    {
        TryParse = new DelegateCommand(TryParseHandler);
    }

    private void TryParseHandler() => Operations = PkoParser.TryParseBlocked(Text);
}