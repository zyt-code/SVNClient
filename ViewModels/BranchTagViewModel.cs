using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;
using Svns.Services;

namespace Svns.ViewModels;

public partial class BranchTagViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _sourceUrl = string.Empty;

    [ObservableProperty]
    private string _destinationUrl = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBranch = true;

    [ObservableProperty]
    private bool _isTag;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _repositoryRoot = string.Empty;

    [ObservableProperty]
    private string _branchName = string.Empty;

    public bool CanCreate => !string.IsNullOrWhiteSpace(BranchName) &&
                              !string.IsNullOrWhiteSpace(Message) &&
                              !IsProcessing;

    public event EventHandler<bool>? CloseRequested;

    public BranchTagViewModel(WorkingCopyService workingCopyService)
    {
        _workingCopyService = workingCopyService;
    }

    public void Initialize(string sourceUrl, string repositoryRoot)
    {
        SourceUrl = sourceUrl;
        RepositoryRoot = repositoryRoot;
        UpdateDestinationUrl();
    }

    partial void OnIsBranchChanged(bool value)
    {
        if (value)
        {
            IsTag = false;
            UpdateDestinationUrl();
        }
    }

    partial void OnIsTagChanged(bool value)
    {
        if (value)
        {
            IsBranch = false;
            UpdateDestinationUrl();
        }
    }

    partial void OnBranchNameChanged(string value)
    {
        UpdateDestinationUrl();
        OnPropertyChanged(nameof(CanCreate));
    }

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(CanCreate));
    }

    private void UpdateDestinationUrl()
    {
        if (string.IsNullOrWhiteSpace(RepositoryRoot) || string.IsNullOrWhiteSpace(BranchName))
        {
            DestinationUrl = string.Empty;
            return;
        }

        var folder = IsBranch ? "branches" : "tags";
        DestinationUrl = $"{RepositoryRoot.TrimEnd('/')}/{folder}/{BranchName}";
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (!CanCreate) return;

        try
        {
            IsProcessing = true;
            StatusMessage = IsBranch ? "Creating branch..." : "Creating tag...";

            var result = await _workingCopyService.CopyAsync(SourceUrl, DestinationUrl, Message);

            if (result.Success)
            {
                StatusMessage = IsBranch ? "Branch created successfully!" : "Tag created successfully!";
                await Task.Delay(1000);
                CloseRequested?.Invoke(this, true);
            }
            else
            {
                // Provide user-friendly error messages with debugging info
                StatusMessage = FormatErrorMessage(result.StandardError, result.StandardOutput);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Formats SVN error messages into more user-friendly descriptions
    /// </summary>
    private string FormatErrorMessage(string error, string? output = null)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            if (!string.IsNullOrWhiteSpace(output))
            {
                return $"Operation failed.\n\nOutput: {output}";
            }
            return "An unknown error occurred. Please try again.";
        }

        // E160013: Path not found error
        if (error.Contains("E160013") || error.Contains("File not found") || error.Contains("path not found") ||
            error.Contains("E200009") || error.Contains("is not a working copy") ||
            error.Contains("E155015"))
        {
            return $"Source path not found.\n\n" +
                   $"Source: '{SourceUrl}'\n" +
                   $"Destination: '{DestinationUrl}'\n\n" +
                   $"Possible causes:\n" +
                   $"• The source path has not been committed to the repository\n" +
                   $"• The source path does not exist\n" +
                   $"• For local repositories, ensure the path is a valid SVN working copy\n\n" +
                   $"SVN error: {error}";
        }

        // E155015: No such transaction error
        if (error.Contains("E155015") || (error.Contains("transaction") && error.Contains("not found")))
        {
            return "SVN transaction not found.\n\nTry running 'svn cleanup' followed by 'svn update' on your working copy.";
        }

        // E170013: Unable to connect to repository
        if (error.Contains("E170013") || error.Contains("Unable to connect"))
        {
            return "Cannot connect to SVN repository.\n\nPlease check:\n• Network connection is stable\n• Repository URL is correct\n• VPN is connected if required";
        }

        // E195019: Authorization failed
        if (error.Contains("E195019") || error.Contains("authorization") || error.Contains("credentials"))
        {
            return "Authorization failed.\n\nPlease check:\n• Your username and password are correct\n• You have permission to create branches/tags";
        }

        // E200007: Merging or copying requires source to be a repository root
        if (error.Contains("E200007") || error.Contains("repository root"))
        {
            return "Invalid source path.\n\nThe source must be a valid repository URL. Please select a path from the repository browser.";
        }

        // E215004: No more credentials available or credentials failed
        if (error.Contains("E215004") || error.Contains("credentials failed"))
        {
            return "Authentication failed.\n\nYour SVN credentials were rejected. Please check your username and password.";
        }

        // Return original error if no specific handling (truncate for display)
        if (error.Length > 300)
        {
            return $"Error: {error.Substring(0, 300)}...\n\n(Try running the command manually for more details)";
        }
        return $"Error: {error}";
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }
}
