using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;
using Svns.Services;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Cleanup dialog
/// </summary>
public partial class CleanupViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _workingCopyPath;

    [ObservableProperty]
    private bool _removeUnversioned;

    [ObservableProperty]
    private bool _removeIgnored;

    [ObservableProperty]
    private bool _isRunning;

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
    /// Event raised when cleanup is successful
    /// </summary>
    public event EventHandler<string>? CleanupSucceeded;

    public CleanupViewModel(WorkingCopyService workingCopyService, string workingCopyPath)
    {
        _workingCopyService = workingCopyService;
        _workingCopyPath = workingCopyPath;
    }

    /// <summary>
    /// Gets the working copy path being cleaned up
    /// </summary>
    public string WorkingCopyPath => _workingCopyPath;

    /// <summary>
    /// Gets whether cleanup can be executed
    /// </summary>
    public bool CanCleanup => !IsRunning;

    /// <summary>
    /// Executes the cleanup operation
    /// </summary>
    [RelayCommand]
    private async Task CleanupAsync()
    {
        if (!CanCleanup) return;

        try
        {
            IsRunning = true;
            IsCompleted = false;
            HasError = false;
            ResultMessage = string.Empty;

            StatusMessage = "Running cleanup...";

            if (RemoveUnversioned)
            {
                StatusMessage = "Removing unversioned files...";
            }
            else if (RemoveIgnored)
            {
                StatusMessage = "Removing ignored files...";
            }

            var result = await _workingCopyService.CleanupAsync(
                _workingCopyPath,
                RemoveUnversioned,
                RemoveIgnored);

            if (result.Success)
            {
                StatusMessage = "Cleanup completed successfully!";
                ResultMessage = string.IsNullOrEmpty(result.StandardOutput)
                    ? "Working copy cleanup completed."
                    : result.StandardOutput;
                IsCompleted = true;
                CleanupSucceeded?.Invoke(this, ResultMessage);

                // Notify that cleanup was completed - triggers refresh in main window
                RefreshService.Instance.RequestRefresh(RefreshType.FileStatus);
            }
            else
            {
                StatusMessage = "Cleanup failed";
                ResultMessage = result.StandardError;
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Cleanup failed";
            ResultMessage = ex.Message;
            HasError = true;
        }
        finally
        {
            IsRunning = false;
            OnPropertyChanged(nameof(CanCleanup));
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
