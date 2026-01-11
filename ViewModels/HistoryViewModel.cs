using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Models;
using Svns.Services.Svn.Operations;
using Svns.Services.Localization;

namespace Svns.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SvnLogEntry> _logEntries = new();

    [ObservableProperty]
    private SvnLogEntry? _selectedEntry;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _loadedCount;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public HistoryViewModel(WorkingCopyService workingCopyService, string filePath)
    {
        _workingCopyService = workingCopyService;
        FilePath = filePath;
        FileName = System.IO.Path.GetFileName(filePath);
    }

    /// <summary>
    /// Loads the history for the file
    /// </summary>
    public async Task LoadHistoryAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            StatusMessage = Localize.Get("Status.LoadingHistory");

            var logs = await _workingCopyService.GetLogAsync(FilePath, limit: limit, includeChangedPaths: true);

            LogEntries.Clear();
            foreach (var log in logs)
            {
                LogEntries.Add(log);
            }

            LoadedCount = LogEntries.Count;
            StatusMessage = Localize.Get("History.LoadedRevisions", LoadedCount);
        }
        catch (Exception ex)
        {
            StatusMessage = Localize.Get("History.ErrorLoading", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads more history entries
    /// </summary>
    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        await LoadHistoryAsync(LoadedCount + 50);
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
