using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Svns.Converters;

/// <summary>
/// Multi-value converter that performs logical AND on all boolean values
/// </summary>
public class AndMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // All values must be true for the result to be true
        return values.All(v => v is bool b && b);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // ConvertBack is not supported for multi-value converters
        throw new NotImplementedException();
    }

    // Static instance for XAML access
    public static AndMultiConverter And { get; } = new();
}

public class BoolToStringConverter : IValueConverter
{
    public string TrueValue { get; set; } = "True";
    public string FalseValue { get; set; } = "False";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            // If parameter is provided in format "TrueValue;FalseValue"
            if (parameter is string param && param.Contains(';'))
            {
                var parts = param.Split(';');
                return b ? parts[0] : parts[1];
            }
            return b ? TrueValue : FalseValue;
        }
        return FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToBrushConverter : IValueConverter
{
    public IBrush TrueBrush { get; set; } = Brushes.Green;
    public IBrush FalseBrush { get; set; } = Brushes.Red;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            // If parameter is provided in format "TrueColor;FalseColor"
            if (parameter is string param && param.Contains(';'))
            {
                var parts = param.Split(';');
                var colorStr = b ? parts[0] : parts[1];
                if (Color.TryParse(colorStr, out var color))
                {
                    return new SolidColorBrush(color);
                }
            }
            return b ? TrueBrush : FalseBrush;
        }
        return FalseBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
