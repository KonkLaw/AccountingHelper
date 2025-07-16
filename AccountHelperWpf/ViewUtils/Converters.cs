using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AccountHelperWpf.Models;

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

public class CategoryIsSortedConverter : DependencyObject, IMultiValueConverter
{
    public static DependencyProperty DefaultBrushProperty = DependencyProperty.Register(
        nameof(DefaultBrush), typeof(Brush), typeof(CategoryIsSortedConverter), new PropertyMetadata(Brushes.White));

    public Brush DefaultBrush
    {
        get => (Brush)GetValue(DefaultBrushProperty);
        set => SetValue(DefaultBrushProperty, value);
    }

    public static DependencyProperty HighlightedBrushProperty = DependencyProperty.Register(
        nameof(HighlightedBrush), typeof(Brush), typeof(CategoryIsSortedConverter), new PropertyMetadata(Brushes.Orange));

    public Brush HighlightedBrush
    {
        get => (Brush)GetValue(HighlightedBrushProperty);
        set => SetValue(HighlightedBrushProperty, value);
    }

    public static DependencyProperty NotSortedBrushProperty = DependencyProperty.Register(
        nameof(NotSortedBrush), typeof(Brush), typeof(CategoryIsSortedConverter), new PropertyMetadata(Brushes.Gray));

    public Brush NotSortedBrush
    {
        get => (Brush)GetValue(NotSortedBrushProperty);
        set => SetValue(NotSortedBrushProperty, value);
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[3] is not bool)
            return Brushes.Red;

        Category category = (Category)values[0];
        bool isHighlighted = (bool)values[1];
        bool isAutoMappedNotApproved = (bool)values[2];
        bool highlightNonSorted = (bool)values[3];

        if (isHighlighted)
            return HighlightedBrush;

        bool isNotSorted = category.IsDefault || isAutoMappedNotApproved;
        return isNotSorted && highlightNonSorted ? NotSortedBrush : DefaultBrush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}