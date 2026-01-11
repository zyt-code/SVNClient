using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Svns.Models;

namespace Svns.Converters;

/// <summary>
/// Converts MergeAcceptType to a human-readable string
/// </summary>
public class MergeAcceptTypeConverter : IValueConverter
{
    public static readonly MergeAcceptTypeConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MergeAcceptType acceptType)
        {
            return acceptType.GetDescription();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
