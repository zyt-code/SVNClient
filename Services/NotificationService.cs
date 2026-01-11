using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Svns.Models;

namespace Svns.Services;

/// <summary>
/// Service for managing application notifications
/// </summary>
public partial class NotificationService : ObservableObject
{
    private static NotificationService? _instance;
    public static NotificationService Instance => _instance ??= new NotificationService();

    [ObservableProperty]
    private ObservableCollection<NotificationMessage> _notifications = new();

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private bool _isPanelOpen;

    private NotificationService()
    {
        Notifications.CollectionChanged += (_, _) => UpdateUnreadCount();
    }

    private void UpdateUnreadCount()
    {
        UnreadCount = Notifications.Count(n => !n.IsRead);
    }

    /// <summary>
    /// Adds a new notification
    /// </summary>
    public void AddNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        var notification = new NotificationMessage
        {
            Title = title,
            Message = message,
            Type = type,
            IsRead = false
        };

        // Add to the beginning of the list
        Notifications.Insert(0, notification);

        // Keep only the last 100 notifications
        if (Notifications.Count > 100)
        {
            for (int i = Notifications.Count - 1; i >= 100; i--)
            {
                Notifications.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Adds an info notification
    /// </summary>
    public void Info(string title, string message)
    {
        AddNotification(title, message, NotificationType.Info);
    }

    /// <summary>
    /// Adds a success notification
    /// </summary>
    public void Success(string title, string message)
    {
        AddNotification(title, message, NotificationType.Success);
    }

    /// <summary>
    /// Adds a warning notification
    /// </summary>
    public void Warning(string title, string message)
    {
        AddNotification(title, message, NotificationType.Warning);
    }

    /// <summary>
    /// Adds an error notification
    /// </summary>
    public void Error(string title, string message)
    {
        AddNotification(title, message, NotificationType.Error);
    }

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    public void MarkAsRead(NotificationMessage notification)
    {
        notification.IsRead = true;
        UpdateUnreadCount();
    }

    /// <summary>
    /// Marks all notifications as read
    /// </summary>
    public void MarkAllAsRead()
    {
        foreach (var notification in Notifications)
        {
            notification.IsRead = true;
        }
        UpdateUnreadCount();
    }

    /// <summary>
    /// Removes a notification
    /// </summary>
    public void RemoveNotification(NotificationMessage notification)
    {
        Notifications.Remove(notification);
    }

    /// <summary>
    /// Clears all notifications
    /// </summary>
    public void ClearAll()
    {
        Notifications.Clear();
    }

    /// <summary>
    /// Toggles the notification panel
    /// </summary>
    public void TogglePanel()
    {
        IsPanelOpen = !IsPanelOpen;
    }

    /// <summary>
    /// Opens the notification panel
    /// </summary>
    public void OpenPanel()
    {
        IsPanelOpen = true;
    }

    /// <summary>
    /// Closes the notification panel
    /// </summary>
    public void ClosePanel()
    {
        IsPanelOpen = false;
    }
}
