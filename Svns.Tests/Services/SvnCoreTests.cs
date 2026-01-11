using Xunit;
using Svns.Services.Svn.Core;

namespace Svns.Tests.Services;

public class SvnResultExtendedTests
{
    [Fact]
    public void Success_ReturnsTrue_WhenSetToTrue()
    {
        var result = new SvnResult
        {
            Success = true,
            ExitCode = 0,
            StandardOutput = "output",
            StandardError = ""
        };

        Assert.True(result.Success);
    }

    [Fact]
    public void Success_ReturnsFalse_WhenSetToFalse()
    {
        var result = new SvnResult
        {
            Success = false,
            ExitCode = 1,
            StandardOutput = "",
            StandardError = "error"
        };

        Assert.False(result.Success);
    }

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
        var ex = new Exception("test");
        var result = SvnResult.Fail("error", 1, ex);

        Assert.False(result.Success);
        Assert.Equal("error", result.StandardError);
        Assert.Equal(1, result.ExitCode);
        Assert.Same(ex, result.Exception);
    }

    [Fact]
    public void StandardOutput_CanBeSet()
    {
        var result = new SvnResult
        {
            StandardOutput = "some output"
        };

        Assert.Equal("some output", result.StandardOutput);
    }

    [Fact]
    public void StandardError_CanBeSet()
    {
        var result = new SvnResult
        {
            StandardError = "error message"
        };

        Assert.Equal("error message", result.StandardError);
    }

    [Fact]
    public void ExitCode_CanBeSet()
    {
        var result = new SvnResult
        {
            ExitCode = 42
        };

        Assert.Equal(42, result.ExitCode);
    }
}

public class SvnExceptionExtendedTests
{
    [Fact]
    public void Constructor_Full_SetsAllProperties()
    {
        var ex = new SvnException("status", new[] { "-v" }, 1, "Error output");

        Assert.Equal("status", ex.Command);
        Assert.Contains("-v", ex.Arguments);
        Assert.Equal(1, ex.ExitCode);
        Assert.Equal("Error output", ex.ErrorOutput);
    }

    [Fact]
    public void Constructor_WithInner_SetsMessage()
    {
        var inner = new InvalidOperationException("Inner error");
        var ex = new SvnException("Outer error", inner);

        Assert.Contains("Outer error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Message_ContainsCommandAndExitCode()
    {
        var ex = new SvnException("update", new[] { "." }, 128, "Auth failed");

        Assert.Contains("update", ex.Message);
        Assert.Contains("128", ex.Message);
    }
}
