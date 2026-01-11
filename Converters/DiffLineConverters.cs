using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Svns.ViewModels;

namespace Svns.Converters;

/// <summary>
/// Converts DiffLineType to background color
/// </summary>
public class DiffLineTypeToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffLineType type)
        {
            return type switch
            {
                DiffLineType.Added => new SolidColorBrush(Color.FromArgb(30, 76, 175, 80)),      // Green with alpha
                DiffLineType.Removed => new SolidColorBrush(Color.FromArgb(30, 244, 67, 54)),   // Red with alpha
                DiffLineType.Header => new SolidColorBrush(Color.FromArgb(30, 33, 150, 243)),   // Blue with alpha
                DiffLineType.FileHeader => new SolidColorBrush(Color.FromArgb(20, 156, 39, 176)), // Purple with alpha
                _ => Brushes.Transparent
            };
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts DiffLineType to foreground color
/// </summary>
public class DiffLineTypeToForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffLineType type)
        {
            return type switch
            {
                DiffLineType.Added => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Green
                DiffLineType.Removed => new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Red
                DiffLineType.Header => new SolidColorBrush(Color.FromRgb(33, 150, 243)),   // Blue
                DiffLineType.FileHeader => new SolidColorBrush(Color.FromRgb(156, 39, 176)), // Purple
                _ => new SolidColorBrush(Color.FromRgb(200, 200, 200))  // Default text color
            };
        }
        return new SolidColorBrush(Color.FromRgb(200, 200, 200));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
