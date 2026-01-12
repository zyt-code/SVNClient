using System;
using System.Collections.Generic;
using System.Linq;

namespace Svns.Models;

/// <summary>
/// Defines all possible SVN file operations
/// </summary>
public enum SvnFileAction
{
    // Basic operations
    Add,
    Delete,
    Revert,
    Commit,
    Update,

    // Content modifications
    Modify,
    Replace,

    // Conflict resolution
    Resolve,
    MarkResolved,

    // Special operations
    Ignore,
    Unignore
}

/// <summary>
/// State machine for SVN file status transitions
/// Defines valid state transitions and the operations that cause them
/// </summary>
public static class SvnFileStateMachine
{
    // Maps from current state + action -> next state
    private static readonly Dictionary<SvnStatusType, Dictionary<SvnFileAction, SvnStatusType>> _transitions = new()
    {
        // Normal state (under version control, no modifications)
        {
            SvnStatusType.Normal, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Modify, SvnStatusType.Modified },
                { SvnFileAction.Delete, SvnStatusType.Deleted },
                { SvnFileAction.Replace, SvnStatusType.Replaced },
            }
        },

        // Modified state
        {
            SvnStatusType.Modified, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Commit, SvnStatusType.Normal },
                { SvnFileAction.Revert, SvnStatusType.Normal },
                { SvnFileAction.Delete, SvnStatusType.Deleted },
                { SvnFileAction.Update, SvnStatusType.Conflicted }, // Potentially
            }
        },

        // Added state (scheduled for addition)
        {
            SvnStatusType.Added, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Commit, SvnStatusType.Normal },
                { SvnFileAction.Revert, SvnStatusType.Unversioned }, // Revert addition
                { SvnFileAction.Delete, SvnStatusType.Unversioned }, // Cancel addition
                { SvnFileAction.Modify, SvnStatusType.Added }, // Still added
            }
        },

        // Deleted state (scheduled for deletion)
        {
            SvnStatusType.Deleted, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Commit, SvnStatusType.Normal }, // Committed deletion
                { SvnFileAction.Revert, SvnStatusType.Normal }, // Restore
                { SvnFileAction.Modify, SvnStatusType.Deleted }, // Still deleted
            }
        },

        // Replaced state
        {
            SvnStatusType.Replaced, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Commit, SvnStatusType.Normal },
                { SvnFileAction.Revert, SvnStatusType.Normal },
            }
        },

        // Conflicted state
        {
            SvnStatusType.Conflicted, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Resolve, SvnStatusType.Modified }, // After resolving
                { SvnFileAction.MarkResolved, SvnStatusType.Modified },
                { SvnFileAction.Revert, SvnStatusType.Normal }, // Discard changes
                { SvnFileAction.Delete, SvnStatusType.Deleted },
            }
        },

        // Unversioned state
        {
            SvnStatusType.Unversioned, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Add, SvnStatusType.Added },
                { SvnFileAction.Ignore, SvnStatusType.Ignored },
            }
        },

        // Ignored state
        {
            SvnStatusType.Ignored, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Unignore, SvnStatusType.Unversioned },
            }
        },

        // Missing state (version control knows but file is gone)
        {
            SvnStatusType.Missing, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Revert, SvnStatusType.Normal }, // Restore
                { SvnFileAction.Delete, SvnStatusType.Deleted }, // Schedule deletion
                { SvnFileAction.Update, SvnStatusType.Normal }, // Restore
            }
        },

        // Obstructed state
        {
            SvnStatusType.Obstructed, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Revert, SvnStatusType.Normal },
            }
        },

        // Incomplete state
        {
            SvnStatusType.Incomplete, new Dictionary<SvnFileAction, SvnStatusType>
            {
                { SvnFileAction.Update, SvnStatusType.Normal },
            }
        }
    };

    /// <summary>
    /// Gets the next state after performing an action on the current state
    /// </summary>
    /// <param name="currentState">Current SVN status</param>
    /// <param name="action">Action to perform</param>
    /// <returns>Next state, or null if transition is not valid</returns>
    public static SvnStatusType? GetNextState(SvnStatusType currentState, SvnFileAction action)
    {
        if (_transitions.TryGetValue(currentState, out var actionMap))
        {
            if (actionMap.TryGetValue(action, out var nextState))
            {
                return nextState;
            }
        }
        return null; // Invalid transition
    }

    /// <summary>
    /// Checks if a transition is valid
    /// </summary>
    public static bool IsValidTransition(SvnStatusType currentState, SvnFileAction action)
    {
        return GetNextState(currentState, action) != null;
    }

    /// <summary>
    /// Gets all valid actions for a given state
    /// </summary>
    public static IEnumerable<SvnFileAction> GetValidActions(SvnStatusType currentState)
    {
        if (_transitions.TryGetValue(currentState, out var actionMap))
        {
            return actionMap.Keys;
        }
        return Enumerable.Empty<SvnFileAction>();
    }

    /// <summary>
    /// Gets the recommended action to perform on a file with the given status
    /// </summary>
    public static SvnFileAction? GetRecommendedAction(SvnStatusType status)
    {
        return status switch
        {
            SvnStatusType.Unversioned => SvnFileAction.Add,
            SvnStatusType.Modified => SvnFileAction.Commit,
            SvnStatusType.Added => SvnFileAction.Commit,
            SvnStatusType.Deleted => SvnFileAction.Commit,
            SvnStatusType.Replaced => SvnFileAction.Commit,
            SvnStatusType.Conflicted => SvnFileAction.Resolve,
            SvnStatusType.Missing => SvnFileAction.Revert,
            SvnStatusType.Obstructed => SvnFileAction.Revert,
            SvnStatusType.Incomplete => SvnFileAction.Update,
            _ => null
        };
    }

    /// <summary>
    /// Checks if a file can be deleted given its current status
    /// </summary>
    public static bool CanDelete(SvnStatusType status)
    {
        return status switch
        {
            SvnStatusType.Normal => true,      // Can delete versioned file
            SvnStatusType.Modified => true,     // Can delete modified file
            SvnStatusType.Added => true,       // Can cancel addition
            SvnStatusType.Replaced => true,    // Can delete replaced file
            SvnStatusType.Conflicted => true,  // Can delete conflicted file
            SvnStatusType.Unversioned => true, // Direct filesystem delete
            SvnStatusType.Missing => true,     // Can schedule deletion
            SvnStatusType.Ignored => true,     // Direct filesystem delete
            SvnStatusType.Deleted => false,    // Already deleted
            SvnStatusType.Incomplete => false,
            SvnStatusType.Obstructed => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if a file can be reverted given its current status
    /// </summary>
    public static bool CanRevert(SvnStatusType status)
    {
        return status switch
        {
            SvnStatusType.Modified => true,
            SvnStatusType.Added => true,     // Cancel addition
            SvnStatusType.Deleted => true,   // Restore deleted file
            SvnStatusType.Replaced => true,
            SvnStatusType.Conflicted => true,
            SvnStatusType.Missing => true,   // Restore missing file
            SvnStatusType.Obstructed => true,
            SvnStatusType.Normal => false,
            SvnStatusType.Unversioned => false,
            SvnStatusType.Ignored => false,
            SvnStatusType.Incomplete => false,
            _ => false
        };
    }

    /// <summary>
    /// Checks if a file can be committed given its current status
    /// </summary>
    public static bool CanCommit(SvnStatusType status)
    {
        return status switch
        {
            SvnStatusType.Modified => true,
            SvnStatusType.Added => true,
            SvnStatusType.Deleted => true,
            SvnStatusType.Replaced => true,
            SvnStatusType.Conflicted => false, // Must resolve first
            SvnStatusType.Normal => false,
            SvnStatusType.Unversioned => false,
            SvnStatusType.Missing => false,
            SvnStatusType.Ignored => false,
            SvnStatusType.Incomplete => false,
            SvnStatusType.Obstructed => false,
            _ => false
        };
    }

    /// <summary>
    /// Checks if a file has local modifications that should be committed
    /// </summary>
    public static bool HasLocalModifications(SvnStatusType status)
    {
        return status switch
        {
            SvnStatusType.Modified => true,
            SvnStatusType.Added => true,
            SvnStatusType.Deleted => true,
            SvnStatusType.Replaced => true,
            SvnStatusType.Conflicted => true,
            SvnStatusType.Missing => true,
            SvnStatusType.Obstructed => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets a human-readable description of what will happen when an action is performed
    /// </summary>
    public static string GetActionDescription(SvnFileAction action, SvnStatusType currentStatus)
    {
        var nextState = GetNextState(currentStatus, action);
        return (currentStatus, action) switch
        {
            (SvnStatusType.Unversioned, SvnFileAction.Add) => "Schedule file for addition to version control",
            (SvnStatusType.Modified, SvnFileAction.Commit) => "Commit modifications to repository",
            (SvnStatusType.Modified, SvnFileAction.Revert) => "Discard local modifications",
            (SvnStatusType.Modified, SvnFileAction.Delete) => "Schedule file for deletion",
            (SvnStatusType.Added, SvnFileAction.Commit) => "Commit new file to repository",
            (SvnStatusType.Added, SvnFileAction.Revert) => "Cancel addition (file will become unversioned)",
            (SvnStatusType.Deleted, SvnFileAction.Commit) => "Commit file deletion to repository",
            (SvnStatusType.Deleted, SvnFileAction.Revert) => "Restore deleted file",
            (SvnStatusType.Conflicted, SvnFileAction.Resolve) => "Mark conflict as resolved",
            (SvnStatusType.Conflicted, SvnFileAction.Revert) => "Discard changes and restore original",
            (SvnStatusType.Missing, SvnFileAction.Revert) => "Restore missing file from repository",
            (SvnStatusType.Missing, SvnFileAction.Delete) => "Confirm deletion of missing file",
            (SvnStatusType.Normal, SvnFileAction.Delete) => "Schedule file for deletion",
            _ => $"Perform {action} on {currentStatus}"
        };
    }
}
