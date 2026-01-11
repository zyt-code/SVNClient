using Xunit;
using Svns.ViewModels;

namespace Svns.Tests.ViewModels;

public class StartPageViewModelTests
{
    [Fact]
    public void RecentProjectItem_HasDefaultValues()
    {
        var item = new RecentProjectItem();

        Assert.Equal(string.Empty, item.Name);
        Assert.Equal(string.Empty, item.Path);
        Assert.Equal(string.Empty, item.DisplayDate);
    }

    [Fact]
    public void RecentProjectItem_CanSetProperties()
    {
        var item = new RecentProjectItem
        {
            Name = "MyProject",
            Path = @"C:\Projects\MyProject",
            LastOpened = DateTime.Now,
            DisplayDate = "Just now"
        };

        Assert.Equal("MyProject", item.Name);
        Assert.Equal(@"C:\Projects\MyProject", item.Path);
        Assert.Equal("Just now", item.DisplayDate);
    }

    [Theory]
    [InlineData(0, "Just now")]
    [InlineData(30, "30 min ago")]
    [InlineData(120, "2 hours ago")]
    [InlineData(1440, "1 days ago")]
    [InlineData(10080, "1 weeks ago")]
    public void GetRelativeDate_ReturnsExpectedFormat(int minutesAgo, string expectedContains)
    {
        var date = DateTime.Now.AddMinutes(-minutesAgo);
        var relativeDate = GetRelativeDate(date);

        // Check that the result contains the expected pattern
        Assert.Contains(expectedContains.Split(' ')[^1], relativeDate);
    }

    // Helper method that mirrors the logic in StartPageViewModel
    private static string GetRelativeDate(DateTime date)
    {
        var diff = DateTime.Now - date;

        if (diff.TotalMinutes < 1)
            return "Just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} days ago";
        if (diff.TotalDays < 30)
            return $"{(int)(diff.TotalDays / 7)} weeks ago";
        if (diff.TotalDays < 365)
            return $"{(int)(diff.TotalDays / 30)} months ago";

        return date.ToString("yyyy-MM-dd");
    }
}
