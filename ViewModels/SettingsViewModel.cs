using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services;
using Svns.Services.Localization;

namespace Svns.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppSettingsService _settingsService;

    [ObservableProperty]
    private string _svnPath = "svn";

    [ObservableProperty]
    private string _defaultRepositoryUrl = string.Empty;

    [ObservableProperty]
    private bool _autoRefresh = true;

    [ObservableProperty]
    private int _autoRefreshInterval = 30; // seconds

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private bool _isLightThemeSelected;

    [ObservableProperty]
    private bool _isDarkThemeSelected;

    [ObservableProperty]
    private bool _isSystemThemeSelected = true;

    [ObservableProperty]
    private string _selectedLanguage = "";

    [ObservableProperty]
    private bool _languageChangePending;

    public List<LanguageItem> AvailableLanguages { get; } = new()
    {
        new LanguageItem("", "System Default", "跟随系统"),
        new LanguageItem("en-US", "English", "English"),
        new LanguageItem("zh-CN", "简体中文", "简体中文")
    };

    public class LanguageItem
    {
        public string Code { get; }
        public string DisplayName { get; }
        public string NativeName { get; }

        public LanguageItem(string code, string displayName, string nativeName)
        {
            Code = code;
            DisplayName = displayName;
            NativeName = nativeName;
        }

        public override string ToString() => string.IsNullOrEmpty(Code) ? DisplayName : $"{NativeName} ({DisplayName})";
    }

    public SettingsViewModel()
    {
        _settingsService = new AppSettingsService();
    }

    public SettingsViewModel(AppSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Loads settings from storage
    /// </summary>
    public async Task LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.LoadSettingsAsync();

        SvnPath = settings.SvnPath;
        DefaultRepositoryUrl = settings.DefaultRepositoryUrl;
        SelectedTheme = settings.Theme;
        SelectedLanguage = settings.Language;
        LanguageChangePending = false;

        UpdateThemeSelection(settings.Theme);
    }

    /// <summary>
    /// Updates the theme selection UI state
    /// </summary>
    private void UpdateThemeSelection(string theme)
    {
        IsLightThemeSelected = theme == "Light";
        IsDarkThemeSelected = theme == "Dark";
        IsSystemThemeSelected = theme == "System";
    }

    /// <summary>
    /// Selects the light theme
    /// </summary>
    [RelayCommand]
    private async Task SelectLightThemeAsync()
    {
        SelectedTheme = "Light";
        UpdateThemeSelection("Light");
        await _settingsService.SaveThemeAsync("Light");
    }

    /// <summary>
    /// Selects the dark theme
    /// </summary>
    [RelayCommand]
    private async Task SelectDarkThemeAsync()
    {
        SelectedTheme = "Dark";
        UpdateThemeSelection("Dark");
        await _settingsService.SaveThemeAsync("Dark");
    }

    /// <summary>
    /// Selects the system theme
    /// </summary>
    [RelayCommand]
    private async Task SelectSystemThemeAsync()
    {
        SelectedTheme = "System";
        UpdateThemeSelection("System");
        await _settingsService.SaveThemeAsync("System");
    }

    /// <summary>
    /// Saves the SVN path
    /// </summary>
    [RelayCommand]
    private async Task SaveSvnPathAsync()
    {
        await _settingsService.SaveSvnPathAsync(SvnPath);
    }

    /// <summary>
    /// Saves the default repository URL
    /// </summary>
    [RelayCommand]
    private async Task SaveDefaultRepositoryUrlAsync()
    {
        await _settingsService.SaveDefaultRepositoryUrlAsync(DefaultRepositoryUrl);
    }

    /// <summary>
    /// Called when SVN path changes
    /// </summary>
    partial void OnSvnPathChanged(string value)
    {
        // Auto-save when path changes
        _ = _settingsService.SaveSvnPathAsync(value);
    }

    /// <summary>
    /// Called when default repository URL changes
    /// </summary>
    partial void OnDefaultRepositoryUrlChanged(string value)
    {
        // Auto-save when URL changes
        _ = _settingsService.SaveDefaultRepositoryUrlAsync(value);
    }

    /// <summary>
    /// Called when language selection changes
    /// </summary>
    partial void OnSelectedLanguageChanged(string value)
    {
        // Save the language setting
        _ = SaveLanguageAsync(value);
    }

    /// <summary>
    /// Saves the language preference
    /// </summary>
    private async Task SaveLanguageAsync(string languageCode)
    {
        var currentLanguage = await _settingsService.GetLanguageAsync();

        await _settingsService.SaveLanguageAsync(languageCode);

        // Apply the language change immediately
        if (string.IsNullOrEmpty(languageCode))
        {
            LocalizationService.Instance.SetCulture(CultureInfo.CurrentUICulture);
        }
        else
        {
            LocalizationService.Instance.SetCulture(languageCode);
        }

        // Mark that a language change is pending (restart recommended for full effect)
        if (currentLanguage != languageCode)
        {
            LanguageChangePending = true;
        }
    }
}
