using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Parsers;
using Svns.Services.Svn.Operations;

namespace Svns.ViewModels;

public partial class RepositoryBrowserViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _repositoryUrl = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RepositoryItem> _items = new();

    [ObservableProperty]
    private RepositoryItem? _selectedItem;

    [ObservableProperty]
    private string _currentPath = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _pathHistory = new();

    public bool CanGoUp => !string.IsNullOrEmpty(CurrentPath) && CurrentPath != RepositoryUrl;

    public event EventHandler<string>? ItemSelected;
    public event EventHandler? CloseRequested;

    public RepositoryBrowserViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    public async Task InitializeAsync(string repositoryUrl, CancellationToken cancellationToken = default)
    {
        RepositoryUrl = repositoryUrl.TrimEnd('/');
        CurrentPath = RepositoryUrl;
        await NavigateToAsync(CurrentPath);
    }

    [RelayCommand]
    private async Task NavigateToAsync(string path)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading...";

            var items = await _workingCopyService.ListAsync(path);

            Items.Clear();
            foreach (var item in items.OrderByDescending(i => i.IsDirectory).ThenBy(i => i.Name))
            {
                Items.Add(new RepositoryItem
                {
                    Name = item.Name.TrimEnd('/'),
                    Path = $"{path}/{item.Name.TrimEnd('/')}",
                    IsDirectory = item.IsDirectory,
                    Size = item.Size,
                    Revision = item.Revision,
                    Author = item.Author ?? "",
                    Date = item.Date
                });
            }

            CurrentPath = path;
            if (!PathHistory.Contains(path))
            {
                PathHistory.Add(path);
            }

            OnPropertyChanged(nameof(CanGoUp));
            StatusMessage = $"{Items.Count} items";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoUpAsync()
    {
        if (!CanGoUp) return;

        var lastSlash = CurrentPath.LastIndexOf('/');
        if (lastSlash > 0)
        {
            var parentPath = CurrentPath.Substring(0, lastSlash);
            await NavigateToAsync(parentPath);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await NavigateToAsync(CurrentPath);
    }

    [RelayCommand]
    private async Task OpenItemAsync(RepositoryItem? item)
    {
        if (item == null) return;

        if (item.IsDirectory)
        {
            await NavigateToAsync(item.Path);
        }
        else
        {
            ItemSelected?.Invoke(this, item.Path);
        }
    }

    [RelayCommand]
    private void SelectItem()
    {
        if (SelectedItem != null)
        {
            ItemSelected?.Invoke(this, SelectedItem.Path);
        }
        else
        {
            ItemSelected?.Invoke(this, CurrentPath);
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

public partial class RepositoryItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private bool _isDirectory;

    [ObservableProperty]
    private long _size;

    [ObservableProperty]
    private long _revision;

    [ObservableProperty]
    private string _author = string.Empty;

    [ObservableProperty]
    private DateTime _date;

    public string Icon => IsDirectory ? "📁" : "📄";
    public string SizeText => IsDirectory ? "" : FormatSize(Size);

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }
}
