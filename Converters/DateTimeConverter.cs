using Avalonia.Data.Converters;
using System.Globalization;
using System;
using Svns.Services.Localization;

namespace Svns.Converters;

/// <summary>
/// Converts DateTime to display string
/// </summary>
public class DateTimeConverter : IValueConverter
{
    public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return LocalizationService.Instance.GetString("RelativeTime.NotAvailable");
            }

            var format = parameter?.ToString() ?? Format;
            return dateTime.ToString(format, culture);
        }

        return LocalizationService.Instance.GetString("RelativeTime.NotAvailable");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string strValue && DateTime.TryParse(strValue, culture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        return DateTime.MinValue;
    }
}

/// <summary>
/// Converts DateTime to relative time string (e.g., "2 hours ago")
/// Uses localization service for cross-language support
/// </summary>
public class RelativeTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return LocalizationService.Instance.GetString("RelativeTime.NotAvailable");
            }

            var now = DateTime.Now;
            var span = now - dateTime;

            if (span.TotalSeconds < 60)
            {
                return LocalizationService.Instance.GetString("RelativeTime.JustNow");
            }

            if (span.TotalMinutes < 60)
            {
                var minutes = (int)span.TotalMinutes;
                var key = minutes == 1 ? "RelativeTime.MinuteAgo" : "RelativeTime.MinutesAgo";
                return LocalizationService.Instance.GetString(key, minutes);
            }

            if (span.TotalHours < 24)
            {
                var hours = (int)span.TotalHours;
                var key = hours == 1 ? "RelativeTime.HourAgo" : "RelativeTime.HoursAgo";
                return LocalizationService.Instance.GetString(key, hours);
            }

            if (span.TotalDays < 30)
            {
                var days = (int)span.TotalDays;
                var key = days == 1 ? "RelativeTime.DayAgo" : "RelativeTime.DaysAgo";
                return LocalizationService.Instance.GetString(key, days);
            }

            if (span.TotalDays < 365)
            {
                var months = (int)(span.TotalDays / 30);
                var key = months == 1 ? "RelativeTime.MonthAgo" : "RelativeTime.MonthsAgo";
                return LocalizationService.Instance.GetString(key, months);
            }

            var years = (int)(span.TotalDays / 365);
            var yearKey = years == 1 ? "RelativeTime.YearAgo" : "RelativeTime.YearsAgo";
            return LocalizationService.Instance.GetString(yearKey, years);
        }

        return LocalizationService.Instance.GetString("RelativeTime.NotAvailable");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
