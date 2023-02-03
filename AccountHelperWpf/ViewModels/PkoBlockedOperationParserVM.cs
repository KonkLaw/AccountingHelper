using System.Collections.ObjectModel;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class PkoBlockedOperationParserVM : BaseNotifyProperty
{
    public OperationsGroup? OperationsGroup { get; set; }

    private SortedOperationsGroupVM? operations;
    public SortedOperationsGroupVM? Operations
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

    public PkoBlockedOperationParserVM()
    {
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
        OperationsGroup = PkoParser.TryParseBlocked(Text);
        if (OperationsGroup != null)
        {
            Operations = new SortedOperationsGroupVM(OperationsGroup.Value, new ReadOnlyObservableCollection<CategoryVm>(new ObservableCollection<CategoryVm>()), new Fake());
        }
    }

    class Fake : ISummaryChangedListener
    {
        public void Changed() { }
    }
}