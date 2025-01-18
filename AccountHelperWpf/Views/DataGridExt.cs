using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Views;

public class DataGridExt : DataGrid
{
    public static DependencyProperty ColumnDescriptionsProperty =
        DependencyProperty.Register(nameof(ColumnDescriptions), typeof(IEnumerable<ColumnDescription>), typeof(DataGridExt), new PropertyMetadata(null));

    public IEnumerable<ColumnDescription> ColumnDescriptions
    {
        get => (IEnumerable<ColumnDescription>)GetValue(ColumnDescriptionsProperty);
        set => SetValue(ColumnDescriptionsProperty, value);
    }

    public static readonly DependencyProperty SelectedItemsListProperty =
        DependencyProperty.Register (nameof(SelectedItemsList), typeof (IList), typeof (DataGridExt), new PropertyMetadata (null));

    public IList SelectedItemsList
    {
        get => (IList)GetValue(SelectedItemsListProperty);
        set => SetValue(SelectedItemsListProperty, value);
    }

    private static readonly Dictionary<object, double> ViewModelToOffset = new();

    private (HashSet<string> properties, DataGridColumn[] columns)? xamlColumns;

    static DataGridExt()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DataGridExt), new FrameworkPropertyMetadata(typeof(DataGridExt)));
    }

    public DataGridExt()
    {
        AutoGenerateColumns = false;
        

        Loaded += OnLoaded;
        Unloaded += DataGridExt_Unloaded;
    }


    public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        // Recursively look for the first child of type T
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T tChild)
            {
                return tChild;
            }
            T? result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public override void EndInit()
    {
        base.EndInit();
        var wellKnownProperties = new HashSet<string>();
        foreach (DataGridColumn dataGridColumn in Columns)
        {
            if (dataGridColumn is not DataGridBoundColumn dataGridBoundColumn)
                continue;
            wellKnownProperties.Add(((Binding)dataGridBoundColumn.Binding).Path.Path);
        }
        xamlColumns = new(wellKnownProperties, Columns.ToArray());
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

    private void CorrectColumns(
        IEnumerable? itemsSource,
        ObservableCollection<DataGridColumn> columns)
    {
        if (xamlColumns == null)
            throw new InvalidOperationException("Well known properties are not initialized");

        if (itemsSource == null)
            return;

        IEnumerator enumerator = itemsSource.GetEnumerator();
        using IDisposable? disposable = enumerator as IDisposable;

        if (!enumerator.MoveNext())
            return;

        Dictionary<string, ColumnDescription> customization = GetCustomization();

        OperationVM firstItem = (OperationVM)enumerator.Current!;
        Type operationType = firstItem.Operation.GetType();
        string prefix = nameof(OperationVM.Operation) + ".";

        PropertyInfo[] properties = operationType.GetProperties();
        columns.Clear();
        foreach (DataGridColumn column in xamlColumns.Value.columns)
            columns.Add(column);

        HashSet<string> wellKnownProperties = xamlColumns.Value.properties;
        foreach (PropertyInfo propertyInfo in properties)
        {
            string propertyPath = prefix + propertyInfo.Name;

            if (wellKnownProperties.Contains(propertyPath))
                continue;
            
            customization.TryGetValue(propertyInfo.Name, out ColumnDescription customDescription);
            if (customDescription.IsInvisible)
                continue;
            
            Style style = new (typeof(DataGridCell));
            style.Setters.Add(new Setter
            {
                Property = ToolTipService.ToolTipProperty,
                Value = new Binding(propertyPath)
            });

            DataGridTextColumn column = new()
            {
                IsReadOnly = true,
                Header = propertyInfo.Name,
                Binding = new Binding(propertyPath),
                CellStyle = style
            };

            if (customDescription.CustomWidth.HasValue)
                column.Width = customDescription.CustomWidth.Value;
            if (customDescription.CustomFormat != null)
                column.Binding.StringFormat = customDescription.CustomFormat;
            
            columns.Add(column);
        }

        // last column should take all available space, not more
        columns[^1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
    }

    public Dictionary<string, ColumnDescription> GetCustomization()
    {
        Dictionary<string, ColumnDescription> customization = ColumnDescriptions.ToDictionary(cd => cd.PropertyName, cd => cd);
        string propName = nameof(BaseOperation.TransactionDateTime);
        customization.Add(propName, new ColumnDescription(propName, null, null, true));
        return customization;
    }

    private void DataGridExt_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext == null) // for designer
            return;

        ScrollViewer? scrollViewer = FindVisualChild<ScrollViewer>(this);
        if (scrollViewer != null)
        {
            ViewModelToOffset[DataContext] = scrollViewer.VerticalOffset;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext == null) // for designer
            return;

        ScrollViewer? scrollViewer = FindVisualChild<ScrollViewer>(this);
        if (scrollViewer != null && ViewModelToOffset.TryGetValue(DataContext, out double offset))
        {
            scrollViewer.ScrollToVerticalOffset(offset);
        }
    }
}

public readonly record struct ColumnDescription(
    string PropertyName,
    double? CustomWidth,
    string? CustomFormat,
    bool IsInvisible)
{
    public ColumnDescription(string propertyName, double? customWidth, string? customFormat)
        : this(propertyName, customWidth, customFormat, false) { }
};