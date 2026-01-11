using Xunit;
using Svns.Models;

namespace Svns.Tests.Models;

public class SvnStatusTests
{
    [Fact]
    public void Name_ReturnsFileName_FromPath()
    {
        var status = new SvnStatus { Path = @"C:\repo\src\file.cs" };
        Assert.Equal("file.cs", status.Name);
    }

    [Fact]
    public void IsFile_ReturnsTrue_WhenNodeTypeIsFile()
    {
        var status = new SvnStatus { NodeType = SvnNodeType.File };
        Assert.True(status.IsFile);
    }

    [Fact]
    public void IsFile_ReturnsFalse_WhenNodeTypeIsDirectory()
    {
        var status = new SvnStatus { NodeType = SvnNodeType.Directory };
        Assert.False(status.IsFile);
    }

    [Theory]
    [InlineData(SvnStatusType.Modified, true)]
    [InlineData(SvnStatusType.Added, true)]
    [InlineData(SvnStatusType.Deleted, true)]
    [InlineData(SvnStatusType.Conflicted, true)]
    [InlineData(SvnStatusType.Replaced, true)]
    [InlineData(SvnStatusType.Merged, true)]
    [InlineData(SvnStatusType.Normal, false)] // Note: Normal == None (both are ' ')
    public void ShowStatusBadge_ReturnsExpected(SvnStatusType status, bool expected)
    {
        var svnStatus = new SvnStatus { WorkingCopyStatus = status };
        Assert.Equal(expected, svnStatus.ShowStatusBadge);
    }

    [Theory]
    [InlineData(SvnStatusType.Modified, true)]
    [InlineData(SvnStatusType.Added, true)]
    [InlineData(SvnStatusType.Deleted, true)]
    [InlineData(SvnStatusType.Normal, false)] // Note: Normal == None (both are ' ')
    [InlineData(SvnStatusType.Ignored, false)]
    [InlineData(SvnStatusType.External, false)]
    public void HasLocalModifications_ReturnsExpected(SvnStatusType status, bool expected)
    {
        var svnStatus = new SvnStatus { WorkingCopyStatus = status };
        Assert.Equal(expected, svnStatus.HasLocalModifications);
    }

    [Fact]
    public void DisplayStatus_ReturnsWorkingCopyStatus_WhenNotNone()
    {
        var status = new SvnStatus { WorkingCopyStatus = SvnStatusType.Modified };
        Assert.Equal("Modified", status.DisplayStatus);
    }

    [Fact]
    public void DisplayStatus_ReturnsNormal_WhenNoStatus()
    {
        var status = new SvnStatus
        {
            WorkingCopyStatus = SvnStatusType.None,
            RepositoryStatus = SvnStatusType.None
        };
        Assert.Equal("Normal", status.DisplayStatus);
    }

    [Fact]
    public void Children_DefaultsToEmptyCollection()
    {
        var status = new SvnStatus();
        Assert.NotNull(status.Children);
        Assert.Empty(status.Children);
    }
}
