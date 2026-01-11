using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Svns.ViewModels;
using Svns.Views;
using Svns.Services;
using Svns.Services.Localization;

namespace Svns;

public partial class App : Application
{
    private static readonly AppSettingsService _settingsService = new AppSettingsService();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Load saved theme and language
            _ = LoadSavedSettingsAsync();

            // Start with the StartPage, let the user choose to open a working copy
            desktop.MainWindow = new StartPageWindow
            {
                DataContext = new StartPageViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task LoadSavedSettingsAsync()
    {
        try
        {
            // Load theme
            var theme = await _settingsService.GetThemeAsync();
            ApplyTheme(theme);

            // Load language
            var language = await _settingsService.GetLanguageAsync();
            if (!string.IsNullOrEmpty(language))
            {
                LocalizationService.Instance.SetCulture(language);
            }
        }
        catch
        {
            // Ignore errors, use defaults
        }
    }

    public static void ApplyTheme(string theme)
    {
        if (Current == null) return;

        Current.RequestedThemeVariant = theme switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
