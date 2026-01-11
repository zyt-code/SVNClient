using System;
using System.Collections.Generic;
using System.Globalization;

namespace Svns.Services.Localization;

/// <summary>
/// Interface for localization service
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current culture
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets available cultures
    /// </summary>
    IReadOnlyList<CultureInfo> AvailableCultures { get; }

    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string with format parameters
    /// </summary>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Changes the current culture
    /// </summary>
    void SetCulture(CultureInfo culture);

    /// <summary>
    /// Changes the current culture by language code
    /// </summary>
    void SetCulture(string languageCode);

    /// <summary>
    /// Event fired when culture changes
    /// </summary>
    event EventHandler<CultureInfo>? CultureChanged;
}
