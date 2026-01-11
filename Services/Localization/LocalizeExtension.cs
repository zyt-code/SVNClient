using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Svns.Services.Localization;

/// <summary>
/// XAML markup extension for localized strings
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new LocalizedString(Key);
    }
}

/// <summary>
/// Observable localized string that updates when culture changes
/// </summary>
public class LocalizedString : IObservable<string>
{
    private readonly string _key;

    public LocalizedString(string key)
    {
        _key = key;
    }

    public IDisposable Subscribe(IObserver<string> observer)
    {
        // Send initial value
        observer.OnNext(LocalizationService.Instance.GetString(_key));

        // Subscribe to culture changes
        void OnCultureChanged(object? sender, System.Globalization.CultureInfo culture)
        {
            observer.OnNext(LocalizationService.Instance.GetString(_key));
        }

        LocalizationService.Instance.CultureChanged += OnCultureChanged;

        return new CultureChangeUnsubscriber(() =>
        {
            LocalizationService.Instance.CultureChanged -= OnCultureChanged;
        });
    }

    private class CultureChangeUnsubscriber : IDisposable
    {
        private readonly Action _unsubscribe;

        public CultureChangeUnsubscriber(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe();
        }
    }

    public override string ToString()
    {
        return LocalizationService.Instance.GetString(_key);
    }

    public static implicit operator string(LocalizedString ls)
    {
        return ls.ToString();
    }
}
