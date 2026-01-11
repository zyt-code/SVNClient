using Avalonia.Data.Converters;
using Avalonia.Media;
using Svns.Constants;
using Svns.Models;
using System.Globalization;
using System;

namespace Svns.Converters;

/// <summary>
/// Converts SVN status to brush color
/// </summary>
public class SvnStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SvnStatusType status)
        {
            var colorHex = status switch
            {
                SvnStatusType.Normal => SvnStatusColors.Normal,
                SvnStatusType.Modified => SvnStatusColors.Modified,
                SvnStatusType.Added => SvnStatusColors.Added,
                SvnStatusType.Deleted => SvnStatusColors.Deleted,
                SvnStatusType.Conflicted => SvnStatusColors.Conflicted,
                SvnStatusType.Ignored => SvnStatusColors.Ignored,
                SvnStatusType.Unversioned => SvnStatusColors.Unversioned,
                SvnStatusType.Missing => SvnStatusColors.Missing,
                SvnStatusType.Replaced => SvnStatusColors.Replaced,
                SvnStatusType.External => SvnStatusColors.External,
                SvnStatusType.Merged => SvnStatusColors.Merged,
                _ => "#000000"
            };

            return Color.TryParse(colorHex, out var color) ? new SolidColorBrush(color) : Brushes.Black;
        }

        return Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
