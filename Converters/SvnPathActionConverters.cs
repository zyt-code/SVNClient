using Avalonia.Data.Converters;
using Svns.Models;
using System;
using System.Globalization;
using Avalonia.Media;
using Avalonia;

namespace Svns.Converters;

/// <summary>
/// Converts SvnPathAction enum to display character (A, M, D, R)
/// </summary>
public class SvnPathActionToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SvnPathAction action)
        {
            return action switch
            {
                SvnPathAction.Added => "A",
                SvnPathAction.Deleted => "D",
                SvnPathAction.Modified => "M",
                SvnPathAction.Replaced => "R",
                _ => ""
            };
        }
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts SvnPathAction enum to color
/// </summary>
public class SvnPathActionToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SvnPathAction action)
        {
            return action switch
            {
                SvnPathAction.Added => Color.Parse("#10B981"),  // Green
                SvnPathAction.Deleted => Color.Parse("#EF4444"), // Red
                SvnPathAction.Modified => Color.Parse("#F59E0B"), // Orange
                SvnPathAction.Replaced => Color.Parse("#3B82F6"), // Blue
                _ => Color.Parse("#9CA3AF")                       // Gray
            };
        }
        return Color.Parse("#9CA3AF");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
