﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.Parsing;
using AccountHelperWpf.ViewUtils;
using Microsoft.Win32;

namespace AccountHelperWpf.ViewModels;

class MainWindowModel : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    private readonly CategoriesVM categoriesVM;
    private readonly SaveController saveController;

    private readonly AssociationStorage associationStorage;
    private readonly Dictionary<string, FileSortingViewModel> filesVm = new ();

    public ICommand LoadOperationFileCommand { get; }
    public ICommand SaveAssociation { get; }
    public ICommand WindowClosing { get; }

    public ObservableCollection<TabInfo> Tabs { get; } = new ();

    public MainWindowModel(IViewResolver viewResolver, InitData initData)
    {
        this.viewResolver = viewResolver;
        saveController = new SaveController(viewResolver, initData);

        associationStorage = new AssociationStorage(initData.Associations, initData.ExcludedOperations, saveController);
        InitCategories(out categoriesVM, Tabs, initData, viewResolver);
        InitAssociations(Tabs, associationStorage);

        LoadOperationFileCommand = new DelegateCommand(LoadOperationFile);
        SaveAssociation = new DelegateCommand(saveController.Save);
        WindowClosing = new DelegateCommand<CancelEventArgs>(WindowClosingHandler);
    }

    private void LoadOperationFile()
    {
        OpenFileDialog fileDialog = new() { Filter = "csv|*.csv" };
        bool? dialogResult = fileDialog.ShowDialog();
        if (dialogResult.HasValue && dialogResult.Value)
        {
            string fullPath = fileDialog.FileName;
            if (filesVm.ContainsKey(fullPath))
            {
                viewResolver.ShowWarning("File already added");
                return;
            }
            AccountFile? accountFile = ParserChooser.ParseFile(fullPath, viewResolver);
            if (accountFile == null)
            {
                viewResolver.ShowWarning("Sorry, the fle wasn't recognized as any known bank report.");
                return;
            }
            FileSortingViewModel fileSortingViewModel = new (accountFile, categoriesVM, RemoveHandler, associationStorage, saveController);
            Tabs.Insert(Tabs.Count - 2, fileSortingViewModel.GetTabItem());
            filesVm.Add(fullPath, fileSortingViewModel);
        }
    }

    private void WindowClosingHandler(CancelEventArgs? arg) => arg!.Cancel = !saveController.RequestForClose();

    private void RemoveHandler(object viewModel)
    {
        if (viewResolver.ShowYesNoQuestion("Are you sure you want to remove current file from sorting?"))
        {
            TabInfo tabToRemove = Tabs.First(tab => tab.Content == viewModel);
            Tabs.Remove(tabToRemove);
            string fileToDell = filesVm.First(p => p.Value == viewModel).Key;
            filesVm.Remove(fileToDell);
        }
    }

    private static void InitCategories(
        out CategoriesVM categoriesVM, ObservableCollection<TabInfo> tabs, InitData initData, IViewResolver viewResolver)
    {
        categoriesVM = new CategoriesVM(initData.Categories, viewResolver);
        tabs.Add(new TabInfo("Categories", categoriesVM) { IsSorted = true });
    }

    private static void InitAssociations(ObservableCollection<TabInfo> tabs, AssociationStorage storage) 
    {
        var associationVM = new AssociationsVM(storage);
        tabs.Add(new TabInfo("Associations", associationVM) { IsSorted = true });
    }
}