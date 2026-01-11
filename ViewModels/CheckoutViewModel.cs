using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;

namespace Svns.ViewModels;

public partial class CheckoutViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _repositoryUrl = string.Empty;

    [ObservableProperty]
    private string _localPath = string.Empty;

    [ObservableProperty]
    private long? _revision;

    [ObservableProperty]
    private bool _useRevision;

    [ObservableProperty]
    private bool _isExportMode;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private double _progress;

    public bool CanCheckout => !string.IsNullOrWhiteSpace(RepositoryUrl) &&
                                !string.IsNullOrWhiteSpace(LocalPath) &&
                                !IsProcessing;

    public string ActionText => IsExportMode ? "Export" : "Checkout";
    public string Title => IsExportMode ? "Export Repository" : "Checkout Repository";
    public string Description => IsExportMode
        ? "Export a clean copy without version control files"
        : "Checkout a working copy from the repository";

    public event EventHandler<bool>? CloseRequested;

    public CheckoutViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    partial void OnRepositoryUrlChanged(string value)
    {
        OnPropertyChanged(nameof(CanCheckout));
    }

    partial void OnLocalPathChanged(string value)
    {
        OnPropertyChanged(nameof(CanCheckout));
    }

    partial void OnUseRevisionChanged(bool value)
    {
        if (!value)
        {
            Revision = null;
        }
    }

    partial void OnIsExportModeChanged(bool value)
    {
        OnPropertyChanged(nameof(ActionText));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
    }

    [RelayCommand]
    private async Task BrowseLocalPathAsync()
    {
        // This would open a folder browser dialog
        // For now, we'll just leave it for the user to type
        StatusMessage = "Please enter or paste the local path";
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!CanCheckout) return;

        try
        {
            IsProcessing = true;
            StatusMessage = IsExportMode ? "Exporting..." : "Checking out...";
            Progress = 0;

            var rev = UseRevision ? Revision : null;

            var result = IsExportMode
                ? await _workingCopyService.ExportAsync(RepositoryUrl, LocalPath, rev)
                : await _workingCopyService.CheckoutAsync(RepositoryUrl, LocalPath, rev);

            if (result.Success)
            {
                Progress = 100;
                StatusMessage = IsExportMode ? "Export completed!" : "Checkout completed!";
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
