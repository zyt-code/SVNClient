using Avalonia.Data.Converters;
using System.Globalization;
using System;

namespace Svns.Converters;

/// <summary>
/// Converts integer to string
/// </summary>
public class IntToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue.ToString();
        }

        if (value is long longValue)
        {
            return longValue.ToString();
        }

        return "0";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string strValue && int.TryParse(strValue, out var result))
        {
            return result;
        }

        return 0;
    }
}

/// <summary>
/// Converts integer to pluralized string (e.g., "1 file", "2 files")
/// </summary>
public class IntToPluralStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int count)
            count = 0;

        var singularForm = parameter?.ToString() ?? "item";
        var pluralForm = singularForm + "s";

        return count == 1 ? $"{count} {singularForm}" : $"{count} {pluralForm}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
