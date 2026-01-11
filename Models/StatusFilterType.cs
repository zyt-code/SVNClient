namespace Svns.Models;

/// <summary>
/// Status filter options for file tree
/// </summary>
public enum StatusFilterType
{
    /// <summary>
    /// Show all files
    /// </summary>
    All,

    /// <summary>
    /// Show only modified files
    /// </summary>
    Modified,

    /// <summary>
    /// Show only added files
    /// </summary>
    Added,

    /// <summary>
    /// Show only deleted files
    /// </summary>
    Deleted,

    /// <summary>
    /// Show only conflicted files
    /// </summary>
    Conflicted,

    /// <summary>
    /// Show only unversioned files
    /// </summary>
    Unversioned,

    /// <summary>
    /// Show files with any local modifications
    /// </summary>
    LocalChanges
}
