﻿using System.Collections.ObjectModel;
using System.Windows.Controls;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;

namespace AccountHelperWpf.ViewModels;

class FilesSortingViewModel
{
    private readonly IViewResolver viewResolver;
    public ObservableCollection<TabItem> Tabs { get; } = new ();
    public DelegateCommand Next { get; }

    private readonly List<FileSortingViewModel> filesVm = new ();

    public FilesSortingViewModel(IViewResolver viewResolver, AccountFile[] accountFiles, List<CategoryVm> categories)
    {
        this.viewResolver = viewResolver;
        Next = new DelegateCommand(NextHandler);
        CategoriesViewModel categoriesVm = new (categories);
        Tabs.Add(viewResolver.ResolveTabItem("Categories", categoriesVm));
        foreach (AccountFile accountFile in accountFiles)
        {
            Action handler = UpdateNextButtonState;
            FileSortingViewModel fileSortingViewModel = new (accountFile, categoriesVm.GetCategories(), handler);
            filesVm.Add(fileSortingViewModel);
            Tabs.Add(viewResolver.ResolveTabItem(accountFile.Description.Name, fileSortingViewModel));
        }
        //Tabs.Add(viewResolver.ResolveTabItem("History", new HistoryViewModel()));
        UpdateNextButtonState();
    }

    private void UpdateNextButtonState() => Next.IsEnabled = filesVm.All(f => f.IsSorted);

    private void NextHandler() { }
}