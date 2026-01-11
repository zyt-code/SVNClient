using Xunit;
using Svns.Services;
using Svns.Models;

namespace Svns.Tests.Services;

public class NotificationServiceTests
{
    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = NotificationService.Instance;
        var instance2 = NotificationService.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Notifications_IsInitiallyEmpty()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        Assert.Empty(service.Notifications);
    }

    [Fact]
    public void UnreadCount_IsInitiallyZero()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        Assert.Equal(0, service.UnreadCount);
    }

    [Fact]
    public void IsPanelOpen_IsInitiallyFalse()
    {
        var service = NotificationService.Instance;
        service.ClosePanel();

        Assert.False(service.IsPanelOpen);
    }

    [Fact]
    public void AddNotification_AddsToNotifications()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.AddNotification("Test Title", "Test Message", NotificationType.Info);

        Assert.Single(service.Notifications);
        Assert.Equal("Test Title", service.Notifications[0].Title);
        Assert.Equal("Test Message", service.Notifications[0].Message);
    }

    [Fact]
    public void AddNotification_InsertsAtBeginning()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.AddNotification("First", "First Message", NotificationType.Info);
        service.AddNotification("Second", "Second Message", NotificationType.Info);

        Assert.Equal(2, service.Notifications.Count);
        Assert.Equal("Second", service.Notifications[0].Title);
        Assert.Equal("First", service.Notifications[1].Title);
    }

    [Fact]
    public void AddNotification_IncrementsUnreadCount()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.AddNotification("Title", "Message", NotificationType.Info);

        Assert.Equal(1, service.UnreadCount);
    }

    [Fact]
    public void AddNotification_KeepsOnlyLast100()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        for (int i = 0; i < 150; i++)
        {
            service.AddNotification($"Title {i}", $"Message {i}", NotificationType.Info);
        }

        Assert.Equal(100, service.Notifications.Count);
    }

    [Theory]
    [InlineData(NotificationType.Info)]
    [InlineData(NotificationType.Success)]
    [InlineData(NotificationType.Warning)]
    [InlineData(NotificationType.Error)]
    public void Info_AddsInfoNotification(NotificationType type)
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        switch (type)
        {
            case NotificationType.Info:
                service.Info("Info Title", "Info Message");
                break;
            case NotificationType.Success:
                service.Success("Success Title", "Success Message");
                break;
            case NotificationType.Warning:
                service.Warning("Warning Title", "Warning Message");
                break;
            case NotificationType.Error:
                service.Error("Error Title", "Error Message");
                break;
        }

        Assert.Single(service.Notifications);
        Assert.Equal(type, service.Notifications[0].Type);
    }

    [Fact]
    public void Info_AddsNotificationWithCorrectType()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.Info("Title", "Message");

        Assert.Equal(NotificationType.Info, service.Notifications[0].Type);
    }

    [Fact]
    public void Success_AddsNotificationWithCorrectType()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.Success("Title", "Message");

        Assert.Equal(NotificationType.Success, service.Notifications[0].Type);
    }

    [Fact]
    public void Warning_AddsNotificationWithCorrectType()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.Warning("Title", "Message");

        Assert.Equal(NotificationType.Warning, service.Notifications[0].Type);
    }

    [Fact]
    public void Error_AddsNotificationWithCorrectType()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.Error("Title", "Message");

        Assert.Equal(NotificationType.Error, service.Notifications[0].Type);
    }

    [Fact]
    public void MarkAsRead_SetsIsReadToTrue()
    {
        var service = NotificationService.Instance;
        service.ClearAll();
        service.AddNotification("Title", "Message", NotificationType.Info);

        service.MarkAsRead(service.Notifications[0]);

        Assert.True(service.Notifications[0].IsRead);
    }

    [Fact]
    public void MarkAsRead_DecrementsUnreadCount()
    {
        var service = NotificationService.Instance;
        service.ClearAll();
        service.AddNotification("Title1", "Message1", NotificationType.Info);
        service.AddNotification("Title2", "Message2", NotificationType.Info);

        Assert.Equal(2, service.UnreadCount);

        service.MarkAsRead(service.Notifications[0]);

        Assert.Equal(1, service.UnreadCount);
    }

    [Fact]
    public void MarkAllAsRead_SetsAllToRead()
    {
        var service = NotificationService.Instance;
        service.ClearAll();
        service.AddNotification("Title1", "Message1", NotificationType.Info);
        service.AddNotification("Title2", "Message2", NotificationType.Info);

        service.MarkAllAsRead();

        Assert.True(service.Notifications.All(n => n.IsRead));
    }

    [Fact]
    public void MarkAllAsRead_SetsUnreadCountToZero()
    {
        var service = NotificationService.Instance;
        service.ClearAll();
        service.AddNotification("Title1", "Message1", NotificationType.Info);
        service.AddNotification("Title2", "Message2", NotificationType.Info);

        service.MarkAllAsRead();

        Assert.Equal(0, service.UnreadCount);
    }

    [Fact]
    public void RemoveNotification_RemovesFromList()
    {
        var service = NotificationService.Instance;
        service.ClearAll();
        service.AddNotification("Title", "Message", NotificationType.Info);

        service.RemoveNotification(service.Notifications[0]);

        Assert.Empty(service.Notifications);
    }

    [Fact]
    public void ClearAll_RemovesAllNotifications()
    {
        var service = NotificationService.Instance;
        service.AddNotification("Title1", "Message1", NotificationType.Info);
        service.AddNotification("Title2", "Message2", NotificationType.Info);
        service.AddNotification("Title3", "Message3", NotificationType.Info);

        service.ClearAll();

        Assert.Empty(service.Notifications);
    }

    [Fact]
    public void ClearAll_ResetsUnreadCount()
    {
        var service = NotificationService.Instance;
        service.AddNotification("Title1", "Message1", NotificationType.Info);
        service.AddNotification("Title2", "Message2", NotificationType.Info);

        service.ClearAll();

        Assert.Equal(0, service.UnreadCount);
    }

    [Fact]
    public void TogglePanel_TogglesIsPanelOpen()
    {
        var service = NotificationService.Instance;
        service.ClosePanel();

        var initialState = service.IsPanelOpen;
        service.TogglePanel();

        Assert.NotEqual(initialState, service.IsPanelOpen);
    }

    [Fact]
    public void OpenPanel_SetsIsPanelOpenToTrue()
    {
        var service = NotificationService.Instance;
        service.ClosePanel();

        service.OpenPanel();

        Assert.True(service.IsPanelOpen);
    }

    [Fact]
    public void ClosePanel_SetsIsPanelOpenToFalse()
    {
        var service = NotificationService.Instance;
        service.OpenPanel();

        service.ClosePanel();

        Assert.False(service.IsPanelOpen);
    }

    [Fact]
    public void Notifications_CollectionChanged_UpdatesUnreadCount()
    {
        var service = NotificationService.Instance;
        service.ClearAll();

        service.AddNotification("Title1", "Message1", NotificationType.Info);
        Assert.Equal(1, service.UnreadCount);

        service.AddNotification("Title2", "Message2", NotificationType.Info);
        Assert.Equal(2, service.UnreadCount);

        service.ClearAll();
        Assert.Equal(0, service.UnreadCount);
    }
}
