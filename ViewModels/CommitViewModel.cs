using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;
using Svns.Services.Svn.Core;
using Svns.Services.Svn.Operations;
using Svns.Services.Localization;
using Svns.Services;

namespace Svns.ViewModels;

public partial class CommitViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _workingCopyPath;

    [ObservableProperty]
    private string _commitMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CommitFileItem> _files = new();

    [ObservableProperty]
    private bool _isCommitting;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _selectAll = true;

    /// <summary>
    /// Event raised when commit is successful
    /// </summary>
    public event EventHandler<string>? CommitSucceeded;

    /// <summary>
    /// Gets whether the commit was successful
    /// </summary>
    public bool WasCommitSuccessful { get; private set; }

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public CommitViewModel(WorkingCopyService workingCopyService, string workingCopyPath)
    {
        _workingCopyService = workingCopyService;
        _workingCopyPath = workingCopyPath;
    }

    /// <summary>
    /// Gets the count of selected files
    /// </summary>
    public int SelectedCount => Files.Count(f => f.IsSelected);

    /// <summary>
    /// Gets whether commit can be executed
    /// </summary>
    public bool CanCommit => !IsCommitting &&
                             !string.IsNullOrWhiteSpace(CommitMessage) &&
                             Files.Any(f => f.IsSelected);

    /// <summary>
    /// Loads the changed files
    /// </summary>
    public async Task LoadChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            StatusMessage = Localize.Get("Status.LoadingChanges");
            var statuses = await _workingCopyService.GetStatusAsync(_workingCopyPath);

            Files.Clear();
            foreach (var status in statuses.Where(s => s.HasLocalModifications))
            {
                var item = new CommitFileItem
                {
                    Path = status.Path,
                    Name = status.Name,
                    Status = status.WorkingCopyStatus,
                    IsSelected = true
                };
                item.PropertyChanged += (_, _) => OnPropertyChanged(nameof(SelectedCount));
                Files.Add(item);
            }

            StatusMessage = Localize.Get("Commit.FilesWithChanges", Files.Count);
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(CanCommit));
        }
        catch (Exception ex)
        {
            StatusMessage = Localize.Get("Commit.ErrorLoading", ex.Message);
        }
    }

    /// <summary>
    /// Toggles selection of all files
    /// </summary>
    partial void OnSelectAllChanged(bool value)
    {
        foreach (var file in Files)
        {
            file.IsSelected = value;
        }
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(CanCommit));
    }

    /// <summary>
    /// Executes the commit
    /// </summary>
    [RelayCommand]
    private async Task CommitAsync()
    {
        if (!CanCommit) return;

        try
        {
            IsCommitting = true;
            StatusMessage = Localize.Get("Status.CommittingChanges");

            var selectedPaths = Files
                .Where(f => f.IsSelected)
                .Select(f => f.Path)
                .ToArray();

            var result = await _workingCopyService.CommitAsync(CommitMessage, selectedPaths);

            if (result.Success)
            {
                StatusMessage = Localize.Get("Status.CommitSuccessful");
                WasCommitSuccessful = true;

                CommitSucceeded?.Invoke(this, result.StandardOutput);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusMessage = Localize.Get("Commit.FailedWithMessage", result.StandardError);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = Localize.Get("Commit.ErrorMessage", ex.Message);
        }
        finally
        {
            IsCommitting = false;
            OnPropertyChanged(nameof(CanCommit));
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

    partial void OnCommitMessageChanged(string value)
    {
        OnPropertyChanged(nameof(CanCommit));
    }
}

/// <summary>
/// Represents a file item in the commit dialog
/// </summary>
public partial class CommitFileItem : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected = true;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private SvnStatusType _status;

    /// <summary>
    /// Gets the status display text
    /// </summary>
    public string StatusText => Status switch
    {
        SvnStatusType.Modified => "Modified",
        SvnStatusType.Added => "Added",
        SvnStatusType.Deleted => "Deleted",
        SvnStatusType.Replaced => "Replaced",
        SvnStatusType.Conflicted => "Conflicted",
        _ => Status.ToString()
    };
}
