using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.ViewUtils;

public class BackColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
            return Brushes.Black;
        return Brushes.OrangeRed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new InvalidOperationException();
}

public class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => ((ApprovementStatus)value) == ApprovementStatus.NotApproved ? Visibility.Visible : Visibility.Hidden;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new InvalidOperationException();
}