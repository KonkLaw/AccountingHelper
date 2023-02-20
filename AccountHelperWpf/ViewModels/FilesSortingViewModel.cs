﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AccountHelperWpf.Common;
using AccountHelperWpf.Parsing;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class FilesSortingViewModel
{
    private readonly IViewResolver viewResolver;
    private readonly CategoriesViewModel categoriesViewModel;
    private readonly List<(string fullPath, FileSortingViewModel vm)> filesVm = new ();

    public ObservableCollection<TabInfo> Tabs { get; } = new ();
    public ICommand LoadFile { get; }

    public FilesSortingViewModel(IViewResolver viewResolver, List<CategoryViewModel> categories)
    {
        this.viewResolver = viewResolver;
        LoadFile = new DelegateCommand(LoadFileHandler);
        categoriesViewModel = new CategoriesViewModel(categories);
        Tabs.Add(new TabInfo("Categories", categoriesViewModel) { IsSorted = true });
    }

    private void LoadFileHandler()
    {
        OpenFileDialog fileDialog = new() { Filter = "csv|*.csv" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string fullPath = fileDialog.FileName;
            if (filesVm.Any(f => f.fullPath == fullPath))
            {
                viewResolver.ShowWarning("File already added");
                return;
            }
            AccountFile accountFile = ParserChooser.ParseFile(fullPath, viewResolver);
            FileSortingViewModel fileSortingViewModel = new (accountFile, categoriesViewModel, RemoveHandler);
            Tabs.Add(fileSortingViewModel.GetTabItem());
            filesVm.Add((fullPath, fileSortingViewModel));
        }
    }

    private void RemoveHandler(object viewModel)
    {
        if (viewResolver.ShowYesNoDialog("Are you sure you want to remove current file from sorting?"))
            Tabs.Remove(Tabs.First(tab => ((FrameworkElement)tab.Content).DataContext == viewModel));
    }
}