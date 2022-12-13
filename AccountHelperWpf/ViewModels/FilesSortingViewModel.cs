using System.Collections.ObjectModel;
using System.Windows.Controls;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class FilesSortingViewModel
{
    public ObservableCollection<TabItem> Tabs { get; } = new ();

    public FilesSortingViewModel(IViewResolver viewResolver, AccountFile[] accountFiles)
    {
        CategoriesViewModel categoriesVm = new ();
        Tabs.Add(viewResolver.ResolveTabItem("Categories", categoriesVm));
        foreach (AccountFile accountFile in accountFiles)
        {
            Tabs.Add(viewResolver.ResolveTabItem(accountFile.Description.Name, new FileSortingViewModel(accountFile, categoriesVm.Categories)));
        }
    }
}