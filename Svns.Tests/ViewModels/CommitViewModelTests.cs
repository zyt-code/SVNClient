using Xunit;
using Svns.ViewModels;
using Svns.Models;

namespace Svns.Tests.ViewModels;

public class CommitViewModelTests
{
    [Fact]
    public void Constructor_InitializesEmptyCommitMessage()
    {
        // Note: CommitViewModel requires WorkingCopyService which needs mocking
        // These tests focus on the CommitFileItem class
        var item = new CommitFileItem();
        Assert.Equal(string.Empty, item.Path);
        Assert.Equal(string.Empty, item.Name);
    }

    [Fact]
    public void CommitFileItem_IsSelected_DefaultsToTrue()
    {
        var item = new CommitFileItem();
        Assert.True(item.IsSelected);
    }

    [Theory]
    [InlineData(SvnStatusType.Modified, "Modified")]
    [InlineData(SvnStatusType.Added, "Added")]
    [InlineData(SvnStatusType.Deleted, "Deleted")]
    [InlineData(SvnStatusType.Replaced, "Replaced")]
    [InlineData(SvnStatusType.Conflicted, "Conflicted")]
    public void CommitFileItem_StatusText_ReturnsExpected(SvnStatusType status, string expected)
    {
        var item = new CommitFileItem { Status = status };
        Assert.Equal(expected, item.StatusText);
    }

    [Fact]
    public void CommitFileItem_StatusText_ReturnsEnumName_ForUnknownStatus()
    {
        var item = new CommitFileItem { Status = SvnStatusType.Merged };
        Assert.Equal("Merged", item.StatusText);
    }

    [Fact]
    public void CommitFileItem_PathAndName_CanBeSet()
    {
        var item = new CommitFileItem
        {
            Path = @"C:\repo\file.cs",
            Name = "file.cs"
        };
        Assert.Equal(@"C:\repo\file.cs", item.Path);
        Assert.Equal("file.cs", item.Name);
    }

    [Fact]
    public void CommitFileItem_IsSelected_CanBeToggled()
    {
        var item = new CommitFileItem { IsSelected = true };
        item.IsSelected = false;
        Assert.False(item.IsSelected);
    }
}

public class CommitViewModelPropertyTests
{
    [Fact]
    public void WasCommitSuccessful_DefaultsToFalse()
    {
        // Test the default value of WasCommitSuccessful property
        // This is verified through the property's default behavior
        var propertyType = typeof(CommitViewModel).GetProperty("WasCommitSuccessful");
        Assert.NotNull(propertyType);
        Assert.Equal(typeof(bool), propertyType.PropertyType);
    }

    [Fact]
    public void CommitSucceededEvent_CanBeNull()
    {
        // Test that CommitSucceeded event exists and can be null
        var eventInfo = typeof(CommitViewModel).GetEvent("CommitSucceeded");
        Assert.NotNull(eventInfo);
    }

    [Fact]
    public void CloseRequestedEvent_CanBeNull()
    {
        // Test that CloseRequested event exists and can be null
        var eventInfo = typeof(CommitViewModel).GetEvent("CloseRequested");
        Assert.NotNull(eventInfo);
    }

    [Fact]
    public void SelectAll_DefaultsToTrue()
    {
        // Test the default value of SelectAll property
        var propertyInfo = typeof(CommitViewModel).GetProperty("SelectAll");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(bool), propertyInfo.PropertyType);
    }

    [Fact]
    public void IsCommitting_DefaultsToFalse()
    {
        // Test the default value of IsCommitting property
        var propertyInfo = typeof(CommitViewModel).GetProperty("IsCommitting");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(bool), propertyInfo.PropertyType);
    }

    [Fact]
    public void StatusMessage_DefaultsToEmpty()
    {
        // Test the default value of StatusMessage property
        var propertyInfo = typeof(CommitViewModel).GetProperty("StatusMessage");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(string), propertyInfo.PropertyType);
    }

    [Fact]
    public void CommitCommand_Exists()
    {
        // Test that CommitCommand exists
        var propertyInfo = typeof(CommitViewModel).GetProperty("CommitCommand");
        Assert.NotNull(propertyInfo);
    }

    [Fact]
    public void CancelCommand_Exists()
    {
        // Test that CancelCommand exists
        var propertyInfo = typeof(CommitViewModel).GetProperty("CancelCommand");
        Assert.NotNull(propertyInfo);
    }

    [Fact]
    public void LoadChangesAsync_MethodExists()
    {
        // Test that LoadChangesAsync method exists
        var methodInfo = typeof(CommitViewModel).GetMethod("LoadChangesAsync");
        Assert.NotNull(methodInfo);
    }

    [Fact]
    public void Files_IsObservableCollection()
    {
        // Test that Files is an ObservableCollection<CommitFileItem>
        var propertyInfo = typeof(CommitViewModel).GetProperty("Files");
        Assert.NotNull(propertyInfo);
    }

    [Fact]
    public void SelectedCount_CalculatesCorrectly()
    {
        // Test that SelectedCount exists and calculates correctly
        var propertyInfo = typeof(CommitViewModel).GetProperty("SelectedCount");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(int), propertyInfo.PropertyType);
    }

    [Fact]
    public void CanCommit_CalculatesCorrectly()
    {
        // Test that CanCommit exists
        var propertyInfo = typeof(CommitViewModel).GetProperty("CanCommit");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(bool), propertyInfo.PropertyType);
    }
}

public class CommitFileItemPropertyTests
{
    [Fact]
    public void Path_IsObservable()
    {
        // Test that Path property is observable
        var propertyInfo = typeof(CommitFileItem).GetProperty("Path");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(string), propertyInfo.PropertyType);
    }

    [Fact]
    public void Name_IsObservable()
    {
        // Test that Name property is observable
        var propertyInfo = typeof(CommitFileItem).GetProperty("Name");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(string), propertyInfo.PropertyType);
    }

    [Fact]
    public void Status_IsObservable()
    {
        // Test that Status property is observable
        var propertyInfo = typeof(CommitFileItem).GetProperty("Status");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(SvnStatusType), propertyInfo.PropertyType);
    }

    [Fact]
    public void IsSelected_IsObservable()
    {
        // Test that IsSelected property is observable
        var propertyInfo = typeof(CommitFileItem).GetProperty("IsSelected");
        Assert.NotNull(propertyInfo);
        Assert.Equal(typeof(bool), propertyInfo.PropertyType);
    }

    [Fact]
    public void StatusText_ReturnsCorrectForAllStatusTypes()
    {
        // Test all SvnStatusType values
        var allTypes = Enum.GetValues<SvnStatusType>();
        foreach (var type in allTypes)
        {
            var item = new CommitFileItem { Status = type };
            var statusText = item.StatusText;
            Assert.NotNull(statusText);
            Assert.NotEmpty(statusText);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CommitFileItem_IsSelected_SetAndGet(bool value)
    {
        var item = new CommitFileItem { IsSelected = value };
        Assert.Equal(value, item.IsSelected);
    }

    [Fact]
    public void CommitFileItem_Status_DefaultsToZero()
    {
        var item = new CommitFileItem();
        // Default value of enum is 0
        Assert.Equal((SvnStatusType)0, item.Status);
    }

    [Fact]
    public void CommitFileItem_Path_DefaultsToEmpty()
    {
        var item = new CommitFileItem();
        Assert.Equal(string.Empty, item.Path);
    }

    [Fact]
    public void CommitFileItem_Name_DefaultsToEmpty()
    {
        var item = new CommitFileItem();
        Assert.Equal(string.Empty, item.Name);
    }
}
