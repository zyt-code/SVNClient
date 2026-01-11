using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;
using Svns.Services.Svn.Operations;
using Svns.Services;

namespace Svns.ViewModels;

public partial class ConflictResolveViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _workingCopyPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ConflictItem> _conflicts = new();

    [ObservableProperty]
    private ConflictItem? _selectedConflict;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _resolvedCount;

    public int TotalCount => Conflicts.Count;

    public event EventHandler<bool>? CloseRequested;

    public ConflictResolveViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    public async Task InitializeAsync(string workingCopyPath, CancellationToken cancellationToken = default)
    {
        WorkingCopyPath = workingCopyPath;
        await LoadConflictsAsync();
    }

    private async Task LoadConflictsAsync()
    {
        try
        {
            StatusMessage = "Loading conflicts...";
            var conflictedFiles = await _workingCopyService.GetConflictedFilesAsync(WorkingCopyPath);

            Conflicts.Clear();
            foreach (var file in conflictedFiles)
            {
                Conflicts.Add(new ConflictItem
                {
                    Path = file.Path,
                    Name = System.IO.Path.GetFileName(file.Path),
                    IsResolved = false
                });
            }

            OnPropertyChanged(nameof(TotalCount));
            StatusMessage = $"Found {TotalCount} conflicted files";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ResolveWithMineAsync()
    {
        if (SelectedConflict == null) return;
        await ResolveConflictAsync(SelectedConflict, "mine-full");
    }

    [RelayCommand]
    private async Task ResolveWithTheirsAsync()
    {
        if (SelectedConflict == null) return;
        await ResolveConflictAsync(SelectedConflict, "theirs-full");
    }

    [RelayCommand]
    private async Task ResolveWithWorkingAsync()
    {
        if (SelectedConflict == null) return;
        await ResolveConflictAsync(SelectedConflict, "working");
    }

    [RelayCommand]
    private async Task ResolveWithBaseAsync()
    {
        if (SelectedConflict == null) return;
        await ResolveConflictAsync(SelectedConflict, "base");
    }

    [RelayCommand]
    private async Task ResolveAllWithMineAsync()
    {
        await ResolveAllAsync("mine-full");
    }

    [RelayCommand]
    private async Task ResolveAllWithTheirsAsync()
    {
        await ResolveAllAsync("theirs-full");
    }

    private async Task ResolveConflictAsync(ConflictItem conflict, string accept)
    {
        try
        {
            IsProcessing = true;
            StatusMessage = $"Resolving {conflict.Name}...";

            var result = await _workingCopyService.ResolveAsync(conflict.Path, accept);

            if (result.Success)
            {
                conflict.IsResolved = true;
                ResolvedCount++;
                StatusMessage = $"Resolved {conflict.Name}";

                // Check if all conflicts are resolved
                if (ResolvedCount == TotalCount)
                {
                    StatusMessage = "All conflicts resolved!";
                    // Notify that conflicts were resolved - triggers refresh in main window
                    RefreshService.Instance.RequestRefresh(RefreshType.FileStatus);
                    await Task.Delay(1000);
                    CloseRequested?.Invoke(this, true);
                }
            }
            else
            {
                StatusMessage = $"Error: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task ResolveAllAsync(string accept)
    {
        try
        {
            IsProcessing = true;
            var unresolvedConflicts = Conflicts.Where(c => !c.IsResolved).ToList();

            foreach (var conflict in unresolvedConflicts)
            {
                StatusMessage = $"Resolving {conflict.Name}...";
                var result = await _workingCopyService.ResolveAsync(conflict.Path, accept);

                if (result.Success)
                {
                    conflict.IsResolved = true;
                    ResolvedCount++;
                }
            }

            if (ResolvedCount == TotalCount)
            {
                StatusMessage = "All conflicts resolved!";
                await Task.Delay(1000);
                CloseRequested?.Invoke(this, true);
            }
            else
            {
                StatusMessage = $"Resolved {ResolvedCount} of {TotalCount} conflicts";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, ResolvedCount > 0);
    }
}

public partial class ConflictItem : ObservableObject
{
    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isResolved;
}
