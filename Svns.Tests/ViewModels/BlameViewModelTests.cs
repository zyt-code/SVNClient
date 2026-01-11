using Xunit;
using Svns.ViewModels;
using System.Collections.ObjectModel;

namespace Svns.Tests.ViewModels;

public class BlameViewModelTests
{
    [Fact]
    public void BlameLineItem_HasDefaultValues()
    {
        var item = new BlameLineItem();

        Assert.Equal(0, item.LineNumber);
        Assert.Equal(0, item.Revision);
        Assert.Equal(string.Empty, item.Author);
        Assert.Equal(string.Empty, item.Content);
        Assert.Equal("#808080", item.AuthorColor); // Default color
    }

    [Fact]
    public void BlameLineItem_CanSetProperties()
    {
        var item = new BlameLineItem
        {
            LineNumber = 1,
            Revision = 123,
            Author = "testuser",
            Date = DateTime.Now,
            Content = "using System;",
            AuthorColor = "#FF5733"
        };

        Assert.Equal(1, item.LineNumber);
        Assert.Equal(123, item.Revision);
        Assert.Equal("testuser", item.Author);
        Assert.Equal("using System;", item.Content);
        Assert.Equal("#FF5733", item.AuthorColor);
    }

    [Fact]
    public void BlameLineItem_DisplayRevision_FormatsCorrectly()
    {
        var item = new BlameLineItem { Revision = 123 };
        Assert.Equal("r123", item.DisplayRevision);

        var itemZero = new BlameLineItem { Revision = 0 };
        Assert.Equal("r0", itemZero.DisplayRevision);
    }

    [Fact]
    public void AuthorStatItem_HasDefaultValues()
    {
        var item = new AuthorStatItem();

        Assert.Equal(string.Empty, item.Author);
        Assert.Equal(0, item.LineCount);
        Assert.Equal("#808080", item.Color); // Default color
    }

    [Fact]
    public void AuthorStatItem_CanSetProperties()
    {
        var item = new AuthorStatItem
        {
            Author = "testuser",
            LineCount = 50,
            Color = "#FF5733"
        };

        Assert.Equal("testuser", item.Author);
        Assert.Equal(50, item.LineCount);
        Assert.Equal("#FF5733", item.Color);
    }

    [Fact]
    public void AuthorStatItem_Percentage_CalculatesCorrectly()
    {
        var item = new AuthorStatItem
        {
            LineCount = 25,
            Percentage = 25.0
        };

        Assert.Equal(25.0, item.Percentage);
    }

    [Fact]
    public void AuthorStatItem_DisplayPercentage_FormatsCorrectly()
    {
        var item = new AuthorStatItem
        {
            Percentage = 25.5
        };

        Assert.Equal("25.5%", item.DisplayPercentage);
    }

    [Fact]
    public void AuthorColors_ContainsExpectedColors()
    {
        // Test that we have a good set of distinguishable colors
        var colors = new[]
        {
            "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6",
            "#EC4899", "#06B6D4", "#84CC16", "#F97316", "#6366F1"
        };

        Assert.Equal(10, colors.Length);
        Assert.All(colors, c => Assert.StartsWith("#", c));
    }
}
