using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Svns.Services;
using Svns.ViewModels;
using Svns.Views.Dialogs;

namespace Svns.Views;

public partial class MainWindow : Window
{
    private readonly AppSettingsService _settingsService;
    private bool _isInitialized;

    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new AppSettingsService();

        // Subscribe to Opened event to set window reference after DataContext is assigned
        this.Opened += OnWindowOpened;
        this.Closing += OnWindowClosing;
    }

    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SetWindow(this);
        }

        // Load and apply window settings
        try
        {
            var windowSettings = await _settingsService.GetWindowSettingsAsync();

            // Apply size (with minimum bounds)
            Width = Math.Max(windowSettings.Width, 800);
            Height = Math.Max(windowSettings.Height, 500);

            // Apply position if valid (not at 0,0 which might be default)
            if (windowSettings.X > 0 || windowSettings.Y > 0)
            {
                // Ensure the window is visible on screen
                var screens = Screens;
                var validPosition = false;

                foreach (var screen in screens.All)
                {
                    var bounds = screen.WorkingArea;
                    if (windowSettings.X >= bounds.X && windowSettings.X < bounds.X + bounds.Width &&
                        windowSettings.Y >= bounds.Y && windowSettings.Y < bounds.Y + bounds.Height)
                    {
                        validPosition = true;
                        break;
                    }
                }

                if (validPosition)
                {
                    Position = new PixelPoint(windowSettings.X, windowSettings.Y);
                }
            }

            // Apply maximized state
            if (windowSettings.IsMaximized)
            {
                WindowState = WindowState.Maximized;
            }

            _isInitialized = true;
        }
        catch
        {
            // Ignore errors, use default window settings
            _isInitialized = true;
        }
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_isInitialized)
            return;

        // Save window settings
        try
        {
            var windowSettings = new AppSettingsService.WindowSettings
            {
                IsMaximized = WindowState == WindowState.Maximized
            };

            // Save normal bounds (not maximized bounds)
            if (WindowState != WindowState.Maximized)
            {
                windowSettings.Width = (int)Width;
                windowSettings.Height = (int)Height;
                windowSettings.X = Position.X;
                windowSettings.Y = Position.Y;
            }
            else
            {
                // When maximized, try to get the restore bounds
                // For now, save reasonable defaults
                var settings = await _settingsService.GetWindowSettingsAsync();
                windowSettings.Width = settings.Width > 0 ? settings.Width : 1200;
                windowSettings.Height = settings.Height > 0 ? settings.Height : 700;
                windowSettings.X = settings.X;
                windowSettings.Y = settings.Y;
            }

            await _settingsService.SaveWindowSettingsAsync(windowSettings);
        }
        catch
        {
            // Ignore save errors
        }
    }

    /// <summary>
    /// Opens the Settings window
    /// </summary>
    private void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            DataContext = new SettingsViewModel()
        };
        settingsWindow.ShowDialog(this);
    }

    /// <summary>
    /// Goes back to the welcome page
    /// </summary>
    private void OnBackToWelcomeClick(object? sender, RoutedEventArgs e)
    {
        var startPage = new StartPageWindow
        {
            DataContext = new StartPageViewModel()
        };
        startPage.Show();
        Close();
    }

    /// <summary>
    /// Opens the About dialog
    /// </summary>
    private void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog(new AboutViewModel());
        aboutDialog.ShowDialog(this);
    }
}
