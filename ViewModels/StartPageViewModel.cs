using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services;

namespace Svns.ViewModels;

public partial class StartPageViewModel : ViewModelBase
{
    private readonly AppSettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<RecentProjectItem> _recentProjects = new();

    [ObservableProperty]
    private bool _hasNoRecentProjects = true;

    /// <summary>
    /// Event raised when a project should be opened
    /// </summary>
    public event EventHandler<string>? OpenProjectRequested;

    public StartPageViewModel()
    {
        _settingsService = new AppSettingsService();
        _ = LoadRecentProjectsAsync();
    }

    private async Task LoadRecentProjectsAsync()
    {
        try
        {
            var projects = await _settingsService.GetRecentProjectsAsync();

            RecentProjects.Clear();
            foreach (var project in projects)
            {
                // Only add if path still exists
                if (Directory.Exists(project.Path))
                {
                    RecentProjects.Add(new RecentProjectItem
                    {
                        Name = project.Name,
                        Path = project.Path,
                        LastOpened = project.LastOpened,
                        DisplayDate = GetRelativeDate(project.LastOpened)
                    });
                }
            }

            HasNoRecentProjects = RecentProjects.Count == 0;
        }
        catch
        {
            RecentProjects.Clear();
            HasNoRecentProjects = true;
        }
    }

    /// <summary>
    /// Opens a recent project
    /// </summary>
    [RelayCommand]
    private void OpenRecentProject(RecentProjectItem? project)
    {
        if (project == null || string.IsNullOrEmpty(project.Path))
            return;

        if (!Directory.Exists(project.Path))
        {
            // Remove invalid project
            RecentProjects.Remove(project);
            HasNoRecentProjects = RecentProjects.Count == 0;
            return;
        }

        OpenProjectRequested?.Invoke(this, project.Path);
    }

    /// <summary>
    /// Removes a project from recent list
    /// </summary>
    [RelayCommand]
    private async Task RemoveRecentProjectAsync(RecentProjectItem? project)
    {
        if (project == null)
            return;

        RecentProjects.Remove(project);
        HasNoRecentProjects = RecentProjects.Count == 0;

        // Update settings
        var settings = await _settingsService.LoadSettingsAsync();
        settings.RecentProjects.RemoveAll(p => p.Path == project.Path);
        await _settingsService.SaveSettingsAsync(settings);
    }

    /// <summary>
    /// Clears all recent projects
    /// </summary>
    [RelayCommand]
    private async Task ClearRecentProjectsAsync()
    {
        RecentProjects.Clear();
        HasNoRecentProjects = true;

        // Update settings
        var settings = await _settingsService.LoadSettingsAsync();
        settings.RecentProjects.Clear();
        await _settingsService.SaveSettingsAsync(settings);
    }

    private static string GetRelativeDate(DateTime date)
    {
        var diff = DateTime.Now - date;

        if (diff.TotalMinutes < 1)
            return "Just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} days ago";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)} weeks ago";
        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} months ago";

        return date.ToString("yyyy-MM-dd");
    }
}

public class RecentProjectItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }
    public string DisplayDate { get; set; } = string.Empty;
}
