using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;
using Svns.Services.Svn.Core;
using Svns.Services.Svn.Operations;
using Svns.Services.Localization;
using Svns.Utils;

namespace Svns.ViewModels;

public partial class CommitViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _workingCopyPath;

    [ObservableProperty]
    private string _commitMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CommitFileNode> _files = new();

    [ObservableProperty]
    private bool _isCommitting;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

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
    public int SelectedCount
    {
        get
        {
            return CountCheckedFiles(Files);
        }
    }

    private int CountCheckedFiles(IEnumerable<CommitFileNode> nodes)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.IsFile && node.IsChecked == true)
            {
                count++;
            }
            count += CountCheckedFiles(node.Children);
        }
        return count;
    }

    /// <summary>
    /// Gets total file count
    /// </summary>
    public int TotalFileCount
    {
        get
        {
            return CountTotalFiles(Files);
        }
    }

    private int CountTotalFiles(IEnumerable<CommitFileNode> nodes)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.IsFile) count++;
            count += CountTotalFiles(node.Children);
        }
        return count;
    }

    /// <summary>
    /// Gets whether commit can be executed
    /// </summary>
    public bool CanCommit => !IsCommitting &&
                             !string.IsNullOrWhiteSpace(CommitMessage) &&
                             SelectedCount > 0;

    /// <summary>
    /// Loads the changed files and builds the tree
    /// </summary>
    public async Task LoadChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            StatusMessage = Localize.Get("Status.LoadingChanges");
            var statuses = await _workingCopyService.GetStatusAsync(_workingCopyPath);

            Files.Clear();
            var filteredStatuses = statuses.Where(s => s.HasLocalModifications).ToList();

            BuildFileTree(filteredStatuses);

            StatusMessage = Localize.Get("Commit.FilesWithChanges", filteredStatuses.Count);

            // Subscribe to selection changes
            SubscribeToSelectionChanges(Files);

            RefreshCounts();
        }
        catch (Exception ex)
        {
            StatusMessage = Localize.Get("Commit.ErrorLoading", ex.Message);
        }
    }

    private void SubscribeToSelectionChanges(IEnumerable<CommitFileNode> nodes)
    {
        foreach (var node in nodes)
        {
            node.SelectionChanged += OnFileSelectionChanged;
            SubscribeToSelectionChanges(node.Children);
        }
    }

    private void OnFileSelectionChanged(object? sender, EventArgs e)
    {
        RefreshCounts();
    }

    private void RefreshCounts()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(CanCommit));
        OnPropertyChanged(nameof(IsAllSelected));
        OnPropertyChanged(nameof(SelectAllIcon));
        OnPropertyChanged(nameof(SelectAllText));
    }

    private void BuildFileTree(List<SvnStatus> statuses)
    {
        var rootNodes = new List<CommitFileNode>();
        var nodeMap = new Dictionary<string, CommitFileNode>(); // Path -> Node

        foreach (var status in statuses)
        {
            string fullPath = status.Path;
            // Assuming status.Path is absolute or we can get relative from working copy
            string relPath = PathHelper.GetRelativePath(_workingCopyPath, fullPath);
            
            string[] parts = relPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            
            CommitFileNode? parent = null;
            string currentPath = "";

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                currentPath = i == 0 ? part : Path.Combine(currentPath, part);
                
                if (!nodeMap.TryGetValue(currentPath, out var node))
                {
                    bool isFile = i == parts.Length - 1; // Last part is the item itself
                    
                    node = new CommitFileNode
                    {
                        Name = part,
                        Path = isFile ? fullPath : Path.Combine(_workingCopyPath, currentPath),
                        Status = isFile ? status.WorkingCopyStatus : SvnStatusType.Normal, // Folders might be 'Normal' unless listed explicitly
                        IsFile = isFile
                    };

                    if (isFile)
                    {
                        // Use the actual status for the leaf node
                        node.Status = status.WorkingCopyStatus;
                    }

                    nodeMap[currentPath] = node;

                    if (parent == null)
                    {
                        rootNodes.Add(node);
                    }
                    else
                    {
                        node.Parent = parent;
                        parent.Children.Add(node);
                    }
                }
                else
                {
                    // If we found an existing node and it's the leaf for this status, update its status
                    // This handles case where directory itself is listed in status (e.g. Added/Deleted)
                    if (i == parts.Length - 1)
                    {
                        node.Status = status.WorkingCopyStatus;
                        node.IsFile = false; // It's a directory listed in status
                    }
                }

                parent = node;
            }
        }

        foreach (var node in rootNodes)
        {
            Files.Add(node);
        }
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

            var selectedPaths = GetSelectedPaths(Files).ToArray();

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
    /// Gets selected file paths from the tree
    /// </summary>
    private List<string> GetSelectedPaths(IEnumerable<CommitFileNode> nodes)
    {
        var paths = new List<string>();
        foreach (var node in nodes)
        {
            if (node.IsChecked == true)
            {
                // Add files or folders with status changes
                if (node.IsFile || node.Status != SvnStatusType.Normal)
                {
                    paths.Add(node.Path);
                }
            }
            // Always recurse into children to find selected files
            paths.AddRange(GetSelectedPaths(node.Children));
        }
        return paths;
    }

    /// <summary>
    /// Icon to show for select all button
    /// </summary>
    public string SelectAllIcon => IsAllSelected ? "CheckboxBlankOutline" : "CheckAll";

    /// <summary>
    /// Text to show for select all button
    /// </summary>
    public string SelectAllText => IsAllSelected ? "Deselect" : "Select All";

    /// <summary>
    /// Toggles between select all and deselect all
    /// </summary>
    [RelayCommand]
    private void ToggleSelection()
    {
        bool shouldSelectAll = !IsAllSelected;
        SetAllNodesChecked(Files, shouldSelectAll);
    }

    private void SetAllNodesChecked(IEnumerable<CommitFileNode> nodes, bool isChecked)
    {
        foreach (var node in nodes)
        {
            node.SetCheckedNoCascade(isChecked);
            SetAllNodesChecked(node.Children, isChecked);
        }
        // After setting all, refresh the UI
        OnPropertyChanged(nameof(IsAllSelected));
        OnPropertyChanged(nameof(SelectAllIcon));
        OnPropertyChanged(nameof(SelectAllText));
    }

    /// <summary>
    /// Gets whether all files are selected
    /// </summary>
    public bool IsAllSelected
    {
        get
        {
            if (Files.Count == 0) return false;
            return AreAllNodesSelected(Files);
        }
    }

    private bool AreAllNodesSelected(IEnumerable<CommitFileNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.IsChecked != true) return false;
            if (node.Children.Count > 0 && !AreAllNodesSelected(node.Children)) return false;
        }
        return true;
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

