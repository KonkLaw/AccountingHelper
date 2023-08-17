﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class SummaryVM : BaseNotifyProperty
{
    private readonly ObservableCollection<CategoryDetails> collection;
    public IEnumerable<CategoryDetails> CategoriesDetails => collection;

    private string textSummary;
    public string TextSummary
    {
        get => textSummary;
        set => SetProperty(ref textSummary, value);
    }

    private decimal amount;
    public decimal Amount
    {
        get => amount;
        set => SetProperty(ref amount, value);
    }

    public ICommand UnselectCommand { get; }
    public ICommand SelectCommand { get; }
    public ICommand InvertCommand { get; }

    public SummaryVM()
    {
        collection = new ObservableCollection<CategoryDetails>();
        textSummary = string.Empty;
        UnselectCommand = new DelegateCommand(Unselect);
        SelectCommand = new DelegateCommand(Select);
        InvertCommand = new DelegateCommand(Invert);
    }

    public void Update(ICollection<CategoryDetails> newCollection)
    {
        if (collection.All(c => c.IsSelected))
        {
            foreach (CategoryDetails categoryDetails in newCollection)
                categoryDetails.IsSelected = true;
        }
        else if (collection.All(c => !c.IsSelected))
        {
            foreach (CategoryDetails categoryDetails in newCollection)
                categoryDetails.IsSelected = false;
        }
        else
        {
            foreach (CategoryDetails oldItem in collection)
            {
                CategoryDetails? details = newCollection.FirstOrDefault(c => c.Name == oldItem.Name);
                if (details != null)
                    details.IsSelected = oldItem.IsSelected;
            }
        }
        collection.Clear();
        foreach (CategoryDetails categoryDetails in newCollection)
        {
            collection.Add(categoryDetails);
            categoryDetails.PropertyChanged += CategoryDetailsChanged;
        }
        UpdateAmount();

        decimal sum = 0;
        StringBuilder result = new();
        foreach (CategoryDetails categoryDetails in newCollection)
        {
            result.Append('#');
            result.Append(categoryDetails.Name);
            result.Append(' ');
            result.Append(categoryDetails.Amount);
            if (!string.IsNullOrEmpty(categoryDetails.Description))
            {
                result.Append(" (");
                result.Append(categoryDetails.Description);
                result.Append(')');
            }
            sum += categoryDetails.Amount;
            result.AppendLine();
        }
        result.AppendLine($"*** Total = {sum}");
        TextSummary = result.ToString();
    }

    private void CategoryDetailsChanged(object? sender, PropertyChangedEventArgs e)
        => UpdateAmount();

    private void UpdateAmount()
        => Amount = CategoriesDetails.Where(c => c.IsSelected).Sum(c => c.Amount);

    private void Unselect()
    {
        foreach (CategoryDetails categoryDetails in collection)
            categoryDetails.IsSelected = false;
    }

    private void Select()
    {
        foreach (CategoryDetails categoryDetails in collection)
            categoryDetails.IsSelected = true;
    }

    private void Invert()
    {
        foreach (CategoryDetails categoryDetails in collection)
            categoryDetails.IsSelected = !categoryDetails.IsSelected;
    }
}