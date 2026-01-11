using Xunit;
using Svns.Models;
using System;
using System.Collections.ObjectModel;

namespace Svns.Tests.Models;

public class SvnLogEntryTests
{
    [Fact]
    public void DisplayRevision_ReturnsFormattedRevision()
    {
        var entry = new SvnLogEntry { Revision = 123 };
        Assert.Equal("r123", entry.DisplayRevision);
    }

    [Fact]
    public void ChangedPathCount_ReturnsCorrectCount()
    {
        var entry = new SvnLogEntry();
        entry.ChangedPaths.Add(new SvnChangedPath { Path = "/path1" });
        entry.ChangedPaths.Add(new SvnChangedPath { Path = "/path2" });
        Assert.Equal(2, entry.ChangedPathCount);
    }

    [Fact]
    public void HasChangedPaths_ReturnsTrue_WhenHasChanges()
    {
        var entry = new SvnLogEntry();
        entry.ChangedPaths.Add(new SvnChangedPath { Path = "/path1" });
        Assert.True(entry.HasChangedPaths);
    }

    [Fact]
    public void HasChangedPaths_ReturnsFalse_WhenNoChanges()
    {
        var entry = new SvnLogEntry();
        Assert.False(entry.HasChangedPaths);
    }

    [Fact]
    public void DisplayDate_FormatsRecentDate_AsMinutesAgo()
    {
        var entry = new SvnLogEntry { Date = DateTime.Now.AddMinutes(-30) };
        Assert.Contains("minutes ago", entry.DisplayDate);
    }

    [Fact]
    public void DisplayDate_FormatsDate_AsHoursAgo()
    {
        var entry = new SvnLogEntry { Date = DateTime.Now.AddHours(-5) };
        Assert.Contains("hours ago", entry.DisplayDate);
    }

    [Fact]
    public void DisplayDate_FormatsDate_AsDaysAgo()
    {
        var entry = new SvnLogEntry { Date = DateTime.Now.AddDays(-3) };
        Assert.Contains("days ago", entry.DisplayDate);
    }

    [Fact]
    public void DisplayDate_FormatsOldDate_AsYearMonthDay()
    {
        var entry = new SvnLogEntry { Date = DateTime.Now.AddDays(-30) };
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", entry.DisplayDate);
    }

    [Fact]
    public void ChangedPaths_DefaultsToEmptyCollection()
    {
        var entry = new SvnLogEntry();
        Assert.NotNull(entry.ChangedPaths);
        Assert.Empty(entry.ChangedPaths);
    }

    [Fact]
    public void Author_DefaultsToEmptyString()
    {
        var entry = new SvnLogEntry();
        Assert.Equal(string.Empty, entry.Author);
    }

    [Fact]
    public void Message_DefaultsToEmptyString()
    {
        var entry = new SvnLogEntry();
        Assert.Equal(string.Empty, entry.Message);
    }
}

public class SvnChangedPathTests
{
    [Fact]
    public void FileName_ReturnsFileName_FromPath()
    {
        var path = new SvnChangedPath { Path = "/trunk/src/file.cs" };
        Assert.Equal("file.cs", path.FileName);
    }

    [Fact]
    public void FileName_ReturnsPath_WhenNoFileName()
    {
        var path = new SvnChangedPath { Path = "/" };
        Assert.Equal("", path.FileName);
    }

    [Theory]
    [InlineData(SvnPathAction.Added)]
    [InlineData(SvnPathAction.Deleted)]
    [InlineData(SvnPathAction.Modified)]
    [InlineData(SvnPathAction.Replaced)]
    public void Action_CanBeSet(SvnPathAction action)
    {
        var path = new SvnChangedPath { Action = action };
        Assert.Equal(action, path.Action);
    }

    [Fact]
    public void CopyFromPath_CanBeNull()
    {
        var path = new SvnChangedPath();
        Assert.Null(path.CopyFromPath);
    }

    [Fact]
    public void CopyFromRevision_CanBeNull()
    {
        var path = new SvnChangedPath();
        Assert.Null(path.CopyFromRevision);
    }
}
