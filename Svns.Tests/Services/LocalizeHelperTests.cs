using Xunit;
using Svns.Services.Localization;

namespace Svns.Tests.Services;

public class LocalizeHelperTests
{
    [Fact]
    public void Get_ReturnsString_ForValidKey()
    {
        var result = Localize.Get("Common.OK");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Get_WithArgs_FormatsString()
    {
        var result = Localize.Get("Commit.Success", 456);

        Assert.Contains("456", result);
    }

    [Fact]
    public void T_IsSameAsGet()
    {
        var getResult = Localize.Get("Common.Cancel");
        var tResult = Localize.T("Common.Cancel");

        Assert.Equal(getResult, tResult);
    }

    [Fact]
    public void T_WithArgs_IsSameAsGetWithArgs()
    {
        var getResult = Localize.Get("Update.Success", 789);
        var tResult = Localize.T("Update.Success", 789);

        Assert.Equal(getResult, tResult);
    }

    [Fact]
    public void Get_ReturnsKeyInBrackets_ForMissingKey()
    {
        var result = Localize.Get("NonExistent.Key");

        Assert.Equal("[NonExistent.Key]", result);
    }
}
