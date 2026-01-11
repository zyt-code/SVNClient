using Xunit;
using Svns.Services.Svn.Core;

namespace Svns.Tests.Services;

public class SvnResultTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResult()
    {
        var result = SvnResult.Ok("output", 0);
        Assert.True(result.Success);
        Assert.Equal("output", result.StandardOutput);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Fail_CreatesFailedResult()
    {
        var result = SvnResult.Fail("error", 1);
        Assert.False(result.Success);
        Assert.Equal("error", result.StandardError);
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public void Fail_WithException_IncludesException()
    {
        var ex = new Exception("Test exception");
        var result = SvnResult.Fail("error", -1, ex);
        Assert.False(result.Success);
        Assert.Equal(ex, result.Exception);
    }
}
