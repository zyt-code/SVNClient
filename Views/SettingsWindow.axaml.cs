using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Svns.Services;
using Svns.Utils;
using Svns.ViewModels;

namespace Svns.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettingsService _settingsService;
    private string _currentTheme = "System";

    public SettingsWindow()
    {
        InitializeComponent();
        _settingsService = new AppSettingsService();

        Opened += OnWindowOpened;
    }

    private async void OnWindowOpened(object? sender, System.EventArgs e)
    {
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();
        _currentTheme = settings.Theme;

        // Update ViewModel
        if (DataContext is SettingsViewModel vm)
        {
            vm.SvnPath = settings.SvnPath;
            vm.DefaultRepositoryUrl = settings.DefaultRepositoryUrl;
        }

        UpdateThemeButtonStates();
    }

    private void UpdateThemeButtonStates()
    {
        var accentBrush = this.FindResource("AccentBrush") as IBrush ?? Brushes.Blue;
        var transparentBrush = Brushes.Transparent;

        LightThemeButton.BorderBrush = _currentTheme == "Light" ? accentBrush : transparentBrush;
        DarkThemeButton.BorderBrush = _currentTheme == "Dark" ? accentBrush : transparentBrush;
        SystemThemeButton.BorderBrush = _currentTheme == "System" ? accentBrush : transparentBrush;
    }

    private async void OnLightThemeClick(object? sender, RoutedEventArgs e)
    {
        if (App.Current != null)
        {
            App.Current.RequestedThemeVariant = ThemeVariant.Light;
            _currentTheme = "Light";
            UpdateThemeButtonStates();
            await _settingsService.SaveThemeAsync("Light");
        }
    }

    private async void OnDarkThemeClick(object? sender, RoutedEventArgs e)
    {
        if (App.Current != null)
        {
            App.Current.RequestedThemeVariant = ThemeVariant.Dark;
            _currentTheme = "Dark";
            UpdateThemeButtonStates();
            await _settingsService.SaveThemeAsync("Dark");
        }
    }

    private async void OnSystemThemeClick(object? sender, RoutedEventArgs e)
    {
        if (App.Current != null)
        {
            App.Current.RequestedThemeVariant = ThemeVariant.Default;
            _currentTheme = "System";
            UpdateThemeButtonStates();
            await _settingsService.SaveThemeAsync("System");
        }
    }

    private async void OnBrowseSvnPathClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return;

        // Platform-specific file patterns for SVN executable
        var executablePatterns = ProcessHelper.IsWindows()
            ? new[] { "*.exe", "svn.exe" }
            : ProcessHelper.IsMacOS()
                ? new[] { "svn" }
                : new[] { "svn", "svn.bin" };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select SVN Executable",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("SVN Executable")
                {
                    Patterns = executablePatterns
                },
                new FilePickerFileType("All Files")
                {
                    Patterns = new[] { "*" }
                }
            }
        });

        if (files.Count > 0)
        {
            var path = files[0].Path.LocalPath;
            if (DataContext is SettingsViewModel vm)
            {
                vm.SvnPath = path;
            }
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
