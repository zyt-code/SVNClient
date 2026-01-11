using System;
using System.Linq;

namespace Svns.Services;

/// <summary>
/// Types of refresh events
/// </summary>
public enum RefreshType
{
    /// <summary>
    /// All data should be refreshed
    /// </summary>
    All,

    /// <summary>
    /// File statuses changed
    /// </summary>
    FileStatus,

    /// <summary>
    /// Commit history changed
    /// </summary>
    Log,

    /// <summary>
    /// Working copy info changed
    /// </summary>
    Info,

    /// <summary>
    /// A commit was made
    /// </summary>
    Commit,

    /// <summary>
    /// An update was performed
    /// </summary>
    Update,

    /// <summary>
    /// A merge was performed
    /// </summary>
    Merge,

    /// <summary>
    /// A branch/tag was created
    /// </summary>
    BranchTag,

    /// <summary>
    /// A switch was performed
    /// </summary>
    Switch,

    /// <summary>
    /// Files were reverted
    /// </summary>
    Revert,

    /// <summary>
    /// Properties were changed
    /// </summary>
    Properties
}

/// <summary>
/// Singleton service for coordinating refresh events across the application
/// </summary>
public class RefreshService
{
    private static RefreshService? _instance;
    private static readonly object _lock = new();

    public static RefreshService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new RefreshService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Event fired when a refresh is requested
    /// </summary>
    public event EventHandler<RefreshType>? RefreshRequested;

    private RefreshService() { }

    /// <summary>
    /// Requests a refresh of the specified type
    /// </summary>
    public void RequestRefresh(RefreshType type)
    {
        RefreshRequested?.Invoke(this, type);
    }

    /// <summary>
    /// Subscribes to refresh events
    /// </summary>
    public void Subscribe(Action<RefreshType> onRefresh)
    {
        RefreshRequested += (sender, type) => onRefresh(type);
    }

    /// <summary>
    /// Subscribes to specific refresh types
    /// </summary>
    public void Subscribe(RefreshType type, Action onRefresh)
    {
        RefreshRequested += (sender, refreshType) =>
        {
            if (refreshType == type || refreshType == RefreshType.All)
            {
                onRefresh();
            }
        };
    }

    /// <summary>
    /// Subscribes to multiple refresh types
    /// </summary>
    public void Subscribe(RefreshType[] types, Action<RefreshType> onRefresh)
    {
        RefreshRequested += (sender, refreshType) =>
        {
            if (refreshType == RefreshType.All || types.Contains(refreshType))
            {
                onRefresh(refreshType);
            }
        };
    }
}
