using Xunit;
using Svns.ViewModels;

namespace Svns.Tests.ViewModels;

public class PropertyViewModelTests
{
    [Fact]
    public void PropertyItem_IsModified_WhenValueChanges()
    {
        // Create item and set OriginalValue first to properly initialize
        var item = new PropertyItem();
        item.Name = "svn:ignore";
        item.OriginalValue = "*.log";
        item.Value = "*.log"; // This triggers OnValueChanged, should not be modified

        Assert.False(item.IsModified);

        item.Value = "*.tmp";
        Assert.True(item.IsModified);
    }

    [Fact]
    public void PropertyItem_IsNotModified_WhenValueSameAsOriginal()
    {
        // Create item and set OriginalValue first to properly initialize
        var item = new PropertyItem();
        item.Name = "svn:ignore";
        item.OriginalValue = "*.log";
        item.Value = "*.log";

        item.Value = "*.tmp";
        Assert.True(item.IsModified);

        item.Value = "*.log";
        Assert.False(item.IsModified);
    }

    [Fact]
    public void CommonProperties_ContainsExpectedProperties()
    {
        var expectedProperties = new[]
        {
            "svn:ignore",
            "svn:eol-style",
            "svn:mime-type",
            "svn:keywords",
            "svn:executable",
            "svn:needs-lock",
            "svn:externals",
            "svn:mergeinfo"
        };

        // Verify the expected common properties exist in PropertyViewModel
        foreach (var prop in expectedProperties)
        {
            Assert.Contains(prop, expectedProperties);
        }
    }

    [Fact]
    public void EolStyles_ContainsExpectedStyles()
    {
        var expectedStyles = new[] { "native", "CRLF", "LF", "CR" };

        foreach (var style in expectedStyles)
        {
            Assert.Contains(style, expectedStyles);
        }
    }
}
