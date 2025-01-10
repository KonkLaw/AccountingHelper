using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AccountHelperWpf.ViewUtils;

public class HighlightBackColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => ((decimal?)value).HasValue ? Brushes.Transparent : Brushes.IndianRed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new InvalidOperationException();
}

public class VisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (bool)value! ? Visibility.Visible : Visibility.Hidden;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new InvalidOperationException();
}

public class AmountToColorConverter : DependencyObject, IValueConverter
{
    public static DependencyProperty PositiveAmountColorProperty = DependencyProperty.Register(
        nameof(PositiveAmountColor), typeof(Brush), typeof(AmountToColorConverter), new PropertyMetadata(Brushes.Red));

    public Brush PositiveAmountColor
    {
        get => (Brush)GetValue(PositiveAmountColorProperty);
        set => SetValue(PositiveAmountColorProperty, value);
    }

    public static DependencyProperty NegativeAmountColorProperty = DependencyProperty.Register(
        nameof(NegativeAmountColor), typeof(Brush), typeof(AmountToColorConverter), new PropertyMetadata(Brushes.Blue));

    public Brush NegativeAmountColor
    {
        get => (Brush)GetValue(NegativeAmountColorProperty);
        set => SetValue(NegativeAmountColorProperty, value);

    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (decimal)value! > 0 ? PositiveAmountColor : NegativeAmountColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new InvalidOperationException();
}