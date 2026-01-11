using Avalonia.Data.Converters;
using System.Globalization;
using System.IO;
using System;

namespace Svns.Converters;

/// <summary>
/// Converts file path to file name
/// </summary>
public class FilePathToFileNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            return System.IO.Path.GetFileName(path);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts file path to directory name
/// </summary>
public class FilePathToDirectoryNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(directory) ? directory : path;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Shortens file path for display
/// </summary>
public class FilePathShortenerConverter : IValueConverter
{
    public int MaxLength { get; set; } = 50;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            var maxLength = MaxLength;

            if (parameter is int paramLength)
            {
                maxLength = paramLength;
            }
            else if (parameter is string paramStr && int.TryParse(paramStr, out var parsedLength))
            {
                maxLength = parsedLength;
            }

            if (path.Length <= maxLength)
            {
                return path;
            }

            var fileName = System.IO.Path.GetFileName(path);
            var directory = System.IO.Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(directory))
            {
                var availableLength = maxLength - fileName.Length - 4; // 4 for "...\"
                if (availableLength > 10)
                {
                    return directory.Substring(0, availableLength) + "..." + Path.DirectorySeparatorChar + fileName;
                }
            }

            return path.Substring(0, maxLength - 3) + "...";
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
