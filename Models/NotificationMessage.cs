using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Svns.Models;

/// <summary>
/// Represents a notification message type
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Represents a notification message in the notification center
/// </summary>
public partial class NotificationMessage : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private NotificationType _type = NotificationType.Info;

    [ObservableProperty]
    private DateTime _timestamp = DateTime.Now;

    [ObservableProperty]
    private bool _isRead;

    /// <summary>
    /// Gets the icon kind for this notification type
    /// </summary>
    public string IconKind => Type switch
    {
        NotificationType.Info => "InformationOutline",
        NotificationType.Success => "CheckCircleOutline",
        NotificationType.Warning => "AlertOutline",
        NotificationType.Error => "AlertCircleOutline",
        _ => "InformationOutline"
    };

    /// <summary>
    /// Gets the color for this notification type
    /// </summary>
    public string Color => Type switch
    {
        NotificationType.Info => "#2196F3",
        NotificationType.Success => "#4CAF50",
        NotificationType.Warning => "#FF9800",
        NotificationType.Error => "#F44336",
        _ => "#2196F3"
    };
}
