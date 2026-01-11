using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Svns.Services;

/// <summary>
/// Application-level settings service
/// </summary>
public class AppSettingsService
{
    private const string SettingsFileName = "svns-settings.json";
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Svns",
        SettingsFileName
    );

    public class AppSettings
    {
        public string LastWorkingCopy { get; set; } = string.Empty;
        public DateTime LastOpened { get; set; } = DateTime.Now;
        public WindowSettings Window { get; set; } = new();

        // Theme settings
        public string Theme { get; set; } = "System"; // "Light", "Dark", "System"

        // Language settings
        public string Language { get; set; } = ""; // Empty means system default, "en-US", "zh-CN"

        // SVN settings
        public string SvnPath { get; set; } = "svn";
        public string DefaultRepositoryUrl { get; set; } = string.Empty;

        // Recent projects
        public List<RecentProject> RecentProjects { get; set; } = new();
    }

    public class RecentProject
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime LastOpened { get; set; } = DateTime.Now;
    }

    public class WindowSettings
    {
        public int Width { get; set; } = 1200;
        public int Height { get; set; } = 700;
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsMaximized { get; set; }
    }

    /// <summary>
    /// Loads application settings
    /// </summary>
    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = await File.ReadAllTextAsync(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return settings ?? new AppSettings();
            }
        }
        catch
        {
            // Ignore errors, return default settings
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves application settings
    /// </summary>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            settings.LastOpened = DateTime.Now;
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(SettingsFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    /// <summary>
    /// Saves the last working copy path
    /// </summary>
    public async Task SaveLastWorkingCopyAsync(string workingCopyPath)
    {
        var settings = await LoadSettingsAsync();
        settings.LastWorkingCopy = workingCopyPath;
        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the last working copy path
    /// </summary>
    public async Task<string> GetLastWorkingCopyAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.LastWorkingCopy;
    }

    /// <summary>
    /// Saves the theme preference
    /// </summary>
    public async Task SaveThemeAsync(string theme)
    {
        var settings = await LoadSettingsAsync();
        settings.Theme = theme;
        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the theme preference
    /// </summary>
    public async Task<string> GetThemeAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.Theme;
    }

    /// <summary>
    /// Saves the language preference
    /// </summary>
    public async Task SaveLanguageAsync(string language)
    {
        var settings = await LoadSettingsAsync();
        settings.Language = language;
        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the language preference
    /// </summary>
    public async Task<string> GetLanguageAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.Language;
    }

    /// <summary>
    /// Saves the SVN path
    /// </summary>
    public async Task SaveSvnPathAsync(string svnPath)
    {
        var settings = await LoadSettingsAsync();
        settings.SvnPath = svnPath;
        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the SVN path
    /// </summary>
    public async Task<string> GetSvnPathAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.SvnPath;
    }

    /// <summary>
    /// Saves the default repository URL
    /// </summary>
    public async Task SaveDefaultRepositoryUrlAsync(string url)
    {
        var settings = await LoadSettingsAsync();
        settings.DefaultRepositoryUrl = url;
        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the default repository URL
    /// </summary>
    public async Task<string> GetDefaultRepositoryUrlAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.DefaultRepositoryUrl;
    }

    /// <summary>
    /// Adds a project to recent projects
    /// </summary>
    public async Task AddRecentProjectAsync(string name, string path)
    {
        var settings = await LoadSettingsAsync();

        // Remove existing entry if present
        settings.RecentProjects.RemoveAll(p => p.Path.Equals(path, StringComparison.OrdinalIgnoreCase));

        // Add to the beginning
        settings.RecentProjects.Insert(0, new RecentProject
        {
            Name = name,
            Path = path,
            LastOpened = DateTime.Now
        });

        // Keep only last 10 projects
        if (settings.RecentProjects.Count > 10)
        {
            settings.RecentProjects = settings.RecentProjects.Take(10).ToList();
        }

        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the recent projects
    /// </summary>
    public async Task<List<RecentProject>> GetRecentProjectsAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.RecentProjects;
    }

    /// <summary>
    /// Saves the window settings
    /// </summary>
    public async Task SaveWindowSettingsAsync(WindowSettings windowSettings)
    {
        var settings = await LoadSettingsAsync();
        settings.Window = windowSettings;
        await SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Gets the window settings
    /// </summary>
    public async Task<WindowSettings> GetWindowSettingsAsync()
    {
        var settings = await LoadSettingsAsync();
        return settings.Window;
    }
}
