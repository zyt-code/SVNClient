using System.Collections.ObjectModel;
using System;

namespace Svns.Models;

/// <summary>
/// Represents a SVN log entry (commit)
/// </summary>
public class SvnLogEntry
{
    /// <summary>
    /// The revision number
    /// </summary>
    public long Revision { get; set; }

    /// <summary>
    /// The author who made the commit
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The date of the commit
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The commit message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Changed paths in this commit
    /// </summary>
    public ObservableCollection<SvnChangedPath> ChangedPaths { get; set; } = new();

    /// <summary>
    /// Formatted date string for display
    /// </summary>
    public string DisplayDate => FormatDate(Date);

    /// <summary>
    /// Formatted revision string for display
    /// </summary>
    public string DisplayRevision => $"r{Revision}";

    /// <summary>
    /// Number of changed paths
    /// </summary>
    public int ChangedPathCount => ChangedPaths.Count;

    /// <summary>
    /// Whether there are any changed paths
    /// </summary>
    public bool HasChangedPaths => ChangedPaths.Count > 0;

    private string FormatDate(DateTime date)
    {
        // Convert UTC to local time if the date is UTC
        var localDate = date.Kind == DateTimeKind.Utc
            ? date.ToLocalTime()
            : date;

        var now = DateTime.Now;
        var span = now - localDate;

        if (span.TotalDays < 1)
        {
            if (span.TotalHours < 1)
            {
                return $"{Math.Floor(span.TotalMinutes)} minutes ago";
            }
            return $"{Math.Floor(span.TotalHours)} hours ago";
        }
        else if (span.TotalDays < 7)
        {
            return $"{Math.Floor(span.TotalDays)} days ago";
        }
        else
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}

/// <summary>
/// Represents a changed path in a SVN log entry
/// </summary>
public class SvnChangedPath
{
    /// <summary>
    /// The path that was changed
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The type of modification
    /// </summary>
    public SvnPathAction Action { get; set; }

    /// <summary>
    /// Whether the copy was from a different path
    /// </summary>
    public string? CopyFromPath { get; set; }

    /// <summary>
    /// The revision from which the copy was made
    /// </summary>
    public long? CopyFromRevision { get; set; }

    /// <summary>
    /// Gets the file name from the path
    /// </summary>
    public string FileName => System.IO.Path.GetFileName(Path) ?? Path;
}

/// <summary>
/// Types of path modifications in SVN
/// </summary>
public enum SvnPathAction
{
    /// <summary>
    /// Path was added
    /// </summary>
    Added,

    /// <summary>
    /// Path was deleted
    /// </summary>
    Deleted,

    /// <summary>
    /// Path was modified
    /// </summary>
    Modified,

    /// <summary>
    /// Path was replaced
    /// </summary>
    Replaced
}
