using Xunit;
using Svns.ViewModels;
using Svns.Models;
using System;

namespace Svns.Tests.ViewModels;

public class InfoViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var vm = new InfoViewModel();

        Assert.Null(vm.WorkingCopyInfo);
        Assert.Equal(string.Empty, vm.Path);
        Assert.Equal(string.Empty, vm.RepositoryUrl);
        Assert.Equal(string.Empty, vm.RepositoryRoot);
        Assert.Equal(string.Empty, vm.RepositoryUuid);
        Assert.Equal(0, vm.Revision);
        Assert.Equal(0, vm.LastChangedRevision);
        Assert.Equal(string.Empty, vm.LastChangedAuthor);
        Assert.Equal(default, vm.LastChangedDate);
        Assert.Equal(string.Empty, vm.RelativePath);
        Assert.Equal(string.Empty, vm.Depth);
        Assert.Equal(0, vm.ModifiedCount);
        Assert.Equal(0, vm.AddedCount);
        Assert.Equal(0, vm.DeletedCount);
        Assert.Equal(0, vm.ConflictedCount);
        Assert.False(vm.HasUncommittedChanges);
        Assert.Equal(string.Empty, vm.BranchName);
        Assert.Equal(string.Empty, vm.ChangesSummary);
    }

    [Fact]
    public void Constructor_WithWorkingCopyInfo_InitializesProperties()
    {
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test\repo",
            RepositoryUrl = "https://svn.example.com/repo/trunk",
            RepositoryRoot = "https://svn.example.com",
            RepositoryUuid = "uuid-123",
            Revision = 100,
            LastChangedRevision = 100,
            LastChangedAuthor = "tester",
            LastChangedDate = new DateTime(2024, 1, 15, 10, 30, 0),
            RelativePath = ".",
            Depth = SvnDepth.Infinity,
            ModifiedFileCount = 5,
            AddedFileCount = 2,
            DeletedFileCount = 1,
            ConflictedFileCount = 0,
            HasUncommittedChanges = true
            // BranchName is computed from RepositoryUrl
            // ChangesSummary is computed from file counts
        };

        var vm = new InfoViewModel(info);

        Assert.Same(info, vm.WorkingCopyInfo);
        Assert.Equal(@"C:\test\repo", vm.Path);
        Assert.Equal("https://svn.example.com/repo/trunk", vm.RepositoryUrl);
        Assert.Equal("https://svn.example.com", vm.RepositoryRoot);
        Assert.Equal("uuid-123", vm.RepositoryUuid);
        Assert.Equal(100, vm.Revision);
        Assert.Equal(100, vm.LastChangedRevision);
        Assert.Equal("tester", vm.LastChangedAuthor);
        Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0), vm.LastChangedDate);
        Assert.Equal(".", vm.RelativePath);
        Assert.Equal("Infinity", vm.Depth);
        Assert.Equal(5, vm.ModifiedCount);
        Assert.Equal(2, vm.AddedCount);
        Assert.Equal(1, vm.DeletedCount);
        Assert.Equal(0, vm.ConflictedCount);
        Assert.True(vm.HasUncommittedChanges);
        Assert.Equal("trunk", vm.BranchName);
        Assert.Equal("5 modified, 2 added, 1 deleted", vm.ChangesSummary);
    }

    [Fact]
    public void Initialize_SetsPropertiesFromWorkingCopyInfo()
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test\repo",
            RepositoryUrl = "https://svn.example.com/repo/main",
            Revision = 50,
            LastChangedAuthor = "author"
            // BranchName computed from RepositoryUrl
        };

        vm.Initialize(info);

        Assert.Same(info, vm.WorkingCopyInfo);
        Assert.Equal(@"C:\test\repo", vm.Path);
        Assert.Equal("https://svn.example.com/repo/main", vm.RepositoryUrl);
        Assert.Equal(50, vm.Revision);
        Assert.Equal("author", vm.LastChangedAuthor);
        Assert.Equal("main", vm.BranchName);
    }

    [Fact]
    public void FormattedLastChangedDate_ReturnsCorrectFormat()
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test",
            LastChangedDate = new DateTime(2024, 1, 15, 14, 30, 45)
        };

        vm.Initialize(info);

        Assert.Equal("2024-01-15 14:30:45", vm.FormattedLastChangedDate);
    }

    [Fact]
    public void TotalChangesCount_SumsAllChanges()
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test",
            ModifiedFileCount = 5,
            AddedFileCount = 3,
            DeletedFileCount = 2,
            ConflictedFileCount = 1
        };

        vm.Initialize(info);

        Assert.Equal(11, vm.TotalChangesCount);
    }

    [Fact]
    public void TotalChangesCount_ReturnsZero_WhenNoChanges()
    {
        var vm = new InfoViewModel();

        Assert.Equal(0, vm.TotalChangesCount);
    }

    [Fact]
    public void CloseRequested_Event_CanBeNull()
    {
        var vm = new InfoViewModel();
        var eventInfo = typeof(InfoViewModel).GetEvent("CloseRequested");
        Assert.NotNull(eventInfo);
    }

    [Fact]
    public void CloseCommand_Exists()
    {
        var vm = new InfoViewModel();
        var propertyInfo = typeof(InfoViewModel).GetProperty("CloseCommand");
        Assert.NotNull(propertyInfo);
    }

    [Fact]
    public void Initialize_WithNullLastChangedAuthor_SetsUnknown()
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test",
            LastChangedAuthor = null
        };

        vm.Initialize(info);

        Assert.Equal("Unknown", vm.LastChangedAuthor);
    }

    [Fact]
    public void Initialize_WithNullBranchName_SetsUnknown()
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test",
            RepositoryUrl = "" // Empty URL results in null BranchName
        };

        vm.Initialize(info);

        Assert.Equal("Unknown", vm.BranchName);
    }

    [Theory]
    [InlineData(SvnDepth.Empty, "Empty")]
    [InlineData(SvnDepth.Files, "Files")]
    [InlineData(SvnDepth.Immediates, "Immediates")]
    [InlineData(SvnDepth.Infinity, "Infinity")]
    [InlineData(SvnDepth.Unknown, "Unknown")]
    public void Depth_ConvertsToString(SvnDepth depth, string expected)
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test",
            Depth = depth
        };

        vm.Initialize(info);

        Assert.Equal(expected, vm.Depth);
    }

    [Fact]
    public void ChangesSummary_ReturnsNoUncommittedChanges_WhenNoChanges()
    {
        var vm = new InfoViewModel();
        var info = new WorkingCopyInfo
        {
            Path = @"C:\test",
            ModifiedFileCount = 0,
            AddedFileCount = 0,
            DeletedFileCount = 0,
            ConflictedFileCount = 0,
            HasUncommittedChanges = false
        };

        vm.Initialize(info);

        Assert.Equal("No uncommitted changes", vm.ChangesSummary);
    }
}
