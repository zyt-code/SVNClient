using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
namespace Svns.Models;

/// <summary>
/// Represents information about a working copy or repository item
/// </summary>
public class SvnInfo
{
    /// <summary>
    /// The path of the item
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The relative path from the repository root
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// The repository root URL
    /// </summary>
    public string RepositoryRootUrl { get; set; } = string.Empty;

    /// <summary>
    /// The repository UUID
    /// </summary>
    public string RepositoryUuid { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the item in the repository
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The working copy root path
    /// </summary>
    public string WorkingCopyRoot { get; set; } = string.Empty;

    /// <summary>
    /// The revision of the item
    /// </summary>
    public long Revision { get; set; }

    /// <summary>
    /// The node kind (file or directory)
    /// </summary>
    public string NodeKind { get; set; } = string.Empty;

    /// <summary>
    /// The schedule state (normal, add, delete)
    /// </summary>
    public string Schedule { get; set; } = string.Empty;

    /// <summary>
    /// The last changed author
    /// </summary>
    public string? LastChangedAuthor { get; set; }

    /// <summary>
    /// The last changed revision
    /// </summary>
    public long LastChangedRevision { get; set; }

    /// <summary>
    /// The last changed date
    /// </summary>
    public DateTime LastChangedDate { get; set; }

    /// <summary>
    /// The repository UUID
    /// </summary>
    public string? RepositoryId { get; set; }

    /// <summary>
    /// The depth of the working copy
    /// </summary>
    public SvnDepth Depth { get; set; }

    /// <summary>
    /// Whether the item has a conflict
    /// </summary>
    public bool HasConflict { get; set; }

    /// <summary>
    /// The conflict old file path
    /// </summary>
    public string? ConflictOld { get; set; }

    /// <summary>
    /// The conflict working file path
    /// </summary>
    public string? ConflictWorking { get; set; }

    /// <summary>
    /// The conflict new file path
    /// </summary>
    public string? ConflictNew { get; set; }

    /// <summary>
    /// Whether the item is copied
    /// </summary>
    public bool IsCopied { get; set; }

    /// <summary>
    /// The copy from URL
    /// </summary>
    public string? CopyFromUrl { get; set; }

    /// <summary>
    /// The copy from revision
    /// </summary>
    public long? CopyFromRevision { get; set; }

    /// <summary>
    /// Whether the item is locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// The lock owner
    /// </summary>
    public string? LockOwner { get; set; }

    /// <summary>
    /// The lock creation date
    /// </summary>
    public DateTime? LockCreationDate { get; set; }

    /// <summary>
    /// The lock comment
    /// </summary>
    public string? LockComment { get; set; }

    /// <summary>
    /// The lock token
    /// </summary>
    public string? LockToken { get; set; }

    /// <summary>
    /// Whether the item is a file
    /// </summary>
    public bool IsFile => NodeKind.Equals("file", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the item is a directory
    /// </summary>
    public bool IsDirectory => NodeKind.Equals("dir", StringComparison.OrdinalIgnoreCase);
}
