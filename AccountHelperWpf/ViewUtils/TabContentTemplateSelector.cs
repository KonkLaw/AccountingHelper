﻿using System.Windows;
using System.Windows.Controls;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.ViewUtils;

class TabContentTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        object content = ((TabInfo)item).Content;

        if (content is FileSortingViewModel)
            return (DataTemplate)Application.Current!.FindResource("FileSortingTemplate")!;
        if (content is CategoriesVM)
            return (DataTemplate)Application.Current!.FindResource("CategoriesTemplate")!;
        if (content is AssociationsVM)
            return (DataTemplate)Application.Current!.FindResource("AssociationsTemplate")!;

        throw new InvalidOperationException();
    }
}