using Xunit;
using Svns.Models;

namespace Svns.Tests.Models;

public class MergeAcceptTypeTests
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
    public void ToSvnArgument_ReturnsCorrectValue(MergeAcceptType acceptType, string expected)
    {
        var result = acceptType.ToSvnArgument();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(MergeAcceptType.Postpone, "Postpone (resolve later)")]
    [InlineData(MergeAcceptType.Base, "Base (discard all changes)")]
    [InlineData(MergeAcceptType.MineConflict, "Mine (keep my changes for conflicts)")]
    [InlineData(MergeAcceptType.TheirsConflict, "Theirs (keep their changes for conflicts)")]
    [InlineData(MergeAcceptType.MineFull, "Mine Full (keep all my changes)")]
    [InlineData(MergeAcceptType.TheirsFull, "Theirs Full (keep all their changes)")]
    [InlineData(MergeAcceptType.Edit, "Edit (manual edit)")]
    [InlineData(MergeAcceptType.Launch, "Launch (external tool)")]
    public void GetDescription_ReturnsCorrectValue(MergeAcceptType acceptType, string expected)
    {
        var result = acceptType.GetDescription();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AcceptTypes_ContainsAllValues()
    {
        var values = Enum.GetValues<MergeAcceptType>();
        Assert.Equal(8, values.Length);
    }
}
