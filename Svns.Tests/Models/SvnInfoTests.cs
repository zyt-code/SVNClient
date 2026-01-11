using Xunit;
using Svns.Models;

namespace Svns.Tests.Models;

public class SvnInfoTests
{
    [Fact]
    public void DefaultValues_AreInitialized()
    {
        var info = new SvnInfo();
        Assert.Equal(string.Empty, info.Path);
        Assert.Equal(string.Empty, info.RelativePath);
        Assert.Equal(string.Empty, info.RepositoryRootUrl);
        Assert.Equal(string.Empty, info.RepositoryUuid);
        Assert.Equal(string.Empty, info.Url);
        Assert.Equal(string.Empty, info.WorkingCopyRoot);
        Assert.Equal(string.Empty, info.NodeKind);
        Assert.Equal(string.Empty, info.Schedule);
        Assert.Equal(0, info.Revision);
        Assert.Equal(0, info.LastChangedRevision);
        Assert.Null(info.LastChangedAuthor);
        Assert.False(info.HasConflict);
        Assert.False(info.IsCopied);
        Assert.False(info.IsLocked);
    }

    [Fact]
    public void IsFile_ReturnsTrue_WhenNodeKindIsFile()
    {
        var info = new SvnInfo { NodeKind = "file" };
        Assert.True(info.IsFile);
        Assert.False(info.IsDirectory);
    }

    [Fact]
    public void IsFile_ReturnsFalse_WhenNodeKindIsDir()
    {
        var info = new SvnInfo { NodeKind = "dir" };
        Assert.False(info.IsFile);
        Assert.True(info.IsDirectory);
    }

    [Fact]
    public void IsFile_IsCaseInsensitive()
    {
        var info = new SvnInfo { NodeKind = "FILE" };
        Assert.True(info.IsFile);
    }

    [Fact]
    public void IsDirectory_IsCaseInsensitive()
    {
        var info = new SvnInfo { NodeKind = "DIR" };
        Assert.True(info.IsDirectory);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var date = DateTime.Now;
        var info = new SvnInfo
        {
            Path = @"C:\repo\file.cs",
            RelativePath = "file.cs",
            RepositoryRootUrl = "http://svn/repo",
            RepositoryUuid = "12345-67890",
            Url = "http://svn/repo/file.cs",
            WorkingCopyRoot = @"C:\repo",
            Revision = 100,
            NodeKind = "file",
            Schedule = "normal",
            LastChangedAuthor = "user",
            LastChangedRevision = 99,
            LastChangedDate = date,
            RepositoryId = "repo-id",
            Depth = SvnDepth.Infinity,
            HasConflict = true,
            ConflictOld = "file.cs.old",
            ConflictWorking = "file.cs.working",
            ConflictNew = "file.cs.new",
            IsCopied = true,
            CopyFromUrl = "http://svn/repo/original.cs",
            CopyFromRevision = 50,
            IsLocked = true,
            LockOwner = "admin",
            LockCreationDate = date,
            LockComment = "locked for editing",
            LockToken = "token123"
        };

        Assert.Equal(@"C:\repo\file.cs", info.Path);
        Assert.Equal("user", info.LastChangedAuthor);
        Assert.Equal(100, info.Revision);
        Assert.True(info.HasConflict);
        Assert.True(info.IsLocked);
        Assert.Equal("admin", info.LockOwner);
    }
}