public partial class CommitFileNode : ObservableObject
{
    public ObservableCollection<CommitFileNode> Children { get; } = new();
    public CommitFileNode? Parent { get; set; }

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private SvnStatusType _status;

    [ObservableProperty]
    private bool _isFile = true;

    [ObservableProperty]
    private bool _isExpanded = true;

    private bool? _isChecked = true;
    private bool _isUpdating = false;

    /// <summary>
    /// Three-state checkbox: true = checked, false = unchecked, null = indeterminate (some children checked)
    /// </summary>
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value && !_isUpdating)
            {
                _isUpdating = true;

                // Set this node's value
                SetProperty(ref _isChecked, value);

                // If user explicitly checked/unchecked, cascade to children
                if (value.HasValue)
                {
                    UpdateChildren(value.Value);
                }

                // Update parent's indeterminate state
                UpdateParent();

                _isUpdating = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Updates all children recursively to the same checked state
    /// </summary>
    private void UpdateChildren(bool isChecked)
    {
        foreach (var child in Children)
        {
            child._isUpdating = true;
            child._isChecked = isChecked;
            child.UpdateChildren(isChecked);
            child._isUpdating = false;
            child.SelectionChanged?.Invoke(child, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Updates parent's state based on children's states
    /// </summary>
    private void UpdateParent()
    {
        if (Parent == null) return;

        bool? parentState = null;
        bool allTrue = true;
        bool allFalse = true;

        foreach (var child in Parent.Children)
        {
            if (child.IsChecked != true) allTrue = false;
            if (child.IsChecked != false) allFalse = false;
        }

        if (allTrue) parentState = true;
        else if (allFalse) parentState = false;
        else parentState = null; // Indeterminate

        if (Parent._isChecked != parentState)
        {
            Parent._isUpdating = true;
            Parent.SetProperty(ref Parent._isChecked, parentState, nameof(IsChecked));
            Parent._isUpdating = false;
            Parent.UpdateParent();
            Parent.SelectionChanged?.Invoke(Parent, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Forces the checked state without triggering cascading updates
    /// Used for programmatic bulk updates
    /// </summary>
    internal void SetCheckedNoCascade(bool? value)
    {
        _isUpdating = true;
        _isChecked = value;
        OnPropertyChanged(nameof(IsChecked));
        _isUpdating = false;
    }

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
        SvnStatusType.Missing => "Missing",
        SvnStatusType.Normal => "",
        _ => Status.ToString()
    };

    public bool ShowStatusBadge => Status != SvnStatusType.Normal;
}

