namespace Svns.Services.Localization;

/// <summary>
/// Static helper class for accessing localized strings in code
/// </summary>
public static class Localize
{
    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    public static string Get(string key) => LocalizationService.Instance.GetString(key);

    /// <summary>
    /// Gets a localized string with format parameters
    /// </summary>
    public static string Get(string key, params object[] args) => LocalizationService.Instance.GetString(key, args);

    /// <summary>
    /// Shorthand for Get method
    /// </summary>
    public static string T(string key) => Get(key);

    /// <summary>
    /// Shorthand for Get method with format parameters
    /// </summary>
    public static string T(string key, params object[] args) => Get(key, args);
}
