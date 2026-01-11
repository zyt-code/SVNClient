using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Svns.Models;
using Svns.Services;
using Svns.Services.Svn.Core;
using Svns.Services.Svn.Operations;
using System.Collections.ObjectModel;

namespace Svns.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SvnCommandService _svnCommandService;
    private readonly WorkingCopyService _workingCopyService;
    private readonly ClipboardService _clipboardService;
    private readonly NotificationService _notificationService;
    private Window? _ownerWindow;

    [ObservableProperty]
    private string _workingCopyPath = string.Empty;

    [ObservableProperty]
    private WorkingCopyInfo? _workingCopyInfo;

    [ObservableProperty]
    private ObservableCollection<SvnStatus> _fileStatuses = new();

    [ObservableProperty]
    private ObservableCollection<SvnStatus> _selectedFiles = new();

    [ObservableProperty]
    private SvnStatus? _selectedFile;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isWorkingCopyLoaded;

    [ObservableProperty]
    private string _currentBranch = string.Empty;

    [ObservableProperty]
    private long _currentRevision;

    [ObservableProperty]
    private int _modifiedCount;

    [ObservableProperty]
    private int _addedCount;

    [ObservableProperty]
    private int _deletedCount;

    [ObservableProperty]
    private int _conflictedCount;

    [ObservableProperty]
    private ObservableCollection<SvnLogEntry> _logEntries = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private StatusFilterType _selectedStatusFilter = StatusFilterType.All;

    // Log filter properties
    [ObservableProperty]
    private string _logAuthorFilter = string.Empty;

    [ObservableProperty]
    private string _logMessageFilter = string.Empty;

    [ObservableProperty]
    private DateTime? _logDateFrom;

    [ObservableProperty]
    private DateTime? _logDateTo;

    [ObservableProperty]
    private ObservableCollection<string> _availableAuthors = new();

    [ObservableProperty]
    private bool _showFilters = true;

    /// <summary>
    /// All log entries (unfiltered)
    /// </summary>
    private ObservableCollection<SvnLogEntry> _allLogEntries = new();

    /// <summary>
    /// Available status filter options
    /// </summary>
    public StatusFilterType[] StatusFilterOptions { get; } = Enum.GetValues<StatusFilterType>();

    /// <summary>
    /// Notification service instance
    /// </summary>
    public NotificationService Notifications => _notificationService;

    /// <summary>
    /// All file statuses (unfiltered)
    /// </summary>
    private ObservableCollection<SvnStatus> _allFileStatuses = new();

    private readonly AppSettingsService _settingsService;

    // Notification Center Properties
    [ObservableProperty]
    private bool _showNotificationCenter;

    public MainWindowViewModel()
    {
        _svnCommandService = new SvnCommandService();
        _workingCopyService = new WorkingCopyService(_svnCommandService);
        _settingsService = new AppSettingsService();
        _clipboardService = new ClipboardService();
        _notificationService = NotificationService.Instance;

        // Subscribe to refresh events
        RefreshService.Instance.Subscribe(new[]
        {
            RefreshType.All,
            RefreshType.FileStatus,
            RefreshType.Log,
            RefreshType.Info,
            RefreshType.Commit,
            RefreshType.Update,
            RefreshType.Merge,
            RefreshType.BranchTag,
            RefreshType.Switch,
            RefreshType.Revert,
            RefreshType.Properties
        }, OnRefreshRequested);
    }

    /// <summary>
    /// Handles refresh requests from other parts of the application
    /// </summary>
    private async void OnRefreshRequested(RefreshType type)
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
            return;

        try
        {
            switch (type)
            {
                case RefreshType.All:
                case RefreshType.Commit:
                case RefreshType.Update:
                case RefreshType.Merge:
                case RefreshType.BranchTag:
                case RefreshType.Switch:
                case RefreshType.Revert:
                case RefreshType.Properties:
                case RefreshType.Log:
                    // Refresh everything for these operations
                    await RefreshAllAsync();
                    break;
                case RefreshType.FileStatus:
                    await RefreshFileStatusesAsync();
                    break;
                case RefreshType.Info:
                    await RefreshInfoAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing: {ex.Message}";
        }
    }

    /// <summary>
    /// Refreshes all data
    /// </summary>
    private async Task RefreshAllAsync()
    {
        await RefreshFileStatusesAsync();
        await LoadLogsAsync();
    }

    /// <summary>
    /// Refreshes working copy info
    /// </summary>
    private async Task RefreshInfoAsync()
    {
        var info = await _workingCopyService.GetWorkingCopyInfoAsync(WorkingCopyPath);
        if (info != null)
        {
            WorkingCopyInfo = info;
            CurrentRevision = info.Revision;
            ModifiedCount = info.ModifiedFileCount;
            AddedCount = info.AddedFileCount;
            DeletedCount = info.DeletedFileCount;
            ConflictedCount = info.ConflictedFileCount;
        }
    }

    /// <summary>
    /// Loads the SVN commit history
    /// </summary>
    private async Task LoadLogsAsync()
    {
        if (string.IsNullOrEmpty(WorkingCopyPath))
        {
            LogEntries.Clear();
            _allLogEntries.Clear();
            return;
        }

        try
        {
            // Fetch logs directly from SVN
            var logs = await _workingCopyService.GetLogAsync(WorkingCopyPath, limit: 200);

            // Store all logs
            _allLogEntries.Clear();
            foreach (var log in logs)
            {
                _allLogEntries.Add(log);
            }

            // Extract unique authors for filter dropdown
            var authors = logs
                .Select(l => l.Author)
                .Where(a => !string.IsNullOrEmpty(a))
                .Distinct()
                .OrderBy(a => a)
                .ToList();

            AvailableAuthors.Clear();
            AvailableAuthors.Add(""); // Empty option for "All"
            foreach (var author in authors)
            {
                AvailableAuthors.Add(author);
            }

            // Apply filter and update UI
            ApplyLogFilter();
        }
        catch
        {
            LogEntries.Clear();
            _allLogEntries.Clear();
        }
    }

    /// <summary>
    /// Applies the current log filters
    /// </summary>
    private void ApplyLogFilter()
    {
        LogEntries.Clear();

        foreach (var log in _allLogEntries)
        {
            if (MatchesLogFilter(log))
            {
                LogEntries.Add(log);
            }
        }
    }

    /// <summary>
    /// Checks if a log entry matches the current filter
    /// </summary>
    private bool MatchesLogFilter(SvnLogEntry log)
    {
        // Author filter
        if (!string.IsNullOrWhiteSpace(LogAuthorFilter))
        {
            if (!string.Equals(log.Author, LogAuthorFilter, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Message filter
        if (!string.IsNullOrWhiteSpace(LogMessageFilter))
        {
            if (string.IsNullOrEmpty(log.Message) ||
                !log.Message.Contains(LogMessageFilter, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Date from filter
        if (LogDateFrom.HasValue)
        {
            if (log.Date < LogDateFrom.Value.Date)
            {
                return false;
            }
        }

        // Date to filter
        if (LogDateTo.HasValue)
        {
            if (log.Date > LogDateTo.Value.Date.AddDays(1))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears all log filters
    /// </summary>
    [RelayCommand]
    private void ClearLogFilters()
    {
        LogAuthorFilter = string.Empty;
        LogMessageFilter = string.Empty;
        LogDateFrom = null;
        LogDateTo = null;
        ApplyLogFilter();
        StatusMessage = $"Showing all {LogEntries.Count} log entries";
    }

    /// <summary>
    /// Toggles the visibility of the filters section
    /// </summary>
    [RelayCommand]
    private void ToggleFilters()
    {
        ShowFilters = !ShowFilters;
    }

    /// <summary>
    /// Toggles the notification center panel
    /// </summary>
    [RelayCommand]
    private void ToggleNotificationCenter()
    {
        ShowNotificationCenter = !ShowNotificationCenter;
        _notificationService.TogglePanel();
    }

    /// <summary>
    /// Clears all notifications
    /// </summary>
    [RelayCommand]
    private void ClearAllNotifications()
    {
        _notificationService.ClearAll();
    }

    /// <summary>
    /// Marks all notifications as read
    /// </summary>
    [RelayCommand]
    private void MarkAllNotificationsAsRead()
    {
        _notificationService.MarkAllAsRead();
    }

    /// <summary>
    /// Called when log author filter changes
    /// </summary>
    partial void OnLogAuthorFilterChanged(string value)
    {
        ApplyLogFilter();
        StatusMessage = string.IsNullOrWhiteSpace(value)
            ? $"Showing all {LogEntries.Count} log entries"
            : $"Filtered by author '{value}': {LogEntries.Count} entries";
    }

    /// <summary>
    /// Called when log message filter changes
    /// </summary>
    partial void OnLogMessageFilterChanged(string value)
    {
        ApplyLogFilter();
        StatusMessage = string.IsNullOrWhiteSpace(value)
            ? $"Showing {LogEntries.Count} log entries"
            : $"Filtered by message: {LogEntries.Count} entries";
    }

    /// <summary>
    /// Called when log date from filter changes
    /// </summary>
    partial void OnLogDateFromChanged(DateTime? value)
    {
        ApplyLogFilter();
        StatusMessage = $"Date filter applied: {LogEntries.Count} entries";
    }

    /// <summary>
    /// Called when log date to filter changes
    /// </summary>
    partial void OnLogDateToChanged(DateTime? value)
    {
        ApplyLogFilter();
        StatusMessage = $"Date filter applied: {LogEntries.Count} entries";
    }

    /// <summary>
    /// Sets the owner window for dialog operations
    /// </summary>
    public void SetWindow(Window window)
    {
        _ownerWindow = window;
    }

    /// <summary>
    /// Opens a working copy
    /// </summary>
    [RelayCommand]
    private async Task OpenWorkingCopyAsync()
    {
        // TODO: Show folder browser dialog
        // For now, use a placeholder
        var folderPath = await ShowFolderBrowserDialogAsync();

        if (!string.IsNullOrEmpty(folderPath))
        {
            await LoadWorkingCopyAsync(folderPath);
        }
    }

    /// <summary>
    /// Loads a working copy
    /// </summary>
    public async Task LoadWorkingCopyAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            StatusMessage = $"Loading working copy: {path}...";

            // Validate and find working copy root
            var rootPath = await _workingCopyService.FindWorkingCopyRootAsync(path);

            if (string.IsNullOrEmpty(rootPath))
            {
                StatusMessage = $"Error: Not a valid working copy: {path}";
                IsLoading = false;
                return;
            }

            WorkingCopyPath = rootPath;
            var info = await _workingCopyService.GetWorkingCopyInfoAsync(rootPath);

            if (info == null)
            {
                StatusMessage = $"Error: Failed to load working copy info";
                IsLoading = false;
                return;
            }

            WorkingCopyInfo = info;
            CurrentBranch = info.BranchName ?? "trunk";
            CurrentRevision = info.Revision;
            ModifiedCount = info.ModifiedFileCount;
            AddedCount = info.AddedFileCount;
            DeletedCount = info.DeletedFileCount;
            ConflictedCount = info.ConflictedFileCount;

            // Load file statuses
            await RefreshFileStatusesAsync();

            // Load commit history
            await LoadLogsAsync();

            // Save last working copy to app settings
            await _settingsService.SaveLastWorkingCopyAsync(rootPath);

            IsWorkingCopyLoaded = true;
            StatusMessage = $"Working copy loaded: {info.DisplayName} (r{info.Revision})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading working copy: {ex.Message}";
            IsWorkingCopyLoaded = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes file statuses
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        await RefreshFileStatusesAsync();
    }

    /// <summary>
    /// Updates the working copy
    /// </summary>
    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Updating working copy...";

            var result = await _workingCopyService.UpdateAsync(WorkingCopyPath);

            if (result.Success)
            {
                StatusMessage = "Update completed successfully";
                RefreshService.Instance.RequestRefresh(RefreshType.Update);
            }
            else
            {
                StatusMessage = $"Update failed: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Update error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Commits changes
    /// </summary>
    [RelayCommand]
    private async Task CommitAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var commitViewModel = new CommitViewModel(_workingCopyService, WorkingCopyPath);
            await commitViewModel.LoadChangesAsync();

            if (commitViewModel.Files.Count == 0)
            {
                StatusMessage = "No changes to commit";
                return;
            }

            var dialog = new Views.Dialogs.CommitDialog(commitViewModel);
            await dialog.ShowDialog(_ownerWindow);

            // Refresh after dialog closes if commit was successful
            if (commitViewModel.WasCommitSuccessful)
            {
                StatusMessage = "Commit successful!";
                // Notify that a commit was made - triggers refresh in main window
                RefreshService.Instance.RequestRefresh(RefreshType.Commit);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening commit dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Shows file diff
    /// </summary>
    [RelayCommand]
    private async Task DiffAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Getting diff for {SelectedFile.Name}...";

            var diff = await _workingCopyService.GetDiffAsync(SelectedFile.Path);

            if (string.IsNullOrEmpty(diff))
            {
                StatusMessage = "No differences found";
                return;
            }

            var diffViewModel = new DiffViewModel(SelectedFile.Path, diff);
            var dialog = new Views.Dialogs.DiffDialog(diffViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error getting diff: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Reverts selected file
    /// </summary>
    [RelayCommand]
    private async Task RevertAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        // Show confirmation dialog
        var confirmed = await Views.Dialogs.ConfirmDialog.ShowAsync(
            _ownerWindow,
            "Revert File",
            $"Are you sure you want to revert '{SelectedFile.Name}'?\n\nThis will discard all local changes.",
            "Revert",
            "Cancel");

        if (!confirmed)
        {
            StatusMessage = "Revert cancelled";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Reverting {SelectedFile.Name}...";

            var result = await _workingCopyService.RevertAsync(SelectedFile.Path);

            if (result.Success)
            {
                StatusMessage = $"Reverted: {SelectedFile.Name}";
                RefreshService.Instance.RequestRefresh(RefreshType.Revert);
            }
            else
            {
                StatusMessage = $"Revert failed: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error reverting: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Adds selected file
    /// </summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Adding {SelectedFile.Name}...";

            var result = await _workingCopyService.AddAsync(SelectedFile.Path);

            if (result.Success)
            {
                StatusMessage = $"Added: {SelectedFile.Name}";
                RefreshService.Instance.RequestRefresh(RefreshType.FileStatus);
            }
            else
            {
                StatusMessage = $"Add failed: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deletes selected file
    /// </summary>
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        // Show confirmation dialog
        var confirmed = await Views.Dialogs.ConfirmDialog.ShowAsync(
            _ownerWindow,
            "Delete File",
            $"Are you sure you want to delete '{SelectedFile.Name}'?\n\nThis will schedule the file for deletion from version control.",
            "Delete",
            "Cancel");

        if (!confirmed)
        {
            StatusMessage = "Delete cancelled";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Deleting {SelectedFile.Name}...";

            var result = await _workingCopyService.DeleteAsync(SelectedFile.Path);

            if (result.Success)
            {
                StatusMessage = $"Deleted: {SelectedFile.Name}";
                RefreshService.Instance.RequestRefresh(RefreshType.FileStatus);
            }
            else
            {
                StatusMessage = $"Delete failed: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Shows file history
    /// </summary>
    [RelayCommand]
    private async Task ShowHistoryAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Loading history for {SelectedFile.Name}...";

            var historyViewModel = new HistoryViewModel(_workingCopyService, SelectedFile.Path);
            await historyViewModel.LoadHistoryAsync();

            var dialog = new Views.Dialogs.HistoryDialog(historyViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading history: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Shows file blame (annotate)
    /// </summary>
    [RelayCommand]
    private async Task BlameAsync()
    {
        if (SelectedFile == null || !SelectedFile.IsFile)
        {
            StatusMessage = "Please select a file to blame";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Loading blame for {SelectedFile.Name}...";

            var blameViewModel = new BlameViewModel(_workingCopyService, SelectedFile.Path);
            await blameViewModel.LoadBlameAsync();

            var dialog = new Views.Dialogs.BlameDialog(blameViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading blame: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Renames the selected file or folder
    /// </summary>
    [RelayCommand]
    private async Task RenameAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "Please select a file or folder to rename";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var renameViewModel = new RenameViewModel(_workingCopyService, SelectedFile.Path);
            var dialog = new Views.Dialogs.RenameDialog(renameViewModel);
            renameViewModel.RenameSucceeded += async (_, newPath) =>
            {
                StatusMessage = $"Renamed to: {System.IO.Path.GetFileName(newPath)}";
                await RefreshFileStatusesAsync();
            };

            await dialog.ShowDialog(_ownerWindow);
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening rename dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens lock dialog for the selected file
    /// </summary>
    [RelayCommand]
    private async Task LockFileAsync()
    {
        if (SelectedFile == null || !SelectedFile.IsFile)
        {
            StatusMessage = "Please select a file to lock";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var lockViewModel = new LockViewModel(_workingCopyService, SelectedFile.Path);
            var dialog = new Views.Dialogs.LockDialog(lockViewModel);

            await dialog.ShowDialog(_ownerWindow);
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening lock dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens property dialog for the selected file
    /// </summary>
    [RelayCommand]
    private async Task ShowPropertiesAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "Please select a file or folder";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Loading properties for {SelectedFile.Name}...";

            var propertyViewModel = new PropertyViewModel(_workingCopyService, SelectedFile.Path);
            await propertyViewModel.LoadPropertiesAsync();

            var dialog = new Views.Dialogs.PropertyDialog(propertyViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening property dialog: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Shows working copy info dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowInfoAsync()
    {
        if (!IsWorkingCopyLoaded || WorkingCopyInfo == null)
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var infoViewModel = new InfoViewModel(WorkingCopyInfo);
            var dialog = new Views.Dialogs.InfoDialog(infoViewModel);
            await dialog.ShowDialog(_ownerWindow);
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening info dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens cleanup dialog
    /// </summary>
    [RelayCommand]
    private async Task CleanupAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var cleanupViewModel = new CleanupViewModel(_workingCopyService, WorkingCopyPath);
            var dialog = new Views.Dialogs.CleanupDialog(cleanupViewModel);

            await dialog.ShowDialog(_ownerWindow);
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening cleanup dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens file in default application
    /// </summary>
    [RelayCommand]
    private void OpenFileInExplorer()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        try
        {
            if (File.Exists(SelectedFile.Path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedFile.Path,
                    UseShellExecute = true
                });
            }
            else if (Directory.Exists(SelectedFile.Path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedFile.Path,
                    UseShellExecute = true
                });
            }
            else
            {
                StatusMessage = "File or directory does not exist";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening file: {ex.Message}";
        }
    }

    /// <summary>
    /// Copies file path to clipboard
    /// </summary>
    [RelayCommand]
    private async Task CopyPathAsync()
    {
        if (SelectedFile == null)
        {
            StatusMessage = "No file selected";
            return;
        }

        try
        {
            var success = await _clipboardService.SetTextAsync(SelectedFile.Path);
            StatusMessage = success
                ? $"Path copied: {SelectedFile.Path}"
                : "Failed to copy path to clipboard";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error copying path: {ex.Message}";
        }
    }

    /// <summary>
    /// Refreshes file statuses
    /// </summary>
    private async Task RefreshFileStatusesAsync()
    {
        try
        {
            var tree = await _workingCopyService.GetStatusTreeAsync(WorkingCopyPath);

            _allFileStatuses.Clear();
            foreach (var status in tree)
            {
                _allFileStatuses.Add(status);
            }

            // Apply filter
            ApplyFilter();

            // Update counts
            WorkingCopyInfo = await _workingCopyService.GetWorkingCopyInfoAsync(WorkingCopyPath);
            if (WorkingCopyInfo != null)
            {
                ModifiedCount = WorkingCopyInfo.ModifiedFileCount;
                AddedCount = WorkingCopyInfo.AddedFileCount;
                DeletedCount = WorkingCopyInfo.DeletedFileCount;
                ConflictedCount = WorkingCopyInfo.ConflictedFileCount;
                CurrentRevision = WorkingCopyInfo.Revision;
            }

            // Load commit history
            await LoadLogsAsync();

            StatusMessage = $"Refreshed {_allFileStatuses.Count} items ({FileStatuses.Count} shown)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing statuses: {ex.Message}";
        }
    }

    /// <summary>
    /// Applies the current filter to file statuses
    /// </summary>
    private void ApplyFilter()
    {
        FileStatuses.Clear();

        foreach (var status in _allFileStatuses)
        {
            if (MatchesFilter(status))
            {
                FileStatuses.Add(status);
            }
        }
    }

    /// <summary>
    /// Checks if a status matches the current filter
    /// </summary>
    private bool MatchesFilter(SvnStatus status)
    {
        // Check text filter
        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            if (!status.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) &&
                !status.Path.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Check status filter
        return SelectedStatusFilter switch
        {
            StatusFilterType.All => true,
            StatusFilterType.Modified => status.WorkingCopyStatus == SvnStatusType.Modified,
            StatusFilterType.Added => status.WorkingCopyStatus == SvnStatusType.Added,
            StatusFilterType.Deleted => status.WorkingCopyStatus == SvnStatusType.Deleted,
            StatusFilterType.Conflicted => status.WorkingCopyStatus == SvnStatusType.Conflicted,
            StatusFilterType.Unversioned => status.WorkingCopyStatus == SvnStatusType.Unversioned,
            StatusFilterType.LocalChanges => status.HasLocalModifications,
            _ => true
        };
    }

    /// <summary>
    /// Called when filter text changes
    /// </summary>
    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
        StatusMessage = string.IsNullOrWhiteSpace(value)
            ? $"Showing {FileStatuses.Count} items"
            : $"Filter: {FileStatuses.Count} of {_allFileStatuses.Count} items";
    }

    /// <summary>
    /// Called when status filter changes
    /// </summary>
    partial void OnSelectedStatusFilterChanged(StatusFilterType value)
    {
        ApplyFilter();
        StatusMessage = value == StatusFilterType.All
            ? $"Showing all {FileStatuses.Count} items"
            : $"Filter '{value}': {FileStatuses.Count} of {_allFileStatuses.Count} items";
    }

    /// <summary>
    /// Shows folder browser dialog
    /// </summary>
    private async Task<string> ShowFolderBrowserDialogAsync()
    {
        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return string.Empty;
        }

        try
        {
            var topLevel = TopLevel.GetTopLevel(_ownerWindow);
            if (topLevel == null)
            {
                StatusMessage = "Error: Cannot get top-level window";
                return string.Empty;
            }

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select SVN Working Copy Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                return folders[0].Path.LocalPath;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening folder dialog: {ex.Message}";
            return string.Empty;
        }
    }

    /// <summary>
    /// Opens checkout dialog to checkout from repository
    /// </summary>
    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var checkoutViewModel = new CheckoutViewModel(_workingCopyService);
            var dialog = new Views.Dialogs.CheckoutDialog(checkoutViewModel);
            var result = await dialog.ShowDialog<bool?>(_ownerWindow);

            if (result == true && !string.IsNullOrEmpty(checkoutViewModel.LocalPath))
            {
                StatusMessage = "Checkout completed successfully";
                await LoadWorkingCopyAsync(checkoutViewModel.LocalPath);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening checkout dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens branch/tag dialog to create branch or tag
    /// </summary>
    [RelayCommand]
    private async Task BranchTagAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var sourceUrl = WorkingCopyInfo?.RepositoryUrl ?? "";
            var repositoryRoot = WorkingCopyInfo?.RepositoryRoot ?? "";
            var branchTagViewModel = new BranchTagViewModel(_workingCopyService);
            branchTagViewModel.Initialize(sourceUrl, repositoryRoot);

            var dialog = new Views.Dialogs.BranchTagDialog(branchTagViewModel);
            var result = await dialog.ShowDialog<bool?>(_ownerWindow);

            if (result == true)
            {
                StatusMessage = branchTagViewModel.IsBranch
                    ? "Branch created successfully"
                    : "Tag created successfully";
                // Branch/tag creation modifies repository - refresh everything
                RefreshService.Instance.RequestRefresh(RefreshType.BranchTag);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening branch/tag dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens switch dialog to switch working copy to different branch/tag
    /// </summary>
    [RelayCommand]
    private async Task SwitchAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var currentUrl = WorkingCopyInfo?.RepositoryUrl ?? "";
            var repositoryRoot = WorkingCopyInfo?.RepositoryRoot ?? "";
            var switchViewModel = new SwitchViewModel(_workingCopyService);
            await switchViewModel.InitializeAsync(WorkingCopyPath, currentUrl, repositoryRoot);

            var dialog = new Views.Dialogs.SwitchDialog(switchViewModel);
            var result = await dialog.ShowDialog<bool?>(_ownerWindow);

            if (result == true)
            {
                StatusMessage = "Switch completed successfully";
                // Switch modifies repository - refresh everything
                RefreshService.Instance.RequestRefresh(RefreshType.Switch);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening switch dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens merge dialog to merge changes from another branch
    /// </summary>
    [RelayCommand]
    private async Task MergeAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var currentUrl = WorkingCopyInfo?.RepositoryUrl ?? "";
            var repositoryRoot = WorkingCopyInfo?.RepositoryRoot ?? "";
            var mergeViewModel = new MergeViewModel(_workingCopyService);
            await mergeViewModel.InitializeAsync(WorkingCopyPath, currentUrl, repositoryRoot);

            var dialog = new Views.Dialogs.MergeDialog(mergeViewModel);
            var result = await dialog.ShowDialog<bool?>(_ownerWindow);

            if (result == true)
            {
                StatusMessage = "Merge completed successfully";
                // Merge modifies repository - refresh everything
                RefreshService.Instance.RequestRefresh(RefreshType.Merge);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening merge dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens conflict resolution dialog
    /// </summary>
    [RelayCommand]
    private async Task ResolveConflictsAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var conflictViewModel = new ConflictResolveViewModel(_workingCopyService);
            await conflictViewModel.InitializeAsync(WorkingCopyPath);

            if (conflictViewModel.Conflicts.Count == 0)
            {
                StatusMessage = "No conflicts to resolve";
                return;
            }

            var dialog = new Views.Dialogs.ConflictResolveDialog(conflictViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = $"Resolved {conflictViewModel.ResolvedCount} of {conflictViewModel.TotalCount} conflicts";
            await RefreshFileStatusesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening conflict resolution dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens repository browser dialog
    /// </summary>
    [RelayCommand]
    private async Task BrowseRepositoryAsync()
    {
        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var repoUrl = WorkingCopyInfo?.RepositoryRoot ?? "";
            var browserViewModel = new RepositoryBrowserViewModel(_workingCopyService);

            if (!string.IsNullOrEmpty(repoUrl))
            {
                await browserViewModel.InitializeAsync(repoUrl);
            }

            var dialog = new Views.Dialogs.RepositoryBrowserDialog(browserViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening repository browser: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens relocate dialog to change repository URL
    /// </summary>
    [RelayCommand]
    private async Task RelocateAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var relocateViewModel = new RelocateViewModel(_workingCopyService, WorkingCopyPath);
            var dialog = new Views.Dialogs.RelocateDialog(relocateViewModel);
            await dialog.ShowDialog(_ownerWindow);

            // Refresh working copy info after relocate
            if (relocateViewModel.IsSuccess)
            {
                await LoadWorkingCopyAsync(WorkingCopyPath);
            }

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening relocate dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Opens import dialog to import files to repository
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_ownerWindow == null)
        {
            StatusMessage = "Error: Window not initialized";
            return;
        }

        try
        {
            var importViewModel = new ImportViewModel(_workingCopyService);
            var dialog = new Views.Dialogs.ImportDialog(importViewModel);
            await dialog.ShowDialog(_ownerWindow);

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening import dialog: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates a new directory under version control
    /// </summary>
    [RelayCommand]
    private async Task NewFolderAsync()
    {
        if (!IsWorkingCopyLoaded || string.IsNullOrEmpty(WorkingCopyPath))
        {
            StatusMessage = "No working copy loaded";
            return;
        }

        // Get the target directory - use selected folder or working copy root
        var targetDir = WorkingCopyPath;
        if (SelectedFile != null)
        {
            targetDir = SelectedFile.IsFile
                ? System.IO.Path.GetDirectoryName(SelectedFile.Path) ?? WorkingCopyPath
                : SelectedFile.Path;
        }

        // Prompt for folder name
        var folderName = await PromptForFolderNameAsync();
        if (string.IsNullOrEmpty(folderName))
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Creating new folder...";

            var newFolderPath = System.IO.Path.Combine(targetDir, folderName);
            var result = await _workingCopyService.MkdirAsync(newFolderPath);

            if (result.Success)
            {
                StatusMessage = $"Folder '{folderName}' created successfully";
                await RefreshAsync();
            }
            else
            {
                StatusMessage = $"Failed to create folder: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating folder: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<string?> PromptForFolderNameAsync()
    {
        // Simple implementation using message box - in a real app, you'd create a dialog
        // For now, we'll use a fixed name pattern. A proper implementation would need a dialog.
        return await Task.FromResult<string?>("NewFolder");
    }
}