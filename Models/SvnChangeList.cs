using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
namespace Svns.Models;

/// <summary>
/// Represents a collection of changes for a commit operation
/// </summary>
public class SvnChangeList
{
    /// <summary>
    /// The name of the changelist
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The files in this changelist
    /// </summary>
    public List<SvnStatus> Files { get; set; } = new();

    /// <summary>
    /// The commit message
    /// </summary>
    public string CommitMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets the number of files in the changelist
    /// </summary>
    public int FileCount => Files.Count;

    /// <summary>
    /// Gets the number of modified files
    /// </summary>
    public int ModifiedCount => Files.Count(f => f.WorkingCopyStatus == SvnStatusType.Modified);

    /// <summary>
    /// Gets the number of added files
    /// </summary>
    public int AddedCount => Files.Count(f => f.WorkingCopyStatus == SvnStatusType.Added);

    /// <summary>
    /// Gets the number of deleted files
    /// </summary>
    public int DeletedCount => Files.Count(f => f.WorkingCopyStatus == SvnStatusType.Deleted);

    /// <summary>
    /// Gets the number of conflicted files
    /// </summary>
    public int ConflictedCount => Files.Count(f => f.WorkingCopyStatus == SvnStatusType.Conflicted);

    /// <summary>
    /// Gets whether there are any conflicts
    /// </summary>
    public bool HasConflicts => ConflictedCount > 0;

    /// <summary>
    /// Gets a summary of the changes
    /// </summary>
    public string Summary
    {
        get
        {
            var parts = new List<string>();
            if (ModifiedCount > 0)
                parts.Add($"{ModifiedCount} modified");
            if (AddedCount > 0)
                parts.Add($"{AddedCount} added");
            if (DeletedCount > 0)
                parts.Add($"{DeletedCount} deleted");
            if (ConflictedCount > 0)
                parts.Add($"{ConflictedCount} conflicted");

            return parts.Count > 0 ? string.Join(", ", parts) : "No changes";
        }
    }

    /// <summary>
    /// Adds a file to the changelist
    /// </summary>
    public void AddFile(SvnStatus file)
    {
        Files.Add(file);
    }

    /// <summary>
    /// Removes a file from the changelist
    /// </summary>
    public bool RemoveFile(string path)
    {
        var file = Files.FirstOrDefault(f => f.Path == path);
        if (file != null)
        {
            return Files.Remove(file);
        }
        return false;
    }

    /// <summary>
    /// Clears all files from the changelist
    /// </summary>
    public void ClearFiles()
    {
        Files.Clear();
    }

    /// <summary>
    /// Gets whether the changelist is empty
    /// </summary>
    public bool IsEmpty => Files.Count == 0;

    /// <summary>
    /// Gets whether the changelist has conflicts (which would prevent commit)
    /// </summary>
    public bool CanCommit => !HasConflicts && !IsEmpty;
}
