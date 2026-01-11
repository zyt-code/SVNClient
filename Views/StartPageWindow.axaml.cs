using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Svns.Services;

namespace Svns.Views;

public partial class StartPageWindow : Window
{
    private readonly AppSettingsService _settingsService;

    public StartPageWindow()
    {
        InitializeComponent();
        _settingsService = new AppSettingsService();

        // Subscribe to ViewModel events after DataContext is set
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ViewModels.StartPageViewModel viewModel)
        {
            viewModel.OpenProjectRequested += OnOpenProjectRequested;
        }
    }

    private void OnOpenProjectRequested(object? sender, string path)
    {
        OpenMainWindow(path);
        Close();
    }

    /// <summary>
    /// Opens the working copy browser dialog
    /// </summary>
    private async void OnOpenWorkingCopyClick(object? sender, RoutedEventArgs e)
    {
        var folderPath = await ShowFolderPickerAsync();
        if (!string.IsNullOrEmpty(folderPath))
        {
            OpenMainWindow(folderPath);
            Close();
        }
    }

    /// <summary>
    /// Opens the checkout dialog
    /// </summary>
    private void OnCheckoutClick(object? sender, RoutedEventArgs e)
    {
        // TODO: Show checkout dialog
        // For now, show a message
        var dialog = new Window
        {
            Title = "Coming Soon",
            Width = 300,
            Height =150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel
        {
            Spacing =16,
            Margin = new Thickness(24),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        panel.Children.Add(new TextBlock
        {
            Text = "Checkout Dialog",
            FontWeight = FontWeight.SemiBold,
            TextAlignment = TextAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = "This feature is coming soon!\nPlease use 'svn checkout' from command line first.",
            TextAlignment = TextAlignment.Center,
            Foreground = Brush.Parse("#888")
        });

        var button = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Avalonia.Thickness(24,8)
        };
        button.Click += (s, e) => dialog.Close();
        panel.Children.Add(button);

        dialog.Content = panel;
        dialog.ShowDialog(this);
    }

    /// <summary>
    /// Opens the Settings window
    /// </summary>
    private void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            DataContext = new ViewModels.SettingsViewModel()
        };
        settingsWindow.ShowDialog(this);
    }

    /// <summary>
    /// Shows the folder picker dialog
    /// </summary>
    private async System.Threading.Tasks.Task<string> ShowFolderPickerAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return string.Empty;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                Title = "Select SVN Working Copy Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                return folders[0].Path.LocalPath;
            }
        }
        catch
        {
            // Ignore errors
        }

        return string.Empty;
    }

    /// <summary>
    /// Opens the main window with the specified working copy
    /// </summary>
    private void OpenMainWindow(string workingCopyPath)
    {
        var mainWindow = new MainWindow
        {
            DataContext = new ViewModels.MainWindowViewModel()
        };

        // Load the working copy after the window is opened
        mainWindow.Opened += async (s, e) =>
        {
            if (mainWindow.DataContext is ViewModels.MainWindowViewModel vm)
            {
                vm.SetWindow(mainWindow);
                await vm.LoadWorkingCopyAsync(workingCopyPath);

                // Save to recent projects
                var projectName = Path.GetFileName(workingCopyPath);
                if (string.IsNullOrEmpty(projectName))
                {
                    projectName = workingCopyPath;
                }

                await _settingsService.AddRecentProjectAsync(projectName, workingCopyPath);
            }
        };

        mainWindow.Show();
    }
}
