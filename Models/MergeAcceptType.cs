namespace Svns.Models;

/// <summary>
/// Specifies how to handle conflicts during svn merge operations
/// </summary>
public enum MergeAcceptType
{
    /// <summary>
    /// Mark the conflict to be resolved later (default)
    /// </summary>
    Postpone,

    /// <summary>
    /// Accept the base revision (discard both local and incoming changes)
    /// </summary>
    Base,

    /// <summary>
    /// Accept my version for conflicting hunks only
    /// </summary>
    MineConflict,

    /// <summary>
    /// Accept their version for conflicting hunks only
    /// </summary>
    TheirsConflict,

    /// <summary>
    /// Accept my version entirely
    /// </summary>
    MineFull,

    /// <summary>
    /// Accept their version entirely
    /// </summary>
    TheirsFull,

    /// <summary>
    /// Edit the file in place to resolve conflicts
    /// </summary>
    Edit,

    /// <summary>
    /// Launch external merge tool
    /// </summary>
    Launch
}

/// <summary>
/// Extension methods for MergeAcceptType
/// </summary>
public static class MergeAcceptTypeExtensions
{
    /// <summary>
    /// Converts the enum to the SVN command line argument value
    /// </summary>
    public static string ToSvnArgument(this MergeAcceptType acceptType)
    {
        return acceptType switch
        {
            MergeAcceptType.Postpone => "postpone",
            MergeAcceptType.Base => "base",
            MergeAcceptType.MineConflict => "mine-conflict",
            MergeAcceptType.TheirsConflict => "theirs-conflict",
            MergeAcceptType.MineFull => "mine-full",
            MergeAcceptType.TheirsFull => "theirs-full",
            MergeAcceptType.Edit => "edit",
            MergeAcceptType.Launch => "launch",
            _ => "postpone"
        };
    }

    /// <summary>
    /// Gets a human-readable description of the accept type
    /// </summary>
    public static string GetDescription(this MergeAcceptType acceptType)
    {
        return acceptType switch
        {
            MergeAcceptType.Postpone => "Postpone (resolve later)",
            MergeAcceptType.Base => "Base (discard all changes)",
            MergeAcceptType.MineConflict => "Mine (keep my changes for conflicts)",
            MergeAcceptType.TheirsConflict => "Theirs (keep their changes for conflicts)",
            MergeAcceptType.MineFull => "Mine Full (keep all my changes)",
            MergeAcceptType.TheirsFull => "Theirs Full (keep all their changes)",
            MergeAcceptType.Edit => "Edit (manual edit)",
            MergeAcceptType.Launch => "Launch (external tool)",
            _ => "Postpone"
        };
    }
}
