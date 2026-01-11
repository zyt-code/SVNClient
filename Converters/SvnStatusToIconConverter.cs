using Avalonia.Data.Converters;
using Svns.Constants;
using Svns.Models;
using System.Globalization;
using System;

namespace Svns.Converters;

/// <summary>
/// Converts SVN status to icon string
/// </summary>
public class SvnStatusToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SvnStatusType status)
        {
            return status switch
            {
                SvnStatusType.Normal => SvnStatusIcons.Normal,
                SvnStatusType.Modified => SvnStatusIcons.Modified,
                SvnStatusType.Added => SvnStatusIcons.Added,
                SvnStatusType.Deleted => SvnStatusIcons.Deleted,
                SvnStatusType.Conflicted => SvnStatusIcons.Conflicted,
                SvnStatusType.Ignored => SvnStatusIcons.Ignored,
                SvnStatusType.Unversioned => SvnStatusIcons.Unversioned,
                SvnStatusType.Missing => SvnStatusIcons.Missing,
                SvnStatusType.Replaced => SvnStatusIcons.Replaced,
                SvnStatusType.External => SvnStatusIcons.External,
                SvnStatusType.Merged => SvnStatusIcons.Merged,
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
