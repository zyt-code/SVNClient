using Xunit;
using Svns.Services.Svn.Core;

namespace Svns.Tests.Services;

public class SvnExceptionTests
{
    [Fact]
    public void SvnException_SetsAllProperties()
    {
        var args = new[] { "arg1", "arg2" };
        var ex = new SvnException("status", args, 1, "Error occurred");

        Assert.Equal("status", ex.Command);
        Assert.Equal(args, ex.Arguments);
        Assert.Equal(1, ex.ExitCode);
        Assert.Equal("Error occurred", ex.ErrorOutput);
        Assert.Contains("status", ex.Message);
        Assert.Contains("1", ex.Message);
        Assert.Contains("Error occurred", ex.Message);
    }

    [Fact]
    public void SvnException_WithInnerException_SetsProperties()
    {
        var inner = new Exception("Inner error");
        var ex = new SvnException("Outer message", inner);

        Assert.Equal(string.Empty, ex.Command);
        Assert.Empty(ex.Arguments);
        Assert.Equal(-1, ex.ExitCode);
        Assert.Equal("Inner error", ex.ErrorOutput);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void WorkingCopyNotFoundException_SetsPath()
    {
        var ex = new WorkingCopyNotFoundException(@"C:\test\repo");

        Assert.Equal(@"C:\test\repo", ex.Path);
        Assert.Contains(@"C:\test\repo", ex.ErrorOutput);
        Assert.Equal("status", ex.Command);
        Assert.Equal(-1, ex.ExitCode);
    }

    [Fact]
    public void SvnNotFoundException_HasCorrectMessage()
    {
        var ex = new SvnNotFoundException();

        Assert.Contains("SVN executable", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void SvnOperationCanceledException_HasCorrectMessage()
    {
        var ex = new SvnOperationCanceledException();

        Assert.Contains("cancelled", ex.Message);
    }
}
