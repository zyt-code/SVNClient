using Xunit;
using Svns.Models;
using System;

namespace Svns.Tests.Models;

public class NotificationMessageTests
{
    [Fact]
    public void Constructor_CreatesWithDefaultValues()
    {
        var notification = new NotificationMessage();

        Assert.NotNull(notification.Id);
        Assert.NotEmpty(notification.Id);
        Assert.Equal(string.Empty, notification.Title);
        Assert.Equal(string.Empty, notification.Message);
        Assert.Equal(NotificationType.Info, notification.Type);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void Id_GeneratesUniqueId()
    {
        var notification1 = new NotificationMessage();
        var notification2 = new NotificationMessage();

        Assert.NotEqual(notification1.Id, notification2.Id);
    }

    [Fact]
    public void Id_IsValidGuid()
    {
        var notification = new NotificationMessage();

        Assert.True(Guid.TryParse(notification.Id, out _));
    }

    [Fact]
    public void Title_CanBeSet()
    {
        var notification = new NotificationMessage
        {
            Title = "Test Title"
        };

        Assert.Equal("Test Title", notification.Title);
    }

    [Fact]
    public void Message_CanBeSet()
    {
        var notification = new NotificationMessage
        {
            Message = "Test Message"
        };

        Assert.Equal("Test Message", notification.Message);
    }

    [Fact]
    public void Type_CanBeSet()
    {
        var notification = new NotificationMessage
        {
            Type = NotificationType.Error
        };

        Assert.Equal(NotificationType.Error, notification.Type);
    }

    [Fact]
    public void Timestamp_IsSetToCurrentTime()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var notification = new NotificationMessage();
        var after = DateTime.Now.AddSeconds(1);

        Assert.InRange(notification.Timestamp, before, after);
    }

    [Fact]
    public void Timestamp_CanBeSet()
    {
        var expectedTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var notification = new NotificationMessage
        {
            Timestamp = expectedTime
        };

        Assert.Equal(expectedTime, notification.Timestamp);
    }

    [Fact]
    public void IsRead_CanBeSet()
    {
        var notification = new NotificationMessage
        {
            IsRead = true
        };

        Assert.True(notification.IsRead);
    }

    [Theory]
    [InlineData(NotificationType.Info, "InformationOutline")]
    [InlineData(NotificationType.Success, "CheckCircleOutline")]
    [InlineData(NotificationType.Warning, "AlertOutline")]
    [InlineData(NotificationType.Error, "AlertCircleOutline")]
    public void IconKind_ReturnsCorrectIcon(NotificationType type, string expectedIcon)
    {
        var notification = new NotificationMessage
        {
            Type = type
        };

        Assert.Equal(expectedIcon, notification.IconKind);
    }

    [Theory]
    [InlineData(NotificationType.Info, "#2196F3")]
    [InlineData(NotificationType.Success, "#4CAF50")]
    [InlineData(NotificationType.Warning, "#FF9800")]
    [InlineData(NotificationType.Error, "#F44336")]
    public void Color_ReturnsCorrectColor(NotificationType type, string expectedColor)
    {
        var notification = new NotificationMessage
        {
            Type = type
        };

        Assert.Equal(expectedColor, notification.Color);
    }

    [Fact]
    public void IconKind_ReturnsDefaultForUnknownType()
    {
        // If a new type is added without updating the switch, it should return "InformationOutline"
        var notification = new NotificationMessage
        {
            Type = (NotificationType)999
        };

        Assert.Equal("InformationOutline", notification.IconKind);
    }

    [Fact]
    public void Color_ReturnsDefaultForUnknownType()
    {
        // If a new type is added without updating the switch, it should return "#2196F3"
        var notification = new NotificationMessage
        {
            Type = (NotificationType)999
        };

        Assert.Equal("#2196F3", notification.Color);
    }

    [Fact]
    public void IsRead_DefaultsToFalse()
    {
        var notification = new NotificationMessage();

        Assert.False(notification.IsRead);
    }

    [Fact]
    public void Type_DefaultsToInfo()
    {
        var notification = new NotificationMessage();

        Assert.Equal(NotificationType.Info, notification.Type);
    }

    [Fact]
    public void PropertyChanged_FiresWhenTitleChanges()
    {
        var notification = new NotificationMessage();
        bool fired = false;
        notification.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(notification.Title))
                fired = true;
        };

        notification.Title = "New Title";

        Assert.True(fired);
    }

    [Fact]
    public void PropertyChanged_FiresWhenMessageChanges()
    {
        var notification = new NotificationMessage();
        bool fired = false;
        notification.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(notification.Message))
                fired = true;
        };

        notification.Message = "New Message";

        Assert.True(fired);
    }

    [Fact]
    public void PropertyChanged_FiresWhenTypeChanges()
    {
        var notification = new NotificationMessage();
        bool fired = false;
        notification.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(notification.Type))
                fired = true;
        };

        notification.Type = NotificationType.Error;

        Assert.True(fired);
    }

    [Fact]
    public void PropertyChanged_FiresWhenIsReadChanges()
    {
        var notification = new NotificationMessage();
        bool fired = false;
        notification.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(notification.IsRead))
                fired = true;
        };

        notification.IsRead = true;

        Assert.True(fired);
    }
}
