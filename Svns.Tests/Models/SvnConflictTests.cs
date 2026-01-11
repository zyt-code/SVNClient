using Xunit;
using Svns.Models;

namespace Svns.Tests.Models;

public class SvnConflictTests
{
    [Fact]
    public void DefaultValues_AreInitialized()
    {
        var conflict = new SvnConflict();
        Assert.Equal(string.Empty, conflict.Path);
        Assert.Equal(SvnConflictType.Text, conflict.ConflictType);
        Assert.Null(conflict.RepositoryLocation);
        Assert.Null(conflict.BaseFile);
        Assert.Null(conflict.TheirFile);
        Assert.Null(conflict.MyFile);
        Assert.Null(conflict.MergedFile);
        Assert.Null(conflict.Description);
        Assert.False(conflict.IsPropertyConflict);
        Assert.False(conflict.IsBinary);
        Assert.Null(conflict.PropertyName);
        Assert.False(conflict.IsResolved);
        Assert.Null(conflict.ResolutionMethod);
    }

    [Fact]
    public void FileName_ReturnsFileName_FromPath()
    {
        var conflict = new SvnConflict { Path = @"C:\repo\src\file.cs" };
        Assert.Equal("file.cs", conflict.FileName);
    }

    [Fact]
    public void FileName_ReturnsEmptyString_WhenPathIsEmpty()
    {
        var conflict = new SvnConflict { Path = "" };
        Assert.Equal("", conflict.FileName);
    }

    [Theory]
    [InlineData(SvnConflictType.Text)]
    [InlineData(SvnConflictType.Property)]
    [InlineData(SvnConflictType.Tree)]
    [InlineData(SvnConflictType.Binary)]
    public void ConflictType_AllValuesExist(SvnConflictType type)
    {
        Assert.True(Enum.IsDefined(typeof(SvnConflictType), type));
    }

    [Theory]
    [InlineData(SvnConflictResolution.Working)]
    [InlineData(SvnConflictResolution.Base)]
    [InlineData(SvnConflictResolution.Repository)]
    [InlineData(SvnConflictResolution.Merged)]
    [InlineData(SvnConflictResolution.Manual)]
    [InlineData(SvnConflictResolution.Postpone)]
    public void ConflictResolution_AllValuesExist(SvnConflictResolution resolution)
    {
        Assert.True(Enum.IsDefined(typeof(SvnConflictResolution), resolution));
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var conflict = new SvnConflict
        {
            Path = @"C:\repo\conflict.cs",
            ConflictType = SvnConflictType.Tree,
            RepositoryLocation = "http://svn/repo",
            BaseFile = "conflict.cs.base",
            TheirFile = "conflict.cs.theirs",
            MyFile = "conflict.cs.mine",
            MergedFile = "conflict.cs.merged",
            Description = "Tree conflict",
            IsPropertyConflict = true,
            IsBinary = true,
            PropertyName = "svn:keywords",
            IsResolved = true,
            ResolutionMethod = SvnConflictResolution.Working
        };

        Assert.Equal(SvnConflictType.Tree, conflict.ConflictType);
        Assert.True(conflict.IsPropertyConflict);
        Assert.True(conflict.IsBinary);
        Assert.True(conflict.IsResolved);
        Assert.Equal(SvnConflictResolution.Working, conflict.ResolutionMethod);
    }
}
