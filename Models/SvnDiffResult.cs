using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
namespace Svns.Models;

/// <summary>
/// Represents the result of an SVN diff operation
/// </summary>
public class SvnDiffResult
{
    /// <summary>
    /// The path being compared
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The original file path (for display)
    /// </summary>
    public string OriginalPath { get; set; } = string.Empty;

    /// <summary>
    /// The modified file path (for display)
    /// </summary>
    public string ModifiedPath { get; set; } = string.Empty;

    /// <summary>
    /// The start revision (null for working copy)
    /// </summary>
    public long? StartRevision { get; set; }

    /// <summary>
    /// The end revision (null for working copy)
    /// </summary>
    public long? EndRevision { get; set; }

    /// <summary>
    /// The type of diff (unified, etc.)
    /// </summary>
    public string DiffType { get; set; } = "unified";

    /// <summary>
    /// The diff lines
    /// </summary>
    public List<SvnDiffLine> Lines { get; set; } = new();

    /// <summary>
    /// Whether this is a binary diff
    /// </summary>
    public bool IsBinary { get; set; }

    /// <summary>
    /// The binary diff message
    /// </summary>
    public string? BinaryMessage { get; set; }

    /// <summary>
    /// The total number of additions
    /// </summary>
    public int AdditionCount => Lines.Count(l => l.Type == SvnDiffLineType.Addition);

    /// <summary>
    /// The total number of deletions
    /// </summary>
    public int DeletionCount => Lines.Count(l => l.Type == SvnDiffLineType.Deletion);

    /// <summary>
    /// The total number of modifications
    /// </summary>
    public int ModificationCount => Lines.Count(l => l.Type == SvnDiffLineType.Modification);

    /// <summary>
    /// Gets the file extension
    /// </summary>
    public string? FileExtension
    {
        get
        {
            var lastDot = Path.LastIndexOf('.');
            return lastDot >= 0 ? Path.Substring(lastDot) : null;
        }
    }
}

/// <summary>
/// Represents a line in a diff
/// </summary>
public class SvnDiffLine
{
    /// <summary>
    /// The line type
    /// </summary>
    public SvnDiffLineType Type { get; set; }

    /// <summary>
    /// The original line number (null for context lines)
    /// </summary>
    public int? OriginalLineNumber { get; set; }

    /// <summary>
    /// The modified line number (null for context lines)
    /// </summary>
    public int? ModifiedLineNumber { get; set; }

    /// <summary>
    /// The line content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a context line
    /// </summary>
    public bool IsContext => Type == SvnDiffLineType.Context;

    /// <summary>
    /// Whether this is an addition
    /// </summary>
    public bool IsAddition => Type == SvnDiffLineType.Addition;

    /// <summary>
    /// Whether this is a deletion
    /// </summary>
    public bool IsDeletion => Type == SvnDiffLineType.Deletion;

    /// <summary>
    /// Whether this is a header line
    /// </summary>
    public bool IsHeader => Type == SvnDiffLineType.Header;

    /// <summary>
    /// Whether this is a hunk header
    /// </summary>
    public bool IsHunkHeader => Type == SvnDiffLineType.HunkHeader;
}

/// <summary>
/// SVN diff line types
/// </summary>
public enum SvnDiffLineType
{
    /// <summary>
    /// Context line (unchanged)
    /// </summary>
    Context,

    /// <summary>
    /// Addition line (added in new version)
    /// </summary>
    Addition,

    /// <summary>
    /// Deletion line (removed from old version)
    /// </summary>
    Deletion,

    /// <summary>
    /// Header line (file headers)
    /// </summary>
    Header,

    /// <summary>
    /// Hunk header (@@ line)
    /// </summary>
    HunkHeader,

    /// <summary>
    /// Modification (both addition and deletion)
    /// </summary>
    Modification
}
