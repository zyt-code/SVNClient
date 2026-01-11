using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;
using Svns.Services.Svn.Operations;
using Svns.Services;

namespace Svns.ViewModels;

public partial class MergeViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _workingCopyPath = string.Empty;

    [ObservableProperty]
    private string _currentUrl = string.Empty;

    [ObservableProperty]
    private string _sourceUrl = string.Empty;

    [ObservableProperty]
    private long _revisionStart = 1;

    [ObservableProperty]
    private long _revisionEnd = 1;

    [ObservableProperty]
    private bool _dryRun;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _previewResult = string.Empty;

    [ObservableProperty]
    private string _repositoryRoot = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _availableSources = new();

    [ObservableProperty]
    private MergeAcceptType _selectedAcceptType = MergeAcceptType.Postpone;

    /// <summary>
    /// Available accept types for conflict resolution
    /// </summary>
    public MergeAcceptType[] AcceptTypes { get; } = Enum.GetValues<MergeAcceptType>();

    public bool CanMerge => !string.IsNullOrWhiteSpace(SourceUrl) &&
                             !IsProcessing &&
                             RevisionStart <= RevisionEnd;

    public bool CanPreview => CanMerge && !IsProcessing;

    public event EventHandler<bool>? CloseRequested;

    public MergeViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    public async Task InitializeAsync(string workingCopyPath, string currentUrl, string repositoryRoot, CancellationToken cancellationToken = default)
    {
        WorkingCopyPath = workingCopyPath;
        CurrentUrl = currentUrl;
        RepositoryRoot = repositoryRoot;

        await LoadAvailableSourcesAsync();
    }

    private async Task LoadAvailableSourcesAsync()
    {
        try
        {
            StatusMessage = "Loading available sources...";

            // Load trunk
            AvailableSources.Add($"{RepositoryRoot}/trunk");

            // Load branches
            var branchesUrl = $"{RepositoryRoot}/branches";
            var branches = await _workingCopyService.ListAsync(branchesUrl);
            foreach (var branch in branches)
            {
                if (branch.IsDirectory && !string.IsNullOrEmpty(branch.Name))
                {
                    AvailableSources.Add($"{branchesUrl}/{branch.Name.TrimEnd('/')}");
                }
            }

            StatusMessage = $"Found {AvailableSources.Count} sources";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading sources: {ex.Message}";
        }
    }

    partial void OnSourceUrlChanged(string value)
    {
        OnPropertyChanged(nameof(CanMerge));
        OnPropertyChanged(nameof(CanPreview));
        PreviewResult = string.Empty;
    }

    partial void OnRevisionStartChanged(long value)
    {
        OnPropertyChanged(nameof(CanMerge));
        OnPropertyChanged(nameof(CanPreview));
    }

    partial void OnRevisionEndChanged(long value)
    {
        OnPropertyChanged(nameof(CanMerge));
        OnPropertyChanged(nameof(CanPreview));
    }

    [RelayCommand]
    private async Task PreviewAsync()
    {
        if (!CanPreview) return;

        try
        {
            IsProcessing = true;
            StatusMessage = "Running merge preview...";

            var result = await _workingCopyService.MergeAsync(
                SourceUrl, RevisionStart, RevisionEnd, WorkingCopyPath, dryRun: true);

            if (result.Success)
            {
                PreviewResult = string.IsNullOrEmpty(result.StandardOutput)
                    ? "No changes would be merged."
                    : result.StandardOutput;
                StatusMessage = "Preview completed";
            }
            else
            {
                PreviewResult = $"Error: {result.StandardError}";
                StatusMessage = "Preview failed";
            }
        }
        catch (Exception ex)
        {
            PreviewResult = $"Error: {ex.Message}";
            StatusMessage = "Preview failed";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task MergeAsync()
    {
        if (!CanMerge) return;

        try
        {
            IsProcessing = true;
            StatusMessage = "Merging changes...";

            var acceptArg = SelectedAcceptType.ToSvnArgument();
            var result = await _workingCopyService.MergeAsync(
                SourceUrl, RevisionStart, RevisionEnd, WorkingCopyPath, dryRun: false, accept: acceptArg);

            if (result.Success)
            {
                StatusMessage = "Merge completed successfully!";
                await Task.Delay(1000);
                CloseRequested?.Invoke(this, true);
            }
            else
            {
                StatusMessage = $"Error: {result.StandardError}";
                PreviewResult = result.StandardError;
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
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }
}
