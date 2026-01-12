using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Svns.Models;

/// <summary>
/// Represents the status of a file or directory in a working copy
/// </summary>
public partial class SvnStatus : ObservableObject
{
    /// <summary>
    /// The file system path
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Child items (for directory nodes in the tree)
    /// </summary>
    public ObservableCollection<SvnStatus> Children { get; set; } = new();

    /// <summary>
    /// Display name (file or folder name)
    /// </summary>
    public string Name => System.IO.Path.GetFileName(Path) ?? Path;

    /// <summary>
    /// File name for display
    /// </summary>
    public string FileName => Name;

    /// <summary>
    /// Whether this item is a file (vs directory)
    /// </summary>
    public bool IsFile => NodeType == SvnNodeType.File;

    /// <summary>
    /// Whether this item is selected for commit
    /// </summary>
    [ObservableProperty]
    private bool _isSelected = true;

    /// <summary>
    /// Whether this item has a meaningful status to display (not Normal/None/Unversioned)
    /// </summary>
    public bool ShowStatusBadge => HasLocalModifications ||
        WorkingCopyStatus == SvnStatusType.Conflicted ||
        WorkingCopyStatus == SvnStatusType.Merged ||
        WorkingCopyStatus == SvnStatusType.Replaced;

    /// <summary>
    /// The working copy status (first column in svn status output)
    /// </summary>
    public SvnStatusType WorkingCopyStatus { get; set; }

    /// <summary>
    /// The repository status (second column in svn status output)
    /// </summary>
    public SvnStatusType RepositoryStatus { get; set; }

    /// <summary>
    /// The working copy revision
    /// </summary>
    public long? WorkingCopyRevision { get; set; }

    /// <summary>
    /// The last changed revision
    /// </summary>
    public long? LastChangedRevision { get; set; }

    /// <summary>
    /// The author of the last change
    /// </summary>
    public string? LastChangedAuthor { get; set; }

    /// <summary>
    /// Whether the item is locked
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Whether the item has a conflict
    /// </summary>
    public bool HasConflict { get; set; }

    /// <summary>
    /// Whether the item is switched
    /// </summary>
    public bool IsSwitched { get; set; }

    /// <summary>
    /// The properties status (seventh column)
    /// </summary>
    public SvnStatusType PropertyStatus { get; set; }

    /// <summary>
    /// Whether this is a file or directory
    /// </summary>
    public SvnNodeType NodeType { get; set; }

    /// <summary>
    /// The depth of the item (for directories)
    /// </summary>
    public SvnDepth Depth { get; set; }

    /// <summary>
    /// The tree conflict status
    /// </summary>
    public string? TreeConflict { get; set; }

    /// <summary>
    /// Whether the item has local modifications
    /// </summary>
    public bool HasLocalModifications => SvnFileStateMachine.HasLocalModifications(WorkingCopyStatus);

    /// <summary>
    /// Gets the display status (combining working copy and repository status)
    /// </summary>
    public string DisplayStatus
    {
        get
        {
            if (WorkingCopyStatus != SvnStatusType.None)
            {
                return WorkingCopyStatus.ToString();
            }

            if (RepositoryStatus != SvnStatusType.None)
            {
                return $"{RepositoryStatus} (out of date)";
            }

            return "Normal";
        }
    }

    /// <summary>
    /// Gets the status text for display in badges
    /// </summary>
    public string StatusText => WorkingCopyStatus switch
    {
        SvnStatusType.Modified => "Modified",
        SvnStatusType.Added => "Added",
        SvnStatusType.Deleted => "Deleted",
        SvnStatusType.Conflicted => "Conflicted",
        SvnStatusType.Replaced => "Replaced",
        SvnStatusType.Missing => "Missing",
        SvnStatusType.Unversioned => "Unversioned",
        SvnStatusType.Ignored => "Ignored",
        _ => "Normal"
    };
}

/// <summary>
/// SVN status types
/// </summary>
public enum SvnStatusType
{
    /// <summary>
    /// No modifications
    /// </summary>
    None = ' ',

    /// <summary>
    /// Item is added
    /// </summary>
    Added = 'A',

    /// <summary>
    /// Item is blocked (obstruction)
    /// </summary>
    Blocked = '~',

    /// <summary>
    /// Item is deleted
    /// </summary>
    Deleted = 'D',

    /// <summary>
    /// Item is conflicted
    /// </summary>
    Conflicted = 'C',

    /// <summary>
    /// Item is ignored
    /// </summary>
    Ignored = 'I',

    /// <summary>
    /// Item is merged
    /// </summary>
    Merged = 'G',

    /// <summary>
    /// Item is modified
    /// </summary>
    Modified = 'M',

    /// <summary>
    /// Item was replaced (another version with same name)
    /// </summary>
    Replaced = 'R',

    /// <summary>
    /// Item is absent
    /// </summary>
    Absent = '!',

    /// <summary>
    /// Item is under version control
    /// </summary>
    Normal = ' ',

    /// <summary>
    /// Item is not under version control
    /// </summary>
    Unversioned = '?',

    /// <summary>
    /// Item is missing
    /// </summary>
    Missing = '!',

    /// <summary>
    /// Item is obstructed
    /// </summary>
    Obstructed = '~',

    /// <summary>
    /// Item has property modifications
    /// </summary>
    PropertyModified = 'M',

    /// <summary>
    /// Item is an external
    /// </summary>
    External = 'X',

    /// <summary>
    /// Item is incomplete
    /// </summary>
    Incomplete = 'L'
}

/// <summary>
/// SVN node types
/// </summary>
public enum SvnNodeType
{
    /// <summary>
    /// File
    /// </summary>
    File,

    /// <summary>
    /// Directory
    /// </summary>
    Directory
}

/// <summary>
/// SVN depth types
/// </summary>
public enum SvnDepth
{
    /// <summary>
    /// Depth not specified
    /// </summary>
    Unknown,

    /// <summary>
    /// Exclude this item
    /// </summary>
    Exclude,

    /// <summary>
    /// Only this item
    /// </summary>
    Empty,

    /// <summary>
    /// This item and its file children
    /// </summary>
    Files,

    /// <summary>
    /// This item and all immediate children
    /// </summary>
    Immediates,

    /// <summary>
    /// This item and all descendants
    /// </summary>
    Infinity
}
