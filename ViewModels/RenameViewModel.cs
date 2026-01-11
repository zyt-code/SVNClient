using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Rename dialog
/// </summary>
public partial class RenameViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _originalPath;

    [ObservableProperty]
    private string _originalName = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private bool _isRenaming;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Event raised when rename is successful
    /// </summary>
    public event EventHandler<string>? RenameSucceeded;

    public RenameViewModel(WorkingCopyService workingCopyService, string filePath)
    {
        _workingCopyService = workingCopyService;
        _originalPath = filePath;
        OriginalName = Path.GetFileName(filePath);
        NewName = OriginalName;
    }

    /// <summary>
    /// Gets whether the name has changed
    /// </summary>
    public bool HasChanges => !string.IsNullOrWhiteSpace(NewName) &&
                              NewName != OriginalName;

    /// <summary>
    /// Gets whether rename can be executed
    /// </summary>
    public bool CanRename => !IsRenaming && HasChanges && IsValidFileName(NewName);

    /// <summary>
    /// Validates the file name
    /// </summary>
    private bool IsValidFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            if (name.Contains(c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Called when NewName changes
    /// </summary>
    partial void OnNewNameChanged(string value)
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (!string.IsNullOrWhiteSpace(value) && !IsValidFileName(value))
        {
            HasError = true;
            ErrorMessage = "Invalid file name";
        }

        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(CanRename));
    }

    /// <summary>
    /// Executes the rename operation
    /// </summary>
    [RelayCommand]
    private async Task RenameAsync()
    {
        if (!CanRename) return;

        try
        {
            IsRenaming = true;
            HasError = false;
            ErrorMessage = string.Empty;
            StatusMessage = "Renaming...";

            // Calculate new path
            var directory = Path.GetDirectoryName(_originalPath) ?? string.Empty;
            var newPath = Path.Combine(directory, NewName);

            // Check if destination already exists
            if (File.Exists(newPath) || Directory.Exists(newPath))
            {
                HasError = true;
                ErrorMessage = "A file or folder with this name already exists";
                StatusMessage = "Rename failed";
                return;
            }

            var result = await _workingCopyService.MoveAsync(_originalPath, newPath);

            if (result.Success)
            {
                StatusMessage = "Rename successful!";
                RenameSucceeded?.Invoke(this, newPath);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.StandardError;
                StatusMessage = "Rename failed";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "Rename failed";
        }
        finally
        {
            IsRenaming = false;
            OnPropertyChanged(nameof(CanRename));
        }
    }

    /// <summary>
    /// Cancels and closes the dialog
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
