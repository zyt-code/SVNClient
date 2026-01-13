using Xunit;
using Svns.Models;
using System.Collections.Generic;

namespace Svns.Tests.Models;

public class SvnDiffResultTests
{
    [Fact]
    public void SvnDiffResult_HasDefaultValues()
    {
        var diff = new SvnDiffResult();

        Assert.Equal(string.Empty, diff.Path);
        Assert.Equal(string.Empty, diff.OriginalPath);
        Assert.Equal(string.Empty, diff.ModifiedPath);
        Assert.Null(diff.StartRevision);
        Assert.Null(diff.EndRevision);
        Assert.Equal("unified", diff.DiffType);
        Assert.NotNull(diff.Lines);
        Assert.Empty(diff.Lines);
        Assert.False(diff.IsBinary);
        Assert.Null(diff.BinaryMessage);
    }

    [Fact]
    public void AdditionCount_ReturnsCorrectCount()
    {
        var diff = new SvnDiffResult
        {
            Lines = new List<SvnDiffLine>
            {
                new() { Type = SvnDiffLineType.Addition },
                new() { Type = SvnDiffLineType.Addition },
                new() { Type = SvnDiffLineType.Deletion },
                new() { Type = SvnDiffLineType.Context }
            }
        };

        Assert.Equal(2, diff.AdditionCount);
    }

    [Fact]
    public void DeletionCount_ReturnsCorrectCount()
    {
        var diff = new SvnDiffResult
        {
            Lines = new List<SvnDiffLine>
            {
                new() { Type = SvnDiffLineType.Deletion },
                new() { Type = SvnDiffLineType.Deletion },
                new() { Type = SvnDiffLineType.Deletion },
                new() { Type = SvnDiffLineType.Addition }
            }
        };

        Assert.Equal(3, diff.DeletionCount);
    }

    [Fact]
    public void ModificationCount_ReturnsCorrectCount()
    {
        var diff = new SvnDiffResult
        {
            Lines = new List<SvnDiffLine>
            {
                new() { Type = SvnDiffLineType.Modification },
                new() { Type = SvnDiffLineType.Context }
            }
        };

        Assert.Equal(1, diff.ModificationCount);
    }

    [Theory]
    [InlineData("file.cs", ".cs")]
    [InlineData("file.txt", ".txt")]
    [InlineData("path/to/file.xml", ".xml")]
    [InlineData("file.test.js", ".js")]
    public void FileExtension_ReturnsCorrectExtension(string path, string expected)
    {
        var diff = new SvnDiffResult { Path = path };

        Assert.Equal(expected, diff.FileExtension);
    }

    [Theory]
    [InlineData("noextension")]
    [InlineData("")]
    public void FileExtension_ReturnsNull_WhenNoExtension(string path)
    {
        var diff = new SvnDiffResult { Path = path };

        Assert.Null(diff.FileExtension);
    }
}

public class SvnDiffLineTests
{
    [Fact]
    public void SvnDiffLine_HasDefaultValues()
    {
        var line = new SvnDiffLine();

        Assert.Equal(SvnDiffLineType.Context, line.Type);
        Assert.Null(line.OriginalLineNumber);
        Assert.Null(line.ModifiedLineNumber);
        Assert.Equal(string.Empty, line.Content);
    }

    [Fact]
    public void IsContext_ReturnsTrue_WhenTypeIsContext()
    {
        var line = new SvnDiffLine { Type = SvnDiffLineType.Context };

        Assert.True(line.IsContext);
        Assert.False(line.IsAddition);
        Assert.False(line.IsDeletion);
        Assert.False(line.IsHeader);
        Assert.False(line.IsHunkHeader);
    }

    [Fact]
    public void IsAddition_ReturnsTrue_WhenTypeIsAddition()
    {
        var line = new SvnDiffLine { Type = SvnDiffLineType.Addition };

        Assert.True(line.IsAddition);
        Assert.False(line.IsContext);
        Assert.False(line.IsDeletion);
    }

