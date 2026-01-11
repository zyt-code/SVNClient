using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;
using Svns.Services.Svn.Operations;
using Svns.Services;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Lock dialog
/// </summary>
public partial class LockViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string[] _filePaths;

    [ObservableProperty]
    private ObservableCollection<LockFileItem> _files = new();

    [ObservableProperty]
    private string _lockMessage = string.Empty;

    [ObservableProperty]
    private bool _forceLock;

    [ObservableProperty]
    private bool _isLocking;

    [ObservableProperty]
    private bool _isUnlocking;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _resultMessage = string.Empty;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Event raised when lock/unlock is successful
    /// </summary>
    public event EventHandler<string>? OperationSucceeded;

    public LockViewModel(WorkingCopyService workingCopyService, params string[] filePaths)
    {
        _workingCopyService = workingCopyService;
        _filePaths = filePaths;

        foreach (var path in filePaths)
        {
            Files.Add(new LockFileItem
            {
                Path = path,
                Name = System.IO.Path.GetFileName(path),
                IsSelected = true
            });
        }
    }

    /// <summary>
    /// Gets the number of selected files
    /// </summary>
    public int SelectedCount => Files.Count(f => f.IsSelected);

    /// <summary>
    /// Gets whether operations can be executed
    /// </summary>
    public bool CanExecute => !IsLocking && !IsUnlocking && SelectedCount > 0;

    /// <summary>
    /// Locks the selected files
    /// </summary>
    [RelayCommand]
    private async Task LockAsync()
    {
        if (!CanExecute) return;

        try
        {
            IsLocking = true;
            IsCompleted = false;
            HasError = false;
            ResultMessage = string.Empty;

            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            var successCount = 0;
            var failCount = 0;
            var results = new System.Text.StringBuilder();

            foreach (var file in selectedFiles)
            {
                StatusMessage = $"Locking {file.Name}...";
                var result = await _workingCopyService.LockAsync(file.Path, LockMessage, ForceLock);

                if (result.Success)
                {
                    successCount++;
                    file.IsLocked = true;
                    results.AppendLine($"Locked: {file.Name}");
                }
                else
                {
                    failCount++;
                    results.AppendLine($"Failed to lock {file.Name}: {result.StandardError}");
                }
            }

            if (failCount == 0)
            {
                StatusMessage = $"Successfully locked {successCount} file(s)";
                ResultMessage = results.ToString();
                IsCompleted = true;
                OperationSucceeded?.Invoke(this, ResultMessage);

                // Notify that lock operation was completed - triggers refresh in main window
                RefreshService.Instance.RequestRefresh(RefreshType.FileStatus);
            }
            else
            {
                StatusMessage = $"Locked {successCount}, failed {failCount}";
                ResultMessage = results.ToString();
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Lock failed";
            ResultMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsLocking = false;
            OnPropertyChanged(nameof(CanExecute));
        }
    }

    /// <summary>
    /// Unlocks the selected files
    /// </summary>
    [RelayCommand]
    private async Task UnlockAsync()
    {
        if (!CanExecute) return;

        try
        {
            IsUnlocking = true;
            IsCompleted = false;
            HasError = false;
            ResultMessage = string.Empty;

            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            var successCount = 0;
            var failCount = 0;
            var results = new System.Text.StringBuilder();

            foreach (var file in selectedFiles)
            {
                StatusMessage = $"Unlocking {file.Name}...";
                var result = await _workingCopyService.UnlockAsync(file.Path, ForceLock);

                if (result.Success)
                {
                    successCount++;
                    file.IsLocked = false;
                    results.AppendLine($"Unlocked: {file.Name}");
                }
                else
                {
                    failCount++;
                    results.AppendLine($"Failed to unlock {file.Name}: {result.StandardError}");
                }
            }

            if (failCount == 0)
            {
                StatusMessage = $"Successfully unlocked {successCount} file(s)";
                ResultMessage = results.ToString();
                IsCompleted = true;
                OperationSucceeded?.Invoke(this, ResultMessage);

                // Notify that lock operation was completed - triggers refresh in main window
                RefreshService.Instance.RequestRefresh(RefreshType.FileStatus);
            }
            else
            {
                StatusMessage = $"Unlocked {successCount}, failed {failCount}";
                ResultMessage = results.ToString();
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Unlock failed";
            ResultMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsUnlocking = false;
            OnPropertyChanged(nameof(CanExecute));
        }
    }

    /// <summary>
    /// Closes the dialog
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Represents a file item in the lock dialog
/// </summary>
public partial class LockFileItem : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isLocked;
}
