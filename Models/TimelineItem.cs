using System.Collections.ObjectModel;

namespace Svns.Models;

public class TimelineItem
{
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimelineStatus Status { get; set; }
    public bool IsNotLast { get; set; }

    public string StatusIcon => Status switch
    {
        TimelineStatus.Completed => "✓",
        TimelineStatus.InProgress => "◐",
        TimelineStatus.Pending => "○",
        _ => "○"
    };

    public string StatusColor => Status switch
    {
        TimelineStatus.Completed => "#10B981",  // Green
        TimelineStatus.InProgress => "#3B82F6", // Blue
        TimelineStatus.Pending => "#9CA3AF",    // Gray
        _ => "#9CA3AF"
    };
}

public enum TimelineStatus
{
    Pending,
    InProgress,
    Completed
}
