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
using Svns.Services.Svn.Operations;
using Svns.Services.Localization;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Blame dialog
/// </summary>
public partial class BlameViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _filePath;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BlameLineItem> _lines = new();

    [ObservableProperty]
    private ObservableCollection<AuthorStatItem> _authorStats = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalLines;

    [ObservableProperty]
    private int _uniqueRevisions;

    [ObservableProperty]
    private int _uniqueAuthors;

    [ObservableProperty]
    private BlameLineItem? _selectedLine;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public BlameViewModel(WorkingCopyService workingCopyService, string filePath)
    {
        _workingCopyService = workingCopyService;
        _filePath = filePath;
        FileName = Path.GetFileName(filePath);
        FullPath = filePath;
    }

    /// <summary>
    /// Loads the blame data for the file
    /// </summary>
    public async Task LoadBlameAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            StatusMessage = Localize.Get("Status.LoadingBlame");

            var result = await _workingCopyService.BlameAsync(_filePath, cancellationToken);

            Lines.Clear();
            AuthorStats.Clear();

            // Generate colors for authors
            var authorColors = GenerateAuthorColors(result.UniqueAuthors.ToList());

            foreach (var line in result.Lines)
            {
                var color = authorColors.TryGetValue(line.Author, out var c) ? c : "#808080";
                Lines.Add(new BlameLineItem
                {
                    LineNumber = line.LineNumber,
                    Revision = line.Revision,
                    Author = line.Author,
                    Date = line.Date,
                    Content = line.Content,
                    AuthorColor = color
                });
            }

            // Calculate author statistics
            foreach (var kvp in result.AuthorLineCount.OrderByDescending(x => x.Value))
            {
                var color = authorColors.TryGetValue(kvp.Key, out var c) ? c : "#808080";
                var percentage = result.Lines.Count > 0 ? (double)kvp.Value / result.Lines.Count * 100 : 0;
                AuthorStats.Add(new AuthorStatItem
                {
                    Author = kvp.Key,
                    LineCount = kvp.Value,
                    Percentage = percentage,
                    Color = color
                });
            }

            TotalLines = result.Lines.Count;
            UniqueRevisions = result.UniqueRevisions.Count();
            UniqueAuthors = result.UniqueAuthors.Count();

            StatusMessage = Localize.Get("Blame.Loaded", TotalLines, UniqueRevisions, UniqueAuthors);
        }
        catch (Exception ex)
        {
            StatusMessage = Localize.Get("Blame.ErrorLoading", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Generates distinct colors for each author
    /// </summary>
    private Dictionary<string, string> GenerateAuthorColors(List<string> authors)
    {
        var colors = new[]
        {
            "#4CAF50", "#2196F3", "#FF9800", "#9C27B0",
            "#E91E63", "#00BCD4", "#8BC34A", "#673AB7",
            "#009688", "#FF5722", "#3F51B5", "#CDDC39",
            "#795548", "#607D8B", "#F44336", "#03A9F4"
        };

        var result = new Dictionary<string, string>();
        for (int i = 0; i < authors.Count; i++)
        {
            result[authors[i]] = colors[i % colors.Length];
        }
        return result;
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

/// <summary>
/// Represents a blame line item for display
/// </summary>
public class BlameLineItem
{
    public int LineNumber { get; set; }
    public long Revision { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorColor { get; set; } = "#808080";

    public string DisplayRevision => $"r{Revision}";
    public string DisplayDate => Date.ToString("yyyy-MM-dd");
}

/// <summary>
/// Represents author statistics for the blame view
/// </summary>
public class AuthorStatItem
{
    public string Author { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public double Percentage { get; set; }
    public string Color { get; set; } = "#808080";

    public string DisplayPercentage => $"{Percentage:F1}%";
}
