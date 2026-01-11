using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Relocate dialog
/// </summary>
public partial class RelocateViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _workingCopyPath;

    [ObservableProperty]
    private string _currentUrl = string.Empty;

    [ObservableProperty]
    private string _newUrl = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public RelocateViewModel(WorkingCopyService workingCopyService, string workingCopyPath)
    {
        _workingCopyService = workingCopyService;
        _workingCopyPath = workingCopyPath;
    }

    /// <summary>
    /// Loads the current repository URL
    /// </summary>
    [RelayCommand]
    public async Task LoadCurrentUrlAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading current URL...";

            var info = await _workingCopyService.GetWorkingCopyInfoAsync(_workingCopyPath);
            if (info != null)
            {
                CurrentUrl = info.RepositoryUrl ?? string.Empty;
                NewUrl = CurrentUrl; // Pre-fill with current URL for easy editing
            }

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading URL: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Relocates the working copy to the new URL
    /// </summary>
    [RelayCommand]
    private async Task RelocateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUrl))
        {
            StatusMessage = "Please enter a new repository URL";
            return;
        }

        if (NewUrl == CurrentUrl)
        {
            StatusMessage = "New URL is the same as the current URL";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Relocating working copy...";

            var result = await _workingCopyService.RelocateAsync(_workingCopyPath, CurrentUrl, NewUrl);

            if (result.Success)
            {
                IsSuccess = true;
                StatusMessage = "Working copy relocated successfully!";
                CurrentUrl = NewUrl;
            }
            else
            {
                StatusMessage = $"Relocate failed: {result.StandardError}";
            }
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

    /// <summary>
    /// Closes the dialog
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
