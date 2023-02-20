using AccountHelperWpf.Common;

namespace AccountHelperWpf.ViewModels;

class TabInfo : BaseNotifyProperty
{
    private bool isSorted;
    public bool IsSorted
    {
        get => isSorted;
        set => SetProperty(ref isSorted, value);
    }

    public string Header { get; }
    public object Content { get; }

    public TabInfo(string header, object content)
    {
        Content = content;
        Header = header;
    }
}