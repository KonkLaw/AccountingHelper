using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using AccountHelperWpf.BaseObjects;

namespace AccountHelperWpf.Views;

public class DataGridExt : DataGrid
{
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        List<string> wellKnownProperties = new ();
        foreach (DataGridColumn dataGridColumn in Columns)
        {
            DataGridBoundColumn? dataGridBoundColumn = dataGridColumn as DataGridBoundColumn;
            if (dataGridBoundColumn == null)
                continue;
            wellKnownProperties.Add(((Binding)dataGridBoundColumn.Binding).Path.Path);
        }

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
                DataGridTextColumn column = new ()
                {
                    IsReadOnly = true,
                    Header = propertyInfo.Name,
                    Binding = new Binding(prefix + propertyInfo.Name),
                };
                Columns.Add(column);
            }

        }
    }
}