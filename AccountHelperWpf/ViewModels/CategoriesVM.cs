﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class CategoriesVM : BaseNotifyProperty
{
    private readonly IViewResolver viewResolver;
    private readonly ReadOnlyObservableCollection<Category> collection;
    private readonly ObservableCollection<Category> externalCategories;

    public ObservableCollection<Category> Categories { get; }

    public DelegateCommand RemoveCommand { get; }
    public DelegateCommand MoveUpCommand { get; }
    public DelegateCommand MoveDownCommand { get; }

    private int selectedIndex;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (SetProperty(ref selectedIndex, value))
                UpdateContextMenuValidity();
        }
    }

    private Category? selectedItem;

    public Category? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }

    public event Action<Category>? OnCategoryRemoving;
    public event Action? OnCategoryRemoved;
    public event Action? CategoryOrListChanged;

    public CategoriesVM(ObservableCollection<Category> loadedCategories, IViewResolver viewResolver)
    {
        this.viewResolver = viewResolver;
        externalCategories = loadedCategories;
        collection = new ReadOnlyObservableCollection<Category>(externalCategories);
        Categories = new ObservableCollection<Category>(externalCategories);
        RemoveCommand = new DelegateCommand(RemoveCategory);
        MoveUpCommand = new DelegateCommand(MoveUp);
        MoveDownCommand = new DelegateCommand(MoveDown);

        Categories.CollectionChanged += CategoriesCollectionChanged;
        foreach (Category? categoryViewModel in Categories)
        {
            categoryViewModel.PropertyChanged += CategoryChanged;
        }
        UpdateContextMenuValidity();
        CheckOrderExternal();
    }

    private void UpdateContextMenuValidity()
    {
        RemoveCommand.IsEnabled = SelectedItem is { IsDefault: false };
        MoveUpCommand.IsEnabled = SelectedIndex > 1;
        MoveDownCommand.IsEnabled = SelectedIndex > 0 && SelectedIndex < Categories.Count - 1;
    }

    private void RemoveCategory()
    {
        if (viewResolver.ShowQuestion("Are you sure? All saved associations for this category will be removed", MessageBoxButton.YesNo) == MessageBoxResult.No)
            return;
        Category categoryToRemove = Categories[SelectedIndex];
        OnCategoryRemoving?.Invoke(categoryToRemove);
        Categories.RemoveAt(SelectedIndex);
        OnCategoryRemoved?.Invoke();
    }

    private void MoveUp() => Categories.Move(SelectedIndex, SelectedIndex - 1);

    private void MoveDown() => Categories.Move(SelectedIndex, SelectedIndex + 1);

    private void CategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems!= null)
                    foreach (Category categoryViewModel in e.NewItems)
                    {
                        categoryViewModel.PropertyChanged += CategoryChanged;
                        externalCategories.Add(categoryViewModel);
                    }
                if (e.OldItems != null)
                    foreach (Category categoryViewModel in e.OldItems)
                    {
                        categoryViewModel.PropertyChanged -= CategoryChanged;
                        externalCategories.Remove(categoryViewModel);
                    }
                CheckOrderExternal();
                break;
            case NotifyCollectionChangedAction.Reset:
                throw new NotImplementedException();
            case NotifyCollectionChangedAction.Move:
                externalCategories.Move(e.OldStartingIndex - 1, e.NewStartingIndex - 1);
                break;
        }
        Notify();
        // update is required - selected changed not always cover all cases
        UpdateContextMenuValidity();
    }

    private void CheckOrderExternal()
    {
        int index = externalCategories.IndexOf(Category.Default);
        if (index != externalCategories.Count - 1)
        {
            externalCategories.Move(index, externalCategories.Count - 1);
        }
    }

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e) => Notify();

    private void Notify() => CategoryOrListChanged?.Invoke();

    public ReadOnlyObservableCollection<Category> GetCategories() => collection;
}