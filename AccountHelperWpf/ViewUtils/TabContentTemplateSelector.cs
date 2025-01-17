using System.Windows;
using System.Windows.Controls;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.ViewUtils;

class TabContentTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FileSortingTemplate { get; set; }
    public DataTemplate? CategoriesTemplate { get; set; }
    public DataTemplate? AssociationsTemplate { get; set; }
    public DataTemplate? SummaryTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        object content = ((TabInfo)item).Content;

        if (content is FileSortingVM)
            return FileSortingTemplate!;
        if (content is CategoriesVM)
            return CategoriesTemplate!;
        if (content is AssociationsVM)
            return AssociationsTemplate!;
        if (content is SummaryVM)
            return SummaryTemplate!;

        throw new InvalidOperationException();
    }
}