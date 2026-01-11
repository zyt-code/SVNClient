using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
namespace Svns.Models;

/// <summary>
/// Represents information about a working copy
/// </summary>
public class WorkingCopyInfo
{
    /// <summary>
    /// The root path of the working copy
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the repository
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// The repository root URL
    /// </summary>
    public string RepositoryRoot { get; set; } = string.Empty;

    /// <summary>
    /// The repository UUID
    /// </summary>
    public string RepositoryUuid { get; set; } = string.Empty;

    /// <summary>
    /// The current revision
    /// </summary>
    public long Revision { get; set; }

    /// <summary>
    /// The last changed revision
    /// </summary>
    public long LastChangedRevision { get; set; }

    /// <summary>
    /// The last changed author
    /// </summary>
    public string? LastChangedAuthor { get; set; }

    /// <summary>
    /// The last changed date
    /// </summary>
    public DateTime LastChangedDate { get; set; }

    /// <summary>
    /// The relative path from repository root
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether the working copy has uncommitted changes
    /// </summary>
    public bool HasUncommittedChanges { get; set; }

    /// <summary>
    /// The number of modified files
    /// </summary>
    public int ModifiedFileCount { get; set; }

    /// <summary>
    /// The number of added files
    /// </summary>
    public int AddedFileCount { get; set; }

    /// <summary>
    /// The number of deleted files
    /// </summary>
    public int DeletedFileCount { get; set; }

    /// <summary>
    /// The number of conflicted files
    /// </summary>
    public int ConflictedFileCount { get; set; }

    /// <summary>
    /// Whether the working copy has any conflicts
    /// </summary>
    public bool HasConflicts => ConflictedFileCount > 0;

    /// <summary>
    /// The working copy format version
    /// </summary>
    public int? FormatVersion { get; set; }

    /// <summary>
    /// The depth of the working copy
    /// </summary>
    public SvnDepth Depth { get; set; }

    /// <summary>
    /// Gets the display name for the working copy
    /// </summary>
    public string DisplayName => System.IO.Path.GetFileName(Path) ?? Path;

    /// <summary>
    /// Gets a summary of changes
    /// </summary>
    public string ChangesSummary
    {
        get
        {
            if (!HasUncommittedChanges)
                return "No uncommitted changes";

            var parts = new List<string>();
            if (ModifiedFileCount > 0)
                parts.Add($"{ModifiedFileCount} modified");
            if (AddedFileCount > 0)
                parts.Add($"{AddedFileCount} added");
            if (DeletedFileCount > 0)
                parts.Add($"{DeletedFileCount} deleted");
            if (ConflictedFileCount > 0)
                parts.Add($"{ConflictedFileCount} conflicted");

            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Gets the repository branch/tag name from the URL
    /// </summary>
    public string? BranchName
    {
        get
        {
            if (string.IsNullOrEmpty(RepositoryUrl))
                return null;

            var segments = RepositoryUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments[^1] : null;
        }
    }
}
