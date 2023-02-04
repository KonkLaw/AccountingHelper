﻿using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AccountHelperWpf.Common;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Views;

public class DataGridExt : DataGrid
{
    public static readonly DependencyProperty SelectedItemsListProperty =
        DependencyProperty.Register (nameof(SelectedItemsList), typeof (IList), typeof (DataGridExt), new PropertyMetadata (null));

    public IList SelectedItemsList
    {
        get => (IList)GetValue(SelectedItemsListProperty);
        set => SetValue(SelectedItemsListProperty, value);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        SelectedItemsList = SelectedItems;
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        
        List<string> wellKnownProperties = new ();
        foreach (DataGridColumn dataGridColumn in Columns)
        {
            DataGridBoundColumn? dataGridBoundColumn = dataGridColumn as DataGridBoundColumn;
            if (dataGridBoundColumn == null)
                continue;
            wellKnownProperties.Add(((Binding)dataGridBoundColumn.Binding).Path.Path);
        }

        if (ItemsSource == null)
            return;

        if (!ItemsSource.GetEnumerator().MoveNext())
            return;

        OperationViewModel firstItem = (OperationViewModel)((IEnumerable<object>)ItemsSource).First();
        Type operationType = firstItem.Operation.GetType();
        string prefix = nameof(OperationViewModel.Operation) + ".";

        PropertyInfo[] properties = operationType.GetProperties();
        foreach (PropertyInfo propertyInfo in properties)
        {
            bool shouldAdd = true;
            foreach (string wellKnownProperty in wellKnownProperties)
            {
                if (wellKnownProperty.EndsWith(propertyInfo.Name))
                {
                    shouldAdd = false;
                    break;
                }
            }

            if (shouldAdd)
            {
                WidthAttribute? widthAttribute = propertyInfo.GetCustomAttribute<WidthAttribute>();
                StringFormatAttribute? formatAttribute = propertyInfo.GetCustomAttribute<StringFormatAttribute>();

                DataGridTextColumn column = new ()
                {
                    IsReadOnly = true,
                    Header = propertyInfo.Name,
                    Binding = new Binding(prefix + propertyInfo.Name)
                };
                if (widthAttribute != null)
                    column.Width = widthAttribute.Width;
                if (formatAttribute != null)
                    column.Binding.StringFormat = formatAttribute.Format;

                Columns.Add(column);
            }
        }
    }
}