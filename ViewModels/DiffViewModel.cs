using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Svns.ViewModels;

public partial class DiffViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _diffContent = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DiffLine> _diffLines = new();

    [ObservableProperty]
    private int _addedLines;

    [ObservableProperty]
    private int _removedLines;

    [ObservableProperty]
    private int _currentDiffIndex;

    [ObservableProperty]
    private int _totalDiffs;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public DiffViewModel()
    {
    }

    public DiffViewModel(string filePath, string diffContent)
    {
        FilePath = filePath;
        FileName = System.IO.Path.GetFileName(filePath);
        DiffContent = diffContent;
        ParseDiff(diffContent);
    }

    /// <summary>
    /// Parses the diff content into structured lines
    /// </summary>
    private void ParseDiff(string content)
    {
        DiffLines.Clear();
        AddedLines = 0;
        RemovedLines = 0;
        TotalDiffs = 0;

        if (string.IsNullOrEmpty(content))
            return;

        var lines = content.Split('\n');
        int oldLineNum = 0;
        int newLineNum = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd('\r');
            var diffLine = new DiffLine { Content = line };

            if (line.StartsWith("@@"))
            {
                // Parse hunk header: @@ -start,count +start,count @@
                diffLine.Type = DiffLineType.Header;
                TotalDiffs++;

                var match = Regex.Match(line, @"@@ -(\d+)(?:,\d+)? \+(\d+)(?:,\d+)? @@");
                if (match.Success)
                {
                    oldLineNum = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    newLineNum = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                }
            }
            else if (line.StartsWith("---") || line.StartsWith("+++"))
            {
                diffLine.Type = DiffLineType.FileHeader;
            }
            else if (line.StartsWith("-"))
            {
                diffLine.Type = DiffLineType.Removed;
                diffLine.OldLineNumber = oldLineNum++;
                RemovedLines++;
            }
            else if (line.StartsWith("+"))
            {
                diffLine.Type = DiffLineType.Added;
                diffLine.NewLineNumber = newLineNum++;
                AddedLines++;
            }
            else if (line.StartsWith(" ") || line.Length == 0)
            {
                diffLine.Type = DiffLineType.Context;
                diffLine.OldLineNumber = oldLineNum++;
                diffLine.NewLineNumber = newLineNum++;
            }
            else
            {
                diffLine.Type = DiffLineType.Info;
            }

            DiffLines.Add(diffLine);
        }
    }

    /// <summary>
    /// Navigates to the previous diff hunk
    /// </summary>
    [RelayCommand]
    private void PreviousDiff()
    {
        if (CurrentDiffIndex > 0)
        {
            CurrentDiffIndex--;
        }
    }

    /// <summary>
    /// Navigates to the next diff hunk
    /// </summary>
    [RelayCommand]
    private void NextDiff()
    {
        if (CurrentDiffIndex < TotalDiffs - 1)
        {
            CurrentDiffIndex++;
        }
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
/// Represents a line in a diff
/// </summary>
public class DiffLine
{
    public string Content { get; set; } = string.Empty;
    public DiffLineType Type { get; set; }
    public int? OldLineNumber { get; set; }
    public int? NewLineNumber { get; set; }

    public string DisplayOldLineNumber => OldLineNumber?.ToString() ?? "";
    public string DisplayNewLineNumber => NewLineNumber?.ToString() ?? "";
    public string DisplayContent => Type == DiffLineType.Context ? Content.Substring(1) :
                                    Type == DiffLineType.Added ? Content.Substring(1) :
                                    Type == DiffLineType.Removed ? Content.Substring(1) :
                                    Content;
}

/// <summary>
/// Type of diff line
/// </summary>
public enum DiffLineType
{
    Context,
    Added,
    Removed,
    Header,
    FileHeader,
    Info
}
