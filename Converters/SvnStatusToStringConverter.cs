using Avalonia.Data.Converters;
using Svns.Models;
using System.Globalization;
using System;

namespace Svns.Converters;

/// <summary>
/// Converts SVN status to display string
/// </summary>
public class SvnStatusToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SvnStatusType status)
        {
            return status switch
            {
                SvnStatusType.Normal => "Normal",
                SvnStatusType.Modified => "Modified",
                SvnStatusType.Added => "Added",
                SvnStatusType.Deleted => "Deleted",
                SvnStatusType.Conflicted => "Conflicted",
                SvnStatusType.Ignored => "Ignored",
                SvnStatusType.Unversioned => "Unversioned",
                SvnStatusType.Missing => "Missing",
                SvnStatusType.Replaced => "Replaced",
                SvnStatusType.External => "External",
                SvnStatusType.Merged => "Merged",
                SvnStatusType.Incomplete => "Incomplete",
                SvnStatusType.Obstructed => "Obstructed",
                _ => "Unknown"
            };
        }

        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