    [Fact]
    public void IsDeletion_ReturnsTrue_WhenTypeIsDeletion()
    {
        var line = new SvnDiffLine { Type = SvnDiffLineType.Deletion };

        Assert.True(line.IsDeletion);
        Assert.False(line.IsContext);
        Assert.False(line.IsAddition);
    }

    [Fact]
    public void IsHeader_ReturnsTrue_WhenTypeIsHeader()
    {
        var line = new SvnDiffLine { Type = SvnDiffLineType.Header };

        Assert.True(line.IsHeader);
    }

    [Fact]
    public void IsHunkHeader_ReturnsTrue_WhenTypeIsHunkHeader()
    {
        var line = new SvnDiffLine { Type = SvnDiffLineType.HunkHeader };

        Assert.True(line.IsHunkHeader);
    }
}

public class WorkingCopyInfoTests
{
    [Fact]
    public void WorkingCopyInfo_HasDefaultValues()
    {
        var info = new WorkingCopyInfo();

        Assert.Equal(string.Empty, info.Path);
        Assert.Equal(string.Empty, info.RepositoryUrl);
        Assert.Equal(string.Empty, info.RepositoryRoot);
        Assert.Equal(string.Empty, info.RepositoryUuid);
        Assert.Equal(0, info.Revision);
        Assert.Null(info.LastChangedAuthor);
        Assert.Equal(0, info.LastChangedRevision);
    }

    [Fact]
    public void DisplayName_ReturnsLastDirectoryName()
    {
        // Use platform-agnostic path
        var path = System.IO.Path.Combine("Projects", "MyProject");
        var info = new WorkingCopyInfo { Path = path };

        Assert.Equal("MyProject", info.DisplayName);
    }

    [Fact]
    public void DisplayName_ReturnsPath_WhenNoDirectorySeparator()
    {
        var info = new WorkingCopyInfo { Path = "MyProject" };

        Assert.Equal("MyProject", info.DisplayName);
    }

    [Fact]
    public void DisplayName_HandlesEmptyPath()
    {
        var info = new WorkingCopyInfo { Path = "" };

        Assert.Equal("", info.DisplayName);
    }
}

public class SvnPropertyTests
{
    [Fact]
    public void SvnProperty_HasDefaultValues()
    {
        var prop = new SvnProperty();

        Assert.Equal(string.Empty, prop.Name);
        Assert.Equal(string.Empty, prop.Value);
    }

    [Fact]
    public void SvnProperty_CanSetValues()
    {
        var prop = new SvnProperty
        {
            Name = "svn:ignore",
            Value = "*.dll\n*.pdb"
        };

        Assert.Equal("svn:ignore", prop.Name);
        Assert.Equal("*.dll\n*.pdb", prop.Value);
    }
}

public class SvnBlameLineTests
{
    [Fact]
    public void SvnBlameLine_HasDefaultValues()
    {
        var line = new SvnBlameLine();

        Assert.Equal(0, line.LineNumber);
        Assert.Equal(0, line.Revision);
        Assert.Equal(string.Empty, line.Author);
        Assert.Equal(string.Empty, line.Content);
    }

    [Fact]
    public void SvnBlameLine_CanSetAllProperties()
    {
        var date = DateTime.Now;
        var line = new SvnBlameLine
        {
            LineNumber = 42,
            Revision = 123,
            Author = "developer",
            Date = date,
            Content = "public void Method()"
        };

        Assert.Equal(42, line.LineNumber);
        Assert.Equal(123, line.Revision);
        Assert.Equal("developer", line.Author);
        Assert.Equal(date, line.Date);
        Assert.Equal("public void Method()", line.Content);
    }
}

public class SvnChangeListTests
{
    [Fact]
    public void SvnChangeList_HasDefaultValues()
    {
        var changeList = new SvnChangeList();

        Assert.Equal(string.Empty, changeList.Name);
        Assert.NotNull(changeList.Files);
        Assert.Empty(changeList.Files);
    }

