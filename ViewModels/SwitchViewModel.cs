using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;
using Svns.Services;

namespace Svns.ViewModels;

public partial class SwitchViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _workingCopyPath = string.Empty;

    [ObservableProperty]
    private string _currentUrl = string.Empty;

    [ObservableProperty]
    private string _targetUrl = string.Empty;

    [ObservableProperty]
    private long? _revision;

    [ObservableProperty]
    private bool _useRevision;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _repositoryRoot = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _availableBranches = new();

    public bool CanSwitch => !string.IsNullOrWhiteSpace(TargetUrl) &&
                              !IsProcessing &&
                              TargetUrl != CurrentUrl;

    public event EventHandler<bool>? CloseRequested;

    public SwitchViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    public async Task InitializeAsync(string workingCopyPath, string currentUrl, string repositoryRoot, CancellationToken cancellationToken = default)
    {
        WorkingCopyPath = workingCopyPath;
        CurrentUrl = currentUrl;
        RepositoryRoot = repositoryRoot;
        TargetUrl = currentUrl;

        await LoadAvailableBranchesAsync();
    }

    private async Task LoadAvailableBranchesAsync()
    {
        try
        {
            StatusMessage = "Loading branches...";

            // Load trunk
            AvailableBranches.Add($"{RepositoryRoot}/trunk");

            // Load branches
            var branchesUrl = $"{RepositoryRoot}/branches";
            var branches = await _workingCopyService.ListAsync(branchesUrl);
            foreach (var branch in branches)
            {
                if (branch.IsDirectory && !string.IsNullOrEmpty(branch.Name))
                {
                    AvailableBranches.Add($"{branchesUrl}/{branch.Name.TrimEnd('/')}");
                }
            }

            // Load tags
            var tagsUrl = $"{RepositoryRoot}/tags";
            var tags = await _workingCopyService.ListAsync(tagsUrl);
            foreach (var tag in tags)
            {
                if (tag.IsDirectory && !string.IsNullOrEmpty(tag.Name))
                {
                    AvailableBranches.Add($"{tagsUrl}/{tag.Name.TrimEnd('/')}");
                }
            }

            StatusMessage = $"Found {AvailableBranches.Count} branches/tags";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading branches: {ex.Message}";
        }
    }

    partial void OnTargetUrlChanged(string value)
    {
        OnPropertyChanged(nameof(CanSwitch));
    }

    partial void OnUseRevisionChanged(bool value)
    {
        if (!value)
        {
            Revision = null;
        }
    }

    [RelayCommand]
    private async Task SwitchAsync()
    {
        if (!CanSwitch) return;

        try
        {
            IsProcessing = true;
            StatusMessage = "Switching working copy...";

            var rev = UseRevision ? Revision : null;
            var result = await _workingCopyService.SwitchAsync(WorkingCopyPath, TargetUrl, rev);

            if (result.Success)
            {
                StatusMessage = "Switch completed successfully!";
                await Task.Delay(1000);
                CloseRequested?.Invoke(this, true);
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

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }
}
