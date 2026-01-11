using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Info dialog
/// </summary>
public partial class InfoViewModel : ViewModelBase
{
    [ObservableProperty]
    private WorkingCopyInfo? _workingCopyInfo;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _repositoryUrl = string.Empty;

    [ObservableProperty]
    private string _repositoryRoot = string.Empty;

    [ObservableProperty]
    private string _repositoryUuid = string.Empty;

    [ObservableProperty]
    private long _revision;

    [ObservableProperty]
    private long _lastChangedRevision;

    [ObservableProperty]
    private string _lastChangedAuthor = string.Empty;

    [ObservableProperty]
    private DateTime _lastChangedDate;

    [ObservableProperty]
    private string _relativePath = string.Empty;

    [ObservableProperty]
    private string _depth = string.Empty;

    [ObservableProperty]
    private int _modifiedCount;

    [ObservableProperty]
    private int _addedCount;

    [ObservableProperty]
    private int _deletedCount;

    [ObservableProperty]
    private int _conflictedCount;

    [ObservableProperty]
    private bool _hasUncommittedChanges;

    [ObservableProperty]
    private string _branchName = string.Empty;

    [ObservableProperty]
    private string _changesSummary = string.Empty;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public InfoViewModel()
    {
    }

    public InfoViewModel(WorkingCopyInfo info) : this()
    {
        Initialize(info);
    }

    /// <summary>
    /// Initializes the ViewModel with working copy info
    /// </summary>
    public void Initialize(WorkingCopyInfo info)
    {
        WorkingCopyInfo = info;
        Path = info.Path;
        RepositoryUrl = info.RepositoryUrl;
        RepositoryRoot = info.RepositoryRoot;
        RepositoryUuid = info.RepositoryUuid;
        Revision = info.Revision;
        LastChangedRevision = info.LastChangedRevision;
        LastChangedAuthor = info.LastChangedAuthor ?? "Unknown";
        LastChangedDate = info.LastChangedDate;
        RelativePath = info.RelativePath;
        Depth = info.Depth.ToString();
        ModifiedCount = info.ModifiedFileCount;
        AddedCount = info.AddedFileCount;
        DeletedCount = info.DeletedFileCount;
        ConflictedCount = info.ConflictedFileCount;
        HasUncommittedChanges = info.HasUncommittedChanges;
        BranchName = info.BranchName ?? "Unknown";
        ChangesSummary = info.ChangesSummary;
    }

    /// <summary>
    /// Gets the last changed date formatted for display
    /// </summary>
    public string FormattedLastChangedDate => LastChangedDate.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Gets the total changes count
    /// </summary>
    public int TotalChangesCount => ModifiedCount + AddedCount + DeletedCount + ConflictedCount;

    /// <summary>
    /// Closes the dialog
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