    [Fact]
    public void SvnChangeList_CanAddFile()
    {
        var changeList = new SvnChangeList { Name = "feature-1" };
        changeList.AddFile(new SvnStatus { Path = "file1.cs" });
        changeList.AddFile(new SvnStatus { Path = "file2.cs" });

        Assert.Equal("feature-1", changeList.Name);
        Assert.Equal(2, changeList.FileCount);
    }

    [Fact]
    public void SvnChangeList_CanRemoveFile()
    {
        var changeList = new SvnChangeList();
        changeList.AddFile(new SvnStatus { Path = "file1.cs" });
        changeList.AddFile(new SvnStatus { Path = "file2.cs" });

        var result = changeList.RemoveFile("file1.cs");

        Assert.True(result);
        Assert.Equal(1, changeList.FileCount);
    }

    [Fact]
    public void IsEmpty_ReturnsTrue_WhenNoFiles()
    {
        var changeList = new SvnChangeList();

        Assert.True(changeList.IsEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalse_WhenHasFiles()
    {
        var changeList = new SvnChangeList();
        changeList.AddFile(new SvnStatus { Path = "file.cs" });

        Assert.False(changeList.IsEmpty);
    }
}

public class TimelineItemTests
{
    [Fact]
    public void TimelineItem_HasDefaultValues()
    {
        var item = new TimelineItem();

        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.Description);
        Assert.Equal(string.Empty, item.Date);
        Assert.Equal(TimelineStatus.Pending, item.Status);
    }

    [Fact]
    public void TimelineItem_CanSetValues()
    {
        var item = new TimelineItem
        {
            Title = "Step 1",
            Description = "First step",
            Date = "2024-01-15",
            Status = TimelineStatus.Completed
        };

        Assert.Equal("Step 1", item.Title);
        Assert.Equal("First step", item.Description);
        Assert.Equal("2024-01-15", item.Date);
        Assert.Equal(TimelineStatus.Completed, item.Status);
    }

    [Theory]
    [InlineData(TimelineStatus.Completed, "✓")]
    [InlineData(TimelineStatus.InProgress, "◐")]
    [InlineData(TimelineStatus.Pending, "○")]
    public void StatusIcon_ReturnsCorrectIcon(TimelineStatus status, string expected)
    {
        var item = new TimelineItem { Status = status };

        Assert.Equal(expected, item.StatusIcon);
    }

    [Fact]
    public void StatusColor_ReturnsGreen_ForCompleted()
    {
        var item = new TimelineItem { Status = TimelineStatus.Completed };

        Assert.Equal("#10B981", item.StatusColor);
    }

    [Fact]
    public void StatusColor_ReturnsBlue_ForInProgress()
    {
        var item = new TimelineItem { Status = TimelineStatus.InProgress };

        Assert.Equal("#3B82F6", item.StatusColor);
    }

    [Fact]
    public void StatusColor_ReturnsGray_ForPending()
    {
        var item = new TimelineItem { Status = TimelineStatus.Pending };

        Assert.Equal("#9CA3AF", item.StatusColor);
    }
}

public class MergeAcceptTypeExtensionsTests
{
    [Theory]
    [InlineData(MergeAcceptType.Postpone, "postpone")]
    [InlineData(MergeAcceptType.Base, "base")]
    [InlineData(MergeAcceptType.MineConflict, "mine-conflict")]
    [InlineData(MergeAcceptType.TheirsConflict, "theirs-conflict")]
    [InlineData(MergeAcceptType.MineFull, "mine-full")]
    [InlineData(MergeAcceptType.TheirsFull, "theirs-full")]
    [InlineData(MergeAcceptType.Edit, "edit")]
    [InlineData(MergeAcceptType.Launch, "launch")]
    public void ToSvnArgument_ReturnsCorrectValue(MergeAcceptType type, string expected)
    {
        Assert.Equal(expected, type.ToSvnArgument());
    }

    [Fact]
    public void GetDescription_ReturnsNonEmptyString()
    {
        foreach (MergeAcceptType type in Enum.GetValues<MergeAcceptType>())
        {
            var description = type.GetDescription();
            Assert.NotNull(description);
            Assert.NotEmpty(description);
        }
    }
}
