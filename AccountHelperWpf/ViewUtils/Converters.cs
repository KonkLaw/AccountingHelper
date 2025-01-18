using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AccountHelperWpf.ViewUtils;

public class VisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (bool)value! ? Visibility.Visible : Visibility.Hidden;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new InvalidOperationException();
}

public abstract class BoolToColorConverter : DependencyObject, IValueConverter
{
    public static DependencyProperty PositiveAmountColorProperty = DependencyProperty.Register(
        nameof(PositiveAmountColor), typeof(Brush), typeof(BoolToColorConverter), new PropertyMetadata(Brushes.Red));

    public Brush PositiveAmountColor
    {
        get => (Brush)GetValue(PositiveAmountColorProperty);
        set => SetValue(PositiveAmountColorProperty, value);
    }

    public static DependencyProperty NegativeAmountColorProperty = DependencyProperty.Register(
        nameof(NegativeAmountColor), typeof(Brush), typeof(BoolToColorConverter), new PropertyMetadata(Brushes.Blue));

    public Brush NegativeAmountColor
    {
        get => (Brush)GetValue(NegativeAmountColorProperty);
        set => SetValue(NegativeAmountColorProperty, value);

    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => GetIsTrue(value) ? PositiveAmountColor : NegativeAmountColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new InvalidOperationException();

    public abstract bool GetIsTrue(object? value);
}

public class AmountToColorConverter : BoolToColorConverter
{
    public override bool GetIsTrue(object? value) => (decimal)value! > 0;
}

public class HasValueBackColorConverter : BoolToColorConverter
{
    public override bool GetIsTrue(object? value) => ((decimal?)value).HasValue;
}

public class MultibindingConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.Clone();

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}