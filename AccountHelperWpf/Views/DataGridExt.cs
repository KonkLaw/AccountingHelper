using System.Collections;
using System.Collections.ObjectModel;
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
        CorrectColumns(ItemsSource, Columns);
    }

    private static void CorrectColumns(IEnumerable? itemsSource, ObservableCollection<DataGridColumn> columns)
    {
        List<string> wellKnownProperties = new ();
        foreach (DataGridColumn dataGridColumn in columns)
        {
            DataGridBoundColumn? dataGridBoundColumn = dataGridColumn as DataGridBoundColumn;
            if (dataGridBoundColumn == null)
                continue;
            wellKnownProperties.Add(((Binding)dataGridBoundColumn.Binding).Path.Path);
        }

        if (itemsSource == null)
            return;
        IEnumerator enumerator = itemsSource.GetEnumerator();

        if (!enumerator.MoveNext())
            return;

        OperationViewModel firstItem = (OperationViewModel)enumerator.Current!;
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


                string propertyPath = prefix + propertyInfo.Name;

                Style style = new (typeof(DataGridCell));
                style.Setters.Add(new Setter
                {
                    Property = ToolTipService.ToolTipProperty,
                    Value = new Binding(propertyPath)
                });

                DataGridTextColumn column = new ()
                {
                    IsReadOnly = true,
                    Header = propertyInfo.Name,
                    Binding = new Binding(propertyPath),
                    CellStyle = style
                };

                if (widthAttribute != null)
                    column.Width = widthAttribute.Width;
                if (formatAttribute != null)
                    column.Binding.StringFormat = formatAttribute.Format;

                columns.Add(column);
            }
        }
    }
}