using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Svns.Converters;

/// <summary>
/// Converts boolean to double width (true = specified width, false = 0)
/// Usage: ConverterParameter can specify the width when true (e.g., "300" or "8")
/// </summary>
public class BoolToGridLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            // Return the width specified in parameter, or default to 300
            var widthStr = parameter?.ToString();
            if (string.IsNullOrEmpty(widthStr))
            {
                return 300.0;
            }

            // Try to parse as pixel value
            if (double.TryParse(widthStr, out var pixels))
            {
                return pixels;
            }
        }

        // Return 0 when false or invalid
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
