using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace Svns.Models;

/// <summary>
/// Represents a single line in a blame (annotate) output
/// </summary>
public class SvnBlameLine
{
    /// <summary>
    /// The line number (1-indexed)
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// The revision that last modified this line
    /// </summary>
    public long Revision { get; set; }

    /// <summary>
    /// The author who last modified this line
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The date of the last modification
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The line content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this line was merged
    /// </summary>
    public bool IsMerged { get; set; }
}

/// <summary>
/// Represents the result of a blame operation
/// </summary>
public class SvnBlameResult
{
    /// <summary>
    /// The file path
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The lines of the file
    /// </summary>
    public List<SvnBlameLine> Lines { get; set; } = new();

    /// <summary>
    /// The revision being blamed
    /// </summary>
    public long Revision { get; set; }

    /// <summary>
    /// Gets all unique revisions in the blame
    /// </summary>
    public IEnumerable<long> UniqueRevisions => Lines
        .Select(l => l.Revision)
        .Distinct()
        .OrderBy(r => r);

    /// <summary>
    /// Gets all unique authors in the blame
    /// </summary>
    public IEnumerable<string> UniqueAuthors => Lines
        .Select(l => l.Author)
        .Distinct()
        .OrderBy(a => a);

    /// <summary>
    /// Gets the number of lines modified by each author
    /// </summary>
    public Dictionary<string, int> AuthorLineCount
    {
        get
        {
            return Lines
                .GroupBy(l => l.Author)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }

    /// <summary>
    /// Gets the date range of the blame
    /// </summary>
    public (DateTime? Start, DateTime? End) DateRange
    {
        get
        {
            if (Lines.Count == 0)
                return (null, null);

            return (Lines.Min(l => l.Date), Lines.Max(l => l.Date));
        }
    }
}
