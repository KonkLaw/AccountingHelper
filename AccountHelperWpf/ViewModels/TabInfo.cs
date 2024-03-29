﻿using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class TabInfo : BaseNotifyProperty
{
    private bool isHighlighted;
    public bool IsHighlighted
    {
        get => isHighlighted;
        set => SetProperty(ref isHighlighted, value);
    }

    public string Header { get; }
    public object Content { get; }

    public TabInfo(string header, object content)
    {
        Content = content;
        Header = header;
    }
}