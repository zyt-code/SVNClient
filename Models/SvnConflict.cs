namespace Svns.Models;

/// <summary>
/// Represents a conflict in a working copy
/// </summary>
public class SvnConflict
{
    /// <summary>
    /// The file path with the conflict
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The conflict type
    /// </summary>
    public SvnConflictType ConflictType { get; set; }

    /// <summary>
    /// The repository location that caused the conflict
    /// </summary>
    public string? RepositoryLocation { get; set; }

    /// <summary>
    /// The base file (the common ancestor)
    /// </summary>
    public string? BaseFile { get; set; }

    /// <summary>
    /// The their file (the repository version)
    /// </summary>
    public string? TheirFile { get; set; }

    /// <summary>
    /// The my file (the working copy version)
    /// </summary>
    public string? MyFile { get; set; }

    /// <summary>
    /// The merged file (the result of automatic merge)
    /// </summary>
    public string? MergedFile { get; set; }

    /// <summary>
    /// The conflict description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the conflict is property-related
    /// </summary>
    public bool IsPropertyConflict { get; set; }

    /// <summary>
    /// Whether the conflict is binary
    /// </summary>
    public bool IsBinary { get; set; }

    /// <summary>
    /// The property name (if property conflict)
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Whether the conflict is resolved
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// The resolution method used
    /// </summary>
    public SvnConflictResolution? ResolutionMethod { get; set; }

    /// <summary>
    /// Gets the file name without path
    /// </summary>
    public string FileName => System.IO.Path.GetFileName(Path);
}

/// <summary>
/// SVN conflict types
/// </summary>
public enum SvnConflictType
{
    /// <summary>
    /// Text conflict (content conflict)
    /// </summary>
    Text,

    /// <summary>
    /// Property conflict
    /// </summary>
    Property,

    /// <summary>
    /// Tree conflict
    /// </summary>
    Tree,

    /// <summary>
    /// Binary conflict
    /// </summary>
    Binary
}

/// <summary>
/// SVN conflict resolution methods
/// </summary>
public enum SvnConflictResolution
{
    /// <summary>
    /// Use the working copy version
    /// </summary>
    Working,

    /// <summary>
    /// Use the base version
    /// </summary>
    Base,

    /// <summary>
    /// Use the repository version
    /// </summary>
    Repository,

    /// <summary>
    /// Use the merged version
    /// </summary>
    Merged,

    /// <summary>
    /// Manually resolved
    /// </summary>
    Manual,

    /// <summary>
    /// Postpone resolution
    /// </summary>
    Postpone
}
