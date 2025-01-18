using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class TabInfo : BaseNotifyProperty
{
    private bool isHighlighted;
    public bool IsHighlighted
    {
        get => isHighlighted;
        set => SetProperty(ref isHighlighted, value);
    }

    public string IconCode { get; }

    public string Header { get; }

    public object Content { get; }

    public TabInfo(TabTypeEnum tabType, string header, object content)
    {
        IconCode = tabType switch
        {
            TabTypeEnum.File => "\ue75b",
            TabTypeEnum.Category => "\uE71D",
            TabTypeEnum.Associations => "\ue8d3",
            TabTypeEnum.Summary => "\ue9f9",
            _ => throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null)
        };
        Content = content;
        Header = "  " + header;
    }

    public enum TabTypeEnum
    {
        File,
        Category,
        Associations,
        Summary
    }
}