using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Import dialog
/// </summary>
public partial class ImportViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _localPath = string.Empty;

    [ObservableProperty]
    private string _repositoryUrl = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _noIgnore;

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

    /// <summary>
    /// Event raised when folder browsing is requested
    /// </summary>
    public event EventHandler? BrowseFolderRequested;

    public ImportViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    /// <summary>
    /// Opens folder browser
    /// </summary>
    [RelayCommand]
    private void BrowseFolder()
    {
        BrowseFolderRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Imports files to repository
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        if (string.IsNullOrWhiteSpace(LocalPath))
        {
            StatusMessage = "Please select a local path to import";
            return;
        }

        if (string.IsNullOrWhiteSpace(RepositoryUrl))
        {
            StatusMessage = "Please enter a repository URL";
            return;
        }

        if (string.IsNullOrWhiteSpace(Message))
        {
            StatusMessage = "Please enter a commit message";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Importing files...";

            var result = await _workingCopyService.ImportAsync(LocalPath, RepositoryUrl, Message, NoIgnore);

            if (result.Success)
            {
                IsSuccess = true;
                StatusMessage = "Files imported successfully!";
            }
            else
            {
                StatusMessage = $"Import failed: {result.StandardError}";
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
